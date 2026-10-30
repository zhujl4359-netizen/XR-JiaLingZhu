// ！！！文件功能：飞行练习关卡的检查点触发标记！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 定义 CapeFlightCourseMarker 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeFlightCourseMarker : MonoBehaviour
{
    // 声明 finish（finish）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool finish;
    // 声明 checkpointPosition（checkpoint位置）：三维向量；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Vector3 checkpointPosition;
    // 声明 activated（activated）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool activated;

    // ！！！功能：ResetMarker——复位Marker！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void ResetMarker() => activated = false;
    // ！！！功能结束：ResetMarker！！！

    // ！！！功能：Awake——对象创建时建立初始数据和所需组件引用！！！
    // 何时执行：Unity 创建并启用对象时执行一次，适合寻找组件和记录初始状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Awake() => GetComponent<Collider>().isTrigger = true;
    // ！！！功能结束：Awake！！！

    // ！！！功能：OnTriggerEnter——碰撞体刚进入触发区域时处理一次！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Collider other 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnTriggerEnter(Collider other)
    {
        // 步骤 1：从目标物体找到所需组件并存为 course；使用前应检查是否为 null。
        CapeFlightCourseRuntime course = other.GetComponentInParent<CapeFlightCourseRuntime>();
        // 步骤 2：检查course是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (course == null || activated) return;
        // 步骤 3：把 activated 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        activated = true;
        // 步骤 4：检查条件 finish；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Finish：Finish。
        if (finish) course.Finish();
        // 步骤 5：前面的 if 条件为假时执行这里的备用处理。
        else course.ReachCheckpoint(checkpointPosition);
    }
    // ！！！功能结束：OnTriggerEnter！！！
}
