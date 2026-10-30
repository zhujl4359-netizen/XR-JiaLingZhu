// ！！！文件功能：在编辑器创建独立的 Quest 飞行练习场景！！！
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

/// <summary>Creates an isolated, primitive-only Quest flight practice scene.</summary>
// 定义 CapeFlightTestLevelBuilder 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class CapeFlightTestLevelBuilder
{
    // 定义固定文字 ScenePath（场景路径）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string ScenePath = "Assets/SkyQuest/CapeFlightTest.unity";
    // 定义固定文字 RigPath（Rig路径）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string RigPath = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
    // 定义固定文字 MaterialFolder（材质文件夹）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string MaterialFolder = "Assets/SkyQuest/FlightMaterials";

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Build cape flight test level")]
    // ！！！功能：Build——根据代码在 Unity 编辑器里创建或更新场景资源！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void Build()
    {
        // 步骤 1：计算 EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        // 步骤 2：计算 AssetDatabase.LoadAssetAtPath<GameObject>(RigPath)，把结果存进 prefab（prefab）；之后引用 prefab 时用的就是这次结果。
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigPath);
        // 步骤 3：检查prefab是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("XRI XR Origin prefab is missing.");
        if (prefab == null) throw new Exception("XRI XR Origin prefab is missing.");
        // 步骤 4：计算 (GameObject)PrefabUtility.InstantiatePrefab(prefab)，把结果存进 rig（rig）；之后引用 rig 时用的就是这次结果。
        GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        // 步骤 5：把计算结果 "Cape Flight XR Origin" 保存为name；下一条判断或调用会读取这个具体值。
        rig.name = "Cape Flight XR Origin";
        // 步骤 6：执行 rig.transform.position = new Vector3(0f, 0.55f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rig.transform.position = new Vector3(0f, 0.55f, 0f);
        // 步骤 7：逐个读取 MonoBehaviour behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (MonoBehaviour behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true))
        {
            // 步骤 8：检查behaviour是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (behaviour == null) continue;
            // 步骤 9：计算 behaviour.GetType().Name，把结果存进 name（name）；之后引用 name 时用的就是这次结果。
            string name = behaviour.GetType().Name;
            // 步骤 10：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (name.Contains("MoveProvider") || name.Contains("TurnProvider") ||
                // 步骤 11：调用 Contains：Contains。
                name.Contains("TeleportationProvider") || name.Contains("JumpProvider") ||
                // 步骤 12：调用 Contains：Contains。
                name.Contains("ClimbProvider") || name.Contains("GrabMoveProvider") ||
                // 步骤 13：调用 Contains：Contains。
                name.Contains("GravityProvider") || behaviour.gameObject.name == "Gravity")
                // 步骤 14：把 behaviour.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                behaviour.enabled = false;
        }
        // 步骤 15：从目标物体找到所需组件并存为 head；使用前应检查是否为 null。
        Camera head = rig.GetComponentInChildren<Camera>(true);
        // 步骤 16：计算 Find(rig.transform, "Left Controller")，把结果存进 left（left）；之后引用 left 时用的就是这次结果。
        Transform left = Find(rig.transform, "Left Controller");
        // 步骤 17：计算 Find(rig.transform, "Right Controller")，把结果存进 right（right）；之后引用 right 时用的就是这次结果。
        Transform right = Find(rig.transform, "Right Controller");
        // 步骤 18：检查头显相机、left、right是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 throw new Exception("XR rig camera/controllers missing.");
        if (head == null || left == null || right == null) throw new Exception("XR rig camera/controllers missing.");
        // 步骤 19：显示这个场景对象及其子对象；界面会立即反映这一状态。
        head.gameObject.SetActive(true);
        // 步骤 20：把计算结果 "MainCamera" 保存为tag；下一条判断或调用会读取这个具体值。
        head.tag = "MainCamera";
        // 步骤 21：计算 rig.AddComponent<SkyQuestLocomotion>()，把结果存进 locomotion（locomotion）；之后引用 locomotion 时用的就是这次结果。
        SkyQuestLocomotion locomotion = rig.AddComponent<SkyQuestLocomotion>();
        // 步骤 22：把计算结果 head.transform 保存为头显相机；下一条判断或调用会读取这个具体值。
        locomotion.head = head.transform;
        // 步骤 23：把计算结果 left 保存为左手柄位置；下一条判断或调用会读取这个具体值。
        locomotion.leftHand = left;
        // 步骤 24：把计算结果 right 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        locomotion.rightHand = right;
        // 步骤 25：把计算结果 -4.3f 保存为地面基准高度；下一条判断或调用会读取这个具体值。
        locomotion.groundHeight = -4.3f;
        // 步骤 26：把计算结果 90f 保存为场景可活动半径；下一条判断或调用会读取这个具体值。
        locomotion.playRadius = 90f;
        // 步骤 27：执行 rig.AddComponent<CapeEnergySystem>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rig.AddComponent<CapeEnergySystem>();
        // 步骤 28：计算 rig.AddComponent<CapeFlightController>()，把结果存进 flight（flight）；之后引用 flight 时用的就是这次结果。
        CapeFlightController flight = rig.AddComponent<CapeFlightController>();
        // 步骤 29：把计算结果 AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>( 保存为actions；下一条判断或调用会读取这个具体值。
        flight.actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
            // 步骤 30：执行 "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");
        // 步骤 31：执行 rig.AddComponent<CapeEnergyUI>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rig.AddComponent<CapeEnergyUI>();
        // 步骤 32：执行 rig.AddComponent<CapeFlightFeedback>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rig.AddComponent<CapeFlightFeedback>();
        // 步骤 33：执行 rig.AddComponent<CapeRechargeReceiver>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rig.AddComponent<CapeRechargeReceiver>();
        // 步骤 34：执行 rig.AddComponent<CapeFlightCourseRuntime>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rig.AddComponent<CapeFlightCourseRuntime>();

        // 步骤 35：这里的 ! 表示取反；当 AssetDatabase.IsValidFolder(MaterialFolder) 不成立时才进入此分支。
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            // 步骤 36：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateFolder("Assets/SkyQuest", "FlightMaterials");
        // 步骤 37：创建新的 Material 并存为 stone；构造参数决定其初始内容或对象名称。
        Material stone = GetMaterial("Training stone", new Color(0.20f, 0.35f, 0.43f));
        // 步骤 38：创建新的 Material 并存为 safe；构造参数决定其初始内容或对象名称。
        Material safe = GetMaterial("Safety floor", new Color(0.07f, 0.17f, 0.20f));
        // 步骤 39：创建新的 Material 并存为 light；构造参数决定其初始内容或对象名称。
        Material light = GetMaterial("Recharge gold", new Color(1f, 0.80f, 0.20f));
        // 步骤 40：创建新的 Material 并存为 goal；构造参数决定其初始内容或对象名称。
        Material goal = GetMaterial("Goal cyan", new Color(0.15f, 0.92f, 0.72f));
        // 步骤 41：创建新的 GameObject 并存为 root；构造参数决定其初始内容或对象名称。
        GameObject root = new GameObject("Cape flight training course");
        // 步骤 42：调用 Platform：Platform。
        Platform(root.transform, "1 Starting platform", new Vector3(0f, -0.30f, 0f), new Vector3(6f, 0.6f, 6f), stone);
        // 步骤 43：调用 Platform：Platform。
        Platform(root.transform, "2 Flight training A", new Vector3(0f, -0.10f, 6f), new Vector3(4f, 0.6f, 4f), stone);
        // 步骤 44：调用 Platform：Platform。
        Platform(root.transform, "2 Flight training B", new Vector3(1f, 0.25f, 12f), new Vector3(4f, 0.6f, 4f), stone);
        // 步骤 45：调用 Platform：Platform。
        Platform(root.transform, "3 Ground recharge platform", new Vector3(1f, 0.50f, 18f), new Vector3(5f, 0.6f, 5f), stone);
        // 步骤 46：调用 Platform：Platform。
        Platform(root.transform, "4 Planning platform A", new Vector3(-1f, 0.70f, 25f), new Vector3(4f, 0.6f, 4f), stone);
        // 步骤 47：调用 Platform：Platform。
        Platform(root.transform, "4 Planning platform B", new Vector3(1f, 0.85f, 31f), new Vector3(4f, 0.6f, 4f), stone);
        // 步骤 48：调用 Platform：Platform。
        Platform(root.transform, "5 Final destination", new Vector3(0f, 1.05f, 38f), new Vector3(6f, 0.6f, 6f), goal);
        // 步骤 49：调用 Platform：Platform。
        Platform(root.transform, "Recovery floor", new Vector3(0f, -4.60f, 18f), new Vector3(26f, 0.6f, 50f), safe);
        // 步骤 50：调用 Recharge：Recharge。
        Recharge(root.transform, "Air light source", new Vector3(0f, 2.2f, 10f), 2f, false, light);
        // 步骤 51：调用 Recharge：Recharge。
        Recharge(root.transform, "Ground recharge", new Vector3(1f, 1.35f, 18f), 1f, true, light);
        // 步骤 52：调用 Recharge：Recharge。
        Recharge(root.transform, "Challenge light source", new Vector3(0f, 2.1f, 28f), 2f, false, light);
        // 步骤 53：计算 GameObject.CreatePrimitive(PrimitiveType.Sphere)，把结果存进 pickup（pickup）；之后引用 pickup 时用的就是这次结果。
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // 步骤 54：把计算结果 "Instant recharge +2" 保存为name；下一条判断或调用会读取这个具体值。
        pickup.name = "Instant recharge +2";
        // 步骤 55：执行 pickup.transform.SetParent(root.transform);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        pickup.transform.SetParent(root.transform);
        // 步骤 56：执行 pickup.transform.position = new Vector3(1f, 1.5f, 18f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        pickup.transform.position = new Vector3(1f, 1.5f, 18f);
        // 步骤 57：执行 pickup.transform.localScale = Vector3.one * 0.45f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        pickup.transform.localScale = Vector3.one * 0.45f;
        // 步骤 58：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        pickup.GetComponent<Renderer>().sharedMaterial = light;
        // 步骤 59：执行 pickup.AddComponent<CapeInstantRechargePickup>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        pickup.AddComponent<CapeInstantRechargePickup>();
        // 步骤 60：调用 Marker：Marker。
        Marker(root.transform, "Recharge checkpoint", new Vector3(1f, 1.25f, 18f),
            // 步骤 61：用 new 创建 Vector3(1f, 1.2f, 18f), false, light);；构造参数决定新对象的初始内容。
            new Vector3(1f, 1.2f, 18f), false, light);
        // 步骤 62：调用 Marker：Marker。
        Marker(root.transform, "Finish checkpoint", new Vector3(0f, 1.8f, 38f),
            // 步骤 63：用 new 创建 Vector3(0f, 1.85f, 38f), true, goal);；构造参数决定新对象的初始内容。
            new Vector3(0f, 1.85f, 38f), true, goal);
        // 步骤 64：调用 Sign：Sign。
        Sign(root.transform, "Controls", new Vector3(-1.1f, 1.55f, 2.5f),
            // 步骤 65：执行 "A: TAKE OFF / BOOST\nLEFT STICK: STEER\nRIGHT STICK: TURN\nR CLICK: SAFE STOP");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "A: TAKE OFF / BOOST\nLEFT STICK: STEER\nRIGHT STICK: TURN\nR CLICK: SAFE STOP");
        // 步骤 66：调用 Sign：Sign。
        Sign(root.transform, "Recharge instructions", new Vector3(-1.1f, 2.3f, 18f),
            // 步骤 67：执行 "GOLD LIGHT  RECHARGES CAPE\nLAND HERE TO RECHARGE SLOWLY");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "GOLD LIGHT  RECHARGES CAPE\nLAND HERE TO RECHARGE SLOWLY");
        // 步骤 68：调用 Sign：Sign。
        Sign(root.transform, "Finish instructions", new Vector3(0f, 2.8f, 38f),
            // 步骤 69：执行 "LAND HERE TO COMPLETE\nFALLS RETURN TO CHECKPOINT");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "LAND HERE TO COMPLETE\nFALLS RETURN TO CHECKPOINT");
        // 步骤 70：创建新的 GameObject 并存为 sun；构造参数决定其初始内容或对象名称。
        GameObject sun = new GameObject("Sun", typeof(Light));
        // 步骤 71：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        sun.GetComponent<Light>().type = LightType.Directional;
        // 步骤 72：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        sun.GetComponent<Light>().intensity = 1.1f;
        // 步骤 73：执行 sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        // 步骤 74：新建用于ambient灯光的对象或数值结构，并把构造参数保存进去。
        RenderSettings.ambientLight = new Color(0.45f, 0.55f, 0.65f);
        // 步骤 75：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 76：这里的 ! 表示取反；当 EditorSceneManager.SaveScene(scene, ScenePath) 不成立时才进入此分支。 成立时执行 throw new Exception("Could not save cape flight scene.");
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Could not save cape flight scene.");
        // 步骤 77：创建新的 List<EditorBuildSettingsScene> 并存为 buildScenes；构造参数决定其初始内容或对象名称。
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        // 步骤 78：这里的 ! 表示取反；当 buildScenes.Exists(item => item.path == ScenePath) 不成立时才进入此分支。
        if (!buildScenes.Exists(item => item.path == ScenePath))
        {
            // 步骤 79：把括号里的新元素加入 buildScenes 集合；后续会按集合顺序使用它。
            buildScenes.Add(new EditorBuildSettingsScene(ScenePath, false));
            // 步骤 80：把计算结果 buildScenes.ToArray() 保存为scenes；下一条判断或调用会读取这个具体值。
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }
        // 步骤 81：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Cape flight test level built at " + ScenePath);
    }
    // ！！！功能结束：Build！！！

    // ！！！功能：GetMaterial——获取材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Color color 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Material 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Material GetMaterial(string name, Color color)
    {
        // 步骤 1：计算 MaterialFolder + "/" + name + ".mat"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
        string path = MaterialFolder + "/" + name + ".mat";
        // 步骤 2：计算 AssetDatabase.LoadAssetAtPath<Material>(path)，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        // 步骤 3：确认material已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (material != null) return material;
        // 步骤 4：计算 Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")，把结果存进 shader（shader）；之后引用 shader 时用的就是这次结果。
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        // 步骤 5：新建用于material的对象或数值结构，并把构造参数保存进去。
        material = new Material(shader) { color = color };
        // 步骤 6：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.CreateAsset(material, path);
        // 步骤 7：把 material 作为方法结果返回给调用方；return 后本方法立即结束。
        return material;
    }
    // ！！！功能结束：GetMaterial！！！

    // ！！！功能：Platform——Platform！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform parent, string name, Vector3 center, Vector3 size, Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject Platform(Transform parent, string name, Vector3 center, Vector3 size, Material material)
    {
        // 步骤 1：计算 GameObject.CreatePrimitive(PrimitiveType.Cube)，把结果存进 obj（obj）；之后引用 obj 时用的就是这次结果。
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // 步骤 2：把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        obj.name = name;
        // 步骤 3：执行 obj.transform.SetParent(parent);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(parent);
        // 步骤 4：执行 obj.transform.position = center;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.position = center;
        // 步骤 5：执行 obj.transform.localScale = size;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.localScale = size;
        // 步骤 6：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        obj.GetComponent<Renderer>().sharedMaterial = material;
        // 步骤 7：把 obj 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj;
    }
    // ！！！功能结束：Platform！！！

    // ！！！功能：Recharge——Recharge！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform parent, string name, Vector3 center, float rate, bool groundOnly, Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Recharge(Transform parent, string name, Vector3 center, float rate, bool groundOnly, Material material)
    {
        // 步骤 1：计算 GameObject.CreatePrimitive(PrimitiveType.Sphere)，把结果存进 glow（glow）；之后引用 glow 时用的就是这次结果。
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // 步骤 2：把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        glow.name = name;
        // 步骤 3：执行 glow.transform.SetParent(parent);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        glow.transform.SetParent(parent);
        // 步骤 4：执行 glow.transform.position = center;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        glow.transform.position = center;
        // 步骤 5：执行 glow.transform.localScale = Vector3.one * 0.55f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        glow.transform.localScale = Vector3.one * 0.55f;
        // 步骤 6：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        glow.GetComponent<Renderer>().sharedMaterial = material;
        // 步骤 7：从目标物体找到所需组件并存为 trigger；使用前应检查是否为 null。
        SphereCollider trigger = glow.GetComponent<SphereCollider>();
        // 步骤 8：把 trigger.isTrigger 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        trigger.isTrigger = true;
        // 步骤 9：把计算结果 groundOnly ? 5f : 4f 保存为radius；下一条判断或调用会读取这个具体值。
        trigger.radius = groundOnly ? 5f : 4f;
        // 步骤 10：计算 glow.AddComponent<CapeRechargeZone>()，把结果存进 zone（zone）；之后引用 zone 时用的就是这次结果。
        CapeRechargeZone zone = glow.AddComponent<CapeRechargeZone>();
        // 步骤 11：把计算结果 rate 保存为rechargePerSecond；下一条判断或调用会读取这个具体值。
        zone.rechargePerSecond = rate;
        // 步骤 12：把计算结果 groundOnly 保存为groundOnly；下一条判断或调用会读取这个具体值。
        zone.groundOnly = groundOnly;
    }
    // ！！！功能结束：Recharge！！！

    // 继续填写上一行调用的参数：private static void Marker(Transform parent, string name, Vector3 center, Vector3 checkpoint；参数位置决定它表示的坐标、速度或选项。
    private static void Marker(Transform parent, string name, Vector3 center, Vector3 checkpoint,
        // 这一行补充 bool finish, Material material)，与相邻行合起来完成同一次计算或对象配置。
        bool finish, Material material)
    {
        // 计算 GameObject.CreatePrimitive(PrimitiveType.Cylinder)，把结果存进 marker（marker）；之后引用 marker 时用的就是这次结果。
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        // 把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        marker.name = name;
        // 执行 marker.transform.SetParent(parent);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        marker.transform.SetParent(parent);
        // 执行 marker.transform.position = center;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        marker.transform.position = center;
        // 执行 marker.transform.localScale = new Vector3(0.8f, 0.03f, 0.8f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        marker.transform.localScale = new Vector3(0.8f, 0.03f, 0.8f);
        // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        marker.GetComponent<Renderer>().sharedMaterial = material;
        // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
        // 计算 marker.AddComponent<BoxCollider>()，把结果存进 trigger（trigger）；之后引用 trigger 时用的就是这次结果。
        BoxCollider trigger = marker.AddComponent<BoxCollider>();
        // 把 trigger.isTrigger 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        trigger.isTrigger = true;
        // 新建用于size的对象或数值结构，并把构造参数保存进去。
        trigger.size = new Vector3(2f, 40f, 2f);
        // 计算 marker.AddComponent<CapeFlightCourseMarker>()，把结果存进 courseMarker（courseMarker）；之后引用 courseMarker 时用的就是这次结果。
        CapeFlightCourseMarker courseMarker = marker.AddComponent<CapeFlightCourseMarker>();
        // 把计算结果 finish 保存为finish；下一条判断或调用会读取这个具体值。
        courseMarker.finish = finish;
        // 把计算结果 checkpoint 保存为checkpoint位置；下一条判断或调用会读取这个具体值。
        courseMarker.checkpointPosition = checkpoint;
    }

    // ！！！功能：Sign——Sign！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform parent, string name, Vector3 position, string message 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Sign(Transform parent, string name, Vector3 position, string message)
    {
        // 步骤 1：创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name, typeof(TextMesh), typeof(MeshRenderer));
        // 步骤 2：执行 obj.transform.SetParent(parent);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(parent);
        // 步骤 3：执行 obj.transform.position = position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.position = position;
        // 步骤 4：从目标物体找到所需组件并存为 text；使用前应检查是否为 null。
        TextMesh text = obj.GetComponent<TextMesh>();
        // 步骤 5：把新内容写入 text.text；玩家看到的文字会在本帧更新。
        text.text = message;
        // 步骤 6：把计算结果 48 保存为fontSize；下一条判断或调用会读取这个具体值。
        text.fontSize = 48;
        // 步骤 7：把计算结果 0.0085f 保存为characterSize；下一条判断或调用会读取这个具体值。
        text.characterSize = 0.0085f;
        // 步骤 8：把计算结果 TextAnchor.MiddleCenter 保存为anchor；下一条判断或调用会读取这个具体值。
        text.anchor = TextAnchor.MiddleCenter;
        // 步骤 9：把计算结果 TextAlignment.Center 保存为alignment；下一条判断或调用会读取这个具体值。
        text.alignment = TextAlignment.Center;
        // 步骤 10：新建用于color的对象或数值结构，并把构造参数保存进去。
        text.color = new Color(0.08f, 0.16f, 0.20f);
        // 步骤 11：执行 obj.AddComponent<CapeFlightCourseSign>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.AddComponent<CapeFlightCourseSign>();
    }
    // ！！！功能结束：Sign！！！

    // ！！！功能：Find——查找！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform parent, string childName 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Transform Find(Transform parent, string childName)
    {
        // 步骤 1：检查条件 parent.name == childName；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (parent.name == childName) return parent;
        // 步骤 2：逐个读取 Transform child in parent 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform child in parent)
        {
            // 步骤 3：计算 Find(child, childName)，把结果存进 found（found）；之后引用 found 时用的就是这次结果。
            Transform found = Find(child, childName);
            // 步骤 4：确认found已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
            if (found != null) return found;
        }
        // 步骤 5：没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }
    // ！！！功能结束：Find！！！
}
