// ！！！文件功能：旧版先祖交互：显示记忆标记、兑换树和手中物品！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System.Collections 命名空间；下方可直接使用该库提供的类型和方法。
using System.Collections;
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 定义 SkyQuestAncestor 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestAncestor : MonoBehaviour
{
    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Tracked devices")]
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 leftHand（左手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform leftHand;
    // 声明 rightHand（右手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform rightHand;
    // 声明 ancestor（ancestor）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform ancestor;
    // 声明 memorialStone（memorialStone）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform memorialStone;
    // 声明 avatar（角色模型）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform avatar;
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    public SkyQuestLocomotion locomotion;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    public SkyQuestStatus status;
    // magicExchange 保存magicExchange（类型 SkyQuestMagicExchange）；下方方法依靠它记录或查找场景状态。
    public SkyQuestMagicExchange magicExchange;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Interaction")]
    // 把 Inspector 可调整的 proximity（proximity）设为默认值 4f；具体用途看它在下方的判断和计算。
    public float proximity = 4f;
    // 把 Inspector 可调整的 summonRadius（summonRadius）设为默认值 6f；具体用途看它在下方的判断和计算。
    public float summonRadius = 6f;
    // 把 Inspector 可调整的 dismissRadius（dismissRadius）设为默认值 8f；具体用途看它在下方的判断和计算。
    public float dismissRadius = 8f;
    // 把 Inspector 可调整的 holdDuration（holdDuration）设为默认值 0.55f；具体用途看它在下方的判断和计算。
    public float holdDuration = 0.55f;
    // 声明 maskPrefab（mask预制体）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject maskPrefab;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;

    // 用三元表达式为 IsMenuOpen 二选一：问号前条件为真取前值，为假取冒号后的值。
    public bool IsMenuOpen => desktopMode ? desktopMenuOpen : menuRoot != null && menuRoot.activeSelf;
    // 这是只读计算属性；箭头右侧条件每次读取时重新计算，不需要另存一个布尔字段。
    public bool IsNearAncestor => summoned && head != null && ancestor != null &&
        // 调用 Distance：Distance。
        Vector3.Distance(head.position, ancestor.position + Vector3.up * .8f) <= proximity;

    // 计算 { "Cape Energy I", "Cape Energy II", "Cape Energy III", "Cape Energy IV", "Cape Energy V" ，把结果存进 names（names）；之后引用 names 时用的就是这次结果。
    private readonly string[] names = { "Cape Energy I", "Cape Energy II", "Cape Energy III", "Cape Energy IV", "Cape Energy V" };
    // 计算 { "◆", "◆", "◆", "◆", "◆" }，把结果存进 glyphs（glyphs）；之后引用 glyphs 时用的就是这次结果。
    private readonly string[] glyphs = { "◆", "◆", "◆", "◆", "◆" };
    // 这一行补充 private readonly Color[] energyColors = {，与相邻行合起来完成同一次计算或对象配置。
    private readonly Color[] energyColors = {
        // 用 new 创建 Color(1f, .78f, .32f), new Color(.55f, .88f, 1f),；构造参数决定新对象的初始内容。
        new Color(1f, .78f, .32f), new Color(.55f, .88f, 1f),
        // 用 new 创建 Color(.85f, .52f, 1f), new Color(.55f, 1f, .72f),；构造参数决定新对象的初始内容。
        new Color(.85f, .52f, 1f), new Color(.55f, 1f, .72f),
        // 用 new 创建 Color(1f, .55f, .72f)；构造参数决定新对象的初始内容。
        new Color(1f, .55f, .72f)
    };
    // 这一行补充 private readonly Vector2[] positions =，与相邻行合起来完成同一次计算或对象配置。
    private readonly Vector2[] positions =
    {
        // 用 new 创建 Vector2(0f, 0.72f), new Vector2(-0.72f, 0.05f), new Vector2(0.72f, 0.05f),；构造参数决定新对象的初始内容。
        new Vector2(0f, 0.72f), new Vector2(-0.72f, 0.05f), new Vector2(0.72f, 0.05f),
        // 用 new 创建 Vector2(-0.5f, -0.68f), new Vector2(0.5f, -0.68f)；构造参数决定新对象的初始内容。
        new Vector2(-0.5f, -0.68f), new Vector2(0.5f, -0.68f)
    };
    // 创建新的 bool[] 并存为 obtained；构造参数决定其初始内容或对象名称。
    private readonly bool[] obtained = new bool[5];
    // 创建新的 bool[] 并存为 equipped；构造参数决定其初始内容或对象名称。
    private readonly bool[] equipped = new bool[5];
    // 声明 menuRoot（menuRoot）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject menuRoot;
    // nodes 保存nodes（类型 Transform[]）；下方方法依靠它记录或查找场景状态。
    private Transform[] nodes;
    // labels 保存labels（类型 TextMeshPro[]）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro[] labels;
    // 声明 ray（ray）：可见射线组件引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private LineRenderer ray;
    // 声明 heldVisual（heldVisual）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject heldVisual;
    // 把数值 -1 暂存为 selected；它只在当前方法里参与后续判断或计算。
    private int selected = -1;
    // 把数值 -1 暂存为 heldIndex；它只在当前方法里参与后续判断或计算。
    private int heldIndex = -1;
    // 把数值 -1 暂存为 focused；它只在当前方法里参与后续判断或计算。
    private int focused = -1;
    // 声明 pressStart（pressStart）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float pressStart;
    // 声明 controllerDragStart（controllerDragStart）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 controllerDragStart;
    // 把数值 -1f 暂存为 overheadStart；它只在当前方法里参与后续判断或计算。
    private float overheadStart = -1f;
    // 声明 nextToggle（nextToggle）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextToggle;
    // 声明 wasTrigger（wasTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool wasTrigger;
    // 声明 wasMenuButton（wasMenuButton）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool wasMenuButton;
    // 声明 wasEquipButton（wasEquipButton）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool wasEquipButton;
    // 声明 overheadConsumed（overheadConsumed）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool overheadConsumed;
    // moveToHandRoutine 保存move到HandRoutine（类型 Coroutine）；下方方法依靠它记录或查找场景状态。
    private Coroutine moveToHandRoutine;
    // spiritRenderers 保存spiritRenderers（类型 Renderer[]）；下方方法依靠它记录或查找场景状态。
    private Renderer[] spiritRenderers;
    // spiritInitiallyVisible 保存spiritInitiallyVisible（类型 bool[]）；下方方法依靠它记录或查找场景状态。
    private bool[] spiritInitiallyVisible;
    // 声明 memorial（memorial）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform memorial;
    // 声明 iconRoot（iconRoot）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject iconRoot;
    // 声明 memorialIconAnchor（memorialIconAnchor）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform memorialIconAnchor;
    // 声明 summoned（summoned）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool summoned;
    // 声明 insideSummonRadius（insideSummonRadius）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool insideSummonRadius;
    // 声明 desktopMenuOpen（desktopMenuOpen）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool desktopMenuOpen;
    // emergence 保存emergence（类型 SkyQuestAncestorEmergence）；下方方法依靠它记录或查找场景状态。
    private SkyQuestAncestorEmergence emergence;
    // hoverHelp 保存hoverHelp（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro hoverHelp;
    // 声明 hoverHelpPlate（hoverHelpPlate）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject hoverHelpPlate;
    // 把数值 -1 暂存为 guiDragIndex；它只在当前方法里参与后续判断或计算。
    private int guiDragIndex = -1;
    // 声明 guiDragStart（guiDragStart）：二维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector2 guiDragStart;
    // 声明 guiDragTriggered（guiDragTriggered）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool guiDragTriggered;
    // 把数值 -1 暂存为 hoveredNode；它只在当前方法里参与后续判断或计算。
    private int hoveredNode = -1;
    // 声明 hoveredSince（hoveredSince）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float hoveredSince;
    // 把数值 -1 暂存为 focusedBefore；它只在当前方法里参与后续判断或计算。
    private int focusedBefore = -1;
    // 声明 focusedSince（focusedSince）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float focusedSince;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：检查ancestor、头显相机、右手柄位置是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (ancestor == null || head == null || rightHand == null) return;
        // 步骤 2：从目标 GameObject 查找并保存 spiritRenderers 组件；后面的移动或显示逻辑会直接使用这个缓存。
        spiritRenderers = ancestor.GetComponentsInChildren<Renderer>(true);
        // 步骤 3：新建用于spiritInitiallyVisible的对象或数值结构，并把构造参数保存进去。
        spiritInitiallyVisible = new bool[spiritRenderers.Length];
        // 步骤 4：按 int i = 0; i < spiritRenderers.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < spiritRenderers.Length; i++)
            // 步骤 5：执行 spiritInitiallyVisible[i] = spiritRenderers[i].enabled;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            spiritInitiallyVisible[i] = spiritRenderers[i].enabled;
        // 步骤 6：调用 MakeMemorial：MakeMemorial。
        MakeMemorial();
        // 步骤 7：从目标 GameObject 查找并保存 emergence 组件；后面的移动或显示逻辑会直接使用这个缓存。
        emergence = ancestor.GetComponent<SkyQuestAncestorEmergence>();
        // 步骤 8：如果场景对象没有 SkyQuestAncestorEmergence 组件，就当场创建并保存，保证后续调用有对象可用。
        if (emergence == null) emergence = ancestor.gameObject.AddComponent<SkyQuestAncestorEmergence>();
        // 步骤 9：把括号里的依赖对象交给 emergence 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
        emergence.Initialize(ancestor, memorial);
        // 步骤 10：调用 MakeMarker：MakeMarker。
        MakeMarker();
        // 步骤 11：调用 MakeMenu：MakeMenu。
        MakeMenu();
        // 步骤 12：调用 MakeRay：Make射线。
        MakeRay();
        // 步骤 13：调用 SetSummoned：设置Summoned。
        SetSummoned(false);
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：检查ancestor、头显相机、右手柄位置、左手柄位置是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (ancestor == null || head == null || rightHand == null || leftHand == null) return;
        // 步骤 2：调用 UpdateSummon：更新Summon。
        UpdateSummon();
        // 步骤 3：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 4：调用 UpdateIcon：更新Icon。
            UpdateIcon();
            // 步骤 5：这里用 && 要求两边条件同时成立：IsMenuOpen && (!summoned || !IsNearAncestor)；任意一边为假就跳过本分支。 成立时调用 CloseMenu：关闭兑换树并恢复正常角色控制。
            if (IsMenuOpen && (!summoned || !IsNearAncestor)) CloseMenu();
            // 步骤 6：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 7：调用 UpdateIcon：更新Icon。
        UpdateIcon();
        // 步骤 8：用 menuButton 记住此处真假结果（SkyQuestInput.Primary(SkyQuestInput.Left)），供下一条 if 选择执行路径。
        bool menuButton = SkyQuestInput.Primary(SkyQuestInput.Left);
        // 步骤 9：把计算结果 IsNearAncestor && 保存为overhead；下一条判断或调用会读取这个具体值。
        bool overhead = IsNearAncestor &&
            // 步骤 10：这一行补充 leftHand.position.y > head.position.y + 0.04f &&，与相邻行合起来完成同一次计算或对象配置。
            leftHand.position.y > head.position.y + 0.04f &&
            // 步骤 11：这一行补充 rightHand.position.y > head.position.y + 0.04f &&，与相邻行合起来完成同一次计算或对象配置。
            rightHand.position.y > head.position.y + 0.04f &&
            // 步骤 12：调用 Grip：Grip。
            SkyQuestInput.Grip(SkyQuestInput.Left) > 0.65f &&
            // 步骤 13：调用 Grip：Grip。
            SkyQuestInput.Grip(SkyQuestInput.Right) > 0.65f;
        // 步骤 14：检查条件 overhead；只有为真才运行下面的步骤，否则沿原流程继续。
        if (overhead)
        {
            // 步骤 15：检查条件 overheadStart < 0f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 overheadStart = Time.time;
            if (overheadStart < 0f) overheadStart = Time.time;
            // 步骤 16：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
            if (!overheadConsumed && Time.time - overheadStart > 0.4f && Time.time > nextToggle)
            {
                // 步骤 17：调用 ToggleMenu：ToggleMenu。
                ToggleMenu();
                // 步骤 18：把 overheadConsumed 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                overheadConsumed = true;
            }
        }
        // 步骤 19：前面的 if 条件为假时执行这里的备用处理。
        else { overheadStart = -1f; overheadConsumed = false; }
        // 步骤 20：把计算结果 magicExchange == null || !magicExchange.IsNearAny || 保存为legacyRequested；下一条判断或调用会读取这个具体值。
        bool legacyRequested = magicExchange == null || !magicExchange.IsNearAny ||
            // 步骤 21：调用 Grip：Grip。
            SkyQuestInput.Grip(SkyQuestInput.Left) > 0.65f;
        // 步骤 22：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (menuButton && !wasMenuButton && IsNearAncestor && legacyRequested &&
            // 步骤 23：执行 Time.time > nextToggle) ToggleMenu();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Time.time > nextToggle) ToggleMenu();
        // 步骤 24：把计算结果 menuButton 保存为wasMenuButton；下一条判断或调用会读取这个具体值。
        wasMenuButton = menuButton;

        // 步骤 25：用 equip 记住此处真假结果（SkyQuestInput.Grip(SkyQuestInput.Right) > 0.75f），供下一条 if 选择执行路径。
        bool equip = SkyQuestInput.Grip(SkyQuestInput.Right) > 0.75f;
        // 步骤 26：这里用 && 要求两边条件同时成立：equip && !wasEquipButton；任意一边为假就跳过本分支。 成立时调用 EquipHeld：EquipHeld。
        if (equip && !wasEquipButton) EquipHeld();
        // 步骤 27：把计算结果 equip 保存为wasEquipButton；下一条判断或调用会读取这个具体值。
        wasEquipButton = equip;
        // 步骤 28：这里的 ! 表示取反；当 IsMenuOpen 不成立时才进入此分支。
        if (!IsMenuOpen)
        {
            // 步骤 29：确认ray已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 ray.enabled = false;
            if (ray != null) ray.enabled = false;
            // 步骤 30：用 iconTrigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.65f），供下一条 if 选择执行路径。
            bool iconTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.65f;
            // 步骤 31：这里用 && 要求两边条件同时成立：iconTrigger && !wasTrigger && IsNearAncestor && PointingAtIcon()；任意一边为假就跳过本分支。 成立时调用 OpenMenu：打开先祖兑换树并解锁相关鼠标或手柄输入。
            if (iconTrigger && !wasTrigger && IsNearAncestor && PointingAtIcon()) OpenMenu();
            // 步骤 32：把计算结果 iconTrigger 保存为wasTrigger；下一条判断或调用会读取这个具体值。
            wasTrigger = iconTrigger;
            // 步骤 33：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 34：这里的 ! 表示取反；当 IsNearAncestor 不成立时才进入此分支。 成立时执行 { CloseMenu(); return; }
        if (!IsNearAncestor) { CloseMenu(); return; }

        // 步骤 35：调用 UpdateRay：更新射线。
        UpdateRay();
        // 步骤 36：检查条件 focused != focusedBefore；只有为真才运行下面的步骤，否则沿原流程继续。
        if (focused != focusedBefore)
        {
            // 步骤 37：把计算结果 focused 保存为focusedBefore；下一条判断或调用会读取这个具体值。
            focusedBefore = focused;
            // 步骤 38：把计算结果 Time.unscaledTime 保存为focusedSince；下一条判断或调用会读取这个具体值。
            focusedSince = Time.unscaledTime;
        }
        // 步骤 39：确认hoverHelp已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (hoverHelp != null)
            // 步骤 40：把新内容写入 hoverHelp.text；玩家看到的文字会在本帧更新。
            hoverHelp.text = focused >= 0 ?
                // 步骤 41：执行 "HOLD RIGHT TRIGGER: AIM; RELEASE: THROW\nNEAR FACE + RIGHT TRIGGER: EAT +3 ENERGY" : "";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "HOLD RIGHT TRIGGER: AIM; RELEASE: THROW\nNEAR FACE + RIGHT TRIGGER: EAT +3 ENERGY" : "";
        // 步骤 42：确认hoverHelpPlate已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (hoverHelpPlate != null) hoverHelpPlate.SetActive(focused >= 0);
        // 步骤 43：用 trigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.65f），供下一条 if 选择执行路径。
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.65f;
        // 步骤 44：这里用 && 要求两边条件同时成立：trigger && !wasTrigger && focused >= 0 && !obtained[focused]；任意一边为假就跳过本分支。
        if (trigger && !wasTrigger && focused >= 0 && !obtained[focused])
        {
            // 步骤 45：把计算结果 focused 保存为selected；下一条判断或调用会读取这个具体值。
            selected = focused;
            // 步骤 46：根据当前游戏时间计算并保存pressStart，用于后续延迟或冷却判断。
            pressStart = Time.time;
            // 步骤 47：把计算结果 rightHand.position 保存为controllerDragStart；下一条判断或调用会读取这个具体值。
            controllerDragStart = rightHand.position;
            // 步骤 48：执行 status?.Show(names[selected] + " selected. Keep holding to obtain.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show(names[selected] + " selected. Keep holding to obtain.");
        }
        // 步骤 49：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (trigger && selected >= 0 && !obtained[selected] &&
            // 步骤 50：这一行补充 ((focused == selected && Time.time - pressStart >= holdDuration) ||，与相邻行合起来完成同一次计算或对象配置。
            ((focused == selected && Time.time - pressStart >= holdDuration) ||
             // 步骤 51：调用 Distance：Distance。
             Vector3.Distance(rightHand.position, controllerDragStart) >= .14f))
        {
            // 步骤 52：调用 Obtain：兑换成功后创建奖励物并登记拥有状态。
            Obtain(selected);
            // 步骤 53：把计算结果 -1 保存为selected；下一条判断或调用会读取这个具体值。
            selected = -1;
        }
        // 步骤 54：这里的 ! 表示取反；当 trigger 不成立时才进入此分支。 成立时执行 selected = -1;
        if (!trigger) selected = -1;
        // 步骤 55：把计算结果 trigger 保存为wasTrigger；下一条判断或调用会读取这个具体值。
        wasTrigger = trigger;
        // 步骤 56：调用 RefreshNodes：刷新Nodes。
        RefreshNodes();
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnGUI()
    {
        // 步骤 1：检查头显相机、ancestor是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!desktopMode || head == null || ancestor == null) return;
        // 步骤 2：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (summoned && IsNearAncestor && !IsMenuOpen &&
            // 步骤 3：这一行补充 (magicExchange == null || !magicExchange.IsMenuOpen))，与相邻行合起来完成同一次计算或对象配置。
            (magicExchange == null || !magicExchange.IsMenuOpen))
        {
            // 步骤 4：从目标物体找到所需组件并存为 camera；使用前应检查是否为 null。
            Camera camera = head.GetComponent<Camera>();
            // 步骤 5：确认camera已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (camera != null)
            {
                // 步骤 6：计算 InteractionIconPosition()，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
                Vector3 target = InteractionIconPosition();
                // 步骤 7：计算 camera.WorldToScreenPoint(target)，把结果存进 screen（screen）；之后引用 screen 时用的就是这次结果。
                Vector3 screen = camera.WorldToScreenPoint(target);
                // 步骤 8：检查条件 screen.z > 0f；只有为真才运行下面的步骤，否则沿原流程继续。
                if (screen.z > 0f)
                {
                    // 步骤 9：计算 x 并限制在安全上下界，防止不合理的大小影响移动或显示。
                    float x = Mathf.Clamp(screen.x - 28f, 10f, Screen.width - 66f);
                    // 步骤 10：计算 y 并限制在安全上下界，防止不合理的大小影响移动或显示。
                    float y = Mathf.Clamp(Screen.height - screen.y - 28f, 10f, Screen.height - 66f);
                    // 步骤 11：计算 GUI.color，把结果存进 old（old）；之后引用 old 时用的就是这次结果。
                    Color old = GUI.color;
                    // 步骤 12：新建用于color的对象或数值结构，并把构造参数保存进去。
                    GUI.color = new Color(0.1f, 0.15f, 0.19f, 0.9f);
                    // 步骤 13：调用 DrawTexture：Draw贴图。
                    GUI.DrawTexture(new Rect(x, y, 56f, 56f), Texture2D.whiteTexture);
                    // 步骤 14：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
                    GUI.color = Color.white;
                    // 步骤 15：创建新的 GUIStyle 并存为 iconStyle；构造参数决定其初始内容或对象名称。
                    GUIStyle iconStyle = new GUIStyle(GUI.skin.button);
                    // 步骤 16：把计算结果 32 保存为fontSize；下一条判断或调用会读取这个具体值。
                    iconStyle.fontSize = 32;
                    // 步骤 17：把计算结果 FontStyle.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
                    iconStyle.fontStyle = FontStyle.Bold;
                    // 步骤 18：检查条件 GUI.Button(new Rect(x, y, 56f, 56f), "✦", iconStyle)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 OpenMenu：打开先祖兑换树并解锁相关鼠标或手柄输入。
                    if (GUI.Button(new Rect(x, y, 56f, 56f), "✦", iconStyle)) OpenMenu();
                    // 步骤 19：把计算结果 old 保存为color；下一条判断或调用会读取这个具体值。
                    GUI.color = old;
                }
            }
        }
        // 步骤 20：这里的 ! 表示取反；当 IsMenuOpen 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!IsMenuOpen) return;

        // 步骤 21：计算 width 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float width = Mathf.Clamp(Screen.width * 0.36f, 340f, 560f);
        // 步骤 22：计算 Screen.width - width，把结果存进 left（left）；之后引用 left 时用的就是这次结果。
        float left = Screen.width - width;
        // 步骤 23：把数值 108f 暂存为 top；它只在当前方法里参与后续判断或计算。
        float top = 108f;
        // 步骤 24：计算 Screen.height - top，把结果存进 height（height）；之后引用 height 时用的就是这次结果。
        float height = Screen.height - top;
        // 步骤 25：计算 GUI.color，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        Color previous = GUI.color;
        // 步骤 26：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = new Color(0.06f, 0.13f, 0.2f, 0.85f);
        // 步骤 27：调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(new Rect(left, top, width, height), Texture2D.whiteTexture);
        // 步骤 28：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = Color.white;
        // 步骤 29：新建用于heading的对象或数值结构，并把构造参数保存进去。
        GUIStyle heading = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold,
            // 步骤 30：把计算结果 TextAnchor.MiddleCenter } 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleCenter };
        // 步骤 31：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(left + 20f, top + 15f, width - 80f, 38f), "ANCESTOR EXCHANGE", heading);
        // 步骤 32：检查条件 GUI.Button(new Rect(Screen.width - 52f, top + 16f, 36f, 36f), "X")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 CloseMenu：关闭兑换树并恢复正常角色控制。
        if (GUI.Button(new Rect(Screen.width - 52f, top + 16f, 36f, 36f), "X")) CloseMenu();

        // 步骤 33：计算 { 0.50f, 0.25f, 0.75f, 0.34f, 0.66f }，把结果存进 px（px）；之后引用 px 时用的就是这次结果。
        float[] px = { 0.50f, 0.25f, 0.75f, 0.34f, 0.66f };
        // 步骤 34：计算 { 0.08f, 0.25f, 0.43f, 0.61f, 0.77f }，把结果存进 py（py）；之后引用 py 时用的就是这次结果。
        float[] py = { 0.08f, 0.25f, 0.43f, 0.61f, 0.77f };
        // 步骤 35：计算 top + 65f，把结果存进 treeTop（treeTop）；之后引用 treeTop 时用的就是这次结果。
        float treeTop = top + 65f;
        // 步骤 36：计算 Mathf.Max(320f, height - 150f)，把结果存进 treeHeight（treeHeight）；之后引用 treeHeight 时用的就是这次结果。
        float treeHeight = Mathf.Max(320f, height - 150f);
        // 步骤 37：按 int i = 1; i < 5; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 1; i < 5; i++)
        {
            // 步骤 38：用三元表达式为 parent 二选一：问号前条件为真取前值，为假取冒号后的值。
            int parent = i == 1 || i == 2 ? 0 : i - 2;
            // 步骤 39：创建新的 Vector2 并存为 a；构造参数决定其初始内容或对象名称。
            Vector2 a = new Vector2(left + width * px[parent], treeTop + treeHeight * py[parent]);
            // 步骤 40：创建新的 Vector2 并存为 b；构造参数决定其初始内容或对象名称。
            Vector2 b = new Vector2(left + width * px[i], treeTop + treeHeight * py[i]);
            // 步骤 41：调用 DrawGuiLine：DrawGuiLine。
            DrawGuiLine(a, b, new Color(1f, 0.92f, 0.8f, 0.8f), 2f);
        }
        // 步骤 42：新建用于nodeStyle的对象或数值结构，并把构造参数保存进去。
        GUIStyle nodeStyle = new GUIStyle(GUI.skin.label) { fontSize = 29,
            // 步骤 43：把计算结果 FontStyle.Bold, alignment = TextAnchor.MiddleCenter } 保存为fontStyle；下一条判断或调用会读取这个具体值。
            fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        // 步骤 44：新建用于labelStyle的对象或数值结构，并把构造参数保存进去。
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 15,
            // 步骤 45：把计算结果 TextAnchor.MiddleCenter } 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.MiddleCenter };
        // 步骤 46：计算 Event.current，把结果存进 guiEvent（guiEvent）；之后引用 guiEvent 时用的就是这次结果。
        Event guiEvent = Event.current;
        // 步骤 47：把数值 -1 暂存为 currentHover；它只在当前方法里参与后续判断或计算。
        int currentHover = -1;
        // 步骤 48：按 int i = 0; i < 5; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 5; i++)
        {
            // 步骤 49：计算 left + width * px[i]，把结果存进 cx（cx）；之后引用 cx 时用的就是这次结果。
            float cx = left + width * px[i];
            // 步骤 50：计算 treeTop + treeHeight * py[i]，把结果存进 cy（cy）；之后引用 cy 时用的就是这次结果。
            float cy = treeTop + treeHeight * py[i];
            // 步骤 51：创建新的 Rect 并存为 iconRect；构造参数决定其初始内容或对象名称。
            Rect iconRect = new Rect(cx - 28f, cy - 28f, 56f, 56f);
            // 步骤 52：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = obtained[i] ? new Color(.55f, 1f, .75f) : energyColors[i];
            // 步骤 53：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(iconRect, glyphs[i], nodeStyle);
            // 步骤 54：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
            GUI.color = Color.white;
            // 步骤 55：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(new Rect(cx - 74f, cy + 28f, 148f, 40f), names[i] +
                // 步骤 56：执行 (obtained[i] ? "  Obtained" : ""), labelStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                (obtained[i] ? "  Obtained" : ""), labelStyle);
            // 步骤 57：检查条件 iconRect.Contains(guiEvent.mousePosition)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 currentHover = i;
            if (iconRect.Contains(guiEvent.mousePosition)) currentHover = i;
            // 步骤 58：调用 HandleGuiNodeDrag：跟踪兑换树图标的鼠标拖动并在合适位置生成奖励。
            HandleGuiNodeDrag(guiEvent, i, iconRect);
        }
        // 步骤 59：检查条件 currentHover != hoveredNode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (currentHover != hoveredNode)
        {
            // 步骤 60：把计算结果 currentHover 保存为鼠标悬停的树节点；下一条判断或调用会读取这个具体值。
            hoveredNode = currentHover;
            // 步骤 61：把计算结果 Time.unscaledTime 保存为hoveredSince；下一条判断或调用会读取这个具体值。
            hoveredSince = Time.unscaledTime;
        }
        // 步骤 62：检查条件 hoveredNode >= 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if (hoveredNode >= 0)
        {
            // 步骤 63：创建新的 Rect 并存为 tooltip；构造参数决定其初始内容或对象名称。
            Rect tooltip = new Rect(left + 40f, Screen.height - 140f, width - 80f, 40f);
            // 步骤 64：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = new Color(1f, .9f, .12f, .95f);
            // 步骤 65：调用 DrawTexture：Draw贴图。
            GUI.DrawTexture(tooltip, Texture2D.whiteTexture);
            // 步骤 66：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
            GUI.color = Color.white;
            // 步骤 67：创建新的 GUIStyle 并存为 tooltipStyle；构造参数决定其初始内容或对象名称。
            GUIStyle tooltipStyle = new GUIStyle(labelStyle);
            // 步骤 68：执行 tooltipStyle.normal.textColor = Color.black;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            tooltipStyle.normal.textColor = Color.black;
            // 步骤 69：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(tooltip, "E: EAT +3 FLY ENERGY", tooltipStyle);
        }
        // 步骤 70：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = Color.white;
        // 步骤 71：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(left + 35f, Screen.height - 80f, width - 70f, 32f),
            // 步骤 72：执行 "Drag an icon to exchange  ·  E to eat  ·  F to close", labelStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Drag an icon to exchange  ·  E to eat  ·  F to close", labelStyle);
        // 步骤 73：把计算结果 previous 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = previous;
    }
    // ！！！功能结束：OnGUI！！！

    // ！！！功能：HandleGuiNodeDrag——跟踪兑换树图标的鼠标拖动并在合适位置生成奖励！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Event guiEvent, int index, Rect iconRect 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void HandleGuiNodeDrag(Event guiEvent, int index, Rect iconRect)
    {
        // 步骤 1：检查条件 guiEvent.button != 0；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (guiEvent.button != 0) return;
        // 步骤 2：这里用 && 要求两边条件同时成立：guiEvent.type == EventType.MouseDown && iconRect.Contains(guiEvent.mousePosition)；任意一边为假就跳过本分支。
        if (guiEvent.type == EventType.MouseDown && iconRect.Contains(guiEvent.mousePosition))
        {
            // 步骤 3：把计算结果 index 保存为selected；下一条判断或调用会读取这个具体值。
            selected = index;
            // 步骤 4：把计算结果 index 保存为guiDragIndex；下一条判断或调用会读取这个具体值。
            guiDragIndex = index;
            // 步骤 5：把计算结果 guiEvent.mousePosition 保存为guiDragStart；下一条判断或调用会读取这个具体值。
            guiDragStart = guiEvent.mousePosition;
            // 步骤 6：把 guiDragTriggered 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            guiDragTriggered = false;
            // 步骤 7：调用 Use：Use。
            guiEvent.Use();
            // 步骤 8：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 9：检查条件 guiDragIndex != index；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (guiDragIndex != index) return;
        // 步骤 10：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (guiEvent.type == EventType.MouseDrag && !guiDragTriggered &&
            // 步骤 11：调用 Distance：Distance。
            Vector2.Distance(guiDragStart, guiEvent.mousePosition) >= 18f)
        {
            // 步骤 12：把 guiDragTriggered 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            guiDragTriggered = true;
            // 步骤 13：调用 Obtain：兑换成功后创建奖励物并登记拥有状态。
            Obtain(index);
            // 步骤 14：调用 Use：Use。
            guiEvent.Use();
        }
        // 步骤 15：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 guiEvent.type == EventType.MouseUp；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (guiEvent.type == EventType.MouseUp)
        {
            // 步骤 16：把计算结果 -1 保存为guiDragIndex；下一条判断或调用会读取这个具体值。
            guiDragIndex = -1;
            // 步骤 17：把 guiDragTriggered 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            guiDragTriggered = false;
            // 步骤 18：调用 Use：Use。
            guiEvent.Use();
        }
    }
    // ！！！功能结束：HandleGuiNodeDrag！！！

    // ！！！功能：DrawGuiLine——DrawGuiLine！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector2 from, Vector2 to, Color color, float thickness 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void DrawGuiLine(Vector2 from, Vector2 to, Color color, float thickness)
    {
        // 步骤 1：计算 GUI.matrix，把结果存进 oldMatrix（oldMatrix）；之后引用 oldMatrix 时用的就是这次结果。
        Matrix4x4 oldMatrix = GUI.matrix;
        // 步骤 2：计算 GUI.color，把结果存进 oldColor（oldColor）；之后引用 oldColor 时用的就是这次结果。
        Color oldColor = GUI.color;
        // 步骤 3：计算 Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
        float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        // 步骤 4：调用 RotateAroundPivot：RotateAroundPivot。
        GUIUtility.RotateAroundPivot(angle, from);
        // 步骤 5：把计算结果 color 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = color;
        // 步骤 6：调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(new Rect(from.x, from.y - thickness * 0.5f,
            // 步骤 7：调用 Distance：Distance。
            Vector2.Distance(from, to), thickness), Texture2D.whiteTexture);
        // 步骤 8：把计算结果 oldMatrix 保存为matrix；下一条判断或调用会读取这个具体值。
        GUI.matrix = oldMatrix;
        // 步骤 9：把计算结果 oldColor 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = oldColor;
    }
    // ！！！功能结束：DrawGuiLine！！！

    // ！！！功能：ToggleMenu——ToggleMenu！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ToggleMenu()
    {
        // 步骤 1：检查条件 IsMenuOpen；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 CloseMenu：关闭兑换树并恢复正常角色控制。
        if (IsMenuOpen) CloseMenu(); else OpenMenu();
        // 步骤 2：根据当前游戏时间计算并保存nextToggle，用于后续延迟或冷却判断。
        nextToggle = Time.time + 1f;
        // 步骤 3：把计算结果 -1f 保存为overheadStart；下一条判断或调用会读取这个具体值。
        overheadStart = -1f;
        // 步骤 4：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Left);
    }
    // ！！！功能结束：ToggleMenu！！！

    // ！！！功能：OpenMenu——打开先祖兑换树并解锁相关鼠标或手柄输入！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OpenMenu()
    {
        // 步骤 1：检查menuRoot是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (menuRoot == null) return;
        // 步骤 2：执行 magicExchange?.CloseMenu();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        magicExchange?.CloseMenu();
        // 步骤 3：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 4：把 desktopMenuOpen 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            desktopMenuOpen = true;
            // 步骤 5：执行 status?.Show("Ancestor exchange: select an item on the right. F or X closes.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show("Ancestor exchange: select an item on the right. F or X closes.", 5f);
            // 步骤 6：调用 RefreshNodes：刷新Nodes。
            RefreshNodes();
            // 步骤 7：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 8：计算 head.position - ancestor.position，把结果存进 towardPlayer（toward玩家）；之后引用 towardPlayer 时用的就是这次结果。
        Vector3 towardPlayer = head.position - ancestor.position;
        // 步骤 9：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
        towardPlayer.y = 0f;
        // 步骤 10：把计算结果 towardPlayer.sqrMagnitude > 0.01f ? towardPlayer.normalized : Vector3.forward 保存为toward玩家；下一条判断或调用会读取这个具体值。
        towardPlayer = towardPlayer.sqrMagnitude > 0.01f ? towardPlayer.normalized : Vector3.forward;
        // 步骤 11：把原三维朝向投影到地面平面并存为 viewForward；行走或转向不受头部俯仰影响。
        Vector3 viewForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        // 步骤 12：检查条件 viewForward.sqrMagnitude < 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 viewForward = transform.forward;
        if (viewForward.sqrMagnitude < 0.01f) viewForward = transform.forward;
        // 步骤 13：通过叉积算出与原方向垂直的向量，存为 viewRight 供左右移动使用。
        Vector3 viewRight = Vector3.Cross(Vector3.up, viewForward);
        // 步骤 14：执行 menuRoot.transform.position = head.position + viewForward * 2.2f + viewRight * 0.7f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        menuRoot.transform.position = head.position + viewForward * 2.2f + viewRight * 0.7f;
        // 步骤 15：执行 menuRoot.transform.rotation = Quaternion.LookRotation(menuRoot.transform.position - head.position, V；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        menuRoot.transform.rotation = Quaternion.LookRotation(menuRoot.transform.position - head.position, Vector3.up);
        // 步骤 16：执行 menuRoot.transform.localScale = Vector3.one * 0.68f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        menuRoot.transform.localScale = Vector3.one * 0.68f;
        // 步骤 17：显示这个场景对象及其子对象；界面会立即反映这一状态。
        menuRoot.SetActive(true);
        // 步骤 18：这一行补充 status?.Show(desktopMode ? "Ancestor items: 1-5 select, Enter obtain, E equip, F close." :，与相邻行合起来完成同一次计算或对象配置。
        status?.Show(desktopMode ? "Ancestor items: 1-5 select, Enter obtain, E equip, F close." :
            // 步骤 19：执行 "Ancestor items: point and hold right trigger. X closes the tree.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Ancestor items: point and hold right trigger. X closes the tree.", 5f);
        // 步骤 20：调用 RefreshNodes：刷新Nodes。
        RefreshNodes();
    }
    // ！！！功能结束：OpenMenu！！！

    // ！！！功能：CloseMenu——关闭兑换树并恢复正常角色控制！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CloseMenu()
    {
        // 步骤 1：把 desktopMenuOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        desktopMenuOpen = false;
        // 步骤 2：确认menuRoot已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (menuRoot != null) menuRoot.SetActive(false);
        // 步骤 3：确认ray已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 ray.enabled = false;
        if (ray != null) ray.enabled = false;
        // 步骤 4：把计算结果 -1 保存为selected；下一条判断或调用会读取这个具体值。
        selected = -1;
        // 步骤 5：把计算结果 -1 保存为当前正在阅读或观察的物品；下一条判断或调用会读取这个具体值。
        focused = -1;
        // 步骤 6：把计算结果 hoveredNode = focusedBefore = -1 保存为guiDragIndex；下一条判断或调用会读取这个具体值。
        guiDragIndex = hoveredNode = focusedBefore = -1;
    }
    // ！！！功能结束：CloseMenu！！！

    // 这一行补充 public void CloseForMagic() { if (IsMenuOpen) CloseMenu(); }，与相邻行合起来完成同一次计算或对象配置。
    public void CloseForMagic() { if (IsMenuOpen) CloseMenu(); }

    // ！！！功能：MakeMarker——MakeMarker！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void MakeMarker()
    {
        // 步骤 1：把计算结果 FindMemorialIconAnchor() 保存为memorialIconAnchor；下一条判断或调用会读取这个具体值。
        memorialIconAnchor = FindMemorialIconAnchor();
        // 步骤 2：创建新的 Transform 并存为 marker；构造参数决定其初始内容或对象名称。
        Transform marker = new GameObject("Ancestor interaction marker").transform;
        // 步骤 3：把position更新为 InteractionIconPosition()；世界或局部位置会立即影响物体摆放。
        marker.position = InteractionIconPosition();
        // 步骤 4：把计算结果 marker.gameObject 保存为iconRoot；下一条判断或调用会读取这个具体值。
        iconRoot = marker.gameObject;
        // 步骤 5：把计算结果 SkyQuestVisuals.Primitive("Interact with ancestor", PrimitiveType.Sphere, 保存为orb；下一条判断或调用会读取这个具体值。
        GameObject orb = SkyQuestVisuals.Primitive("Interact with ancestor", PrimitiveType.Sphere,
            // 步骤 6：执行 marker, Vector3.zero, Vector3.one * 0.36f, new Color(0.14f, 0.17f, 0.22f), true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            marker, Vector3.zero, Vector3.one * 0.36f, new Color(0.14f, 0.17f, 0.22f), true);
        // 步骤 7：执行 orb.AddComponent<SkyQuestAncestorIcon>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        orb.AddComponent<SkyQuestAncestorIcon>();
        // 步骤 8：把计算结果 SkyQuestVisuals.Primitive("Icon vertical ray", PrimitiveType.Cube, 保存为vertical；下一条判断或调用会读取这个具体值。
        GameObject vertical = SkyQuestVisuals.Primitive("Icon vertical ray", PrimitiveType.Cube,
            // 步骤 9：执行 marker, new Vector3(0f, 0f, -0.19f), new Vector3(0.035f, 0.22f, 0.025f), Color.white, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            marker, new Vector3(0f, 0f, -0.19f), new Vector3(0.035f, 0.22f, 0.025f), Color.white, true);
        // 步骤 10：把计算结果 SkyQuestVisuals.Primitive("Icon horizontal ray", PrimitiveType.Cube, 保存为horizontal；下一条判断或调用会读取这个具体值。
        GameObject horizontal = SkyQuestVisuals.Primitive("Icon horizontal ray", PrimitiveType.Cube,
            // 步骤 11：执行 marker, new Vector3(0f, 0f, -0.19f), new Vector3(0.22f, 0.035f, 0.025f), Color.white, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            marker, new Vector3(0f, 0f, -0.19f), new Vector3(0.22f, 0.035f, 0.025f), Color.white, true);
        // 步骤 12：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(vertical);
        // 步骤 13：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(horizontal);
    }
    // ！！！功能结束：MakeMarker！！！

    // ！！！功能：MakeMemorial——MakeMemorial！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void MakeMemorial()
    {
        // 步骤 1：确认memorialStone已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (memorialStone != null)
        {
            // 步骤 2：把计算结果 memorialStone 保存为memorial；下一条判断或调用会读取这个具体值。
            memorial = memorialStone;
            // 步骤 3：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 4：新建用于memorial的对象或数值结构，并把构造参数保存进去。
        memorial = new GameObject("Ancestor memorial stone").transform;
        // 步骤 5：新建用于position的对象或数值结构，并把构造参数保存进去。
        memorial.position = ancestor.position + new Vector3(0f, 0f, 0.8f);
        // 步骤 6：把计算结果 SkyQuestVisuals.Primitive("Carved memorial", PrimitiveType.Cube, memorial, 保存为stone；下一条判断或调用会读取这个具体值。
        var stone = SkyQuestVisuals.Primitive("Carved memorial", PrimitiveType.Cube, memorial,
            // 步骤 7：用 new 创建 Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 1.8f, 0.38f),；构造参数决定新对象的初始内容。
            new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 1.8f, 0.38f),
            // 步骤 8：用 new 创建 Color(0.24f, 0.37f, 0.43f));；构造参数决定新对象的初始内容。
            new Color(0.24f, 0.37f, 0.43f));
        // 步骤 9：执行 stone.transform.rotation = Quaternion.Euler(0f, 0f, -6f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        stone.transform.rotation = Quaternion.Euler(0f, 0f, -6f);
        // 步骤 10：调用 Text：Text。
        SkyQuestVisuals.Text("Memorial symbol", "◇", memorial,
            // 步骤 11：用 new 创建 Vector3(0f, 1.35f, -0.21f), 2f, new Color(1f, 0.88f, 0.64f), 0.44f);；构造参数决定新对象的初始内容。
            new Vector3(0f, 1.35f, -0.21f), 2f, new Color(1f, 0.88f, 0.64f), 0.44f);
    }
    // ！！！功能结束：MakeMemorial！！！

    // ！！！功能：UpdateSummon——更新Summon！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateSummon()
    {
        // 步骤 1：检查memorial是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (memorial == null) return;
        // 步骤 2：把计算结果 avatar != null ? avatar.position + Vector3.up * 1.2f : 保存为player；下一条判断或调用会读取这个具体值。
        Vector3 player = avatar != null ? avatar.position + Vector3.up * 1.2f :
            // 步骤 3：执行 (head != null ? head.position : transform.position);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (head != null ? head.position : transform.position);
        // 步骤 4：计算 player - memorial.position，把结果存进 delta（delta）；之后引用 delta 时用的就是这次结果。
        Vector3 delta = player - memorial.position;
        // 步骤 5：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
        delta.y = 0f;
        // 步骤 6：计算 delta.magnitude，把结果存进 distance（distance）；之后引用 distance 时用的就是这次结果。
        float distance = delta.magnitude;
        // 步骤 7：用 isInside 记住此处真假结果（distance <= summonRadius），供下一条 if 选择执行路径。
        bool isInside = distance <= summonRadius;
        // 步骤 8：这里用 && 要求两边条件同时成立：isInside && !insideSummonRadius；任意一边为假就跳过本分支。
        if (isInside && !insideSummonRadius)
        {
            // 步骤 9：把 insideSummonRadius 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            insideSummonRadius = true;
            // 步骤 10：这里的 ! 表示取反；当 summoned 不成立时才进入此分支。 成立时调用 SetSummoned：设置Summoned。
            if (!summoned) SetSummoned(true);
            // 步骤 11：前面的 if 条件为假时执行这里的备用处理。
            else emergence?.PlayReveal();
        }
        // 步骤 12：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：!isInside && distance > summonRadius + .25f；任意一边为假就跳过本分支。
        else if (!isInside && distance > summonRadius + .25f)
            // 步骤 13：把 insideSummonRadius 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            insideSummonRadius = false;

        // 步骤 14：这里用 && 要求两边条件同时成立：summoned && distance >= dismissRadius；任意一边为假就跳过本分支。 成立时调用 SetSummoned：设置Summoned。
        if (summoned && distance >= dismissRadius) SetSummoned(false);
    }
    // ！！！功能结束：UpdateSummon！！！

    // ！！！功能：SetSummoned——设置Summoned！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool visible 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetSummoned(bool visible)
    {
        // 步骤 1：把计算结果 visible 保存为summoned；下一条判断或调用会读取这个具体值。
        summoned = visible;
        // 步骤 2：确认emergence已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (emergence != null)
        {
            // 步骤 3：检查条件 visible；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 PlayReveal：PlayReveal。
            if (visible) emergence.PlayReveal();
            // 步骤 4：前面的 if 条件为假时执行这里的备用处理。
            else emergence.PlayDismiss();
        }
        // 步骤 5：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。确认spiritRenderers已经有值；只有存在时才能安全读取它的属性或调用方法。
        else if (spiritRenderers != null)
        {
            // 步骤 6：按 int i = 0; i < spiritRenderers.Length; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < spiritRenderers.Length; i++)
                // 步骤 7：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。
                if (spiritRenderers[i] != null)
                    // 步骤 8：执行 spiritRenderers[i].enabled = visible && spiritInitiallyVisible[i];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    spiritRenderers[i].enabled = visible && spiritInitiallyVisible[i];
        }
        // 步骤 9：确认iconRoot已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (iconRoot != null) iconRoot.SetActive(false);
        // 步骤 10：这里用 && 要求两边条件同时成立：!visible && IsMenuOpen；任意一边为假就跳过本分支。 成立时调用 CloseMenu：关闭兑换树并恢复正常角色控制。
        if (!visible && IsMenuOpen) CloseMenu();
    }
    // ！！！功能结束：SetSummoned！！！

    // ！！！功能：UpdateIcon——更新Icon！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateIcon()
    {
        // 步骤 1：把计算结果 summoned && IsNearAncestor && !IsMenuOpen && 保存为show；下一条判断或调用会读取这个具体值。
        bool show = summoned && IsNearAncestor && !IsMenuOpen &&
            // 步骤 2：执行 (magicExchange == null || !magicExchange.IsMenuOpen);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (magicExchange == null || !magicExchange.IsMenuOpen);
        // 步骤 3：确认iconRoot已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (iconRoot != null) iconRoot.SetActive(show);
        // 步骤 4：检查iconRoot是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (show && iconRoot != null)
        {
            // 步骤 5：执行 iconRoot.transform.position = InteractionIconPosition();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            iconRoot.transform.position = InteractionIconPosition();
            // 步骤 6：执行 iconRoot.transform.rotation = Quaternion.LookRotation(iconRoot.transform.position - head.position);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            iconRoot.transform.rotation = Quaternion.LookRotation(iconRoot.transform.position - head.position);
        }
    }
    // ！！！功能结束：UpdateIcon！！！

    // ！！！功能：InteractionIconPosition——InteractionIcon位置！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private Vector3 InteractionIconPosition()
    {
        // 步骤 1：确认memorialIconAnchor已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (memorialIconAnchor != null) return memorialIconAnchor.position;
        // 步骤 2：确认memorial已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (memorial != null) return memorial.position + Vector3.up * 1.35f;
        // 步骤 3：把 ancestor.position + Vector3.up * 2f 作为方法结果返回给调用方；return 后本方法立即结束。
        return ancestor.position + Vector3.up * 2f;
    }
    // ！！！功能结束：InteractionIconPosition！！！

    // ！！！功能：FindMemorialIconAnchor——查找MemorialIconAnchor！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private Transform FindMemorialIconAnchor()
    {
        // 步骤 1：检查memorial是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (memorial == null) return null;
        // 步骤 2：计算 null，把结果存进 fallback（fallback）；之后引用 fallback 时用的就是这次结果。
        Transform fallback = null;
        // 步骤 3：逐个读取 Transform child in memorial.GetComponentsInChildren<Transform>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform child in memorial.GetComponentsInChildren<Transform>(true))
        {
            // 步骤 4：计算 child.name.ToLowerInvariant()，把结果存进 childName（childName）；之后引用 childName 时用的就是这次结果。
            string childName = child.name.ToLowerInvariant();
            // 步骤 5：这里用 || 表示任一条件成立就处理：childName == "memorial star" || childName == "memorial symbol"；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
            if (childName == "memorial star" || childName == "memorial symbol") return child;
            // 步骤 6：检查fallback是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (fallback == null && (childName.Contains("star") || childName.Contains("symbol")))
                // 步骤 7：把计算结果 child 保存为fallback；下一条判断或调用会读取这个具体值。
                fallback = child;
        }
        // 步骤 8：把 fallback 作为方法结果返回给调用方；return 后本方法立即结束。
        return fallback;
    }
    // ！！！功能结束：FindMemorialIconAnchor！！！

    // ！！！功能：PointingAtIcon——PointingAtIcon！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private bool PointingAtIcon()
    {
        // 步骤 1：计算 Physics.RaycastAll(rightHand.position, rightHand.forward, 4f)，把结果存进 hits（hits）；之后引用 hits 时用的就是这次结果。
        RaycastHit[] hits = Physics.RaycastAll(rightHand.position, rightHand.forward, 4f);
        // 步骤 2：逐个读取 RaycastHit hit in hits 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in hits)
            // 步骤 3：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
            if (hit.collider.GetComponent<SkyQuestAncestorIcon>() != null) return true;
        // 步骤 4：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
        return false;
    }
    // ！！！功能结束：PointingAtIcon！！！

    // ！！！功能：MakeMenu——MakeMenu！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void MakeMenu()
    {
        // 步骤 1：新建用于menuRoot的对象或数值结构，并把构造参数保存进去。
        menuRoot = new GameObject("Ancestor spatial item tree");
        // 步骤 2：执行 menuRoot.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        menuRoot.transform.SetParent(transform, false);
        // 步骤 3：新建用于nodes的对象或数值结构，并把构造参数保存进去。
        nodes = new Transform[names.Length];
        // 步骤 4：新建用于labels的对象或数值结构，并把构造参数保存进去。
        labels = new TextMeshPro[names.Length];
        // 步骤 5：把计算结果 SkyQuestVisuals.Primitive("Tree backing", PrimitiveType.Cube, 保存为backing；下一条判断或调用会读取这个具体值。
        GameObject backing = SkyQuestVisuals.Primitive("Tree backing", PrimitiveType.Cube,
            // 步骤 6：继续填写上一行调用的参数：menuRoot.transform, new Vector3(0f, 0f, 0.1f), new Vector3(3.25f, 2.85f, 0.05f)；参数位置决定它表示的坐标、速度或选项。
            menuRoot.transform, new Vector3(0f, 0f, 0.1f), new Vector3(3.25f, 2.85f, 0.05f),
            // 步骤 7：用 new 创建 Color(0.11f, 0.12f, 0.2f, 0.75f));；构造参数决定新对象的初始内容。
            new Color(0.11f, 0.12f, 0.2f, 0.75f));
        // 步骤 8：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(backing);
        // 步骤 9：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        backing.SetActive(false);
        // 步骤 10：调用 Text：Text。
        SkyQuestVisuals.Text("Title", "ANCESTOR ITEMS", menuRoot.transform,
            // 步骤 11：用 new 创建 Vector3(0f, 1.28f, -0.04f), 2.4f, new Color(1f, 0.87f, 0.58f), 1.1f);；构造参数决定新对象的初始内容。
            new Vector3(0f, 1.28f, -0.04f), 2.4f, new Color(1f, 0.87f, 0.58f), 1.1f);
        // 步骤 12：按 int i = 0; i < names.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < names.Length; i++)
        {
            // 步骤 13：把计算结果 SkyQuestVisuals.Primitive(names[i] + " node", PrimitiveType.Sphere, 保存为sphere；下一条判断或调用会读取这个具体值。
            GameObject sphere = SkyQuestVisuals.Primitive(names[i] + " node", PrimitiveType.Sphere,
                // 步骤 14：继续填写上一行调用的参数：menuRoot.transform, new Vector3(positions[i].x, positions[i].y, -0.08f)；参数位置决定它表示的坐标、速度或选项。
                menuRoot.transform, new Vector3(positions[i].x, positions[i].y, -0.08f),
                // 步骤 15：执行 Vector3.one * 0.19f, Color.white);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * 0.19f, Color.white);
            // 步骤 16：执行 sphere.AddComponent<SkyQuestNode>().index = i;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            sphere.AddComponent<SkyQuestNode>().index = i;
            // 步骤 17：执行 nodes[i] = sphere.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            nodes[i] = sphere.transform;
            // 步骤 18：继续填写上一行调用的参数：labels[i] = SkyQuestVisuals.Text(names[i] + " label", (i + 1) + " " + names[i]；参数位置决定它表示的坐标、速度或选项。
            labels[i] = SkyQuestVisuals.Text(names[i] + " label", (i + 1) + " " + names[i],
                // 步骤 19：继续填写上一行调用的参数：menuRoot.transform, new Vector3(positions[i].x, positions[i].y - 0.19f, -0.11f)；参数位置决定它表示的坐标、速度或选项。
                menuRoot.transform, new Vector3(positions[i].x, positions[i].y - 0.19f, -0.11f),
                // 步骤 20：执行 1.55f, Color.white, 0.42f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                1.55f, Color.white, 0.42f);
        }
        // 步骤 21：按 int i = 1; i < names.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 1; i < names.Length; i++)
        {
            // 步骤 22：用三元表达式为 parent 二选一：问号前条件为真取前值，为假取冒号后的值。
            int parent = i < 3 ? 0 : i - 2;
            // 步骤 23：创建新的 GameObject 并存为 link；构造参数决定其初始内容或对象名称。
            GameObject link = new GameObject("Exchange branch " + i, typeof(LineRenderer));
            // 步骤 24：执行 link.transform.SetParent(menuRoot.transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            link.transform.SetParent(menuRoot.transform, false);
            // 步骤 25：从目标物体找到所需组件并存为 line；使用前应检查是否为 null。
            LineRenderer line = link.GetComponent<LineRenderer>();
            // 步骤 26：把 line.useWorldSpace 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            line.useWorldSpace = false;
            // 步骤 27：把positionCount更新为 2；世界或局部位置会立即影响物体摆放。
            line.positionCount = 2;
            // 步骤 28：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
            line.SetPosition(0, new Vector3(positions[parent].x, positions[parent].y, -0.06f));
            // 步骤 29：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
            line.SetPosition(1, new Vector3(positions[i].x, positions[i].y, -0.06f));
            // 步骤 30：把计算结果 line.endWidth = 0.012f 保存为startWidth；下一条判断或调用会读取这个具体值。
            line.startWidth = line.endWidth = 0.012f;
            // 步骤 31：新建用于material的对象或数值结构，并把构造参数保存进去。
            line.material = SkyQuestVisuals.Material(new Color(1f, 0.92f, 0.77f));
        }
        // 步骤 32：调用 Text：Text。
        SkyQuestVisuals.Text("Instructions", desktopMode ?
                // 步骤 33：这一行补充 "Drag icon: exchange    E: eat    F: close" :，与相邻行合起来完成同一次计算或对象配置。
                "Drag icon: exchange    E: eat    F: close" :
                // 步骤 34：继续填写上一行调用的参数："Right trigger: hold or pull icon to exchange    Right grip: pick up / drop"；参数位置决定它表示的坐标、速度或选项。
                "Right trigger: hold or pull icon to exchange    Right grip: pick up / drop",
            // 步骤 35：执行 menuRoot.transform, new Vector3(0f, -1.24f, -0.08f), 1.35f, Color.white, 1.05f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            menuRoot.transform, new Vector3(0f, -1.24f, -0.08f), 1.35f, Color.white, 1.05f);
        // 步骤 36：把计算结果 SkyQuestVisuals.Text("Energy reward hint", "", menuRoot.transform, 保存为hoverHelp；下一条判断或调用会读取这个具体值。
        hoverHelp = SkyQuestVisuals.Text("Energy reward hint", "", menuRoot.transform,
            // 步骤 37：用 new 创建 Vector3(0f, -1.02f, -0.08f), 1.4f, Color.black, 1.2f);；构造参数决定新对象的初始内容。
            new Vector3(0f, -1.02f, -0.08f), 1.4f, Color.black, 1.2f);
        // 步骤 38：把计算结果 SkyQuestVisuals.Primitive("Energy hint yellow plate", PrimitiveType.Cube, 保存为hoverHelpPlate；下一条判断或调用会读取这个具体值。
        hoverHelpPlate = SkyQuestVisuals.Primitive("Energy hint yellow plate", PrimitiveType.Cube,
            // 步骤 39：继续填写上一行调用的参数：menuRoot.transform, new Vector3(0f, -1.02f, -.06f)；参数位置决定它表示的坐标、速度或选项。
            menuRoot.transform, new Vector3(0f, -1.02f, -.06f),
            // 步骤 40：用 new 创建 Vector3(.33f, .21f, .012f), new Color(1f, .9f, .12f), true);；构造参数决定新对象的初始内容。
            new Vector3(.33f, .21f, .012f), new Color(1f, .9f, .12f), true);
        // 步骤 41：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(hoverHelpPlate);
        // 步骤 42：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        hoverHelpPlate.SetActive(false);
        // 步骤 43：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 hoverHelp.enabled = false;
        if (desktopMode) hoverHelp.enabled = false;
        // 步骤 44：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        menuRoot.SetActive(false);
    }
    // ！！！功能结束：MakeMenu！！！

    // ！！！功能：MakeRay——Make射线！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void MakeRay()
    {
        // 步骤 1：创建新的 GameObject 并存为 pointer；构造参数决定其初始内容或对象名称。
        GameObject pointer = new GameObject("Ancestor controller ray", typeof(LineRenderer));
        // 步骤 2：执行 pointer.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        pointer.transform.SetParent(transform, false);
        // 步骤 3：从目标 GameObject 查找并保存 ray 组件；后面的移动或显示逻辑会直接使用这个缓存。
        ray = pointer.GetComponent<LineRenderer>();
        // 步骤 4：把positionCount更新为 2；世界或局部位置会立即影响物体摆放。
        ray.positionCount = 2;
        // 步骤 5：把计算结果 0.01f 保存为startWidth；下一条判断或调用会读取这个具体值。
        ray.startWidth = 0.01f;
        // 步骤 6：把计算结果 0.005f 保存为endWidth；下一条判断或调用会读取这个具体值。
        ray.endWidth = 0.005f;
        // 步骤 7：新建用于material的对象或数值结构，并把构造参数保存进去。
        ray.material = SkyQuestVisuals.Material(new Color(1f, 0.85f, 0.45f));
        // 步骤 8：把 ray.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        ray.enabled = false;
    }
    // ！！！功能结束：MakeRay！！！

    // ！！！功能：UpdateRay——更新射线！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateRay()
    {
        // 步骤 1：计算 Physics.RaycastAll(rightHand.position, rightHand.forward, 4f)，把结果存进 hits（hits）；之后引用 hits 时用的就是这次结果。
        RaycastHit[] hits = Physics.RaycastAll(rightHand.position, rightHand.forward, 4f);
        // 步骤 2：把计算结果 -1 保存为当前正在阅读或观察的物品；下一条判断或调用会读取这个具体值。
        focused = -1;
        // 步骤 3：计算 float.PositiveInfinity，把结果存进 bestDistance（bestDistance）；之后引用 bestDistance 时用的就是这次结果。
        float bestDistance = float.PositiveInfinity;
        // 步骤 4：计算 rightHand.position + rightHand.forward * 3f，把结果存进 end（end）；之后引用 end 时用的就是这次结果。
        Vector3 end = rightHand.position + rightHand.forward * 3f;
        // 步骤 5：逐个读取 RaycastHit hit in hits 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in hits)
        {
            // 步骤 6：从目标物体找到所需组件并存为 node；使用前应检查是否为 null。
            SkyQuestNode node = hit.collider.GetComponent<SkyQuestNode>();
            // 步骤 7：检查node是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (node == null || hit.distance >= bestDistance) continue;
            // 步骤 8：把计算结果 hit.distance 保存为bestDistance；下一条判断或调用会读取这个具体值。
            bestDistance = hit.distance;
            // 步骤 9：把计算结果 node.index 保存为当前正在阅读或观察的物品；下一条判断或调用会读取这个具体值。
            focused = node.index;
            // 步骤 10：把计算结果 hit.point 保存为end；下一条判断或调用会读取这个具体值。
            end = hit.point;
        }
        // 步骤 11：把 ray.enabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        ray.enabled = true;
        // 步骤 12：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
        ray.SetPosition(0, rightHand.position);
        // 步骤 13：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
        ray.SetPosition(1, end);
    }
    // ！！！功能结束：UpdateRay！！！

    // ！！！功能：RefreshNodes——刷新Nodes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RefreshNodes()
    {
        // 步骤 1：检查nodes是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (nodes == null) return;
        // 步骤 2：按 int i = 0; i < nodes.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < nodes.Length; i++)
        {
            // 步骤 3：检查对象引用是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (nodes[i] == null) continue;
            // 步骤 4：新建用于color的对象或数值结构，并把构造参数保存进去。
            Color color = equipped[i] ? new Color(0.35f, 1f, 0.7f) :
                // 步骤 5：这一行补充 obtained[i] ? new Color(0.35f, 0.7f, 1f) :，与相邻行合起来完成同一次计算或对象配置。
                obtained[i] ? new Color(0.35f, 0.7f, 1f) :
                // 步骤 6：新建用于selected的对象或数值结构，并把构造参数保存进去。
                selected == i ? new Color(1f, 0.6f, 0.25f) :
                // 步骤 7：新建用于当前正在阅读或观察的物品的对象或数值结构，并把构造参数保存进去。
                focused == i ? new Color(1f, 0.95f, 0.5f) : Color.white;
            // 步骤 8：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            nodes[i].GetComponent<Renderer>().material.color = color;
            // 步骤 9：这一行补充 labels[i].text = (i + 1) + " " + names[i] +，与相邻行合起来完成同一次计算或对象配置。
            labels[i].text = (i + 1) + " " + names[i] +
                // 步骤 10：执行 (equipped[i] ? " EQUIPPED" : obtained[i] ? " OBTAINED" : selected == i ? " SELECTED" : "");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                (equipped[i] ? " EQUIPPED" : obtained[i] ? " OBTAINED" : selected == i ? " SELECTED" : "");
        }
    }
    // ！！！功能结束：RefreshNodes！！！

    // ！！！功能：Obtain——兑换成功后创建奖励物并登记拥有状态！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Obtain(int index)
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：index < 0 || index >= obtained.Length || obtained[index]；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (index < 0 || index >= obtained.Length || obtained[index]) return;
        // 步骤 2：执行 obtained[index] = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obtained[index] = true;
        // 步骤 3：创建新的 GameObject 并存为 reward；构造参数决定其初始内容或对象名称。
        GameObject reward = new GameObject(names[index] + " reward");
        // 步骤 4：把计算结果 SkyQuestVisuals.Primitive("Cape energy cube", PrimitiveType.Cube, 保存为cube；下一条判断或调用会读取这个具体值。
        GameObject cube = SkyQuestVisuals.Primitive("Cape energy cube", PrimitiveType.Cube,
            // 步骤 5：继续填写上一行调用的参数：reward.transform, new Vector3(0f, .3f, 0f), Vector3.one * .45f；参数位置决定它表示的坐标、速度或选项。
            reward.transform, new Vector3(0f, .3f, 0f), Vector3.one * .45f,
            // 步骤 6：执行 energyColors[index], true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            energyColors[index], true);
        // 步骤 7：从目标物体找到所需组件并存为 cubeCollider；使用前应检查是否为 null。
        Collider cubeCollider = cube.GetComponent<Collider>();
        // 步骤 8：确认cubeCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 cubeCollider.enabled = false;
        if (cubeCollider != null) cubeCollider.enabled = false;
        // 步骤 9：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(cube);
        // 步骤 10：执行 reward.transform.position = rightHand.position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        reward.transform.position = rightHand.position;
        // 步骤 11：计算 reward.AddComponent<SkyQuestMagicPickup>()，把结果存进 pickup（pickup）；之后引用 pickup 时用的就是这次结果。
        SkyQuestMagicPickup pickup = reward.AddComponent<SkyQuestMagicPickup>();
        // 步骤 12：把计算结果 head 保存为desktopHead；下一条判断或调用会读取这个具体值。
        pickup.desktopHead = head;
        // 步骤 13：把计算结果 rightHand 保存为desktopHand；下一条判断或调用会读取这个具体值。
        pickup.desktopHand = rightHand;
        // 步骤 14：把计算结果 avatar 保存为desktopAvatar；下一条判断或调用会读取这个具体值。
        pickup.desktopAvatar = avatar;
        // 步骤 15：把计算结果 desktopMode 保存为desktop模式；下一条判断或调用会读取这个具体值。
        pickup.desktopMode = desktopMode;
        // 步骤 16：把 pickup.restoresCapeEnergy 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        pickup.restoresCapeEnergy = true;
        pickup.capeEnergyGain = 3;
        // 步骤 17：调用 SetHeld：设置Held。
        pickup.SetHeld(rightHand);
        // 步骤 18：这一行补充 status?.Show(names[index] + (desktopMode ?，与相邻行合起来完成同一次计算或对象配置。
        status?.Show(names[index] + (desktopMode ?
            // 步骤 19：这一行补充 " exchanged. E to eat for +3 fly energy; Z to drop/pick up." :，与相邻行合起来完成同一次计算或对象配置。
            " exchanged. E to eat for +3 fly energy; Z to drop/pick up." :
            // 步骤 20：执行 " exchanged. Point and hold right trigger to aim; release to throw. Hold near face to eat."), 6f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            " exchanged. Point and hold right trigger to aim; release to throw. Hold near face to eat."), 6f);
        // 步骤 21：执行 status?.PlayConfirm();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.PlayConfirm();
        // 步骤 22：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.6f, 0.15f);
        // 步骤 23：调用 RefreshNodes：刷新Nodes。
        RefreshNodes();
    }
    // ！！！功能结束：Obtain！！！

    // ！！！功能：MoveToHand——Move到Hand！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject item 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private IEnumerator MoveToHand(GameObject item)
    {
        // 步骤 1：计算 item.transform.position，把结果存进 start（start）；之后引用 start 时用的就是这次结果。
        Vector3 start = item.transform.position;
        // 步骤 2：把数值 0f 暂存为 elapsed；它只在当前方法里参与后续判断或计算。
        float elapsed = 0f;
        // 步骤 3：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (elapsed < 0.55f && item != null)
        {
            // 步骤 4：把每秒变化率乘本帧秒数，更新elapsed；帧率变化时速度仍接近一致。
            elapsed += Time.deltaTime;
            // 步骤 5：计算 Mathf.SmoothStep(0f, 1f, elapsed / 0.55f)，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.55f);
            // 步骤 6：执行 item.transform.position = Vector3.Lerp(start, rightHand.position, t);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            item.transform.position = Vector3.Lerp(start, rightHand.position, t);
            // 步骤 7：执行 yield return null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            yield return null;
        }
        // 步骤 8：确认item已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (item != null)
        {
            // 步骤 9：执行 item.transform.SetParent(rightHand, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            item.transform.SetParent(rightHand, false);
            // 步骤 10：执行 item.transform.localPosition = new Vector3(0f, 0f, 0.12f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            item.transform.localPosition = new Vector3(0f, 0f, 0.12f);
            // 步骤 11：执行 item.transform.localRotation = Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            item.transform.localRotation = Quaternion.identity;
        }
        // 步骤 12：清空 moveToHandRoutine 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        moveToHandRoutine = null;
    }
    // ！！！功能结束：MoveToHand！！！

    // ！！！功能：EquipHeld——EquipHeld！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void EquipHeld()
    {
        // 步骤 1：检查角色模型、heldVisual是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (heldIndex < 0 || avatar == null || heldVisual == null) return;
        // 步骤 2：确认move到HandRoutine已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (moveToHandRoutine != null)
        {
            // 步骤 3：调用 StopCoroutine：StopCoroutine。
            StopCoroutine(moveToHandRoutine);
            // 步骤 4：清空 moveToHandRoutine 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            moveToHandRoutine = null;
        }
        // 步骤 5：计算 heldIndex，把结果存进 index（index）；之后引用 index 时用的就是这次结果。
        int index = heldIndex;
        // 步骤 6：执行 heldVisual.transform.SetParent(avatar, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        heldVisual.transform.SetParent(avatar, false);
        // 步骤 7：这一行补充 heldVisual.transform.localPosition = index == 2 ? new Vector3(0f, 1.5f, 0.23f) :，与相邻行合起来完成同一次计算或对象配置。
        heldVisual.transform.localPosition = index == 2 ? new Vector3(0f, 1.5f, 0.23f) :
            // 步骤 8：新建用于index的对象或数值结构，并把构造参数保存进去。
            index == 0 ? new Vector3(0f, 1.1f, -0.18f) : new Vector3(0f, 1.65f, 0.05f);
        // 步骤 9：执行 heldVisual.transform.localRotation = Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        heldVisual.transform.localRotation = Quaternion.identity;
        // 步骤 10：执行 heldVisual.transform.localScale = index == 2 ? new Vector3(0.34f, 0.22f, 0.12f) : Vector3.one * 0.3f；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        heldVisual.transform.localScale = index == 2 ? new Vector3(0.34f, 0.22f, 0.12f) : Vector3.one * 0.3f;
        // 步骤 11：执行 equipped[index] = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        equipped[index] = true;
        // 步骤 12：把计算结果 -1 保存为heldIndex；下一条判断或调用会读取这个具体值。
        heldIndex = -1;
        // 步骤 13：清空 heldVisual 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        heldVisual = null;
        // 步骤 14：调用 CloseMenu：关闭兑换树并恢复正常角色控制。
        CloseMenu();
        // 步骤 15：执行 locomotion?.ShowEquippedAppearance();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        locomotion?.ShowEquippedAppearance();
        // 步骤 16：执行 status?.Show(names[index] + " EQUIPPED", 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show(names[index] + " EQUIPPED", 4f);
        // 步骤 17：执行 status?.PlayConfirm();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.PlayConfirm();
        // 步骤 18：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.5f, 0.12f);
    }
    // ！！！功能结束：EquipHeld！！！

    // ！！！功能：DesktopToggleMenu——桌面ToggleMenu！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopToggleMenu()
    {
        // 步骤 1：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!desktopMode) return;
        // 步骤 2：检查条件 IsMenuOpen；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 CloseMenu：关闭兑换树并恢复正常角色控制。
        if (IsMenuOpen) CloseMenu();
        // 步骤 3：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 IsNearAncestor；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (IsNearAncestor)
        {
            // 步骤 4：执行 magicExchange?.CloseMenu();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            magicExchange?.CloseMenu();
            // 步骤 5：调用 OpenMenu：打开先祖兑换树并解锁相关鼠标或手柄输入。
            OpenMenu();
        }
        // 步骤 6：前面的 if 条件为假时执行这里的备用处理。
        else status?.Show("Move closer to the Ancestor before opening the tree.");
    }
    // ！！！功能结束：DesktopToggleMenu！！！

    // ！！！功能：DesktopSelect——桌面Select！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopSelect(int index)
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：!desktopMode || !IsMenuOpen || index < 0 || index >= names.Length；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!desktopMode || !IsMenuOpen || index < 0 || index >= names.Length) return;
        // 步骤 2：检查条件 obtained[index]；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { status?.Show(names[index] + " is already obtained."); return; }
        if (obtained[index]) { status?.Show(names[index] + " is already obtained."); return; }
        // 步骤 3：把计算结果 index 保存为selected；下一条判断或调用会读取这个具体值。
        selected = index;
        // 步骤 4：执行 status?.Show(names[index] + " selected. Press Enter to obtain.", 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show(names[index] + " selected. Press Enter to obtain.", 4f);
        // 步骤 5：调用 RefreshNodes：刷新Nodes。
        RefreshNodes();
    }
    // ！！！功能结束：DesktopSelect！！！

    // ！！！功能：DesktopObtainSelected——桌面ObtainSelected！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopObtainSelected()
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：!desktopMode || !IsMenuOpen || selected < 0 || obtained[selected]；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!desktopMode || !IsMenuOpen || selected < 0 || obtained[selected]) return;
        // 步骤 2：计算 selected，把结果存进 index（index）；之后引用 index 时用的就是这次结果。
        int index = selected;
        // 步骤 3：把计算结果 -1 保存为selected；下一条判断或调用会读取这个具体值。
        selected = -1;
        // 步骤 4：调用 Obtain：兑换成功后创建奖励物并登记拥有状态。
        Obtain(index);
    }
    // ！！！功能结束：DesktopObtainSelected！！！

    // ！！！功能：MakeItemVisual——Make物品Visual！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private GameObject MakeItemVisual(int index)
    {
        // 步骤 1：检查mask预制体是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (index == 2 && maskPrefab != null)
        {
            // 步骤 2：创建新的 GameObject 并存为 root；构造参数决定其初始内容或对象名称。
            GameObject root = new GameObject("Obtained mask");
            // 步骤 3：计算 Instantiate(maskPrefab, root.transform)，把结果存进 model（model）；之后引用 model 时用的就是这次结果。
            GameObject model = Instantiate(maskPrefab, root.transform);
            // 步骤 4：显示这个场景对象及其子对象；界面会立即反映这一状态。
            model.SetActive(true);
            // 步骤 5：从目标物体找到所需组件并存为 renderers；使用前应检查是否为 null。
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            // 步骤 6：检查条件 renderers.Length > 0；只有为真才运行下面的步骤，否则沿原流程继续。
            if (renderers.Length > 0)
            {
                // 步骤 7：计算 renderers[0].bounds，把结果存进 bounds（bounds）；之后引用 bounds 时用的就是这次结果。
                Bounds bounds = renderers[0].bounds;
                // 步骤 8：逐个读取 Renderer renderer in renderers 中的元素；每轮处理当前一个，直到全部遍历完。
                foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
                // 步骤 9：检查条件 bounds.size.y > 0.001f；只有为真才运行下面的步骤，否则沿原流程继续。
                if (bounds.size.y > 0.001f)
                {
                    // 步骤 10：计算 1f / bounds.size.y，把结果存进 scale（scale）；之后引用 scale 时用的就是这次结果。
                    float scale = 1f / bounds.size.y;
                    // 步骤 11：执行 model.transform.localScale *= scale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    model.transform.localScale *= scale;
                    // 步骤 12：执行 model.transform.position -= (bounds.center - root.transform.position) * scale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    model.transform.position -= (bounds.center - root.transform.position) * scale;
                }
            }
            // 步骤 13：逐个读取 Collider collider in model.GetComponentsInChildren<Collider>() 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Collider collider in model.GetComponentsInChildren<Collider>())
            {
                // 步骤 14：把 collider.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                collider.enabled = false;
                // 步骤 15：调用 Destroy：Destroy。
                Destroy(collider);
            }
            // 步骤 16：把 root 作为方法结果返回给调用方；return 后本方法立即结束。
            return root;
        }
        // 步骤 17：用三元表达式为 shape 二选一：问号前条件为真取前值，为假取冒号后的值。
        PrimitiveType shape = index == 0 ? PrimitiveType.Capsule : index == 4 ? PrimitiveType.Cylinder : PrimitiveType.Sphere;
        // 步骤 18：把计算结果 SkyQuestVisuals.Primitive(names[index] + " item", shape, transform, 保存为item；下一条判断或调用会读取这个具体值。
        GameObject item = SkyQuestVisuals.Primitive(names[index] + " item", shape, transform,
            // 步骤 19：执行 Vector3.zero, Vector3.one * 0.2f, new Color(1f, 0.8f, 0.45f), true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.zero, Vector3.one * 0.2f, new Color(1f, 0.8f, 0.45f), true);
        // 步骤 20：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(item);
        // 步骤 21：把 item 作为方法结果返回给调用方；return 后本方法立即结束。
        return item;
    }
    // ！！！功能结束：MakeItemVisual！！！
}

// 定义 SkyQuestAncestorIcon 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestAncestorIcon : MonoBehaviour { }

// 定义 SkyQuestNode 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestNode : MonoBehaviour
{
    // 声明 index（index）：整数计数；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public int index;
}
