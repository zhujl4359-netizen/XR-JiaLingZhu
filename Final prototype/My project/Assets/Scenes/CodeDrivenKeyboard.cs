// ！！！文件功能：用即时 GUI 绘制桌面虚拟键盘并处理按键展示！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

/// <summary>
/// 使用 Unity 的即时模式 GUI 绘制桌面虚拟键盘；按住 T 时显示。
/// </summary>
// 定义 CodeDrivenKeyboard 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class CodeDrivenKeyboard : MonoBehaviour
{
    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("UI 颜色设置 (实时生效)")]
    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("调节 A (Alpha) 通道可控制半透明度")]
    // 创建新的 Color 并存为 panelBackgroundColor；构造参数决定其初始内容或对象名称。
    public Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.85f); // 默认深色半透明
    // 创建新的 Color 并存为 keyBackgroundColor；构造参数决定其初始内容或对象名称。
    public Color keyBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);     // 按键底色
    // 计算 Color.white，把结果存进 fontColor（fontColor）；之后引用 fontColor 时用的就是这次结果。
    public Color fontColor = Color.white;                                    // 字体颜色

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("面板尺寸")]
    // 把 Inspector 可调整的 panelWidth（panelWidth）设为默认值 700f；具体用途看它在下方的判断和计算。
    public float panelWidth = 700f; // 完整键盘较宽，需调大面板宽度

    // 用 isTHeld 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool isTHeld = false;
    // panelStyle 保存panelStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle panelStyle;
    // buttonStyle 保存buttonStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle buttonStyle;
    
    // 用于检测颜色变化，避免每帧重复生成贴图造成内存泄漏
    // 声明 lastPanelColor（last面板Color）：颜色；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Color lastPanelColor;
    // 声明 lastKeyColor（lastKeyColor）：颜色；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Color lastKeyColor;
    // 用 stylesInitialized 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool stylesInitialized = false;
    // bgTexture 保存bg贴图（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private Texture2D bgTexture;
    // btnTexture 保存btn贴图（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private Texture2D btnTexture;

    // 严格按照标准实体打字键盘的 5 排布局定义
    // 这一行补充 private readonly string[][] keyboardLayout = new string[][]，与相邻行合起来完成同一次计算或对象配置。
    private readonly string[][] keyboardLayout = new string[][]
    {
        // 用 new 创建 string[] { "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", ；构造参数决定新对象的初始内容。
        new string[] { "`", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "=", "Backspace" },
        // 用 new 创建 string[] { "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]"；构造参数决定新对象的初始内容。
        new string[] { "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "[", "]", "\\" },
        // 用 new 创建 string[] { "Caps", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "E；构造参数决定新对象的初始内容。
        new string[] { "Caps", "A", "S", "D", "F", "G", "H", "J", "K", "L", ";", "'", "Enter" },
        // 用 new 创建 string[] { "Shift", "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/", "Shift；构造参数决定新对象的初始内容。
        new string[] { "Shift", "Z", "X", "C", "V", "B", "N", "M", ",", ".", "/", "Shift " },
        // 用 new 创建 string[] { "Ctrl", "Win", "Alt", "Space", "Alt ", "Win ", "Ctrl " }；构造参数决定新对象的初始内容。
        new string[] { "Ctrl", "Win", "Alt", "Space", "Alt ", "Win ", "Ctrl " }
    };

    /// <summary>
    /// 读取 T 键是否按住，决定本帧是否显示虚拟键盘。
    /// </summary>
    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：先确认启用了键盘备用操作且当前存在键盘设备；桌面测试时才能读取按键。
        if (Keyboard.current != null)
        {
            // 步骤 2：把计算结果 Keyboard.current.tKey.isPressed 保存为istHeld；下一条判断或调用会读取这个具体值。
            isTHeld = Keyboard.current.tKey.isPressed;
        }
    }
    // ！！！功能结束：Update！！！

    /// <summary>
    /// 仅在按住 T 时绘制面板；颜色变化时才重建 GUI 样式。
    /// </summary>
    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void OnGUI()
    {
        // 步骤 1：这里的 ! 表示取反；当 isTHeld 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!isTHeld) return;

        // 监听 Inspector 颜色变化，若改变则重新生成样式
        // 步骤 2：这里用 || 表示任一条件成立就处理：!stylesInitialized || lastPanelColor != panelBackgroundColor || lastKeyColor != keyBackgroundColor；用于合并几种停止或跳过原因。
        if (!stylesInitialized || lastPanelColor != panelBackgroundColor || lastKeyColor != keyBackgroundColor)
        {
            // 步骤 3：调用 InitStyles：InitStyles。
            InitStyles();
            // 步骤 4：把计算结果 panelBackgroundColor 保存为last面板Color；下一条判断或调用会读取这个具体值。
            lastPanelColor = panelBackgroundColor;
            // 步骤 5：把计算结果 keyBackgroundColor 保存为lastKeyColor；下一条判断或调用会读取这个具体值。
            lastKeyColor = keyBackgroundColor;
            // 步骤 6：把 stylesInitialized 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            stylesInitialized = true;
        }

        // 步骤 7：计算 Screen.height，把结果存进 panelHeight（panelHeight）；之后引用 panelHeight 时用的就是这次结果。
        float panelHeight = Screen.height;
        // 步骤 8：计算 Screen.width - panelWidth，把结果存进 panelX（panelx）；之后引用 panelX 时用的就是这次结果。
        float panelX = Screen.width - panelWidth;
        // 步骤 9：把数值 0f 暂存为 panelY；它只在当前方法里参与后续判断或计算。
        float panelY = 0f;

        // 绘制右侧半透明底板
        // 步骤 10：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "Standard QWERTY Keyboard", panelStyle);

        // 绘制键盘阵列
        // 步骤 11：调用 DrawAuthenticKeyboard：DrawAuthenticKeyboard。
        DrawAuthenticKeyboard(panelX, panelY, panelWidth);
    }
    // ！！！功能结束：OnGUI！！！

    /// <summary>
    /// 销毁旧颜色贴图并生成新的面板、按键样式，避免重复分配累积。
    /// </summary>
    // ！！！功能：InitStyles——InitStyles！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void InitStyles()
    {
        // 清理旧贴图防内存泄漏
        // 步骤 1：确认bg贴图已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (bgTexture != null) Destroy(bgTexture);
        // 步骤 2：确认btn贴图已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (btnTexture != null) Destroy(btnTexture);

        // 生成底板贴图
        // 步骤 3：新建用于bg贴图的对象或数值结构，并把构造参数保存进去。
        bgTexture = new Texture2D(1, 1);
        // 步骤 4：调用 SetPixel：设置Pixel。
        bgTexture.SetPixel(0, 0, panelBackgroundColor);
        // 步骤 5：调用 Apply：应用。
        bgTexture.Apply();

        // 步骤 6：新建用于panelStyle的对象或数值结构，并把构造参数保存进去。
        panelStyle = new GUIStyle(GUI.skin.box)
        {
            // 步骤 7：把计算结果 { background = bgTexture, textColor = fontColor }, 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { background = bgTexture, textColor = fontColor },
            // 步骤 8：把计算结果 18, 保存为fontSize；下一条判断或调用会读取这个具体值。
            fontSize = 18,
            // 步骤 9：把计算结果 FontStyle.Bold, 保存为fontStyle；下一条判断或调用会读取这个具体值。
            fontStyle = FontStyle.Bold,
            // 步骤 10：把计算结果 TextAnchor.UpperCenter, 保存为alignment；下一条判断或调用会读取这个具体值。
            alignment = TextAnchor.UpperCenter,
            // 步骤 11：新建用于padding的对象或数值结构，并把构造参数保存进去。
            padding = new RectOffset(0, 0, 40, 0)
        };

        // 生成按键贴图
        // 步骤 12：新建用于btn贴图的对象或数值结构，并把构造参数保存进去。
        btnTexture = new Texture2D(1, 1);
        // 步骤 13：调用 SetPixel：设置Pixel。
        btnTexture.SetPixel(0, 0, keyBackgroundColor);
        // 步骤 14：调用 Apply：应用。
        btnTexture.Apply();

        // 步骤 15：新建用于buttonStyle的对象或数值结构，并把构造参数保存进去。
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            // 步骤 16：把计算结果 { background = btnTexture, textColor = fontColor }, 保存为normal；下一条判断或调用会读取这个具体值。
            normal = { background = btnTexture, textColor = fontColor },
            // 步骤 17：把计算结果 16 保存为fontSize；下一条判断或调用会读取这个具体值。
            fontSize = 16
        };
    }
    // ！！！功能结束：InitStyles！！！

    /// <summary>
    /// 按预设的五排布局绘制键位，点击时将键名输出到日志。
    /// </summary>
    // ！！！功能：DrawAuthenticKeyboard——DrawAuthenticKeyboard！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float startX, float startY, float width 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawAuthenticKeyboard(float startX, float startY, float width)
    {
        // 步骤 1：把数值 4f 暂存为 spacing；它只在当前方法里参与后续判断或计算。
        float spacing = 4f;          // 按键之间的缝隙
        // 步骤 2：把数值 38f 暂存为 baseKeySize；它只在当前方法里参与后续判断或计算。
        float baseKeySize = 38f;     // 基础按键（字母键）的宽高
        // 步骤 3：把数值 120f 暂存为 startOffsetY；它只在当前方法里参与后续判断或计算。
        float startOffsetY = 120f;   // 距离屏幕顶部的初始高度
        
        // 让整个键盘在面板中水平居中
        // 步骤 4：计算 (14 * baseKeySize) + (13 * spacing)，把结果存进 keyboardTotalWidth（keyboardTotalWidth）；之后引用 keyboardTotalWidth 时用的就是这次结果。
        float keyboardTotalWidth = (14 * baseKeySize) + (13 * spacing); // 第一排的总宽度估算
        // 步骤 5：计算 startX + (width - keyboardTotalWidth) / 2f，把结果存进 startOffsetX（startOffsetx）；之后引用 startOffsetX 时用的就是这次结果。
        float startOffsetX = startX + (width - keyboardTotalWidth) / 2f;

        // 步骤 6：按 int r = 0; r < keyboardLayout.Length; r++ 变化索引；循环体依次处理每个位置。
        for (int r = 0; r < keyboardLayout.Length; r++)
        {
            // 步骤 7：计算 startOffsetX，把结果存进 currentX（currentx）；之后引用 currentX 时用的就是这次结果。
            float currentX = startOffsetX;
            // 步骤 8：计算 startY + startOffsetY + r * (baseKeySize + spacing)，把结果存进 currentY（currenty）；之后引用 currentY 时用的就是这次结果。
            float currentY = startY + startOffsetY + r * (baseKeySize + spacing);

            // 步骤 9：按 int c = 0; c < keyboardLayout[r].Length; c++ 变化索引；循环体依次处理每个位置。
            for (int c = 0; c < keyboardLayout[r].Length; c++)
            {
                // 步骤 10：计算 keyboardLayout[r][c]，把结果存进 key（key）；之后引用 key 时用的就是这次结果。
                string key = keyboardLayout[r][c];
                
                // 根据标准键盘映射每个特殊按键的宽度
                // 步骤 11：计算 GetAuthenticKeyWidth(key, baseKeySize)，把结果存进 keyWidth（keyWidth）；之后引用 keyWidth 时用的就是这次结果。
                float keyWidth = GetAuthenticKeyWidth(key, baseKeySize);

                // 绘制按钮
                // 步骤 12：检查条件 GUI.Button(new Rect(currentX, currentY, keyWidth, baseKeySize), key.Trim(), buttonStyle)；只有为真才运行下面的步骤，否则沿原流程继续。
                if (GUI.Button(new Rect(currentX, currentY, keyWidth, baseKeySize), key.Trim(), buttonStyle))
                {
                    // 步骤 13：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
                    Debug.Log($"<color=cyan>键盘输入: {key.Trim()}</color>");
                }

                // 步骤 14：在currentx原值上增加 keyWidth + spacing；旧状态与本次输入共同决定新状态。
                currentX += keyWidth + spacing;
            }
        }
    }
    // ！！！功能结束：DrawAuthenticKeyboard！！！

    // 获取实体键盘真实比例的按键宽度
    /// <summary>
    /// 给空格、回车等特殊键分配更宽的绘制尺寸。
    /// </summary>
    // ！！！功能：GetAuthenticKeyWidth——获取AuthenticKeyWidth！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string key, float baseWidth 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    private float GetAuthenticKeyWidth(string key, float baseWidth)
    {
        // 步骤 1：查看括号中的当前操作类型，跳到对应 case；其他动作不会一起执行。
        switch (key.Trim())
        {
            // 步骤 2：当操作等于 "Backspace" 时运行这一支；本分支会在 break 处结束。
            case "Backspace": return baseWidth * 2.0f;
            // 步骤 3：当操作等于 "Tab" 时运行这一支；本分支会在 break 处结束。
            case "Tab":       return baseWidth * 1.5f;
            // 步骤 4：当操作等于 "\\" 时运行这一支；本分支会在 break 处结束。
            case "\\":        return baseWidth * 1.5f;
            // 步骤 5：当操作等于 "Caps" 时运行这一支；本分支会在 break 处结束。
            case "Caps":      return baseWidth * 1.75f;
            // 步骤 6：当操作等于 "Enter" 时运行这一支；本分支会在 break 处结束。
            case "Enter":     return baseWidth * 2.25f;
            // 步骤 7：当操作等于 "Shift" 时运行这一支；本分支会在 break 处结束。
            case "Shift":     return baseWidth * 2.25f;
            // 步骤 8：当操作等于 "Ctrl" 时运行这一支；本分支会在 break 处结束。
            case "Ctrl":
            // 步骤 9：当操作等于 "Win" 时运行这一支；本分支会在 break 处结束。
            case "Win":
            // 步骤 10：当操作等于 "Alt" 时运行这一支；本分支会在 break 处结束。
            case "Alt":       return baseWidth * 1.25f;
            // 步骤 11：当操作等于 "Space" 时运行这一支；本分支会在 break 处结束。
            case "Space":     return baseWidth * 6.5f; // 空格键超长宽
            // 步骤 12：没有明确匹配的 case 时执行默认处理，避免状态遗漏。
            default:          return baseWidth;        // 普通字母数字键
        }
    }
    // ！！！功能结束：GetAuthenticKeyWidth！！！
}
