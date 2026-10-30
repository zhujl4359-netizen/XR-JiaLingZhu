// ！！！文件功能：单个飞行火焰的运动、发光和可收集状态！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using System.Collections;
using UnityEngine;

// A single collectible in the sanctuary flight challenge.
// 定义 SkyQuestFlightFlame 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class SkyQuestFlightFlame : MonoBehaviour
{
    // 把 Inspector 可调整的 collectRadius（collectRadius）设为默认值 1.15f；具体用途看它在下方的判断和计算。
    public float collectRadius = 1.15f;
    // 把 Inspector 可调整的 bobHeight（bobHeight）设为默认值 0.14f；具体用途看它在下方的判断和计算。
    public float bobHeight = 0.14f;
    // 把 Inspector 可调整的 bobSpeed（bobSpeed）设为默认值 1.8f；具体用途看它在下方的判断和计算。
    public float bobSpeed = 1.8f;
    // 声明 restingPosition（resting位置）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 restingPosition;
    // 声明 phase（phase）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float phase;
    // wisps 保存wisps（类型 Transform[]）；下方方法依靠它记录或查找场景状态。
    private Transform[] wisps;
    // wispScales 保存wispScales（类型 Vector3[]）；下方方法依靠它记录或查找场景状态。
    private Vector3[] wispScales;
    // wispRotations 保存wispRotations（类型 Quaternion[]）；下方方法依靠它记录或查找场景状态。
    private Quaternion[] wispRotations;
    private Coroutine collectionRoutine;
    private Vector3 originalScale;

    // ！！！功能：Awake——对象创建时建立初始数据和所需组件引用！！！
    // 何时执行：Unity 创建并启用对象时执行一次，适合寻找组件和记录初始状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Awake()
    {
        // 步骤 1：把计算结果 transform.position 保存为resting位置；下一条判断或调用会读取这个具体值。
        restingPosition = transform.position;
        originalScale = transform.localScale;
        // 步骤 2：把计算结果 transform.GetSiblingIndex() * 0.7f 保存为phase；下一条判断或调用会读取这个具体值。
        phase = transform.GetSiblingIndex() * 0.7f;
        // 步骤 3：从目标物体找到所需组件并存为 parts；使用前应检查是否为 null。
        MeshFilter[] parts = GetComponentsInChildren<MeshFilter>(true);
        // 步骤 4：把数值 0 暂存为 count；它只在当前方法里参与后续判断或计算。
        int count = 0;
        // 步骤 5：逐个读取 MeshFilter part in parts 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (MeshFilter part in parts)
            // 步骤 6：这里用 || 表示任一条件成立就处理：part.name.Contains("tongue") || part.name.Contains("wisp")；用于合并几种停止或跳过原因。 成立时执行 count++;
            if (part.name.Contains("tongue") || part.name.Contains("wisp")) count++;
        // 步骤 7：新建用于wisps的对象或数值结构，并把构造参数保存进去。
        wisps = new Transform[count];
        // 步骤 8：新建用于wispScales的对象或数值结构，并把构造参数保存进去。
        wispScales = new Vector3[count];
        // 步骤 9：新建用于wispRotations的对象或数值结构，并把构造参数保存进去。
        wispRotations = new Quaternion[count];
        // 步骤 10：把数值 0 暂存为 index；它只在当前方法里参与后续判断或计算。
        int index = 0;
        // 步骤 11：逐个读取 MeshFilter part in parts 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (MeshFilter part in parts)
        {
            // 步骤 12：这里用 && 要求两边条件同时成立：!part.name.Contains("tongue") && !part.name.Contains("wisp")；任意一边为假就跳过本分支。 成立时执行 continue;
            if (!part.name.Contains("tongue") && !part.name.Contains("wisp")) continue;
            // 步骤 13：执行 wisps[index] = part.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            wisps[index] = part.transform;
            // 步骤 14：执行 wispScales[index] = part.transform.localScale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            wispScales[index] = part.transform.localScale;
            // 步骤 15：执行 wispRotations[index] = part.transform.localRotation;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            wispRotations[index] = part.transform.localRotation;
            // 步骤 16：执行 index++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            index++;
        }
    }
    // ！！！功能结束：Awake！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Update()
    {
        if (collectionRoutine != null) return;
        // 步骤 1：把position更新为 restingPosition + Vector3.up *；世界或局部位置会立即影响物体摆放。
        transform.position = restingPosition + Vector3.up *
            // 步骤 2：执行 (Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight);
        // 步骤 3：调用 Rotate：Rotate。
        transform.Rotate(Vector3.up, 24f * Time.deltaTime, Space.World);
        // 步骤 4：按 int i = 0; i < wisps.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < wisps.Length; i++)
        {
            // 步骤 5：计算 Mathf.Sin(Time.time * (2.3f + i * .19f) + phase + i * 1.4f)，把结果存进 pulse（pulse）；之后引用 pulse 时用的就是这次结果。
            float pulse = Mathf.Sin(Time.time * (2.3f + i * .19f) + phase + i * 1.4f);
            // 步骤 6：执行 wisps[i].localScale = wispScales[i] * (1f + pulse * .065f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            wisps[i].localScale = wispScales[i] * (1f + pulse * .065f);
            // 步骤 7：执行 wisps[i].localRotation = wispRotations[i] * Quaternion.Euler(0f, 0f, pulse * 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            wisps[i].localRotation = wispRotations[i] * Quaternion.Euler(0f, 0f, pulse * 5f);
        }
    }
    // ！！！功能结束：Update！！！

    public void PlayCollection()
    {
        if (collectionRoutine == null && gameObject.activeInHierarchy)
            collectionRoutine = StartCoroutine(CollectionAnimation());
    }

    public void ResetCollection()
    {
        if (collectionRoutine != null) StopCoroutine(collectionRoutine);
        collectionRoutine = null;
        transform.localScale = originalScale;
        transform.position = restingPosition;
        gameObject.SetActive(true);
    }

    private IEnumerator CollectionAnimation()
    {
        Vector3 start = transform.position;
        const float duration = .52f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t);
            transform.position = start + Vector3.up * (.32f * eased);
            transform.localScale = originalScale * Mathf.Max(0f, 1f - eased);
            yield return null;
        }
        gameObject.SetActive(false);
        collectionRoutine = null;
    }
}
