// ！！！文件功能：让飞行玩法指示牌面向观看者！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 定义 CapeFlightCourseSign 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeFlightCourseSign : MonoBehaviour
{
    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void LateUpdate()
    {
        // 步骤 1：计算 Camera.main，把结果存进 camera（camera）；之后引用 camera 时用的就是这次结果。
        Camera camera = Camera.main;
        // 步骤 2：检查camera是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (camera == null) return;
        // 步骤 3：计算 transform.position - camera.transform.position，把结果存进 direction（direction）；之后引用 direction 时用的就是这次结果。
        Vector3 direction = transform.position - camera.transform.position;
        // 步骤 4：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
        direction.y = 0f;
        // 步骤 5：检查条件 direction.sqrMagnitude > 0.001f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (direction.sqrMagnitude > 0.001f)
            // 步骤 6：把rotation更新为 Quaternion.LookRotation(direction, Vector3.up)；物体朝向会在本帧改变。
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
    // ！！！功能结束：LateUpdate！！！
}
