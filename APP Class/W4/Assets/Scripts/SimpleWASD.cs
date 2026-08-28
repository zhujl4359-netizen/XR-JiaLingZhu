using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleWASD : MonoBehaviour
{
    public float moveSpeed = 5f;
    InputAction moveAction;

    void Start()
    {
        // 从项目的输入操作资产中查找 "Move" 操作
        moveAction = InputSystem.actions.FindAction("Move");
    }

    void Update()
    {
        // 将操作读取为 2D 方向（每个轴上的值为 -1 到 1）
        Vector2 input = moveAction.ReadValue<Vector2>();

        // 操作的 Y 轴（W 和 S）驱动前进/后退移动
        Vector3 movement = new Vector3(0f, 0f, input.y);

        // 操作的 X 轴（A 和 D）驱动旋转（向左和向右转）
        transform.Rotate(0f, input.x * 90f * Time.deltaTime, 0f);

        // 为了提高效率，我们在调用 Translate 之前检查是否需要移动
        if (movement != Vector3.zero)
        {
            transform.Translate(movement.normalized * moveSpeed * Time.deltaTime, Space.Self);
        }

        // 添加一键归位功能 (按下 R 键)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            // 将位置重置到起点 (根据你的教程，玩家起点是 0, 1, 0)
            transform.position = new Vector3(0f, 1f, 0f);
            
            // 将旋转重置为默认朝向
            transform.rotation = Quaternion.identity;
            
            Debug.Log("胶囊体已归位！");
        }
    }

    void OnDrawGizmos()
    {
        // 绘制一条从玩家指向前方的调试射线，其高度下降到
        // 停留在地面上的物体的高度，以便显示抓取射线真正的去向
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.down * 0.5f, transform.forward * 3f);
    }
}

