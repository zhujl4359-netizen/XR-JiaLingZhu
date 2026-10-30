// ！！！文件功能：IP1 先祖兑换树：蜡烛余额、解锁、拖拽物品与菜单！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

// 定义 SkyQuestCandleWallet 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestCandleWallet
{
    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public static int Balance { get; private set; } = 100;

    // 让 Unity 在应用运行初始化阶段自动调用下方方法；用于清除上次 Play 留下的静态重启状态。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    // ！！！功能：ResetBalance——复位Balance！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ResetBalance() => Balance = 100;
    // ！！！功能结束：ResetBalance！！！

    // ！！！功能：Add——把当前充能来源或条目加入集合供后续统计！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int amount 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void Add(int amount)
    {
        // 步骤 1：检查条件 amount > 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 Balance += amount;
        if (amount > 0) Balance += amount;
    }
    // ！！！功能结束：Add！！！

    // ！！！功能：TrySpend——TrySpend！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int amount 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public static bool TrySpend(int amount)
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：amount < 0 || Balance < amount；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (amount < 0 || Balance < amount) return false;
        // 步骤 2：在Balance原值上减少 amount；旧状态与本次输入共同决定新状态。
        Balance -= amount;
        // 步骤 3：本次检测或激活成功，返回 true 让调用方继续相应反馈。
        return true;
    }
    // ！！！功能结束：TrySpend！！！
}

