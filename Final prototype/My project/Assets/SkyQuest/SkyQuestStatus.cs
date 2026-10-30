// ！！！文件功能：蜡烛数量界面、提示文字与兑换音效！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.UI 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.UI;

// 定义 SkyQuestStatus 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestStatus : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;
    // message 保存message（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro message;
    // 声明 statusPanel（status面板）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform statusPanel;
    // player 保存player（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion player;
    // 声明 expires（expires）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float expires;
    // confirmation 保存confirmation（类型 AudioSource）；下方方法依靠它记录或查找场景状态。
    private AudioSource confirmation;
    // 声明 currentText（currentText）：文字内容；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private string currentText;
    // candleNumberStyle 保存candleNumberStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle candleNumberStyle;
    // candleHeartStyle 保存candleHeartStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle candleHeartStyle;
    // vrCandlePanel 保存vrCandle面板（类型 RectTransform）；下方方法依靠它记录或查找场景状态。
    private RectTransform vrCandlePanel;
    // vrCandleNumber 保存vrCandleNumber（类型 TextMeshProUGUI）；下方方法依靠它记录或查找场景状态。
    private TextMeshProUGUI vrCandleNumber;
    // 计算 int.MinValue，把结果存进 displayedCandleBalance（displayedCandleBalance）；之后引用 displayedCandleBalance 时用的就是这次结果。
    private int displayedCandleBalance = int.MinValue;
    // candleIcon 保存candleIcon（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private static Texture2D candleIcon;
    // crunchClip 保存crunchClip（类型 AudioClip）；下方方法依靠它记录或查找场景状态。
    private static AudioClip crunchClip;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (head == null) return;
        // 步骤 2：从目标 GameObject 查找并保存 player 组件；后面的移动或显示逻辑会直接使用这个缓存。
        player = GetComponent<SkyQuestLocomotion>();
        // 步骤 3：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 成立时调用 BuildVrCandleCounter：构建VrCandleCounter。
        if (!desktopMode) BuildVrCandleCounter();
        // 步骤 4：创建新的 Transform 并存为 panel；构造参数决定其初始内容或对象名称。
        Transform panel = new GameObject("Sky status panel").transform;
        // 步骤 5：把计算结果 panel 保存为status面板；下一条判断或调用会读取这个具体值。
        statusPanel = panel;
        // 步骤 6：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        panel.SetParent(head, false);
        // 步骤 7：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        panel.localPosition = new Vector3(0f, -0.26f, 1.15f);
        // 步骤 8：把计算结果 SkyQuestVisuals.Text("Status", "", panel, Vector3.zero, 2.4f, 保存为message；下一条判断或调用会读取这个具体值。
        message = SkyQuestVisuals.Text("Status", "", panel, Vector3.zero, 2.4f,
            // 步骤 9：用 new 创建 Color(1f, 0.94f, 0.7f), 1.25f);；构造参数决定新对象的初始内容。
            new Color(1f, 0.94f, 0.7f), 1.25f);
        // 步骤 10：执行 message.rectTransform.sizeDelta = new Vector2(1.25f, 0.28f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        message.rectTransform.sizeDelta = new Vector2(1.25f, 0.28f);
        // 步骤 11：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 message.enabled = false;
        if (desktopMode) message.enabled = false;
        // 步骤 12：把 message.enableWordWrapping 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        message.enableWordWrapping = true;
        // 步骤 13：把计算结果 head.gameObject.AddComponent<AudioSource>() 保存为confirmation；下一条判断或调用会读取这个具体值。
        confirmation = head.gameObject.AddComponent<AudioSource>();
        // 步骤 14：把计算结果 0f 保存为spatialBlend；下一条判断或调用会读取这个具体值。
        confirmation.spatialBlend = 0f;
        // 步骤 15：用 0.25f 更新音源音量；播放器之后以这个强度输出声音。
        confirmation.volume = 0.25f;
        // 步骤 16：定义不可在运行时改变的数值 sampleRate（sampleRate）；后续计算统一使用这个阈值、速度或尺寸。
        const int sampleRate = 22050;
        // 步骤 17：定义不可在运行时改变的数值 samples（samples）；后续计算统一使用这个阈值、速度或尺寸。
        const int samples = 4400;
        // 步骤 18：创建新的 float[] 并存为 data；构造参数决定其初始内容或对象名称。
        float[] data = new float[samples];
        // 步骤 19：按 int i = 0; i < samples; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < samples; i++)
        {
            // 步骤 20：计算 (float)i / sampleRate，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = (float)i / sampleRate;
            // 步骤 21：计算 Mathf.Exp(-14f * t)，把结果存进 envelope（envelope）；之后引用 envelope 时用的就是这次结果。
            float envelope = Mathf.Exp(-14f * t);
            // 步骤 22：执行 data[i] = Mathf.Sin(2f * Mathf.PI * (660f + 550f * t) * t) * envelope * 0.32f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            data[i] = Mathf.Sin(2f * Mathf.PI * (660f + 550f * t) * t) * envelope * 0.32f;
        }
        // 步骤 23：计算 AudioClip.Create("Sky confirmation chime", samples, 1, sampleRate, false)，把结果存进 clip（clip）；之后引用 clip 时用的就是这次结果。
        AudioClip clip = AudioClip.Create("Sky confirmation chime", samples, 1, sampleRate, false);
        // 步骤 24：调用 SetData：设置Data。
        clip.SetData(data, 0);
        // 步骤 25：把计算结果 clip 保存为clip；下一条判断或调用会读取这个具体值。
        confirmation.clip = clip;
        // 步骤 26：从目标物体找到所需组件并存为 guide；使用前应检查是否为 null。
        SkyQuestGuideUI guide = GetComponent<SkyQuestGuideUI>();
        // 步骤 27：如果场景对象没有 SkyQuestGuideUI 组件，就当场创建并保存，保证后续调用有对象可用。
        if (guide == null) guide = gameObject.AddComponent<SkyQuestGuideUI>();
        // 步骤 28：把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        guide.head = head;
        // 步骤 29：把计算结果 desktopMode 保存为desktop模式；下一条判断或调用会读取这个具体值。
        guide.desktopMode = desktopMode;
        // 步骤 30：调用 Show：Show。
        Show(desktopMode ? "WASD move • Arrows look • F near Ancestor • K type" :
            // 步骤 31：执行 "Left stick: move • Right stick: turn • Approach the Ancestor and press X", 8f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Left stick: move • Right stick: turn • Approach the Ancestor and press X", 8f);
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 成立时调用 UpdateVrCandleCounter：刷新 Quest 视野中蜡烛数量的显示。
        if (!desktopMode) UpdateVrCandleCounter();
        // 步骤 2：检查player、status面板是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (!desktopMode && player != null && statusPanel != null)
        {
            // 步骤 3：用三元表达式为 parent 二选一：问号前条件为真取前值，为假取冒号后的值。
            Transform parent = player.ThirdPerson ? player.transform : head;
            // 步骤 4：检查条件 statusPanel.parent != parent；只有为真才运行下面的步骤，否则沿原流程继续。
            if (statusPanel.parent != parent)
            {
                // 步骤 5：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
                statusPanel.SetParent(parent, false);
                // 步骤 6：把计算结果 player.ThirdPerson ? 保存为local位置；下一条判断或调用会读取这个具体值。
                statusPanel.localPosition = player.ThirdPerson ?
                    // 步骤 7：用 new 创建 Vector3(-.25f, 1.95f, 2.15f) : new Vector3(0f, -.26f, 1.15f);；构造参数决定新对象的初始内容。
                    new Vector3(-.25f, 1.95f, 2.15f) : new Vector3(0f, -.26f, 1.15f);
                // 步骤 8：恢复 statusPanel.localRotation 到初始本地朝向；本地表示相对于父对象。
                statusPanel.localRotation = Quaternion.identity;
                // 步骤 9：恢复 statusPanel.localScale 到初始缩放，避免拿取或归位后尺寸变化。
                statusPanel.localScale = Vector3.one * (player.ThirdPerson ? 1.5f : 1f);
            }
        }
        // 步骤 10：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
        if (Time.time > expires)
        {
            // 步骤 11：把计算结果 "" 保存为currentText；下一条判断或调用会读取这个具体值。
            currentText = "";
            // 步骤 12：确认message已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 message.text = "";
            if (message != null) message.text = "";
        }
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnGUI()
    {
        // 步骤 1：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 DrawCandleCounter：在游戏屏幕左上角绘制蜡烛图标与数量。
        if (desktopMode) DrawCandleCounter();
        // 步骤 2：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
        if (desktopMode && !string.IsNullOrEmpty(currentText) && Time.time <= expires)
            // 步骤 3：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            GUI.Box(new Rect(16f, 82f, 490f, 40f), currentText);
    }
    // ！！！功能结束：OnGUI！！！

    // ！！！功能：DrawCandleCounter——在游戏屏幕左上角绘制蜡烛图标与数量！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void DrawCandleCounter()
    {
        // 步骤 1：检查candleIcon是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时调用 CreateCandleIcon：创建CandleIcon。
        if (candleIcon == null) CreateCandleIcon();
        // 步骤 2：检查candleNumberStyle是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (candleNumberStyle == null)
        {
            // 步骤 3：新建用于candleNumberStyle的对象或数值结构，并把构造参数保存进去。
            candleNumberStyle = new GUIStyle(GUI.skin.label)
            {
                // 步骤 4：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
                fontSize = Mathf.Clamp(Screen.width / 62, 20, 30),
                // 步骤 5：把计算结果 TextAnchor.MiddleLeft 保存为alignment；下一条判断或调用会读取这个具体值。
                alignment = TextAnchor.MiddleLeft
            };
            // 步骤 6：执行 candleNumberStyle.normal.textColor = new Color(1f, .97f, .88f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            candleNumberStyle.normal.textColor = new Color(1f, .97f, .88f);
            // 步骤 7：新建用于candleHeartStyle的对象或数值结构，并把构造参数保存进去。
            candleHeartStyle = new GUIStyle(candleNumberStyle)
            {
                // 步骤 8：计算fontSize并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
                fontSize = Mathf.Clamp(Screen.width / 82, 16, 22),
                // 步骤 9：把计算结果 TextAnchor.MiddleCenter 保存为alignment；下一条判断或调用会读取这个具体值。
                alignment = TextAnchor.MiddleCenter
            };
            // 步骤 10：执行 candleHeartStyle.normal.textColor = new Color(.23f, .21f, .18f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            candleHeartStyle.normal.textColor = new Color(.23f, .21f, .18f);
        }

        // 步骤 11：计算 GUI.color，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        Color previous = GUI.color;
        // 步骤 12：计算 Screen.width * .5f - 62f，把结果存进 left（left）；之后引用 left 时用的就是这次结果。
        float left = Screen.width * .5f - 62f;
        // 步骤 13：创建新的 Rect 并存为 iconRect；构造参数决定其初始内容或对象名称。
        Rect iconRect = new Rect(left, 17f, 32f, 48f);
        // 步骤 14：把计算结果 Color.white 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = Color.white;
        // 步骤 15：调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(iconRect, candleIcon, ScaleMode.StretchToFill, true);
        // 步骤 16：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(iconRect.x + 1f, iconRect.y + 10f, iconRect.width - 2f, 27f), "♥", candleHeartStyle);
        // 步骤 17：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = new Color(1f, .97f, .88f, 1f);
        // 步骤 18：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(left + 37f, 25f, 90f, 34f), SkyQuestCandleWallet.Balance.ToString(), candleNumberStyle);
        // 步骤 19：把计算结果 previous 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = previous;
    }
    // ！！！功能结束：DrawCandleCounter！！！

    // ！！！功能：BuildVrCandleCounter——构建VrCandleCounter！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BuildVrCandleCounter()
    {
        // 步骤 1：检查candleIcon是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时调用 CreateCandleIcon：创建CandleIcon。
        if (candleIcon == null) CreateCandleIcon();
        // 步骤 2：新建用于root的对象或数值结构，并把构造参数保存进去。
        GameObject root = new GameObject("Top-center candle counter",
            // 步骤 3：调用 typeof：typeof。
            typeof(RectTransform), typeof(Canvas));
        // 步骤 4：从目标 GameObject 查找并保存 vrCandle面板 组件；后面的移动或显示逻辑会直接使用这个缓存。
        vrCandlePanel = root.GetComponent<RectTransform>();
        // 步骤 5：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        vrCandlePanel.SetParent(head, false);
        // 步骤 6：新建用于sizeDelta的对象或数值结构，并把构造参数保存进去。
        vrCandlePanel.sizeDelta = new Vector2(218f, 76f);
        // 步骤 7：从目标物体找到所需组件并存为 canvas；使用前应检查是否为 null。
        Canvas canvas = root.GetComponent<Canvas>();
        // 步骤 8：把计算结果 RenderMode.WorldSpace 保存为render模式；下一条判断或调用会读取这个具体值。
        canvas.renderMode = RenderMode.WorldSpace;
        // 步骤 9：从目标 GameObject 查找并保存 world相机 组件；后面的移动或显示逻辑会直接使用这个缓存。
        canvas.worldCamera = head.GetComponent<Camera>();
        // 步骤 10：把 canvas.overrideSorting 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        canvas.overrideSorting = true;
        // 步骤 11：把计算结果 95 保存为sortingOrder；下一条判断或调用会读取这个具体值。
        canvas.sortingOrder = 95;

        // 步骤 12：新建用于plate的对象或数值结构，并把构造参数保存进去。
        GameObject plate = new GameObject("Subtle counter backing",
            // 步骤 13：调用 typeof：typeof。
            typeof(RectTransform), typeof(Image));
        // 步骤 14：从目标物体找到所需组件并存为 plateRect；使用前应检查是否为 null。
        RectTransform plateRect = plate.GetComponent<RectTransform>();
        // 步骤 15：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        plateRect.SetParent(vrCandlePanel, false);
        // 步骤 16：把 plateRect.anchorMin 清为零向量；清除上次移动、投掷或位置偏移的残留值。
        plateRect.anchorMin = Vector2.zero;
        // 步骤 17：把计算结果 Vector2.one 保存为anchorMax；下一条判断或调用会读取这个具体值。
        plateRect.anchorMax = Vector2.one;
        // 步骤 18：把计算结果 plateRect.offsetMax = Vector2.zero 保存为offsetMin；下一条判断或调用会读取这个具体值。
        plateRect.offsetMin = plateRect.offsetMax = Vector2.zero;
        // 步骤 19：从目标物体找到所需组件并存为 backing；使用前应检查是否为 null。
        Image backing = plate.GetComponent<Image>();
        // 步骤 20：新建用于color的对象或数值结构，并把构造参数保存进去。
        backing.color = new Color(.015f, .025f, .045f, .55f);
        // 步骤 21：把 backing.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        backing.raycastTarget = false;

        // 步骤 22：新建用于candle的对象或数值结构，并把构造参数保存进去。
        GameObject candle = new GameObject("Candle symbol",
            // 步骤 23：调用 typeof：typeof。
            typeof(RectTransform), typeof(RawImage));
        // 步骤 24：从目标物体找到所需组件并存为 candleRect；使用前应检查是否为 null。
        RectTransform candleRect = candle.GetComponent<RectTransform>();
        // 步骤 25：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        candleRect.SetParent(vrCandlePanel, false);
        // 步骤 26：新建用于anchorMin的对象或数值结构，并把构造参数保存进去。
        candleRect.anchorMin = candleRect.anchorMax = new Vector2(.5f, .5f);
        // 步骤 27：新建用于sizeDelta的对象或数值结构，并把构造参数保存进去。
        candleRect.sizeDelta = new Vector2(32f, 48f);
        // 步骤 28：新建用于anchored位置的对象或数值结构，并把构造参数保存进去。
        candleRect.anchoredPosition = new Vector2(-55f, 0f);
        // 步骤 29：从目标物体找到所需组件并存为 symbol；使用前应检查是否为 null。
        RawImage symbol = candle.GetComponent<RawImage>();
        // 步骤 30：把 candleIcon 写入texture；TextMeshPro 会把新文字显示给玩家。
        symbol.texture = candleIcon;
        // 步骤 31：把 symbol.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        symbol.raycastTarget = false;

        // 步骤 32：新建用于count的对象或数值结构，并把构造参数保存进去。
        GameObject count = new GameObject("Candles remaining",
            // 步骤 33：调用 typeof：typeof。
            typeof(RectTransform), typeof(TextMeshProUGUI));
        // 步骤 34：从目标物体找到所需组件并存为 countRect；使用前应检查是否为 null。
        RectTransform countRect = count.GetComponent<RectTransform>();
        // 步骤 35：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        countRect.SetParent(vrCandlePanel, false);
        // 步骤 36：新建用于anchorMin的对象或数值结构，并把构造参数保存进去。
        countRect.anchorMin = countRect.anchorMax = new Vector2(.5f, .5f);
        // 步骤 37：新建用于sizeDelta的对象或数值结构，并把构造参数保存进去。
        countRect.sizeDelta = new Vector2(116f, 62f);
        // 步骤 38：新建用于anchored位置的对象或数值结构，并把构造参数保存进去。
        countRect.anchoredPosition = new Vector2(34f, 0f);
        // 步骤 39：从目标 GameObject 查找并保存 vrCandleNumber 组件；后面的移动或显示逻辑会直接使用这个缓存。
        vrCandleNumber = count.GetComponent<TextMeshProUGUI>();
        // 步骤 40：把计算结果 42f 保存为fontSize；下一条判断或调用会读取这个具体值。
        vrCandleNumber.fontSize = 42f;
        // 步骤 41：把计算结果 TextAlignmentOptions.MidlineLeft 保存为alignment；下一条判断或调用会读取这个具体值。
        vrCandleNumber.alignment = TextAlignmentOptions.MidlineLeft;
        // 步骤 42：新建用于color的对象或数值结构，并把构造参数保存进去。
        vrCandleNumber.color = new Color(1f, .97f, .86f);
        // 步骤 43：把 vrCandleNumber.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        vrCandleNumber.raycastTarget = false;
        // 步骤 44：调用 UpdateVrCandleCounter：刷新 Quest 视野中蜡烛数量的显示。
        UpdateVrCandleCounter();
    }
    // ！！！功能结束：BuildVrCandleCounter！！！

    // ！！！功能：UpdateVrCandleCounter——刷新 Quest 视野中蜡烛数量的显示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateVrCandleCounter()
    {
        // 步骤 1：检查vrCandle面板、头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (vrCandlePanel == null || head == null) return;
        // 步骤 2：用 thirdPerson 记住此处真假结果（player != null && player.ThirdPerson && player.avatar != null），供下一条 if 选择执行路径。
        bool thirdPerson = player != null && player.ThirdPerson && player.avatar != null;
        // 步骤 3：用三元表达式为 anchor 二选一：问号前条件为真取前值，为假取冒号后的值。
        Transform anchor = thirdPerson ? player.avatar.transform : head;
        // 步骤 4：检查条件 vrCandlePanel.parent != anchor；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        if (vrCandlePanel.parent != anchor) vrCandlePanel.SetParent(anchor, false);
        // 步骤 5：检查条件 thirdPerson；只有为真才运行下面的步骤，否则沿原流程继续。
        if (thirdPerson)
        {
            // Keep the third-person counter with the avatar, not the turning headset.
            // 步骤 6：把position更新为 player.avatar.transform.position + Vector3.up * 2.8f；世界或局部位置会立即影响物体摆放。
            vrCandlePanel.position = player.avatar.transform.position + Vector3.up * 2.8f;
            // 步骤 7：把rotation更新为 Quaternion.LookRotation(；物体朝向会在本帧改变。
            vrCandlePanel.rotation = Quaternion.LookRotation(
                // 步骤 8：执行 vrCandlePanel.position - head.position, Vector3.up);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                vrCandlePanel.position - head.position, Vector3.up);
            // 步骤 9：恢复 vrCandlePanel.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            vrCandlePanel.localScale = Vector3.one * .004f;
        }
        // 步骤 10：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 11：新建用于local位置的对象或数值结构，并把构造参数保存进去。
            vrCandlePanel.localPosition = new Vector3(0f, .54f, 1.45f);
            // 步骤 12：恢复 vrCandlePanel.localRotation 到初始本地朝向；本地表示相对于父对象。
            vrCandlePanel.localRotation = Quaternion.identity;
            // 步骤 13：恢复 vrCandlePanel.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            vrCandlePanel.localScale = Vector3.one * .00165f;
        }
        // 步骤 14：计算 SkyQuestCandleWallet.Balance，把结果存进 balance（balance）；之后引用 balance 时用的就是这次结果。
        int balance = SkyQuestCandleWallet.Balance;
        // 步骤 15：检查vrCandleNumber是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (vrCandleNumber != null && balance != displayedCandleBalance)
        {
            // 步骤 16：把新内容写入 vrCandleNumber.text；玩家看到的文字会在本帧更新。
            vrCandleNumber.text = balance.ToString();
            // 步骤 17：把计算结果 balance 保存为displayedCandleBalance；下一条判断或调用会读取这个具体值。
            displayedCandleBalance = balance;
        }
    }
    // ！！！功能结束：UpdateVrCandleCounter！！！

    // ！！！功能：CreateCandleIcon——创建CandleIcon！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void CreateCandleIcon()
    {
        // 步骤 1：定义不可在运行时改变的数值 width（width）；后续计算统一使用这个阈值、速度或尺寸。
        const int width = 32;
        // 步骤 2：定义不可在运行时改变的数值 height（height）；后续计算统一使用这个阈值、速度或尺寸。
        const int height = 48;
        // 步骤 3：新建用于candleIcon的对象或数值结构，并把构造参数保存进去。
        candleIcon = new Texture2D(width, height, TextureFormat.RGBA32, false);
        // 步骤 4：把计算结果 HideFlags.HideAndDontSave 保存为hideFlags；下一条判断或调用会读取这个具体值。
        candleIcon.hideFlags = HideFlags.HideAndDontSave;
        // 步骤 5：把计算结果 FilterMode.Bilinear 保存为filter模式；下一条判断或调用会读取这个具体值。
        candleIcon.filterMode = FilterMode.Bilinear;
        // 步骤 6：创建新的 Color 并存为 clear；构造参数决定其初始内容或对象名称。
        Color clear = new Color(1f, 1f, 1f, 0f);
        // 步骤 7：创建新的 Color 并存为 wax；构造参数决定其初始内容或对象名称。
        Color wax = new Color(1f, .97f, .87f, 1f);
        // 步骤 8：创建新的 Color 并存为 highlight；构造参数决定其初始内容或对象名称。
        Color highlight = new Color(1f, 1f, .96f, 1f);
        // 步骤 9：创建新的 Color[] 并存为 pixels；构造参数决定其初始内容或对象名称。
        Color[] pixels = new Color[width * height];
        // 步骤 10：按 int i = 0; i < pixels.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
        // 步骤 11：按 int y = 0; y < height; y++ 变化索引；循环体依次处理每个位置。
        for (int y = 0; y < height; y++)
        {
            // 步骤 12：按 int x = 0; x < width; x++ 变化索引；循环体依次处理每个位置。
            for (int x = 0; x < width; x++)
            {
                // 步骤 13：用 wick 记住此处真假结果（x >= 15 && x <= 16 && y >= 41 && y <= 47），供下一条 if 选择执行路径。
                bool wick = x >= 15 && x <= 16 && y >= 41 && y <= 47;
                // 步骤 14：用 body 记住此处真假结果（y >= 2 && y <= 42 && x >= 4 && x <= 27），供下一条 if 选择执行路径。
                bool body = y >= 2 && y <= 42 && x >= 4 && x <= 27;
                // 步骤 15：这里用 && 要求两边条件同时成立：body && y > 37 && (x < 7 || x > 24)；任意一边为假就跳过本分支。 成立时执行 body = false;
                if (body && y > 37 && (x < 7 || x > 24)) body = false;
                // 步骤 16：这里用 && 要求两边条件同时成立：body && y < 7 && (x < 7 || x > 24)；任意一边为假就跳过本分支。 成立时执行 body = false;
                if (body && y < 7 && (x < 7 || x > 24)) body = false;
                // 步骤 17：这里用 && 要求两边条件同时成立：!body && !wick；任意一边为假就跳过本分支。 成立时执行 continue;
                if (!body && !wick) continue;
                // 步骤 18：执行 pixels[y * width + x] = body && x == 6 ? highlight : wax;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                pixels[y * width + x] = body && x == 6 ? highlight : wax;
            }
        }
        // 步骤 19：调用 SetPixels：设置Pixels。
        candleIcon.SetPixels(pixels);
        // 步骤 20：调用 Apply：应用。
        candleIcon.Apply(false, true);
    }
    // ！！！功能结束：CreateCandleIcon！！！

    // ！！！功能：Show——Show！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string text, float seconds = 3f 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Show(string text, float seconds = 3f)
    {
        // 步骤 1：把计算结果 text 保存为currentText；下一条判断或调用会读取这个具体值。
        currentText = text;
        // 步骤 2：确认message已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 message.text = text;
        if (message != null) message.text = text;
        // 步骤 3：根据当前游戏时间计算并保存expires，用于后续延迟或冷却判断。
        expires = Time.time + seconds;
    }
    // ！！！功能结束：Show！！！

    // ！！！功能：PlayConfirm——PlayConfirm！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void PlayConfirm()
    {
        // 步骤 1：确认confirmation已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 PlayOneShot：PlayOneShot。
        if (confirmation != null) confirmation.PlayOneShot(confirmation.clip);
    }
    // ！！！功能结束：PlayConfirm！！！

    // ！！！功能：PlayCrunch——PlayCrunch！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void PlayCrunch()
    {
        // 步骤 1：检查confirmation是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (confirmation == null) return;
        // 步骤 2：检查crunchClip是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时调用 CreateCrunchClip：创建CrunchClip。
        if (crunchClip == null) CreateCrunchClip();
        // 步骤 3：调用 PlayOneShot：PlayOneShot。
        confirmation.PlayOneShot(crunchClip, 1.7f);
    }
    // ！！！功能结束：PlayCrunch！！！

    // ！！！功能：CreateCrunchClip——创建CrunchClip！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void CreateCrunchClip()
    {
        // 步骤 1：定义不可在运行时改变的数值 rate（rate）；后续计算统一使用这个阈值、速度或尺寸。
        const int rate = 22050;
        // 步骤 2：定义不可在运行时改变的数值 count（count）；后续计算统一使用这个阈值、速度或尺寸。
        const int count = 17640;
        // 步骤 3：创建新的 float[] 并存为 data；构造参数决定其初始内容或对象名称。
        float[] data = new float[count];
        // 步骤 4：创建新的 System.Random 并存为 random；构造参数决定其初始内容或对象名称。
        System.Random random = new System.Random(4271);
        // 步骤 5：把数值 0f 暂存为 filtered；它只在当前方法里参与后续判断或计算。
        float filtered = 0f;
        // 步骤 6：按 int i = 0; i < count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < count; i++)
        {
            // 步骤 7：计算 i / (float)rate，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = i / (float)rate;
            // 步骤 8：计算first并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            float first = Mathf.Clamp01((t - .11f) / .015f) *
                // 步骤 9：调用 Exp：Exp。
                Mathf.Exp(-19f * Mathf.Max(0f, t - .13f));
            // 步骤 10：计算second并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            float second = Mathf.Clamp01((t - .41f) / .012f) *
                // 步骤 11：调用 Exp：Exp。
                Mathf.Exp(-17f * Mathf.Max(0f, t - .43f));
            // 步骤 12：检查条件 t < .11f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 first = 0f;
            if (t < .11f) first = 0f;
            // 步骤 13：检查条件 t < .41f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 second = 0f;
            if (t < .41f) second = 0f;
            // 步骤 14：计算 (float)random.NextDouble() * 2f - 1f，把结果存进 noise（noise）；之后引用 noise 时用的就是这次结果。
            float noise = (float)random.NextDouble() * 2f - 1f;
            // 步骤 15：把计算结果 Mathf.Lerp(filtered, noise, .65f) 保存为filtered；下一条判断或调用会读取这个具体值。
            filtered = Mathf.Lerp(filtered, noise, .65f);
            // 步骤 16：计算 Mathf.Sin(2f * Mathf.PI * 173f * t) * .18f，把结果存进 grain（grain）；之后引用 grain 时用的就是这次结果。
            float grain = Mathf.Sin(2f * Mathf.PI * 173f * t) * .18f;
            // 步骤 17：这一行补充 data[i] = Mathf.Clamp((filtered * .75f + grain) *，与相邻行合起来完成同一次计算或对象配置。
            data[i] = Mathf.Clamp((filtered * .75f + grain) *
                // 步骤 18：执行 (first + second) * .65f, -.9f, .9f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                (first + second) * .65f, -.9f, .9f);
        }
        // 步骤 19：把计算结果 AudioClip.Create("Two soft crunchy bites", count, 1, rate, false) 保存为crunchClip；下一条判断或调用会读取这个具体值。
        crunchClip = AudioClip.Create("Two soft crunchy bites", count, 1, rate, false);
        // 步骤 20：调用 SetData：设置Data。
        crunchClip.SetData(data, 0);
    }
    // ！！！功能结束：CreateCrunchClip！！！
}
