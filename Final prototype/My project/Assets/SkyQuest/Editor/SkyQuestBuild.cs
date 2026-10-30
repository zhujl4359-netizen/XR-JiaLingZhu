// ！！！文件功能：在编辑器创建、配置并验证 IP1 的桌面和 Quest 场景与 APK！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：此文件位于 Assets/Editor，仅用于 Unity 编辑器的搭建与验证，玩家安装的 APK 不执行这里的方法。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 UnityEditor 命名空间；下方可直接使用仅在 Unity 编辑器内使用的资源和菜单接口。
using UnityEditor;
// 引入 UnityEditor.Build 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEditor.Build;
// 引入 UnityEditor.SceneManagement 命名空间；下方可直接使用编辑器内创建、打开、保存场景的接口。
using UnityEditor.SceneManagement;
// 引入 UnityEditor.XR.Management 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEditor.XR.Management;
// 引入 UnityEditor.XR.Management.Metadata 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEditor.XR.Management.Metadata;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.SceneManagement 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.SceneManagement;
// 引入 UnityEngine.Rendering 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.Rendering;
// 引入 UnityEngine.Rendering.Universal 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.Rendering.Universal;
// 引入 Unity.XR.CoreUtils 命名空间；下方可直接使用该库提供的类型和方法。
using Unity.XR.CoreUtils;
// 引入 UnityEngine.XR.Management 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.XR.Management;
// 引入 UnityEngine.XR.OpenXR 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.XR.OpenXR;
// 引入 UnityEngine.XR.OpenXR.Features.Interactions 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.XR.OpenXR.Features.Interactions;
// 引入 UnityEngine.XR.OpenXR.Features.MetaQuestSupport 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
// 引入 UnityEngine.XR.OpenXR.Features.CompositionLayers 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.XR.OpenXR.Features.CompositionLayers;

