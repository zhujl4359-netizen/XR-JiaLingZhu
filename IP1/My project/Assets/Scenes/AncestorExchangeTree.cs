using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

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

    void HandleEKey()
    {
        // -------------------------------------------------
        // If tree is open -> close
        // -------------------------------------------------

        if (treeOpen)
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

    void OpenTree()
    {
        treeOpen = true;

        selectedNode = -1;

        hoveredNode = -1;

        isPressing = false;

        pressingNode = -1;

        Cursor.visible = true;

        Cursor.lockState =
            CursorLockMode.None;

        Debug.Log(
            "Ancestor Tree Opened"
        );
    }


    // =====================================================
    // CLOSE TREE
    // =====================================================

    void CloseTree()
    {
        treeOpen = false;

        selectedNode = -1;

        hoveredNode = -1;

        isPressing = false;

        pressingNode = -1;

        Cursor.visible = true;

        Cursor.lockState =
            CursorLockMode.None;

        Debug.Log(
            "Ancestor Tree Closed"
        );
    }


    // =====================================================
    // LONG PRESS START
    // =====================================================

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


        Debug.Log(
            "Started holding: " +
            items[index].itemName
        );
    }


    // =====================================================
    // CHECK LONG PRESS
    // =====================================================

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

    void OnGUI()
    {
        if (!treeOpen)
            return;


        DrawBackground();

        DrawTitle();

        DrawConnections();

        DrawNodes();

        DrawSelectedItem();

        DrawInstruction();
    }


    // =====================================================
    // BACKGROUND
    // =====================================================

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
                "EXTRACTED";
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
                "Hold an unlocked item to extract it     ESC: Close";
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

        items[index].extracted =
            true;


        selectedNode =
            index;


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


    // =====================================================
    // SCREEN POSITION
    // =====================================================

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