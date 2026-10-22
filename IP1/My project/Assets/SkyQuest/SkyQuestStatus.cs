using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkyQuestStatus : MonoBehaviour
{
    public Transform head;
    public bool desktopMode;
    private TextMeshPro message;
    private Transform statusPanel;
    private SkyQuestLocomotion player;
    private float expires;
    private AudioSource confirmation;
    private string currentText;
    private GUIStyle candleNumberStyle;
    private GUIStyle candleHeartStyle;
    private RectTransform vrCandlePanel;
    private TextMeshProUGUI vrCandleNumber;
    private int displayedCandleBalance = int.MinValue;
    private static Texture2D candleIcon;
    private static AudioClip crunchClip;

    void Start()
    {
        if (head == null) return;
        player = GetComponent<SkyQuestLocomotion>();
        if (!desktopMode) BuildVrCandleCounter();
        Transform panel = new GameObject("Sky status panel").transform;
        statusPanel = panel;
        panel.SetParent(head, false);
        panel.localPosition = new Vector3(0f, -0.26f, 1.15f);
        message = SkyQuestVisuals.Text("Status", "", panel, Vector3.zero, 2.4f,
            new Color(1f, 0.94f, 0.7f), 1.25f);
        message.rectTransform.sizeDelta = new Vector2(1.25f, 0.28f);
        if (desktopMode) message.enabled = false;
        message.enableWordWrapping = true;
        confirmation = head.gameObject.AddComponent<AudioSource>();
        confirmation.spatialBlend = 0f;
        confirmation.volume = 0.25f;
        const int sampleRate = 22050;
        const int samples = 4400;
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-14f * t);
            data[i] = Mathf.Sin(2f * Mathf.PI * (660f + 550f * t) * t) * envelope * 0.32f;
        }
        AudioClip clip = AudioClip.Create("Sky confirmation chime", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        confirmation.clip = clip;
        SkyQuestGuideUI guide = GetComponent<SkyQuestGuideUI>();
        if (guide == null) guide = gameObject.AddComponent<SkyQuestGuideUI>();
        guide.head = head;
        guide.desktopMode = desktopMode;
        Show(desktopMode ? "WASD move • Arrows look • F near Ancestor • K type" :
            "Left stick: move • Right stick: turn • Approach the Ancestor and press X", 8f);
    }

    void Update()
    {
        if (!desktopMode) UpdateVrCandleCounter();
        if (!desktopMode && player != null && statusPanel != null)
        {
            Transform parent = player.ThirdPerson ? player.transform : head;
            if (statusPanel.parent != parent)
            {
                statusPanel.SetParent(parent, false);
                statusPanel.localPosition = player.ThirdPerson ?
                    new Vector3(-.25f, 1.95f, 2.15f) : new Vector3(0f, -.26f, 1.15f);
                statusPanel.localRotation = Quaternion.identity;
                statusPanel.localScale = Vector3.one * (player.ThirdPerson ? 1.5f : 1f);
            }
        }
        if (Time.time > expires)
        {
            currentText = "";
            if (message != null) message.text = "";
        }
    }

    private void OnGUI()
    {
        if (desktopMode) DrawCandleCounter();
        if (desktopMode && !string.IsNullOrEmpty(currentText) && Time.time <= expires)
            GUI.Box(new Rect(16f, 82f, 490f, 40f), currentText);
    }

    private void DrawCandleCounter()
    {
        if (candleIcon == null) CreateCandleIcon();
        if (candleNumberStyle == null)
        {
            candleNumberStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Clamp(Screen.width / 62, 20, 30),
                alignment = TextAnchor.MiddleLeft
            };
            candleNumberStyle.normal.textColor = new Color(1f, .97f, .88f);
            candleHeartStyle = new GUIStyle(candleNumberStyle)
            {
                fontSize = Mathf.Clamp(Screen.width / 82, 16, 22),
                alignment = TextAnchor.MiddleCenter
            };
            candleHeartStyle.normal.textColor = new Color(.23f, .21f, .18f);
        }

        Color previous = GUI.color;
        float left = Screen.width * .5f - 62f;
        Rect iconRect = new Rect(left, 17f, 32f, 48f);
        GUI.color = Color.white;
        GUI.DrawTexture(iconRect, candleIcon, ScaleMode.StretchToFill, true);
        GUI.Label(new Rect(iconRect.x + 1f, iconRect.y + 10f, iconRect.width - 2f, 27f), "♥", candleHeartStyle);
        GUI.color = new Color(1f, .97f, .88f, 1f);
        GUI.Label(new Rect(left + 37f, 25f, 90f, 34f), SkyQuestCandleWallet.Balance.ToString(), candleNumberStyle);
        GUI.color = previous;
    }

    private void BuildVrCandleCounter()
    {
        if (candleIcon == null) CreateCandleIcon();
        GameObject root = new GameObject("Top-center candle counter",
            typeof(RectTransform), typeof(Canvas));
        vrCandlePanel = root.GetComponent<RectTransform>();
        vrCandlePanel.SetParent(head, false);
        vrCandlePanel.sizeDelta = new Vector2(218f, 76f);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = head.GetComponent<Camera>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 95;

        GameObject plate = new GameObject("Subtle counter backing",
            typeof(RectTransform), typeof(Image));
        RectTransform plateRect = plate.GetComponent<RectTransform>();
        plateRect.SetParent(vrCandlePanel, false);
        plateRect.anchorMin = Vector2.zero;
        plateRect.anchorMax = Vector2.one;
        plateRect.offsetMin = plateRect.offsetMax = Vector2.zero;
        Image backing = plate.GetComponent<Image>();
        backing.color = new Color(.015f, .025f, .045f, .55f);
        backing.raycastTarget = false;

        GameObject candle = new GameObject("Candle symbol",
            typeof(RectTransform), typeof(RawImage));
        RectTransform candleRect = candle.GetComponent<RectTransform>();
        candleRect.SetParent(vrCandlePanel, false);
        candleRect.anchorMin = candleRect.anchorMax = new Vector2(.5f, .5f);
        candleRect.sizeDelta = new Vector2(32f, 48f);
        candleRect.anchoredPosition = new Vector2(-55f, 0f);
        RawImage symbol = candle.GetComponent<RawImage>();
        symbol.texture = candleIcon;
        symbol.raycastTarget = false;

        GameObject count = new GameObject("Candles remaining",
            typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform countRect = count.GetComponent<RectTransform>();
        countRect.SetParent(vrCandlePanel, false);
        countRect.anchorMin = countRect.anchorMax = new Vector2(.5f, .5f);
        countRect.sizeDelta = new Vector2(116f, 62f);
        countRect.anchoredPosition = new Vector2(34f, 0f);
        vrCandleNumber = count.GetComponent<TextMeshProUGUI>();
        vrCandleNumber.fontSize = 42f;
        vrCandleNumber.alignment = TextAlignmentOptions.MidlineLeft;
        vrCandleNumber.color = new Color(1f, .97f, .86f);
        vrCandleNumber.raycastTarget = false;
        UpdateVrCandleCounter();
    }

    private void UpdateVrCandleCounter()
    {
        if (vrCandlePanel == null || head == null) return;
        bool thirdPerson = player != null && player.ThirdPerson && player.avatar != null;
        Transform anchor = thirdPerson ? player.avatar.transform : head;
        if (vrCandlePanel.parent != anchor) vrCandlePanel.SetParent(anchor, false);
        if (thirdPerson)
        {
            // Keep the third-person counter with the avatar, not the turning headset.
            vrCandlePanel.position = player.avatar.transform.position + Vector3.up * 2.8f;
            vrCandlePanel.rotation = Quaternion.LookRotation(
                vrCandlePanel.position - head.position, Vector3.up);
            vrCandlePanel.localScale = Vector3.one * .004f;
        }
        else
        {
            vrCandlePanel.localPosition = new Vector3(0f, .54f, 1.45f);
            vrCandlePanel.localRotation = Quaternion.identity;
            vrCandlePanel.localScale = Vector3.one * .00165f;
        }
        int balance = SkyQuestCandleWallet.Balance;
        if (vrCandleNumber != null && balance != displayedCandleBalance)
        {
            vrCandleNumber.text = balance.ToString();
            displayedCandleBalance = balance;
        }
    }

    private static void CreateCandleIcon()
    {
        const int width = 32;
        const int height = 48;
        candleIcon = new Texture2D(width, height, TextureFormat.RGBA32, false);
        candleIcon.hideFlags = HideFlags.HideAndDontSave;
        candleIcon.filterMode = FilterMode.Bilinear;
        Color clear = new Color(1f, 1f, 1f, 0f);
        Color wax = new Color(1f, .97f, .87f, 1f);
        Color highlight = new Color(1f, 1f, .96f, 1f);
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool wick = x >= 15 && x <= 16 && y >= 41 && y <= 47;
                bool body = y >= 2 && y <= 42 && x >= 4 && x <= 27;
                if (body && y > 37 && (x < 7 || x > 24)) body = false;
                if (body && y < 7 && (x < 7 || x > 24)) body = false;
                if (!body && !wick) continue;
                pixels[y * width + x] = body && x == 6 ? highlight : wax;
            }
        }
        candleIcon.SetPixels(pixels);
        candleIcon.Apply(false, true);
    }

    public void Show(string text, float seconds = 3f)
    {
        currentText = text;
        if (message != null) message.text = text;
        expires = Time.time + seconds;
    }

    public void PlayConfirm()
    {
        if (confirmation != null) confirmation.PlayOneShot(confirmation.clip);
    }

    public void PlayCrunch()
    {
        if (confirmation == null) return;
        if (crunchClip == null) CreateCrunchClip();
        confirmation.PlayOneShot(crunchClip, 1.7f);
    }

    private static void CreateCrunchClip()
    {
        const int rate = 22050;
        const int count = 17640;
        float[] data = new float[count];
        System.Random random = new System.Random(4271);
        float filtered = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = i / (float)rate;
            float first = Mathf.Clamp01((t - .11f) / .015f) *
                Mathf.Exp(-19f * Mathf.Max(0f, t - .13f));
            float second = Mathf.Clamp01((t - .41f) / .012f) *
                Mathf.Exp(-17f * Mathf.Max(0f, t - .43f));
            if (t < .11f) first = 0f;
            if (t < .41f) second = 0f;
            float noise = (float)random.NextDouble() * 2f - 1f;
            filtered = Mathf.Lerp(filtered, noise, .65f);
            float grain = Mathf.Sin(2f * Mathf.PI * 173f * t) * .18f;
            data[i] = Mathf.Clamp((filtered * .75f + grain) *
                (first + second) * .65f, -.9f, .9f);
        }
        crunchClip = AudioClip.Create("Two soft crunchy bites", count, 1, rate, false);
        crunchClip.SetData(data, 0);
    }
}
