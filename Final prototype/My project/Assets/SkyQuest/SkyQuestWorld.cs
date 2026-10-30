// ！！！文件功能：游戏世界的启动与逐帧更新！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 定义 SkyQuestWorld 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestWorld : MonoBehaviour
{
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    public SkyQuestLocomotion locomotion;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    public SkyQuestStatus status;
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 ancestor（ancestor）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform ancestor;
    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public int EnergyCollected { get; private set; }

    // 这一行补充 private readonly Vector3[] windOffsets =，与相邻行合起来完成同一次计算或对象配置。
    private readonly Vector3[] windOffsets =
    {
        // 用 new 创建 Vector3(-4f, 0f, -10f), new Vector3(3f, 0f, -18f), new Vector3(-5f, 0f, -26f；构造参数决定新对象的初始内容。
        new Vector3(-4f, 0f, -10f), new Vector3(3f, 0f, -18f), new Vector3(-5f, 0f, -26f)
    };
    // 这一行补充 private readonly Vector3[] energyOffsets =，与相邻行合起来完成同一次计算或对象配置。
    private readonly Vector3[] energyOffsets =
    {
        // 用 new 创建 Vector3(0f, 2f, -5f), new Vector3(-3f, 3f, -11f),；构造参数决定新对象的初始内容。
        new Vector3(0f, 2f, -5f), new Vector3(-3f, 3f, -11f),
        // 用 new 创建 Vector3(2f, 4f, -16f), new Vector3(-2f, 3f, -21f),；构造参数决定新对象的初始内容。
        new Vector3(2f, 4f, -16f), new Vector3(-2f, 3f, -21f),
        // 用 new 创建 Vector3(1f, 4f, -26f), new Vector3(-1f, 2f, -31f)；构造参数决定新对象的初始内容。
        new Vector3(1f, 4f, -26f), new Vector3(-1f, 2f, -31f)
    };
    // energyOrbs 保存energyOrbs（类型 GameObject[]）；下方方法依靠它记录或查找场景状态。
    private GameObject[] energyOrbs;
    // collected 保存collected（类型 bool[]）；下方方法依靠它记录或查找场景状态。
    private bool[] collected;
    // windPositions 保存windPositions（类型 Vector3[]）；下方方法依靠它记录或查找场景状态。
    private Vector3[] windPositions;
    // counter 保存counter（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro counter;
    // 声明 nextWindMessage（nextWindMessage）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextWindMessage;
    // 声明 nextWindHaptic（nextWindHaptic）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextWindHaptic;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：计算 transform.position，把结果存进 origin（origin）；之后引用 origin 时用的就是这次结果。
        Vector3 origin = transform.position;
        // 步骤 2：新建用于windPositions的对象或数值结构，并把构造参数保存进去。
        windPositions = new Vector3[windOffsets.Length];
        // 步骤 3：新建用于energyOrbs的对象或数值结构，并把构造参数保存进去。
        energyOrbs = new GameObject[energyOffsets.Length];
        // 步骤 4：新建用于collected的对象或数值结构，并把构造参数保存进去。
        collected = new bool[energyOffsets.Length];
        // 步骤 5：按 int i = 0; i < windOffsets.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < windOffsets.Length; i++)
        {
            // 步骤 6：执行 windPositions[i] = origin + windOffsets[i];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            windPositions[i] = origin + windOffsets[i];
            // 步骤 7：把计算结果 SkyQuestVisuals.Primitive("Wind current " + (i + 1), 保存为zone；下一条判断或调用会读取这个具体值。
            GameObject zone = SkyQuestVisuals.Primitive("Wind current " + (i + 1),
                // 步骤 8：继续填写上一行调用的参数：PrimitiveType.Sphere, null, windPositions[i] + Vector3.up * 1.5f；参数位置决定它表示的坐标、速度或选项。
                PrimitiveType.Sphere, null, windPositions[i] + Vector3.up * 1.5f,
                // 步骤 9：执行 Vector3.one * 0.35f, new Color(0.35f, 0.8f, 1f), true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * 0.35f, new Color(0.35f, 0.8f, 1f), true);
            // 步骤 10：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
            SkyQuestVisuals.RemoveCollider(zone);
            // 步骤 11：检查locomotion是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (locomotion != null && !locomotion.desktopControlled)
            {
                // 步骤 12：创建新的 GameObject 并存为 recharge；构造参数决定其初始内容或对象名称。
                GameObject recharge = new GameObject("Cape light recharge trigger " + (i + 1));
                // 步骤 13：执行 recharge.transform.position = windPositions[i] + Vector3.up * 1.5f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                recharge.transform.position = windPositions[i] + Vector3.up * 1.5f;
                // 步骤 14：计算 recharge.AddComponent<SphereCollider>()，把结果存进 trigger（trigger）；之后引用 trigger 时用的就是这次结果。
                SphereCollider trigger = recharge.AddComponent<SphereCollider>();
                // 步骤 15：把计算结果 2.2f 保存为radius；下一条判断或调用会读取这个具体值。
                trigger.radius = 2.2f;
                // 步骤 16：把 trigger.isTrigger 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                trigger.isTrigger = true;
                // 步骤 17：执行 recharge.AddComponent<CapeRechargeZone>().rechargePerSecond = 2f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                recharge.AddComponent<CapeRechargeZone>().rechargePerSecond = 2f;
            }
        }
        // 步骤 18：按 int i = 0; i < energyOffsets.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < energyOffsets.Length; i++)
        {
            // 步骤 19：继续填写上一行调用的参数：energyOrbs[i] = SkyQuestVisuals.Primitive("Sky energy " + (i + 1)；参数位置决定它表示的坐标、速度或选项。
            energyOrbs[i] = SkyQuestVisuals.Primitive("Sky energy " + (i + 1),
                // 步骤 20：继续填写上一行调用的参数：PrimitiveType.Sphere, null, origin + energyOffsets[i]；参数位置决定它表示的坐标、速度或选项。
                PrimitiveType.Sphere, null, origin + energyOffsets[i],
                // 步骤 21：执行 Vector3.one * 0.25f, new Color(1f, 0.9f, 0.4f), true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * 0.25f, new Color(1f, 0.9f, 0.4f), true);
            // 步骤 22：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
            SkyQuestVisuals.RemoveCollider(energyOrbs[i]);
        }
        // 步骤 23：检查头显相机、locomotion是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (head != null && locomotion != null && locomotion.desktopControlled)
            // 步骤 24：把计算结果 SkyQuestVisuals.Text("Sky light counter", "Sky lights 0 / 6", head, 保存为counter；下一条判断或调用会读取这个具体值。
            counter = SkyQuestVisuals.Text("Sky light counter", "Sky lights 0 / 6", head,
                // 步骤 25：用 new 创建 Vector3(0f, 0.37f, 1.2f), 2f, Color.white, 0.9f);；构造参数决定新对象的初始内容。
                new Vector3(0f, 0.37f, 1.2f), 2f, Color.white, 0.9f);
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：检查头显相机、locomotion是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (head == null || locomotion == null) return;
        // 步骤 2：按 int i = 0; i < windPositions.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < windPositions.Length; i++)
        {
            // 步骤 3：用三元表达式为 playerPosition 二选一：问号前条件为真取前值，为假取冒号后的值。
            Vector3 playerPosition = locomotion.desktopControlled ? transform.position : head.position;
            // 步骤 4：新建用于distance的对象或数值结构，并把构造参数保存进去。
            Vector2 distance = new Vector2(playerPosition.x - windPositions[i].x,
                // 步骤 5：执行 playerPosition.z - windPositions[i].z);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                playerPosition.z - windPositions[i].z);
            // 步骤 6：这里用 && 要求两边条件同时成立：distance.magnitude < 2.2f && locomotion.IsFlying；任意一边为假就跳过本分支。
            if (distance.magnitude < 2.2f && locomotion.IsFlying)
            {
                // 步骤 7：调用 AddWindLift：添加WindLift。
                locomotion.AddWindLift(3f * Time.deltaTime);
                // 步骤 8：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
                if (Time.time > nextWindHaptic)
                {
                    // 步骤 9：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                    SkyQuestInput.Pulse(SkyQuestInput.Left, 0.14f, 0.06f);
                    // 步骤 10：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                    SkyQuestInput.Pulse(SkyQuestInput.Right, 0.14f, 0.06f);
                    // 步骤 11：根据当前游戏时间计算并保存nextWindHaptic，用于后续延迟或冷却判断。
                    nextWindHaptic = Time.time + 0.55f;
                }
                // 步骤 12：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
                if (Time.time > nextWindMessage)
                {
                    // 步骤 13：这一行补充 status?.Show(locomotion.desktopControlled ? "Wind current lifts you." :，与相邻行合起来完成同一次计算或对象配置。
                    status?.Show(locomotion.desktopControlled ? "Wind current lifts you." :
                        // 步骤 14：执行 "Wind current lifts you and recharges your cape.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                        "Wind current lifts you and recharges your cape.");
                    // 步骤 15：根据当前游戏时间计算并保存nextWindMessage，用于后续延迟或冷却判断。
                    nextWindMessage = Time.time + 5f;
                }
            }
        }
        // 步骤 16：按 int i = 0; i < energyOrbs.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < energyOrbs.Length; i++)
        {
            // 步骤 17：检查对象引用是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (collected[i] || energyOrbs[i] == null) continue;
            // 步骤 18：执行 energyOrbs[i].transform.Rotate(Vector3.up, 40f * Time.deltaTime, Space.World);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            energyOrbs[i].transform.Rotate(Vector3.up, 40f * Time.deltaTime, Space.World);
            // 步骤 19：把计算结果 locomotion.desktopControlled ? 保存为player位置；下一条判断或调用会读取这个具体值。
            Vector3 playerPosition = locomotion.desktopControlled ?
                // 步骤 20：执行 transform.position + Vector3.up * 1.2f : head.position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                transform.position + Vector3.up * 1.2f : head.position;
            // 步骤 21：检查条件 Vector3.Distance(playerPosition, energyOrbs[i].transform.position) > 0.9f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (Vector3.Distance(playerPosition, energyOrbs[i].transform.position) > 0.9f) continue;
            // 步骤 22：执行 collected[i] = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            collected[i] = true;
            // 步骤 23：执行 EnergyCollected++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            EnergyCollected++;
            // 步骤 24：调用 Destroy：Destroy。
            Destroy(energyOrbs[i]);
            // 步骤 25：这里的 ! 表示取反；当 locomotion.desktopControlled 不成立时才进入此分支。
            if (!locomotion.desktopControlled)
                // 步骤 26：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
                GetComponent<CapeEnergySystem>()?.RestoreEnergy(2f);
            // 步骤 27：确认counter已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 counter.text = "Sky lights " + EnergyCollected + " / " + energyOrbs.Length;
            if (counter != null) counter.text = "Sky lights " + EnergyCollected + " / " + energyOrbs.Length;
            // 步骤 28：执行 status?.Show("Sky energy collected: " + EnergyCollected + " / " + energyOrbs.Length);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show("Sky energy collected: " + EnergyCollected + " / " + energyOrbs.Length);
            // 步骤 29：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Left, 0.35f, 0.08f);
        }
    }
    // ！！！功能结束：Update！！！
}
