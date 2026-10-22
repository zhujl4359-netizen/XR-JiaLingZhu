using System.Collections.Generic;
using Meta.WitAi.Data.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SkyQuestCommunication : MonoBehaviour
{
    public Transform head;
    public Transform rightHand;
    public SkyQuestStatus status;
    public SkyQuestAncestor itemTree;
    public bool desktopMode;
    public float maxMessageLength = 120f;
    public Transform avatar;
    public WitConfiguration witConfiguration;

    public string CurrentDraft { get; private set; } = "";
    public readonly List<string> SentMessages = new List<string>();
    private readonly List<float> sentMessageTimes = new List<float>();
    private readonly List<bool> sentMessageOnLeft = new List<bool>();
    private const float MessageVisibleDuration = 10f;
    private const float MessageFadeDuration = 2f;
    public bool IsOpen => chatOpen || (panel != null && panel.activeSelf);

    private GameObject panel;
    private Material keyboardBackingMaterial;
    private TextMeshPro draftText;
    private TextMeshPro historyText;
    private LineRenderer ray;
    private SkyQuestKey focused;
    private bool previousToggle;
    private bool previousTrigger;
    private float nextToggle;
    private SkyQuestSpeech speech;
    private SkyQuestLocomotion locomotion;
    private bool chatOpen;
    private bool tableChatOpen;
    private bool previousTableTrigger;
    private float tableTriggerStarted;
    private SkyQuestTeaTable activeTable;
    private bool previousLeftTrigger;
    private bool leftVoiceActivated;
    private float leftTriggerStarted;
    private bool voiceMessagePending;
    private GUIStyle iconStyle;
    private GUIStyle inputStyle;
    private GUIStyle messageStyle;
    private GUIStyle leftBubbleStyle;
    private GUIStyle typingStyle;
    private GUIStyle buttonStyle;
    private GUIStyle bubbleLabelStyle;
    private GUIStyle controlsBackdropStyle;
    private GUIStyle keyboardPanelStyle;
    private GUIStyle keyboardKeyStyle;
    private GUIStyle keyboardKeySelectedStyle;
    private GUIStyle keyboardActionStyle;
    private GUIStyle inputLabelStyle;
    private static Texture2D chatBackground;
    private static Texture2D bubbleBackground;
    private static Texture2D typingBackground;
    private static Texture2D controlBackground;
    private static Texture2D keyboardBackground;
    private static Texture2D keyboardKeyBackground;
    private static Texture2D keyboardKeySelectedBackground;
    private static Texture2D keyboardActionBackground;
    private int selectedKeyboardKey;
    private char highlightedKey;
    private float highlightedKeyUntil;
    private float nextGamepadNavigation;
    private Keyboard boundKeyboard;
    private GameObject vrBubbleCanvas;
    private readonly List<TextMeshProUGUI> vrBubbleTexts = new List<TextMeshProUGUI>();
    private readonly List<RectTransform> vrBubbleRects = new List<RectTransform>();
    private readonly List<CanvasGroup> vrBubbleGroups = new List<CanvasGroup>();
    private bool IsFirstPerson => locomotion != null && !locomotion.ThirdPerson;

    void Start()
    {
        if (head == null || rightHand == null) return;
        SkyQuestVisuals.EnsureChineseFont();
        locomotion = FindFirstObjectByType<SkyQuestLocomotion>();
        if (avatar == null)
        {
            if (locomotion != null && locomotion.avatar != null) avatar = locomotion.avatar.transform;
        }
        speech = gameObject.AddComponent<SkyQuestSpeech>();
        speech.Configure(witConfiguration);
        speech.OnRecognized += OnVoiceRecognized;
        speech.OnStatusChanged += OnVoiceStatus;
        BuildPanel();
        BuildRay();
        if (!desktopMode) BuildVrBubbles();
        RefreshText();
    }

    void Update()
    {
        ExpireOldMessages();
        UpdateVrBubbles();
        if (panel == null) return;
        if (!desktopMode) UpdateLeftTriggerVoice();
        if (itemTree != null && itemTree.IsMenuOpen && IsOpen)
        {
            chatOpen = false;
            tableChatOpen = false;
            activeTable = null;
            panel.SetActive(false);
        }
        if (!desktopMode) PlacePanel();
        if (!IsOpen) UpdateTeaTableClick();
        if (desktopMode)
        {
            BindDesktopKeyboard();
            UpdateDesktopKeyboard();
            return;
        }
        bool toggle = SkyQuestInputExtensions.StickClick(SkyQuestInput.Left);
        if (toggle && !previousToggle && Time.time > nextToggle &&
            (itemTree == null || !itemTree.IsMenuOpen))
        {
            DesktopToggle();
            nextToggle = Time.time + 0.4f;
        }
        previousToggle = toggle;
        if (!panel.activeSelf)
        {
            if (ray != null) ray.enabled = false;
            previousTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
            return;
        }
        UpdatePointer();
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        if (trigger && !previousTrigger && focused != null) Press(focused.value);
        previousTrigger = trigger;
    }

    private void BuildPanel()
    {
        panel = new GameObject("Spatial communication panel");
        panel.transform.SetParent(head, false);
        panel.transform.localPosition = new Vector3(0f, -0.04f, 1.05f);
        GameObject keyboardBacking = SkyQuestVisuals.Primitive("Panel background", PrimitiveType.Cube, panel.transform,
            new Vector3(0f, 0f, 0.06f), new Vector3(desktopMode ? 0.95f : 1.3f, desktopMode ? 0.78f : 0.96f, 0.02f),
            new Color(0.12f, 0.15f, 0.22f));
        if (!desktopMode)
        {
            // The keyboard backing must actually blend in URP; an alpha-only
            // tint on the default opaque Lit material still appears solid.
            Color tint = new Color(.61f, .82f, .96f, .43f);
            keyboardBackingMaterial = new Material(keyboardBacking.GetComponent<Renderer>().sharedMaterial);
            keyboardBackingMaterial.color = tint;
            keyboardBackingMaterial.SetFloat("_Surface", 1f);
            keyboardBackingMaterial.SetFloat("_Blend", 0f);
            keyboardBackingMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            keyboardBackingMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            keyboardBackingMaterial.SetInt("_ZWrite", 0);
            keyboardBackingMaterial.SetOverrideTag("RenderType", "Transparent");
            keyboardBackingMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            keyboardBackingMaterial.renderQueue = (int)RenderQueue.Transparent;
            keyboardBacking.GetComponent<Renderer>().sharedMaterial = keyboardBackingMaterial;
        }
        SkyQuestVisuals.Text("Communication title", desktopMode ? "KEYBOARD COMMUNICATION" : "SKY COMMUNICATION", panel.transform,
            new Vector3(0f, desktopMode ? 0.32f : 0.41f, 0f), 2.8f, new Color(1f, 0.9f, 0.65f), 1.2f);
        draftText = SkyQuestVisuals.Text("Draft", "", panel.transform,
            new Vector3(0f, desktopMode ? 0.21f : 0.29f, 0f), 2.2f, Color.white, 1.12f);
        historyText = SkyQuestVisuals.Text("Last message", "", panel.transform,
            new Vector3(0f, desktopMode ? -0.29f : -0.38f, 0f), 1.8f, new Color(0.8f, 0.95f, 1f), 1.12f);
        if (desktopMode)
        {
            SkyQuestVisuals.Text("Keyboard hint", "TYPE • ENTER SENDS • ESC CLOSES",
                panel.transform, new Vector3(0f, -0.05f, 0f), 2f, Color.white, 0.85f);
            panel.SetActive(false);
            return;
        }
        string[] rows = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
        for (int row = 0; row < rows.Length; row++)
        {
            string keys = rows[row];
            float width = 0.11f;
            for (int col = 0; col < keys.Length; col++)
                AddKey(keys[col].ToString(), (col - (keys.Length - 1) * 0.5f) * width,
                    0.13f - row * 0.11f, 0.095f);
        }
        AddKey("SPACE", -0.36f, -0.23f, 0.27f);
        AddKey("DEL", -0.08f, -0.23f, 0.17f);
        AddKey("SEND", 0.20f, -0.23f, 0.19f);
        AddKey("MIC", 0.47f, -0.23f, 0.15f);
        panel.SetActive(false);
    }

    private void BuildVrBubbles()
    {
        // IMGUI is visible in the desktop mirror but is not reliable in both
        // XR eyes. Render the same messages on a stereo world-space canvas.
        vrBubbleCanvas = new GameObject("Visible message bubbles",
            typeof(RectTransform), typeof(Canvas));
        RectTransform canvasRect = vrBubbleCanvas.GetComponent<RectTransform>();
        canvasRect.SetParent(head, false);
        canvasRect.localPosition = new Vector3(-.31f, -.29f, 1.3f);
        canvasRect.localRotation = Quaternion.identity;
        canvasRect.localScale = Vector3.one * .0007f;
        canvasRect.pivot = new Vector2(.5f, 1f);
        canvasRect.sizeDelta = new Vector2(640f, 560f);
        Canvas canvas = vrBubbleCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = head.GetComponent<Camera>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 80;

        for (int i = 0; i < 4; i++)
        {
            GameObject bubble = new GameObject("Message " + (i + 1),
                typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            RectTransform bubbleRect = bubble.GetComponent<RectTransform>();
            bubbleRect.SetParent(canvasRect, false);
            bubbleRect.anchorMin = bubbleRect.anchorMax = new Vector2(0f, 1f);
            bubbleRect.pivot = new Vector2(0f, 1f);
            Image background = bubble.GetComponent<Image>();
            background.color = new Color(.96f, .77f, .84f, .78f);
            background.raycastTarget = false;
            vrBubbleRects.Add(bubbleRect);
            vrBubbleGroups.Add(bubble.GetComponent<CanvasGroup>());

            GameObject words = new GameObject("Words", typeof(RectTransform),
                typeof(TextMeshProUGUI));
            RectTransform wordsRect = words.GetComponent<RectTransform>();
            wordsRect.SetParent(bubbleRect, false);
            wordsRect.anchorMin = Vector2.zero;
            wordsRect.anchorMax = Vector2.one;
            wordsRect.offsetMin = new Vector2(19f, 8f);
            wordsRect.offsetMax = new Vector2(-19f, -8f);
            TextMeshProUGUI label = words.GetComponent<TextMeshProUGUI>();
            label.fontSize = 29f;
            label.color = new Color(.08f, .25f, .67f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            vrBubbleTexts.Add(label);
            bubble.SetActive(false);
        }
        vrBubbleCanvas.SetActive(false);
    }

    private void UpdateVrBubbles()
    {
        if (vrBubbleCanvas == null) return;
        bool visible = SentMessages.Count > 0;
        if (vrBubbleCanvas.activeSelf != visible) vrBubbleCanvas.SetActive(visible);
        if (!visible) return;

        RectTransform canvasRect = vrBubbleCanvas.GetComponent<RectTransform>();
        Transform anchor = IsFirstPerson ? head : avatar;
        if (anchor == null) anchor = head;
        if (canvasRect.parent != anchor) canvasRect.SetParent(anchor, false);
        if (IsFirstPerson)
        {
            // The controls drawer ends near y=-.24. Start the message feed
            // just below it, clear of the energy fan and right-side keyboard.
            canvasRect.pivot = new Vector2(.5f, 1f);
            canvasRect.localPosition = new Vector3(-.31f, -.29f, 1.3f);
            canvasRect.localRotation = Quaternion.identity;
            canvasRect.localScale = Vector3.one * .0007f;
        }
        else
        {
            canvasRect.pivot = new Vector2(.5f, 0f);
            canvasRect.localPosition = new Vector3(0f, 2.35f, 0f);
            canvasRect.rotation = Quaternion.LookRotation(canvasRect.position - head.position,
                Vector3.up);
            canvasRect.localScale = Vector3.one * .0016f;
        }

        float offset = 0f;
        for (int row = 0; row < vrBubbleTexts.Count; row++)
        {
            // Put the newest line first so speech arriving after transcription
            // appears in the most prominent position instead of below old text.
            int index = SentMessages.Count - 1 - row;
            bool hasMessage = index >= 0;
            GameObject bubble = vrBubbleRects[row].gameObject;
            if (bubble.activeSelf != hasMessage) bubble.SetActive(hasMessage);
            if (!hasMessage) continue;

            TextMeshProUGUI label = vrBubbleTexts[row];
            string message = SentMessages[index];
            if (label.text != message) label.text = message;
            float height = Mathf.Clamp(label.GetPreferredValues(message, 590f, 0f).y + 23f,
                58f, 125f);
            vrBubbleRects[row].sizeDelta = new Vector2(620f, height);
            vrBubbleRects[row].anchoredPosition = new Vector2(0f, -offset);
            offset += height + 9f;
            float age = Time.unscaledTime - sentMessageTimes[index];
            vrBubbleGroups[row].alpha = age <= MessageVisibleDuration ? 1f :
                1f - Mathf.Clamp01((age - MessageVisibleDuration) / MessageFadeDuration);
        }
    }

    private void AddKey(string value, float x, float y, float width)
    {
        GameObject key = SkyQuestVisuals.Primitive("Key " + value, PrimitiveType.Cube,
            panel.transform, new Vector3(x, y, -0.025f), new Vector3(width, 0.088f, 0.025f),
            new Color(0.31f, 0.42f, 0.55f));
        key.AddComponent<SkyQuestKey>().value = value;
        // Keep labels outside the scaled key mesh: as a child their glyphs were
        // scaled a second time and appeared blank or illegible in the headset.
        SkyQuestVisuals.Text("Label " + value, value, panel.transform,
            new Vector3(x, y, -0.055f), value.Length == 1 ? 3f : 2.2f,
            Color.white, width / 0.12f);
    }

    private void BuildRay()
    {
        GameObject pointer = new GameObject("Communication pointer", typeof(LineRenderer));
        ray = pointer.GetComponent<LineRenderer>();
        ray.positionCount = 2;
        ray.startWidth = 0.007f;
        ray.endWidth = 0.003f;
        ray.material = SkyQuestVisuals.Material(new Color(0.6f, 0.9f, 1f));
        ray.enabled = false;
    }

    private void UpdatePointer()
    {
        focused = null;
        ray.enabled = true;
        Vector3 end = rightHand.position + rightHand.forward * 3f;
        float nearest = float.PositiveInfinity;
        foreach (RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 3f))
        {
            SkyQuestKey key = hit.collider.GetComponent<SkyQuestKey>();
            if (key == null || hit.distance >= nearest) continue;
            nearest = hit.distance;
            focused = key;
            end = hit.point;
        }
        ray.SetPosition(0, rightHand.position);
        ray.SetPosition(1, end);
    }

    private void Press(string value)
    {
        if (value == "DEL")
        {
            if (CurrentDraft.Length > 0) CurrentDraft = CurrentDraft.Substring(0, CurrentDraft.Length - 1);
        }
        else if (value == "SPACE" && CurrentDraft.Length < maxMessageLength) CurrentDraft += " ";
        else if (value == "SEND")
        {
            if (!string.IsNullOrWhiteSpace(CurrentDraft))
            {
                AddSentMessage(CurrentDraft, false);
                CurrentDraft = "";
            }
        }
        else if (value == "MIC")
        {
            Debug.Log("SkyQuest MIC key pressed");
            if (IsFirstPerson) BeginVoiceMessage();
            else
            {
                status?.Show("Listening — speak now.", 5f);
                speech?.BeginRecognition();
            }
        }
        else if (CurrentDraft.Length < maxMessageLength) CurrentDraft += value;
        RefreshText();
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.2f, 0.04f);
    }

    private void OnVoiceRecognized(string result)
    {
        if (voiceMessagePending)
        {
            voiceMessagePending = false;
            if (string.IsNullOrWhiteSpace(result))
            {
                status?.Show(speech != null && !string.IsNullOrWhiteSpace(speech.LastFailure) ?
                    speech.LastFailure : "Speech unavailable. Try again.", 5f);
                return;
            }
            status?.Show("English voice captured.", 3f);
            AddSentMessage(result, true);
            RefreshText();
            return;
        }
        if (string.IsNullOrWhiteSpace(result))
        {
            status?.Show(speech != null && !string.IsNullOrWhiteSpace(speech.LastFailure) ?
                speech.LastFailure : "Speech unavailable. Use the controller keyboard.", 5f);
            return;
        }
        CurrentDraft = result.Length > maxMessageLength ? result.Substring(0, (int)maxMessageLength) : result;
        RefreshText();
        status?.Show("Voice captured. Review and press SEND.", 4f);
    }

    private void BeginVoiceMessage()
    {
        if (voiceMessagePending) return;
        chatOpen = false;
        tableChatOpen = false;
        activeTable = null;
        if (panel != null) panel.SetActive(false);
        voiceMessagePending = true;
        status?.Show("Preparing English voice...", 8f);
        Debug.Log("SkyQuest English voice requested");
        speech?.BeginRecognition();
    }

    private void OnVoiceStatus(string message)
    {
        status?.Show(message, 9f);
        Debug.Log("SkyQuest English voice: " + message);
    }

    private void UpdateLeftTriggerVoice()
    {
        bool pressed = SkyQuestInput.Trigger(SkyQuestInput.Left) > .72f;
        if (pressed && !previousLeftTrigger)
        {
            leftTriggerStarted = Time.unscaledTime;
            leftVoiceActivated = false;
        }
        if (pressed && !leftVoiceActivated && Time.unscaledTime - leftTriggerStarted >= .65f &&
            (itemTree == null || !itemTree.IsMenuOpen))
        {
            leftVoiceActivated = true;
            BeginVoiceMessage();
            SkyQuestInput.Pulse(SkyQuestInput.Left, .3f, .08f);
        }
        if (!pressed && previousLeftTrigger && !leftVoiceActivated &&
            SkyQuestMagicPickup.Held != null && SkyQuestMagicPickup.Held.held)
        {
            SkyQuestMagicPickup.Held.Release(false);
            SkyQuestInput.Pulse(SkyQuestInput.Left, .18f, .04f);
        }
        previousLeftTrigger = pressed;
    }

    private void PlacePanel()
    {
        if (panel == null || head == null) return;
        bool thirdPerson = locomotion != null && locomotion.ThirdPerson;
        Transform parent = !thirdPerson ? head :
            activeTable != null && tableChatOpen ? activeTable.transform : locomotion.transform;
        if (panel.transform.parent != parent) panel.transform.SetParent(parent, false);
        if (!thirdPerson)
        {
            // Keep the whole keyboard compact and closer to the view center,
            // with its labels and key colliders scaling together.
            panel.transform.localPosition = new Vector3(.48f, -.04f, 1.2f);
            panel.transform.localRotation = Quaternion.identity;
            panel.transform.localScale = Vector3.one * .72f;
        }
        else if (activeTable != null && tableChatOpen)
        {
            panel.transform.localPosition = new Vector3(0f, 1.37f, 0f);
            panel.transform.rotation = Quaternion.LookRotation(panel.transform.position - head.position,
                Vector3.up);
            panel.transform.localScale = Vector3.one;
        }
        else
        {
            panel.transform.localPosition = new Vector3(0f, 1.7f, 1.6f);
            panel.transform.localRotation = Quaternion.identity;
            panel.transform.localScale = Vector3.one;
        }
    }

    private void AddSentMessage(string text, bool onLeft)
    {
        string message = text.Trim();
        if (message.Length > maxMessageLength)
            message = message.Substring(0, (int)maxMessageLength);
        SentMessages.Add(message);
        sentMessageTimes.Add(Time.unscaledTime);
        sentMessageOnLeft.Add(onLeft);
        Debug.Log("SkyQuest message queued: " + (onLeft ? "voice" : "keyboard") +
            ", " + message.Length + " characters");
        while (SentMessages.Count > 4)
        {
            SentMessages.RemoveAt(0);
            sentMessageTimes.RemoveAt(0);
            sentMessageOnLeft.RemoveAt(0);
        }
        RefreshText();
    }

    private void ExpireOldMessages()
    {
        float now = Time.unscaledTime;
        for (int i = sentMessageTimes.Count - 1; i >= 0; i--)
        {
            if (now - sentMessageTimes[i] < MessageVisibleDuration + MessageFadeDuration) continue;
            sentMessageTimes.RemoveAt(i);
            SentMessages.RemoveAt(i);
            sentMessageOnLeft.RemoveAt(i);
        }
    }

    private void RefreshText()
    {
        if (draftText != null) draftText.text = "Draft: " + CurrentDraft;
        if (historyText != null) historyText.text = SentMessages.Count > 0
            ? "Last: " + SentMessages[SentMessages.Count - 1] :
                desktopMode ? "Enter: send    Esc: close" : "Left stick click: close";
    }

    public void DesktopToggle()
    {
        if (panel == null) return;
        chatOpen = !chatOpen;
        if (!chatOpen) { tableChatOpen = false; activeTable = null; }
        if (!desktopMode) panel.SetActive(chatOpen);
        status?.Show(chatOpen ? (desktopMode ? "Type a message. Enter sends; Esc closes." :
            "Point at letters and press right trigger to type.") :
            "Communication closed.", 5f);
        RefreshText();
    }

    public void DesktopType(char character)
    {
        if (!desktopMode || !IsOpen || char.IsControl(character) ||
            CurrentDraft.Length >= maxMessageLength) return;
        CurrentDraft += character;
        RefreshText();
    }

    public void DesktopDelete()
    {
        if (IsOpen) Press("DEL");
    }

    public void DesktopSend()
    {
        if (IsOpen) Press("SEND");
    }

    public void DesktopVoice()
    {
        if (IsFirstPerson)
        {
            BeginVoiceMessage();
            return;
        }
        if (!IsOpen) DesktopToggle();
        Press("MIC");
    }

    private void OpenChat()
    {
        DesktopToggle();
    }

    public void OpenFromTeaTable(SkyQuestTeaTable table)
    {
        if (panel == null || itemTree != null && itemTree.IsMenuOpen) return;
        activeTable = table;
        tableChatOpen = true;
        chatOpen = true;
        if (!desktopMode) panel.SetActive(true);
        if (!desktopMode) PlacePanel();
        RefreshText();
        status?.Show(desktopMode ? "Tea table chat open. Type and press Enter." :
            "Tea table chat open. Point at letters and press right trigger.", 4f);
    }

    private void UpdateTeaTableClick()
    {
        if (itemTree != null && itemTree.IsMenuOpen) return;
        Ray ray;
        if (desktopMode)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;
            Camera camera = Camera.main;
            if (camera == null) return;
            Vector2 point = Cursor.lockState == CursorLockMode.Locked ?
                new Vector2(Screen.width * .5f, Screen.height * .5f) : mouse.position.ReadValue();
            ray = camera.ScreenPointToRay(point);
        }
        else
        {
            bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f;
            if (trigger && !previousTableTrigger) tableTriggerStarted = Time.unscaledTime;
            bool shortClick = !trigger && previousTableTrigger &&
                Time.unscaledTime - tableTriggerStarted < .32f;
            previousTableTrigger = trigger;
            if (!shortClick || rightHand == null) return;
            ray = new Ray(rightHand.position, rightHand.forward);
        }
        RaycastHit[] hits = Physics.RaycastAll(ray, desktopMode ? 12f : 5f,
            ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            SkyQuestTeaTable table = hit.collider.GetComponentInParent<SkyQuestTeaTable>();
            if (table == null) return; // A nearer solid object blocks the click.
            if (Time.unscaledTime < table.readyAt) return;
            SkyQuestMagicPickup pickup = table.GetComponent<SkyQuestMagicPickup>();
            if (pickup != null && pickup.held) return;
            Transform viewer = avatar != null ? avatar : head;
            if (viewer == null || Vector3.Distance(viewer.position, table.transform.position) > 4f) return;
            OpenFromTeaTable(table);
            return;
        }
    }

    private void SendChat()
    {
        if (string.IsNullOrWhiteSpace(CurrentDraft)) return;
        Press("SEND");
    }

    private Vector2 HeadScreenPoint()
    {
        Transform anchor = avatar != null ? avatar : head;
        if (anchor == null) return new Vector2(Screen.width * .5f, Screen.height * .35f);
        Vector3 world = anchor.position + Vector3.up * (avatar != null ? 2.05f : .42f);
        Camera camera = Camera.main;
        if (camera == null) camera = head != null ? head.GetComponent<Camera>() : null;
        if (camera == null) return new Vector2(Screen.width * .5f, Screen.height * .35f);
        Vector3 screen = camera.WorldToScreenPoint(world);
        if (screen.z <= 0f) return new Vector2(-1000f, -1000f);
        return new Vector2(screen.x, Screen.height - screen.y);
    }

    private void EnsureGuiStyles()
    {
        if (iconStyle != null) return;
        if (chatBackground == null) chatBackground = MakeGuiTexture(new Color(.025f, .035f, .045f, .78f));
        if (bubbleBackground == null) bubbleBackground = MakeRoundedGuiTexture();
        if (typingBackground == null) typingBackground = MakeGuiTexture(new Color(1f, .94f, .69f, .96f));
        if (controlBackground == null) controlBackground = MakeGuiTexture(new Color(.08f, .10f, .12f, .76f));
        if (keyboardBackground == null) keyboardBackground = MakeGuiTexture(new Color(.61f, .82f, .96f, .46f));
        if (keyboardKeyBackground == null) keyboardKeyBackground = MakeGuiTexture(new Color(.96f, .99f, .88f, .90f));
        if (keyboardKeySelectedBackground == null) keyboardKeySelectedBackground = MakeGuiTexture(new Color(1f, .82f, .36f, .98f));
        if (keyboardActionBackground == null) keyboardActionBackground = MakeGuiTexture(new Color(.82f, .88f, .75f, .94f));
        iconStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.Clamp(Screen.width / 34, 20, 34),
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, .98f, .88f) },
            hover = { textColor = Color.white },
            active = { textColor = Color.white }
        };
        inputStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = Mathf.Clamp(Screen.width / 55, 18, 28),
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(14, 12, 8, 8),
            normal = { textColor = Color.white, background = chatBackground },
            focused = { textColor = Color.white, background = chatBackground },
            hover = { textColor = Color.white, background = chatBackground },
            active = { textColor = Color.white, background = chatBackground }
        };
        messageStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = Mathf.Clamp(Screen.width / 75, 16, 24),
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { textColor = Color.white, background = chatBackground },
            padding = new RectOffset(12, 12, 7, 7)
        };
        leftBubbleStyle = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(8, 8, 8, 8),
            normal = { background = bubbleBackground }
        };
        typingStyle = new GUIStyle(messageStyle)
        {
            fontSize = Mathf.Clamp(Screen.width / 60, 18, 28),
            normal = { textColor = new Color(.3f, .27f, .15f), background = typingBackground }
        };
        bubbleLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = messageStyle.fontSize,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { textColor = new Color(.08f, .25f, .67f) }
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.Clamp(Screen.width / 60, 18, 26),
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        inputLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = inputStyle.fontSize,
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(14, 12, 8, 8),
            normal = { textColor = Color.white }
        };
        controlsBackdropStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = controlBackground }
        };
        keyboardPanelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = keyboardBackground },
            padding = new RectOffset(12, 12, 10, 10)
        };
        keyboardKeyStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.Clamp(Screen.width / 42, 18, 34),
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(.20f, .22f, .18f), background = keyboardKeyBackground },
            hover = { textColor = Color.black, background = keyboardKeySelectedBackground },
            active = { textColor = Color.black, background = keyboardKeySelectedBackground }
        };
        keyboardKeySelectedStyle = new GUIStyle(keyboardKeyStyle)
        {
            normal = { textColor = Color.black, background = keyboardKeySelectedBackground }
        };
        keyboardActionStyle = new GUIStyle(keyboardKeyStyle)
        {
            fontSize = Mathf.Clamp(Screen.width / 62, 16, 26),
            normal = { textColor = new Color(.18f, .20f, .16f), background = keyboardActionBackground }
        };
    }

    private static Texture2D MakeGuiTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Texture2D MakeRoundedGuiTexture()
    {
        const int size = 24;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = Mathf.Max(0f, Mathf.Abs(x + .5f - size * .5f) - (size * .5f - 7f));
            float dy = Mathf.Max(0f, Mathf.Abs(y + .5f - size * .5f) - (size * .5f - 7f));
            float coverage = Mathf.Clamp01(7.5f - Mathf.Sqrt(dx * dx + dy * dy));
            texture.SetPixel(x, y, new Color(.96f, .77f, .84f, .78f * coverage));
        }
        texture.Apply();
        return texture;
    }

    private void BindDesktopKeyboard()
    {
        Keyboard current = Keyboard.current;
        if (current == boundKeyboard) return;
        if (boundKeyboard != null) boundKeyboard.onTextInput -= OnDesktopTextInput;
        boundKeyboard = current;
        if (boundKeyboard != null) boundKeyboard.onTextInput += OnDesktopTextInput;
    }

    private void OnDesktopTextInput(char character)
    {
        if (!desktopMode || !IsOpen || char.IsControl(character)) return;
        AddCharacter(character);
        highlightedKey = char.ToLowerInvariant(character);
        highlightedKeyUntil = Time.unscaledTime + .22f;
    }

    void OnDestroy()
    {
        if (keyboardBackingMaterial != null) Destroy(keyboardBackingMaterial);
        if (boundKeyboard != null) boundKeyboard.onTextInput -= OnDesktopTextInput;
        if (speech != null)
        {
            speech.OnRecognized -= OnVoiceRecognized;
            speech.OnStatusChanged -= OnVoiceStatus;
        }
    }

    private void UpdateDesktopKeyboard()
    {
        if (!chatOpen) return;
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            for (char letter = 'a'; letter <= 'z'; letter++)
            {
                if (!System.Enum.TryParse(letter.ToString().ToUpperInvariant(), out Key key)) continue;
                if (!keyboard[key].wasPressedThisFrame) continue;
                highlightedKey = letter;
                highlightedKeyUntil = Time.unscaledTime + .22f;
            }
            if (keyboard.leftArrowKey.wasPressedThisFrame) MoveKeyboardSelection(-1, 0);
            if (keyboard.rightArrowKey.wasPressedThisFrame) MoveKeyboardSelection(1, 0);
            if (keyboard.upArrowKey.wasPressedThisFrame) MoveKeyboardSelection(0, -1);
            if (keyboard.downArrowKey.wasPressedThisFrame) MoveKeyboardSelection(0, 1);
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;
        Vector2 stick = gamepad.leftStick.ReadValue();
        bool left = gamepad.dpad.left.wasPressedThisFrame || (stick.x < -.65f && Time.unscaledTime >= nextGamepadNavigation);
        bool right = gamepad.dpad.right.wasPressedThisFrame || (stick.x > .65f && Time.unscaledTime >= nextGamepadNavigation);
        bool up = gamepad.dpad.up.wasPressedThisFrame || (stick.y > .65f && Time.unscaledTime >= nextGamepadNavigation);
        bool down = gamepad.dpad.down.wasPressedThisFrame || (stick.y < -.65f && Time.unscaledTime >= nextGamepadNavigation);
        if (left || right || up || down)
        {
            MoveKeyboardSelection(left ? -1 : right ? 1 : 0, up ? -1 : down ? 1 : 0);
            nextGamepadNavigation = Time.unscaledTime + .2f;
        }
        if (gamepad.buttonSouth.wasPressedThisFrame) ActivateKeyboardSelection();
        if (gamepad.buttonEast.wasPressedThisFrame) DesktopToggle();
    }

    private void MoveKeyboardSelection(int horizontal, int vertical)
    {
        int row = selectedKeyboardKey < 10 ? 0 : selectedKeyboardKey < 19 ? 1 : selectedKeyboardKey < 26 ? 2 : 3;
        int[] rowStarts = { 0, 10, 19, 26 };
        int[] rowLengths = { 10, 9, 7, 3 };
        int column = selectedKeyboardKey - rowStarts[row];
        if (horizontal != 0)
            column = (column + horizontal + rowLengths[row]) % rowLengths[row];
        if (vertical != 0)
        {
            int nextRow = Mathf.Clamp(row + vertical, 0, rowLengths.Length - 1);
            float normalizedColumn = rowLengths[row] <= 1 ? .5f : column / (float)(rowLengths[row] - 1);
            column = Mathf.RoundToInt(normalizedColumn * (rowLengths[nextRow] - 1));
            row = nextRow;
        }
        selectedKeyboardKey = rowStarts[row] + column;
    }

    private void ActivateKeyboardSelection()
    {
        if (selectedKeyboardKey < 26)
        {
            AddCharacter((char)('a' + selectedKeyboardKey));
            highlightedKey = (char)('a' + selectedKeyboardKey);
            highlightedKeyUntil = Time.unscaledTime + .22f;
        }
        else if (selectedKeyboardKey == 26) AddCharacter(' ');
        else if (selectedKeyboardKey == 27) Press("DEL");
        else SendChat();
    }

    private void AddCharacter(char character)
    {
        if (CurrentDraft.Length >= maxMessageLength) return;
        CurrentDraft += character;
        RefreshText();
    }

    private void DrawVirtualKeyboard(float edge)
    {
        float panelHeight = Mathf.Clamp(Screen.height * .38f, 260f, 430f);
        float panelTop = Screen.height - panelHeight;
        Rect panel = new Rect(0f, panelTop, Screen.width, panelHeight);
        GUI.color = Color.white;
        GUI.Box(panel, GUIContent.none, keyboardPanelStyle);

        float gap = Mathf.Clamp(Screen.width * .008f, 5f, 12f);
        float rowHeight = Mathf.Clamp((panelHeight - 54f) / 4f, 42f, 72f);
        float y = panelTop + 12f;
        DrawKeyRow("qwertyuiop", 0, 10, edge, y, rowHeight, gap);
        y += rowHeight + gap;
        float indent = Screen.width * .045f;
        DrawKeyRow("asdfghjkl", 10, 9, edge + indent, y, rowHeight, gap,
            Screen.width - 2f * (edge + indent));
        y += rowHeight + gap;
        indent = Screen.width * .17f;
        DrawKeyRow("zxcvbnm", 19, 7, edge + indent, y, rowHeight, gap,
            Screen.width - 2f * (edge + indent));
        y += rowHeight + gap;
        float actionWidth = (Screen.width - 2f * edge - 2f * gap) / 3f;
        DrawActionKey(new Rect(edge, y, actionWidth, rowHeight), "空格", 26);
        DrawActionKey(new Rect(edge + actionWidth + gap, y, actionWidth, rowHeight), "删除", 27);
        DrawActionKey(new Rect(edge + 2f * (actionWidth + gap), y, actionWidth, rowHeight), "发送", 28);
    }

    private void DrawKeyRow(string letters, int firstIndex, int count, float x, float y,
        float height, float gap, float totalWidth = -1f)
    {
        float width = totalWidth < 0f ? Screen.width - 2f * x : totalWidth;
        float keyWidth = (width - gap * (count - 1)) / count;
        for (int i = 0; i < letters.Length; i++)
        {
            int index = firstIndex + i;
            Rect rect = new Rect(x + i * (keyWidth + gap), y, keyWidth, height);
            bool selected = index == selectedKeyboardKey;
            bool flashing = char.ToLowerInvariant(letters[i]) == highlightedKey && Time.unscaledTime < highlightedKeyUntil;
            GUI.color = selected || flashing ? Color.white : new Color(1f, 1f, 1f, .93f);
            if (GUI.Button(rect, letters[i].ToString(), selected || flashing ? keyboardKeySelectedStyle : keyboardKeyStyle))
            {
                selectedKeyboardKey = index;
                AddCharacter(letters[i]);
                highlightedKey = letters[i];
                highlightedKeyUntil = Time.unscaledTime + .22f;
            }
        }
    }

    private void DrawActionKey(Rect rect, string label, int index)
    {
        GUI.color = index == selectedKeyboardKey ? Color.white : new Color(1f, 1f, 1f, .95f);
        if (!GUI.Button(rect, label, index == selectedKeyboardKey ? keyboardKeySelectedStyle : keyboardActionStyle)) return;
        selectedKeyboardKey = index;
        ActivateKeyboardSelection();
    }

    private void OnGUI()
    {
        if (head == null) return;
        EnsureGuiStyles();
        Color previous = GUI.color;

        // Head labels follow the local character in screen space so they
        // remain readable from the third-person desktop camera and in XR.
        Vector2 headPoint = HeadScreenPoint();
        bool headVisible = headPoint.x > 0f && headPoint.y > 0f && headPoint.y < Screen.height;
        float bubbleWidth = Mathf.Clamp(Screen.width * .24f, 190f, 420f);
        float bubbleHeight = Mathf.Clamp(Screen.height * .052f, 32f, 54f);
        float gap = 5f;
        float top = headPoint.y - bubbleHeight - 14f;
        float leftTop = IsFirstPerson ? 210f : 106f;
        int headRow = 0;
        float leftOffset = 0f;
        int first = Mathf.Max(0, SentMessages.Count - 4);
        // XR messages are already drawn on a stereo world-space canvas.
        // Drawing IMGUI copies as well can cover the cape-energy fan.
        for (int i = first; desktopMode && i < SentMessages.Count; i++)
        {
            bool onLeft = IsFirstPerson || sentMessageOnLeft[i];
            int row = onLeft ? 0 : headRow++;
            if (!onLeft && !headVisible) continue;
            GUIContent content = new GUIContent(SentMessages[i]);
            float leftWidth = Mathf.Clamp(bubbleLabelStyle.CalcSize(content).x + 30f,
                110f, Mathf.Min(440f, Screen.width * .32f));
            float leftHeight = Mathf.Max(34f,
                bubbleLabelStyle.CalcHeight(content, leftWidth - 24f) + 14f);
            Rect rect = onLeft
                ? new Rect(IsFirstPerson ? (Screen.width - leftWidth) * .5f : 18f,
                    leftTop + leftOffset, leftWidth, leftHeight)
                : new Rect(headPoint.x - bubbleWidth * .5f,
                    top - row * (bubbleHeight + gap), bubbleWidth, bubbleHeight);
            if (onLeft) leftOffset += leftHeight + gap;
            float age = Time.unscaledTime - sentMessageTimes[i];
            float alpha = age <= MessageVisibleDuration ? 1f :
                1f - Mathf.Clamp01((age - MessageVisibleDuration) / MessageFadeDuration);
            GUI.color = onLeft ? new Color(1f, 1f, 1f, alpha) :
                new Color(.035f, .045f, .055f, .76f * alpha);
            GUI.Box(rect, GUIContent.none, onLeft ? leftBubbleStyle : messageStyle);
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(rect, SentMessages[i], bubbleLabelStyle);
        }
        if (desktopMode && headVisible && IsOpen)
        {
            Rect typing = new Rect(headPoint.x - 31f, top - headRow * (bubbleHeight + gap) - 38f, 62f, 34f);
            GUI.color = new Color(1f, .93f, .66f, .94f);
            GUI.Box(typing, GUIContent.none, typingStyle);
            GUI.color = new Color(.28f, .25f, .16f, 1f);
            GUI.Label(typing, "…", typingStyle);
        }

        // In third-person Quest mode only world-anchored chat and bubbles are
        // visible; the head-locked icons and screen keyboard belong to first-person.
        if (!desktopMode && !IsFirstPerson)
        {
            GUI.color = previous;
            return;
        }

        float edge = Mathf.Clamp(Screen.width * .018f, 12f, 28f);
        float iconSize = Mathf.Clamp(Screen.width * .047f, 52f, 76f);
        float x = Screen.width - edge - iconSize;
        float keyboardHeight = chatOpen ? Mathf.Clamp(Screen.height * .38f, 260f, 430f) : 0f;
        float chatY = Screen.height - edge - iconSize - keyboardHeight;
        float micY = chatY - iconSize - 12f;
        GUI.color = Color.white;
        GUI.Box(new Rect(x - 5f, micY - 5f, iconSize + 10f, iconSize * 2f + 29f),
            GUIContent.none, controlsBackdropStyle);
        GUI.color = new Color(.98f, .95f, .84f, 1f);
        if (GUI.Button(new Rect(x, micY, iconSize, iconSize), "🎙", iconStyle)) DesktopVoice();
        if (GUI.Button(new Rect(x, chatY, iconSize, iconSize), "💬", iconStyle)) OpenChat();
        if (chatOpen)
        {
            float barHeight = Mathf.Clamp(Screen.height * .052f, 42f, 62f);
            float barWidth = Screen.width * .74f;
            float barX = (Screen.width - barWidth) * .5f;
            float barY = Screen.height - keyboardHeight - barHeight - 12f;
            GUI.color = Color.white;
            GUI.Box(new Rect(barX, barY, barWidth, barHeight), GUIContent.none);
            GUI.color = new Color(1f, .98f, .9f, 1f);
            Rect inputRect = new Rect(barX + 8f, barY + 3f, barWidth - 96f, barHeight - 6f);
            GUI.Box(inputRect, GUIContent.none, messageStyle);
            GUI.Label(inputRect, string.IsNullOrEmpty(CurrentDraft) ? "聊一聊…" : CurrentDraft, inputLabelStyle);
            if (GUI.Button(new Rect(barX + barWidth - 82f, barY + 3f, 76f, barHeight - 6f), "发送", buttonStyle)) SendChat();
            DrawVirtualKeyboard(edge);
            Event evt = Event.current;
            if (evt != null && evt.type == EventType.KeyDown &&
                (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter))
            {
                SendChat();
                evt.Use();
            }
        }
        GUI.color = previous;
    }
}

public class SkyQuestKey : MonoBehaviour
{
    public string value;
}
