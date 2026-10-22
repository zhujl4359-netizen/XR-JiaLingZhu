using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <summary>
/// 桌面版先祖兑换树：管理节点解锁、鼠标长按兑换、物品拿取/装备/投掷及界面反馈。
/// </summary>
public class AncestorExchangeTree : MonoBehaviour
{
    // =====================================================
    // INTERACTION
    // =====================================================

    [Header("Interaction")]

    public Transform player;

    public float interactionDistance = 18f;

    [Tooltip("How long the player must hold the mouse button.")]
    public float longPressTime = 0.5f;


    // =====================================================
    // CAMERA / HOLD POINT
    // =====================================================

    [Header("Item Hold Point")]

    [Tooltip("Point in front of Main Camera where extracted item appears.")]
    public Transform itemHoldPoint;

    [Tooltip("Default distance in front of camera.")]
    public float defaultHoldDistance = 2f;


    // =====================================================
    // THROW
    // =====================================================

    [Header("Throw Settings")]

    public float throwForce = 12f;

    public float upwardThrowForce = 2f;


    // =====================================================
    // 3D MODEL TEMPLATES
    // =====================================================

    [Header("3D Model Templates")]

    public GameObject capeModel;

    public GameObject maskModel;

    public GameObject emoteModel;

    public GameObject hairModel;

    public GameObject instrumentModel;


    // =====================================================
    // VIRTUAL KEYBOARD
    // =====================================================

    [Header("Virtual Keyboard")]

    public GameObject virtualKeyboard;

    private bool keyboardOpen = false;

    [Header("Guidance and feedback")]
    public float approachPromptDistance = 24f;
    public float feedbackDuration = 4f;
    public float collectionAnimationDuration = 0.65f;
    public Color promptColor = new Color(1f, 0.9f, 0.55f, 1f);

    private CursorLockMode cursorLockBeforeTree;
    private bool cursorVisibleBeforeTree;
    private string feedbackMessage;
    private float feedbackUntil;
    private float collectionAnimationStart = -1f;
    private Vector2 collectionAnimationOrigin;
    private string collectionAnimationIcon;
    private AudioSource feedbackAudio;
    private AudioClip feedbackClip;
    private string lastEquippedItemName;



    // =====================================================
    // TREE STATE
    // =====================================================

    private bool treeOpen = false;

    private int hoveredNode = -1;

    private int selectedNode = -1;


    // =====================================================
    // CURRENT HELD ITEM
    // =====================================================

    private GameObject currentHeldObject = null;

    private int currentHeldItemIndex = -1;


    // =====================================================
    // LONG PRESS
    // =====================================================

    private bool isPressing = false;

    private int pressingNode = -1;

    private float pressStartTime = 0f;


    // =====================================================
    // ITEM NODE
    // =====================================================

    [System.Serializable]
    /// <summary>
    /// 记录兑换节点的名称、图标、价格、前置节点与领取状态。
    /// </summary>
    public class ItemNode
    {
        public string itemName;

        public string icon;

        public int cost;

        public int parentIndex;

        public bool unlocked;

        public bool extracted;

        public Vector2 position;
    }


    // =====================================================
    // ITEM LIST
    // =====================================================

    private List<ItemNode> items =
        new List<ItemNode>();


    // =====================================================
    // NODE COLORS
    // =====================================================

    [Header("Node Colors")]

    public Color unlockedColor =
        new Color(0.8f, 0.8f, 0.8f, 1f);

    public Color lockedColor =
        new Color(0.35f, 0.35f, 0.35f, 1f);

    public Color hoverColor =
        new Color(1f, 1f, 0.3f, 1f);

    public Color selectedColor =
        new Color(1f, 0.8f, 0.1f, 1f);

    public Color extractedColor =
        new Color(0.18f, 0.18f, 0.18f, 0.7f);


    // =====================================================
    // CONNECTION
    // =====================================================

    [Header("Connection Line")]

    public Color connectionColor =
        new Color(1f, 1f, 1f, 0.5f);

    public float connectionWidth = 4f;


    // =====================================================
    // BACKGROUND
    // =====================================================

    [Header("Background")]

    public Color backgroundColor =
        new Color(0f, 0f, 0f, 0.65f);


    // =====================================================
    // NODE APPEARANCE
    // =====================================================

    [Header("Node Appearance")]

    public float nodeSize = 80f;

    public float iconFontSize = 26f;

    public float hoverIconFontSize = 32f;


    // =====================================================
    // START
    // =====================================================

