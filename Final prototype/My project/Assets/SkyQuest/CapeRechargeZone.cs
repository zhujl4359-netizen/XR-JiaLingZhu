// ！！！文件功能：检测玩家进入、停留、离开充能区域！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 挂载脚本时要求相应组件同时存在，避免运行时找不到依赖。
[RequireComponent(typeof(Collider))]
// 定义 CapeRechargeZone 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeRechargeZone : MonoBehaviour
{
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Min(0f)] public float rechargePerSecond = 2f;
    // 声明 groundOnly（groundOnly）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool groundOnly;
    // 创建新的 HashSet<CapeRechargeReceiver> 并存为 receivers；构造参数决定其初始内容或对象名称。
    private readonly HashSet<CapeRechargeReceiver> receivers = new HashSet<CapeRechargeReceiver>();
    // 从目标物体找到所需组件并存为 ZoneCollider；使用前应检查是否为 null。
    public Collider ZoneCollider => GetComponent<Collider>();

    // ！！！功能：Awake——对象创建时建立初始数据和所需组件引用！！！
    // 何时执行：Unity 创建并启用对象时执行一次，适合寻找组件和记录初始状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Awake()
    {
        // 步骤 1：把 ZoneCollider.isTrigger 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        ZoneCollider.isTrigger = true;
    }
    // ！！！功能结束：Awake！！！

    // ！！！功能：OnTriggerEnter——碰撞体刚进入触发区域时处理一次！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Collider other 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnTriggerEnter(Collider other) => Add(other);
    // ！！！功能结束：OnTriggerEnter！！！
    // ！！！功能：OnTriggerStay——碰撞体停留在触发区域时持续处理！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Collider other 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnTriggerStay(Collider other) => Add(other);
    // ！！！功能结束：OnTriggerStay！！！

    // ！！！功能：Add——把当前充能来源或条目加入集合供后续统计！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Collider other 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Add(Collider other)
    {
        // 步骤 1：从目标物体找到所需组件并存为 receiver；使用前应检查是否为 null。
        CapeRechargeReceiver receiver = other.GetComponentInParent<CapeRechargeReceiver>();
        // 步骤 2：检查receiver是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (receiver == null || !receiver.isActiveAndEnabled) return;
        // 步骤 3：检查条件 receivers.Add(receiver)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Register：登记一个正在作用于玩家的充能区域。
        if (receivers.Add(receiver)) receiver.Register(this);
    }
    // ！！！功能结束：Add！！！

    // ！！！功能：OnTriggerExit——碰撞体离开触发区域时清理状态！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Collider other 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnTriggerExit(Collider other)
    {
        // 步骤 1：从目标物体找到所需组件并存为 receiver；使用前应检查是否为 null。
        CapeRechargeReceiver receiver = other.GetComponentInParent<CapeRechargeReceiver>();
        // 步骤 2：检查receiver是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 Unregister：玩家离开时移除对应充能区域，停止该区域持续充能。
        if (receiver != null && receivers.Remove(receiver)) receiver.Unregister(this);
    }
    // ！！！功能结束：OnTriggerExit！！！

    // ！！！功能：OnDisable——组件停用回调：解除事件或释放输入状态，防止重复监听！！！
    // 何时执行：对象或脚本被停用时调用；适合取消事件监听。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDisable()
    {
        // 步骤 1：逐个读取 CapeRechargeReceiver receiver in receivers 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (CapeRechargeReceiver receiver in receivers)
            // 步骤 2：确认receiver已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Unregister：玩家离开时移除对应充能区域，停止该区域持续充能。
            if (receiver != null) receiver.Unregister(this);
        // 步骤 3：调用 Clear：Clear。
        receivers.Clear();
    }
    // ！！！功能结束：OnDisable！！！
}
