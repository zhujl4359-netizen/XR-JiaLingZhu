// ！！！文件功能：离线语音模型的加载、录音和识别回调！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 System.Collections 命名空间；下方可直接使用该库提供的类型和方法。
using System.Collections;
// 引入 System.IO 命名空间；下方可直接使用File 文件与 Directory 文件夹操作。
using System.IO;
// 引入 System.IO.Compression 命名空间；下方可直接使用该库提供的类型和方法。
using System.IO.Compression;
// 引入 System.Text 命名空间；下方可直接使用该库提供的类型和方法。
using System.Text;
// 引入 System.Threading.Tasks 命名空间；下方可直接使用该库提供的类型和方法。
using System.Threading.Tasks;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.Networking 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.Networking;

// Local English dictation for Quest headsets without Meta's platform service.
// 定义 SkyQuestOfflineSpeech 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class SkyQuestOfflineSpeech : MonoBehaviour
{
    // 定义不可在运行时改变的数值 SampleRate（SampleRate）；后续计算统一使用这个阈值、速度或尺寸。
    private const int SampleRate = 16000;
    // 定义不可在运行时改变的数值 RecordingSeconds（RecordingSeconds）；后续计算统一使用这个阈值、速度或尺寸。
    private const int RecordingSeconds = 7;
    // 定义固定文字 ArchiveName（ArchiveName）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string ArchiveName = "vosk-model-small-en-us-0.15.zip";
    // 定义固定文字 ModelName（ModelName）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string ModelName = "vosk-model-small-en-us-0.15";

    // 执行 public event Action<string> OnRecognized;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action<string> OnRecognized;
    // 执行 public event Action<string> OnFailure;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action<string> OnFailure;
    // 执行 public event Action<string> OnStateChanged;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action<string> OnStateChanged;
    // modelTask 保存modelTask（类型 Task<Vosk.Model>）；下方方法依靠它记录或查找场景状态。
    private Task<Vosk.Model> modelTask;
    // model 保存model（类型 Vosk.Model）；下方方法依靠它记录或查找场景状态。
    private Vosk.Model model;
    // 声明 recording（recording）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool recording;
    // 声明 loading（loading）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool loading;

    // 告诉 Unity 这个普通 C# 数据类允许序列化；字段可被保存或从配置读取。
    [Serializable] private sealed class RecognitionResult { public string text; }

    // ！！！功能：Preload——Preload！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Preload()
    {
// 以下代码只编译进 Android 真机版本，并在 Unity 编辑器运行时跳过；Quest 使用这段调用系统接口。
#if UNITY_ANDROID && !UNITY_EDITOR
        // 步骤 1：检查modelTask、model是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 StartCoroutine：StartCoroutine。
        if (!loading && modelTask == null && model == null) StartCoroutine(LoadModel());
// 结束平台条件编译范围。
#endif
    }
    // ！！！功能结束：Preload！！！

    // ！！！功能：BeginRecognition——开始语音识别并等待 SDK 返回识别文字！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void BeginRecognition()
    {
// 以下代码只编译进 Android 真机版本，并在 Unity 编辑器运行时跳过；Quest 使用这段调用系统接口。
#if UNITY_ANDROID && !UNITY_EDITOR
        // 步骤 1：这里的 ! 表示取反；当 recording 不成立时才进入此分支。 成立时调用 StartCoroutine：StartCoroutine。
        if (!recording) StartCoroutine(Record());
// 下面改为编译另一种平台条件下的实现。
#else
        // 步骤 2：执行 OnFailure?.Invoke("Offline dictation is available on the Quest build.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnFailure?.Invoke("Offline dictation is available on the Quest build.");
// 结束平台条件编译范围。
#endif
    }
    // ！！！功能结束：BeginRecognition！！！

    // ！！！功能：LoadModel——加载Model！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private IEnumerator LoadModel()
    {
        // 步骤 1：把 loading 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        loading = true;
        // 步骤 2：执行 OnStateChanged?.Invoke("Preparing English speech model...");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnStateChanged?.Invoke("Preparing English speech model...");
        // 步骤 3：计算 Path.Combine(Application.persistentDataPath, "VoskModels")，把结果存进 root（root）；之后引用 root 时用的就是这次结果。
        string root = Path.Combine(Application.persistentDataPath, "VoskModels");
        // 步骤 4：计算 Path.Combine(root, ModelName)，把结果存进 modelPath（model路径）；之后引用 modelPath 时用的就是这次结果。
        string modelPath = Path.Combine(root, ModelName);
        // 步骤 5：计算 Path.Combine(modelPath, ".ready")，把结果存进 marker（marker）；之后引用 marker 时用的就是这次结果。
        string marker = Path.Combine(modelPath, ".ready");
        // 步骤 6：这里的 ! 表示取反；当 File.Exists(marker) 不成立时才进入此分支。
        if (!File.Exists(marker))
        {
            // 步骤 7：计算 Path.Combine(Application.streamingAssetsPath, ArchiveName)，把结果存进 source（反馈跟随的来源物品）；之后引用 source 时用的就是这次结果。
            string source = Path.Combine(Application.streamingAssetsPath, ArchiveName);
            // 步骤 8：在 using 括号中创建临时对象；离开下面的作用域时自动释放，防止资源泄漏。
            using (UnityWebRequest request = UnityWebRequest.Get(source))
            {
                // 步骤 9：执行 yield return request.SendWebRequest();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                yield return request.SendWebRequest();
                // 步骤 10：检查条件 request.result != UnityWebRequest.Result.Success；只有为真才运行下面的步骤，否则沿原流程继续。
                if (request.result != UnityWebRequest.Result.Success)
                {
                    // 步骤 11：把 loading 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                    loading = false;
                    // 步骤 12：执行 OnFailure?.Invoke("Speech model could not be loaded: " + request.error);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    OnFailure?.Invoke("Speech model could not be loaded: " + request.error);
                    // 步骤 13：break 保存break（类型 yield）；下方方法依靠它记录或查找场景状态。
                    yield break;
                }
                // 步骤 14：计算 request.downloadHandler.data，把结果存进 archive（archive）；之后引用 archive 时用的就是这次结果。
                byte[] archive = request.downloadHandler.data;
                // 步骤 15：计算 Task.Run(() => ExtractModel(archive, root, marker))，把结果存进 extract（extract）；之后引用 extract 时用的就是这次结果。
                Task extract = Task.Run(() => ExtractModel(archive, root, marker));
                // 步骤 16：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
                while (!extract.IsCompleted) yield return null;
                // 步骤 17：检查条件 extract.IsFaulted；只有为真才运行下面的步骤，否则沿原流程继续。
                if (extract.IsFaulted)
                {
                    // 步骤 18：把 loading 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                    loading = false;
                    // 步骤 19：执行 OnFailure?.Invoke("Speech model extraction failed: " + extract.Exception?.GetBaseException().Message；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    OnFailure?.Invoke("Speech model extraction failed: " + extract.Exception?.GetBaseException().Message);
                    // 步骤 20：break 保存break（类型 yield）；下方方法依靠它记录或查找场景状态。
                    yield break;
                }
            }
        }
        // 步骤 21：把计算结果 Task.Run(() => 保存为modelTask；下一条判断或调用会读取这个具体值。
        modelTask = Task.Run(() =>
        {
            // 步骤 22：执行 Vosk.Vosk.SetLogLevel(-1);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vosk.Vosk.SetLogLevel(-1);
            // 步骤 23：把 new Vosk.Model(modelPath) 作为方法结果返回给调用方；return 后本方法立即结束。
            return new Vosk.Model(modelPath);
        // 步骤 24：执行 });；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        });
        // 步骤 25：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (!modelTask.IsCompleted) yield return null;
        // 步骤 26：把 loading 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        loading = false;
        // 步骤 27：检查条件 modelTask.IsFaulted；只有为真才运行下面的步骤，否则沿原流程继续。
        if (modelTask.IsFaulted)
            // 步骤 28：执行 OnFailure?.Invoke("Speech model initialization failed: " + modelTask.Exception?.GetBaseException().M；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnFailure?.Invoke("Speech model initialization failed: " + modelTask.Exception?.GetBaseException().Message);
        // 步骤 29：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 30：把计算结果 modelTask.Result 保存为model；下一条判断或调用会读取这个具体值。
            model = modelTask.Result;
            // 步骤 31：执行 OnStateChanged?.Invoke("English voice ready.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnStateChanged?.Invoke("English voice ready.");
        }
    }
    // ！！！功能结束：LoadModel！！！

    // ！！！功能：ExtractModel——ExtractModel！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 byte[] archive, string root, string marker 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ExtractModel(byte[] archive, string root, string marker)
    {
        // 步骤 1：确认目标文件夹存在；不存在时创建它，后续写入场景、材质或资源文件才不会失败。
        Directory.CreateDirectory(root);
        // 步骤 2：计算 Path.GetFullPath(root) + Path.DirectorySeparatorChar，把结果存进 safeRoot（safeRoot）；之后引用 safeRoot 时用的就是这次结果。
        string safeRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        // 步骤 3：在 using 括号中创建临时对象；离开下面的作用域时自动释放，防止资源泄漏。
        using (var memory = new MemoryStream(archive, false))
        // 步骤 4：在 using 括号中创建临时对象；离开下面的作用域时自动释放，防止资源泄漏。
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Read))
        {
            // 步骤 5：逐个读取 ZipArchiveEntry entry in zip.Entries 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (ZipArchiveEntry entry in zip.Entries)
            {
                // 步骤 6：计算 Path.GetFullPath(Path.Combine(root, entry.FullName))，把结果存进 destination（destination）；之后引用 destination 时用的就是这次结果。
                string destination = Path.GetFullPath(Path.Combine(root, entry.FullName));
                // 步骤 7：这里的 ! 表示取反；当 destination.StartsWith(safeRoot, StringComparison.Ordinal) 不成立时才进入此分支。
                if (!destination.StartsWith(safeRoot, StringComparison.Ordinal))
                    // 步骤 8：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                    throw new IOException("Invalid speech model archive path.");
                // 步骤 9：检查条件 string.IsNullOrEmpty(entry.Name)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 CreateDirectory：创建Directory。
                if (string.IsNullOrEmpty(entry.Name)) Directory.CreateDirectory(destination);
                // 步骤 10：前面的 if 条件为假时执行这里的备用处理。
                else
                {
                    // 步骤 11：确认目标文件夹存在；不存在时创建它，后续写入场景、材质或资源文件才不会失败。
                    Directory.CreateDirectory(Path.GetDirectoryName(destination));
                    // 步骤 12：在 using 括号中创建临时对象；离开下面的作用域时自动释放，防止资源泄漏。
                    using (Stream input = entry.Open())
                    // 步骤 13：在 using 括号中创建临时对象；离开下面的作用域时自动释放，防止资源泄漏。
                    using (FileStream output = File.Create(destination)) input.CopyTo(output);
                }
            }
        }
        // 步骤 14：调用 WriteAllText：Write全部Text。
        File.WriteAllText(marker, "ready");
    }
    // ！！！功能结束：ExtractModel！！！

    // ！！！功能：Record——Record！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private IEnumerator Record()
    {
        // 步骤 1：把 recording 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        recording = true;
        // 步骤 2：调用 Preload：Preload。
        Preload();
// 以下代码只编译进 Android 真机版本，并在 Unity 编辑器运行时跳过；Quest 使用这段调用系统接口。
#if UNITY_ANDROID && !UNITY_EDITOR
        // 步骤 3：这里的 ! 表示取反；当 UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphon 不成立时才进入此分支。
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            // 步骤 4：执行 UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            // 步骤 5：计算 Time.realtimeSinceStartup + 12f，把结果存进 deadline（deadline）；之后引用 deadline 时用的就是这次结果。
            float deadline = Time.realtimeSinceStartup + 12f;
            // 步骤 6：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone)
                // 步骤 7：执行 && Time.realtimeSinceStartup < deadline) yield return null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                && Time.realtimeSinceStartup < deadline) yield return null;
        }
        // 步骤 8：这里的 ! 表示取反；当 UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphon 不成立时才进入此分支。
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            // 步骤 9：把 recording 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            recording = false;
            // 步骤 10：执行 OnFailure?.Invoke("Microphone permission was not granted.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnFailure?.Invoke("Microphone permission was not granted.");
            // 步骤 11：break 保存break（类型 yield）；下方方法依靠它记录或查找场景状态。
            yield break;
        }