    /// <summary>
    /// 初始化玩家、摄像机与手持点引用；创建节点数据和反馈音效，隐藏模板及键盘。
    /// </summary>
    void Start()
    {
        // -------------------------------------------------
        // Find Player
        // -------------------------------------------------

        if (player == null)
        {
            GameObject p =
                GameObject.Find("C1_Player");

            if (p != null)
            {
                player = p.transform;
            }
        }


        // -------------------------------------------------
        // Find Main Camera
        // -------------------------------------------------

        Camera mainCamera =
            Camera.main;


        // -------------------------------------------------
        // Find ItemHoldPoint
        // -------------------------------------------------

        if (itemHoldPoint == null &&
            mainCamera != null)
        {
            Transform found =
                mainCamera.transform.Find(
                    "ItemHoldPoint"
                );

            if (found != null)
            {
                itemHoldPoint = found;
            }
        }


        // -------------------------------------------------
        // Create ItemHoldPoint if missing
        // -------------------------------------------------

        if (itemHoldPoint == null &&
            mainCamera != null)
        {
            GameObject holdPoint =
                new GameObject("ItemHoldPoint");

            holdPoint.transform.SetParent(
                mainCamera.transform,
                false
            );

            holdPoint.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    defaultHoldDistance
                );

            holdPoint.transform.localRotation =
                Quaternion.identity;

            itemHoldPoint =
                holdPoint.transform;

            Debug.Log(
                "ItemHoldPoint was missing. Created automatically."
            );
        }


        // -------------------------------------------------
        // Create Tree
        // -------------------------------------------------

        CreateItemTree();
        feedbackAudio = GetComponent<AudioSource>();
        if (feedbackAudio == null) feedbackAudio = gameObject.AddComponent<AudioSource>();
        feedbackAudio.playOnAwake = false;
        feedbackAudio.spatialBlend = 0f;
        feedbackClip = CreateConfirmationChime();


        // -------------------------------------------------
        // Hide Templates
        // -------------------------------------------------

        HideModelTemplates();


        // -------------------------------------------------
        // Hide Keyboard
        // -------------------------------------------------

