// ！！！文件功能：统一保存斗篷能量，处理消耗、补充与上限！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

/// <summary>Owns the cape's energy. Flight physics never changes this value directly.</summary>
// 定义 CapeEnergySystem 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeEnergySystem : MonoBehaviour
{
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Min(0.01f)] public float maxEnergy = 15f;
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Min(0f)] public float initialEnergy = 15f;
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Min(0.01f)] public float energyCostPerBoost = 1f;
    // 声明 allowPartialBoost（allowPartialBoost）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool allowPartialBoost;
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Range(0f, 1f)] public float lowEnergyThreshold = 0.2f;

    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public float CurrentEnergy { get; private set; }
    // 用三元表达式为 Normalized 二选一：问号前条件为真取前值，为假取冒号后的值。
    public float Normalized => maxEnergy > 0f ? CurrentEnergy / maxEnergy : 0f;
    // 执行 public event Action<float, float> OnEnergyChanged;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action<float, float> OnEnergyChanged;
    // 执行 public event Action OnEnergyDepleted;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action OnEnergyDepleted;
    // 执行 public event Action OnEnergyFull;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action OnEnergyFull;
    // 执行 public event Action OnLowEnergy;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    public event Action OnLowEnergy;

    // ！！！功能：Awake——对象创建时建立初始数据和所需组件引用！！！
    // 何时执行：Unity 创建并启用对象时执行一次，适合寻找组件和记录初始状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Awake() => ResetEnergy();
    // ！！！功能结束：Awake！！！

    // ！！！功能：CanBoost——CanBoost！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public bool CanBoost() => allowPartialBoost ? CurrentEnergy > 0f : CurrentEnergy >= energyCostPerBoost;
    // ！！！功能结束：CanBoost！！！

    // ！！！功能：TryConsumeBoostEnergy——先验证剩余能量，再扣除一次加速所需数值！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public bool TryConsumeBoostEnergy()
    {
        // 步骤 1：这里的 ! 表示取反；当 CanBoost() 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!CanBoost()) return false;
        // 步骤 2：调用 SetEnergy：设置Energy。
        SetEnergy(CurrentEnergy - (allowPartialBoost ? Mathf.Min(CurrentEnergy, energyCostPerBoost) : energyCostPerBoost));
        // 步骤 3：本次检测或激活成功，返回 true 让调用方继续相应反馈。
        return true;
    }
    // ！！！功能结束：TryConsumeBoostEnergy！！！

    // ！！！功能：RestoreEnergy——把指定能量加回斗篷并限制不超过最大值！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float amount 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void RestoreEnergy(float amount)
    {
        // 步骤 1：检查条件 amount > 0f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetEnergy：设置Energy。
        if (amount > 0f) SetEnergy(CurrentEnergy + amount);
    }
    // ！！！功能结束：RestoreEnergy！！！

    // ！！！功能：GetCurrentEnergy——获取CurrentEnergy！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    public float GetCurrentEnergy() => CurrentEnergy;
    // ！！！功能结束：GetCurrentEnergy！！！
    // ！！！功能：GetEnergyNormalized——获取EnergyNormalized！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    public float GetEnergyNormalized() => Normalized;
    // ！！！功能结束：GetEnergyNormalized！！！
    // ！！！功能：ResetEnergy——将当前能量恢复为场景规定的初始值！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void ResetEnergy() => SetEnergy(initialEnergy);
    // ！！！功能结束：ResetEnergy！！！

    // ！！！功能：SetMaximumEnergy——设置MaximumEnergy！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float value 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void SetMaximumEnergy(float value)
    {
        // 步骤 1：把计算结果 Mathf.Max(0.01f, value) 保存为maxEnergy；下一条判断或调用会读取这个具体值。
        maxEnergy = Mathf.Max(0.01f, value);
        // 步骤 2：调用 SetEnergy：设置Energy。
        SetEnergy(CurrentEnergy);
    }
    // ！！！功能结束：SetMaximumEnergy！！！

    // ！！！功能：SetEnergy——设置Energy！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float value 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetEnergy(float value)
    {
        // 步骤 1：计算 CurrentEnergy，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        float previous = CurrentEnergy;
        // 步骤 2：计算CurrentEnergy并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        CurrentEnergy = Mathf.Clamp(value, 0f, maxEnergy);
        // 步骤 3：检查条件 Mathf.Approximately(previous, CurrentEnergy)；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (Mathf.Approximately(previous, CurrentEnergy)) return;
        // 步骤 4：执行 OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        // 步骤 5：检查条件 CurrentEnergy <= 0f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 OnEnergyDepleted?.Invoke();
        if (CurrentEnergy <= 0f) OnEnergyDepleted?.Invoke();
        // 步骤 6：检查条件 CurrentEnergy >= maxEnergy；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 OnEnergyFull?.Invoke();
        if (CurrentEnergy >= maxEnergy) OnEnergyFull?.Invoke();
        // 步骤 7：这里用 && 要求两边条件同时成立：previous > maxEnergy * lowEnergyThreshold && CurrentEnergy <= maxEnergy * lowEnergyThreshold；任意一边为假就跳过本分支。
        if (previous > maxEnergy * lowEnergyThreshold && CurrentEnergy <= maxEnergy * lowEnergyThreshold)
            // 步骤 8：执行 OnLowEnergy?.Invoke();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            OnLowEnergy?.Invoke();
    }
    // ！！！功能结束：SetEnergy！！！
}