// Independent exchange trees for every visible gallery character. Rewards become
// tangible Sky-inspired props and move directly into the player's right hand.
// 定义 SkyQuestMagicExchange 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestMagicExchange : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 leftHand（左手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform leftHand;
    // 声明 rightHand（右手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform rightHand;
    // 声明 avatar（角色模型）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform avatar;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    public SkyQuestStatus status;
    // legacyTree 保存legacyTree（类型 SkyQuestAncestor）；下方方法依靠它记录或查找场景状态。
    public SkyQuestAncestor legacyTree;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;
    // 创建新的 Transform[] 并存为 ancestors；构造参数决定其初始内容或对象名称。
    public Transform[] ancestors = new Transform[6];
    // 创建新的 Transform[] 并存为 memorials；构造参数决定其初始内容或对象名称。
    public Transform[] memorials = new Transform[6];
    // 创建新的 GameObject[] 并存为 ceramicRewardPrefabs；构造参数决定其初始内容或对象名称。
    public GameObject[] ceramicRewardPrefabs = new GameObject[8];
    // 把 Inspector 可调整的 proximity（proximity）设为默认值 3.8f；具体用途看它在下方的判断和计算。
    public float proximity = 3.8f;
    // 把 Inspector 可调整的 holdDuration（holdDuration）设为默认值 0.55f；具体用途看它在下方的判断和计算。
    public float holdDuration = 0.55f;
    // 把 Inspector 可调整的 summonRadius（summonRadius）设为默认值 6f；具体用途看它在下方的判断和计算。
    public float summonRadius = 6f;
    // 把 Inspector 可调整的 dismissRadius（dismissRadius）设为默认值 8f；具体用途看它在下方的判断和计算。
    public float dismissRadius = 8f;

    // 这一行补充 private static readonly string[,] SpellNames =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly string[,] SpellNames =
    {
        // 继续填写上一行调用的参数：{ "Glow Spell", "Anti-Gravity Spell", "Fast Recharge Spell", "Fireworks Spell", "Shrink Spell" }；参数位置决定它表示的坐标、速度或选项。
        { "Glow Spell", "Anti-Gravity Spell", "Fast Recharge Spell", "Fireworks Spell", "Shrink Spell" },
        // 继续填写上一行调用的参数：{ "Rainbow Trail Spell", "Resize Potion", "Chat Table Spell", "Bonfire Spell", "Instant Recharge Pot；参数位置决定它表示的坐标、速度或选项。
        { "Rainbow Trail Spell", "Resize Potion", "Chat Table Spell", "Bonfire Spell", "Instant Recharge Potion" },
        // 继续填写上一行调用的参数：{ "Flight Energy I", "Flight Energy II", "Flight Energy III", "Flight Energy IV", "Flight Energy V" ；参数位置决定它表示的坐标、速度或选项。
        { "Flight Energy I", "Flight Energy II", "Flight Energy III", "Flight Energy IV", "Flight Energy V" },
        // 继续填写上一行调用的参数：{ "Candlelight Spell", "Warmth Spell", "Glitter Spell", "Trail Spell", "Bloom Spell" }；参数位置决定它表示的坐标、速度或选项。
        { "Candlelight Spell", "Warmth Spell", "Glitter Spell", "Trail Spell", "Bloom Spell" },
        // 继续填写上一行调用的参数：{ "Celadon Offering", "Ivory Ritual Jar", "Azure Incense Vessel", "Amber Offering Bowl", "Moon Glaze；参数位置决定它表示的坐标、速度或选项。
        { "Celadon Offering", "Ivory Ritual Jar", "Azure Incense Vessel", "Amber Offering Bowl", "Moon Glaze Vase" },
        // 这一行补充 { "Flight Energy I", "Flight Energy II", "Flight Energy III", "Flight Energy IV", "Flight Energy V" ，与相邻行合起来完成同一次计算或对象配置。
        { "Flight Energy I", "Flight Energy II", "Flight Energy III", "Flight Energy IV", "Flight Energy V" }
    };
    // 这一行补充 private static readonly string[,] DisplayNames =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly string[,] DisplayNames =
    {
        // 继续填写上一行调用的参数：{ "Glow", "Anti-Gravity", "Fast Recharge", "Fireworks", "Shrink" }；参数位置决定它表示的坐标、速度或选项。
        { "Glow", "Anti-Gravity", "Fast Recharge", "Fireworks", "Shrink" },
        // 继续填写上一行调用的参数：{ "Rainbow Trail", "Resize Potion", "Chat Table", "Bonfire", "Instant Recharge" }；参数位置决定它表示的坐标、速度或选项。
        { "Rainbow Trail", "Resize Potion", "Chat Table", "Bonfire", "Instant Recharge" },
        // 继续填写上一行调用的参数：{ "Starlight", "Cloud", "Lantern", "Feather", "Moonlight" }；参数位置决定它表示的坐标、速度或选项。
        { "Starlight", "Cloud", "Lantern", "Feather", "Moonlight" },
        // 继续填写上一行调用的参数：{ "Candlelight", "Warmth", "Glitter", "Trail", "Bloom" }；参数位置决定它表示的坐标、速度或选项。
        { "Candlelight", "Warmth", "Glitter", "Trail", "Bloom" },
        // 继续填写上一行调用的参数：{ "Celadon Offering", "Ivory Ritual Jar", "Azure Incense Vessel", "Amber Offering Bowl", "Moon Glaze；参数位置决定它表示的坐标、速度或选项。
        { "Celadon Offering", "Ivory Ritual Jar", "Azure Incense Vessel", "Amber Offering Bowl", "Moon Glaze Vase" },
        // 这一行补充 { "Cloud Pattern Urn", "Temple Tea Vessel", "Gold Rim Pottery", "Calm", "Wish" }，与相邻行合起来完成同一次计算或对象配置。
        { "Cloud Pattern Urn", "Temple Tea Vessel", "Gold Rim Pottery", "Calm", "Wish" }
    };
    // 这一行补充 private static readonly string[] TreeTitles =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly string[] TreeTitles =
    {
        // 继续填写上一行调用的参数："SHIBIYAMA DANCER", "PRAYER GUARDIAN", "COTL TRAVELLER"；参数位置决定它表示的坐标、速度或选项。
        "SHIBIYAMA DANCER", "PRAYER GUARDIAN", "COTL TRAVELLER",
        // 这一行补充 "CANDLE PILGRIM", "SKELETON PILGRIM", "C1 MEMORY"，与相邻行合起来完成同一次计算或对象配置。
        "CANDLE PILGRIM", "SKELETON PILGRIM", "C1 MEMORY"
    };
    // 这一行补充 private static readonly Color[] CubeColors =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly Color[] CubeColors =
    {
        // 用 new 创建 Color(1f, .78f, .3f), new Color(.62f, .8f, 1f), new Color(.42f, 1f, .72f),；构造参数决定新对象的初始内容。
        new Color(1f, .78f, .3f), new Color(.62f, .8f, 1f), new Color(.42f, 1f, .72f),
        // 用 new 创建 Color(1f, .42f, .35f), new Color(.72f, .55f, 1f),；构造参数决定新对象的初始内容。
        new Color(1f, .42f, .35f), new Color(.72f, .55f, 1f),
        // 用 new 创建 Color(1f, .5f, .8f), new Color(.72f, .95f, 1f), new Color(.77f, .58f, .38f),；构造参数决定新对象的初始内容。
        new Color(1f, .5f, .8f), new Color(.72f, .95f, 1f), new Color(.77f, .58f, .38f),
        // 用 new 创建 Color(1f, .55f, .2f), new Color(.95f, .95f, .65f),；构造参数决定新对象的初始内容。
        new Color(1f, .55f, .2f), new Color(.95f, .95f, .65f),
        // 用 new 创建 Color(.72f, .91f, 1f), new Color(.86f, .94f, 1f), new Color(1f, .82f, .48f),；构造参数决定新对象的初始内容。
        new Color(.72f, .91f, 1f), new Color(.86f, .94f, 1f), new Color(1f, .82f, .48f),
        // 用 new 创建 Color(.94f, .88f, .7f), new Color(.72f, .74f, 1f),；构造参数决定新对象的初始内容。
        new Color(.94f, .88f, .7f), new Color(.72f, .74f, 1f),
        // 用 new 创建 Color(1f, .78f, .42f), new Color(1f, .6f, .33f), new Color(.98f, .87f, .54f)；构造参数决定新对象的初始内容。
        new Color(1f, .78f, .42f), new Color(1f, .6f, .33f), new Color(.98f, .87f, .54f),
        // 用 new 创建 Color(.67f, .81f, 1f), new Color(.79f, 1f, .62f),；构造参数决定新对象的初始内容。
        new Color(.67f, .81f, 1f), new Color(.79f, 1f, .62f),
        // 用 new 创建 Color(.65f, .9f, .96f), new Color(.6f, .75f, .91f), new Color(.78f, .89f, .9；构造参数决定新对象的初始内容。
        new Color(.65f, .9f, .96f), new Color(.6f, .75f, .91f), new Color(.78f, .89f, .98f),
        // 用 new 创建 Color(.59f, .61f, .82f), new Color(.95f, .88f, .74f),；构造参数决定新对象的初始内容。
        new Color(.59f, .61f, .82f), new Color(.95f, .88f, .74f),
        // 用 new 创建 Color(1f, .88f, .65f), new Color(.83f, .83f, 1f), new Color(1f, .94f, .51f),；构造参数决定新对象的初始内容。
        new Color(1f, .88f, .65f), new Color(.83f, .83f, 1f), new Color(1f, .94f, .51f),
        // 用 new 创建 Color(.71f, .97f, .88f), new Color(.98f, .75f, .85f)；构造参数决定新对象的初始内容。
        new Color(.71f, .97f, .88f), new Color(.98f, .75f, .85f)
    };
    // 这一行补充 private static readonly Vector2[] NodePositions =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly Vector2[] NodePositions =
    {
        // 用 new 创建 Vector2(0f, -.86f), new Vector2(-.46f, -.43f), new Vector2(0f, 0f),；构造参数决定新对象的初始内容。
        new Vector2(0f, -.86f), new Vector2(-.46f, -.43f), new Vector2(0f, 0f),
        // 用 new 创建 Vector2(.46f, .43f), new Vector2(0f, .86f)；构造参数决定新对象的初始内容。
        new Vector2(.46f, .43f), new Vector2(0f, .86f)
    };
    // 计算 { "✧", "♥", "✧", "✺", "+" }，把结果存进 NodeGlyphs（NodeGlyphs）；之后引用 NodeGlyphs 时用的就是这次结果。
    private static readonly string[] NodeGlyphs = { "✧", "♥", "✧", "✺", "+" };

    // 创建新的 bool[,] 并存为 obtained；构造参数决定其初始内容或对象名称。
    private readonly bool[,] obtained = new bool[6, 5];
    // spiritRenderers 保存spiritRenderers（类型 Renderer[][]）；下方方法依靠它记录或查找场景状态。
    private Renderer[][] spiritRenderers;
    // originalRendererStates 保存originalRendererStates（类型 bool[][]）；下方方法依靠它记录或查找场景状态。
    private bool[][] originalRendererStates;
    // summoned 保存summoned（类型 bool[]）；下方方法依靠它记录或查找场景状态。
    private bool[] summoned;
    // insideSummonRadius 保存insideSummonRadius（类型 bool[]）；下方方法依靠它记录或查找场景状态。
    private bool[] insideSummonRadius;
    // emergence 保存emergence（类型 SkyQuestAncestorEmergence[]）；下方方法依靠它记录或查找场景状态。
    private SkyQuestAncestorEmergence[] emergence;
    // icons 保存icons（类型 GameObject[]）；下方方法依靠它记录或查找场景状态。
    private GameObject[] icons;
    // iconAnchors 保存iconAnchors（类型 Transform[]）；下方方法依靠它记录或查找场景状态。
    private Transform[] iconAnchors;
    // 声明 menuRoot（menuRoot）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject menuRoot;
    // nodes 保存nodes（类型 Transform[]）；下方方法依靠它记录或查找场景状态。
    private Transform[] nodes;
    // labels 保存labels（类型 TextMeshPro[]）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro[] labels;
    // iconGlyphs 保存iconGlyphs（类型 TextMeshPro[]）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro[] iconGlyphs;
    // title 保存物品标题（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro title;
    // prompt 保存prompt（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro prompt;
    // hoverHelp 保存hoverHelp（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro hoverHelp;
    // 声明 hoverHelpPlate（hoverHelpPlate）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject hoverHelpPlate;
    // 声明 pointer（pointer）：可见射线组件引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private LineRenderer pointer;
    // 把数值 -1 暂存为 activeAncestor；它只在当前方法里参与后续判断或计算。
    private int activeAncestor = -1;
    // 把数值 -1 暂存为 selected；它只在当前方法里参与后续判断或计算。
    private int selected = -1;
    // 把数值 -1 暂存为 focused；它只在当前方法里参与后续判断或计算。
    private int focused = -1;
    // 声明 holdStarted（holdStarted）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float holdStarted;
    // 声明 controllerDragStart（controllerDragStart）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 controllerDragStart;
    // 声明 nextToggle（nextToggle）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextToggle;
    // 声明 previousX（previousx）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousX;
    // 声明 previousTrigger（previousTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousTrigger;
    // 计算 { 10, 8, 12, 15, 20 }，把结果存进 candleCosts（candleCosts）；之后引用 candleCosts 时用的就是这次结果。
    private readonly int[] candleCosts = { 10, 8, 12, 15, 20 };
    // heldCube 保存heldCube（类型 SkyQuestMagicPickup）；下方方法依靠它记录或查找场景状态。
    private SkyQuestMagicPickup heldCube;
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

    // IsMenuOpen是 Inspector 中的开关，默认 > menuRoot != null && menuRoot.activeSelf；勾选状态决定相关功能是否运行。
    public bool IsMenuOpen => menuRoot != null && menuRoot.activeSelf;
    // IsNearAny是 Inspector 中的开关，默认 > NearestAncestor() >= 0；勾选状态决定相关功能是否运行。
    public bool IsNearAny => NearestAncestor() >= 0;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：调用 MakeMenu：MakeMenu。
        MakeMenu();
        // 步骤 2：调用 MakePointer：MakePointer。
        MakePointer();
        // 步骤 3：用三元表达式为 count 二选一：问号前条件为真取前值，为假取冒号后的值。
        int count = ancestors == null ? 0 : ancestors.Length;
        // 步骤 4：新建用于spiritRenderers的对象或数值结构，并把构造参数保存进去。
        spiritRenderers = new Renderer[count][];
        // 步骤 5：新建用于originalRendererStates的对象或数值结构，并把构造参数保存进去。
        originalRendererStates = new bool[count][];
        // 步骤 6：新建用于summoned的对象或数值结构，并把构造参数保存进去。
        summoned = new bool[count];
        // 步骤 7：新建用于insideSummonRadius的对象或数值结构，并把构造参数保存进去。
        insideSummonRadius = new bool[count];
        // 步骤 8：新建用于emergence的对象或数值结构，并把构造参数保存进去。
        emergence = new SkyQuestAncestorEmergence[count];
        // 步骤 9：新建用于icons的对象或数值结构，并把构造参数保存进去。
        icons = new GameObject[count];
        // 步骤 10：新建用于iconAnchors的对象或数值结构，并把构造参数保存进去。
        iconAnchors = new Transform[count];
        // 步骤 11：按 int i = 0; i < count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < count; i++)
        {
            // 步骤 12：计算 ancestors[i]，把结果存进 ancestor（ancestor）；之后引用 ancestor 时用的就是这次结果。
            Transform ancestor = ancestors[i];
            // 步骤 13：检查ancestor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (ancestor == null) continue;
            // 步骤 14：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            spiritRenderers[i] = ancestor.GetComponentsInChildren<Renderer>(true);
            // 步骤 15：执行 originalRendererStates[i] = new bool[spiritRenderers[i].Length];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            originalRendererStates[i] = new bool[spiritRenderers[i].Length];
            // 步骤 16：按 int j = 0; j < spiritRenderers[i].Length; j++ 变化索引；循环体依次处理每个位置。
            for (int j = 0; j < spiritRenderers[i].Length; j++)
                // 步骤 17：执行 originalRendererStates[i][j] = spiritRenderers[i][j].enabled;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                originalRendererStates[i][j] = spiritRenderers[i][j].enabled;
            // 步骤 18：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            emergence[i] = ancestor.GetComponent<SkyQuestAncestorEmergence>();
            // 步骤 19：检查对象引用是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
            if (emergence[i] == null)
                // 步骤 20：执行 emergence[i] = ancestor.gameObject.AddComponent<SkyQuestAncestorEmergence>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                emergence[i] = ancestor.gameObject.AddComponent<SkyQuestAncestorEmergence>();
            // 步骤 21：用三元表达式为 stone 二选一：问号前条件为真取前值，为假取冒号后的值。
            Transform stone = memorials != null && i < memorials.Length ? memorials[i] : null;
            // 步骤 22：把括号里的依赖对象交给 emergence[i] 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            emergence[i].Initialize(ancestor, stone);
            // 步骤 23：执行 iconAnchors[i] = FindMemorialIconAnchor(stone);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            iconAnchors[i] = FindMemorialIconAnchor(stone);
            // 步骤 24：创建新的 Transform 并存为 marker；构造参数决定其初始内容或对象名称。
            Transform marker = new GameObject(ancestor.name + " interaction icon").transform;
            // 步骤 25：把position更新为 InteractionIconPosition(i)；世界或局部位置会立即影响物体摆放。
            marker.position = InteractionIconPosition(i);
            // 步骤 26：把计算结果 SkyQuestVisuals.Primitive("Interact", PrimitiveType.Sphere, 保存为orb；下一条判断或调用会读取这个具体值。
            GameObject orb = SkyQuestVisuals.Primitive("Interact", PrimitiveType.Sphere,
                // 步骤 27：执行 marker, Vector3.zero, Vector3.one * .34f, new Color(1f, .52f, .66f), true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                marker, Vector3.zero, Vector3.one * .34f, new Color(1f, .52f, .66f), true);
            // 步骤 28：执行 orb.AddComponent<SkyQuestGalleryIcon>().index = i;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            orb.AddComponent<SkyQuestGalleryIcon>().index = i;
            // 步骤 29：把计算结果 SkyQuestVisuals.Text("Exchange icon glyph", "✦", marker, 保存为openGlyph；下一条判断或调用会读取这个具体值。
            TextMeshPro openGlyph = SkyQuestVisuals.Text("Exchange icon glyph", "✦", marker,
                // 步骤 30：用 new 创建 Vector3(0f, 0f, -.23f), 6.4f, Color.white, 1.3f);；构造参数决定新对象的初始内容。
                new Vector3(0f, 0f, -.23f), 6.4f, Color.white, 1.3f);
            // 步骤 31：把计算结果 FontStyles.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
            openGlyph.fontStyle = FontStyles.Bold;
            // 步骤 32：把 TextWrappingModes.NoWrap 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
            openGlyph.textWrappingMode = TextWrappingModes.NoWrap;
            // 步骤 33：执行 icons[i] = marker.gameObject;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            icons[i] = marker.gameObject;
            // 步骤 34：调用 SetSummoned：设置Summoned。
            SetSummoned(i, false);
        }
        // 步骤 35：调用 RegisterScenePottery：Register场景Pottery。
        RegisterScenePottery();
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：RegisterScenePottery——Register场景Pottery！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RegisterScenePottery()
    {
        // The eight A242 vessels in the temple were scenery only. Give them
        // the same single-item carry interaction as exchanged rewards while
        // keeping them anchored on their plinths until first picked up.
        // 步骤 1：逐个读取 Transform vessel in FindObjectsByType<Transform>(FindObjectsSortMode.None) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform vessel in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            // 步骤 2：这里的 ! 表示取反；当 vessel.name.StartsWith("A242 ceramic display ", StringComparison.Ordinal) 不成立时才进入此分支。 成立时执行 continue;
            if (!vessel.name.StartsWith("A242 ceramic display ", StringComparison.Ordinal)) continue;
            // 步骤 3：从目标物体找到所需组件并存为 pickup；使用前应检查是否为 null。
            SkyQuestMagicPickup pickup = vessel.GetComponent<SkyQuestMagicPickup>();
            // 步骤 4：如果场景对象没有 SkyQuestMagicPickup 组件，就当场创建并保存，保证后续调用有对象可用。
            if (pickup == null) pickup = vessel.gameObject.AddComponent<SkyQuestMagicPickup>();
            // 步骤 5：把计算结果 head 保存为desktopHead；下一条判断或调用会读取这个具体值。
            pickup.desktopHead = head;
            // 步骤 6：把计算结果 rightHand 保存为desktopHand；下一条判断或调用会读取这个具体值。
            pickup.desktopHand = rightHand;
            // 步骤 7：把计算结果 avatar 保存为desktopAvatar；下一条判断或调用会读取这个具体值。
            pickup.desktopAvatar = avatar;
            // 步骤 8：把计算结果 desktopMode 保存为desktop模式；下一条判断或调用会读取这个具体值。
            pickup.desktopMode = desktopMode;
            // 步骤 9：调用 AnchorOnDisplay：AnchorOnDisplay。
            pickup.AnchorOnDisplay();
        }
    }
    // ！！！功能结束：RegisterScenePottery！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    // 原理：每帧更新交互距离、树菜单和输入状态，确保拖拽兑换时角色不会误跳。
    void Update()
    {
        // 步骤 1：检查头显相机、左手柄位置、右手柄位置是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (head == null || leftHand == null || rightHand == null) return;
        // 步骤 2：调用 UpdateSummons：更新Summons。
        UpdateSummons();
        // 步骤 3：调用 UpdateIcons：更新Icons。
        UpdateIcons();
        // 步骤 4：这里用 && 要求两边条件同时成立：IsMenuOpen && (activeAncestor < 0 || !IsNear(activeAncestor))；任意一边为假就跳过本分支。 成立时调用 CloseMenu：关闭兑换树并恢复正常角色控制。
        if (IsMenuOpen && (activeAncestor < 0 || !IsNear(activeAncestor))) CloseMenu();
        // 步骤 5：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 6：把计算结果 SkyQuestMagicPickup.Held 保存为heldCube；下一条判断或调用会读取这个具体值。
            heldCube = SkyQuestMagicPickup.Held;
            // 步骤 7：调用 UpdateDesktopHover：检查桌面鼠标悬停的兑换节点并更新提示。
            UpdateDesktopHover();
            // 步骤 8：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 9：用 x 记住此处真假结果（SkyQuestInput.Primary(SkyQuestInput.Left)），供下一条 if 选择执行路径。
        bool x = SkyQuestInput.Primary(SkyQuestInput.Left);
        // 步骤 10：用 legacyModifier 记住此处真假结果（SkyQuestInput.Grip(SkyQuestInput.Left) > .65f），供下一条 if 选择执行路径。
        bool legacyModifier = SkyQuestInput.Grip(SkyQuestInput.Left) > .65f;
        // 步骤 11：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (x && !previousX && !legacyModifier &&
            // 步骤 12：这一行补充 (legacyTree == null || !legacyTree.IsMenuOpen) && Time.time >= nextToggle)，与相邻行合起来完成同一次计算或对象配置。
            (legacyTree == null || !legacyTree.IsMenuOpen) && Time.time >= nextToggle)
            // 步骤 13：调用 ToggleNearest：ToggleNearest。
            ToggleNearest();
        // 步骤 14：把计算结果 x 保存为previousx；下一条判断或调用会读取这个具体值。
        previousX = x;
        // 步骤 15：这里的 ! 表示取反；当 IsMenuOpen 不成立时才进入此分支。
        if (!IsMenuOpen)
        {
            // 步骤 16：确认pointer已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 pointer.enabled = false;
            if (pointer != null) pointer.enabled = false;
            // 步骤 17：用 iconTrigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > .65f），供下一条 if 选择执行路径。
            bool iconTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .65f;
            // 步骤 18：这里用 && 要求两边条件同时成立：iconTrigger && !previousTrigger；任意一边为假就跳过本分支。
            if (iconTrigger && !previousTrigger)
            {
                // 步骤 19：逐个读取 RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 5f) 中的元素；每轮处理当前一个，直到全部遍历完。
                foreach (RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 5f))
                {
                    // 步骤 20：从目标物体找到所需组件并存为 icon；使用前应检查是否为 null。
                    SkyQuestGalleryIcon icon = hit.collider.GetComponent<SkyQuestGalleryIcon>();
                    // 步骤 21：检查icon是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
                    if (icon == null || !IsNear(icon.index)) continue;
                    // 步骤 22：调用 OpenMenu：打开先祖兑换树并解锁相关鼠标或手柄输入。
                    OpenMenu(icon.index);
                    // 步骤 23：结束当前 switch 分支或循环，不继续落入后面的处理。
                    break;
                }
            }
            // 步骤 24：把计算结果 iconTrigger 保存为previousTrigger；下一条判断或调用会读取这个具体值。
            previousTrigger = iconTrigger;
            // 步骤 25：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 26：调用 UpdatePointer：更新Pointer。
        UpdatePointer();
        // 步骤 27：检查条件 focused != focusedBefore；只有为真才运行下面的步骤，否则沿原流程继续。
        if (focused != focusedBefore)
        {
            // 步骤 28：把计算结果 focused 保存为focusedBefore；下一条判断或调用会读取这个具体值。
            focusedBefore = focused;
            // 步骤 29：把计算结果 Time.unscaledTime 保存为focusedSince；下一条判断或调用会读取这个具体值。
            focusedSince = Time.unscaledTime;
        }
        // 步骤 30：确认hoverHelp已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (hoverHelp != null)
        {
            // 步骤 31：用 show 记住此处真假结果（focused >= 0），供下一条 if 选择执行路径。
            bool show = focused >= 0;
            // 步骤 32：把新内容写入 hoverHelp.text；玩家看到的文字会在本帧更新。
            hoverHelp.text = show ? RewardDisplayName(activeAncestor, focused) + "\n" +
                // 步骤 33：调用 RewardTooltip：RewardTooltip。
                RewardTooltip(activeAncestor, desktopMode) : "";
            // 步骤 34：确认hoverHelpPlate已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
            if (hoverHelpPlate != null) hoverHelpPlate.SetActive(show);
            // 步骤 35：检查条件 show；只有为真才运行下面的步骤，否则沿原流程继续。
            if (show)
            {
                // 步骤 36：这一行补充 hoverHelp.transform.localPosition = new Vector3(，与相邻行合起来完成同一次计算或对象配置。
                hoverHelp.transform.localPosition = new Vector3(
                    // 步骤 37：执行 NodePositions[focused].x, NodePositions[focused].y + .27f, -.3f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    NodePositions[focused].x, NodePositions[focused].y + .27f, -.3f);
                // 步骤 38：这一行补充 hoverHelpPlate.transform.localPosition = new Vector3(，与相邻行合起来完成同一次计算或对象配置。
                hoverHelpPlate.transform.localPosition = new Vector3(
                    // 步骤 39：执行 NodePositions[focused].x, NodePositions[focused].y + .27f, -.27f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    NodePositions[focused].x, NodePositions[focused].y + .27f, -.27f);
            }
        }
        // 步骤 40：用 trigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > .65f），供下一条 if 选择执行路径。
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .65f;
        // 步骤 41：这里用 && 要求两边条件同时成立：trigger && !previousTrigger && focused >= 0 && !obtained[activeAncestor, focused]；任意一边为假就跳过本分支。
        if (trigger && !previousTrigger && focused >= 0 && !obtained[activeAncestor, focused])
        {
            // 步骤 42：把计算结果 focused 保存为selected；下一条判断或调用会读取这个具体值。
            selected = focused;
            // 步骤 43：根据当前游戏时间计算并保存holdStarted，用于后续延迟或冷却判断。
            holdStarted = Time.time;
            // 步骤 44：把计算结果 rightHand.position 保存为controllerDragStart；下一条判断或调用会读取这个具体值。
            controllerDragStart = rightHand.position;
            // 步骤 45：执行 status?.Show(SpellNames[activeAncestor, selected] + " selected. Hold or pull outward to exchange.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show(SpellNames[activeAncestor, selected] + " selected. Hold or pull outward to exchange.");
        }
        // 步骤 46：把计算结果 selected >= 0 && 保存为pulled从Icon；下一条判断或调用会读取这个具体值。
        bool pulledFromIcon = selected >= 0 &&
            // 步骤 47：调用 Distance：Distance。
            Vector3.Distance(rightHand.position, controllerDragStart) >= .14f;
        // 步骤 48：把计算结果 selected >= 0 && focused == selected && 保存为heldOnIcon；下一条判断或调用会读取这个具体值。
        bool heldOnIcon = selected >= 0 && focused == selected &&
            // 步骤 49：执行 Time.time - holdStarted >= holdDuration;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Time.time - holdStarted >= holdDuration;
        // 步骤 50：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (trigger && selected >= 0 && (pulledFromIcon || heldOnIcon) &&
            // 步骤 51：这一行补充 !obtained[activeAncestor, selected])，与相邻行合起来完成同一次计算或对象配置。
            !obtained[activeAncestor, selected])
        {
            // 步骤 52：调用 Obtain：兑换成功后创建奖励物并登记拥有状态。
            Obtain(selected);
            // 步骤 53：把计算结果 -1 保存为selected；下一条判断或调用会读取这个具体值。
            selected = -1;
        }
        // 步骤 54：这里的 ! 表示取反；当 trigger 不成立时才进入此分支。 成立时执行 selected = -1;
        if (!trigger) selected = -1;
        // 步骤 55：把计算结果 trigger 保存为previousTrigger；下一条判断或调用会读取这个具体值。
        previousTrigger = trigger;
        // 步骤 56：调用 RefreshNodes：刷新Nodes。
        RefreshNodes();
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：UpdateSummons——更新Summons！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateSummons()
    {
        // 步骤 1：检查summoned、insideSummonRadius是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (summoned == null || insideSummonRadius == null) return;
        // 步骤 2：计算 PlayerPosition，把结果存进 player（player）；之后引用 player 时用的就是这次结果。
        Vector3 player = PlayerPosition;
        // 步骤 3：按 int i = 0; i < summoned.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < summoned.Length; i++)
        {
            // 步骤 4：检查对象引用是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (ancestors[i] == null) continue;
            // 步骤 5：把计算结果 memorials != null && i < memorials.Length && memorials[i] != null ? 保存为center；下一条判断或调用会读取这个具体值。
            Vector3 center = memorials != null && i < memorials.Length && memorials[i] != null ?
                // 步骤 6：执行 memorials[i].position : ancestors[i].position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                memorials[i].position : ancestors[i].position;
            // 步骤 7：把原三维朝向投影到地面平面并存为 distance；行走或转向不受头部俯仰影响。
            float distance = Vector3.ProjectOnPlane(player - center, Vector3.up).magnitude;
            // 步骤 8：用 isInside 记住此处真假结果（distance <= summonRadius），供下一条 if 选择执行路径。
            bool isInside = distance <= summonRadius;
            // 步骤 9：这里用 && 要求两边条件同时成立：isInside && !insideSummonRadius[i]；任意一边为假就跳过本分支。
            if (isInside && !insideSummonRadius[i])
            {
                // 步骤 10：执行 insideSummonRadius[i] = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                insideSummonRadius[i] = true;
                // 步骤 11：这里的 ! 表示取反；当 summoned[i] 不成立时才进入此分支。 成立时调用 SetSummoned：设置Summoned。
                if (!summoned[i]) SetSummoned(i, true);
                // 步骤 12：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查emergence是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
                else if (emergence != null && i < emergence.Length && emergence[i] != null)
                    // 步骤 13：执行 emergence[i].PlayReveal();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    emergence[i].PlayReveal();
            }
            // 步骤 14：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：!isInside && distance > summonRadius + .25f；任意一边为假就跳过本分支。
            else if (!isInside && distance > summonRadius + .25f)
                // 步骤 15：执行 insideSummonRadius[i] = false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                insideSummonRadius[i] = false;

            // 步骤 16：这里用 && 要求两边条件同时成立：summoned[i] && distance >= dismissRadius；任意一边为假就跳过本分支。 成立时调用 SetSummoned：设置Summoned。
            if (summoned[i] && distance >= dismissRadius) SetSummoned(i, false);
        }
    }
    // ！！！功能结束：UpdateSummons！！！

    // ！！！功能：SetSummoned——设置Summoned！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index, bool visible 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetSummoned(int index, bool visible)
    {
        // 步骤 1：检查summoned是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (summoned == null || index < 0 || index >= summoned.Length) return;
        // 步骤 2：执行 summoned[index] = visible;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        summoned[index] = visible;
        // 步骤 3：把计算结果 spiritRenderers != null && index < spiritRenderers.Length ? 保存为renderers；下一条判断或调用会读取这个具体值。
        Renderer[] renderers = spiritRenderers != null && index < spiritRenderers.Length ?
            // 步骤 4：执行 spiritRenderers[index] : null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            spiritRenderers[index] : null;
        // 步骤 5：把计算结果 originalRendererStates != null && index < originalRendererStates.Length ? 保存为rendererStates；下一条判断或调用会读取这个具体值。
        bool[] rendererStates = originalRendererStates != null && index < originalRendererStates.Length ?
            // 步骤 6：执行 originalRendererStates[index] : null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            originalRendererStates[index] : null;
        // 步骤 7：检查emergence是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (emergence != null && index < emergence.Length && emergence[index] != null)
        {
            // 步骤 8：检查条件 visible；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 emergence[index].PlayReveal();
            if (visible) emergence[index].PlayReveal();
            // 步骤 9：前面的 if 条件为假时执行这里的备用处理。
            else emergence[index].PlayDismiss();
        }
        // 步骤 10：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查renderers、rendererStates是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        else if (renderers != null && rendererStates != null)
        {
            // 步骤 11：按 int j = 0; j < renderers.Length && j < rendererStates.Length; j++ 变化索引；循环体依次处理每个位置。
            for (int j = 0; j < renderers.Length && j < rendererStates.Length; j++)
                // 步骤 12：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。
                if (renderers[j] != null)
                    // 步骤 13：执行 renderers[j].enabled = visible && rendererStates[j];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    renderers[j].enabled = visible && rendererStates[j];
        }
        // 步骤 14：这里用 && 要求两边条件同时成立：!visible && activeAncestor == index && IsMenuOpen；任意一边为假就跳过本分支。 成立时调用 CloseMenu：关闭兑换树并恢复正常角色控制。
        if (!visible && activeAncestor == index && IsMenuOpen) CloseMenu();
        // 步骤 15：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 icons[index].SetActive(false);
        if (icons[index] != null) icons[index].SetActive(false);
    }
    // ！！！功能结束：SetSummoned！！！

    // ！！！功能：UpdateIcons——更新Icons！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateIcons()
    {
        // 步骤 1：检查icons是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (icons == null) return;
        // 步骤 2：按 int i = 0; i < icons.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < icons.Length; i++)
        {
            // 步骤 3：检查对象引用是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (icons[i] == null || ancestors[i] == null) continue;
            // 步骤 4：把计算结果 IsNear(i) && !IsMenuOpen && 保存为show；下一条判断或调用会读取这个具体值。
            bool show = IsNear(i) && !IsMenuOpen &&
                // 步骤 5：执行 (legacyTree == null || !legacyTree.IsMenuOpen);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                (legacyTree == null || !legacyTree.IsMenuOpen);
            // 步骤 6：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            icons[i].SetActive(show);
            // 步骤 7：这里的 ! 表示取反；当 show 不成立时才进入此分支。 成立时执行 continue;
            if (!show) continue;
            // 步骤 8：执行 icons[i].transform.position = InteractionIconPosition(i);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            icons[i].transform.position = InteractionIconPosition(i);
            // 步骤 9：执行 icons[i].transform.rotation = Quaternion.LookRotation(icons[i].transform.position - head.position);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            icons[i].transform.rotation = Quaternion.LookRotation(icons[i].transform.position - head.position);
        }
    }
    // ！！！功能结束：UpdateIcons！！！

    // ！！！功能：InteractionIconPosition——InteractionIcon位置！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private Vector3 InteractionIconPosition(int index)
    {
        // 步骤 1：检查iconAnchors是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (iconAnchors != null && index >= 0 && index < iconAnchors.Length && iconAnchors[index] != null)
            // 步骤 2：把 iconAnchors[index].position 作为方法结果返回给调用方；return 后本方法立即结束。
            return iconAnchors[index].position;
        // 步骤 3：用三元表达式为 stone 二选一：问号前条件为真取前值，为假取冒号后的值。
        Transform stone = memorials != null && index >= 0 && index < memorials.Length ? memorials[index] : null;
        // 步骤 4：确认stone已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (stone != null) return stone.position + Vector3.up * 1.35f;
        // 步骤 5：把 ancestors[index].position + Vector3.up * 2.1f 作为方法结果返回给调用方；return 后本方法立即结束。
        return ancestors[index].position + Vector3.up * 2.1f;
    }
    // ！！！功能结束：InteractionIconPosition！！！

    // ！！！功能：FindMemorialIconAnchor——查找MemorialIconAnchor！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform stone 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Transform FindMemorialIconAnchor(Transform stone)
    {
        // 步骤 1：检查stone是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (stone == null) return null;
        // 步骤 2：计算 null，把结果存进 fallback（fallback）；之后引用 fallback 时用的就是这次结果。
        Transform fallback = null;
        // 步骤 3：逐个读取 Transform child in stone.GetComponentsInChildren<Transform>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform child in stone.GetComponentsInChildren<Transform>(true))
        {
            // 步骤 4：计算 child.name.ToLowerInvariant()，把结果存进 childName（childName）；之后引用 childName 时用的就是这次结果。
            string childName = child.name.ToLowerInvariant();
            // 步骤 5：检查条件 childName == "memorial star"；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
            if (childName == "memorial star") return child;
            // 步骤 6：检查fallback是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (fallback == null && (childName.Contains("star") || childName.Contains("symbol")))
                // 步骤 7：把计算结果 child 保存为fallback；下一条判断或调用会读取这个具体值。
                fallback = child;
        }
        // 步骤 8：把 fallback 作为方法结果返回给调用方；return 后本方法立即结束。
        return fallback;
    }
    // ！！！功能结束：FindMemorialIconAnchor！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnGUI()
    {
        // 步骤 1：检查头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!desktopMode || head == null) return;
        // 步骤 2：从目标物体找到所需组件并存为 camera；使用前应检查是否为 null。
        Camera camera = head.GetComponent<Camera>();
        // 步骤 3：检查camera、icons是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (camera != null && icons != null && !IsMenuOpen)
        {
            // 步骤 4：按 int i = 0; i < icons.Length; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < icons.Length; i++)
            {
                // 步骤 5：检查对象引用是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
                if (!IsNear(i) || icons[i] == null || !icons[i].activeSelf) continue;
                // 步骤 6：计算 camera.WorldToScreenPoint(icons[i].transform.position)，把结果存进 point（point）；之后引用 point 时用的就是这次结果。
                Vector3 point = camera.WorldToScreenPoint(icons[i].transform.position);
                // 步骤 7：检查条件 point.z <= 0f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
                if (point.z <= 0f) continue;
                // 步骤 8：计算 x 并限制在安全上下界，防止不合理的大小影响移动或显示。
                float x = Mathf.Clamp(point.x - 28f, 10f, Screen.width - 66f);
                // 步骤 9：计算 y 并限制在安全上下界，防止不合理的大小影响移动或显示。
                float y = Mathf.Clamp(Screen.height - point.y - 28f, 10f, Screen.height - 66f);
                // 步骤 10：新建用于iconHaloStyle的对象或数值结构，并把构造参数保存进去。
                GUIStyle iconHaloStyle = new GUIStyle(GUI.skin.label) { fontSize = 50,
                    // 步骤 11：把计算结果 FontStyle.Bold, alignment = TextAnchor.MiddleCenter } 保存为fontStyle；下一条判断或调用会读取这个具体值。
                    fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                // 步骤 12：执行 iconHaloStyle.normal.textColor = new Color(1f, .58f, .7f, .78f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                iconHaloStyle.normal.textColor = new Color(1f, .58f, .7f, .78f);
                // 步骤 13：调用 Label：在指定 GUI 矩形区域显示说明文字。
                GUI.Label(new Rect(x, y, 56f, 56f), "●", iconHaloStyle);
                // 步骤 14：新建用于iconStyle的对象或数值结构，并把构造参数保存进去。
                GUIStyle iconStyle = new GUIStyle(GUI.skin.label) { fontSize = 30,
                    // 步骤 15：把计算结果 FontStyle.Bold, alignment = TextAnchor.MiddleCenter } 保存为fontStyle；下一条判断或调用会读取这个具体值。
                    fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                // 步骤 16：执行 iconStyle.normal.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                iconStyle.normal.textColor = Color.white;
                // 步骤 17：检查条件 GUI.Button(new Rect(x, y, 56f, 56f), "✦", iconStyle)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 OpenMenu：打开先祖兑换树并解锁相关鼠标或手柄输入。
                if (GUI.Button(new Rect(x, y, 56f, 56f), "✦", iconStyle)) OpenMenu(i);
                // 步骤 18：创建新的 GUIStyle 并存为 iconNameStyle；构造参数决定其初始内容或对象名称。
                GUIStyle iconNameStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                // 步骤 19：执行 iconNameStyle.normal.textColor = new Color(1f, .9f, .7f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                iconNameStyle.normal.textColor = new Color(1f, .9f, .7f);
                // 步骤 20：调用 Label：在指定 GUI 矩形区域显示说明文字。
                GUI.Label(new Rect(x - 76f, y - 26f, 208f, 24f), TreeTitles[i], iconNameStyle);
            }
        }
        // 步骤 21：这里用 || 表示任一条件成立就处理：!IsMenuOpen || activeAncestor < 0 || activeAncestor >= obtained.GetLength(0)；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!IsMenuOpen || activeAncestor < 0 || activeAncestor >= obtained.GetLength(0)) return;
        // 步骤 22：计算 width 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float width = Mathf.Clamp(Screen.width * .36f, 340f, 560f);
        // 步骤 23：计算 Screen.width - width，把结果存进 left（left）；之后引用 left 时用的就是这次结果。
        float left = Screen.width - width;
        // 步骤 24：把数值 108f 暂存为 top；它只在当前方法里参与后续判断或计算。
        float top = 108f;
        // 步骤 25：计算 Screen.height - top，把结果存进 height（height）；之后引用 height 时用的就是这次结果。
        float height = Screen.height - top;
        // 步骤 26：创建新的 Rect 并存为 panel；构造参数决定其初始内容或对象名称。
        Rect panel = new Rect(left, top, width, height);
        // 步骤 27：计算 GUI.color，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        Color previous = GUI.color;
        // 步骤 28：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = new Color(.06f, .13f, .2f, .85f);
        // 步骤 29：调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        // 步骤 30：把计算结果 previous 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = previous;
        // 步骤 31：创建新的 GUIStyle 并存为 titleStyle；构造参数决定其初始内容或对象名称。
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        // 步骤 32：把计算结果 TextAnchor.MiddleCenter 保存为alignment；下一条判断或调用会读取这个具体值。
        titleStyle.alignment = TextAnchor.MiddleCenter;
        // 步骤 33：把计算结果 23 保存为fontSize；下一条判断或调用会读取这个具体值。
        titleStyle.fontSize = 23;
        // 步骤 34：把计算结果 FontStyle.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
        titleStyle.fontStyle = FontStyle.Bold;
        // 步骤 35：执行 titleStyle.normal.textColor = new Color(1f, .87f, .62f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        titleStyle.normal.textColor = new Color(1f, .87f, .62f);
        // 步骤 36：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(left + 18f, top + 14f, width - 72f, 38f),
            // 步骤 37：执行 TreeTitles[activeAncestor], titleStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TreeTitles[activeAncestor], titleStyle);
        // 步骤 38：创建新的 GUIStyle 并存为 candleStyle；构造参数决定其初始内容或对象名称。
        GUIStyle candleStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, alignment = TextAnchor.MiddleLeft };
        // 步骤 39：执行 candleStyle.normal.textColor = new Color(1f, .88f, .62f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        candleStyle.normal.textColor = new Color(1f, .88f, .62f);
        // 步骤 40：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(left + 22f, top + 52f, width - 44f, 28f), "Candles: " + SkyQuestCandleWallet.Balance, candleStyle);
        // 步骤 41：检查条件 GUI.Button(new Rect(Screen.width - 52f, top + 16f, 36f, 36f), "X")；只有为真才运行下面的步骤，否则沿原流程继续。
        if (GUI.Button(new Rect(Screen.width - 52f, top + 16f, 36f, 36f), "X"))
        {
            // 步骤 42：调用 CloseMenu：关闭兑换树并恢复正常角色控制。
            CloseMenu();
            // 步骤 43：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 44：计算 top + 88f，把结果存进 treeTop（treeTop）；之后引用 treeTop 时用的就是这次结果。
        float treeTop = top + 88f;
        // 步骤 45：计算 Mathf.Max(360f, height - 210f)，把结果存进 treeHeight（treeHeight）；之后引用 treeHeight 时用的就是这次结果。
        float treeHeight = Mathf.Max(360f, height - 210f);
        // 步骤 46：计算 { .50f, .34f, .50f, .66f, .50f }，把结果存进 px（px）；之后引用 px 时用的就是这次结果。
        float[] px = { .50f, .34f, .50f, .66f, .50f };
        // 步骤 47：计算 { .88f, .70f, .52f, .34f, .16f }，把结果存进 py（py）；之后引用 py 时用的就是这次结果。
        float[] py = { .88f, .70f, .52f, .34f, .16f };
        // 步骤 48：把计算结果 { 保存为points；下一条判断或调用会读取这个具体值。
        Vector2[] points = {
            // 步骤 49：用 new 创建 Vector2(left + width * px[0], treeTop + treeHeight * py[0]),；构造参数决定新对象的初始内容。
            new Vector2(left + width * px[0], treeTop + treeHeight * py[0]),
            // 步骤 50：用 new 创建 Vector2(left + width * px[1], treeTop + treeHeight * py[1]),；构造参数决定新对象的初始内容。
            new Vector2(left + width * px[1], treeTop + treeHeight * py[1]),
            // 步骤 51：用 new 创建 Vector2(left + width * px[2], treeTop + treeHeight * py[2]),；构造参数决定新对象的初始内容。
            new Vector2(left + width * px[2], treeTop + treeHeight * py[2]),
            // 步骤 52：用 new 创建 Vector2(left + width * px[3], treeTop + treeHeight * py[3]),；构造参数决定新对象的初始内容。
            new Vector2(left + width * px[3], treeTop + treeHeight * py[3]),
            // 步骤 53：用 new 创建 Vector2(left + width * px[4], treeTop + treeHeight * py[4])；构造参数决定新对象的初始内容。
            new Vector2(left + width * px[4], treeTop + treeHeight * py[4])
        };
        // 步骤 54：按 int i = 1; i < points.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 1; i < points.Length; i++) DrawGuiBranch(points[i - 1], points[i]);
        // 步骤 55：新建用于glyphStyle的对象或数值结构，并把构造参数保存进去。
        GUIStyle glyphStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
            // 步骤 56：把计算结果 34, fontStyle = FontStyle.Bold } 保存为fontSize；下一条判断或调用会读取这个具体值。
            fontSize = 34, fontStyle = FontStyle.Bold };
        // 步骤 57：执行 glyphStyle.normal.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        glyphStyle.normal.textColor = Color.white;
        // 步骤 58：新建用于nameStyle的对象或数值结构，并把构造参数保存进去。
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft,
            // 步骤 59：把计算结果 16, fontStyle = FontStyle.Bold, wordWrap = false, 保存为fontSize；下一条判断或调用会读取这个具体值。
            fontSize = 16, fontStyle = FontStyle.Bold, wordWrap = false,
            // 步骤 60：把计算结果 TextClipping.Overflow } 保存为clipping；下一条判断或调用会读取这个具体值。
            clipping = TextClipping.Overflow };
        // 步骤 61：新建用于costStyle的对象或数值结构，并把构造参数保存进去。
        GUIStyle costStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft,
            // 步骤 62：把计算结果 14, wordWrap = false, clipping = TextClipping.Overflow } 保存为fontSize；下一条判断或调用会读取这个具体值。
            fontSize = 14, wordWrap = false, clipping = TextClipping.Overflow };
        // 步骤 63：计算 Event.current，把结果存进 guiEvent（guiEvent）；之后引用 guiEvent 时用的就是这次结果。
        Event guiEvent = Event.current;
        // 步骤 64：按 int i = 0; i < points.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < points.Length; i++)
        {
            // 步骤 65：用 claimed 记住此处真假结果（obtained[activeAncestor, i]），供下一条 if 选择执行路径。
            bool claimed = obtained[activeAncestor, i];
            // 步骤 66：用 locked 记住此处真假结果（i > 0 && !obtained[activeAncestor, i - 1]），供下一条 if 选择执行路径。
            bool locked = i > 0 && !obtained[activeAncestor, i - 1];
            // 步骤 67：创建新的 Rect 并存为 iconRect；构造参数决定其初始内容或对象名称。
            Rect iconRect = new Rect(points[i].x - 32f, points[i].y - 32f, 64f, 64f);
            // 步骤 68：把计算结果 IsEnergyReward(activeAncestor) ? CubeColors[activeAncestor * 5 + i] : 保存为baseIconColor；下一条判断或调用会读取这个具体值。
            Color baseIconColor = IsEnergyReward(activeAncestor) ? CubeColors[activeAncestor * 5 + i] :
                // 步骤 69：用 new 创建 Color(1f, .64f, .74f);；构造参数决定新对象的初始内容。
                new Color(1f, .64f, .74f);
            // 步骤 70：这一行补充 glyphStyle.normal.textColor = claimed ? new Color(.55f, 1f, .8f) :，与相邻行合起来完成同一次计算或对象配置。
            glyphStyle.normal.textColor = claimed ? new Color(.55f, 1f, .8f) :
                // 步骤 71：这一行补充 locked ? new Color(1f, 1f, 1f, .62f) :，与相邻行合起来完成同一次计算或对象配置。
                locked ? new Color(1f, 1f, 1f, .62f) :
                // 步骤 72：新建用于selected的对象或数值结构，并把构造参数保存进去。
                selected == i ? new Color(1f, .82f, .42f) : baseIconColor;
            // 步骤 73：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(iconRect, IsEnergyReward(activeAncestor) ? "◆" : NodeGlyphs[i], glyphStyle);
            // 步骤 74：用 labelOnRight 记住此处真假结果（i % 2 == 0），供下一条 if 选择执行路径。
            bool labelOnRight = i % 2 == 0;
            // 步骤 75：用三元表达式为 labelX 二选一：问号前条件为真取前值，为假取冒号后的值。
            float labelX = labelOnRight ? points[i].x + 39f : points[i].x - 183f;
            // 步骤 76：把计算结果 labelOnRight ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight 保存为alignment；下一条判断或调用会读取这个具体值。
            nameStyle.alignment = labelOnRight ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            // 步骤 77：把计算结果 nameStyle.alignment 保存为alignment；下一条判断或调用会读取这个具体值。
            costStyle.alignment = nameStyle.alignment;
            // 步骤 78：这一行补充 nameStyle.normal.textColor = claimed ? new Color(.55f, 1f, .8f) :，与相邻行合起来完成同一次计算或对象配置。
            nameStyle.normal.textColor = claimed ? new Color(.55f, 1f, .8f) :
                // 步骤 79：执行 locked ? new Color(.62f, .65f, .7f) : Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                locked ? new Color(.62f, .65f, .7f) : Color.white;
            // 步骤 80：执行 costStyle.normal.textColor = new Color(1f, .78f, .68f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            costStyle.normal.textColor = new Color(1f, .78f, .68f);
            // 步骤 81：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(new Rect(labelX, points[i].y - 19f, 180f, 23f),
                // 步骤 82：执行 (i + 1) + "  " + RewardDisplayName(activeAncestor, i), nameStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                (i + 1) + "  " + RewardDisplayName(activeAncestor, i), nameStyle);
            // 步骤 83：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(new Rect(labelX, points[i].y + 3f, 180f, 20f),
                // 步骤 84：执行 claimed ? "Obtained" : locked ? "Locked" : candleCosts[i] + " candles · drag", costStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                claimed ? "Obtained" : locked ? "Locked" : candleCosts[i] + " candles · drag", costStyle);
            // 步骤 85：调用 HandleGuiNodeDrag：跟踪兑换树图标的鼠标拖动并在合适位置生成奖励。
            HandleGuiNodeDrag(guiEvent, i, iconRect);
        }
        // 步骤 86：检查条件 hoveredNode >= 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if (hoveredNode >= 0)
        {
            // 步骤 87：定义不可在运行时改变的数值 tooltipWidth（tooltipWidth）；后续计算统一使用这个阈值、速度或尺寸。
            const float tooltipWidth = 310f;
            // 步骤 88：新建用于helpRect的对象或数值结构，并把构造参数保存进去。
            Rect helpRect = new Rect(
                // 步骤 89：调用 Clamp：Clamp。
                Mathf.Clamp(points[hoveredNode].x - tooltipWidth * .5f,
                    // 步骤 90：继续填写上一行调用的参数：left + 12f, Screen.width - tooltipWidth - 12f)；参数位置决定它表示的坐标、速度或选项。
                    left + 12f, Screen.width - tooltipWidth - 12f),
                // 步骤 91：调用 Clamp：Clamp。
                Mathf.Clamp(points[hoveredNode].y - 105f, top + 86f, Screen.height - 160f),
                // 步骤 92：执行 tooltipWidth, 76f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                tooltipWidth, 76f);
            // 步骤 93：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = new Color(1f, .9f, .12f, .95f);
            // 步骤 94：调用 DrawTexture：Draw贴图。
            GUI.DrawTexture(helpRect, Texture2D.whiteTexture);
            // 步骤 95：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
            GUI.color = Color.white;
            // 步骤 96：新建用于helpStyle的对象或数值结构，并把构造参数保存进去。
            GUIStyle helpStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
                // 步骤 97：把计算结果 15, fontStyle = FontStyle.Bold, wordWrap = true } 保存为fontSize；下一条判断或调用会读取这个具体值。
                fontSize = 15, fontStyle = FontStyle.Bold, wordWrap = true };
            // 步骤 98：执行 helpStyle.normal.textColor = Color.black;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            helpStyle.normal.textColor = Color.black;
            // 步骤 99：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(helpRect, RewardDisplayName(activeAncestor, hoveredNode) + "\n" +
                // 步骤 100：调用 RewardTooltip：RewardTooltip。
                RewardTooltip(activeAncestor, desktopMode), helpStyle);
        }
        // 步骤 101：这里用 && 要求两边条件同时成立：guiDragIndex >= 0 && !guiDragTriggered；任意一边为假就跳过本分支。
        if (guiDragIndex >= 0 && !guiDragTriggered)
        {
            // 步骤 102：创建新的 GUIStyle 并存为 dragStyle；构造参数决定其初始内容或对象名称。
            GUIStyle dragStyle = new GUIStyle(glyphStyle) { fontSize = 38 };
            // 步骤 103：执行 dragStyle.normal.textColor = new Color(1f, .78f, .86f, .9f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            dragStyle.normal.textColor = new Color(1f, .78f, .86f, .9f);
            // 步骤 104：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(new Rect(guiEvent.mousePosition.x - 28f, guiEvent.mousePosition.y - 28f, 56f, 56f),
                // 步骤 105：执行 "◆", dragStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "◆", dragStyle);
        }
        // 步骤 106：新建用于footerStyle的对象或数值结构，并把构造参数保存进去。
        GUIStyle footerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
            // 步骤 107：把计算结果 14 } 保存为fontSize；下一条判断或调用会读取这个具体值。
            fontSize = 14 };
        // 步骤 108：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(left + 22f, Screen.height - 90f, width - 44f, 32f),
            // 步骤 109：执行 "Drag an unlocked icon to exchange  ·  F close", footerStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Drag an unlocked icon to exchange  ·  F close", footerStyle);
        // 步骤 110：把计算结果 previous 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = previous;
    }
    // ！！！功能结束：OnGUI！！！

    // ！！！功能：DrawGuiBranch——DrawGuiBranch！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector2 from, Vector2 to 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void DrawGuiBranch(Vector2 from, Vector2 to)
    {
        // 步骤 1：计算 GUI.matrix，把结果存进 previousMatrix（previousMatrix）；之后引用 previousMatrix 时用的就是这次结果。
        Matrix4x4 previousMatrix = GUI.matrix;
        // 步骤 2：计算 GUI.color，把结果存进 previousColor（previousColor）；之后引用 previousColor 时用的就是这次结果。
        Color previousColor = GUI.color;
        // 步骤 3：计算 to - from，把结果存进 delta（delta）；之后引用 delta 时用的就是这次结果。
        Vector2 delta = to - from;
        // 步骤 4：调用 RotateAroundPivot：RotateAroundPivot。
        GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, from);
        // 步骤 5：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = new Color(.84f, .76f, .57f, .85f);
        // 步骤 6：调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(new Rect(from.x, from.y - 1f, delta.magnitude, 2f), Texture2D.whiteTexture);
        // 步骤 7：把计算结果 previousMatrix 保存为matrix；下一条判断或调用会读取这个具体值。
        GUI.matrix = previousMatrix;
        // 步骤 8：把计算结果 previousColor 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = previousColor;
    }
    // ！！！功能结束：DrawGuiBranch！！！

    // ！！！功能：UpdateDesktopHover——检查桌面鼠标悬停的兑换节点并更新提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateDesktopHover()
    {
        // 步骤 1：把数值 -1 暂存为 current；它只在当前方法里参与后续判断或计算。
        int current = -1;
        // 步骤 2：计算 Mouse.current，把结果存进 mouse（mouse）；之后引用 mouse 时用的就是这次结果。
        Mouse mouse = Mouse.current;
        // 步骤 3：检查mouse是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (IsMenuOpen && mouse != null && Cursor.lockState == CursorLockMode.None)
        {
            // 步骤 4：计算 mouse.position.ReadValue()，把结果存进 pointer（pointer）；之后引用 pointer 时用的就是这次结果。
            Vector2 pointer = mouse.position.ReadValue();
            // 步骤 5：创建新的 Vector2 并存为 guiPoint；构造参数决定其初始内容或对象名称。
            Vector2 guiPoint = new Vector2(pointer.x, Screen.height - pointer.y);
            // 步骤 6：计算 width 并限制在安全上下界，防止不合理的大小影响移动或显示。
            float width = Mathf.Clamp(Screen.width * .36f, 340f, 560f);
            // 步骤 7：计算 Screen.width - width，把结果存进 left（left）；之后引用 left 时用的就是这次结果。
            float left = Screen.width - width;
            // 步骤 8：计算 108f + 88f，把结果存进 treeTop（treeTop）；之后引用 treeTop 时用的就是这次结果。
            float treeTop = 108f + 88f;
            // 步骤 9：计算 Mathf.Max(360f, Screen.height - 108f - 210f)，把结果存进 treeHeight（treeHeight）；之后引用 treeHeight 时用的就是这次结果。
            float treeHeight = Mathf.Max(360f, Screen.height - 108f - 210f);
            // 步骤 10：计算 { .50f, .34f, .50f, .66f, .50f }，把结果存进 px（px）；之后引用 px 时用的就是这次结果。
            float[] px = { .50f, .34f, .50f, .66f, .50f };
            // 步骤 11：计算 { .88f, .70f, .52f, .34f, .16f }，把结果存进 py（py）；之后引用 py 时用的就是这次结果。
            float[] py = { .88f, .70f, .52f, .34f, .16f };
            // 步骤 12：按 int i = 0; i < 5; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < 5; i++)
            {
                // 步骤 13：新建用于icon的对象或数值结构，并把构造参数保存进去。
                Rect icon = new Rect(left + width * px[i] - 32f,
                    // 步骤 14：执行 treeTop + treeHeight * py[i] - 32f, 64f, 64f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    treeTop + treeHeight * py[i] - 32f, 64f, 64f);
                // 步骤 15：检查条件 icon.Contains(guiPoint)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { current = i; break; }
                if (icon.Contains(guiPoint)) { current = i; break; }
            }
        }
        // 步骤 16：检查条件 current == hoveredNode；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (current == hoveredNode) return;
        // 步骤 17：把计算结果 current 保存为鼠标悬停的树节点；下一条判断或调用会读取这个具体值。
        hoveredNode = current;
        // 步骤 18：把计算结果 Time.unscaledTime 保存为hoveredSince；下一条判断或调用会读取这个具体值。
        hoveredSince = Time.unscaledTime;
    }
    // ！！！功能结束：UpdateDesktopHover！！！

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

    // ！！！功能：DesktopToggleNearest——桌面ToggleNearest！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopToggleNearest()
    {
        // 步骤 1：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!desktopMode) return;
        // 步骤 2：调用 ToggleNearest：ToggleNearest。
        ToggleNearest();
    }
    // ！！！功能结束：DesktopToggleNearest！！！

    // ！！！功能：DesktopSelect——桌面Select！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopSelect(int index)
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：!desktopMode || !IsMenuOpen || index < 0 || index >= 5；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!desktopMode || !IsMenuOpen || index < 0 || index >= 5) return;
        // 步骤 2：把计算结果 index 保存为selected；下一条判断或调用会读取这个具体值。
        selected = index;
        // 步骤 3：这一行补充 status?.Show(SpellNames[activeAncestor, index] + (obtained[activeAncestor, index] ?，与相邻行合起来完成同一次计算或对象配置。
        status?.Show(SpellNames[activeAncestor, index] + (obtained[activeAncestor, index] ?
            // 步骤 4：执行 " already exchanged." : " selected. Drag its icon outward to exchange."), 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            " already exchanged." : " selected. Drag its icon outward to exchange."), 4f);
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
        // 步骤 1：这里用 || 表示任一条件成立就处理：!desktopMode || !IsMenuOpen || selected < 0；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!desktopMode || !IsMenuOpen || selected < 0) return;
        // 步骤 2：调用 Obtain：兑换成功后创建奖励物并登记拥有状态。
        Obtain(selected);
        // 步骤 3：把计算结果 -1 保存为selected；下一条判断或调用会读取这个具体值。
        selected = -1;
    }
    // ！！！功能结束：DesktopObtainSelected！！！

    // ！！！功能：ToggleNearest——ToggleNearest！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ToggleNearest()
    {
        // 步骤 1：检查条件 IsMenuOpen；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { CloseMenu(); nextToggle = Time.time + .2f; return; }
        if (IsMenuOpen) { CloseMenu(); nextToggle = Time.time + .2f; return; }
        // 步骤 2：计算 NearestAncestor()，把结果存进 index（index）；之后引用 index 时用的就是这次结果。
        int index = NearestAncestor();
        // 步骤 3：检查条件 index < 0；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (index < 0) return;
        // 步骤 4：调用 OpenMenu：打开先祖兑换树并解锁相关鼠标或手柄输入。
        OpenMenu(index);
    }
    // ！！！功能结束：ToggleNearest！！！

    // ！！！功能：OpenMenu——打开先祖兑换树并解锁相关鼠标或手柄输入！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OpenMenu(int index)
    {
        // 步骤 1：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。 条件成立时立即退出当前方法。
        if (index < 0 || !IsNear(index) || Time.time < nextToggle) return;
        // 步骤 2：执行 legacyTree?.CloseForMagic();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        legacyTree?.CloseForMagic();
        // 步骤 3：把计算结果 index 保存为activeAncestor；下一条判断或调用会读取这个具体值。
        activeAncestor = index;
        // 步骤 4：把计算结果 focusedBefore = -1 保存为鼠标悬停的树节点；下一条判断或调用会读取这个具体值。
        hoveredNode = focusedBefore = -1;
        // 步骤 5：把计算结果 focusedSince = Time.unscaledTime 保存为hoveredSince；下一条判断或调用会读取这个具体值。
        hoveredSince = focusedSince = Time.unscaledTime;
        // 步骤 6：把原三维朝向投影到地面平面并存为 forward；行走或转向不受头部俯仰影响。
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        // 步骤 7：检查条件 forward.sqrMagnitude < .01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 forward = Vector3.forward;
        if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
        // 步骤 8：执行 menuRoot.transform.position = head.position + forward * 1.65f + head.right * .72f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        menuRoot.transform.position = head.position + forward * 1.65f + head.right * .72f;
        // World-space TMP faces its local -Z side. Point +Z away from the
        // viewer so the text and node order are readable instead of mirrored.
        // 步骤 9：执行 menuRoot.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        menuRoot.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        // 步骤 10：把新内容写入 title.text；玩家看到的文字会在本帧更新。
        title.text = "ANCESTOR MAGIC";
        // 步骤 11：显示这个场景对象及其子对象；界面会立即反映这一状态。
        menuRoot.SetActive(true);
        // 步骤 12：调用 RefreshNodes：刷新Nodes。
        RefreshNodes();
        // 步骤 13：这一行补充 status?.Show(desktopMode ? "Drag an unlocked icon outward to exchange  •  F closes" :，与相邻行合起来完成同一次计算或对象配置。
        status?.Show(desktopMode ? "Drag an unlocked icon outward to exchange  •  F closes" :
            // 步骤 14：执行 "Hold an icon or pull it outward with the right trigger. X closes.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Hold an icon or pull it outward with the right trigger. X closes.", 5f);
        // 步骤 15：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Left, .24f, .06f);
        // 步骤 16：根据当前游戏时间计算并保存nextToggle，用于后续延迟或冷却判断。
        nextToggle = Time.time + .25f;
    }
    // ！！！功能结束：OpenMenu！！！

    // ！！！功能：CloseMenu——关闭兑换树并恢复正常角色控制！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void CloseMenu()
    {
        // 步骤 1：确认menuRoot已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (menuRoot != null) menuRoot.SetActive(false);
        // 步骤 2：确认pointer已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 pointer.enabled = false;
        if (pointer != null) pointer.enabled = false;
        // 步骤 3：把计算结果 -1 保存为activeAncestor；下一条判断或调用会读取这个具体值。
        activeAncestor = -1;
        // 步骤 4：把计算结果 focused = -1 保存为selected；下一条判断或调用会读取这个具体值。
        selected = focused = -1;
        // 步骤 5：把计算结果 -1 保存为guiDragIndex；下一条判断或调用会读取这个具体值。
        guiDragIndex = -1;
        // 步骤 6：把 guiDragTriggered 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        guiDragTriggered = false;
        // 步骤 7：把计算结果 focusedBefore = -1 保存为鼠标悬停的树节点；下一条判断或调用会读取这个具体值。
        hoveredNode = focusedBefore = -1;
    }
    // ！！！功能结束：CloseMenu！！！

    // 这一行补充 private Vector3 PlayerPosition => avatar != null ? avatar.position + Vector3.up * 1.2f :，与相邻行合起来完成同一次计算或对象配置。
    private Vector3 PlayerPosition => avatar != null ? avatar.position + Vector3.up * 1.2f :
        // 执行 (head != null ? head.position : transform.position + Vector3.up * 1.2f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        (head != null ? head.position : transform.position + Vector3.up * 1.2f);

    // ！！！功能：IsNear——IsNear！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private bool IsNear(int index)
    {
        // 步骤 1：检查ancestors是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (index < 0 || ancestors == null || index >= ancestors.Length || ancestors[index] == null)
            // 步骤 2：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;
        // 步骤 3：把计算结果 memorials != null && index < memorials.Length && memorials[index] != null ? 保存为center；下一条判断或调用会读取这个具体值。
        Transform center = memorials != null && index < memorials.Length && memorials[index] != null ?
            // 步骤 4：执行 memorials[index] : ancestors[index];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            memorials[index] : ancestors[index];
        // 步骤 5：把 Vector3.ProjectOnPlane(PlayerPosition - center.position, Vector3.up).magnitude <= proximity 作为方法结果返回给调用方；return 后本方法立即结束。
        return Vector3.ProjectOnPlane(PlayerPosition - center.position, Vector3.up).magnitude <= proximity;
    }
    // ！！！功能结束：IsNear！！！

    // ！！！功能：IsNearTarget——IsNearTarget！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform target 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public bool IsNearTarget(Transform target)
    {
        // 步骤 1：按 int i = 0; i < ancestors.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < ancestors.Length; i++)
            // 步骤 2：检查条件 ancestors[i] == target；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
            if (ancestors[i] == target) return IsNear(i);
        // 步骤 3：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
        return false;
    }
    // ！！！功能结束：IsNearTarget！！！

    // ！！！功能：NearestAncestor——NearestAncestor！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 int 类型的计算结果；执行到 return 时把该值交回调用者。
    private int NearestAncestor()
    {
        // 步骤 1：把数值 -1 暂存为 nearest；它只在当前方法里参与后续判断或计算。
        int nearest = -1;
        // 步骤 2：计算 float.PositiveInfinity，把结果存进 best（best）；之后引用 best 时用的就是这次结果。
        float best = float.PositiveInfinity;
        // 步骤 3：按 int i = 0; i < ancestors.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < ancestors.Length; i++)
        {
            // 步骤 4：这里的 ! 表示取反；当 IsNear(i) 不成立时才进入此分支。 成立时执行 continue;
            if (!IsNear(i)) continue;
            // 步骤 5：计算 Vector3.Distance(PlayerPosition, ancestors[i].position)，把结果存进 distance（distance）；之后引用 distance 时用的就是这次结果。
            float distance = Vector3.Distance(PlayerPosition, ancestors[i].position);
            // 步骤 6：检查条件 distance < best；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { best = distance; nearest = i; }
            if (distance < best) { best = distance; nearest = i; }
        }
        // 步骤 7：把 nearest 作为方法结果返回给调用方；return 后本方法立即结束。
        return nearest;
    }
    // ！！！功能结束：NearestAncestor！！！

    // ！！！功能：MakeMenu——MakeMenu！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void MakeMenu()
    {
        // 步骤 1：新建用于menuRoot的对象或数值结构，并把构造参数保存进去。
        menuRoot = new GameObject("Magic exchange tree");
        // 步骤 2：执行 menuRoot.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        menuRoot.transform.SetParent(transform, false);
        // 步骤 3：执行 menuRoot.transform.localScale = Vector3.one * .8f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        menuRoot.transform.localScale = Vector3.one * .8f;
        // 步骤 4：把计算结果 SkyQuestVisuals.Text("Magic tree title", "ANCESTOR MAGIC", menuRoot.transform, 保存为物品标题；下一条判断或调用会读取这个具体值。
        title = SkyQuestVisuals.Text("Magic tree title", "ANCESTOR MAGIC", menuRoot.transform,
            // 步骤 5：用 new 创建 Vector3(0f, 1.27f, -.05f), 5f, new Color(1f, .88f, .58f), 4f);；构造参数决定新对象的初始内容。
            new Vector3(0f, 1.27f, -.05f), 5f, new Color(1f, .88f, .58f), 4f);
        // 步骤 6：把 TextWrappingModes.NoWrap 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
        title.textWrappingMode = TextWrappingModes.NoWrap;
        // 步骤 7：把计算结果 TextOverflowModes.Overflow 保存为overflow模式；下一条判断或调用会读取这个具体值。
        title.overflowMode = TextOverflowModes.Overflow;
        // 步骤 8：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 title.enabled = false;
        if (desktopMode) title.enabled = false;
        // 步骤 9：新建用于nodes的对象或数值结构，并把构造参数保存进去。
        nodes = new Transform[5];
        // 步骤 10：新建用于labels的对象或数值结构，并把构造参数保存进去。
        labels = new TextMeshPro[5];
        // 步骤 11：新建用于iconGlyphs的对象或数值结构，并把构造参数保存进去。
        iconGlyphs = new TextMeshPro[5];
        // 步骤 12：按 int i = 0; i < 5; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 5; i++)
        {
            // 步骤 13：把计算结果 SkyQuestVisuals.Primitive("Magic " + (i + 1), PrimitiveType.Sphere, 保存为node；下一条判断或调用会读取这个具体值。
            GameObject node = SkyQuestVisuals.Primitive("Magic " + (i + 1), PrimitiveType.Sphere,
                // 步骤 14：继续填写上一行调用的参数：menuRoot.transform, new Vector3(NodePositions[i].x, NodePositions[i].y, -.08f)；参数位置决定它表示的坐标、速度或选项。
                menuRoot.transform, new Vector3(NodePositions[i].x, NodePositions[i].y, -.08f),
                // 步骤 15：执行 Vector3.one * .19f, new Color(1f, .55f, .67f), true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * .19f, new Color(1f, .55f, .67f), true);
            // 步骤 16：执行 node.AddComponent<SkyQuestMagicNode>().index = i;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            node.AddComponent<SkyQuestMagicNode>().index = i;
            // 步骤 17：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            node.GetComponent<Renderer>().enabled = false;
            // 步骤 18：把计算结果 SkyQuestVisuals.Text("Magic icon glyph " + (i + 1), NodeGlyphs[i], 保存为glyph；下一条判断或调用会读取这个具体值。
            TextMeshPro glyph = SkyQuestVisuals.Text("Magic icon glyph " + (i + 1), NodeGlyphs[i],
                // 步骤 19：执行 node.transform, new Vector3(0f, 0f, -.16f), 5.2f, Color.white, 1.3f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                node.transform, new Vector3(0f, 0f, -.16f), 5.2f, Color.white, 1.3f);
            // 步骤 20：把计算结果 FontStyles.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
            glyph.fontStyle = FontStyles.Bold;
            // 步骤 21：把 TextWrappingModes.NoWrap 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
            glyph.textWrappingMode = TextWrappingModes.NoWrap;
            // 步骤 22：执行 iconGlyphs[i] = glyph;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            iconGlyphs[i] = glyph;
            // 步骤 23：执行 nodes[i] = node.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            nodes[i] = node.transform;
            // 步骤 24：继续填写上一行调用的参数：labels[i] = SkyQuestVisuals.Text("Magic name " + (i + 1), "", menuRoot.transform；参数位置决定它表示的坐标、速度或选项。
            labels[i] = SkyQuestVisuals.Text("Magic name " + (i + 1), "", menuRoot.transform,
                // 步骤 25：用 new 创建 Vector3(NodePositions[i].x, NodePositions[i].y - .22f, -.13f),；构造参数决定新对象的初始内容。
                new Vector3(NodePositions[i].x, NodePositions[i].y - .22f, -.13f),
                // 步骤 26：执行 3.2f, Color.white, 3.3f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                3.2f, Color.white, 3.3f);
            // 步骤 27：执行 labels[i].textWrappingMode = TextWrappingModes.NoWrap;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            labels[i].textWrappingMode = TextWrappingModes.NoWrap;
            // 步骤 28：执行 labels[i].overflowMode = TextOverflowModes.Overflow;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            labels[i].overflowMode = TextOverflowModes.Overflow;
            // 步骤 29：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 labels[i].enabled = false;
            if (desktopMode) labels[i].enabled = false;
            // 步骤 30：检查条件 i > 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 MakeBranch：MakeBranch。
            if (i > 0) MakeBranch(i - 1, i);
        }
        // 步骤 31：把计算结果 SkyQuestVisuals.Text("Magic instructions", desktopMode ? 保存为instructions；下一条判断或调用会读取这个具体值。
        TextMeshPro instructions = SkyQuestVisuals.Text("Magic instructions", desktopMode ?
            // 步骤 32：这一行补充 "Drag icon outward: exchange     F: close" :，与相邻行合起来完成同一次计算或对象配置。
            "Drag icon outward: exchange     F: close" :
            // 步骤 33：继续填写上一行调用的参数："Right trigger hold or pull icon outward: exchange     X: close", menuRoot.transform；参数位置决定它表示的坐标、速度或选项。
            "Right trigger hold or pull icon outward: exchange     X: close", menuRoot.transform,
            // 步骤 34：用 new 创建 Vector3(0f, -1.29f, -.1f), 3.1f, Color.white, 4f);；构造参数决定新对象的初始内容。
            new Vector3(0f, -1.29f, -.1f), 3.1f, Color.white, 4f);
        // 步骤 35：把 TextWrappingModes.NoWrap 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
        instructions.textWrappingMode = TextWrappingModes.NoWrap;
        // 步骤 36：把计算结果 TextOverflowModes.Overflow 保存为overflow模式；下一条判断或调用会读取这个具体值。
        instructions.overflowMode = TextOverflowModes.Overflow;
        // 步骤 37：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 instructions.enabled = false;
        if (desktopMode) instructions.enabled = false;
        // 步骤 38：把计算结果 SkyQuestVisuals.Text("Magic reward hint", "", menuRoot.transform, 保存为hoverHelp；下一条判断或调用会读取这个具体值。
        hoverHelp = SkyQuestVisuals.Text("Magic reward hint", "", menuRoot.transform,
            // 步骤 39：用 new 创建 Vector3(0f, -1.08f, -.3f), 2.7f, Color.black, 4.8f);；构造参数决定新对象的初始内容。
            new Vector3(0f, -1.08f, -.3f), 2.7f, Color.black, 4.8f);
        // 步骤 40：把 TextWrappingModes.Normal 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
        hoverHelp.textWrappingMode = TextWrappingModes.Normal;
        // 步骤 41：执行 hoverHelp.rectTransform.sizeDelta = new Vector2(4.8f, 1.7f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        hoverHelp.rectTransform.sizeDelta = new Vector2(4.8f, 1.7f);
        // 步骤 42：把计算结果 SkyQuestVisuals.Primitive("Magic hint yellow plate", PrimitiveType.Cube, 保存为hoverHelpPlate；下一条判断或调用会读取这个具体值。
        hoverHelpPlate = SkyQuestVisuals.Primitive("Magic hint yellow plate", PrimitiveType.Cube,
            // 步骤 43：继续填写上一行调用的参数：menuRoot.transform, new Vector3(0f, -1.08f, -.27f)；参数位置决定它表示的坐标、速度或选项。
            menuRoot.transform, new Vector3(0f, -1.08f, -.27f),
            // 步骤 44：用 new 创建 Vector3(.69f, .29f, .012f), new Color(1f, .9f, .12f), true);；构造参数决定新对象的初始内容。
            new Vector3(.69f, .29f, .012f), new Color(1f, .9f, .12f), true);
        // 步骤 45：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(hoverHelpPlate);
        // 步骤 46：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        hoverHelpPlate.SetActive(false);
        // 步骤 47：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 hoverHelp.enabled = false;
        if (desktopMode) hoverHelp.enabled = false;
        // 步骤 48：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
            // 步骤 49：逐个读取 Renderer renderer in menuRoot.GetComponentsInChildren<Renderer>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Renderer renderer in menuRoot.GetComponentsInChildren<Renderer>(true))
                // 步骤 50：把 renderer.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                renderer.enabled = false;
        // 步骤 51：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        menuRoot.SetActive(false);
    }
    // ！！！功能结束：MakeMenu！！！

    // ！！！功能：MakeBranch——MakeBranch！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int parent, int child 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void MakeBranch(int parent, int child)
    {
        // 步骤 1：创建新的 Vector3 并存为 a；构造参数决定其初始内容或对象名称。
        Vector3 a = new Vector3(NodePositions[parent].x, NodePositions[parent].y, -.06f);
        // 步骤 2：创建新的 Vector3 并存为 b；构造参数决定其初始内容或对象名称。
        Vector3 b = new Vector3(NodePositions[child].x, NodePositions[child].y, -.06f);
        // 步骤 3：把计算结果 SkyQuestVisuals.Primitive("Magic branch", PrimitiveType.Cube, 保存为line；下一条判断或调用会读取这个具体值。
        GameObject line = SkyQuestVisuals.Primitive("Magic branch", PrimitiveType.Cube,
            // 步骤 4：继续填写上一行调用的参数：menuRoot.transform, (a + b) * .5f, new Vector3(.018f, Vector3.Distance(a, b), .012f)；参数位置决定它表示的坐标、速度或选项。
            menuRoot.transform, (a + b) * .5f, new Vector3(.018f, Vector3.Distance(a, b), .012f),
            // 步骤 5：用 new 创建 Color(.85f, .82f, .7f), true);；构造参数决定新对象的初始内容。
            new Color(.85f, .82f, .7f), true);
        // 步骤 6：执行 line.transform.up = (b - a).normalized;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        line.transform.up = (b - a).normalized;
        // 步骤 7：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(line);
    }
    // ！！！功能结束：MakeBranch！！！

    // ！！！功能：MakePointer——MakePointer！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void MakePointer()
    {
        // 步骤 1：创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject("Magic controller pointer", typeof(LineRenderer));
        // 步骤 2：执行 obj.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(transform, false);
        // 步骤 3：从目标 GameObject 查找并保存 pointer 组件；后面的移动或显示逻辑会直接使用这个缓存。
        pointer = obj.GetComponent<LineRenderer>();
        // 步骤 4：把positionCount更新为 2；世界或局部位置会立即影响物体摆放。
        pointer.positionCount = 2;
        // 步骤 5：把计算结果 .012f 保存为startWidth；下一条判断或调用会读取这个具体值。
        pointer.startWidth = .012f;
        // 步骤 6：把计算结果 .004f 保存为endWidth；下一条判断或调用会读取这个具体值。
        pointer.endWidth = .004f;
        // 步骤 7：新建用于material的对象或数值结构，并把构造参数保存进去。
        pointer.material = SkyQuestVisuals.Material(new Color(.65f, .85f, 1f), true);
        // 步骤 8：把 pointer.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        pointer.enabled = false;
    }
    // ！！！功能结束：MakePointer！！！

    // ！！！功能：UpdatePointer——更新Pointer！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdatePointer()
    {
        // 步骤 1：把计算结果 -1 保存为当前正在阅读或观察的物品；下一条判断或调用会读取这个具体值。
        focused = -1;
        // 步骤 2：计算 float.PositiveInfinity，把结果存进 best（best）；之后引用 best 时用的就是这次结果。
        float best = float.PositiveInfinity;
        // 步骤 3：计算 rightHand.position + rightHand.forward * 4f，把结果存进 end（end）；之后引用 end 时用的就是这次结果。
        Vector3 end = rightHand.position + rightHand.forward * 4f;
        // 步骤 4：逐个读取 RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 5f) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 5f))
        {
            // 步骤 5：从目标物体找到所需组件并存为 node；使用前应检查是否为 null。
            SkyQuestMagicNode node = hit.collider.GetComponent<SkyQuestMagicNode>();
            // 步骤 6：检查node是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (node == null || hit.distance >= best) continue;
            // 步骤 7：把计算结果 hit.distance 保存为best；下一条判断或调用会读取这个具体值。
            best = hit.distance;
            // 步骤 8：把计算结果 node.index 保存为当前正在阅读或观察的物品；下一条判断或调用会读取这个具体值。
            focused = node.index;
            // 步骤 9：把计算结果 hit.point 保存为end；下一条判断或调用会读取这个具体值。
            end = hit.point;
        }
        // 步骤 10：把 pointer.enabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        pointer.enabled = true;
        // 步骤 11：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
        pointer.SetPosition(0, rightHand.position);
        // 步骤 12：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
        pointer.SetPosition(1, end);
    }
    // ！！！功能结束：UpdatePointer！！！

    // ！！！功能：ToggleHeldCube——ToggleHeldCube！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ToggleHeldCube()
    {
        // 步骤 1：确认heldCube已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { heldCube.TogglePickup(rightHand); if (!heldCube.held) heldCube = null; re
        if (heldCube != null) { heldCube.TogglePickup(rightHand); if (!heldCube.held) heldCube = null; return; }
        // 步骤 2：计算 FindObjectsByType<SkyQuestMagicPickup>(FindObjectsSortMode.None)，把结果存进 cubes（cubes）；之后引用 cubes 时用的就是这次结果。
        SkyQuestMagicPickup[] cubes = FindObjectsByType<SkyQuestMagicPickup>(FindObjectsSortMode.None);
        // 步骤 3：把数值 2.4f 暂存为 best；它只在当前方法里参与后续判断或计算。
        float best = 2.4f;
        // 步骤 4：逐个读取 SkyQuestMagicPickup cube in cubes 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (SkyQuestMagicPickup cube in cubes)
        {
            // 步骤 5：检查cube是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (cube == null || cube.held) continue;
            // 步骤 6：计算 Vector3.Distance(cube.transform.position, head.position)，把结果存进 distance（distance）；之后引用 distance 时用的就是这次结果。
            float distance = Vector3.Distance(cube.transform.position, head.position);
            // 步骤 7：检查条件 distance <= best；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { best = distance; heldCube = cube; }
            if (distance <= best) { best = distance; heldCube = cube; }
        }
        // 步骤 8：确认heldCube已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetHeld：设置Held。
        if (heldCube != null) heldCube.SetHeld(rightHand);
    }
    // ！！！功能结束：ToggleHeldCube！！！

    // ！！！功能：RefreshNodes——刷新Nodes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RefreshNodes()
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：!IsMenuOpen || activeAncestor < 0；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!IsMenuOpen || activeAncestor < 0) return;
        // 步骤 2：按 int i = 0; i < 5; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 5; i++)
        {
            // 步骤 3：用 claimed 记住此处真假结果（obtained[activeAncestor, i]），供下一条 if 选择执行路径。
            bool claimed = obtained[activeAncestor, i];
            // 步骤 4：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            nodes[i].GetComponent<Renderer>().material.color = claimed ?
                // 步骤 5：用 new 创建 Color(.48f, 1f, .78f) : selected == i || focused == i ?；构造参数决定新对象的初始内容。
                new Color(.48f, 1f, .78f) : selected == i || focused == i ?
                // 步骤 6：用 new 创建 Color(1f, .87f, .48f) : new Color(.8f, .88f, 1f);；构造参数决定新对象的初始内容。
                new Color(1f, .87f, .48f) : new Color(.8f, .88f, 1f);
            // 步骤 7：执行 iconGlyphs[i].text = IsEnergyReward(activeAncestor) ? "◆" : NodeGlyphs[i];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            iconGlyphs[i].text = IsEnergyReward(activeAncestor) ? "◆" : NodeGlyphs[i];
            // 步骤 8：这一行补充 iconGlyphs[i].color = IsEnergyReward(activeAncestor) ?，与相邻行合起来完成同一次计算或对象配置。
            iconGlyphs[i].color = IsEnergyReward(activeAncestor) ?
                // 步骤 9：执行 CubeColors[activeAncestor * 5 + i] : Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                CubeColors[activeAncestor * 5 + i] : Color.white;
            // 步骤 10：执行 labels[i].text = RewardDisplayName(activeAncestor, i) + (claimed ? "  Done" : "");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            labels[i].text = RewardDisplayName(activeAncestor, i) + (claimed ? "  Done" : "");
        }
    }
    // ！！！功能结束：RefreshNodes！！！

    // ！！！功能：Obtain——兑换成功后创建奖励物并登记拥有状态！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Obtain(int index)
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：activeAncestor < 0 || index < 0 || index >= 5；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (activeAncestor < 0 || index < 0 || index >= 5) return;
        // 步骤 2：检查条件 obtained[activeAncestor, index]；只有为真才运行下面的步骤，否则沿原流程继续。
        if (obtained[activeAncestor, index])
        {
            // 步骤 3：执行 status?.Show(SpellNames[activeAncestor, index] + " already exchanged.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show(SpellNames[activeAncestor, index] + " already exchanged.");
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 5：这里用 && 要求两边条件同时成立：index > 0 && !obtained[activeAncestor, index - 1]；任意一边为假就跳过本分支。
        if (index > 0 && !obtained[activeAncestor, index - 1])
        // 步骤 6：这一行补充 { status?.Show("Unlock the previous Magic first."); return; }，与相邻行合起来完成同一次计算或对象配置。
        { status?.Show("Unlock the previous Magic first."); return; }
        // 步骤 7：检查条件 SkyQuestCandleWallet.Balance < candleCosts[index]；只有为真才运行下面的步骤，否则沿原流程继续。
        if (SkyQuestCandleWallet.Balance < candleCosts[index])
        // 步骤 8：这一行补充 { status?.Show("Not enough candles. Need " + candleCosts[index] + "."); return; }，与相邻行合起来完成同一次计算或对象配置。
        { status?.Show("Not enough candles. Need " + candleCosts[index] + "."); return; }
        // 步骤 9：调用 TrySpend：TrySpend。
        SkyQuestCandleWallet.TrySpend(candleCosts[index]);
        // 步骤 10：执行 obtained[activeAncestor, index] = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obtained[activeAncestor, index] = true;
        // 步骤 11：计算 activeAncestor * 5 + index，把结果存进 paletteIndex（paletteIndex）；之后引用 paletteIndex 时用的就是这次结果。
        int paletteIndex = activeAncestor * 5 + index;
        // 步骤 12：用 energyReward 记住此处真假结果（IsEnergyReward(activeAncestor)），供下一条 if 选择执行路径。
        bool energyReward = IsEnergyReward(activeAncestor);
        // 步骤 13：用 placeable 记住此处真假结果（!energyReward && IsPlaceableReward(activeAncestor, index)），供下一条 if 选择执行路径。
        bool placeable = !energyReward && IsPlaceableReward(activeAncestor, index);
        // 步骤 14：计算 CreateRewardItem(RewardDisplayName(activeAncestor, index), paletteIndex, placeable, energy，把结果存进 reward（reward）；之后引用 reward 时用的就是这次结果。
        GameObject reward = CreateRewardItem(RewardDisplayName(activeAncestor, index), paletteIndex, placeable, energyReward);
        // 步骤 15：执行 reward.transform.position = rightHand != null ? rightHand.position : head.position + head.forward * ；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        reward.transform.position = rightHand != null ? rightHand.position : head.position + head.forward * .65f;
        // 步骤 16：执行 reward.transform.rotation = rightHand != null ? rightHand.rotation : Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        reward.transform.rotation = rightHand != null ? rightHand.rotation : Quaternion.identity;
        // 步骤 17：计算 reward.AddComponent<SkyQuestMagicPickup>()，把结果存进 pickup（pickup）；之后引用 pickup 时用的就是这次结果。
        SkyQuestMagicPickup pickup = reward.AddComponent<SkyQuestMagicPickup>();
        // 步骤 18：把计算结果 head 保存为desktopHead；下一条判断或调用会读取这个具体值。
        pickup.desktopHead = head;
        // 步骤 19：把计算结果 rightHand 保存为desktopHand；下一条判断或调用会读取这个具体值。
        pickup.desktopHand = rightHand;
        // 步骤 20：把计算结果 avatar 保存为desktopAvatar；下一条判断或调用会读取这个具体值。
        pickup.desktopAvatar = avatar;
        // 步骤 21：把计算结果 desktopMode 保存为desktop模式；下一条判断或调用会读取这个具体值。
        pickup.desktopMode = desktopMode;
        // 步骤 22：把计算结果 placeable 保存为placeable；下一条判断或调用会读取这个具体值。
        pickup.placeable = placeable;
        // 步骤 23：这里用 && 要求两边条件同时成立：placeable && RewardDisplayName(activeAncestor, index).Contains("Table")；任意一边为假就跳过本分支。
        if (placeable && RewardDisplayName(activeAncestor, index).Contains("Table"))
            // 步骤 24：执行 reward.AddComponent<SkyQuestTeaTable>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            reward.AddComponent<SkyQuestTeaTable>();
        // 步骤 25：把计算结果 energyReward 保存为restoresCapeEnergy；下一条判断或调用会读取这个具体值。
        pickup.restoresCapeEnergy = energyReward;
        if (energyReward) pickup.capeEnergyGain = 3;
        // 步骤 26：调用 SetHeld：设置Held。
        pickup.SetHeld(rightHand);
        // 步骤 27：把计算结果 pickup 保存为heldCube；下一条判断或调用会读取这个具体值。
        heldCube = pickup;
        // 步骤 28：这一行补充 status?.Show(SpellNames[activeAncestor, index] + (desktopMode ?，与相邻行合起来完成同一次计算或对象配置。
        status?.Show(SpellNames[activeAncestor, index] + (desktopMode ?
            // 步骤 29：这一行补充 (placeable ? " exchanged — held in right hand. Hold Q to place; Z drops/picks up." :，与相邻行合起来完成同一次计算或对象配置。
            (placeable ? " exchanged — held in right hand. Hold Q to place; Z drops/picks up." :
                // 步骤 30：这一行补充 " exchanged — held in right hand. Z drops/picks up; hold Q to choose a launch angle.") :，与相邻行合起来完成同一次计算或对象配置。
                " exchanged — held in right hand. Z drops/picks up; hold Q to choose a launch angle.") :
            // 步骤 31：执行 " exchanged. Point and hold right trigger to aim; release to throw. Hold near face to eat."), 6f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            " exchanged. Point and hold right trigger to aim; release to throw. Hold near face to eat."), 6f);
        // 步骤 32：执行 status?.PlayConfirm();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.PlayConfirm();
        // 步骤 33：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right, .5f, .12f);
        // 步骤 34：调用 RefreshNodes：刷新Nodes。
        RefreshNodes();
    }
    // ！！！功能结束：Obtain！！！

    // ！！！功能：IsPlaceableReward——IsPlaceableReward！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int ancestorIndex, int itemIndex 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private static bool IsPlaceableReward(int ancestorIndex, int itemIndex)
    {
        // 步骤 1：计算 DisplayNames[ancestorIndex, itemIndex]，把结果存进 name（name）；之后引用 name 时用的就是这次结果。
        string name = DisplayNames[ancestorIndex, itemIndex];
        // 步骤 2：把 name.Contains("Table") || name.Contains("Bonfire") 作为方法结果返回给调用方；return 后本方法立即结束。
        return name.Contains("Table") || name.Contains("Bonfire");
    }
    // ！！！功能结束：IsPlaceableReward！！！

    // ！！！功能：IsEnergyReward——IsEnergyReward！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int ancestorIndex 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private static bool IsEnergyReward(int ancestorIndex) => ancestorIndex == 2 || ancestorIndex == 5;
    // ！！！功能结束：IsEnergyReward！！！

    // ！！！功能：RewardTooltip——RewardTooltip！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int ancestorIndex, bool desktop 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 string 类型的计算结果；执行到 return 时把该值交回调用者。
    private static string RewardTooltip(int ancestorIndex, bool desktop) => desktop ?
        // 步骤 1：这一行补充 (IsEnergyReward(ancestorIndex) ? "E: EAT +3 FLY ENERGY" :，与相邻行合起来完成同一次计算或对象配置。
        (IsEnergyReward(ancestorIndex) ? "E: EAT +3 FLY ENERGY" :
            // 步骤 2：这一行补充 "E: EAT +5 CANDLES  ·  Z: PICK UP/DROP  ·  Q: AIM") :，与相邻行合起来完成同一次计算或对象配置。
            "E: EAT +5 CANDLES  ·  Z: PICK UP/DROP  ·  Q: AIM") :
        // 步骤 3：这一行补充 (IsEnergyReward(ancestorIndex) ? "+3 FLY ENERGY" : "+5 CANDLES") +，与相邻行合起来完成同一次计算或对象配置。
        (IsEnergyReward(ancestorIndex) ? "+3 FLY ENERGY" : "+5 CANDLES") +
        // 步骤 4：执行 "\nPOINT + RIGHT TRIGGER: GRAB; HOLD TO AIM; RELEASE: THROW\nNEAR FACE + RIGHT TRIGGER: EAT";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        "\nPOINT + RIGHT TRIGGER: GRAB; HOLD TO AIM; RELEASE: THROW\nNEAR FACE + RIGHT TRIGGER: EAT";

    // ！！！功能：RewardDisplayName——RewardDisplayName！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int ancestorIndex, int itemIndex 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 string 类型的计算结果；执行到 return 时把该值交回调用者。
    private static string RewardDisplayName(int ancestorIndex, int itemIndex) =>
        // 步骤 1：调用 IsEnergyReward：IsEnergyReward。
        IsEnergyReward(ancestorIndex) ? "Cape Energy " + (itemIndex + 1) :
        // 步骤 2：执行 DisplayNames[ancestorIndex, itemIndex];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        DisplayNames[ancestorIndex, itemIndex];

    // ！！！功能：CreateRewardItem——创建Reward物品！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string rewardName, int paletteIndex, bool placeable, bool energyReward 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private GameObject CreateRewardItem(string rewardName, int paletteIndex, bool placeable, bool energyReward)
    {
        // 步骤 1：创建新的 GameObject 并存为 root；构造参数决定其初始内容或对象名称。
        GameObject root = new GameObject(rewardName + " reward");
        // 步骤 2：计算 main 并限制在安全上下界，防止不合理的大小影响移动或显示。
        Color main = CubeColors[Mathf.Clamp(paletteIndex, 0, CubeColors.Length - 1)];
        // 步骤 3：计算 Color.Lerp(main, Color.white, .48f)，把结果存进 light（light）；之后引用 light 时用的就是这次结果。
        Color light = Color.Lerp(main, Color.white, .48f);
        // 步骤 4：创建新的 Color 并存为 dark；构造参数决定其初始内容或对象名称。
        Color dark = Color.Lerp(main, new Color(.14f, .1f, .18f), .38f);

        // All tree rewards use the supplied A242 pottery meshes. The reward's
        // existing ability and exchange cost are still determined above.
        // 步骤 5：计算 paletteIndex % 8，把结果存进 ceramicIndex（ceramicIndex）；之后引用 ceramicIndex 时用的就是这次结果。
        int ceramicIndex = paletteIndex % 8;
        // 步骤 6：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (ceramicIndex >= 0 &&
            // 步骤 7：这一行补充 ceramicRewardPrefabs != null && ceramicIndex < ceramicRewardPrefabs.Length &&，与相邻行合起来完成同一次计算或对象配置。
            ceramicRewardPrefabs != null && ceramicIndex < ceramicRewardPrefabs.Length &&
            // 步骤 8：这一行补充 ceramicRewardPrefabs[ceramicIndex] != null)，与相邻行合起来完成同一次计算或对象配置。
            ceramicRewardPrefabs[ceramicIndex] != null)
        {
            // 步骤 9：计算 Instantiate(ceramicRewardPrefabs[ceramicIndex], root.transform, false)，把结果存进 ceramic（ceramic）；之后引用 ceramic 时用的就是这次结果。
            GameObject ceramic = Instantiate(ceramicRewardPrefabs[ceramicIndex], root.transform, false);
            // 步骤 10：把计算结果 rewardName + " A242 ceramic" 保存为name；下一条判断或调用会读取这个具体值。
            ceramic.name = rewardName + " A242 ceramic";
        }
        // 步骤 11：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 energyReward；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (energyReward)
        {
            // 步骤 12：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
            Part(root.transform, "Cape energy cube", PrimitiveType.Cube,
                // 步骤 13：用 new 创建 Vector3(0f, .3f, 0f), new Vector3(.45f, .45f, .45f), main, true);；构造参数决定新对象的初始内容。
                new Vector3(0f, .3f, 0f), new Vector3(.45f, .45f, .45f), main, true);
            // 步骤 14：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
            Part(root.transform, "Cape energy core", PrimitiveType.Cube,
                // 步骤 15：用 new 创建 Vector3(0f, .3f, -.226f), new Vector3(.2f, .2f, .025f), light, true);；构造参数决定新对象的初始内容。
                new Vector3(0f, .3f, -.226f), new Vector3(.2f, .2f, .025f), light, true);
        }
        // 步骤 16：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：placeable && rewardName.Contains("Table")；任意一边为假就跳过本分支。
        else if (placeable && rewardName.Contains("Table")) MakeTable(root.transform, main, light, dark);
        // 步骤 17：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：placeable && rewardName.Contains("Bonfire")；任意一边为假就跳过本分支。
        else if (placeable && rewardName.Contains("Bonfire")) MakeBonfire(root.transform, main, light, dark);
        // 步骤 18：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 19：查看括号中的当前操作类型，跳到对应 case；其他动作不会一起执行。
            switch (paletteIndex % 6)
            {
                // 步骤 20：当操作等于 0 时运行这一支；本分支会在 break 处结束。
                case 0: MakeBottle(root.transform, main, light, dark); break;
                // 步骤 21：当操作等于 1 时运行这一支；本分支会在 break 处结束。
                case 1: MakeJar(root.transform, main, light, dark); break;
                // 步骤 22：当操作等于 2 时运行这一支；本分支会在 break 处结束。
                case 2: MakeBouquet(root.transform, main, light, dark); break;
                // 步骤 23：当操作等于 3 时运行这一支；本分支会在 break 处结束。
                case 3: MakeStar(root.transform, main, light); break;
                // 步骤 24：当操作等于 4 时运行这一支；本分支会在 break 处结束。
                case 4: MakeLamp(root.transform, main, light, dark); break;
                // 步骤 25：没有明确匹配的 case 时执行默认处理，避免状态遗漏。
                default: MakeTorch(root.transform, main, light, dark); break;
            }
        }
        // 步骤 26：把 root 作为方法结果返回给调用方；return 后本方法立即结束。
        return root;
    }
    // ！！！功能结束：CreateRewardItem！！！

    // 继续填写上一行调用的参数：private static GameObject Part(Transform parent, string name, PrimitiveType type；参数位置决定它表示的坐标、速度或选项。
    private static GameObject Part(Transform parent, string name, PrimitiveType type,
        // 这一行补充 Vector3 position, Vector3 scale, Color color, bool glow = false)，与相邻行合起来完成同一次计算或对象配置。
        Vector3 position, Vector3 scale, Color color, bool glow = false)
    {
        // 计算 SkyQuestVisuals.Primitive(name, type, parent, position, scale, color, glow)，把结果存进 part（part）；之后引用 part 时用的就是这次结果。
        GameObject part = SkyQuestVisuals.Primitive(name, type, parent, position, scale, color, glow);
        // Reward decoration must not briefly collide with the player while
        // Destroy() defers removing its primitive collider until frame end.
        // 从目标物体找到所需组件并存为 partCollider；使用前应检查是否为 null。
        Collider partCollider = part.GetComponent<Collider>();
        // 确认partCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 partCollider.enabled = false;
        if (partCollider != null) partCollider.enabled = false;
        // 调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(part);
        // 把 part 作为方法结果返回给调用方；return 后本方法立即结束。
        return part;
    }

    // ！！！功能：MakeBottle——MakeBottle！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, Color main, Color light, Color dark 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MakeBottle(Transform root, Color main, Color light, Color dark)
    {
        // 步骤 1：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Potion bottle", PrimitiveType.Cylinder, new Vector3(0f, .22f, 0f), new Vector3(.18f, .22f, .18f), main, true);
        // 步骤 2：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Bottle shoulder", PrimitiveType.Sphere, new Vector3(0f, .43f, 0f), new Vector3(.19f, .12f, .19f), light, true);
        // 步骤 3：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Bottle neck", PrimitiveType.Cylinder, new Vector3(0f, .55f, 0f), new Vector3(.075f, .09f, .075f), light, true);
        // 步骤 4：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Bottle stopper", PrimitiveType.Cylinder, new Vector3(0f, .66f, 0f), new Vector3(.1f, .035f, .1f), dark);
    }
    // ！！！功能结束：MakeBottle！！！

    // ！！！功能：MakeJar——MakeJar！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, Color main, Color light, Color dark 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MakeJar(Transform root, Color main, Color light, Color dark)
    {
        // 步骤 1：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Jar body", PrimitiveType.Sphere, new Vector3(0f, .25f, 0f), new Vector3(.25f, .28f, .25f), main, true);
        // 步骤 2：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Jar rim", PrimitiveType.Cylinder, new Vector3(0f, .5f, 0f), new Vector3(.17f, .045f, .17f), light, true);
        // 步骤 3：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Jar lid", PrimitiveType.Cylinder, new Vector3(0f, .57f, 0f), new Vector3(.2f, .035f, .2f), dark);
    }
    // ！！！功能结束：MakeJar！！！

    // ！！！功能：MakeBouquet——MakeBouquet！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, Color main, Color light, Color dark 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MakeBouquet(Transform root, Color main, Color light, Color dark)
    {
        // 步骤 1：按 int i = 0; i < 5; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 5; i++)
        {
            // 步骤 2：计算 (i - 2) * .075f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = (i - 2) * .075f;
            // 步骤 3：创建新的 GameObject 并存为 stem；构造参数决定其初始内容或对象名称。
            GameObject stem = Part(root, "Flower stem", PrimitiveType.Cylinder, new Vector3(x * .45f, .24f, 0f), new Vector3(.018f, .22f, .018f), new Color(.35f, .7f, .38f));
            // 步骤 4：执行 stem.transform.localRotation = Quaternion.Euler(0f, 0f, -(i - 2) * 7f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            stem.transform.localRotation = Quaternion.Euler(0f, 0f, -(i - 2) * 7f);
            // 步骤 5：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
            Part(root, "Flower", PrimitiveType.Sphere, new Vector3(x, .51f + Mathf.Abs(i - 2) * .025f, 0f), new Vector3(.1f, .075f, .1f), i % 2 == 0 ? main : light, true);
        }
        // 步骤 6：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Bouquet wrap", PrimitiveType.Cylinder, new Vector3(0f, .13f, 0f), new Vector3(.11f, .14f, .11f), dark);
    }
    // ！！！功能结束：MakeBouquet！！！

    // ！！！功能：MakeStar——MakeStar！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, Color main, Color light 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MakeStar(Transform root, Color main, Color light)
    {
        // 步骤 1：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Star core", PrimitiveType.Sphere, new Vector3(0f, .3f, 0f), new Vector3(.18f, .18f, .08f), light, true);
        // 步骤 2：按 int i = 0; i < 5; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 5; i++)
        {
            // 步骤 3：计算 i * 72f，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
            float angle = i * 72f;
            // 步骤 4：计算 Quaternion.Euler(0f, 0f, angle) * Vector3.up，把结果存进 direction（direction）；之后引用 direction 时用的就是这次结果。
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
            // 步骤 5：把计算结果 Part(root, "Star ray", PrimitiveType.Cube, 保存为ray；下一条判断或调用会读取这个具体值。
            GameObject ray = Part(root, "Star ray", PrimitiveType.Cube,
                // 步骤 6：用 new 创建 Vector3(direction.x * .22f, .3f + direction.y * .22f, 0f), new Vector3(.09f,；构造参数决定新对象的初始内容。
                new Vector3(direction.x * .22f, .3f + direction.y * .22f, 0f), new Vector3(.09f, .25f, .07f), main, true);
            // 步骤 7：执行 ray.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ray.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
    // ！！！功能结束：MakeStar！！！

    // ！！！功能：MakeLamp——MakeLamp！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, Color main, Color light, Color dark 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MakeLamp(Transform root, Color main, Color light, Color dark)
    {
        // 步骤 1：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Lamp base", PrimitiveType.Cylinder, new Vector3(0f, .08f, 0f), new Vector3(.2f, .06f, .2f), dark);
        // 步骤 2：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Lamp stem", PrimitiveType.Cylinder, new Vector3(0f, .29f, 0f), new Vector3(.035f, .2f, .035f), main, true);
        // 步骤 3：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Lamp shade", PrimitiveType.Cylinder, new Vector3(0f, .54f, 0f), new Vector3(.24f, .12f, .24f), main, true);
        // 步骤 4：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Lamp light", PrimitiveType.Sphere, new Vector3(0f, .49f, 0f), new Vector3(.12f, .12f, .12f), light, true);
    }
    // ！！！功能结束：MakeLamp！！！

    // ！！！功能：MakeTorch——MakeTorch！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, Color main, Color light, Color dark 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MakeTorch(Transform root, Color main, Color light, Color dark)
    {
        // 步骤 1：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Torch handle", PrimitiveType.Cylinder, new Vector3(0f, .24f, 0f), new Vector3(.045f, .25f, .045f), dark);
        // 步骤 2：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Torch cup", PrimitiveType.Cylinder, new Vector3(0f, .5f, 0f), new Vector3(.13f, .08f, .13f), main);
        // 步骤 3：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Torch flame", PrimitiveType.Sphere, new Vector3(0f, .66f, 0f), new Vector3(.13f, .2f, .13f), light, true);
        // 步骤 4：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Torch flame tip", PrimitiveType.Sphere, new Vector3(.025f, .82f, 0f), new Vector3(.07f, .14f, .07f), Color.white, true);
    }
    // ！！！功能结束：MakeTorch！！！

    // ！！！功能：MakeTable——MakeTable！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, Color main, Color light, Color dark 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MakeTable(Transform root, Color main, Color light, Color dark)
    {
        // 步骤 1：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Table top", PrimitiveType.Cylinder, new Vector3(0f, .38f, 0f), new Vector3(.34f, .045f, .34f), main, true);
        // 步骤 2：按 int i = 0; i < 4; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 4; i++)
        {
            // 步骤 3：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = (i % 2 == 0 ? -1f : 1f) * .23f;
            // 步骤 4：用三元表达式为 z 二选一：问号前条件为真取前值，为假取冒号后的值。
            float z = (i < 2 ? -1f : 1f) * .23f;
            // 步骤 5：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
            Part(root, "Table leg", PrimitiveType.Cylinder, new Vector3(x, .18f, z), new Vector3(.035f, .18f, .035f), dark);
        }
        // 步骤 6：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Tea light", PrimitiveType.Sphere, new Vector3(0f, .49f, 0f), new Vector3(.1f, .08f, .1f), light, true);
    }
    // ！！！功能结束：MakeTable！！！

    // ！！！功能：MakeBonfire——MakeBonfire！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, Color main, Color light, Color dark 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MakeBonfire(Transform root, Color main, Color light, Color dark)
    {
        // 步骤 1：按 int i = 0; i < 3; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 3; i++)
        {
            // 步骤 2：创建新的 GameObject 并存为 log；构造参数决定其初始内容或对象名称。
            GameObject log = Part(root, "Bonfire log", PrimitiveType.Cylinder, new Vector3(0f, .11f, 0f), new Vector3(.065f, .3f, .065f), dark);
            // 步骤 3：执行 log.transform.localRotation = Quaternion.Euler(90f, i * 60f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            log.transform.localRotation = Quaternion.Euler(90f, i * 60f, 0f);
        }
        // 步骤 4：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Bonfire glow", PrimitiveType.Sphere, new Vector3(0f, .32f, 0f), new Vector3(.2f, .3f, .2f), main, true);
        // 步骤 5：调用 Part：创建或查找一个局部模型部件，供斗篷、角色或 UI 组合。
        Part(root, "Bonfire flame", PrimitiveType.Sphere, new Vector3(.03f, .55f, 0f), new Vector3(.11f, .24f, .11f), light, true);
    }
    // ！！！功能结束：MakeBonfire！！！
}

// 定义 SkyQuestMagicNode 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestMagicNode : MonoBehaviour { public int index; }

// 定义 SkyQuestGalleryIcon 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestGalleryIcon : MonoBehaviour { public int index; }

// 定义 SkyQuestMagicPrompt 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestMagicPrompt : MonoBehaviour
{
    // owner 保存owner（类型 SkyQuestMagicExchange）；下方方法依靠它记录或查找场景状态。
    public SkyQuestMagicExchange owner;
    // 声明 target（target）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform target;
    // label 保存label（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro label;
    // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
    void Start() { label = GetComponent<TextMeshPro>(); }
    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void LateUpdate()
    {
        // 步骤 1：检查owner、label、owner的头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (owner == null || label == null || owner.head == null) return;
        // 步骤 2：把计算结果 owner.IsNearTarget(target) && !owner.IsMenuOpen 保存为enabled；下一条判断或调用会读取这个具体值。
        label.enabled = owner.IsNearTarget(target) && !owner.IsMenuOpen;
        // 步骤 3：检查条件 label.enabled；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 transform.rotation = Quaternion.LookRotation(transform.position - owner.hea
        if (label.enabled) transform.rotation = Quaternion.LookRotation(transform.position - owner.head.position);
    }
    // ！！！功能结束：LateUpdate！！！
}
