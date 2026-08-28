using UnityEngine;
using UnityEngine.InputSystem;

public class MaskInteraction : MonoBehaviour
{
    public Transform player;
    public Material normalMaterial;
    public Material highlightMaterial;
    public Transform holdPoint;

    public float interactionDistance = 10f;

    private Renderer maskRenderer;
    private Rigidbody rb;

    private bool isNear = false;
    private bool isHeld = false;

    void Start()
    {
        maskRenderer = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();

        Debug.Log("=== Mask Interaction Start ===");

        if (maskRenderer != null)
            Debug.Log("找到 Renderer：" + gameObject.name);
        else
            Debug.LogError("找不到 Renderer！");

        if (player != null)
            Debug.Log("Player：" + player.name);
        else
            Debug.LogError("Player 没有绑定！");

        if (holdPoint != null)
            Debug.Log("HoldPoint：" + holdPoint.name);
        else
            Debug.LogError("HoldPoint 没有绑定！");
    }

    void Update()
    {
        if (player == null || isHeld)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        Debug.Log("C1 到鹿头距离：" + distance);

        if (distance <= interactionDistance)
        {
            if (!isNear)
            {
                isNear = true;

                Debug.Log("🟢 已进入鹿头互动范围");

                if (maskRenderer != null && highlightMaterial != null)
                {
                    maskRenderer.material = highlightMaterial;
                }
            }

            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("🟢 按到了 E！");
                Pickup();
            }
        }
        else
        {
            if (isNear)
            {
                isNear = false;

                Debug.Log("🔴 离开互动范围");

                if (maskRenderer != null && normalMaterial != null)
                {
                    maskRenderer.material = normalMaterial;
                }
            }
        }
    }

    void Pickup()
    {
        if (holdPoint == null)
        {
            Debug.LogError("HoldPoint 没有绑定！");
            return;
        }

        isHeld = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        transform.SetParent(holdPoint);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log("🎭 鹿头拾取成功！");
    }

    void OnDrawGizmos()
    {
        if (player == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawLine(
            player.position,
            transform.position
        );

        Gizmos.DrawWireSphere(
            transform.position,
            interactionDistance
        );
    }
}