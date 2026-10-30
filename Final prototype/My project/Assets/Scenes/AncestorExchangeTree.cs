// ！！！文件功能：桌面先祖树：显示解锁节点，处理鼠标兑换、物品拿取和投掷！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

/// <summary>
/// 桌面版先祖兑换树：管理节点解锁、鼠标长按兑换、物品拿取/装备/投掷及界面反馈。
/// </summary>
// 定义 AncestorExchangeTree 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class AncestorExchangeTree : MonoBehaviour
{
    // =====================================================
    // INTERACTION
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Interaction")]

    // 声明 player（player）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform player;

    // 把 Inspector 可调整的 interactionDistance（允许靠近先祖的距离）设为默认值 18f；具体用途看它在下方的判断和计算。
    public float interactionDistance = 18f;

    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("How long the player must hold the mouse button.")]
    // 把 Inspector 可调整的 longPressTime（鼠标兑换长按时长）设为默认值 0.5f；具体用途看它在下方的判断和计算。
    public float longPressTime = 0.5f;


    // =====================================================
    // CAMERA / HOLD POINT
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Item Hold Point")]

    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("Point in front of Main Camera where extracted item appears.")]
    // 声明 itemHoldPoint（itemHoldPoint）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform itemHoldPoint;

    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("Default distance in front of camera.")]
    // 把 Inspector 可调整的 defaultHoldDistance（物品默认持有距离）设为默认值 2f；具体用途看它在下方的判断和计算。
    public float defaultHoldDistance = 2f;


    // =====================================================
    // THROW
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Throw Settings")]

    // 把 Inspector 可调整的 throwForce（向前投掷力度）设为默认值 12f；具体用途看它在下方的判断和计算。
    public float throwForce = 12f;

    // 把 Inspector 可调整的 upwardThrowForce（投掷时向上抬的力度）设为默认值 2f；具体用途看它在下方的判断和计算。
    public float upwardThrowForce = 2f;


    // =====================================================
    // 3D MODEL TEMPLATES
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("3D Model Templates")]

    // 声明 capeModel（capeModel）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject capeModel;

    // 声明 maskModel（maskModel）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject maskModel;

    // 声明 emoteModel（emoteModel）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject emoteModel;

    // 声明 hairModel（hairModel）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject hairModel;

    // 声明 instrumentModel（instrumentModel）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject instrumentModel;


    // =====================================================
    // VIRTUAL KEYBOARD
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Virtual Keyboard")]

    // 声明 virtualKeyboard（virtualKeyboard）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject virtualKeyboard;

    // 用 keyboardOpen 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool keyboardOpen = false;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Guidance and feedback")]
    // 把 Inspector 可调整的 approachPromptDistance（approachPromptDistance）设为默认值 24f；具体用途看它在下方的判断和计算。
    public float approachPromptDistance = 24f;
    // 把 Inspector 可调整的 feedbackDuration（feedbackDuration）设为默认值 4f；具体用途看它在下方的判断和计算。
    public float feedbackDuration = 4f;
    // 把 Inspector 可调整的 collectionAnimationDuration（collectionAnimationDuration）设为默认值 0.65f；具体用途看它在下方的判断和计算。
    public float collectionAnimationDuration = 0.65f;
    // 创建新的 Color 并存为 promptColor；构造参数决定其初始内容或对象名称。
    public Color promptColor = new Color(1f, 0.9f, 0.55f, 1f);

    // cursorLockBeforeTree 保存cursorLockBeforeTree（类型 CursorLockMode）；下方方法依靠它记录或查找场景状态。
    private CursorLockMode cursorLockBeforeTree;
    // 声明 cursorVisibleBeforeTree（cursorVisibleBeforeTree）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool cursorVisibleBeforeTree;
    // 声明 feedbackMessage（feedbackMessage）：文字内容；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private string feedbackMessage;
    // 声明 feedbackUntil（feedbackUntil）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float feedbackUntil;
    // 把数值 -1f 暂存为 collectionAnimationStart；它只在当前方法里参与后续判断或计算。
    private float collectionAnimationStart = -1f;
    // 声明 collectionAnimationOrigin（collectionAnimation原点）：二维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector2 collectionAnimationOrigin;
    // 声明 collectionAnimationIcon（collectionAnimationIcon）：文字内容；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private string collectionAnimationIcon;
    // feedbackAudio 保存feedback音频（类型 AudioSource）；下方方法依靠它记录或查找场景状态。
    private AudioSource feedbackAudio;
    // feedbackClip 保存feedbackClip（类型 AudioClip）；下方方法依靠它记录或查找场景状态。
    private AudioClip feedbackClip;
    // 声明 lastEquippedItemName（lastEquipped物品Name）：文字内容；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private string lastEquippedItemName;



    // =====================================================
    // TREE STATE
    // =====================================================

    // 用 treeOpen 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool treeOpen = false;

    // 把数值 -1 暂存为 hoveredNode；它只在当前方法里参与后续判断或计算。
    private int hoveredNode = -1;

    // 把数值 -1 暂存为 selectedNode；它只在当前方法里参与后续判断或计算。
    private int selectedNode = -1;


    // =====================================================
    // CURRENT HELD ITEM
    // =====================================================

    // 计算 null，把结果存进 currentHeldObject（当前手持的物品）；之后引用 currentHeldObject 时用的就是这次结果。
    private GameObject currentHeldObject = null;

    // 把数值 -1 暂存为 currentHeldItemIndex；它只在当前方法里参与后续判断或计算。
    private int currentHeldItemIndex = -1;


    // =====================================================
    // LONG PRESS
    // =====================================================

    // 用 isPressing 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool isPressing = false;

    // 把数值 -1 暂存为 pressingNode；它只在当前方法里参与后续判断或计算。
    private int pressingNode = -1;

    // 把数值 0f 暂存为 pressStartTime；它只在当前方法里参与后续判断或计算。
    private float pressStartTime = 0f;


    // =====================================================
    // ITEM NODE
    // =====================================================

    // 告诉 Unity 这个普通 C# 数据类允许序列化；字段可被保存或从配置读取。
    [System.Serializable]
    /// <summary>
    /// 记录兑换节点的名称、图标、价格、前置节点与领取状态。
    /// </summary>
    // 定义 ItemNode 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
    public class ItemNode
    {
        // 声明 itemName（itemName）：文字内容；可在 Unity Inspector 指定，赋值前是该类型的默认值。
        public string itemName;

        // 声明 icon（icon）：文字内容；可在 Unity Inspector 指定，赋值前是该类型的默认值。
        public string icon;

        // 声明 cost（cost）：整数计数；可在 Unity Inspector 指定，赋值前是该类型的默认值。
        public int cost;

        // 声明 parentIndex（parentIndex）：整数计数；可在 Unity Inspector 指定，赋值前是该类型的默认值。
        public int parentIndex;

        // 声明 unlocked（unlocked）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
        public bool unlocked;

        // 声明 extracted（extracted）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
        public bool extracted;

        // 声明 position（position）：二维向量；可在 Unity Inspector 指定，赋值前是该类型的默认值。
        public Vector2 position;
    }


    // =====================================================
    // ITEM LIST
    // =====================================================

    // 这一行补充 private List<ItemNode> items =，与相邻行合起来完成同一次计算或对象配置。
    private List<ItemNode> items =
        // 用 new 创建 List<ItemNode>();；构造参数决定新对象的初始内容。
        new List<ItemNode>();


    // =====================================================
    // NODE COLORS
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Node Colors")]

    // 这一行补充 public Color unlockedColor =，与相邻行合起来完成同一次计算或对象配置。
    public Color unlockedColor =
        // 用 new 创建 Color(0.8f, 0.8f, 0.8f, 1f);；构造参数决定新对象的初始内容。
        new Color(0.8f, 0.8f, 0.8f, 1f);

    // 这一行补充 public Color lockedColor =，与相邻行合起来完成同一次计算或对象配置。
    public Color lockedColor =
        // 用 new 创建 Color(0.35f, 0.35f, 0.35f, 1f);；构造参数决定新对象的初始内容。
        new Color(0.35f, 0.35f, 0.35f, 1f);

    // 这一行补充 public Color hoverColor =，与相邻行合起来完成同一次计算或对象配置。
    public Color hoverColor =
        // 用 new 创建 Color(1f, 1f, 0.3f, 1f);；构造参数决定新对象的初始内容。
        new Color(1f, 1f, 0.3f, 1f);

    // 这一行补充 public Color selectedColor =，与相邻行合起来完成同一次计算或对象配置。
    public Color selectedColor =
        // 用 new 创建 Color(1f, 0.8f, 0.1f, 1f);；构造参数决定新对象的初始内容。
        new Color(1f, 0.8f, 0.1f, 1f);

    // 这一行补充 public Color extractedColor =，与相邻行合起来完成同一次计算或对象配置。
    public Color extractedColor =
        // 用 new 创建 Color(0.18f, 0.18f, 0.18f, 0.7f);；构造参数决定新对象的初始内容。
        new Color(0.18f, 0.18f, 0.18f, 0.7f);


    // =====================================================
    // CONNECTION
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Connection Line")]

    // 这一行补充 public Color connectionColor =，与相邻行合起来完成同一次计算或对象配置。
    public Color connectionColor =
        // 用 new 创建 Color(1f, 1f, 1f, 0.5f);；构造参数决定新对象的初始内容。
        new Color(1f, 1f, 1f, 0.5f);

    // 把 Inspector 可调整的 connectionWidth（connectionWidth）设为默认值 4f；具体用途看它在下方的判断和计算。
    public float connectionWidth = 4f;


    // =====================================================
    // BACKGROUND
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Background")]

    // 这一行补充 public Color backgroundColor =，与相邻行合起来完成同一次计算或对象配置。
    public Color backgroundColor =
        // 用 new 创建 Color(0f, 0f, 0f, 0.65f);；构造参数决定新对象的初始内容。
        new Color(0f, 0f, 0f, 0.65f);


    // =====================================================
    // NODE APPEARANCE
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Node Appearance")]

    // 把 Inspector 可调整的 nodeSize（nodeSize）设为默认值 80f；具体用途看它在下方的判断和计算。
    public float nodeSize = 80f;

    // 把 Inspector 可调整的 iconFontSize（iconFontSize）设为默认值 26f；具体用途看它在下方的判断和计算。
    public float iconFontSize = 26f;

    // 把 Inspector 可调整的 hoverIconFontSize（hoverIconFontSize）设为默认值 32f；具体用途看它在下方的判断和计算。
    public float hoverIconFontSize = 32f;


    // =====================================================
    // START
    // =====================================================

    /// <summary>
    /// 初始化玩家、摄像机与手持点引用；创建节点数据和反馈音效，隐藏模板及键盘。
    /// </summary>
    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // -------------------------------------------------
        // Find Player
        // -------------------------------------------------

        // 步骤 1：检查player是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (player == null)
        {
            // 步骤 2：这一行补充 GameObject p =，与相邻行合起来完成同一次计算或对象配置。
            GameObject p =
                // 步骤 3：调用 Find：查找。
                GameObject.Find("C1_Player");

            // 步骤 4：确认p已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (p != null)
            {
                // 步骤 5：把计算结果 p.transform 保存为player；下一条判断或调用会读取这个具体值。
                player = p.transform;
            }
        }


        // -------------------------------------------------
        // Find Main Camera
        // -------------------------------------------------

        // 步骤 6：这一行补充 Camera mainCamera =，与相邻行合起来完成同一次计算或对象配置。
        Camera mainCamera =
            // 步骤 7：执行 Camera.main;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Camera.main;


        // -------------------------------------------------
        // Find ItemHoldPoint
        // -------------------------------------------------

        // 步骤 8：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (itemHoldPoint == null &&
            // 步骤 9：这一行补充 mainCamera != null)，与相邻行合起来完成同一次计算或对象配置。
            mainCamera != null)
        {
            // 步骤 10：这一行补充 Transform found =，与相邻行合起来完成同一次计算或对象配置。
            Transform found =
                // 步骤 11：这一行补充 mainCamera.transform.Find(，与相邻行合起来完成同一次计算或对象配置。
                mainCamera.transform.Find(
                    // 步骤 12：这一行补充 "ItemHoldPoint"，与相邻行合起来完成同一次计算或对象配置。
                    "ItemHoldPoint"
                );

            // 步骤 13：确认found已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (found != null)
            {
                // 步骤 14：把计算结果 found 保存为itemHoldPoint；下一条判断或调用会读取这个具体值。
                itemHoldPoint = found;
            }
        }


        // -------------------------------------------------
        // Create ItemHoldPoint if missing
        // -------------------------------------------------

        // 步骤 15：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (itemHoldPoint == null &&
            // 步骤 16：这一行补充 mainCamera != null)，与相邻行合起来完成同一次计算或对象配置。
            mainCamera != null)
        {
            // 步骤 17：这一行补充 GameObject holdPoint =，与相邻行合起来完成同一次计算或对象配置。
            GameObject holdPoint =
                // 步骤 18：用 new 创建 GameObject("ItemHoldPoint");；构造参数决定新对象的初始内容。
                new GameObject("ItemHoldPoint");

            // 步骤 19：这一行补充 holdPoint.transform.SetParent(，与相邻行合起来完成同一次计算或对象配置。
            holdPoint.transform.SetParent(
                // 步骤 20：继续填写上一行调用的参数：mainCamera.transform；参数位置决定它表示的坐标、速度或选项。
                mainCamera.transform,
                // 步骤 21：这一行补充 false，与相邻行合起来完成同一次计算或对象配置。
                false
            );

            // 步骤 22：这一行补充 holdPoint.transform.localPosition =，与相邻行合起来完成同一次计算或对象配置。
            holdPoint.transform.localPosition =
                // 步骤 23：用 new 创建 Vector3(；构造参数决定新对象的初始内容。
                new Vector3(
                    // 步骤 24：继续填写上一行调用的参数：0f；参数位置决定它表示的坐标、速度或选项。
                    0f,
                    // 步骤 25：继续填写上一行调用的参数：0f；参数位置决定它表示的坐标、速度或选项。
                    0f,
                    // 步骤 26：这一行补充 defaultHoldDistance，与相邻行合起来完成同一次计算或对象配置。
                    defaultHoldDistance
                );

            // 步骤 27：这一行补充 holdPoint.transform.localRotation =，与相邻行合起来完成同一次计算或对象配置。
            holdPoint.transform.localRotation =
                // 步骤 28：执行 Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Quaternion.identity;

            // 步骤 29：这一行补充 itemHoldPoint =，与相邻行合起来完成同一次计算或对象配置。
            itemHoldPoint =
                // 步骤 30：执行 holdPoint.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                holdPoint.transform;

            // 步骤 31：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log(
                // 步骤 32：这一行补充 "ItemHoldPoint was missing. Created automatically."，与相邻行合起来完成同一次计算或对象配置。
                "ItemHoldPoint was missing. Created automatically."
            );
        }


        // -------------------------------------------------
        // Create Tree
        // -------------------------------------------------

        // 步骤 33：调用 CreateItemTree：创建物品Tree。
        CreateItemTree();
        // 步骤 34：从目标 GameObject 查找并保存 feedback音频 组件；后面的移动或显示逻辑会直接使用这个缓存。
        feedbackAudio = GetComponent<AudioSource>();
        // 步骤 35：如果场景对象没有 AudioSource 组件，就当场创建并保存，保证后续调用有对象可用。
        if (feedbackAudio == null) feedbackAudio = gameObject.AddComponent<AudioSource>();
        // 步骤 36：把 feedbackAudio.playOnAwake 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        feedbackAudio.playOnAwake = false;
        // 步骤 37：把计算结果 0f 保存为spatialBlend；下一条判断或调用会读取这个具体值。
        feedbackAudio.spatialBlend = 0f;
        // 步骤 38：把计算结果 CreateConfirmationChime() 保存为feedbackClip；下一条判断或调用会读取这个具体值。
        feedbackClip = CreateConfirmationChime();


        // -------------------------------------------------
        // Hide Templates
        // -------------------------------------------------

        // 步骤 39：调用 HideModelTemplates：HideModelTemplates。
        HideModelTemplates();


        // -------------------------------------------------
        // Hide Keyboard
        // -------------------------------------------------

        // 步骤 40：确认virtualKeyboard已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (virtualKeyboard != null)
        {
            // 步骤 41：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            virtualKeyboard.SetActive(false);
        }
    }
    // ！！！功能结束：Start！！！


    // =====================================================
    // HIDE MODEL TEMPLATES
    // =====================================================

    /// <summary>
    /// 隐藏仅供实例化的模型模板，避免模板物体本身出现在场景中。
    /// </summary>
    // ！！！功能：HideModelTemplates——HideModelTemplates！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void HideModelTemplates()
    {
        // 步骤 1：确认capeModel已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (capeModel != null)
            // 步骤 2：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            capeModel.SetActive(false);

        // 步骤 3：确认maskModel已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (maskModel != null)
            // 步骤 4：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            maskModel.SetActive(false);

        // 步骤 5：确认emoteModel已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (emoteModel != null)
            // 步骤 6：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            emoteModel.SetActive(false);

        // 步骤 7：确认hairModel已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (hairModel != null)
            // 步骤 8：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            hairModel.SetActive(false);

        // 步骤 9：确认instrumentModel已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (instrumentModel != null)
            // 步骤 10：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            instrumentModel.SetActive(false);
    }
    // ！！！功能结束：HideModelTemplates！！！


    // =====================================================
    // CREATE TREE
    // =====================================================

    /// <summary>
    /// 构造兑换树的节点、价格、父子关系和界面坐标；首次可兑换节点在此设定。
    /// </summary>
    // ！！！功能：CreateItemTree——创建物品Tree！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void CreateItemTree()
    {
        // 步骤 1：调用 Clear：Clear。
        items.Clear();


        // =================================================
        // CAPE
        // CENTER OF TREE
        // =================================================

        // 步骤 2：把括号里的新元素加入 items 集合；后续会按集合顺序使用它。
        items.Add(
            // 步骤 3：用 new 创建 ItemNode；构造参数决定新对象的初始内容。
            new ItemNode
            {
                // 步骤 4：把计算结果 "Cape", 保存为itemName；下一条判断或调用会读取这个具体值。
                itemName = "Cape",
                // 步骤 5：把计算结果 "◆", 保存为icon；下一条判断或调用会读取这个具体值。
                icon = "◆",
                // 步骤 6：把计算结果 10, 保存为cost；下一条判断或调用会读取这个具体值。
                cost = 10,
                // 步骤 7：把计算结果 -1, 保存为parentIndex；下一条判断或调用会读取这个具体值。
                parentIndex = -1,
                // 步骤 8：把计算结果 true, 保存为unlocked；下一条判断或调用会读取这个具体值。
                unlocked = true,
                // 步骤 9：把计算结果 false, 保存为extracted；下一条判断或调用会读取这个具体值。
                extracted = false,

                // 步骤 10：这一行补充 position =，与相邻行合起来完成同一次计算或对象配置。
                position =
                    // 步骤 11：用 new 创建 Vector2(0, 0)；构造参数决定新对象的初始内容。
                    new Vector2(0, 0)
            }
        );


        // =================================================
        // HAIR
        // =================================================

        // 步骤 12：把括号里的新元素加入 items 集合；后续会按集合顺序使用它。
        items.Add(
            // 步骤 13：用 new 创建 ItemNode；构造参数决定新对象的初始内容。
            new ItemNode
            {
                // 步骤 14：把计算结果 "Hair", 保存为itemName；下一条判断或调用会读取这个具体值。
                itemName = "Hair",
                // 步骤 15：把计算结果 "★", 保存为icon；下一条判断或调用会读取这个具体值。
                icon = "★",
                // 步骤 16：把计算结果 5, 保存为cost；下一条判断或调用会读取这个具体值。
                cost = 5,
                // 步骤 17：把计算结果 0, 保存为parentIndex；下一条判断或调用会读取这个具体值。
                parentIndex = 0,
                // 步骤 18：把计算结果 true, 保存为unlocked；下一条判断或调用会读取这个具体值。
                unlocked = true,
                // 步骤 19：把计算结果 false, 保存为extracted；下一条判断或调用会读取这个具体值。
                extracted = false,

                // 步骤 20：这一行补充 position =，与相邻行合起来完成同一次计算或对象配置。
                position =
                    // 步骤 21：用 new 创建 Vector2(-180, 150)；构造参数决定新对象的初始内容。
                    new Vector2(-180, 150)
            }
        );


        // =================================================
        // MASK
        // =================================================

        // 步骤 22：把括号里的新元素加入 items 集合；后续会按集合顺序使用它。
        items.Add(
            // 步骤 23：用 new 创建 ItemNode；构造参数决定新对象的初始内容。
            new ItemNode
            {
                // 步骤 24：把计算结果 "Mask", 保存为itemName；下一条判断或调用会读取这个具体值。
                itemName = "Mask",
                // 步骤 25：把计算结果 "●", 保存为icon；下一条判断或调用会读取这个具体值。
                icon = "●",
                // 步骤 26：把计算结果 8, 保存为cost；下一条判断或调用会读取这个具体值。
                cost = 8,
                // 步骤 27：把计算结果 0, 保存为parentIndex；下一条判断或调用会读取这个具体值。
                parentIndex = 0,
                // 步骤 28：把计算结果 true, 保存为unlocked；下一条判断或调用会读取这个具体值。
                unlocked = true,
                // 步骤 29：把计算结果 false, 保存为extracted；下一条判断或调用会读取这个具体值。
                extracted = false,

                // 步骤 30：这一行补充 position =，与相邻行合起来完成同一次计算或对象配置。
                position =
                    // 步骤 31：用 new 创建 Vector2(180, 150)；构造参数决定新对象的初始内容。
                    new Vector2(180, 150)
            }
        );


        // =================================================
        // EMOTE
        // =================================================

        // 步骤 32：把括号里的新元素加入 items 集合；后续会按集合顺序使用它。
        items.Add(
            // 步骤 33：用 new 创建 ItemNode；构造参数决定新对象的初始内容。
            new ItemNode
            {
                // 步骤 34：把计算结果 "Emote", 保存为itemName；下一条判断或调用会读取这个具体值。
                itemName = "Emote",
                // 步骤 35：把计算结果 "✦", 保存为icon；下一条判断或调用会读取这个具体值。
                icon = "✦",
                // 步骤 36：把计算结果 3, 保存为cost；下一条判断或调用会读取这个具体值。
                cost = 3,
                // 步骤 37：把计算结果 1, 保存为parentIndex；下一条判断或调用会读取这个具体值。
                parentIndex = 1,
                // 步骤 38：把计算结果 false, 保存为unlocked；下一条判断或调用会读取这个具体值。
                unlocked = false,
                // 步骤 39：把计算结果 false, 保存为extracted；下一条判断或调用会读取这个具体值。
                extracted = false,

                // 步骤 40：这一行补充 position =，与相邻行合起来完成同一次计算或对象配置。
                position =
                    // 步骤 41：用 new 创建 Vector2(-180, 300)；构造参数决定新对象的初始内容。
                    new Vector2(-180, 300)
            }
        );


        // =================================================
        // INSTRUMENT
        // =================================================

        // 步骤 42：把括号里的新元素加入 items 集合；后续会按集合顺序使用它。
        items.Add(
            // 步骤 43：用 new 创建 ItemNode；构造参数决定新对象的初始内容。
            new ItemNode
            {
                // 步骤 44：把计算结果 "Instrument", 保存为itemName；下一条判断或调用会读取这个具体值。
                itemName = "Instrument",
                // 步骤 45：把计算结果 "♫", 保存为icon；下一条判断或调用会读取这个具体值。
                icon = "♫",
                // 步骤 46：把计算结果 12, 保存为cost；下一条判断或调用会读取这个具体值。
                cost = 12,
                // 步骤 47：把计算结果 2, 保存为parentIndex；下一条判断或调用会读取这个具体值。
                parentIndex = 2,
                // 步骤 48：把计算结果 false, 保存为unlocked；下一条判断或调用会读取这个具体值。
                unlocked = false,
                // 步骤 49：把计算结果 false, 保存为extracted；下一条判断或调用会读取这个具体值。
                extracted = false,

                // 步骤 50：这一行补充 position =，与相邻行合起来完成同一次计算或对象配置。
                position =
                    // 步骤 51：用 new 创建 Vector2(180, 300)；构造参数决定新对象的初始内容。
                    new Vector2(180, 300)
            }
        );
    }
    // ！！！功能结束：CreateItemTree！！！


    // =====================================================
    // UPDATE
    // =====================================================

    /// <summary>
    /// 每帧处理 E、Esc、Q、T 输入，并检查鼠标长按兑换与松开状态。
    /// </summary>
    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：当前没有键盘设备时结束本帧处理，避免读取不存在的按键。
        if (Keyboard.current == null)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;


        // =================================================
        // E
        // =================================================

        // 步骤 3：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            // 步骤 4：调用 HandleEKey：判断本帧 E 键是否触发拿取、食用或兑换动作。
            HandleEKey();
        }


        // =================================================
        // ESC
        // =================================================

        // 步骤 5：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 步骤 6：调用 HandleEscape：HandleEscape。
            HandleEscape();
        }


        // =================================================
        // Q THROW
        // =================================================

        // 步骤 7：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            // 步骤 8：调用 ThrowCurrentItem：ThrowCurrent物品。
            ThrowCurrentItem();
        }


        // =================================================
        // T KEYBOARD
        // =================================================

        // 步骤 9：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            // 步骤 10：调用 ToggleVirtualKeyboard：ToggleVirtualKeyboard。
            ToggleVirtualKeyboard();
        }


        // =================================================
        // LONG PRESS
        // =================================================

        // 步骤 11：调用 CheckLongPress：检查LongPress。
        CheckLongPress();


        // =================================================
        // CHECK MOUSE RELEASE
        // =================================================

        // 步骤 12：调用 CheckMouseRelease：检查MouseRelease。
        CheckMouseRelease();
    }
    // ！！！功能结束：Update！！！


    // =====================================================
    // E KEY
    // =====================================================

    /// <summary>
    /// 按优先级处理 E：手中有物品则装备；否则关闭已打开的树或在范围内打开树。
    /// </summary>
    // ！！！功能：HandleEKey——判断本帧 E 键是否触发拿取、食用或兑换动作！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void HandleEKey()
    {
        // -------------------------------------------------
        // If tree is open -> close
        // -------------------------------------------------

        // 步骤 1：检查当前手持的物品是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (treeOpen && currentHeldObject == null)
        {
            // 步骤 2：调用 CloseTree：CloseTree。
            CloseTree();
            // 步骤 3：提前结束这个方法；不再执行下方语句。
            return;
        }


        // -------------------------------------------------
        // If holding item -> equip
        // -------------------------------------------------

        // 步骤 4：确认当前手持的物品已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (currentHeldObject != null)
        {
            // 步骤 5：调用 EquipCurrentItem：EquipCurrent物品。
            EquipCurrentItem();
            // 步骤 6：提前结束这个方法；不再执行下方语句。
            return;
        }


        // -------------------------------------------------
        // Open tree
        // -------------------------------------------------

        // 步骤 7：检查player是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (player == null)
            // 步骤 8：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 9：这一行补充 float distance =，与相邻行合起来完成同一次计算或对象配置。
        float distance =
            // 步骤 10：调用 Distance：Distance。
            Vector3.Distance(
                // 步骤 11：继续填写上一行调用的参数：player.position；参数位置决定它表示的坐标、速度或选项。
                player.position,
                // 步骤 12：这一行补充 transform.position，与相邻行合起来完成同一次计算或对象配置。
                transform.position
            );


        // 步骤 13：检查条件 distance <= interactionDistance；只有为真才运行下面的步骤，否则沿原流程继续。
        if (distance <= interactionDistance)
        {
            // 步骤 14：调用 OpenTree：OpenTree。
            OpenTree();
        }
    }
    // ！！！功能结束：HandleEKey！！！


    // =====================================================
    // ESC
    // =====================================================

    /// <summary>
    /// Esc 优先关闭虚拟键盘；没有键盘时关闭兑换树。
    /// </summary>
    // ！！！功能：HandleEscape——HandleEscape！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void HandleEscape()
    {
        // Close keyboard first

        // 步骤 1：检查条件 keyboardOpen；只有为真才运行下面的步骤，否则沿原流程继续。
        if (keyboardOpen)
        {
            // 步骤 2：调用 ToggleVirtualKeyboard：ToggleVirtualKeyboard。
            ToggleVirtualKeyboard();
            // 步骤 3：提前结束这个方法；不再执行下方语句。
            return;
        }


        // Close tree

        // 步骤 4：检查条件 treeOpen；只有为真才运行下面的步骤，否则沿原流程继续。
        if (treeOpen)
        {
            // 步骤 5：调用 CloseTree：CloseTree。
            CloseTree();
        }
    }
    // ！！！功能结束：HandleEscape！！！


    // =====================================================
    // OPEN TREE
    // =====================================================

    /// <summary>
    /// 重置节点交互状态，保存原鼠标状态并显示可点击的兑换树。
    /// </summary>
    // ！！！功能：OpenTree——OpenTree！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void OpenTree()
    {
        // 步骤 1：把 treeOpen 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        treeOpen = true;

        // 步骤 2：把计算结果 -1 保存为当前选中的树节点；下一条判断或调用会读取这个具体值。
        selectedNode = -1;

        // 步骤 3：把计算结果 -1 保存为鼠标悬停的树节点；下一条判断或调用会读取这个具体值。
        hoveredNode = -1;

        // 步骤 4：把 isPressing 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        isPressing = false;

        // 步骤 5：把计算结果 -1 保存为pressingNode；下一条判断或调用会读取这个具体值。
        pressingNode = -1;

        // 步骤 6：把计算结果 Cursor.lockState 保存为cursorLockBeforeTree；下一条判断或调用会读取这个具体值。
        cursorLockBeforeTree = Cursor.lockState;
        // 步骤 7：把计算结果 Cursor.visible 保存为cursorVisibleBeforeTree；下一条判断或调用会读取这个具体值。
        cursorVisibleBeforeTree = Cursor.visible;
        // 步骤 8：把 Cursor.visible 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        Cursor.visible = true;
        // 步骤 9：把计算结果 CursorLockMode.None 保存为lock状态；下一条判断或调用会读取这个具体值。
        Cursor.lockState = CursorLockMode.None;
        // 步骤 10：调用 SetFeedback：设置Feedback。
        SetFeedback("Select an item, then hold the left mouse button to obtain it.", 5f);

        // 步骤 11：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 12：这一行补充 "Ancestor Tree Opened"，与相邻行合起来完成同一次计算或对象配置。
            "Ancestor Tree Opened"
        );
    }
    // ！！！功能结束：OpenTree！！！


    // =====================================================
    // CLOSE TREE
    // =====================================================

    /// <summary>
    /// 关闭兑换树并恢复打开前的鼠标锁定和可见状态。
    /// </summary>
    // ！！！功能：CloseTree——CloseTree！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void CloseTree()
    {
        // 步骤 1：把 treeOpen 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        treeOpen = false;

        // 步骤 2：把计算结果 -1 保存为当前选中的树节点；下一条判断或调用会读取这个具体值。
        selectedNode = -1;

        // 步骤 3：把计算结果 -1 保存为鼠标悬停的树节点；下一条判断或调用会读取这个具体值。
        hoveredNode = -1;

        // 步骤 4：把 isPressing 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        isPressing = false;

        // 步骤 5：把计算结果 -1 保存为pressingNode；下一条判断或调用会读取这个具体值。
        pressingNode = -1;

        // 步骤 6：把计算结果 cursorLockBeforeTree 保存为lock状态；下一条判断或调用会读取这个具体值。
        Cursor.lockState = cursorLockBeforeTree;
        // 步骤 7：把计算结果 cursorVisibleBeforeTree 保存为visible；下一条判断或调用会读取这个具体值。
        Cursor.visible = cursorVisibleBeforeTree;

        // 步骤 8：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 9：这一行补充 "Ancestor Tree Closed"，与相邻行合起来完成同一次计算或对象配置。
            "Ancestor Tree Closed"
        );
    }
    // ！！！功能结束：CloseTree！！！


    // =====================================================
    // LONG PRESS START
    // =====================================================

    /// <summary>
    /// 仅对已解锁、未取出且当前可持有的节点开始计时，防止重复兑换。
    /// </summary>
    // ！！！功能：StartLongPress——StartLongPress！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void StartLongPress(int index)
    {
        // 步骤 1：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (index < 0 ||
            // 步骤 2：这一行补充 index >= items.Count)，与相邻行合起来完成同一次计算或对象配置。
            index >= items.Count)
            // 步骤 3：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 4：这里的 ! 表示取反；当 items[index].unlocked 不成立时才进入此分支。
        if (!items[index].unlocked)
            // 步骤 5：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 6：检查条件 items[index].extracted；只有为真才运行下面的步骤，否则沿原流程继续。
        if (items[index].extracted)
            // 步骤 7：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 8：确认当前手持的物品已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (currentHeldObject != null)
            // 步骤 9：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 10：把 isPressing 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        isPressing = true;

        // 步骤 11：把计算结果 index 保存为pressingNode；下一条判断或调用会读取这个具体值。
        pressingNode = index;

        // 步骤 12：这一行补充 pressStartTime =，与相邻行合起来完成同一次计算或对象配置。
        pressStartTime =
            // 步骤 13：执行 Time.realtimeSinceStartup;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Time.realtimeSinceStartup;

        // 步骤 14：把计算结果 index 保存为当前选中的树节点；下一条判断或调用会读取这个具体值。
        selectedNode = index;
        // 步骤 15：调用 SetFeedback：设置Feedback。
        SetFeedback(items[index].itemName + " selected - keep holding to obtain.", 2f);

        // 步骤 16：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 17：这一行补充 "Started holding: " +，与相邻行合起来完成同一次计算或对象配置。
            "Started holding: " +
            // 步骤 18：这一行补充 items[index].itemName，与相邻行合起来完成同一次计算或对象配置。
            items[index].itemName
        );
    }
    // ！！！功能结束：StartLongPress！！！


    // =====================================================
    // CHECK LONG PRESS
    // =====================================================

    /// <summary>
    /// 鼠标持续按住达到设定时间后兑换选中节点；提前松开则取消。
    /// </summary>
    // ！！！功能：CheckLongPress——检查LongPress！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void CheckLongPress()
    {
        // 步骤 1：这里的 ! 表示取反；当 treeOpen 不成立时才进入此分支。
        if (!treeOpen)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 3：这里的 ! 表示取反；当 isPressing 不成立时才进入此分支。
        if (!isPressing)
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 5：检查条件 pressingNode < 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if (pressingNode < 0)
            // 步骤 6：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 7：确认当前手持的物品已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (currentHeldObject != null)
            // 步骤 8：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 9：这一行补充 float heldTime =，与相邻行合起来完成同一次计算或对象配置。
        float heldTime =
            // 步骤 10：这一行补充 Time.realtimeSinceStartup -，与相邻行合起来完成同一次计算或对象配置。
            Time.realtimeSinceStartup -
            // 步骤 11：执行 pressStartTime;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            pressStartTime;


        // 步骤 12：检查Mouse的current是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
        {
            // 步骤 13：把 isPressing 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            isPressing = false;
            // 步骤 14：把计算结果 -1 保存为pressingNode；下一条判断或调用会读取这个具体值。
            pressingNode = -1;
            // 步骤 15：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 16：检查条件 heldTime >= longPressTime；只有为真才运行下面的步骤，否则沿原流程继续。
        if (heldTime >= longPressTime)
        {
            // 步骤 17：这一行补充 int index =，与相邻行合起来完成同一次计算或对象配置。
            int index =
                // 步骤 18：执行 pressingNode;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                pressingNode;


            // 步骤 19：把 isPressing 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            isPressing = false;

            // 步骤 20：把计算结果 -1 保存为pressingNode；下一条判断或调用会读取这个具体值。
            pressingNode = -1;


            // 步骤 21：调用 ExtractItem：Extract物品。
            ExtractItem(index);
        }
    }
    // ！！！功能结束：CheckLongPress！！！


    // =====================================================
    // CHECK MOUSE RELEASE
    // =====================================================

    /// <summary>
    /// 鼠标提前松开时清理长按状态，避免下一次点击沿用旧计时。
    /// </summary>
    // ！！！功能：CheckMouseRelease——检查MouseRelease！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void CheckMouseRelease()
    {
        // 步骤 1：这里的 ! 表示取反；当 isPressing 不成立时才进入此分支。
        if (!isPressing)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 3：检查Mouse的current是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (Mouse.current == null)
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 5：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (
            // 步骤 6：这一行补充 Mouse.current.leftButton，与相邻行合起来完成同一次计算或对象配置。
            Mouse.current.leftButton
                // 步骤 7：这是上一行链式调用的下一步 .wasReleasedThisFrame；前一结果会继续传给这里筛选或处理。
                .wasReleasedThisFrame
        )
        {
            // 步骤 8：把 isPressing 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            isPressing = false;

            // 步骤 9：把计算结果 -1 保存为pressingNode；下一条判断或调用会读取这个具体值。
            pressingNode = -1;
        }
    }
    // ！！！功能结束：CheckMouseRelease！！！


    // =====================================================
    // GUI
    // =====================================================

    /// <summary>
    /// 按照树是否打开，绘制引导提示或兑换树主体、节点、反馈及收集动画。
    /// </summary>
    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void OnGUI()
    {
        // 步骤 1：这里的 ! 表示取反；当 treeOpen 不成立时才进入此分支。
        if (!treeOpen)
        {
            // 步骤 2：调用 DrawApproachPrompt：DrawApproachPrompt。
            DrawApproachPrompt();
            // 步骤 3：调用 DrawItemStatusBadge：Draw物品状态Badge。
            DrawItemStatusBadge();
            // 步骤 4：调用 DrawFeedback：DrawFeedback。
            DrawFeedback();
            // 步骤 5：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 6：调用 DrawBackground：DrawBackground。
        DrawBackground();

        // 步骤 7：调用 DrawTitle：DrawTitle。
        DrawTitle();

        // 步骤 8：调用 DrawConnections：DrawConnections。
        DrawConnections();

        // 步骤 9：调用 DrawNodes：DrawNodes。
        DrawNodes();

        // 步骤 10：调用 DrawSelectedItem：DrawSelected物品。
        DrawSelectedItem();

        // 步骤 11：调用 DrawInstruction：DrawInstruction。
        DrawInstruction();
        // 步骤 12：调用 DrawCollectionAnimation：DrawCollectionAnimation。
        DrawCollectionAnimation();
        // 步骤 13：调用 DrawItemStatusBadge：Draw物品状态Badge。
        DrawItemStatusBadge();
        // 步骤 14：调用 DrawFeedback：DrawFeedback。
        DrawFeedback();
    }
    // ！！！功能结束：OnGUI！！！


    // =====================================================
    // BACKGROUND
    // =====================================================

    /// <summary>
    /// 绘制覆盖游戏画面的半透明兑换树背景。
    /// </summary>
    // ！！！功能：DrawBackground——DrawBackground！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void DrawBackground()
    {
        // 步骤 1：这一行补充 GUI.color =，与相邻行合起来完成同一次计算或对象配置。
        GUI.color =
            // 步骤 2：执行 backgroundColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            backgroundColor;


        // 步骤 3：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(
            // 步骤 4：用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 步骤 5：继续填写上一行调用的参数：0；参数位置决定它表示的坐标、速度或选项。
                0,
                // 步骤 6：继续填写上一行调用的参数：0；参数位置决定它表示的坐标、速度或选项。
                0,
                // 步骤 7：继续填写上一行调用的参数：Screen.width；参数位置决定它表示的坐标、速度或选项。
                Screen.width,
                // 步骤 8：这一行补充 Screen.height，与相邻行合起来完成同一次计算或对象配置。
                Screen.height
            // 步骤 9：继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 步骤 10：这一行补充 ""，与相邻行合起来完成同一次计算或对象配置。
            ""
        );


        // 步骤 11：这一行补充 GUI.color =，与相邻行合起来完成同一次计算或对象配置。
        GUI.color =
            // 步骤 12：执行 Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Color.white;
    }
    // ！！！功能结束：DrawBackground！！！


    // =====================================================
    // TITLE
    // =====================================================

    /// <summary>
    /// 在兑换树顶部绘制标题和货币等说明。
    /// </summary>
    // ！！！功能：DrawTitle——DrawTitle！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void DrawTitle()
    {
        // 步骤 1：这一行补充 GUIStyle title =，与相邻行合起来完成同一次计算或对象配置。
        GUIStyle title =
            // 步骤 2：用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
            new GUIStyle(
                // 步骤 3：这一行补充 GUI.skin.label，与相邻行合起来完成同一次计算或对象配置。
                GUI.skin.label
            );


        // 步骤 4：把计算结果 28 保存为fontSize；下一条判断或调用会读取这个具体值。
        title.fontSize = 28;

        // 步骤 5：这一行补充 title.alignment =，与相邻行合起来完成同一次计算或对象配置。
        title.alignment =
            // 步骤 6：执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TextAnchor.MiddleCenter;

        // 步骤 7：这一行补充 title.fontStyle =，与相邻行合起来完成同一次计算或对象配置。
        title.fontStyle =
            // 步骤 8：执行 FontStyle.Bold;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            FontStyle.Bold;


        // 步骤 9：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 步骤 10：用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 步骤 11：继续填写上一行调用的参数：Screen.width / 2f - 300f；参数位置决定它表示的坐标、速度或选项。
                Screen.width / 2f - 300f,
                // 步骤 12：继续填写上一行调用的参数：40f；参数位置决定它表示的坐标、速度或选项。
                40f,
                // 步骤 13：继续填写上一行调用的参数：600f；参数位置决定它表示的坐标、速度或选项。
                600f,
                // 步骤 14：这一行补充 50f，与相邻行合起来完成同一次计算或对象配置。
                50f
            // 步骤 15：继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 步骤 16：继续填写上一行调用的参数："ANCESTOR EXCHANGE"；参数位置决定它表示的坐标、速度或选项。
            "ANCESTOR EXCHANGE",
            // 步骤 17：这一行补充 title，与相邻行合起来完成同一次计算或对象配置。
            title
        );
    }
    // ！！！功能结束：DrawTitle！！！


    // =====================================================
    // CONNECTIONS
    // =====================================================

    /// <summary>
    /// 根据节点父子关系在界面上绘制连接线。
    /// </summary>
    // ！！！功能：DrawConnections——DrawConnections！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void DrawConnections()
    {
        // 步骤 1：按  变化索引；循环体依次处理每个位置。
        for (
            // 步骤 2：把数值 0 暂存为 i；它只在当前方法里参与后续判断或计算。
            int i = 0;
            // 步骤 3：执行 i < items.Count;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            i < items.Count;
            // 步骤 4：这一行补充 i++，与相邻行合起来完成同一次计算或对象配置。
            i++
        )
        {
            // 步骤 5：这一行补充 int parent =，与相邻行合起来完成同一次计算或对象配置。
            int parent =
                // 步骤 6：执行 items[i].parentIndex;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                items[i].parentIndex;


            // 步骤 7：检查条件 parent < 0；只有为真才运行下面的步骤，否则沿原流程继续。
            if (parent < 0)
                // 步骤 8：忽略当前无效元素，立刻转到下一轮循环。
                continue;


            // 步骤 9：这一行补充 Vector2 start =，与相邻行合起来完成同一次计算或对象配置。
            Vector2 start =
                // 步骤 10：调用 GetScreenPosition：获取Screen位置。
                GetScreenPosition(
                    // 步骤 11：这一行补充 items[parent].position，与相邻行合起来完成同一次计算或对象配置。
                    items[parent].position
                );


            // 步骤 12：这一行补充 Vector2 end =，与相邻行合起来完成同一次计算或对象配置。
            Vector2 end =
                // 步骤 13：调用 GetScreenPosition：获取Screen位置。
                GetScreenPosition(
                    // 步骤 14：这一行补充 items[i].position，与相邻行合起来完成同一次计算或对象配置。
                    items[i].position
                );


            // 步骤 15：调用 DrawLine：DrawLine。
            DrawLine(
                // 步骤 16：继续填写上一行调用的参数：start；参数位置决定它表示的坐标、速度或选项。
                start,
                // 步骤 17：继续填写上一行调用的参数：end；参数位置决定它表示的坐标、速度或选项。
                end,
                // 步骤 18：继续填写上一行调用的参数：connectionWidth；参数位置决定它表示的坐标、速度或选项。
                connectionWidth,
                // 步骤 19：这一行补充 connectionColor，与相邻行合起来完成同一次计算或对象配置。
                connectionColor
            );
        }
    }
    // ！！！功能结束：DrawConnections！！！


    // =====================================================
    // NODES
    // =====================================================

    /// <summary>
    /// 遍历所有兑换节点，并为每个节点调用单独的绘制逻辑。
    /// </summary>
    // ！！！功能：DrawNodes——DrawNodes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void DrawNodes()
    {
        // 步骤 1：把计算结果 -1 保存为鼠标悬停的树节点；下一条判断或调用会读取这个具体值。
        hoveredNode = -1;


        // 步骤 2：按  变化索引；循环体依次处理每个位置。
        for (
            // 步骤 3：把数值 0 暂存为 i；它只在当前方法里参与后续判断或计算。
            int i = 0;
            // 步骤 4：执行 i < items.Count;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            i < items.Count;
            // 步骤 5：这一行补充 i++，与相邻行合起来完成同一次计算或对象配置。
            i++
        )
        {
            // 步骤 6：调用 DrawNode：DrawNode。
            DrawNode(
                // 步骤 7：继续填写上一行调用的参数：i；参数位置决定它表示的坐标、速度或选项。
                i,
                // 步骤 8：这一行补充 items[i]，与相邻行合起来完成同一次计算或对象配置。
                items[i]
            );
        }
    }
    // ！！！功能结束：DrawNodes！！！


    // =====================================================
    // SINGLE NODE
    // =====================================================

    /// <summary>
    /// 绘制单个节点，处理悬停、选中、锁定、价格和鼠标点击反馈。
    /// </summary>
    // 这一行补充 void DrawNode(，与相邻行合起来完成同一次计算或对象配置。
    void DrawNode(
        // 继续填写上一行调用的参数：int index；参数位置决定它表示的坐标、速度或选项。
        int index,
        // 这一行补充 ItemNode item)，与相邻行合起来完成同一次计算或对象配置。
        ItemNode item)
    {
        // 这一行补充 Vector2 center =，与相邻行合起来完成同一次计算或对象配置。
        Vector2 center =
            // 调用 GetScreenPosition：获取Screen位置。
            GetScreenPosition(
                // 这一行补充 item.position，与相邻行合起来完成同一次计算或对象配置。
                item.position
            );


        // 这一行补充 float size =，与相邻行合起来完成同一次计算或对象配置。
        float size =
            // 执行 nodeSize;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            nodeSize;


        // 这一行补充 Rect rect =，与相邻行合起来完成同一次计算或对象配置。
        Rect rect =
            // 用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 继续填写上一行调用的参数：center.x - size / 2f；参数位置决定它表示的坐标、速度或选项。
                center.x - size / 2f,
                // 继续填写上一行调用的参数：center.y - size / 2f；参数位置决定它表示的坐标、速度或选项。
                center.y - size / 2f,
                // 继续填写上一行调用的参数：size；参数位置决定它表示的坐标、速度或选项。
                size,
                // 这一行补充 size，与相邻行合起来完成同一次计算或对象配置。
                size
            );


        // 这一行补充 Event e =，与相邻行合起来完成同一次计算或对象配置。
        Event e =
            // 执行 Event.current;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Event.current;


        // 这一行补充 bool hover =，与相邻行合起来完成同一次计算或对象配置。
        bool hover =
            // 调用 Contains：Contains。
            rect.Contains(
                // 这一行补充 e.mousePosition，与相邻行合起来完成同一次计算或对象配置。
                e.mousePosition
            );


        // =================================================
        // EXTRACTED
        // =================================================

        // 检查条件 item.extracted；只有为真才运行下面的步骤，否则沿原流程继续。
        if (item.extracted)
        {
            // 调用 DrawExtractedNode：DrawExtractedNode。
            DrawExtractedNode(
                // 继续填写上一行调用的参数：rect；参数位置决定它表示的坐标、速度或选项。
                rect,
                // 继续填写上一行调用的参数：center；参数位置决定它表示的坐标、速度或选项。
                center,
                // 这一行补充 item，与相邻行合起来完成同一次计算或对象配置。
                item
            );

            // 提前结束这个方法；不再执行下方语句。
            return;
        }


        // =================================================
        // HOVER
        // =================================================

        // 检查条件 hover；只有为真才运行下面的步骤，否则沿原流程继续。
        if (hover)
        {
            // 把计算结果 index 保存为鼠标悬停的树节点；下一条判断或调用会读取这个具体值。
            hoveredNode = index;
        }


        // =================================================
        // MOUSE DOWN
        // =================================================

        // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (
            // 这一行补充 hover &&，与相邻行合起来完成同一次计算或对象配置。
            hover &&
            // 这一行补充 item.unlocked &&，与相邻行合起来完成同一次计算或对象配置。
            item.unlocked &&
            // 把计算结果 = null && 保存为当前手持的物品；下一条判断或调用会读取这个具体值。
            currentHeldObject == null &&
            // 把计算结果 = EventType.MouseDown && 保存为type；下一条判断或调用会读取这个具体值。
            e.type == EventType.MouseDown &&
            // 把计算结果 = 0 保存为button；下一条判断或调用会读取这个具体值。
            e.button == 0
        )
        {
            // 调用 StartLongPress：StartLongPress。
            StartLongPress(index);

            // 调用 Use：Use。
            e.Use();
        }


        // =================================================
        // NODE COLOR
        // =================================================

        // 声明 nodeColor（nodeColor）：颜色；只供本脚本内部保存状态，赋值前是该类型的默认值。
        Color nodeColor;


        // 检查条件 selectedNode == index；只有为真才运行下面的步骤，否则沿原流程继续。
        if (selectedNode == index)
        {
            // 这一行补充 nodeColor =，与相邻行合起来完成同一次计算或对象配置。
            nodeColor =
                // 执行 selectedColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                selectedColor;
        }
        // 上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 ；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (
            // 这一行补充 hover &&，与相邻行合起来完成同一次计算或对象配置。
            hover &&
            // 这一行补充 item.unlocked &&，与相邻行合起来完成同一次计算或对象配置。
            item.unlocked &&
            // 把计算结果 = null 保存为当前手持的物品；下一条判断或调用会读取这个具体值。
            currentHeldObject == null
        )
        {
            // 这一行补充 nodeColor =，与相邻行合起来完成同一次计算或对象配置。
            nodeColor =
                // 执行 hoverColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                hoverColor;
        }
        // 上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 item.unlocked；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (item.unlocked)
        {
            // 这一行补充 nodeColor =，与相邻行合起来完成同一次计算或对象配置。
            nodeColor =
                // 执行 unlockedColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                unlockedColor;
        }
        // 前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 这一行补充 nodeColor =，与相邻行合起来完成同一次计算或对象配置。
            nodeColor =
                // 执行 lockedColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                lockedColor;
        }


        // =================================================
        // NODE STYLE
        // =================================================

        // 这一行补充 GUIStyle nodeStyle =，与相邻行合起来完成同一次计算或对象配置。
        GUIStyle nodeStyle =
            // 用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
            new GUIStyle(
                // 这一行补充 GUI.skin.box，与相邻行合起来完成同一次计算或对象配置。
                GUI.skin.box
            );


        // 这一行补充 nodeStyle.fontSize =，与相邻行合起来完成同一次计算或对象配置。
        nodeStyle.fontSize =
            // 这一行补充 hover，与相邻行合起来完成同一次计算或对象配置。
            hover
            // 三元表达式中，前面的条件为真时选这个值。
            ? (int)hoverIconFontSize
            // 三元表达式中，前面的条件为假时选这个备用值。
            : (int)iconFontSize;


        // 这一行补充 nodeStyle.fontStyle =，与相邻行合起来完成同一次计算或对象配置。
        nodeStyle.fontStyle =
            // 执行 FontStyle.Bold;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            FontStyle.Bold;


        // 这一行补充 nodeStyle.alignment =，与相邻行合起来完成同一次计算或对象配置。
        nodeStyle.alignment =
            // 执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TextAnchor.MiddleCenter;


        // 这一行补充 GUI.color =，与相邻行合起来完成同一次计算或对象配置。
        GUI.color =
            // 执行 nodeColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            nodeColor;


        // IMPORTANT:
        // GUI.Box instead of GUI.Button.
        // This prevents GUI.Button from consuming
        // the long-press event.

        // 调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(
            // 继续填写上一行调用的参数：rect；参数位置决定它表示的坐标、速度或选项。
            rect,
            // 继续填写上一行调用的参数：item.icon；参数位置决定它表示的坐标、速度或选项。
            item.icon,
            // 这一行补充 nodeStyle，与相邻行合起来完成同一次计算或对象配置。
            nodeStyle
        );


        // 这一行补充 GUI.color =，与相邻行合起来完成同一次计算或对象配置。
        GUI.color =
            // 执行 Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Color.white;


        // =================================================
        // NAME
        // =================================================

        // 这一行补充 GUIStyle nameStyle =，与相邻行合起来完成同一次计算或对象配置。
        GUIStyle nameStyle =
            // 用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
            new GUIStyle(
                // 这一行补充 GUI.skin.label，与相邻行合起来完成同一次计算或对象配置。
                GUI.skin.label
            );


        // 把计算结果 14 保存为fontSize；下一条判断或调用会读取这个具体值。
        nameStyle.fontSize = 14;

        // 这一行补充 nameStyle.alignment =，与相邻行合起来完成同一次计算或对象配置。
        nameStyle.alignment =
            // 执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TextAnchor.MiddleCenter;


        // 调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 继续填写上一行调用的参数：center.x - 80f；参数位置决定它表示的坐标、速度或选项。
                center.x - 80f,
                // 继续填写上一行调用的参数：center.y + 42f；参数位置决定它表示的坐标、速度或选项。
                center.y + 42f,
                // 继续填写上一行调用的参数：160f；参数位置决定它表示的坐标、速度或选项。
                160f,
                // 这一行补充 25f，与相邻行合起来完成同一次计算或对象配置。
                25f
            // 继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 继续填写上一行调用的参数：item.itemName；参数位置决定它表示的坐标、速度或选项。
            item.itemName,
            // 这一行补充 nameStyle，与相邻行合起来完成同一次计算或对象配置。
            nameStyle
        );


        // =================================================
        // COST
        // =================================================

        // 这一行补充 GUIStyle costStyle =，与相邻行合起来完成同一次计算或对象配置。
        GUIStyle costStyle =
            // 用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
            new GUIStyle(
                // 这一行补充 GUI.skin.label，与相邻行合起来完成同一次计算或对象配置。
                GUI.skin.label
            );


        // 把计算结果 12 保存为fontSize；下一条判断或调用会读取这个具体值。
        costStyle.fontSize = 12;

        // 这一行补充 costStyle.alignment =，与相邻行合起来完成同一次计算或对象配置。
        costStyle.alignment =
            // 执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TextAnchor.MiddleCenter;


        // 调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 继续填写上一行调用的参数：center.x - 80f；参数位置决定它表示的坐标、速度或选项。
                center.x - 80f,
                // 继续填写上一行调用的参数：center.y + 62f；参数位置决定它表示的坐标、速度或选项。
                center.y + 62f,
                // 继续填写上一行调用的参数：160f；参数位置决定它表示的坐标、速度或选项。
                160f,
                // 这一行补充 20f，与相邻行合起来完成同一次计算或对象配置。
                20f
            // 继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 继续填写上一行调用的参数：item.cost + " Candles"；参数位置决定它表示的坐标、速度或选项。
            item.cost + " Candles",
            // 这一行补充 costStyle，与相邻行合起来完成同一次计算或对象配置。
            costStyle
        );


        // =================================================
        // HOLDING INDICATOR
        // =================================================

        // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (
            // 这一行补充 isPressing &&，与相邻行合起来完成同一次计算或对象配置。
            isPressing &&
            // 把计算结果 = index 保存为pressingNode；下一条判断或调用会读取这个具体值。
            pressingNode == index
        )
        {
            // 这一行补充 float progress =，与相邻行合起来完成同一次计算或对象配置。
            float progress =
                // 调用 Clamp01：Clamp01。
                Mathf.Clamp01(
                    // 这一行补充 (，与相邻行合起来完成同一次计算或对象配置。
                    (
                        // 这一行补充 Time.realtimeSinceStartup -，与相邻行合起来完成同一次计算或对象配置。
                        Time.realtimeSinceStartup -
                        // 这一行补充 pressStartTime，与相邻行合起来完成同一次计算或对象配置。
                        pressStartTime
                    )
                    // 这一行补充 /，与相邻行合起来完成同一次计算或对象配置。
                    /
                    // 这一行补充 longPressTime，与相邻行合起来完成同一次计算或对象配置。
                    longPressTime
                );


            // 这一行补充 GUIStyle holdStyle =，与相邻行合起来完成同一次计算或对象配置。
            GUIStyle holdStyle =
                // 用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
                new GUIStyle(
                    // 这一行补充 GUI.skin.label，与相邻行合起来完成同一次计算或对象配置。
                    GUI.skin.label
                );


            // 把计算结果 12 保存为fontSize；下一条判断或调用会读取这个具体值。
            holdStyle.fontSize = 12;

            // 这一行补充 holdStyle.alignment =，与相邻行合起来完成同一次计算或对象配置。
            holdStyle.alignment =
                // 执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                TextAnchor.MiddleCenter;


            // 调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(
                // 用 new 创建 Rect(；构造参数决定新对象的初始内容。
                new Rect(
                    // 继续填写上一行调用的参数：center.x - 100f；参数位置决定它表示的坐标、速度或选项。
                    center.x - 100f,
                    // 继续填写上一行调用的参数：center.y + 82f；参数位置决定它表示的坐标、速度或选项。
                    center.y + 82f,
                    // 继续填写上一行调用的参数：200f；参数位置决定它表示的坐标、速度或选项。
                    200f,
                    // 这一行补充 25f，与相邻行合起来完成同一次计算或对象配置。
                    25f
                // 继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
                ),
                // 这一行补充 "HOLDING " +，与相邻行合起来完成同一次计算或对象配置。
                "HOLDING " +
                // 调用 RoundToInt：Round到Int。
                Mathf.RoundToInt(
                    // 这一行补充 progress * 100f，与相邻行合起来完成同一次计算或对象配置。
                    progress * 100f
                // 这一行补充 ) +，与相邻行合起来完成同一次计算或对象配置。
                ) +
                // 继续填写上一行调用的参数："%"；参数位置决定它表示的坐标、速度或选项。
                "%",
                // 这一行补充 holdStyle，与相邻行合起来完成同一次计算或对象配置。
                holdStyle
            );
        }
    }


    // =====================================================
    // EXTRACTED NODE
    // =====================================================

    /// <summary>
    /// 为已经取出的节点绘制不可再次领取的视觉状态。
    /// </summary>
    // 这一行补充 void DrawExtractedNode(，与相邻行合起来完成同一次计算或对象配置。
    void DrawExtractedNode(
        // 继续填写上一行调用的参数：Rect rect；参数位置决定它表示的坐标、速度或选项。
        Rect rect,
        // 继续填写上一行调用的参数：Vector2 center；参数位置决定它表示的坐标、速度或选项。
        Vector2 center,
        // 这一行补充 ItemNode item)，与相邻行合起来完成同一次计算或对象配置。
        ItemNode item)
    {
        // 这一行补充 GUIStyle style =，与相邻行合起来完成同一次计算或对象配置。
        GUIStyle style =
            // 用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
            new GUIStyle(
                // 这一行补充 GUI.skin.box，与相邻行合起来完成同一次计算或对象配置。
                GUI.skin.box
            );


        // 这一行补充 style.fontSize =，与相邻行合起来完成同一次计算或对象配置。
        style.fontSize =
            // 执行 (int)iconFontSize;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (int)iconFontSize;


        // 这一行补充 style.fontStyle =，与相邻行合起来完成同一次计算或对象配置。
        style.fontStyle =
            // 执行 FontStyle.Bold;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            FontStyle.Bold;


        // 这一行补充 style.alignment =，与相邻行合起来完成同一次计算或对象配置。
        style.alignment =
            // 执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TextAnchor.MiddleCenter;


        // 这一行补充 GUI.color =，与相邻行合起来完成同一次计算或对象配置。
        GUI.color =
            // 执行 extractedColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            extractedColor;


        // 调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(
            // 继续填写上一行调用的参数：rect；参数位置决定它表示的坐标、速度或选项。
            rect,
            // 继续填写上一行调用的参数：item.icon；参数位置决定它表示的坐标、速度或选项。
            item.icon,
            // 这一行补充 style，与相邻行合起来完成同一次计算或对象配置。
            style
        );


        // 这一行补充 GUI.color =，与相邻行合起来完成同一次计算或对象配置。
        GUI.color =
            // 执行 Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Color.white;


        // 这一行补充 GUIStyle nameStyle =，与相邻行合起来完成同一次计算或对象配置。
        GUIStyle nameStyle =
            // 用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
            new GUIStyle(
                // 这一行补充 GUI.skin.label，与相邻行合起来完成同一次计算或对象配置。
                GUI.skin.label
            );


        // 把计算结果 14 保存为fontSize；下一条判断或调用会读取这个具体值。
        nameStyle.fontSize = 14;

        // 这一行补充 nameStyle.alignment =，与相邻行合起来完成同一次计算或对象配置。
        nameStyle.alignment =
            // 执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TextAnchor.MiddleCenter;


        // 调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 继续填写上一行调用的参数：center.x - 80f；参数位置决定它表示的坐标、速度或选项。
                center.x - 80f,
                // 继续填写上一行调用的参数：center.y + 42f；参数位置决定它表示的坐标、速度或选项。
                center.y + 42f,
                // 继续填写上一行调用的参数：160f；参数位置决定它表示的坐标、速度或选项。
                160f,
                // 这一行补充 25f，与相邻行合起来完成同一次计算或对象配置。
                25f
            // 继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 继续填写上一行调用的参数：item.itemName；参数位置决定它表示的坐标、速度或选项。
            item.itemName,
            // 这一行补充 nameStyle，与相邻行合起来完成同一次计算或对象配置。
            nameStyle
        );


        // 调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 继续填写上一行调用的参数：center.x - 80f；参数位置决定它表示的坐标、速度或选项。
                center.x - 80f,
                // 继续填写上一行调用的参数：center.y + 62f；参数位置决定它表示的坐标、速度或选项。
                center.y + 62f,
                // 继续填写上一行调用的参数：160f；参数位置决定它表示的坐标、速度或选项。
                160f,
                // 这一行补充 20f，与相邻行合起来完成同一次计算或对象配置。
                20f
            // 继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 继续填写上一行调用的参数："EXTRACTED"；参数位置决定它表示的坐标、速度或选项。
            "EXTRACTED",
            // 这一行补充 nameStyle，与相邻行合起来完成同一次计算或对象配置。
            nameStyle
        );
    }


    // =====================================================
    // SELECTED ITEM
    // =====================================================

    /// <summary>
    /// 显示当前选中物品的名称、说明或兑换状态。
    /// </summary>
    // ！！！功能：DrawSelectedItem——DrawSelected物品！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void DrawSelectedItem()
    {
        // 步骤 1：检查条件 selectedNode < 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if (selectedNode < 0)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 3：检查条件 selectedNode >= items.Count；只有为真才运行下面的步骤，否则沿原流程继续。
        if (selectedNode >= items.Count)
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 5：这一行补充 ItemNode item =，与相邻行合起来完成同一次计算或对象配置。
        ItemNode item =
            // 步骤 6：执行 items[selectedNode];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            items[selectedNode];


        // 步骤 7：这一行补充 GUIStyle panelStyle =，与相邻行合起来完成同一次计算或对象配置。
        GUIStyle panelStyle =
            // 步骤 8：用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
            new GUIStyle(
                // 步骤 9：这一行补充 GUI.skin.box，与相邻行合起来完成同一次计算或对象配置。
                GUI.skin.box
            );


        // 步骤 10：把计算结果 18 保存为fontSize；下一条判断或调用会读取这个具体值。
        panelStyle.fontSize = 18;

        // 步骤 11：这一行补充 panelStyle.alignment =，与相邻行合起来完成同一次计算或对象配置。
        panelStyle.alignment =
            // 步骤 12：执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TextAnchor.MiddleCenter;


        // 步骤 13：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(
            // 步骤 14：用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 步骤 15：继续填写上一行调用的参数：Screen.width - 350f；参数位置决定它表示的坐标、速度或选项。
                Screen.width - 350f,
                // 步骤 16：继续填写上一行调用的参数：100f；参数位置决定它表示的坐标、速度或选项。
                100f,
                // 步骤 17：继续填写上一行调用的参数：300f；参数位置决定它表示的坐标、速度或选项。
                300f,
                // 步骤 18：这一行补充 250f，与相邻行合起来完成同一次计算或对象配置。
                250f
            // 步骤 19：继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 步骤 20：这一行补充 ""，与相邻行合起来完成同一次计算或对象配置。
            ""
        );


        // 步骤 21：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 步骤 22：用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 步骤 23：继续填写上一行调用的参数：Screen.width - 330f；参数位置决定它表示的坐标、速度或选项。
                Screen.width - 330f,
                // 步骤 24：继续填写上一行调用的参数：120f；参数位置决定它表示的坐标、速度或选项。
                120f,
                // 步骤 25：继续填写上一行调用的参数：260f；参数位置决定它表示的坐标、速度或选项。
                260f,
                // 步骤 26：这一行补充 40f，与相邻行合起来完成同一次计算或对象配置。
                40f
            // 步骤 27：继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 步骤 28：继续填写上一行调用的参数：item.itemName；参数位置决定它表示的坐标、速度或选项。
            item.itemName,
            // 步骤 29：这一行补充 panelStyle，与相邻行合起来完成同一次计算或对象配置。
            panelStyle
        );


        // 步骤 30：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 步骤 31：用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 步骤 32：继续填写上一行调用的参数：Screen.width - 330f；参数位置决定它表示的坐标、速度或选项。
                Screen.width - 330f,
                // 步骤 33：继续填写上一行调用的参数：165f；参数位置决定它表示的坐标、速度或选项。
                165f,
                // 步骤 34：继续填写上一行调用的参数：260f；参数位置决定它表示的坐标、速度或选项。
                260f,
                // 步骤 35：这一行补充 30f，与相邻行合起来完成同一次计算或对象配置。
                30f
            // 步骤 36：继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 步骤 37：这一行补充 "Cost: " +，与相邻行合起来完成同一次计算或对象配置。
            "Cost: " +
            // 步骤 38：这一行补充 item.cost +，与相邻行合起来完成同一次计算或对象配置。
            item.cost +
            // 步骤 39：继续填写上一行调用的参数：" Candles"；参数位置决定它表示的坐标、速度或选项。
            " Candles",
            // 步骤 40：这一行补充 panelStyle，与相邻行合起来完成同一次计算或对象配置。
            panelStyle
        );


        // 步骤 41：声明 status（status）：文字内容；只供本脚本内部保存状态，赋值前是该类型的默认值。
        string status;


        // 步骤 42：检查条件 item.extracted；只有为真才运行下面的步骤，否则沿原流程继续。
        if (item.extracted)
        {
            // 步骤 43：这一行补充 status =，与相邻行合起来完成同一次计算或对象配置。
            status =
                // 步骤 44：把计算结果 = selectedNode ? "OBTAINED - IN HAND" : "OBTAINED" 保存为currentHeld物品Index；下一条判断或调用会读取这个具体值。
                currentHeldItemIndex == selectedNode ? "OBTAINED - IN HAND" : "OBTAINED";
        }
        // 步骤 45：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 item.unlocked；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (item.unlocked)
        {
            // 步骤 46：这一行补充 status =，与相邻行合起来完成同一次计算或对象配置。
            status =
                // 步骤 47：执行 "UNLOCKED";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "UNLOCKED";
        }
        // 步骤 48：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 49：这一行补充 status =，与相邻行合起来完成同一次计算或对象配置。
            status =
                // 步骤 50：执行 "LOCKED";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "LOCKED";
        }


        // 步骤 51：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 步骤 52：用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 步骤 53：继续填写上一行调用的参数：Screen.width - 330f；参数位置决定它表示的坐标、速度或选项。
                Screen.width - 330f,
                // 步骤 54：继续填写上一行调用的参数：205f；参数位置决定它表示的坐标、速度或选项。
                205f,
                // 步骤 55：继续填写上一行调用的参数：260f；参数位置决定它表示的坐标、速度或选项。
                260f,
                // 步骤 56：这一行补充 30f，与相邻行合起来完成同一次计算或对象配置。
                30f
            // 步骤 57：继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 步骤 58：继续填写上一行调用的参数：status；参数位置决定它表示的坐标、速度或选项。
            status,
            // 步骤 59：这一行补充 panelStyle，与相邻行合起来完成同一次计算或对象配置。
            panelStyle
        );


        // =================================================
        // UNLOCK
        // =================================================

        // 步骤 60：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (
            // 步骤 61：这一行补充 !item.unlocked &&，与相邻行合起来完成同一次计算或对象配置。
            !item.unlocked &&
            // 步骤 62：这一行补充 !item.extracted，与相邻行合起来完成同一次计算或对象配置。
            !item.extracted
        )
        {
            // 步骤 63：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (
                // 步骤 64：调用 Button：绘制可点击按钮，并根据点击结果触发对应操作。
                GUI.Button(
                    // 步骤 65：用 new 创建 Rect(；构造参数决定新对象的初始内容。
                    new Rect(
                        // 步骤 66：继续填写上一行调用的参数：Screen.width - 310f；参数位置决定它表示的坐标、速度或选项。
                        Screen.width - 310f,
                        // 步骤 67：继续填写上一行调用的参数：250f；参数位置决定它表示的坐标、速度或选项。
                        250f,
                        // 步骤 68：继续填写上一行调用的参数：220f；参数位置决定它表示的坐标、速度或选项。
                        220f,
                        // 步骤 69：这一行补充 40f，与相邻行合起来完成同一次计算或对象配置。
                        40f
                    // 步骤 70：继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
                    ),
                    // 步骤 71：这一行补充 "Unlock"，与相邻行合起来完成同一次计算或对象配置。
                    "Unlock"
                )
            )
            {
                // 步骤 72：调用 UnlockItem：Unlock物品。
                UnlockItem(
                    // 步骤 73：这一行补充 selectedNode，与相邻行合起来完成同一次计算或对象配置。
                    selectedNode
                );
            }
        }
    }
    // ！！！功能结束：DrawSelectedItem！！！


    // =====================================================
    // INSTRUCTION
    // =====================================================

    /// <summary>
    /// 根据当前交互状态显示底部操作提示。
    /// </summary>
    // ！！！功能：DrawInstruction——DrawInstruction！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void DrawInstruction()
    {
        // 步骤 1：这一行补充 GUIStyle style =，与相邻行合起来完成同一次计算或对象配置。
        GUIStyle style =
            // 步骤 2：用 new 创建 GUIStyle(；构造参数决定新对象的初始内容。
            new GUIStyle(
                // 步骤 3：这一行补充 GUI.skin.label，与相邻行合起来完成同一次计算或对象配置。
                GUI.skin.label
            );


        // 步骤 4：把计算结果 16 保存为fontSize；下一条判断或调用会读取这个具体值。
        style.fontSize = 16;

        // 步骤 5：这一行补充 style.alignment =，与相邻行合起来完成同一次计算或对象配置。
        style.alignment =
            // 步骤 6：执行 TextAnchor.MiddleCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            TextAnchor.MiddleCenter;


        // 步骤 7：声明 text（text）：文字内容；只供本脚本内部保存状态，赋值前是该类型的默认值。
        string text;


        // 步骤 8：确认当前手持的物品已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (currentHeldObject != null)
        {
            // 步骤 9：这一行补充 text =，与相邻行合起来完成同一次计算或对象配置。
            text =
                // 步骤 10：执行 "E: Equip     Q: Throw     ESC: Close";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "E: Equip     Q: Throw     ESC: Close";
        }
        // 步骤 11：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 12：这一行补充 text =，与相邻行合起来完成同一次计算或对象配置。
            text =
                // 步骤 13：执行 "Click and hold an unlocked item to obtain it     ESC: Close";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Click and hold an unlocked item to obtain it     ESC: Close";
        }


        // 步骤 14：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(
            // 步骤 15：用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 步骤 16：继续填写上一行调用的参数：Screen.width / 2f - 450f；参数位置决定它表示的坐标、速度或选项。
                Screen.width / 2f - 450f,
                // 步骤 17：继续填写上一行调用的参数：Screen.height - 90f；参数位置决定它表示的坐标、速度或选项。
                Screen.height - 90f,
                // 步骤 18：继续填写上一行调用的参数：900f；参数位置决定它表示的坐标、速度或选项。
                900f,
                // 步骤 19：这一行补充 40f，与相邻行合起来完成同一次计算或对象配置。
                40f
            // 步骤 20：继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 步骤 21：继续填写上一行调用的参数：text；参数位置决定它表示的坐标、速度或选项。
            text,
            // 步骤 22：这一行补充 style，与相邻行合起来完成同一次计算或对象配置。
            style
        );
    }
    // ！！！功能结束：DrawInstruction！！！


    // =====================================================
    // UNLOCK
    // =====================================================

    /// <summary>
    /// 把指定节点标记为已解锁，使其可参与后续兑换。
    /// </summary>
    // ！！！功能：UnlockItem——Unlock物品！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void UnlockItem(int index)
    {
        // 步骤 1：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (
            // 步骤 2：这一行补充 index < 0 ||，与相邻行合起来完成同一次计算或对象配置。
            index < 0 ||
            // 步骤 3：这一行补充 index >= items.Count，与相邻行合起来完成同一次计算或对象配置。
            index >= items.Count
        )
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 5：检查条件 items[index].extracted；只有为真才运行下面的步骤，否则沿原流程继续。
        if (items[index].extracted)
            // 步骤 6：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 7：这一行补充 items[index].unlocked =，与相邻行合起来完成同一次计算或对象配置。
        items[index].unlocked =
            // 步骤 8：执行 true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            true;


        // 步骤 9：这一行补充 selectedNode =，与相邻行合起来完成同一次计算或对象配置。
        selectedNode =
            // 步骤 10：执行 index;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            index;


        // 步骤 11：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 12：这一行补充 "Item unlocked: " +，与相邻行合起来完成同一次计算或对象配置。
            "Item unlocked: " +
            // 步骤 13：这一行补充 items[index].itemName，与相邻行合起来完成同一次计算或对象配置。
            items[index].itemName
        );
    }
    // ！！！功能结束：UnlockItem！！！


    // =====================================================
    // EXTRACT ITEM
    // =====================================================

    /// <summary>
    /// 从模板生成物品实例，放到摄像机前的手持点，关闭物理并记录领取状态。
    /// </summary>
    // ！！！功能：ExtractItem——Extract物品！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void ExtractItem(int index)
    {
        // 步骤 1：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (
            // 步骤 2：这一行补充 index < 0 ||，与相邻行合起来完成同一次计算或对象配置。
            index < 0 ||
            // 步骤 3：这一行补充 index >= items.Count，与相邻行合起来完成同一次计算或对象配置。
            index >= items.Count
        )
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 5：确认当前手持的物品已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (currentHeldObject != null)
        {
            // 步骤 6：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log(
                // 步骤 7：这一行补充 "You can only carry one item at a time."，与相邻行合起来完成同一次计算或对象配置。
                "You can only carry one item at a time."
            );

            // 步骤 8：提前结束这个方法；不再执行下方语句。
            return;
        }


        // 步骤 9：这一行补充 ItemNode item =，与相邻行合起来完成同一次计算或对象配置。
        ItemNode item =
            // 步骤 10：执行 items[index];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            items[index];


        // 步骤 11：这里的 ! 表示取反；当 item.unlocked 不成立时才进入此分支。
        if (!item.unlocked)
            // 步骤 12：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 13：检查条件 item.extracted；只有为真才运行下面的步骤，否则沿原流程继续。
        if (item.extracted)
            // 步骤 14：提前结束这个方法；不再执行下方语句。
            return;


        // =================================================
        // GET TEMPLATE
        // =================================================

        // 步骤 15：这一行补充 GameObject template =，与相邻行合起来完成同一次计算或对象配置。
        GameObject template =
            // 步骤 16：调用 GetModelTemplate：获取ModelTemplate。
            GetModelTemplate(
                // 步骤 17：这一行补充 item.itemName，与相邻行合起来完成同一次计算或对象配置。
                item.itemName
            );


        // 步骤 18：检查template是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (template == null)
        {
            // 步骤 19：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError(
                // 步骤 20：这一行补充 "NO MODEL ASSIGNED FOR: " +，与相邻行合起来完成同一次计算或对象配置。
                "NO MODEL ASSIGNED FOR: " +
                // 步骤 21：这一行补充 item.itemName，与相邻行合起来完成同一次计算或对象配置。
                item.itemName
            );

            // 步骤 22：提前结束这个方法；不再执行下方语句。
            return;
        }


        // =================================================
        // CREATE OBJECT
        // =================================================

        // 步骤 23：这一行补充 GameObject newObject =，与相邻行合起来完成同一次计算或对象配置。
        GameObject newObject =
            // 步骤 24：调用 Instantiate：Instantiate。
            Instantiate(
                // 步骤 25：这一行补充 template，与相邻行合起来完成同一次计算或对象配置。
                template
            );


        // 步骤 26：这一行补充 newObject.name =，与相邻行合起来完成同一次计算或对象配置。
        newObject.name =
            // 步骤 27：这一行补充 item.itemName +，与相邻行合起来完成同一次计算或对象配置。
            item.itemName +
            // 步骤 28：执行 "_Held";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "_Held";


        // 步骤 29：显示这个场景对象及其子对象；界面会立即反映这一状态。
        newObject.SetActive(true);


        // =================================================
        // PUT IN FRONT OF CAMERA
        // =================================================

        // 步骤 30：确认itemHoldPoint已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (itemHoldPoint != null)
        {
            // 步骤 31：这一行补充 newObject.transform.SetParent(，与相邻行合起来完成同一次计算或对象配置。
            newObject.transform.SetParent(
                // 步骤 32：继续填写上一行调用的参数：itemHoldPoint；参数位置决定它表示的坐标、速度或选项。
                itemHoldPoint,
                // 步骤 33：这一行补充 false，与相邻行合起来完成同一次计算或对象配置。
                false
            );


            // 步骤 34：这一行补充 newObject.transform.localPosition =，与相邻行合起来完成同一次计算或对象配置。
            newObject.transform.localPosition =
                // 步骤 35：执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.zero;


            // 步骤 36：这一行补充 newObject.transform.localRotation =，与相邻行合起来完成同一次计算或对象配置。
            newObject.transform.localRotation =
                // 步骤 37：执行 Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Quaternion.identity;


            // 步骤 38：这一行补充 newObject.transform.localScale =，与相邻行合起来完成同一次计算或对象配置。
            newObject.transform.localScale =
                // 步骤 39：执行 template.transform.localScale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                template.transform.localScale;
        }
        // 步骤 40：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 41：这一行补充 Camera cam =，与相邻行合起来完成同一次计算或对象配置。
            Camera cam =
                // 步骤 42：执行 Camera.main;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Camera.main;


            // 步骤 43：确认cam已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (cam != null)
            {
                // 步骤 44：这一行补充 newObject.transform.position =，与相邻行合起来完成同一次计算或对象配置。
                newObject.transform.position =
                    // 步骤 45：这一行补充 cam.transform.position +，与相邻行合起来完成同一次计算或对象配置。
                    cam.transform.position +
                    // 步骤 46：这一行补充 cam.transform.forward *，与相邻行合起来完成同一次计算或对象配置。
                    cam.transform.forward *
                    // 步骤 47：执行 defaultHoldDistance;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    defaultHoldDistance;


                // 步骤 48：这一行补充 newObject.transform.rotation =，与相邻行合起来完成同一次计算或对象配置。
                newObject.transform.rotation =
                    // 步骤 49：执行 cam.transform.rotation;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    cam.transform.rotation;
            }
        }


        // =================================================
        // DISABLE PHYSICS
        // =================================================

        // 步骤 50：调用 DisablePhysics：DisablePhysics。
        DisablePhysics(
            // 步骤 51：这一行补充 newObject，与相邻行合起来完成同一次计算或对象配置。
            newObject
        );


        // =================================================
        // SAVE CURRENT ITEM
        // =================================================

        // 步骤 52：这一行补充 currentHeldObject =，与相邻行合起来完成同一次计算或对象配置。
        currentHeldObject =
            // 步骤 53：执行 newObject;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            newObject;


        // 步骤 54：这一行补充 currentHeldItemIndex =，与相邻行合起来完成同一次计算或对象配置。
        currentHeldItemIndex =
            // 步骤 55：执行 index;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            index;


        // =================================================
        // MARK EXTRACTED
        // =================================================

        // 步骤 56：执行 items[index].extracted = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        items[index].extracted = true;
        // 步骤 57：把计算结果 index 保存为当前选中的树节点；下一条判断或调用会读取这个具体值。
        selectedNode = index;
        // 步骤 58：把计算结果 GetScreenPosition(item.position) 保存为collectionAnimation原点；下一条判断或调用会读取这个具体值。
        collectionAnimationOrigin = GetScreenPosition(item.position);
        // 步骤 59：把计算结果 item.icon 保存为collectionAnimationIcon；下一条判断或调用会读取这个具体值。
        collectionAnimationIcon = item.icon;
        // 步骤 60：把计算结果 Time.realtimeSinceStartup 保存为collectionAnimationStart；下一条判断或调用会读取这个具体值。
        collectionAnimationStart = Time.realtimeSinceStartup;
        // 步骤 61：调用 SetFeedback：设置Feedback。
        SetFeedback(item.itemName + " Obtained - now in your hand. Press E to equip.", feedbackDuration);
        // 步骤 62：调用 PlayConfirmationChime：PlayConfirmationChime。
        PlayConfirmationChime();


        // 步骤 63：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 64：这一行补充 "================================"，与相邻行合起来完成同一次计算或对象配置。
            "================================"
        );

        // 步骤 65：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 66：这一行补充 "ITEM EXTRACTED: " +，与相邻行合起来完成同一次计算或对象配置。
            "ITEM EXTRACTED: " +
            // 步骤 67：这一行补充 item.itemName，与相邻行合起来完成同一次计算或对象配置。
            item.itemName
        );

        // 步骤 68：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 69：这一行补充 "Hold Point: " +，与相邻行合起来完成同一次计算或对象配置。
            "Hold Point: " +
            // 步骤 70：这一行补充 (，与相邻行合起来完成同一次计算或对象配置。
            (
                // 步骤 71：这一行补充 itemHoldPoint != null，与相邻行合起来完成同一次计算或对象配置。
                itemHoldPoint != null
                // 步骤 72：三元表达式中，前面的条件为真时选这个值。
                ? itemHoldPoint.name
                // 步骤 73：三元表达式中，前面的条件为假时选这个备用值。
                : "NONE"
            )
        );

        // 步骤 74：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 75：这一行补充 "================================"，与相邻行合起来完成同一次计算或对象配置。
            "================================"
        );
    }
    // ！！！功能结束：ExtractItem！！！


    // =====================================================
    // GET MODEL
    // =====================================================

    /// <summary>
    /// 将兑换节点名称映射到 Inspector 中绑定的模型模板。
    /// </summary>
    // 这一行补充 GameObject GetModelTemplate(，与相邻行合起来完成同一次计算或对象配置。
    GameObject GetModelTemplate(
        // 这一行补充 string itemName)，与相邻行合起来完成同一次计算或对象配置。
        string itemName)
    {
        // 查看括号中的当前操作类型，跳到对应 case；其他动作不会一起执行。
        switch (itemName)
        {
            // 当操作等于 "Cape" 时运行这一支；本分支会在 break 处结束。
            case "Cape":
                // 把 capeModel 作为方法结果返回给调用方；return 后本方法立即结束。
                return capeModel;

            // 当操作等于 "Mask" 时运行这一支；本分支会在 break 处结束。
            case "Mask":
                // 把 maskModel 作为方法结果返回给调用方；return 后本方法立即结束。
                return maskModel;

            // 当操作等于 "Emote" 时运行这一支；本分支会在 break 处结束。
            case "Emote":
                // 把 emoteModel 作为方法结果返回给调用方；return 后本方法立即结束。
                return emoteModel;

            // 当操作等于 "Hair" 时运行这一支；本分支会在 break 处结束。
            case "Hair":
                // 把 hairModel 作为方法结果返回给调用方；return 后本方法立即结束。
                return hairModel;

            // 当操作等于 "Instrument" 时运行这一支；本分支会在 break 处结束。
            case "Instrument":
                // 把 instrumentModel 作为方法结果返回给调用方；return 后本方法立即结束。
                return instrumentModel;
        }


        // 没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }


    // =====================================================
    // DISABLE PHYSICS
    // =====================================================

    /// <summary>
    /// 手持或装备物品时关闭刚体运动及碰撞，避免物体与角色相互推挤。
    /// </summary>
    // 这一行补充 void DisablePhysics(，与相邻行合起来完成同一次计算或对象配置。
    void DisablePhysics(
        // 这一行补充 GameObject obj)，与相邻行合起来完成同一次计算或对象配置。
        GameObject obj)
    {
        // 检查obj是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (obj == null)
            // 提前结束这个方法；不再执行下方语句。
            return;


        // 这一行补充 Rigidbody[] rigidbodies =，与相邻行合起来完成同一次计算或对象配置。
        Rigidbody[] rigidbodies =
            // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            obj.GetComponentsInChildren<Rigidbody>(
                // 这一行补充 true，与相邻行合起来完成同一次计算或对象配置。
                true
            );


        // 逐个读取  中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (
            // 这一行补充 Rigidbody rb，与相邻行合起来完成同一次计算或对象配置。
            Rigidbody rb
            // 这一行补充 in rigidbodies，与相邻行合起来完成同一次计算或对象配置。
            in rigidbodies
        )
        {
            // 把 rb.isKinematic 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            rb.isKinematic = true;

            // 把 rb.useGravity 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            rb.useGravity = false;

            // 这一行补充 rb.linearVelocity =，与相邻行合起来完成同一次计算或对象配置。
            rb.linearVelocity =
                // 执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.zero;

            // 这一行补充 rb.angularVelocity =，与相邻行合起来完成同一次计算或对象配置。
            rb.angularVelocity =
                // 执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.zero;
        }


        // 这一行补充 Collider[] colliders =，与相邻行合起来完成同一次计算或对象配置。
        Collider[] colliders =
            // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            obj.GetComponentsInChildren<Collider>(
                // 这一行补充 true，与相邻行合起来完成同一次计算或对象配置。
                true
            );


        // 逐个读取  中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (
            // 这一行补充 Collider col，与相邻行合起来完成同一次计算或对象配置。
            Collider col
            // 这一行补充 in colliders，与相邻行合起来完成同一次计算或对象配置。
            in colliders
        )
        {
            // 把 col.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            col.enabled = false;
        }
    }


    // =====================================================
    // ENABLE PHYSICS
    // =====================================================

    /// <summary>
    /// 投出物品前恢复刚体和碰撞，使其重新受重力影响。
    /// </summary>
    // 这一行补充 void EnablePhysics(，与相邻行合起来完成同一次计算或对象配置。
    void EnablePhysics(
        // 这一行补充 GameObject obj)，与相邻行合起来完成同一次计算或对象配置。
        GameObject obj)
    {
        // 检查obj是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (obj == null)
            // 提前结束这个方法；不再执行下方语句。
            return;


        // 这一行补充 Rigidbody rb =，与相邻行合起来完成同一次计算或对象配置。
        Rigidbody rb =
            // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            obj.GetComponent<Rigidbody>();


        // 检查rb是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (rb == null)
        {
            // 这一行补充 rb =，与相邻行合起来完成同一次计算或对象配置。
            rb =
                // 执行 obj.AddComponent<Rigidbody>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                obj.AddComponent<Rigidbody>();
        }


        // 这一行补充 Collider[] colliders =，与相邻行合起来完成同一次计算或对象配置。
        Collider[] colliders =
            // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            obj.GetComponentsInChildren<Collider>(
                // 这一行补充 true，与相邻行合起来完成同一次计算或对象配置。
                true
            );


        // 逐个读取  中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (
            // 这一行补充 Collider col，与相邻行合起来完成同一次计算或对象配置。
            Collider col
            // 这一行补充 in colliders，与相邻行合起来完成同一次计算或对象配置。
            in colliders
        )
        {
            // 把 col.enabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            col.enabled = true;
        }


        // 把 rb.isKinematic 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        rb.isKinematic = false;

        // 把 rb.useGravity 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        rb.useGravity = true;
    }


    // =====================================================
    // EQUIP
    // =====================================================

    /// <summary>
    /// 把当前手持物从镜头前移到角色装备点，随后清空手持槽。
    /// </summary>
    // ！！！功能：EquipCurrentItem——EquipCurrent物品！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void EquipCurrentItem()
    {
        // 步骤 1：检查当前手持的物品是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (currentHeldObject == null)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 3：检查条件 currentHeldItemIndex < 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if (currentHeldItemIndex < 0)
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 5：这一行补充 string itemName =，与相邻行合起来完成同一次计算或对象配置。
        string itemName =
            // 步骤 6：这一行补充 items[currentHeldItemIndex]，与相邻行合起来完成同一次计算或对象配置。
            items[currentHeldItemIndex]
                // 步骤 7：这是上一行链式调用的下一步 .itemName;；前一结果会继续传给这里筛选或处理。
                .itemName;


        // -------------------------------------------------
        // Remove Camera Parent
        // -------------------------------------------------

        // 步骤 8：这一行补充 currentHeldObject.transform.SetParent(，与相邻行合起来完成同一次计算或对象配置。
        currentHeldObject.transform.SetParent(
            // 步骤 9：这一行补充 null，与相邻行合起来完成同一次计算或对象配置。
            null
        );


        // -------------------------------------------------
        // Disable Physics
        // -------------------------------------------------

        // 步骤 10：调用 DisablePhysics：DisablePhysics。
        DisablePhysics(
            // 步骤 11：这一行补充 currentHeldObject，与相邻行合起来完成同一次计算或对象配置。
            currentHeldObject
        );


        // -------------------------------------------------
        // Find Equipment Point
        // -------------------------------------------------

        // 步骤 12：这一行补充 Transform equipPoint =，与相邻行合起来完成同一次计算或对象配置。
        Transform equipPoint =
            // 步骤 13：调用 FindEquipmentPoint：查找EquipmentPoint。
            FindEquipmentPoint(
                // 步骤 14：这一行补充 itemName，与相邻行合起来完成同一次计算或对象配置。
                itemName
            );


        // 步骤 15：确认equipPoint已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (equipPoint != null)
        {
            // 步骤 16：这一行补充 currentHeldObject.transform.SetParent(，与相邻行合起来完成同一次计算或对象配置。
            currentHeldObject.transform.SetParent(
                // 步骤 17：继续填写上一行调用的参数：equipPoint；参数位置决定它表示的坐标、速度或选项。
                equipPoint,
                // 步骤 18：这一行补充 false，与相邻行合起来完成同一次计算或对象配置。
                false
            );


            // 步骤 19：这一行补充 currentHeldObject.transform.localPosition =，与相邻行合起来完成同一次计算或对象配置。
            currentHeldObject.transform.localPosition =
                // 步骤 20：执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.zero;


            // 步骤 21：这一行补充 currentHeldObject.transform.localRotation =，与相邻行合起来完成同一次计算或对象配置。
            currentHeldObject.transform.localRotation =
                // 步骤 22：执行 Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Quaternion.identity;
        }
        // 步骤 23：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 24：确认player已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (player != null)
            {
                // 步骤 25：这一行补充 currentHeldObject.transform.SetParent(，与相邻行合起来完成同一次计算或对象配置。
                currentHeldObject.transform.SetParent(
                    // 步骤 26：继续填写上一行调用的参数：player；参数位置决定它表示的坐标、速度或选项。
                    player,
                    // 步骤 27：这一行补充 true，与相邻行合起来完成同一次计算或对象配置。
                    true
                );
            }
        }


        // 步骤 28：把计算结果 itemName 保存为lastEquipped物品Name；下一条判断或调用会读取这个具体值。
        lastEquippedItemName = itemName;
        // 步骤 29：调用 SetFeedback：设置Feedback。
        SetFeedback(itemName + " Equipped", feedbackDuration);
        // 步骤 30：调用 PlayConfirmationChime：PlayConfirmationChime。
        PlayConfirmationChime();

        // 步骤 31：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 32：这一行补充 "Equipped item: " +，与相邻行合起来完成同一次计算或对象配置。
            "Equipped item: " +
            // 步骤 33：这一行补充 itemName，与相邻行合起来完成同一次计算或对象配置。
            itemName
        );


        // 步骤 34：清空 currentHeldObject 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        currentHeldObject = null;

        // 步骤 35：把计算结果 -1 保存为currentHeld物品Index；下一条判断或调用会读取这个具体值。
        currentHeldItemIndex = -1;


        // 步骤 36：调用 CloseTree：CloseTree。
        CloseTree();
    }
    // ！！！功能结束：EquipCurrentItem！！！


    // =====================================================
    // EQUIPMENT POINT
    // =====================================================

    /// <summary>
    /// 按物品名称寻找角色上的专用挂点；鹿头优先使用 MaskHoldPoint。
    /// </summary>
    // 这一行补充 Transform FindEquipmentPoint(，与相邻行合起来完成同一次计算或对象配置。
    Transform FindEquipmentPoint(
        // 这一行补充 string itemName)，与相邻行合起来完成同一次计算或对象配置。
        string itemName)
    {
        // 检查player是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (player == null)
            // 没有找到有效结果，返回 null；调用方要检查后才能继续使用。
            return null;


        // -------------------------------------------------
        // MASK
        // -------------------------------------------------

        // 检查条件 itemName == "Mask"；只有为真才运行下面的步骤，否则沿原流程继续。
        if (itemName == "Mask")
        {
            // 这一行补充 Transform point =，与相邻行合起来完成同一次计算或对象配置。
            Transform point =
                // 调用 FindDeepChild：查找DeepChild。
                FindDeepChild(
                    // 继续填写上一行调用的参数：player；参数位置决定它表示的坐标、速度或选项。
                    player,
                    // 这一行补充 "MaskHoldPoint"，与相邻行合起来完成同一次计算或对象配置。
                    "MaskHoldPoint"
                );


            // 确认point已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (point != null)
                // 把 point 作为方法结果返回给调用方；return 后本方法立即结束。
                return point;
        }


        // -------------------------------------------------
        // OTHER ITEMS
        // -------------------------------------------------

        // 这一行补充 Transform generic =，与相邻行合起来完成同一次计算或对象配置。
        Transform generic =
            // 调用 FindDeepChild：查找DeepChild。
            FindDeepChild(
                // 继续填写上一行调用的参数：player；参数位置决定它表示的坐标、速度或选项。
                player,
                // 这一行补充 itemName +，与相邻行合起来完成同一次计算或对象配置。
                itemName +
                // 这一行补充 "HoldPoint"，与相邻行合起来完成同一次计算或对象配置。
                "HoldPoint"
            );


        // 确认generic已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (generic != null)
            // 把 generic 作为方法结果返回给调用方；return 后本方法立即结束。
            return generic;


        // 没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }


    // =====================================================
    // FIND CHILD
    // =====================================================

    /// <summary>
    /// 在指定父物体的全部子层级中按名称查找挂点，包括未激活物体。
    /// </summary>
    // 这一行补充 Transform FindDeepChild(，与相邻行合起来完成同一次计算或对象配置。
    Transform FindDeepChild(
        // 继续填写上一行调用的参数：Transform parent；参数位置决定它表示的坐标、速度或选项。
        Transform parent,
        // 这一行补充 string childName)，与相邻行合起来完成同一次计算或对象配置。
        string childName)
    {
        // 这一行补充 Transform[] children =，与相邻行合起来完成同一次计算或对象配置。
        Transform[] children =
            // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            parent.GetComponentsInChildren<Transform>(
                // 这一行补充 true，与相邻行合起来完成同一次计算或对象配置。
                true
            );


        // 逐个读取  中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (
            // 这一行补充 Transform child，与相邻行合起来完成同一次计算或对象配置。
            Transform child
            // 这一行补充 in children，与相邻行合起来完成同一次计算或对象配置。
            in children
        )
        {
            // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (
                // 把计算结果 = 保存为name；下一条判断或调用会读取这个具体值。
                child.name ==
                // 这一行补充 childName，与相邻行合起来完成同一次计算或对象配置。
                childName
            )
            {
                // 把 child 作为方法结果返回给调用方；return 后本方法立即结束。
                return child;
            }
        }


        // 没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }


    // =====================================================
    // THROW
    // =====================================================

    /// <summary>
    /// 从摄像机手持点解除物品，恢复物理并沿视线方向施加投掷冲量。
    /// </summary>
    // ！！！功能：ThrowCurrentItem——ThrowCurrent物品！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void ThrowCurrentItem()
    {
        // 步骤 1：检查当前手持的物品是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (currentHeldObject == null)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 3：这一行补充 Camera cam =，与相邻行合起来完成同一次计算或对象配置。
        Camera cam =
            // 步骤 4：执行 Camera.main;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Camera.main;


        // 步骤 5：检查cam是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (cam == null)
            // 步骤 6：提前结束这个方法；不再执行下方语句。
            return;


        // -------------------------------------------------
        // Remove Camera Parent
        // -------------------------------------------------

        // 步骤 7：这一行补充 currentHeldObject.transform.SetParent(，与相邻行合起来完成同一次计算或对象配置。
        currentHeldObject.transform.SetParent(
            // 步骤 8：这一行补充 null，与相邻行合起来完成同一次计算或对象配置。
            null
        );


        // -------------------------------------------------
        // Move in Front of Camera
        // -------------------------------------------------

        // 步骤 9：这一行补充 currentHeldObject.transform.position =，与相邻行合起来完成同一次计算或对象配置。
        currentHeldObject.transform.position =
            // 步骤 10：这一行补充 cam.transform.position +，与相邻行合起来完成同一次计算或对象配置。
            cam.transform.position +
            // 步骤 11：这一行补充 cam.transform.forward *，与相邻行合起来完成同一次计算或对象配置。
            cam.transform.forward *
            // 步骤 12：执行 1.2f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            1.2f;


        // 步骤 13：这一行补充 currentHeldObject.transform.rotation =，与相邻行合起来完成同一次计算或对象配置。
        currentHeldObject.transform.rotation =
            // 步骤 14：执行 cam.transform.rotation;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            cam.transform.rotation;


        // -------------------------------------------------
        // Enable Physics
        // -------------------------------------------------

        // 步骤 15：调用 EnablePhysics：EnablePhysics。
        EnablePhysics(
            // 步骤 16：这一行补充 currentHeldObject，与相邻行合起来完成同一次计算或对象配置。
            currentHeldObject
        );


        // 步骤 17：这一行补充 Rigidbody rb =，与相邻行合起来完成同一次计算或对象配置。
        Rigidbody rb =
            // 步骤 18：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            currentHeldObject.GetComponent<Rigidbody>();


        // 步骤 19：确认rb已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (rb != null)
        {
            // 步骤 20：这一行补充 Vector3 force =，与相邻行合起来完成同一次计算或对象配置。
            Vector3 force =
                // 步骤 21：这一行补充 cam.transform.forward *，与相邻行合起来完成同一次计算或对象配置。
                cam.transform.forward *
                // 步骤 22：执行 throwForce;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                throwForce;


            // 步骤 23：这一行补充 force +=，与相邻行合起来完成同一次计算或对象配置。
            force +=
                // 步骤 24：这一行补充 Vector3.up *，与相邻行合起来完成同一次计算或对象配置。
                Vector3.up *
                // 步骤 25：执行 upwardThrowForce;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                upwardThrowForce;


            // 步骤 26：这一行补充 rb.linearVelocity =，与相邻行合起来完成同一次计算或对象配置。
            rb.linearVelocity =
                // 步骤 27：执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.zero;


            // 步骤 28：这一行补充 rb.angularVelocity =，与相邻行合起来完成同一次计算或对象配置。
            rb.angularVelocity =
                // 步骤 29：执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.zero;


            // 步骤 30：调用 AddForce：添加Force。
            rb.AddForce(
                // 步骤 31：继续填写上一行调用的参数：force；参数位置决定它表示的坐标、速度或选项。
                force,
                // 步骤 32：这一行补充 ForceMode.Impulse，与相邻行合起来完成同一次计算或对象配置。
                ForceMode.Impulse
            );
        }


        // 步骤 33：执行 items[currentHeldItemIndex].extracted = false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        items[currentHeldItemIndex].extracted = false;
        // 步骤 34：调用 SetFeedback：设置Feedback。
        SetFeedback(items[currentHeldItemIndex].itemName + " thrown - obtain it again from the tree.", feedbackDuration);
        // 步骤 35：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 36：这一行补充 "Thrown item."，与相邻行合起来完成同一次计算或对象配置。
            "Thrown item."
        );


        // 步骤 37：清空 currentHeldObject 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        currentHeldObject = null;

        // 步骤 38：把计算结果 -1 保存为currentHeld物品Index；下一条判断或调用会读取这个具体值。
        currentHeldItemIndex = -1;


        // 步骤 39：调用 CloseTree：CloseTree。
        CloseTree();
    }
    // ！！！功能结束：ThrowCurrentItem！！！


    // =====================================================
    // VIRTUAL KEYBOARD
    // =====================================================

    /// <summary>
    /// 切换虚拟键盘的显示状态，并同步内部开关标记。
    /// </summary>
    // ！！！功能：ToggleVirtualKeyboard——ToggleVirtualKeyboard！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void ToggleVirtualKeyboard()
    {
        // 步骤 1：这一行补充 keyboardOpen =，与相邻行合起来完成同一次计算或对象配置。
        keyboardOpen =
            // 步骤 2：执行 !keyboardOpen;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            !keyboardOpen;


        // 步骤 3：确认virtualKeyboard已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (virtualKeyboard != null)
        {
            // 步骤 4：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            virtualKeyboard.SetActive(
                // 步骤 5：这一行补充 keyboardOpen，与相邻行合起来完成同一次计算或对象配置。
                keyboardOpen
            );
        }


        // 步骤 6：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 7：这一行补充 "Virtual Keyboard: " +，与相邻行合起来完成同一次计算或对象配置。
            "Virtual Keyboard: " +
            // 步骤 8：这一行补充 (，与相邻行合起来完成同一次计算或对象配置。
            (
                // 步骤 9：这一行补充 keyboardOpen，与相邻行合起来完成同一次计算或对象配置。
                keyboardOpen
                // 步骤 10：三元表达式中，前面的条件为真时选这个值。
                ? "OPEN"
                // 步骤 11：三元表达式中，前面的条件为假时选这个备用值。
                : "CLOSED"
            )
        );
    }
    // ！！！功能结束：ToggleVirtualKeyboard！！！


    /// <summary>
    /// 暂存提示文字及过期时间，供后续界面绘制。
    /// </summary>
    // ！！！功能：SetFeedback——设置Feedback！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string message, float duration 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetFeedback(string message, float duration)
    {
        // 步骤 1：把计算结果 message 保存为feedbackMessage；下一条判断或调用会读取这个具体值。
        feedbackMessage = message;
        // 步骤 2：把计算结果 Time.realtimeSinceStartup + duration 保存为feedbackUntil；下一条判断或调用会读取这个具体值。
        feedbackUntil = Time.realtimeSinceStartup + duration;
    }
    // ！！！功能结束：SetFeedback！！！

    /// <summary>
    /// 玩家接近先祖但尚未打开兑换树时显示交互引导。
    /// </summary>
    // ！！！功能：DrawApproachPrompt——DrawApproachPrompt！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawApproachPrompt()
    {
        // 步骤 1：检查player、当前手持的物品是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (player == null || currentHeldObject != null) return;
        // 步骤 2：计算 Vector3.Distance(player.position, transform.position)，把结果存进 distance（distance）；之后引用 distance 时用的就是这次结果。
        float distance = Vector3.Distance(player.position, transform.position);
        // 步骤 3：检查条件 distance > approachPromptDistance；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (distance > approachPromptDistance) return;

        // 步骤 4：用 canOpen 记住此处真假结果（distance <= interactionDistance），供下一条 if 选择执行路径。
        bool canOpen = distance <= interactionDistance;
        // 步骤 5：把计算结果 canOpen 保存为message；下一条判断或调用会读取这个具体值。
        string message = canOpen
            // 步骤 6：三元表达式中，前面的条件为真时选这个值。
            ? "ANCESTOR  •  Press E to open the item tree"
            // 步骤 7：三元表达式中，前面的条件为假时选这个备用值。
            : "ANCESTOR  •  Move closer to explore items";
        // 步骤 8：创建新的 GUIStyle 并存为 style；构造参数决定其初始内容或对象名称。
        GUIStyle style = new GUIStyle(GUI.skin.box);
        // 步骤 9：把计算结果 18 保存为fontSize；下一条判断或调用会读取这个具体值。
        style.fontSize = 18;
        // 步骤 10：把计算结果 FontStyle.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
        style.fontStyle = FontStyle.Bold;
        // 步骤 11：把计算结果 TextAnchor.MiddleCenter 保存为alignment；下一条判断或调用会读取这个具体值。
        style.alignment = TextAnchor.MiddleCenter;
        // 步骤 12：执行 style.normal.textColor = promptColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        style.normal.textColor = promptColor;
        // 步骤 13：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(new Rect(Screen.width / 2f - 230f, Screen.height - 150f, 460f, 48f), message, style);

        // 步骤 14：计算 Camera.main，把结果存进 cam（cam）；之后引用 cam 时用的就是这次结果。
        Camera cam = Camera.main;
        // 步骤 15：检查cam是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (cam == null) return;
        // 步骤 16：计算 cam.WorldToScreenPoint(transform.position + Vector3.up * 2.5f)，把结果存进 point（point）；之后引用 point 时用的就是这次结果。
        Vector3 point = cam.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);
        // 步骤 17：检查条件 point.z <= 0f；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (point.z <= 0f) return;
        // 步骤 18：计算 x 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float x = Mathf.Clamp(point.x, 30f, Screen.width - 30f);
        // 步骤 19：计算 y 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float y = Mathf.Clamp(Screen.height - point.y, 30f, Screen.height - 180f);
        // 步骤 20：把计算结果 canOpen ? promptColor : Color.white 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = canOpen ? promptColor : Color.white;
        // 步骤 21：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(x - 50f, y - 25f, 100f, 50f), "✦ ANCESTOR", style);
        // 步骤 22：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = Color.white;
    }
    // ！！！功能结束：DrawApproachPrompt！！！

    /// <summary>
    /// 在界面上显示当前持有或装备的物品状态。
    /// </summary>
    // ！！！功能：DrawItemStatusBadge——Draw物品状态Badge！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawItemStatusBadge()
    {
        // 步骤 1：把计算结果 currentHeldObject != null && currentHeldItemIndex >= 0 保存为status；下一条判断或调用会读取这个具体值。
        string status = currentHeldObject != null && currentHeldItemIndex >= 0
            // 步骤 2：三元表达式中，前面的条件为真时选这个值。
            ? items[currentHeldItemIndex].itemName + "  •  IN HAND  •  E to equip"
            // 步骤 3：三元表达式中，前面的条件为假时选这个备用值。
            : string.IsNullOrEmpty(lastEquippedItemName)
                // 步骤 4：三元表达式中，前面的条件为真时选这个值。
                ? null : lastEquippedItemName + "  •  EQUIPPED";
        // 步骤 5：检查status是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (status == null) return;
        // 步骤 6：创建新的 GUIStyle 并存为 style；构造参数决定其初始内容或对象名称。
        GUIStyle style = new GUIStyle(GUI.skin.box);
        // 步骤 7：把计算结果 15 保存为fontSize；下一条判断或调用会读取这个具体值。
        style.fontSize = 15;
        // 步骤 8：把计算结果 FontStyle.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
        style.fontStyle = FontStyle.Bold;
        // 步骤 9：把计算结果 TextAnchor.MiddleCenter 保存为alignment；下一条判断或调用会读取这个具体值。
        style.alignment = TextAnchor.MiddleCenter;
        // 步骤 10：执行 style.normal.textColor = promptColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        style.normal.textColor = promptColor;
        // 步骤 11：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(new Rect(Screen.width - 340f, 20f, 320f, 42f), status, style);
    }
    // ！！！功能结束：DrawItemStatusBadge！！！

    /// <summary>
    /// 在有效时间内绘制最近一次操作的提示文字。
    /// </summary>
    // ！！！功能：DrawFeedback——DrawFeedback！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawFeedback()
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：string.IsNullOrEmpty(feedbackMessage) || Time.realtimeSinceStartup >= feedbackUntil；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (string.IsNullOrEmpty(feedbackMessage) || Time.realtimeSinceStartup >= feedbackUntil) return;
        // 步骤 2：创建新的 GUIStyle 并存为 style；构造参数决定其初始内容或对象名称。
        GUIStyle style = new GUIStyle(GUI.skin.box);
        // 步骤 3：把计算结果 18 保存为fontSize；下一条判断或调用会读取这个具体值。
        style.fontSize = 18;
        // 步骤 4：把计算结果 FontStyle.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
        style.fontStyle = FontStyle.Bold;
        // 步骤 5：把计算结果 TextAnchor.MiddleCenter 保存为alignment；下一条判断或调用会读取这个具体值。
        style.alignment = TextAnchor.MiddleCenter;
        // 步骤 6：执行 style.normal.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        style.normal.textColor = Color.white;
        // 步骤 7：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(new Rect(Screen.width / 2f - 300f, 105f, 600f, 52f), feedbackMessage, style);
    }
    // ！！！功能结束：DrawFeedback！！！

    /// <summary>
    /// 绘制物品领取时从节点出发的短暂收集动画。
    /// </summary>
    // ！！！功能：DrawCollectionAnimation——DrawCollectionAnimation！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawCollectionAnimation()
    {
        // 步骤 1：检查条件 collectionAnimationStart < 0f；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (collectionAnimationStart < 0f) return;
        // 步骤 2：把计算结果 (Time.realtimeSinceStartup - collectionAnimationStart) / 保存为progress；下一条判断或调用会读取这个具体值。
        float progress = (Time.realtimeSinceStartup - collectionAnimationStart) /
            // 步骤 3：调用 Max：Max。
            Mathf.Max(0.1f, collectionAnimationDuration);
        // 步骤 4：检查条件 progress >= 1f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (progress >= 1f)
        {
            // 步骤 5：把计算结果 -1f 保存为collectionAnimationStart；下一条判断或调用会读取这个具体值。
            collectionAnimationStart = -1f;
            // 步骤 6：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 7：创建新的 Vector2 并存为 destination；构造参数决定其初始内容或对象名称。
        Vector2 destination = new Vector2(Screen.width - 120f, 75f);
        // 步骤 8：把计算结果 Vector2.Lerp(collectionAnimationOrigin, destination, 保存为position；下一条判断或调用会读取这个具体值。
        Vector2 position = Vector2.Lerp(collectionAnimationOrigin, destination,
            // 步骤 9：调用 SmoothStep：SmoothStep。
            Mathf.SmoothStep(0f, 1f, progress));
        // 步骤 10：创建新的 GUIStyle 并存为 style；构造参数决定其初始内容或对象名称。
        GUIStyle style = new GUIStyle(GUI.skin.box);
        // 步骤 11：把计算结果 28 保存为fontSize；下一条判断或调用会读取这个具体值。
        style.fontSize = 28;
        // 步骤 12：把计算结果 TextAnchor.MiddleCenter 保存为alignment；下一条判断或调用会读取这个具体值。
        style.alignment = TextAnchor.MiddleCenter;
        // 步骤 13：把计算结果 promptColor 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = promptColor;
        // 步骤 14：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(new Rect(position.x - 30f, position.y - 30f, 60f, 60f), collectionAnimationIcon, style);
        // 步骤 15：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = Color.white;
    }
    // ！！！功能结束：DrawCollectionAnimation！！！

    /// <summary>
    /// 生成不依赖外部音频文件的确认提示音。
    /// </summary>
    // ！！！功能：CreateConfirmationChime——创建ConfirmationChime！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 AudioClip 类型的计算结果；执行到 return 时把该值交回调用者。
    private AudioClip CreateConfirmationChime()
    {
        // 步骤 1：定义不可在运行时改变的数值 sampleRate（sampleRate）；后续计算统一使用这个阈值、速度或尺寸。
        const int sampleRate = 22050;
        // 步骤 2：定义不可在运行时改变的数值 seconds（seconds）；后续计算统一使用这个阈值、速度或尺寸。
        const float seconds = 0.22f;
        // 步骤 3：计算 Mathf.CeilToInt(sampleRate * seconds)，把结果存进 sampleCount（sampleCount）；之后引用 sampleCount 时用的就是这次结果。
        int sampleCount = Mathf.CeilToInt(sampleRate * seconds);
        // 步骤 4：创建新的 float[] 并存为 samples；构造参数决定其初始内容或对象名称。
        float[] samples = new float[sampleCount];
        // 步骤 5：按 int i = 0; i < sampleCount; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < sampleCount; i++)
        {
            // 步骤 6：计算 (float)i / sampleRate，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = (float)i / sampleRate;
            // 步骤 7：计算 Mathf.Sin(Mathf.PI * i / sampleCount)，把结果存进 envelope（envelope）；之后引用 envelope 时用的就是这次结果。
            float envelope = Mathf.Sin(Mathf.PI * i / sampleCount);
            // 步骤 8：执行 samples[i] = Mathf.Sin(2f * Mathf.PI * 660f * t) * envelope * 0.12f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            samples[i] = Mathf.Sin(2f * Mathf.PI * 660f * t) * envelope * 0.12f;
        }
        // 步骤 9：计算 AudioClip.Create("Sky confirmation chime", sampleCount, 1, sampleRate, false)，把结果存进 clip（clip）；之后引用 clip 时用的就是这次结果。
        AudioClip clip = AudioClip.Create("Sky confirmation chime", sampleCount, 1, sampleRate, false);
        // 步骤 10：调用 SetData：设置Data。
        clip.SetData(samples, 0);
        // 步骤 11：把 clip 作为方法结果返回给调用方；return 后本方法立即结束。
        return clip;
    }
    // ！！！功能结束：CreateConfirmationChime！！！

    /// <summary>
    /// 播放已生成的领取或装备确认音效。
    /// </summary>
    // ！！！功能：PlayConfirmationChime——PlayConfirmationChime！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PlayConfirmationChime()
    {
        // 步骤 1：检查feedback音频、feedbackClip是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (feedbackAudio != null && feedbackClip != null)
            // 步骤 2：调用 PlayOneShot：PlayOneShot。
            feedbackAudio.PlayOneShot(feedbackClip);
    }
    // ！！！功能结束：PlayConfirmationChime！！！

    // =====================================================
    // SCREEN POSITION
    // =====================================================

    /// <summary>
    /// 把节点的设计坐标换算为当前屏幕上的实际位置。
    /// </summary>
    // 这一行补充 Vector2 GetScreenPosition(，与相邻行合起来完成同一次计算或对象配置。
    Vector2 GetScreenPosition(
        // 这一行补充 Vector2 offset)，与相邻行合起来完成同一次计算或对象配置。
        Vector2 offset)
    {
        // 把 new Vector2( 作为方法结果返回给调用方；return 后本方法立即结束。
        return new Vector2(
            // 这一行补充 Screen.width / 2f +，与相邻行合起来完成同一次计算或对象配置。
            Screen.width / 2f +
            // 继续填写上一行调用的参数：offset.x；参数位置决定它表示的坐标、速度或选项。
            offset.x,

            // 这一行补充 Screen.height / 2f +，与相邻行合起来完成同一次计算或对象配置。
            Screen.height / 2f +
            // 这一行补充 offset.y，与相邻行合起来完成同一次计算或对象配置。
            offset.y
        );
    }


    // =====================================================
    // DRAW LINE
    // =====================================================

    /// <summary>
    /// 使用 GUI 绘制两个节点之间的连接线段。
    /// </summary>
    // 这一行补充 void DrawLine(，与相邻行合起来完成同一次计算或对象配置。
    void DrawLine(
        // 继续填写上一行调用的参数：Vector2 start；参数位置决定它表示的坐标、速度或选项。
        Vector2 start,
        // 继续填写上一行调用的参数：Vector2 end；参数位置决定它表示的坐标、速度或选项。
        Vector2 end,
        // 继续填写上一行调用的参数：float width；参数位置决定它表示的坐标、速度或选项。
        float width,
        // 这一行补充 Color color)，与相邻行合起来完成同一次计算或对象配置。
        Color color)
    {
        // 这一行补充 Matrix4x4 matrix =，与相邻行合起来完成同一次计算或对象配置。
        Matrix4x4 matrix =
            // 执行 GUI.matrix;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            GUI.matrix;


        // 这一行补充 float angle =，与相邻行合起来完成同一次计算或对象配置。
        float angle =
            // 调用 Atan2：Atan2。
            Mathf.Atan2(
                // 继续填写上一行调用的参数：end.y - start.y；参数位置决定它表示的坐标、速度或选项。
                end.y - start.y,
                // 这一行补充 end.x - start.x，与相邻行合起来完成同一次计算或对象配置。
                end.x - start.x
            )
            // 这一行补充 *，与相邻行合起来完成同一次计算或对象配置。
            *
            // 执行 Mathf.Rad2Deg;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Mathf.Rad2Deg;


        // 这一行补充 float length =，与相邻行合起来完成同一次计算或对象配置。
        float length =
            // 调用 Distance：Distance。
            Vector2.Distance(
                // 继续填写上一行调用的参数：start；参数位置决定它表示的坐标、速度或选项。
                start,
                // 这一行补充 end，与相邻行合起来完成同一次计算或对象配置。
                end
            );


        // 这一行补充 GUI.color =，与相邻行合起来完成同一次计算或对象配置。
        GUI.color =
            // 执行 color;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            color;


        // 调用 RotateAroundPivot：RotateAroundPivot。
        GUIUtility.RotateAroundPivot(
            // 继续填写上一行调用的参数：angle；参数位置决定它表示的坐标、速度或选项。
            angle,
            // 这一行补充 start，与相邻行合起来完成同一次计算或对象配置。
            start
        );


        // 调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(
            // 用 new 创建 Rect(；构造参数决定新对象的初始内容。
            new Rect(
                // 继续填写上一行调用的参数：start.x；参数位置决定它表示的坐标、速度或选项。
                start.x,
                // 继续填写上一行调用的参数：start.y - width / 2f；参数位置决定它表示的坐标、速度或选项。
                start.y - width / 2f,
                // 继续填写上一行调用的参数：length；参数位置决定它表示的坐标、速度或选项。
                length,
                // 这一行补充 width，与相邻行合起来完成同一次计算或对象配置。
                width
            // 继续填写上一行调用的参数：)；参数位置决定它表示的坐标、速度或选项。
            ),
            // 这一行补充 Texture2D.whiteTexture，与相邻行合起来完成同一次计算或对象配置。
            Texture2D.whiteTexture
        );


        // 这一行补充 GUI.matrix =，与相邻行合起来完成同一次计算或对象配置。
        GUI.matrix =
            // 执行 matrix;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            matrix;


        // 这一行补充 GUI.color =，与相邻行合起来完成同一次计算或对象配置。
        GUI.color =
            // 执行 Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Color.white;
    }
}