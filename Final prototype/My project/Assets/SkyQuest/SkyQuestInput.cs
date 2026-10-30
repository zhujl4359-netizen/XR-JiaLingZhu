// ！！！文件功能：集中读取 Quest Touch 手柄的按钮、摇杆与震动！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.XR 命名空间；下方可直接使用Quest 头显及手柄输入设备。
using UnityEngine.XR;

// Input boundary for Quest Touch controllers only.
// 定义 SkyQuestInput 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestInput
{
    // 从 XR 系统取得左手柄并存为 Left；后面的追踪姿态和按钮都从这个设备读取。
    public static InputDevice Left => InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    // 从 XR 系统取得右手柄并存为 Right；后面的追踪姿态和按钮都从这个设备读取。
    public static InputDevice Right => InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

    // ！！！功能：Trigger——Trigger！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 InputDevice device 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    public static float Trigger(InputDevice device)
    {
        // 步骤 1：把 device.TryGetFeatureValue(CommonUsages.trigger, out float value) ? value : 0f 作为方法结果返回给调用方；return 后本方法立即结束。
        return device.TryGetFeatureValue(CommonUsages.trigger, out float value) ? value : 0f;
    }
    // ！！！功能结束：Trigger！！！

    // ！！！功能：Grip——Grip！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 InputDevice device 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    public static float Grip(InputDevice device)
    {
        // 步骤 1：把 device.TryGetFeatureValue(CommonUsages.grip, out float value) ? value : 0f 作为方法结果返回给调用方；return 后本方法立即结束。
        return device.TryGetFeatureValue(CommonUsages.grip, out float value) ? value : 0f;
    }
    // ！！！功能结束：Grip！！！

    // ！！！功能：Primary——Primary！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 InputDevice device 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public static bool Primary(InputDevice device)
    {
        // 步骤 1：把 device.TryGetFeatureValue(CommonUsages.primaryButton, out bool value) && value 作为方法结果返回给调用方；return 后本方法立即结束。
        return device.TryGetFeatureValue(CommonUsages.primaryButton, out bool value) && value;
    }
    // ！！！功能结束：Primary！！！

    // ！！！功能：Secondary——Secondary！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 InputDevice device 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public static bool Secondary(InputDevice device)
    {
        // 步骤 1：把 device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool value) && value 作为方法结果返回给调用方；return 后本方法立即结束。
        return device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool value) && value;
    }
    // ！！！功能结束：Secondary！！！

    // ！！！功能：Stick——Stick！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 InputDevice device 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Vector2 类型的计算结果；执行到 return 时把该值交回调用者。
    public static Vector2 Stick(InputDevice device)
    {
        // 步骤 1：把 device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 value) ? value : Vector2.zero 作为方法结果返回给调用方；return 后本方法立即结束。
        return device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 value) ? value : Vector2.zero;
    }
    // ！！！功能结束：Stick！！！

    // ！！！功能：Pulse——检测设备震动能力并发送短促触觉反馈！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 InputDevice device, float amplitude = 0.35f, float duration = 0.08f 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void Pulse(InputDevice device, float amplitude = 0.35f, float duration = 0.08f)
    {
        // 步骤 1：这里用 && 要求两边条件同时成立：device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse；任意一边为假就跳过本分支。
        if (device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
            // 步骤 2：向手柄发送指定幅度和时长的震动，让玩家确认已选中或释放。
            device.SendHapticImpulse(0, Mathf.Clamp01(amplitude), duration);
    }
    // ！！！功能结束：Pulse！！！
}

// 定义 SkyQuestInputExtensions 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestInputExtensions
{
    // ！！！功能：StickClick——StickClick！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 UnityEngine.XR.InputDevice device 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public static bool StickClick(UnityEngine.XR.InputDevice device)
    {
        // 步骤 1：把 device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool value) && value 作为方法结果返回给调用方；return 后本方法立即结束。
        return device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool value) && value;
    }
    // ！！！功能结束：StickClick！！！
}
