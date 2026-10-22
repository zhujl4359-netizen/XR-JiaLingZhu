using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 桌面版视角控制：按 1/3 切换第一、第三人称，并平滑跟随角色。
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("绑定目标")]
    public Transform cameraTransform;        // Main Camera
    public Transform c1HeadTransform;        // C1头部/身体上方的目标点
    public GameObject leftHandVisual;
    public GameObject rightHandVisual;

    [Header("第三人称摄像机")]
    public float cameraDistance = 10f;
    public float mouseSensitivity = 0.15f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("第三人称初始角度")]
    public float initialYaw = 0f;
    public float initialPitch = 25f;

    [Header("第一人称局部偏移")]
    public Vector3 firstPersonLocalPos = new Vector3(0f, 1.4f, 0.2f);
    public Vector3 firstPersonLocalRot = Vector3.zero;

    private bool isThirdPerson = true;

    private float cameraYaw;
    private float cameraPitch;
    private Vector3 cameraVelocity;

    [Header("Camera comfort")]
    [Min(0.01f)] public float followSmoothTime = 0.22f;
    [Min(0.01f)] public float rotationSmoothSpeed = 8f;


    /// <summary>
    /// 以 Inspector 指定的初始角度设置镜头，并默认进入第三人称。
    /// </summary>
    void Start()
    {
        cameraYaw = initialYaw;
        cameraPitch = initialPitch;

        SetThirdPerson();
    }

    /// <summary>
    /// 监听数字 1、3 和 Esc，分别切换视角或释放鼠标。
    /// </summary>
    void Update()
    {
        if (Keyboard.current == null) return;

        // 按 1：第一人称
        if (Keyboard.current.digit1Key.wasPressedThisFrame ||
            Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            SetFirstPerson();
        }

        // 按 3：第三人称
        if (Keyboard.current.digit3Key.wasPressedThisFrame ||
            Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            SetThirdPerson();
        }


        // ESC：释放鼠标
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// 角色位置更新之后再移动第三人称镜头，减少跟随抖动。
    /// </summary>
    void LateUpdate()
    {
        if (isThirdPerson) UpdateThirdPersonCamera();
    }

    /// <summary>
    /// 读取鼠标角度，限制俯仰并平滑计算镜头位置和朝向。
    /// </summary>
    void UpdateThirdPersonCamera()
    {
        if (cameraTransform == null || c1HeadTransform == null)
            return;

        // 鼠标移动
        if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            cameraYaw += mouseDelta.x * mouseSensitivity;
            cameraPitch -= mouseDelta.y * mouseSensitivity;

            cameraPitch = Mathf.Clamp(
                cameraPitch,
                minPitch,
                maxPitch
            );
        }

        // 根据旋转角度计算摄像机位置
        Quaternion rotation = Quaternion.Euler(
            cameraPitch,
            cameraYaw,
            0f
        );

        Vector3 targetPosition = c1HeadTransform.position;

        Vector3 cameraPosition =
            targetPosition - rotation * Vector3.forward * cameraDistance;

        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, cameraPosition,
            ref cameraVelocity, followSmoothTime);

        Quaternion lookRotation = Quaternion.LookRotation(targetPosition - cameraTransform.position);
        cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, lookRotation,
            1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime));
    }

    /// <summary>
    /// 把镜头挂到角色头部，应用局部偏移并显示第一人称手部模型。
    /// </summary>
    public void SetFirstPerson()
    {
        isThirdPerson = false;

        // 释放鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraTransform != null && c1HeadTransform != null)
        {
            cameraTransform.SetParent(c1HeadTransform);

            cameraTransform.localPosition = firstPersonLocalPos;
            cameraTransform.localEulerAngles = firstPersonLocalRot;
        }

        if (leftHandVisual != null)
            leftHandVisual.SetActive(true);

        if (rightHandVisual != null)
            rightHandVisual.SetActive(true);
    }

    /// <summary>
    /// 镜头脱离角色层级，锁定鼠标并隐藏第一人称手部模型。
    /// </summary>
    public void SetThirdPerson()
    {
        isThirdPerson = true;

        // 鼠标锁定到游戏窗口
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
        {
            // 摄像机脱离 C1
            cameraTransform.SetParent(null);
        }

        cameraVelocity = Vector3.zero;
        UpdateThirdPersonCamera();

        if (leftHandVisual != null)
            leftHandVisual.SetActive(false);

        if (rightHandVisual != null)
            rightHandVisual.SetActive(false);
    }
}