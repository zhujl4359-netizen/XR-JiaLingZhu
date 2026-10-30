// ！！！文件功能：桌面相机：切换第一、第三人称并平滑跟随角色！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

/// <summary>
/// 桌面版视角控制：按 1/3 切换第一、第三人称，并平滑跟随角色。
/// </summary>
// 定义 CameraController 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class CameraController : MonoBehaviour
{
    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("绑定目标")]
    // 声明 cameraTransform（camera变换）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform cameraTransform;        // Main Camera
    // 声明 c1HeadTransform（c1Head变换）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform c1HeadTransform;        // C1头部/身体上方的目标点
    // 声明 leftHandVisual（leftHandVisual）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject leftHandVisual;
    // 声明 rightHandVisual（rightHandVisual）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject rightHandVisual;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("第三人称摄像机")]
    // 把 Inspector 可调整的 cameraDistance（cameraDistance）设为默认值 10f；具体用途看它在下方的判断和计算。
    public float cameraDistance = 10f;
    // 把 Inspector 可调整的 mouseSensitivity（mouseSensitivity）设为默认值 0.15f；具体用途看它在下方的判断和计算。
    public float mouseSensitivity = 0.15f;
    // 把 Inspector 可调整的 minPitch（minPitch）设为默认值 -20f；具体用途看它在下方的判断和计算。
    public float minPitch = -20f;
    // 把 Inspector 可调整的 maxPitch（maxPitch）设为默认值 60f；具体用途看它在下方的判断和计算。
    public float maxPitch = 60f;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("第三人称初始角度")]
    // 把 Inspector 可调整的 initialYaw（initialYaw）设为默认值 0f；具体用途看它在下方的判断和计算。
    public float initialYaw = 0f;
    // 把 Inspector 可调整的 initialPitch（initialPitch）设为默认值 25f；具体用途看它在下方的判断和计算。
    public float initialPitch = 25f;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("第一人称局部偏移")]
    // 创建新的 Vector3 并存为 firstPersonLocalPos；构造参数决定其初始内容或对象名称。
    public Vector3 firstPersonLocalPos = new Vector3(0f, 1.4f, 0.2f);
    // 计算 Vector3.zero，把结果存进 firstPersonLocalRot（firstPersonLocalRot）；之后引用 firstPersonLocalRot 时用的就是这次结果。
    public Vector3 firstPersonLocalRot = Vector3.zero;

    // 用 isThirdPerson 记住此处真假结果（true），供下一条 if 选择执行路径。
    private bool isThirdPerson = true;

    // 声明 cameraYaw（cameraYaw）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float cameraYaw;
    // 声明 cameraPitch（cameraPitch）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float cameraPitch;
    // 声明 cameraVelocity（cameraVelocity）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 cameraVelocity;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Camera comfort")]
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Min(0.01f)] public float followSmoothTime = 0.22f;
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Min(0.01f)] public float rotationSmoothSpeed = 8f;


    /// <summary>
    /// 以 Inspector 指定的初始角度设置镜头，并默认进入第三人称。
    /// </summary>
    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：把计算结果 initialYaw 保存为cameraYaw；下一条判断或调用会读取这个具体值。
        cameraYaw = initialYaw;
        // 步骤 2：把计算结果 initialPitch 保存为cameraPitch；下一条判断或调用会读取这个具体值。
        cameraPitch = initialPitch;

        // 步骤 3：调用 SetThirdPerson：设置ThirdPerson。
        SetThirdPerson();
    }
    // ！！！功能结束：Start！！！

    /// <summary>
    /// 监听数字 1、3 和 Esc，分别切换视角或释放鼠标。
    /// </summary>
    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：检查Keyboard的current是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (Keyboard.current == null) return;

        // 按 1：第一人称
        // 步骤 2：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (Keyboard.current.digit1Key.wasPressedThisFrame ||
            // 步骤 3：这一行补充 Keyboard.current.numpad1Key.wasPressedThisFrame)，与相邻行合起来完成同一次计算或对象配置。
            Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            // 步骤 4：调用 SetFirstPerson：设置FirstPerson。
            SetFirstPerson();
        }

        // 按 3：第三人称
        // 步骤 5：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (Keyboard.current.digit3Key.wasPressedThisFrame ||
            // 步骤 6：这一行补充 Keyboard.current.numpad3Key.wasPressedThisFrame)，与相邻行合起来完成同一次计算或对象配置。
            Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            // 步骤 7：调用 SetThirdPerson：设置ThirdPerson。
            SetThirdPerson();
        }


        // ESC：释放鼠标
        // 步骤 8：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 步骤 9：把计算结果 CursorLockMode.None 保存为lock状态；下一条判断或调用会读取这个具体值。
            Cursor.lockState = CursorLockMode.None;
            // 步骤 10：把 Cursor.visible 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            Cursor.visible = true;
        }
    }
    // ！！！功能结束：Update！！！

    /// <summary>
    /// 角色位置更新之后再移动第三人称镜头，减少跟随抖动。
    /// </summary>
    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void LateUpdate()
    {
        // 步骤 1：检查条件 isThirdPerson；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 UpdateThirdPersonCamera：更新ThirdPerson相机。
        if (isThirdPerson) UpdateThirdPersonCamera();
    }
    // ！！！功能结束：LateUpdate！！！

    /// <summary>
    /// 读取鼠标角度，限制俯仰并平滑计算镜头位置和朝向。
    /// </summary>
    // ！！！功能：UpdateThirdPersonCamera——更新ThirdPerson相机！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void UpdateThirdPersonCamera()
    {
        // 步骤 1：检查camera变换、c1Head变换是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (cameraTransform == null || c1HeadTransform == null)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;

        // 鼠标移动
        // 步骤 3：检查Mouse的current是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            // 步骤 4：计算 Mouse.current.delta.ReadValue()，把结果存进 mouseDelta（mouseDelta）；之后引用 mouseDelta 时用的就是这次结果。
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // 步骤 5：在cameraYaw原值上增加 mouseDelta.x * mouseSensitivity；旧状态与本次输入共同决定新状态。
            cameraYaw += mouseDelta.x * mouseSensitivity;
            // 步骤 6：在cameraPitch原值上减少 mouseDelta.y * mouseSensitivity；旧状态与本次输入共同决定新状态。
            cameraPitch -= mouseDelta.y * mouseSensitivity;

            // 步骤 7：计算cameraPitch并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
            cameraPitch = Mathf.Clamp(
                // 步骤 8：继续填写上一行调用的参数：cameraPitch；参数位置决定它表示的坐标、速度或选项。
                cameraPitch,
                // 步骤 9：继续填写上一行调用的参数：minPitch；参数位置决定它表示的坐标、速度或选项。
                minPitch,
                // 步骤 10：这一行补充 maxPitch，与相邻行合起来完成同一次计算或对象配置。
                maxPitch
            );
        }

        // 根据旋转角度计算摄像机位置
        // 步骤 11：把计算结果 Quaternion.Euler( 保存为rotation；下一条判断或调用会读取这个具体值。
        Quaternion rotation = Quaternion.Euler(
            // 步骤 12：继续填写上一行调用的参数：cameraPitch；参数位置决定它表示的坐标、速度或选项。
            cameraPitch,
            // 步骤 13：继续填写上一行调用的参数：cameraYaw；参数位置决定它表示的坐标、速度或选项。
            cameraYaw,
            // 步骤 14：这一行补充 0f，与相邻行合起来完成同一次计算或对象配置。
            0f
        );

        // 步骤 15：计算 c1HeadTransform.position，把结果存进 targetPosition（target位置）；之后引用 targetPosition 时用的就是这次结果。
        Vector3 targetPosition = c1HeadTransform.position;

        // 步骤 16：这一行补充 Vector3 cameraPosition =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 cameraPosition =
            // 步骤 17：执行 targetPosition - rotation * Vector3.forward * cameraDistance;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            targetPosition - rotation * Vector3.forward * cameraDistance;

        // 步骤 18：把position更新为 Vector3.SmoothDamp(cameraTransform.position, cameraPosition,；世界或局部位置会立即影响物体摆放。
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, cameraPosition,
            // 步骤 19：执行 ref cameraVelocity, followSmoothTime);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ref cameraVelocity, followSmoothTime);

        // 步骤 20：计算 Quaternion.LookRotation(targetPosition - cameraTransform.position)，把结果存进 lookRotation（look旋转）；之后引用 lookRotation 时用的就是这次结果。
        Quaternion lookRotation = Quaternion.LookRotation(targetPosition - cameraTransform.position);
        // 步骤 21：对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, lookRotation,
            // 步骤 22：执行 1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime));
    }
    // ！！！功能结束：UpdateThirdPersonCamera！！！

    /// <summary>
    /// 把镜头挂到角色头部，应用局部偏移并显示第一人称手部模型。
    /// </summary>
    // ！！！功能：SetFirstPerson——设置FirstPerson！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void SetFirstPerson()
    {
        // 步骤 1：把 isThirdPerson 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        isThirdPerson = false;

        // 释放鼠标
        // 步骤 2：把计算结果 CursorLockMode.None 保存为lock状态；下一条判断或调用会读取这个具体值。
        Cursor.lockState = CursorLockMode.None;
        // 步骤 3：把 Cursor.visible 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        Cursor.visible = true;

        // 步骤 4：检查camera变换、c1Head变换是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (cameraTransform != null && c1HeadTransform != null)
        {
            // 步骤 5：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
            cameraTransform.SetParent(c1HeadTransform);

            // 步骤 6：把计算结果 firstPersonLocalPos 保存为local位置；下一条判断或调用会读取这个具体值。
            cameraTransform.localPosition = firstPersonLocalPos;
            // 步骤 7：把计算结果 firstPersonLocalRot 保存为localEulerAngles；下一条判断或调用会读取这个具体值。
            cameraTransform.localEulerAngles = firstPersonLocalRot;
        }

        // 步骤 8：确认leftHandVisual已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (leftHandVisual != null)
            // 步骤 9：显示这个场景对象及其子对象；界面会立即反映这一状态。
            leftHandVisual.SetActive(true);

        // 步骤 10：确认rightHandVisual已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (rightHandVisual != null)
            // 步骤 11：显示这个场景对象及其子对象；界面会立即反映这一状态。
            rightHandVisual.SetActive(true);
    }
    // ！！！功能结束：SetFirstPerson！！！

    /// <summary>
    /// 镜头脱离角色层级，锁定鼠标并隐藏第一人称手部模型。
    /// </summary>
    // ！！！功能：SetThirdPerson——设置ThirdPerson！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void SetThirdPerson()
    {
        // 步骤 1：把 isThirdPerson 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        isThirdPerson = true;

        // 鼠标锁定到游戏窗口
        // 步骤 2：把计算结果 CursorLockMode.Locked 保存为lock状态；下一条判断或调用会读取这个具体值。
        Cursor.lockState = CursorLockMode.Locked;
        // 步骤 3：把 Cursor.visible 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        Cursor.visible = false;

        // 步骤 4：确认camera变换已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (cameraTransform != null)
        {
            // 摄像机脱离 C1
            // 步骤 5：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
            cameraTransform.SetParent(null);
        }

        // 步骤 6：把 cameraVelocity 清为零向量；清除上次移动、投掷或位置偏移的残留值。
        cameraVelocity = Vector3.zero;
        // 步骤 7：调用 UpdateThirdPersonCamera：更新ThirdPerson相机。
        UpdateThirdPersonCamera();

        // 步骤 8：确认leftHandVisual已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (leftHandVisual != null)
            // 步骤 9：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            leftHandVisual.SetActive(false);

        // 步骤 10：确认rightHandVisual已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (rightHandVisual != null)
            // 步骤 11：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            rightHandVisual.SetActive(false);
    }
    // ！！！功能结束：SetThirdPerson！！！
}