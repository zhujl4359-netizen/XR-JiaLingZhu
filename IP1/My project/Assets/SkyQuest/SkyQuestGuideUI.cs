using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// English controls drawer and the flight-course rules sign for both desktop and Quest.
/// All help panels are visual only: no HUD collider is allowed to affect player movement.
/// </summary>
public sealed class SkyQuestGuideUI : MonoBehaviour
{
    public Transform head;
    public bool desktopMode;

    private static readonly string[,] QuestControls =
    {
        { "LEFT STICK", "Move / steer while flying" },
        { "LEFT STICK CLICK", "Open / close the chat keyboard" },
        { "X", "Open / close a nearby ancestor tree" },
        { "Y", "Preview / cancel a tea table" },
        { "LEFT TRIGGER TAP", "Drop a held item / light a candle" },
        { "LEFT TRIGGER HOLD", "English voice message" },
        { "LEFT GRIP + B", "Toggle comfort teleport" },
        { "RIGHT STICK", "Turn smoothly" },
        { "RIGHT STICK CLICK", "Slow down into a safe glide" },
        { "A", "Take off / boost (uses 1 cape energy)" },
        { "B", "Switch first / third person" },
        { "RIGHT TRIGGER", "Hold on any loose item to aim; release to throw" },
        { "RIGHT TRIGGER ON TABLE", "Open the tea table keyboard" },
        { "RIGHT GRIP", "Pick up / drop pottery or exchanged items" },
        { "RIGHT TRIGGER NEAR FACE", "Eat a held reward (+energy or candles)" },
        { "BOTH STICKS CLICK", "Calibrate seated height" },
        { "BOTH GRIPS OVERHEAD", "Open the original ancestor tree" },
        { "META BUTTON", "Open the headset system menu" }
    };

    private static readonly string[,] DesktopControls =
    {
        { "W A S D", "Move / steer while flying" },
        { "MOUSE", "Look around" },
        { "SPACE", "Take off / boost (uses 1 cape energy)" },
        { "LEFT CTRL", "Descend while flying" },
        { "1 / 3", "First / third person" },
        { "F", "Open a nearby ancestor tree" },
        { "CLICK + HOLD", "Exchange a selected tree item" },
        { "E", "Eat held item / equip ancestor item" },
        { "Z", "Pick up / drop exchanged item" },
        { "HOLD Q", "Aim and throw held item" },
        { "P / R", "Preview / place tea table" },
        { "CLICK TABLE", "Open the tea table keyboard" },
        { "T / K", "Open chat" },
        { "ARROW KEYS", "Turn / tilt camera" },
        { "F1", "Show / hide this guide" }
    };

    private static readonly string[,] QuestFlightControls =
    {
        { "BLUE RING", "Enter to start or replay; cape refills" },
        { "LEFT STICK", "Steer through the floating flames" },
        { "RIGHT STICK", "Turn smoothly" },
        { "A", "Take off / boost; costs 1 energy" },
        { "RIGHT STICK CLICK", "Slow down into a safe glide" },
        { "FLAMES", "Fly through each one; no trigger needed" },
        { "NEXT HINT", "Follow the distance and direction on HUD" }
    };

    private static readonly string[,] DesktopFlightControls =
    {
        { "BLUE RING", "Enter to start or replay; cape refills" },
        { "W A S D", "Steer through the floating flames" },
        { "MOUSE", "Look around" },
        { "SPACE", "Take off / boost; costs 1 energy" },
        { "LEFT CTRL", "Descend" },
        { "FLAMES", "Fly through each one; no click needed" },
        { "NEXT HINT", "Follow the distance and direction on HUD" }
    };

    private SkyQuestLocomotion player;
    private SkyQuestFlightFlameGame course;
    private Transform signBoard;
    private bool guideOpen = true;
    private bool flightOpen;
    private bool guideBeforeFlight;
    private bool previousRightTrigger;
    private bool lastThirdPerson;
    private RectTransform guidePanel;
    private RectTransform guideClose;
    private RectTransform guideTab;
    private RectTransform flightPanel;
    private RectTransform flightClose;
    private TextMeshProUGUI tabText;
    private GUIStyle headingStyle;
    private GUIStyle cellStyle;
    private GUIStyle keyStyle;
    private GUIStyle buttonStyle;
    private Vector2 guideScroll;
    private Vector2 flightScroll;

