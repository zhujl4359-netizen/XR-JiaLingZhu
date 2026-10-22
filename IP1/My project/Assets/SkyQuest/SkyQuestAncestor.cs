using System.Collections;
using TMPro;
using UnityEngine;

public class SkyQuestAncestor : MonoBehaviour
{
    [Header("Tracked devices")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform ancestor;
    public Transform memorialStone;
    public Transform avatar;
    public SkyQuestLocomotion locomotion;
    public SkyQuestStatus status;
    public SkyQuestMagicExchange magicExchange;

    [Header("Interaction")]
    public float proximity = 4f;
    public float summonRadius = 6f;
    public float dismissRadius = 8f;
    public float holdDuration = 0.55f;
    public GameObject maskPrefab;
    public bool desktopMode;

    public bool IsMenuOpen => desktopMode ? desktopMenuOpen : menuRoot != null && menuRoot.activeSelf;
    public bool IsNearAncestor => summoned && head != null && ancestor != null &&
        Vector3.Distance(head.position, ancestor.position + Vector3.up * .8f) <= proximity;

    private readonly string[] names = { "Cape Energy I", "Cape Energy II", "Cape Energy III", "Cape Energy IV", "Cape Energy V" };
    private readonly string[] glyphs = { "◆", "◆", "◆", "◆", "◆" };
    private readonly Color[] energyColors = {
        new Color(1f, .78f, .32f), new Color(.55f, .88f, 1f),
        new Color(.85f, .52f, 1f), new Color(.55f, 1f, .72f),
        new Color(1f, .55f, .72f)
    };
    private readonly Vector2[] positions =
    {
        new Vector2(0f, 0.72f), new Vector2(-0.72f, 0.05f), new Vector2(0.72f, 0.05f),
        new Vector2(-0.5f, -0.68f), new Vector2(0.5f, -0.68f)
    };
    private readonly bool[] obtained = new bool[5];
    private readonly bool[] equipped = new bool[5];
    private GameObject menuRoot;
    private Transform[] nodes;
    private TextMeshPro[] labels;
    private LineRenderer ray;
    private GameObject heldVisual;
    private int selected = -1;
    private int heldIndex = -1;
    private int focused = -1;
    private float pressStart;
    private Vector3 controllerDragStart;
    private float overheadStart = -1f;
    private float nextToggle;
    private bool wasTrigger;
    private bool wasMenuButton;
    private bool wasEquipButton;
    private bool overheadConsumed;
    private Coroutine moveToHandRoutine;
    private Renderer[] spiritRenderers;
    private bool[] spiritInitiallyVisible;
    private Transform memorial;
    private GameObject iconRoot;
    private Transform memorialIconAnchor;
    private bool summoned;
    private bool insideSummonRadius;
    private bool desktopMenuOpen;
    private SkyQuestAncestorEmergence emergence;
    private TextMeshPro hoverHelp;
    private GameObject hoverHelpPlate;
    private int guiDragIndex = -1;
    private Vector2 guiDragStart;
    private bool guiDragTriggered;
    private int hoveredNode = -1;
    private float hoveredSince;
    private int focusedBefore = -1;
    private float focusedSince;

    void Start()
    {
        if (ancestor == null || head == null || rightHand == null) return;
        spiritRenderers = ancestor.GetComponentsInChildren<Renderer>(true);
        spiritInitiallyVisible = new bool[spiritRenderers.Length];
        for (int i = 0; i < spiritRenderers.Length; i++)
            spiritInitiallyVisible[i] = spiritRenderers[i].enabled;
        MakeMemorial();
        emergence = ancestor.GetComponent<SkyQuestAncestorEmergence>();
        if (emergence == null) emergence = ancestor.gameObject.AddComponent<SkyQuestAncestorEmergence>();
        emergence.Initialize(ancestor, memorial);
        MakeMarker();
        MakeMenu();
        MakeRay();
        SetSummoned(false);
    }

    void Update()
    {
        if (ancestor == null || head == null || rightHand == null || leftHand == null) return;
        UpdateSummon();
        if (desktopMode)
        {
            UpdateIcon();
            if (IsMenuOpen && (!summoned || !IsNearAncestor)) CloseMenu();
            return;
        }
        UpdateIcon();
        bool menuButton = SkyQuestInput.Primary(SkyQuestInput.Left);
        bool overhead = IsNearAncestor &&
            leftHand.position.y > head.position.y + 0.04f &&
            rightHand.position.y > head.position.y + 0.04f &&
            SkyQuestInput.Grip(SkyQuestInput.Left) > 0.65f &&
            SkyQuestInput.Grip(SkyQuestInput.Right) > 0.65f;
        if (overhead)
        {
            if (overheadStart < 0f) overheadStart = Time.time;
            if (!overheadConsumed && Time.time - overheadStart > 0.4f && Time.time > nextToggle)
            {
                ToggleMenu();
                overheadConsumed = true;
            }
        }
        else { overheadStart = -1f; overheadConsumed = false; }
        bool legacyRequested = magicExchange == null || !magicExchange.IsNearAny ||
            SkyQuestInput.Grip(SkyQuestInput.Left) > 0.65f;
        if (menuButton && !wasMenuButton && IsNearAncestor && legacyRequested &&
            Time.time > nextToggle) ToggleMenu();
        wasMenuButton = menuButton;

        bool equip = SkyQuestInput.Grip(SkyQuestInput.Right) > 0.75f;
        if (equip && !wasEquipButton) EquipHeld();
        wasEquipButton = equip;
        if (!IsMenuOpen)
        {
            if (ray != null) ray.enabled = false;
            bool iconTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.65f;
            if (iconTrigger && !wasTrigger && IsNearAncestor && PointingAtIcon()) OpenMenu();
            wasTrigger = iconTrigger;
            return;
        }
        if (!IsNearAncestor) { CloseMenu(); return; }

        UpdateRay();
        if (focused != focusedBefore)
        {
            focusedBefore = focused;
            focusedSince = Time.unscaledTime;
        }
        if (hoverHelp != null)
            hoverHelp.text = focused >= 0 ?
                "HOLD RIGHT TRIGGER: AIM; RELEASE: THROW\nNEAR FACE + RIGHT TRIGGER: EAT +1 ENERGY" : "";
        if (hoverHelpPlate != null) hoverHelpPlate.SetActive(focused >= 0);
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.65f;
        if (trigger && !wasTrigger && focused >= 0 && !obtained[focused])
        {
            selected = focused;
            pressStart = Time.time;
            controllerDragStart = rightHand.position;
            status?.Show(names[selected] + " selected. Keep holding to obtain.");
        }
        if (trigger && selected >= 0 && !obtained[selected] &&
            ((focused == selected && Time.time - pressStart >= holdDuration) ||
             Vector3.Distance(rightHand.position, controllerDragStart) >= .14f))
        {
            Obtain(selected);
            selected = -1;
        }
        if (!trigger) selected = -1;
        wasTrigger = trigger;
        RefreshNodes();
    }

    private void OnGUI()
    {
        if (!desktopMode || head == null || ancestor == null) return;
        if (summoned && IsNearAncestor && !IsMenuOpen &&
            (magicExchange == null || !magicExchange.IsMenuOpen))
        {
            Camera camera = head.GetComponent<Camera>();
            if (camera != null)
            {
                Vector3 target = InteractionIconPosition();
                Vector3 screen = camera.WorldToScreenPoint(target);
                if (screen.z > 0f)
                {
                    float x = Mathf.Clamp(screen.x - 28f, 10f, Screen.width - 66f);
                    float y = Mathf.Clamp(Screen.height - screen.y - 28f, 10f, Screen.height - 66f);
                    Color old = GUI.color;
                    GUI.color = new Color(0.1f, 0.15f, 0.19f, 0.9f);
                    GUI.DrawTexture(new Rect(x, y, 56f, 56f), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    GUIStyle iconStyle = new GUIStyle(GUI.skin.button);
                    iconStyle.fontSize = 32;
                    iconStyle.fontStyle = FontStyle.Bold;
                    if (GUI.Button(new Rect(x, y, 56f, 56f), "✦", iconStyle)) OpenMenu();
                    GUI.color = old;
                }
            }
        }
        if (!IsMenuOpen) return;

        float width = Mathf.Clamp(Screen.width * 0.36f, 340f, 560f);
        float left = Screen.width - width;
        float top = 108f;
        float height = Screen.height - top;
        Color previous = GUI.color;
        GUI.color = new Color(0.06f, 0.13f, 0.2f, 0.85f);
        GUI.DrawTexture(new Rect(left, top, width, height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUIStyle heading = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(left + 20f, top + 15f, width - 80f, 38f), "ANCESTOR EXCHANGE", heading);
        if (GUI.Button(new Rect(Screen.width - 52f, top + 16f, 36f, 36f), "X")) CloseMenu();

        float[] px = { 0.50f, 0.25f, 0.75f, 0.34f, 0.66f };
        float[] py = { 0.08f, 0.25f, 0.43f, 0.61f, 0.77f };
        float treeTop = top + 65f;
        float treeHeight = Mathf.Max(320f, height - 150f);
        for (int i = 1; i < 5; i++)
        {
            int parent = i == 1 || i == 2 ? 0 : i - 2;
            Vector2 a = new Vector2(left + width * px[parent], treeTop + treeHeight * py[parent]);
            Vector2 b = new Vector2(left + width * px[i], treeTop + treeHeight * py[i]);
            DrawGuiLine(a, b, new Color(1f, 0.92f, 0.8f, 0.8f), 2f);
        }
        GUIStyle nodeStyle = new GUIStyle(GUI.skin.label) { fontSize = 29,
            fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 15,
            alignment = TextAnchor.MiddleCenter };
        Event guiEvent = Event.current;
        int currentHover = -1;
        for (int i = 0; i < 5; i++)
        {
            float cx = left + width * px[i];
            float cy = treeTop + treeHeight * py[i];
            Rect iconRect = new Rect(cx - 28f, cy - 28f, 56f, 56f);
            GUI.color = obtained[i] ? new Color(.55f, 1f, .75f) : energyColors[i];
            GUI.Label(iconRect, glyphs[i], nodeStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(cx - 74f, cy + 28f, 148f, 40f), names[i] +
                (obtained[i] ? "  Obtained" : ""), labelStyle);
            if (iconRect.Contains(guiEvent.mousePosition)) currentHover = i;
            HandleGuiNodeDrag(guiEvent, i, iconRect);
        }
        if (currentHover != hoveredNode)
        {
            hoveredNode = currentHover;
            hoveredSince = Time.unscaledTime;
        }
        if (hoveredNode >= 0)
        {
            Rect tooltip = new Rect(left + 40f, Screen.height - 140f, width - 80f, 40f);
            GUI.color = new Color(1f, .9f, .12f, .95f);
            GUI.DrawTexture(tooltip, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUIStyle tooltipStyle = new GUIStyle(labelStyle);
            tooltipStyle.normal.textColor = Color.black;
            GUI.Label(tooltip, "E: EAT +1 FLY ENERGY", tooltipStyle);
        }
        GUI.color = Color.white;
        GUI.Label(new Rect(left + 35f, Screen.height - 80f, width - 70f, 32f),
            "Drag an icon to exchange  ·  E to eat  ·  F to close", labelStyle);
        GUI.color = previous;
    }

    private void HandleGuiNodeDrag(Event guiEvent, int index, Rect iconRect)
    {
        if (guiEvent.button != 0) return;
        if (guiEvent.type == EventType.MouseDown && iconRect.Contains(guiEvent.mousePosition))
        {
            selected = index;
            guiDragIndex = index;
            guiDragStart = guiEvent.mousePosition;
            guiDragTriggered = false;
            guiEvent.Use();
            return;
        }
        if (guiDragIndex != index) return;
        if (guiEvent.type == EventType.MouseDrag && !guiDragTriggered &&
            Vector2.Distance(guiDragStart, guiEvent.mousePosition) >= 18f)
        {
            guiDragTriggered = true;
            Obtain(index);
            guiEvent.Use();
        }
        else if (guiEvent.type == EventType.MouseUp)
        {
            guiDragIndex = -1;
            guiDragTriggered = false;
            guiEvent.Use();
        }
    }

    private static void DrawGuiLine(Vector2 from, Vector2 to, Color color, float thickness)
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        Color oldColor = GUI.color;
        float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        GUIUtility.RotateAroundPivot(angle, from);
        GUI.color = color;
        GUI.DrawTexture(new Rect(from.x, from.y - thickness * 0.5f,
            Vector2.Distance(from, to), thickness), Texture2D.whiteTexture);
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
    }

    private void ToggleMenu()
    {
        if (IsMenuOpen) CloseMenu(); else OpenMenu();
        nextToggle = Time.time + 1f;
        overheadStart = -1f;
        SkyQuestInput.Pulse(SkyQuestInput.Left);
    }

    private void OpenMenu()
    {
        if (menuRoot == null) return;
        magicExchange?.CloseMenu();
        if (desktopMode)
        {
            desktopMenuOpen = true;
            status?.Show("Ancestor exchange: select an item on the right. F or X closes.", 5f);
            RefreshNodes();
            return;
        }
        Vector3 towardPlayer = head.position - ancestor.position;
        towardPlayer.y = 0f;
        towardPlayer = towardPlayer.sqrMagnitude > 0.01f ? towardPlayer.normalized : Vector3.forward;
        Vector3 viewForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (viewForward.sqrMagnitude < 0.01f) viewForward = transform.forward;
        Vector3 viewRight = Vector3.Cross(Vector3.up, viewForward);
        menuRoot.transform.position = head.position + viewForward * 2.2f + viewRight * 0.7f;
        menuRoot.transform.rotation = Quaternion.LookRotation(menuRoot.transform.position - head.position, Vector3.up);
        menuRoot.transform.localScale = Vector3.one * 0.68f;
        menuRoot.SetActive(true);
        status?.Show(desktopMode ? "Ancestor items: 1-5 select, Enter obtain, E equip, F close." :
            "Ancestor items: point and hold right trigger. X closes the tree.", 5f);
        RefreshNodes();
    }

    private void CloseMenu()
    {
        desktopMenuOpen = false;
        if (menuRoot != null) menuRoot.SetActive(false);
        if (ray != null) ray.enabled = false;
        selected = -1;
        focused = -1;
        guiDragIndex = hoveredNode = focusedBefore = -1;
    }

    public void CloseForMagic() { if (IsMenuOpen) CloseMenu(); }

    private void MakeMarker()
    {
        memorialIconAnchor = FindMemorialIconAnchor();
        Transform marker = new GameObject("Ancestor interaction marker").transform;
        marker.position = InteractionIconPosition();
        iconRoot = marker.gameObject;
        GameObject orb = SkyQuestVisuals.Primitive("Interact with ancestor", PrimitiveType.Sphere,
            marker, Vector3.zero, Vector3.one * 0.36f, new Color(0.14f, 0.17f, 0.22f), true);
        orb.AddComponent<SkyQuestAncestorIcon>();
        GameObject vertical = SkyQuestVisuals.Primitive("Icon vertical ray", PrimitiveType.Cube,
            marker, new Vector3(0f, 0f, -0.19f), new Vector3(0.035f, 0.22f, 0.025f), Color.white, true);
        GameObject horizontal = SkyQuestVisuals.Primitive("Icon horizontal ray", PrimitiveType.Cube,
            marker, new Vector3(0f, 0f, -0.19f), new Vector3(0.22f, 0.035f, 0.025f), Color.white, true);
        SkyQuestVisuals.RemoveCollider(vertical);
        SkyQuestVisuals.RemoveCollider(horizontal);
    }

    private void MakeMemorial()
    {
        if (memorialStone != null)
        {
            memorial = memorialStone;
            return;
        }
        memorial = new GameObject("Ancestor memorial stone").transform;
        memorial.position = ancestor.position + new Vector3(0f, 0f, 0.8f);
        var stone = SkyQuestVisuals.Primitive("Carved memorial", PrimitiveType.Cube, memorial,
            new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 1.8f, 0.38f),
            new Color(0.24f, 0.37f, 0.43f));
        stone.transform.rotation = Quaternion.Euler(0f, 0f, -6f);
        SkyQuestVisuals.Text("Memorial symbol", "◇", memorial,
            new Vector3(0f, 1.35f, -0.21f), 2f, new Color(1f, 0.88f, 0.64f), 0.44f);
    }

    private void UpdateSummon()
    {
        if (memorial == null) return;
        Vector3 player = avatar != null ? avatar.position + Vector3.up * 1.2f :
            (head != null ? head.position : transform.position);
        Vector3 delta = player - memorial.position;
        delta.y = 0f;
        float distance = delta.magnitude;
        bool isInside = distance <= summonRadius;
        if (isInside && !insideSummonRadius)
        {
            insideSummonRadius = true;
            if (!summoned) SetSummoned(true);
            else emergence?.PlayReveal();
        }
        else if (!isInside && distance > summonRadius + .25f)
            insideSummonRadius = false;

        if (summoned && distance >= dismissRadius) SetSummoned(false);
    }

    private void SetSummoned(bool visible)
    {
        summoned = visible;
        if (emergence != null)
        {
            if (visible) emergence.PlayReveal();
            else emergence.PlayDismiss();
        }
        else if (spiritRenderers != null)
        {
            for (int i = 0; i < spiritRenderers.Length; i++)
                if (spiritRenderers[i] != null)
                    spiritRenderers[i].enabled = visible && spiritInitiallyVisible[i];
        }
        if (iconRoot != null) iconRoot.SetActive(false);
        if (!visible && IsMenuOpen) CloseMenu();
    }

    private void UpdateIcon()
    {
        bool show = summoned && IsNearAncestor && !IsMenuOpen &&
            (magicExchange == null || !magicExchange.IsMenuOpen);
        if (iconRoot != null) iconRoot.SetActive(show);
        if (show && iconRoot != null)
        {
            iconRoot.transform.position = InteractionIconPosition();
            iconRoot.transform.rotation = Quaternion.LookRotation(iconRoot.transform.position - head.position);
        }
    }

    private Vector3 InteractionIconPosition()
    {
        if (memorialIconAnchor != null) return memorialIconAnchor.position;
        if (memorial != null) return memorial.position + Vector3.up * 1.35f;
        return ancestor.position + Vector3.up * 2f;
    }

    private Transform FindMemorialIconAnchor()
    {
        if (memorial == null) return null;
        Transform fallback = null;
        foreach (Transform child in memorial.GetComponentsInChildren<Transform>(true))
        {
            string childName = child.name.ToLowerInvariant();
            if (childName == "memorial star" || childName == "memorial symbol") return child;
            if (fallback == null && (childName.Contains("star") || childName.Contains("symbol")))
                fallback = child;
        }
        return fallback;
    }

    private bool PointingAtIcon()
    {
        RaycastHit[] hits = Physics.RaycastAll(rightHand.position, rightHand.forward, 4f);
        foreach (RaycastHit hit in hits)
            if (hit.collider.GetComponent<SkyQuestAncestorIcon>() != null) return true;
        return false;
    }

    private void MakeMenu()
    {
        menuRoot = new GameObject("Ancestor spatial item tree");
        menuRoot.transform.SetParent(transform, false);
        nodes = new Transform[names.Length];
        labels = new TextMeshPro[names.Length];
        GameObject backing = SkyQuestVisuals.Primitive("Tree backing", PrimitiveType.Cube,
            menuRoot.transform, new Vector3(0f, 0f, 0.1f), new Vector3(3.25f, 2.85f, 0.05f),
            new Color(0.11f, 0.12f, 0.2f, 0.75f));
        SkyQuestVisuals.RemoveCollider(backing);
        backing.SetActive(false);
        SkyQuestVisuals.Text("Title", "ANCESTOR ITEMS", menuRoot.transform,
            new Vector3(0f, 1.28f, -0.04f), 2.4f, new Color(1f, 0.87f, 0.58f), 1.1f);
        for (int i = 0; i < names.Length; i++)
        {
            GameObject sphere = SkyQuestVisuals.Primitive(names[i] + " node", PrimitiveType.Sphere,
                menuRoot.transform, new Vector3(positions[i].x, positions[i].y, -0.08f),
                Vector3.one * 0.19f, Color.white);
            sphere.AddComponent<SkyQuestNode>().index = i;
            nodes[i] = sphere.transform;
            labels[i] = SkyQuestVisuals.Text(names[i] + " label", (i + 1) + " " + names[i],
                menuRoot.transform, new Vector3(positions[i].x, positions[i].y - 0.19f, -0.11f),
                1.55f, Color.white, 0.42f);
        }
        for (int i = 1; i < names.Length; i++)
        {
            int parent = i < 3 ? 0 : i - 2;
            GameObject link = new GameObject("Exchange branch " + i, typeof(LineRenderer));
            link.transform.SetParent(menuRoot.transform, false);
            LineRenderer line = link.GetComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(positions[parent].x, positions[parent].y, -0.06f));
            line.SetPosition(1, new Vector3(positions[i].x, positions[i].y, -0.06f));
            line.startWidth = line.endWidth = 0.012f;
            line.material = SkyQuestVisuals.Material(new Color(1f, 0.92f, 0.77f));
        }
        SkyQuestVisuals.Text("Instructions", desktopMode ?
                "Drag icon: exchange    E: eat    F: close" :
                "Right trigger: hold or pull icon to exchange    Right grip: pick up / drop",
            menuRoot.transform, new Vector3(0f, -1.24f, -0.08f), 1.35f, Color.white, 1.05f);
        hoverHelp = SkyQuestVisuals.Text("Energy reward hint", "", menuRoot.transform,
            new Vector3(0f, -1.02f, -0.08f), 1.4f, Color.black, 1.2f);
        hoverHelpPlate = SkyQuestVisuals.Primitive("Energy hint yellow plate", PrimitiveType.Cube,
            menuRoot.transform, new Vector3(0f, -1.02f, -.06f),
            new Vector3(.33f, .21f, .012f), new Color(1f, .9f, .12f), true);
        SkyQuestVisuals.RemoveCollider(hoverHelpPlate);
        hoverHelpPlate.SetActive(false);
        if (desktopMode) hoverHelp.enabled = false;
        menuRoot.SetActive(false);
    }

    private void MakeRay()
    {
        GameObject pointer = new GameObject("Ancestor controller ray", typeof(LineRenderer));
        pointer.transform.SetParent(transform, false);
        ray = pointer.GetComponent<LineRenderer>();
        ray.positionCount = 2;
        ray.startWidth = 0.01f;
        ray.endWidth = 0.005f;
        ray.material = SkyQuestVisuals.Material(new Color(1f, 0.85f, 0.45f));
        ray.enabled = false;
    }

    private void UpdateRay()
    {
        RaycastHit[] hits = Physics.RaycastAll(rightHand.position, rightHand.forward, 4f);
        focused = -1;
        float bestDistance = float.PositiveInfinity;
        Vector3 end = rightHand.position + rightHand.forward * 3f;
        foreach (RaycastHit hit in hits)
        {
            SkyQuestNode node = hit.collider.GetComponent<SkyQuestNode>();
            if (node == null || hit.distance >= bestDistance) continue;
            bestDistance = hit.distance;
            focused = node.index;
            end = hit.point;
        }
        ray.enabled = true;
        ray.SetPosition(0, rightHand.position);
        ray.SetPosition(1, end);
    }

    private void RefreshNodes()
    {
        if (nodes == null) return;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] == null) continue;
            Color color = equipped[i] ? new Color(0.35f, 1f, 0.7f) :
                obtained[i] ? new Color(0.35f, 0.7f, 1f) :
                selected == i ? new Color(1f, 0.6f, 0.25f) :
                focused == i ? new Color(1f, 0.95f, 0.5f) : Color.white;
            nodes[i].GetComponent<Renderer>().material.color = color;
            labels[i].text = (i + 1) + " " + names[i] +
                (equipped[i] ? " EQUIPPED" : obtained[i] ? " OBTAINED" : selected == i ? " SELECTED" : "");
        }
    }

    private void Obtain(int index)
    {
        if (index < 0 || index >= obtained.Length || obtained[index]) return;
        obtained[index] = true;
        GameObject reward = new GameObject(names[index] + " reward");
        GameObject cube = SkyQuestVisuals.Primitive("Cape energy cube", PrimitiveType.Cube,
            reward.transform, new Vector3(0f, .3f, 0f), Vector3.one * .45f,
            energyColors[index], true);
        Collider cubeCollider = cube.GetComponent<Collider>();
        if (cubeCollider != null) cubeCollider.enabled = false;
        SkyQuestVisuals.RemoveCollider(cube);
        reward.transform.position = rightHand.position;
        SkyQuestMagicPickup pickup = reward.AddComponent<SkyQuestMagicPickup>();
        pickup.desktopHead = head;
        pickup.desktopHand = rightHand;
        pickup.desktopAvatar = avatar;
        pickup.desktopMode = desktopMode;
        pickup.restoresCapeEnergy = true;
        pickup.SetHeld(rightHand);
        status?.Show(names[index] + (desktopMode ?
            " exchanged. E to eat for +1 fly energy; Z to drop/pick up." :
            " exchanged. Point and hold right trigger to aim; release to throw. Hold near face to eat."), 6f);
        status?.PlayConfirm();
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.6f, 0.15f);
        RefreshNodes();
    }

    private IEnumerator MoveToHand(GameObject item)
    {
        Vector3 start = item.transform.position;
        float elapsed = 0f;
        while (elapsed < 0.55f && item != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.55f);
            item.transform.position = Vector3.Lerp(start, rightHand.position, t);
            yield return null;
        }
        if (item != null)
        {
            item.transform.SetParent(rightHand, false);
            item.transform.localPosition = new Vector3(0f, 0f, 0.12f);
            item.transform.localRotation = Quaternion.identity;
        }
        moveToHandRoutine = null;
    }

    public void EquipHeld()
    {
        if (heldIndex < 0 || avatar == null || heldVisual == null) return;
        if (moveToHandRoutine != null)
        {
            StopCoroutine(moveToHandRoutine);
            moveToHandRoutine = null;
        }
        int index = heldIndex;
        heldVisual.transform.SetParent(avatar, false);
        heldVisual.transform.localPosition = index == 2 ? new Vector3(0f, 1.5f, 0.23f) :
            index == 0 ? new Vector3(0f, 1.1f, -0.18f) : new Vector3(0f, 1.65f, 0.05f);
        heldVisual.transform.localRotation = Quaternion.identity;
        heldVisual.transform.localScale = index == 2 ? new Vector3(0.34f, 0.22f, 0.12f) : Vector3.one * 0.3f;
        equipped[index] = true;
        heldIndex = -1;
        heldVisual = null;
        CloseMenu();
        locomotion?.ShowEquippedAppearance();
        status?.Show(names[index] + " EQUIPPED", 4f);
        status?.PlayConfirm();
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.5f, 0.12f);
    }

    public void DesktopToggleMenu()
    {
        if (!desktopMode) return;
        if (IsMenuOpen) CloseMenu();
        else if (IsNearAncestor)
        {
            magicExchange?.CloseMenu();
            OpenMenu();
        }
        else status?.Show("Move closer to the Ancestor before opening the tree.");
    }

    public void DesktopSelect(int index)
    {
        if (!desktopMode || !IsMenuOpen || index < 0 || index >= names.Length) return;
        if (obtained[index]) { status?.Show(names[index] + " is already obtained."); return; }
        selected = index;
        status?.Show(names[index] + " selected. Press Enter to obtain.", 4f);
        RefreshNodes();
    }

    public void DesktopObtainSelected()
    {
        if (!desktopMode || !IsMenuOpen || selected < 0 || obtained[selected]) return;
        int index = selected;
        selected = -1;
        Obtain(index);
    }

    private GameObject MakeItemVisual(int index)
    {
        if (index == 2 && maskPrefab != null)
        {
            GameObject root = new GameObject("Obtained mask");
            GameObject model = Instantiate(maskPrefab, root.transform);
            model.SetActive(true);
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
                if (bounds.size.y > 0.001f)
                {
                    float scale = 1f / bounds.size.y;
                    model.transform.localScale *= scale;
                    model.transform.position -= (bounds.center - root.transform.position) * scale;
                }
            }
            foreach (Collider collider in model.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
                Destroy(collider);
            }
            return root;
        }
        PrimitiveType shape = index == 0 ? PrimitiveType.Capsule : index == 4 ? PrimitiveType.Cylinder : PrimitiveType.Sphere;
        GameObject item = SkyQuestVisuals.Primitive(names[index] + " item", shape, transform,
            Vector3.zero, Vector3.one * 0.2f, new Color(1f, 0.8f, 0.45f), true);
        SkyQuestVisuals.RemoveCollider(item);
        return item;
    }
}

public class SkyQuestAncestorIcon : MonoBehaviour { }

public class SkyQuestNode : MonoBehaviour
{
    public int index;
}
