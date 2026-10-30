// ！！！文件功能：在编辑器检查飞行练习场景和充能区设置！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：此文件位于 Assets/Editor，仅用于 Unity 编辑器的搭建与验证，玩家安装的 APK 不执行这里的方法。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 System.IO 命名空间；下方可直接使用File 文件与 Directory 文件夹操作。
using System.IO;
// 引入 UnityEditor 命名空间；下方可直接使用仅在 Unity 编辑器内使用的资源和菜单接口。
using UnityEditor;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

// 定义 CapeFlightValidation 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class CapeFlightValidation
{
    // 定义固定文字 Report（Report）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string Report = "Assets/SkyQuest/CapeFlightValidationReport.txt";

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Validate cape flight system")]
    // ！！！功能：Run——Run！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void Run()
    {
        // 步骤 1：创建新的 List<string> 并存为 lines；构造参数决定其初始内容或对象名称。
        List<string> lines = new List<string> { "Cape flight editor validation — " + DateTime.Now.ToString("u") };
        // 步骤 2：计算 null，把结果存进 rig（rig）；之后引用 rig 时用的就是这次结果。
        GameObject rig = null;
        // 步骤 3：计算 null，把结果存进 firstZone（firstZone）；之后引用 firstZone 时用的就是这次结果。
        GameObject firstZone = null;
        // 步骤 4：计算 null，把结果存进 secondZone（secondZone）；之后引用 secondZone 时用的就是这次结果。
        GameObject secondZone = null;
        // 步骤 5：这一行补充 try，与相邻行合起来完成同一次计算或对象配置。
        try
        {
            // 步骤 6：把计算结果 AssetDatabase.LoadAssetAtPath<InputActionAsset>( 保存为actions；下一条判断或调用会读取这个具体值。
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                // 步骤 7：执行 "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");
            // 步骤 8：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(actions != null && actions.FindActionMap("Cape Flight") != null,
                // 步骤 9：执行 "OpenXR Input Actions asset and map import", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "OpenXR Input Actions asset and map import", lines);
            // 步骤 10：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(actions.FindAction("Cape Flight/Boost") != null &&
                // 步骤 11：调用 FindAction：查找Action。
                actions.FindAction("Cape Flight/Move") != null,
                // 步骤 12：执行 "Boost and steering actions exist", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Boost and steering actions exist", lines);
            // 步骤 13：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            Check(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/SkyQuest/CapeFlightTest.unity") != null,
                // 步骤 14：执行 "Saved VR test scene exists", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Saved VR test scene exists", lines);

            // 步骤 15：新建用于rig的对象或数值结构，并把构造参数保存进去。
            rig = new GameObject("Cape flight validation rig");
            // 步骤 16：计算 rig.AddComponent<CapeEnergySystem>()，把结果存进 energy（energy）；之后引用 energy 时用的就是这次结果。
            CapeEnergySystem energy = rig.AddComponent<CapeEnergySystem>();
            // 步骤 17：调用 ResetEnergy：将当前能量恢复为场景规定的初始值。
            energy.ResetEnergy();
            // 步骤 18：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(Mathf.Approximately(energy.CurrentEnergy, 15f), "Initial energy is 15", lines);
            // 步骤 19：计算 rig.AddComponent<CapeFlightController>()，把结果存进 flight（flight）；之后引用 flight 时用的就是这次结果。
            CapeFlightController flight = rig.AddComponent<CapeFlightController>();
            // 步骤 20：把括号里的依赖对象交给 flight 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            flight.Initialize(energy);
            // 步骤 21：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(flight.TryBoost() && Mathf.Approximately(energy.CurrentEnergy, 14f) &&
                // 步骤 22：把计算结果 = CapeFlightState.Takeoff, "Ground takeoff spends exactly 1", lines) 保存为状态；下一条判断或调用会读取这个具体值。
                flight.State == CapeFlightState.Takeoff, "Ground takeoff spends exactly 1", lines);
            // 步骤 23：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(!flight.TryBoost() && Mathf.Approximately(energy.CurrentEnergy, 14f),
                // 步骤 24：执行 "Cooldown rejects repeat without charge", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Cooldown rejects repeat without charge", lines);
            // 步骤 25：调用 ReportCollision：Report碰撞。
            flight.ReportCollision(CollisionFlags.Sides);
            // 步骤 26：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(flight.State == CapeFlightState.Takeoff, "Wall contact does not cause landing", lines);
            // 步骤 27：调用 ReportCollision：Report碰撞。
            flight.ReportCollision(CollisionFlags.Below);
            // 步骤 28：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(flight.State == CapeFlightState.Takeoff, "Upward contact does not cancel takeoff", lines);
            // 步骤 29：调用 SafeStop：SafeStop。
            flight.SafeStop();
            // 步骤 30：调用 ReportCollision：Report碰撞。
            flight.ReportCollision(CollisionFlags.Below);
            // 步骤 31：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(flight.State == CapeFlightState.Landing, "Downward ground contact enters landing", lines);
            // 步骤 32：调用 ReportCollision：Report碰撞。
            flight.ReportCollision(CollisionFlags.Below);
            // 步骤 33：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(flight.State == CapeFlightState.Grounded, "Stable support enters grounded", lines);

            // 步骤 34：把计算结果 0f 保存为initialEnergy；下一条判断或调用会读取这个具体值。
            energy.initialEnergy = 0f;
            // 步骤 35：调用 ResetEnergy：将当前能量恢复为场景规定的初始值。
            energy.ResetEnergy();
            // 步骤 36：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(!flight.TryBoost() && Mathf.Approximately(energy.CurrentEnergy, 0f),
                // 步骤 37：执行 "Zero energy cannot boost", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Zero energy cannot boost", lines);
            // 步骤 38：计算 rig.AddComponent<CapeRechargeReceiver>()，把结果存进 receiver（receiver）；之后引用 receiver 时用的就是这次结果。
            CapeRechargeReceiver receiver = rig.AddComponent<CapeRechargeReceiver>();
            // 步骤 39：把括号里的依赖对象交给 receiver 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            receiver.Initialize(energy, flight);
            // 步骤 40：把计算结果 Zone("Slow recharge", 1f) 保存为firstZone；下一条判断或调用会读取这个具体值。
            firstZone = Zone("Slow recharge", 1f);
            // 步骤 41：把计算结果 Zone("Fast recharge", 2f) 保存为secondZone；下一条判断或调用会读取这个具体值。
            secondZone = Zone("Fast recharge", 2f);
            // 步骤 42：从目标物体找到所需组件并存为 slow；使用前应检查是否为 null。
            CapeRechargeZone slow = firstZone.GetComponent<CapeRechargeZone>();
            // 步骤 43：从目标物体找到所需组件并存为 fast；使用前应检查是否为 null。
            CapeRechargeZone fast = secondZone.GetComponent<CapeRechargeZone>();
            // 步骤 44：调用 Register：登记一个正在作用于玩家的充能区域。
            receiver.Register(slow);
            // 步骤 45：调用 Register：登记一个正在作用于玩家的充能区域。
            receiver.Register(fast);
            // 步骤 46：调用 Step：Step。
            receiver.Step(0.5f);
            // 步骤 47：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(Mathf.Approximately(energy.CurrentEnergy, 1f),
                // 步骤 48：执行 "Overlapping zones use highest rate only", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Overlapping zones use highest rate only", lines);
            // 步骤 49：调用 Unregister：玩家离开时移除对应充能区域，停止该区域持续充能。
            receiver.Unregister(fast);
            // 步骤 50：调用 Step：Step。
            receiver.Step(0.5f);
            // 步骤 51：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(Mathf.Approximately(energy.CurrentEnergy, 1.5f),
                // 步骤 52：执行 "Leaving fast zone leaves slow contribution", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Leaving fast zone leaves slow contribution", lines);
            // 步骤 53：调用 Unregister：玩家离开时移除对应充能区域，停止该区域持续充能。
            receiver.Unregister(slow);
            // 步骤 54：调用 Step：Step。
            receiver.Step(0.5f);
            // 步骤 55：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(Mathf.Approximately(energy.CurrentEnergy, 1.5f),
                // 步骤 56：执行 "Leaving all zones stops recharge", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Leaving all zones stops recharge", lines);
            // 步骤 57：调用 RestoreEnergy：把指定能量加回斗篷并限制不超过最大值。
            energy.RestoreEnergy(100f);
            // 步骤 58：调用 Check：检查场景对象、引用或配置是否满足预期，并登记结果。
            Check(Mathf.Approximately(energy.CurrentEnergy, energy.maxEnergy),
                // 步骤 59：执行 "Recharge clamps to maximum", lines);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Recharge clamps to maximum", lines);
            // 步骤 60：把括号里的新元素加入 lines 集合；后续会按集合顺序使用它。
            lines.Add("RESULT: PASS — editor checks only; Quest controller/headset checks remain manual.");
            // 步骤 61：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log("Cape flight validation passed. " + Report);
        }
        // 步骤 62：调用 catch：catch。
        catch (Exception exception)
        {
            // 步骤 63：把括号里的新元素加入 lines 集合；后续会按集合顺序使用它。
            lines.Add("RESULT: FAIL — " + exception);
            // 步骤 64：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError("Cape flight validation failed: " + exception);
        }
        // 步骤 65：这一行补充 finally，与相邻行合起来完成同一次计算或对象配置。
        finally
        {
            // 步骤 66：确认secondZone已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 DestroyImmediate：DestroyImmediate。
            if (secondZone != null) UnityEngine.Object.DestroyImmediate(secondZone);
            // 步骤 67：确认firstZone已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 DestroyImmediate：DestroyImmediate。
            if (firstZone != null) UnityEngine.Object.DestroyImmediate(firstZone);
            // 步骤 68：确认rig已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 DestroyImmediate：DestroyImmediate。
            if (rig != null) UnityEngine.Object.DestroyImmediate(rig);
            // 步骤 69：把内存中的多行文字写到磁盘文件；例如生成场景配置或资源列表。
            File.WriteAllLines(Report, lines);
            // 步骤 70：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.ImportAsset(Report);
        }
    }
    // ！！！功能结束：Run！！！

    // ！！！功能：Zone——Zone！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, float rate 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject Zone(string name, float rate)
    {
        // 步骤 1：创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name);
        // 步骤 2：计算 obj.AddComponent<SphereCollider>()，把结果存进 collider（collider）；之后引用 collider 时用的就是这次结果。
        SphereCollider collider = obj.AddComponent<SphereCollider>();
        // 步骤 3：把 collider.isTrigger 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        collider.isTrigger = true;
        // 步骤 4：执行 obj.AddComponent<CapeRechargeZone>().rechargePerSecond = rate;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.AddComponent<CapeRechargeZone>().rechargePerSecond = rate;
        // 步骤 5：把 obj 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj;
    }
    // ！！！功能结束：Zone！！！

    // ！！！功能：Check——检查场景对象、引用或配置是否满足预期，并登记结果！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool condition, string name, List<string> lines 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Check(bool condition, string name, List<string> lines)
    {
        // 步骤 1：把括号里的新元素加入 lines 集合；后续会按集合顺序使用它。
        lines.Add((condition ? "PASS " : "FAIL ") + name);
        // 步骤 2：这里的 ! 表示取反；当 condition 不成立时才进入此分支。 成立时执行 throw new Exception(name);
        if (!condition) throw new Exception(name);
    }
    // ！！！功能结束：Check！！！
}
