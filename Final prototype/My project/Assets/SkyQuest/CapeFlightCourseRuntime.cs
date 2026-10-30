// ！！！文件功能：管理飞行练习关卡的检查点、完成条件与回到安全位置！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

/// <summary>Checkpoint and recovery for the standalone flight practice scene.</summary>
// 定义 CapeFlightCourseRuntime 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeFlightCourseRuntime : MonoBehaviour
{
    // 创建新的 Vector3 并存为 startingCheckpoint；构造参数决定其初始内容或对象名称。
    public Vector3 startingCheckpoint = new Vector3(0f, 0.55f, 0f);
    // 把 Inspector 可调整的 recoveryHeight（recoveryHeight）设为默认值 -3.4f；具体用途看它在下方的判断和计算。
    public float recoveryHeight = -3.4f;
    // 声明 checkpoint（checkpoint）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 checkpoint;
    // flight 保存flight（类型 CapeFlightController）；下方方法依靠它记录或查找场景状态。
    private CapeFlightController flight;
    // energy 保存energy（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    private CapeEnergySystem energy;
    // 声明 nextRecovery（nextRecovery）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextRecovery;
    // 声明 finished（finished）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool finished;
    // 声明 suppressSafeStop（suppressSafeStop）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool suppressSafeStop;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Start()
    {
        // 步骤 1：把计算结果 startingCheckpoint 保存为checkpoint；下一条判断或调用会读取这个具体值。
        checkpoint = startingCheckpoint;
        // 步骤 2：从目标 GameObject 查找并保存 flight 组件；后面的移动或显示逻辑会直接使用这个缓存。
        flight = GetComponent<CapeFlightController>();
        // 步骤 3：从目标 GameObject 查找并保存 energy 组件；后面的移动或显示逻辑会直接使用这个缓存。
        energy = GetComponent<CapeEnergySystem>();
        // 步骤 4：确认flight已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 flight.OnSafeStop += SafeStop;
        if (flight != null) flight.OnSafeStop += SafeStop;
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Update()
    {
        // 步骤 1：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
        if (transform.position.y < recoveryHeight && Time.time >= nextRecovery)
            // 步骤 2：调用 ReturnToCheckpoint：Return到Checkpoint。
            ReturnToCheckpoint();
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：ReachCheckpoint——ReachCheckpoint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 position 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void ReachCheckpoint(Vector3 position)
    {
        // 步骤 1：把计算结果 position 保存为checkpoint；下一条判断或调用会读取这个具体值。
        checkpoint = position;
        // 步骤 2：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Cape flight checkpoint reached.");
    }
    // ！！！功能结束：ReachCheckpoint！！！

    // ！！！功能：Finish——Finish！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Finish()
    {
        // 步骤 1：把 finished 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        finished = true;
        // 步骤 2：计算 GameObject.Find("Finish instructions")，把结果存进 sign（sign）；之后引用 sign 时用的就是这次结果。
        GameObject sign = GameObject.Find("Finish instructions");
        // 步骤 3：检查sign是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (sign != null && sign.TryGetComponent(out TextMesh text))
            // 步骤 4：把新内容写入 text.text；玩家看到的文字会在本帧更新。
            text.text = "COURSE COMPLETE\nR CLICK: RESTART";
        // 步骤 5：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Cape flight course completed. Right stick click restarts the course.");
        // 步骤 6：执行 energy?.RestoreEnergy(energy.maxEnergy);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        energy?.RestoreEnergy(energy.maxEnergy);
    }
    // ！！！功能结束：Finish！！！

    // ！！！功能：SafeStop——SafeStop！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SafeStop()
    {
        // 步骤 1：检查条件 suppressSafeStop；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (suppressSafeStop) return;
        // 步骤 2：检查条件 finished；只有为真才运行下面的步骤，否则沿原流程继续。
        if (finished)
        {
            // 步骤 3：把 finished 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            finished = false;
            // 步骤 4：把计算结果 startingCheckpoint 保存为checkpoint；下一条判断或调用会读取这个具体值。
            checkpoint = startingCheckpoint;
            // 步骤 5：逐个读取 CapeFlightCourseMarker marker in FindObjectsByType<CapeFlightCourseMarker>(FindObject 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (CapeFlightCourseMarker marker in FindObjectsByType<CapeFlightCourseMarker>(FindObjectsSortMode.None))
                // 步骤 6：调用 ResetMarker：复位Marker。
                marker.ResetMarker();
            // 步骤 7：计算 GameObject.Find("Finish instructions")，把结果存进 sign（sign）；之后引用 sign 时用的就是这次结果。
            GameObject sign = GameObject.Find("Finish instructions");
            // 步骤 8：检查sign是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (sign != null && sign.TryGetComponent(out TextMesh text))
                // 步骤 9：把新内容写入 text.text；玩家看到的文字会在本帧更新。
                text.text = "LAND HERE TO COMPLETE\nFALLS RETURN TO CHECKPOINT";
            // 步骤 10：调用 ReturnToCheckpoint：Return到Checkpoint。
            ReturnToCheckpoint();
            // 步骤 11：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 12：检查条件 transform.position.y < -1f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 ReturnToCheckpoint：Return到Checkpoint。
        if (transform.position.y < -1f) ReturnToCheckpoint();
    }
    // ！！！功能结束：SafeStop！！！

    // ！！！功能：ReturnToCheckpoint——Return到Checkpoint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ReturnToCheckpoint()
    {
        // 步骤 1：根据当前游戏时间计算并保存nextRecovery，用于后续延迟或冷却判断。
        nextRecovery = Time.time + 1f;
        // 步骤 2：从目标物体找到所需组件并存为 controller；使用前应检查是否为 null。
        CharacterController controller = GetComponent<CharacterController>();
        // 步骤 3：确认controller已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 controller.enabled = false;
        if (controller != null) controller.enabled = false;
        // 步骤 4：把position更新为 checkpoint；世界或局部位置会立即影响物体摆放。
        transform.position = checkpoint;
        // 步骤 5：确认controller已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 controller.enabled = true;
        if (controller != null) controller.enabled = true;
        // 步骤 6：把 suppressSafeStop 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        suppressSafeStop = true;
        // 步骤 7：执行 flight?.SafeStop();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        flight?.SafeStop();
        // 步骤 8：把 suppressSafeStop 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        suppressSafeStop = false;
        // 步骤 9：执行 energy?.RestoreEnergy(energy.maxEnergy);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        energy?.RestoreEnergy(energy.maxEnergy);
        // 步骤 10：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Cape flight recovery: returned to checkpoint with full energy.");
    }
    // ！！！功能结束：ReturnToCheckpoint！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDestroy()
    {
        // 步骤 1：确认flight已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 flight.OnSafeStop -= SafeStop;
        if (flight != null) flight.OnSafeStop -= SafeStop;
    }
    // ！！！功能结束：OnDestroy！！！
}
