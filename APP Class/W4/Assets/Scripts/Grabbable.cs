using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public bool isGrabbed = false;

    void Start()
    {
        // 确保对象具有刚体 (Rigidbody)
        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }
    }
}