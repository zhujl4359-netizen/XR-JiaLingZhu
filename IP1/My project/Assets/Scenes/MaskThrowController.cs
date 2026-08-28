using UnityEngine;
using UnityEngine.InputSystem;

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
    public float maxAimDistance = 35f;

    [Tooltip("抛物线角度。越小越平、越远")]
    [Range(5f, 30f)]
    public float aimAngle = 10f;

    [Tooltip("如果没有检测到地面，使用这个距离")]
    public float fallbackAimDistance = 25f;


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
    public int trajectoryPoints = 100;

    [Tooltip("抛物线粗细")]
    public float trajectoryWidth = 0.22f;


    // =====================================================
    // 落点圆圈
    // =====================================================

    [Header("落点圆圈")]

    public float targetCircleRadius = 1.3f;

    public int targetCircleSegments = 60;

    public float targetCircleHeight = 0.08f;

    public float targetCircleWidth = 0.18f;


    // =====================================================
    // 内部变量
    // =====================================================

    private Rigidbody rb;

    private bool isAiming = false;

    private bool hasValidTarget = false;

    private GameObject targetCircle;

    private Vector3 currentTarget;

    private Vector3 currentLaunchVelocity;

    private float currentFlightTime;


    // =====================================================
    // Start
    // =====================================================

    void Start()
    {
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


    // =====================================================
    // Update
    // =====================================================

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

        float horizontalSpeed =
            18f;


        float verticalSpeed =
            6f;


        currentLaunchVelocity =
            direction *
            horizontalSpeed
            +
            Vector3.up *
            verticalSpeed;


        currentFlightTime =
            fallbackAimDistance /
            horizontalSpeed;


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


        // -------------------------------------------------
        // ★ 青色
        // -------------------------------------------------

        material.color =
            Color.cyan;


        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                Color.cyan
            );
        }


        trajectoryLine.material =
            material;


        trajectoryLine.startColor =
            Color.cyan;

        trajectoryLine.endColor =
            Color.cyan;


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


            float x =
                Mathf.Cos(angle)
                *
                targetCircleRadius;


            float z =
                Mathf.Sin(angle)
                *
                targetCircleRadius;


            Vector3 point =
                target
                +
                new Vector3(
                    x,
                    targetCircleHeight,
                    z
                );


            line.SetPosition(
                i,
                point
            );
        }
    }


    // =====================================================
    // 隐藏抛物线
    // =====================================================

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