// ！！！文件功能：桌面版键盘行走和原地转向！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

/// <summary>
/// 桌面版角色移动：W/S 前后行走，A/D 原地转向。
/// </summary>
// 定义 PlayerMovement 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class PlayerMovement : MonoBehaviour
{
    // 把 Inspector 可调整的 moveSpeed（前后行走速度）设为默认值 5f；具体用途看它在下方的判断和计算。
    public float moveSpeed = 5f;
    // 把 Inspector 可调整的 rotateSpeed（原地转向速度）设为默认值 120f；具体用途看它在下方的判断和计算。
    public float rotateSpeed = 120f;

    /// <summary>
    /// 每帧读取 W/S 和 A/D，根据帧间时间推进角色并在原地旋转。
    /// </summary>
    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：当前没有键盘设备时结束本帧处理，避免读取不存在的按键。
        if (Keyboard.current == null)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;

        // 步骤 3：把数值 0f 暂存为 move；它只在当前方法里参与后续判断或计算。
        float move = 0f;
        // 步骤 4：把数值 0f 暂存为 rotate；它只在当前方法里参与后续判断或计算。
        float rotate = 0f;

        // W / S：前后移动
        // 步骤 5：W 键被持续按住时，把下一步的前后输入设为前进。
        if (Keyboard.current.wKey.isPressed)
            // 步骤 6：把前后输入设为 +1，后面的 transform.forward 计算会让角色向前走。
            move = 1f;

        // 步骤 7：S 键被持续按住时，把下一步的前后输入设为后退。
        if (Keyboard.current.sKey.isPressed)
            // 步骤 8：把前后输入设为 -1，后面的 transform.forward 计算会让角色向后退。
            move = -1f;

        // A / D：原地旋转
        // 步骤 9：A 键被持续按住时，把下一步的转向输入设为向左。
        if (Keyboard.current.aKey.isPressed)
            // 步骤 10：把转向输入设为 -1，后面的角度计算会向左旋转。
            rotate = -1f;

        // 步骤 11：D 键被持续按住时，把下一步的转向输入设为向右。
        if (Keyboard.current.dKey.isPressed)
            // 步骤 12：把转向输入设为 +1，后面的角度计算会向右旋转。
            rotate = 1f;

        // 移动
        // 步骤 13：开始计算本帧位移；下一行把“前进方向 × 输入 × 速度 × 帧时间”加到当前坐标。
        transform.position +=
            // 步骤 14：transform.forward 是角色正前方；乘输入决定前进或后退，再乘速度和帧时间得到本帧要走的距离。
            transform.forward * move * moveSpeed * Time.deltaTime;

        // 原地旋转
        // 步骤 15：围绕角色自身坐标轴旋转；下面四个参数分别给出 X、Y、Z 角度和坐标空间。
        transform.Rotate(
            // 步骤 16：继续填写上一行调用的参数：0f；参数位置决定它表示的坐标、速度或选项。
            0f,
            // 步骤 17：本帧水平转角 = 左右输入 × 每秒转速 × 本帧秒数；负数左转，正数右转。
            rotate * rotateSpeed * Time.deltaTime,
            // 步骤 18：继续填写上一行调用的参数：0f；参数位置决定它表示的坐标、速度或选项。
            0f,
            // 步骤 19：使用角色自己的局部坐标轴旋转，原地转身而不是绕世界原点转圈。
            Space.Self
        );
    }
    // ！！！功能结束：Update！！！
}