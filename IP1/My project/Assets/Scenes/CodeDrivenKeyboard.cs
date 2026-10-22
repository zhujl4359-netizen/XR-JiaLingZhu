using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 使用 Unity 的即时模式 GUI 绘制桌面虚拟键盘；按住 T 时显示。
/// </summary>
public class CodeDrivenKeyboard : MonoBehaviour
{
    [Header("UI 颜色设置 (实时生效)")]
    [Tooltip("调节 A (Alpha) 通道可控制半透明度")]
    public Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.85f); // 默认深色半透明
    public Color keyBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);     // 按键底色
    public Color fontColor = Color.white;                                    // 字体颜色

    [Header("面板尺寸")]
    public float panelWidth = 700f; // 完整键盘较宽，需调大面板宽度

    private bool isTHeld = false;
    private GUIStyle panelStyle;
    private GUIStyle buttonStyle;
    
    // 用于检测颜色变化，避免每帧重复生成贴图造成内存泄漏
    private Color lastPanelColor;
    private Color lastKeyColor;
    private bool stylesInitialized = false;
    private Texture2D bgTexture;
    private Texture2D btnTexture;

    // 严格按照标准实体打字键盘的 5 排布局定义
    private readonly string[][] keyboardLayout = new string[][]
    {
        new string[] { "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "Backspace" },
        new string[] { "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]", "\\" },
        new string[] { "Caps", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "Enter" },
        new string[] { "Shift", "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/", "Shift " },
        new string[] { "Ctrl", "Win", "Alt", "Space", "Alt ", "Win ", "Ctrl " }
    };

    /// <summary>
    /// 读取 T 键是否按住，决定本帧是否显示虚拟键盘。
    /// </summary>
    void Update()
    {
        if (Keyboard.current != null)
        {
            isTHeld = Keyboard.current.tKey.isPressed;
        }
    }

    /// <summary>
    /// 仅在按住 T 时绘制面板；颜色变化时才重建 GUI 样式。
    /// </summary>
    void OnGUI()
    {
        if (!isTHeld) return;

        // 监听 Inspector 颜色变化，若改变则重新生成样式
        if (!stylesInitialized || lastPanelColor != panelBackgroundColor || lastKeyColor != keyBackgroundColor)
        {
            InitStyles();
            lastPanelColor = panelBackgroundColor;
            lastKeyColor = keyBackgroundColor;
            stylesInitialized = true;
        }

        float panelHeight = Screen.height;
        float panelX = Screen.width - panelWidth;
        float panelY = 0f;

        // 绘制右侧半透明底板
        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "Standard QWERTY Keyboard", panelStyle);

        // 绘制键盘阵列
        DrawAuthenticKeyboard(panelX, panelY, panelWidth);
    }

    /// <summary>
    /// 销毁旧颜色贴图并生成新的面板、按键样式，避免重复分配累积。
    /// </summary>
    private void InitStyles()
    {
        // 清理旧贴图防内存泄漏
        if (bgTexture != null) Destroy(bgTexture);
        if (btnTexture != null) Destroy(btnTexture);

        // 生成底板贴图
        bgTexture = new Texture2D(1, 1);
        bgTexture.SetPixel(0, 0, panelBackgroundColor);
        bgTexture.Apply();

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = bgTexture, textColor = fontColor },
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.UpperCenter,
            padding = new RectOffset(0, 0, 40, 0)
        };

        // 生成按键贴图
        btnTexture = new Texture2D(1, 1);
        btnTexture.SetPixel(0, 0, keyBackgroundColor);
        btnTexture.Apply();

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = btnTexture, textColor = fontColor },
            fontSize = 16
        };
    }

    /// <summary>
    /// 按预设的五排布局绘制键位，点击时将键名输出到日志。
    /// </summary>
    private void DrawAuthenticKeyboard(float startX, float startY, float width)
    {
        float spacing = 4f;          // 按键之间的缝隙
        float baseKeySize = 38f;     // 基础按键（字母键）的宽高
        float startOffsetY = 120f;   // 距离屏幕顶部的初始高度
        
        // 让整个键盘在面板中水平居中
        float keyboardTotalWidth = (14 * baseKeySize) + (13 * spacing); // 第一排的总宽度估算
        float startOffsetX = startX + (width - keyboardTotalWidth) / 2f;

        for (int r = 0; r < keyboardLayout.Length; r++)
        {
            float currentX = startOffsetX;
            float currentY = startY + startOffsetY + r * (baseKeySize + spacing);

            for (int c = 0; c < keyboardLayout[r].Length; c++)
            {
                string key = keyboardLayout[r][c];
                
                // 根据标准键盘映射每个特殊按键的宽度
                float keyWidth = GetAuthenticKeyWidth(key, baseKeySize);

                // 绘制按钮
                if (GUI.Button(new Rect(currentX, currentY, keyWidth, baseKeySize), key.Trim(), buttonStyle))
                {
                    Debug.Log($"<color=cyan>键盘输入: {key.Trim()}</color>");
                }

                currentX += keyWidth + spacing;
            }
        }
    }

    // 获取实体键盘真实比例的按键宽度
    /// <summary>
    /// 给空格、回车等特殊键分配更宽的绘制尺寸。
    /// </summary>
    private float GetAuthenticKeyWidth(string key, float baseWidth)
    {
        switch (key.Trim())
        {
            case "Backspace": return baseWidth * 2.0f;
            case "Tab":       return baseWidth * 1.5f;
            case "\\":        return baseWidth * 1.5f;
            case "Caps":      return baseWidth * 1.75f;
            case "Enter":     return baseWidth * 2.25f;
            case "Shift":     return baseWidth * 2.25f;
            case "Ctrl":
            case "Win":
            case "Alt":       return baseWidth * 1.25f;
            case "Space":     return baseWidth * 6.5f; // 空格键超长宽
            default:          return baseWidth;        // 普通字母数字键
        }
    }
}
