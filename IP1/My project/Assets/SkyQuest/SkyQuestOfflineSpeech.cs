using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

// Local English dictation for Quest headsets without Meta's platform service.
public sealed class SkyQuestOfflineSpeech : MonoBehaviour
{
    private const int SampleRate = 16000;
    private const int RecordingSeconds = 7;
    private const string ArchiveName = "vosk-model-small-en-us-0.15.zip";
    private const string ModelName = "vosk-model-small-en-us-0.15";

    public event Action<string> OnRecognized;
    public event Action<string> OnFailure;
    public event Action<string> OnStateChanged;
    private Task<Vosk.Model> modelTask;
    private Vosk.Model model;
    private bool recording;
    private bool loading;

    [Serializable] private sealed class RecognitionResult { public string text; }

    public void Preload()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!loading && modelTask == null && model == null) StartCoroutine(LoadModel());
#endif
    }

    public void BeginRecognition()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!recording) StartCoroutine(Record());
#else
        OnFailure?.Invoke("Offline dictation is available on the Quest build.");
#endif
    }

    private IEnumerator LoadModel()
    {
        loading = true;
        OnStateChanged?.Invoke("Preparing English speech model...");
        string root = Path.Combine(Application.persistentDataPath, "VoskModels");
        string modelPath = Path.Combine(root, ModelName);
        string marker = Path.Combine(modelPath, ".ready");
        if (!File.Exists(marker))
        {
            string source = Path.Combine(Application.streamingAssetsPath, ArchiveName);
            using (UnityWebRequest request = UnityWebRequest.Get(source))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    loading = false;
                    OnFailure?.Invoke("Speech model could not be loaded: " + request.error);
                    yield break;
                }
                byte[] archive = request.downloadHandler.data;
                Task extract = Task.Run(() => ExtractModel(archive, root, marker));
                while (!extract.IsCompleted) yield return null;
                if (extract.IsFaulted)
                {
                    loading = false;
                    OnFailure?.Invoke("Speech model extraction failed: " + extract.Exception?.GetBaseException().Message);
                    yield break;
                }
            }
        }
        modelTask = Task.Run(() =>
        {
            Vosk.Vosk.SetLogLevel(-1);
            return new Vosk.Model(modelPath);
        });
        while (!modelTask.IsCompleted) yield return null;
        loading = false;
        if (modelTask.IsFaulted)
            OnFailure?.Invoke("Speech model initialization failed: " + modelTask.Exception?.GetBaseException().Message);
        else
        {
            model = modelTask.Result;
            OnStateChanged?.Invoke("English voice ready.");
        }
    }

    private static void ExtractModel(byte[] archive, string root, string marker)
    {
        Directory.CreateDirectory(root);
        string safeRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        using (var memory = new MemoryStream(archive, false))
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Read))
        {
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                string destination = Path.GetFullPath(Path.Combine(root, entry.FullName));
                if (!destination.StartsWith(safeRoot, StringComparison.Ordinal))
                    throw new IOException("Invalid speech model archive path.");
                if (string.IsNullOrEmpty(entry.Name)) Directory.CreateDirectory(destination);
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    using (Stream input = entry.Open())
                    using (FileStream output = File.Create(destination)) input.CopyTo(output);
                }
            }
        }
        File.WriteAllText(marker, "ready");
    }

    private IEnumerator Record()
    {
        recording = true;
        Preload();
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            float deadline = Time.realtimeSinceStartup + 12f;
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone)
                && Time.realtimeSinceStartup < deadline) yield return null;
        }
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            recording = false;
            OnFailure?.Invoke("Microphone permission was not granted.");
            yield break;
        }
#endif
        if (loading) OnStateChanged?.Invoke("Loading English voice; please wait...");
        while (loading) yield return null;
        if (model == null)
        {
            recording = false;
            OnFailure?.Invoke("Offline speech model is unavailable.");
            yield break;
        }
        AudioClip clip = Microphone.Start(null, false, RecordingSeconds + 1, SampleRate);
        if (clip == null)
        {
            recording = false;
            OnFailure?.Invoke("Could not start the microphone.");
            yield break;
        }
        float startDeadline = Time.realtimeSinceStartup + 3f;
        while (Microphone.GetPosition(null) <= 0 && Time.realtimeSinceStartup < startDeadline)
            yield return null;
        if (Microphone.GetPosition(null) <= 0)
        {
            Microphone.End(null);
            recording = false;
            OnFailure?.Invoke("Microphone did not provide audio.");
            yield break;
        }
        OnStateChanged?.Invoke("Listening: speak English now.");
        yield return new WaitForSecondsRealtime(RecordingSeconds);
        int frames = Mathf.Clamp(Microphone.GetPosition(null), 1, clip.samples);
        float[] samples = new float[frames * clip.channels];
        bool gotAudio = clip.GetData(samples, 0);
        Microphone.End(null);
        recording = false;
        if (!gotAudio)
        {
            OnFailure?.Invoke("Could not read microphone audio.");
            yield break;
        }
        int channels = clip.channels;
        OnStateChanged?.Invoke("Transcribing English...");
        Task<string> recognition = Task.Run(() => Recognize(samples, channels));
        while (!recognition.IsCompleted) yield return null;
        if (recognition.IsFaulted)
            OnFailure?.Invoke("Offline recognition failed: " + recognition.Exception?.GetBaseException().Message);
        else if (string.IsNullOrWhiteSpace(recognition.Result))
            OnFailure?.Invoke("No English speech detected. Please try again.");
        else OnRecognized?.Invoke(recognition.Result);
    }

    private string Recognize(float[] samples, int channels)
    {
        byte[] pcm = new byte[(samples.Length / channels) * 2];
        for (int frame = 0; frame < pcm.Length / 2; frame++)
        {
            float sum = 0f;
            for (int channel = 0; channel < channels; channel++) sum += samples[frame * channels + channel];
            short value = (short)(Mathf.Clamp(sum / channels, -1f, 1f) * short.MaxValue);
            pcm[frame * 2] = (byte)value;
            pcm[frame * 2 + 1] = (byte)(value >> 8);
        }
        var text = new StringBuilder();
        using (var recognizer = new Vosk.VoskRecognizer(model, SampleRate))
        {
            for (int offset = 0; offset < pcm.Length; offset += 8192)
            {
                int count = Math.Min(8192, pcm.Length - offset);
                byte[] chunk = new byte[count];
                Buffer.BlockCopy(pcm, offset, chunk, 0, count);
                if (recognizer.AcceptWaveform(chunk, count)) AppendText(text, recognizer.Result());
            }
            AppendText(text, recognizer.FinalResult());
        }
        return text.ToString().Trim();
    }

    private static void AppendText(StringBuilder text, string json)
    {
        RecognitionResult result = JsonUtility.FromJson<RecognitionResult>(json);
        if (result != null && !string.IsNullOrWhiteSpace(result.text)) text.Append(result.text.Trim());
    }

    private void OnDestroy()
    {
        if (Microphone.IsRecording(null)) Microphone.End(null);
        if (modelTask != null && !modelTask.IsCompleted) return;
        model?.Dispose();
    }
}
