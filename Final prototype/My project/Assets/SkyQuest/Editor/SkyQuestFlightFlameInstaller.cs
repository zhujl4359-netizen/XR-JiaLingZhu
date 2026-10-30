// ！！！文件功能：在桌面和 Quest 场景里安装相同的飞行火焰路线！！！
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
// 引入 UnityEngine.SceneManagement 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.SceneManagement;

// Keeps the same hand-built course in the desktop and Quest versions of the sanctuary.
// 定义 SkyQuestFlightFlameInstaller 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestFlightFlameInstaller
{
    // 定义固定文字 DesktopScene（桌面场景）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string DesktopScene = "Assets/SkyQuest/SkyDesktop.unity";
    // 定义固定文字 QuestScene（Quest场景）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string QuestScene = "Assets/SkyQuest/SkyQuest3S.unity";
    // 定义固定文字 MaterialFolder（材质文件夹）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string MaterialFolder = "Assets/SkyQuest/FlightMaterials";
    // 定义固定文字 FlameMeshPath（Flame网格路径）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string FlameMeshPath = MaterialFolder + "/Sky flight flame.asset";
    // 定义固定文字 HaloMeshPath（Halo网格路径）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string HaloMeshPath = MaterialFolder + "/Sky flight halo.asset";
    // 这一行补充 private static readonly Vector3[] FlamePositions =，与相邻行合起来完成同一次计算或对象配置。
    private static readonly Vector3[] FlamePositions =
    {
        // 用 new 创建 Vector3(43f, 3.4f, 44f), new Vector3(47f, 3.35f, 47f),；构造参数决定新对象的初始内容。
        new Vector3(43f, 2.55f, 44f), new Vector3(47f, 3.35f, 47f),
        // 用 new 创建 Vector3(52f, 5.0f, 49f), new Vector3(57f, 4.75f, 46f),；构造参数决定新对象的初始内容。
        new Vector3(52f, 4.15f, 49f), new Vector3(57f, 4.75f, 46f),
        // 用 new 创建 Vector3(62f, 6.1f, 43f), new Vector3(67f, 5.55f, 47f),；构造参数决定新对象的初始内容。
        new Vector3(62f, 5.25f, 43f), new Vector3(67f, 5.55f, 47f),
        // 用 new 创建 Vector3(64f, 7.0f, 53f), new Vector3(58f, 5.65f, 56f),；构造参数决定新对象的初始内容。
        new Vector3(64f, 6.15f, 53f), new Vector3(58f, 5.65f, 56f),
        // 用 new 创建 Vector3(52f, 5.8f, 59f), new Vector3(46f, 4.35f, 57f),；构造参数决定新对象的初始内容。
        new Vector3(52f, 4.95f, 59f), new Vector3(46f, 4.35f, 57f),
        // 用 new 创建 Vector3(41f, 4.6f, 53f), new Vector3(39f, 2.85f, 47f)；构造参数决定新对象的初始内容。
        new Vector3(41.25f, 4.2f, 52.75f), new Vector3(39f, 2.85f, 47f)
    };

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Install flying flame game in both scenes")]
    // ！！！功能：Install——Install！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void Install()
    {
        // 步骤 1：计算 SceneManager.GetActiveScene().path，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        string previous = SceneManager.GetActiveScene().path;
        // 步骤 2：检查条件 SceneManager.GetActiveScene().isDirty；只有为真才运行下面的步骤，否则沿原流程继续。
        if (SceneManager.GetActiveScene().isDirty)
            // 步骤 3：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
            EditorSceneManager.SaveOpenScenes();
        // 步骤 4：调用 EnsureAssets：确保资源。
        EnsureAssets(out Mesh flameMesh, out Material outer, out Material inner,
            // 步骤 5：执行 out Material spark, out Material start);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            out Material spark, out Material start);
        // 步骤 6：计算 HaloMesh()，把结果存进 haloMesh（halo网格）；之后引用 haloMesh 时用的就是这次结果。
        Mesh haloMesh = HaloMesh();
        // 步骤 7：调用 InstallInScene：Install在场景。
        InstallInScene(DesktopScene, flameMesh, haloMesh, outer, inner, spark, start);
        // 步骤 8：调用 InstallInScene：Install在场景。
        InstallInScene(QuestScene, flameMesh, haloMesh, outer, inner, spark, start);
        // 步骤 9：这里的 ! 表示取反；当 string.IsNullOrEmpty(previous) 不成立时才进入此分支。 成立时调用 OpenScene：Open场景。
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
        // 步骤 10：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 11：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Flying flame game installed in desktop and Quest sanctuary scenes.");
    }
    // ！！！功能结束：Install！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Polish flying flames in both scenes")]
    // ！！！功能：PolishExistingFlames——PolishExistingFlames！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void PolishExistingFlames()
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty；用于合并几种停止或跳过原因。
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)
        {
            // 步骤 2：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError("Stop Play mode and save the current scene before polishing the flying flames.");
            // 步骤 3：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 4：计算 SceneManager.GetActiveScene().path，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        string previous = SceneManager.GetActiveScene().path;
        // 步骤 5：调用 EnsureAssets：确保资源。
        EnsureAssets(out Mesh flameMesh, out Material outer, out Material inner,
            // 步骤 6：执行 out Material spark, out _);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            out Material spark, out _);
        // 步骤 7：计算 BuildFlameMesh()，把结果存进 refined（refined）；之后引用 refined 时用的就是这次结果。
        Mesh refined = BuildFlameMesh();
        // 步骤 8：调用 Clear：Clear。
        flameMesh.Clear();
        // 步骤 9：把计算结果 refined.vertices 保存为vertices；下一条判断或调用会读取这个具体值。
        flameMesh.vertices = refined.vertices;
        // 步骤 10：把计算结果 refined.triangles 保存为triangles；下一条判断或调用会读取这个具体值。
        flameMesh.triangles = refined.triangles;
        // 步骤 11：把计算结果 refined.uv 保存为uv；下一条判断或调用会读取这个具体值。
        flameMesh.uv = refined.uv;
        // 步骤 12：调用 RecalculateNormals：RecalculateNormals。
        flameMesh.RecalculateNormals();
        // 步骤 13：调用 RecalculateBounds：RecalculateBounds。
        flameMesh.RecalculateBounds();
        // 步骤 14：在编辑器里立即移除旧对象；重新搭建时避免上次生成内容重复叠加。
        UnityEngine.Object.DestroyImmediate(refined);
        // 步骤 15：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(flameMesh);
        // 步骤 16：计算 HaloMesh()，把结果存进 haloMesh（halo网格）；之后引用 haloMesh 时用的就是这次结果。
        Mesh haloMesh = HaloMesh();
        // 步骤 17：调用 PolishScene：Polish场景。
        PolishScene(DesktopScene, flameMesh, haloMesh, inner, spark);
        // 步骤 18：调用 PolishScene：Polish场景。
        PolishScene(QuestScene, flameMesh, haloMesh, inner, spark);
        // 步骤 19：这里的 ! 表示取反；当 string.IsNullOrEmpty(previous) 不成立时才进入此分支。 成立时调用 OpenScene：Open场景。
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
        // 步骤 20：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 21：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Flying flames polished in desktop and Quest scenes.");
    }
    // ！！！功能结束：PolishExistingFlames！！！

    // 继续填写上一行调用的参数：private static void PolishScene(string path, Mesh flameMesh, Mesh haloMesh；参数位置决定它表示的坐标、速度或选项。
    private static void PolishScene(string path, Mesh flameMesh, Mesh haloMesh,
        // 这一行补充 Material inner, Material spark)，与相邻行合起来完成同一次计算或对象配置。
        Material inner, Material spark)
    {
        // 计算 EditorSceneManager.OpenScene(path, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        // 计算 GameObject.Find("Flying flame challenge")，把结果存进 challenge（challenge）；之后引用 challenge 时用的就是这次结果。
        GameObject challenge = GameObject.Find("Flying flame challenge");
        // 检查challenge是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("Flying flame challenge missing from " + path);
        if (challenge == null) throw new Exception("Flying flame challenge missing from " + path);
        // 从目标物体找到所需组件并存为 flames；使用前应检查是否为 null。
        SkyQuestFlightFlame[] flames = challenge.GetComponentsInChildren<SkyQuestFlightFlame>(true);
        // 检查条件 flames.Length != FlamePositions.Length；只有为真才运行下面的步骤，否则沿原流程继续。
        if (flames.Length != FlamePositions.Length)
            // 遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Expected " + FlamePositions.Length + " flames in " + path);
        // 逐个读取 SkyQuestFlightFlame flame in flames 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (SkyQuestFlightFlame flame in flames)
        {
            // 取目标对象的 Transform 并存为 parent；Transform 提供位置、旋转和父子关系。
            Transform parent = flame.transform;
            // 逐个读取 MeshFilter filter in parent.GetComponentsInChildren<MeshFilter>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (MeshFilter filter in parent.GetComponentsInChildren<MeshFilter>(true))
                // 检查条件 filter.sharedMesh != haloMesh；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 filter.sharedMesh = flameMesh;
                if (filter.sharedMesh != haloMesh) filter.sharedMesh = flameMesh;
            // 调用 AddRefinedDetails：添加RefinedDetails。
            AddRefinedDetails(parent, flameMesh, haloMesh, inner, spark);
        }
        // 调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 这里的 ! 表示取反；当 EditorSceneManager.SaveScene(scene, path) 不成立时才进入此分支。
        if (!EditorSceneManager.SaveScene(scene, path))
            // 遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Could not save polished flames in " + path);
    }

    // 继续填写上一行调用的参数：private static void InstallInScene(string path, Mesh mesh, Mesh haloMesh, Material outer；参数位置决定它表示的坐标、速度或选项。
    private static void InstallInScene(string path, Mesh mesh, Mesh haloMesh, Material outer,
        // 这一行补充 Material inner, Material spark, Material start)，与相邻行合起来完成同一次计算或对象配置。
        Material inner, Material spark, Material start)
    {
        // 计算 EditorSceneManager.OpenScene(path, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        // 计算 GameObject.Find("Flying flame challenge")，把结果存进 existing（existing）；之后引用 existing 时用的就是这次结果。
        GameObject existing = GameObject.Find("Flying flame challenge");
        // 确认existing已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 DestroyImmediate：DestroyImmediate。
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
        // 创建新的 GameObject 并存为 root；构造参数决定其初始内容或对象名称。
        GameObject root = new GameObject("Flying flame challenge");
        // 计算 root.AddComponent<SkyQuestFlightFlameGame>()，把结果存进 game（game）；之后引用 game 时用的就是这次结果。
        SkyQuestFlightFlameGame game = root.AddComponent<SkyQuestFlightFlameGame>();
        // 把计算结果 90f 保存为roundSeconds；下一条判断或调用会读取这个具体值。
        game.roundSeconds = 90f;
        // 把计算结果 10 保存为pointsPerFlame；下一条判断或调用会读取这个具体值。
        game.pointsPerFlame = 10;

        // 计算 GroundHeight(40.5f, 41.5f)，把结果存进 ground（ground）；之后引用 ground 时用的就是这次结果。
        float ground = GroundHeight(40.5f, 41.5f);
        // 创建新的 GameObject 并存为 startRoot；构造参数决定其初始内容或对象名称。
        GameObject startRoot = new GameObject("Blue flight challenge start ring");
        // 执行 startRoot.transform.SetParent(root.transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        startRoot.transform.SetParent(root.transform, false);
        // 执行 startRoot.transform.position = new Vector3(40.5f, ground + .08f, 41.5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        startRoot.transform.position = new Vector3(40.5f, ground + .08f, 41.5f);
        // 把计算结果 startRoot.transform 保存为startMarker；下一条判断或调用会读取这个具体值。
        game.startMarker = startRoot.transform;
        // 调用 BuildStartRing：构建StartRing。
        BuildStartRing(startRoot.transform, start);

        // 新建用于flames的对象或数值结构，并把构造参数保存进去。
        game.flames = new SkyQuestFlightFlame[FlamePositions.Length];
        // 按 int i = 0; i < FlamePositions.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < FlamePositions.Length; i++)
        {
            // 计算 ClearPosition(FlamePositions[i])，把结果存进 position（position）；之后引用 position 时用的就是这次结果。
            Vector3 position = ClearPosition(FlamePositions[i]);
            // 创建新的 GameObject 并存为 flameRoot；构造参数决定其初始内容或对象名称。
            GameObject flameRoot = new GameObject("Flight flame " + (i + 1).ToString("00"));
            // 执行 flameRoot.transform.SetParent(root.transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            flameRoot.transform.SetParent(root.transform, false);
            // 执行 flameRoot.transform.position = position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            flameRoot.transform.position = position;
            // 计算 flameRoot.AddComponent<SkyQuestFlightFlame>()，把结果存进 flame（flame）；之后引用 flame 时用的就是这次结果。
            SkyQuestFlightFlame flame = flameRoot.AddComponent<SkyQuestFlightFlame>();
            // 把计算结果 1.2f 保存为collectRadius；下一条判断或调用会读取这个具体值。
            flame.collectRadius = .72f;
            // 调用 FlamePart：FlamePart。
            FlamePart(flameRoot.transform, "Amber flame", mesh, outer,
                // 执行 Vector3.zero, Vector3.one);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.zero, Vector3.one);
            // 调用 FlamePart：FlamePart。
            FlamePart(flameRoot.transform, "Golden inner flame", mesh, inner,
                // 用 new 创建 Vector3(0f, -.12f, -.02f), Vector3.one * .55f);；构造参数决定新对象的初始内容。
                new Vector3(0f, -.12f, -.02f), Vector3.one * .55f);
            // 按 int tongue = 0; tongue < 3; tongue++ 变化索引；循环体依次处理每个位置。
            for (int tongue = 0; tongue < 3; tongue++)
            {
                // 计算 tongue * 120f * Mathf.Deg2Rad，把结果存进 tongueAngle（tongueAngle）；之后引用 tongueAngle 时用的就是这次结果。
                float tongueAngle = tongue * 120f * Mathf.Deg2Rad;
                // 调用 FlamePart：FlamePart。
                FlamePart(flameRoot.transform, "Visible golden flame tongue", mesh, inner,
                    // 用 new 创建 Vector3(Mathf.Cos(tongueAngle) * .32f, -.12f,；构造参数决定新对象的初始内容。
                    new Vector3(Mathf.Cos(tongueAngle) * .32f, -.12f,
                        // 调用 Sin：Sin。
                        Mathf.Sin(tongueAngle) * .32f), Vector3.one * .52f);
            }
            // 按 int sparkIndex = 0; sparkIndex < 3; sparkIndex++ 变化索引；循环体依次处理每个位置。
            for (int sparkIndex = 0; sparkIndex < 3; sparkIndex++)
            {
                // 计算 (sparkIndex * 120f + i * 25f) * Mathf.Deg2Rad，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
                float angle = (sparkIndex * 120f + i * 25f) * Mathf.Deg2Rad;
                // 把计算结果 Decor(PrimitiveType.Sphere, 保存为spark物品；下一条判断或调用会读取这个具体值。
                GameObject sparkObject = Decor(PrimitiveType.Sphere,
                    // 执行 "Floating flame spark", flameRoot.transform, spark);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    "Floating flame spark", flameRoot.transform, spark);
                // 继续填写上一行调用的参数：sparkObject.transform.localPosition = new Vector3(Mathf.Cos(angle) * .39f；参数位置决定它表示的坐标、速度或选项。
                sparkObject.transform.localPosition = new Vector3(Mathf.Cos(angle) * .39f,
                    // 执行 (sparkIndex - 1) * .22f, Mathf.Sin(angle) * .39f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    (sparkIndex - 1) * .22f, Mathf.Sin(angle) * .39f);
                // 执行 sparkObject.transform.localScale = Vector3.one * .09f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                sparkObject.transform.localScale = Vector3.one * .09f;
            }
            // 调用 AddRefinedDetails：添加RefinedDetails。
            AddRefinedDetails(flameRoot.transform, mesh, haloMesh, inner, spark);
            // 执行 game.flames[i] = flame;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            game.flames[i] = flame;
        }
        // 把计算结果 FindScenePlayer(scene, path == DesktopScene) 保存为player；下一条判断或调用会读取这个具体值。
        game.player = FindScenePlayer(scene, path == DesktopScene);
        // 调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 这里的 ! 表示取反；当 EditorSceneManager.SaveScene(scene, path) 不成立时才进入此分支。
        if (!EditorSceneManager.SaveScene(scene, path))
            // 遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Could not save flight game into " + path);
    }

    // ！！！功能：FindScenePlayer——查找场景玩家！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Scene scene, bool desktop 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static SkyQuestLocomotion FindScenePlayer(Scene scene, bool desktop)
    {
        // 步骤 1：计算 null，把结果存进 fallback（fallback）；之后引用 fallback 时用的就是这次结果。
        SkyQuestLocomotion fallback = null;
        // 步骤 2：逐个读取 GameObject root in scene.GetRootGameObjects() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (GameObject root in scene.GetRootGameObjects())
            // 步骤 3：逐个读取 SkyQuestLocomotion candidate in root.GetComponentsInChildren<SkyQuestLocomotion>(true 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (SkyQuestLocomotion candidate in root.GetComponentsInChildren<SkyQuestLocomotion>(true))
            {
                // 步骤 4：检查fallback是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 fallback = candidate;
                if (fallback == null) fallback = candidate;
                // 步骤 5：检查条件 candidate.desktopControlled == desktop；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
                if (candidate.desktopControlled == desktop) return candidate;
            }
        // 步骤 6：把 fallback 作为方法结果返回给调用方；return 后本方法立即结束。
        return fallback;
    }
    // ！！！功能结束：FindScenePlayer！！！

    // ！！！功能：BuildStartRing——构建StartRing！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform parent, Material start 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void BuildStartRing(Transform parent, Material start)
    {
        // 步骤 1：定义不可在运行时改变的数值 count（count）；后续计算统一使用这个阈值、速度或尺寸。
        const int count = 20;
        // 步骤 2：按 int i = 0; i < count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < count; i++)
        {
            // 步骤 3：计算 i * Mathf.PI * 2f / count，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * Mathf.PI * 2f / count;
            // 步骤 4：计算 (i + 1) * Mathf.PI * 2f / count，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
            float b = (i + 1) * Mathf.PI * 2f / count;
            // 步骤 5：创建新的 Vector3 并存为 p；构造参数决定其初始内容或对象名称。
            Vector3 p = new Vector3(Mathf.Cos(a) * 1.25f, 0f, Mathf.Sin(a) * 1.25f);
            // 步骤 6：创建新的 Vector3 并存为 q；构造参数决定其初始内容或对象名称。
            Vector3 q = new Vector3(Mathf.Cos(b) * 1.25f, 0f, Mathf.Sin(b) * 1.25f);
            // 步骤 7：计算 Decor(PrimitiveType.Cube, "Start ring edge", parent, start)，把结果存进 segment（segment）；之后引用 segment 时用的就是这次结果。
            GameObject segment = Decor(PrimitiveType.Cube, "Start ring edge", parent, start);
            // 步骤 8：执行 segment.transform.localPosition = (p + q) * .5f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            segment.transform.localPosition = (p + q) * .5f;
            // 步骤 9：执行 segment.transform.localRotation = Quaternion.FromToRotation(Vector3.right, (q - p).normalized);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            segment.transform.localRotation = Quaternion.FromToRotation(Vector3.right, (q - p).normalized);
            // 步骤 10：执行 segment.transform.localScale = new Vector3(Vector3.Distance(p, q) + .03f, .06f, .12f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            segment.transform.localScale = new Vector3(Vector3.Distance(p, q) + .03f, .06f, .12f);
        }
        // 步骤 11：计算 Decor(PrimitiveType.Sphere, "Start beacon", parent, start)，把结果存进 beacon（beacon）；之后引用 beacon 时用的就是这次结果。
        GameObject beacon = Decor(PrimitiveType.Sphere, "Start beacon", parent, start);
        // 步骤 12：执行 beacon.transform.localPosition = Vector3.up * 1.45f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        beacon.transform.localPosition = Vector3.up * 1.45f;
        // 步骤 13：执行 beacon.transform.localScale = Vector3.one * .24f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        beacon.transform.localScale = Vector3.one * .24f;
        // 步骤 14：计算 Decor(PrimitiveType.Cylinder, "Start beacon ray", parent, start)，把结果存进 stem（stem）；之后引用 stem 时用的就是这次结果。
        GameObject stem = Decor(PrimitiveType.Cylinder, "Start beacon ray", parent, start);
        // 步骤 15：执行 stem.transform.localPosition = Vector3.up * .75f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        stem.transform.localPosition = Vector3.up * .75f;
        // 步骤 16：执行 stem.transform.localScale = new Vector3(.035f, .7f, .035f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        stem.transform.localScale = new Vector3(.035f, .7f, .035f);
    }
    // ！！！功能结束：BuildStartRing！！！

    // 继续填写上一行调用的参数：private static GameObject FlamePart(Transform parent, string name, Mesh mesh, Material material；参数位置决定它表示的坐标、速度或选项。
    private static GameObject FlamePart(Transform parent, string name, Mesh mesh, Material material,
        // 这一行补充 Vector3 localPosition, Vector3 localScale)，与相邻行合起来完成同一次计算或对象配置。
        Vector3 localPosition, Vector3 localScale)
    {
        // 创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        // 执行 obj.transform.SetParent(parent, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(parent, false);
        // 执行 obj.transform.localPosition = localPosition;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.localPosition = localPosition;
        // 执行 obj.transform.localScale = localScale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.localScale = localScale;
        // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        obj.GetComponent<MeshRenderer>().sharedMaterial = material;
        // 把 obj 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj;
    }

    // 继续填写上一行调用的参数：private static void AddRefinedDetails(Transform parent, Mesh flameMesh, Mesh haloMesh；参数位置决定它表示的坐标、速度或选项。
    private static void AddRefinedDetails(Transform parent, Mesh flameMesh, Mesh haloMesh,
        // 这一行补充 Material inner, Material spark)，与相邻行合起来完成同一次计算或对象配置。
        Material inner, Material spark)
    {
        // 检查对象引用是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (parent.Find("White-hot heart") == null)
            // 调用 FlamePart：FlamePart。
            FlamePart(parent, "White-hot heart", flameMesh, spark,
                // 用 new 创建 Vector3(0f, -.16f, 0f), new Vector3(.28f, .53f, .28f));；构造参数决定新对象的初始内容。
                new Vector3(0f, -.16f, 0f), new Vector3(.28f, .53f, .28f));
        // 按 int i = 0; i < 2; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 2; i++)
        {
            // 计算 "Fine gold wisp " + (i + 1)，把结果存进 name（name）；之后引用 name 时用的就是这次结果。
            string name = "Fine gold wisp " + (i + 1);
            // 确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 continue;
            if (parent.Find(name) != null) continue;
            // 用三元表达式为 side 二选一：问号前条件为真取前值，为假取冒号后的值。
            float side = i == 0 ? -1f : 1f;
            // 把计算结果 FlamePart(parent, name, flameMesh, inner, 保存为wisp；下一条判断或调用会读取这个具体值。
            GameObject wisp = FlamePart(parent, name, flameMesh, inner,
                // 用 new 创建 Vector3(side * .19f, -.19f, i == 0 ? -.1f : .1f),；构造参数决定新对象的初始内容。
                new Vector3(side * .19f, -.19f, i == 0 ? -.1f : .1f),
                // 用 new 创建 Vector3(.22f, .53f, .22f));；构造参数决定新对象的初始内容。
                new Vector3(.22f, .53f, .22f));
            // 执行 wisp.transform.localRotation = Quaternion.Euler(0f, 0f, side * 18f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            wisp.transform.localRotation = Quaternion.Euler(0f, 0f, side * 18f);
        }
        // 检查对象引用是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (parent.Find("Golden base halo") == null)
        {
            // 把计算结果 FlamePart(parent, "Golden base halo", haloMesh, spark, 保存为halo；下一条判断或调用会读取这个具体值。
            GameObject halo = FlamePart(parent, "Golden base halo", haloMesh, spark,
                // 用 new 创建 Vector3(0f, -.35f, 0f), Vector3.one * .61f);；构造参数决定新对象的初始内容。
                new Vector3(0f, -.35f, 0f), Vector3.one * .61f);
            // 执行 halo.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            halo.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
        }
    }

    // ！！！功能：Decor——Decor！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 PrimitiveType type, string name, Transform parent, Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject Decor(PrimitiveType type, string name, Transform parent, Material material)
    {
        // 步骤 1：计算 GameObject.CreatePrimitive(type)，把结果存进 obj（obj）；之后引用 obj 时用的就是这次结果。
        GameObject obj = GameObject.CreatePrimitive(type);
        // 步骤 2：把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        obj.name = name;
        // 步骤 3：执行 obj.transform.SetParent(parent, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(parent, false);
        // 步骤 4：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        obj.GetComponent<Renderer>().sharedMaterial = material;
        // 步骤 5：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        // 步骤 6：把 obj 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj;
    }
    // ！！！功能结束：Decor！！！

    // ！！！功能：ClearPosition——Clear位置！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 position 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Vector3 ClearPosition(Vector3 position)
    {
        // 步骤 1：调用 SyncTransforms：SyncTransforms。
        Physics.SyncTransforms();
        // 步骤 2：按 int attempt = 0; attempt < 7; attempt++ 变化索引；循环体依次处理每个位置。
        for (int attempt = 0; attempt < 7; attempt++)
        {
            // 步骤 3：用 blocked 记住此处真假结果（false），供下一条 if 选择执行路径。
            bool blocked = false;
            // 步骤 4：逐个读取 Collider hit in Physics.OverlapSphere(position, .9f, ~0, QueryTriggerInteraction.Igno 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Collider hit in Physics.OverlapSphere(position, .9f, ~0, QueryTriggerInteraction.Ignore))
                // 步骤 5：检查hit是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 { blocked = true; break; }
                if (hit != null && hit.gameObject.scene.IsValid()) { blocked = true; break; }
            // 步骤 6：这里的 ! 表示取反；当 blocked 不成立时才进入此分支。 条件成立时立即退出当前方法。
            if (!blocked) return position;
            // 步骤 7：在y原值上增加 .7f；旧状态与本次输入共同决定新状态。
            position.y += .7f;
        }
        // 步骤 8：把 position 作为方法结果返回给调用方；return 后本方法立即结束。
        return position;
    }
    // ！！！功能结束：ClearPosition！！！

    // ！！！功能：GroundHeight——GroundHeight！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float x, float z 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    private static float GroundHeight(float x, float z)
    {
        // 步骤 1：计算 float.NegativeInfinity，把结果存进 best（best）；之后引用 best 时用的就是这次结果。
        float best = float.NegativeInfinity;
        // 步骤 2：逐个读取 RaycastHit hit in Physics.RaycastAll(new Vector3(x, 18f, z), Vector3.down, 24f, 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(x, 18f, z), Vector3.down, 24f,
                     // 步骤 3：这一行补充 ~0, QueryTriggerInteraction.Ignore))，与相邻行合起来完成同一次计算或对象配置。
                     ~0, QueryTriggerInteraction.Ignore))
            // 步骤 4：这里用 && 要求两边条件同时成立：hit.normal.y > .6f && hit.point.y < 2.5f && hit.point.y > best；任意一边为假就跳过本分支。
            if (hit.normal.y > .6f && hit.point.y < 2.5f && hit.point.y > best)
                // 步骤 5：把计算结果 hit.point.y 保存为best；下一条判断或调用会读取这个具体值。
                best = hit.point.y;
        // 步骤 6：把 float.IsNegativeInfinity(best) ? 0f : best 作为方法结果返回给调用方；return 后本方法立即结束。
        return float.IsNegativeInfinity(best) ? 0f : best;
    }
    // ！！！功能结束：GroundHeight！！！

    // 继续填写上一行调用的参数：private static void EnsureAssets(out Mesh mesh, out Material outer, out Material inner；参数位置决定它表示的坐标、速度或选项。
    private static void EnsureAssets(out Mesh mesh, out Material outer, out Material inner,
        // 这一行补充 out Material spark, out Material start)，与相邻行合起来完成同一次计算或对象配置。
        out Material spark, out Material start)
    {
        // 这里的 ! 表示取反；当 AssetDatabase.IsValidFolder(MaterialFolder) 不成立时才进入此分支。
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            // 调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateFolder("Assets/SkyQuest", "FlightMaterials");
        // 把计算结果 AssetDatabase.LoadAssetAtPath<Mesh>(FlameMeshPath) 保存为mesh；下一条判断或调用会读取这个具体值。
        mesh = AssetDatabase.LoadAssetAtPath<Mesh>(FlameMeshPath);
        // 检查mesh是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (mesh == null)
        {
            // 把计算结果 BuildFlameMesh() 保存为mesh；下一条判断或调用会读取这个具体值。
            mesh = BuildFlameMesh();
            // 调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateAsset(mesh, FlameMeshPath);
        }
        // 新建用于outer的对象或数值结构，并把构造参数保存进去。
        outer = MaterialAsset("Flight flame amber", new Color(1f, .56f, .12f));
        // 新建用于inner的对象或数值结构，并把构造参数保存进去。
        inner = MaterialAsset("Flight flame gold", new Color(1f, .91f, .3f));
        // 新建用于spark的对象或数值结构，并把构造参数保存进去。
        spark = MaterialAsset("Flight flame sparks", new Color(1f, .96f, .58f));
        // 新建用于start的对象或数值结构，并把构造参数保存进去。
        start = MaterialAsset("Flight game start blue", new Color(.16f, .82f, 1f));
    }

    // ！！！功能：MaterialAsset——材质资源！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Color color 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Material 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Material MaterialAsset(string name, Color color)
    {
        // 步骤 1：计算 MaterialFolder + "/" + name + ".mat"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
        string path = MaterialFolder + "/" + name + ".mat";
        // 步骤 2：计算 AssetDatabase.LoadAssetAtPath<Material>(path)，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        // 步骤 3：确认material已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (material != null)
        {
            // 步骤 4：把计算结果 color 保存为color；下一条判断或调用会读取这个具体值。
            material.color = color;
            // 步骤 5：调用 EnableKeyword：EnableKeyword。
            material.EnableKeyword("_EMISSION");
            // 步骤 6：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
            material.SetColor("_EmissionColor", color * 2f);
            // 步骤 7：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(material);
            // 步骤 8：把 material 作为方法结果返回给调用方；return 后本方法立即结束。
            return material;
        }
        // 步骤 9：计算 Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")，把结果存进 shader（shader）；之后引用 shader 时用的就是这次结果。
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        // 步骤 10：新建用于material的对象或数值结构，并把构造参数保存进去。
        material = new Material(shader) { name = name, color = color };
        // 步骤 11：调用 EnableKeyword：EnableKeyword。
        material.EnableKeyword("_EMISSION");
        // 步骤 12：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
        material.SetColor("_EmissionColor", color * 2f);
        // 步骤 13：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.CreateAsset(material, path);
        // 步骤 14：把 material 作为方法结果返回给调用方；return 后本方法立即结束。
        return material;
    }
    // ！！！功能结束：MaterialAsset！！！

    // ！！！功能：BuildFlameMesh——构建Flame网格！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Mesh 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Mesh BuildFlameMesh()
    {
        // 步骤 1：定义不可在运行时改变的数值 sides（sides）；后续计算统一使用这个阈值、速度或尺寸。
        const int sides = 16;
        // 步骤 2：计算 { -.44f, -.37f, -.23f, -.02f, .17f, .34f, .52f, .68f, .81f }，把结果存进 heights（heights）；之后引用 heights 时用的就是这次结果。
        float[] heights = { -.44f, -.37f, -.23f, -.02f, .17f, .34f, .52f, .68f, .81f };
        // 步骤 3：计算 { .12f, .22f, .32f, .34f, .29f, .22f, .15f, .075f, .012f }，把结果存进 radii（radii）；之后引用 radii 时用的就是这次结果。
        float[] radii = { .12f, .22f, .32f, .34f, .29f, .22f, .15f, .075f, .012f };
        // 步骤 4：计算 heights.Length，把结果存进 rings（rings）；之后引用 rings 时用的就是这次结果。
        int rings = heights.Length;
        // 步骤 5：创建新的 Vector3[] 并存为 vertices；构造参数决定其初始内容或对象名称。
        Vector3[] vertices = new Vector3[sides * rings];
        // 步骤 6：创建新的 Vector2[] 并存为 uv；构造参数决定其初始内容或对象名称。
        Vector2[] uv = new Vector2[vertices.Length];
        // 步骤 7：创建新的 int[] 并存为 triangles；构造参数决定其初始内容或对象名称。
        int[] triangles = new int[sides * (rings - 1) * 6];
        // 步骤 8：按 int ring = 0; ring < rings; ring++ 变化索引；循环体依次处理每个位置。
        for (int ring = 0; ring < rings; ring++)
        {
            // 步骤 9：计算 (float)ring / (rings - 1)，把结果存进 progress（progress）；之后引用 progress 时用的就是这次结果。
            float progress = (float)ring / (rings - 1);
            // 步骤 10：按 int side = 0; side < sides; side++ 变化索引；循环体依次处理每个位置。
            for (int side = 0; side < sides; side++)
            {
                // 步骤 11：计算 side * Mathf.PI * 2f / sides，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
                float angle = side * Mathf.PI * 2f / sides;
                // 步骤 12：计算 1f + .09f * Mathf.Sin(angle * 3f + progress * 5f)，把结果存进 ripple（ripple）；之后引用 ripple 时用的就是这次结果。
                float ripple = 1f + .09f * Mathf.Sin(angle * 3f + progress * 5f);
                // 步骤 13：计算 .12f * progress * progress，把结果存进 bend（bend）；之后引用 bend 时用的就是这次结果。
                float bend = .12f * progress * progress;
                // 步骤 14：计算 ring * sides + side，把结果存进 index（index）；之后引用 index 时用的就是这次结果。
                int index = ring * sides + side;
                // 步骤 15：继续填写上一行调用的参数：vertices[index] = new Vector3(Mathf.Cos(angle) * radii[ring] * ripple + bend；参数位置决定它表示的坐标、速度或选项。
                vertices[index] = new Vector3(Mathf.Cos(angle) * radii[ring] * ripple + bend,
                    // 步骤 16：执行 heights[ring], Mathf.Sin(angle) * radii[ring] * ripple - bend * .35f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    heights[ring], Mathf.Sin(angle) * radii[ring] * ripple - bend * .35f);
                // 步骤 17：执行 uv[index] = new Vector2((float)side / sides, progress);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                uv[index] = new Vector2((float)side / sides, progress);
                // 步骤 18：检查条件 ring == rings - 1；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
                if (ring == rings - 1) continue;
                // 步骤 19：计算 (side + 1) % sides，把结果存进 next（next）；之后引用 next 时用的就是这次结果。
                int next = (side + 1) % sides;
                // 步骤 20：计算 (ring * sides + side) * 6，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
                int t = (ring * sides + side) * 6;
                // 步骤 21：执行 triangles[t] = index;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                triangles[t] = index;
                // 步骤 22：执行 triangles[t + 1] = index + sides;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                triangles[t + 1] = index + sides;
                // 步骤 23：执行 triangles[t + 2] = ring * sides + next;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                triangles[t + 2] = ring * sides + next;
                // 步骤 24：执行 triangles[t + 3] = ring * sides + next;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                triangles[t + 3] = ring * sides + next;
                // 步骤 25：执行 triangles[t + 4] = index + sides;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                triangles[t + 4] = index + sides;
                // 步骤 26：执行 triangles[t + 5] = (ring + 1) * sides + next;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                triangles[t + 5] = (ring + 1) * sides + next;
            }
        }
        // 步骤 27：创建新的 Mesh 并存为 result；构造参数决定其初始内容或对象名称。
        Mesh result = new Mesh { name = "Sky flight flame" };
        // 步骤 28：把计算结果 vertices 保存为vertices；下一条判断或调用会读取这个具体值。
        result.vertices = vertices;
        // 步骤 29：把计算结果 triangles 保存为triangles；下一条判断或调用会读取这个具体值。
        result.triangles = triangles;
        // 步骤 30：把计算结果 uv 保存为uv；下一条判断或调用会读取这个具体值。
        result.uv = uv;
        // 步骤 31：调用 RecalculateNormals：RecalculateNormals。
        result.RecalculateNormals();
        // 步骤 32：调用 RecalculateBounds：RecalculateBounds。
        result.RecalculateBounds();
        // 步骤 33：把 result 作为方法结果返回给调用方；return 后本方法立即结束。
        return result;
    }
    // ！！！功能结束：BuildFlameMesh！！！

    // ！！！功能：HaloMesh——Halo网格！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Mesh 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Mesh HaloMesh()
    {
        // 步骤 1：计算 AssetDatabase.LoadAssetAtPath<Mesh>(HaloMeshPath)，把结果存进 mesh（mesh）；之后引用 mesh 时用的就是这次结果。
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(HaloMeshPath);
        // 步骤 2：确认mesh已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (mesh != null) return mesh;
        // 步骤 3：定义不可在运行时改变的数值 segments（segments）；后续计算统一使用这个阈值、速度或尺寸。
        const int segments = 24;
        // 步骤 4：定义不可在运行时改变的数值 sides（sides）；后续计算统一使用这个阈值、速度或尺寸。
        const int sides = 5;
        // 步骤 5：创建新的 Vector3[] 并存为 vertices；构造参数决定其初始内容或对象名称。
        Vector3[] vertices = new Vector3[segments * sides];
        // 步骤 6：创建新的 int[] 并存为 triangles；构造参数决定其初始内容或对象名称。
        int[] triangles = new int[segments * sides * 6];
        // 步骤 7：按 int i = 0; i < segments; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < segments; i++)
        // 步骤 8：按 int j = 0; j < sides; j++ 变化索引；循环体依次处理每个位置。
        for (int j = 0; j < sides; j++)
        {
            // 步骤 9：计算 i * Mathf.PI * 2f / segments，把结果存进 around（around）；之后引用 around 时用的就是这次结果。
            float around = i * Mathf.PI * 2f / segments;
            // 步骤 10：计算 j * Mathf.PI * 2f / sides，把结果存进 cross（cross）；之后引用 cross 时用的就是这次结果。
            float cross = j * Mathf.PI * 2f / sides;
            // 步骤 11：计算 .52f + .025f * Mathf.Cos(cross)，把结果存进 radius（radius）；之后引用 radius 时用的就是这次结果。
            float radius = .52f + .025f * Mathf.Cos(cross);
            // 步骤 12：计算 i * sides + j，把结果存进 v（v）；之后引用 v 时用的就是这次结果。
            int v = i * sides + j;
            // 步骤 13：继续填写上一行调用的参数：vertices[v] = new Vector3(Mathf.Cos(around) * radius；参数位置决定它表示的坐标、速度或选项。
            vertices[v] = new Vector3(Mathf.Cos(around) * radius,
                // 步骤 14：这是上一行链式调用的下一步 .025f * Mathf.Sin(cross), Mathf.Sin(around) * radius);；前一结果会继续传给这里筛选或处理。
                .025f * Mathf.Sin(cross), Mathf.Sin(around) * radius);
            // 步骤 15：计算 ((i + 1) % segments) * sides，把结果存进 nextAround（nextAround）；之后引用 nextAround 时用的就是这次结果。
            int nextAround = ((i + 1) % segments) * sides;
            // 步骤 16：计算 (j + 1) % sides，把结果存进 nextCross（nextCross）；之后引用 nextCross 时用的就是这次结果。
            int nextCross = (j + 1) % sides;
            // 步骤 17：计算 v * 6，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            int t = v * 6;
            // 步骤 18：执行 triangles[t] = v;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            triangles[t] = v;
            // 步骤 19：执行 triangles[t + 1] = nextAround + j;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            triangles[t + 1] = nextAround + j;
            // 步骤 20：执行 triangles[t + 2] = i * sides + nextCross;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            triangles[t + 2] = i * sides + nextCross;
            // 步骤 21：执行 triangles[t + 3] = i * sides + nextCross;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            triangles[t + 3] = i * sides + nextCross;
            // 步骤 22：执行 triangles[t + 4] = nextAround + j;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            triangles[t + 4] = nextAround + j;
            // 步骤 23：执行 triangles[t + 5] = nextAround + nextCross;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            triangles[t + 5] = nextAround + nextCross;
        }
        // 步骤 24：新建用于mesh的对象或数值结构，并把构造参数保存进去。
        mesh = new Mesh { name = "Sky flight halo", vertices = vertices, triangles = triangles };
        // 步骤 25：调用 RecalculateNormals：RecalculateNormals。
        mesh.RecalculateNormals();
        // 步骤 26：调用 RecalculateBounds：RecalculateBounds。
        mesh.RecalculateBounds();
        // 步骤 27：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.CreateAsset(mesh, HaloMeshPath);
        // 步骤 28：把 mesh 作为方法结果返回给调用方；return 后本方法立即结束。
        return mesh;
    }
    // ！！！功能结束：HaloMesh！！！
}
