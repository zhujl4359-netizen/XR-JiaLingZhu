// ！！！文件功能：在桌面场景用键盘鼠标驱动 Quest 对应功能！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

// Keyboard-only controls for the separate desktop scene. Quest controls remain untouched.
// 定义 SkyQuestDesktopControls 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestDesktopControls : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    public SkyQuestLocomotion locomotion;
    // ancestor 保存ancestor（类型 SkyQuestAncestor）；下方方法依靠它记录或查找场景状态。
    public SkyQuestAncestor ancestor;
    // magicExchange 保存magicExchange（类型 SkyQuestMagicExchange）；下方方法依靠它记录或查找场景状态。
    public SkyQuestMagicExchange magicExchange;
    // social 保存social（类型 SkyQuestSocial）；下方方法依靠它记录或查找场景状态。
    public SkyQuestSocial social;
    // props 保存props（类型 SkyQuestProps）；下方方法依靠它记录或查找场景状态。
    public SkyQuestProps props;
    // communication 保存communication（类型 SkyQuestCommunication）；下方方法依靠它记录或查找场景状态。
    public SkyQuestCommunication communication;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    public SkyQuestStatus status;
    // 把 Inspector 可调整的 mouseSensitivity（mouseSensitivity）设为默认值 0.12f；具体用途看它在下方的判断和计算。
    public float mouseSensitivity = 0.12f;

    // 声明 yaw（yaw）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float yaw;
    // 声明 pitch（pitch）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float pitch;
    // 声明 exchangeWasOpen（exchangeWasOpen）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool exchangeWasOpen;
    // 声明 previousMousePosition（previousMouse位置）：二维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector2 previousMousePosition;
    // 声明 hasMousePosition（hasMouse位置）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool hasMousePosition;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：确认头显相机已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (head != null)
        {
            // 步骤 2：把计算结果 head.localEulerAngles.y 保存为yaw；下一条判断或调用会读取这个具体值。
            yaw = head.localEulerAngles.y;
            // 步骤 3：把计算结果 Mathf.DeltaAngle(0f, head.localEulerAngles.x) 保存为pitch；下一条判断或调用会读取这个具体值。
            pitch = Mathf.DeltaAngle(0f, head.localEulerAngles.x);
        }
        // 步骤 4：执行 status?.Show("WASD: move   Mouse: look   F: ancestor tree   Z: pick up/drop   Hold Q: aim/launch", 1；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("WASD: move   Mouse: look   F: ancestor tree   Z: pick up/drop   Hold Q: aim/launch", 10f);
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：计算 Keyboard.current，把结果存进 keyboard（keyboard）；之后引用 keyboard 时用的就是这次结果。
        Keyboard keyboard = Keyboard.current;
        // 步骤 2：检查keyboard、locomotion、头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (keyboard == null || locomotion == null || head == null) return;

        // 步骤 3：检查communication是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (communication != null && communication.IsOpen)
        {
            // 步骤 4：计算 Mouse.current，把结果存进 chatMouse（chatMouse）；之后引用 chatMouse 时用的就是这次结果。
            Mouse chatMouse = Mouse.current;
            // 步骤 5：确认chatMouse已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (chatMouse != null)
            {
                // 步骤 6：计算 ReadMouseMovement(chatMouse)，把结果存进 look（look）；之后引用 look 时用的就是这次结果。
                Vector2 look = ReadMouseMovement(chatMouse);
                // 步骤 7：在yaw原值上增加 look.x * mouseSensitivity；旧状态与本次输入共同决定新状态。
                yaw += look.x * mouseSensitivity;
                // 步骤 8：计算pitch并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
                pitch = Mathf.Clamp(pitch - look.y * mouseSensitivity, -60f, 60f);
                // 步骤 9：恢复 head.localRotation 到初始本地朝向；本地表示相对于父对象。
                head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            // 步骤 10：新建用于chatMove的对象或数值结构，并把构造参数保存进去。
            Vector2 chatMove = new Vector2(
                // 步骤 11：继续填写上一行调用的参数：(keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f)；参数位置决定它表示的坐标、速度或选项。
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                // 步骤 12：执行 (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
            // 步骤 13：计算 Gamepad.current，把结果存进 chatGamepad（chatGamepad）；之后引用 chatGamepad 时用的就是这次结果。
            Gamepad chatGamepad = Gamepad.current;
            // 步骤 14：检查chatGamepad是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (chatGamepad != null && chatGamepad.leftStick.ReadValue().sqrMagnitude > chatMove.sqrMagnitude)
                // 步骤 15：把计算结果 chatGamepad.leftStick.ReadValue() 保存为chatMove；下一条判断或调用会读取这个具体值。
                chatMove = chatGamepad.leftStick.ReadValue();
            // 步骤 16：调用 DesktopMove：把键盘方向转换成水平移动并处理桌面碰撞。
            locomotion.DesktopMove(chatMove, keyboard.spaceKey.isPressed, keyboard.leftCtrlKey.isPressed,
                // 步骤 17：执行 keyboard.spaceKey.wasPressedThisFrame);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                keyboard.spaceKey.wasPressedThisFrame);
            // 步骤 18：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopToggle：桌面Toggle。
            if (keyboard.escapeKey.wasPressedThisFrame) communication.DesktopToggle();
            // 步骤 19：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopDelete：桌面Delete。
            if (keyboard.backspaceKey.wasPressedThisFrame) communication.DesktopDelete();
            // 步骤 20：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSend：桌面Send。
            if (keyboard.enterKey.wasPressedThisFrame) communication.DesktopSend();
            // 步骤 21：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 22：把计算结果 (ancestor != null && ancestor.IsMenuOpen) || 保存为exchangeOpen；下一条判断或调用会读取这个具体值。
        bool exchangeOpen = (ancestor != null && ancestor.IsMenuOpen) ||
            // 步骤 23：执行 (magicExchange != null && magicExchange.IsMenuOpen);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (magicExchange != null && magicExchange.IsMenuOpen);
        // Ignore one extra mouse sample after the exchange panel closes. The
        // pointer can move a long distance during a drag, and that accumulated
        // delta must not be applied to the camera on the closing frame.
        // 步骤 24：用 suppressMouseLook 记住此处真假结果（exchangeOpen || exchangeWasOpen），供下一条 if 选择执行路径。
        bool suppressMouseLook = exchangeOpen || exchangeWasOpen;
        // 步骤 25：把计算结果 exchangeOpen 保存为exchangeWasOpen；下一条判断或调用会读取这个具体值。
        exchangeWasOpen = exchangeOpen;
        // 步骤 26：计算 Mouse.current，把结果存进 mouse（mouse）；之后引用 mouse 时用的就是这次结果。
        Mouse mouse = Mouse.current;
        // 步骤 27：确认mouse已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (mouse != null)
        {
            // 步骤 28：计算 ReadMouseMovement(mouse)，把结果存进 delta（delta）；之后引用 delta 时用的就是这次结果。
            Vector2 delta = ReadMouseMovement(mouse);
            // 步骤 29：这里的 ! 表示取反；当 suppressMouseLook 不成立时才进入此分支。
            if (!suppressMouseLook)
            {
                // 步骤 30：在yaw原值上增加 delta.x * mouseSensitivity；旧状态与本次输入共同决定新状态。
                yaw += delta.x * mouseSensitivity;
                // 步骤 31：计算pitch并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
                pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, -60f, 60f);
            }
        }

        // 步骤 32：这里的 ! 表示取反；当 exchangeOpen 不成立时才进入此分支。
        if (!exchangeOpen)
        {
            // 步骤 33：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 yaw -= locomotion.snapAngle;
            if (keyboard.leftArrowKey.wasPressedThisFrame) yaw -= locomotion.snapAngle;
            // 步骤 34：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 yaw += locomotion.snapAngle;
            if (keyboard.rightArrowKey.wasPressedThisFrame) yaw += locomotion.snapAngle;
            // 步骤 35：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 pitch = Mathf.Clamp(pitch - 10f, -30f, 30f);
            if (keyboard.upArrowKey.wasPressedThisFrame) pitch = Mathf.Clamp(pitch - 10f, -30f, 30f);
            // 步骤 36：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 pitch = Mathf.Clamp(pitch + 10f, -30f, 30f);
            if (keyboard.downArrowKey.wasPressedThisFrame) pitch = Mathf.Clamp(pitch + 10f, -30f, 30f);
        }
        // 步骤 37：恢复 head.localRotation 到初始本地朝向；本地表示相对于父对象。
        head.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        // 步骤 38：新建用于move的对象或数值结构，并把构造参数保存进去。
        Vector2 move = new Vector2(
            // 步骤 39：继续填写上一行调用的参数：(keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f)；参数位置决定它表示的坐标、速度或选项。
            (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
            // 步骤 40：执行 (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
        // 步骤 41：调用 DesktopMove：把键盘方向转换成水平移动并处理桌面碰撞。
        locomotion.DesktopMove(move, keyboard.spaceKey.isPressed, keyboard.leftCtrlKey.isPressed,
            // 步骤 42：执行 keyboard.spaceKey.wasPressedThisFrame);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            keyboard.spaceKey.wasPressedThisFrame);

        // 步骤 43：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 SetPerspective：调整相机、角色可见性和第三人称跟随距离。
        if (!exchangeOpen && keyboard.digit1Key.wasPressedThisFrame) locomotion.SetPerspective(false);
        // 步骤 44：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 SetPerspective：调整相机、角色可见性和第三人称跟随距离。
        if (!exchangeOpen && keyboard.digit3Key.wasPressedThisFrame) locomotion.SetPerspective(true);
        // 步骤 45：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (keyboard.fKey.wasPressedThisFrame)
        {
            // 步骤 46：用 legacy 记住此处真假结果（keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed），供下一条 if 选择执行路径。
            bool legacy = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            // 步骤 47：检查magicExchange是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
            if (legacy || magicExchange == null || !magicExchange.IsNearAny)
                // 步骤 48：执行 ancestor?.DesktopToggleMenu();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                ancestor?.DesktopToggleMenu();
            // 步骤 49：前面的 if 条件为假时执行这里的备用处理。
            else magicExchange.DesktopToggleNearest();
        }
        // 步骤 50：检查magicExchange是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (magicExchange != null && magicExchange.IsMenuOpen)
        {
            // 步骤 51：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit1Key.wasPressedThisFrame) magicExchange.DesktopSelect(0);
            // 步骤 52：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit2Key.wasPressedThisFrame) magicExchange.DesktopSelect(1);
            // 步骤 53：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit3Key.wasPressedThisFrame) magicExchange.DesktopSelect(2);
            // 步骤 54：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit4Key.wasPressedThisFrame) magicExchange.DesktopSelect(3);
            // 步骤 55：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit5Key.wasPressedThisFrame) magicExchange.DesktopSelect(4);
            // 步骤 56：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopObtainSelected：桌面ObtainSelected。
            if (keyboard.enterKey.wasPressedThisFrame) magicExchange.DesktopObtainSelected();
        }
        // 步骤 57：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查ancestor是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        else if (ancestor != null && ancestor.IsMenuOpen)
        {
            // 步骤 58：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit1Key.wasPressedThisFrame) ancestor.DesktopSelect(0);
            // 步骤 59：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit2Key.wasPressedThisFrame) ancestor.DesktopSelect(1);
            // 步骤 60：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit3Key.wasPressedThisFrame) ancestor.DesktopSelect(2);
            // 步骤 61：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit4Key.wasPressedThisFrame) ancestor.DesktopSelect(3);
            // 步骤 62：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopSelect：桌面Select。
            if (keyboard.digit5Key.wasPressedThisFrame) ancestor.DesktopSelect(4);
            // 步骤 63：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时调用 DesktopObtainSelected：桌面ObtainSelected。
            if (keyboard.enterKey.wasPressedThisFrame) ancestor.DesktopObtainSelected();
        }
        // 步骤 64：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (keyboard.eKey.wasPressedThisFrame)
        {
            // 步骤 65：用三元表达式为 eater 二选一：问号前条件为真取前值，为假取冒号后的值。
            Transform eater = locomotion.avatar != null ? locomotion.avatar.transform : head;
            // 步骤 66：这里的 ! 表示取反；当 SkyQuestMagicPickup.TryConsumeWithE(eater) 不成立时才进入此分支。 成立时执行 ancestor?.EquipHeld();
            if (!SkyQuestMagicPickup.TryConsumeWithE(eater)) ancestor?.EquipHeld();
        }
        // 步骤 67：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 social?.DesktopShareHeart();
        if (keyboard.hKey.wasPressedThisFrame) social?.DesktopShareHeart();
        // 步骤 68：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 social?.DesktopOfferCandle();
        if (keyboard.cKey.wasPressedThisFrame) social?.DesktopOfferCandle();
        // 步骤 69：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 social?.DesktopGreet();
        if (keyboard.gKey.wasPressedThisFrame) social?.DesktopGreet();
        // 步骤 70：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 social?.DesktopReadStory(0);
        if (keyboard.digit7Key.wasPressedThisFrame) social?.DesktopReadStory(0);
        // 步骤 71：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 social?.DesktopReadStory(1);
        if (keyboard.digit8Key.wasPressedThisFrame) social?.DesktopReadStory(1);
        // 步骤 72：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 social?.DesktopReadStory(2);
        if (keyboard.digit9Key.wasPressedThisFrame) social?.DesktopReadStory(2);
        // 步骤 73：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 props?.DesktopTogglePreview();
        if (keyboard.pKey.wasPressedThisFrame) props?.DesktopTogglePreview();
        // 步骤 74：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 props?.DesktopPlace();
        if (keyboard.rKey.wasPressedThisFrame) props?.DesktopPlace();
        // 步骤 75：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 communication?.DesktopToggle();
        if (keyboard.tKey.wasPressedThisFrame) communication?.DesktopToggle();
        // 步骤 76：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。 成立时执行 communication?.DesktopToggle();
        if (keyboard.kKey.wasPressedThisFrame) communication?.DesktopToggle();
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：ReadMouseMovement——ReadMouseMovement！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Mouse mouse 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Vector2 类型的计算结果；执行到 return 时把该值交回调用者。
    private Vector2 ReadMouseMovement(Mouse mouse)
    {
        // 步骤 1：计算 mouse.position.ReadValue()，把结果存进 position（position）；之后引用 position 时用的就是这次结果。
        Vector2 position = mouse.position.ReadValue();
        // 步骤 2：计算 mouse.delta.ReadValue()，把结果存进 movement（movement）；之后引用 movement 时用的就是这次结果。
        Vector2 movement = mouse.delta.ReadValue();
        // 步骤 3：这里用 && 要求两边条件同时成立：movement.sqrMagnitude < 0.01f && hasMousePosition；任意一边为假就跳过本分支。
        if (movement.sqrMagnitude < 0.01f && hasMousePosition)
        {
            // Some Editor input backends report position but no delta while
            // the pointer is free over the Game view.
            // 步骤 4：计算 position - previousMousePosition，把结果存进 fallback（fallback）；之后引用 fallback 时用的就是这次结果。
            Vector2 fallback = position - previousMousePosition;
            // 步骤 5：检查条件 fallback.sqrMagnitude < 80f * 80f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 movement = fallback;
            if (fallback.sqrMagnitude < 80f * 80f) movement = fallback;
        }
        // 步骤 6：把计算结果 position 保存为previousMouse位置；下一条判断或调用会读取这个具体值。
        previousMousePosition = position;
        // 步骤 7：把 hasMousePosition 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        hasMousePosition = true;
        // 步骤 8：把 movement 作为方法结果返回给调用方；return 后本方法立即结束。
        return movement;
    }
    // ！！！功能结束：ReadMouseMovement！！！
}
