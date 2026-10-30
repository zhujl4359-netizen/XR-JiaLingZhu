// ！！！文件功能：左侧英文操作指南抽屉和飞行指示牌说明面板！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;
// 引入 UnityEngine.UI 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.UI;

/// <summary>
/// English controls drawer and the flight-course rules sign for both desktop and Quest.
/// All help panels are visual only: no HUD collider is allowed to affect player movement.
/// </summary>
// 定义 SkyQuestGuideUI 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class SkyQuestGuideUI : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;

    // 这一行补充 private static readonly string[,] QuestControls =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly string[,] QuestControls =
    {
        // 继续填写上一行调用的参数：{ "LEFT STICK", "Move / steer while flying" }；参数位置决定它表示的坐标、速度或选项。
        { "LEFT STICK", "Move / steer while flying" },
        // 继续填写上一行调用的参数：{ "LEFT STICK CLICK", "Open / close the chat keyboard" }；参数位置决定它表示的坐标、速度或选项。
        { "LEFT STICK CLICK", "Open / close the chat keyboard" },
        // 继续填写上一行调用的参数：{ "X", "Open / close a nearby ancestor tree" }；参数位置决定它表示的坐标、速度或选项。
        { "X", "Open / close a nearby ancestor tree" },
        // 继续填写上一行调用的参数：{ "Y", "Preview / cancel a tea table" }；参数位置决定它表示的坐标、速度或选项。
        { "Y", "Preview / cancel a tea table" },
        // 继续填写上一行调用的参数：{ "LEFT TRIGGER TAP", "Drop a held item / light a candle" }；参数位置决定它表示的坐标、速度或选项。
        { "LEFT TRIGGER TAP", "Drop a held item / light a candle" },
        // 继续填写上一行调用的参数：{ "LEFT TRIGGER HOLD", "English voice message" }；参数位置决定它表示的坐标、速度或选项。
        { "LEFT TRIGGER HOLD", "English voice message" },
        // 继续填写上一行调用的参数：{ "LEFT GRIP + B", "Toggle comfort teleport" }；参数位置决定它表示的坐标、速度或选项。
        { "LEFT GRIP + B", "Toggle comfort teleport" },
        // 继续填写上一行调用的参数：{ "RIGHT STICK", "Turn smoothly" }；参数位置决定它表示的坐标、速度或选项。
        { "RIGHT STICK", "Turn smoothly" },
        // 继续填写上一行调用的参数：{ "RIGHT STICK CLICK", "Slow down into a safe glide" }；参数位置决定它表示的坐标、速度或选项。
        { "RIGHT STICK CLICK", "Slow down into a safe glide" },
        // 继续填写上一行调用的参数：{ "A", "Take off / boost (uses 1 cape energy)" }；参数位置决定它表示的坐标、速度或选项。
        { "A", "Take off / boost (uses 1 cape energy)" },
        // 继续填写上一行调用的参数：{ "B", "Switch first / third person" }；参数位置决定它表示的坐标、速度或选项。
        { "B", "Switch first / third person" },
        // 继续填写上一行调用的参数：{ "RIGHT TRIGGER", "Hold on any loose item to aim; release to throw" }；参数位置决定它表示的坐标、速度或选项。
        { "RIGHT TRIGGER", "Hold on any loose item to aim; release to throw" },
        // 继续填写上一行调用的参数：{ "RIGHT TRIGGER ON TABLE", "Open the tea table keyboard" }；参数位置决定它表示的坐标、速度或选项。
        { "RIGHT TRIGGER ON TABLE", "Open the tea table keyboard" },
        // 继续填写上一行调用的参数：{ "RIGHT GRIP", "Pick up / drop pottery or exchanged items" }；参数位置决定它表示的坐标、速度或选项。
        { "RIGHT GRIP", "Pick up / drop pottery or exchanged items" },
        // 继续填写上一行调用的参数：{ "RIGHT TRIGGER NEAR FACE", "Eat a held reward (+energy or candles)" }；参数位置决定它表示的坐标、速度或选项。
        { "RIGHT TRIGGER NEAR FACE", "Eat a held reward (+energy or candles)" },
        // 继续填写上一行调用的参数：{ "BOTH STICKS CLICK", "Calibrate seated height" }；参数位置决定它表示的坐标、速度或选项。
        { "BOTH STICKS CLICK", "Calibrate seated height" },
        // 继续填写上一行调用的参数：{ "BOTH GRIPS OVERHEAD", "Open the original ancestor tree" }；参数位置决定它表示的坐标、速度或选项。
        { "BOTH GRIPS OVERHEAD", "Open the original ancestor tree" },
        // 这一行补充 { "META BUTTON", "Open the headset system menu" }，与相邻行合起来完成同一次计算或对象配置。
        { "META BUTTON", "Open the headset system menu" }
    };

    // 这一行补充 private static readonly string[,] DesktopControls =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly string[,] DesktopControls =
    {
        // 继续填写上一行调用的参数：{ "W A S D", "Move / steer while flying" }；参数位置决定它表示的坐标、速度或选项。
        { "W A S D", "Move / steer while flying" },
        // 继续填写上一行调用的参数：{ "MOUSE", "Look around" }；参数位置决定它表示的坐标、速度或选项。
        { "MOUSE", "Look around" },
        // 继续填写上一行调用的参数：{ "SPACE", "Take off / boost (uses 1 cape energy)" }；参数位置决定它表示的坐标、速度或选项。
        { "SPACE", "Take off / boost (uses 1 cape energy)" },
        // 继续填写上一行调用的参数：{ "LEFT CTRL", "Descend while flying" }；参数位置决定它表示的坐标、速度或选项。
        { "LEFT CTRL", "Descend while flying" },
        // 继续填写上一行调用的参数：{ "1 / 3", "First / third person" }；参数位置决定它表示的坐标、速度或选项。
        { "1 / 3", "First / third person" },
        // 继续填写上一行调用的参数：{ "F", "Open a nearby ancestor tree" }；参数位置决定它表示的坐标、速度或选项。
        { "F", "Open a nearby ancestor tree" },
        // 继续填写上一行调用的参数：{ "CLICK + HOLD", "Exchange a selected tree item" }；参数位置决定它表示的坐标、速度或选项。
        { "CLICK + HOLD", "Exchange a selected tree item" },
        // 继续填写上一行调用的参数：{ "E", "Eat held item / equip ancestor item" }；参数位置决定它表示的坐标、速度或选项。
        { "E", "Eat held item / equip ancestor item" },
        // 继续填写上一行调用的参数：{ "Z", "Pick up / drop exchanged item" }；参数位置决定它表示的坐标、速度或选项。
        { "Z", "Pick up / drop exchanged item" },
        // 继续填写上一行调用的参数：{ "HOLD Q", "Aim and throw held item" }；参数位置决定它表示的坐标、速度或选项。
        { "HOLD Q", "Aim and throw held item" },
        // 继续填写上一行调用的参数：{ "P / R", "Preview / place tea table" }；参数位置决定它表示的坐标、速度或选项。
        { "P / R", "Preview / place tea table" },
        // 继续填写上一行调用的参数：{ "CLICK TABLE", "Open the tea table keyboard" }；参数位置决定它表示的坐标、速度或选项。
        { "CLICK TABLE", "Open the tea table keyboard" },
        // 继续填写上一行调用的参数：{ "T / K", "Open chat" }；参数位置决定它表示的坐标、速度或选项。
        { "T / K", "Open chat" },
        // 继续填写上一行调用的参数：{ "ARROW KEYS", "Turn / tilt camera" }；参数位置决定它表示的坐标、速度或选项。
        { "ARROW KEYS", "Turn / tilt camera" },
        // 这一行补充 { "F1", "Show / hide this guide" }，与相邻行合起来完成同一次计算或对象配置。
        { "F1", "Show / hide this guide" }
    };

    // 这一行补充 private static readonly string[,] QuestFlightControls =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly string[,] QuestFlightControls =
    {
        // 继续填写上一行调用的参数：{ "BLUE RING", "Enter to start or replay; cape refills" }；参数位置决定它表示的坐标、速度或选项。
        { "BLUE RING", "Enter to start or replay; cape refills" },
        // 继续填写上一行调用的参数：{ "LEFT STICK", "Steer through the floating flames" }；参数位置决定它表示的坐标、速度或选项。
        { "LEFT STICK", "Steer through the floating flames" },
        // 继续填写上一行调用的参数：{ "RIGHT STICK", "Turn smoothly" }；参数位置决定它表示的坐标、速度或选项。
        { "RIGHT STICK", "Turn smoothly" },
        // 继续填写上一行调用的参数：{ "A", "Take off / boost; costs 1 energy" }；参数位置决定它表示的坐标、速度或选项。
        { "A", "Take off / boost; costs 1 energy" },
        // 继续填写上一行调用的参数：{ "RIGHT STICK CLICK", "Slow down into a safe glide" }；参数位置决定它表示的坐标、速度或选项。
        { "RIGHT STICK CLICK", "Slow down into a safe glide" },
        // 继续填写上一行调用的参数：{ "FLAMES", "Fly through each one; no trigger needed" }；参数位置决定它表示的坐标、速度或选项。
        { "FLAMES", "Fly through each one; no trigger needed" },
        // 这一行补充 { "NEXT HINT", "Follow the distance and direction on HUD" }，与相邻行合起来完成同一次计算或对象配置。
        { "NEXT HINT", "Follow the distance and direction on HUD" }
    };

    // 这一行补充 private static readonly string[,] DesktopFlightControls =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly string[,] DesktopFlightControls =
    {
        // 继续填写上一行调用的参数：{ "BLUE RING", "Enter to start or replay; cape refills" }；参数位置决定它表示的坐标、速度或选项。
        { "BLUE RING", "Enter to start or replay; cape refills" },
        // 继续填写上一行调用的参数：{ "W A S D", "Steer through the floating flames" }；参数位置决定它表示的坐标、速度或选项。
        { "W A S D", "Steer through the floating flames" },
        // 继续填写上一行调用的参数：{ "MOUSE", "Look around" }；参数位置决定它表示的坐标、速度或选项。
        { "MOUSE", "Look around" },
        // 继续填写上一行调用的参数：{ "SPACE", "Take off / boost; costs 1 energy" }；参数位置决定它表示的坐标、速度或选项。
        { "SPACE", "Take off / boost; costs 1 energy" },
        // 继续填写上一行调用的参数：{ "LEFT CTRL", "Descend" }；参数位置决定它表示的坐标、速度或选项。
        { "LEFT CTRL", "Descend" },
        // 继续填写上一行调用的参数：{ "FLAMES", "Fly through each one; no click needed" }；参数位置决定它表示的坐标、速度或选项。
        { "FLAMES", "Fly through each one; no click needed" },
        // 这一行补充 { "NEXT HINT", "Follow the distance and direction on HUD" }，与相邻行合起来完成同一次计算或对象配置。
        { "NEXT HINT", "Follow the distance and direction on HUD" }
    };

    // player 保存player（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion player;
    // course 保存course（类型 SkyQuestFlightFlameGame）；下方方法依靠它记录或查找场景状态。
    private SkyQuestFlightFlameGame course;
    // 声明 signBoard（signBoard）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform signBoard;
    // 用 guideOpen 记住此处真假结果（true），供下一条 if 选择执行路径。
    private bool guideOpen = true;
    // 声明 flightOpen（flightOpen）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool flightOpen;
    // 声明 guideBeforeFlight（guideBeforeFlight）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool guideBeforeFlight;
    // 声明 previousRightTrigger（previousRightTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousRightTrigger;
    // 声明 lastThirdPerson（lastThirdPerson）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool lastThirdPerson;
    private float signAimStarted = -1f;
    // guidePanel 保存guide面板（类型 RectTransform）；下方方法依靠它记录或查找场景状态。
    private RectTransform guidePanel;
    // guideClose 保存guideClose（类型 RectTransform）；下方方法依靠它记录或查找场景状态。
    private RectTransform guideClose;
    // guideTab 保存guideTab（类型 RectTransform）；下方方法依靠它记录或查找场景状态。
    private RectTransform guideTab;
    // flightPanel 保存flight面板（类型 RectTransform）；下方方法依靠它记录或查找场景状态。
    private RectTransform flightPanel;
    // flightClose 保存flightClose（类型 RectTransform）；下方方法依靠它记录或查找场景状态。
    private RectTransform flightClose;
    // tabText 保存tabText（类型 TextMeshProUGUI）；下方方法依靠它记录或查找场景状态。
    private TextMeshProUGUI tabText;
    // headingStyle 保存headingStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle headingStyle;
    // cellStyle 保存cellStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle cellStyle;
    // keyStyle 保存keyStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle keyStyle;
    // buttonStyle 保存buttonStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle buttonStyle;
    // 声明 guideScroll（guideScroll）：二维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector2 guideScroll;
    // 声明 flightScroll（flightScroll）：二维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector2 flightScroll;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Start()
    {
        // 步骤 1：从目标 GameObject 查找并保存 player 组件；后面的移动或显示逻辑会直接使用这个缓存。
        player = GetComponent<SkyQuestLocomotion>();
        // 步骤 2：检查头显相机、player是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 head = player.head;
        if (head == null && player != null) head = player.head;
        // 步骤 3：执行 desktopMode |= player != null && player.desktopControlled;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        desktopMode |= player != null && player.desktopControlled;
        // 步骤 4：检查头显相机是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 BuildQuestHud：构建QuestHud。
        if (!desktopMode && head != null) BuildQuestHud();
        // 步骤 5：把计算结果 player != null && player.ThirdPerson 保存为lastThirdPerson；下一条判断或调用会读取这个具体值。
        lastThirdPerson = player != null && player.ThirdPerson;
        // 步骤 6：调用 TryBuildSign：Try构建Sign。
        TryBuildSign();
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Update()
    {
        // 步骤 1：检查signBoard是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时调用 TryBuildSign：Try构建Sign。
        if (signBoard == null) TryBuildSign();
        // 步骤 2：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 3：先确认启用了键盘备用操作且当前存在键盘设备；桌面测试时才能读取按键。
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                // 步骤 4：把计算结果 !guideOpen 保存为guideOpen；下一条判断或调用会读取这个具体值。
                guideOpen = !guideOpen;
            // 步骤 5：计算 Mouse.current，把结果存进 mouse（mouse）；之后引用 mouse 时用的就是这次结果。
            Mouse mouse = Mouse.current;
            // 步骤 6：检查mouse、signBoard是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && signBoard != null)
            {
                // 步骤 7：计算 Camera.main，把结果存进 camera（camera）；之后引用 camera 时用的就是这次结果。
                Camera camera = Camera.main;
                // 步骤 8：确认camera已经有值；只有存在时才能安全读取它的属性或调用方法。
                if (camera != null)
                {
                    // 步骤 9：把计算结果 Cursor.lockState == CursorLockMode.Locked ? 保存为mousePoint；下一条判断或调用会读取这个具体值。
                    Vector2 mousePoint = Cursor.lockState == CursorLockMode.Locked ?
                        // 步骤 10：用 new 创建 Vector2(Screen.width * .5f, Screen.height * .5f) : mouse.position.ReadValue(；构造参数决定新对象的初始内容。
                        new Vector2(Screen.width * .5f, Screen.height * .5f) : mouse.position.ReadValue();
                    // 步骤 11：这里用 && 要求两边条件同时成立：!IsOverDesktopPanel(mousePoint) && HitSign(camera.ScreenPointToRay(mousePoint), 9f)；任意一边为假就跳过本分支。
                    if (!IsOverDesktopPanel(mousePoint) && HitSign(camera.ScreenPointToRay(mousePoint), 9f))
                        // 步骤 12：调用 OpenFlightRules：打开飞行玩法的详细按键说明。
                        OpenFlightRules();
                }
            }
            // 步骤 13：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 14：用 thirdPerson 记住此处真假结果（player != null && player.ThirdPerson），供下一条 if 选择执行路径。
        bool thirdPerson = player != null && player.ThirdPerson;
        // 步骤 15：检查条件 thirdPerson != lastThirdPerson；只有为真才运行下面的步骤，否则沿原流程继续。
        if (thirdPerson != lastThirdPerson)
        {
            // 步骤 16：把计算结果 thirdPerson 保存为lastThirdPerson；下一条判断或调用会读取这个具体值。
            lastThirdPerson = thirdPerson;
            // 步骤 17：调用 RefreshQuestHud：刷新QuestHud。
            RefreshQuestHud();
        }
        // 步骤 18：检查条件 thirdPerson；只有为真才运行下面的步骤，否则沿原流程继续。
        if (thirdPerson)
        {
            signAimStarted = -1f;
            previousRightTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f;
            return;
        }

        // Holding the right controller's aim on the pink sign for three seconds
        // reveals the existing rule-and-controls panel. No trigger is needed.
        bool aimingAtSign = !flightOpen && signBoard != null && player != null &&
            player.rightHand != null && HitSign(
                new Ray(player.rightHand.position, player.rightHand.forward), 5f);
        if (aimingAtSign)
        {
            if (signAimStarted < 0f) signAimStarted = Time.unscaledTime;
            if (Time.unscaledTime - signAimStarted >= 3f)
            {
                OpenFlightRules();
                SkyQuestInput.Pulse(SkyQuestInput.Right, .3f, .08f);
                signAimStarted = -1f;
            }
        }
        else signAimStarted = -1f;

        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f;
        // 步骤 22：检查player、player的右手柄位置是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (trigger && !previousRightTrigger && player != null && player.rightHand != null)
        {
            // 步骤 23：创建新的 Ray 并存为 ray；构造参数决定其初始内容或对象名称。
            Ray ray = new Ray(player.rightHand.position, player.rightHand.forward);
            // 步骤 24：这里用 && 要求两边条件同时成立：flightOpen && HitUi(ray, flightClose, 3f)；任意一边为假就跳过本分支。 成立时调用 CloseFlightRules：关闭飞行玩法说明并恢复原有视野。
            if (flightOpen && HitUi(ray, flightClose, 3f)) CloseFlightRules();
            // 步骤 25：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：!flightOpen && guideOpen && HitUi(ray, guideClose, 3f, 12f)；任意一边为假就跳过本分支。
            else if (!flightOpen && guideOpen && HitUi(ray, guideClose, 3f, 12f)) ToggleGuide();
            // 步骤 26：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：!flightOpen && HitUi(ray, guideTab, 3f, 22f)；任意一边为假就跳过本分支。
            else if (!flightOpen && HitUi(ray, guideTab, 3f, 22f)) ToggleGuide();
            // 步骤 27：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查signBoard是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        }
        // 步骤 28：把计算结果 trigger 保存为previousRightTrigger；下一条判断或调用会读取这个具体值。
        previousRightTrigger = trigger;
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：TryBuildSign——Try构建Sign！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void TryBuildSign()
    {
        // 步骤 1：检查course是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 course = FindFirstObjectByType<SkyQuestFlightFlameGame>();
        if (course == null) course = FindFirstObjectByType<SkyQuestFlightFlameGame>();
        // 步骤 2：检查course、course的startMarker是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (course == null || course.startMarker == null) return;
        // 步骤 3：计算 course.startMarker.Find("Flight rules sign")，把结果存进 existing（existing）；之后引用 existing 时用的就是这次结果。
        Transform existing = course.startMarker.Find("Flight rules sign");
        // 步骤 4：确认existing已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (existing != null)
        {
            // 步骤 5：把计算结果 existing.Find("Sign board") 保存为signBoard；下一条判断或调用会读取这个具体值。
            signBoard = existing.Find("Sign board");
            // 步骤 6：提前结束这个方法；不再执行下方语句。
            return;
        }

        // Stand beside the blue start ring, outside its activation radius and flight path.
        // 步骤 7：创建新的 Transform 并存为 root；构造参数决定其初始内容或对象名称。
        Transform root = new GameObject("Flight rules sign").transform;
        // 步骤 8：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        root.SetParent(course.startMarker, false);
        // 步骤 9：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        root.localPosition = new Vector3(-2.75f, -.08f, -.65f);
        // 步骤 10：恢复 root.localRotation 到初始本地朝向；本地表示相对于父对象。
        root.localRotation = Quaternion.identity;
        // 步骤 11：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part("Sign post", root, new Vector3(0f, .75f, 0f),
            new Vector3(.15f, 1.5f, .15f), new Color(.46f, .21f, .37f));
        GameObject board = Part("Sign board", root, new Vector3(0f, 1.68f, 0f),
            new Vector3(1.9f, 1.05f, .1f), new Color(1f, .27f, .58f));
        signBoard = board.transform;
        Color trim = new Color(1f, .86f, .96f);
        Part("Top trim", root, new Vector3(0f, 2.23f, 0f), new Vector3(2.04f, .08f, .15f), trim);
        Part("Bottom trim", root, new Vector3(0f, 1.13f, 0f), new Vector3(2.04f, .08f, .15f), trim);
        Part("Left trim", root, new Vector3(-.99f, 1.68f, 0f), new Vector3(.08f, 1.14f, .15f), trim);
        Part("Right trim", root, new Vector3(.99f, 1.68f, 0f), new Vector3(.08f, 1.14f, .15f), trim);
        string label = "FLIGHT FLAMES\n" + (desktopMode ? "CLICK: RULES" : "AIM 3s: RULES");
        // 步骤 22：调用 SignText：SignText。
        SignText("Front rules", label, root, new Vector3(0f, 1.68f, .061f), Quaternion.Euler(0f, 180f, 0f));
        SignText("Back rules", label, root, new Vector3(0f, 1.68f, -.061f), Quaternion.identity);
    }
    // ！！！功能结束：TryBuildSign！！！

    // ！！！功能：Part——创建或查找一个局部模型部件，供斗篷、角色或 UI 组合！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Transform parent, Vector3 position, Vector3 scale, Color color 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject Part(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        // 步骤 1：计算 SkyQuestVisuals.Primitive(name, PrimitiveType.Cube, parent, position, scale, color)，把结果存进 part（part）；之后引用 part 时用的就是这次结果。
        GameObject part = SkyQuestVisuals.Primitive(name, PrimitiveType.Cube, parent, position, scale, color);
        // 步骤 2：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(part);
        // 步骤 3：把 part 作为方法结果返回给调用方；return 后本方法立即结束。
        return part;
    }
    // ！！！功能结束：Part！！！

    // ！！！功能：SignText——SignText！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, string value, Transform parent, Vector3 position, Quaternion rotation 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void SignText(string name, string value, Transform parent, Vector3 position, Quaternion rotation)
    {
        // 步骤 1：计算 SkyQuestVisuals.Text(name, value, parent, position, 2.25f, Color.white, 10f)，把结果存进 label（label）；之后引用 label 时用的就是这次结果。
        TextMeshPro label = SkyQuestVisuals.Text(name, value, parent, position, 12f, Color.white, 14f);
        label.transform.localRotation = rotation;
        label.rectTransform.sizeDelta = new Vector2(14f, 5.4f);
        // 步骤 4：把计算结果 TextAlignmentOptions.Center 保存为alignment；下一条判断或调用会读取这个具体值。
        label.alignment = TextAlignmentOptions.Center;
        // 步骤 5：把 TextWrappingModes.NoWrap 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
        label.textWrappingMode = TextWrappingModes.NoWrap;
    }
    // ！！！功能结束：SignText！！！

    // Ray/plane checks give the headset UI and sign clickable areas without physics colliders.
    // ！！！功能：HitSign——HitSign！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Ray ray, float maxDistance 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private bool HitSign(Ray ray, float maxDistance)
    {
        // 步骤 1：检查signBoard是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (signBoard == null) return false;
        // 步骤 2：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!new Plane(signBoard.forward, signBoard.position).Raycast(ray, out float distance) ||
            // 步骤 3：执行 distance < 0f || distance > maxDistance) return false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            distance < 0f || distance > maxDistance) return false;
        // 步骤 4：计算 signBoard.InverseTransformPoint(ray.GetPoint(distance))，把结果存进 local（local）；之后引用 local 时用的就是这次结果。
        Vector3 local = signBoard.InverseTransformPoint(ray.GetPoint(distance));
        // 步骤 5：把 Mathf.Abs(local.x) <= .5f && Mathf.Abs(local.y) <= .5f 作为方法结果返回给调用方；return 后本方法立即结束。
        return Mathf.Abs(local.x) <= .5f && Mathf.Abs(local.y) <= .5f;
    }
    // ！！！功能结束：HitSign！！！

    // 继续填写上一行调用的参数：private static bool HitUi(Ray ray, RectTransform rect, float maxDistance；参数位置决定它表示的坐标、速度或选项。
    private static bool HitUi(Ray ray, RectTransform rect, float maxDistance,
        // 把计算结果 0f) 保存为padding；下一条判断或调用会读取这个具体值。
        float padding = 0f)
    {
        // 检查rect是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (rect == null || !rect.gameObject.activeInHierarchy) return false;
        // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!new Plane(rect.forward, rect.position).Raycast(ray, out float distance) ||
            // 执行 distance < 0f || distance > maxDistance) return false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            distance < 0f || distance > maxDistance) return false;
        // 计算 rect.InverseTransformPoint(ray.GetPoint(distance))，把结果存进 local（local）；之后引用 local 时用的就是这次结果。
        Vector3 local = rect.InverseTransformPoint(ray.GetPoint(distance));
        // 把 Rect.MinMaxRect(rect.rect.xMin - padding, rect.rect.yMin - padding, 作为方法结果返回给调用方；return 后本方法立即结束。
        return Rect.MinMaxRect(rect.rect.xMin - padding, rect.rect.yMin - padding,
            // 这一行补充 rect.rect.xMax + padding, rect.rect.yMax + padding).Contains(，与相邻行合起来完成同一次计算或对象配置。
            rect.rect.xMax + padding, rect.rect.yMax + padding).Contains(
                // 用 new 创建 Vector2(local.x, local.y));；构造参数决定新对象的初始内容。
                new Vector2(local.x, local.y));
    }

    // ！！！功能：ToggleGuide——ToggleGuide！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ToggleGuide()
    {
        // 步骤 1：把计算结果 !guideOpen 保存为guideOpen；下一条判断或调用会读取这个具体值。
        guideOpen = !guideOpen;
        // 步骤 2：调用 RefreshQuestHud：刷新QuestHud。
        RefreshQuestHud();
        // 步骤 3：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right, .22f, .05f);
    }
    // ！！！功能结束：ToggleGuide！！！

    // ！！！功能：OpenFlightRules——打开飞行玩法的详细按键说明！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OpenFlightRules()
    {
        // 步骤 1：检查条件 flightOpen；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (flightOpen) return;
        // 步骤 2：把计算结果 guideOpen 保存为guideBeforeFlight；下一条判断或调用会读取这个具体值。
        guideBeforeFlight = guideOpen;
        // 步骤 3：把 flightOpen 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        flightOpen = true;
        // 步骤 4：把 guideOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        guideOpen = false;
        // 步骤 5：调用 RefreshQuestHud：刷新QuestHud。
        RefreshQuestHud();
    }
    // ！！！功能结束：OpenFlightRules！！！

    // ！！！功能：CloseFlightRules——关闭飞行玩法说明并恢复原有视野！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CloseFlightRules()
    {
        // 步骤 1：把 flightOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        flightOpen = false;
        // 步骤 2：把计算结果 guideBeforeFlight 保存为guideOpen；下一条判断或调用会读取这个具体值。
        guideOpen = guideBeforeFlight;
        // 步骤 3：调用 RefreshQuestHud：刷新QuestHud。
        RefreshQuestHud();
    }
    // ！！！功能结束：CloseFlightRules！！！

    // ！！！功能：RefreshQuestHud——刷新QuestHud！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RefreshQuestHud()
    {
        // 步骤 1：用 visible 记住此处真假结果（desktopMode || player == null || !player.ThirdPerson），供下一条 if 选择执行路径。
        bool visible = desktopMode || player == null || !player.ThirdPerson;
        // 步骤 2：确认guide面板已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (guidePanel != null) guidePanel.gameObject.SetActive(visible && guideOpen && !flightOpen);
        // 步骤 3：确认guideTab已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (guideTab != null) guideTab.gameObject.SetActive(visible && !flightOpen);
        // 步骤 4：确认flight面板已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (flightPanel != null) flightPanel.gameObject.SetActive(visible && flightOpen);
        // 步骤 5：确认tabText已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 tabText.text = guideOpen ? "CLOSE\n◀" : "OPEN\n▶";
        if (tabText != null) tabText.text = guideOpen ? "CLOSE\n◀" : "OPEN\n▶";
    }
    // ！！！功能结束：RefreshQuestHud！！！

    // ！！！功能：BuildQuestHud——构建QuestHud！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BuildQuestHud()
    {
        // 步骤 1：创建新的 GameObject 并存为 root；构造参数决定其初始内容或对象名称。
        GameObject root = new GameObject("Quest help HUD", typeof(RectTransform), typeof(Canvas));
        // 步骤 2：执行 root.transform.SetParent(head, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        root.transform.SetParent(head, false);
        // 步骤 3：执行 root.transform.localPosition = new Vector3(0f, .07f, 1.25f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        root.transform.localPosition = new Vector3(0f, .07f, 1.25f);
        // 步骤 4：执行 root.transform.localRotation = Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        root.transform.localRotation = Quaternion.identity;
        // 步骤 5：执行 root.transform.localScale = Vector3.one * .001f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        root.transform.localScale = Vector3.one * .001f;
        // 步骤 6：取目标对象的 Transform 并存为 canvasRect；Transform 提供位置、旋转和父子关系。
        RectTransform canvasRect = (RectTransform)root.transform;
        // 步骤 7：新建用于sizeDelta的对象或数值结构，并把构造参数保存进去。
        canvasRect.sizeDelta = new Vector2(1200f, 800f);
        // 步骤 8：从目标物体找到所需组件并存为 canvas；使用前应检查是否为 null。
        Canvas canvas = root.GetComponent<Canvas>();
        // 步骤 9：把计算结果 RenderMode.WorldSpace 保存为render模式；下一条判断或调用会读取这个具体值。
        canvas.renderMode = RenderMode.WorldSpace;
        // 步骤 10：把计算结果 25 保存为sortingOrder；下一条判断或调用会读取这个具体值。
        canvas.sortingOrder = 25;

        // 步骤 11：新建用于guide面板的对象或数值结构，并把构造参数保存进去。
        guidePanel = Panel("Controls drawer", canvasRect, new Vector2(-310f, -5f),
            // 步骤 12：用 new 创建 Vector2(510f, 610f), .80f);；构造参数决定新对象的初始内容。
            new Vector2(510f, 610f), .80f);
        // 步骤 13：调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Controls title", "QUEST 3S CONTROLS", guidePanel,
            // 步骤 14：用 new 创建 Vector2(0f, 275f), new Vector2(450f, 39f), 25f, TextAlignmentOptions.Center,；构造参数决定新对象的初始内容。
            new Vector2(0f, 275f), new Vector2(450f, 39f), 25f, TextAlignmentOptions.Center, true);
        // 步骤 15：新建用于guideClose的对象或数值结构，并把构造参数保存进去。
        guideClose = Panel("Close controls menu", guidePanel, new Vector2(226f, 273f),
            // 步骤 16：用 new 创建 Vector2(50f, 48f), .55f);；构造参数决定新对象的初始内容。
            new Vector2(50f, 48f), .55f);
        // 步骤 17：调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Close controls icon", "×", guideClose, Vector2.zero,
            // 步骤 18：用 new 创建 Vector2(46f, 44f), 28f, TextAlignmentOptions.Center, true);；构造参数决定新对象的初始内容。
            new Vector2(46f, 44f), 28f, TextAlignmentOptions.Center, true);
        // 步骤 19：调用 BuildTable：构建Table。
        BuildTable(guidePanel, QuestControls, 204f, 31f, 470f, 17f);
        // 步骤 20：调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Controls hint", "Aim at X + RIGHT TRIGGER to close. Voice uses offline dictation.", guidePanel,
            // 步骤 21：用 new 创建 Vector2(0f, -285f), new Vector2(470f, 32f), 14f, TextAlignmentOptions.Center；构造参数决定新对象的初始内容。
            new Vector2(0f, -285f), new Vector2(470f, 32f), 14f, TextAlignmentOptions.Center);
        // 步骤 22：把计算结果 Panel("Right-trigger guide toggle", canvasRect, 保存为guideTab；下一条判断或调用会读取这个具体值。
        guideTab = Panel("Right-trigger guide toggle", canvasRect,
            // 步骤 23：用 new 创建 Vector2(-545f, 160f), new Vector2(106f, 144f), .94f);；构造参数决定新对象的初始内容。
            new Vector2(-545f, 160f), new Vector2(106f, 144f), .94f);
        // 步骤 24：把计算结果 Label("Guide toggle text", "CLOSE\n◀", guideTab, Vector2.zero, 保存为tabText；下一条判断或调用会读取这个具体值。
        tabText = Label("Guide toggle text", "CLOSE\n◀", guideTab, Vector2.zero,
            // 步骤 25：用 new 创建 Vector2(98f, 130f), 23f, TextAlignmentOptions.Center, true);；构造参数决定新对象的初始内容。
            new Vector2(98f, 130f), 23f, TextAlignmentOptions.Center, true);

        // 步骤 26：把计算结果 Panel("Flight rules panel", canvasRect, Vector2.zero, 保存为flight面板；下一条判断或调用会读取这个具体值。
        flightPanel = Panel("Flight rules panel", canvasRect, Vector2.zero,
            // 步骤 27：用 new 创建 Vector2(570f, 615f), .84f);；构造参数决定新对象的初始内容。
            new Vector2(570f, 615f), .84f);
        // 步骤 28：调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Flight title", "FLIGHT FLAMES  |  HOW TO PLAY", flightPanel,
            // 步骤 29：用 new 创建 Vector2(-17f, 265f), new Vector2(480f, 42f), 23f, TextAlignmentOptions.Cente；构造参数决定新对象的初始内容。
            new Vector2(-17f, 265f), new Vector2(480f, 42f), 23f, TextAlignmentOptions.Center, true);
        // 步骤 30：新建用于flightClose的对象或数值结构，并把构造参数保存进去。
        flightClose = Panel("Close flight rules", flightPanel, new Vector2(257f, 269f),
            // 步骤 31：用 new 创建 Vector2(52f, 48f), .35f);；构造参数决定新对象的初始内容。
            new Vector2(52f, 48f), .35f);
        // 步骤 32：调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Close text", "X", flightClose, Vector2.zero,
            // 步骤 33：用 new 创建 Vector2(48f, 45f), 24f, TextAlignmentOptions.Center, true);；构造参数决定新对象的初始内容。
            new Vector2(48f, 45f), 24f, TextAlignmentOptions.Center, true);
        // 步骤 34：调用 BuildFlightPanelText：构建Flight面板Text。
        BuildFlightPanelText(flightPanel, QuestFlightControls);
        // 步骤 35：调用 RefreshQuestHud：刷新QuestHud。
        RefreshQuestHud();
    }
    // ！！！功能结束：BuildQuestHud！！！

    // 继续填写上一行调用的参数：private static RectTransform Panel(string name, RectTransform parent, Vector2 position；参数位置决定它表示的坐标、速度或选项。
    private static RectTransform Panel(string name, RectTransform parent, Vector2 position,
        // 这一行补充 Vector2 size, float alpha)，与相邻行合起来完成同一次计算或对象配置。
        Vector2 size, float alpha)
    {
        // 创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        // 取目标对象的 Transform 并存为 rect；Transform 提供位置、旋转和父子关系。
        RectTransform rect = (RectTransform)obj.transform;
        // 调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        rect.SetParent(parent, false);
        // 新建用于anchorMin的对象或数值结构，并把构造参数保存进去。
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        // 把计算结果 position 保存为anchored位置；下一条判断或调用会读取这个具体值。
        rect.anchoredPosition = position;
        // 把计算结果 size 保存为sizeDelta；下一条判断或调用会读取这个具体值。
        rect.sizeDelta = size;
        // 从目标物体找到所需组件并存为 image；使用前应检查是否为 null。
        Image image = obj.GetComponent<Image>();
        // 新建用于color的对象或数值结构，并把构造参数保存进去。
        image.color = new Color(0f, 0f, 0f, alpha);
        // 把 image.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        image.raycastTarget = false;
        // 把 rect 作为方法结果返回给调用方；return 后本方法立即结束。
        return rect;
    }

    // 继续填写上一行调用的参数：private static TextMeshProUGUI Label(string name, string value, RectTransform parent；参数位置决定它表示的坐标、速度或选项。
    private static TextMeshProUGUI Label(string name, string value, RectTransform parent,
        // 这一行补充 Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment, bool bold = false)，与相邻行合起来完成同一次计算或对象配置。
        Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment, bool bold = false)
    {
        // 创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        // 取目标对象的 Transform 并存为 rect；Transform 提供位置、旋转和父子关系。
        RectTransform rect = (RectTransform)obj.transform;
        // 调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        rect.SetParent(parent, false);
        // 新建用于anchorMin的对象或数值结构，并把构造参数保存进去。
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        // 把计算结果 position 保存为anchored位置；下一条判断或调用会读取这个具体值。
        rect.anchoredPosition = position;
        // 把计算结果 size 保存为sizeDelta；下一条判断或调用会读取这个具体值。
        rect.sizeDelta = size;
        // 从目标物体找到所需组件并存为 label；使用前应检查是否为 null。
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        // 把新内容写入 label.text；玩家看到的文字会在本帧更新。
        label.text = value;
        // 把计算结果 fontSize 保存为fontSize；下一条判断或调用会读取这个具体值。
        label.fontSize = fontSize;
        // 把计算结果 bold ? FontStyles.Bold : FontStyles.Normal 保存为fontStyle；下一条判断或调用会读取这个具体值。
        label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        // 把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
        label.color = Color.white;
        // 把计算结果 alignment 保存为alignment；下一条判断或调用会读取这个具体值。
        label.alignment = alignment;
        // 把 TextWrappingModes.Normal 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
        label.textWrappingMode = TextWrappingModes.Normal;
        // 把 label.enableAutoSizing 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        label.enableAutoSizing = true;
        // 把计算结果 Mathf.Max(12f, fontSize - 4f) 保存为fontSizeMin；下一条判断或调用会读取这个具体值。
        label.fontSizeMin = Mathf.Max(12f, fontSize - 4f);
        // 把计算结果 fontSize 保存为fontSizeMax；下一条判断或调用会读取这个具体值。
        label.fontSizeMax = fontSize;
        // 把 label.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        label.raycastTarget = false;
        // 把 label 作为方法结果返回给调用方；return 后本方法立即结束。
        return label;
    }

    // 继续填写上一行调用的参数：private static void BuildTable(RectTransform parent, string[,] rows；参数位置决定它表示的坐标、速度或选项。
    private static void BuildTable(RectTransform parent, string[,] rows,
        // 这一行补充 float firstY, float rowHeight, float width, float fontSize)，与相邻行合起来完成同一次计算或对象配置。
        float firstY, float rowHeight, float width, float fontSize)
    {
        // 调用 Panel：面板。
        Panel("Table heading", parent, new Vector2(0f, firstY + rowHeight),
            // 用 new 创建 Vector2(width, rowHeight), .22f);；构造参数决定新对象的初始内容。
            new Vector2(width, rowHeight), .22f);
        // 调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Key heading", "KEY / BUTTON", parent,
            // 用 new 创建 Vector2(-width * .34f, firstY + rowHeight),；构造参数决定新对象的初始内容。
            new Vector2(-width * .34f, firstY + rowHeight),
            // 用 new 创建 Vector2(width * .30f, rowHeight), fontSize, TextAlignmentOptions.MidlineLeft；构造参数决定新对象的初始内容。
            new Vector2(width * .30f, rowHeight), fontSize, TextAlignmentOptions.MidlineLeft, true);
        // 调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Action heading", "ACTION", parent,
            // 用 new 创建 Vector2(width * .18f, firstY + rowHeight),；构造参数决定新对象的初始内容。
            new Vector2(width * .18f, firstY + rowHeight),
            // 用 new 创建 Vector2(width * .62f, rowHeight), fontSize, TextAlignmentOptions.MidlineLeft；构造参数决定新对象的初始内容。
            new Vector2(width * .62f, rowHeight), fontSize, TextAlignmentOptions.MidlineLeft, true);
        // 按 int i = 0; i < rows.GetLength(0); i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < rows.GetLength(0); i++)
        {
            // 计算 firstY - i * rowHeight，把结果存进 y（y）；之后引用 y 时用的就是这次结果。
            float y = firstY - i * rowHeight;
            // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (i % 2 == 0) Panel("Row " + i, parent, new Vector2(0f, y),
                // 用 new 创建 Vector2(width, rowHeight), .13f);；构造参数决定新对象的初始内容。
                new Vector2(width, rowHeight), .13f);
            // 调用 Label：在指定 GUI 矩形区域显示说明文字。
            Label("Key " + i, rows[i, 0], parent,
                // 用 new 创建 Vector2(-width * .34f, y), new Vector2(width * .30f, rowHeight),；构造参数决定新对象的初始内容。
                new Vector2(-width * .34f, y), new Vector2(width * .30f, rowHeight),
                // 执行 fontSize, TextAlignmentOptions.MidlineLeft, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                fontSize, TextAlignmentOptions.MidlineLeft, true);
            // 调用 Label：在指定 GUI 矩形区域显示说明文字。
            Label("Action " + i, rows[i, 1], parent,
                // 用 new 创建 Vector2(width * .18f, y), new Vector2(width * .62f, rowHeight),；构造参数决定新对象的初始内容。
                new Vector2(width * .18f, y), new Vector2(width * .62f, rowHeight),
                // 执行 fontSize, TextAlignmentOptions.MidlineLeft);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                fontSize, TextAlignmentOptions.MidlineLeft);
        }
    }

    // ！！！功能：BuildFlightPanelText——构建Flight面板Text！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 RectTransform panel, string[,] rows 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BuildFlightPanelText(RectTransform panel, string[,] rows)
    {
        // 步骤 1：用三元表达式为 flameCount 二选一：问号前条件为真取前值，为假取冒号后的值。
        int flameCount = course != null && course.flames != null ? course.flames.Length : 12;
        // 步骤 2：调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Flight goal", "Enter the blue ring. Fly through all " + flameCount +
            // 步骤 3：继续填写上一行调用的参数：" flames within 90 seconds. Touch each flame to collect it.", panel；参数位置决定它表示的坐标、速度或选项。
            " flames within 90 seconds. Touch each flame to collect it.", panel,
            // 步骤 4：用 new 创建 Vector2(0f, 209f), new Vector2(520f, 55f), 19f, TextAlignmentOptions.Center)；构造参数决定新对象的初始内容。
            new Vector2(0f, 209f), new Vector2(520f, 55f), 19f, TextAlignmentOptions.Center);
        // 步骤 5：调用 BuildTable：构建Table。
        BuildTable(panel, rows, 101f, 43f, 520f, 18f);
        // 步骤 6：调用 Label：在指定 GUI 矩形区域显示说明文字。
        Label("Flight score", "+10 points per flame. Finish bonus: 2 points per second left.\n" +
            // 步骤 7：继续填写上一行调用的参数："Your cape refills at the start and end of each round.", panel；参数位置决定它表示的坐标、速度或选项。
            "Your cape refills at the start and end of each round.", panel,
            // 步骤 8：用 new 创建 Vector2(0f, -249f), new Vector2(520f, 69f), 17f, TextAlignmentOptions.Center；构造参数决定新对象的初始内容。
            new Vector2(0f, -249f), new Vector2(520f, 69f), 17f, TextAlignmentOptions.Center);
    }
    // ！！！功能结束：BuildFlightPanelText！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnGUI()
    {
        // 步骤 1：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!desktopMode) return;
        // 步骤 2：调用 EnsureStyles：确保Styles。
        EnsureStyles();
        // 步骤 3：计算 Mathf.Min(550f, Screen.width * .31f)，把结果存进 width（width）；之后引用 width 时用的就是这次结果。
        float width = Mathf.Min(550f, Screen.width * .31f);
        // 步骤 4：计算 Mathf.Min(800f, Screen.height * .78f)，把结果存进 height（height）；之后引用 height 时用的就是这次结果。
        float height = Mathf.Min(800f, Screen.height * .78f);
        // 步骤 5：把数值 132f 暂存为 top；它只在当前方法里参与后续判断或计算。
        float top = 132f;
        // 步骤 6：检查条件 flightOpen；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 DrawDesktopFlight：Draw桌面Flight。
        if (flightOpen) DrawDesktopFlight();
        // 步骤 7：这里的 ! 表示取反；当 guideOpen 不成立时才进入此分支。
        if (!guideOpen)
        {
            // 步骤 8：创建新的 Rect 并存为 tab；构造参数决定其初始内容或对象名称。
            Rect tab = new Rect(0f, top, 102f, 39f);
            // 步骤 9：调用 Fill：Fill。
            Fill(tab, .85f);
            // 步骤 10：检查条件 GUI.Button(tab, "GUIDE  ▶", buttonStyle)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 guideOpen = true;
            if (GUI.Button(tab, "GUIDE  ▶", buttonStyle)) guideOpen = true;
            // 步骤 11：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 12：创建新的 Rect 并存为 panel；构造参数决定其初始内容或对象名称。
        Rect panel = new Rect(0f, top, width, height);
        // 步骤 13：调用 Fill：Fill。
        Fill(panel, .80f);
        // 步骤 14：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(20f, top + 12f, width - 118f, 36f),
            // 步骤 15：执行 "DESKTOP CONTROLS", headingStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "DESKTOP CONTROLS", headingStyle);
        // 步骤 16：检查条件 GUI.Button(new Rect(width - 54f, top + 10f, 44f, 37f), "×", buttonStyle)；只有为真才运行下面的步骤，否则沿原流程继续。
        if (GUI.Button(new Rect(width - 54f, top + 10f, 44f, 37f), "×", buttonStyle))
            // 步骤 17：把 guideOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            guideOpen = false;
        // 步骤 18：调用 DrawDesktopTable：Draw桌面Table。
        DrawDesktopTable(DesktopControls, new Rect(13f, top + 58f, width - 26f, height - 70f), ref guideScroll);
    }
    // ！！！功能结束：OnGUI！！！

    // ！！！功能：DrawDesktopFlight——Draw桌面Flight！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawDesktopFlight()
    {
        // 步骤 1：计算 Mathf.Min(630f, Screen.width * .48f)，把结果存进 width（width）；之后引用 width 时用的就是这次结果。
        float width = Mathf.Min(630f, Screen.width * .48f);
        // 步骤 2：计算 Mathf.Min(610f, Screen.height * .76f)，把结果存进 height（height）；之后引用 height 时用的就是这次结果。
        float height = Mathf.Min(610f, Screen.height * .76f);
        // 步骤 3：创建新的 Rect 并存为 panel；构造参数决定其初始内容或对象名称。
        Rect panel = new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, width, height);
        // 步骤 4：调用 Fill：Fill。
        Fill(panel, .85f);
        // 步骤 5：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 20f, panel.y + 13f, width - 104f, 37f),
            // 步骤 6：执行 "FLIGHT FLAMES  |  HOW TO PLAY", headingStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "FLIGHT FLAMES  |  HOW TO PLAY", headingStyle);
        // 步骤 7：检查条件 GUI.Button(new Rect(panel.xMax - 85f, panel.y + 12f, 70f, 36f), "CLOSE", buttonStyle)；只有为真才运行下面的步骤，否则沿原流程继续。
        if (GUI.Button(new Rect(panel.xMax - 85f, panel.y + 12f, 70f, 36f), "CLOSE", buttonStyle))
            // 步骤 8：调用 CloseFlightRules：关闭飞行玩法说明并恢复原有视野。
            CloseFlightRules();
        // 步骤 9：用三元表达式为 flameCount 二选一：问号前条件为真取前值，为假取冒号后的值。
        int flameCount = course != null && course.flames != null ? course.flames.Length : 12;
        // 步骤 10：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 20f, panel.y + 56f, width - 40f, 54f),
            // 步骤 11：这一行补充 "Enter the blue ring. Fly through all " + flameCount +，与相邻行合起来完成同一次计算或对象配置。
            "Enter the blue ring. Fly through all " + flameCount +
            // 步骤 12：执行 " flames in 90 seconds. Touch each flame to collect it.", cellStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            " flames in 90 seconds. Touch each flame to collect it.", cellStyle);
        // 步骤 13：调用 DrawDesktopTable：Draw桌面Table。
        DrawDesktopTable(DesktopFlightControls,
            // 步骤 14：用 new 创建 Rect(panel.x + 15f, panel.y + 116f, width - 30f, height - 188f), ref flightS；构造参数决定新对象的初始内容。
            new Rect(panel.x + 15f, panel.y + 116f, width - 30f, height - 188f), ref flightScroll);
        // 步骤 15：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 22f, panel.yMax - 67f, width - 44f, 56f),
            // 步骤 16：这一行补充 "+10 points per flame. Finish bonus: 2 points per second left.\n" +，与相邻行合起来完成同一次计算或对象配置。
            "+10 points per flame. Finish bonus: 2 points per second left.\n" +
            // 步骤 17：执行 "Cape energy refills when the round starts or ends.", cellStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Cape energy refills when the round starts or ends.", cellStyle);
    }
    // ！！！功能结束：DrawDesktopFlight！！！

    // ！！！功能：DrawDesktopTable——Draw桌面Table！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string[,] rows, Rect viewport, ref Vector2 scroll 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawDesktopTable(string[,] rows, Rect viewport, ref Vector2 scroll)
    {
        // 步骤 1：定义不可在运行时改变的数值 rowHeight（rowHeight）；后续计算统一使用这个阈值、速度或尺寸。
        const float rowHeight = 39f;
        // 步骤 2：创建新的 Rect 并存为 content；构造参数决定其初始内容或对象名称。
        Rect content = new Rect(0f, 0f, viewport.width - 18f, (rows.GetLength(0) + 1) * rowHeight);
        // 步骤 3：把计算结果 GUI.BeginScrollView(viewport, scroll, content) 保存为scroll；下一条判断或调用会读取这个具体值。
        scroll = GUI.BeginScrollView(viewport, scroll, content);
        // 步骤 4：计算 content.width * .32f，把结果存进 keyWidth（keyWidth）；之后引用 keyWidth 时用的就是这次结果。
        float keyWidth = content.width * .32f;
        // 步骤 5：按 int i = -1; i < rows.GetLength(0); i++ 变化索引；循环体依次处理每个位置。
        for (int i = -1; i < rows.GetLength(0); i++)
        {
            // 步骤 6：计算 (i + 1) * rowHeight，把结果存进 y（y）；之后引用 y 时用的就是这次结果。
            float y = (i + 1) * rowHeight;
            // 步骤 7：这里用 || 表示任一条件成立就处理：i == -1 || i % 2 == 0；用于合并几种停止或跳过原因。 成立时调用 Fill：Fill。
            if (i == -1 || i % 2 == 0) Fill(new Rect(0f, y, content.width, rowHeight), i == -1 ? .30f : .16f);
            // 步骤 8：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(new Rect(9f, y + 3f, keyWidth - 12f, rowHeight - 6f),
                // 步骤 9：把计算结果 = -1 ? "KEY / BUTTON" : rows[i, 0], keyStyle) 保存为i；下一条判断或调用会读取这个具体值。
                i == -1 ? "KEY / BUTTON" : rows[i, 0], keyStyle);
            // 步骤 10：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(new Rect(keyWidth + 8f, y + 3f, content.width - keyWidth - 16f, rowHeight - 6f),
                // 步骤 11：把计算结果 = -1 ? "ACTION" : rows[i, 1], cellStyle) 保存为i；下一条判断或调用会读取这个具体值。
                i == -1 ? "ACTION" : rows[i, 1], cellStyle);
        }
        // 步骤 12：调用 EndScrollView：EndScrollView。
        GUI.EndScrollView();
    }
    // ！！！功能结束：DrawDesktopTable！！！

    // ！！！功能：IsOverDesktopPanel——IsOver桌面面板！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector2 mousePoint 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private bool IsOverDesktopPanel(Vector2 mousePoint)
    {
        // Input System coordinates start at the bottom; IMGUI coordinates start at the top.
        // 步骤 1：创建新的 Vector2 并存为 guiPoint；构造参数决定其初始内容或对象名称。
        Vector2 guiPoint = new Vector2(mousePoint.x, Screen.height - mousePoint.y);
        // 步骤 2：计算 Mathf.Min(550f, Screen.width * .31f)，把结果存进 guideWidth（guideWidth）；之后引用 guideWidth 时用的就是这次结果。
        float guideWidth = Mathf.Min(550f, Screen.width * .31f);
        // 步骤 3：计算 Mathf.Min(800f, Screen.height * .78f)，把结果存进 guideHeight（guideHeight）；之后引用 guideHeight 时用的就是这次结果。
        float guideHeight = Mathf.Min(800f, Screen.height * .78f);
        // 步骤 4：这里用 && 要求两边条件同时成立：guideOpen && new Rect(0f, 132f, guideWidth, guideHeight).Contains(guiPoint)；任意一边为假就跳过本分支。 条件成立时立即退出当前方法。
        if (guideOpen && new Rect(0f, 132f, guideWidth, guideHeight).Contains(guiPoint)) return true;
        // 步骤 5：这里用 && 要求两边条件同时成立：!guideOpen && new Rect(0f, 132f, 102f, 39f).Contains(guiPoint)；任意一边为假就跳过本分支。 条件成立时立即退出当前方法。
        if (!guideOpen && new Rect(0f, 132f, 102f, 39f).Contains(guiPoint)) return true;
        // 步骤 6：这里的 ! 表示取反；当 flightOpen 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!flightOpen) return false;
        // 步骤 7：计算 Mathf.Min(630f, Screen.width * .48f)，把结果存进 width（width）；之后引用 width 时用的就是这次结果。
        float width = Mathf.Min(630f, Screen.width * .48f);
        // 步骤 8：计算 Mathf.Min(610f, Screen.height * .76f)，把结果存进 height（height）；之后引用 height 时用的就是这次结果。
        float height = Mathf.Min(610f, Screen.height * .76f);
        // 步骤 9：把 new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, 作为方法结果返回给调用方；return 后本方法立即结束。
        return new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f,
            // 步骤 10：执行 width, height).Contains(guiPoint);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            width, height).Contains(guiPoint);
    }
    // ！！！功能结束：IsOverDesktopPanel！！！

    // ！！！功能：EnsureStyles——确保Styles！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void EnsureStyles()
    {
        // 步骤 1：确认headingStyle已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (headingStyle != null) return;
        // 步骤 2：新建用于headingStyle的对象或数值结构，并把构造参数保存进去。
        headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 21, fontStyle = FontStyle.Bold,
            // 步骤 3：把计算结果 TextAnchor.MiddleLeft, wordWrap = true } 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleLeft, wordWrap = true };
        // 步骤 4：执行 headingStyle.normal.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        headingStyle.normal.textColor = Color.white;
        // 步骤 5：新建用于cellStyle的对象或数值结构，并把构造参数保存进去。
        cellStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleLeft,
            // 步骤 6：把计算结果 true } 保存为wordWrap；下一条判断或调用会读取这个具体值。
            wordWrap = true };
        // 步骤 7：执行 cellStyle.normal.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        cellStyle.normal.textColor = Color.white;
        // 步骤 8：新建用于keyStyle的对象或数值结构，并把构造参数保存进去。
        keyStyle = new GUIStyle(cellStyle) { fontStyle = FontStyle.Bold };
        // 步骤 9：执行 keyStyle.normal.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        keyStyle.normal.textColor = Color.white;
        // 步骤 10：新建用于buttonStyle的对象或数值结构，并把构造参数保存进去。
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold,
            // 步骤 11：把计算结果 TextAnchor.MiddleCenter } 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleCenter };
        // 步骤 12：执行 buttonStyle.normal.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        buttonStyle.normal.textColor = Color.white;
        // 步骤 13：执行 buttonStyle.hover.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        buttonStyle.hover.textColor = Color.white;
        // 步骤 14：执行 buttonStyle.active.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        buttonStyle.active.textColor = Color.white;
        // 步骤 15：执行 buttonStyle.normal.background = null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        buttonStyle.normal.background = null;
        // 步骤 16：执行 buttonStyle.hover.background = null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        buttonStyle.hover.background = null;
        // 步骤 17：执行 buttonStyle.active.background = null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        buttonStyle.active.background = null;
    }
    // ！！！功能结束：EnsureStyles！！！

    // ！！！功能：Fill——Fill！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Rect rect, float alpha 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Fill(Rect rect, float alpha)
    {
        // 步骤 1：计算 GUI.color，把结果存进 before（before）；之后引用 before 时用的就是这次结果。
        Color before = GUI.color;
        // 步骤 2：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = new Color(0f, 0f, 0f, alpha);
        // 步骤 3：调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        // 步骤 4：把计算结果 before 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = before;
    }
    // ！！！功能结束：Fill！！！
}
