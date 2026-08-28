using UnityEngine;
using UnityEngine.InputSystem;

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

    void Start()
    {
        cameraYaw = initialYaw;
        cameraPitch = initialPitch;

        SetThirdPerson();
    }

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

        // 第三人称时允许鼠标控制摄像机
        if (isThirdPerson)
        {
            UpdateThirdPersonCamera();
        }

        // ESC：释放鼠标
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void UpdateThirdPersonCamera()
    {
        if (cameraTransform == null || c1HeadTransform == null)
            return;

        // 鼠标移动
        if (Mouse.current != null)
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

        cameraTransform.position = cameraPosition;

        // 摄像机始终看向 C1
        cameraTransform.LookAt(targetPosition);
    }

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

        // 第三人称初始角度
        cameraYaw = initialYaw;
        cameraPitch = initialPitch;

        UpdateThirdPersonCamera();

        if (leftHandVisual != null)
            leftHandVisual.SetActive(false);

        if (rightHandVisual != null)
            rightHandVisual.SetActive(false);
    }
}