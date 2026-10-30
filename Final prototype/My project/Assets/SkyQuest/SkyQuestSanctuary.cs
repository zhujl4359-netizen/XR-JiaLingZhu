// ！！！文件功能：圣殿区域的靠近交互和灯塔激活！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// Shared proximity interactions for the keyboard and Quest scenes.
// 定义 SkyQuestSanctuary 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestSanctuary : MonoBehaviour
{
    // beacons 保存beacons（类型 Transform[]）；下方方法依靠它记录或查找场景状态。
    public Transform[] beacons;
    // 声明 gate（gate）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform gate;
    // 声明 reward（reward）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform reward;
    // 把 Inspector 可调整的 activationDistance（activationDistance）设为默认值 1.45f；具体用途看它在下方的判断和计算。
    public float activationDistance = 1.45f;
    // 把 Inspector 可调整的 completionDistance（completionDistance）设为默认值 2.2f；具体用途看它在下方的判断和计算。
    public float completionDistance = 2.2f;

    // 声明 head（头显相机）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform head;
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion locomotion;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    private SkyQuestStatus status;
    // lit 保存lit（类型 bool[]）；下方方法依靠它记录或查找场景状态。
    private bool[] lit;
    // 声明 litCount（litCount）：整数计数；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private int litCount;
    // 声明 completed（completed）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool completed;
    // 声明 gateClosedPosition（gateClosed位置）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 gateClosedPosition;
    // beaconMaterials 保存beacon材质（类型 Material[]）；下方方法依靠它记录或查找场景状态。
    private Material[] beaconMaterials;

    // IsComplete是 Inspector 中的开关，默认 > completed；勾选状态决定相关功能是否运行。
    public bool IsComplete => completed;
    // 计算 > gate，把结果存进 Gate（Gate）；之后引用 Gate 时用的就是这次结果。
    public Transform Gate => gate;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：计算 Camera.main，把结果存进 camera（camera）；之后引用 camera 时用的就是这次结果。
        Camera camera = Camera.main;
        // 步骤 2：把计算结果 camera != null ? camera.transform : null 保存为头显相机；下一条判断或调用会读取这个具体值。
        head = camera != null ? camera.transform : null;
        // 步骤 3：把计算结果 FindFirstObjectByType<SkyQuestLocomotion>() 保存为locomotion；下一条判断或调用会读取这个具体值。
        locomotion = FindFirstObjectByType<SkyQuestLocomotion>();
        // 步骤 4：把计算结果 FindFirstObjectByType<SkyQuestStatus>() 保存为status；下一条判断或调用会读取这个具体值。
        status = FindFirstObjectByType<SkyQuestStatus>();
        // 步骤 5：新建用于lit的对象或数值结构，并把构造参数保存进去。
        lit = new bool[beacons != null ? beacons.Length : 0];
        // 步骤 6：新建用于beacon材质的对象或数值结构，并把构造参数保存进去。
        beaconMaterials = new Material[lit.Length];
        // 步骤 7：按 int i = 0; i < lit.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < lit.Length; i++)
        {
            // 步骤 8：从目标物体找到所需组件并存为 renderer；使用前应检查是否为 null。
            Renderer renderer = beacons[i] != null ? beacons[i].GetComponent<Renderer>() : null;
            // 步骤 9：确认renderer已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 beaconMaterials[i] = renderer.material;
            if (renderer != null) beaconMaterials[i] = renderer.material;
        }
        // 步骤 10：确认gate已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 gateClosedPosition = gate.position;
        if (gate != null) gateClosedPosition = gate.position;
        // 步骤 11：确认reward已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (reward != null) reward.gameObject.SetActive(false);
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (head == null) return;
        // 步骤 2：按 int i = 0; i < lit.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < lit.Length; i++)
        {
            // 步骤 3：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (lit[i] || beacons[i] == null ||
                // 步骤 4：调用 Distance：Distance。
                Vector3.Distance(PlayerPosition, beacons[i].position) > activationDistance) continue;
            // 步骤 5：调用 ActivateBeacon：ActivateBeacon。
            ActivateBeacon(i, "proximity");
        }
        // 步骤 6：检查gate是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (gate != null && lit.Length > 0 && litCount == lit.Length)
        {
            // 步骤 7：把position更新为 Vector3.MoveTowards(gate.position,；世界或局部位置会立即影响物体摆放。
            gate.position = Vector3.MoveTowards(gate.position,
                // 步骤 8：执行 gateClosedPosition + Vector3.up * 4.3f, 1.4f * Time.deltaTime);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                gateClosedPosition + Vector3.up * 4.3f, 1.4f * Time.deltaTime);
            // 步骤 9：检查reward是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 SetActive：设置Active。
            if (reward != null && !reward.gameObject.activeSelf) reward.gameObject.SetActive(true);
            // 步骤 10：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (!completed && reward != null &&
                // 步骤 11：调用 Distance：Distance。
                Vector3.Distance(PlayerPosition, reward.position) < completionDistance)
            {
                // 步骤 12：把 completed 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                completed = true;
                // 步骤 13：执行 status?.Show("Sanctuary complete — the sky remembers your light.", 8f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                status?.Show("Sanctuary complete — the sky remembers your light.", 8f);
                // 步骤 14：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                SkyQuestInput.Pulse(SkyQuestInput.Left, 0.45f, 0.18f);
                // 步骤 15：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                SkyQuestInput.Pulse(SkyQuestInput.Right, 0.45f, 0.18f);
            }
        }
        // 步骤 16：检查reward是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (reward != null && reward.gameObject.activeSelf)
            // 步骤 17：调用 Rotate：Rotate。
            reward.Rotate(Vector3.up, 24f * Time.deltaTime, Space.World);
    }
    // ！！！功能结束：Update！！！

    // 这一行补充 private Vector3 PlayerPosition => locomotion != null && locomotion.desktopControlled ?，与相邻行合起来完成同一次计算或对象配置。
    private Vector3 PlayerPosition => locomotion != null && locomotion.desktopControlled ?
        // 执行 locomotion.transform.position + Vector3.up * 1.2f : head.position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        locomotion.transform.position + Vector3.up * 1.2f : head.position;

    // ！！！功能：ActivateBeacon——ActivateBeacon！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index, string source = "interaction" 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public bool ActivateBeacon(int index, string source = "interaction")
    {
        // 步骤 1：检查对象引用是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (index < 0 || index >= lit.Length || lit[index] || beacons[index] == null) return false;
        // 步骤 2：执行 lit[index] = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        lit[index] = true;
        // 步骤 3：执行 litCount++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        litCount++;
        // 步骤 4：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (beaconMaterials[index] != null)
        {
            // 步骤 5：创建新的 Color 并存为 gold；构造参数决定其初始内容或对象名称。
            Color gold = new Color(1f, 0.75f, 0.29f);
            // 步骤 6：执行 beaconMaterials[index].color = gold;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            beaconMaterials[index].color = gold;
            // 步骤 7：执行 beaconMaterials[index].EnableKeyword("_EMISSION");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            beaconMaterials[index].EnableKeyword("_EMISSION");
            // 步骤 8：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
            beaconMaterials[index].SetColor("_EmissionColor", gold * 2f);
        }
        // 步骤 9：执行 status?.Show("Light awakened: " + litCount + " / " + lit.Length, 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Light awakened: " + litCount + " / " + lit.Length, 5f);
        // 步骤 10：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Left, 0.22f, 0.09f);
        // 步骤 11：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.22f, 0.09f);
        // 步骤 12：本次检测或激活成功，返回 true 让调用方继续相应反馈。
        return true;
    }
    // ！！！功能结束：ActivateBeacon！！！
}
