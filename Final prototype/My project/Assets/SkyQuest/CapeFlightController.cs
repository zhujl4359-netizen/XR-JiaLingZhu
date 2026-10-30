// ！！！文件功能：计算斗篷起飞、滑翔、加速与风力，交由玩家移动组件执行位移！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

// 定义可选操作的枚举；用名字代替魔法数字，switch 可以按选项分派行为。
public enum CapeFlightState { Grounded, Takeoff, Ascending, Gliding, Falling, Landing }

/// <summary>Calculates one rig displacement per frame; SkyQuestLocomotion remains the only mover.</summary>
// 定义 CapeFlightController 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeFlightController : MonoBehaviour
{
    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Controller input")]
    // actions 保存actions（类型 InputActionAsset）；下方方法依靠它记录或查找场景状态。
    public InputActionAsset actions;
    // 声明 headRelativeSteering（headRelativeSteering）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool headRelativeSteering;
    // 声明 comfortMode（comfort模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool comfortMode;
    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Flight tuning")]
    // 把 Inspector 可调整的 takeoffVelocityChange（takeoffVelocityChange）设为默认值 3f；具体用途看它在下方的判断和计算。
    public float takeoffVelocityChange = 3f;
    // 把 Inspector 可调整的 airBoostVelocityChange（airBoostVelocityChange）设为默认值 2.2f；具体用途看它在下方的判断和计算。
    public float airBoostVelocityChange = 2.2f;
    // 把 Inspector 可调整的 forwardBoostVelocityChange（forwardBoostVelocityChange）设为默认值 1.2f；具体用途看它在下方的判断和计算。
    public float forwardBoostVelocityChange = 1.2f;
    // 把 Inspector 可调整的 maxVerticalSpeed（maxVerticalSpeed）设为默认值 5f；具体用途看它在下方的判断和计算。
    public float maxVerticalSpeed = 5f;
    // 把 Inspector 可调整的 maxHorizontalSpeed（maxHorizontalSpeed）设为默认值 6f；具体用途看它在下方的判断和计算。
    public float maxHorizontalSpeed = 6f;
    // 把 Inspector 可调整的 glideGravityMultiplier（glideGravityMultiplier）设为默认值 0.35f；具体用途看它在下方的判断和计算。
    public float glideGravityMultiplier = 0.35f;
    // 把 Inspector 可调整的 flightCooldown（flightCooldown）设为默认值 0.6f；具体用途看它在下方的判断和计算。
    public float flightCooldown = 0.6f;
    // 把 Inspector 可调整的 horizontalDrag（horizontalDrag）设为默认值 0.3f；具体用途看它在下方的判断和计算。
    public float horizontalDrag = 0.3f;
    // 把 Inspector 可调整的 groundSpeed（地面行走速度）设为默认值 2.7f；具体用途看它在下方的判断和计算。
    public float groundSpeed = 2.7f;
    // 把 Inspector 可调整的 groundAcceleration（groundAcceleration）设为默认值 8f；具体用途看它在下方的判断和计算。
    public float groundAcceleration = 8f;
    // 把 Inspector 可调整的 airAcceleration（airAcceleration）设为默认值 3f；具体用途看它在下方的判断和计算。
    public float airAcceleration = 3f;

    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public CapeFlightState State { get; private set; } = CapeFlightState.Grounded;
    // IsFlying是 Inspector 中的开关，默认 > State != CapeFlightState.Grounded && State != CapeFlightState.Landing；勾选状态决定相关功能是否运行。
    public bool IsFlying => State != CapeFlightState.Grounded && State != CapeFlightState.Landing;
    // 计算 > horizontalVelocity + Vector3.up * verticalVelocity，把结果存进 Velocity（Velocity）；之后引用 Velocity 时用的就是这次结果。
    public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;
    // 执行 public event Action<CapeFlightState> OnStateChanged;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action<CapeFlightState> OnStateChanged;
    // 执行 public event Action OnBoostSucceeded;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action OnBoostSucceeded;
    // 执行 public event Action OnBoostRejected;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action OnBoostRejected;
    // 执行 public event Action OnSafeStop;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action OnSafeStop;

    // energy 保存energy（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    private CapeEnergySystem energy;
    // boostAction 保存boostAction（类型 InputAction）；下方方法依靠它记录或查找场景状态。
    private InputAction boostAction;
    // moveAction 保存moveAction（类型 InputAction）；下方方法依靠它记录或查找场景状态。
    private InputAction moveAction;
    // turnAction 保存turnAction（类型 InputAction）；下方方法依靠它记录或查找场景状态。
    private InputAction turnAction;
    // safeStopAction 保存safeStopAction（类型 InputAction）；下方方法依靠它记录或查找场景状态。
    private InputAction safeStopAction;
    // 声明 horizontalVelocity（当前水平速度）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 horizontalVelocity;
    // 声明 verticalVelocity（当前竖直速度）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float verticalVelocity;
    // 声明 nextBoostTime（nextBoostTime）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextBoostTime;
    // 声明 takeoffTime（takeoffTime）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float takeoffTime;
    // 用 glideEnabled 记住此处真假结果（true），供下一条 if 选择执行路径。
    private bool glideEnabled = true;
    // 声明 lastGroundContact（lastGroundContact）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float lastGroundContact;

    // ！！！功能：Initialize——Initialize！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 CapeEnergySystem owner 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Initialize(CapeEnergySystem owner)
    {
        // 步骤 1：把计算结果 owner 保存为energy；下一条判断或调用会读取这个具体值。
        energy = owner;
        // 步骤 2：根据当前游戏时间计算并保存lastGroundContact，用于后续延迟或冷却判断。
        lastGroundContact = Time.time;
        // 步骤 3：检查actions是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 actions = Resources.Load<InputActionAsset>("CapeFlightActions");
        if (actions == null) actions = Resources.Load<InputActionAsset>("CapeFlightActions");
        // 步骤 4：检查actions是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (actions == null)
        {
            // 步骤 5：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError("CapeFlightActions input asset is missing from Resources.", this);
            // 步骤 6：把 enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            enabled = false;
            // 步骤 7：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 8：计算 actions.FindActionMap("Cape Flight", true)，把结果存进 map（map）；之后引用 map 时用的就是这次结果。
        InputActionMap map = actions.FindActionMap("Cape Flight", true);
        // 步骤 9：把计算结果 map.FindAction("Boost", true) 保存为boostAction；下一条判断或调用会读取这个具体值。
        boostAction = map.FindAction("Boost", true);
        // 步骤 10：把计算结果 map.FindAction("Move", true) 保存为moveAction；下一条判断或调用会读取这个具体值。
        moveAction = map.FindAction("Move", true);
        // 步骤 11：把计算结果 map.FindAction("Snap Turn", true) 保存为turnAction；下一条判断或调用会读取这个具体值。
        turnAction = map.FindAction("Snap Turn", true);
        // 步骤 12：把计算结果 map.FindAction("Safe Stop", true) 保存为safeStopAction；下一条判断或调用会读取这个具体值。
        safeStopAction = map.FindAction("Safe Stop", true);
        // 步骤 13：调用 Enable：Enable。
        map.Enable();
    }
    // ！！！功能结束：Initialize！！！

    // ！！！功能：OnEnable——组件启用回调：注册事件或立即刷新当前显示！！！
    // 何时执行：对象或脚本被启用时调用；适合注册事件。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnEnable()
    {
        // 步骤 1：检查actions、energy是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 FindActionMap：查找ActionMap。
        if (actions != null && energy != null) actions.FindActionMap("Cape Flight")?.Enable();
    }
    // ！！！功能结束：OnEnable！！！

    // ！！！功能：OnDisable——组件停用回调：解除事件或释放输入状态，防止重复监听！！！
    // 何时执行：对象或脚本被停用时调用；适合取消事件监听。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDisable()
    {
        // 步骤 1：确认actions已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 FindActionMap：查找ActionMap。
        if (actions != null) actions.FindActionMap("Cape Flight")?.Disable();
    }
    // ！！！功能结束：OnDisable！！！

    // ！！！功能：ReadMove——ReadMove！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Vector2 类型的计算结果；执行到 return 时把该值交回调用者。
    public Vector2 ReadMove() => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    // ！！！功能结束：ReadMove！！！
    // ！！！功能：ReadTurn——ReadTurn！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Vector2 类型的计算结果；执行到 return 时把该值交回调用者。
    public Vector2 ReadTurn() => turnAction != null ? turnAction.ReadValue<Vector2>() : Vector2.zero;
    // ！！！功能结束：ReadTurn！！！

    // ！！！功能：Tick——Tick！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float dt, Vector3 bodyForward, Vector3 headForward, float movementScale 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    // 原理：根据当前飞行状态计算位移建议，但最终由 SkyQuestLocomotion 移动玩家。
    public Vector3 Tick(float dt, Vector3 bodyForward, Vector3 headForward, float movementScale)
    {
        // 步骤 1：检查energy是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (energy == null) return Vector3.zero;
        // 步骤 2：检查safeStopAction是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 SafeStop：SafeStop。
        if (safeStopAction != null && safeStopAction.WasPressedThisFrame()) SafeStop();
        // 步骤 3：检查boostAction是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 TryBoost：检查能量是否足够，并在允许时执行一次飞行加速。
        if (boostAction != null && boostAction.WasPressedThisFrame()) TryBoost();

        // 步骤 4：把原三维朝向投影到地面平面并存为 forward；行走或转向不受头部俯仰影响。
        Vector3 forward = Vector3.ProjectOnPlane(headRelativeSteering ? headForward : bodyForward, Vector3.up).normalized;
        // 步骤 5：检查条件 forward.sqrMagnitude < 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 forward = Vector3.forward;
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
        // 步骤 6：用竖直向量与前方向做叉积，算出与前方垂直的右方向。
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        // 步骤 7：计算 ReadMove()，把结果存进 stick（stick）；之后引用 stick 时用的就是这次结果。
        Vector2 stick = ReadMove();
        // Backward input allows retreat without matching forward flight acceleration.
        // 步骤 8：限制 direction 向量的最大长度，防止斜向输入或投掷速度超出预期。
        Vector3 direction = Vector3.ClampMagnitude(forward * (stick.y < 0f ? stick.y * 0.55f : stick.y) + right * stick.x, 1f);
        // 步骤 9：用三元表达式为 speedCap 二选一：问号前条件为真取前值，为假取冒号后的值。
        float speedCap = Mathf.Min(maxHorizontalSpeed, comfortMode ? 3.5f : maxHorizontalSpeed);
        // 步骤 10：用三元表达式为 targetSpeed 二选一：问号前条件为真取前值，为假取冒号后的值。
        float targetSpeed = Mathf.Min(speedCap, (IsFlying ? 3.2f : groundSpeed) * movementScale);
        // 步骤 11：计算 direction * targetSpeed，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
        Vector3 target = direction * targetSpeed;
        // 步骤 12：用三元表达式为 acceleration 二选一：问号前条件为真取前值，为假取冒号后的值。
        float acceleration = IsFlying ? airAcceleration : groundAcceleration;
        // 步骤 13：检查条件 comfortMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 acceleration *= 0.65f;
        if (comfortMode) acceleration *= 0.65f;
        // 步骤 14：这里用 || 表示任一条件成立就处理：!IsFlying || stick.sqrMagnitude >= 0.01f；用于合并几种停止或跳过原因。
        if (!IsFlying || stick.sqrMagnitude >= 0.01f)
            // 步骤 15：把计算结果 Vector3.MoveTowards(horizontalVelocity, target, acceleration * dt) 保存为当前水平速度；下一条判断或调用会读取这个具体值。
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, acceleration * dt);
        // 步骤 16：前面的 if 条件为假时执行这里的备用处理。
        else
            // 步骤 17：把计算结果 Vector3.MoveTowards(horizontalVelocity, Vector3.zero, horizontalDrag * dt) 保存为当前水平速度；下一条判断或调用会读取这个具体值。
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, horizontalDrag * dt);
        // 步骤 18：把计算结果 Vector3.ClampMagnitude(horizontalVelocity, speedCap) 保存为当前水平速度；下一条判断或调用会读取这个具体值。
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, speedCap);

        // 步骤 19：检查条件 IsFlying；只有为真才运行下面的步骤，否则沿原流程继续。
        if (IsFlying)
        {
            // 步骤 20：用三元表达式为 gravity 二选一：问号前条件为真取前值，为假取冒号后的值。
            float gravity = 9.81f * (glideEnabled ? glideGravityMultiplier : 1f);
            // 步骤 21：把计算结果 Mathf.Max(-4f, verticalVelocity - gravity * dt) 保存为当前竖直速度；下一条判断或调用会读取这个具体值。
            verticalVelocity = Mathf.Max(-4f, verticalVelocity - gravity * dt);
            // 步骤 22：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
            if (State == CapeFlightState.Takeoff && Time.time - takeoffTime > 0.12f)
                // 步骤 23：调用 ChangeState：Change状态。
                ChangeState(CapeFlightState.Ascending);
            // 步骤 24：这里用 && 要求两边条件同时成立：State == CapeFlightState.Ascending && verticalVelocity <= 0f；任意一边为假就跳过本分支。
            if (State == CapeFlightState.Ascending && verticalVelocity <= 0f)
                // 步骤 25：调用 ChangeState：Change状态。
                ChangeState(glideEnabled ? CapeFlightState.Gliding : CapeFlightState.Falling);
        }
        // 步骤 26：前面的 if 条件为假时执行这里的备用处理。
        else verticalVelocity = -2f;
        // 步骤 27：把 (horizontalVelocity + Vector3.up * verticalVelocity) * dt 作为方法结果返回给调用方；return 后本方法立即结束。
        return (horizontalVelocity + Vector3.up * verticalVelocity) * dt;
    }
    // ！！！功能结束：Tick！！！

    // ！！！功能：TryBoost——检查能量是否足够，并在允许时执行一次飞行加速！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public bool TryBoost()
    {
        // 步骤 1：检查energy是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (energy == null || Time.time < nextBoostTime || !energy.TryConsumeBoostEnergy())
        {
            // 步骤 2：执行 OnBoostRejected?.Invoke();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnBoostRejected?.Invoke();
            // 步骤 3：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;
        }
        // 步骤 4：用 grounded 记住此处真假结果（State == CapeFlightState.Grounded || State == CapeFlightState.Landing），供下一条 if 选择执行路径。
        bool grounded = State == CapeFlightState.Grounded || State == CapeFlightState.Landing;
        // 步骤 5：把计算结果 Mathf.Min(maxVerticalSpeed, Mathf.Max(0f, verticalVelocity) + 保存为当前竖直速度；下一条判断或调用会读取这个具体值。
        verticalVelocity = Mathf.Min(maxVerticalSpeed, Mathf.Max(0f, verticalVelocity) +
            // 步骤 6：执行 (grounded ? takeoffVelocityChange : airBoostVelocityChange));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (grounded ? takeoffVelocityChange : airBoostVelocityChange));
        // 步骤 7：把原三维朝向投影到地面平面并存为 forward；行走或转向不受头部俯仰影响。
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        // 步骤 8：把计算结果 Vector3.ClampMagnitude(horizontalVelocity + forward * forwardBoostVelocityChange, 保存为当前水平速度；下一条判断或调用会读取这个具体值。
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity + forward * forwardBoostVelocityChange,
            // 步骤 9：执行 maxHorizontalSpeed);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            maxHorizontalSpeed);
        // 步骤 10：根据当前游戏时间计算并保存nextBoostTime，用于后续延迟或冷却判断。
        nextBoostTime = Time.time + flightCooldown;
        // 步骤 11：检查条件 grounded；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { takeoffTime = Time.time; ChangeState(CapeFlightState.Takeoff); }
        if (grounded) { takeoffTime = Time.time; ChangeState(CapeFlightState.Takeoff); }
        // 步骤 12：前面的 if 条件为假时执行这里的备用处理。
        else ChangeState(CapeFlightState.Ascending);
        // 步骤 13：执行 OnBoostSucceeded?.Invoke();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnBoostSucceeded?.Invoke();
        // 步骤 14：本次检测或激活成功，返回 true 让调用方继续相应反馈。
        return true;
    }
    // ！！！功能结束：TryBoost！！！

    // ！！！功能：ReportCollision——Report碰撞！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 CollisionFlags flags 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void ReportCollision(CollisionFlags flags)
    {
        // 步骤 1：这里用 && 要求两边条件同时成立：(flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f；任意一边为假就跳过本分支。 成立时执行 verticalVelocity = 0f;
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
        // 步骤 2：检查条件 (flags & CollisionFlags.Sides) != 0；只有为真才运行下面的步骤，否则沿原流程继续。
        if ((flags & CollisionFlags.Sides) != 0)
            // 步骤 3：执行 horizontalVelocity *= 0.5f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            horizontalVelocity *= 0.5f;
        // 步骤 4：这里用 && 要求两边条件同时成立：(flags & CollisionFlags.Below) != 0 && verticalVelocity <= 0f；任意一边为假就跳过本分支。
        if ((flags & CollisionFlags.Below) != 0 && verticalVelocity <= 0f)
        {
            // 步骤 5：根据当前游戏时间计算并保存lastGroundContact，用于后续延迟或冷却判断。
            lastGroundContact = Time.time;
            // 步骤 6：检查条件 IsFlying；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 ChangeState：Change状态。
            if (IsFlying) ChangeState(CapeFlightState.Landing);
            // 步骤 7：前面的 if 条件为假时执行这里的备用处理。
            else ChangeState(CapeFlightState.Grounded);
            // 步骤 8：把计算结果 -2f 保存为当前竖直速度；下一条判断或调用会读取这个具体值。
            verticalVelocity = -2f;
        }
        // 步骤 9：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：(flags & CollisionFlags.Below) == 0 && State == CapeFlightState.Grounded &&；任意一边为假就跳过本分支。
        else if ((flags & CollisionFlags.Below) == 0 && State == CapeFlightState.Grounded &&
            // 步骤 10：这一行补充 Time.time - lastGroundContact > 0.08f)，与相邻行合起来完成同一次计算或对象配置。
            Time.time - lastGroundContact > 0.08f)
            // 步骤 11：调用 ChangeState：Change状态。
            ChangeState(glideEnabled ? CapeFlightState.Gliding : CapeFlightState.Falling);
    }
    // ！！！功能结束：ReportCollision！！！

    // ！！！功能：AddWindLift——添加WindLift！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float amount 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void AddWindLift(float amount)
    {
        // 步骤 1：检查条件 IsFlying；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 verticalVelocity = Mathf.Min(maxVerticalSpeed, verticalVelocity + amount);
        if (IsFlying) verticalVelocity = Mathf.Min(maxVerticalSpeed, verticalVelocity + amount);
    }
    // ！！！功能结束：AddWindLift！！！

    // ！！！功能：SafeStop——SafeStop！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void SafeStop()
    {
        // 步骤 1：把 horizontalVelocity 清为零向量；清除上次移动、投掷或位置偏移的残留值。
        horizontalVelocity = Vector3.zero;
        // 步骤 2：把计算结果 -0.5f 保存为当前竖直速度；下一条判断或调用会读取这个具体值。
        verticalVelocity = -0.5f;
        // 步骤 3：把 glideEnabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        glideEnabled = true;
        // 步骤 4：检查条件 IsFlying；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 ChangeState：Change状态。
        if (IsFlying) ChangeState(CapeFlightState.Gliding);
        // 步骤 5：执行 OnSafeStop?.Invoke();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnSafeStop?.Invoke();
    }
    // ！！！功能结束：SafeStop！！！

    // ！！！功能：SetGliding——设置Gliding！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool enabled 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void SetGliding(bool enabled)
    {
        // 步骤 1：把计算结果 enabled 保存为glideEnabled；下一条判断或调用会读取这个具体值。
        glideEnabled = enabled;
        // 步骤 2：这里用 || 表示任一条件成立就处理：!IsFlying || State == CapeFlightState.Ascending || State == CapeFlightState.Takeoff；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!IsFlying || State == CapeFlightState.Ascending || State == CapeFlightState.Takeoff) return;
        // 步骤 3：调用 ChangeState：Change状态。
        ChangeState(enabled ? CapeFlightState.Gliding : CapeFlightState.Falling);
    }
    // ！！！功能结束：SetGliding！！！

    // ！！！功能：ChangeState——Change状态！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 CapeFlightState next 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ChangeState(CapeFlightState next)
    {
        // 步骤 1：检查条件 State == next；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (State == next) return;
        // 步骤 2：把计算结果 next 保存为状态；下一条判断或调用会读取这个具体值。
        State = next;
        // 步骤 3：执行 OnStateChanged?.Invoke(next);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnStateChanged?.Invoke(next);
    }
    // ！！！功能结束：ChangeState！！！
}
