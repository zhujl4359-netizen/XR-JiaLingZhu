// ！！！文件功能：Meta Voice 与离线语音识别的启动、回调和文字提交！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 System.Text 命名空间；下方可直接使用该库提供的类型和方法。
using System.Text;
// 引入 Meta.WitAi.Configuration 命名空间；下方可直接使用该库提供的类型和方法。
using Meta.WitAi.Configuration;
// 引入 Meta.WitAi.Data.Configuration 命名空间；下方可直接使用该库提供的类型和方法。
using Meta.WitAi.Data.Configuration;
// 引入 Oculus.Voice.Dictation 命名空间；下方可直接使用该库提供的类型和方法。
using Oculus.Voice.Dictation;
// 引入 Oculus.Voice.Dictation.Configuration 命名空间；下方可直接使用该库提供的类型和方法。
using Oculus.Voice.Dictation.Configuration;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 定义 SkyQuestSpeech 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestSpeech : MonoBehaviour
{
    // 执行 public event Action<string> OnRecognized;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action<string> OnRecognized;
    // 执行 public event Action<string> OnStatusChanged;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action<string> OnStatusChanged;
    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public string LastFailure { get; private set; }

    // witConfiguration 保存witConfiguration（类型 WitConfiguration）；下方方法依靠它记录或查找场景状态。
    private WitConfiguration witConfiguration;
    // dictation 保存dictation（类型 AppDictationExperience）；下方方法依靠它记录或查找场景状态。
    private AppDictationExperience dictation;
    // offline 保存offline（类型 SkyQuestOfflineSpeech）；下方方法依靠它记录或查找场景状态。
    private SkyQuestOfflineSpeech offline;
    // 声明 pending（pending）：文字内容；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private string pending;
    // 声明 hasPending（hasPending）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool hasPending;
    // 声明 listening（listening）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool listening;
    // 声明 initialized（initialized）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool initialized;

    // ！！！功能：Configure——配置！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 WitConfiguration configuration 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Configure(WitConfiguration configuration)
    {
        // 步骤 1：把计算结果 configuration 保存为witConfiguration；下一条判断或调用会读取这个具体值。
        witConfiguration = configuration;
    }
    // ！！！功能结束：Configure！！！

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Start()
    {
// 以下代码只编译进 Android 真机版本，并在 Unity 编辑器运行时跳过；Quest 使用这段调用系统接口。
#if UNITY_ANDROID && !UNITY_EDITOR
        // 步骤 1：把计算结果 gameObject.AddComponent<SkyQuestOfflineSpeech>() 保存为offline；下一条判断或调用会读取这个具体值。
        offline = gameObject.AddComponent<SkyQuestOfflineSpeech>();
        // 步骤 2：在OnRecognized原值上增加 OnOfflineTranscription；旧状态与本次输入共同决定新状态。
        offline.OnRecognized += OnOfflineTranscription;
        // 步骤 3：在OnFailure原值上增加 OnOfflineError；旧状态与本次输入共同决定新状态。
        offline.OnFailure += OnOfflineError;
        // 步骤 4：在On状态Changed原值上增加 OnOfflineState；旧状态与本次输入共同决定新状态。
        offline.OnStateChanged += OnOfflineState;
        // 步骤 5：调用 Preload：Preload。
        offline.Preload();
// 结束平台条件编译范围。
#endif
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：BeginRecognition——开始语音识别并等待 SDK 返回识别文字！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    // 原理：先选可用的语音识别提供者，再通过回调取得文字；录音结束前不能当作已发送。
    public void BeginRecognition()
    {
        // 步骤 1：检查条件 listening；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (listening) return;
        // 步骤 2：清空 LastFailure 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        LastFailure = null;
        // 步骤 3：把计算结果 witConfiguration != null ? witConfiguration : 保存为config；下一条判断或调用会读取这个具体值。
        WitConfiguration config = witConfiguration != null ? witConfiguration :
            // 步骤 4：执行 Resources.Load<WitConfiguration>("SkyQuestWitConfig");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Resources.Load<WitConfiguration>("SkyQuestWitConfig");
        // 步骤 5：检查config是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 config = null;
        if (config != null && string.IsNullOrWhiteSpace(config.GetClientAccessToken())) config = null;
// 以下代码只编译进 Android 真机版本，并在 Unity 编辑器运行时跳过；Quest 使用这段调用系统接口。
#if UNITY_ANDROID && !UNITY_EDITOR
        // 步骤 6：检查config是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (config == null)
        {
            // 步骤 7：检查offline是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
            if (offline == null)
            {
                // 步骤 8：把计算结果 gameObject.AddComponent<SkyQuestOfflineSpeech>() 保存为offline；下一条判断或调用会读取这个具体值。
                offline = gameObject.AddComponent<SkyQuestOfflineSpeech>();
                // 步骤 9：在OnRecognized原值上增加 OnOfflineTranscription；旧状态与本次输入共同决定新状态。
                offline.OnRecognized += OnOfflineTranscription;
                // 步骤 10：在OnFailure原值上增加 OnOfflineError；旧状态与本次输入共同决定新状态。
                offline.OnFailure += OnOfflineError;
                // 步骤 11：在On状态Changed原值上增加 OnOfflineState；旧状态与本次输入共同决定新状态。
                offline.OnStateChanged += OnOfflineState;
            }
            // 步骤 12：把 listening 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            listening = true;
            // 步骤 13：调用 BeginRecognition：开始语音识别并等待 SDK 返回识别文字。
            offline.BeginRecognition();
            // 步骤 14：提前结束这个方法；不再执行下方语句。
            return;
        }
// 结束平台条件编译范围。
#endif
// 以下代码只编译进 Android 真机版本，并在 Unity 编辑器运行时跳过；Quest 使用这段调用系统接口。
#if !UNITY_ANDROID || UNITY_EDITOR
        // 步骤 15：检查config是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (config == null)
        {
            // 步骤 16：把计算结果 "Voice input needs a linked Wit.ai config in the Editor. Quest uses the installed Meta platform dictation 保存为LastFailure；下一条判断或调用会读取这个具体值。
            LastFailure = "Voice input needs a linked Wit.ai config in the Editor. Quest uses the installed Meta platform dictation service.";
            // 步骤 17：调用 QueueResult：Queue结果。
            QueueResult("");
            // 步骤 18：提前结束这个方法；不再执行下方语句。
            return;
        }
// 结束平台条件编译范围。
#endif

        // 步骤 19：把 listening 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        listening = true;
        // 步骤 20：检查dictation是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (dictation == null || dictation.Configuration != config)
            // 步骤 21：调用 CreateDictation：创建Dictation。
            CreateDictation(config);
        // 步骤 22：这里用 && 要求两边条件同时成立：initialized && !dictation.Active && !dictation.MicActive；任意一边为假就跳过本分支。
        if (initialized && !dictation.Active && !dictation.MicActive)
            // 步骤 23：调用 Activate：首次交互关闭教程，并按物品类型选择拿取或普通信息面板。
            dictation.Activate();
    }
    // ！！！功能结束：BeginRecognition！！！

    // ！！！功能：CreateDictation——创建Dictation！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 WitConfiguration config 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateDictation(WitConfiguration config)
    {
        // 步骤 1：确认dictation已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (dictation != null) Destroy(dictation.gameObject);
        // 步骤 2：把 initialized 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        initialized = false;
        // 步骤 3：创建新的 GameObject 并存为 voiceObject；构造参数决定其初始内容或对象名称。
        GameObject voiceObject = new GameObject("Meta Voice dictation");
        // 步骤 4：执行 voiceObject.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        voiceObject.transform.SetParent(transform, false);
        // 步骤 5：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        voiceObject.SetActive(false);
        // 步骤 6：把计算结果 voiceObject.AddComponent<AppDictationExperience>() 保存为dictation；下一条判断或调用会读取这个具体值。
        dictation = voiceObject.AddComponent<AppDictationExperience>();
        // 步骤 7：新建用于RuntimeDictationConfiguration的对象或数值结构，并把构造参数保存进去。
        dictation.RuntimeDictationConfiguration = new WitDictationRuntimeConfiguration
        {
            // 步骤 8：把计算结果 config, 保存为witConfiguration；下一条判断或调用会读取这个具体值。
            witConfiguration = config,
            // 步骤 9：新建用于dictationConfiguration的对象或数值结构，并把构造参数保存进去。
            dictationConfiguration = new DictationConfiguration
            {
                // 步骤 10：把计算结果 false, 保存为multiPhrase；下一条判断或调用会读取这个具体值。
                multiPhrase = false,
                // 步骤 11：把计算结果 "messaging", 保存为scenario；下一条判断或调用会读取这个具体值。
                scenario = "messaging",
                // 步骤 12：把计算结果 "text_default" 保存为inputType；下一条判断或调用会读取这个具体值。
                inputType = "text_default"
            }
        };
// 以下代码只编译进 Android 真机版本，并在 Unity 编辑器运行时跳过；Quest 使用这段调用系统接口。
#if UNITY_ANDROID && !UNITY_EDITOR
        // 步骤 13：把计算结果 config == null 保存为DoNotFallback到Wit；下一条判断或调用会读取这个具体值。
        dictation.DoNotFallbackToWit = config == null;
        // 步骤 14：把 dictation.UsePlatformIntegrations 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        dictation.UsePlatformIntegrations = true;
// 结束平台条件编译范围。
#endif
        // 步骤 15：把处理方法登记为事件监听器；事件触发时 Unity 会调用它。
        dictation.DictationEvents.OnFullTranscription.AddListener(OnMetaTranscription);
        // 步骤 16：把处理方法登记为事件监听器；事件触发时 Unity 会调用它。
        dictation.DictationEvents.OnError.AddListener(OnMetaError);
        // 步骤 17：在OnInitialized原值上增加 OnMetaInitialized；旧状态与本次输入共同决定新状态。
        dictation.OnInitialized += OnMetaInitialized;
        // 步骤 18：显示这个场景对象及其子对象；界面会立即反映这一状态。
        voiceObject.SetActive(true);
    }
    // ！！！功能结束：CreateDictation！！！

    // ！！！功能：OnMetaInitialized——OnMetaInitialized！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnMetaInitialized()
    {
        // 步骤 1：把 initialized 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        initialized = true;
        // 步骤 2：这里用 && 要求两边条件同时成立：listening && dictation.isActiveAndEnabled；任意一边为假就跳过本分支。 成立时调用 Activate：首次交互关闭教程，并按物品类型选择拿取或普通信息面板。
        if (listening && dictation.isActiveAndEnabled) dictation.Activate();
    }
    // ！！！功能结束：OnMetaInitialized！！！

    // ！！！功能：OnMetaTranscription——接收 Meta Voice 返回的语音转写文字并交给聊天界面！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string text 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnMetaTranscription(string text)
    {
        // 步骤 1：这里的 ! 表示取反；当 listening 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!listening) return;
        // 步骤 2：把 listening 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        listening = false;
        // 步骤 3：调用 Deactivate：Deactivate。
        dictation.Deactivate();
        // 步骤 4：调用 QueueResult：Queue结果。
        QueueResult(text);
    }
    // ！！！功能结束：OnMetaTranscription！！！

    // ！！！功能：OnMetaError——OnMetaError！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string error, string message 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnMetaError(string error, string message)
    {
        // 步骤 1：这里的 ! 表示取反；当 listening 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!listening) return;
        // 步骤 2：把 listening 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        listening = false;
        // 步骤 3：把计算结果 string.IsNullOrWhiteSpace(message) ? error : message 保存为LastFailure；下一条判断或调用会读取这个具体值。
        LastFailure = string.IsNullOrWhiteSpace(message) ? error : message;
        // 步骤 4：调用 QueueResult：Queue结果。
        QueueResult("");
    }
    // ！！！功能结束：OnMetaError！！！

    // ！！！功能：OnOfflineTranscription——接收本地离线识别结果并交给聊天界面！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string text 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnOfflineTranscription(string text)
    {
        // 步骤 1：这里的 ! 表示取反；当 listening 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!listening) return;
        // 步骤 2：把 listening 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        listening = false;
        // 步骤 3：调用 QueueResult：Queue结果。
        QueueResult(text);
    }
    // ！！！功能结束：OnOfflineTranscription！！！

    // ！！！功能：OnOfflineError——OnOfflineError！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string message 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnOfflineError(string message)
    {
        // 步骤 1：这里的 ! 表示取反；当 listening 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!listening) return;
        // 步骤 2：把 listening 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        listening = false;
        // 步骤 3：把计算结果 message 保存为LastFailure；下一条判断或调用会读取这个具体值。
        LastFailure = message;
        // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.LogWarning("SkyQuest English voice: " + message);
        // 步骤 5：调用 QueueResult：Queue结果。
        QueueResult("");
    }
    // ！！！功能结束：OnOfflineError！！！

    // ！！！功能：OnOfflineState——OnOffline状态！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string message 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnOfflineState(string message)
    {
        // 步骤 1：检查条件 listening；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 OnStatusChanged?.Invoke(message);
        if (listening) OnStatusChanged?.Invoke(message);
    }
    // ！！！功能结束：OnOfflineState！！！

    // ！！！功能：QueueResult——Queue结果！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string value 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void QueueResult(string value)
    {
        // 步骤 1：把计算结果 EnglishOnly(value) 保存为pending；下一条判断或调用会读取这个具体值。
        pending = EnglishOnly(value);
        // 步骤 2：把 hasPending 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        hasPending = true;
    }
    // ！！！功能结束：QueueResult！！！

    // ！！！功能：EnglishOnly——EnglishOnly！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string value 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 string 类型的计算结果；执行到 return 时把该值交回调用者。
    private static string EnglishOnly(string value)
    {
        // 步骤 1：检查条件 string.IsNullOrEmpty(value)；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (string.IsNullOrEmpty(value)) return "";
        // 步骤 2：创建新的 StringBuilder 并存为 english；构造参数决定其初始内容或对象名称。
        StringBuilder english = new StringBuilder(value.Length);
        // 步骤 3：逐个读取 char c in value 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (char c in value)
        {
            // 步骤 4：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' ||
                // 步骤 5：这一行补充 c >= '0' && c <= '9' || c == ' ' || c == '\'' ||，与相邻行合起来完成同一次计算或对象配置。
                c >= '0' && c <= '9' || c == ' ' || c == '\'' ||
                // 步骤 6：把计算结果 = ',' || c == '.' || c == '?' || c == '!') english.Append(c) 保存为c；下一条判断或调用会读取这个具体值。
                c == ',' || c == '.' || c == '?' || c == '!') english.Append(c);
        }
        // 步骤 7：把 english.ToString().Trim() 作为方法结果返回给调用方；return 后本方法立即结束。
        return english.ToString().Trim();
    }
    // ！！！功能结束：EnglishOnly！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：这里的 ! 表示取反；当 hasPending 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!hasPending) return;
        // 步骤 2：计算 pending，把结果存进 result（result）；之后引用 result 时用的就是这次结果。
        string result = pending;
        // 步骤 3：把 hasPending 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        hasPending = false;
        // 步骤 4：执行 OnRecognized?.Invoke(result);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnRecognized?.Invoke(result);
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void OnDestroy()
    {
        // 步骤 1：确认offline已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (offline != null)
        {
            // 步骤 2：在OnRecognized原值上减少 OnOfflineTranscription；旧状态与本次输入共同决定新状态。
            offline.OnRecognized -= OnOfflineTranscription;
            // 步骤 3：在OnFailure原值上减少 OnOfflineError；旧状态与本次输入共同决定新状态。
            offline.OnFailure -= OnOfflineError;
            // 步骤 4：在On状态Changed原值上减少 OnOfflineState；旧状态与本次输入共同决定新状态。
            offline.OnStateChanged -= OnOfflineState;
        }
        // 步骤 5：检查dictation是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (dictation == null) return;
        // 步骤 6：在OnInitialized原值上减少 OnMetaInitialized；旧状态与本次输入共同决定新状态。
        dictation.OnInitialized -= OnMetaInitialized;
        // 步骤 7：先移除已有监听器，避免组件重新启用后同一事件触发多次。
        dictation.DictationEvents.OnFullTranscription.RemoveListener(OnMetaTranscription);
        // 步骤 8：先移除已有监听器，避免组件重新启用后同一事件触发多次。
        dictation.DictationEvents.OnError.RemoveListener(OnMetaError);
        // 步骤 9：检查条件 dictation.Active；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Cancel：Cancel。
        if (dictation.Active) dictation.Cancel();
    }
    // ！！！功能结束：OnDestroy！！！
}
