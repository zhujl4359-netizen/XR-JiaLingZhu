using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGrabber : MonoBehaviour
{
    public float grabDistance = 3f;
    private GameObject grabbedObject = null;
    InputAction interactAction;

    void Start()
    {
        // 从项目的输入操作资产中查找 "Interact" 操作
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            if (grabbedObject == null)
            {
                GrabObject();
            }
            else
            {
                ThrowObject();
            }
        }
    }

    void GrabObject()
{
    // 在与放在地面上的物体平齐的高度发射射线，而不是从胶囊体的中心发射
    Ray ray = new Ray(transform.position + Vector3.down * 0.5f, transform.forward);
    RaycastHit hit;
    
    if (Physics.Raycast(ray, out hit, grabDistance))
    {
        Grabbable grabbable = hit.collider.GetComponent<Grabbable>();
        if (grabbable != null && !grabbable.isGrabbed)
        {
            // 抓取我们正在看着的对象
            grabbedObject = grabbable.gameObject;
            grabbable.isGrabbed = true;
            
            // 当对象被持有时，将其移出物理模拟
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // 让对象跟随玩家
            grabbedObject.transform.SetParent(transform);
            grabbedObject.transform.localPosition = new Vector3(0, 1, 1);
            Debug.Log("使用射线抓取了: " + grabbedObject.name);
        }
    }
}

    void ThrowObject()
    {
        if (grabbedObject != null)
        {
            // 释放对象
            grabbedObject.transform.SetParent(null);
            
            // 获取 Grabbable 组件并标记为未抓取
            Grabbable grabbable = grabbedObject.GetComponent<Grabbable>();
            if (grabbable != null)
            {
                grabbable.isGrabbed = false;
            }
            
            // 施加投掷力
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 在施加力之前，将对象交还给物理引擎
                rb.isKinematic = false;
                rb.AddForce(transform.forward * 10f, ForceMode.Impulse);
            }
            
            Debug.Log("投掷了: " + grabbedObject.name);
            grabbedObject = null;
        }
    }
}