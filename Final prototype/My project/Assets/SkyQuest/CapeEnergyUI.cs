// ！！！文件功能：在左手柄上显示斗篷能量分段！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

/// <summary>Fifteen cape energy segments fixed to the left controller.</summary>
// 定义 CapeEnergyUI 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeEnergyUI : MonoBehaviour
{
    // alwaysVisible是 Inspector 中的开关，默认 true；勾选状态决定相关功能是否运行。
    public bool alwaysVisible = true;
    // 创建新的 Vector3 并存为 wristOffset；构造参数决定其初始内容或对象名称。
    public Vector3 wristOffset = new Vector3(0f, 0.10f, 0.14f);
    // 创建新的 Vector3 并存为 wristRotation；构造参数决定其初始内容或对象名称。
    public Vector3 wristRotation = new Vector3(55f, 0f, 0f);
    // energy 保存energy（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    private CapeEnergySystem energy;
    // 声明 panel（panel）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform panel;
    // fills 保存fills（类型 Transform[]）；下方方法依靠它记录或查找场景状态。
    private Transform[] fills;
    // fillRenderers 保存fillRenderers（类型 Renderer[]）；下方方法依靠它记录或查找场景状态。
    private Renderer[] fillRenderers;
    // label 保存label（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro label;

    // ！！！功能：Initialize——Initialize！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 CapeEnergySystem owner, Transform leftHand 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Initialize(CapeEnergySystem owner, Transform leftHand)
    {
        // 步骤 1：把计算结果 owner 保存为energy；下一条判断或调用会读取这个具体值。
        energy = owner;
        // 步骤 2：检查energy、左手柄位置是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (energy == null || leftHand == null) return;
        // 步骤 3：新建用于panel的对象或数值结构，并把构造参数保存进去。
        panel = new GameObject("Cape energy wrist display").transform;
        // 步骤 4：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        panel.SetParent(leftHand, false);
        // 步骤 5：把计算结果 wristOffset 保存为local位置；下一条判断或调用会读取这个具体值。
        panel.localPosition = wristOffset;
        // 步骤 6：恢复 panel.localRotation 到初始本地朝向；本地表示相对于父对象。
        panel.localRotation = Quaternion.Euler(wristRotation);
        // 步骤 7：新建用于backing的对象或数值结构，并把构造参数保存进去。
        GameObject backing = MakePart("Dark backing", panel, new Vector3(0f, 0f, 0f),
            // 步骤 8：用 new 创建 Vector3(0.25f, 0.085f, 0.006f), new Color(0.035f, 0.07f, 0.10f));；构造参数决定新对象的初始内容。
            new Vector3(0.25f, 0.085f, 0.006f), new Color(0.035f, 0.07f, 0.10f));
        // 步骤 9：新建用于fills的对象或数值结构，并把构造参数保存进去。
        fills = new Transform[15];
        // 步骤 10：新建用于fillRenderers的对象或数值结构，并把构造参数保存进去。
        fillRenderers = new Renderer[15];
        // 步骤 11：按 int i = 0; i < fills.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < fills.Length; i++)
        {
            // 步骤 12：计算 -0.102f + i * 0.0145f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = -0.102f + i * 0.0145f;
            // 步骤 13：调用 MakePart：MakePart。
            MakePart("Empty segment " + (i + 1), panel, new Vector3(x, 0.009f, -0.005f),
                // 步骤 14：用 new 创建 Vector3(0.012f, 0.037f, 0.003f), new Color(0.18f, 0.23f, 0.27f));；构造参数决定新对象的初始内容。
                new Vector3(0.012f, 0.037f, 0.003f), new Color(0.18f, 0.23f, 0.27f));
            // 步骤 15：把计算结果 MakePart("Energy segment " + (i + 1), panel, 保存为fill；下一条判断或调用会读取这个具体值。
            GameObject fill = MakePart("Energy segment " + (i + 1), panel,
                // 步骤 16：用 new 创建 Vector3(x, 0.009f, -0.008f), new Vector3(0.011f, 0.034f, 0.004f),；构造参数决定新对象的初始内容。
                new Vector3(x, 0.009f, -0.008f), new Vector3(0.011f, 0.034f, 0.004f),
                // 步骤 17：用 new 创建 Color(1f, 0.82f, 0.32f));；构造参数决定新对象的初始内容。
                new Color(1f, 0.82f, 0.32f));
            // 步骤 18：执行 fills[i] = fill.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            fills[i] = fill.transform;
            // 步骤 19：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            fillRenderers[i] = fill.GetComponent<Renderer>();
        }
        // 步骤 20：把计算结果 SkyQuestVisuals.Text("Cape energy label", "CAPE 15 / 15", panel, 保存为label；下一条判断或调用会读取这个具体值。
        label = SkyQuestVisuals.Text("Cape energy label", "CAPE 15 / 15", panel,
            // 步骤 21：用 new 创建 Vector3(0f, -0.027f, -0.009f), 0.7f, Color.white, 0.22f);；构造参数决定新对象的初始内容。
            new Vector3(0f, -0.027f, -0.009f), 0.7f, Color.white, 0.22f);
        // 步骤 22：把计算结果 TextAlignmentOptions.Center 保存为alignment；下一条判断或调用会读取这个具体值。
        label.alignment = TextAlignmentOptions.Center;
        // 步骤 23：在OnEnergyChanged原值上增加 Refresh；旧状态与本次输入共同决定新状态。
        energy.OnEnergyChanged += Refresh;
        // 步骤 24：调用 Refresh：刷新。
        Refresh(energy.CurrentEnergy, energy.maxEnergy);
    }
    // ！！！功能结束：Initialize！！！

    // ！！！功能：MakePart——MakePart！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Transform parent, Vector3 position, Vector3 scale, Color color 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject MakePart(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        // 步骤 1：计算 GameObject.CreatePrimitive(PrimitiveType.Cube)，把结果存进 part（part）；之后引用 part 时用的就是这次结果。
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // 步骤 2：把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        part.name = name;
        // 步骤 3：执行 part.transform.SetParent(parent, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        part.transform.SetParent(parent, false);
        // 步骤 4：执行 part.transform.localPosition = position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        part.transform.localPosition = position;
        // 步骤 5：执行 part.transform.localScale = scale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        part.transform.localScale = scale;
        // 步骤 6：从目标物体找到所需组件并存为 collider；使用前应检查是否为 null。
        Collider collider = part.GetComponent<Collider>();
        // 步骤 7：确认collider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (collider != null) Destroy(collider);
        // 步骤 8：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        part.GetComponent<Renderer>().sharedMaterial = SkyQuestVisuals.Material(color);
        // 步骤 9：把 part 作为方法结果返回给调用方；return 后本方法立即结束。
        return part;
    }
    // ！！！功能结束：MakePart！！！

    // ！！！功能：Refresh——刷新！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float current, float maximum 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Refresh(float current, float maximum)
    {
        // 步骤 1：检查fills是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (fills == null) return;
        // 步骤 2：计算 maximum / fills.Length，把结果存进 segmentCapacity（segmentCapacity）；之后引用 segmentCapacity 时用的就是这次结果。
        float segmentCapacity = maximum / fills.Length;
        // 步骤 3：按 int i = 0; i < fills.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < fills.Length; i++)
        {
            // 步骤 4：计算 fraction 并限制在安全上下界，防止不合理的大小影响移动或显示。
            float fraction = segmentCapacity > 0f ? Mathf.Clamp01((current - i * segmentCapacity) / segmentCapacity) : 0f;
            // 步骤 5：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            fills[i].gameObject.SetActive(fraction > 0.001f);
            // 步骤 6：执行 fills[i].localScale = new Vector3(0.011f * fraction, 0.034f, 0.004f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            fills[i].localScale = new Vector3(0.011f * fraction, 0.034f, 0.004f);
            // 步骤 7：执行 fills[i].localPosition = new Vector3(-0.102f + i * 0.0145f - 0.0055f * (1f - fraction), 0.009f, -0.0；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            fills[i].localPosition = new Vector3(-0.102f + i * 0.0145f - 0.0055f * (1f - fraction), 0.009f, -0.008f);
            // 步骤 8：这一行补充 fillRenderers[i].sharedMaterial = SkyQuestVisuals.Material(current <= maximum * 0.2f ?，与相邻行合起来完成同一次计算或对象配置。
            fillRenderers[i].sharedMaterial = SkyQuestVisuals.Material(current <= maximum * 0.2f ?
                // 步骤 9：用 new 创建 Color(1f, 0.35f, 0.22f) : new Color(1f, 0.82f, 0.32f));；构造参数决定新对象的初始内容。
                new Color(1f, 0.35f, 0.22f) : new Color(1f, 0.82f, 0.32f));
        }
        // 步骤 10：确认label已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 label.text = "CAPE " + current.ToString("0.#") + " / " + maximum.ToString("
        if (label != null) label.text = "CAPE " + current.ToString("0.#") + " / " + maximum.ToString("0.#");
    }
    // ！！！功能结束：Refresh！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDestroy()
    {
        // 步骤 1：确认energy已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 energy.OnEnergyChanged -= Refresh;
        if (energy != null) energy.OnEnergyChanged -= Refresh;
        // 步骤 2：确认panel已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (panel != null) Destroy(panel.gameObject);
    }
    // ！！！功能结束：OnDestroy！！！
}
