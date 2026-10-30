// ！！！文件功能：玩家碰到即刻补充能量的拾取物！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 挂载脚本时要求相应组件同时存在，避免运行时找不到依赖。
[RequireComponent(typeof(Collider))]
// 定义 CapeInstantRechargePickup 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeInstantRechargePickup : MonoBehaviour
{
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Min(0f)] public float amount = 2f;
    // singleUse是 Inspector 中的开关，默认 true；勾选状态决定相关功能是否运行。
    public bool singleUse = true;
    // 声明 collected（collected）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool collected;

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
        // 步骤 1：检查条件 collected；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (collected) return;
        // 步骤 2：从目标物体找到所需组件并存为 energy；使用前应检查是否为 null。
        CapeEnergySystem energy = other.GetComponentInParent<CapeEnergySystem>();
        // 步骤 3：检查energy是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (energy == null) return;
        // 步骤 4：调用 RestoreEnergy：把指定能量加回斗篷并限制不超过最大值。
        energy.RestoreEnergy(amount);
        // 步骤 5：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Left, 0.3f, 0.08f);
        // 步骤 6：检查条件 singleUse；只有为真才运行下面的步骤，否则沿原流程继续。
        if (singleUse)
        {
            // 步骤 7：把 collected 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            collected = true;
            // 步骤 8：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            gameObject.SetActive(false);
        }
    }
    // ！！！功能结束：OnTriggerEnter！！！
}
