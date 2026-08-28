using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 120f;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        float move = 0f;
        float rotate = 0f;

        // W / S 前后移动
        if (Keyboard.current.wKey.isPressed)
            move = 1f;

        if (Keyboard.current.sKey.isPressed)
            move = -1f;

        // A / D 原地旋转
        if (Keyboard.current.aKey.isPressed)
            rotate = -1f;

        if (Keyboard.current.dKey.isPressed)
            rotate = 1f;

        // 移动
        transform.Translate(
            Vector3.forward * move * moveSpeed * Time.deltaTime,
            Space.Self
        );

        // 原地旋转
        transform.Rotate(
            Vector3.up * rotate * rotateSpeed * Time.deltaTime,
            Space.Self
        );
    }
}