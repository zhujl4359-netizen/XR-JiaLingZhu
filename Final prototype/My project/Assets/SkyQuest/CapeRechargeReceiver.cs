// ！！！文件功能：合并周围充能来源，按最高充能速率恢复能量！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

/// <summary>Combines overlapping sources by their highest applicable rate.</summary>
// 定义 CapeRechargeReceiver 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeRechargeReceiver : MonoBehaviour
{
    // 创建新的 HashSet<CapeRechargeZone> 并存为 zones；构造参数决定其初始内容或对象名称。
    private readonly HashSet<CapeRechargeZone> zones = new HashSet<CapeRechargeZone>();
    // energy 保存energy（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    private CapeEnergySystem energy;
    // flight 保存flight（类型 CapeFlightController）；下方方法依靠它记录或查找场景状态。
    private CapeFlightController flight;
    // 声明 lastFeedback（lastFeedback）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float lastFeedback;

    // ！！！功能：Initialize——Initialize！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 CapeEnergySystem owner, CapeFlightController controller 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Initialize(CapeEnergySystem owner, CapeFlightController controller)
    {
        // 步骤 1：把计算结果 owner 保存为energy；下一条判断或调用会读取这个具体值。
        energy = owner;
        // 步骤 2：把计算结果 controller 保存为flight；下一条判断或调用会读取这个具体值。
        flight = controller;
    }
    // ！！！功能结束：Initialize！！！

    // ！！！功能：Register——登记一个正在作用于玩家的充能区域！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 CapeRechargeZone zone 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Register(CapeRechargeZone zone)
    {
        // 步骤 1：确认zone已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Add：把当前充能来源或条目加入集合供后续统计。
        if (zone != null) zones.Add(zone);
    }
    // ！！！功能结束：Register！！！

    // ！！！功能：Unregister——玩家离开时移除对应充能区域，停止该区域持续充能！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 CapeRechargeZone zone 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Unregister(CapeRechargeZone zone)
    {
        // 步骤 1：调用 Remove：移除。
        zones.Remove(zone);
    }
    // ！！！功能结束：Unregister！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Update() => Step(Time.deltaTime);
    // ！！！功能结束：Update！！！

    // ！！！功能：Step——Step！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Step(float dt)
    {
        // 步骤 1：检查energy是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (energy == null) return;
        // 步骤 2：把数值 0f 暂存为 rate；它只在当前方法里参与后续判断或计算。
        float rate = 0f;
        // 步骤 3：调用 RemoveWhere：移除Where。
        zones.RemoveWhere(zone => zone == null || !zone.isActiveAndEnabled ||
            // 步骤 4：把计算结果 = null || !zone.ZoneCollider.enabled) 保存为ZoneCollider；下一条判断或调用会读取这个具体值。
            zone.ZoneCollider == null || !zone.ZoneCollider.enabled);
        // 步骤 5：逐个读取 CapeRechargeZone zone in zones 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (CapeRechargeZone zone in zones)
        {
            // 步骤 6：检查flight是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (zone.groundOnly && (flight == null || flight.State != CapeFlightState.Grounded)) continue;
            // 步骤 7：把计算结果 Mathf.Max(rate, zone.rechargePerSecond) 保存为rate；下一条判断或调用会读取这个具体值。
            rate = Mathf.Max(rate, zone.rechargePerSecond);
        }
        // 步骤 8：这里用 || 表示任一条件成立就处理：rate <= 0f || energy.CurrentEnergy >= energy.maxEnergy；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (rate <= 0f || energy.CurrentEnergy >= energy.maxEnergy) return;
        // 步骤 9：调用 RestoreEnergy：把指定能量加回斗篷并限制不超过最大值。
        energy.RestoreEnergy(rate * Mathf.Max(0f, dt));
        // 步骤 10：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
        if (Time.time - lastFeedback > 0.65f)
        {
            // 步骤 11：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Left, 0.10f, 0.04f);
            // 步骤 12：根据当前游戏时间计算并保存lastFeedback，用于后续延迟或冷却判断。
            lastFeedback = Time.time;
        }
    }
    // ！！！功能结束：Step！！！
}
