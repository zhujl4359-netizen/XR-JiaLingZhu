using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 桌面版鹿头投掷：Q 直接抛出，按住 P 预览低弧线和落点后松开发射。
/// </summary>
public class MaskThrowController : MonoBehaviour
{
    // =====================================================
    // 摄像机
    // =====================================================

    [Header("摄像机")]
    public Camera playerCamera;


    // =====================================================
    // Q：普通投掷
    // =====================================================

    [Header("Q：普通投掷")]

    public float throwForce = 20f;

    public float upwardForce = 2f;


    // =====================================================
    // P：瞄准投掷
    // =====================================================

    [Header("P：瞄准投掷")]

    [Tooltip("最大瞄准距离")]
    public float maxAimDistance = 8f;

    [Tooltip("抛物线角度。越小越平、越远")]
    [Range(5f, 30f)]
    public float aimAngle = 10f;

    [Tooltip("如果没有检测到地面，使用这个距离")]
    public float fallbackAimDistance = 8f;


    // =====================================================
    // 地面
    // =====================================================

    [Header("地面")]

    public LayerMask groundLayer;


    // =====================================================
    // 抛物线
    // =====================================================

    [Header("抛物线")]

    public LineRenderer trajectoryLine;

    [Tooltip("抛物线分段")]
    public int trajectoryPoints = 48;

    [Tooltip("抛物线粗细")]
    public float trajectoryWidth = 0.035f;


    // =====================================================
    // 落点圆圈
    // =====================================================

    [Header("落点圆圈")]

    public float targetCircleRadius = 1.65f;

    public int targetCircleSegments = 60;

    public float targetCircleHeight = 0.018f;

    public float targetCircleWidth = 0.075f;


    // =====================================================
    // 内部变量
    // =====================================================

    private Rigidbody rb;

    private bool isAiming = false;

    private bool hasValidTarget = false;

    private GameObject targetCircle;

    private Vector3 currentTarget;

    private Vector3 currentTargetNormal = Vector3.up;

    private Vector3 currentLaunchVelocity;

    private float currentFlightTime;


    // =====================================================
    // Start
    // =====================================================

    /// <summary>
    /// 规范化旧场景中的瞄准参数，取得刚体/摄像机并准备轨迹和落点预览。
    /// </summary>
    void Start()
    {
        ApplyUnifiedAimStyle();

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(
                "MaskThrowController：没有找到 Rigidbody！"
            );
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null)
        {
            Debug.LogError(
                "MaskThrowController：没有找到 Main Camera！"
            );
        }

        CreateTrajectory();

        CreateTargetCircle();

        HideAimVisuals();