// 结束平台条件编译范围。
#endif
        // 步骤 12：检查条件 loading；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 OnStateChanged?.Invoke("Loading English voice; please wait...");
        if (loading) OnStateChanged?.Invoke("Loading English voice; please wait...");
        // 步骤 13：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (loading) yield return null;
        // 步骤 14：检查model是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (model == null)
        {
            // 步骤 15：把 recording 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            recording = false;
            // 步骤 16：执行 OnFailure?.Invoke("Offline speech model is unavailable.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnFailure?.Invoke("Offline speech model is unavailable.");
            // 步骤 17：break 保存break（类型 yield）；下方方法依靠它记录或查找场景状态。
            yield break;
        }
        // 步骤 18：计算 Microphone.Start(null, false, RecordingSeconds + 1, SampleRate)，把结果存进 clip（clip）；之后引用 clip 时用的就是这次结果。
        AudioClip clip = Microphone.Start(null, false, RecordingSeconds + 1, SampleRate);
        // 步骤 19：检查clip是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (clip == null)
        {
            // 步骤 20：把 recording 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            recording = false;
            // 步骤 21：执行 OnFailure?.Invoke("Could not start the microphone.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnFailure?.Invoke("Could not start the microphone.");
            // 步骤 22：break 保存break（类型 yield）；下方方法依靠它记录或查找场景状态。
            yield break;
        }
        // 步骤 23：计算 Time.realtimeSinceStartup + 3f，把结果存进 startDeadline（startDeadline）；之后引用 startDeadline 时用的就是这次结果。
        float startDeadline = Time.realtimeSinceStartup + 3f;
        // 步骤 24：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (Microphone.GetPosition(null) <= 0 && Time.realtimeSinceStartup < startDeadline)
            // 步骤 25：执行 yield return null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            yield return null;
        // 步骤 26：检查条件 Microphone.GetPosition(null) <= 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if (Microphone.GetPosition(null) <= 0)
        {
            // 步骤 27：调用 End：End。
            Microphone.End(null);
            // 步骤 28：把 recording 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            recording = false;
            // 步骤 29：执行 OnFailure?.Invoke("Microphone did not provide audio.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnFailure?.Invoke("Microphone did not provide audio.");
            // 步骤 30：break 保存break（类型 yield）；下方方法依靠它记录或查找场景状态。
            yield break;
        }
        // 步骤 31：执行 OnStateChanged?.Invoke("Listening: speak English now.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnStateChanged?.Invoke("Listening: speak English now.");
        // 步骤 32：执行 yield return new WaitForSecondsRealtime(RecordingSeconds);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        yield return new WaitForSecondsRealtime(RecordingSeconds);
        // 步骤 33：计算 frames 并限制在安全上下界，防止不合理的大小影响移动或显示。
        int frames = Mathf.Clamp(Microphone.GetPosition(null), 1, clip.samples);
        // 步骤 34：创建新的 float[] 并存为 samples；构造参数决定其初始内容或对象名称。
        float[] samples = new float[frames * clip.channels];
        // 步骤 35：用 gotAudio 记住此处真假结果（clip.GetData(samples, 0)），供下一条 if 选择执行路径。
        bool gotAudio = clip.GetData(samples, 0);
        // 步骤 36：调用 End：End。
        Microphone.End(null);
        // 步骤 37：把 recording 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        recording = false;
        // 步骤 38：这里的 ! 表示取反；当 gotAudio 不成立时才进入此分支。
        if (!gotAudio)
        {
            // 步骤 39：执行 OnFailure?.Invoke("Could not read microphone audio.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnFailure?.Invoke("Could not read microphone audio.");
            // 步骤 40：break 保存break（类型 yield）；下方方法依靠它记录或查找场景状态。
            yield break;
        }
        // 步骤 41：计算 clip.channels，把结果存进 channels（channels）；之后引用 channels 时用的就是这次结果。
        int channels = clip.channels;
        // 步骤 42：执行 OnStateChanged?.Invoke("Transcribing English...");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnStateChanged?.Invoke("Transcribing English...");
        // 步骤 43：计算 Task.Run(() => Recognize(samples, channels))，把结果存进 recognition（recognition）；之后引用 recognition 时用的就是这次结果。
        Task<string> recognition = Task.Run(() => Recognize(samples, channels));
        // 步骤 44：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (!recognition.IsCompleted) yield return null;
        // 步骤 45：检查条件 recognition.IsFaulted；只有为真才运行下面的步骤，否则沿原流程继续。
        if (recognition.IsFaulted)
            // 步骤 46：执行 OnFailure?.Invoke("Offline recognition failed: " + recognition.Exception?.GetBaseException().Message；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnFailure?.Invoke("Offline recognition failed: " + recognition.Exception?.GetBaseException().Message);
        // 步骤 47：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 string.IsNullOrWhiteSpace(recognition.Result)；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (string.IsNullOrWhiteSpace(recognition.Result))
            // 步骤 48：执行 OnFailure?.Invoke("No English speech detected. Please try again.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnFailure?.Invoke("No English speech detected. Please try again.");
        // 步骤 49：前面的 if 条件为假时执行这里的备用处理。
        else OnRecognized?.Invoke(recognition.Result);
    }
    // ！！！功能结束：Record！！！

    // ！！！功能：Recognize——Recognize！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float[] samples, int channels 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 string 类型的计算结果；执行到 return 时把该值交回调用者。
    private string Recognize(float[] samples, int channels)
    {
        // 步骤 1：创建新的 byte[] 并存为 pcm；构造参数决定其初始内容或对象名称。
        byte[] pcm = new byte[(samples.Length / channels) * 2];
        // 步骤 2：按 int frame = 0; frame < pcm.Length / 2; frame++ 变化索引；循环体依次处理每个位置。
        for (int frame = 0; frame < pcm.Length / 2; frame++)
        {
            // 步骤 3：把数值 0f 暂存为 sum；它只在当前方法里参与后续判断或计算。
            float sum = 0f;
            // 步骤 4：按 int channel = 0; channel < channels; channel++ 变化索引；循环体依次处理每个位置。
            for (int channel = 0; channel < channels; channel++) sum += samples[frame * channels + channel];
            // 步骤 5：计算 value 并限制在安全上下界，防止不合理的大小影响移动或显示。
            short value = (short)(Mathf.Clamp(sum / channels, -1f, 1f) * short.MaxValue);
            // 步骤 6：执行 pcm[frame * 2] = (byte)value;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            pcm[frame * 2] = (byte)value;
            // 步骤 7：执行 pcm[frame * 2 + 1] = (byte)(value >> 8);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            pcm[frame * 2 + 1] = (byte)(value >> 8);
        }
        // 步骤 8：创建新的 var 并存为 text；构造参数决定其初始内容或对象名称。
        var text = new StringBuilder();
        // 步骤 9：在 using 括号中创建临时对象；离开下面的作用域时自动释放，防止资源泄漏。
        using (var recognizer = new Vosk.VoskRecognizer(model, SampleRate))
        {
            // 步骤 10：按 int offset = 0; offset < pcm.Length; offset += 8192 变化索引；循环体依次处理每个位置。
            for (int offset = 0; offset < pcm.Length; offset += 8192)
            {
                // 步骤 11：计算 Math.Min(8192, pcm.Length - offset)，把结果存进 count（count）；之后引用 count 时用的就是这次结果。
                int count = Math.Min(8192, pcm.Length - offset);
                // 步骤 12：创建新的 byte[] 并存为 chunk；构造参数决定其初始内容或对象名称。
                byte[] chunk = new byte[count];
                // 步骤 13：调用 BlockCopy：BlockCopy。
                Buffer.BlockCopy(pcm, offset, chunk, 0, count);
                // 步骤 14：检查条件 recognizer.AcceptWaveform(chunk, count)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 AppendText：AppendText。
                if (recognizer.AcceptWaveform(chunk, count)) AppendText(text, recognizer.Result());
            }
            // 步骤 15：调用 AppendText：AppendText。
            AppendText(text, recognizer.FinalResult());
        }
        // 步骤 16：把 text.ToString().Trim() 作为方法结果返回给调用方；return 后本方法立即结束。
        return text.ToString().Trim();
    }
    // ！！！功能结束：Recognize！！！

    // ！！！功能：AppendText——AppendText！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 StringBuilder text, string json 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void AppendText(StringBuilder text, string json)
    {
        // 步骤 1：计算 JsonUtility.FromJson<RecognitionResult>(json)，把结果存进 result（result）；之后引用 result 时用的就是这次结果。
        RecognitionResult result = JsonUtility.FromJson<RecognitionResult>(json);
        // 步骤 2：检查result是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 Append：Append。
        if (result != null && !string.IsNullOrWhiteSpace(result.text)) text.Append(result.text.Trim());
    }
    // ！！！功能结束：AppendText！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDestroy()
    {
        // 步骤 1：检查条件 Microphone.IsRecording(null)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 End：End。
        if (Microphone.IsRecording(null)) Microphone.End(null);
        // 步骤 2：检查modelTask是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (modelTask != null && !modelTask.IsCompleted) return;
        // 步骤 3：执行 model?.Dispose();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        model?.Dispose();
    }
    // ！！！功能结束：OnDestroy！！！
}
