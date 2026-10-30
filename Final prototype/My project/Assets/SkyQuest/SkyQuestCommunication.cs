// ！！！文件功能：桌面与 VR 的话题聊天、语音输入和消息气泡！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 Meta.WitAi.Data.Configuration 命名空间；下方可直接使用该库提供的类型和方法。
using Meta.WitAi.Data.Configuration;
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;
// 引入 UnityEngine.Rendering 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.Rendering;
// 引入 UnityEngine.UI 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.UI;

// 定义 SkyQuestCommunication 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestCommunication : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 rightHand（右手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform rightHand;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    public SkyQuestStatus status;
    // itemTree 保存itemTree（类型 SkyQuestAncestor）；下方方法依靠它记录或查找场景状态。
    public SkyQuestAncestor itemTree;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;
    // 把 Inspector 可调整的 maxMessageLength（maxMessageLength）设为默认值 120f；具体用途看它在下方的判断和计算。
    public float maxMessageLength = 120f;
    // 声明 avatar（角色模型）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform avatar;
    // witConfiguration 保存witConfiguration（类型 WitConfiguration）；下方方法依靠它记录或查找场景状态。
    public WitConfiguration witConfiguration;

    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public string CurrentDraft { get; private set; } = "";
    // 创建新的 List<string> 并存为 SentMessages；构造参数决定其初始内容或对象名称。
    public readonly List<string> SentMessages = new List<string>();
    // 创建新的 List<float> 并存为 sentMessageTimes；构造参数决定其初始内容或对象名称。
    private readonly List<float> sentMessageTimes = new List<float>();
    // 创建新的 List<bool> 并存为 sentMessageOnLeft；构造参数决定其初始内容或对象名称。
    private readonly List<bool> sentMessageOnLeft = new List<bool>();
    // 定义不可在运行时改变的数值 MessageVisibleDuration（MessageVisibleDuration）；后续计算统一使用这个阈值、速度或尺寸。
    private const float MessageVisibleDuration = 10f;
    // 定义不可在运行时改变的数值 MessageFadeDuration（MessageFadeDuration）；后续计算统一使用这个阈值、速度或尺寸。
    private const float MessageFadeDuration = 2f;
    // IsOpen是 Inspector 中的开关，默认 > chatOpen || (panel != null && panel.activeSelf)；勾选状态决定相关功能是否运行。
    public bool IsOpen => chatOpen || (panel != null && panel.activeSelf);

    // 声明 panel（panel）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject panel;
    // 声明 keyboardBackingMaterial（keyboardBacking材质）：Unity 材质资源；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Material keyboardBackingMaterial;
    // draftText 保存draftText（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro draftText;
    // historyText 保存historyText（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro historyText;
    // 声明 ray（ray）：可见射线组件引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private LineRenderer ray;
    // focused 保存当前正在阅读或观察的物品（类型 SkyQuestKey）；下方方法依靠它记录或查找场景状态。
    private SkyQuestKey focused;
    // 声明 previousToggle（previousToggle）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousToggle;
    // 声明 previousTrigger（previousTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousTrigger;
    // 声明 nextToggle（nextToggle）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextToggle;
    // speech 保存speech（类型 SkyQuestSpeech）；下方方法依靠它记录或查找场景状态。
    private SkyQuestSpeech speech;
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion locomotion;
    // 声明 chatOpen（chatOpen）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool chatOpen;
    // 声明 tableChatOpen（tableChatOpen）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool tableChatOpen;
    // 声明 previousTableTrigger（previousTableTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousTableTrigger;
    // 声明 tableTriggerStarted（tableTriggerStarted）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float tableTriggerStarted;
    // activeTable 保存activeTable（类型 SkyQuestTeaTable）；下方方法依靠它记录或查找场景状态。
    private SkyQuestTeaTable activeTable;
    // 声明 previousLeftTrigger（previousLeftTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousLeftTrigger;
    // 声明 leftVoiceActivated（leftVoiceActivated）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool leftVoiceActivated;
    // 声明 leftTriggerStarted（leftTriggerStarted）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float leftTriggerStarted;
    // 声明 voiceMessagePending（voiceMessagePending）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool voiceMessagePending;
    // iconStyle 保存iconStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle iconStyle;
    // inputStyle 保存inputStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle inputStyle;
    // messageStyle 保存messageStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle messageStyle;
    // leftBubbleStyle 保存leftBubbleStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle leftBubbleStyle;
    // typingStyle 保存typingStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle typingStyle;
    // buttonStyle 保存buttonStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle buttonStyle;
    // bubbleLabelStyle 保存bubbleLabelStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle bubbleLabelStyle;
    // controlsBackdropStyle 保存controlsBackdropStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle controlsBackdropStyle;
    // keyboardPanelStyle 保存keyboard面板Style（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle keyboardPanelStyle;
    // keyboardKeyStyle 保存keyboardKeyStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle keyboardKeyStyle;
    // keyboardKeySelectedStyle 保存keyboardKeySelectedStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle keyboardKeySelectedStyle;
    // keyboardActionStyle 保存keyboardActionStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle keyboardActionStyle;
    // inputLabelStyle 保存inputLabelStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle inputLabelStyle;
    // chatBackground 保存chatBackground（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D chatBackground;
    // bubbleBackground 保存bubbleBackground（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D bubbleBackground;
    // typingBackground 保存typingBackground（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D typingBackground;
    // controlBackground 保存controlBackground（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D controlBackground;
    // keyboardBackground 保存keyboardBackground（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D keyboardBackground;
    // keyboardKeyBackground 保存keyboardKeyBackground（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D keyboardKeyBackground;
    // keyboardKeySelectedBackground 保存keyboardKeySelectedBackground（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D keyboardKeySelectedBackground;
    // keyboardActionBackground 保存keyboardActionBackground（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D keyboardActionBackground;
    // 声明 selectedKeyboardKey（selectedKeyboardKey）：整数计数；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private int selectedKeyboardKey;
    // highlightedKey 保存highlightedKey（类型 char）；下方方法依靠它记录或查找场景状态。
    private char highlightedKey;
    // 声明 highlightedKeyUntil（highlightedKeyUntil）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float highlightedKeyUntil;
    // 声明 nextGamepadNavigation（nextGamepadNavigation）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextGamepadNavigation;
    // boundKeyboard 保存boundKeyboard（类型 Keyboard）；下方方法依靠它记录或查找场景状态。
    private Keyboard boundKeyboard;
    // 声明 vrBubbleCanvas（vrBubbleCanvas）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject vrBubbleCanvas;
    // 创建新的 List<TextMeshProUGUI> 并存为 vrBubbleTexts；构造参数决定其初始内容或对象名称。
    private readonly List<TextMeshProUGUI> vrBubbleTexts = new List<TextMeshProUGUI>();
    // 创建新的 List<RectTransform> 并存为 vrBubbleRects；构造参数决定其初始内容或对象名称。
    private readonly List<RectTransform> vrBubbleRects = new List<RectTransform>();
    // 创建新的 List<CanvasGroup> 并存为 vrBubbleGroups；构造参数决定其初始内容或对象名称。
    private readonly List<CanvasGroup> vrBubbleGroups = new List<CanvasGroup>();
    // 用 IsFirstPerson 记住此处真假结果（> locomotion != null && !locomotion.ThirdPerson），供下一条 if 选择执行路径。
    private bool IsFirstPerson => locomotion != null && !locomotion.ThirdPerson;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：检查头显相机、右手柄位置是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (head == null || rightHand == null) return;
        // 步骤 2：调用 EnsureChineseFont：确保ChineseFont。
        SkyQuestVisuals.EnsureChineseFont();
        // 步骤 3：把计算结果 FindFirstObjectByType<SkyQuestLocomotion>() 保存为locomotion；下一条判断或调用会读取这个具体值。
        locomotion = FindFirstObjectByType<SkyQuestLocomotion>();
        // 步骤 4：检查角色模型是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (avatar == null)
        {
            // 步骤 5：检查locomotion、locomotion的角色模型是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 avatar = locomotion.avatar.transform;
            if (locomotion != null && locomotion.avatar != null) avatar = locomotion.avatar.transform;
        }
        // 步骤 6：把计算结果 gameObject.AddComponent<SkyQuestSpeech>() 保存为speech；下一条判断或调用会读取这个具体值。
        speech = gameObject.AddComponent<SkyQuestSpeech>();
        // 步骤 7：调用 Configure：配置。
        speech.Configure(witConfiguration);
        // 步骤 8：在OnRecognized原值上增加 OnVoiceRecognized；旧状态与本次输入共同决定新状态。
        speech.OnRecognized += OnVoiceRecognized;
        // 步骤 9：在On状态Changed原值上增加 OnVoiceStatus；旧状态与本次输入共同决定新状态。
        speech.OnStatusChanged += OnVoiceStatus;
        // 步骤 10：调用 BuildPanel：构建面板。
        BuildPanel();
        // 步骤 11：调用 BuildRay：构建射线。
        BuildRay();
        // 步骤 12：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 成立时调用 BuildVrBubbles：构建VrBubbles。
        if (!desktopMode) BuildVrBubbles();
        // 步骤 13：调用 RefreshText：刷新Text。
        RefreshText();
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：调用 ExpireOldMessages：ExpireOldMessages。
        ExpireOldMessages();
        // 步骤 2：调用 UpdateVrBubbles：更新VrBubbles。
        UpdateVrBubbles();
        // 步骤 3：检查panel是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (panel == null) return;
        // 步骤 4：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 成立时调用 UpdateLeftTriggerVoice：更新LeftTriggerVoice。
        if (!desktopMode) UpdateLeftTriggerVoice();
        // 步骤 5：检查itemTree是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (itemTree != null && itemTree.IsMenuOpen && IsOpen)
        {
            // 步骤 6：把 chatOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            chatOpen = false;
            // 步骤 7：把 tableChatOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            tableChatOpen = false;
            // 步骤 8：清空 activeTable 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            activeTable = null;
            // 步骤 9：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            panel.SetActive(false);
        }
        // 步骤 10：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 成立时调用 PlacePanel：Place面板。
        if (!desktopMode) PlacePanel();
        // 步骤 11：这里的 ! 表示取反；当 IsOpen 不成立时才进入此分支。 成立时调用 UpdateTeaTableClick：更新TeaTableClick。
        if (!IsOpen) UpdateTeaTableClick();
        // 步骤 12：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 13：调用 BindDesktopKeyboard：Bind桌面Keyboard。
            BindDesktopKeyboard();
            // 步骤 14：调用 UpdateDesktopKeyboard：更新桌面Keyboard。
            UpdateDesktopKeyboard();
            // 步骤 15：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 16：用 toggle 记住此处真假结果（SkyQuestInputExtensions.StickClick(SkyQuestInput.Left)），供下一条 if 选择执行路径。
        bool toggle = SkyQuestInputExtensions.StickClick(SkyQuestInput.Left);
        // 步骤 17：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (toggle && !previousToggle && Time.time > nextToggle &&
            // 步骤 18：这一行补充 (itemTree == null || !itemTree.IsMenuOpen))，与相邻行合起来完成同一次计算或对象配置。
            (itemTree == null || !itemTree.IsMenuOpen))
        {
            // 步骤 19：调用 DesktopToggle：桌面Toggle。
            DesktopToggle();
            // 步骤 20：根据当前游戏时间计算并保存nextToggle，用于后续延迟或冷却判断。
            nextToggle = Time.time + 0.4f;
        }
        // 步骤 21：把计算结果 toggle 保存为previousToggle；下一条判断或调用会读取这个具体值。
        previousToggle = toggle;
        // 步骤 22：这里的 ! 表示取反；当 panel.activeSelf 不成立时才进入此分支。
        if (!panel.activeSelf)
        {
            // 步骤 23：确认ray已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 ray.enabled = false;
            if (ray != null) ray.enabled = false;
            // 步骤 24：把计算结果 SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f 保存为previousTrigger；下一条判断或调用会读取这个具体值。
            previousTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
            // 步骤 25：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 26：调用 UpdatePointer：更新Pointer。
        UpdatePointer();
        // 步骤 27：用 trigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f），供下一条 if 选择执行路径。
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        // 步骤 28：检查当前正在阅读或观察的物品是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 Press：Press。
        if (trigger && !previousTrigger && focused != null) Press(focused.value);
        // 步骤 29：把计算结果 trigger 保存为previousTrigger；下一条判断或调用会读取这个具体值。
        previousTrigger = trigger;
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：BuildPanel——构建面板！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BuildPanel()
    {
        // 步骤 1：新建用于panel的对象或数值结构，并把构造参数保存进去。
        panel = new GameObject("Spatial communication panel");
        // 步骤 2：执行 panel.transform.SetParent(head, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        panel.transform.SetParent(head, false);
        // 步骤 3：执行 panel.transform.localPosition = new Vector3(0f, -0.04f, 1.05f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        panel.transform.localPosition = new Vector3(0f, -0.04f, 1.05f);
        // 步骤 4：把计算结果 SkyQuestVisuals.Primitive("Panel background", PrimitiveType.Cube, panel.transform, 保存为keyboardBacking；下一条判断或调用会读取这个具体值。
        GameObject keyboardBacking = SkyQuestVisuals.Primitive("Panel background", PrimitiveType.Cube, panel.transform,
            // 步骤 5：用 new 创建 Vector3(0f, 0f, 0.06f), new Vector3(desktopMode ? 0.95f : 1.3f, desktopMode ；构造参数决定新对象的初始内容。
            new Vector3(0f, 0f, 0.06f), new Vector3(desktopMode ? 0.95f : 1.3f, desktopMode ? 0.78f : 0.96f, 0.02f),
            // 步骤 6：用 new 创建 Color(0.12f, 0.15f, 0.22f));；构造参数决定新对象的初始内容。
            new Color(0.12f, 0.15f, 0.22f));
        // 步骤 7：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。
        if (!desktopMode)
        {
            // The keyboard backing must actually blend in URP; an alpha-only
            // tint on the default opaque Lit material still appears solid.
            // 步骤 8：创建新的 Color 并存为 tint；构造参数决定其初始内容或对象名称。
            Color tint = new Color(.61f, .82f, .96f, .43f);
            // 步骤 9：从目标 GameObject 查找并保存 keyboardBacking材质 组件；后面的移动或显示逻辑会直接使用这个缓存。
            keyboardBackingMaterial = new Material(keyboardBacking.GetComponent<Renderer>().sharedMaterial);
            // 步骤 10：把计算结果 tint 保存为color；下一条判断或调用会读取这个具体值。
            keyboardBackingMaterial.color = tint;
            // 步骤 11：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            keyboardBackingMaterial.SetFloat("_Surface", 1f);
            // 步骤 12：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            keyboardBackingMaterial.SetFloat("_Blend", 0f);
            // 步骤 13：调用 SetInt：设置Int。
            keyboardBackingMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            // 步骤 14：调用 SetInt：设置Int。
            keyboardBackingMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            // 步骤 15：调用 SetInt：设置Int。
            keyboardBackingMaterial.SetInt("_ZWrite", 0);
            // 步骤 16：调用 SetOverrideTag：设置OverrideTag。
            keyboardBackingMaterial.SetOverrideTag("RenderType", "Transparent");
            // 步骤 17：调用 EnableKeyword：EnableKeyword。
            keyboardBackingMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            // 步骤 18：把计算结果 (int)RenderQueue.Transparent 保存为renderQueue；下一条判断或调用会读取这个具体值。
            keyboardBackingMaterial.renderQueue = (int)RenderQueue.Transparent;
            // 步骤 19：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            keyboardBacking.GetComponent<Renderer>().sharedMaterial = keyboardBackingMaterial;
        }
        // 步骤 20：调用 Text：Text。
        SkyQuestVisuals.Text("Communication title", desktopMode ? "KEYBOARD COMMUNICATION" : "SKY COMMUNICATION", panel.transform,
            // 步骤 21：用 new 创建 Vector3(0f, desktopMode ? 0.32f : 0.41f, 0f), 2.8f, new Color(1f, 0.9f, 0.65；构造参数决定新对象的初始内容。
            new Vector3(0f, desktopMode ? 0.32f : 0.41f, 0f), 2.8f, new Color(1f, 0.9f, 0.65f), 1.2f);
        // 步骤 22：把计算结果 SkyQuestVisuals.Text("Draft", "", panel.transform, 保存为draftText；下一条判断或调用会读取这个具体值。
        draftText = SkyQuestVisuals.Text("Draft", "", panel.transform,
            // 步骤 23：用 new 创建 Vector3(0f, desktopMode ? 0.21f : 0.29f, 0f), 2.2f, Color.white, 1.12f);；构造参数决定新对象的初始内容。
            new Vector3(0f, desktopMode ? 0.21f : 0.29f, 0f), 2.2f, Color.white, 1.12f);
        // 步骤 24：把计算结果 SkyQuestVisuals.Text("Last message", "", panel.transform, 保存为historyText；下一条判断或调用会读取这个具体值。
        historyText = SkyQuestVisuals.Text("Last message", "", panel.transform,
            // 步骤 25：用 new 创建 Vector3(0f, desktopMode ? -0.29f : -0.38f, 0f), 1.8f, new Color(0.8f, 0.95f,；构造参数决定新对象的初始内容。
            new Vector3(0f, desktopMode ? -0.29f : -0.38f, 0f), 1.8f, new Color(0.8f, 0.95f, 1f), 1.12f);
        // 步骤 26：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 27：调用 Text：Text。
            SkyQuestVisuals.Text("Keyboard hint", "TYPE • ENTER SENDS • ESC CLOSES",
                // 步骤 28：执行 panel.transform, new Vector3(0f, -0.05f, 0f), 2f, Color.white, 0.85f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                panel.transform, new Vector3(0f, -0.05f, 0f), 2f, Color.white, 0.85f);
            // 步骤 29：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            panel.SetActive(false);
            // 步骤 30：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 31：计算 { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" }，把结果存进 rows（rows）；之后引用 rows 时用的就是这次结果。
        string[] rows = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
        // 步骤 32：按 int row = 0; row < rows.Length; row++ 变化索引；循环体依次处理每个位置。
        for (int row = 0; row < rows.Length; row++)
        {
            // 步骤 33：计算 rows[row]，把结果存进 keys（keys）；之后引用 keys 时用的就是这次结果。
            string keys = rows[row];
            // 步骤 34：把数值 0.11f 暂存为 width；它只在当前方法里参与后续判断或计算。
            float width = 0.11f;
            // 步骤 35：按 int col = 0; col < keys.Length; col++ 变化索引；循环体依次处理每个位置。
            for (int col = 0; col < keys.Length; col++)
                // 步骤 36：调用 AddKey：添加Key。
                AddKey(keys[col].ToString(), (col - (keys.Length - 1) * 0.5f) * width,
                    // 步骤 37：执行 0.13f - row * 0.11f, 0.095f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0.13f - row * 0.11f, 0.095f);
        }
        // 步骤 38：调用 AddKey：添加Key。
        AddKey("SPACE", -0.36f, -0.23f, 0.27f);
        // 步骤 39：调用 AddKey：添加Key。
        AddKey("DEL", -0.08f, -0.23f, 0.17f);
        // 步骤 40：调用 AddKey：添加Key。
        AddKey("SEND", 0.20f, -0.23f, 0.19f);
        // 步骤 41：调用 AddKey：添加Key。
        AddKey("MIC", 0.47f, -0.23f, 0.15f);
        // 步骤 42：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        panel.SetActive(false);
    }
    // ！！！功能结束：BuildPanel！！！

    // ！！！功能：BuildVrBubbles——构建VrBubbles！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BuildVrBubbles()
    {
        // IMGUI is visible in the desktop mirror but is not reliable in both
        // XR eyes. Render the same messages on a stereo world-space canvas.
        // 步骤 1：新建用于vrBubbleCanvas的对象或数值结构，并把构造参数保存进去。
        vrBubbleCanvas = new GameObject("Visible message bubbles",
            // 步骤 2：调用 typeof：typeof。
            typeof(RectTransform), typeof(Canvas));
        // 步骤 3：从目标物体找到所需组件并存为 canvasRect；使用前应检查是否为 null。
        RectTransform canvasRect = vrBubbleCanvas.GetComponent<RectTransform>();
        // 步骤 4：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        canvasRect.SetParent(head, false);
        // 步骤 5：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        canvasRect.localPosition = new Vector3(-.31f, -.29f, 1.3f);
        // 步骤 6：恢复 canvasRect.localRotation 到初始本地朝向；本地表示相对于父对象。
        canvasRect.localRotation = Quaternion.identity;
        // 步骤 7：恢复 canvasRect.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        canvasRect.localScale = Vector3.one * .0007f;
        // 步骤 8：新建用于pivot的对象或数值结构，并把构造参数保存进去。
        canvasRect.pivot = new Vector2(.5f, 1f);
        // 步骤 9：新建用于sizeDelta的对象或数值结构，并把构造参数保存进去。
        canvasRect.sizeDelta = new Vector2(640f, 560f);
        // 步骤 10：从目标物体找到所需组件并存为 canvas；使用前应检查是否为 null。
        Canvas canvas = vrBubbleCanvas.GetComponent<Canvas>();
        // 步骤 11：把计算结果 RenderMode.WorldSpace 保存为render模式；下一条判断或调用会读取这个具体值。
        canvas.renderMode = RenderMode.WorldSpace;
        // 步骤 12：从目标 GameObject 查找并保存 world相机 组件；后面的移动或显示逻辑会直接使用这个缓存。
        canvas.worldCamera = head.GetComponent<Camera>();
        // 步骤 13：把 canvas.overrideSorting 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        canvas.overrideSorting = true;
        // 步骤 14：把计算结果 80 保存为sortingOrder；下一条判断或调用会读取这个具体值。
        canvas.sortingOrder = 80;

        // 步骤 15：按 int i = 0; i < 4; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 4; i++)
        {
            // 步骤 16：新建用于bubble的对象或数值结构，并把构造参数保存进去。
            GameObject bubble = new GameObject("Message " + (i + 1),
                // 步骤 17：调用 typeof：typeof。
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            // 步骤 18：从目标物体找到所需组件并存为 bubbleRect；使用前应检查是否为 null。
            RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
            // 步骤 19：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
            bubbleRect.SetParent(canvasRect, false);
            // 步骤 20：新建用于anchorMin的对象或数值结构，并把构造参数保存进去。
            bubbleRect.anchorMin = bubbleRect.anchorMax = new Vector2(0f, 1f);
            // 步骤 21：新建用于pivot的对象或数值结构，并把构造参数保存进去。
            bubbleRect.pivot = new Vector2(0f, 1f);
            // 步骤 22：从目标物体找到所需组件并存为 background；使用前应检查是否为 null。
            Image background = bubble.GetComponent<Image>();
            // 步骤 23：新建用于color的对象或数值结构，并把构造参数保存进去。
            background.color = new Color(.96f, .77f, .84f, .78f);
            // 步骤 24：把 background.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            background.raycastTarget = false;
            // 步骤 25：把括号里的新元素加入 vrBubbleRects 集合；后续会按集合顺序使用它。
            vrBubbleRects.Add(bubbleRect);
            // 步骤 26：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            vrBubbleGroups.Add(bubble.GetComponent<CanvasGroup>());

            // 步骤 27：新建用于words的对象或数值结构，并把构造参数保存进去。
            GameObject words = new GameObject("Words", typeof(RectTransform),
                // 步骤 28：调用 typeof：typeof。
                typeof(TextMeshProUGUI));
            // 步骤 29：从目标物体找到所需组件并存为 wordsRect；使用前应检查是否为 null。
            RectTransform wordsRect = words.GetComponent<RectTransform>();
            // 步骤 30：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
            wordsRect.SetParent(bubbleRect, false);
            // 步骤 31：把 wordsRect.anchorMin 清为零向量；清除上次移动、投掷或位置偏移的残留值。
            wordsRect.anchorMin = Vector2.zero;
            // 步骤 32：把计算结果 Vector2.one 保存为anchorMax；下一条判断或调用会读取这个具体值。
            wordsRect.anchorMax = Vector2.one;
            // 步骤 33：新建用于offsetMin的对象或数值结构，并把构造参数保存进去。
            wordsRect.offsetMin = new Vector2(19f, 8f);
            // 步骤 34：新建用于offsetMax的对象或数值结构，并把构造参数保存进去。
            wordsRect.offsetMax = new Vector2(-19f, -8f);
            // 步骤 35：从目标物体找到所需组件并存为 label；使用前应检查是否为 null。
            TextMeshProUGUI label = words.GetComponent<TextMeshProUGUI>();
            // 步骤 36：把计算结果 29f 保存为fontSize；下一条判断或调用会读取这个具体值。
            label.fontSize = 29f;
            // 步骤 37：新建用于color的对象或数值结构，并把构造参数保存进去。
            label.color = new Color(.08f, .25f, .67f);
            // 步骤 38：把计算结果 TextAlignmentOptions.MidlineLeft 保存为alignment；下一条判断或调用会读取这个具体值。
            label.alignment = TextAlignmentOptions.MidlineLeft;
            // 步骤 39：把 label.enableWordWrapping 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            label.enableWordWrapping = true;
            // 步骤 40：把 label.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            label.raycastTarget = false;
            // 步骤 41：把括号里的新元素加入 vrBubbleTexts 集合；后续会按集合顺序使用它。
            vrBubbleTexts.Add(label);
            // 步骤 42：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            bubble.SetActive(false);
        }
        // 步骤 43：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        vrBubbleCanvas.SetActive(false);
    }
    // ！！！功能结束：BuildVrBubbles！！！

    // ！！！功能：UpdateVrBubbles——更新VrBubbles！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateVrBubbles()
    {
        // 步骤 1：检查vrBubbleCanvas是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (vrBubbleCanvas == null) return;
        // 步骤 2：用 visible 记住此处真假结果（SentMessages.Count > 0），供下一条 if 选择执行路径。
        bool visible = SentMessages.Count > 0;
        // 步骤 3：检查条件 vrBubbleCanvas.activeSelf != visible；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetActive：设置Active。
        if (vrBubbleCanvas.activeSelf != visible) vrBubbleCanvas.SetActive(visible);
        // 步骤 4：这里的 ! 表示取反；当 visible 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!visible) return;

        // 步骤 5：从目标物体找到所需组件并存为 canvasRect；使用前应检查是否为 null。
        RectTransform canvasRect = vrBubbleCanvas.GetComponent<RectTransform>();
        // 步骤 6：用三元表达式为 anchor 二选一：问号前条件为真取前值，为假取冒号后的值。
        Transform anchor = IsFirstPerson ? head : avatar;
        // 步骤 7：检查anchor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 anchor = head;
        if (anchor == null) anchor = head;
        // 步骤 8：检查条件 canvasRect.parent != anchor；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        if (canvasRect.parent != anchor) canvasRect.SetParent(anchor, false);
        // 步骤 9：检查条件 IsFirstPerson；只有为真才运行下面的步骤，否则沿原流程继续。
        if (IsFirstPerson)
        {
            // The controls drawer ends near y=-.24. Start the message feed
            // just below it, clear of the energy fan and right-side keyboard.
            // 步骤 10：新建用于pivot的对象或数值结构，并把构造参数保存进去。
            canvasRect.pivot = new Vector2(.5f, 1f);
            // 步骤 11：新建用于local位置的对象或数值结构，并把构造参数保存进去。
            canvasRect.localPosition = new Vector3(-.31f, -.29f, 1.3f);
            // 步骤 12：恢复 canvasRect.localRotation 到初始本地朝向；本地表示相对于父对象。
            canvasRect.localRotation = Quaternion.identity;
            // 步骤 13：恢复 canvasRect.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            canvasRect.localScale = Vector3.one * .0007f;
        }
        // 步骤 14：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 15：新建用于pivot的对象或数值结构，并把构造参数保存进去。
            canvasRect.pivot = new Vector2(.5f, 0f);
            // 步骤 16：新建用于local位置的对象或数值结构，并把构造参数保存进去。
            canvasRect.localPosition = new Vector3(0f, 2.35f, 0f);
            // 步骤 17：把rotation更新为 Quaternion.LookRotation(canvasRect.position - head.position,；物体朝向会在本帧改变。
            canvasRect.rotation = Quaternion.LookRotation(canvasRect.position - head.position,
                // 步骤 18：执行 Vector3.up);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.up);
            // 步骤 19：恢复 canvasRect.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            canvasRect.localScale = Vector3.one * .0016f;
        }

        // 步骤 20：把数值 0f 暂存为 offset；它只在当前方法里参与后续判断或计算。
        float offset = 0f;
        // 步骤 21：按 int row = 0; row < vrBubbleTexts.Count; row++ 变化索引；循环体依次处理每个位置。
        for (int row = 0; row < vrBubbleTexts.Count; row++)
        {
            // Put the newest line first so speech arriving after transcription
            // appears in the most prominent position instead of below old text.
            // 步骤 22：计算 SentMessages.Count - 1 - row，把结果存进 index（index）；之后引用 index 时用的就是这次结果。
            int index = SentMessages.Count - 1 - row;
            // 步骤 23：用 hasMessage 记住此处真假结果（index >= 0），供下一条 if 选择执行路径。
            bool hasMessage = index >= 0;
            // 步骤 24：计算 vrBubbleRects[row].gameObject，把结果存进 bubble（bubble）；之后引用 bubble 时用的就是这次结果。
            GameObject bubble = vrBubbleRects[row].gameObject;
            // 步骤 25：检查条件 bubble.activeSelf != hasMessage；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetActive：设置Active。
            if (bubble.activeSelf != hasMessage) bubble.SetActive(hasMessage);
            // 步骤 26：这里的 ! 表示取反；当 hasMessage 不成立时才进入此分支。 成立时执行 continue;
            if (!hasMessage) continue;

            // 步骤 27：计算 vrBubbleTexts[row]，把结果存进 label（label）；之后引用 label 时用的就是这次结果。
            TextMeshProUGUI label = vrBubbleTexts[row];
            // 步骤 28：计算 SentMessages[index]，把结果存进 message（message）；之后引用 message 时用的就是这次结果。
            string message = SentMessages[index];
            // 步骤 29：检查条件 label.text != message；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 label.text = message;
            if (label.text != message) label.text = message;
            // 步骤 30：计算height并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            float height = Mathf.Clamp(label.GetPreferredValues(message, 590f, 0f).y + 23f,
                // 步骤 31：执行 58f, 125f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                58f, 125f);
            // 步骤 32：执行 vrBubbleRects[row].sizeDelta = new Vector2(620f, height);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            vrBubbleRects[row].sizeDelta = new Vector2(620f, height);
            // 步骤 33：执行 vrBubbleRects[row].anchoredPosition = new Vector2(0f, -offset);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            vrBubbleRects[row].anchoredPosition = new Vector2(0f, -offset);
            // 步骤 34：在offset原值上增加 height + 9f；旧状态与本次输入共同决定新状态。
            offset += height + 9f;
            // 步骤 35：计算 Time.unscaledTime - sentMessageTimes[index]，把结果存进 age（age）；之后引用 age 时用的就是这次结果。
            float age = Time.unscaledTime - sentMessageTimes[index];
            // 步骤 36：这一行补充 vrBubbleGroups[row].alpha = age <= MessageVisibleDuration ? 1f :，与相邻行合起来完成同一次计算或对象配置。
            vrBubbleGroups[row].alpha = age <= MessageVisibleDuration ? 1f :
                // 步骤 37：执行 1f - Mathf.Clamp01((age - MessageVisibleDuration) / MessageFadeDuration);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                1f - Mathf.Clamp01((age - MessageVisibleDuration) / MessageFadeDuration);
        }
    }
    // ！！！功能结束：UpdateVrBubbles！！！

    // ！！！功能：AddKey——添加Key！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string value, float x, float y, float width 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void AddKey(string value, float x, float y, float width)
    {
        // 步骤 1：把计算结果 SkyQuestVisuals.Primitive("Key " + value, PrimitiveType.Cube, 保存为key；下一条判断或调用会读取这个具体值。
        GameObject key = SkyQuestVisuals.Primitive("Key " + value, PrimitiveType.Cube,
            // 步骤 2：继续填写上一行调用的参数：panel.transform, new Vector3(x, y, -0.025f), new Vector3(width, 0.088f, 0.025f)；参数位置决定它表示的坐标、速度或选项。
            panel.transform, new Vector3(x, y, -0.025f), new Vector3(width, 0.088f, 0.025f),
            // 步骤 3：用 new 创建 Color(0.31f, 0.42f, 0.55f));；构造参数决定新对象的初始内容。
            new Color(0.31f, 0.42f, 0.55f));
        // 步骤 4：执行 key.AddComponent<SkyQuestKey>().value = value;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        key.AddComponent<SkyQuestKey>().value = value;
        // Keep labels outside the scaled key mesh: as a child their glyphs were
        // scaled a second time and appeared blank or illegible in the headset.
        // 步骤 5：调用 Text：Text。
        SkyQuestVisuals.Text("Label " + value, value, panel.transform,
            // 步骤 6：用 new 创建 Vector3(x, y, -0.055f), value.Length == 1 ? 3f : 2.2f,；构造参数决定新对象的初始内容。
            new Vector3(x, y, -0.055f), value.Length == 1 ? 3f : 2.2f,
            // 步骤 7：执行 Color.white, width / 0.12f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Color.white, width / 0.12f);
    }
    // ！！！功能结束：AddKey！！！

    // ！！！功能：BuildRay——构建射线！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BuildRay()
    {
        // 步骤 1：创建新的 GameObject 并存为 pointer；构造参数决定其初始内容或对象名称。
        GameObject pointer = new GameObject("Communication pointer", typeof(LineRenderer));
        // 步骤 2：从目标 GameObject 查找并保存 ray 组件；后面的移动或显示逻辑会直接使用这个缓存。
        ray = pointer.GetComponent<LineRenderer>();
        // 步骤 3：把positionCount更新为 2；世界或局部位置会立即影响物体摆放。
        ray.positionCount = 2;
        // 步骤 4：把计算结果 0.007f 保存为startWidth；下一条判断或调用会读取这个具体值。
        ray.startWidth = 0.007f;
        // 步骤 5：把计算结果 0.003f 保存为endWidth；下一条判断或调用会读取这个具体值。
        ray.endWidth = 0.003f;
        // 步骤 6：新建用于material的对象或数值结构，并把构造参数保存进去。
        ray.material = SkyQuestVisuals.Material(new Color(0.6f, 0.9f, 1f));
        // 步骤 7：把 ray.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        ray.enabled = false;
    }
    // ！！！功能结束：BuildRay！！！

    // ！！！功能：UpdatePointer——更新Pointer！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdatePointer()
    {
        // 步骤 1：清空 focused 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        focused = null;
        // 步骤 2：把 ray.enabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        ray.enabled = true;
        // 步骤 3：计算 rightHand.position + rightHand.forward * 3f，把结果存进 end（end）；之后引用 end 时用的就是这次结果。
        Vector3 end = rightHand.position + rightHand.forward * 3f;
        // 步骤 4：计算 float.PositiveInfinity，把结果存进 nearest（nearest）；之后引用 nearest 时用的就是这次结果。
        float nearest = float.PositiveInfinity;
        // 步骤 5：逐个读取 RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 3f) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 3f))
        {
            // 步骤 6：从目标物体找到所需组件并存为 key；使用前应检查是否为 null。
            SkyQuestKey key = hit.collider.GetComponent<SkyQuestKey>();
            // 步骤 7：检查key是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (key == null || hit.distance >= nearest) continue;
            // 步骤 8：把计算结果 hit.distance 保存为nearest；下一条判断或调用会读取这个具体值。
            nearest = hit.distance;
            // 步骤 9：把计算结果 key 保存为当前正在阅读或观察的物品；下一条判断或调用会读取这个具体值。
            focused = key;
            // 步骤 10：把计算结果 hit.point 保存为end；下一条判断或调用会读取这个具体值。
            end = hit.point;
        }
        // 步骤 11：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
        ray.SetPosition(0, rightHand.position);
        // 步骤 12：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
        ray.SetPosition(1, end);
    }
    // ！！！功能结束：UpdatePointer！！！

    // ！！！功能：Press——Press！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string value 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Press(string value)
    {
        // 步骤 1：检查条件 value == "DEL"；只有为真才运行下面的步骤，否则沿原流程继续。
        if (value == "DEL")
        {
            // 步骤 2：检查条件 CurrentDraft.Length > 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 CurrentDraft = CurrentDraft.Substring(0, CurrentDraft.Length - 1);
            if (CurrentDraft.Length > 0) CurrentDraft = CurrentDraft.Substring(0, CurrentDraft.Length - 1);
        }
        // 步骤 3：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：value == "SPACE" && CurrentDraft.Length < maxMessageLength；任意一边为假就跳过本分支。
        else if (value == "SPACE" && CurrentDraft.Length < maxMessageLength) CurrentDraft += " ";
        // 步骤 4：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 value == "SEND"；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (value == "SEND")
        {
            // 步骤 5：这里的 ! 表示取反；当 string.IsNullOrWhiteSpace(CurrentDraft) 不成立时才进入此分支。
            if (!string.IsNullOrWhiteSpace(CurrentDraft))
            {
                // 步骤 6：调用 AddSentMessage：添加SentMessage。
                AddSentMessage(CurrentDraft, false);
                // 步骤 7：把计算结果 "" 保存为CurrentDraft；下一条判断或调用会读取这个具体值。
                CurrentDraft = "";
            }
        }
        // 步骤 8：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 value == "MIC"；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (value == "MIC")
        {
            // 步骤 9：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log("SkyQuest MIC key pressed");
            // 步骤 10：检查条件 IsFirstPerson；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 BeginVoiceMessage：BeginVoiceMessage。
            if (IsFirstPerson) BeginVoiceMessage();
            // 步骤 11：前面的 if 条件为假时执行这里的备用处理。
            else
            {
                // 步骤 12：执行 status?.Show("Listening — speak now.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                status?.Show("Listening — speak now.", 5f);
                // 步骤 13：执行 speech?.BeginRecognition();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                speech?.BeginRecognition();
            }
        }
        // 步骤 14：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 CurrentDraft.Length < maxMessageLength；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (CurrentDraft.Length < maxMessageLength) CurrentDraft += value;
        // 步骤 15：调用 RefreshText：刷新Text。
        RefreshText();
        // 步骤 16：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.2f, 0.04f);
    }
    // ！！！功能结束：Press！！！

    // ！！！功能：OnVoiceRecognized——把识别出的语音文字当作玩家消息显示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string result 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnVoiceRecognized(string result)
    {
        // 步骤 1：检查条件 voiceMessagePending；只有为真才运行下面的步骤，否则沿原流程继续。
        if (voiceMessagePending)
        {
            // 步骤 2：把 voiceMessagePending 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            voiceMessagePending = false;
            // 步骤 3：检查条件 string.IsNullOrWhiteSpace(result)；只有为真才运行下面的步骤，否则沿原流程继续。
            if (string.IsNullOrWhiteSpace(result))
            {
                // 步骤 4：这一行补充 status?.Show(speech != null && !string.IsNullOrWhiteSpace(speech.LastFailure) ?，与相邻行合起来完成同一次计算或对象配置。
                status?.Show(speech != null && !string.IsNullOrWhiteSpace(speech.LastFailure) ?
                    // 步骤 5：执行 speech.LastFailure : "Speech unavailable. Try again.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    speech.LastFailure : "Speech unavailable. Try again.", 5f);
                // 步骤 6：提前结束这个方法；不再执行下方语句。
                return;
            }
            // 步骤 7：执行 status?.Show("English voice captured.", 3f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show("English voice captured.", 3f);
            // 步骤 8：调用 AddSentMessage：添加SentMessage。
            AddSentMessage(result, true);
            // 步骤 9：调用 RefreshText：刷新Text。
            RefreshText();
            // 步骤 10：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 11：检查条件 string.IsNullOrWhiteSpace(result)；只有为真才运行下面的步骤，否则沿原流程继续。
        if (string.IsNullOrWhiteSpace(result))
        {
            // 步骤 12：这一行补充 status?.Show(speech != null && !string.IsNullOrWhiteSpace(speech.LastFailure) ?，与相邻行合起来完成同一次计算或对象配置。
            status?.Show(speech != null && !string.IsNullOrWhiteSpace(speech.LastFailure) ?
                // 步骤 13：执行 speech.LastFailure : "Speech unavailable. Use the controller keyboard.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                speech.LastFailure : "Speech unavailable. Use the controller keyboard.", 5f);
            // 步骤 14：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 15：把计算结果 result.Length > maxMessageLength ? result.Substring(0, (int)maxMessageLength) : result 保存为CurrentDraft；下一条判断或调用会读取这个具体值。
        CurrentDraft = result.Length > maxMessageLength ? result.Substring(0, (int)maxMessageLength) : result;
        // 步骤 16：调用 RefreshText：刷新Text。
        RefreshText();
        // 步骤 17：执行 status?.Show("Voice captured. Review and press SEND.", 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Voice captured. Review and press SEND.", 4f);
    }
    // ！！！功能结束：OnVoiceRecognized！！！

    // ！！！功能：BeginVoiceMessage——BeginVoiceMessage！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BeginVoiceMessage()
    {
        // 步骤 1：检查条件 voiceMessagePending；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (voiceMessagePending) return;
        // 步骤 2：把 chatOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        chatOpen = false;
        // 步骤 3：把 tableChatOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        tableChatOpen = false;
        // 步骤 4：清空 activeTable 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        activeTable = null;
        // 步骤 5：确认panel已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (panel != null) panel.SetActive(false);
        // 步骤 6：把 voiceMessagePending 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        voiceMessagePending = true;
        // 步骤 7：执行 status?.Show("Preparing English voice...", 8f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Preparing English voice...", 8f);
        // 步骤 8：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("SkyQuest English voice requested");
        // 步骤 9：执行 speech?.BeginRecognition();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        speech?.BeginRecognition();
    }
    // ！！！功能结束：BeginVoiceMessage！！！

    // ！！！功能：OnVoiceStatus——OnVoice状态！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string message 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnVoiceStatus(string message)
    {
        // 步骤 1：执行 status?.Show(message, 9f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show(message, 9f);
        // 步骤 2：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("SkyQuest English voice: " + message);
    }
    // ！！！功能结束：OnVoiceStatus！！！

    // ！！！功能：UpdateLeftTriggerVoice——更新LeftTriggerVoice！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateLeftTriggerVoice()
    {
        // 步骤 1：用 pressed 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Left) > .72f），供下一条 if 选择执行路径。
        bool pressed = SkyQuestInput.Trigger(SkyQuestInput.Left) > .72f;
        // 步骤 2：这里用 && 要求两边条件同时成立：pressed && !previousLeftTrigger；任意一边为假就跳过本分支。
        if (pressed && !previousLeftTrigger)
        {
            // 步骤 3：把计算结果 Time.unscaledTime 保存为leftTriggerStarted；下一条判断或调用会读取这个具体值。
            leftTriggerStarted = Time.unscaledTime;
            // 步骤 4：把 leftVoiceActivated 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            leftVoiceActivated = false;
        }
        // 步骤 5：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (pressed && !leftVoiceActivated && Time.unscaledTime - leftTriggerStarted >= .65f &&
            // 步骤 6：这一行补充 (itemTree == null || !itemTree.IsMenuOpen))，与相邻行合起来完成同一次计算或对象配置。
            (itemTree == null || !itemTree.IsMenuOpen))
        {
            // 步骤 7：把 leftVoiceActivated 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            leftVoiceActivated = true;
            // 步骤 8：调用 BeginVoiceMessage：BeginVoiceMessage。
            BeginVoiceMessage();
            // 步骤 9：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Left, .3f, .08f);
        }
        // 步骤 10：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!pressed && previousLeftTrigger && !leftVoiceActivated &&
            // 步骤 11：这一行补充 SkyQuestMagicPickup.Held != null && SkyQuestMagicPickup.Held.held)，与相邻行合起来完成同一次计算或对象配置。
            SkyQuestMagicPickup.Held != null && SkyQuestMagicPickup.Held.held)
        {
            // 步骤 12：执行 SkyQuestMagicPickup.Held.Release(false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            SkyQuestMagicPickup.Held.Release(false);
            // 步骤 13：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Left, .18f, .04f);
        }
        // 步骤 14：把计算结果 pressed 保存为previousLeftTrigger；下一条判断或调用会读取这个具体值。
        previousLeftTrigger = pressed;
    }
    // ！！！功能结束：UpdateLeftTriggerVoice！！！

    // ！！！功能：PlacePanel——Place面板！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PlacePanel()
    {
        // 步骤 1：检查panel、头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (panel == null || head == null) return;
        // 步骤 2：用 thirdPerson 记住此处真假结果（locomotion != null && locomotion.ThirdPerson），供下一条 if 选择执行路径。
        bool thirdPerson = locomotion != null && locomotion.ThirdPerson;
        // 步骤 3：把计算结果 !thirdPerson ? head : 保存为parent；下一条判断或调用会读取这个具体值。
        Transform parent = !thirdPerson ? head :
            // 步骤 4：执行 activeTable != null && tableChatOpen ? activeTable.transform : locomotion.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            activeTable != null && tableChatOpen ? activeTable.transform : locomotion.transform;
        // 步骤 5：检查条件 panel.transform.parent != parent；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        if (panel.transform.parent != parent) panel.transform.SetParent(parent, false);
        // 步骤 6：这里的 ! 表示取反；当 thirdPerson 不成立时才进入此分支。
        if (!thirdPerson)
        {
            // Keep the whole keyboard compact and closer to the view center,
            // with its labels and key colliders scaling together.
            // 步骤 7：执行 panel.transform.localPosition = new Vector3(.48f, -.04f, 1.2f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            panel.transform.localPosition = new Vector3(.48f, -.04f, 1.2f);
            // 步骤 8：执行 panel.transform.localRotation = Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            panel.transform.localRotation = Quaternion.identity;
            // 步骤 9：执行 panel.transform.localScale = Vector3.one * .72f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            panel.transform.localScale = Vector3.one * .72f;
        }
        // 步骤 10：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查activeTable是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        else if (activeTable != null && tableChatOpen)
        {
            // 步骤 11：执行 panel.transform.localPosition = new Vector3(0f, 1.37f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            panel.transform.localPosition = new Vector3(0f, 1.37f, 0f);
            // 步骤 12：继续填写上一行调用的参数：panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - head.position；参数位置决定它表示的坐标、速度或选项。
            panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - head.position,
                // 步骤 13：执行 Vector3.up);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.up);
            // 步骤 14：执行 panel.transform.localScale = Vector3.one;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            panel.transform.localScale = Vector3.one;
        }
        // 步骤 15：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 16：执行 panel.transform.localPosition = new Vector3(0f, 1.7f, 1.6f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            panel.transform.localPosition = new Vector3(0f, 1.7f, 1.6f);
            // 步骤 17：执行 panel.transform.localRotation = Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            panel.transform.localRotation = Quaternion.identity;
            // 步骤 18：执行 panel.transform.localScale = Vector3.one;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            panel.transform.localScale = Vector3.one;
        }
    }
    // ！！！功能结束：PlacePanel！！！

    // ！！！功能：AddSentMessage——添加SentMessage！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string text, bool onLeft 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void AddSentMessage(string text, bool onLeft)
    {
        // 步骤 1：计算 text.Trim()，把结果存进 message（message）；之后引用 message 时用的就是这次结果。
        string message = text.Trim();
        // 步骤 2：检查条件 message.Length > maxMessageLength；只有为真才运行下面的步骤，否则沿原流程继续。
        if (message.Length > maxMessageLength)
            // 步骤 3：把计算结果 message.Substring(0, (int)maxMessageLength) 保存为message；下一条判断或调用会读取这个具体值。
            message = message.Substring(0, (int)maxMessageLength);
        // 步骤 4：把括号里的新元素加入 SentMessages 集合；后续会按集合顺序使用它。
        SentMessages.Add(message);
        // 步骤 5：把括号里的新元素加入 sentMessageTimes 集合；后续会按集合顺序使用它。
        sentMessageTimes.Add(Time.unscaledTime);
        // 步骤 6：把括号里的新元素加入 sentMessageOnLeft 集合；后续会按集合顺序使用它。
        sentMessageOnLeft.Add(onLeft);
        // 步骤 7：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("SkyQuest message queued: " + (onLeft ? "voice" : "keyboard") +
            // 步骤 8：执行 ", " + message.Length + " characters");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ", " + message.Length + " characters");
        // 步骤 9：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (SentMessages.Count > 4)
        {
            // 步骤 10：调用 RemoveAt：移除At。
            SentMessages.RemoveAt(0);
            // 步骤 11：调用 RemoveAt：移除At。
            sentMessageTimes.RemoveAt(0);
            // 步骤 12：调用 RemoveAt：移除At。
            sentMessageOnLeft.RemoveAt(0);
        }
        // 步骤 13：调用 RefreshText：刷新Text。
        RefreshText();
    }
    // ！！！功能结束：AddSentMessage！！！

    // ！！！功能：ExpireOldMessages——ExpireOldMessages！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ExpireOldMessages()
    {
        // 步骤 1：计算 Time.unscaledTime，把结果存进 now（now）；之后引用 now 时用的就是这次结果。
        float now = Time.unscaledTime;
        // 步骤 2：按 int i = sentMessageTimes.Count - 1; i >= 0; i-- 变化索引；循环体依次处理每个位置。
        for (int i = sentMessageTimes.Count - 1; i >= 0; i--)
        {
            // 步骤 3：检查条件 now - sentMessageTimes[i] < MessageVisibleDuration + MessageFadeDuration；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (now - sentMessageTimes[i] < MessageVisibleDuration + MessageFadeDuration) continue;
            // 步骤 4：调用 RemoveAt：移除At。
            sentMessageTimes.RemoveAt(i);
            // 步骤 5：调用 RemoveAt：移除At。
            SentMessages.RemoveAt(i);
            // 步骤 6：调用 RemoveAt：移除At。
            sentMessageOnLeft.RemoveAt(i);
        }
    }
    // ！！！功能结束：ExpireOldMessages！！！

    // ！！！功能：RefreshText——刷新Text！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RefreshText()
    {
        // 步骤 1：确认draftText已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 draftText.text = "Draft: " + CurrentDraft;
        if (draftText != null) draftText.text = "Draft: " + CurrentDraft;
        // 步骤 2：确认historyText已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 historyText.text = SentMessages.Count > 0
        if (historyText != null) historyText.text = SentMessages.Count > 0
            // 步骤 3：三元表达式中，前面的条件为真时选这个值。
            ? "Last: " + SentMessages[SentMessages.Count - 1] :
                // 步骤 4：执行 desktopMode ? "Enter: send    Esc: close" : "Left stick click: close";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                desktopMode ? "Enter: send    Esc: close" : "Left stick click: close";
    }
    // ！！！功能结束：RefreshText！！！

    // ！！！功能：DesktopToggle——桌面Toggle！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopToggle()
    {
        // 步骤 1：检查panel是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (panel == null) return;
        // 步骤 2：把计算结果 !chatOpen 保存为chatOpen；下一条判断或调用会读取这个具体值。
        chatOpen = !chatOpen;
        // 步骤 3：这里的 ! 表示取反；当 chatOpen 不成立时才进入此分支。 成立时执行 { tableChatOpen = false; activeTable = null; }
        if (!chatOpen) { tableChatOpen = false; activeTable = null; }
        // 步骤 4：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 成立时调用 SetActive：设置Active。
        if (!desktopMode) panel.SetActive(chatOpen);
        // 步骤 5：这一行补充 status?.Show(chatOpen ? (desktopMode ? "Type a message. Enter sends; Esc closes." :，与相邻行合起来完成同一次计算或对象配置。
        status?.Show(chatOpen ? (desktopMode ? "Type a message. Enter sends; Esc closes." :
            // 步骤 6：这一行补充 "Point at letters and press right trigger to type.") :，与相邻行合起来完成同一次计算或对象配置。
            "Point at letters and press right trigger to type.") :
            // 步骤 7：执行 "Communication closed.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Communication closed.", 5f);
        // 步骤 8：调用 RefreshText：刷新Text。
        RefreshText();
    }
    // ！！！功能结束：DesktopToggle！！！

    // ！！！功能：DesktopType——桌面Type！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 char character 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopType(char character)
    {
        // 步骤 1：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!desktopMode || !IsOpen || char.IsControl(character) ||
            // 步骤 2：执行 CurrentDraft.Length >= maxMessageLength) return;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            CurrentDraft.Length >= maxMessageLength) return;
        // 步骤 3：在CurrentDraft原值上增加 character；旧状态与本次输入共同决定新状态。
        CurrentDraft += character;
        // 步骤 4：调用 RefreshText：刷新Text。
        RefreshText();
    }
    // ！！！功能结束：DesktopType！！！

    // ！！！功能：DesktopDelete——桌面Delete！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopDelete()
    {
        // 步骤 1：检查条件 IsOpen；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Press：Press。
        if (IsOpen) Press("DEL");
    }
    // ！！！功能结束：DesktopDelete！！！

    // ！！！功能：DesktopSend——桌面Send！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopSend()
    {
        // 步骤 1：检查条件 IsOpen；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Press：Press。
        if (IsOpen) Press("SEND");
    }
    // ！！！功能结束：DesktopSend！！！

    // ！！！功能：DesktopVoice——桌面Voice！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopVoice()
    {
        // 步骤 1：检查条件 IsFirstPerson；只有为真才运行下面的步骤，否则沿原流程继续。
        if (IsFirstPerson)
        {
            // 步骤 2：调用 BeginVoiceMessage：BeginVoiceMessage。
            BeginVoiceMessage();
            // 步骤 3：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 4：这里的 ! 表示取反；当 IsOpen 不成立时才进入此分支。 成立时调用 DesktopToggle：桌面Toggle。
        if (!IsOpen) DesktopToggle();
        // 步骤 5：调用 Press：Press。
        Press("MIC");
    }
    // ！！！功能结束：DesktopVoice！！！

    // ！！！功能：OpenChat——OpenChat！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OpenChat()
    {
        // 步骤 1：调用 DesktopToggle：桌面Toggle。
        DesktopToggle();
    }
    // ！！！功能结束：OpenChat！！！

    // ！！！功能：OpenFromTeaTable——Open从TeaTable！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 SkyQuestTeaTable table 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void OpenFromTeaTable(SkyQuestTeaTable table)
    {
        // 步骤 1：检查panel、itemTree是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (panel == null || itemTree != null && itemTree.IsMenuOpen) return;
        // 步骤 2：把计算结果 table 保存为activeTable；下一条判断或调用会读取这个具体值。
        activeTable = table;
        // 步骤 3：把 tableChatOpen 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        tableChatOpen = true;
        // 步骤 4：把 chatOpen 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        chatOpen = true;
        // 步骤 5：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 成立时调用 SetActive：设置Active。
        if (!desktopMode) panel.SetActive(true);
        // 步骤 6：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 成立时调用 PlacePanel：Place面板。
        if (!desktopMode) PlacePanel();
        // 步骤 7：调用 RefreshText：刷新Text。
        RefreshText();
        // 步骤 8：这一行补充 status?.Show(desktopMode ? "Tea table chat open. Type and press Enter." :，与相邻行合起来完成同一次计算或对象配置。
        status?.Show(desktopMode ? "Tea table chat open. Type and press Enter." :
            // 步骤 9：执行 "Tea table chat open. Point at letters and press right trigger.", 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Tea table chat open. Point at letters and press right trigger.", 4f);
    }
    // ！！！功能结束：OpenFromTeaTable！！！

    // ！！！功能：UpdateTeaTableClick——更新TeaTableClick！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateTeaTableClick()
    {
        // 步骤 1：检查itemTree是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (itemTree != null && itemTree.IsMenuOpen) return;
        // 步骤 2：声明 ray（ray）：三维射线；只供本脚本内部保存状态，赋值前是该类型的默认值。
        Ray ray;
        // 步骤 3：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 4：计算 Mouse.current，把结果存进 mouse（mouse）；之后引用 mouse 时用的就是这次结果。
            Mouse mouse = Mouse.current;
            // 步骤 5：检查mouse是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
            // 步骤 6：计算 Camera.main，把结果存进 camera（camera）；之后引用 camera 时用的就是这次结果。
            Camera camera = Camera.main;
            // 步骤 7：检查camera是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
            if (camera == null) return;
            // 步骤 8：把计算结果 Cursor.lockState == CursorLockMode.Locked ? 保存为point；下一条判断或调用会读取这个具体值。
            Vector2 point = Cursor.lockState == CursorLockMode.Locked ?
                // 步骤 9：用 new 创建 Vector2(Screen.width * .5f, Screen.height * .5f) : mouse.position.ReadValue(；构造参数决定新对象的初始内容。
                new Vector2(Screen.width * .5f, Screen.height * .5f) : mouse.position.ReadValue();
            // 步骤 10：把计算结果 camera.ScreenPointToRay(point) 保存为ray；下一条判断或调用会读取这个具体值。
            ray = camera.ScreenPointToRay(point);
        }
        // 步骤 11：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 12：用 trigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f），供下一条 if 选择执行路径。
            bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f;
            // 步骤 13：这里用 && 要求两边条件同时成立：trigger && !previousTableTrigger；任意一边为假就跳过本分支。 成立时执行 tableTriggerStarted = Time.unscaledTime;
            if (trigger && !previousTableTrigger) tableTriggerStarted = Time.unscaledTime;
            // 步骤 14：把计算结果 !trigger && previousTableTrigger && 保存为shortClick；下一条判断或调用会读取这个具体值。
            bool shortClick = !trigger && previousTableTrigger &&
                // 步骤 15：执行 Time.unscaledTime - tableTriggerStarted < .32f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Time.unscaledTime - tableTriggerStarted < .32f;
            // 步骤 16：把计算结果 trigger 保存为previousTableTrigger；下一条判断或调用会读取这个具体值。
            previousTableTrigger = trigger;
            // 步骤 17：检查右手柄位置是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
            if (!shortClick || rightHand == null) return;
            // 步骤 18：新建用于ray的对象或数值结构，并把构造参数保存进去。
            ray = new Ray(rightHand.position, rightHand.forward);
        }
        // 步骤 19：把计算结果 Physics.RaycastAll(ray, desktopMode ? 12f : 5f, 保存为hits；下一条判断或调用会读取这个具体值。
        RaycastHit[] hits = Physics.RaycastAll(ray, desktopMode ? 12f : 5f,
            // 步骤 20：执行 ~0, QueryTriggerInteraction.Ignore);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ~0, QueryTriggerInteraction.Ignore);
        // 步骤 21：执行 System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        // 步骤 22：逐个读取 RaycastHit hit in hits 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in hits)
        {
            // 步骤 23：从目标物体找到所需组件并存为 table；使用前应检查是否为 null。
            SkyQuestTeaTable table = hit.collider.GetComponentInParent<SkyQuestTeaTable>();
            // 步骤 24：检查table是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
            if (table == null) return; // A nearer solid object blocks the click.
            // 步骤 25：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。 条件成立时立即退出当前方法。
            if (Time.unscaledTime < table.readyAt) return;
            // 步骤 26：从目标物体找到所需组件并存为 pickup；使用前应检查是否为 null。
            SkyQuestMagicPickup pickup = table.GetComponent<SkyQuestMagicPickup>();
            // 步骤 27：检查pickup是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
            if (pickup != null && pickup.held) return;
            // 步骤 28：用三元表达式为 viewer 二选一：问号前条件为真取前值，为假取冒号后的值。
            Transform viewer = avatar != null ? avatar : head;
            // 步骤 29：检查观看者头部或相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
            if (viewer == null || Vector3.Distance(viewer.position, table.transform.position) > 4f) return;
            // 步骤 30：调用 OpenFromTeaTable：Open从TeaTable。
            OpenFromTeaTable(table);
            // 步骤 31：提前结束这个方法；不再执行下方语句。
            return;
        }
    }
    // ！！！功能结束：UpdateTeaTableClick！！！

    // ！！！功能：SendChat——SendChat！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SendChat()
    {
        // 步骤 1：检查条件 string.IsNullOrWhiteSpace(CurrentDraft)；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (string.IsNullOrWhiteSpace(CurrentDraft)) return;
        // 步骤 2：调用 Press：Press。
        Press("SEND");
    }
    // ！！！功能结束：SendChat！！！

    // ！！！功能：HeadScreenPoint——HeadScreenPoint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Vector2 类型的计算结果；执行到 return 时把该值交回调用者。
    private Vector2 HeadScreenPoint()
    {
        // 步骤 1：用三元表达式为 anchor 二选一：问号前条件为真取前值，为假取冒号后的值。
        Transform anchor = avatar != null ? avatar : head;
        // 步骤 2：检查anchor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (anchor == null) return new Vector2(Screen.width * .5f, Screen.height * .35f);
        // 步骤 3：用三元表达式为 world 二选一：问号前条件为真取前值，为假取冒号后的值。
        Vector3 world = anchor.position + Vector3.up * (avatar != null ? 2.05f : .42f);
        // 步骤 4：计算 Camera.main，把结果存进 camera（camera）；之后引用 camera 时用的就是这次结果。
        Camera camera = Camera.main;
        // 步骤 5：检查camera是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 camera = head != null ? head.GetComponent<Camera>() : null;
        if (camera == null) camera = head != null ? head.GetComponent<Camera>() : null;
        // 步骤 6：检查camera是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (camera == null) return new Vector2(Screen.width * .5f, Screen.height * .35f);
        // 步骤 7：计算 camera.WorldToScreenPoint(world)，把结果存进 screen（screen）；之后引用 screen 时用的就是这次结果。
        Vector3 screen = camera.WorldToScreenPoint(world);
        // 步骤 8：检查条件 screen.z <= 0f；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (screen.z <= 0f) return new Vector2(-1000f, -1000f);
        // 步骤 9：把 new Vector2(screen.x, Screen.height - screen.y) 作为方法结果返回给调用方；return 后本方法立即结束。
        return new Vector2(screen.x, Screen.height - screen.y);
    }
    // ！！！功能结束：HeadScreenPoint！！！

    // ！！！功能：EnsureGuiStyles——确保GuiStyles！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void EnsureGuiStyles()
    {
        // 步骤 1：确认iconStyle已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (iconStyle != null) return;
        // 步骤 2：检查chatBackground是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 chatBackground = MakeGuiTexture(new Color(.025f, .035f, .045f, .78f));
        if (chatBackground == null) chatBackground = MakeGuiTexture(new Color(.025f, .035f, .045f, .78f));
        // 步骤 3：检查bubbleBackground是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 bubbleBackground = MakeRoundedGuiTexture();
        if (bubbleBackground == null) bubbleBackground = MakeRoundedGuiTexture();
        // 步骤 4：检查typingBackground是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 typingBackground = MakeGuiTexture(new Color(1f, .94f, .69f, .96f));
        if (typingBackground == null) typingBackground = MakeGuiTexture(new Color(1f, .94f, .69f, .96f));
        // 步骤 5：检查controlBackground是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 controlBackground = MakeGuiTexture(new Color(.08f, .10f, .12f, .76f));
        if (controlBackground == null) controlBackground = MakeGuiTexture(new Color(.08f, .10f, .12f, .76f));
        // 步骤 6：检查keyboardBackground是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 keyboardBackground = MakeGuiTexture(new Color(.61f, .82f, .96f, .46f));
        if (keyboardBackground == null) keyboardBackground = MakeGuiTexture(new Color(.61f, .82f, .96f, .46f));
        // 步骤 7：检查keyboardKeyBackground是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 keyboardKeyBackground = MakeGuiTexture(new Color(.96f, .99f, .88f, .90f));
        if (keyboardKeyBackground == null) keyboardKeyBackground = MakeGuiTexture(new Color(.96f, .99f, .88f, .90f));
        // 步骤 8：检查keyboardKeySelectedBackground是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 keyboardKeySelectedBackground = MakeGuiTexture(new Color(1f, .82f, .36f, .9
        if (keyboardKeySelectedBackground == null) keyboardKeySelectedBackground = MakeGuiTexture(new Color(1f, .82f, .36f, .98f));
        // 步骤 9：检查keyboardActionBackground是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 keyboardActionBackground = MakeGuiTexture(new Color(.82f, .88f, .75f, .94f)
        if (keyboardActionBackground == null) keyboardActionBackground = MakeGuiTexture(new Color(.82f, .88f, .75f, .94f));
        // 步骤 10：新建用于iconStyle的对象或数值结构，并把构造参数保存进去。
        iconStyle = new GUIStyle(GUI.skin.button)
        {
            // 步骤 11：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            fontSize = Mathf.Clamp(Screen.width / 34, 20, 34),
            // 步骤 12：把计算结果 TextAnchor.MiddleCenter, 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleCenter,
            // 步骤 13：新建用于normal的对象或数值结构，并把构造参数保存进去。
            normal = { textColor = new Color(1f, .98f, .88f) },
            // 步骤 14：把计算结果 { textColor = Color.white }, 保存为hover；下一条判断或调用会读取这个具体值。
            hover = { textColor = Color.white },
            // 步骤 15：把计算结果 { textColor = Color.white } 保存为active；下一条判断或调用会读取这个具体值。
            active = { textColor = Color.white }
        };
        // 步骤 16：新建用于inputStyle的对象或数值结构，并把构造参数保存进去。
        inputStyle = new GUIStyle(GUI.skin.textField)
        {
            // 步骤 17：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            fontSize = Mathf.Clamp(Screen.width / 55, 18, 28),
            // 步骤 18：把计算结果 TextAnchor.MiddleLeft, 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleLeft,
            // 步骤 19：新建用于padding的对象或数值结构，并把构造参数保存进去。
            padding = new RectOffset(14, 12, 8, 8),
            // 步骤 20：把计算结果 { textColor = Color.white, background = chatBackground }, 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { textColor = Color.white, background = chatBackground },
            // 步骤 21：把计算结果 { textColor = Color.white, background = chatBackground }, 保存为当前正在阅读或观察的物品；下一条判断或调用会读取这个具体值。
            focused = { textColor = Color.white, background = chatBackground },
            // 步骤 22：把计算结果 { textColor = Color.white, background = chatBackground }, 保存为hover；下一条判断或调用会读取这个具体值。
            hover = { textColor = Color.white, background = chatBackground },
            // 步骤 23：把计算结果 { textColor = Color.white, background = chatBackground } 保存为active；下一条判断或调用会读取这个具体值。
            active = { textColor = Color.white, background = chatBackground }
        };
        // 步骤 24：新建用于messageStyle的对象或数值结构，并把构造参数保存进去。
        messageStyle = new GUIStyle(GUI.skin.box)
        {
            // 步骤 25：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            fontSize = Mathf.Clamp(Screen.width / 75, 16, 24),
            // 步骤 26：把计算结果 TextAnchor.MiddleCenter, 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleCenter,
            // 步骤 27：把计算结果 true, 保存为wordWrap；下一条判断或调用会读取这个具体值。
            wordWrap = true,
            // 步骤 28：把计算结果 { textColor = Color.white, background = chatBackground }, 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { textColor = Color.white, background = chatBackground },
            // 步骤 29：新建用于padding的对象或数值结构，并把构造参数保存进去。
            padding = new RectOffset(12, 12, 7, 7)
        };
        // 步骤 30：新建用于leftBubbleStyle的对象或数值结构，并把构造参数保存进去。
        leftBubbleStyle = new GUIStyle(GUI.skin.box)
        {
            // 步骤 31：新建用于border的对象或数值结构，并把构造参数保存进去。
            border = new RectOffset(8, 8, 8, 8),
            // 步骤 32：把计算结果 { background = bubbleBackground } 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { background = bubbleBackground }
        };
        // 步骤 33：新建用于typingStyle的对象或数值结构，并把构造参数保存进去。
        typingStyle = new GUIStyle(messageStyle)
        {
            // 步骤 34：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            fontSize = Mathf.Clamp(Screen.width / 60, 18, 28),
            // 步骤 35：新建用于normal的对象或数值结构，并把构造参数保存进去。
            normal = { textColor = new Color(.3f, .27f, .15f), background = typingBackground }
        };
        // 步骤 36：新建用于bubbleLabelStyle的对象或数值结构，并把构造参数保存进去。
        bubbleLabelStyle = new GUIStyle(GUI.skin.label)
        {
            // 步骤 37：把计算结果 messageStyle.fontSize, 保存为fontSize；下一条判断或调用会读取这个具体值。
            fontSize = messageStyle.fontSize,
            // 步骤 38：把计算结果 TextAnchor.MiddleCenter, 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleCenter,
            // 步骤 39：把计算结果 true, 保存为wordWrap；下一条判断或调用会读取这个具体值。
            wordWrap = true,
            // 步骤 40：新建用于normal的对象或数值结构，并把构造参数保存进去。
            normal = { textColor = new Color(.08f, .25f, .67f) }
        };
        // 步骤 41：新建用于buttonStyle的对象或数值结构，并把构造参数保存进去。
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            // 步骤 42：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            fontSize = Mathf.Clamp(Screen.width / 60, 18, 26),
            // 步骤 43：把计算结果 TextAnchor.MiddleCenter, 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleCenter,
            // 步骤 44：把计算结果 { textColor = Color.white } 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { textColor = Color.white }
        };
        // 步骤 45：新建用于inputLabelStyle的对象或数值结构，并把构造参数保存进去。
        inputLabelStyle = new GUIStyle(GUI.skin.label)
        {
            // 步骤 46：把计算结果 inputStyle.fontSize, 保存为fontSize；下一条判断或调用会读取这个具体值。
            fontSize = inputStyle.fontSize,
            // 步骤 47：把计算结果 TextAnchor.MiddleLeft, 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleLeft,
            // 步骤 48：新建用于padding的对象或数值结构，并把构造参数保存进去。
            padding = new RectOffset(14, 12, 8, 8),
            // 步骤 49：把计算结果 { textColor = Color.white } 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { textColor = Color.white }
        };
        // 步骤 50：新建用于controlsBackdropStyle的对象或数值结构，并把构造参数保存进去。
        controlsBackdropStyle = new GUIStyle(GUI.skin.box)
        {
            // 步骤 51：把计算结果 { background = controlBackground } 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { background = controlBackground }
        };
        // 步骤 52：新建用于keyboard面板Style的对象或数值结构，并把构造参数保存进去。
        keyboardPanelStyle = new GUIStyle(GUI.skin.box)
        {
            // 步骤 53：把计算结果 { background = keyboardBackground }, 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { background = keyboardBackground },
            // 步骤 54：新建用于padding的对象或数值结构，并把构造参数保存进去。
            padding = new RectOffset(12, 12, 10, 10)
        };
        // 步骤 55：新建用于keyboardKeyStyle的对象或数值结构，并把构造参数保存进去。
        keyboardKeyStyle = new GUIStyle(GUI.skin.button)
        {
            // 步骤 56：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            fontSize = Mathf.Clamp(Screen.width / 42, 18, 34),
            // 步骤 57：把计算结果 TextAnchor.MiddleCenter, 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleCenter,
            // 步骤 58：新建用于normal的对象或数值结构，并把构造参数保存进去。
            normal = { textColor = new Color(.20f, .22f, .18f), background = keyboardKeyBackground },
            // 步骤 59：把计算结果 { textColor = Color.black, background = keyboardKeySelectedBackground }, 保存为hover；下一条判断或调用会读取这个具体值。
            hover = { textColor = Color.black, background = keyboardKeySelectedBackground },
            // 步骤 60：把计算结果 { textColor = Color.black, background = keyboardKeySelectedBackground } 保存为active；下一条判断或调用会读取这个具体值。
            active = { textColor = Color.black, background = keyboardKeySelectedBackground }
        };
        // 步骤 61：新建用于keyboardKeySelectedStyle的对象或数值结构，并把构造参数保存进去。
        keyboardKeySelectedStyle = new GUIStyle(keyboardKeyStyle)
        {
            // 步骤 62：把计算结果 { textColor = Color.black, background = keyboardKeySelectedBackground } 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { textColor = Color.black, background = keyboardKeySelectedBackground }
        };
        // 步骤 63：新建用于keyboardActionStyle的对象或数值结构，并把构造参数保存进去。
        keyboardActionStyle = new GUIStyle(keyboardKeyStyle)
        {
            // 步骤 64：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            fontSize = Mathf.Clamp(Screen.width / 62, 16, 26),
            // 步骤 65：新建用于normal的对象或数值结构，并把构造参数保存进去。
            normal = { textColor = new Color(.18f, .20f, .16f), background = keyboardActionBackground }
        };
    }
    // ！！！功能结束：EnsureGuiStyles！！！

    // ！！！功能：MakeGuiTexture——MakeGui贴图！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Color color 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Texture2D 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Texture2D MakeGuiTexture(Color color)
    {
        // 步骤 1：创建新的 Texture2D 并存为 texture；构造参数决定其初始内容或对象名称。
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        // 步骤 2：把计算结果 HideFlags.HideAndDontSave 保存为hideFlags；下一条判断或调用会读取这个具体值。
        texture.hideFlags = HideFlags.HideAndDontSave;
        // 步骤 3：调用 SetPixel：设置Pixel。
        texture.SetPixel(0, 0, color);
        // 步骤 4：调用 Apply：应用。
        texture.Apply();
        // 步骤 5：把 texture 作为方法结果返回给调用方；return 后本方法立即结束。
        return texture;
    }
    // ！！！功能结束：MakeGuiTexture！！！

    // ！！！功能：MakeRoundedGuiTexture——MakeRoundedGui贴图！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Texture2D 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Texture2D MakeRoundedGuiTexture()
    {
        // 步骤 1：定义不可在运行时改变的数值 size（size）；后续计算统一使用这个阈值、速度或尺寸。
        const int size = 24;
        // 步骤 2：创建新的 Texture2D 并存为 texture；构造参数决定其初始内容或对象名称。
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        // 步骤 3：把计算结果 HideFlags.HideAndDontSave 保存为hideFlags；下一条判断或调用会读取这个具体值。
        texture.hideFlags = HideFlags.HideAndDontSave;
        // 步骤 4：把计算结果 FilterMode.Bilinear 保存为filter模式；下一条判断或调用会读取这个具体值。
        texture.filterMode = FilterMode.Bilinear;
        // 步骤 5：按 int y = 0; y < size; y++ 变化索引；循环体依次处理每个位置。
        for (int y = 0; y < size; y++)
        // 步骤 6：按 int x = 0; x < size; x++ 变化索引；循环体依次处理每个位置。
        for (int x = 0; x < size; x++)
        {
            // 步骤 7：取输入的绝对值并存为 dx；只比较幅度时暂时不考虑正负方向。
            float dx = Mathf.Max(0f, Mathf.Abs(x + .5f - size * .5f) - (size * .5f - 7f));
            // 步骤 8：取输入的绝对值并存为 dy；只比较幅度时暂时不考虑正负方向。
            float dy = Mathf.Max(0f, Mathf.Abs(y + .5f - size * .5f) - (size * .5f - 7f));
            // 步骤 9：计算 coverage 并限制在安全上下界，防止不合理的大小影响移动或显示。
            float coverage = Mathf.Clamp01(7.5f - Mathf.Sqrt(dx * dx + dy * dy));
            // 步骤 10：调用 SetPixel：设置Pixel。
            texture.SetPixel(x, y, new Color(.96f, .77f, .84f, .78f * coverage));
        }
        // 步骤 11：调用 Apply：应用。
        texture.Apply();
        // 步骤 12：把 texture 作为方法结果返回给调用方；return 后本方法立即结束。
        return texture;
    }
    // ！！！功能结束：MakeRoundedGuiTexture！！！

    // ！！！功能：BindDesktopKeyboard——Bind桌面Keyboard！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BindDesktopKeyboard()
    {
        // 步骤 1：计算 Keyboard.current，把结果存进 current（current）；之后引用 current 时用的就是这次结果。
        Keyboard current = Keyboard.current;
        // 步骤 2：检查条件 current == boundKeyboard；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (current == boundKeyboard) return;
        // 步骤 3：确认boundKeyboard已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 boundKeyboard.onTextInput -= OnDesktopTextInput;
        if (boundKeyboard != null) boundKeyboard.onTextInput -= OnDesktopTextInput;
        // 步骤 4：把计算结果 current 保存为boundKeyboard；下一条判断或调用会读取这个具体值。
        boundKeyboard = current;
        // 步骤 5：确认boundKeyboard已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 boundKeyboard.onTextInput += OnDesktopTextInput;
        if (boundKeyboard != null) boundKeyboard.onTextInput += OnDesktopTextInput;
    }
    // ！！！功能结束：BindDesktopKeyboard！！！

    // ！！！功能：OnDesktopTextInput——On桌面Text输入！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 char character 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDesktopTextInput(char character)
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：!desktopMode || !IsOpen || char.IsControl(character)；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!desktopMode || !IsOpen || char.IsControl(character)) return;
        // 步骤 2：调用 AddCharacter：添加Character。
        AddCharacter(character);
        // 步骤 3：把计算结果 char.ToLowerInvariant(character) 保存为highlightedKey；下一条判断或调用会读取这个具体值。
        highlightedKey = char.ToLowerInvariant(character);
        // 步骤 4：把计算结果 Time.unscaledTime + .22f 保存为highlightedKeyUntil；下一条判断或调用会读取这个具体值。
        highlightedKeyUntil = Time.unscaledTime + .22f;
    }
    // ！！！功能结束：OnDesktopTextInput！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void OnDestroy()
    {
        // 步骤 1：确认keyboardBacking材质已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (keyboardBackingMaterial != null) Destroy(keyboardBackingMaterial);
        // 步骤 2：确认boundKeyboard已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 boundKeyboard.onTextInput -= OnDesktopTextInput;
        if (boundKeyboard != null) boundKeyboard.onTextInput -= OnDesktopTextInput;
        // 步骤 3：确认speech已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (speech != null)
        {
            // 步骤 4：在OnRecognized原值上减少 OnVoiceRecognized；旧状态与本次输入共同决定新状态。
            speech.OnRecognized -= OnVoiceRecognized;
            // 步骤 5：在On状态Changed原值上减少 OnVoiceStatus；旧状态与本次输入共同决定新状态。
            speech.OnStatusChanged -= OnVoiceStatus;
        }
    }
    // ！！！功能结束：OnDestroy！！！

    // ！！！功能：UpdateDesktopKeyboard——更新桌面Keyboard！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateDesktopKeyboard()
    {
        // 步骤 1：这里的 ! 表示取反；当 chatOpen 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!chatOpen) return;
        // 步骤 2：计算 Keyboard.current，把结果存进 keyboard（keyboard）；之后引用 keyboard 时用的就是这次结果。
        Keyboard keyboard = Keyboard.current;
        // 步骤 3：确认keyboard已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (keyboard != null)
        {
            // 步骤 4：按 char letter = 'a'; letter <= 'z'; letter++ 变化索引；循环体依次处理每个位置。
            for (char letter = 'a'; letter <= 'z'; letter++)
            {
                // 步骤 5：这里的 ! 表示取反；当 System.Enum.TryParse(letter.ToString().ToUpperInvariant(), out Key key) 不成立时才进入此分支。 成立时执行 continue;
                if (!System.Enum.TryParse(letter.ToString().ToUpperInvariant(), out Key key)) continue;
                // 步骤 6：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 continue;
                if (!keyboard[key].wasPressedThisFrame) continue;
                // 步骤 7：把计算结果 letter 保存为highlightedKey；下一条判断或调用会读取这个具体值。
                highlightedKey = letter;
                // 步骤 8：把计算结果 Time.unscaledTime + .22f 保存为highlightedKeyUntil；下一条判断或调用会读取这个具体值。
                highlightedKeyUntil = Time.unscaledTime + .22f;
            }
            // 步骤 9：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 MoveKeyboardSelection：MoveKeyboardSelection。
            if (keyboard.leftArrowKey.wasPressedThisFrame) MoveKeyboardSelection(-1, 0);
            // 步骤 10：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 MoveKeyboardSelection：MoveKeyboardSelection。
            if (keyboard.rightArrowKey.wasPressedThisFrame) MoveKeyboardSelection(1, 0);
            // 步骤 11：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 MoveKeyboardSelection：MoveKeyboardSelection。
            if (keyboard.upArrowKey.wasPressedThisFrame) MoveKeyboardSelection(0, -1);
            // 步骤 12：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 MoveKeyboardSelection：MoveKeyboardSelection。
            if (keyboard.downArrowKey.wasPressedThisFrame) MoveKeyboardSelection(0, 1);
        }

        // 步骤 13：计算 Gamepad.current，把结果存进 gamepad（gamepad）；之后引用 gamepad 时用的就是这次结果。
        Gamepad gamepad = Gamepad.current;
        // 步骤 14：检查gamepad是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (gamepad == null) return;
        // 步骤 15：计算 gamepad.leftStick.ReadValue()，把结果存进 stick（stick）；之后引用 stick 时用的就是这次结果。
        Vector2 stick = gamepad.leftStick.ReadValue();
        // 步骤 16：用 left 记住此处真假结果（gamepad.dpad.left.wasPressedThisFrame || (stick.x < -.65f && Time.unscaledTime >= nex），供下一条 if 选择执行路径。
        bool left = gamepad.dpad.left.wasPressedThisFrame || (stick.x < -.65f && Time.unscaledTime >= nextGamepadNavigation);
        // 步骤 17：用 right 记住此处真假结果（gamepad.dpad.right.wasPressedThisFrame || (stick.x > .65f && Time.unscaledTime >= nex），供下一条 if 选择执行路径。
        bool right = gamepad.dpad.right.wasPressedThisFrame || (stick.x > .65f && Time.unscaledTime >= nextGamepadNavigation);
        // 步骤 18：用 up 记住此处真假结果（gamepad.dpad.up.wasPressedThisFrame || (stick.y > .65f && Time.unscaledTime >= nextGa），供下一条 if 选择执行路径。
        bool up = gamepad.dpad.up.wasPressedThisFrame || (stick.y > .65f && Time.unscaledTime >= nextGamepadNavigation);
        // 步骤 19：用 down 记住此处真假结果（gamepad.dpad.down.wasPressedThisFrame || (stick.y < -.65f && Time.unscaledTime >= nex），供下一条 if 选择执行路径。
        bool down = gamepad.dpad.down.wasPressedThisFrame || (stick.y < -.65f && Time.unscaledTime >= nextGamepadNavigation);
        // 步骤 20：这里用 || 表示任一条件成立就处理：left || right || up || down；用于合并几种停止或跳过原因。
        if (left || right || up || down)
        {
            // 步骤 21：调用 MoveKeyboardSelection：MoveKeyboardSelection。
            MoveKeyboardSelection(left ? -1 : right ? 1 : 0, up ? -1 : down ? 1 : 0);
            // 步骤 22：把计算结果 Time.unscaledTime + .2f 保存为nextGamepadNavigation；下一条判断或调用会读取这个具体值。
            nextGamepadNavigation = Time.unscaledTime + .2f;
        }
        // 步骤 23：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 ActivateKeyboardSelection：ActivateKeyboardSelection。
        if (gamepad.buttonSouth.wasPressedThisFrame) ActivateKeyboardSelection();
        // 步骤 24：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopToggle：桌面Toggle。
        if (gamepad.buttonEast.wasPressedThisFrame) DesktopToggle();
    }
    // ！！！功能结束：UpdateDesktopKeyboard！！！

    // ！！！功能：MoveKeyboardSelection——MoveKeyboardSelection！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int horizontal, int vertical 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void MoveKeyboardSelection(int horizontal, int vertical)
    {
        // 步骤 1：用三元表达式为 row 二选一：问号前条件为真取前值，为假取冒号后的值。
        int row = selectedKeyboardKey < 10 ? 0 : selectedKeyboardKey < 19 ? 1 : selectedKeyboardKey < 26 ? 2 : 3;
        // 步骤 2：计算 { 0, 10, 19, 26 }，把结果存进 rowStarts（rowStarts）；之后引用 rowStarts 时用的就是这次结果。
        int[] rowStarts = { 0, 10, 19, 26 };
        // 步骤 3：计算 { 10, 9, 7, 3 }，把结果存进 rowLengths（rowLengths）；之后引用 rowLengths 时用的就是这次结果。
        int[] rowLengths = { 10, 9, 7, 3 };
        // 步骤 4：计算 selectedKeyboardKey - rowStarts[row]，把结果存进 column（column）；之后引用 column 时用的就是这次结果。
        int column = selectedKeyboardKey - rowStarts[row];
        // 步骤 5：检查条件 horizontal != 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if (horizontal != 0)
            // 步骤 6：把计算结果 (column + horizontal + rowLengths[row]) % rowLengths[row] 保存为column；下一条判断或调用会读取这个具体值。
            column = (column + horizontal + rowLengths[row]) % rowLengths[row];
        // 步骤 7：检查条件 vertical != 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if (vertical != 0)
        {
            // 步骤 8：计算 nextRow 并限制在安全上下界，防止不合理的大小影响移动或显示。
            int nextRow = Mathf.Clamp(row + vertical, 0, rowLengths.Length - 1);
            // 步骤 9：用三元表达式为 normalizedColumn 二选一：问号前条件为真取前值，为假取冒号后的值。
            float normalizedColumn = rowLengths[row] <= 1 ? .5f : column / (float)(rowLengths[row] - 1);
            // 步骤 10：把计算结果 Mathf.RoundToInt(normalizedColumn * (rowLengths[nextRow] - 1)) 保存为column；下一条判断或调用会读取这个具体值。
            column = Mathf.RoundToInt(normalizedColumn * (rowLengths[nextRow] - 1));
            // 步骤 11：把计算结果 nextRow 保存为row；下一条判断或调用会读取这个具体值。
            row = nextRow;
        }
        // 步骤 12：把计算结果 rowStarts[row] + column 保存为selectedKeyboardKey；下一条判断或调用会读取这个具体值。
        selectedKeyboardKey = rowStarts[row] + column;
    }
    // ！！！功能结束：MoveKeyboardSelection！！！

    // ！！！功能：ActivateKeyboardSelection——ActivateKeyboardSelection！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ActivateKeyboardSelection()
    {
        // 步骤 1：检查条件 selectedKeyboardKey < 26；只有为真才运行下面的步骤，否则沿原流程继续。
        if (selectedKeyboardKey < 26)
        {
            // 步骤 2：调用 AddCharacter：添加Character。
            AddCharacter((char)('a' + selectedKeyboardKey));
            // 步骤 3：把计算结果 (char)('a' + selectedKeyboardKey) 保存为highlightedKey；下一条判断或调用会读取这个具体值。
            highlightedKey = (char)('a' + selectedKeyboardKey);
            // 步骤 4：把计算结果 Time.unscaledTime + .22f 保存为highlightedKeyUntil；下一条判断或调用会读取这个具体值。
            highlightedKeyUntil = Time.unscaledTime + .22f;
        }
        // 步骤 5：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 selectedKeyboardKey == 26；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (selectedKeyboardKey == 26) AddCharacter(' ');
        // 步骤 6：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 selectedKeyboardKey == 27；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (selectedKeyboardKey == 27) Press("DEL");
        // 步骤 7：前面的 if 条件为假时执行这里的备用处理。
        else SendChat();
    }
    // ！！！功能结束：ActivateKeyboardSelection！！！

    // ！！！功能：AddCharacter——添加Character！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 char character 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void AddCharacter(char character)
    {
        // 步骤 1：检查条件 CurrentDraft.Length >= maxMessageLength；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (CurrentDraft.Length >= maxMessageLength) return;
        // 步骤 2：在CurrentDraft原值上增加 character；旧状态与本次输入共同决定新状态。
        CurrentDraft += character;
        // 步骤 3：调用 RefreshText：刷新Text。
        RefreshText();
    }
    // ！！！功能结束：AddCharacter！！！

    // ！！！功能：DrawVirtualKeyboard——DrawVirtualKeyboard！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float edge 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawVirtualKeyboard(float edge)
    {
        // 步骤 1：计算 panelHeight 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float panelHeight = Mathf.Clamp(Screen.height * .38f, 260f, 430f);
        // 步骤 2：计算 Screen.height - panelHeight，把结果存进 panelTop（panelTop）；之后引用 panelTop 时用的就是这次结果。
        float panelTop = Screen.height - panelHeight;
        // 步骤 3：创建新的 Rect 并存为 panel；构造参数决定其初始内容或对象名称。
        Rect panel = new Rect(0f, panelTop, Screen.width, panelHeight);
        // 步骤 4：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = Color.white;
        // 步骤 5：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(panel, GUIContent.none, keyboardPanelStyle);

        // 步骤 6：计算 gap 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float gap = Mathf.Clamp(Screen.width * .008f, 5f, 12f);
        // 步骤 7：计算 rowHeight 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float rowHeight = Mathf.Clamp((panelHeight - 54f) / 4f, 42f, 72f);
        // 步骤 8：计算 panelTop + 12f，把结果存进 y（y）；之后引用 y 时用的就是这次结果。
        float y = panelTop + 12f;
        // 步骤 9：调用 DrawKeyRow：DrawKeyRow。
        DrawKeyRow("qwertyuiop", 0, 10, edge, y, rowHeight, gap);
        // 步骤 10：在y原值上增加 rowHeight + gap；旧状态与本次输入共同决定新状态。
        y += rowHeight + gap;
        // 步骤 11：计算 Screen.width * .045f，把结果存进 indent（indent）；之后引用 indent 时用的就是这次结果。
        float indent = Screen.width * .045f;
        // 步骤 12：调用 DrawKeyRow：DrawKeyRow。
        DrawKeyRow("asdfghjkl", 10, 9, edge + indent, y, rowHeight, gap,
            // 步骤 13：执行 Screen.width - 2f * (edge + indent));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Screen.width - 2f * (edge + indent));
        // 步骤 14：在y原值上增加 rowHeight + gap；旧状态与本次输入共同决定新状态。
        y += rowHeight + gap;
        // 步骤 15：把计算结果 Screen.width * .17f 保存为indent；下一条判断或调用会读取这个具体值。
        indent = Screen.width * .17f;
        // 步骤 16：调用 DrawKeyRow：DrawKeyRow。
        DrawKeyRow("zxcvbnm", 19, 7, edge + indent, y, rowHeight, gap,
            // 步骤 17：执行 Screen.width - 2f * (edge + indent));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Screen.width - 2f * (edge + indent));
        // 步骤 18：在y原值上增加 rowHeight + gap；旧状态与本次输入共同决定新状态。
        y += rowHeight + gap;
        // 步骤 19：计算 (Screen.width - 2f * edge - 2f * gap) / 3f，把结果存进 actionWidth（actionWidth）；之后引用 actionWidth 时用的就是这次结果。
        float actionWidth = (Screen.width - 2f * edge - 2f * gap) / 3f;
        // 步骤 20：调用 DrawActionKey：DrawActionKey。
        DrawActionKey(new Rect(edge, y, actionWidth, rowHeight), "空格", 26);
        // 步骤 21：调用 DrawActionKey：DrawActionKey。
        DrawActionKey(new Rect(edge + actionWidth + gap, y, actionWidth, rowHeight), "删除", 27);
        // 步骤 22：调用 DrawActionKey：DrawActionKey。
        DrawActionKey(new Rect(edge + 2f * (actionWidth + gap), y, actionWidth, rowHeight), "发送", 28);
    }
    // ！！！功能结束：DrawVirtualKeyboard！！！

    // 继续填写上一行调用的参数：private void DrawKeyRow(string letters, int firstIndex, int count, float x, float y；参数位置决定它表示的坐标、速度或选项。
    private void DrawKeyRow(string letters, int firstIndex, int count, float x, float y,
        // 这一行补充 float height, float gap, float totalWidth = -1f)，与相邻行合起来完成同一次计算或对象配置。
        float height, float gap, float totalWidth = -1f)
    {
        // 用三元表达式为 width 二选一：问号前条件为真取前值，为假取冒号后的值。
        float width = totalWidth < 0f ? Screen.width - 2f * x : totalWidth;
        // 计算 (width - gap * (count - 1)) / count，把结果存进 keyWidth（keyWidth）；之后引用 keyWidth 时用的就是这次结果。
        float keyWidth = (width - gap * (count - 1)) / count;
        // 按 int i = 0; i < letters.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < letters.Length; i++)
        {
            // 计算 firstIndex + i，把结果存进 index（index）；之后引用 index 时用的就是这次结果。
            int index = firstIndex + i;
            // 创建新的 Rect 并存为 rect；构造参数决定其初始内容或对象名称。
            Rect rect = new Rect(x + i * (keyWidth + gap), y, keyWidth, height);
            // 用 selected 记住此处真假结果（index == selectedKeyboardKey），供下一条 if 选择执行路径。
            bool selected = index == selectedKeyboardKey;
            // 用 flashing 记住此处真假结果（char.ToLowerInvariant(letters[i]) == highlightedKey && Time.unscaledTime < highlighte），供下一条 if 选择执行路径。
            bool flashing = char.ToLowerInvariant(letters[i]) == highlightedKey && Time.unscaledTime < highlightedKeyUntil;
            // 新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = selected || flashing ? Color.white : new Color(1f, 1f, 1f, .93f);
            // 这里用 || 表示任一条件成立就处理：GUI.Button(rect, letters[i].ToString(), selected || flashing ? keyboardKeySelectedStyle : keyboardKeyStyle)；用于合并几种停止或跳过原因。
            if (GUI.Button(rect, letters[i].ToString(), selected || flashing ? keyboardKeySelectedStyle : keyboardKeyStyle))
            {
                // 把计算结果 index 保存为selectedKeyboardKey；下一条判断或调用会读取这个具体值。
                selectedKeyboardKey = index;
                // 调用 AddCharacter：添加Character。
                AddCharacter(letters[i]);
                // 把计算结果 letters[i] 保存为highlightedKey；下一条判断或调用会读取这个具体值。
                highlightedKey = letters[i];
                // 把计算结果 Time.unscaledTime + .22f 保存为highlightedKeyUntil；下一条判断或调用会读取这个具体值。
                highlightedKeyUntil = Time.unscaledTime + .22f;
            }
        }
    }

    // ！！！功能：DrawActionKey——DrawActionKey！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Rect rect, string label, int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawActionKey(Rect rect, string label, int index)
    {
        // 步骤 1：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = index == selectedKeyboardKey ? Color.white : new Color(1f, 1f, 1f, .95f);
        // 步骤 2：这里的 ! 表示取反；当 GUI.Button(rect, label, index == selectedKeyboardKey ? keyboardKeySelectedStyle : keyboardActionSty 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!GUI.Button(rect, label, index == selectedKeyboardKey ? keyboardKeySelectedStyle : keyboardActionStyle)) return;
        // 步骤 3：把计算结果 index 保存为selectedKeyboardKey；下一条判断或调用会读取这个具体值。
        selectedKeyboardKey = index;
        // 步骤 4：调用 ActivateKeyboardSelection：ActivateKeyboardSelection。
        ActivateKeyboardSelection();
    }
    // ！！！功能结束：DrawActionKey！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnGUI()
    {
        // 步骤 1：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (head == null) return;
        // 步骤 2：调用 EnsureGuiStyles：确保GuiStyles。
        EnsureGuiStyles();
        // 步骤 3：计算 GUI.color，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        Color previous = GUI.color;

        // Head labels follow the local character in screen space so they
        // remain readable from the third-person desktop camera and in XR.
        // 步骤 4：计算 HeadScreenPoint()，把结果存进 headPoint（headPoint）；之后引用 headPoint 时用的就是这次结果。
        Vector2 headPoint = HeadScreenPoint();
        // 步骤 5：用 headVisible 记住此处真假结果（headPoint.x > 0f && headPoint.y > 0f && headPoint.y < Screen.height），供下一条 if 选择执行路径。
        bool headVisible = headPoint.x > 0f && headPoint.y > 0f && headPoint.y < Screen.height;
        // 步骤 6：计算 bubbleWidth 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float bubbleWidth = Mathf.Clamp(Screen.width * .24f, 190f, 420f);
        // 步骤 7：计算 bubbleHeight 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float bubbleHeight = Mathf.Clamp(Screen.height * .052f, 32f, 54f);
        // 步骤 8：把数值 5f 暂存为 gap；它只在当前方法里参与后续判断或计算。
        float gap = 5f;
        // 步骤 9：计算 headPoint.y - bubbleHeight - 14f，把结果存进 top（top）；之后引用 top 时用的就是这次结果。
        float top = headPoint.y - bubbleHeight - 14f;
        // 步骤 10：用三元表达式为 leftTop 二选一：问号前条件为真取前值，为假取冒号后的值。
        float leftTop = IsFirstPerson ? 210f : 106f;
        // 步骤 11：把数值 0 暂存为 headRow；它只在当前方法里参与后续判断或计算。
        int headRow = 0;
        // 步骤 12：把数值 0f 暂存为 leftOffset；它只在当前方法里参与后续判断或计算。
        float leftOffset = 0f;
        // 步骤 13：计算 Mathf.Max(0, SentMessages.Count - 4)，把结果存进 first（first）；之后引用 first 时用的就是这次结果。
        int first = Mathf.Max(0, SentMessages.Count - 4);
        // XR messages are already drawn on a stereo world-space canvas.
        // Drawing IMGUI copies as well can cover the cape-energy fan.
        // 步骤 14：按 int i = first; desktopMode && i < SentMessages.Count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = first; desktopMode && i < SentMessages.Count; i++)
        {
            // 步骤 15：用 onLeft 记住此处真假结果（IsFirstPerson || sentMessageOnLeft[i]），供下一条 if 选择执行路径。
            bool onLeft = IsFirstPerson || sentMessageOnLeft[i];
            // 步骤 16：用三元表达式为 row 二选一：问号前条件为真取前值，为假取冒号后的值。
            int row = onLeft ? 0 : headRow++;
            // 步骤 17：这里用 && 要求两边条件同时成立：!onLeft && !headVisible；任意一边为假就跳过本分支。 成立时执行 continue;
            if (!onLeft && !headVisible) continue;
            // 步骤 18：创建新的 GUIContent 并存为 content；构造参数决定其初始内容或对象名称。
            GUIContent content = new GUIContent(SentMessages[i]);
            // 步骤 19：计算leftWidth并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            float leftWidth = Mathf.Clamp(bubbleLabelStyle.CalcSize(content).x + 30f,
                // 步骤 20：执行 110f, Mathf.Min(440f, Screen.width * .32f));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                110f, Mathf.Min(440f, Screen.width * .32f));
            // 步骤 21：把计算结果 Mathf.Max(34f, 保存为leftHeight；下一条判断或调用会读取这个具体值。
            float leftHeight = Mathf.Max(34f,
                // 步骤 22：调用 CalcHeight：CalcHeight。
                bubbleLabelStyle.CalcHeight(content, leftWidth - 24f) + 14f);
            // 步骤 23：把计算结果 onLeft 保存为rect；下一条判断或调用会读取这个具体值。
            Rect rect = onLeft
                // 步骤 24：三元表达式中，前面的条件为真时选这个值。
                ? new Rect(IsFirstPerson ? (Screen.width - leftWidth) * .5f : 18f,
                    // 步骤 25：这一行补充 leftTop + leftOffset, leftWidth, leftHeight)，与相邻行合起来完成同一次计算或对象配置。
                    leftTop + leftOffset, leftWidth, leftHeight)
                // 步骤 26：三元表达式中，前面的条件为假时选这个备用值。
                : new Rect(headPoint.x - bubbleWidth * .5f,
                    // 步骤 27：执行 top - row * (bubbleHeight + gap), bubbleWidth, bubbleHeight);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    top - row * (bubbleHeight + gap), bubbleWidth, bubbleHeight);
            // 步骤 28：检查条件 onLeft；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 leftOffset += leftHeight + gap;
            if (onLeft) leftOffset += leftHeight + gap;
            // 步骤 29：计算 Time.unscaledTime - sentMessageTimes[i]，把结果存进 age（age）；之后引用 age 时用的就是这次结果。
            float age = Time.unscaledTime - sentMessageTimes[i];
            // 步骤 30：把计算结果 age <= MessageVisibleDuration ? 1f : 保存为alpha；下一条判断或调用会读取这个具体值。
            float alpha = age <= MessageVisibleDuration ? 1f :
                // 步骤 31：执行 1f - Mathf.Clamp01((age - MessageVisibleDuration) / MessageFadeDuration);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                1f - Mathf.Clamp01((age - MessageVisibleDuration) / MessageFadeDuration);
            // 步骤 32：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = onLeft ? new Color(1f, 1f, 1f, alpha) :
                // 步骤 33：用 new 创建 Color(.035f, .045f, .055f, .76f * alpha);；构造参数决定新对象的初始内容。
                new Color(.035f, .045f, .055f, .76f * alpha);
            // 步骤 34：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            GUI.Box(rect, GUIContent.none, onLeft ? leftBubbleStyle : messageStyle);
            // 步骤 35：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = new Color(1f, 1f, 1f, alpha);
            // 步骤 36：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(rect, SentMessages[i], bubbleLabelStyle);
        }
        // 步骤 37：这里用 && 要求两边条件同时成立：desktopMode && headVisible && IsOpen；任意一边为假就跳过本分支。
        if (desktopMode && headVisible && IsOpen)
        {
            // 步骤 38：创建新的 Rect 并存为 typing；构造参数决定其初始内容或对象名称。
            Rect typing = new Rect(headPoint.x - 31f, top - headRow * (bubbleHeight + gap) - 38f, 62f, 34f);
            // 步骤 39：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = new Color(1f, .93f, .66f, .94f);
            // 步骤 40：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            GUI.Box(typing, GUIContent.none, typingStyle);
            // 步骤 41：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = new Color(.28f, .25f, .16f, 1f);
            // 步骤 42：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(typing, "…", typingStyle);
        }

        // In third-person Quest mode only world-anchored chat and bubbles are
        // visible; the head-locked icons and screen keyboard belong to first-person.
        // 步骤 43：这里用 && 要求两边条件同时成立：!desktopMode && !IsFirstPerson；任意一边为假就跳过本分支。
        if (!desktopMode && !IsFirstPerson)
        {
            // 步骤 44：把计算结果 previous 保存为color；下一条判断或调用会读取这个具体值。
            GUI.color = previous;
            // 步骤 45：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 46：计算 edge 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float edge = Mathf.Clamp(Screen.width * .018f, 12f, 28f);
        // 步骤 47：计算 iconSize 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float iconSize = Mathf.Clamp(Screen.width * .047f, 52f, 76f);
        // 步骤 48：计算 Screen.width - edge - iconSize，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
        float x = Screen.width - edge - iconSize;
        // 步骤 49：计算 keyboardHeight 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float keyboardHeight = chatOpen ? Mathf.Clamp(Screen.height * .38f, 260f, 430f) : 0f;
        // 步骤 50：计算 Screen.height - edge - iconSize - keyboardHeight，把结果存进 chatY（chaty）；之后引用 chatY 时用的就是这次结果。
        float chatY = Screen.height - edge - iconSize - keyboardHeight;
        // 步骤 51：计算 chatY - iconSize - 12f，把结果存进 micY（micy）；之后引用 micY 时用的就是这次结果。
        float micY = chatY - iconSize - 12f;
        // 步骤 52：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = Color.white;
        // 步骤 53：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(new Rect(x - 5f, micY - 5f, iconSize + 10f, iconSize * 2f + 29f),
            // 步骤 54：执行 GUIContent.none, controlsBackdropStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            GUIContent.none, controlsBackdropStyle);
        // 步骤 55：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = new Color(.98f, .95f, .84f, 1f);
        // 步骤 56：检查条件 GUI.Button(new Rect(x, micY, iconSize, iconSize), "🎙", iconStyle)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 DesktopVoice：桌面Voice。
        if (GUI.Button(new Rect(x, micY, iconSize, iconSize), "🎙", iconStyle)) DesktopVoice();
        // 步骤 57：检查条件 GUI.Button(new Rect(x, chatY, iconSize, iconSize), "💬", iconStyle)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 OpenChat：OpenChat。
        if (GUI.Button(new Rect(x, chatY, iconSize, iconSize), "💬", iconStyle)) OpenChat();
        // 步骤 58：检查条件 chatOpen；只有为真才运行下面的步骤，否则沿原流程继续。
        if (chatOpen)
        {
            // 步骤 59：计算 barHeight 并限制在安全上下界，防止不合理的大小影响移动或显示。
            float barHeight = Mathf.Clamp(Screen.height * .052f, 42f, 62f);
            // 步骤 60：计算 Screen.width * .74f，把结果存进 barWidth（barWidth）；之后引用 barWidth 时用的就是这次结果。
            float barWidth = Screen.width * .74f;
            // 步骤 61：计算 (Screen.width - barWidth) * .5f，把结果存进 barX（barx）；之后引用 barX 时用的就是这次结果。
            float barX = (Screen.width - barWidth) * .5f;
            // 步骤 62：计算 Screen.height - keyboardHeight - barHeight - 12f，把结果存进 barY（bary）；之后引用 barY 时用的就是这次结果。
            float barY = Screen.height - keyboardHeight - barHeight - 12f;
            // 步骤 63：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
            GUI.color = Color.white;
            // 步骤 64：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            GUI.Box(new Rect(barX, barY, barWidth, barHeight), GUIContent.none);
            // 步骤 65：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = new Color(1f, .98f, .9f, 1f);
            // 步骤 66：创建新的 Rect 并存为 inputRect；构造参数决定其初始内容或对象名称。
            Rect inputRect = new Rect(barX + 8f, barY + 3f, barWidth - 96f, barHeight - 6f);
            // 步骤 67：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            GUI.Box(inputRect, GUIContent.none, messageStyle);
            // 步骤 68：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(inputRect, string.IsNullOrEmpty(CurrentDraft) ? "聊一聊…" : CurrentDraft, inputLabelStyle);
            // 步骤 69：检查条件 GUI.Button(new Rect(barX + barWidth - 82f, barY + 3f, 76f, barHeight - 6f), "发送", buttonStyle)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SendChat：SendChat。
            if (GUI.Button(new Rect(barX + barWidth - 82f, barY + 3f, 76f, barHeight - 6f), "发送", buttonStyle)) SendChat();
            // 步骤 70：调用 DrawVirtualKeyboard：DrawVirtualKeyboard。
            DrawVirtualKeyboard(edge);
            // 步骤 71：计算 Event.current，把结果存进 evt（evt）；之后引用 evt 时用的就是这次结果。
            Event evt = Event.current;
            // 步骤 72：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (evt != null && evt.type == EventType.KeyDown &&
                // 步骤 73：这一行补充 (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter))，与相邻行合起来完成同一次计算或对象配置。
                (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter))
            {
                // 步骤 74：调用 SendChat：SendChat。
                SendChat();
                // 步骤 75：调用 Use：Use。
                evt.Use();
            }
        }
        // 步骤 76：把计算结果 previous 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = previous;
    }
    // ！！！功能结束：OnGUI！！！
}

// 定义 SkyQuestKey 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestKey : MonoBehaviour
{
    // 声明 value（value）：文字内容；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public string value;
}
