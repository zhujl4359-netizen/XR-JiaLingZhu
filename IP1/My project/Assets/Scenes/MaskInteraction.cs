using UnityEngine;
using UnityEngine.InputSystem;

public class MaskInteraction : MonoBehaviour
{
    [Header("玩家")]
    public Transform player;

    [Header("材质")]
    public Material normalMaterial;
    public Material highlightMaterial;

    [Header("拾取位置")]
    public Transform holdPoint;

    [Header("互动距离")]
    public float interactionDistance = 3f;

    private Renderer maskRenderer;
    private Rigidbody rb;

    private bool isNear = false;
    private bool isPickedUp = false;

    // 让其他脚本可以知道鹿头是否被拿着
    public bool IsPickedUp
    {
        get { return isPickedUp; }
    }

    void Start()
    {
        maskRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();

        if (maskRenderer != null && normalMaterial != null)
        {
            maskRenderer.material = normalMaterial;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        isNear = distance <= interactionDistance;

        // =====================================
        // 靠近 → 变色
        // =====================================

        if (isNear && !isPickedUp)
        {
            if (maskRenderer != null && highlightMaterial != null)
            {
                maskRenderer.material = highlightMaterial;
            }

            // E 拾取
            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                PickUp();
            }
        }
        else if (!isPickedUp)
        {
            if (maskRenderer != null && normalMaterial != null)
            {
                maskRenderer.material = normalMaterial;
            }
        }
    }

    // =====================================
    // 拾取
    // =====================================

    void PickUp()
    {
        isPickedUp = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (holdPoint != null)
        {
            transform.SetParent(holdPoint);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    // =====================================
    // 被投掷后调用
    // =====================================

    public void OnThrown()
    {
        isPickedUp = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // 确保脱离手部
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
    }
}