// 定义 SkyQuestBuild 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestBuild
{
    // 定义固定文字 SourceScene（Source场景）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string SourceScene = "Assets/Scenes/SampleScene.unity";
    // 定义固定文字 QuestScene（Quest场景）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string QuestScene = "Assets/SkyQuest/SkyQuest3S.unity";
    // 定义固定文字 DesktopScene（桌面场景）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string DesktopScene = "Assets/SkyQuest/SkyDesktop.unity";
    // 定义固定文字 RigPrefab（Rig预制体）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string RigPrefab = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Install Magic exchange trees in both scenes")]
    // ！！！功能：InstallMagicTreesInBothScenes——InstallMagicTrees在BothScenes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void InstallMagicTreesInBothScenes()
    {
        // 步骤 1：调用 InstallMagicTree：InstallMagicTree。
        InstallMagicTree(QuestScene);
        // 步骤 2：调用 InstallMagicTree：InstallMagicTree。
        InstallMagicTree(DesktopScene);
        // 步骤 3：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Magic exchange trees installed for both plaza ancestors in Quest and desktop scenes.");
    }
    // ！！！功能结束：InstallMagicTreesInBothScenes！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Install ancestor memorial in both scenes")]
    // ！！！功能：InstallAncestorMemorialBothScenes——InstallAncestorMemorialBothScenes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void InstallAncestorMemorialBothScenes()
    {
        // 步骤 1：调用 InstallAncestorMemorial：InstallAncestorMemorial。
        InstallAncestorMemorial(QuestScene);
        // 步骤 2：调用 InstallAncestorMemorial：InstallAncestorMemorial。
        InstallAncestorMemorial(DesktopScene);
        // 步骤 3：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Ancestor memorial installed in Quest and keyboard scenes.");
    }
    // ！！！功能结束：InstallAncestorMemorialBothScenes！！！

    // ！！！功能：InstallAncestorMemorial——InstallAncestorMemorial！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string scenePath 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void InstallAncestorMemorial(string scenePath)
    {
        // 步骤 1：计算 EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        // 步骤 2：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>()，把结果存进 tree（tree）；之后引用 tree 时用的就是这次结果。
        SkyQuestAncestor tree = UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>();
        // 步骤 3：检查tree、tree的ancestor是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (tree == null || tree.ancestor == null)
            // 步骤 4：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Original ancestor missing in " + scenePath);
        // 步骤 5：确认tree的memorialStone已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (tree.memorialStone != null)
            // 步骤 6：在编辑器里立即移除旧对象；重新搭建时避免上次生成内容重复叠加。
            UnityEngine.Object.DestroyImmediate(tree.memorialStone.gameObject);
        // 步骤 7：把计算结果 BuildAncestorMemorial(tree.ancestor) 保存为memorialStone；下一条判断或调用会读取这个具体值。
        tree.memorialStone = BuildAncestorMemorial(tree.ancestor);
        // 步骤 8：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(tree);
        // 步骤 9：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 10：这里的 ! 表示取反；当 EditorSceneManager.SaveScene(scene) 不成立时才进入此分支。
        if (!EditorSceneManager.SaveScene(scene))
            // 步骤 11：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Could not save memorial in " + scenePath);
    }
    // ！！！功能结束：InstallAncestorMemorial！！！

    // ！！！功能：BuildAncestorMemorial——构建AncestorMemorial！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform ancestor 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Transform BuildAncestorMemorial(Transform ancestor)
    {
        // 步骤 1：创建新的 Transform 并存为 root；构造参数决定其初始内容或对象名称。
        Transform root = new GameObject("Ancestor memorial stone").transform;
        // 步骤 2：把position更新为 MemorialPosition(ancestor)；世界或局部位置会立即影响物体摆放。
        root.position = MemorialPosition(ancestor);
        // 步骤 3：计算 VisualForward(ancestor)，把结果存进 forward（forward）；之后引用 forward 时用的就是这次结果。
        Vector3 forward = VisualForward(ancestor);
        // 步骤 4：把rotation更新为 Quaternion.LookRotation(forward, Vector3.up)；物体朝向会在本帧改变。
        root.rotation = Quaternion.LookRotation(forward, Vector3.up);
        // 步骤 5：计算 AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/BlueStone.m，把结果存进 blue（blue）；之后引用 blue 时用的就是这次结果。
        Material blue = AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/BlueStone.mat");
        // 步骤 6：计算 AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/WarmLimesto，把结果存进 limestone（limestone）；之后引用 limestone 时用的就是这次结果。
        Material limestone = AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/WarmLimestone.mat");
        // 步骤 7：计算 AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/PaleGold.ma，把结果存进 gold（gold）；之后引用 gold 时用的就是这次结果。
        Material gold = AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/PaleGold.mat");
        // 步骤 8：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Limestone footing", new Vector3(0f, 0.08f, 0f), new Vector3(1.18f, 0.16f, 0.52f), limestone);
        // 步骤 9：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Blue stone tablet", new Vector3(0f, 1.12f, 0f), new Vector3(0.89f, 2.05f, 0.32f), blue);
        // 步骤 10：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Carved crown", new Vector3(0f, 2.18f, 0f), new Vector3(1.04f, 0.12f, 0.39f), limestone, false);
        // 步骤 11：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Left border", new Vector3(-0.39f, 1.13f, 0.17f), new Vector3(0.035f, 1.8f, 0.025f), limestone, false);
        // 步骤 12：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Right border", new Vector3(0.39f, 1.13f, 0.17f), new Vector3(0.035f, 1.8f, 0.025f), limestone, false);
        // 步骤 13：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Top carved line", new Vector3(0f, 1.91f, 0.17f), new Vector3(0.63f, 0.025f, 0.025f), gold, false);
        // 步骤 14：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Lower carved line", new Vector3(0f, 0.37f, 0.17f), new Vector3(0.63f, 0.025f, 0.025f), gold, false);
        // 步骤 15：新建用于diamond的对象或数值结构，并把构造参数保存进去。
        GameObject diamond = MemorialPart(root, "Memorial star", new Vector3(0f, 1.52f, 0.185f),
            // 步骤 16：用 new 创建 Vector3(0.23f, 0.23f, 0.025f), gold, false);；构造参数决定新对象的初始内容。
            new Vector3(0.23f, 0.23f, 0.025f), gold, false);
        // 步骤 17：执行 diamond.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        diamond.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        // 步骤 18：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Star upper ray", new Vector3(0f, 1.77f, 0.185f), new Vector3(0.018f, 0.16f, 0.02f), gold, false);
        // 步骤 19：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Star lower ray", new Vector3(0f, 1.27f, 0.185f), new Vector3(0.018f, 0.16f, 0.02f), gold, false);
        // 步骤 20：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Star left ray", new Vector3(-0.25f, 1.52f, 0.185f), new Vector3(0.16f, 0.018f, 0.02f), gold, false);
        // 步骤 21：调用 MemorialPart：MemorialPart。
        MemorialPart(root, "Star right ray", new Vector3(0.25f, 1.52f, 0.185f), new Vector3(0.16f, 0.018f, 0.02f), gold, false);
        // 步骤 22：把 root 作为方法结果返回给调用方；return 后本方法立即结束。
        return root;
    }
    // ！！！功能结束：BuildAncestorMemorial！！！

    // 继续填写上一行调用的参数：private static GameObject MemorialPart(Transform parent, string name, Vector3 position；参数位置决定它表示的坐标、速度或选项。
    private static GameObject MemorialPart(Transform parent, string name, Vector3 position,
        // 这一行补充 Vector3 scale, Material material, bool collision = true)，与相邻行合起来完成同一次计算或对象配置。
        Vector3 scale, Material material, bool collision = true)
    {
        // 计算 GameObject.CreatePrimitive(PrimitiveType.Cube)，把结果存进 part（part）；之后引用 part 时用的就是这次结果。
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // 把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        part.name = name;
        // 执行 part.transform.SetParent(parent, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        part.transform.SetParent(parent, false);
        // 执行 part.transform.localPosition = position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        part.transform.localPosition = position;
        // 执行 part.transform.localScale = scale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        part.transform.localScale = scale;
        // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        part.GetComponent<Renderer>().sharedMaterial = material;
        // 这里的 ! 表示取反；当 collision 不成立时才进入此分支。 成立时调用 DestroyImmediate：DestroyImmediate。
        if (!collision) UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
        // 把 part 作为方法结果返回给调用方；return 后本方法立即结束。
        return part;
    }

    // ！！！功能：MemorialPosition——Memorial位置！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform ancestor 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Vector3 MemorialPosition(Transform ancestor)
    {
        // 步骤 1：计算 VisibleBounds(ancestor)，把结果存进 bounds（bounds）；之后引用 bounds 时用的就是这次结果。
        Bounds bounds = VisibleBounds(ancestor);
        // 步骤 2：用三元表达式为 ground 二选一：问号前条件为真取前值，为假取冒号后的值。
        float ground = bounds.size.y > 0.01f ? bounds.min.y : ancestor.position.y;
        // 步骤 3：计算 VisualForward(ancestor)，把结果存进 forward（forward）；之后引用 forward 时用的就是这次结果。
        Vector3 forward = VisualForward(ancestor);
        // 步骤 4：计算 ancestor.position - forward * 0.95f，把结果存进 behind（behind）；之后引用 behind 时用的就是这次结果。
        Vector3 behind = ancestor.position - forward * 0.95f;
        // 步骤 5：把 new Vector3(behind.x, ground, behind.z) 作为方法结果返回给调用方；return 后本方法立即结束。
        return new Vector3(behind.x, ground, behind.z);
    }
    // ！！！功能结束：MemorialPosition！！！

    // ！！！功能：VisualForward——VisualForward！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform ancestor 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Vector3 VisualForward(Transform ancestor)
    {
        // The COTL figure faces local -Z. C1 has a nested 180-degree visual,
        // so its root's +Z is the visible forward direction.
        // 步骤 1：用 reversed 记住此处真假结果（ancestor.name == "COTL traveller"），供下一条 if 选择执行路径。
        bool reversed = ancestor.name == "COTL traveller";
        // 步骤 2：用三元表达式为 axis 二选一：问号前条件为真取前值，为假取冒号后的值。
        Vector3 axis = reversed ? -ancestor.forward : ancestor.forward;
        // 步骤 3：把 Vector3.ProjectOnPlane(axis, Vector3.up).normalized 作为方法结果返回给调用方；return 后本方法立即结束。
        return Vector3.ProjectOnPlane(axis, Vector3.up).normalized;
    }
    // ！！！功能结束：VisualForward！！！

    // ！！！功能：ValidateMemorial——验证Memorial！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 SkyQuestAncestor tree 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ValidateMemorial(SkyQuestAncestor tree)
    {
        // 步骤 1：调用 ValidateMemorial：验证Memorial。
        ValidateMemorial(tree.ancestor, tree.memorialStone);
    }
    // ！！！功能结束：ValidateMemorial！！！

    // ！！！功能：ValidateMemorial——验证Memorial！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform ancestor, Transform memorial 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ValidateMemorial(Transform ancestor, Transform memorial)
    {
        // 步骤 1：计算 VisualForward(ancestor)，把结果存进 forward（forward）；之后引用 forward 时用的就是这次结果。
        Vector3 forward = VisualForward(ancestor);
        // 步骤 2：把原三维朝向投影到地面平面并存为 offset；行走或转向不受头部俯仰影响。
        Vector3 offset = Vector3.ProjectOnPlane(memorial.position - ancestor.position, Vector3.up);
        // 步骤 3：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (Mathf.Abs(Vector3.Dot(offset, forward) + 0.95f) > 0.05f ||
            // 步骤 4：调用 Cross：Cross。
            Vector3.Cross(offset, forward).magnitude > 0.05f ||
            // 步骤 5：调用 Dot：Dot。
            Vector3.Dot(memorial.forward, forward) < 0.99f)
            // 步骤 6：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Memorial is not centered directly behind its ancestor.");
        // 步骤 7：计算 memorial.Find("Blue stone tablet")，把结果存进 tablet（tablet）；之后引用 tablet 时用的就是这次结果。
        Transform tablet = memorial.Find("Blue stone tablet");
        // 步骤 8：检查tablet、shared材质是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (tablet == null || tablet.GetComponent<Renderer>().sharedMaterial == null)
            // 步骤 9：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Memorial stone material is missing.");
    }
    // ！！！功能结束：ValidateMemorial！！！

    // ！！！功能：InstallMagicTree——InstallMagicTree！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string scenePath 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void InstallMagicTree(string scenePath)
    {
        // 步骤 1：计算 EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        // 步骤 2：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>()，把结果存进 locomotion（locomotion）；之后引用 locomotion 时用的就是这次结果。
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        // 步骤 3：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>()，把结果存进 original（original）；之后引用 original 时用的就是这次结果。
        SkyQuestAncestor original = UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>();
        // 步骤 4：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestStatus>()，把结果存进 status（status）；之后引用 status 时用的就是这次结果。
        SkyQuestStatus status = UnityEngine.Object.FindFirstObjectByType<SkyQuestStatus>();
        // 步骤 5：把计算结果 { "Shibiyama dancer", "Sky prayer guardian", "COTL traveller", 保存为names；下一条判断或调用会读取这个具体值。
        string[] names = { "Shibiyama dancer", "Sky prayer guardian", "COTL traveller",
            // 步骤 6：继续填写上一行调用的参数："Candle pilgrim", "Skeleton pilgrim - grounded gallery character"；参数位置决定它表示的坐标、速度或选项。
            "Candle pilgrim", "Skeleton pilgrim - grounded gallery character",
            // 步骤 7：执行 "Original C1 player - visible gallery character" };；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Original C1 player - visible gallery character" };
        // 步骤 8：检查locomotion、original、status是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (locomotion == null || original == null || status == null)
            // 步骤 9：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Cannot install Magic exchange; a required actor or gameplay system is missing in " + scenePath);
        // 步骤 10：从目标物体找到所需组件并存为 magic；使用前应检查是否为 null。
        SkyQuestMagicExchange magic = locomotion.GetComponent<SkyQuestMagicExchange>();
        // 步骤 11：如果场景对象没有 SkyQuestMagicExchange 组件，就当场创建并保存，保证后续调用有对象可用。
        if (magic == null) magic = locomotion.gameObject.AddComponent<SkyQuestMagicExchange>();
        // 步骤 12：把计算结果 locomotion.head 保存为头显相机；下一条判断或调用会读取这个具体值。
        magic.head = locomotion.head;
        // 步骤 13：把计算结果 locomotion.leftHand 保存为左手柄位置；下一条判断或调用会读取这个具体值。
        magic.leftHand = locomotion.leftHand;
        // 步骤 14：把计算结果 locomotion.rightHand 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        magic.rightHand = locomotion.rightHand;
        // 步骤 15：把计算结果 locomotion.avatar.transform 保存为角色模型；下一条判断或调用会读取这个具体值。
        magic.avatar = locomotion.avatar.transform;
        // 步骤 16：把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        magic.status = status;
        // 步骤 17：把计算结果 original 保存为legacyTree；下一条判断或调用会读取这个具体值。
        magic.legacyTree = original;
        // 步骤 18：把计算结果 locomotion.desktopControlled 保存为desktop模式；下一条判断或调用会读取这个具体值。
        magic.desktopMode = locomotion.desktopControlled;
        // 步骤 19：新建用于ancestors的对象或数值结构，并把构造参数保存进去。
        magic.ancestors = new Transform[names.Length];
        // 步骤 20：新建用于memorials的对象或数值结构，并把构造参数保存进去。
        magic.memorials = new Transform[names.Length];
        // 步骤 21：按 int i = 0; i < names.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < names.Length; i++)
        {
            // 步骤 22：计算 GameObject.Find(names[i])，把结果存进 actor（actor）；之后引用 actor 时用的就是这次结果。
            GameObject actor = GameObject.Find(names[i]);
            // 步骤 23：检查actor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("Gallery actor missing: " + names[i]);
            if (actor == null) throw new Exception("Gallery actor missing: " + names[i]);
            // 步骤 24：执行 magic.ancestors[i] = actor.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            magic.ancestors[i] = actor.transform;
            // 步骤 25：计算 GameObject.Find("Gallery memorial - " + names[i])，把结果存进 oldStone（oldStone）；之后引用 oldStone 时用的就是这次结果。
            GameObject oldStone = GameObject.Find("Gallery memorial - " + names[i]);
            // 步骤 26：确认oldStone已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 DestroyImmediate：DestroyImmediate。
            if (oldStone != null) UnityEngine.Object.DestroyImmediate(oldStone);
            // 步骤 27：计算 BuildAncestorMemorial(actor.transform)，把结果存进 stone（stone）；之后引用 stone 时用的就是这次结果。
            Transform stone = BuildAncestorMemorial(actor.transform);
            // 步骤 28：把计算结果 "Gallery memorial - " + names[i] 保存为name；下一条判断或调用会读取这个具体值。
            stone.name = "Gallery memorial - " + names[i];
            // 步骤 29：执行 magic.memorials[i] = stone;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            magic.memorials[i] = stone;
        }
        // 步骤 30：把计算结果 magic 保存为magicExchange；下一条判断或调用会读取这个具体值。
        original.magicExchange = magic;
        // 步骤 31：从目标物体找到所需组件并存为 controls；使用前应检查是否为 null。
        SkyQuestDesktopControls controls = locomotion.GetComponent<SkyQuestDesktopControls>();
        // 步骤 32：确认controls已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 controls.magicExchange = magic;
        if (controls != null) controls.magicExchange = magic;
        // 步骤 33：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(magic);
        // 步骤 34：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(original);
        // 步骤 35：确认controls已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetDirty：设置Dirty。
        if (controls != null) EditorUtility.SetDirty(controls);
        // 步骤 36：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 37：这里的 ! 表示取反；当 EditorSceneManager.SaveScene(scene) 不成立时才进入此分支。 成立时执行 throw new Exception("Could not save " + scenePath);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save " + scenePath);
    }
    // ！！！功能结束：InstallMagicTree！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Build and validate Quest + Keyboard scenes")]
    // ！！！功能：BuildAndValidateScenes——构建和验证Scenes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void BuildAndValidateScenes()
    {
        // 步骤 1：调用 CreateQuestScene：创建支持 Quest 头显与 Touch 手柄的 IP1 场景。
        CreateQuestScene();
        // 步骤 2：调用 CreateDesktopScene：创建可用键盘鼠标操作的 IP1 桌面场景。
        CreateDesktopScene();
        // 步骤 3：调用 BuildBoth：构建Both。
        SkyQuestEnvironmentBuilder.BuildBoth();
        // 步骤 4：调用 InstallMagicTreesInBothScenes：InstallMagicTrees在BothScenes。
        InstallMagicTreesInBothScenes();
        // 步骤 5：调用 ValidateBoth：验证Both。
        SkyQuestEnvironmentBuilder.ValidateBoth();
        // 步骤 6：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("SKY scenes rebuilt and validated.");
    }
    // ！！！功能结束：BuildAndValidateScenes！！！

    // ！！！功能：CreateQuestScene——创建支持 Quest 头显与 Touch 手柄的 IP1 场景！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void CreateQuestScene()
    {
        // 步骤 1：调用 ConfigureQuestProject：配置QuestProject。
        ConfigureQuestProject();
        // 步骤 2：计算 PrepareEnvironment(out Vector3 spawn, out AncestorExchangeTree oldTree)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = PrepareEnvironment(out Vector3 spawn, out AncestorExchangeTree oldTree);
        // 步骤 3：逐个读取 GameObject root in scene.GetRootGameObjects() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (GameObject root in scene.GetRootGameObjects())
            // 步骤 4：检查条件 root.name == "XR Origin (XR Rig)"；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetActive：设置Active。
            if (root.name == "XR Origin (XR Rig)") root.SetActive(false);
        // 步骤 5：计算 AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefab)，把结果存进 prefab（prefab）；之后引用 prefab 时用的就是这次结果。
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefab);
        // 步骤 6：检查prefab是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("XRI Starter Assets XR Origin prefab was not found.");
        if (prefab == null) throw new Exception("XRI Starter Assets XR Origin prefab was not found.");
        // 步骤 7：计算 (GameObject)PrefabUtility.InstantiatePrefab(prefab)，把结果存进 rig（rig）；之后引用 rig 时用的就是这次结果。
        GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        // 步骤 8：把计算结果 "Sky Quest 3S XR Origin" 保存为name；下一条判断或调用会读取这个具体值。
        rig.name = "Sky Quest 3S XR Origin";
        // 步骤 9：执行 rig.transform.position = spawn;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rig.transform.position = spawn;
        // 步骤 10：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        rig.GetComponent<XROrigin>().RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        // 步骤 11：逐个读取 MonoBehaviour behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (MonoBehaviour behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true))
        {
            // 步骤 12：检查behaviour是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (behaviour == null) continue;
            // 步骤 13：计算 behaviour.GetType().Name，把结果存进 typeName（typeName）；之后引用 typeName 时用的就是这次结果。
            string typeName = behaviour.GetType().Name;
            // 步骤 14：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (typeName.Contains("MoveProvider") || typeName.Contains("TurnProvider") ||
                // 步骤 15：调用 Contains：Contains。
                typeName.Contains("TeleportationProvider") || typeName.Contains("JumpProvider") ||
                // 步骤 16：调用 Contains：Contains。
                typeName.Contains("ClimbProvider") || typeName.Contains("GrabMoveProvider") ||
                // 步骤 17：调用 Contains：Contains。
                typeName.Contains("GravityProvider") || behaviour.gameObject.name == "Gravity")
                // 步骤 18：把 behaviour.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                behaviour.enabled = false;
        }
        // 步骤 19：从目标物体找到所需组件并存为 xrCamera；使用前应检查是否为 null。
        Camera xrCamera = rig.GetComponentInChildren<Camera>(true);
        // 步骤 20：检查xr相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("XR Origin has no camera.");
        if (xrCamera == null) throw new Exception("XR Origin has no camera.");
        // 步骤 21：显示这个场景对象及其子对象；界面会立即反映这一状态。
        xrCamera.gameObject.SetActive(true);
        // 步骤 22：把计算结果 "MainCamera" 保存为tag；下一条判断或调用会读取这个具体值。
        xrCamera.tag = "MainCamera";
        // 步骤 23：逐个读取 Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Inc 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
                     // 步骤 24：这一行补充 FindObjectsSortMode.None))，与相邻行合起来完成同一次计算或对象配置。
                     FindObjectsSortMode.None))
            // 步骤 25：检查条件 camera != xrCamera；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 DestroyImmediate：DestroyImmediate。
            if (camera != xrCamera) UnityEngine.Object.DestroyImmediate(camera);
        // 步骤 26：取目标对象的 Transform 并存为 head；Transform 提供位置、旋转和父子关系。
        Transform head = xrCamera.transform;
        // 步骤 27：计算 FindChild(rig.transform, "Left Controller")，把结果存进 left（left）；之后引用 left 时用的就是这次结果。
        Transform left = FindChild(rig.transform, "Left Controller");
        // 步骤 28：计算 FindChild(rig.transform, "Right Controller")，把结果存进 right（right）；之后引用 right 时用的就是这次结果。
        Transform right = FindChild(rig.transform, "Right Controller");
        // 步骤 29：检查left、right是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 throw new Exception("XR Origin has no controller transforms.");
        if (left == null || right == null) throw new Exception("XR Origin has no controller transforms.");
        // 步骤 30：调用 AttachGameplay：AttachGameplay。
        AttachGameplay(rig, head, left, right, oldTree.transform, false);

        // 步骤 31：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 32：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.SaveScene(scene, QuestScene);
        // 步骤 33：把计算结果 new[] 保存为scenes；下一条判断或调用会读取这个具体值。
        EditorBuildSettings.scenes = new[]
        {
            // 步骤 34：用 new 创建 EditorBuildSettingsScene(QuestScene, true),；构造参数决定新对象的初始内容。
            new EditorBuildSettingsScene(QuestScene, true),
            // 步骤 35：用 new 创建 EditorBuildSettingsScene(DesktopScene, false),；构造参数决定新对象的初始内容。
            new EditorBuildSettingsScene(DesktopScene, false),
            // 步骤 36：用 new 创建 EditorBuildSettingsScene(SourceScene, false)；构造参数决定新对象的初始内容。
            new EditorBuildSettingsScene(SourceScene, false)
        };
        // 步骤 37：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 38：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Sky Quest 3S scene created: " + QuestScene);
    }
    // ！！！功能结束：CreateQuestScene！！！

    // ！！！功能：PrepareEnvironment——PrepareEnvironment！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 out Vector3 spawn, out AncestorExchangeTree oldTree 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static Scene PrepareEnvironment(out Vector3 spawn, out AncestorExchangeTree oldTree)
    {
        // 步骤 1：计算 EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
        // 步骤 2：计算 GameObject.Find("C1_Player")，把结果存进 desktopPlayer（desktop玩家）；之后引用 desktopPlayer 时用的就是这次结果。
        GameObject desktopPlayer = GameObject.Find("C1_Player");
        // 步骤 3：新建用于spawn的对象或数值结构，并把构造参数保存进去。
        spawn = desktopPlayer != null ? desktopPlayer.transform.position : new Vector3(35f, 0f, 35f);
        // 步骤 4：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
        spawn.y = 0f;
        // 步骤 5：确认desktop玩家已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (desktopPlayer != null) desktopPlayer.SetActive(false);
        // 步骤 6：逐个读取 Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Inc 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
                     // 步骤 7：这一行补充 FindObjectsSortMode.None))，与相邻行合起来完成同一次计算或对象配置。
                     FindObjectsSortMode.None))
            // 步骤 8：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            camera.gameObject.SetActive(false);
        // 步骤 9：把计算结果 UnityEngine.Object.FindFirstObjectByType<AncestorExchangeTree>(FindObjectsInactive.Include) 保存为oldTree；下一条判断或调用会读取这个具体值。
        oldTree = UnityEngine.Object.FindFirstObjectByType<AncestorExchangeTree>(FindObjectsInactive.Include);
        // 步骤 10：检查oldTree是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("The source scene has no AncestorExchangeTree.");
        if (oldTree == null) throw new Exception("The source scene has no AncestorExchangeTree.");
        // 步骤 11：把 oldTree.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        oldTree.enabled = false;
        // 步骤 12：调用 ResizeAncestorForSanctuary：ResizeAncestor用于Sanctuary。
        ResizeAncestorForSanctuary(oldTree.transform, 1.6f);
        // 步骤 13：逐个读取 MaskInteraction oldMask in UnityEngine.Object.FindObjectsByType<MaskInteraction>( 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (MaskInteraction oldMask in UnityEngine.Object.FindObjectsByType<MaskInteraction>(
                     // 步骤 14：执行 FindObjectsInactive.Include, FindObjectsSortMode.None)) oldMask.enabled = false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                     FindObjectsInactive.Include, FindObjectsSortMode.None)) oldMask.enabled = false;
        // 步骤 15：逐个读取 MaskThrowController oldThrow in UnityEngine.Object.FindObjectsByType<MaskThrowControl 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (MaskThrowController oldThrow in UnityEngine.Object.FindObjectsByType<MaskThrowController>(
                     // 步骤 16：执行 FindObjectsInactive.Include, FindObjectsSortMode.None)) oldThrow.enabled = false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                     FindObjectsInactive.Include, FindObjectsSortMode.None)) oldThrow.enabled = false;
        // 步骤 17：把 scene 作为方法结果返回给调用方；return 后本方法立即结束。
        return scene;
    }
    // ！！！功能结束：PrepareEnvironment！！！

    // The imported ancestor is authored much larger than the surrounding Sky-style architecture.
    // Keep the feet at the original ground height while reducing the whole visible character.
    // ！！！功能：ResizeAncestorForSanctuary——ResizeAncestor用于Sanctuary！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, float desiredHeight 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ResizeAncestorForSanctuary(Transform root, float desiredHeight)
    {
        // 步骤 1：检查root是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (root == null) return;
        // 步骤 2：计算 VisibleBounds(root)，把结果存进 before（before）；之后引用 before 时用的就是这次结果。
        Bounds before = VisibleBounds(root);
        // 步骤 3：检查条件 before.size.y > 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (before.size.y > 0.01f)
            // 步骤 4：执行 root.localScale *= desiredHeight / before.size.y;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            root.localScale *= desiredHeight / before.size.y;
        // 步骤 5：计算 VisibleBounds(root)，把结果存进 after（after）；之后引用 after 时用的就是这次结果。
        Bounds after = VisibleBounds(root);
        // 步骤 6：这里用 && 要求两边条件同时成立：before.size.y > 0.01f && after.size.y > 0.01f；任意一边为假就跳过本分支。
        if (before.size.y > 0.01f && after.size.y > 0.01f)
            // 步骤 7：把position更新为 Vector3.up * (before.min.y - after.min.y)；世界或局部位置会立即影响物体摆放。
            root.position += Vector3.up * (before.min.y - after.min.y);
    }
    // ！！！功能结束：ResizeAncestorForSanctuary！！！

    // ！！！功能：VisibleBounds——VisibleBounds！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static Bounds VisibleBounds(Transform root)
    {
        // 步骤 1：从目标物体找到所需组件并存为 renderers；使用前应检查是否为 null。
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        // 步骤 2：用 found 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool found = false;
        // 步骤 3：创建新的 Bounds 并存为 bounds；构造参数决定其初始内容或对象名称。
        Bounds bounds = new Bounds(root.position, Vector3.zero);
        // 步骤 4：逐个读取 Renderer renderer in renderers 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in renderers)
        {
            // 步骤 5：检查renderer是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (renderer == null || !renderer.enabled) continue;
            // 步骤 6：这里的 ! 表示取反；当 found 不成立时才进入此分支。 成立时执行 { bounds = renderer.bounds; found = true; }
            if (!found) { bounds = renderer.bounds; found = true; }
            // 步骤 7：前面的 if 条件为假时执行这里的备用处理。
            else bounds.Encapsulate(renderer.bounds);
        }
        // 步骤 8：把 bounds 作为方法结果返回给调用方；return 后本方法立即结束。
        return bounds;
    }
    // ！！！功能结束：VisibleBounds！！！

    // 继续填写上一行调用的参数：private static void AttachGameplay(GameObject rig, Transform head, Transform left；参数位置决定它表示的坐标、速度或选项。
    private static void AttachGameplay(GameObject rig, Transform head, Transform left,
        // 这一行补充 Transform right, Transform ancestor, bool desktop)，与相邻行合起来完成同一次计算或对象配置。
        Transform right, Transform ancestor, bool desktop)
    {
        // 计算 BuildAvatar(rig.transform, desktop)，把结果存进 avatar（角色模型）；之后引用 avatar 时用的就是这次结果。
        GameObject avatar = BuildAvatar(rig.transform, desktop);
        // 计算 rig.AddComponent<SkyQuestStatus>()，把结果存进 status（status）；之后引用 status 时用的就是这次结果。
        SkyQuestStatus status = rig.AddComponent<SkyQuestStatus>();
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        status.head = head;
        // 把计算结果 desktop 保存为desktop模式；下一条判断或调用会读取这个具体值。
        status.desktopMode = desktop;
        // 计算 rig.AddComponent<SkyQuestLocomotion>()，把结果存进 locomotion（locomotion）；之后引用 locomotion 时用的就是这次结果。
        SkyQuestLocomotion locomotion = rig.AddComponent<SkyQuestLocomotion>();
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        locomotion.head = head;
        // 把计算结果 left 保存为左手柄位置；下一条判断或调用会读取这个具体值。
        locomotion.leftHand = left;
        // 把计算结果 right 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        locomotion.rightHand = right;
        // 把计算结果 avatar 保存为角色模型；下一条判断或调用会读取这个具体值。
        locomotion.avatar = avatar;
        // 把计算结果 desktop 保存为是否使用桌面键鼠控制；下一条判断或调用会读取这个具体值。
        locomotion.desktopControlled = desktop;
        // 把计算结果 desktop ? 4.5f : 2.8f 保存为第三人称相机距离；下一条判断或调用会读取这个具体值。
        locomotion.thirdPersonDistance = desktop ? 4.5f : 2.8f;
        // 这里的 ! 表示取反；当 desktop 不成立时才进入此分支。
        if (!desktop)
        {
            // 执行 rig.AddComponent<CapeEnergySystem>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            rig.AddComponent<CapeEnergySystem>();
            // 计算 rig.AddComponent<CapeFlightController>()，把结果存进 flight（flight）；之后引用 flight 时用的就是这次结果。
            CapeFlightController flight = rig.AddComponent<CapeFlightController>();
            // 把计算结果 AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>( 保存为actions；下一条判断或调用会读取这个具体值。
            flight.actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                // 执行 "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");
            // 执行 rig.AddComponent<CapeEnergyUI>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            rig.AddComponent<CapeEnergyUI>();
            // 执行 rig.AddComponent<CapeFlightFeedback>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            rig.AddComponent<CapeFlightFeedback>();
            // 执行 rig.AddComponent<CapeRechargeReceiver>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            rig.AddComponent<CapeRechargeReceiver>();
        }
        // 计算 rig.AddComponent<SkyQuestAncestor>()，把结果存进 tree（tree）；之后引用 tree 时用的就是这次结果。
        SkyQuestAncestor tree = rig.AddComponent<SkyQuestAncestor>();
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        tree.head = head;
        // 把计算结果 left 保存为左手柄位置；下一条判断或调用会读取这个具体值。
        tree.leftHand = left;
        // 把计算结果 right 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        tree.rightHand = right;
        // 把计算结果 ancestor 保存为ancestor；下一条判断或调用会读取这个具体值。
        tree.ancestor = ancestor;
        // 把计算结果 BuildAncestorMemorial(ancestor) 保存为memorialStone；下一条判断或调用会读取这个具体值。
        tree.memorialStone = BuildAncestorMemorial(ancestor);
        // 把计算结果 avatar.transform 保存为角色模型；下一条判断或调用会读取这个具体值。
        tree.avatar = avatar.transform;
        // 把计算结果 locomotion 保存为locomotion；下一条判断或调用会读取这个具体值。
        tree.locomotion = locomotion;
        // 把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        tree.status = status;
        // 把计算结果 desktop 保存为desktop模式；下一条判断或调用会读取这个具体值。
        tree.desktopMode = desktop;
        // 把计算结果 AssetDatabase.LoadAssetAtPath<GameObject>( 保存为mask预制体；下一条判断或调用会读取这个具体值。
        tree.maskPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            // 执行 "Assets/Project files/mask/deer mask ult sky cotl.obj");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Assets/Project files/mask/deer mask ult sky cotl.obj");
        // 计算 rig.AddComponent<SkyQuestMagicExchange>()，把结果存进 magic（magic）；之后引用 magic 时用的就是这次结果。
        SkyQuestMagicExchange magic = rig.AddComponent<SkyQuestMagicExchange>();
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        magic.head = head;
        // 把计算结果 left 保存为左手柄位置；下一条判断或调用会读取这个具体值。
        magic.leftHand = left;
        // 把计算结果 right 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        magic.rightHand = right;
        // 把计算结果 avatar.transform 保存为角色模型；下一条判断或调用会读取这个具体值。
        magic.avatar = avatar.transform;
        // 把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        magic.status = status;
        // 把计算结果 tree 保存为legacyTree；下一条判断或调用会读取这个具体值。
        magic.legacyTree = tree;
        // 把计算结果 desktop 保存为desktop模式；下一条判断或调用会读取这个具体值。
        magic.desktopMode = desktop;
        // 执行 magic.ancestors[0] = ancestor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        magic.ancestors[0] = ancestor;
        // 把计算结果 magic 保存为magicExchange；下一条判断或调用会读取这个具体值。
        tree.magicExchange = magic;
        // 计算 rig.AddComponent<SkyQuestSocial>()，把结果存进 social（social）；之后引用 social 时用的就是这次结果。
        SkyQuestSocial social = rig.AddComponent<SkyQuestSocial>();
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        social.head = head;
        // 把计算结果 left 保存为左手柄位置；下一条判断或调用会读取这个具体值。
        social.leftHand = left;
        // 把计算结果 right 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        social.rightHand = right;
        // 把计算结果 ancestor 保存为ancestor；下一条判断或调用会读取这个具体值。
        social.ancestor = ancestor;
        // 把计算结果 tree 保存为itemTree；下一条判断或调用会读取这个具体值。
        social.itemTree = tree;
        // 把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        social.status = status;
        // 把计算结果 desktop 保存为desktop模式；下一条判断或调用会读取这个具体值。
        social.desktopMode = desktop;
        // 计算 rig.AddComponent<SkyQuestProps>()，把结果存进 props（props）；之后引用 props 时用的就是这次结果。
        SkyQuestProps props = rig.AddComponent<SkyQuestProps>();
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        props.head = head;
        // 把计算结果 right 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        props.rightHand = right;
        // 把计算结果 tree 保存为itemTree；下一条判断或调用会读取这个具体值。
        props.itemTree = tree;
        // 把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        props.status = status;
        // 把计算结果 desktop 保存为desktop模式；下一条判断或调用会读取这个具体值。
        props.desktopMode = desktop;
        // 计算 rig.AddComponent<SkyQuestWorld>()，把结果存进 world（world）；之后引用 world 时用的就是这次结果。
        SkyQuestWorld world = rig.AddComponent<SkyQuestWorld>();
        // 把计算结果 locomotion 保存为locomotion；下一条判断或调用会读取这个具体值。
        world.locomotion = locomotion;
        // 把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        world.status = status;
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        world.head = head;
        // 把计算结果 ancestor 保存为ancestor；下一条判断或调用会读取这个具体值。
        world.ancestor = ancestor;
        // 计算 rig.AddComponent<SkyQuestCommunication>()，把结果存进 communication（communication）；之后引用 communication 时用的就是这次结果。
        SkyQuestCommunication communication = rig.AddComponent<SkyQuestCommunication>();
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        communication.head = head;
        // 把计算结果 right 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        communication.rightHand = right;
        // 把计算结果 tree 保存为itemTree；下一条判断或调用会读取这个具体值。
        communication.itemTree = tree;
        // 把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        communication.status = status;
        // 把计算结果 desktop 保存为desktop模式；下一条判断或调用会读取这个具体值。
        communication.desktopMode = desktop;
        // 把计算结果 communication 保存为communication；下一条判断或调用会读取这个具体值。
        props.communication = communication;
        // 计算 rig.AddComponent<SkyQuestImmersion>()，把结果存进 immersion（immersion）；之后引用 immersion 时用的就是这次结果。
        SkyQuestImmersion immersion = rig.AddComponent<SkyQuestImmersion>();
        // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        immersion.head = head;
        // 把计算结果 left 保存为左手柄位置；下一条判断或调用会读取这个具体值。
        immersion.leftHand = left;
        // 把计算结果 right 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        immersion.rightHand = right;
        // 把计算结果 locomotion 保存为locomotion；下一条判断或调用会读取这个具体值。
        immersion.locomotion = locomotion;
        // 把计算结果 UnityEngine.Object.FindFirstObjectByType<SkyQuestSanctuary>() 保存为sanctuary；下一条判断或调用会读取这个具体值。
        immersion.sanctuary = UnityEngine.Object.FindFirstObjectByType<SkyQuestSanctuary>();
        // 把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        immersion.status = status;
        // 把计算结果 desktop 保存为desktop模式；下一条判断或调用会读取这个具体值。
        immersion.desktopMode = desktop;
        // 检查条件 desktop；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktop)
        {
            // 计算 rig.AddComponent<SkyQuestDesktopControls>()，把结果存进 controls（controls）；之后引用 controls 时用的就是这次结果。
            SkyQuestDesktopControls controls = rig.AddComponent<SkyQuestDesktopControls>();
            // 把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
            controls.head = head;
            // 把计算结果 locomotion 保存为locomotion；下一条判断或调用会读取这个具体值。
            controls.locomotion = locomotion;
            // 把计算结果 tree 保存为ancestor；下一条判断或调用会读取这个具体值。
            controls.ancestor = tree;
            // 把计算结果 magic 保存为magicExchange；下一条判断或调用会读取这个具体值。
            controls.magicExchange = magic;
            // 把计算结果 social 保存为social；下一条判断或调用会读取这个具体值。
            controls.social = social;
            // 把计算结果 props 保存为props；下一条判断或调用会读取这个具体值。
            controls.props = props;
            // 把计算结果 communication 保存为communication；下一条判断或调用会读取这个具体值。
            controls.communication = communication;
            // 把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
            controls.status = status;
        }
    }

    // ！！！功能：CreateDesktopScene——创建可用键盘鼠标操作的 IP1 桌面场景！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void CreateDesktopScene()
    {
        // 步骤 1：计算 PrepareEnvironment(out Vector3 spawn, out AncestorExchangeTree oldTree)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = PrepareEnvironment(out Vector3 spawn, out AncestorExchangeTree oldTree);
        // 步骤 2：创建新的 GameObject 并存为 rig；构造参数决定其初始内容或对象名称。
        GameObject rig = new GameObject("Sky keyboard rig");
        // 步骤 3：执行 rig.transform.position = spawn;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rig.transform.position = spawn;
        // 步骤 4：创建新的 GameObject 并存为 headObject；构造参数决定其初始内容或对象名称。
        GameObject headObject = new GameObject("Desktop head camera");
        // 步骤 5：执行 headObject.transform.SetParent(rig.transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        headObject.transform.SetParent(rig.transform, false);
        // 步骤 6：执行 headObject.transform.localPosition = new Vector3(0f, 1.65f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        headObject.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        // 步骤 7：把计算结果 "MainCamera" 保存为tag；下一条判断或调用会读取这个具体值。
        headObject.tag = "MainCamera";
        // 步骤 8：计算 headObject.AddComponent<Camera>()，把结果存进 camera（camera）；之后引用 camera 时用的就是这次结果。
        Camera camera = headObject.AddComponent<Camera>();
        // 步骤 9：把计算结果 StereoTargetEyeMask.None 保存为stereoTargetEye；下一条判断或调用会读取这个具体值。
        camera.stereoTargetEye = StereoTargetEyeMask.None;
        // 步骤 10：执行 headObject.AddComponent<AudioListener>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        headObject.AddComponent<AudioListener>();
        // 步骤 11：创建新的 Transform 并存为 left；构造参数决定其初始内容或对象名称。
        Transform left = new GameObject("Desktop left hand").transform;
        // 步骤 12：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        left.SetParent(headObject.transform, false);
        // 步骤 13：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        left.localPosition = new Vector3(-0.28f, -0.24f, 0.4f);
        // 步骤 14：创建新的 Transform 并存为 right；构造参数决定其初始内容或对象名称。
        Transform right = new GameObject("Desktop right hand").transform;
        // 步骤 15：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        right.SetParent(headObject.transform, false);
        // 步骤 16：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        right.localPosition = new Vector3(0.28f, -0.24f, 0.4f);
        // 步骤 17：调用 AttachGameplay：AttachGameplay。
        AttachGameplay(rig, headObject.transform, left, right, oldTree.transform, true);

        // 步骤 18：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 19：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.SaveScene(scene, DesktopScene);
        // 步骤 20：把计算结果 new[] 保存为scenes；下一条判断或调用会读取这个具体值。
        EditorBuildSettings.scenes = new[]
        {
            // 步骤 21：用 new 创建 EditorBuildSettingsScene(QuestScene, true),；构造参数决定新对象的初始内容。
            new EditorBuildSettingsScene(QuestScene, true),
            // 步骤 22：用 new 创建 EditorBuildSettingsScene(DesktopScene, false),；构造参数决定新对象的初始内容。
            new EditorBuildSettingsScene(DesktopScene, false),
            // 步骤 23：用 new 创建 EditorBuildSettingsScene(SourceScene, false)；构造参数决定新对象的初始内容。
            new EditorBuildSettingsScene(SourceScene, false)
        };
        // 步骤 24：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 25：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Sky keyboard scene created: " + DesktopScene);
    }
    // ！！！功能结束：CreateDesktopScene！！！

    // ！！！功能：FindChild——查找Child！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root, string name 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Transform FindChild(Transform root, string name)
    {
        // 步骤 1：逐个读取 Transform child in root.GetComponentsInChildren<Transform>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            // 步骤 2：检查条件 child.name == name；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
            if (child.name == name) return child;
        // 步骤 3：没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }
    // ！！！功能结束：FindChild！！！

    // ！！！功能：BuildAvatar——构建Avatar！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform rig, bool desktop 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject BuildAvatar(Transform rig, bool desktop)
    {
        // 步骤 1：创建新的 GameObject 并存为 avatar；构造参数决定其初始内容或对象名称。
        GameObject avatar = new GameObject("Sky avatar");
        // 步骤 2：执行 avatar.transform.SetParent(rig, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        avatar.transform.SetParent(rig, false);
        // 步骤 3：执行 avatar.transform.localPosition = desktop ? Vector3.zero : new Vector3(0f, 0f, 2.8f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        avatar.transform.localPosition = desktop ? Vector3.zero : new Vector3(0f, 0f, 2.8f);
        // 步骤 4：计算 AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project files/COTL_rigged.fbx")，把结果存进 modelAsset（model资源）；之后引用 modelAsset 时用的就是这次结果。
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project files/COTL_rigged.fbx");
        // 步骤 5：确认model资源已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (modelAsset != null)
        {
            // 步骤 6：计算 (GameObject)PrefabUtility.InstantiatePrefab(modelAsset)，把结果存进 model（model）；之后引用 model 时用的就是这次结果。
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            // 步骤 7：执行 model.transform.SetParent(avatar.transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            model.transform.SetParent(avatar.transform, false);
            // 步骤 8：调用 UnpackPrefabInstance：Unpack预制体Instance。
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely,
                // 步骤 9：执行 InteractionMode.AutomatedAction);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                InteractionMode.AutomatedAction);
            // 步骤 10：逐个读取 Camera embeddedCamera in model.GetComponentsInChildren<Camera>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Camera embeddedCamera in model.GetComponentsInChildren<Camera>(true))
                // 步骤 11：在编辑器里立即移除旧对象；重新搭建时避免上次生成内容重复叠加。
                UnityEngine.Object.DestroyImmediate(embeddedCamera.gameObject);
            // 步骤 12：执行 model.transform.localPosition = Vector3.zero;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            model.transform.localPosition = Vector3.zero;
            // 步骤 13：执行 model.transform.localRotation = Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            model.transform.localRotation = Quaternion.identity;
            // 步骤 14：执行 model.transform.localScale = Vector3.one;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            model.transform.localScale = Vector3.one;
            // 步骤 15：从目标物体找到所需组件并存为 renderers；使用前应检查是否为 null。
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            // 步骤 16：检查条件 renderers.Length > 0；只有为真才运行下面的步骤，否则沿原流程继续。
            if (renderers.Length > 0)
            {
                // 步骤 17：计算 renderers[0].bounds，把结果存进 bounds（bounds）；之后引用 bounds 时用的就是这次结果。
                Bounds bounds = renderers[0].bounds;
                // 步骤 18：逐个读取 Renderer renderer in renderers 中的元素；每轮处理当前一个，直到全部遍历完。
                foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
                // 步骤 19：检查条件 bounds.size.y > 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。
                if (bounds.size.y > 0.01f)
                {
                    // 步骤 20：计算 1.6f / bounds.size.y，把结果存进 scale（scale）；之后引用 scale 时用的就是这次结果。
                    float scale = 1.6f / bounds.size.y;
                    // 步骤 21：执行 model.transform.localScale = Vector3.one * scale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    model.transform.localScale = Vector3.one * scale;
                    // 步骤 22：继续填写上一行调用的参数：model.transform.position -= new Vector3(bounds.center.x - avatar.transform.position.x；参数位置决定它表示的坐标、速度或选项。
                    model.transform.position -= new Vector3(bounds.center.x - avatar.transform.position.x,
                        // 步骤 23：执行 bounds.min.y - avatar.transform.position.y, bounds.center.z - avatar.transform.position.z) * scale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                        bounds.min.y - avatar.transform.position.y, bounds.center.z - avatar.transform.position.z) * scale;
                }
            }
            // 步骤 24：逐个读取 Collider collider in model.GetComponentsInChildren<Collider>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
        }
        // 步骤 25：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 26：计算 GameObject.CreatePrimitive(PrimitiveType.Capsule)，把结果存进 body（body）；之后引用 body 时用的就是这次结果。
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            // 步骤 27：把计算结果 "Sky avatar fallback" 保存为name；下一条判断或调用会读取这个具体值。
            body.name = "Sky avatar fallback";
            // 步骤 28：执行 body.transform.SetParent(avatar.transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            body.transform.SetParent(avatar.transform, false);
            // 步骤 29：执行 body.transform.localPosition = new Vector3(0f, 0.8f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            // 步骤 30：执行 body.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            body.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
            // 步骤 31：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());
        }
        // 步骤 32：把 avatar 作为方法结果返回给调用方；return 后本方法立即结束。
        return avatar;
    }
    // ！！！功能结束：BuildAvatar！！！

    // ！！！功能：ValidateQuestScene——验证Quest场景！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void ValidateQuestScene()
    {
        // 步骤 1：计算 EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Single);
        // 步骤 2：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>()，把结果存进 locomotion（locomotion）；之后引用 locomotion 时用的就是这次结果。
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        // 步骤 3：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>()，把结果存进 tree（tree）；之后引用 tree 时用的就是这次结果。
        SkyQuestAncestor tree = UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>();
        // 步骤 4：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestSocial>()，把结果存进 social（social）；之后引用 social 时用的就是这次结果。
        SkyQuestSocial social = UnityEngine.Object.FindFirstObjectByType<SkyQuestSocial>();
        // 步骤 5：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestCommunication>()，把结果存进 communication（communication）；之后引用 communication 时用的就是这次结果。
        SkyQuestCommunication communication = UnityEngine.Object.FindFirstObjectByType<SkyQuestCommunication>();
        // 步骤 6：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestProps>()，把结果存进 props（props）；之后引用 props 时用的就是这次结果。
        SkyQuestProps props = UnityEngine.Object.FindFirstObjectByType<SkyQuestProps>();
        // 步骤 7：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestWorld>()，把结果存进 world（world）；之后引用 world 时用的就是这次结果。
        SkyQuestWorld world = UnityEngine.Object.FindFirstObjectByType<SkyQuestWorld>();
        // 步骤 8：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>()，把结果存进 magic（magic）；之后引用 magic 时用的就是这次结果。
        SkyQuestMagicExchange magic = UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        // 步骤 9：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!scene.IsValid() || locomotion == null || tree == null || tree.memorialStone == null || social == null ||
            // 步骤 10：把计算结果 = null || props == null || world == null || magic == null) 保存为communication；下一条判断或调用会读取这个具体值。
            communication == null || props == null || world == null || magic == null)
            // 步骤 11：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Quest scene is missing one or more core systems.");
        // 步骤 12：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (locomotion.head == null || locomotion.leftHand == null || locomotion.rightHand == null ||
            // 步骤 13：把计算结果 = null || tree.ancestor == null || tree.avatar == null || 保存为角色模型；下一条判断或调用会读取这个具体值。
            locomotion.avatar == null || tree.ancestor == null || tree.avatar == null ||
            // 步骤 14：把计算结果 = null || props.rightHand == null || props.communication == null || world.head == null || 保存为头显相机；下一条判断或调用会读取这个具体值。
            communication.head == null || props.rightHand == null || props.communication == null || world.head == null ||
            // 步骤 15：这一行补充 magic.legacyTree != tree)，与相邻行合起来完成同一次计算或对象配置。
            magic.legacyTree != tree)
            // 步骤 16：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Quest scene has a missing device or gameplay reference.");
        // 步骤 17：调用 ValidateGallery：验证Gallery。
        ValidateGallery(magic);
        // 步骤 18：调用 ValidateMemorial：验证Memorial。
        ValidateMemorial(tree);
        // 步骤 19：把数值 0 暂存为 activeCameras；它只在当前方法里参与后续判断或计算。
        int activeCameras = 0;
        // 步骤 20：计算 ""，把结果存进 activeCameraNames（active相机Names）；之后引用 activeCameraNames 时用的就是这次结果。
        string activeCameraNames = "";
        // 步骤 21：逐个读取 Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Inc 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
                     // 步骤 22：这一行补充 FindObjectsSortMode.None))，与相邻行合起来完成同一次计算或对象配置。
                     FindObjectsSortMode.None))
            // 步骤 23：这里用 && 要求两边条件同时成立：camera.gameObject.activeInHierarchy && camera.enabled；任意一边为假就跳过本分支。
            if (camera.gameObject.activeInHierarchy && camera.enabled)
            {
                // 步骤 24：执行 activeCameras++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                activeCameras++;
                // 步骤 25：在active相机Names原值上增加 camera.name + "[" + camera.transform.root.name + "; " +；旧状态与本次输入共同决定新状态。
                activeCameraNames += camera.name + "[" + camera.transform.root.name + "; " +
                    // 步骤 26：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
                    AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(camera)) + "] ";
            }
        // 步骤 27：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (activeCameras != 1) throw new Exception("Quest scene should have exactly one active camera, found " +
            // 步骤 28：执行 activeCameras + ": " + activeCameraNames);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            activeCameras + ": " + activeCameraNames);
        // 步骤 29：计算 XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android)，把结果存进 general（general）；之后引用 general 时用的就是这次结果。
        XRGeneralSettings general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        // 步骤 30：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (general == null || general.Manager == null || !general.Manager.automaticLoading ||
            // 步骤 31：这一行补充 !general.Manager.automaticRunning)，与相邻行合起来完成同一次计算或对象配置。
            !general.Manager.automaticRunning)
            // 步骤 32：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("OpenXR is not configured to start automatically on Android.");
        // 步骤 33：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Sky Quest 3S scene validation passed.");
    }
    // ！！！功能结束：ValidateQuestScene！！！

    // ！！！功能：ValidateDesktopScene——验证桌面场景！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void ValidateDesktopScene()
    {
        // 步骤 1：计算 EditorSceneManager.OpenScene(DesktopScene, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(DesktopScene, OpenSceneMode.Single);
        // 步骤 2：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestDesktopControls>()，把结果存进 controls（controls）；之后引用 controls 时用的就是这次结果。
        SkyQuestDesktopControls controls = UnityEngine.Object.FindFirstObjectByType<SkyQuestDesktopControls>();
        // 步骤 3：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>()，把结果存进 locomotion（locomotion）；之后引用 locomotion 时用的就是这次结果。
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        // 步骤 4：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>()，把结果存进 ancestor（ancestor）；之后引用 ancestor 时用的就是这次结果。
        SkyQuestAncestor ancestor = UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>();
        // 步骤 5：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestSocial>()，把结果存进 social（social）；之后引用 social 时用的就是这次结果。
        SkyQuestSocial social = UnityEngine.Object.FindFirstObjectByType<SkyQuestSocial>();
        // 步骤 6：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestProps>()，把结果存进 props（props）；之后引用 props 时用的就是这次结果。
        SkyQuestProps props = UnityEngine.Object.FindFirstObjectByType<SkyQuestProps>();
        // 步骤 7：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestCommunication>()，把结果存进 communication（communication）；之后引用 communication 时用的就是这次结果。
        SkyQuestCommunication communication = UnityEngine.Object.FindFirstObjectByType<SkyQuestCommunication>();
        // 步骤 8：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>()，把结果存进 magic（magic）；之后引用 magic 时用的就是这次结果。
        SkyQuestMagicExchange magic = UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        // 步骤 9：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!scene.IsValid() || controls == null || locomotion == null || ancestor == null ||
            // 步骤 10：把计算结果 = null || 保存为memorialStone；下一条判断或调用会读取这个具体值。
            ancestor.memorialStone == null ||
            // 步骤 11：把计算结果 = null || props == null || communication == null || magic == null || 保存为social；下一条判断或调用会读取这个具体值。
            social == null || props == null || communication == null || magic == null ||
            // 步骤 12：这一行补充 !locomotion.desktopControlled || !ancestor.desktopMode || !social.desktopMode ||，与相邻行合起来完成同一次计算或对象配置。
            !locomotion.desktopControlled || !ancestor.desktopMode || !social.desktopMode ||
            // 步骤 13：这一行补充 !props.desktopMode || !communication.desktopMode || controls.head == null ||，与相邻行合起来完成同一次计算或对象配置。
            !props.desktopMode || !communication.desktopMode || controls.head == null ||
            // 步骤 14：这一行补充 controls.locomotion != locomotion || controls.ancestor != ancestor ||，与相邻行合起来完成同一次计算或对象配置。
            controls.locomotion != locomotion || controls.ancestor != ancestor ||
            // 步骤 15：这一行补充 controls.social != social || controls.props != props ||，与相邻行合起来完成同一次计算或对象配置。
            controls.social != social || controls.props != props ||
            // 步骤 16：这一行补充 controls.communication != communication || controls.magicExchange != magic ||，与相邻行合起来完成同一次计算或对象配置。
            controls.communication != communication || controls.magicExchange != magic ||
            // 步骤 17：这一行补充 !magic.desktopMode || magic.legacyTree != ancestor)，与相邻行合起来完成同一次计算或对象配置。
            !magic.desktopMode || magic.legacyTree != ancestor)
            // 步骤 18：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Desktop scene has missing or incorrect keyboard controls.");
        // 步骤 19：调用 ValidateGallery：验证Gallery。
        ValidateGallery(magic);
        // 步骤 20：调用 ValidateMemorial：验证Memorial。
        ValidateMemorial(ancestor);
        // 步骤 21：把数值 0 暂存为 cameras；它只在当前方法里参与后续判断或计算。
        int cameras = 0;
        // 步骤 22：逐个读取 Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Inc 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
                     // 步骤 23：这一行补充 FindObjectsSortMode.None))，与相邻行合起来完成同一次计算或对象配置。
                     FindObjectsSortMode.None))
            // 步骤 24：这里用 && 要求两边条件同时成立：camera.enabled && camera.gameObject.activeInHierarchy；任意一边为假就跳过本分支。 成立时执行 cameras++;
            if (camera.enabled && camera.gameObject.activeInHierarchy) cameras++;
        // 步骤 25：检查条件 cameras != 1；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 throw new Exception("Desktop scene needs one active camera, found " + camer
        if (cameras != 1) throw new Exception("Desktop scene needs one active camera, found " + cameras);
        // 步骤 26：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Sky keyboard scene validation passed.");
    }
    // ！！！功能结束：ValidateDesktopScene！！！

    // ！！！功能：ValidateGallery——验证Gallery！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 SkyQuestMagicExchange magic 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ValidateGallery(SkyQuestMagicExchange magic)
    {
        // 步骤 1：把计算结果 { "Shibiyama dancer", "Sky prayer guardian", "COTL traveller", 保存为names；下一条判断或调用会读取这个具体值。
        string[] names = { "Shibiyama dancer", "Sky prayer guardian", "COTL traveller",
            // 步骤 2：继续填写上一行调用的参数："Candle pilgrim", "Skeleton pilgrim - grounded gallery character"；参数位置决定它表示的坐标、速度或选项。
            "Candle pilgrim", "Skeleton pilgrim - grounded gallery character",
            // 步骤 3：执行 "Original C1 player - visible gallery character" };；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Original C1 player - visible gallery character" };
        // 步骤 4：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (magic.ancestors == null || magic.memorials == null ||
            // 步骤 5：这一行补充 magic.ancestors.Length != names.Length || magic.memorials.Length != names.Length)，与相邻行合起来完成同一次计算或对象配置。
            magic.ancestors.Length != names.Length || magic.memorials.Length != names.Length)
            // 步骤 6：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Gallery ancestor or memorial list is incomplete.");
        // 步骤 7：按 int i = 0; i < names.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < names.Length; i++)
        {
            // 步骤 8：检查对象引用是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
            if (magic.ancestors[i] == null || magic.ancestors[i].name != names[i] || magic.memorials[i] == null)
                // 步骤 9：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                throw new Exception("Gallery ancestor missing: " + names[i]);
            // 步骤 10：调用 ValidateMemorial：验证Memorial。
            ValidateMemorial(magic.ancestors[i], magic.memorials[i]);
        }
    }
    // ！！！功能结束：ValidateGallery！！！

    // ！！！功能：BuildQuestApk——配置 Android/Quest 构建目标并输出可安装的 APK！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void BuildQuestApk()
    {
        // 步骤 1：调用 ConfigureQuestProject：配置QuestProject。
        ConfigureQuestProject();
        // 步骤 2：这里的 ! 表示取反；当 EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android) 不成立时才进入此分支。
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            // 步骤 3：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Unity could not switch to Android build target.");
        // 步骤 4：把计算结果 System.IO.Path.Combine( 保存为outputDirectory；下一条判断或调用会读取这个具体值。
        string outputDirectory = System.IO.Path.Combine(
            // 步骤 5：调用 GetFolderPath：获取文件夹路径。
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SkyQuest3S-Builds");
        // 步骤 6：确认目标文件夹存在；不存在时创建它，后续写入场景、材质或资源文件才不会失败。
        System.IO.Directory.CreateDirectory(outputDirectory);
        // 步骤 7：计算 System.IO.Path.Combine(outputDirectory, "SkyQuest3S.apk")，把结果存进 output（output）；之后引用 output 时用的就是这次结果。
        string output = System.IO.Path.Combine(outputDirectory, "SkyQuest3S.apk");
        // 步骤 8：新建用于options的对象或数值结构，并把构造参数保存进去。
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            // 步骤 9：把计算结果 new[] { QuestScene }, 保存为scenes；下一条判断或调用会读取这个具体值。
            scenes = new[] { QuestScene },
            // 步骤 10：把计算结果 output, 保存为location路径Name；下一条判断或调用会读取这个具体值。
            locationPathName = output,
            // 步骤 11：把计算结果 BuildTarget.Android, 保存为target；下一条判断或调用会读取这个具体值。
            target = BuildTarget.Android,
            // 步骤 12：把计算结果 BuildOptions.None 保存为options；下一条判断或调用会读取这个具体值。
            options = BuildOptions.None
        };
        // 步骤 13：计算 BuildPipeline.BuildPlayer(options)，把结果存进 report（report）；之后引用 report 时用的就是这次结果。
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
        // 步骤 14：检查条件 report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded；只有为真才运行下面的步骤，否则沿原流程继续。
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            // 步骤 15：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Quest APK build failed: " + report.summary.result +
                // 步骤 16：执行 " (" + report.summary.totalErrors + " errors)");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                " (" + report.summary.totalErrors + " errors)");
        // 步骤 17：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Quest APK built: " + output);
    }
    // ！！！功能结束：BuildQuestApk！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Build Quest 3S APK")]
    // ！！！功能：BuildQuestApkMenu——构建QuestApkMenu！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void BuildQuestApkMenu()
    {
        // 步骤 1：调用 BuildQuestApk：配置 Android/Quest 构建目标并输出可安装的 APK。
        BuildQuestApk();
    }
    // ！！！功能结束：BuildQuestApkMenu！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Validate gameplay references")]
    // ！！！功能：ValidateGameplayReferences——验证GameplayReferences！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void ValidateGameplayReferences()
    {
        // 步骤 1：调用 ValidateQuestScene：验证Quest场景。
        ValidateQuestScene();
        // 步骤 2：调用 ValidateDesktopScene：验证桌面场景。
        ValidateDesktopScene();
        // 步骤 3：调用 ValidateBoth：验证Both。
        SkyQuestEnvironmentBuilder.ValidateBoth();
        // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("SKY gameplay references and environment validation passed.");
    }
    // ！！！功能结束：ValidateGameplayReferences！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Build keyboard desktop demo")]
    // ！！！功能：BuildDesktopDemo——构建桌面Demo！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void BuildDesktopDemo()
    {
        // 步骤 1：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
                // 步骤 2：这一行补充 BuildTarget.StandaloneOSX))，与相邻行合起来完成同一次计算或对象配置。
                BuildTarget.StandaloneOSX))
            // 步骤 3：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Unity could not switch to the desktop build target.");
        // 步骤 4：计算 "Build/SkyDesktopDemo.app"，把结果存进 output（output）；之后引用 output 时用的就是这次结果。
        string output = "Build/SkyDesktopDemo.app";
        // 步骤 5：确认目标文件夹存在；不存在时创建它，后续写入场景、材质或资源文件才不会失败。
        System.IO.Directory.CreateDirectory("Build");
        // 步骤 6：新建用于options的对象或数值结构，并把构造参数保存进去。
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            // 步骤 7：把计算结果 new[] { DesktopScene }, 保存为scenes；下一条判断或调用会读取这个具体值。
            scenes = new[] { DesktopScene },
            // 步骤 8：把计算结果 output, 保存为location路径Name；下一条判断或调用会读取这个具体值。
            locationPathName = output,
            // 步骤 9：把计算结果 BuildTarget.StandaloneOSX, 保存为target；下一条判断或调用会读取这个具体值。
            target = BuildTarget.StandaloneOSX,
            // 步骤 10：把计算结果 BuildOptions.None 保存为options；下一条判断或调用会读取这个具体值。
            options = BuildOptions.None
        };
        // 步骤 11：计算 BuildPipeline.BuildPlayer(options)，把结果存进 report（report）；之后引用 report 时用的就是这次结果。
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
        // 步骤 12：检查条件 report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded；只有为真才运行下面的步骤，否则沿原流程继续。
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            // 步骤 13：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Desktop demo build failed: " + report.summary.result);
        // 步骤 14：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Desktop demo built: " + output);
    }
    // ！！！功能结束：BuildDesktopDemo！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Configure Quest 3S project")]
    // ！！！功能：ConfigureQuestProject——配置QuestProject！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void ConfigureQuestProject()
    {
        // 步骤 1：定义固定文字 settingsPath（settings路径）；统一引用这段名称或路径，避免各处拼写不一致。
        const string settingsPath = "Assets/XRI/Settings/SkyQuestXRSettings.asset";
        // 步骤 2：这一行补充 XRGeneralSettingsPerBuildTarget buildSettings =，与相邻行合起来完成同一次计算或对象配置。
        XRGeneralSettingsPerBuildTarget buildSettings =
            // 步骤 3：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(settingsPath);
        // 步骤 4：检查buildSettings是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (buildSettings == null)
        {
            // 步骤 5：把计算结果 ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>() 保存为buildSettings；下一条判断或调用会读取这个具体值。
            buildSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            // 步骤 6：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateAsset(buildSettings, settingsPath);
        }
        // 步骤 7：调用 AddConfigObject：添加Config物品。
        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildSettings, true);
        // 步骤 8：这里的 ! 表示取反；当 buildSettings.HasSettingsForBuildTarget(BuildTargetGroup.Android) 不成立时才进入此分支。
        if (!buildSettings.HasSettingsForBuildTarget(BuildTargetGroup.Android))
            // 步骤 9：调用 CreateDefaultSettingsForBuildTarget：创建DefaultSettings用于构建Target。
            buildSettings.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
        // 步骤 10：这里的 ! 表示取反；当 buildSettings.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android) 不成立时才进入此分支。
        if (!buildSettings.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
            // 步骤 11：调用 CreateDefaultManagerSettingsForBuildTarget：创建DefaultManagerSettings用于构建Target。
            buildSettings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        // 步骤 12：计算 buildSettings.SettingsForBuildTarget(BuildTargetGroup.Android)，把结果存进 general（general）；之后引用 general 时用的就是这次结果。
        XRGeneralSettings general = buildSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
        // 步骤 13：把 general.InitManagerOnStart 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        general.InitManagerOnStart = true;
        // 步骤 14：执行 general.Manager.automaticLoading = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        general.Manager.automaticLoading = true;
        // 步骤 15：执行 general.Manager.automaticRunning = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        general.Manager.automaticRunning = true;
        // 步骤 16：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!XRPackageMetadataStore.AssignLoader(general.Manager, typeof(OpenXRLoader).FullName,
                // 步骤 17：这一行补充 BuildTargetGroup.Android))，与相邻行合起来完成同一次计算或对象配置。
                BuildTargetGroup.Android))
            // 步骤 18：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Could not assign the OpenXR Android loader.");
        // 步骤 19：计算 OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android)，把结果存进 openXR（openXR）；之后引用 openXR 时用的就是这次结果。
        OpenXRSettings openXR = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        // 步骤 20：确认openXR已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (openXR != null)
        {
            // 步骤 21：把计算结果 OpenXRSettings.RenderMode.SinglePassInstanced 保存为render模式；下一条判断或调用会读取这个具体值。
            openXR.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
            // 步骤 22：计算 openXR.GetFeature<OculusTouchControllerProfile>()，把结果存进 touch（touch）；之后引用 touch 时用的就是这次结果。
            OculusTouchControllerProfile touch = openXR.GetFeature<OculusTouchControllerProfile>();
            // 步骤 23：确认touch已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { touch.enabled = true; EditorUtility.SetDirty(touch); }
            if (touch != null) { touch.enabled = true; EditorUtility.SetDirty(touch); }
            // 步骤 24：这一行补充 MetaQuestTouchPlusControllerProfile touchPlus =，与相邻行合起来完成同一次计算或对象配置。
            MetaQuestTouchPlusControllerProfile touchPlus =
                // 步骤 25：执行 openXR.GetFeature<MetaQuestTouchPlusControllerProfile>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                openXR.GetFeature<MetaQuestTouchPlusControllerProfile>();
            // 步骤 26：确认touchPlus已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { touchPlus.enabled = true; EditorUtility.SetDirty(touchPlus); }
            if (touchPlus != null) { touchPlus.enabled = true; EditorUtility.SetDirty(touchPlus); }
            // 步骤 27：计算 openXR.GetFeature<MetaQuestFeature>()，把结果存进 meta（meta）；之后引用 meta 时用的就是这次结果。
            MetaQuestFeature meta = openXR.GetFeature<MetaQuestFeature>();
            // 步骤 28：确认meta已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { meta.enabled = true; EditorUtility.SetDirty(meta); }
            if (meta != null) { meta.enabled = true; EditorUtility.SetDirty(meta); }
            // 步骤 29：计算 openXR.GetFeature<OpenXRCompositionLayersFeature>()，把结果存进 layers（layers）；之后引用 layers 时用的就是这次结果。
            OpenXRCompositionLayersFeature layers = openXR.GetFeature<OpenXRCompositionLayersFeature>();
            // 步骤 30：确认layers已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { layers.enabled = true; EditorUtility.SetDirty(layers); }
            if (layers != null) { layers.enabled = true; EditorUtility.SetDirty(layers); }
            // 步骤 31：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(openXR);
        }
        // 步骤 32：调用 SetScriptingBackend：设置ScriptingBackend。
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        // 步骤 33：执行 PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        // 步骤 34：执行 PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        // 步骤 35：执行 PlayerSettings.Android.optimizedFramePacing = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        PlayerSettings.Android.optimizedFramePacing = true;
        // 步骤 36：把计算结果 MobileTextureSubtarget.ASTC 保存为android构建Subtarget；下一条判断或调用会读取这个具体值。
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        // 步骤 37：执行 PlayerSettings.Android.forceInternetPermission = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        PlayerSettings.Android.forceInternetPermission = true;
        // 步骤 38：调用 SetUseDefaultGraphicsAPIs：设置UseDefaultGraphicsAPIs。
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        // 步骤 39：调用 SetGraphicsAPIs：设置GraphicsAPIs。
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        // 步骤 40：调用 SetApplicationIdentifier：设置ApplicationIdentifier。
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.jialingzhu.skyquestxr");
        // 步骤 41：把计算结果 "Sky Quest XR" 保存为productName；下一条判断或调用会读取这个具体值。
        PlayerSettings.productName = "Sky Quest XR";
        // 步骤 42：把计算结果 AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>( 保存为mobilePipeline；下一条判断或调用会读取这个具体值。
        UniversalRenderPipelineAsset mobilePipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
            // 步骤 43：执行 "Assets/Settings/Mobile_RPAsset.asset");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Assets/Settings/Mobile_RPAsset.asset");
        // 步骤 44：确认mobilePipeline已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (mobilePipeline != null)
        {
            // 步骤 45：把计算结果 4 保存为msaaSampleCount；下一条判断或调用会读取这个具体值。
            mobilePipeline.msaaSampleCount = 4;
            // 步骤 46：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(mobilePipeline);
        }
        // 步骤 47：调用 ConfigureExistingQuestRig：配置ExistingQuestRig。
        ConfigureExistingQuestRig();
        // 步骤 48：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(buildSettings);
        // 步骤 49：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(general);
        // 步骤 50：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
    }
    // ！！！功能结束：ConfigureQuestProject！！！

    // ！！！功能：ConfigureExistingQuestRig——配置ExistingQuestRig！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ConfigureExistingQuestRig()
    {
        // 步骤 1：计算 SceneManager.GetSceneByPath(QuestScene)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = SceneManager.GetSceneByPath(QuestScene);
        // 步骤 2：用 openedHere 记住此处真假结果（!scene.isLoaded），供下一条 if 选择执行路径。
        bool openedHere = !scene.isLoaded;
        // 步骤 3：检查条件 openedHere；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 scene = EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Additive);
        if (openedHere) scene = EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Additive);
        // 步骤 4：这一行补充 try，与相邻行合起来完成同一次计算或对象配置。
        try
        {
            // 步骤 5：逐个读取 GameObject root in scene.GetRootGameObjects() 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (GameObject root in scene.GetRootGameObjects())
            // 步骤 6：逐个读取 XROrigin origin in root.GetComponentsInChildren<XROrigin>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (XROrigin origin in root.GetComponentsInChildren<XROrigin>(true))
            {
                // 步骤 7：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
                if (origin.gameObject.name != "Sky Quest 3S XR Origin" ||
                    // 步骤 8：把计算结果 = XROrigin.TrackingOriginMode.Floor) continue 保存为RequestedTracking原点模式；下一条判断或调用会读取这个具体值。
                    origin.RequestedTrackingOriginMode == XROrigin.TrackingOriginMode.Floor) continue;
                // 步骤 9：把计算结果 XROrigin.TrackingOriginMode.Floor 保存为RequestedTracking原点模式；下一条判断或调用会读取这个具体值。
                origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                // 步骤 10：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
                EditorUtility.SetDirty(origin);
                // 步骤 11：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
                EditorSceneManager.MarkSceneDirty(scene);
            }
            // 步骤 12：检查条件 scene.isDirty；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SaveScene：保存场景。
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }
        // 步骤 13：这一行补充 finally，与相邻行合起来完成同一次计算或对象配置。
        finally
        {
            // 步骤 14：检查条件 openedHere；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 CloseScene：Close场景。
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }
    // ！！！功能结束：ConfigureExistingQuestRig！！！
}
