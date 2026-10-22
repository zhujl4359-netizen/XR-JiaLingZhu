using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public static class SkyQuestCandleWallet
{
    public static int Balance { get; private set; } = 100;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetBalance() => Balance = 100;

    public static void Add(int amount)
    {
        if (amount > 0) Balance += amount;
    }

    public static bool TrySpend(int amount)
    {
        if (amount < 0 || Balance < amount) return false;
        Balance -= amount;
        return true;
    }
}

// Independent exchange trees for every visible gallery character. Rewards become
// tangible Sky-inspired props and move directly into the player's right hand.
public class SkyQuestMagicExchange : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform avatar;
    public SkyQuestStatus status;
    public SkyQuestAncestor legacyTree;
    public bool desktopMode;
    public Transform[] ancestors = new Transform[6];
    public Transform[] memorials = new Transform[6];
    public GameObject[] ceramicRewardPrefabs = new GameObject[8];
    public float proximity = 3.8f;
    public float holdDuration = 0.55f;
    public float summonRadius = 6f;
    public float dismissRadius = 8f;

    private static readonly string[,] SpellNames =
    {
        { "Glow Spell", "Anti-Gravity Spell", "Fast Recharge Spell", "Fireworks Spell", "Shrink Spell" },
        { "Rainbow Trail Spell", "Resize Potion", "Chat Table Spell", "Bonfire Spell", "Instant Recharge Potion" },
        { "Flight Energy I", "Flight Energy II", "Flight Energy III", "Flight Energy IV", "Flight Energy V" },
        { "Candlelight Spell", "Warmth Spell", "Glitter Spell", "Trail Spell", "Bloom Spell" },
        { "Celadon Offering", "Ivory Ritual Jar", "Azure Incense Vessel", "Amber Offering Bowl", "Moon Glaze Vase" },
        { "Flight Energy I", "Flight Energy II", "Flight Energy III", "Flight Energy IV", "Flight Energy V" }
    };
    private static readonly string[,] DisplayNames =
    {
        { "Glow", "Anti-Gravity", "Fast Recharge", "Fireworks", "Shrink" },
        { "Rainbow Trail", "Resize Potion", "Chat Table", "Bonfire", "Instant Recharge" },
        { "Starlight", "Cloud", "Lantern", "Feather", "Moonlight" },
        { "Candlelight", "Warmth", "Glitter", "Trail", "Bloom" },
        { "Celadon Offering", "Ivory Ritual Jar", "Azure Incense Vessel", "Amber Offering Bowl", "Moon Glaze Vase" },
        { "Cloud Pattern Urn", "Temple Tea Vessel", "Gold Rim Pottery", "Calm", "Wish" }
    };
    private static readonly string[] TreeTitles =
    {
        "SHIBIYAMA DANCER", "PRAYER GUARDIAN", "COTL TRAVELLER",
        "CANDLE PILGRIM", "SKELETON PILGRIM", "C1 MEMORY"
    };
    private static readonly Color[] CubeColors =
    {
        new Color(1f, .78f, .3f), new Color(.62f, .8f, 1f), new Color(.42f, 1f, .72f),
        new Color(1f, .42f, .35f), new Color(.72f, .55f, 1f),
        new Color(1f, .5f, .8f), new Color(.72f, .95f, 1f), new Color(.77f, .58f, .38f),
        new Color(1f, .55f, .2f), new Color(.95f, .95f, .65f),
        new Color(.72f, .91f, 1f), new Color(.86f, .94f, 1f), new Color(1f, .82f, .48f),
        new Color(.94f, .88f, .7f), new Color(.72f, .74f, 1f),
        new Color(1f, .78f, .42f), new Color(1f, .6f, .33f), new Color(.98f, .87f, .54f),
        new Color(.67f, .81f, 1f), new Color(.79f, 1f, .62f),
        new Color(.65f, .9f, .96f), new Color(.6f, .75f, .91f), new Color(.78f, .89f, .98f),
        new Color(.59f, .61f, .82f), new Color(.95f, .88f, .74f),
        new Color(1f, .88f, .65f), new Color(.83f, .83f, 1f), new Color(1f, .94f, .51f),
        new Color(.71f, .97f, .88f), new Color(.98f, .75f, .85f)
    };
    private static readonly Vector2[] NodePositions =
    {
        new Vector2(0f, -.86f), new Vector2(-.46f, -.43f), new Vector2(0f, 0f),
        new Vector2(.46f, .43f), new Vector2(0f, .86f)
    };
    private static readonly string[] NodeGlyphs = { "✧", "♥", "✧", "✺", "+" };

    private readonly bool[,] obtained = new bool[6, 5];
    private Renderer[][] spiritRenderers;
    private bool[][] originalRendererStates;
    private bool[] summoned;
    private bool[] insideSummonRadius;
    private SkyQuestAncestorEmergence[] emergence;
    private GameObject[] icons;
    private Transform[] iconAnchors;
    private GameObject menuRoot;
    private Transform[] nodes;
    private TextMeshPro[] labels;
    private TextMeshPro[] iconGlyphs;
    private TextMeshPro title;
    private TextMeshPro prompt;
    private TextMeshPro hoverHelp;
    private GameObject hoverHelpPlate;
    private LineRenderer pointer;
    private int activeAncestor = -1;
    private int selected = -1;
    private int focused = -1;
    private float holdStarted;
    private Vector3 controllerDragStart;
    private float nextToggle;
    private bool previousX;
    private bool previousTrigger;
    private readonly int[] candleCosts = { 10, 8, 12, 15, 20 };
    private SkyQuestMagicPickup heldCube;
    private int guiDragIndex = -1;
    private Vector2 guiDragStart;
    private bool guiDragTriggered;
    private int hoveredNode = -1;
    private float hoveredSince;
    private int focusedBefore = -1;
    private float focusedSince;

    public bool IsMenuOpen => menuRoot != null && menuRoot.activeSelf;
    public bool IsNearAny => NearestAncestor() >= 0;

    void Start()
    {
        MakeMenu();
        MakePointer();
        int count = ancestors == null ? 0 : ancestors.Length;
        spiritRenderers = new Renderer[count][];
        originalRendererStates = new bool[count][];
        summoned = new bool[count];
        insideSummonRadius = new bool[count];
        emergence = new SkyQuestAncestorEmergence[count];
        icons = new GameObject[count];
        iconAnchors = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            Transform ancestor = ancestors[i];
            if (ancestor == null) continue;
            spiritRenderers[i] = ancestor.GetComponentsInChildren<Renderer>(true);
            originalRendererStates[i] = new bool[spiritRenderers[i].Length];
            for (int j = 0; j < spiritRenderers[i].Length; j++)
                originalRendererStates[i][j] = spiritRenderers[i][j].enabled;
            emergence[i] = ancestor.GetComponent<SkyQuestAncestorEmergence>();
            if (emergence[i] == null)
                emergence[i] = ancestor.gameObject.AddComponent<SkyQuestAncestorEmergence>();
            Transform stone = memorials != null && i < memorials.Length ? memorials[i] : null;
            emergence[i].Initialize(ancestor, stone);
            iconAnchors[i] = FindMemorialIconAnchor(stone);
            Transform marker = new GameObject(ancestor.name + " interaction icon").transform;
            marker.position = InteractionIconPosition(i);
            GameObject orb = SkyQuestVisuals.Primitive("Interact", PrimitiveType.Sphere,
                marker, Vector3.zero, Vector3.one * .34f, new Color(1f, .52f, .66f), true);
            orb.AddComponent<SkyQuestGalleryIcon>().index = i;
            TextMeshPro openGlyph = SkyQuestVisuals.Text("Exchange icon glyph", "✦", marker,
                new Vector3(0f, 0f, -.23f), 6.4f, Color.white, 1.3f);
            openGlyph.fontStyle = FontStyles.Bold;
            openGlyph.textWrappingMode = TextWrappingModes.NoWrap;
            icons[i] = marker.gameObject;
            SetSummoned(i, false);
        }
        RegisterScenePottery();
    }

    private void RegisterScenePottery()
    {
        // The eight A242 vessels in the temple were scenery only. Give them
        // the same single-item carry interaction as exchanged rewards while
        // keeping them anchored on their plinths until first picked up.
        foreach (Transform vessel in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!vessel.name.StartsWith("A242 ceramic display ", StringComparison.Ordinal)) continue;
            SkyQuestMagicPickup pickup = vessel.GetComponent<SkyQuestMagicPickup>();
            if (pickup == null) pickup = vessel.gameObject.AddComponent<SkyQuestMagicPickup>();
            pickup.desktopHead = head;
            pickup.desktopHand = rightHand;
            pickup.desktopAvatar = avatar;
            pickup.desktopMode = desktopMode;
            pickup.AnchorOnDisplay();
        }
    }

    void Update()
    {
        if (head == null || leftHand == null || rightHand == null) return;
        UpdateSummons();
        UpdateIcons();
        if (IsMenuOpen && (activeAncestor < 0 || !IsNear(activeAncestor))) CloseMenu();
        if (desktopMode)
        {
            heldCube = SkyQuestMagicPickup.Held;
            UpdateDesktopHover();
            return;
        }

        bool x = SkyQuestInput.Primary(SkyQuestInput.Left);
        bool legacyModifier = SkyQuestInput.Grip(SkyQuestInput.Left) > .65f;
        if (x && !previousX && !legacyModifier &&
            (legacyTree == null || !legacyTree.IsMenuOpen) && Time.time >= nextToggle)
            ToggleNearest();
        previousX = x;
        if (!IsMenuOpen)
        {
            if (pointer != null) pointer.enabled = false;
            bool iconTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .65f;
            if (iconTrigger && !previousTrigger)
            {
                foreach (RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 5f))
                {
                    SkyQuestGalleryIcon icon = hit.collider.GetComponent<SkyQuestGalleryIcon>();
                    if (icon == null || !IsNear(icon.index)) continue;
                    OpenMenu(icon.index);
                    break;
                }
            }
            previousTrigger = iconTrigger;
            return;
        }

        UpdatePointer();
        if (focused != focusedBefore)
        {
            focusedBefore = focused;
            focusedSince = Time.unscaledTime;
        }
        if (hoverHelp != null)
        {
            bool show = focused >= 0;
            hoverHelp.text = show ? RewardDisplayName(activeAncestor, focused) + "\n" +
                RewardTooltip(activeAncestor, desktopMode) : "";
            if (hoverHelpPlate != null) hoverHelpPlate.SetActive(show);
            if (show)
            {
                hoverHelp.transform.localPosition = new Vector3(
                    NodePositions[focused].x, NodePositions[focused].y + .27f, -.3f);
                hoverHelpPlate.transform.localPosition = new Vector3(
                    NodePositions[focused].x, NodePositions[focused].y + .27f, -.27f);
            }
        }
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .65f;
        if (trigger && !previousTrigger && focused >= 0 && !obtained[activeAncestor, focused])
        {
            selected = focused;
            holdStarted = Time.time;
            controllerDragStart = rightHand.position;
            status?.Show(SpellNames[activeAncestor, selected] + " selected. Hold or pull outward to exchange.");
        }
        bool pulledFromIcon = selected >= 0 &&
            Vector3.Distance(rightHand.position, controllerDragStart) >= .14f;
        bool heldOnIcon = selected >= 0 && focused == selected &&
            Time.time - holdStarted >= holdDuration;
        if (trigger && selected >= 0 && (pulledFromIcon || heldOnIcon) &&
            !obtained[activeAncestor, selected])
        {
            Obtain(selected);
            selected = -1;
        }
        if (!trigger) selected = -1;
        previousTrigger = trigger;
        RefreshNodes();
    }

    private void UpdateSummons()
    {
        if (summoned == null || insideSummonRadius == null) return;
        Vector3 player = PlayerPosition;
        for (int i = 0; i < summoned.Length; i++)
        {
            if (ancestors[i] == null) continue;
            Vector3 center = memorials != null && i < memorials.Length && memorials[i] != null ?
                memorials[i].position : ancestors[i].position;
            float distance = Vector3.ProjectOnPlane(player - center, Vector3.up).magnitude;
            bool isInside = distance <= summonRadius;
            if (isInside && !insideSummonRadius[i])
            {
                insideSummonRadius[i] = true;
                if (!summoned[i]) SetSummoned(i, true);
                else if (emergence != null && i < emergence.Length && emergence[i] != null)
                    emergence[i].PlayReveal();
            }
            else if (!isInside && distance > summonRadius + .25f)
                insideSummonRadius[i] = false;

            if (summoned[i] && distance >= dismissRadius) SetSummoned(i, false);
        }
    }

    private void SetSummoned(int index, bool visible)
    {
        if (summoned == null || index < 0 || index >= summoned.Length) return;
        summoned[index] = visible;
        Renderer[] renderers = spiritRenderers != null && index < spiritRenderers.Length ?
            spiritRenderers[index] : null;
        bool[] rendererStates = originalRendererStates != null && index < originalRendererStates.Length ?
            originalRendererStates[index] : null;
        if (emergence != null && index < emergence.Length && emergence[index] != null)
        {
            if (visible) emergence[index].PlayReveal();
            else emergence[index].PlayDismiss();
        }
        else if (renderers != null && rendererStates != null)
        {
            for (int j = 0; j < renderers.Length && j < rendererStates.Length; j++)
                if (renderers[j] != null)
                    renderers[j].enabled = visible && rendererStates[j];
        }
        if (!visible && activeAncestor == index && IsMenuOpen) CloseMenu();
        if (icons[index] != null) icons[index].SetActive(false);
    }

    private void UpdateIcons()
    {
        if (icons == null) return;
        for (int i = 0; i < icons.Length; i++)
        {
            if (icons[i] == null || ancestors[i] == null) continue;
            bool show = IsNear(i) && !IsMenuOpen &&
                (legacyTree == null || !legacyTree.IsMenuOpen);
            icons[i].SetActive(show);
            if (!show) continue;
            icons[i].transform.position = InteractionIconPosition(i);
            icons[i].transform.rotation = Quaternion.LookRotation(icons[i].transform.position - head.position);
        }
    }

    private Vector3 InteractionIconPosition(int index)
    {
        if (iconAnchors != null && index >= 0 && index < iconAnchors.Length && iconAnchors[index] != null)
            return iconAnchors[index].position;
        Transform stone = memorials != null && index >= 0 && index < memorials.Length ? memorials[index] : null;
        if (stone != null) return stone.position + Vector3.up * 1.35f;
        return ancestors[index].position + Vector3.up * 2.1f;
    }

    private static Transform FindMemorialIconAnchor(Transform stone)
    {
        if (stone == null) return null;
        Transform fallback = null;
        foreach (Transform child in stone.GetComponentsInChildren<Transform>(true))
        {
            string childName = child.name.ToLowerInvariant();
            if (childName == "memorial star") return child;
            if (fallback == null && (childName.Contains("star") || childName.Contains("symbol")))
                fallback = child;
        }
        return fallback;
    }

    private void OnGUI()
    {
        if (!desktopMode || head == null) return;
        Camera camera = head.GetComponent<Camera>();
        if (camera != null && icons != null && !IsMenuOpen)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                if (!IsNear(i) || icons[i] == null || !icons[i].activeSelf) continue;
                Vector3 point = camera.WorldToScreenPoint(icons[i].transform.position);
                if (point.z <= 0f) continue;
                float x = Mathf.Clamp(point.x - 28f, 10f, Screen.width - 66f);
                float y = Mathf.Clamp(Screen.height - point.y - 28f, 10f, Screen.height - 66f);
                GUIStyle iconHaloStyle = new GUIStyle(GUI.skin.label) { fontSize = 50,
                    fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                iconHaloStyle.normal.textColor = new Color(1f, .58f, .7f, .78f);
                GUI.Label(new Rect(x, y, 56f, 56f), "●", iconHaloStyle);
                GUIStyle iconStyle = new GUIStyle(GUI.skin.label) { fontSize = 30,
                    fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                iconStyle.normal.textColor = Color.white;
                if (GUI.Button(new Rect(x, y, 56f, 56f), "✦", iconStyle)) OpenMenu(i);
                GUIStyle iconNameStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                iconNameStyle.normal.textColor = new Color(1f, .9f, .7f);
                GUI.Label(new Rect(x - 76f, y - 26f, 208f, 24f), TreeTitles[i], iconNameStyle);
            }
        }
        if (!IsMenuOpen || activeAncestor < 0 || activeAncestor >= obtained.GetLength(0)) return;
        float width = Mathf.Clamp(Screen.width * .36f, 340f, 560f);
        float left = Screen.width - width;
        float top = 108f;
        float height = Screen.height - top;
        Rect panel = new Rect(left, top, width, height);
        Color previous = GUI.color;
        GUI.color = new Color(.06f, .13f, .2f, .85f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = previous;
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 23;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(1f, .87f, .62f);
        GUI.Label(new Rect(left + 18f, top + 14f, width - 72f, 38f),
            TreeTitles[activeAncestor], titleStyle);
        GUIStyle candleStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, alignment = TextAnchor.MiddleLeft };
        candleStyle.normal.textColor = new Color(1f, .88f, .62f);
        GUI.Label(new Rect(left + 22f, top + 52f, width - 44f, 28f), "Candles: " + SkyQuestCandleWallet.Balance, candleStyle);
        if (GUI.Button(new Rect(Screen.width - 52f, top + 16f, 36f, 36f), "X"))
        {
            CloseMenu();
            return;
        }
        float treeTop = top + 88f;
        float treeHeight = Mathf.Max(360f, height - 210f);
        float[] px = { .50f, .34f, .50f, .66f, .50f };
        float[] py = { .88f, .70f, .52f, .34f, .16f };
        Vector2[] points = {
            new Vector2(left + width * px[0], treeTop + treeHeight * py[0]),
            new Vector2(left + width * px[1], treeTop + treeHeight * py[1]),
            new Vector2(left + width * px[2], treeTop + treeHeight * py[2]),
            new Vector2(left + width * px[3], treeTop + treeHeight * py[3]),
            new Vector2(left + width * px[4], treeTop + treeHeight * py[4])
        };
        for (int i = 1; i < points.Length; i++) DrawGuiBranch(points[i - 1], points[i]);
        GUIStyle glyphStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
            fontSize = 34, fontStyle = FontStyle.Bold };
        glyphStyle.normal.textColor = Color.white;
        GUIStyle nameStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft,
            fontSize = 16, fontStyle = FontStyle.Bold, wordWrap = false,
            clipping = TextClipping.Overflow };
        GUIStyle costStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft,
            fontSize = 14, wordWrap = false, clipping = TextClipping.Overflow };
        Event guiEvent = Event.current;
        for (int i = 0; i < points.Length; i++)
        {
            bool claimed = obtained[activeAncestor, i];
            bool locked = i > 0 && !obtained[activeAncestor, i - 1];
            Rect iconRect = new Rect(points[i].x - 32f, points[i].y - 32f, 64f, 64f);
            Color baseIconColor = IsEnergyReward(activeAncestor) ? CubeColors[activeAncestor * 5 + i] :
                new Color(1f, .64f, .74f);
            glyphStyle.normal.textColor = claimed ? new Color(.55f, 1f, .8f) :
                locked ? new Color(1f, 1f, 1f, .62f) :
                selected == i ? new Color(1f, .82f, .42f) : baseIconColor;
            GUI.Label(iconRect, IsEnergyReward(activeAncestor) ? "◆" : NodeGlyphs[i], glyphStyle);
            bool labelOnRight = i % 2 == 0;
            float labelX = labelOnRight ? points[i].x + 39f : points[i].x - 183f;
            nameStyle.alignment = labelOnRight ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
            costStyle.alignment = nameStyle.alignment;
            nameStyle.normal.textColor = claimed ? new Color(.55f, 1f, .8f) :
                locked ? new Color(.62f, .65f, .7f) : Color.white;
            costStyle.normal.textColor = new Color(1f, .78f, .68f);
            GUI.Label(new Rect(labelX, points[i].y - 19f, 180f, 23f),
                (i + 1) + "  " + RewardDisplayName(activeAncestor, i), nameStyle);
            GUI.Label(new Rect(labelX, points[i].y + 3f, 180f, 20f),
                claimed ? "Obtained" : locked ? "Locked" : candleCosts[i] + " candles · drag", costStyle);
            HandleGuiNodeDrag(guiEvent, i, iconRect);
        }
        if (hoveredNode >= 0)
        {
            const float tooltipWidth = 310f;
            Rect helpRect = new Rect(
                Mathf.Clamp(points[hoveredNode].x - tooltipWidth * .5f,
                    left + 12f, Screen.width - tooltipWidth - 12f),
                Mathf.Clamp(points[hoveredNode].y - 105f, top + 86f, Screen.height - 160f),
                tooltipWidth, 76f);
            GUI.color = new Color(1f, .9f, .12f, .95f);
            GUI.DrawTexture(helpRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUIStyle helpStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
                fontSize = 15, fontStyle = FontStyle.Bold, wordWrap = true };
            helpStyle.normal.textColor = Color.black;
            GUI.Label(helpRect, RewardDisplayName(activeAncestor, hoveredNode) + "\n" +
                RewardTooltip(activeAncestor, desktopMode), helpStyle);
        }
        if (guiDragIndex >= 0 && !guiDragTriggered)
        {
            GUIStyle dragStyle = new GUIStyle(glyphStyle) { fontSize = 38 };
            dragStyle.normal.textColor = new Color(1f, .78f, .86f, .9f);
            GUI.Label(new Rect(guiEvent.mousePosition.x - 28f, guiEvent.mousePosition.y - 28f, 56f, 56f),
                "◆", dragStyle);
        }
        GUIStyle footerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter,
            fontSize = 14 };
        GUI.Label(new Rect(left + 22f, Screen.height - 90f, width - 44f, 32f),
            "Drag an unlocked icon to exchange  ·  F close", footerStyle);
        GUI.color = previous;
    }

    private static void DrawGuiBranch(Vector2 from, Vector2 to)
    {
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        Vector2 delta = to - from;
        GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, from);
        GUI.color = new Color(.84f, .76f, .57f, .85f);
        GUI.DrawTexture(new Rect(from.x, from.y - 1f, delta.magnitude, 2f), Texture2D.whiteTexture);
        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }

    private void UpdateDesktopHover()
    {
        int current = -1;
        Mouse mouse = Mouse.current;
        if (IsMenuOpen && mouse != null && Cursor.lockState == CursorLockMode.None)
        {
            Vector2 pointer = mouse.position.ReadValue();
            Vector2 guiPoint = new Vector2(pointer.x, Screen.height - pointer.y);
            float width = Mathf.Clamp(Screen.width * .36f, 340f, 560f);
            float left = Screen.width - width;
            float treeTop = 108f + 88f;
            float treeHeight = Mathf.Max(360f, Screen.height - 108f - 210f);
            float[] px = { .50f, .34f, .50f, .66f, .50f };
            float[] py = { .88f, .70f, .52f, .34f, .16f };
            for (int i = 0; i < 5; i++)
            {
                Rect icon = new Rect(left + width * px[i] - 32f,
                    treeTop + treeHeight * py[i] - 32f, 64f, 64f);
                if (icon.Contains(guiPoint)) { current = i; break; }
            }
        }
        if (current == hoveredNode) return;
        hoveredNode = current;
        hoveredSince = Time.unscaledTime;
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

    public void DesktopToggleNearest()
    {
        if (!desktopMode) return;
        ToggleNearest();
    }

    public void DesktopSelect(int index)
    {
        if (!desktopMode || !IsMenuOpen || index < 0 || index >= 5) return;
        selected = index;
        status?.Show(SpellNames[activeAncestor, index] + (obtained[activeAncestor, index] ?
            " already exchanged." : " selected. Drag its icon outward to exchange."), 4f);
        RefreshNodes();
    }

    public void DesktopObtainSelected()
    {
        if (!desktopMode || !IsMenuOpen || selected < 0) return;
        Obtain(selected);
        selected = -1;
    }

    private void ToggleNearest()
    {
        if (IsMenuOpen) { CloseMenu(); nextToggle = Time.time + .2f; return; }
        int index = NearestAncestor();
        if (index < 0) return;
        OpenMenu(index);
    }

    private void OpenMenu(int index)
    {
        if (index < 0 || !IsNear(index) || Time.time < nextToggle) return;
        legacyTree?.CloseForMagic();
        activeAncestor = index;
        hoveredNode = focusedBefore = -1;
        hoveredSince = focusedSince = Time.unscaledTime;
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < .01f) forward = Vector3.forward;
        menuRoot.transform.position = head.position + forward * 1.65f + head.right * .72f;
        // World-space TMP faces its local -Z side. Point +Z away from the
        // viewer so the text and node order are readable instead of mirrored.
        menuRoot.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        title.text = "ANCESTOR MAGIC";
        menuRoot.SetActive(true);
        RefreshNodes();
        status?.Show(desktopMode ? "Drag an unlocked icon outward to exchange  •  F closes" :
            "Hold an icon or pull it outward with the right trigger. X closes.", 5f);
        SkyQuestInput.Pulse(SkyQuestInput.Left, .24f, .06f);
        nextToggle = Time.time + .25f;
    }

    public void CloseMenu()
    {
        if (menuRoot != null) menuRoot.SetActive(false);
        if (pointer != null) pointer.enabled = false;
        activeAncestor = -1;
        selected = focused = -1;
        guiDragIndex = -1;
        guiDragTriggered = false;
        hoveredNode = focusedBefore = -1;
    }

    private Vector3 PlayerPosition => avatar != null ? avatar.position + Vector3.up * 1.2f :
        (head != null ? head.position : transform.position + Vector3.up * 1.2f);

    private bool IsNear(int index)
    {
        if (index < 0 || ancestors == null || index >= ancestors.Length || ancestors[index] == null)
            return false;
        Transform center = memorials != null && index < memorials.Length && memorials[index] != null ?
            memorials[index] : ancestors[index];
        return Vector3.ProjectOnPlane(PlayerPosition - center.position, Vector3.up).magnitude <= proximity;
    }

    public bool IsNearTarget(Transform target)
    {
        for (int i = 0; i < ancestors.Length; i++)
            if (ancestors[i] == target) return IsNear(i);
        return false;
    }

    private int NearestAncestor()
    {
        int nearest = -1;
        float best = float.PositiveInfinity;
        for (int i = 0; i < ancestors.Length; i++)
        {
            if (!IsNear(i)) continue;
            float distance = Vector3.Distance(PlayerPosition, ancestors[i].position);
            if (distance < best) { best = distance; nearest = i; }
        }
        return nearest;
    }

    private void MakeMenu()
    {
        menuRoot = new GameObject("Magic exchange tree");
        menuRoot.transform.SetParent(transform, false);
        menuRoot.transform.localScale = Vector3.one * .8f;
        title = SkyQuestVisuals.Text("Magic tree title", "ANCESTOR MAGIC", menuRoot.transform,
            new Vector3(0f, 1.27f, -.05f), 5f, new Color(1f, .88f, .58f), 4f);
        title.textWrappingMode = TextWrappingModes.NoWrap;
        title.overflowMode = TextOverflowModes.Overflow;
        if (desktopMode) title.enabled = false;
        nodes = new Transform[5];
        labels = new TextMeshPro[5];
        iconGlyphs = new TextMeshPro[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject node = SkyQuestVisuals.Primitive("Magic " + (i + 1), PrimitiveType.Sphere,
                menuRoot.transform, new Vector3(NodePositions[i].x, NodePositions[i].y, -.08f),
                Vector3.one * .19f, new Color(1f, .55f, .67f), true);
            node.AddComponent<SkyQuestMagicNode>().index = i;
            node.GetComponent<Renderer>().enabled = false;
            TextMeshPro glyph = SkyQuestVisuals.Text("Magic icon glyph " + (i + 1), NodeGlyphs[i],
                node.transform, new Vector3(0f, 0f, -.16f), 5.2f, Color.white, 1.3f);
            glyph.fontStyle = FontStyles.Bold;
            glyph.textWrappingMode = TextWrappingModes.NoWrap;
            iconGlyphs[i] = glyph;
            nodes[i] = node.transform;
            labels[i] = SkyQuestVisuals.Text("Magic name " + (i + 1), "", menuRoot.transform,
                new Vector3(NodePositions[i].x, NodePositions[i].y - .22f, -.13f),
                3.2f, Color.white, 3.3f);
            labels[i].textWrappingMode = TextWrappingModes.NoWrap;
            labels[i].overflowMode = TextOverflowModes.Overflow;
            if (desktopMode) labels[i].enabled = false;
            if (i > 0) MakeBranch(i - 1, i);
        }
        TextMeshPro instructions = SkyQuestVisuals.Text("Magic instructions", desktopMode ?
            "Drag icon outward: exchange     F: close" :
            "Right trigger hold or pull icon outward: exchange     X: close", menuRoot.transform,
            new Vector3(0f, -1.29f, -.1f), 3.1f, Color.white, 4f);
        instructions.textWrappingMode = TextWrappingModes.NoWrap;
        instructions.overflowMode = TextOverflowModes.Overflow;
        if (desktopMode) instructions.enabled = false;
        hoverHelp = SkyQuestVisuals.Text("Magic reward hint", "", menuRoot.transform,
            new Vector3(0f, -1.08f, -.3f), 2.7f, Color.black, 4.8f);
        hoverHelp.textWrappingMode = TextWrappingModes.Normal;
        hoverHelp.rectTransform.sizeDelta = new Vector2(4.8f, 1.7f);
        hoverHelpPlate = SkyQuestVisuals.Primitive("Magic hint yellow plate", PrimitiveType.Cube,
            menuRoot.transform, new Vector3(0f, -1.08f, -.27f),
            new Vector3(.69f, .29f, .012f), new Color(1f, .9f, .12f), true);
        SkyQuestVisuals.RemoveCollider(hoverHelpPlate);
        hoverHelpPlate.SetActive(false);
        if (desktopMode) hoverHelp.enabled = false;
        if (desktopMode)
            foreach (Renderer renderer in menuRoot.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = false;
        menuRoot.SetActive(false);
    }

    private void MakeBranch(int parent, int child)
    {
        Vector3 a = new Vector3(NodePositions[parent].x, NodePositions[parent].y, -.06f);
        Vector3 b = new Vector3(NodePositions[child].x, NodePositions[child].y, -.06f);
        GameObject line = SkyQuestVisuals.Primitive("Magic branch", PrimitiveType.Cube,
            menuRoot.transform, (a + b) * .5f, new Vector3(.018f, Vector3.Distance(a, b), .012f),
            new Color(.85f, .82f, .7f), true);
        line.transform.up = (b - a).normalized;
        SkyQuestVisuals.RemoveCollider(line);
    }

    private void MakePointer()
    {
        GameObject obj = new GameObject("Magic controller pointer", typeof(LineRenderer));
        obj.transform.SetParent(transform, false);
        pointer = obj.GetComponent<LineRenderer>();
        pointer.positionCount = 2;
        pointer.startWidth = .012f;
        pointer.endWidth = .004f;
        pointer.material = SkyQuestVisuals.Material(new Color(.65f, .85f, 1f), true);
        pointer.enabled = false;
    }

    private void UpdatePointer()
    {
        focused = -1;
        float best = float.PositiveInfinity;
        Vector3 end = rightHand.position + rightHand.forward * 4f;
        foreach (RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 5f))
        {
            SkyQuestMagicNode node = hit.collider.GetComponent<SkyQuestMagicNode>();
            if (node == null || hit.distance >= best) continue;
            best = hit.distance;
            focused = node.index;
            end = hit.point;
        }
        pointer.enabled = true;
        pointer.SetPosition(0, rightHand.position);
        pointer.SetPosition(1, end);
    }

    private void ToggleHeldCube()
    {
        if (heldCube != null) { heldCube.TogglePickup(rightHand); if (!heldCube.held) heldCube = null; return; }
        SkyQuestMagicPickup[] cubes = FindObjectsByType<SkyQuestMagicPickup>(FindObjectsSortMode.None);
        float best = 2.4f;
        foreach (SkyQuestMagicPickup cube in cubes)
        {
            if (cube == null || cube.held) continue;
            float distance = Vector3.Distance(cube.transform.position, head.position);
            if (distance <= best) { best = distance; heldCube = cube; }
        }
        if (heldCube != null) heldCube.SetHeld(rightHand);
    }

    private void RefreshNodes()
    {
        if (!IsMenuOpen || activeAncestor < 0) return;
        for (int i = 0; i < 5; i++)
        {
            bool claimed = obtained[activeAncestor, i];
            nodes[i].GetComponent<Renderer>().material.color = claimed ?
                new Color(.48f, 1f, .78f) : selected == i || focused == i ?
                new Color(1f, .87f, .48f) : new Color(.8f, .88f, 1f);
            iconGlyphs[i].text = IsEnergyReward(activeAncestor) ? "◆" : NodeGlyphs[i];
            iconGlyphs[i].color = IsEnergyReward(activeAncestor) ?
                CubeColors[activeAncestor * 5 + i] : Color.white;
            labels[i].text = RewardDisplayName(activeAncestor, i) + (claimed ? "  Done" : "");
        }
    }

    private void Obtain(int index)
    {
        if (activeAncestor < 0 || index < 0 || index >= 5) return;
        if (obtained[activeAncestor, index])
        {
            status?.Show(SpellNames[activeAncestor, index] + " already exchanged.");
            return;
        }
        if (index > 0 && !obtained[activeAncestor, index - 1])
        { status?.Show("Unlock the previous Magic first."); return; }
        if (SkyQuestCandleWallet.Balance < candleCosts[index])
        { status?.Show("Not enough candles. Need " + candleCosts[index] + "."); return; }
        SkyQuestCandleWallet.TrySpend(candleCosts[index]);
        obtained[activeAncestor, index] = true;
        int paletteIndex = activeAncestor * 5 + index;
        bool energyReward = IsEnergyReward(activeAncestor);
        bool placeable = !energyReward && IsPlaceableReward(activeAncestor, index);
        GameObject reward = CreateRewardItem(RewardDisplayName(activeAncestor, index), paletteIndex, placeable, energyReward);
        reward.transform.position = rightHand != null ? rightHand.position : head.position + head.forward * .65f;
        reward.transform.rotation = rightHand != null ? rightHand.rotation : Quaternion.identity;
        SkyQuestMagicPickup pickup = reward.AddComponent<SkyQuestMagicPickup>();
        pickup.desktopHead = head;
        pickup.desktopHand = rightHand;
        pickup.desktopAvatar = avatar;
        pickup.desktopMode = desktopMode;
        pickup.placeable = placeable;
        if (placeable && RewardDisplayName(activeAncestor, index).Contains("Table"))
            reward.AddComponent<SkyQuestTeaTable>();
        pickup.restoresCapeEnergy = energyReward;
        pickup.SetHeld(rightHand);
        heldCube = pickup;
        status?.Show(SpellNames[activeAncestor, index] + (desktopMode ?
            (placeable ? " exchanged — held in right hand. Hold Q to place; Z drops/picks up." :
                " exchanged — held in right hand. Z drops/picks up; hold Q to choose a launch angle.") :
            " exchanged. Point and hold right trigger to aim; release to throw. Hold near face to eat."), 6f);
        status?.PlayConfirm();
        SkyQuestInput.Pulse(SkyQuestInput.Right, .5f, .12f);
        RefreshNodes();
    }

    private static bool IsPlaceableReward(int ancestorIndex, int itemIndex)
    {
        string name = DisplayNames[ancestorIndex, itemIndex];
        return name.Contains("Table") || name.Contains("Bonfire");
    }

    private static bool IsEnergyReward(int ancestorIndex) => ancestorIndex == 2 || ancestorIndex == 5;

    private static string RewardTooltip(int ancestorIndex, bool desktop) => desktop ?
        (IsEnergyReward(ancestorIndex) ? "E: EAT +1 FLY ENERGY" :
            "E: EAT +5 CANDLES  ·  Z: PICK UP/DROP  ·  Q: AIM") :
        (IsEnergyReward(ancestorIndex) ? "+1 FLY ENERGY" : "+5 CANDLES") +
        "\nPOINT + RIGHT TRIGGER: GRAB; HOLD TO AIM; RELEASE: THROW\nNEAR FACE + RIGHT TRIGGER: EAT";

    private static string RewardDisplayName(int ancestorIndex, int itemIndex) =>
        IsEnergyReward(ancestorIndex) ? "Cape Energy " + (itemIndex + 1) :
        DisplayNames[ancestorIndex, itemIndex];

    private GameObject CreateRewardItem(string rewardName, int paletteIndex, bool placeable, bool energyReward)
    {
        GameObject root = new GameObject(rewardName + " reward");
        Color main = CubeColors[Mathf.Clamp(paletteIndex, 0, CubeColors.Length - 1)];
        Color light = Color.Lerp(main, Color.white, .48f);
        Color dark = Color.Lerp(main, new Color(.14f, .1f, .18f), .38f);

        // All tree rewards use the supplied A242 pottery meshes. The reward's
        // existing ability and exchange cost are still determined above.
        int ceramicIndex = paletteIndex % 8;
        if (ceramicIndex >= 0 &&
            ceramicRewardPrefabs != null && ceramicIndex < ceramicRewardPrefabs.Length &&
            ceramicRewardPrefabs[ceramicIndex] != null)
        {
            GameObject ceramic = Instantiate(ceramicRewardPrefabs[ceramicIndex], root.transform, false);
            ceramic.name = rewardName + " A242 ceramic";
        }
        else if (energyReward)
        {
            Part(root.transform, "Cape energy cube", PrimitiveType.Cube,
                new Vector3(0f, .3f, 0f), new Vector3(.45f, .45f, .45f), main, true);
            Part(root.transform, "Cape energy core", PrimitiveType.Cube,
                new Vector3(0f, .3f, -.226f), new Vector3(.2f, .2f, .025f), light, true);
        }
        else if (placeable && rewardName.Contains("Table")) MakeTable(root.transform, main, light, dark);
        else if (placeable && rewardName.Contains("Bonfire")) MakeBonfire(root.transform, main, light, dark);
        else
        {
            switch (paletteIndex % 6)
            {
                case 0: MakeBottle(root.transform, main, light, dark); break;
                case 1: MakeJar(root.transform, main, light, dark); break;
                case 2: MakeBouquet(root.transform, main, light, dark); break;
                case 3: MakeStar(root.transform, main, light); break;
                case 4: MakeLamp(root.transform, main, light, dark); break;
                default: MakeTorch(root.transform, main, light, dark); break;
            }
        }
        return root;
    }

    private static GameObject Part(Transform parent, string name, PrimitiveType type,
        Vector3 position, Vector3 scale, Color color, bool glow = false)
    {
        GameObject part = SkyQuestVisuals.Primitive(name, type, parent, position, scale, color, glow);
        // Reward decoration must not briefly collide with the player while
        // Destroy() defers removing its primitive collider until frame end.
        Collider partCollider = part.GetComponent<Collider>();
        if (partCollider != null) partCollider.enabled = false;
        SkyQuestVisuals.RemoveCollider(part);
        return part;
    }

    private static void MakeBottle(Transform root, Color main, Color light, Color dark)
    {
        Part(root, "Potion bottle", PrimitiveType.Cylinder, new Vector3(0f, .22f, 0f), new Vector3(.18f, .22f, .18f), main, true);
        Part(root, "Bottle shoulder", PrimitiveType.Sphere, new Vector3(0f, .43f, 0f), new Vector3(.19f, .12f, .19f), light, true);
        Part(root, "Bottle neck", PrimitiveType.Cylinder, new Vector3(0f, .55f, 0f), new Vector3(.075f, .09f, .075f), light, true);
        Part(root, "Bottle stopper", PrimitiveType.Cylinder, new Vector3(0f, .66f, 0f), new Vector3(.1f, .035f, .1f), dark);
    }

    private static void MakeJar(Transform root, Color main, Color light, Color dark)
    {
        Part(root, "Jar body", PrimitiveType.Sphere, new Vector3(0f, .25f, 0f), new Vector3(.25f, .28f, .25f), main, true);
        Part(root, "Jar rim", PrimitiveType.Cylinder, new Vector3(0f, .5f, 0f), new Vector3(.17f, .045f, .17f), light, true);
        Part(root, "Jar lid", PrimitiveType.Cylinder, new Vector3(0f, .57f, 0f), new Vector3(.2f, .035f, .2f), dark);
    }

    private static void MakeBouquet(Transform root, Color main, Color light, Color dark)
    {
        for (int i = 0; i < 5; i++)
        {
            float x = (i - 2) * .075f;
            GameObject stem = Part(root, "Flower stem", PrimitiveType.Cylinder, new Vector3(x * .45f, .24f, 0f), new Vector3(.018f, .22f, .018f), new Color(.35f, .7f, .38f));
            stem.transform.localRotation = Quaternion.Euler(0f, 0f, -(i - 2) * 7f);
            Part(root, "Flower", PrimitiveType.Sphere, new Vector3(x, .51f + Mathf.Abs(i - 2) * .025f, 0f), new Vector3(.1f, .075f, .1f), i % 2 == 0 ? main : light, true);
        }
        Part(root, "Bouquet wrap", PrimitiveType.Cylinder, new Vector3(0f, .13f, 0f), new Vector3(.11f, .14f, .11f), dark);
    }

    private static void MakeStar(Transform root, Color main, Color light)
    {
        Part(root, "Star core", PrimitiveType.Sphere, new Vector3(0f, .3f, 0f), new Vector3(.18f, .18f, .08f), light, true);
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72f;
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
            GameObject ray = Part(root, "Star ray", PrimitiveType.Cube,
                new Vector3(direction.x * .22f, .3f + direction.y * .22f, 0f), new Vector3(.09f, .25f, .07f), main, true);
            ray.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }

    private static void MakeLamp(Transform root, Color main, Color light, Color dark)
    {
        Part(root, "Lamp base", PrimitiveType.Cylinder, new Vector3(0f, .08f, 0f), new Vector3(.2f, .06f, .2f), dark);
        Part(root, "Lamp stem", PrimitiveType.Cylinder, new Vector3(0f, .29f, 0f), new Vector3(.035f, .2f, .035f), main, true);
        Part(root, "Lamp shade", PrimitiveType.Cylinder, new Vector3(0f, .54f, 0f), new Vector3(.24f, .12f, .24f), main, true);
        Part(root, "Lamp light", PrimitiveType.Sphere, new Vector3(0f, .49f, 0f), new Vector3(.12f, .12f, .12f), light, true);
    }

    private static void MakeTorch(Transform root, Color main, Color light, Color dark)
    {
        Part(root, "Torch handle", PrimitiveType.Cylinder, new Vector3(0f, .24f, 0f), new Vector3(.045f, .25f, .045f), dark);
        Part(root, "Torch cup", PrimitiveType.Cylinder, new Vector3(0f, .5f, 0f), new Vector3(.13f, .08f, .13f), main);
        Part(root, "Torch flame", PrimitiveType.Sphere, new Vector3(0f, .66f, 0f), new Vector3(.13f, .2f, .13f), light, true);
        Part(root, "Torch flame tip", PrimitiveType.Sphere, new Vector3(.025f, .82f, 0f), new Vector3(.07f, .14f, .07f), Color.white, true);
    }

    private static void MakeTable(Transform root, Color main, Color light, Color dark)
    {
        Part(root, "Table top", PrimitiveType.Cylinder, new Vector3(0f, .38f, 0f), new Vector3(.34f, .045f, .34f), main, true);
        for (int i = 0; i < 4; i++)
        {
            float x = (i % 2 == 0 ? -1f : 1f) * .23f;
            float z = (i < 2 ? -1f : 1f) * .23f;
            Part(root, "Table leg", PrimitiveType.Cylinder, new Vector3(x, .18f, z), new Vector3(.035f, .18f, .035f), dark);
        }
        Part(root, "Tea light", PrimitiveType.Sphere, new Vector3(0f, .49f, 0f), new Vector3(.1f, .08f, .1f), light, true);
    }

    private static void MakeBonfire(Transform root, Color main, Color light, Color dark)
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject log = Part(root, "Bonfire log", PrimitiveType.Cylinder, new Vector3(0f, .11f, 0f), new Vector3(.065f, .3f, .065f), dark);
            log.transform.localRotation = Quaternion.Euler(90f, i * 60f, 0f);
        }
        Part(root, "Bonfire glow", PrimitiveType.Sphere, new Vector3(0f, .32f, 0f), new Vector3(.2f, .3f, .2f), main, true);
        Part(root, "Bonfire flame", PrimitiveType.Sphere, new Vector3(.03f, .55f, 0f), new Vector3(.11f, .24f, .11f), light, true);
    }
}

public class SkyQuestMagicNode : MonoBehaviour { public int index; }

public class SkyQuestGalleryIcon : MonoBehaviour { public int index; }

public class SkyQuestMagicPrompt : MonoBehaviour
{
    public SkyQuestMagicExchange owner;
    public Transform target;
    private TextMeshPro label;
    void Start() { label = GetComponent<TextMeshPro>(); }
    void LateUpdate()
    {
        if (owner == null || label == null || owner.head == null) return;
        label.enabled = owner.IsNearTarget(target) && !owner.IsMenuOpen;
        if (label.enabled) transform.rotation = Quaternion.LookRotation(transform.position - owner.head.position);
    }
}