        Debug.Log(
            "MaskThrowController 已启动，物体：" +
            gameObject.name
        );
    }

    // Scene files made before the unified throw preview contain serialized
    // values as long as 45 metres.  Enforce the compact style at runtime too,
    // so old scenes and newly spawned throwables behave identically.
    /// <summary>
    /// 把瞄准距离、角度及预览尺寸约束到当前版本统一的范围。
    /// </summary>
    void ApplyUnifiedAimStyle()
    {
        maxAimDistance = Mathf.Clamp(maxAimDistance, 5f, 8f);
        fallbackAimDistance = Mathf.Clamp(fallbackAimDistance, 5f, 8f);
        aimAngle = Mathf.Clamp(aimAngle, 9f, 16f);
        trajectoryPoints = Mathf.Clamp(trajectoryPoints, 36, 52);
        trajectoryWidth = Mathf.Clamp(trajectoryWidth, .028f, .04f);
        targetCircleRadius = Mathf.Max(targetCircleRadius, 1.65f);
        targetCircleHeight = .018f;
        targetCircleWidth = Mathf.Clamp(targetCircleWidth, .06f, .085f);
    }


    // =====================================================
    // Update
    // =====================================================

    /// <summary>
    /// 处理 P 的按下、持续瞄准和松开发射，以及 Q 的直接投掷。
    /// </summary>
    void Update()
    {
        if (Keyboard.current == null)
        {
            Debug.LogWarning(
                "Keyboard.current == null"
            );

            return;
        }


        // =================================================
        // P 按下
        // =================================================

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log(
                "========== P 按下 =========="
            );

            StartAim();
        }


        // =================================================
        // P 按住
        // =================================================

        if (isAiming)
        {
            UpdateAim();
        }


        // =================================================
        // P 松开
        // =================================================

        if (Keyboard.current.pKey.wasReleasedThisFrame)
        {
            Debug.Log(
                "========== P 松开 =========="
            );

            if (isAiming)
            {
                if (hasValidTarget)
                {
                    Debug.Log(
                        "P 松开 → 执行瞄准投掷"
                    );

                    ThrowToTarget();
                }
                else
                {
                    Debug.Log(
                        "P 松开，但是没有有效目标"
                    );

                    StopAim();
                }
            }
        }


        // =================================================
        // Q 普通投掷
        // =================================================

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log(
                "========== Q 按下 =========="
            );

            if (IsHoldingMask())
            {
                ThrowForward();
            }
            else
            {
                Debug.Log(
                    "Q：当前没有检测到鹿头正在手里"
                );
            }
        }
    }


    // =====================================================
    // 判断是否拿着鹿头
    // =====================================================

    /// <summary>
    /// 通过父节点或刚体静态状态判断鹿头是否仍被角色拿着。
    /// </summary>
    bool IsHoldingMask()
    {
        if (rb == null)
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

        if (transform.parent != null)
        {
            return true;
        }

        if (rb.isKinematic)
        {
            return true;
        }

        return false;
    }


    // =====================================================
    // P：开始瞄准
    // =====================================================

    /// <summary>
    /// 进入瞄准状态，显示预览对象并立即计算第一帧轨迹。
    /// </summary>
    void StartAim()
    {
        Debug.Log(
            "StartAim() 执行"
        );

        isAiming = true;

        hasValidTarget = false;


        // -------------------------------------------------
        // 显示抛物线
        // -------------------------------------------------

        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = true;
        }


        // -------------------------------------------------
        // 显示落点
        // -------------------------------------------------

        if (targetCircle != null)
        {
            targetCircle.SetActive(true);
        }


        // -------------------------------------------------
        // ★ 立即计算一次
        // -------------------------------------------------

        UpdateAim();


        Debug.Log(
            "开始瞄准完成"
        );
    }


    // =====================================================
    // P：持续更新瞄准
    // =====================================================

    /// <summary>
    /// 从屏幕中心找落点，校正到真实地面并计算低弧线或备用轨迹。
    /// </summary>
    void UpdateAim()
    {
        if (!isAiming)
            return;

        if (playerCamera == null)
            return;


        // =================================================
        // 从屏幕中央发射 Ray
        // =================================================

        Vector3 screenCenter =
            new Vector3(
                Screen.width * 0.5f,
                Screen.height * 0.5f,
                0f
            );


        Ray ray =
            playerCamera.ScreenPointToRay(
                screenCenter
            );


        RaycastHit hit;


        bool hitGround =
            Physics.Raycast(
                ray,
                out hit,
                maxAimDistance,
                groundLayer
            );


        // =================================================
        // 情况 1：成功打到 Ground
        // =================================================

        if (hitGround)
        {
            currentTarget =
                hit.point;

            Debug.DrawLine(
                transform.position,
                currentTarget,
                Color.cyan
            );

            Debug.Log(
                "瞄准到地面：" +
                currentTarget
            );
        }


        // =================================================
        // 情况 2：没有打到 Ground
        // =================================================

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


            Vector3 forward =
                playerCamera.transform.forward;


            /*
             * 让目标不要飘到非常高的位置。
             *
             * 我们只使用摄像机的水平朝向。
             */

            forward.y = 0f;


            if (forward.sqrMagnitude < 0.001f)
            {
                forward =
                    transform.forward;

                forward.y = 0f;
            }


            forward.Normalize();


            currentTarget =
                transform.position
                +
                forward *
                fallbackAimDistance;


            /*
             * 让落点高度稍微低于鹿头。
             *
             * 这样不会再出现
             * “抛物线飞到天上”的情况。
             */

            currentTarget.y =
                transform.position.y;


            Debug.Log(
                "没有检测到 Ground，使用备用落点：" +
                currentTarget
            );
        }

        // Always resolve the marker against a real upward-facing surface.
        // This prevents the circle from floating at the held object's height
        // or being drawn vertically on a wall.
        if (TryFindGroundBelow(currentTarget, out Vector3 groundedPoint, out Vector3 groundedNormal))
        {
            currentTarget = groundedPoint;
            currentTargetNormal = groundedNormal;
        }
        else
        {
            hasValidTarget = false;
            HideAimVisuals();
            return;
        }


        // =================================================
        // 计算低弧线
        // =================================================

        Vector3 velocity;

        float flightTime;


        bool success =
            CalculateLowArcVelocity(
                transform.position,
                currentTarget,
                out velocity,
                out flightTime
            );


        // =================================================
        // 计算成功
        // =================================================

        if (success)
        {
            currentLaunchVelocity =
                velocity;

            currentFlightTime =
                flightTime;

            hasValidTarget =
                true;


            // -------------------------------------------------
            // 显示落点
            // -------------------------------------------------

            UpdateTargetCircle(
                currentTarget
            );


            // -------------------------------------------------
            // 绘制抛物线
            // -------------------------------------------------

            DrawTrajectory(
                transform.position,
                velocity,
                flightTime
            );
        }


        // =================================================
        // 计算失败
        // =================================================

        else
        {
            Debug.LogWarning(
                "CalculateLowArcVelocity 失败"
            );

            /*
             * 即使弹道公式失败，
             * 也使用备用的简单弹道。
             */

            CreateFallbackTrajectory();
        }
    }


    // =====================================================
    // ★ 备用弹道
    // =====================================================

    /// <summary>
    /// 低弧线求解失败时按目标距离估算飞行时间和可用的发射速度。
    /// </summary>
    void CreateFallbackTrajectory()
    {
        Vector3 start =
            transform.position;


        Vector3 direction =
            currentTarget -
            start;


        direction.y = 0f;


        if (direction.sqrMagnitude < 0.001f)
        {
            direction =
                playerCamera.transform.forward;

            direction.y = 0f;

            direction.Normalize();
        }
        else
        {
            direction.Normalize();
        }


        // -------------------------------------------------
        // ★ 强制一个稳定的低弧线速度
        // -------------------------------------------------

        float distance = Vector3.ProjectOnPlane(currentTarget - start, Vector3.up).magnitude;
        currentFlightTime = Mathf.Clamp(distance / 8f, .55f, 1.25f);
        Vector3 displacement = currentTarget - start;
        Vector3 horizontalDisplacement = Vector3.ProjectOnPlane(displacement, Vector3.up);
        float verticalSpeed = (displacement.y - .5f * Physics.gravity.y * currentFlightTime * currentFlightTime) / currentFlightTime;
        currentLaunchVelocity = horizontalDisplacement / currentFlightTime + Vector3.up * verticalSpeed;


        hasValidTarget =
            true;


        DrawTrajectory(
            start,
            currentLaunchVelocity,
            currentFlightTime
        );


        UpdateTargetCircle(
            currentTarget
        );


        Debug.Log(
            "使用备用弹道"
        );
    }


    // =====================================================
    // Q：普通投掷
    // =====================================================

    /// <summary>
    /// Q 键直接投掷：脱离手部、恢复物理，赋予镜头朝向速度。
    /// </summary>
    void ThrowForward()
    {
        if (rb == null)
            return;

        StopAim();


        // -------------------------------------------------
        // 脱离手
        // -------------------------------------------------

        transform.SetParent(null);


        // -------------------------------------------------
        // 开启物理
        // -------------------------------------------------

        rb.isKinematic = false;

        rb.useGravity = true;


        // -------------------------------------------------
        // 清除速度
        // -------------------------------------------------

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;


        // -------------------------------------------------
        // 摄像机前方
        // -------------------------------------------------

        Vector3 velocity =
            playerCamera.transform.forward
            *
            throwForce;


        velocity +=
            Vector3.up
            *
            upwardForce;


        rb.linearVelocity =
            velocity;


        Debug.Log(
            "Q：普通投掷执行"
        );


        NotifyMaskDropped();
    }


    // =====================================================
    // P：瞄准投掷
    // =====================================================

    /// <summary>
    /// P 键瞄准投掷：使用预览阶段保存的发射速度，确保实际轨迹匹配预览。
    /// </summary>
    void ThrowToTarget()
    {
        if (rb == null)
            return;


        if (!hasValidTarget)
        {
            Debug.LogWarning(
                "ThrowToTarget：没有目标"
            );

            StopAim();

            return;
        }


        // -------------------------------------------------
        // 保存速度
        // -------------------------------------------------

        Vector3 velocity =
            currentLaunchVelocity;


        Debug.Log(
            "投掷速度：" +
            velocity
        );


        // -------------------------------------------------
        // 停止瞄准
        // -------------------------------------------------

        StopAim();


        // -------------------------------------------------
        // 脱离手
        // -------------------------------------------------

        transform.SetParent(null);


        // -------------------------------------------------
        // 开启物理
        // -------------------------------------------------

        rb.isKinematic = false;

        rb.useGravity = true;


        // -------------------------------------------------
        // 清除旧速度
        // -------------------------------------------------

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;


        // -------------------------------------------------
        // ★ 发射
        // -------------------------------------------------

        rb.linearVelocity =
            velocity;


        Debug.Log(
            "P：鹿头已经发射"
        );


        NotifyMaskDropped();
    }


    // =====================================================
    // 计算低抛速度
    // =====================================================

    /// <summary>
    /// 根据起点、落点、重力和设定仰角求初速度与飞行时间；不可达时返回 false。
    /// </summary>
    bool CalculateLowArcVelocity(
        Vector3 start,
        Vector3 target,
        out Vector3 velocity,
        out float flightTime)
    {
        velocity =
            Vector3.zero;

        flightTime =
            0f;


        // -------------------------------------------------
        // 位移
        // -------------------------------------------------

        Vector3 displacement =
            target -
            start;


        // -------------------------------------------------
        // 水平距离
        // -------------------------------------------------

        Vector3 horizontal =
            new Vector3(
                displacement.x,
                0f,
                displacement.z
            );


        float distance =
            horizontal.magnitude;


        if (distance < 0.1f)
        {
            return false;
        }


        // -------------------------------------------------
        // 角度
        // -------------------------------------------------

        float angle =
            aimAngle *
            Mathf.Deg2Rad;


        float gravity =
            Mathf.Abs(
                Physics.gravity.y
            );


        float heightDifference =
            displacement.y;


        float cos =
            Mathf.Cos(angle);


        float tan =
            Mathf.Tan(angle);


        // -------------------------------------------------
        // 低抛公式
        // -------------------------------------------------

        float denominator =
            2f *
            cos *
            cos *
            (
                distance *
                tan
                -
                heightDifference
            );


        if (denominator <= 0f)
        {
            return false;
        }


        float speedSquared =
            gravity *
            distance *
            distance
            /
            denominator;


        if (speedSquared <= 0f)
        {
            return false;
        }


        float speed =
            Mathf.Sqrt(
                speedSquared
            );


        // -------------------------------------------------
        // 水平方向
        // -------------------------------------------------

        Vector3 direction =
            horizontal.normalized;


        Vector3 horizontalVelocity =
            direction *
            speed *
            cos;


        // -------------------------------------------------
        // 垂直方向
        // -------------------------------------------------

        Vector3 verticalVelocity =
            Vector3.up *
            speed *
            Mathf.Sin(angle);


        // -------------------------------------------------
        // 最终速度
        // -------------------------------------------------

        velocity =
            horizontalVelocity
            +
            verticalVelocity;


        // -------------------------------------------------
        // 飞行时间
        // -------------------------------------------------

        float horizontalSpeed =
            horizontalVelocity.magnitude;


        if (horizontalSpeed < 0.01f)
        {
            return false;
        }


        flightTime =
            distance /
            horizontalSpeed;


        return true;
    }


    // =====================================================
    // 绘制抛物线
    // =====================================================

    /// <summary>
    /// 用计算出的初速度逐点采样抛物线并更新 LineRenderer。
    /// </summary>
    void DrawTrajectory(
        Vector3 start,
        Vector3 velocity,
        float flightTime)
    {
        if (trajectoryLine == null)
        {
            Debug.LogError(
                "trajectoryLine == null"
            );

            return;
        }


        trajectoryLine.enabled =
            true;


        trajectoryLine.positionCount =
            trajectoryPoints;


        for (
            int i = 0;
            i < trajectoryPoints;
            i++
        )
        {
            float t =
                flightTime
                *
                i
                /
                (trajectoryPoints - 1);


            Vector3 point =
                start
                +
                velocity *
                t
                +
                0.5f *
                Physics.gravity *
                t *
                t;


            trajectoryLine.SetPosition(
                i,
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
    void CreateTrajectory()
    {
        GameObject lineObject =
            new GameObject(
                "ThrowTrajectory"
            );


        trajectoryLine =
            lineObject.AddComponent<
                LineRenderer
            >();


        trajectoryLine.positionCount =
            trajectoryPoints;


        // -------------------------------------------------
        // ★ 非常粗
        // -------------------------------------------------

        trajectoryLine.startWidth =
            trajectoryWidth;

        trajectoryLine.endWidth =
            trajectoryWidth;


        // -------------------------------------------------
        // ★ 材质
        // -------------------------------------------------

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );


        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Sprites/Default"
                );
        }


        Material material =
            new Material(shader);


        // Match the warm, readable trajectory used by all exchanged props.
        Color trajectoryColor = new Color(1f, .9f, .55f, 1f);

        material.color =
            trajectoryColor;


        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                trajectoryColor
            );
        }


        trajectoryLine.material =
            material;


        trajectoryLine.startColor =
            trajectoryColor;

        trajectoryLine.endColor =
            trajectoryColor;


        // -------------------------------------------------
        // ★ 不投射阴影
        // -------------------------------------------------

        trajectoryLine.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        trajectoryLine.receiveShadows =
            false;


        // -------------------------------------------------
        // ★ 圆角
        // -------------------------------------------------

        trajectoryLine.numCornerVertices =
            12;

        trajectoryLine.numCapVertices =
            12;


        trajectoryLine.alignment =
            LineAlignment.View;


        trajectoryLine.textureMode =
            LineTextureMode.Stretch;


        trajectoryLine.enabled =
            false;
    }


    // =====================================================
    // 创建落点圆
    // =====================================================

    /// <summary>
    /// 创建目标落点圆环及其渲染组件。
    /// </summary>
    void CreateTargetCircle()
    {
        targetCircle =
            new GameObject(
                "ThrowTargetCircle"
            );


        LineRenderer line =
            targetCircle.AddComponent<
                LineRenderer
            >();


        line.loop =
            true;


        line.positionCount =
            targetCircleSegments;


        line.startWidth =
            targetCircleWidth;

        line.endWidth =
            targetCircleWidth;


        // -------------------------------------------------
        // 材质
        // -------------------------------------------------

        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Unlit"
            );


        if (shader == null)
        {
            shader =
                Shader.Find(
                    "Sprites/Default"
                );
        }


        Material material =
            new Material(shader);


        material.color =
            Color.yellow;


        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                Color.yellow
            );
        }


        line.material =
            material;


        line.startColor =
            Color.yellow;

        line.endColor =
            Color.yellow;


        line.numCornerVertices =
            12;

        line.numCapVertices =
            12;


        line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        line.receiveShadows =
            false;


        targetCircle.SetActive(
            false
        );
    }


    // =====================================================
    // 更新落点圆
    // =====================================================

    /// <summary>
    /// 先尝试指定地面层；找不到时回退到全部层，以取得有效的水平落点。
    /// </summary>
    bool TryFindGroundBelow(Vector3 candidate, out Vector3 point, out Vector3 normal)
    {
        int preferredMask = groundLayer.value == 0 ? ~0 : groundLayer.value;
        if (TryFindGroundBelow(candidate, preferredMask, out point, out normal)) return true;
        if (preferredMask != ~0 && TryFindGroundBelow(candidate, ~0, out point, out normal)) return true;
        point = candidate;
        normal = Vector3.up;
        return false;
    }

    /// <summary>
    /// 向候选点下方发射射线，排除自身和陡峭表面，选最近的有效地面。
    /// </summary>
    bool TryFindGroundBelow(Vector3 candidate, int mask, out Vector3 point, out Vector3 normal)
    {
        point = candidate;
        normal = Vector3.up;
        float closest = float.PositiveInfinity;
        Vector3 origin = candidate + Vector3.up * 3f;
        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 12f, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == null || hit.normal.y < .45f) continue;
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            if (hit.distance >= closest) continue;
            closest = hit.distance;
            point = hit.point;
            normal = hit.normal.normalized;
        }
        return closest < float.PositiveInfinity;
    }

    /// <summary>
    /// 按照地面法线建立圆环平面，使落点提示贴合坡面。
    /// </summary>
    void UpdateTargetCircle(
        Vector3 target)
    {
        if (targetCircle == null)
            return;


        targetCircle.SetActive(
            true
        );


        LineRenderer line =
            targetCircle.GetComponent<
                LineRenderer
            >();


        Vector3 up = currentTargetNormal.sqrMagnitude > .01f ? currentTargetNormal.normalized : Vector3.up;
        Vector3 tangent = Vector3.Cross(up, Vector3.forward);
        if (tangent.sqrMagnitude < .01f) tangent = Vector3.Cross(up, Vector3.right);
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(up, tangent).normalized;
        Vector3 center = target + up * targetCircleHeight;

        for (
            int i = 0;
            i < targetCircleSegments;
            i++
        )
        {
            float angle =
                2f *
                Mathf.PI *
                i
                /
                targetCircleSegments;


            Vector3 point =
                center
                + tangent * Mathf.Cos(angle) * targetCircleRadius
                + bitangent * Mathf.Sin(angle) * targetCircleRadius;


            line.SetPosition(
                i,
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
    void HideTrajectory()
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled =
                false;
        }
    }


    // =====================================================
    // 停止瞄准
    // =====================================================

    /// <summary>
    /// 退出瞄准、清除有效目标并隐藏全部预览。
    /// </summary>
    void StopAim()
    {
        isAiming =
            false;

        hasValidTarget =
            false;

        HideAimVisuals();
    }


    // =====================================================
    // 隐藏所有瞄准视觉
    // =====================================================

    /// <summary>
    /// 同时隐藏抛物线与落点圆环。
    /// </summary>
    void HideAimVisuals()
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled =
                false;
        }


        if (targetCircle != null)
        {
            targetCircle.SetActive(
                false
            );
        }
    }


    // =====================================================
    // 通知 MaskInteraction
    // =====================================================

    /// <summary>
    /// 通知拾取脚本物品已离手，由它恢复未持有状态。
    /// </summary>
    void NotifyMaskDropped()
    {
        MaskInteraction mask =
            GetComponent<
                MaskInteraction
            >();


        if (mask != null)
        {
            mask.OnThrown();

            Debug.Log(
                "已通知 MaskInteraction.OnThrown()"
            );
        }
        else
        {
            Debug.LogWarning(
                "找不到 MaskInteraction"
            );
        }
    }
}
