// ！！！文件功能：鼠标或手柄停留三秒后显示兑换物说明！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

// Describes physical exchange rewards after the pointer rests on one for three seconds.
// 定义 SkyQuestItemHoverInfo 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class SkyQuestItemHoverInfo : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 rightHand（右手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform rightHand;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;

    // current 保存current（类型 SkyQuestMagicPickup）；下方方法依靠它记录或查找场景状态。
    private SkyQuestMagicPickup current;
    // 声明 hoverStarted（hoverStarted）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float hoverStarted;
    // 把数值 -10f 暂存为 lastControllerAimTime；它只在当前方法里参与后续判断或计算。
    private float lastControllerAimTime = -10f;
    // 声明 screenPoint（screenPoint）：二维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector2 screenPoint;
    // tooltipStyle 保存tooltipStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle tooltipStyle;
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion locomotion;

    // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
    private void Awake() { locomotion = GetComponent<SkyQuestLocomotion>(); }

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Update()
    {
        // 步骤 1：从目标物体找到所需组件并存为 exchange；使用前应检查是否为 null。
        SkyQuestMagicExchange exchange = GetComponent<SkyQuestMagicExchange>();
        // 步骤 2：从目标物体找到所需组件并存为 ancestor；使用前应检查是否为 null。
        SkyQuestAncestor ancestor = GetComponent<SkyQuestAncestor>();
        // 步骤 3：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if ((exchange != null && exchange.IsMenuOpen) ||
            // 步骤 4：这一行补充 (ancestor != null && ancestor.IsMenuOpen)) { SetTarget(null); return; }，与相邻行合起来完成同一次计算或对象配置。
            (ancestor != null && ancestor.IsMenuOpen)) { SetTarget(null); return; }
        // 步骤 5：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 { SetTarget(null); return; }
        if (head == null) { SetTarget(null); return; }

        // 步骤 6：从目标物体找到所需组件并存为 camera；使用前应检查是否为 null。
        Camera camera = head.GetComponent<Camera>();
        // 步骤 7：声明 ray（ray）：三维射线；只供本脚本内部保存状态，赋值前是该类型的默认值。
        Ray ray;
        // 步骤 8：检查camera是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (desktopMode && camera != null)
        {
            // 步骤 9：检查Gamepad的current是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > .01f)
                // 步骤 10：把计算结果 Time.unscaledTime 保存为last控制器AimTime；下一条判断或调用会读取这个具体值。
                lastControllerAimTime = Time.unscaledTime;
            // 步骤 11：把计算结果 Gamepad.current != null && 保存为use控制器Cursor；下一条判断或调用会读取这个具体值。
            bool useControllerCursor = Gamepad.current != null &&
                // 步骤 12：执行 Time.unscaledTime - lastControllerAimTime <= 4f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Time.unscaledTime - lastControllerAimTime <= 4f;
            // 步骤 13：把计算结果 useControllerCursor || Mouse.current == null ? 保存为screenPoint；下一条判断或调用会读取这个具体值。
            screenPoint = useControllerCursor || Mouse.current == null ?
                // 步骤 14：用 new 创建 Vector2(Screen.width * .5f, Screen.height * .5f) :；构造参数决定新对象的初始内容。
                new Vector2(Screen.width * .5f, Screen.height * .5f) :
                // 步骤 15：执行 Mouse.current.position.ReadValue();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Mouse.current.position.ReadValue();
            // 步骤 16：把计算结果 camera.ScreenPointToRay(screenPoint) 保存为ray；下一条判断或调用会读取这个具体值。
            ray = camera.ScreenPointToRay(screenPoint);
        }
        // 步骤 17：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。确认右手柄位置已经有值；只有存在时才能安全读取它的属性或调用方法。
        else if (rightHand != null)
        {
            // 步骤 18：新建用于ray的对象或数值结构，并把构造参数保存进去。
            ray = new Ray(rightHand.position, rightHand.forward);
            // 步骤 19：新建用于screenPoint的对象或数值结构，并把构造参数保存进去。
            screenPoint = new Vector2(Screen.width * .5f, Screen.height * .5f);
        }
        // 步骤 20：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。确认camera已经有值；只有存在时才能安全读取它的属性或调用方法。
        else if (camera != null)
        {
            // 步骤 21：新建用于screenPoint的对象或数值结构，并把构造参数保存进去。
            screenPoint = new Vector2(Screen.width * .5f, Screen.height * .5f);
            // 步骤 22：新建用于ray的对象或数值结构，并把构造参数保存进去。
            ray = camera.ViewportPointToRay(new Vector3(.5f, .5f));
        }
        // 步骤 23：前面的 if 条件为假时执行这里的备用处理。
        else { SetTarget(null); return; }

        // 步骤 24：计算 null，把结果存进 nearest（nearest）；之后引用 nearest 时用的就是这次结果。
        SkyQuestMagicPickup nearest = null;
        // 步骤 25：把数值 12f 暂存为 best；它只在当前方法里参与后续判断或计算。
        float best = 12f;
        // 步骤 26：逐个读取 RaycastHit hit in Physics.RaycastAll(ray, 12f, ~0, QueryTriggerInteraction.Ignore) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in Physics.RaycastAll(ray, 12f, ~0, QueryTriggerInteraction.Ignore))
        {
            // 步骤 27：从目标物体找到所需组件并存为 item；使用前应检查是否为 null。
            SkyQuestMagicPickup item = hit.collider.GetComponentInParent<SkyQuestMagicPickup>();
            // 步骤 28：检查item是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (item == null || item.held || hit.distance >= best) continue;
            // 步骤 29：把计算结果 hit.distance 保存为best；下一条判断或调用会读取这个具体值。
            best = hit.distance;
            // 步骤 30：把计算结果 item 保存为nearest；下一条判断或调用会读取这个具体值。
            nearest = item;
        }
        // 步骤 31：调用 SetTarget：设置当前指向的兑换物并重启三秒悬停计时。
        SetTarget(nearest);
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：SetTarget——设置当前指向的兑换物并重启三秒悬停计时！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 SkyQuestMagicPickup target 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetTarget(SkyQuestMagicPickup target)
    {
        // 步骤 1：检查条件 target == current；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (target == current) return;
        // 步骤 2：把计算结果 target 保存为current；下一条判断或调用会读取这个具体值。
        current = target;
        // 步骤 3：把计算结果 Time.unscaledTime 保存为hoverStarted；下一条判断或调用会读取这个具体值。
        hoverStarted = Time.unscaledTime;
    }
    // ！！！功能结束：SetTarget！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnGUI()
    {
        // Quest already shows the same hint as a world-space panel above the
        // selected item; a second screen-space label would obscure the view.
        // 步骤 1：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!desktopMode) return;
        // 步骤 2：检查current是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (current == null) return;
        // 步骤 3：检查tooltipStyle是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (tooltipStyle == null)
        {
            // 步骤 4：新建用于tooltipStyle的对象或数值结构，并把构造参数保存进去。
            tooltipStyle = new GUIStyle(GUI.skin.label)
            {
                // 步骤 5：把计算结果 TextAnchor.MiddleCenter, fontSize = 16, 保存为alignment；下一条判断或调用会读取这个具体值。
                alignment = TextAnchor.MiddleCenter, fontSize = 16,
                // 步骤 6：把计算结果 FontStyle.Bold, wordWrap = true 保存为fontStyle；下一条判断或调用会读取这个具体值。
                fontStyle = FontStyle.Bold, wordWrap = true
            };
            // 步骤 7：执行 tooltipStyle.normal.textColor = Color.black;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            tooltipStyle.normal.textColor = Color.black;
        }
        // 步骤 8：把计算结果 current.displayPottery ? 保存为message；下一条判断或调用会读取这个具体值。
        string message = current.displayPottery ?
            // 步骤 9：这一行补充 (desktopMode ? "Z: PICK UP / DROP  ·  HOLD Q, RELEASE: THROW" :，与相邻行合起来完成同一次计算或对象配置。
            (desktopMode ? "Z: PICK UP / DROP  ·  HOLD Q, RELEASE: THROW" :
                // 步骤 10：这一行补充 "HOLD RIGHT TRIGGER: AIM  ·  RELEASE: THROW") :，与相邻行合起来完成同一次计算或对象配置。
                "HOLD RIGHT TRIGGER: AIM  ·  RELEASE: THROW") :
            // 步骤 11：这一行补充 desktopMode ?，与相邻行合起来完成同一次计算或对象配置。
            desktopMode ?
                // 步骤 12：这一行补充 (current.restoresCapeEnergy ? "E: EAT +3 FLY ENERGY" :，与相邻行合起来完成同一次计算或对象配置。
                (current.restoresCapeEnergy ? "E: EAT +3 FLY ENERGY" :
                    // 步骤 13：这一行补充 "E: EAT +5 CANDLES") +，与相邻行合起来完成同一次计算或对象配置。
                    "E: EAT +5 CANDLES") +
                // 步骤 14：这一行补充 "  ·  Z: PICK UP / DROP  ·  HOLD Q, RELEASE: " +，与相邻行合起来完成同一次计算或对象配置。
                "  ·  Z: PICK UP / DROP  ·  HOLD Q, RELEASE: " +
                // 步骤 15：这一行补充 (current.placeable ? "PLACE" : "THROW") :，与相邻行合起来完成同一次计算或对象配置。
                (current.placeable ? "PLACE" : "THROW") :
                // 步骤 16：这一行补充 (current.restoresCapeEnergy ? "+3 FLY ENERGY  ·  " : "+5 CANDLES  ·  ") +，与相邻行合起来完成同一次计算或对象配置。
                (current.restoresCapeEnergy ? "+3 FLY ENERGY  ·  " : "+5 CANDLES  ·  ") +
                // 步骤 17：执行 "HOLD RIGHT TRIGGER: AIM  ·  RELEASE: THROW  ·  NEAR FACE + RIGHT TRIGGER: EAT";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "HOLD RIGHT TRIGGER: AIM  ·  RELEASE: THROW  ·  NEAR FACE + RIGHT TRIGGER: EAT";
        // 步骤 18：用三元表达式为 width 二选一：问号前条件为真取前值，为假取冒号后的值。
        float width = desktopMode ? 384f : 480f;
        // 步骤 19：用三元表达式为 height 二选一：问号前条件为真取前值，为假取冒号后的值。
        float height = desktopMode ? 48f : 76f;
        // 步骤 20：计算 x 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float x = Mathf.Clamp(screenPoint.x - width * .5f, 12f, Screen.width - width - 12f);
        // 步骤 21：计算 y 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float y = Mathf.Clamp(Screen.height - screenPoint.y + 28f, 12f, Screen.height - height - 12f);
        // 步骤 22：创建新的 Rect 并存为 panel；构造参数决定其初始内容或对象名称。
        Rect panel = new Rect(x, y, width, height);
        // 步骤 23：计算 GUI.color，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        Color previous = GUI.color;
        // 步骤 24：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = new Color(1f, .9f, .12f, .95f);
        // 步骤 25：调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        // 步骤 26：把计算结果 previous 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = previous;
        // 步骤 27：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(panel, message, tooltipStyle);
    }
    // ！！！功能结束：OnGUI！！！
}
