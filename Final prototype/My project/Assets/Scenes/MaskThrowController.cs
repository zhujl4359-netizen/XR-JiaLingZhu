// ！！！文件功能：桌面鹿头投掷：瞄准弧线、落点提示与物理发射！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

/// <summary>
/// 桌面版鹿头投掷：Q 直接抛出，按住 P 预览低弧线和落点后松开发射。
/// </summary>
// 定义 MaskThrowController 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class MaskThrowController : MonoBehaviour
{
    // =====================================================
    // 摄像机
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("摄像机")]
    // 声明 playerCamera（player相机）：相机引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Camera playerCamera;


    // =====================================================
    // Q：普通投掷
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Q：普通投掷")]

    // 把 Inspector 可调整的 throwForce（向前投掷力度）设为默认值 20f；具体用途看它在下方的判断和计算。
    public float throwForce = 20f;

    // 把 Inspector 可调整的 upwardForce（upwardForce）设为默认值 2f；具体用途看它在下方的判断和计算。
    public float upwardForce = 2f;


    // =====================================================
    // P：瞄准投掷
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("P：瞄准投掷")]

    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("最大瞄准距离")]
    // 把 Inspector 可调整的 maxAimDistance（maxAimDistance）设为默认值 8f；具体用途看它在下方的判断和计算。
    public float maxAimDistance = 8f;

    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("抛物线角度。越小越平、越远")]
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Range(5f, 30f)]
    // 把 Inspector 可调整的 aimAngle（aimAngle）设为默认值 10f；具体用途看它在下方的判断和计算。
    public float aimAngle = 10f;

    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("如果没有检测到地面，使用这个距离")]
    // 把 Inspector 可调整的 fallbackAimDistance（fallbackAimDistance）设为默认值 8f；具体用途看它在下方的判断和计算。
    public float fallbackAimDistance = 8f;


    // =====================================================
    // 地面
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("地面")]

    // 声明 groundLayer（groundLayer）：物理层筛选值；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public LayerMask groundLayer;


    // =====================================================
    // 抛物线
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("抛物线")]

    // 声明 trajectoryLine（trajectoryLine）：可见射线组件引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public LineRenderer trajectoryLine;

    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("抛物线分段")]
    // 把 Inspector 可调整的 trajectoryPoints（trajectoryPoints）设为默认值 48；具体用途看它在下方的判断和计算。
    public int trajectoryPoints = 48;

    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [Tooltip("抛物线粗细")]
    // 把 Inspector 可调整的 trajectoryWidth（trajectoryWidth）设为默认值 0.035f；具体用途看它在下方的判断和计算。
    public float trajectoryWidth = 0.035f;


    // =====================================================
    // 落点圆圈
    // =====================================================

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("落点圆圈")]

    // 把 Inspector 可调整的 targetCircleRadius（targetCircleRadius）设为默认值 1.65f；具体用途看它在下方的判断和计算。
    public float targetCircleRadius = 1.65f;

    // 把 Inspector 可调整的 targetCircleSegments（targetCircleSegments）设为默认值 60；具体用途看它在下方的判断和计算。
    public int targetCircleSegments = 60;

    // 把 Inspector 可调整的 targetCircleHeight（targetCircleHeight）设为默认值 0.018f；具体用途看它在下方的判断和计算。
    public float targetCircleHeight = 0.018f;

    // 把 Inspector 可调整的 targetCircleWidth（targetCircleWidth）设为默认值 0.075f；具体用途看它在下方的判断和计算。
    public float targetCircleWidth = 0.075f;


    // =====================================================
    // 内部变量
    // =====================================================

    // 声明 rb（rb）：物理刚体引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Rigidbody rb;

    // 用 isAiming 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool isAiming = false;

    // 用 hasValidTarget 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool hasValidTarget = false;

    // 声明 targetCircle（targetCircle）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject targetCircle;

    // 声明 currentTarget（currentTarget）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 currentTarget;

    // 计算 Vector3.up，把结果存进 currentTargetNormal（currentTargetNormal）；之后引用 currentTargetNormal 时用的就是这次结果。
    private Vector3 currentTargetNormal = Vector3.up;

    // 声明 currentLaunchVelocity（currentLaunchVelocity）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 currentLaunchVelocity;

    // 声明 currentFlightTime（currentFlightTime）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float currentFlightTime;


    // =====================================================
    // Start
    // =====================================================

    /// <summary>
    /// 规范化旧场景中的瞄准参数，取得刚体/摄像机并准备轨迹和落点预览。
    /// </summary>
    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：调用 ApplyUnifiedAimStyle：应用UnifiedAimStyle。
        ApplyUnifiedAimStyle();

        // 步骤 2：从目标 GameObject 查找并保存 rb 组件；后面的移动或显示逻辑会直接使用这个缓存。
        rb = GetComponent<Rigidbody>();

        // 步骤 3：检查rb是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (rb == null)
        {
            // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError(
                // 步骤 5：这一行补充 "MaskThrowController：没有找到 Rigidbody！"，与相邻行合起来完成同一次计算或对象配置。
                "MaskThrowController：没有找到 Rigidbody！"
            );
        }

        // 步骤 6：检查player相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (playerCamera == null)
        {
            // 步骤 7：把计算结果 Camera.main 保存为player相机；下一条判断或调用会读取这个具体值。
            playerCamera = Camera.main;
        }

        // 步骤 8：检查player相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (playerCamera == null)
        {
            // 步骤 9：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError(
                // 步骤 10：这一行补充 "MaskThrowController：没有找到 Main Camera！"，与相邻行合起来完成同一次计算或对象配置。
                "MaskThrowController：没有找到 Main Camera！"
            );
        }

        // 步骤 11：调用 CreateTrajectory：创建Trajectory。
        CreateTrajectory();

        // 步骤 12：调用 CreateTargetCircle：创建TargetCircle。
        CreateTargetCircle();

        // 步骤 13：调用 HideAimVisuals：HideAimVisuals。
        HideAimVisuals();

        // 步骤 14：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 15：这一行补充 "MaskThrowController 已启动，物体：" +，与相邻行合起来完成同一次计算或对象配置。
            "MaskThrowController 已启动，物体：" +
            // 步骤 16：这一行补充 gameObject.name，与相邻行合起来完成同一次计算或对象配置。
            gameObject.name
        );
    }
    // ！！！功能结束：Start！！！

    // Scene files made before the unified throw preview contain serialized
    // values as long as 45 metres.  Enforce the compact style at runtime too,
    // so old scenes and newly spawned throwables behave identically.
    /// <summary>
    /// 把瞄准距离、角度及预览尺寸约束到当前版本统一的范围。
    /// </summary>
    // ！！！功能：ApplyUnifiedAimStyle——应用UnifiedAimStyle！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void ApplyUnifiedAimStyle()
    {
        // 步骤 1：计算maxAimDistance并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        maxAimDistance = Mathf.Clamp(maxAimDistance, 5f, 8f);
        // 步骤 2：计算fallbackAimDistance并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        fallbackAimDistance = Mathf.Clamp(fallbackAimDistance, 5f, 8f);
        // 步骤 3：计算aimAngle并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        aimAngle = Mathf.Clamp(aimAngle, 9f, 16f);
        // 步骤 4：计算trajectoryPoints并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        trajectoryPoints = Mathf.Clamp(trajectoryPoints, 36, 52);
        // 步骤 5：计算trajectoryWidth并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        trajectoryWidth = Mathf.Clamp(trajectoryWidth, .028f, .04f);
        // 步骤 6：把计算结果 Mathf.Max(targetCircleRadius, 1.65f) 保存为targetCircleRadius；下一条判断或调用会读取这个具体值。
        targetCircleRadius = Mathf.Max(targetCircleRadius, 1.65f);
        // 步骤 7：把计算结果 .018f 保存为targetCircleHeight；下一条判断或调用会读取这个具体值。
        targetCircleHeight = .018f;
        // 步骤 8：计算targetCircleWidth并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        targetCircleWidth = Mathf.Clamp(targetCircleWidth, .06f, .085f);
    }
    // ！！！功能结束：ApplyUnifiedAimStyle！！！


    // =====================================================
    // Update
    // =====================================================

    /// <summary>
    /// 处理 P 的按下、持续瞄准和松开发射，以及 Q 的直接投掷。
    /// </summary>
    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：当前没有键盘设备时结束本帧处理，避免读取不存在的按键。
        if (Keyboard.current == null)
        {
            // 步骤 2：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogWarning(
                // 步骤 3：这一行补充 "Keyboard.current == null"，与相邻行合起来完成同一次计算或对象配置。
                "Keyboard.current == null"
            );

            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;
        }


        // =================================================
        // P 按下
        // =================================================

        // 步骤 5：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            // 步骤 6：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log(
                // 步骤 7：这一行补充 "========== P 按下 =========="，与相邻行合起来完成同一次计算或对象配置。
                "========== P 按下 =========="
            );

            // 步骤 8：调用 StartAim：StartAim。
            StartAim();
        }


        // =================================================
        // P 按住
        // =================================================

        // 步骤 9：检查条件 isAiming；只有为真才运行下面的步骤，否则沿原流程继续。
        if (isAiming)
        {
            // 步骤 10：调用 UpdateAim：更新Aim。
            UpdateAim();
        }


        // =================================================
        // P 松开
        // =================================================

        // 步骤 11：只在按键松开的那一帧执行一次，用于放下物品。
        if (Keyboard.current.pKey.wasReleasedThisFrame)
        {
            // 步骤 12：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log(
                // 步骤 13：这一行补充 "========== P 松开 =========="，与相邻行合起来完成同一次计算或对象配置。
                "========== P 松开 =========="
            );

            // 步骤 14：检查条件 isAiming；只有为真才运行下面的步骤，否则沿原流程继续。
            if (isAiming)
            {
                // 步骤 15：检查条件 hasValidTarget；只有为真才运行下面的步骤，否则沿原流程继续。
                if (hasValidTarget)
                {
                    // 步骤 16：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
                    Debug.Log(
                        // 步骤 17：这一行补充 "P 松开 → 执行瞄准投掷"，与相邻行合起来完成同一次计算或对象配置。
                        "P 松开 → 执行瞄准投掷"
                    );

                    // 步骤 18：调用 ThrowToTarget：按落点计算抛射方向并把物品投出。
                    ThrowToTarget();
                }
                // 步骤 19：前面的 if 条件为假时执行这里的备用处理。
                else
                {
                    // 步骤 20：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
                    Debug.Log(
                        // 步骤 21：这一行补充 "P 松开，但是没有有效目标"，与相邻行合起来完成同一次计算或对象配置。
                        "P 松开，但是没有有效目标"
                    );

                    // 步骤 22：调用 StopAim：StopAim。
                    StopAim();
                }
            }
        }


        // =================================================
        // Q 普通投掷
        // =================================================

        // 步骤 23：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            // 步骤 24：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log(
                // 步骤 25：这一行补充 "========== Q 按下 =========="，与相邻行合起来完成同一次计算或对象配置。
                "========== Q 按下 =========="
            );

            // 步骤 26：检查条件 IsHoldingMask()；只有为真才运行下面的步骤，否则沿原流程继续。
            if (IsHoldingMask())
            {
                // 步骤 27：调用 ThrowForward：ThrowForward。
                ThrowForward();
            }
            // 步骤 28：前面的 if 条件为假时执行这里的备用处理。
            else
            {
                // 步骤 29：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
                Debug.Log(
                    // 步骤 30：这一行补充 "Q：当前没有检测到鹿头正在手里"，与相邻行合起来完成同一次计算或对象配置。
                    "Q：当前没有检测到鹿头正在手里"
                );
            }
        }
    }
    // ！！！功能结束：Update！！！


    // =====================================================
    // 判断是否拿着鹿头
    // =====================================================

    /// <summary>
    /// 通过父节点或刚体静态状态判断鹿头是否仍被角色拿着。
    /// </summary>
    // ！！！功能：IsHoldingMask——IsHoldingMask！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    bool IsHoldingMask()
    {
        // 步骤 1：检查rb是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (rb == null)
            // 步骤 2：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;

        /*
         * 不再只依赖 isKinematic。
         *
         * 只要：
         *
         * 1. 有 parent
         *
         * 或
         *
         * 2. Rigidbody 是 Kinematic
         *
         * 就认为鹿头目前处于拿取状态。
         */

        // 步骤 3：确认transform的parent已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (transform.parent != null)
        {
            // 步骤 4：本次检测或激活成功，返回 true 让调用方继续相应反馈。
            return true;
        }

        // 步骤 5：检查条件 rb.isKinematic；只有为真才运行下面的步骤，否则沿原流程继续。
        if (rb.isKinematic)
        {
            // 步骤 6：本次检测或激活成功，返回 true 让调用方继续相应反馈。
            return true;
        }

        // 步骤 7：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
        return false;
    }
    // ！！！功能结束：IsHoldingMask！！！


    // =====================================================
    // P：开始瞄准
    // =====================================================

    /// <summary>
    /// 进入瞄准状态，显示预览对象并立即计算第一帧轨迹。
    /// </summary>
    // ！！！功能：StartAim——StartAim！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void StartAim()
    {
        // 步骤 1：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 2：这一行补充 "StartAim() 执行"，与相邻行合起来完成同一次计算或对象配置。
            "StartAim() 执行"
        );

        // 步骤 3：把 isAiming 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        isAiming = true;

        // 步骤 4：把 hasValidTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        hasValidTarget = false;


        // -------------------------------------------------
        // 显示抛物线
        // -------------------------------------------------

        // 步骤 5：确认trajectoryLine已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (trajectoryLine != null)
        {
            // 步骤 6：把 trajectoryLine.enabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            trajectoryLine.enabled = true;
        }


        // -------------------------------------------------
        // 显示落点
        // -------------------------------------------------

        // 步骤 7：确认targetCircle已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (targetCircle != null)
        {
            // 步骤 8：显示这个场景对象及其子对象；界面会立即反映这一状态。
            targetCircle.SetActive(true);
        }


        // -------------------------------------------------
        // ★ 立即计算一次
        // -------------------------------------------------

        // 步骤 9：调用 UpdateAim：更新Aim。
        UpdateAim();


        // 步骤 10：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 11：这一行补充 "开始瞄准完成"，与相邻行合起来完成同一次计算或对象配置。
            "开始瞄准完成"
        );
    }
    // ！！！功能结束：StartAim！！！


    // =====================================================
    // P：持续更新瞄准
    // =====================================================

    /// <summary>
    /// 从屏幕中心找落点，校正到真实地面并计算低弧线或备用轨迹。
    /// </summary>
    // ！！！功能：UpdateAim——更新Aim！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void UpdateAim()
    {
        // 步骤 1：这里的 ! 表示取反；当 isAiming 不成立时才进入此分支。
        if (!isAiming)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;

        // 步骤 3：检查player相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (playerCamera == null)
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;


        // =================================================
        // 从屏幕中央发射 Ray
        // =================================================

        // 步骤 5：这一行补充 Vector3 screenCenter =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 screenCenter =
            // 步骤 6：用 new 创建 Vector3(；构造参数决定新对象的初始内容。
            new Vector3(
                // 步骤 7：继续填写上一行调用的参数：Screen.width * 0.5f；参数位置决定它表示的坐标、速度或选项。
                Screen.width * 0.5f,
                // 步骤 8：继续填写上一行调用的参数：Screen.height * 0.5f；参数位置决定它表示的坐标、速度或选项。
                Screen.height * 0.5f,
                // 步骤 9：这一行补充 0f，与相邻行合起来完成同一次计算或对象配置。
                0f
            );


        // 步骤 10：这一行补充 Ray ray =，与相邻行合起来完成同一次计算或对象配置。
        Ray ray =
            // 步骤 11：调用 ScreenPointToRay：ScreenPoint到射线。
            playerCamera.ScreenPointToRay(
                // 步骤 12：这一行补充 screenCenter，与相邻行合起来完成同一次计算或对象配置。
                screenCenter
            );


        // 步骤 13：声明 hit（hit）：射线命中记录；只供本脚本内部保存状态，赋值前是该类型的默认值。
        RaycastHit hit;


        // 步骤 14：这一行补充 bool hitGround =，与相邻行合起来完成同一次计算或对象配置。
        bool hitGround =
            // 步骤 15：调用 Raycast：Raycast。
            Physics.Raycast(
                // 步骤 16：继续填写上一行调用的参数：ray；参数位置决定它表示的坐标、速度或选项。
                ray,
                // 步骤 17：继续填写上一行调用的参数：out hit；参数位置决定它表示的坐标、速度或选项。
                out hit,
                // 步骤 18：继续填写上一行调用的参数：maxAimDistance；参数位置决定它表示的坐标、速度或选项。
                maxAimDistance,
                // 步骤 19：这一行补充 groundLayer，与相邻行合起来完成同一次计算或对象配置。
                groundLayer
            );


        // =================================================
        // 情况 1：成功打到 Ground
        // =================================================

        // 步骤 20：检查条件 hitGround；只有为真才运行下面的步骤，否则沿原流程继续。
        if (hitGround)
        {
            // 步骤 21：这一行补充 currentTarget =，与相邻行合起来完成同一次计算或对象配置。
            currentTarget =
                // 步骤 22：执行 hit.point;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                hit.point;

            // 步骤 23：调用 DrawLine：DrawLine。
            Debug.DrawLine(
                // 步骤 24：继续填写上一行调用的参数：transform.position；参数位置决定它表示的坐标、速度或选项。
                transform.position,
                // 步骤 25：继续填写上一行调用的参数：currentTarget；参数位置决定它表示的坐标、速度或选项。
                currentTarget,
                // 步骤 26：这一行补充 Color.cyan，与相邻行合起来完成同一次计算或对象配置。
                Color.cyan
            );

            // 步骤 27：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log(
                // 步骤 28：这一行补充 "瞄准到地面：" +，与相邻行合起来完成同一次计算或对象配置。
                "瞄准到地面：" +
                // 步骤 29：这一行补充 currentTarget，与相邻行合起来完成同一次计算或对象配置。
                currentTarget
            );
        }


        // =================================================
        // 情况 2：没有打到 Ground
        // =================================================

        // 步骤 30：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            /*
             * ★★★★★★★★★★★★★★★★★★★★★★★★★★★
             *
             * 这是这版最重要的修改。
             *
             * 即使 Ray 没打到地面，
             * 也绝对不会让 P 失效。
             *
             * 我们直接在摄像机前方制造一个目标。
             *
             * ★★★★★★★★★★★★★★★★★★★★★★★★★★★
             */


            // 步骤 31：这一行补充 Vector3 forward =，与相邻行合起来完成同一次计算或对象配置。
            Vector3 forward =
                // 步骤 32：执行 playerCamera.transform.forward;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                playerCamera.transform.forward;


            /*
             * 让目标不要飘到非常高的位置。
             *
             * 我们只使用摄像机的水平朝向。
             */

            // 步骤 33：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
            forward.y = 0f;


            // 步骤 34：检查条件 forward.sqrMagnitude < 0.001f；只有为真才运行下面的步骤，否则沿原流程继续。
            if (forward.sqrMagnitude < 0.001f)
            {
                // 步骤 35：这一行补充 forward =，与相邻行合起来完成同一次计算或对象配置。
                forward =
                    // 步骤 36：执行 transform.forward;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    transform.forward;

                // 步骤 37：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
                forward.y = 0f;
            }


            // 步骤 38：调用 Normalize：Normalize。
            forward.Normalize();


            // 步骤 39：这一行补充 currentTarget =，与相邻行合起来完成同一次计算或对象配置。
            currentTarget =
                // 步骤 40：这一行补充 transform.position，与相邻行合起来完成同一次计算或对象配置。
                transform.position
                // 步骤 41：这一行补充 +，与相邻行合起来完成同一次计算或对象配置。
                +
                // 步骤 42：这一行补充 forward *，与相邻行合起来完成同一次计算或对象配置。
                forward *
                // 步骤 43：执行 fallbackAimDistance;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                fallbackAimDistance;


            /*
             * 让落点高度稍微低于鹿头。
             *
             * 这样不会再出现
             * “抛物线飞到天上”的情况。
             */

            // 步骤 44：这一行补充 currentTarget.y =，与相邻行合起来完成同一次计算或对象配置。
            currentTarget.y =
                // 步骤 45：执行 transform.position.y;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                transform.position.y;


            // 步骤 46：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log(
                // 步骤 47：这一行补充 "没有检测到 Ground，使用备用落点：" +，与相邻行合起来完成同一次计算或对象配置。
                "没有检测到 Ground，使用备用落点：" +
                // 步骤 48：这一行补充 currentTarget，与相邻行合起来完成同一次计算或对象配置。
                currentTarget
            );
        }

        // Always resolve the marker against a real upward-facing surface.
        // This prevents the circle from floating at the held object's height
        // or being drawn vertically on a wall.
        // 步骤 49：检查条件 TryFindGroundBelow(currentTarget, out Vector3 groundedPoint, out Vector3 groundedNormal)；只有为真才运行下面的步骤，否则沿原流程继续。
        if (TryFindGroundBelow(currentTarget, out Vector3 groundedPoint, out Vector3 groundedNormal))
        {
            // 步骤 50：把计算结果 groundedPoint 保存为currentTarget；下一条判断或调用会读取这个具体值。
            currentTarget = groundedPoint;
            // 步骤 51：把计算结果 groundedNormal 保存为currentTargetNormal；下一条判断或调用会读取这个具体值。
            currentTargetNormal = groundedNormal;
        }
        // 步骤 52：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 53：把 hasValidTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            hasValidTarget = false;
            // 步骤 54：调用 HideAimVisuals：HideAimVisuals。
            HideAimVisuals();
            // 步骤 55：提前结束这个方法；不再执行下方语句。
            return;
        }


        // =================================================
        // 计算低弧线
        // =================================================

        // 步骤 56：声明 velocity（velocity）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
        Vector3 velocity;

        // 步骤 57：声明 flightTime（flightTime）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
        float flightTime;


        // 步骤 58：这一行补充 bool success =，与相邻行合起来完成同一次计算或对象配置。
        bool success =
            // 步骤 59：调用 CalculateLowArcVelocity：CalculateLowArcVelocity。
            CalculateLowArcVelocity(
                // 步骤 60：继续填写上一行调用的参数：transform.position；参数位置决定它表示的坐标、速度或选项。
                transform.position,
                // 步骤 61：继续填写上一行调用的参数：currentTarget；参数位置决定它表示的坐标、速度或选项。
                currentTarget,
                // 步骤 62：继续填写上一行调用的参数：out velocity；参数位置决定它表示的坐标、速度或选项。
                out velocity,
                // 步骤 63：这一行补充 out flightTime，与相邻行合起来完成同一次计算或对象配置。
                out flightTime
            );


        // =================================================
        // 计算成功
        // =================================================

        // 步骤 64：检查条件 success；只有为真才运行下面的步骤，否则沿原流程继续。
        if (success)
        {
            // 步骤 65：这一行补充 currentLaunchVelocity =，与相邻行合起来完成同一次计算或对象配置。
            currentLaunchVelocity =
                // 步骤 66：执行 velocity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                velocity;

            // 步骤 67：这一行补充 currentFlightTime =，与相邻行合起来完成同一次计算或对象配置。
            currentFlightTime =
                // 步骤 68：执行 flightTime;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                flightTime;

            // 步骤 69：这一行补充 hasValidTarget =，与相邻行合起来完成同一次计算或对象配置。
            hasValidTarget =
                // 步骤 70：执行 true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                true;


            // -------------------------------------------------
            // 显示落点
            // -------------------------------------------------

            // 步骤 71：调用 UpdateTargetCircle：更新TargetCircle。
            UpdateTargetCircle(
                // 步骤 72：这一行补充 currentTarget，与相邻行合起来完成同一次计算或对象配置。
                currentTarget
            );


            // -------------------------------------------------
            // 绘制抛物线
            // -------------------------------------------------

            // 步骤 73：调用 DrawTrajectory：DrawTrajectory。
            DrawTrajectory(
                // 步骤 74：继续填写上一行调用的参数：transform.position；参数位置决定它表示的坐标、速度或选项。
                transform.position,
                // 步骤 75：继续填写上一行调用的参数：velocity；参数位置决定它表示的坐标、速度或选项。
                velocity,
                // 步骤 76：这一行补充 flightTime，与相邻行合起来完成同一次计算或对象配置。
                flightTime
            );
        }


        // =================================================
        // 计算失败
        // =================================================

        // 步骤 77：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 78：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogWarning(
                // 步骤 79：这一行补充 "CalculateLowArcVelocity 失败"，与相邻行合起来完成同一次计算或对象配置。
                "CalculateLowArcVelocity 失败"
            );

            /*
             * 即使弹道公式失败，
             * 也使用备用的简单弹道。
             */

            // 步骤 80：调用 CreateFallbackTrajectory：创建FallbackTrajectory。
            CreateFallbackTrajectory();
        }
    }
    // ！！！功能结束：UpdateAim！！！


    // =====================================================
    // ★ 备用弹道
    // =====================================================

    /// <summary>
    /// 低弧线求解失败时按目标距离估算飞行时间和可用的发射速度。
    /// </summary>
    // ！！！功能：CreateFallbackTrajectory——创建FallbackTrajectory！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void CreateFallbackTrajectory()
    {
        // 步骤 1：这一行补充 Vector3 start =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 start =
            // 步骤 2：执行 transform.position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            transform.position;


        // 步骤 3：这一行补充 Vector3 direction =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 direction =
            // 步骤 4：这一行补充 currentTarget -，与相邻行合起来完成同一次计算或对象配置。
            currentTarget -
            // 步骤 5：执行 start;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            start;


        // 步骤 6：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
        direction.y = 0f;


        // 步骤 7：检查条件 direction.sqrMagnitude < 0.001f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (direction.sqrMagnitude < 0.001f)
        {
            // 步骤 8：这一行补充 direction =，与相邻行合起来完成同一次计算或对象配置。
            direction =
                // 步骤 9：执行 playerCamera.transform.forward;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                playerCamera.transform.forward;

            // 步骤 10：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
            direction.y = 0f;

            // 步骤 11：调用 Normalize：Normalize。
            direction.Normalize();
        }
        // 步骤 12：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 13：调用 Normalize：Normalize。
            direction.Normalize();
        }


        // -------------------------------------------------
        // ★ 强制一个稳定的低弧线速度
        // -------------------------------------------------

        // 步骤 14：把原三维朝向投影到地面平面并存为 distance；行走或转向不受头部俯仰影响。
        float distance = Vector3.ProjectOnPlane(currentTarget - start, Vector3.up).magnitude;
        // 步骤 15：计算currentFlightTime并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        currentFlightTime = Mathf.Clamp(distance / 8f, .55f, 1.25f);
        // 步骤 16：计算 currentTarget - start，把结果存进 displacement（displacement）；之后引用 displacement 时用的就是这次结果。
        Vector3 displacement = currentTarget - start;
        // 步骤 17：把原三维朝向投影到地面平面并存为 horizontalDisplacement；行走或转向不受头部俯仰影响。
        Vector3 horizontalDisplacement = Vector3.ProjectOnPlane(displacement, Vector3.up);
        // 步骤 18：计算 (displacement.y - .5f * Physics.gravity.y * currentFlightTime * currentFlightTime) / curre，把结果存进 verticalSpeed（当前竖直移动速度）；之后引用 verticalSpeed 时用的就是这次结果。
        float verticalSpeed = (displacement.y - .5f * Physics.gravity.y * currentFlightTime * currentFlightTime) / currentFlightTime;
        // 步骤 19：把计算结果 horizontalDisplacement / currentFlightTime + Vector3.up * verticalSpeed 保存为currentLaunchVelocity；下一条判断或调用会读取这个具体值。
        currentLaunchVelocity = horizontalDisplacement / currentFlightTime + Vector3.up * verticalSpeed;


        // 步骤 20：这一行补充 hasValidTarget =，与相邻行合起来完成同一次计算或对象配置。
        hasValidTarget =
            // 步骤 21：执行 true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            true;


        // 步骤 22：调用 DrawTrajectory：DrawTrajectory。
        DrawTrajectory(
            // 步骤 23：继续填写上一行调用的参数：start；参数位置决定它表示的坐标、速度或选项。
            start,
            // 步骤 24：继续填写上一行调用的参数：currentLaunchVelocity；参数位置决定它表示的坐标、速度或选项。
            currentLaunchVelocity,
            // 步骤 25：这一行补充 currentFlightTime，与相邻行合起来完成同一次计算或对象配置。
            currentFlightTime
        );


        // 步骤 26：调用 UpdateTargetCircle：更新TargetCircle。
        UpdateTargetCircle(
            // 步骤 27：这一行补充 currentTarget，与相邻行合起来完成同一次计算或对象配置。
            currentTarget
        );


        // 步骤 28：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 29：这一行补充 "使用备用弹道"，与相邻行合起来完成同一次计算或对象配置。
            "使用备用弹道"
        );
    }
    // ！！！功能结束：CreateFallbackTrajectory！！！


    // =====================================================
    // Q：普通投掷
    // =====================================================

    /// <summary>
    /// Q 键直接投掷：脱离手部、恢复物理，赋予镜头朝向速度。
    /// </summary>
    // ！！！功能：ThrowForward——ThrowForward！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void ThrowForward()
    {
        // 步骤 1：检查rb是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (rb == null)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;

        // 步骤 3：调用 StopAim：StopAim。
        StopAim();


        // -------------------------------------------------
        // 脱离手
        // -------------------------------------------------

        // 步骤 4：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        transform.SetParent(null);


        // -------------------------------------------------
        // 开启物理
        // -------------------------------------------------

        // 步骤 5：把 rb.isKinematic 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        rb.isKinematic = false;

        // 步骤 6：把 rb.useGravity 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        rb.useGravity = true;


        // -------------------------------------------------
        // 清除速度
        // -------------------------------------------------

        // 步骤 7：这一行补充 rb.linearVelocity =，与相邻行合起来完成同一次计算或对象配置。
        rb.linearVelocity =
            // 步骤 8：执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.zero;

        // 步骤 9：这一行补充 rb.angularVelocity =，与相邻行合起来完成同一次计算或对象配置。
        rb.angularVelocity =
            // 步骤 10：执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.zero;


        // -------------------------------------------------
        // 摄像机前方
        // -------------------------------------------------

        // 步骤 11：这一行补充 Vector3 velocity =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 velocity =
            // 步骤 12：这一行补充 playerCamera.transform.forward，与相邻行合起来完成同一次计算或对象配置。
            playerCamera.transform.forward
            // 步骤 13：这一行补充 *，与相邻行合起来完成同一次计算或对象配置。
            *
            // 步骤 14：执行 throwForce;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            throwForce;


        // 步骤 15：这一行补充 velocity +=，与相邻行合起来完成同一次计算或对象配置。
        velocity +=
            // 步骤 16：这一行补充 Vector3.up，与相邻行合起来完成同一次计算或对象配置。
            Vector3.up
            // 步骤 17：这一行补充 *，与相邻行合起来完成同一次计算或对象配置。
            *
            // 步骤 18：执行 upwardForce;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            upwardForce;


        // 步骤 19：这一行补充 rb.linearVelocity =，与相邻行合起来完成同一次计算或对象配置。
        rb.linearVelocity =
            // 步骤 20：执行 velocity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            velocity;


        // 步骤 21：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 22：这一行补充 "Q：普通投掷执行"，与相邻行合起来完成同一次计算或对象配置。
            "Q：普通投掷执行"
        );


        // 步骤 23：调用 NotifyMaskDropped：NotifyMaskDropped。
        NotifyMaskDropped();
    }
    // ！！！功能结束：ThrowForward！！！


    // =====================================================
    // P：瞄准投掷
    // =====================================================

    /// <summary>
    /// P 键瞄准投掷：使用预览阶段保存的发射速度，确保实际轨迹匹配预览。
    /// </summary>
    // ！！！功能：ThrowToTarget——按落点计算抛射方向并把物品投出！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void ThrowToTarget()
    {
        // 步骤 1：检查rb是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (rb == null)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;


        // 步骤 3：这里的 ! 表示取反；当 hasValidTarget 不成立时才进入此分支。
        if (!hasValidTarget)
        {
            // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogWarning(
                // 步骤 5：这一行补充 "ThrowToTarget：没有目标"，与相邻行合起来完成同一次计算或对象配置。
                "ThrowToTarget：没有目标"
            );

            // 步骤 6：调用 StopAim：StopAim。
            StopAim();

            // 步骤 7：提前结束这个方法；不再执行下方语句。
            return;
        }


        // -------------------------------------------------
        // 保存速度
        // -------------------------------------------------

        // 步骤 8：这一行补充 Vector3 velocity =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 velocity =
            // 步骤 9：执行 currentLaunchVelocity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            currentLaunchVelocity;


        // 步骤 10：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 11：这一行补充 "投掷速度：" +，与相邻行合起来完成同一次计算或对象配置。
            "投掷速度：" +
            // 步骤 12：这一行补充 velocity，与相邻行合起来完成同一次计算或对象配置。
            velocity
        );


        // -------------------------------------------------
        // 停止瞄准
        // -------------------------------------------------

        // 步骤 13：调用 StopAim：StopAim。
        StopAim();


        // -------------------------------------------------
        // 脱离手
        // -------------------------------------------------

        // 步骤 14：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        transform.SetParent(null);


        // -------------------------------------------------
        // 开启物理
        // -------------------------------------------------

        // 步骤 15：把 rb.isKinematic 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        rb.isKinematic = false;

        // 步骤 16：把 rb.useGravity 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        rb.useGravity = true;


        // -------------------------------------------------
        // 清除旧速度
        // -------------------------------------------------

        // 步骤 17：这一行补充 rb.linearVelocity =，与相邻行合起来完成同一次计算或对象配置。
        rb.linearVelocity =
            // 步骤 18：执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.zero;

        // 步骤 19：这一行补充 rb.angularVelocity =，与相邻行合起来完成同一次计算或对象配置。
        rb.angularVelocity =
            // 步骤 20：执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.zero;


        // -------------------------------------------------
        // ★ 发射
        // -------------------------------------------------

        // 步骤 21：这一行补充 rb.linearVelocity =，与相邻行合起来完成同一次计算或对象配置。
        rb.linearVelocity =
            // 步骤 22：执行 velocity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            velocity;


        // 步骤 23：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log(
            // 步骤 24：这一行补充 "P：鹿头已经发射"，与相邻行合起来完成同一次计算或对象配置。
            "P：鹿头已经发射"
        );


        // 步骤 25：调用 NotifyMaskDropped：NotifyMaskDropped。
        NotifyMaskDropped();
    }
    // ！！！功能结束：ThrowToTarget！！！


    // =====================================================
    // 计算低抛速度
    // =====================================================

    /// <summary>
    /// 根据起点、落点、重力和设定仰角求初速度与飞行时间；不可达时返回 false。
    /// </summary>
    // 这一行补充 bool CalculateLowArcVelocity(，与相邻行合起来完成同一次计算或对象配置。
    bool CalculateLowArcVelocity(
        // 继续填写上一行调用的参数：Vector3 start；参数位置决定它表示的坐标、速度或选项。
        Vector3 start,
        // 继续填写上一行调用的参数：Vector3 target；参数位置决定它表示的坐标、速度或选项。
        Vector3 target,
        // 继续填写上一行调用的参数：out Vector3 velocity；参数位置决定它表示的坐标、速度或选项。
        out Vector3 velocity,
        // 这一行补充 out float flightTime)，与相邻行合起来完成同一次计算或对象配置。
        out float flightTime)
    {
        // 这一行补充 velocity =，与相邻行合起来完成同一次计算或对象配置。
        velocity =
            // 执行 Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.zero;

        // 这一行补充 flightTime =，与相邻行合起来完成同一次计算或对象配置。
        flightTime =
            // 执行 0f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            0f;


        // -------------------------------------------------
        // 位移
        // -------------------------------------------------

        // 这一行补充 Vector3 displacement =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 displacement =
            // 这一行补充 target -，与相邻行合起来完成同一次计算或对象配置。
            target -
            // 执行 start;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            start;


        // -------------------------------------------------
        // 水平距离
        // -------------------------------------------------

        // 这一行补充 Vector3 horizontal =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 horizontal =
            // 用 new 创建 Vector3(；构造参数决定新对象的初始内容。
            new Vector3(
                // 继续填写上一行调用的参数：displacement.x；参数位置决定它表示的坐标、速度或选项。
                displacement.x,
                // 继续填写上一行调用的参数：0f；参数位置决定它表示的坐标、速度或选项。
                0f,
                // 这一行补充 displacement.z，与相邻行合起来完成同一次计算或对象配置。
                displacement.z
            );


        // 这一行补充 float distance =，与相邻行合起来完成同一次计算或对象配置。
        float distance =
            // 执行 horizontal.magnitude;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            horizontal.magnitude;


        // 检查条件 distance < 0.1f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (distance < 0.1f)
        {
            // 本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;
        }


        // -------------------------------------------------
        // 角度
        // -------------------------------------------------

        // 这一行补充 float angle =，与相邻行合起来完成同一次计算或对象配置。
        float angle =
            // 这一行补充 aimAngle *，与相邻行合起来完成同一次计算或对象配置。
            aimAngle *
            // 执行 Mathf.Deg2Rad;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Mathf.Deg2Rad;


        // 这一行补充 float gravity =，与相邻行合起来完成同一次计算或对象配置。
        float gravity =
            // 调用 Abs：Abs。
            Mathf.Abs(
                // 这一行补充 Physics.gravity.y，与相邻行合起来完成同一次计算或对象配置。
                Physics.gravity.y
            );


        // 这一行补充 float heightDifference =，与相邻行合起来完成同一次计算或对象配置。
        float heightDifference =
            // 执行 displacement.y;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            displacement.y;


        // 这一行补充 float cos =，与相邻行合起来完成同一次计算或对象配置。
        float cos =
            // 调用 Cos：Cos。
            Mathf.Cos(angle);


        // 这一行补充 float tan =，与相邻行合起来完成同一次计算或对象配置。
        float tan =
            // 调用 Tan：Tan。
            Mathf.Tan(angle);


        // -------------------------------------------------
        // 低抛公式
        // -------------------------------------------------

        // 这一行补充 float denominator =，与相邻行合起来完成同一次计算或对象配置。
        float denominator =
            // 这一行补充 2f *，与相邻行合起来完成同一次计算或对象配置。
            2f *
            // 这一行补充 cos *，与相邻行合起来完成同一次计算或对象配置。
            cos *
            // 这一行补充 cos *，与相邻行合起来完成同一次计算或对象配置。
            cos *
            // 这一行补充 (，与相邻行合起来完成同一次计算或对象配置。
            (
                // 这一行补充 distance *，与相邻行合起来完成同一次计算或对象配置。
                distance *
                // 这一行补充 tan，与相邻行合起来完成同一次计算或对象配置。
                tan
                // 这一行补充 -，与相邻行合起来完成同一次计算或对象配置。
                -
                // 这一行补充 heightDifference，与相邻行合起来完成同一次计算或对象配置。
                heightDifference
            );


        // 检查条件 denominator <= 0f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (denominator <= 0f)
        {
            // 本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;
        }


        // 这一行补充 float speedSquared =，与相邻行合起来完成同一次计算或对象配置。
        float speedSquared =
            // 这一行补充 gravity *，与相邻行合起来完成同一次计算或对象配置。
            gravity *
            // 这一行补充 distance *，与相邻行合起来完成同一次计算或对象配置。
            distance *
            // 这一行补充 distance，与相邻行合起来完成同一次计算或对象配置。
            distance
            // 这一行补充 /，与相邻行合起来完成同一次计算或对象配置。
            /
            // 执行 denominator;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            denominator;


        // 检查条件 speedSquared <= 0f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (speedSquared <= 0f)
        {
            // 本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;
        }


        // 这一行补充 float speed =，与相邻行合起来完成同一次计算或对象配置。
        float speed =
            // 调用 Sqrt：Sqrt。
            Mathf.Sqrt(
                // 这一行补充 speedSquared，与相邻行合起来完成同一次计算或对象配置。
                speedSquared
            );


        // -------------------------------------------------
        // 水平方向
        // -------------------------------------------------

        // 这一行补充 Vector3 direction =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 direction =
            // 执行 horizontal.normalized;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            horizontal.normalized;


        // 这一行补充 Vector3 horizontalVelocity =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 horizontalVelocity =
            // 这一行补充 direction *，与相邻行合起来完成同一次计算或对象配置。
            direction *
            // 这一行补充 speed *，与相邻行合起来完成同一次计算或对象配置。
            speed *
            // 执行 cos;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            cos;


        // -------------------------------------------------
        // 垂直方向
        // -------------------------------------------------

        // 这一行补充 Vector3 verticalVelocity =，与相邻行合起来完成同一次计算或对象配置。
        Vector3 verticalVelocity =
            // 这一行补充 Vector3.up *，与相邻行合起来完成同一次计算或对象配置。
            Vector3.up *
            // 这一行补充 speed *，与相邻行合起来完成同一次计算或对象配置。
            speed *
            // 调用 Sin：Sin。
            Mathf.Sin(angle);


        // -------------------------------------------------
        // 最终速度
        // -------------------------------------------------

        // 这一行补充 velocity =，与相邻行合起来完成同一次计算或对象配置。
        velocity =
            // 这一行补充 horizontalVelocity，与相邻行合起来完成同一次计算或对象配置。
            horizontalVelocity
            // 这一行补充 +，与相邻行合起来完成同一次计算或对象配置。
            +
            // 执行 verticalVelocity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            verticalVelocity;


        // -------------------------------------------------
        // 飞行时间
        // -------------------------------------------------

        // 这一行补充 float horizontalSpeed =，与相邻行合起来完成同一次计算或对象配置。
        float horizontalSpeed =
            // 执行 horizontalVelocity.magnitude;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            horizontalVelocity.magnitude;


        // 检查条件 horizontalSpeed < 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (horizontalSpeed < 0.01f)
        {
            // 本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;
        }


        // 这一行补充 flightTime =，与相邻行合起来完成同一次计算或对象配置。
        flightTime =
            // 这一行补充 distance /，与相邻行合起来完成同一次计算或对象配置。
            distance /
            // 执行 horizontalSpeed;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            horizontalSpeed;


        // 本次检测或激活成功，返回 true 让调用方继续相应反馈。
        return true;
    }


    // =====================================================
    // 绘制抛物线
    // =====================================================

    /// <summary>
    /// 用计算出的初速度逐点采样抛物线并更新 LineRenderer。
    /// </summary>
    // 这一行补充 void DrawTrajectory(，与相邻行合起来完成同一次计算或对象配置。
    void DrawTrajectory(
        // 继续填写上一行调用的参数：Vector3 start；参数位置决定它表示的坐标、速度或选项。
        Vector3 start,
        // 继续填写上一行调用的参数：Vector3 velocity；参数位置决定它表示的坐标、速度或选项。
        Vector3 velocity,
        // 这一行补充 float flightTime)，与相邻行合起来完成同一次计算或对象配置。
        float flightTime)
    {
        // 检查trajectoryLine是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (trajectoryLine == null)
        {
            // 把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError(
                // 这一行补充 "trajectoryLine == null"，与相邻行合起来完成同一次计算或对象配置。
                "trajectoryLine == null"
            );

            // 提前结束这个方法；不再执行下方语句。
            return;
        }


        // 这一行补充 trajectoryLine.enabled =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.enabled =
            // 执行 true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            true;


        // 这一行补充 trajectoryLine.positionCount =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.positionCount =
            // 执行 trajectoryPoints;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            trajectoryPoints;


        // 按  变化索引；循环体依次处理每个位置。
        for (
            // 把数值 0 暂存为 i；它只在当前方法里参与后续判断或计算。
            int i = 0;
            // 执行 i < trajectoryPoints;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            i < trajectoryPoints;
            // 这一行补充 i++，与相邻行合起来完成同一次计算或对象配置。
            i++
        )
        {
            // 这一行补充 float t =，与相邻行合起来完成同一次计算或对象配置。
            float t =
                // 这一行补充 flightTime，与相邻行合起来完成同一次计算或对象配置。
                flightTime
                // 这一行补充 *，与相邻行合起来完成同一次计算或对象配置。
                *
                // 这一行补充 i，与相邻行合起来完成同一次计算或对象配置。
                i
                // 这一行补充 /，与相邻行合起来完成同一次计算或对象配置。
                /
                // 执行 (trajectoryPoints - 1);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                (trajectoryPoints - 1);


            // 这一行补充 Vector3 point =，与相邻行合起来完成同一次计算或对象配置。
            Vector3 point =
                // 这一行补充 start，与相邻行合起来完成同一次计算或对象配置。
                start
                // 这一行补充 +，与相邻行合起来完成同一次计算或对象配置。
                +
                // 这一行补充 velocity *，与相邻行合起来完成同一次计算或对象配置。
                velocity *
                // 这一行补充 t，与相邻行合起来完成同一次计算或对象配置。
                t
                // 这一行补充 +，与相邻行合起来完成同一次计算或对象配置。
                +
                // 这一行补充 0.5f *，与相邻行合起来完成同一次计算或对象配置。
                0.5f *
                // 这一行补充 Physics.gravity *，与相邻行合起来完成同一次计算或对象配置。
                Physics.gravity *
                // 这一行补充 t *，与相邻行合起来完成同一次计算或对象配置。
                t *
                // 执行 t;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                t;


            // 设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
            trajectoryLine.SetPosition(
                // 继续填写上一行调用的参数：i；参数位置决定它表示的坐标、速度或选项。
                i,
                // 这一行补充 point，与相邻行合起来完成同一次计算或对象配置。
                point
            );
        }
    }


    // =====================================================
    // 创建抛物线
    // =====================================================

    /// <summary>
    /// 创建或配置用于显示预测飞行路径的 LineRenderer。
    /// </summary>
    // ！！！功能：CreateTrajectory——创建Trajectory！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void CreateTrajectory()
    {
        // 步骤 1：这一行补充 GameObject lineObject =，与相邻行合起来完成同一次计算或对象配置。
        GameObject lineObject =
            // 步骤 2：用 new 创建 GameObject(；构造参数决定新对象的初始内容。
            new GameObject(
                // 步骤 3：这一行补充 "ThrowTrajectory"，与相邻行合起来完成同一次计算或对象配置。
                "ThrowTrajectory"
            );


        // 步骤 4：这一行补充 trajectoryLine =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine =
            // 步骤 5：这一行补充 lineObject.AddComponent<，与相邻行合起来完成同一次计算或对象配置。
            lineObject.AddComponent<
                // 步骤 6：这一行补充 LineRenderer，与相邻行合起来完成同一次计算或对象配置。
                LineRenderer
            // 步骤 7：执行 >();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            >();


        // 步骤 8：这一行补充 trajectoryLine.positionCount =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.positionCount =
            // 步骤 9：执行 trajectoryPoints;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            trajectoryPoints;


        // -------------------------------------------------
        // ★ 非常粗
        // -------------------------------------------------

        // 步骤 10：这一行补充 trajectoryLine.startWidth =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.startWidth =
            // 步骤 11：执行 trajectoryWidth;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            trajectoryWidth;

        // 步骤 12：这一行补充 trajectoryLine.endWidth =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.endWidth =
            // 步骤 13：执行 trajectoryWidth;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            trajectoryWidth;


        // -------------------------------------------------
        // ★ 材质
        // -------------------------------------------------

        // 步骤 14：这一行补充 Shader shader =，与相邻行合起来完成同一次计算或对象配置。
        Shader shader =
            // 步骤 15：调用 Find：查找。
            Shader.Find(
                // 步骤 16：这一行补充 "Universal Render Pipeline/Unlit"，与相邻行合起来完成同一次计算或对象配置。
                "Universal Render Pipeline/Unlit"
            );


        // 步骤 17：检查shader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (shader == null)
        {
            // 步骤 18：这一行补充 shader =，与相邻行合起来完成同一次计算或对象配置。
            shader =
                // 步骤 19：调用 Find：查找。
                Shader.Find(
                    // 步骤 20：这一行补充 "Sprites/Default"，与相邻行合起来完成同一次计算或对象配置。
                    "Sprites/Default"
                );
        }


        // 步骤 21：这一行补充 Material material =，与相邻行合起来完成同一次计算或对象配置。
        Material material =
            // 步骤 22：用 new 创建 Material(shader);；构造参数决定新对象的初始内容。
            new Material(shader);


        // Match the warm, readable trajectory used by all exchanged props.
        // 步骤 23：创建新的 Color 并存为 trajectoryColor；构造参数决定其初始内容或对象名称。
        Color trajectoryColor = new Color(1f, .9f, .55f, 1f);

        // 步骤 24：这一行补充 material.color =，与相邻行合起来完成同一次计算或对象配置。
        material.color =
            // 步骤 25：执行 trajectoryColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            trajectoryColor;


        // 步骤 26：检查条件 material.HasProperty("_BaseColor")；只有为真才运行下面的步骤，否则沿原流程继续。
        if (material.HasProperty("_BaseColor"))
        {
            // 步骤 27：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
            material.SetColor(
                // 步骤 28：继续填写上一行调用的参数："_BaseColor"；参数位置决定它表示的坐标、速度或选项。
                "_BaseColor",
                // 步骤 29：这一行补充 trajectoryColor，与相邻行合起来完成同一次计算或对象配置。
                trajectoryColor
            );
        }


        // 步骤 30：这一行补充 trajectoryLine.material =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.material =
            // 步骤 31：执行 material;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            material;


        // 步骤 32：这一行补充 trajectoryLine.startColor =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.startColor =
            // 步骤 33：执行 trajectoryColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            trajectoryColor;

        // 步骤 34：这一行补充 trajectoryLine.endColor =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.endColor =
            // 步骤 35：执行 trajectoryColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            trajectoryColor;


        // -------------------------------------------------
        // ★ 不投射阴影
        // -------------------------------------------------

        // 步骤 36：这一行补充 trajectoryLine.shadowCastingMode =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.shadowCastingMode =
            // 步骤 37：执行 UnityEngine.Rendering.ShadowCastingMode.Off;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            UnityEngine.Rendering.ShadowCastingMode.Off;

        // 步骤 38：这一行补充 trajectoryLine.receiveShadows =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.receiveShadows =
            // 步骤 39：执行 false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            false;


        // -------------------------------------------------
        // ★ 圆角
        // -------------------------------------------------

        // 步骤 40：这一行补充 trajectoryLine.numCornerVertices =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.numCornerVertices =
            // 步骤 41：执行 12;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            12;

        // 步骤 42：这一行补充 trajectoryLine.numCapVertices =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.numCapVertices =
            // 步骤 43：执行 12;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            12;


        // 步骤 44：这一行补充 trajectoryLine.alignment =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.alignment =
            // 步骤 45：执行 LineAlignment.View;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            LineAlignment.View;


        // 步骤 46：这一行补充 trajectoryLine.textureMode =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.textureMode =
            // 步骤 47：执行 LineTextureMode.Stretch;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            LineTextureMode.Stretch;


        // 步骤 48：这一行补充 trajectoryLine.enabled =，与相邻行合起来完成同一次计算或对象配置。
        trajectoryLine.enabled =
            // 步骤 49：执行 false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            false;
    }
    // ！！！功能结束：CreateTrajectory！！！


    // =====================================================
    // 创建落点圆
    // =====================================================

    /// <summary>
    /// 创建目标落点圆环及其渲染组件。
    /// </summary>
    // ！！！功能：CreateTargetCircle——创建TargetCircle！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void CreateTargetCircle()
    {
        // 步骤 1：这一行补充 targetCircle =，与相邻行合起来完成同一次计算或对象配置。
        targetCircle =
            // 步骤 2：用 new 创建 GameObject(；构造参数决定新对象的初始内容。
            new GameObject(
                // 步骤 3：这一行补充 "ThrowTargetCircle"，与相邻行合起来完成同一次计算或对象配置。
                "ThrowTargetCircle"
            );


        // 步骤 4：这一行补充 LineRenderer line =，与相邻行合起来完成同一次计算或对象配置。
        LineRenderer line =
            // 步骤 5：这一行补充 targetCircle.AddComponent<，与相邻行合起来完成同一次计算或对象配置。
            targetCircle.AddComponent<
                // 步骤 6：这一行补充 LineRenderer，与相邻行合起来完成同一次计算或对象配置。
                LineRenderer
            // 步骤 7：执行 >();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            >();


        // 步骤 8：这一行补充 line.loop =，与相邻行合起来完成同一次计算或对象配置。
        line.loop =
            // 步骤 9：执行 true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            true;


        // 步骤 10：这一行补充 line.positionCount =，与相邻行合起来完成同一次计算或对象配置。
        line.positionCount =
            // 步骤 11：执行 targetCircleSegments;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            targetCircleSegments;


        // 步骤 12：这一行补充 line.startWidth =，与相邻行合起来完成同一次计算或对象配置。
        line.startWidth =
            // 步骤 13：执行 targetCircleWidth;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            targetCircleWidth;

        // 步骤 14：这一行补充 line.endWidth =，与相邻行合起来完成同一次计算或对象配置。
        line.endWidth =
            // 步骤 15：执行 targetCircleWidth;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            targetCircleWidth;


        // -------------------------------------------------
        // 材质
        // -------------------------------------------------

        // 步骤 16：这一行补充 Shader shader =，与相邻行合起来完成同一次计算或对象配置。
        Shader shader =
            // 步骤 17：调用 Find：查找。
            Shader.Find(
                // 步骤 18：这一行补充 "Universal Render Pipeline/Unlit"，与相邻行合起来完成同一次计算或对象配置。
                "Universal Render Pipeline/Unlit"
            );


        // 步骤 19：检查shader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (shader == null)
        {
            // 步骤 20：这一行补充 shader =，与相邻行合起来完成同一次计算或对象配置。
            shader =
                // 步骤 21：调用 Find：查找。
                Shader.Find(
                    // 步骤 22：这一行补充 "Sprites/Default"，与相邻行合起来完成同一次计算或对象配置。
                    "Sprites/Default"
                );
        }


        // 步骤 23：这一行补充 Material material =，与相邻行合起来完成同一次计算或对象配置。
        Material material =
            // 步骤 24：用 new 创建 Material(shader);；构造参数决定新对象的初始内容。
            new Material(shader);


        // 步骤 25：这一行补充 material.color =，与相邻行合起来完成同一次计算或对象配置。
        material.color =
            // 步骤 26：执行 Color.yellow;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Color.yellow;


        // 步骤 27：检查条件 material.HasProperty("_BaseColor")；只有为真才运行下面的步骤，否则沿原流程继续。
        if (material.HasProperty("_BaseColor"))
        {
            // 步骤 28：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
            material.SetColor(
                // 步骤 29：继续填写上一行调用的参数："_BaseColor"；参数位置决定它表示的坐标、速度或选项。
                "_BaseColor",
                // 步骤 30：这一行补充 Color.yellow，与相邻行合起来完成同一次计算或对象配置。
                Color.yellow
            );
        }


        // 步骤 31：这一行补充 line.material =，与相邻行合起来完成同一次计算或对象配置。
        line.material =
            // 步骤 32：执行 material;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            material;


        // 步骤 33：这一行补充 line.startColor =，与相邻行合起来完成同一次计算或对象配置。
        line.startColor =
            // 步骤 34：执行 Color.yellow;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Color.yellow;

        // 步骤 35：这一行补充 line.endColor =，与相邻行合起来完成同一次计算或对象配置。
        line.endColor =
            // 步骤 36：执行 Color.yellow;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Color.yellow;


        // 步骤 37：这一行补充 line.numCornerVertices =，与相邻行合起来完成同一次计算或对象配置。
        line.numCornerVertices =
            // 步骤 38：执行 12;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            12;

        // 步骤 39：这一行补充 line.numCapVertices =，与相邻行合起来完成同一次计算或对象配置。
        line.numCapVertices =
            // 步骤 40：执行 12;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            12;


        // 步骤 41：这一行补充 line.shadowCastingMode =，与相邻行合起来完成同一次计算或对象配置。
        line.shadowCastingMode =
            // 步骤 42：执行 UnityEngine.Rendering.ShadowCastingMode.Off;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            UnityEngine.Rendering.ShadowCastingMode.Off;

        // 步骤 43：这一行补充 line.receiveShadows =，与相邻行合起来完成同一次计算或对象配置。
        line.receiveShadows =
            // 步骤 44：执行 false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            false;


        // 步骤 45：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
        targetCircle.SetActive(
            // 步骤 46：这一行补充 false，与相邻行合起来完成同一次计算或对象配置。
            false
        );
    }
    // ！！！功能结束：CreateTargetCircle！！！


    // =====================================================
    // 更新落点圆
    // =====================================================

    /// <summary>
    /// 先尝试指定地面层；找不到时回退到全部层，以取得有效的水平落点。
    /// </summary>
    // ！！！功能：TryFindGroundBelow——向下射线寻找可落脚地面，供投掷落点提示使用！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 candidate, out Vector3 point, out Vector3 normal 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    bool TryFindGroundBelow(Vector3 candidate, out Vector3 point, out Vector3 normal)
    {
        // 步骤 1：用三元表达式为 preferredMask 二选一：问号前条件为真取前值，为假取冒号后的值。
        int preferredMask = groundLayer.value == 0 ? ~0 : groundLayer.value;
        // 步骤 2：检查条件 TryFindGroundBelow(candidate, preferredMask, out point, out normal)；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (TryFindGroundBelow(candidate, preferredMask, out point, out normal)) return true;
        // 步骤 3：这里用 && 要求两边条件同时成立：preferredMask != ~0 && TryFindGroundBelow(candidate, ~0, out point, out normal)；任意一边为假就跳过本分支。 条件成立时立即退出当前方法。
        if (preferredMask != ~0 && TryFindGroundBelow(candidate, ~0, out point, out normal)) return true;
        // 步骤 4：把计算结果 candidate 保存为point；下一条判断或调用会读取这个具体值。
        point = candidate;
        // 步骤 5：把计算结果 Vector3.up 保存为normal；下一条判断或调用会读取这个具体值。
        normal = Vector3.up;
        // 步骤 6：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
        return false;
    }
    // ！！！功能结束：TryFindGroundBelow！！！

    /// <summary>
    /// 向候选点下方发射射线，排除自身和陡峭表面，选最近的有效地面。
    /// </summary>
    // ！！！功能：TryFindGroundBelow——向下射线寻找可落脚地面，供投掷落点提示使用！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 candidate, int mask, out Vector3 point, out Vector3 normal 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    bool TryFindGroundBelow(Vector3 candidate, int mask, out Vector3 point, out Vector3 normal)
    {
        // 步骤 1：把计算结果 candidate 保存为point；下一条判断或调用会读取这个具体值。
        point = candidate;
        // 步骤 2：把计算结果 Vector3.up 保存为normal；下一条判断或调用会读取这个具体值。
        normal = Vector3.up;
        // 步骤 3：计算 float.PositiveInfinity，把结果存进 closest（closest）；之后引用 closest 时用的就是这次结果。
        float closest = float.PositiveInfinity;
        // 步骤 4：计算 candidate + Vector3.up * 3f，把结果存进 origin（origin）；之后引用 origin 时用的就是这次结果。
        Vector3 origin = candidate + Vector3.up * 3f;
        // 步骤 5：逐个读取 RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 12f, mask, QueryTriggerInt 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 12f, mask, QueryTriggerInteraction.Ignore))
        {
            // 步骤 6：检查hit的collider是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (hit.collider == null || hit.normal.y < .45f) continue;
            // 步骤 7：这里用 || 表示任一条件成立就处理：hit.transform == transform || hit.transform.IsChildOf(transform)；用于合并几种停止或跳过原因。 成立时执行 continue;
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            // 步骤 8：检查条件 hit.distance >= closest；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (hit.distance >= closest) continue;
            // 步骤 9：把计算结果 hit.distance 保存为closest；下一条判断或调用会读取这个具体值。
            closest = hit.distance;
            // 步骤 10：把计算结果 hit.point 保存为point；下一条判断或调用会读取这个具体值。
            point = hit.point;
            // 步骤 11：把计算结果 hit.normal.normalized 保存为normal；下一条判断或调用会读取这个具体值。
            normal = hit.normal.normalized;
        }
        // 步骤 12：把 closest < float.PositiveInfinity 作为方法结果返回给调用方；return 后本方法立即结束。
        return closest < float.PositiveInfinity;
    }
    // ！！！功能结束：TryFindGroundBelow！！！

    /// <summary>
    /// 按照地面法线建立圆环平面，使落点提示贴合坡面。
    /// </summary>
    // 这一行补充 void UpdateTargetCircle(，与相邻行合起来完成同一次计算或对象配置。
    void UpdateTargetCircle(
        // 这一行补充 Vector3 target)，与相邻行合起来完成同一次计算或对象配置。
        Vector3 target)
    {
        // 检查targetCircle是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (targetCircle == null)
            // 提前结束这个方法；不再执行下方语句。
            return;


        // 根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
        targetCircle.SetActive(
            // 这一行补充 true，与相邻行合起来完成同一次计算或对象配置。
            true
        );


        // 这一行补充 LineRenderer line =，与相邻行合起来完成同一次计算或对象配置。
        LineRenderer line =
            // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            targetCircle.GetComponent<
                // 这一行补充 LineRenderer，与相邻行合起来完成同一次计算或对象配置。
                LineRenderer
            // 执行 >();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            >();


        // 用三元表达式为 up 二选一：问号前条件为真取前值，为假取冒号后的值。
        Vector3 up = currentTargetNormal.sqrMagnitude > .01f ? currentTargetNormal.normalized : Vector3.up;
        // 通过叉积算出与原方向垂直的向量，存为 tangent 供左右移动使用。
        Vector3 tangent = Vector3.Cross(up, Vector3.forward);
        // 检查条件 tangent.sqrMagnitude < .01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 tangent = Vector3.Cross(up, Vector3.right);
        if (tangent.sqrMagnitude < .01f) tangent = Vector3.Cross(up, Vector3.right);
        // 调用 Normalize：Normalize。
        tangent.Normalize();
        // 通过叉积算出与原方向垂直的向量，存为 bitangent 供左右移动使用。
        Vector3 bitangent = Vector3.Cross(up, tangent).normalized;
        // 计算 target + up * targetCircleHeight，把结果存进 center（center）；之后引用 center 时用的就是这次结果。
        Vector3 center = target + up * targetCircleHeight;

        // 按  变化索引；循环体依次处理每个位置。
        for (
            // 把数值 0 暂存为 i；它只在当前方法里参与后续判断或计算。
            int i = 0;
            // 执行 i < targetCircleSegments;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            i < targetCircleSegments;
            // 这一行补充 i++，与相邻行合起来完成同一次计算或对象配置。
            i++
        )
        {
            // 这一行补充 float angle =，与相邻行合起来完成同一次计算或对象配置。
            float angle =
                // 这一行补充 2f *，与相邻行合起来完成同一次计算或对象配置。
                2f *
                // 这一行补充 Mathf.PI *，与相邻行合起来完成同一次计算或对象配置。
                Mathf.PI *
                // 这一行补充 i，与相邻行合起来完成同一次计算或对象配置。
                i
                // 这一行补充 /，与相邻行合起来完成同一次计算或对象配置。
                /
                // 执行 targetCircleSegments;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                targetCircleSegments;


            // 这一行补充 Vector3 point =，与相邻行合起来完成同一次计算或对象配置。
            Vector3 point =
                // 这一行补充 center，与相邻行合起来完成同一次计算或对象配置。
                center
                // 这一行补充 + tangent * Mathf.Cos(angle) * targetCircleRadius，与相邻行合起来完成同一次计算或对象配置。
                + tangent * Mathf.Cos(angle) * targetCircleRadius
                // 执行 + bitangent * Mathf.Sin(angle) * targetCircleRadius;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                + bitangent * Mathf.Sin(angle) * targetCircleRadius;


            // 设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
            line.SetPosition(
                // 继续填写上一行调用的参数：i；参数位置决定它表示的坐标、速度或选项。
                i,
                // 这一行补充 point，与相邻行合起来完成同一次计算或对象配置。
                point
            );
        }
    }


    // =====================================================
    // 隐藏抛物线
    // =====================================================

    /// <summary>
    /// 关闭预测弹道线的显示。
    /// </summary>
    // ！！！功能：HideTrajectory——HideTrajectory！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void HideTrajectory()
    {
        // 步骤 1：确认trajectoryLine已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (trajectoryLine != null)
        {
            // 步骤 2：这一行补充 trajectoryLine.enabled =，与相邻行合起来完成同一次计算或对象配置。
            trajectoryLine.enabled =
                // 步骤 3：执行 false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                false;
        }
    }
    // ！！！功能结束：HideTrajectory！！！


    // =====================================================
    // 停止瞄准
    // =====================================================

    /// <summary>
    /// 退出瞄准、清除有效目标并隐藏全部预览。
    /// </summary>
    // ！！！功能：StopAim——StopAim！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void StopAim()
    {
        // 步骤 1：这一行补充 isAiming =，与相邻行合起来完成同一次计算或对象配置。
        isAiming =
            // 步骤 2：执行 false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            false;

        // 步骤 3：这一行补充 hasValidTarget =，与相邻行合起来完成同一次计算或对象配置。
        hasValidTarget =
            // 步骤 4：执行 false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            false;

        // 步骤 5：调用 HideAimVisuals：HideAimVisuals。
        HideAimVisuals();
    }
    // ！！！功能结束：StopAim！！！


    // =====================================================
    // 隐藏所有瞄准视觉
    // =====================================================

    /// <summary>
    /// 同时隐藏抛物线与落点圆环。
    /// </summary>
    // ！！！功能：HideAimVisuals——HideAimVisuals！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void HideAimVisuals()
    {
        // 步骤 1：确认trajectoryLine已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (trajectoryLine != null)
        {
            // 步骤 2：这一行补充 trajectoryLine.enabled =，与相邻行合起来完成同一次计算或对象配置。
            trajectoryLine.enabled =
                // 步骤 3：执行 false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                false;
        }


        // 步骤 4：确认targetCircle已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (targetCircle != null)
        {
            // 步骤 5：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            targetCircle.SetActive(
                // 步骤 6：这一行补充 false，与相邻行合起来完成同一次计算或对象配置。
                false
            );
        }
    }
    // ！！！功能结束：HideAimVisuals！！！


    // =====================================================
    // 通知 MaskInteraction
    // =====================================================

    /// <summary>
    /// 通知拾取脚本物品已离手，由它恢复未持有状态。
    /// </summary>
    // ！！！功能：NotifyMaskDropped——NotifyMaskDropped！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void NotifyMaskDropped()
    {
        // 步骤 1：这一行补充 MaskInteraction mask =，与相邻行合起来完成同一次计算或对象配置。
        MaskInteraction mask =
            // 步骤 2：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            GetComponent<
                // 步骤 3：这一行补充 MaskInteraction，与相邻行合起来完成同一次计算或对象配置。
                MaskInteraction
            // 步骤 4：执行 >();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            >();


        // 步骤 5：确认mask已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (mask != null)
        {
            // 步骤 6：调用 OnThrown：OnThrown。
            mask.OnThrown();

            // 步骤 7：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log(
                // 步骤 8：这一行补充 "已通知 MaskInteraction.OnThrown()"，与相邻行合起来完成同一次计算或对象配置。
                "已通知 MaskInteraction.OnThrown()"
            );
        }
        // 步骤 9：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 10：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogWarning(
                // 步骤 11：这一行补充 "找不到 MaskInteraction"，与相邻行合起来完成同一次计算或对象配置。
                "找不到 MaskInteraction"
            );
        }
    }
    // ！！！功能结束：NotifyMaskDropped！！！
}
