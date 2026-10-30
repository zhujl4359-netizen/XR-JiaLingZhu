// ！！！文件功能：在编辑器中把飞行系统安装到场景！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：此文件位于 Assets/Editor，仅用于 Unity 编辑器的搭建与验证，玩家安装的 APK 不执行这里的方法。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 UnityEditor 命名空间；下方可直接使用仅在 Unity 编辑器内使用的资源和菜单接口。
using UnityEditor;
// 引入 UnityEditor.SceneManagement 命名空间；下方可直接使用编辑器内创建、打开、保存场景的接口。
using UnityEditor.SceneManagement;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;
// 引入 UnityEngine.SceneManagement 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.SceneManagement;

// 定义 CapeFlightSceneInstaller 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class CapeFlightSceneInstaller
{
    // 定义固定文字 QuestScene（Quest场景）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string QuestScene = "Assets/SkyQuest/SkyQuest3S.unity";
    // 定义固定文字 ActionsPath（Actions路径）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string ActionsPath = "Assets/SkyQuest/Resources/CapeFlightActions.inputactions";

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Install cape flight in Quest scene")]
    // ！！！功能：Install——Install！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void Install()
    {
        // 步骤 1：计算 EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Single);
        // 步骤 2：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>()，把结果存进 locomotion（locomotion）；之后引用 locomotion 时用的就是这次结果。
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        // 步骤 3：检查locomotion是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (locomotion == null || locomotion.desktopControlled)
            // 步骤 4：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Quest scene VR locomotion rig was not found.");
        // 步骤 5：计算 locomotion.gameObject，把结果存进 rig（rig）；之后引用 rig 时用的就是这次结果。
        GameObject rig = locomotion.gameObject;
        // 步骤 6：逐个读取 GameObject root in scene.GetRootGameObjects() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            // 步骤 7：这里用 || 表示任一条件成立就处理：root == rig || root.name != "XR Origin (XR Rig)" || !root.activeSelf；用于合并几种停止或跳过原因。 成立时执行 continue;
            if (root == rig || root.name != "XR Origin (XR Rig)" || !root.activeSelf) continue;
            // 步骤 8：把本次编辑器改动登记到撤销记录；用户可以从 Unity 菜单撤销。
            Undo.RecordObject(root, "Disable duplicate XR Origin");
            // 步骤 9：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            root.SetActive(false);
            // 步骤 10：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(root);
        }
        // 步骤 11：执行 GetOrAdd<CapeEnergySystem>(rig);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        GetOrAdd<CapeEnergySystem>(rig);
        // 步骤 12：从目标物体找到所需组件并存为 flight；使用前应检查是否为 null。
        CapeFlightController flight = rig.GetComponent<CapeFlightController>();
        // 步骤 13：检查flight是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (flight == null)
        {
            // 步骤 14：把计算结果 Undo.AddComponent<CapeFlightController>(rig) 保存为flight；下一条判断或调用会读取这个具体值。
            flight = Undo.AddComponent<CapeFlightController>(rig);
            // 步骤 15：把计算结果 locomotion.groundSpeed 保存为地面行走速度；下一条判断或调用会读取这个具体值。
            flight.groundSpeed = locomotion.groundSpeed;
        }
        // 步骤 16：把计算结果 AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath) 保存为actions；下一条判断或调用会读取这个具体值。
        flight.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
        // 步骤 17：检查flight的actions是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("Cape flight input asset was not imported.");
        if (flight.actions == null) throw new Exception("Cape flight input asset was not imported.");
        // 步骤 18：执行 GetOrAdd<CapeEnergyUI>(rig);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        GetOrAdd<CapeEnergyUI>(rig);
        // 步骤 19：执行 GetOrAdd<CapeFlightFeedback>(rig);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        GetOrAdd<CapeFlightFeedback>(rig);
        // 步骤 20：执行 GetOrAdd<CapeRechargeReceiver>(rig);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        GetOrAdd<CapeRechargeReceiver>(rig);
        // 步骤 21：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(flight);
        // 步骤 22：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 23：这里的 ! 表示取反；当 EditorSceneManager.SaveScene(scene) 不成立时才进入此分支。 成立时执行 throw new Exception("Quest scene could not be saved.");
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Quest scene could not be saved.");
        // 步骤 24：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Cape flight components installed on the existing Quest VR rig.");
    }
    // ！！！功能结束：Install！！！

    // 这一行补充 private static T GetOrAdd<T>(GameObject gameObject) where T : Component，与相邻行合起来完成同一次计算或对象配置。
    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        // 从目标物体找到所需组件并存为 component；使用前应检查是否为 null。
        T component = gameObject.GetComponent<T>();
        // 把 component != null ? component : Undo.AddComponent<T>(gameObject) 作为方法结果返回给调用方；return 后本方法立即结束。
        return component != null ? component : Undo.AddComponent<T>(gameObject);
    }
}
