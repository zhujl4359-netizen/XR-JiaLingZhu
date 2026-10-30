// ！！！文件功能：播放斗篷起飞、拒绝与充能反馈音效！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 定义 CapeFlightFeedback 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeFlightFeedback : MonoBehaviour
{
    // flight 保存flight（类型 CapeFlightController）；下方方法依靠它记录或查找场景状态。
    private CapeFlightController flight;
    // energy 保存energy（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    private CapeEnergySystem energy;
    // audioSource 保存audioSource（类型 AudioSource）；下方方法依靠它记录或查找场景状态。
    private AudioSource audioSource;
    // boostClip 保存boostClip（类型 AudioClip）；下方方法依靠它记录或查找场景状态。
    private AudioClip boostClip;
    // deniedClip 保存deniedClip（类型 AudioClip）；下方方法依靠它记录或查找场景状态。
    private AudioClip deniedClip;
    // rechargeClip 保存rechargeClip（类型 AudioClip）；下方方法依靠它记录或查找场景状态。
    private AudioClip rechargeClip;

    // ！！！功能：Initialize——Initialize！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 CapeFlightController controller, CapeEnergySystem owner, Transform head 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Initialize(CapeFlightController controller, CapeEnergySystem owner, Transform head)
    {
        // 步骤 1：把计算结果 controller 保存为flight；下一条判断或调用会读取这个具体值。
        flight = controller;
        // 步骤 2：把计算结果 owner 保存为energy；下一条判断或调用会读取这个具体值。
        energy = owner;
        // 步骤 3：把计算结果 head.gameObject.AddComponent<AudioSource>() 保存为audioSource；下一条判断或调用会读取这个具体值。
        audioSource = head.gameObject.AddComponent<AudioSource>();
        // 步骤 4：把 audioSource.playOnAwake 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        audioSource.playOnAwake = false;
        // 步骤 5：把计算结果 0f 保存为spatialBlend；下一条判断或调用会读取这个具体值。
        audioSource.spatialBlend = 0f;
        // 步骤 6：用 0.32f 更新音源音量；播放器之后以这个强度输出声音。
        audioSource.volume = 0.32f;
        // 步骤 7：把计算结果 Tone("Cape boost", 420f, 650f, 0.16f) 保存为boostClip；下一条判断或调用会读取这个具体值。
        boostClip = Tone("Cape boost", 420f, 650f, 0.16f);
        // 步骤 8：把计算结果 Tone("Cape empty", 210f, 160f, 0.12f) 保存为deniedClip；下一条判断或调用会读取这个具体值。
        deniedClip = Tone("Cape empty", 210f, 160f, 0.12f);
        // 步骤 9：把计算结果 Tone("Cape full", 540f, 800f, 0.18f) 保存为rechargeClip；下一条判断或调用会读取这个具体值。
        rechargeClip = Tone("Cape full", 540f, 800f, 0.18f);
        // 步骤 10：在OnBoostSucceeded原值上增加 Boosted；旧状态与本次输入共同决定新状态。
        flight.OnBoostSucceeded += Boosted;
        // 步骤 11：在OnBoostRejected原值上增加 Rejected；旧状态与本次输入共同决定新状态。
        flight.OnBoostRejected += Rejected;
        // 步骤 12：在OnEnergyFull原值上增加 Filled；旧状态与本次输入共同决定新状态。
        energy.OnEnergyFull += Filled;
    }
    // ！！！功能结束：Initialize！！！

    // ！！！功能：Boosted——Boosted！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Boosted()
    {
        // 步骤 1：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.48f, 0.12f);
        // 步骤 2：确认audioSource已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 PlayOneShot：PlayOneShot。
        if (audioSource != null) audioSource.PlayOneShot(boostClip);
    }
    // ！！！功能结束：Boosted！！！

    // ！！！功能：Rejected——Rejected！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Rejected()
    {
        // 步骤 1：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.12f, 0.035f);
        // 步骤 2：确认audioSource已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 PlayOneShot：PlayOneShot。
        if (audioSource != null) audioSource.PlayOneShot(deniedClip);
    }
    // ！！！功能结束：Rejected！！！

    // ！！！功能：Filled——Filled！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Filled()
    {
        // 步骤 1：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Left, 0.2f, 0.08f);
        // 步骤 2：确认audioSource已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 PlayOneShot：PlayOneShot。
        if (audioSource != null) audioSource.PlayOneShot(rechargeClip);
    }
    // ！！！功能结束：Filled！！！

    // ！！！功能：Tone——Tone！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, float start, float end, float duration 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 AudioClip 类型的计算结果；执行到 return 时把该值交回调用者。
    private static AudioClip Tone(string name, float start, float end, float duration)
    {
        // 步骤 1：定义不可在运行时改变的数值 sampleRate（sampleRate）；后续计算统一使用这个阈值、速度或尺寸。
        const int sampleRate = 22050;
        // 步骤 2：计算 Mathf.CeilToInt(sampleRate * duration)，把结果存进 count（count）；之后引用 count 时用的就是这次结果。
        int count = Mathf.CeilToInt(sampleRate * duration);
        // 步骤 3：计算 AudioClip.Create(name, count, 1, sampleRate, false)，把结果存进 clip（clip）；之后引用 clip 时用的就是这次结果。
        AudioClip clip = AudioClip.Create(name, count, 1, sampleRate, false);
        // 步骤 4：创建新的 float[] 并存为 samples；构造参数决定其初始内容或对象名称。
        float[] samples = new float[count];
        // 步骤 5：把数值 0f 暂存为 phase；它只在当前方法里参与后续判断或计算。
        float phase = 0f;
        // 步骤 6：按 int i = 0; i < count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < count; i++)
        {
            // 步骤 7：计算 (float)i / count，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = (float)i / count;
            // 步骤 8：在phase原值上增加 2f * Mathf.PI * Mathf.Lerp(start, end, t) / sampleRate；旧状态与本次输入共同决定新状态。
            phase += 2f * Mathf.PI * Mathf.Lerp(start, end, t) / sampleRate;
            // 步骤 9：执行 samples[i] = Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * t) * 0.35f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            samples[i] = Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * t) * 0.35f;
        }
        // 步骤 10：调用 SetData：设置Data。
        clip.SetData(samples, 0);
        // 步骤 11：把 clip 作为方法结果返回给调用方；return 后本方法立即结束。
        return clip;
    }
    // ！！！功能结束：Tone！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDestroy()
    {
        // 步骤 1：确认flight已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { flight.OnBoostSucceeded -= Boosted; flight.OnBoostRejected -= Rejected; }
        if (flight != null) { flight.OnBoostSucceeded -= Boosted; flight.OnBoostRejected -= Rejected; }
        // 步骤 2：确认energy已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 energy.OnEnergyFull -= Filled;
        if (energy != null) energy.OnEnergyFull -= Filled;
    }
    // ！！！功能结束：OnDestroy！！！
}