        if (virtualKeyboard != null)
        {
            virtualKeyboard.SetActive(false);
        }
    }


    // =====================================================
    // HIDE MODEL TEMPLATES
    // =====================================================

    /// <summary>
    /// 隐藏仅供实例化的模型模板，避免模板物体本身出现在场景中。
    /// </summary>
    void HideModelTemplates()
    {
        if (capeModel != null)
            capeModel.SetActive(false);

        if (maskModel != null)
            maskModel.SetActive(false);

        if (emoteModel != null)
            emoteModel.SetActive(false);

        if (hairModel != null)
            hairModel.SetActive(false);

        if (instrumentModel != null)
            instrumentModel.SetActive(false);
    }


    // =====================================================
    // CREATE TREE
    // =====================================================

    /// <summary>
    /// 构造兑换树的节点、价格、父子关系和界面坐标；首次可兑换节点在此设定。
    /// </summary>
    void CreateItemTree()
    {
        items.Clear();


        // =================================================
        // CAPE
        // CENTER OF TREE
        // =================================================

        items.Add(
            new ItemNode
            {
                itemName = "Cape",
                icon = "◆",
                cost = 10,
                parentIndex = -1,
                unlocked = true,
                extracted = false,

                position =
                    new Vector2(0, 0)
            }
        );


        // =================================================
        // HAIR
        // =================================================

        items.Add(
            new ItemNode
            {
                itemName = "Hair",
                icon = "★",
                cost = 5,
                parentIndex = 0,
                unlocked = true,
                extracted = false,

                position =
                    new Vector2(-180, 150)
            }
        );


        // =================================================
        // MASK
        // =================================================

        items.Add(
            new ItemNode
            {
                itemName = "Mask",
                icon = "●",
                cost = 8,
                parentIndex = 0,
                unlocked = true,
                extracted = false,

                position =
                    new Vector2(180, 150)
            }
        );


        // =================================================
        // EMOTE
        // =================================================

        items.Add(
            new ItemNode
            {
                itemName = "Emote",
                icon = "✦",
                cost = 3,
                parentIndex = 1,
                unlocked = false,
                extracted = false,

                position =
                    new Vector2(-180, 300)
            }
        );


        // =================================================
        // INSTRUMENT
        // =================================================

        items.Add(
            new ItemNode
            {
                itemName = "Instrument",
                icon = "♫",
                cost = 12,
                parentIndex = 2,
                unlocked = false,
                extracted = false,

                position =
                    new Vector2(180, 300)
            }
        );
    }


    // =====================================================
    // UPDATE
    // =====================================================

    /// <summary>
    /// 每帧处理 E、Esc、Q、T 输入，并检查鼠标长按兑换与松开状态。
    /// </summary>
    void Update()
    {
        if (Keyboard.current == null)
            return;


        // =================================================
        // E
        // =================================================

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            HandleEKey();
        }


        // =================================================
        // ESC
        // =================================================

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscape();
        }


        // =================================================
        // Q THROW
        // =================================================

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            ThrowCurrentItem();
        }


        // =================================================
        // T KEYBOARD
        // =================================================

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            ToggleVirtualKeyboard();
        }


        // =================================================
        // LONG PRESS
        // =================================================

        CheckLongPress();


        // =================================================
        // CHECK MOUSE RELEASE
        // =================================================

        CheckMouseRelease();
    }


    // =====================================================
    // E KEY
    // =====================================================

    /// <summary>
    /// 按优先级处理 E：手中有物品则装备；否则关闭已打开的树或在范围内打开树。
    /// </summary>
    void HandleEKey()
    {
        // -------------------------------------------------
        // If tree is open -> close
        // -------------------------------------------------

        if (treeOpen && currentHeldObject == null)
        {
            CloseTree();
            return;
        }


        // -------------------------------------------------
        // If holding item -> equip
        // -------------------------------------------------

        if (currentHeldObject != null)
        {
            EquipCurrentItem();
            return;
        }


        // -------------------------------------------------
        // Open tree
        // -------------------------------------------------

        if (player == null)
            return;


        float distance =
            Vector3.Distance(
                player.position,
                transform.position
            );


        if (distance <= interactionDistance)
        {
            OpenTree();
        }
    }


    // =====================================================
    // ESC
    // =====================================================

    /// <summary>
    /// Esc 优先关闭虚拟键盘；没有键盘时关闭兑换树。
    /// </summary>
    void HandleEscape()
    {
        // Close keyboard first

        if (keyboardOpen)
        {
            ToggleVirtualKeyboard();
            return;
        }


        // Close tree

        if (treeOpen)
        {
            CloseTree();
        }
    }


    // =====================================================
    // OPEN TREE
    // =====================================================

    /// <summary>
    /// 重置节点交互状态，保存原鼠标状态并显示可点击的兑换树。
    /// </summary>
    void OpenTree()
    {
        treeOpen = true;

        selectedNode = -1;

        hoveredNode = -1;

        isPressing = false;

        pressingNode = -1;

        cursorLockBeforeTree = Cursor.lockState;
        cursorVisibleBeforeTree = Cursor.visible;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SetFeedback("Select an item, then hold the left mouse button to obtain it.", 5f);

        Debug.Log(
            "Ancestor Tree Opened"
        );
    }


    // =====================================================
    // CLOSE TREE
    // =====================================================

    /// <summary>
    /// 关闭兑换树并恢复打开前的鼠标锁定和可见状态。
    /// </summary>
    void CloseTree()
    {
        treeOpen = false;

        selectedNode = -1;

        hoveredNode = -1;

        isPressing = false;

        pressingNode = -1;

        Cursor.lockState = cursorLockBeforeTree;
        Cursor.visible = cursorVisibleBeforeTree;

        Debug.Log(
            "Ancestor Tree Closed"
        );
    }


    // =====================================================
    // LONG PRESS START
    // =====================================================

    /// <summary>
    /// 仅对已解锁、未取出且当前可持有的节点开始计时，防止重复兑换。
    /// </summary>
    void StartLongPress(int index)
    {
        if (index < 0 ||
            index >= items.Count)
            return;


        if (!items[index].unlocked)
            return;


        if (items[index].extracted)
            return;


        if (currentHeldObject != null)
            return;


        isPressing = true;

        pressingNode = index;

        pressStartTime =
            Time.realtimeSinceStartup;

        selectedNode = index;
        SetFeedback(items[index].itemName + " selected - keep holding to obtain.", 2f);

        Debug.Log(
            "Started holding: " +
            items[index].itemName
        );
    }


    // =====================================================
    // CHECK LONG PRESS
    // =====================================================

    /// <summary>
    /// 鼠标持续按住达到设定时间后兑换选中节点；提前松开则取消。
    /// </summary>
    void CheckLongPress()
    {
        if (!treeOpen)
            return;


        if (!isPressing)
            return;


        if (pressingNode < 0)
            return;


        if (currentHeldObject != null)
            return;


        float heldTime =
            Time.realtimeSinceStartup -
            pressStartTime;


        if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
        {
            isPressing = false;
            pressingNode = -1;
            return;
        }

        if (heldTime >= longPressTime)
        {
            int index =
                pressingNode;


            isPressing = false;

            pressingNode = -1;


            ExtractItem(index);
        }
    }


    // =====================================================
    // CHECK MOUSE RELEASE
    // =====================================================

    /// <summary>
    /// 鼠标提前松开时清理长按状态，避免下一次点击沿用旧计时。
    /// </summary>
    void CheckMouseRelease()
    {
        if (!isPressing)
            return;


        if (Mouse.current == null)
            return;


        if (
            Mouse.current.leftButton
                .wasReleasedThisFrame
        )
        {
            isPressing = false;

            pressingNode = -1;
        }
    }


    // =====================================================
    // GUI
    // =====================================================

    /// <summary>
    /// 按照树是否打开，绘制引导提示或兑换树主体、节点、反馈及收集动画。
    /// </summary>
    void OnGUI()
    {
        if (!treeOpen)
        {
            DrawApproachPrompt();
            DrawItemStatusBadge();
            DrawFeedback();
            return;
        }

        DrawBackground();

        DrawTitle();

        DrawConnections();

        DrawNodes();

        DrawSelectedItem();

        DrawInstruction();
        DrawCollectionAnimation();
        DrawItemStatusBadge();
        DrawFeedback();
    }


    // =====================================================
    // BACKGROUND
    // =====================================================

    /// <summary>
    /// 绘制覆盖游戏画面的半透明兑换树背景。
    /// </summary>
    void DrawBackground()
    {
        GUI.color =
            backgroundColor;


        GUI.Box(
            new Rect(
                0,
                0,
                Screen.width,
                Screen.height
            ),
            ""
        );


        GUI.color =
            Color.white;
    }


    // =====================================================
    // TITLE
    // =====================================================

    /// <summary>
    /// 在兑换树顶部绘制标题和货币等说明。
    /// </summary>
    void DrawTitle()
    {
        GUIStyle title =
            new GUIStyle(
                GUI.skin.label
            );


        title.fontSize = 28;

        title.alignment =
            TextAnchor.MiddleCenter;

        title.fontStyle =
            FontStyle.Bold;


        GUI.Label(
            new Rect(
                Screen.width / 2f - 300f,
                40f,
                600f,
                50f
            ),
            "ANCESTOR EXCHANGE",
            title
        );
    }


    // =====================================================
    // CONNECTIONS
    // =====================================================

    /// <summary>
    /// 根据节点父子关系在界面上绘制连接线。
    /// </summary>
    void DrawConnections()
    {
        for (
            int i = 0;
            i < items.Count;
            i++
        )
        {
            int parent =
                items[i].parentIndex;


            if (parent < 0)
                continue;


            Vector2 start =
                GetScreenPosition(
                    items[parent].position
                );


            Vector2 end =
                GetScreenPosition(
                    items[i].position
                );


            DrawLine(
                start,
                end,
                connectionWidth,
                connectionColor
            );
        }
    }


    // =====================================================
    // NODES
    // =====================================================

    /// <summary>
    /// 遍历所有兑换节点，并为每个节点调用单独的绘制逻辑。
    /// </summary>
    void DrawNodes()
    {
        hoveredNode = -1;


        for (
            int i = 0;
            i < items.Count;
            i++
        )
        {
            DrawNode(
                i,
                items[i]
            );
        }
    }


    // =====================================================
    // SINGLE NODE
    // =====================================================

    /// <summary>
    /// 绘制单个节点，处理悬停、选中、锁定、价格和鼠标点击反馈。
    /// </summary>
    void DrawNode(
        int index,
        ItemNode item)
    {
        Vector2 center =
            GetScreenPosition(
                item.position
            );


        float size =
            nodeSize;


        Rect rect =
            new Rect(
                center.x - size / 2f,
                center.y - size / 2f,
                size,
                size
            );


        Event e =
            Event.current;


        bool hover =
            rect.Contains(
                e.mousePosition
            );


        // =================================================
        // EXTRACTED
        // =================================================

        if (item.extracted)
        {
            DrawExtractedNode(
                rect,
                center,
                item
            );

            return;
        }


        // =================================================
        // HOVER
        // =================================================

        if (hover)
        {
            hoveredNode = index;
        }


        // =================================================
        // MOUSE DOWN
        // =================================================

        if (
            hover &&
            item.unlocked &&
            currentHeldObject == null &&
            e.type == EventType.MouseDown &&
            e.button == 0
        )
        {
            StartLongPress(index);

            e.Use();
        }


        // =================================================
        // NODE COLOR
        // =================================================

        Color nodeColor;


        if (selectedNode == index)
        {
            nodeColor =
                selectedColor;
        }
        else if (
            hover &&
            item.unlocked &&
            currentHeldObject == null
        )
        {
            nodeColor =
                hoverColor;
        }
        else if (item.unlocked)
        {
            nodeColor =
                unlockedColor;
        }
        else
        {
            nodeColor =
                lockedColor;
        }


        // =================================================
        // NODE STYLE
        // =================================================

        GUIStyle nodeStyle =
            new GUIStyle(
                GUI.skin.box
            );


        nodeStyle.fontSize =
            hover
            ? (int)hoverIconFontSize
            : (int)iconFontSize;


        nodeStyle.fontStyle =
            FontStyle.Bold;


        nodeStyle.alignment =
            TextAnchor.MiddleCenter;


        GUI.color =
            nodeColor;


        // IMPORTANT:
        // GUI.Box instead of GUI.Button.
        // This prevents GUI.Button from consuming
        // the long-press event.

        GUI.Box(
            rect,
            item.icon,
            nodeStyle
        );


        GUI.color =
            Color.white;


        // =================================================
        // NAME
        // =================================================

        GUIStyle nameStyle =
            new GUIStyle(
                GUI.skin.label
            );


        nameStyle.fontSize = 14;

        nameStyle.alignment =
            TextAnchor.MiddleCenter;


        GUI.Label(
            new Rect(
                center.x - 80f,
                center.y + 42f,
                160f,
                25f
            ),
            item.itemName,
            nameStyle
        );


        // =================================================
        // COST
        // =================================================

        GUIStyle costStyle =
            new GUIStyle(
                GUI.skin.label
            );


        costStyle.fontSize = 12;

        costStyle.alignment =
            TextAnchor.MiddleCenter;


        GUI.Label(
            new Rect(
                center.x - 80f,
                center.y + 62f,
                160f,
                20f
            ),
            item.cost + " Candles",
            costStyle
        );


        // =================================================
        // HOLDING INDICATOR
        // =================================================

        if (
            isPressing &&
            pressingNode == index
        )
        {
            float progress =
                Mathf.Clamp01(
                    (
                        Time.realtimeSinceStartup -
                        pressStartTime
                    )
                    /
                    longPressTime
                );


            GUIStyle holdStyle =
                new GUIStyle(
                    GUI.skin.label
                );


            holdStyle.fontSize = 12;

            holdStyle.alignment =
                TextAnchor.MiddleCenter;


            GUI.Label(
                new Rect(
                    center.x - 100f,
                    center.y + 82f,
                    200f,
                    25f
                ),
                "HOLDING " +
                Mathf.RoundToInt(
                    progress * 100f
                ) +
                "%",
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
    void DrawExtractedNode(
        Rect rect,
        Vector2 center,
        ItemNode item)
    {
        GUIStyle style =
            new GUIStyle(
                GUI.skin.box
            );


        style.fontSize =
            (int)iconFontSize;


        style.fontStyle =
            FontStyle.Bold;


        style.alignment =
            TextAnchor.MiddleCenter;


        GUI.color =
            extractedColor;


        GUI.Box(
            rect,
            item.icon,
            style
        );


        GUI.color =
            Color.white;


        GUIStyle nameStyle =
            new GUIStyle(
                GUI.skin.label
            );


        nameStyle.fontSize = 14;

        nameStyle.alignment =
            TextAnchor.MiddleCenter;


        GUI.Label(
            new Rect(
                center.x - 80f,
                center.y + 42f,
                160f,
                25f
            ),
            item.itemName,
            nameStyle
        );


        GUI.Label(
            new Rect(
                center.x - 80f,
                center.y + 62f,
                160f,
                20f
            ),
            "EXTRACTED",
            nameStyle
        );
    }


    // =====================================================
    // SELECTED ITEM
    // =====================================================

    /// <summary>
    /// 显示当前选中物品的名称、说明或兑换状态。
    /// </summary>
    void DrawSelectedItem()
    {
        if (selectedNode < 0)
            return;


        if (selectedNode >= items.Count)
            return;


        ItemNode item =
            items[selectedNode];


        GUIStyle panelStyle =
            new GUIStyle(
                GUI.skin.box
            );


        panelStyle.fontSize = 18;

        panelStyle.alignment =
            TextAnchor.MiddleCenter;


        GUI.Box(
            new Rect(
                Screen.width - 350f,
                100f,
                300f,
                250f
            ),
            ""
        );


        GUI.Label(
            new Rect(
                Screen.width - 330f,
                120f,
                260f,
                40f
            ),
            item.itemName,
            panelStyle
        );


        GUI.Label(
            new Rect(
                Screen.width - 330f,
                165f,
                260f,
                30f
            ),
            "Cost: " +
            item.cost +
            " Candles",
            panelStyle
        );


        string status;


        if (item.extracted)
        {
            status =
                currentHeldItemIndex == selectedNode ? "OBTAINED - IN HAND" : "OBTAINED";
        }
        else if (item.unlocked)
        {
            status =
                "UNLOCKED";
        }
        else
        {
            status =
                "LOCKED";
        }


        GUI.Label(
            new Rect(
                Screen.width - 330f,
                205f,
                260f,
                30f
            ),
            status,
            panelStyle
        );


        // =================================================
        // UNLOCK
        // =================================================

        if (
            !item.unlocked &&
            !item.extracted
        )
        {
            if (
                GUI.Button(
                    new Rect(
                        Screen.width - 310f,
                        250f,
                        220f,
                        40f
                    ),
                    "Unlock"
                )
            )
            {
                UnlockItem(
                    selectedNode
                );
            }
        }
    }


    // =====================================================
    // INSTRUCTION
    // =====================================================

    /// <summary>
    /// 根据当前交互状态显示底部操作提示。
    /// </summary>
    void DrawInstruction()
    {
        GUIStyle style =
            new GUIStyle(
                GUI.skin.label
            );


        style.fontSize = 16;

        style.alignment =
            TextAnchor.MiddleCenter;


        string text;


        if (currentHeldObject != null)
        {
            text =
                "E: Equip     Q: Throw     ESC: Close";
        }
        else
        {
            text =
                "Click and hold an unlocked item to obtain it     ESC: Close";
        }


        GUI.Label(
            new Rect(
                Screen.width / 2f - 450f,
                Screen.height - 90f,
                900f,
                40f
            ),
            text,
            style
        );
    }


    // =====================================================
    // UNLOCK
    // =====================================================

    /// <summary>
    /// 把指定节点标记为已解锁，使其可参与后续兑换。
    /// </summary>
    void UnlockItem(int index)
    {
        if (
            index < 0 ||
            index >= items.Count
        )
            return;


        if (items[index].extracted)
            return;


        items[index].unlocked =
            true;


        selectedNode =
            index;


        Debug.Log(
            "Item unlocked: " +
            items[index].itemName
        );
    }


    // =====================================================
    // EXTRACT ITEM
    // =====================================================

    /// <summary>
    /// 从模板生成物品实例，放到摄像机前的手持点，关闭物理并记录领取状态。
    /// </summary>
    void ExtractItem(int index)
    {
        if (
            index < 0 ||
            index >= items.Count
        )
            return;


        if (currentHeldObject != null)
        {
            Debug.Log(
                "You can only carry one item at a time."
            );

            return;
        }


        ItemNode item =
            items[index];


        if (!item.unlocked)
            return;


        if (item.extracted)
            return;


        // =================================================
        // GET TEMPLATE
        // =================================================

        GameObject template =
            GetModelTemplate(
                item.itemName
            );


        if (template == null)
        {
            Debug.LogError(
                "NO MODEL ASSIGNED FOR: " +
                item.itemName
            );

            return;
        }


        // =================================================
        // CREATE OBJECT
        // =================================================

        GameObject newObject =
            Instantiate(
                template
            );


        newObject.name =
            item.itemName +
            "_Held";


        newObject.SetActive(true);


        // =================================================
        // PUT IN FRONT OF CAMERA
        // =================================================

        if (itemHoldPoint != null)
        {
            newObject.transform.SetParent(
                itemHoldPoint,
                false
            );


            newObject.transform.localPosition =
                Vector3.zero;


            newObject.transform.localRotation =
                Quaternion.identity;


            newObject.transform.localScale =
                template.transform.localScale;
        }
        else
        {
            Camera cam =
                Camera.main;


            if (cam != null)
            {
                newObject.transform.position =
                    cam.transform.position +
                    cam.transform.forward *
                    defaultHoldDistance;


                newObject.transform.rotation =
                    cam.transform.rotation;
            }
        }


        // =================================================
        // DISABLE PHYSICS
        // =================================================

        DisablePhysics(
            newObject
        );


        // =================================================
        // SAVE CURRENT ITEM
        // =================================================

        currentHeldObject =
            newObject;


        currentHeldItemIndex =
            index;


        // =================================================
        // MARK EXTRACTED
        // =================================================

        items[index].extracted = true;
        selectedNode = index;
        collectionAnimationOrigin = GetScreenPosition(item.position);
        collectionAnimationIcon = item.icon;
        collectionAnimationStart = Time.realtimeSinceStartup;
        SetFeedback(item.itemName + " Obtained - now in your hand. Press E to equip.", feedbackDuration);
        PlayConfirmationChime();


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "ITEM EXTRACTED: " +
            item.itemName
        );

        Debug.Log(
            "Hold Point: " +
            (
                itemHoldPoint != null
                ? itemHoldPoint.name
                : "NONE"
            )
        );

        Debug.Log(
            "================================"
        );
    }


    // =====================================================
    // GET MODEL
    // =====================================================

    /// <summary>
    /// 将兑换节点名称映射到 Inspector 中绑定的模型模板。
    /// </summary>
    GameObject GetModelTemplate(
        string itemName)
    {
        switch (itemName)
        {
            case "Cape":
                return capeModel;

            case "Mask":
                return maskModel;

            case "Emote":
                return emoteModel;

            case "Hair":
                return hairModel;

            case "Instrument":
                return instrumentModel;
        }


        return null;
    }


    // =====================================================
    // DISABLE PHYSICS
    // =====================================================

    /// <summary>
    /// 手持或装备物品时关闭刚体运动及碰撞，避免物体与角色相互推挤。
    /// </summary>
    void DisablePhysics(
        GameObject obj)
    {
        if (obj == null)
            return;


        Rigidbody[] rigidbodies =
            obj.GetComponentsInChildren<Rigidbody>(
                true
            );


        foreach (
            Rigidbody rb
            in rigidbodies
        )
        {
            rb.isKinematic = true;

            rb.useGravity = false;

            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }


        Collider[] colliders =
            obj.GetComponentsInChildren<Collider>(
                true
            );


        foreach (
            Collider col
            in colliders
        )
        {
            col.enabled = false;
        }
    }


    // =====================================================
    // ENABLE PHYSICS
    // =====================================================

    /// <summary>
    /// 投出物品前恢复刚体和碰撞，使其重新受重力影响。
    /// </summary>
    void EnablePhysics(
        GameObject obj)
    {
        if (obj == null)
            return;


        Rigidbody rb =
            obj.GetComponent<Rigidbody>();


        if (rb == null)
        {
            rb =
                obj.AddComponent<Rigidbody>();
        }


        Collider[] colliders =
            obj.GetComponentsInChildren<Collider>(
                true
            );


        foreach (
            Collider col
            in colliders
        )
        {
            col.enabled = true;
        }


        rb.isKinematic = false;

        rb.useGravity = true;
    }


    // =====================================================
    // EQUIP
    // =====================================================

    /// <summary>
    /// 把当前手持物从镜头前移到角色装备点，随后清空手持槽。
    /// </summary>
    void EquipCurrentItem()
    {
        if (currentHeldObject == null)
            return;


        if (currentHeldItemIndex < 0)
            return;


        string itemName =
            items[currentHeldItemIndex]
                .itemName;


        // -------------------------------------------------
        // Remove Camera Parent
        // -------------------------------------------------

        currentHeldObject.transform.SetParent(
            null
        );


        // -------------------------------------------------
        // Disable Physics
        // -------------------------------------------------

        DisablePhysics(
            currentHeldObject
        );


        // -------------------------------------------------
        // Find Equipment Point
        // -------------------------------------------------

        Transform equipPoint =
            FindEquipmentPoint(
                itemName
            );


        if (equipPoint != null)
        {
            currentHeldObject.transform.SetParent(
                equipPoint,
                false
            );


            currentHeldObject.transform.localPosition =
                Vector3.zero;


            currentHeldObject.transform.localRotation =
                Quaternion.identity;
        }
        else
        {
            if (player != null)
            {
                currentHeldObject.transform.SetParent(
                    player,
                    true
                );
            }
        }


        lastEquippedItemName = itemName;
        SetFeedback(itemName + " Equipped", feedbackDuration);
        PlayConfirmationChime();

        Debug.Log(
            "Equipped item: " +
            itemName
        );


        currentHeldObject = null;

        currentHeldItemIndex = -1;


        CloseTree();
    }


    // =====================================================
    // EQUIPMENT POINT
    // =====================================================

    /// <summary>
    /// 按物品名称寻找角色上的专用挂点；鹿头优先使用 MaskHoldPoint。
    /// </summary>
    Transform FindEquipmentPoint(
        string itemName)
    {
        if (player == null)
            return null;


        // -------------------------------------------------
        // MASK
        // -------------------------------------------------

        if (itemName == "Mask")
        {
            Transform point =
                FindDeepChild(
                    player,
                    "MaskHoldPoint"
                );


            if (point != null)
                return point;
        }


        // -------------------------------------------------
        // OTHER ITEMS
        // -------------------------------------------------

        Transform generic =
            FindDeepChild(
                player,
                itemName +
                "HoldPoint"
            );


        if (generic != null)
            return generic;


        return null;
    }


    // =====================================================
    // FIND CHILD
    // =====================================================

    /// <summary>
    /// 在指定父物体的全部子层级中按名称查找挂点，包括未激活物体。
    /// </summary>
    Transform FindDeepChild(
        Transform parent,
        string childName)
    {
        Transform[] children =
            parent.GetComponentsInChildren<Transform>(
                true
            );


        foreach (
            Transform child
            in children
        )
        {
            if (
                child.name ==
                childName
            )
            {
                return child;
            }
        }


        return null;
    }


    // =====================================================
    // THROW
    // =====================================================

    /// <summary>
    /// 从摄像机手持点解除物品，恢复物理并沿视线方向施加投掷冲量。
    /// </summary>
    void ThrowCurrentItem()
    {
        if (currentHeldObject == null)
            return;


        Camera cam =
            Camera.main;


        if (cam == null)
            return;


        // -------------------------------------------------
        // Remove Camera Parent
        // -------------------------------------------------

        currentHeldObject.transform.SetParent(
            null
        );


        // -------------------------------------------------
        // Move in Front of Camera
        // -------------------------------------------------

        currentHeldObject.transform.position =
            cam.transform.position +
            cam.transform.forward *
            1.2f;


        currentHeldObject.transform.rotation =
            cam.transform.rotation;


        // -------------------------------------------------
        // Enable Physics
        // -------------------------------------------------

        EnablePhysics(
            currentHeldObject
        );


        Rigidbody rb =
            currentHeldObject.GetComponent<Rigidbody>();


        if (rb != null)
        {
            Vector3 force =
                cam.transform.forward *
                throwForce;


            force +=
                Vector3.up *
                upwardThrowForce;


            rb.linearVelocity =
                Vector3.zero;


            rb.angularVelocity =
                Vector3.zero;


            rb.AddForce(
                force,
                ForceMode.Impulse
            );
        }


        items[currentHeldItemIndex].extracted = false;
        SetFeedback(items[currentHeldItemIndex].itemName + " thrown - obtain it again from the tree.", feedbackDuration);
        Debug.Log(
            "Thrown item."
        );


        currentHeldObject = null;

        currentHeldItemIndex = -1;


        CloseTree();
    }


    // =====================================================
    // VIRTUAL KEYBOARD
    // =====================================================

    /// <summary>
    /// 切换虚拟键盘的显示状态，并同步内部开关标记。
    /// </summary>
    void ToggleVirtualKeyboard()
    {
        keyboardOpen =
            !keyboardOpen;


        if (virtualKeyboard != null)
        {
            virtualKeyboard.SetActive(
                keyboardOpen
            );
        }


        Debug.Log(
            "Virtual Keyboard: " +
            (
                keyboardOpen
                ? "OPEN"
                : "CLOSED"
            )
        );
    }


    /// <summary>
    /// 暂存提示文字及过期时间，供后续界面绘制。
    /// </summary>
    private void SetFeedback(string message, float duration)
    {
        feedbackMessage = message;
        feedbackUntil = Time.realtimeSinceStartup + duration;
    }

    /// <summary>
    /// 玩家接近先祖但尚未打开兑换树时显示交互引导。
    /// </summary>
    private void DrawApproachPrompt()
    {
        if (player == null || currentHeldObject != null) return;
        float distance = Vector3.Distance(player.position, transform.position);
        if (distance > approachPromptDistance) return;

        bool canOpen = distance <= interactionDistance;
        string message = canOpen
            ? "ANCESTOR  •  Press E to open the item tree"
            : "ANCESTOR  •  Move closer to explore items";
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = promptColor;
        GUI.Box(new Rect(Screen.width / 2f - 230f, Screen.height - 150f, 460f, 48f), message, style);

        Camera cam = Camera.main;
        if (cam == null) return;
        Vector3 point = cam.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);
        if (point.z <= 0f) return;
        float x = Mathf.Clamp(point.x, 30f, Screen.width - 30f);
        float y = Mathf.Clamp(Screen.height - point.y, 30f, Screen.height - 180f);
        GUI.color = canOpen ? promptColor : Color.white;
        GUI.Label(new Rect(x - 50f, y - 25f, 100f, 50f), "✦ ANCESTOR", style);
        GUI.color = Color.white;
    }

    /// <summary>
    /// 在界面上显示当前持有或装备的物品状态。
    /// </summary>
    private void DrawItemStatusBadge()
    {
        string status = currentHeldObject != null && currentHeldItemIndex >= 0
            ? items[currentHeldItemIndex].itemName + "  •  IN HAND  •  E to equip"
            : string.IsNullOrEmpty(lastEquippedItemName)
                ? null : lastEquippedItemName + "  •  EQUIPPED";
        if (status == null) return;
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = promptColor;
        GUI.Box(new Rect(Screen.width - 340f, 20f, 320f, 42f), status, style);
    }

    /// <summary>
    /// 在有效时间内绘制最近一次操作的提示文字。
    /// </summary>
    private void DrawFeedback()
    {
        if (string.IsNullOrEmpty(feedbackMessage) || Time.realtimeSinceStartup >= feedbackUntil) return;
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;
        GUI.Box(new Rect(Screen.width / 2f - 300f, 105f, 600f, 52f), feedbackMessage, style);
    }

    /// <summary>
    /// 绘制物品领取时从节点出发的短暂收集动画。
    /// </summary>
    private void DrawCollectionAnimation()
    {
        if (collectionAnimationStart < 0f) return;
        float progress = (Time.realtimeSinceStartup - collectionAnimationStart) /
            Mathf.Max(0.1f, collectionAnimationDuration);
        if (progress >= 1f)
        {
            collectionAnimationStart = -1f;
            return;
        }
        Vector2 destination = new Vector2(Screen.width - 120f, 75f);
        Vector2 position = Vector2.Lerp(collectionAnimationOrigin, destination,
            Mathf.SmoothStep(0f, 1f, progress));
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 28;
        style.alignment = TextAnchor.MiddleCenter;
        GUI.color = promptColor;
        GUI.Box(new Rect(position.x - 30f, position.y - 30f, 60f, 60f), collectionAnimationIcon, style);
        GUI.color = Color.white;
    }

    /// <summary>
    /// 生成不依赖外部音频文件的确认提示音。
    /// </summary>
    private AudioClip CreateConfirmationChime()
    {
        const int sampleRate = 22050;
        const float seconds = 0.22f;
        int sampleCount = Mathf.CeilToInt(sampleRate * seconds);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Sin(Mathf.PI * i / sampleCount);
            samples[i] = Mathf.Sin(2f * Mathf.PI * 660f * t) * envelope * 0.12f;
        }
        AudioClip clip = AudioClip.Create("Sky confirmation chime", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// 播放已生成的领取或装备确认音效。
    /// </summary>
    private void PlayConfirmationChime()
    {
        if (feedbackAudio != null && feedbackClip != null)
            feedbackAudio.PlayOneShot(feedbackClip);
    }

    // =====================================================
    // SCREEN POSITION
    // =====================================================

    /// <summary>
    /// 把节点的设计坐标换算为当前屏幕上的实际位置。
    /// </summary>
    Vector2 GetScreenPosition(
        Vector2 offset)
    {
        return new Vector2(
            Screen.width / 2f +
            offset.x,

            Screen.height / 2f +
            offset.y
        );
    }


    // =====================================================
    // DRAW LINE
    // =====================================================

    /// <summary>
    /// 使用 GUI 绘制两个节点之间的连接线段。
    /// </summary>
    void DrawLine(
        Vector2 start,
        Vector2 end,
        float width,
        Color color)
    {
        Matrix4x4 matrix =
            GUI.matrix;


        float angle =
            Mathf.Atan2(
                end.y - start.y,
                end.x - start.x
            )
            *
            Mathf.Rad2Deg;


        float length =
            Vector2.Distance(
                start,
                end
            );


        GUI.color =
            color;


        GUIUtility.RotateAroundPivot(
            angle,
            start
        );


        GUI.DrawTexture(
            new Rect(
                start.x,
                start.y - width / 2f,
                length,
                width
            ),
            Texture2D.whiteTexture
        );


        GUI.matrix =
            matrix;


        GUI.color =
            Color.white;
    }
}