    private void Start()
    {
        player = GetComponent<SkyQuestLocomotion>();
        if (head == null && player != null) head = player.head;
        desktopMode |= player != null && player.desktopControlled;
        if (!desktopMode && head != null) BuildQuestHud();
        lastThirdPerson = player != null && player.ThirdPerson;
        TryBuildSign();
    }

    private void Update()
    {
        if (signBoard == null) TryBuildSign();
        if (desktopMode)
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
                guideOpen = !guideOpen;
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && signBoard != null)
            {
                Camera camera = Camera.main;
                if (camera != null)
                {
                    Vector2 mousePoint = Cursor.lockState == CursorLockMode.Locked ?
                        new Vector2(Screen.width * .5f, Screen.height * .5f) : mouse.position.ReadValue();
                    if (!IsOverDesktopPanel(mousePoint) && HitSign(camera.ScreenPointToRay(mousePoint), 9f))
                        OpenFlightRules();
                }
            }
            return;
        }

        bool thirdPerson = player != null && player.ThirdPerson;
        if (thirdPerson != lastThirdPerson)
        {
            lastThirdPerson = thirdPerson;
            RefreshQuestHud();
        }
        if (thirdPerson)
        {
            previousRightTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f;
            return;
        }

        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f;
        if (trigger && !previousRightTrigger && player != null && player.rightHand != null)
        {
            Ray ray = new Ray(player.rightHand.position, player.rightHand.forward);
            if (flightOpen && HitUi(ray, flightClose, 3f)) CloseFlightRules();
            else if (!flightOpen && guideOpen && HitUi(ray, guideClose, 3f, 12f)) ToggleGuide();
            else if (!flightOpen && HitUi(ray, guideTab, 3f, 22f)) ToggleGuide();
            else if (signBoard != null && HitSign(ray, 5f)) OpenFlightRules();
        }
        previousRightTrigger = trigger;
    }

    private void TryBuildSign()
    {
        if (course == null) course = FindFirstObjectByType<SkyQuestFlightFlameGame>();
        if (course == null || course.startMarker == null) return;
        Transform existing = course.startMarker.Find("Flight rules sign");
        if (existing != null)
        {
            signBoard = existing.Find("Sign board");
            return;
        }

        // Stand beside the blue start ring, outside its activation radius and flight path.
        Transform root = new GameObject("Flight rules sign").transform;
        root.SetParent(course.startMarker, false);
        root.localPosition = new Vector3(-2.55f, -.08f, -.65f);
        root.localRotation = Quaternion.identity;
        Part("Sign post", root, new Vector3(0f, .75f, 0f),
            new Vector3(.12f, 1.5f, .12f), new Color(.27f, .32f, .36f));
        GameObject board = Part("Sign board", root, new Vector3(0f, 1.68f, 0f),
            new Vector3(1.36f, .78f, .09f), new Color(.075f, .14f, .19f));
        signBoard = board.transform;
        Color trim = new Color(.84f, .72f, .43f);
        Part("Top trim", root, new Vector3(0f, 2.09f, 0f), new Vector3(1.48f, .06f, .13f), trim);
        Part("Bottom trim", root, new Vector3(0f, 1.27f, 0f), new Vector3(1.48f, .06f, .13f), trim);
        Part("Left trim", root, new Vector3(-.71f, 1.68f, 0f), new Vector3(.06f, .82f, .13f), trim);
        Part("Right trim", root, new Vector3(.71f, 1.68f, 0f), new Vector3(.06f, .82f, .13f), trim);
        string label = "FLIGHT FLAMES\nRULES: " + (desktopMode ? "LEFT CLICK" : "RIGHT TRIGGER");
        SignText("Front rules", label, root, new Vector3(0f, 1.68f, .057f), Quaternion.identity);
        SignText("Back rules", label, root, new Vector3(0f, 1.68f, -.057f), Quaternion.Euler(0f, 180f, 0f));
    }

    private static GameObject Part(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        GameObject part = SkyQuestVisuals.Primitive(name, PrimitiveType.Cube, parent, position, scale, color);
        SkyQuestVisuals.RemoveCollider(part);
        return part;
    }

    private static void SignText(string name, string value, Transform parent, Vector3 position, Quaternion rotation)
    {
        TextMeshPro label = SkyQuestVisuals.Text(name, value, parent, position, 2.25f, Color.white, 10f);
        label.transform.localRotation = rotation;
        label.rectTransform.sizeDelta = new Vector2(10f, 4.5f);
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
    }

    // Ray/plane checks give the headset UI and sign clickable areas without physics colliders.
    private bool HitSign(Ray ray, float maxDistance)
    {
        if (signBoard == null) return false;
        if (!new Plane(signBoard.forward, signBoard.position).Raycast(ray, out float distance) ||
            distance < 0f || distance > maxDistance) return false;
        Vector3 local = signBoard.InverseTransformPoint(ray.GetPoint(distance));
        return Mathf.Abs(local.x) <= .5f && Mathf.Abs(local.y) <= .5f;
    }

    private static bool HitUi(Ray ray, RectTransform rect, float maxDistance,
        float padding = 0f)
    {
        if (rect == null || !rect.gameObject.activeInHierarchy) return false;
        if (!new Plane(rect.forward, rect.position).Raycast(ray, out float distance) ||
            distance < 0f || distance > maxDistance) return false;
        Vector3 local = rect.InverseTransformPoint(ray.GetPoint(distance));
        return Rect.MinMaxRect(rect.rect.xMin - padding, rect.rect.yMin - padding,
            rect.rect.xMax + padding, rect.rect.yMax + padding).Contains(
                new Vector2(local.x, local.y));
    }

    private void ToggleGuide()
    {
        guideOpen = !guideOpen;
        RefreshQuestHud();
        SkyQuestInput.Pulse(SkyQuestInput.Right, .22f, .05f);
    }

    private void OpenFlightRules()
    {
        if (flightOpen) return;
        guideBeforeFlight = guideOpen;
        flightOpen = true;
        guideOpen = false;
        RefreshQuestHud();
    }

    private void CloseFlightRules()
    {
        flightOpen = false;
        guideOpen = guideBeforeFlight;
        RefreshQuestHud();
    }

    private void RefreshQuestHud()
    {
        bool visible = desktopMode || player == null || !player.ThirdPerson;
        if (guidePanel != null) guidePanel.gameObject.SetActive(visible && guideOpen && !flightOpen);
        if (guideTab != null) guideTab.gameObject.SetActive(visible && !flightOpen);
        if (flightPanel != null) flightPanel.gameObject.SetActive(visible && flightOpen);
        if (tabText != null) tabText.text = guideOpen ? "CLOSE\n◀" : "OPEN\n▶";
    }

    private void BuildQuestHud()
    {
        GameObject root = new GameObject("Quest help HUD", typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(head, false);
        root.transform.localPosition = new Vector3(0f, .07f, 1.25f);
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one * .001f;
        RectTransform canvasRect = (RectTransform)root.transform;
        canvasRect.sizeDelta = new Vector2(1200f, 800f);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 25;

        guidePanel = Panel("Controls drawer", canvasRect, new Vector2(-310f, -5f),
            new Vector2(510f, 610f), .80f);
        Label("Controls title", "QUEST 3S CONTROLS", guidePanel,
            new Vector2(0f, 275f), new Vector2(450f, 39f), 25f, TextAlignmentOptions.Center, true);
        guideClose = Panel("Close controls menu", guidePanel, new Vector2(226f, 273f),
            new Vector2(50f, 48f), .55f);
        Label("Close controls icon", "×", guideClose, Vector2.zero,
            new Vector2(46f, 44f), 28f, TextAlignmentOptions.Center, true);
        BuildTable(guidePanel, QuestControls, 204f, 31f, 470f, 17f);
        Label("Controls hint", "Aim at X + RIGHT TRIGGER to close. Voice uses offline dictation.", guidePanel,
            new Vector2(0f, -285f), new Vector2(470f, 32f), 14f, TextAlignmentOptions.Center);
        guideTab = Panel("Right-trigger guide toggle", canvasRect,
            new Vector2(-545f, 160f), new Vector2(106f, 144f), .94f);
        tabText = Label("Guide toggle text", "CLOSE\n◀", guideTab, Vector2.zero,
            new Vector2(98f, 130f), 23f, TextAlignmentOptions.Center, true);

        flightPanel = Panel("Flight rules panel", canvasRect, Vector2.zero,
            new Vector2(570f, 615f), .84f);
        Label("Flight title", "FLIGHT FLAMES  |  HOW TO PLAY", flightPanel,
            new Vector2(-17f, 265f), new Vector2(480f, 42f), 23f, TextAlignmentOptions.Center, true);
        flightClose = Panel("Close flight rules", flightPanel, new Vector2(257f, 269f),
            new Vector2(52f, 48f), .35f);
        Label("Close text", "X", flightClose, Vector2.zero,
            new Vector2(48f, 45f), 24f, TextAlignmentOptions.Center, true);
        BuildFlightPanelText(flightPanel, QuestFlightControls);
        RefreshQuestHud();
    }

    private static RectTransform Panel(string name, RectTransform parent, Vector2 position,
        Vector2 size, float alpha)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = (RectTransform)obj.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = obj.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, alpha);
        image.raycastTarget = false;
        return rect;
    }

    private static TextMeshProUGUI Label(string name, string value, RectTransform parent,
        Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment, bool bold = false)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rect = (RectTransform)obj.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = fontSize;
        label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        label.color = Color.white;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(12f, fontSize - 4f);
        label.fontSizeMax = fontSize;
        label.raycastTarget = false;
        return label;
    }

    private static void BuildTable(RectTransform parent, string[,] rows,
        float firstY, float rowHeight, float width, float fontSize)
    {
        Panel("Table heading", parent, new Vector2(0f, firstY + rowHeight),
            new Vector2(width, rowHeight), .22f);
        Label("Key heading", "KEY / BUTTON", parent,
            new Vector2(-width * .34f, firstY + rowHeight),
            new Vector2(width * .30f, rowHeight), fontSize, TextAlignmentOptions.MidlineLeft, true);
        Label("Action heading", "ACTION", parent,
            new Vector2(width * .18f, firstY + rowHeight),
            new Vector2(width * .62f, rowHeight), fontSize, TextAlignmentOptions.MidlineLeft, true);
        for (int i = 0; i < rows.GetLength(0); i++)
        {
            float y = firstY - i * rowHeight;
            if (i % 2 == 0) Panel("Row " + i, parent, new Vector2(0f, y),
                new Vector2(width, rowHeight), .13f);
            Label("Key " + i, rows[i, 0], parent,
                new Vector2(-width * .34f, y), new Vector2(width * .30f, rowHeight),
                fontSize, TextAlignmentOptions.MidlineLeft, true);
            Label("Action " + i, rows[i, 1], parent,
                new Vector2(width * .18f, y), new Vector2(width * .62f, rowHeight),
                fontSize, TextAlignmentOptions.MidlineLeft);
        }
    }

    private void BuildFlightPanelText(RectTransform panel, string[,] rows)
    {
        int flameCount = course != null && course.flames != null ? course.flames.Length : 12;
        Label("Flight goal", "Enter the blue ring. Fly through all " + flameCount +
            " flames within 90 seconds. Touch each flame to collect it.", panel,
            new Vector2(0f, 209f), new Vector2(520f, 55f), 19f, TextAlignmentOptions.Center);
        BuildTable(panel, rows, 101f, 43f, 520f, 18f);
        Label("Flight score", "+10 points per flame. Finish bonus: 2 points per second left.\n" +
            "Your cape refills at the start and end of each round.", panel,
            new Vector2(0f, -249f), new Vector2(520f, 69f), 17f, TextAlignmentOptions.Center);
    }

    private void OnGUI()
    {
        if (!desktopMode) return;
        EnsureStyles();
        float width = Mathf.Min(550f, Screen.width * .31f);
        float height = Mathf.Min(800f, Screen.height * .78f);
        float top = 132f;
        if (flightOpen) DrawDesktopFlight();
        if (!guideOpen)
        {
            Rect tab = new Rect(0f, top, 102f, 39f);
            Fill(tab, .85f);
            if (GUI.Button(tab, "GUIDE  ▶", buttonStyle)) guideOpen = true;
            return;
        }
        Rect panel = new Rect(0f, top, width, height);
        Fill(panel, .80f);
        GUI.Label(new Rect(20f, top + 12f, width - 118f, 36f),
            "DESKTOP CONTROLS", headingStyle);
        if (GUI.Button(new Rect(width - 54f, top + 10f, 44f, 37f), "×", buttonStyle))
            guideOpen = false;
        DrawDesktopTable(DesktopControls, new Rect(13f, top + 58f, width - 26f, height - 70f), ref guideScroll);
    }

    private void DrawDesktopFlight()
    {
        float width = Mathf.Min(630f, Screen.width * .48f);
        float height = Mathf.Min(610f, Screen.height * .76f);
        Rect panel = new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, width, height);
        Fill(panel, .85f);
        GUI.Label(new Rect(panel.x + 20f, panel.y + 13f, width - 104f, 37f),
            "FLIGHT FLAMES  |  HOW TO PLAY", headingStyle);
        if (GUI.Button(new Rect(panel.xMax - 85f, panel.y + 12f, 70f, 36f), "CLOSE", buttonStyle))
            CloseFlightRules();
        int flameCount = course != null && course.flames != null ? course.flames.Length : 12;
        GUI.Label(new Rect(panel.x + 20f, panel.y + 56f, width - 40f, 54f),
            "Enter the blue ring. Fly through all " + flameCount +
            " flames in 90 seconds. Touch each flame to collect it.", cellStyle);
        DrawDesktopTable(DesktopFlightControls,
            new Rect(panel.x + 15f, panel.y + 116f, width - 30f, height - 188f), ref flightScroll);
        GUI.Label(new Rect(panel.x + 22f, panel.yMax - 67f, width - 44f, 56f),
            "+10 points per flame. Finish bonus: 2 points per second left.\n" +
            "Cape energy refills when the round starts or ends.", cellStyle);
    }

    private void DrawDesktopTable(string[,] rows, Rect viewport, ref Vector2 scroll)
    {
        const float rowHeight = 39f;
        Rect content = new Rect(0f, 0f, viewport.width - 18f, (rows.GetLength(0) + 1) * rowHeight);
        scroll = GUI.BeginScrollView(viewport, scroll, content);
        float keyWidth = content.width * .32f;
        for (int i = -1; i < rows.GetLength(0); i++)
        {
            float y = (i + 1) * rowHeight;
            if (i == -1 || i % 2 == 0) Fill(new Rect(0f, y, content.width, rowHeight), i == -1 ? .30f : .16f);
            GUI.Label(new Rect(9f, y + 3f, keyWidth - 12f, rowHeight - 6f),
                i == -1 ? "KEY / BUTTON" : rows[i, 0], keyStyle);
            GUI.Label(new Rect(keyWidth + 8f, y + 3f, content.width - keyWidth - 16f, rowHeight - 6f),
                i == -1 ? "ACTION" : rows[i, 1], cellStyle);
        }
        GUI.EndScrollView();
    }

    private bool IsOverDesktopPanel(Vector2 mousePoint)
    {
        // Input System coordinates start at the bottom; IMGUI coordinates start at the top.
        Vector2 guiPoint = new Vector2(mousePoint.x, Screen.height - mousePoint.y);
        float guideWidth = Mathf.Min(550f, Screen.width * .31f);
        float guideHeight = Mathf.Min(800f, Screen.height * .78f);
        if (guideOpen && new Rect(0f, 132f, guideWidth, guideHeight).Contains(guiPoint)) return true;
        if (!guideOpen && new Rect(0f, 132f, 102f, 39f).Contains(guiPoint)) return true;
        if (!flightOpen) return false;
        float width = Mathf.Min(630f, Screen.width * .48f);
        float height = Mathf.Min(610f, Screen.height * .76f);
        return new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f,
            width, height).Contains(guiPoint);
    }

    private void EnsureStyles()
    {
        if (headingStyle != null) return;
        headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 21, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft, wordWrap = true };
        headingStyle.normal.textColor = Color.white;
        cellStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleLeft,
            wordWrap = true };
        cellStyle.normal.textColor = Color.white;
        keyStyle = new GUIStyle(cellStyle) { fontStyle = FontStyle.Bold };
        keyStyle.normal.textColor = Color.white;
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 15, fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter };
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.textColor = Color.white;
        buttonStyle.normal.background = null;
        buttonStyle.hover.background = null;
        buttonStyle.active.background = null;
    }

    private static void Fill(Rect rect, float alpha)
    {
        Color before = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = before;
    }
}
