// ！！！文件功能：在编辑器升级建筑网格和贴图细节，同时保留游戏碰撞逻辑！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：此文件位于 Assets/Editor，仅用于 Unity 编辑器的搭建与验证，玩家安装的 APK 不执行这里的方法。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 UnityEditor 命名空间；下方可直接使用仅在 Unity 编辑器内使用的资源和菜单接口。
using UnityEditor;
// 引入 UnityEditor.SceneManagement 命名空间；下方可直接使用编辑器内创建、打开、保存场景的接口。
using UnityEditor.SceneManagement;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.SceneManagement 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.SceneManagement;

// Improves the authored sanctuary without rebuilding it or changing gameplay colliders.
// 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
[InitializeOnLoad]
// 定义 SkyQuestVisualDetailUpgrade 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestVisualDetailUpgrade
{
    // 定义固定文字 ScenePath（场景路径）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string ScenePath = "Assets/SkyQuest/SkyQuest3S.unity";
    // 定义固定文字 MarkerName（MarkerName）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string MarkerName = "SkyQuest visual detail v2";
    // 定义固定文字 MeshFolder（网格文件夹）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string MeshFolder = "Assets/SkyQuest/Environment/Meshes";
    // 定义固定文字 Source（Source）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string Source = "Assets/Project files/";
    // 定义固定文字 Materials（材质）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string Materials = "Assets/SkyQuest/Environment/Materials/";
    // 声明 attempted（attempted）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static bool attempted;

    // ！！！功能：SkyQuestVisualDetailUpgrade——SkyQuestVisualDetailUpgrade！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    static SkyQuestVisualDetailUpgrade()
    {
        // 步骤 1：在update原值上增加 UpgradeWhenReady；旧状态与本次输入共同决定新状态。
        EditorApplication.update += UpgradeWhenReady;
    }
    // ！！！功能结束：SkyQuestVisualDetailUpgrade！！！

    // ！！！功能：UpgradeWhenReady——UpgradeWhenReady！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void UpgradeWhenReady()
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：attempted || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling；用于合并几种停止或跳过原因。
        if (attempted || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;
        // 步骤 3：检查条件 SceneManager.GetActiveScene().path != ScenePath；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (SceneManager.GetActiveScene().path != ScenePath) return;
        // 步骤 4：把 attempted 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        attempted = true;
        // 步骤 5：在update原值上减少 UpgradeWhenReady；旧状态与本次输入共同决定新状态。
        EditorApplication.update -= UpgradeWhenReady;
        // 步骤 6：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (GameObject.Find(MarkerName) != null &&
            // 步骤 7：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.LoadAssetAtPath<Mesh>(MeshFolder + "/BeveledArchitecture.asset") != null)
            // 步骤 8：提前结束这个方法；不再执行下方语句。
            return;
        // 步骤 9：调用 UpgradeOpenScene：UpgradeOpen场景。
        UpgradeOpenScene();
    }
    // ！！！功能结束：UpgradeWhenReady！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Improve visual detail in open scene")]
    // ！！！功能：UpgradeOpenScene——UpgradeOpen场景！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void UpgradeOpenScene()
    {
        // 步骤 1：检查条件 EditorApplication.isPlayingOrWillChangePlaymode；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        // 步骤 2：计算 SceneManager.GetActiveScene()，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = SceneManager.GetActiveScene();
        // 步骤 3：这里用 && 要求两边条件同时成立：scene.path != ScenePath && scene.path != "Assets/SkyQuest/SkyDesktop.unity"；任意一边为假就跳过本分支。 条件成立时立即退出当前方法。
        if (scene.path != ScenePath && scene.path != "Assets/SkyQuest/SkyDesktop.unity") return;
        // 步骤 4：用 alreadyUpgraded 记住此处真假结果（GameObject.Find(MarkerName) != null），供下一条 if 选择执行路径。
        bool alreadyUpgraded = GameObject.Find(MarkerName) != null;

        // 步骤 5：这里的 ! 表示取反；当 AssetDatabase.IsValidFolder(MeshFolder) 不成立时才进入此分支。
        if (!AssetDatabase.IsValidFolder(MeshFolder))
            // 步骤 6：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateFolder("Assets/SkyQuest/Environment", "Meshes");
        // 步骤 7：计算 LoadOrCreateMesh("SmoothCylinder96", CreateCylinder(false))，把结果存进 smooth（smooth）；之后引用 smooth 时用的就是这次结果。
        Mesh smooth = LoadOrCreateMesh("SmoothCylinder96", CreateCylinder(false));
        // 步骤 8：计算 LoadOrCreateMesh("FlutedColumn96", CreateCylinder(true))，把结果存进 fluted（fluted）；之后引用 fluted 时用的就是这次结果。
        Mesh fluted = LoadOrCreateMesh("FlutedColumn96", CreateCylinder(true));
        // 步骤 9：计算 LoadOrCreateMesh("SmoothSphere96", CreateSphere())，把结果存进 sphere（sphere）；之后引用 sphere 时用的就是这次结果。
        Mesh sphere = LoadOrCreateMesh("SmoothSphere96", CreateSphere());
        // 步骤 10：计算 LoadOrCreateMesh("BeveledArchitecture", CreateBeveledCube())，把结果存进 bevel（bevel）；之后引用 bevel 时用的就是这次结果。
        Mesh bevel = LoadOrCreateMesh("BeveledArchitecture", CreateBeveledCube());
        // 步骤 11：检查条件 alreadyUpgraded；只有为真才运行下面的步骤，否则沿原流程继续。
        if (alreadyUpgraded)
        {
            // 步骤 12：计算 UpgradeArchitecture(scene, bevel)，把结果存进 blocks（blocks）；之后引用 blocks 时用的就是这次结果。
            int blocks = UpgradeArchitecture(scene, bevel);
            // 步骤 13：检查条件 blocks > 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SaveScene：保存场景。
            if (blocks > 0) EditorSceneManager.SaveScene(scene);
            // 步骤 14：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.SaveAssets();
            // 步骤 15：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log($"SkyQuest visual detail refreshed; {blocks} architectural blocks refined.");
            // 步骤 16：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 17：计算 0, spheres = 0，把结果存进 cylinders（cylinders）；之后引用 cylinders 时用的就是这次结果。
        int cylinders = 0, spheres = 0;
        // 步骤 18：逐个读取 GameObject root in scene.GetRootGameObjects() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (GameObject root in scene.GetRootGameObjects())
        // 步骤 19：逐个读取 MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            // 步骤 20：计算 filter.sharedMesh，把结果存进 old（old）；之后引用 old 时用的就是这次结果。
            Mesh old = filter.sharedMesh;
            // 步骤 21：检查old是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (old == null) continue;
            // 步骤 22：从目标物体找到所需组件并存为 size；使用前应检查是否为 null。
            Vector3 size = filter.GetComponent<Renderer>()?.bounds.size ?? Vector3.zero;
            // 步骤 23：这里用 && 要求两边条件同时成立：old.name == "Cylinder" && (size.y > 1.2f || Mathf.Max(size.x, size.z) > 0.45f)；任意一边为假就跳过本分支。
            if (old.name == "Cylinder" && (size.y > 1.2f || Mathf.Max(size.x, size.z) > 0.45f))
            {
                // 步骤 24：把计算结果 size.y > 1.2f ? fluted : smooth 保存为shared网格；下一条判断或调用会读取这个具体值。
                filter.sharedMesh = size.y > 1.2f ? fluted : smooth;
                // 步骤 25：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
                EditorUtility.SetDirty(filter);
                // 步骤 26：执行 cylinders++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                cylinders++;
            }
            // 步骤 27：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里用 && 要求两边条件同时成立：old.name == "Sphere" && Mathf.Max(size.x, size.y, size.z) > 0.5f；任意一边为假就跳过本分支。
            else if (old.name == "Sphere" && Mathf.Max(size.x, size.y, size.z) > 0.5f)
            {
                // 步骤 28：把计算结果 sphere 保存为shared网格；下一条判断或调用会读取这个具体值。
                filter.sharedMesh = sphere;
                // 步骤 29：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
                EditorUtility.SetDirty(filter);
                // 步骤 30：执行 spheres++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                spheres++;
            }
        }

        // 步骤 31：计算 UpgradeArchitecture(scene, bevel)，把结果存进 architecture（architecture）；之后引用 architecture 时用的就是这次结果。
        int architecture = UpgradeArchitecture(scene, bevel);

        // 步骤 32：调用 UpgradeMaterials：Upgrade材质。
        UpgradeMaterials();
        // 步骤 33：创建新的 var 并存为 marker；构造参数决定其初始内容或对象名称。
        var marker = new GameObject(MarkerName);
        // 步骤 34：调用 MoveGameObjectToScene：MoveGame物品到场景。
        SceneManager.MoveGameObjectToScene(marker, scene);
        // 步骤 35：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 36：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.SaveScene(scene);
        // 步骤 37：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 38：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log($"SkyQuest visual detail upgraded: {cylinders} columns/discs, {spheres} large orbs, {architecture} architectural blocks; existing 8K source maps imported with Quest texture limits.");
    }
    // ！！！功能结束：UpgradeOpenScene！！！

    // ！！！功能：UpgradeArchitecture——UpgradeArchitecture！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Scene scene, Mesh bevel 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 int 类型的计算结果；执行到 return 时把该值交回调用者。
    private static int UpgradeArchitecture(Scene scene, Mesh bevel)
    {
        // 步骤 1：把数值 0 暂存为 count；它只在当前方法里参与后续判断或计算。
        int count = 0;
        // 步骤 2：逐个读取 GameObject root in scene.GetRootGameObjects() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            // 步骤 3：这里用 || 表示任一条件成立就处理：root.name.Contains("player") || root.name.Contains("Origin") || root.name.Contains("Ancestor")；用于合并几种停止或跳过原因。
            if (root.name.Contains("player") || root.name.Contains("Origin") || root.name.Contains("Ancestor"))
                // 步骤 4：忽略当前无效元素，立刻转到下一轮循环。
                continue;
            // 步骤 5：逐个读取 MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                // 步骤 6：检查filter的shared网格是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
                if (filter.sharedMesh == null || filter.sharedMesh.name != "Cube") continue;
                // 步骤 7：从目标物体找到所需组件并存为 renderer；使用前应检查是否为 null。
                Renderer renderer = filter.GetComponent<Renderer>();
                // 步骤 8：检查renderer是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
                if (renderer == null) continue;
                // 步骤 9：计算 renderer.bounds.size，把结果存进 size（size）；之后引用 size 时用的就是这次结果。
                Vector3 size = renderer.bounds.size;
                // 步骤 10：检查条件 Mathf.Max(size.x, size.y, size.z) < 1.5f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
                if (Mathf.Max(size.x, size.y, size.z) < 1.5f) continue;
                // 步骤 11：把计算结果 bevel 保存为shared网格；下一条判断或调用会读取这个具体值。
                filter.sharedMesh = bevel;
                // 步骤 12：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
                EditorUtility.SetDirty(filter);
                // 步骤 13：执行 count++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                count++;
            }
        }
        // 步骤 14：检查条件 count > 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 MarkSceneDirty：Mark场景Dirty。
        if (count > 0) EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 15：把 count 作为方法结果返回给调用方；return 后本方法立即结束。
        return count;
    }
    // ！！！功能结束：UpgradeArchitecture！！！

    // ！！！功能：LoadOrCreateMesh——加载Or创建网格！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Mesh mesh 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Mesh 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Mesh LoadOrCreateMesh(string name, Mesh mesh)
    {
        // 步骤 1：把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        mesh.name = name;
        // 步骤 2：计算 MeshFolder + "/" + name + ".asset"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
        string path = MeshFolder + "/" + name + ".asset";
        // 步骤 3：计算 AssetDatabase.LoadAssetAtPath<Mesh>(path)，把结果存进 existing（existing）；之后引用 existing 时用的就是这次结果。
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        // 步骤 4：确认existing已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (existing != null)
        {
            // 步骤 5：调用 CopySerialized：CopySerialized。
            EditorUtility.CopySerialized(mesh, existing);
            // 步骤 6：在编辑器里立即移除旧对象；重新搭建时避免上次生成内容重复叠加。
            UnityEngine.Object.DestroyImmediate(mesh);
            // 步骤 7：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(existing);
            // 步骤 8：把 existing 作为方法结果返回给调用方；return 后本方法立即结束。
            return existing;
        }
        // 步骤 9：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.CreateAsset(mesh, path);
        // 步骤 10：把 mesh 作为方法结果返回给调用方；return 后本方法立即结束。
        return mesh;
    }
    // ！！！功能结束：LoadOrCreateMesh！！！

    // ！！！功能：CreateBeveledCube——创建BeveledCube！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Mesh 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Mesh CreateBeveledCube()
    {
        // Six smooth chamfered faces: 108 triangles versus the primitive cube's 12.
        // 步骤 1：计算 { -0.5f, -0.46f, 0.46f, 0.5f }，把结果存进 edge（edge）；之后引用 edge 时用的就是这次结果。
        float[] edge = { -0.5f, -0.46f, 0.46f, 0.5f };
        // 步骤 2：计算 { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down }，把结果存进 outward（outward）；之后引用 outward 时用的就是这次结果。
        Vector3[] outward = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down };
        // 步骤 3：计算 { Vector3.right, Vector3.left, Vector3.back, Vector3.forward, Vector3.right, Vector3.right，把结果存进 right（right）；之后引用 right 时用的就是这次结果。
        Vector3[] right = { Vector3.right, Vector3.left, Vector3.back, Vector3.forward, Vector3.right, Vector3.right };
        // 步骤 4：计算 { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.back, Vector3.forward }，把结果存进 up（up）；之后引用 up 时用的就是这次结果。
        Vector3[] up = { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.back, Vector3.forward };
        // 步骤 5：创建新的 var 并存为 vertices；构造参数决定其初始内容或对象名称。
        var vertices = new List<Vector3>();
        // 步骤 6：创建新的 var 并存为 normals；构造参数决定其初始内容或对象名称。
        var normals = new List<Vector3>();
        // 步骤 7：创建新的 var 并存为 uv；构造参数决定其初始内容或对象名称。
        var uv = new List<Vector2>();
        // 步骤 8：创建新的 var 并存为 triangles；构造参数决定其初始内容或对象名称。
        var triangles = new List<int>();
        // 步骤 9：按 int face = 0; face < 6; face++ 变化索引；循环体依次处理每个位置。
        for (int face = 0; face < 6; face++)
        {
            // 步骤 10：计算 vertices.Count，把结果存进 offset（offset）；之后引用 offset 时用的就是这次结果。
            int offset = vertices.Count;
            // 步骤 11：按 int row = 0; row < 4; row++ 变化索引；循环体依次处理每个位置。
            for (int row = 0; row < 4; row++)
            // 步骤 12：按 int col = 0; col < 4; col++ 变化索引；循环体依次处理每个位置。
            for (int col = 0; col < 4; col++)
            {
                // 步骤 13：计算 outward[face] * 0.5f + right[face] * edge[col] + up[face] * edge[row]，把结果存进 point（point）；之后引用 point 时用的就是这次结果。
                Vector3 point = outward[face] * 0.5f + right[face] * edge[col] + up[face] * edge[row];
                // 步骤 14：计算core并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
                Vector3 core = new Vector3(Mathf.Clamp(point.x, -0.46f, 0.46f),
                    // 步骤 15：调用 Clamp：Clamp。
                    Mathf.Clamp(point.y, -0.46f, 0.46f), Mathf.Clamp(point.z, -0.46f, 0.46f));
                // 步骤 16：计算 (point - core).normalized，把结果存进 normal（normal）；之后引用 normal 时用的就是这次结果。
                Vector3 normal = (point - core).normalized;
                // 步骤 17：把括号里的新元素加入 vertices 集合；后续会按集合顺序使用它。
                vertices.Add(core + normal * 0.04f);
                // 步骤 18：把括号里的新元素加入 normals 集合；后续会按集合顺序使用它。
                normals.Add(normal);
                // 步骤 19：把括号里的新元素加入 uv 集合；后续会按集合顺序使用它。
                uv.Add(new Vector2(edge[col] + 0.5f, edge[row] + 0.5f));
                // 步骤 20：这里用 || 表示任一条件成立就处理：row == 0 || col == 0；用于合并几种停止或跳过原因。 成立时执行 continue;
                if (row == 0 || col == 0) continue;
                // 步骤 21：计算 offset + (row - 1) * 4 + col - 1，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
                int a = offset + (row - 1) * 4 + col - 1;
                // 步骤 22：计算 a + 1, c = a + 4, d = c + 1，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
                int b = a + 1, c = a + 4, d = c + 1;
                // 步骤 23：把括号里的新元素加入 triangles 集合；后续会按集合顺序使用它。
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                // 步骤 24：把括号里的新元素加入 triangles 集合；后续会按集合顺序使用它。
                triangles.Add(b); triangles.Add(d); triangles.Add(c);
            }
        }
        // 步骤 25：创建新的 var 并存为 mesh；构造参数决定其初始内容或对象名称。
        var mesh = new Mesh();
        // 步骤 26：调用 SetVertices：设置Vertices。
        mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv);
        // 步骤 27：调用 SetTriangles：设置Triangles。
        mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
        // 步骤 28：把 mesh 作为方法结果返回给调用方；return 后本方法立即结束。
        return mesh;
    }
    // ！！！功能结束：CreateBeveledCube！！！

    // ！！！功能：CreateCylinder——创建Cylinder！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool flute 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Mesh 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Mesh CreateCylinder(bool flute)
    {
        // 步骤 1：定义不可在运行时改变的数值 sides（sides）；后续计算统一使用这个阈值、速度或尺寸。
        const int sides = 96, rings = 5;
        // 步骤 2：创建新的 var 并存为 vertices；构造参数决定其初始内容或对象名称。
        var vertices = new List<Vector3>();
        // 步骤 3：创建新的 var 并存为 uv；构造参数决定其初始内容或对象名称。
        var uv = new List<Vector2>();
        // 步骤 4：创建新的 var 并存为 triangles；构造参数决定其初始内容或对象名称。
        var triangles = new List<int>();
        // 步骤 5：按 int ring = 0; ring <= rings; ring++ 变化索引；循环体依次处理每个位置。
        for (int ring = 0; ring <= rings; ring++)
        // 步骤 6：按 int i = 0; i <= sides; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i <= sides; i++)
        {
            // 步骤 7：计算 i / (float)sides, angle = t * Mathf.PI * 2f，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = i / (float)sides, angle = t * Mathf.PI * 2f;
            // 步骤 8：用三元表达式为 r 二选一：问号前条件为真取前值，为假取冒号后的值。
            float r = 0.5f * (flute ? 1f + 0.024f * Mathf.Cos(angle * 16f) : 1f);
            // 步骤 9：把括号里的新元素加入 vertices 集合；后续会按集合顺序使用它。
            vertices.Add(new Vector3(Mathf.Cos(angle) * r, -1f + 2f * ring / rings, Mathf.Sin(angle) * r));
            // 步骤 10：把括号里的新元素加入 uv 集合；后续会按集合顺序使用它。
            uv.Add(new Vector2(t, ring / (float)rings));
            // 步骤 11：这里用 || 表示任一条件成立就处理：ring == 0 || i == 0；用于合并几种停止或跳过原因。 成立时执行 continue;
            if (ring == 0 || i == 0) continue;
            // 步骤 12：计算 (ring - 1) * (sides + 1) + i，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            int a = (ring - 1) * (sides + 1) + i;
            // 步骤 13：计算 a + sides + 1，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
            int b = a + sides + 1;
            // 步骤 14：把括号里的新元素加入 triangles 集合；后续会按集合顺序使用它。
            triangles.Add(a - 1); triangles.Add(b - 1); triangles.Add(a);
            // 步骤 15：把括号里的新元素加入 triangles 集合；后续会按集合顺序使用它。
            triangles.Add(a); triangles.Add(b - 1); triangles.Add(b);
        }
        // 步骤 16：按 int cap = 0; cap < 2; cap++ 变化索引；循环体依次处理每个位置。
        for (int cap = 0; cap < 2; cap++)
        {
            // 步骤 17：用三元表达式为 y 二选一：问号前条件为真取前值，为假取冒号后的值。
            float y = cap == 0 ? -1f : 1f;
            // 步骤 18：计算 vertices.Count，把结果存进 center（center）；之后引用 center 时用的就是这次结果。
            int center = vertices.Count;
            // 步骤 19：把括号里的新元素加入 vertices 集合；后续会按集合顺序使用它。
            vertices.Add(new Vector3(0f, y, 0f)); uv.Add(new Vector2(0.5f, 0.5f));
            // 步骤 20：按 int i = 0; i <= sides; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i <= sides; i++)
            {
                // 步骤 21：计算 i * Mathf.PI * 2f / sides，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
                float angle = i * Mathf.PI * 2f / sides;
                // 步骤 22：用三元表达式为 r 二选一：问号前条件为真取前值，为假取冒号后的值。
                float r = 0.5f * (flute ? 1f + 0.024f * Mathf.Cos(angle * 16f) : 1f);
                // 步骤 23：把括号里的新元素加入 vertices 集合；后续会按集合顺序使用它。
                vertices.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                // 步骤 24：把括号里的新元素加入 uv 集合；后续会按集合顺序使用它。
                uv.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
                // 步骤 25：检查条件 i == 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
                if (i == 0) continue;
                // 步骤 26：把括号里的新元素加入 triangles 集合；后续会按集合顺序使用它。
                triangles.Add(center); triangles.Add(center + i + (cap == 0 ? 0 : 1));
                // 步骤 27：把括号里的新元素加入 triangles 集合；后续会按集合顺序使用它。
                triangles.Add(center + i + (cap == 0 ? 1 : 0));
            }
        }
        // 步骤 28：创建新的 var 并存为 mesh；构造参数决定其初始内容或对象名称。
        var mesh = new Mesh();
        // 步骤 29：调用 SetVertices：设置Vertices。
        mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
        // 步骤 30：调用 RecalculateNormals：RecalculateNormals。
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        // 步骤 31：把 mesh 作为方法结果返回给调用方；return 后本方法立即结束。
        return mesh;
    }
    // ！！！功能结束：CreateCylinder！！！

    // ！！！功能：CreateSphere——创建Sphere！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Mesh 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Mesh CreateSphere()
    {
        // 步骤 1：定义不可在运行时改变的数值 longitude（longitude）；后续计算统一使用这个阈值、速度或尺寸。
        const int longitude = 96, latitude = 48;
        // 步骤 2：创建新的 var 并存为 vertices；构造参数决定其初始内容或对象名称。
        var vertices = new List<Vector3>();
        // 步骤 3：创建新的 var 并存为 uv；构造参数决定其初始内容或对象名称。
        var uv = new List<Vector2>();
        // 步骤 4：创建新的 var 并存为 triangles；构造参数决定其初始内容或对象名称。
        var triangles = new List<int>();
        // 步骤 5：按 int y = 0; y <= latitude; y++ 变化索引；循环体依次处理每个位置。
        for (int y = 0; y <= latitude; y++)
        // 步骤 6：按 int x = 0; x <= longitude; x++ 变化索引；循环体依次处理每个位置。
        for (int x = 0; x <= longitude; x++)
        {
            // 步骤 7：计算 x / (float)longitude, v = y / (float)latitude，把结果存进 u（u）；之后引用 u 时用的就是这次结果。
            float u = x / (float)longitude, v = y / (float)latitude;
            // 步骤 8：计算 u * Mathf.PI * 2f, b = v * Mathf.PI，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = u * Mathf.PI * 2f, b = v * Mathf.PI;
            // 步骤 9：把括号里的新元素加入 vertices 集合；后续会按集合顺序使用它。
            vertices.Add(new Vector3(Mathf.Sin(b) * Mathf.Cos(a), Mathf.Cos(b),
                // 步骤 10：调用 Sin：Sin。
                Mathf.Sin(b) * Mathf.Sin(a)) * 0.5f);
            // 步骤 11：把括号里的新元素加入 uv 集合；后续会按集合顺序使用它。
            uv.Add(new Vector2(u, v));
            // 步骤 12：这里用 || 表示任一条件成立就处理：x == 0 || y == 0；用于合并几种停止或跳过原因。 成立时执行 continue;
            if (x == 0 || y == 0) continue;
            // 步骤 13：计算 y * (longitude + 1) + x，把结果存进 current（current）；之后引用 current 时用的就是这次结果。
            int current = y * (longitude + 1) + x;
            // 步骤 14：计算 current - (longitude + 1)，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
            int previous = current - (longitude + 1);
            // 步骤 15：把括号里的新元素加入 triangles 集合；后续会按集合顺序使用它。
            triangles.Add(previous - 1); triangles.Add(previous); triangles.Add(current - 1);
            // 步骤 16：把括号里的新元素加入 triangles 集合；后续会按集合顺序使用它。
            triangles.Add(previous); triangles.Add(current); triangles.Add(current - 1);
        }
        // 步骤 17：创建新的 var 并存为 mesh；构造参数决定其初始内容或对象名称。
        var mesh = new Mesh();
        // 步骤 18：调用 SetVertices：设置Vertices。
        mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
        // 步骤 19：调用 RecalculateNormals：RecalculateNormals。
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        // 步骤 20：把 mesh 作为方法结果返回给调用方；return 后本方法立即结束。
        return mesh;
    }
    // ！！！功能结束：CreateSphere！！！

    // ！！！功能：UpgradeMaterials——Upgrade材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void UpgradeMaterials()
    {
        // 步骤 1：计算 Source + "material/wood/Wood050_8K-JPG_"，把结果存进 wood（wood）；之后引用 wood 时用的就是这次结果。
        string wood = Source + "material/wood/Wood050_8K-JPG_";
        // 步骤 2：计算 Source + "material/metal/Metal047A_8K-JPG_"，把结果存进 metal（metal）；之后引用 metal 时用的就是这次结果。
        string metal = Source + "material/metal/Metal047A_8K-JPG_";
        // 步骤 3：计算 Source + "land/Ground110_8K-JPG_"，把结果存进 ground（ground）；之后引用 ground 时用的就是这次结果。
        string ground = Source + "land/Ground110_8K-JPG_";
        // 步骤 4：调用 AssignMaps：AssignMaps。
        AssignMaps("HoneyWood", wood + "Color.jpg", wood + "NormalGL.jpg", 0.27f);
        // 步骤 5：调用 AssignMaps：AssignMaps。
        AssignMaps("PaleGold", null, metal + "NormalGL.jpg", 0.18f);
        // 步骤 6：调用 AssignMaps：AssignMaps。
        AssignMaps("DeepGold", null, metal + "NormalGL.jpg", 0.24f);
        // 步骤 7：计算 ImportMap(ground + "Color.jpg", false)，把结果存进 gravelColor（gravelColor）；之后引用 gravelColor 时用的就是这次结果。
        Texture2D gravelColor = ImportMap(ground + "Color.jpg", false);
        // 步骤 8：计算 ImportMap(ground + "NormalGL.jpg", true)，把结果存进 gravelNormal（gravelNormal）；之后引用 gravelNormal 时用的就是这次结果。
        Texture2D gravelNormal = ImportMap(ground + "NormalGL.jpg", true);
        // 步骤 9：计算 AssetDatabase.LoadAssetAtPath<Material>(Materials + "GardenGravel.mat")，把结果存进 gravel（gravel）；之后引用 gravel 时用的就是这次结果。
        Material gravel = AssetDatabase.LoadAssetAtPath<Material>(Materials + "GardenGravel.mat");
        // 步骤 10：检查gravel是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (gravel == null)
        {
            // 步骤 11：新建用于gravel的对象或数值结构，并把构造参数保存进去。
            gravel = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            // 步骤 12：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateAsset(gravel, Materials + "GardenGravel.mat");
        }
        // 步骤 13：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
        gravel.SetColor("_BaseColor", new Color(0.76f, 0.82f, 0.82f));
        // 步骤 14：调用 SetTexture：设置贴图。
        gravel.SetTexture("_BaseMap", gravelColor);
        // 步骤 15：调用 SetTexture：设置贴图。
        gravel.SetTexture("_BumpMap", gravelNormal);
        // 步骤 16：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
        gravel.SetFloat("_BumpScale", 0.2f);
        // 步骤 17：调用 EnableKeyword：EnableKeyword。
        gravel.EnableKeyword("_NORMALMAP");
        // 步骤 18：调用 SetTextureScale：设置贴图缩放。
        gravel.SetTextureScale("_BaseMap", new Vector2(6f, 6f));
        // 步骤 19：调用 SetTextureScale：设置贴图缩放。
        gravel.SetTextureScale("_BumpMap", new Vector2(6f, 6f));
        // 步骤 20：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(gravel);
        // 步骤 21：计算 GameObject.Find("Garden limestone base")，把结果存进 garden（garden）；之后引用 garden 时用的就是这次结果。
        GameObject garden = GameObject.Find("Garden limestone base");
        // 步骤 22：确认garden已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (garden != null)
        {
            // 步骤 23：从目标物体找到所需组件并存为 renderer；使用前应检查是否为 null。
            var renderer = garden.GetComponent<Renderer>();
            // 步骤 24：确认renderer已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { renderer.sharedMaterial = gravel; EditorUtility.SetDirty(renderer); }
            if (renderer != null) { renderer.sharedMaterial = gravel; EditorUtility.SetDirty(renderer); }
        }
    }
    // ！！！功能结束：UpgradeMaterials！！！

    // ！！！功能：AssignMaps——AssignMaps！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, string colorPath, string normalPath, float normalStrength 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void AssignMaps(string name, string colorPath, string normalPath, float normalStrength)
    {
        // 步骤 1：计算 AssetDatabase.LoadAssetAtPath<Material>(Materials + name + ".mat")，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
        Material material = AssetDatabase.LoadAssetAtPath<Material>(Materials + name + ".mat");
        // 步骤 2：检查material是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (material == null) return;
        // 步骤 3：确认color路径已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetTexture：设置贴图。
        if (colorPath != null) material.SetTexture("_BaseMap", ImportMap(colorPath, false));
        // 步骤 4：调用 SetTexture：设置贴图。
        material.SetTexture("_BumpMap", ImportMap(normalPath, true));
        // 步骤 5：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
        material.SetFloat("_BumpScale", normalStrength);
        // 步骤 6：调用 EnableKeyword：EnableKeyword。
        material.EnableKeyword("_NORMALMAP");
        // 步骤 7：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(material);
    }
    // ！！！功能结束：AssignMaps！！！

    // ！！！功能：ImportMap——ImportMap！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string path, bool normal 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Texture2D 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Texture2D ImportMap(string path, bool normal)
    {
        // 步骤 1：计算 AssetImporter.GetAtPath(path) as TextureImporter，把结果存进 importer（importer）；之后引用 importer 时用的就是这次结果。
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        // 步骤 2：检查importer是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("SkyQuest texture missing: " + path);
        if (importer == null) throw new Exception("SkyQuest texture missing: " + path);
        // 步骤 3：用 changed 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool changed = false;
        // 步骤 4：用三元表达式为 type 二选一：问号前条件为真取前值，为假取冒号后的值。
        TextureImporterType type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        // 步骤 5：检查条件 importer.textureType != type；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { importer.textureType = type; changed = true; }
        if (importer.textureType != type) { importer.textureType = type; changed = true; }
        // 步骤 6：检查条件 importer.maxTextureSize != 4096；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { importer.maxTextureSize = 4096; changed = true; }
        if (importer.maxTextureSize != 4096) { importer.maxTextureSize = 4096; changed = true; }
        // 步骤 7：计算 importer.GetPlatformTextureSettings("Android")，把结果存进 android（android）；之后引用 android 时用的就是这次结果。
        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
        // 步骤 8：这里用 || 表示任一条件成立就处理：!android.overridden || android.maxTextureSize != 1024；用于合并几种停止或跳过原因。
        if (!android.overridden || android.maxTextureSize != 1024)
        {
            // 步骤 9：把计算结果 "Android"; android.overridden = true; android.maxTextureSize = 1024 保存为name；下一条判断或调用会读取这个具体值。
            android.name = "Android"; android.overridden = true; android.maxTextureSize = 1024;
            // 步骤 10：调用 SetPlatformTextureSettings：设置Platform贴图Settings。
            importer.SetPlatformTextureSettings(android); changed = true;
        }
        // 步骤 11：检查条件 changed；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SaveAndReimport：保存和Reimport。
        if (changed) importer.SaveAndReimport();
        // 步骤 12：把 AssetDatabase.LoadAssetAtPath<Texture2D>(path) 作为方法结果返回给调用方；return 后本方法立即结束。
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    // ！！！功能结束：ImportMap！！！
}
