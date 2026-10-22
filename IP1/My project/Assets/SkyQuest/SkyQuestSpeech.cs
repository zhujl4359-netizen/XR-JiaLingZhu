using System;
using System.Text;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data.Configuration;
using Oculus.Voice.Dictation;
using Oculus.Voice.Dictation.Configuration;
using UnityEngine;

public class SkyQuestSpeech : MonoBehaviour
{
    public event Action<string> OnRecognized;
    public event Action<string> OnStatusChanged;
    public string LastFailure { get; private set; }

    private WitConfiguration witConfiguration;
    private AppDictationExperience dictation;
    private SkyQuestOfflineSpeech offline;
    private string pending;
    private bool hasPending;
    private bool listening;
    private bool initialized;

    public void Configure(WitConfiguration configuration)
    {
        witConfiguration = configuration;
    }

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        offline = gameObject.AddComponent<SkyQuestOfflineSpeech>();
        offline.OnRecognized += OnOfflineTranscription;
        offline.OnFailure += OnOfflineError;
        offline.OnStateChanged += OnOfflineState;
        offline.Preload();
#endif
    }

    public void BeginRecognition()
    {
        if (listening) return;
        LastFailure = null;
        WitConfiguration config = witConfiguration != null ? witConfiguration :
            Resources.Load<WitConfiguration>("SkyQuestWitConfig");
        if (config != null && string.IsNullOrWhiteSpace(config.GetClientAccessToken())) config = null;
#if UNITY_ANDROID && !UNITY_EDITOR
        if (config == null)
        {
            if (offline == null)
            {
                offline = gameObject.AddComponent<SkyQuestOfflineSpeech>();
                offline.OnRecognized += OnOfflineTranscription;
                offline.OnFailure += OnOfflineError;
                offline.OnStateChanged += OnOfflineState;
            }
            listening = true;
            offline.BeginRecognition();
            return;
        }
#endif
#if !UNITY_ANDROID || UNITY_EDITOR
        if (config == null)
        {
            LastFailure = "Voice input needs a linked Wit.ai config in the Editor. Quest uses the installed Meta platform dictation service.";
            QueueResult("");
            return;
        }
#endif

        listening = true;
        if (dictation == null || dictation.Configuration != config)
            CreateDictation(config);
        if (initialized && !dictation.Active && !dictation.MicActive)
            dictation.Activate();
    }

    private void CreateDictation(WitConfiguration config)
    {
        if (dictation != null) Destroy(dictation.gameObject);
        initialized = false;
        GameObject voiceObject = new GameObject("Meta Voice dictation");
        voiceObject.transform.SetParent(transform, false);
        voiceObject.SetActive(false);
        dictation = voiceObject.AddComponent<AppDictationExperience>();
        dictation.RuntimeDictationConfiguration = new WitDictationRuntimeConfiguration
        {
            witConfiguration = config,
            dictationConfiguration = new DictationConfiguration
            {
                multiPhrase = false,
                scenario = "messaging",
                inputType = "text_default"
            }
        };
#if UNITY_ANDROID && !UNITY_EDITOR
        dictation.DoNotFallbackToWit = config == null;
        dictation.UsePlatformIntegrations = true;
#endif
        dictation.DictationEvents.OnFullTranscription.AddListener(OnMetaTranscription);
        dictation.DictationEvents.OnError.AddListener(OnMetaError);
        dictation.OnInitialized += OnMetaInitialized;
        voiceObject.SetActive(true);
    }

    private void OnMetaInitialized()
    {
        initialized = true;
        if (listening && dictation.isActiveAndEnabled) dictation.Activate();
    }

    private void OnMetaTranscription(string text)
    {
        if (!listening) return;
        listening = false;
        dictation.Deactivate();
        QueueResult(text);
    }

    private void OnMetaError(string error, string message)
    {
        if (!listening) return;
        listening = false;
        LastFailure = string.IsNullOrWhiteSpace(message) ? error : message;
        QueueResult("");
    }

    private void OnOfflineTranscription(string text)
    {
        if (!listening) return;
        listening = false;
        QueueResult(text);
    }

    private void OnOfflineError(string message)
    {
        if (!listening) return;
        listening = false;
        LastFailure = message;
        Debug.LogWarning("SkyQuest English voice: " + message);
        QueueResult("");
    }

    private void OnOfflineState(string message)
    {
        if (listening) OnStatusChanged?.Invoke(message);
    }

    private void QueueResult(string value)
    {
        pending = EnglishOnly(value);
        hasPending = true;
    }

    private static string EnglishOnly(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        StringBuilder english = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' ||
                c >= '0' && c <= '9' || c == ' ' || c == '\'' ||
                c == ',' || c == '.' || c == '?' || c == '!') english.Append(c);
        }
        return english.ToString().Trim();
    }

    void Update()
    {
        if (!hasPending) return;
        string result = pending;
        hasPending = false;
        OnRecognized?.Invoke(result);
    }

    void OnDestroy()
    {
        if (offline != null)
        {
            offline.OnRecognized -= OnOfflineTranscription;
            offline.OnFailure -= OnOfflineError;
            offline.OnStateChanged -= OnOfflineState;
        }
        if (dictation == null) return;
        dictation.OnInitialized -= OnMetaInitialized;
        dictation.DictationEvents.OnFullTranscription.RemoveListener(OnMetaTranscription);
        dictation.DictationEvents.OnError.RemoveListener(OnMetaError);
        if (dictation.Active) dictation.Cancel();
    }
}
