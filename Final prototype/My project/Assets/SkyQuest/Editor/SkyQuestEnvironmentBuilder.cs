// ！！！文件功能：在编辑器生成 IP1 场景、材质、建筑和角色摆放！！！
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
// 引入 UnityEditor.SceneManagement 命名空间；下方可直接使用编辑器内创建、打开、保存场景的接口。
using UnityEditor.SceneManagement;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.SceneManagement 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.SceneManagement;
// 引入 UnityEngine.Rendering 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.Rendering;

// Builds the same spatial extension into the desktop and Quest scenes.
// 定义 SkyQuestEnvironmentBuilder 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestEnvironmentBuilder
{
    // 定义固定文字 Quest（Quest）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string Quest = "Assets/SkyQuest/SkyQuest3S.unity";
    // 定义固定文字 Desktop（桌面）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string Desktop = "Assets/SkyQuest/SkyDesktop.unity";
    // 定义固定文字 MaterialFolder（材质文件夹）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string MaterialFolder = "Assets/SkyQuest/Environment/Materials";
    // 定义固定文字 TextureFolder（贴图文件夹）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string TextureFolder = "Assets/SkyQuest/Environment/Textures";
    // 定义固定文字 RootName（RootName）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string RootName = "Sky Sanctuary Extension";
    // 声明 root（root）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static Transform root;
    // 执行 private static readonly Dictionary<string, Material> palette = new Dictionary<string, Material>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private static readonly Dictionary<string, Material> palette = new Dictionary<string, Material>();
    // 创建新的 List<Transform> 并存为 beacons；构造参数决定其初始内容或对象名称。
    private static readonly List<Transform> beacons = new List<Transform>();
    // 声明 gate（gate）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static Transform gate;
    // 声明 reward（reward）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static Transform reward;

    // ！！！功能：BuildBoth——构建Both！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void BuildBoth()
    {
        // 步骤 1：调用 BuildScene：构建场景。
        BuildScene(Quest);
        // 步骤 2：调用 BuildScene：构建场景。
        BuildScene(Desktop);
        // 步骤 3：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Sky Sanctuary extension built in both scenes.");
    }
    // ！！！功能结束：BuildBoth！！！

    // ！！！功能：BuildScene——构建场景！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string scenePath 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void BuildScene(string scenePath)
    {
        // 步骤 1：计算 EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        // 步骤 2：计算 GameObject.Find(RootName)，把结果存进 old（old）；之后引用 old 时用的就是这次结果。
        GameObject old = GameObject.Find(RootName);
        // 步骤 3：确认old已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 DestroyImmediate：DestroyImmediate。
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        // 步骤 4：调用 Clear：Clear。
        palette.Clear();
        // 步骤 5：调用 Clear：Clear。
        beacons.Clear();
        // 步骤 6：调用 EnsurePalette：确保Palette。
        EnsurePalette();
        // Keep material detail visible in the open courtyards and prevent white clipping.
        // 步骤 7：从目标物体找到所需组件并存为 sun；使用前应检查是否为 null。
        Light sun = GameObject.Find("Directional Light")?.GetComponent<Light>();
        // 步骤 8：确认sun已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { sun.intensity = 1.05f; EditorUtility.SetDirty(sun); }
        if (sun != null) { sun.intensity = 1.05f; EditorUtility.SetDirty(sun); }
        // 步骤 9：把计算结果 0.8f 保存为ambientIntensity；下一条判断或调用会读取这个具体值。
        RenderSettings.ambientIntensity = 0.8f;
        // 步骤 10：新建用于root的对象或数值结构，并把构造参数保存进去。
        root = new GameObject(RootName).transform;
        // 步骤 11：调用 Courtyard：Courtyard。
        Courtyard();
        // 步骤 12：调用 Hall：Hall。
        Hall();
        // 步骤 13：调用 Labyrinth：Labyrinth。
        Labyrinth();
        // 步骤 14：调用 WestCloister：WestCloister。
        WestCloister();
        // 步骤 15：调用 GrandCourt：GrandCourt。
        GrandCourt();
        // 步骤 16：调用 EastAmphitheater：EastAmphitheater。
        EastAmphitheater();
        // 步骤 17：调用 CharacterGallery：CharacterGallery。
        CharacterGallery();
        // 步骤 18：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>()，把结果存进 magic（magic）；之后引用 magic 时用的就是这次结果。
        SkyQuestMagicExchange magic = UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        // 步骤 19：确认magic已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (magic != null)
        {
            // 步骤 20：计算 GameObject.Find("Shibiyama dancer")，把结果存进 dancer（dancer）；之后引用 dancer 时用的就是这次结果。
            GameObject dancer = GameObject.Find("Shibiyama dancer");
            // 步骤 21：计算 GameObject.Find("Sky prayer guardian")，把结果存进 guardian（guardian）；之后引用 guardian 时用的就是这次结果。
            GameObject guardian = GameObject.Find("Sky prayer guardian");
            // 步骤 22：检查dancer、guardian是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
            if (dancer == null || guardian == null)
                // 步骤 23：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                throw new Exception("A plaza Magic exchange ancestor is missing from the scene.");
            // 步骤 24：执行 magic.ancestors[0] = dancer.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            magic.ancestors[0] = dancer.transform;
            // 步骤 25：执行 magic.ancestors[1] = guardian.transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            magic.ancestors[1] = guardian.transform;
            // 步骤 26：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(magic);
        }
        // 步骤 27：调用 Landscape：Landscape。
        Landscape();
        // Imported source models left several standalone authoring lights in the scene.
        // Keep the main sun and the sanctuary's controlled accent lights only.
        // 步骤 28：逐个读取 Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            // 步骤 29：这里用 && 要求两边条件同时成立：light != sun && !light.transform.IsChildOf(root)；任意一边为假就跳过本分支。 成立时执行 light.enabled = false;
            if (light != sun && !light.transform.IsChildOf(root)) light.enabled = false;
        // 步骤 30：计算 root.gameObject.AddComponent<SkyQuestSanctuary>()，把结果存进 experience（experience）；之后引用 experience 时用的就是这次结果。
        SkyQuestSanctuary experience = root.gameObject.AddComponent<SkyQuestSanctuary>();
        // 步骤 31：把计算结果 beacons.ToArray() 保存为beacons；下一条判断或调用会读取这个具体值。
        experience.beacons = beacons.ToArray();
        // 步骤 32：把计算结果 gate 保存为gate；下一条判断或调用会读取这个具体值。
        experience.gate = gate;
        // 步骤 33：把计算结果 reward 保存为reward；下一条判断或调用会读取这个具体值。
        experience.reward = reward;
        // 步骤 34：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>()，把结果存进 locomotion（locomotion）；之后引用 locomotion 时用的就是这次结果。
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        // 步骤 35：确认locomotion已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { locomotion.playRadius = 130f; EditorUtility.SetDirty(locomotion); }
        if (locomotion != null) { locomotion.playRadius = 130f; EditorUtility.SetDirty(locomotion); }
        // 步骤 36：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 37：这里的 ! 表示取反；当 EditorSceneManager.SaveScene(scene) 不成立时才进入此分支。 成立时执行 throw new Exception("Could not save " + scenePath);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save " + scenePath);
        // 步骤 38：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 39：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Sky Sanctuary built: " + scenePath + "; beacons=" + beacons.Count +
            // 步骤 40：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            "; objects=" + root.GetComponentsInChildren<Transform>(true).Length);
    }
    // ！！！功能结束：BuildScene！！！

    // ！！！功能：EnsurePalette——确保Palette！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void EnsurePalette()
    {
        // 步骤 1：这里的 ! 表示取反；当 AssetDatabase.IsValidFolder("Assets/SkyQuest/Environment") 不成立时才进入此分支。
        if (!AssetDatabase.IsValidFolder("Assets/SkyQuest/Environment"))
            // 步骤 2：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateFolder("Assets/SkyQuest", "Environment");
        // 步骤 3：这里的 ! 表示取反；当 AssetDatabase.IsValidFolder(MaterialFolder) 不成立时才进入此分支。
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            // 步骤 4：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateFolder("Assets/SkyQuest/Environment", "Materials");
        // 步骤 5：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("WarmLimestone", new Color(0.72f, 0.66f, 0.56f), 0.15f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        // 步骤 6：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("Ivory", new Color(0.87f, 0.82f, 0.72f), 0.24f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        // 步骤 7：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("IvoryTile", new Color(0.60f, 0.57f, 0.51f), 0.17f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        // 步骤 8：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("MistTile", new Color(0.50f, 0.63f, 0.67f), 0.12f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        // 步骤 9：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("PaleGold", new Color(0.83f, 0.68f, 0.36f), 0.44f, 0f, "Metal_Detail.png", "Metal_Normal.jpg", 0.58f);
        // 步骤 10：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("DeepGold", new Color(0.54f, 0.39f, 0.21f), 0.37f, 0f, "Metal_Detail.png", "Metal_Normal.jpg", 0.5f);
        // 步骤 11：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("BlueStone", new Color(0.43f, 0.67f, 0.76f), 0.18f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        // 步骤 12：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("Slate", new Color(0.28f, 0.45f, 0.54f), 0.17f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        // 步骤 13：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("Mist", new Color(0.7f, 0.79f, 0.81f), 0.08f, 0f, "Cloud_Detail.png");
        // 步骤 14：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("Cloud", new Color(0.89f, 0.91f, 0.86f), 0.04f, 0f, "Cloud_Detail.png");
        // 步骤 15：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("Candle", new Color(1f, 0.72f, 0.29f), 0.1f, 1.5f);
        // 步骤 16：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("MoonLight", new Color(0.43f, 0.85f, 1f), 0.1f, 1.2f);
        // 步骤 17：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("Dormant", new Color(0.27f, 0.52f, 0.61f), 0.25f, 0.2f);
        // 步骤 18：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("Water", new Color(0.39f, 0.69f, 0.72f), 0.82f);
        // 步骤 19：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("Banner", new Color(0.76f, 0.88f, 0.84f), 0.12f, 0f, "Cloth_Detail.png", "Cloth_Normal.png");
        // 步骤 20：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("HoneyWood", new Color(0.87f, 0.77f, 0.64f), 0.29f, 0f, "Wood_Color.jpg", "Wood_Normal.jpg");
        // 步骤 21：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("IvoryBark", new Color(0.83f, 0.78f, 0.66f), 0.12f, 0f, "Bark_Detail.png", "Bark_Normal.png");
        // 步骤 22：调用 AddMaterial：为指定模型部分添加或替换材质。
        AddMaterial("LeafCanopy", new Color(0.68f, 0.82f, 0.70f), 0.08f, 0f, "Leaf_Detail.png", "Leaf_Normal.png");
    }
    // ！！！功能结束：EnsurePalette！！！

    // 继续填写上一行调用的参数：private static void AddMaterial(string name, Color color, float smoothness, float emission = 0f；参数位置决定它表示的坐标、速度或选项。
    private static void AddMaterial(string name, Color color, float smoothness, float emission = 0f,
        // 把计算结果 null, string normalMap = null, float metallic = 0f) 保存为colorMap；下一条判断或调用会读取这个具体值。
        string colorMap = null, string normalMap = null, float metallic = 0f)
    {
        // 计算 MaterialFolder + "/" + name + ".mat"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
        string path = MaterialFolder + "/" + name + ".mat";
        // 计算 AssetDatabase.LoadAssetAtPath<Material>(path)，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        // 检查material是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (material == null)
        {
            // 计算 Shader.Find("Universal Render Pipeline/Lit")，把结果存进 shader（shader）；之后引用 shader 时用的就是这次结果。
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            // 检查shader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Standard");
            // 新建用于material的对象或数值结构，并把构造参数保存进去。
            material = new Material(shader);
            // 调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateAsset(material, path);
        }
        // 把计算结果 color 保存为color；下一条判断或调用会读取这个具体值。
        material.color = color;
        // 检查条件 material.HasProperty("_BaseColor")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetColor：设置Color。
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        // 检查条件 material.HasProperty("_Smoothness")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetFloat：设置Float。
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        // 检查条件 material.HasProperty("_Metallic")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetFloat：设置Float。
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        // 确认colorMap已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (colorMap != null)
        {
            // 计算 AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/" + colorMap)，把结果存进 tex（tex）；之后引用 tex 时用的就是这次结果。
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/" + colorMap);
            // 检查tex是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("Missing environment texture: " + colorMap);
            if (tex == null) throw new Exception("Missing environment texture: " + colorMap);
            // 调用 SetTexture：设置贴图。
            material.SetTexture("_BaseMap", tex);
        }
        // 确认normalMap已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (normalMap != null)
        {
            // 计算 TextureFolder + "/" + normalMap，把结果存进 normalPath（normal路径）；之后引用 normalPath 时用的就是这次结果。
            string normalPath = TextureFolder + "/" + normalMap;
            // 计算 AssetImporter.GetAtPath(normalPath) as TextureImporter，把结果存进 importer（importer）；之后引用 importer 时用的就是这次结果。
            TextureImporter importer = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            // 检查importer是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("Missing environment normal map: " + normalMap);
            if (importer == null) throw new Exception("Missing environment normal map: " + normalMap);
            // 检查条件 importer.textureType != TextureImporterType.NormalMap；只有为真才运行下面的步骤，否则沿原流程继续。
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                // 把 TextureImporterType.NormalMap 写入textureType；TextMeshPro 会把新文字显示给玩家。
                importer.textureType = TextureImporterType.NormalMap;
                // 把计算结果 1024 保存为max贴图Size；下一条判断或调用会读取这个具体值。
                importer.maxTextureSize = 1024;
                // 调用 SaveAndReimport：保存和Reimport。
                importer.SaveAndReimport();
            }
            // 调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
            // 调用 EnableKeyword：EnableKeyword。
            material.EnableKeyword("_NORMALMAP");
            // 设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            material.SetFloat("_BumpScale", 0.4f);
        }
        // 确认colorMap已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (colorMap != null)
        {
            // 用三元表达式为 tiling 二选一：问号前条件为真取前值，为假取冒号后的值。
            Vector2 tiling = name == "HoneyWood" ? Vector2.one : new Vector2(2f, 2f);
            // 调用 SetTextureScale：设置贴图缩放。
            material.SetTextureScale("_BaseMap", tiling);
            // 确认normalMap已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetTextureScale：设置贴图缩放。
            if (normalMap != null) material.SetTextureScale("_BumpMap", tiling);
        }
        // 检查条件 emission > 0f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (emission > 0f)
        {
            // 调用 EnableKeyword：EnableKeyword。
            material.EnableKeyword("_EMISSION");
            // 设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
            material.SetColor("_EmissionColor", color * emission);
        }
        // 告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(material);
        // 执行 palette[name] = material;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        palette[name] = material;
    }

    // ！！！功能：Group——Group！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Transform Group(string name)
    {
        // 步骤 1：创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name);
        // 步骤 2：执行 obj.transform.SetParent(root, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(root, false);
        // 步骤 3：把 obj.transform 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj.transform;
    }
    // ！！！功能结束：Group！！！

    // 继续填写上一行调用的参数：private static GameObject Shape(string name, PrimitiveType type, Transform parent；参数位置决定它表示的坐标、速度或选项。
    private static GameObject Shape(string name, PrimitiveType type, Transform parent,
        // 继续填写上一行调用的参数：Vector3 position, Vector3 scale, string material, bool solid = true；参数位置决定它表示的坐标、速度或选项。
        Vector3 position, Vector3 scale, string material, bool solid = true,
        // 把计算结果 null) 保存为rotation；下一条判断或调用会读取这个具体值。
        Quaternion? rotation = null)
    {
        // 计算 GameObject.CreatePrimitive(type)，把结果存进 obj（obj）；之后引用 obj 时用的就是这次结果。
        GameObject obj = GameObject.CreatePrimitive(type);
        // 把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        obj.name = name;
        // 执行 obj.transform.SetParent(parent, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(parent, false);
        // 执行 obj.transform.position = position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.position = position;
        // 执行 obj.transform.localScale = scale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.localScale = scale;
        // 执行 obj.transform.rotation = rotation ?? Quaternion.identity;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.rotation = rotation ?? Quaternion.identity;
        // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        obj.GetComponent<Renderer>().sharedMaterial = palette[material];
        // 这里的 ! 表示取反；当 solid 不成立时才进入此分支。 成立时调用 DestroyImmediate：DestroyImmediate。
        if (!solid) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        // 调用 SetStaticEditorFlags：设置StaticEditorFlags。
        GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.BatchingStatic);
        // 把 obj 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj;
    }

    // 继续填写上一行调用的参数：private static GameObject Box(string name, Transform parent, float x, float y, float z；参数位置决定它表示的坐标、速度或选项。
    private static GameObject Box(string name, Transform parent, float x, float y, float z,
        // 这一行补充 float width, float height, float depth, string material, bool solid = true)，与相邻行合起来完成同一次计算或对象配置。
        float width, float height, float depth, string material, bool solid = true)
    {
        // 把 Shape(name, PrimitiveType.Cube, parent, new Vector3(x, y, z), 作为方法结果返回给调用方；return 后本方法立即结束。
        return Shape(name, PrimitiveType.Cube, parent, new Vector3(x, y, z),
            // 用 new 创建 Vector3(width, height, depth), material, solid);；构造参数决定新对象的初始内容。
            new Vector3(width, height, depth), material, solid);
    }

    // 继续填写上一行调用的参数：private static GameObject Pillar(string name, Transform parent, float x, float z；参数位置决定它表示的坐标、速度或选项。
    private static GameObject Pillar(string name, Transform parent, float x, float z,
        // 这一行补充 float yBase, float height, float radius, string material, bool solid = true)，与相邻行合起来完成同一次计算或对象配置。
        float yBase, float height, float radius, string material, bool solid = true)
    {
        // 把 Shape(name, PrimitiveType.Cylinder, parent, 作为方法结果返回给调用方；return 后本方法立即结束。
        return Shape(name, PrimitiveType.Cylinder, parent,
            // 用 new 创建 Vector3(x, yBase + height * 0.5f, z),；构造参数决定新对象的初始内容。
            new Vector3(x, yBase + height * 0.5f, z),
            // 用 new 创建 Vector3(radius * 2f, height * 0.5f, radius * 2f), material, solid);；构造参数决定新对象的初始内容。
            new Vector3(radius * 2f, height * 0.5f, radius * 2f), material, solid);
    }

    // 继续填写上一行调用的参数：private static GameObject Orb(string name, Transform parent, Vector3 position；参数位置决定它表示的坐标、速度或选项。
    private static GameObject Orb(string name, Transform parent, Vector3 position,
        // 这一行补充 Vector3 diameter, string material, bool solid = false)，与相邻行合起来完成同一次计算或对象配置。
        Vector3 diameter, string material, bool solid = false)
    {
        // 把 Shape(name, PrimitiveType.Sphere, parent, position, diameter, material, solid) 作为方法结果返回给调用方；return 后本方法立即结束。
        return Shape(name, PrimitiveType.Sphere, parent, position, diameter, material, solid);
    }

    // 继续填写上一行调用的参数：private static void Beam(string name, Transform parent, Vector3 a, Vector3 b；参数位置决定它表示的坐标、速度或选项。
    private static void Beam(string name, Transform parent, Vector3 a, Vector3 b,
        // 这一行补充 float width, string material)，与相邻行合起来完成同一次计算或对象配置。
        float width, string material)
    {
        // 计算 b - a，把结果存进 delta（delta）；之后引用 delta 时用的就是这次结果。
        Vector3 delta = b - a;
        // 调用 Shape：Shape。
        Shape(name, PrimitiveType.Cube, parent, (a + b) * 0.5f,
            // 用 new 创建 Vector3(width, width, delta.magnitude), material, false,；构造参数决定新对象的初始内容。
            new Vector3(width, width, delta.magnitude), material, false,
            // 调用 LookRotation：Look旋转。
            Quaternion.LookRotation(delta.normalized, Vector3.up));
    }

    // 继续填写上一行调用的参数：private static void Arch(Transform parent, string name, Vector3 center；参数位置决定它表示的坐标、速度或选项。
    private static void Arch(Transform parent, string name, Vector3 center,
        // 这一行补充 float radius, float baseHeight, bool alongX, string material)，与相邻行合起来完成同一次计算或对象配置。
        float radius, float baseHeight, bool alongX, string material)
    {
        // 按 int i = 0; i < 12; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 12; i++)
        {
            // 计算 Mathf.PI * i / 12f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = Mathf.PI * i / 12f;
            // 计算 Mathf.PI * (i + 1) / 12f，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
            float b = Mathf.PI * (i + 1) / 12f;
            // 创建新的 Vector3 并存为 p；构造参数决定其初始内容或对象名称。
            Vector3 p = new Vector3(-radius * Mathf.Cos(a), baseHeight + radius * Mathf.Sin(a), 0f);
            // 创建新的 Vector3 并存为 q；构造参数决定其初始内容或对象名称。
            Vector3 q = new Vector3(-radius * Mathf.Cos(b), baseHeight + radius * Mathf.Sin(b), 0f);
            // 这里的 ! 表示取反；当 alongX 不成立时才进入此分支。 成立时执行 { p = new Vector3(0f, p.y, p.x); q = new Vector3(0f, q.y, q.x); }
            if (!alongX) { p = new Vector3(0f, p.y, p.x); q = new Vector3(0f, q.y, q.x); }
            // 调用 Beam：Beam。
            Beam(name + " voussoir " + i, parent, center + p, center + q, 0.25f, material);
        }
    }

    // ！！！功能：Lamp——Lamp！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform parent, string name, float x, float z, float height 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Lamp(Transform parent, string name, float x, float z, float height)
    {
        // 步骤 1：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar(name + " plinth", parent, x, z, 0f, 0.32f, 0.35f, "BlueStone");
        // 步骤 2：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar(name + " stem", parent, x, z, 0.32f, height - 0.5f, 0.085f, "DeepGold", false);
        // 步骤 3：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
        Orb(name + " flame", parent, new Vector3(x, height, z),
            // 步骤 4：用 new 创建 Vector3(0.28f, 0.43f, 0.28f), "Candle");；构造参数决定新对象的初始内容。
            new Vector3(0.28f, 0.43f, 0.28f), "Candle");
        // 步骤 5：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
        Orb(name + " halo", parent, new Vector3(x, height + 0.12f, z),
            // 步骤 6：执行 Vector3.one * 0.52f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.one * 0.52f, "Ivory");
    }
    // ！！！功能结束：Lamp！！！

    // ！！！功能：Courtyard——Courtyard！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Courtyard()
    {
        // 步骤 1：计算 Group("01 - Arrival causeway and cloud garden")，把结果存进 p（p）；之后引用 p 时用的就是这次结果。
        Transform p = Group("01 - Arrival causeway and cloud garden");
        // 步骤 2：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Causeway limestone deck", p, 31f, 0.05f, 38f, 22f, 0.14f, 6.5f, "WarmLimestone");
        // 步骤 3：按 int i = 0; i < 10; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 10; i++)
        {
            // 步骤 4：计算 22.5f + i * 1.9f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 22.5f + i * 1.9f;
            // 步骤 5：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Inlaid path slab " + i, p, x, 0.135f, 38f, 1.6f, 0.025f, 4.5f,
                // 步骤 6：执行 i % 2 == 0 ? "IvoryTile" : "MistTile", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                i % 2 == 0 ? "IvoryTile" : "MistTile", false);
        }
        // 步骤 7：按 int side = -1; side <= 1; side += 2 变化索引；循环体依次处理每个位置。
        for (int side = -1; side <= 1; side += 2)
        {
            // 步骤 8：计算 38f + side * 3.1f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 38f + side * 3.1f;
            // 步骤 9：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Open sky railing", p, 31f, 0.5f, z, 22f, 0.2f, 0.2f, "Ivory");
            // 步骤 10：按 int i = 0; i <= 7; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i <= 7; i++)
            {
                // 步骤 11：计算 21f + i * 2.85f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
                float x = 21f + i * 2.85f;
                // 步骤 12：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Railing post", p, x, z, 0f, 0.95f, 0.16f, "WarmLimestone");
                // 步骤 13：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                Orb("Gold finial", p, new Vector3(x, 1.02f, z), Vector3.one * 0.18f, "PaleGold");
                // 步骤 14：检查条件 i % 2 == 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Lamp：Lamp。
                if (i % 2 == 0) Lamp(p, "Way lantern", x, z + side * 0.5f, 1.75f);
            }
        }
        // 步骤 15：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Arrival medallion", p, 39.5f, 38f, 0.08f, 0.03f, 2.3f, "PaleGold", false);
        // 步骤 16：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Arrival inner circle", p, 39.5f, 38f, 0.115f, 0.02f, 1.75f, "IvoryTile", false);
        // 步骤 17：按 int i = 0; i < 12; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 12; i++)
        {
            // 步骤 18：计算 i * Mathf.PI / 6f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * Mathf.PI / 6f;
            // 步骤 19：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Compass star", p, new Vector3(39.5f + Mathf.Sin(a) * 1.45f, 0.14f,
                // 步骤 20：执行 38f + Mathf.Cos(a) * 1.45f), Vector3.one * 0.12f, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                38f + Mathf.Cos(a) * 1.45f), Vector3.one * 0.12f, "PaleGold");
        }
        // 步骤 21：按 int i = 0; i < 16; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 16; i++)
        {
            // 步骤 22：计算 i * 2.4f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * 2.4f;
            // 步骤 23：计算 25f + (i % 4) * 3.9f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 25f + (i % 4) * 3.9f;
            // 步骤 24：用三元表达式为 z 二选一：问号前条件为真取前值，为假取冒号后的值。
            float z = i % 2 == 0 ? 44f + (i / 4) * 1.8f : 32f - (i / 4) * 1.8f;
            // 步骤 25：调用 CloudCluster：CloudCluster。
            CloudCluster(p, x, -0.35f - (i % 3) * 0.1f, z, 1f + (i % 4) * 0.2f, i);
        }
    }
    // ！！！功能结束：Courtyard！！！

    // ！！！功能：Hall——Hall！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Hall()
    {
        // 步骤 1：计算 Group("02 - Interior Hall of Flight")，把结果存进 p（p）；之后引用 p 时用的就是这次结果。
        Transform p = Group("02 - Interior Hall of Flight");
        // 步骤 2：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Polished sanctuary floor", p, 12f, 0.06f, 38f, 20.5f, 0.13f, 24f, "WarmLimestone");
        // 步骤 3：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("East entrance north wing", p, 22.3f, 3.3f, 45.1f, 0.55f, 6.6f, 8.1f, "Ivory");
        // 步骤 4：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("East entrance south wing", p, 22.3f, 3.3f, 30.9f, 0.55f, 6.6f, 8.1f, "Ivory");
        // 步骤 5：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Entrance lintel", p, 22.3f, 7.5f, 38f, 0.7f, 1.5f, 6.8f, "Ivory");
        // 步骤 6：调用 Arch：Arch。
        Arch(p, "East doorway arch", new Vector3(22.62f, 0f, 38f), 3.25f, 3.95f, false, "PaleGold");
        // 步骤 7：按 int i = 0; i < 2; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 2; i++)
        {
            // 步骤 8：用三元表达式为 z 二选一：问号前条件为真取前值，为假取冒号后的值。
            float z = i == 0 ? 30.6f : 45.4f;
            // 步骤 9：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Entrance azure relief", p, 22.64f, 3.45f, z, 0.07f, 3.2f, 3.5f,
                // 步骤 10：执行 "BlueStone", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "BlueStone", false);
            // 步骤 11：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Relief top gilding", p, 22.7f, 5.12f, z, 0.12f, 0.13f, 3.7f,
                // 步骤 12：执行 "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "PaleGold", false);
            // 步骤 13：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Relief foot gilding", p, 22.7f, 1.78f, z, 0.12f, 0.13f, 3.7f,
                // 步骤 14：执行 "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "PaleGold", false);
            // 步骤 15：按 int side = -1; side <= 1; side += 2 变化索引；循环体依次处理每个位置。
            for (int side = -1; side <= 1; side += 2)
            {
                // 步骤 16：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
                Box("Relief side gilding", p, 22.7f, 3.45f, z + side * 1.8f,
                    // 步骤 17：执行 0.12f, 3.45f, 0.1f, "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0.12f, 3.45f, 0.1f, "PaleGold", false);
                // 步骤 18：调用 Beam：Beam。
                Beam("Relief wing", p,
                    // 步骤 19：用 new 创建 Vector3(22.78f, 3.2f, z),；构造参数决定新对象的初始内容。
                    new Vector3(22.78f, 3.2f, z),
                    // 步骤 20：用 new 创建 Vector3(22.78f, 3.8f, z + side * 1.2f),；构造参数决定新对象的初始内容。
                    new Vector3(22.78f, 3.8f, z + side * 1.2f),
                    // 步骤 21：执行 0.12f, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0.12f, "PaleGold");
            }
            // 步骤 22：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Relief moon", p, new Vector3(22.82f, 3.85f, z),
                // 步骤 23：用 new 创建 Vector3(0.16f, 0.5f, 0.5f), "Candle");；构造参数决定新对象的初始内容。
                new Vector3(0.16f, 0.5f, 0.5f), "Candle");
        }
        // 步骤 24：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
        Orb("Entrance crown star", p, new Vector3(22.8f, 7.8f, 38f),
            // 步骤 25：用 new 创建 Vector3(0.16f, 0.62f, 0.62f), "Candle");；构造参数决定新对象的初始内容。
            new Vector3(0.16f, 0.62f, 0.62f), "Candle");
        // 步骤 26：按 int side = -1; side <= 1; side += 2 变化索引；循环体依次处理每个位置。
        for (int side = -1; side <= 1; side += 2)
        {
            // 步骤 27：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = side < 0 ? 2f : 22f;
            // 步骤 28：按 int bay = 0; bay < 5; bay++ 变化索引；循环体依次处理每个位置。
            for (int bay = 0; bay < 5; bay++)
            {
                // 步骤 29：计算 28f + bay * 5f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
                float z = 28f + bay * 5f;
                // 步骤 30：这里用 || 表示任一条件成立就处理：bay == 1 || bay == 2；用于合并几种停止或跳过原因。 成立时执行 continue;
                if (bay == 1 || bay == 2) continue;
                // 步骤 31：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
                Box("Window parapet", p, x, 0.7f, z, 0.55f, 1.4f, 4.7f, "Ivory");
                // 步骤 32：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
                Box("Window crown", p, x, 6.1f, z, 0.65f, 1.3f, 4.7f, "Ivory");
                // 步骤 33：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
                Box("Window tracer", p, x, 3.7f, z, 0.08f, 4.2f, 0.1f, "PaleGold", false);
                // 步骤 34：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                Orb("Window jewel", p, new Vector3(x + (side < 0 ? 0.32f : -0.32f), 4.9f, z),
                    // 步骤 35：执行 Vector3.one * 0.22f, "MoonLight");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    Vector3.one * 0.22f, "MoonLight");
            }
            // 步骤 36：按 int i = 0; i <= 6; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i <= 6; i++)
            {
                // 步骤 37：计算 26.5f + i * 3.8f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
                float z = 26.5f + i * 3.8f;
                // 步骤 38：这里用 && 要求两边条件同时成立：z > 34f && z < 42f；任意一边为假就跳过本分支。 成立时执行 continue;
                if (z > 34f && z < 42f) continue;
                // 步骤 39：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Fluted wall column", p, x, z, 0f, 6.8f, 0.23f, "Ivory");
                // 步骤 40：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Column capital", p, x, z, 6.5f, 0.4f, 0.38f, "PaleGold", false);
            }
        }
        // 步骤 41：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Southern mural wall", p, 12f, 3.2f, 26.4f, 20.5f, 6.4f, 0.65f, "Ivory");
        // 步骤 42：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("North threshold left", p, 6.7f, 3.15f, 49.5f, 9.9f, 6.3f, 0.6f, "Ivory");
        // 步骤 43：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("North threshold right", p, 18.3f, 3.15f, 49.5f, 7.8f, 6.3f, 0.6f, "Ivory");
        // 步骤 44：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("North threshold crown", p, 12.5f, 7.6f, 49.5f, 20f, 1.7f, 0.75f, "Ivory");
        // 步骤 45：调用 Arch：Arch。
        Arch(p, "North portal arch", new Vector3(12.5f, 0f, 49.12f), 2.65f, 3.8f, true, "PaleGold");
        // 步骤 46：按 int i = 0; i < 5; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 5; i++)
        {
            // 步骤 47：计算 28f + i * 5f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 28f + i * 5f;
            // 步骤 48：按 int side = -1; side <= 1; side += 2 变化索引；循环体依次处理每个位置。
            for (int side = -1; side <= 1; side += 2)
            {
                // 步骤 49：计算 12f + side * 7.2f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
                float x = 12f + side * 7.2f;
                // 步骤 50：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Nave column", p, x, z, 0f, 6.8f, 0.36f, "Ivory");
                // 步骤 51：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Column footing", p, x, z, 0f, 0.3f, 0.53f, "BlueStone");
                // 步骤 52：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Column gilded capital", p, x, z, 6.5f, 0.4f, 0.52f, "PaleGold", false);
                // 步骤 53：检查条件 i % 2 == 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Lamp：Lamp。
                if (i % 2 == 0) Lamp(p, "Aisle candle", x + side * 1.1f, z + 1.3f, 1.6f);
            }
            // 步骤 54：调用 VaultRib：VaultRib。
            VaultRib(p, z);
        }
        // Split ceiling leaves a luminous central skylight; the hall remains clearly indoors.
        // 步骤 55：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("West vault ceiling", p, 4.2f, 8.2f, 38f, 4.8f, 0.4f, 23.7f, "Ivory", false);
        // 步骤 56：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("East vault ceiling", p, 19.5f, 8.2f, 38f, 5.5f, 0.4f, 23.7f, "Ivory", false);
        // 步骤 57：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("South ceiling beam", p, 12f, 8.2f, 29f, 11f, 0.36f, 5.5f, "Ivory", false);
        // 步骤 58：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("North ceiling beam", p, 12f, 8.2f, 46.2f, 11f, 0.36f, 7.3f, "Ivory", false);
        // 步骤 59：按 int i = 0; i < 9; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 9; i++)
        {
            // 步骤 60：计算 28f + i * 2.5f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 28f + i * 2.5f;
            // 步骤 61：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Floor gold seam", p, 12f, 0.143f, z, 17.5f, 0.016f, 0.045f, "PaleGold", false);
        }
        // 步骤 62：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Reflecting pool rim", p, 12f, 38f, 0.13f, 0.17f, 2.45f, "PaleGold", false);
        // 步骤 63：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Reflecting pool water", p, 12f, 38f, 0.27f, 0.015f, 2.1f, "Water", false);
        // 步骤 64：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
        Orb("Light over the pool", p, new Vector3(12f, 2.5f, 38f),
            // 步骤 65：用 new 创建 Vector3(0.65f, 0.65f, 0.65f), "MoonLight");；构造参数决定新对象的初始内容。
            new Vector3(0.65f, 0.65f, 0.65f), "MoonLight");
        // 步骤 66：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++)
        {
            // 步骤 67：计算 i * Mathf.PI * 0.25f，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
            float angle = i * Mathf.PI * 0.25f;
            // 步骤 68：计算 12f + Mathf.Cos(angle) * 3.4f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 12f + Mathf.Cos(angle) * 3.4f;
            // 步骤 69：计算 38f + Mathf.Sin(angle) * 3.4f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 38f + Mathf.Sin(angle) * 3.4f;
            // 步骤 70：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Pool constellation", p, new Vector3(x, 0.19f, z), Vector3.one * 0.11f, "Candle");
        }
        // 步骤 71：按 int i = 0; i < 6; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 6; i++)
        {
            // 步骤 72：计算 30f + i * 3.1f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 30f + i * 3.1f;
            // 步骤 73：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Hanging silk banner", p, 6.7f, 5.45f, z, 0.04f, 2.4f, 0.7f, "Banner", false);
            // 步骤 74：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Hanging silk banner", p, 17.3f, 5.45f, z, 0.04f, 2.4f, 0.7f, "Banner", false);
        }
        // 步骤 75：调用 AddLight：添加灯光。
        AddLight(p, "Pool glow", new Vector3(12f, 5.3f, 38f), new Color(0.55f, 0.86f, 1f), 2.1f, 13f);
        // 步骤 76：调用 AddLight：添加灯光。
        AddLight(p, "Portal glow", new Vector3(21f, 4f, 38f), new Color(1f, 0.72f, 0.39f), 1.5f, 8f);
    }
    // ！！！功能结束：Hall！！！

    // ！！！功能：VaultRib——VaultRib！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform p, float z 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void VaultRib(Transform p, float z)
    {
        // 步骤 1：按 int i = 0; i < 12; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 12; i++)
        {
            // 步骤 2：计算 Mathf.PI * i / 12f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = Mathf.PI * i / 12f;
            // 步骤 3：计算 Mathf.PI * (i + 1) / 12f，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
            float b = Mathf.PI * (i + 1) / 12f;
            // 步骤 4：创建新的 Vector3 并存为 v；构造参数决定其初始内容或对象名称。
            Vector3 v = new Vector3(12f - 8.5f * Mathf.Cos(a), 6.9f + 1.1f * Mathf.Sin(a), z);
            // 步骤 5：创建新的 Vector3 并存为 w；构造参数决定其初始内容或对象名称。
            Vector3 w = new Vector3(12f - 8.5f * Mathf.Cos(b), 6.9f + 1.1f * Mathf.Sin(b), z);
            // 步骤 6：调用 Beam：Beam。
            Beam("Vault rib", p, v, w, 0.18f, "PaleGold");
        }
    }
    // ！！！功能结束：VaultRib！！！

    // ！！！功能：Labyrinth——Labyrinth！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Labyrinth()
    {
        // 步骤 1：计算 Group("03 - Garden of four lights and obstacle route")，把结果存进 p（p）；之后引用 p 时用的就是这次结果。
        Transform p = Group("03 - Garden of four lights and obstacle route");
        // 步骤 2：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Garden limestone base", p, 13f, 0.055f, 60f, 27f, 0.12f, 22f, "BlueStone");
        // 步骤 3：按 int i = 0; i < 10; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 10; i++)
            // 步骤 4：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Garden paving band", p, 13f, 0.125f, 50.8f + i * 2.1f,
                // 步骤 5：执行 25f, 0.02f, 0.09f, "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                25f, 0.02f, 0.09f, "PaleGold", false);
        // Perimeter walls and alternating solid barriers are navigable on foot.
        // 步骤 6：按 int side = -1; side <= 1; side += 2 变化索引；循环体依次处理每个位置。
        for (int side = -1; side <= 1; side += 2)
        {
            // 步骤 7：计算 13f + side * 13.4f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 13f + side * 13.4f;
            // 步骤 8：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Garden outer wall", p, x, 1.6f, 59.7f, 0.55f, 3.2f, 19.5f, "Ivory");
            // 步骤 9：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Garden outer cornice", p, x, 3.25f, 59.7f, 0.9f, 0.25f, 19.5f, "PaleGold", false);
            // 步骤 10：按 int i = 0; i < 6; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < 6; i++)
                // 步骤 11：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Garden wall buttress", p, x, 50.7f + i * 3.5f, 0f, 3.5f,
                    // 步骤 12：执行 0.28f, "WarmLimestone");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0.28f, "WarmLimestone");
        }
        // 步骤 13：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Garden south wall left", p, 5f, 1.6f, 50.2f, 10f, 3.2f, 0.55f, "Ivory");
        // 步骤 14：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Garden south wall right", p, 21f, 1.6f, 50.2f, 10f, 3.2f, 0.55f, "Ivory");
        // 步骤 15：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Garden far wall left", p, 5f, 1.6f, 69.8f, 10f, 3.2f, 0.55f, "Ivory");
        // 步骤 16：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Garden far wall right", p, 21f, 1.6f, 69.8f, 10f, 3.2f, 0.55f, "Ivory");
        // 步骤 17：按 int row = 0; row < 4; row++ 变化索引；循环体依次处理每个位置。
        for (int row = 0; row < 4; row++)
        {
            // 步骤 18：计算 54f + row * 4f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 54f + row * 4f;
            // 步骤 19：用 eastGap 记住此处真假结果（row % 2 == 0），供下一条 if 选择执行路径。
            bool eastGap = row % 2 == 0;
            // 步骤 20：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = eastGap ? 10.5f : 15.5f;
            // 步骤 21：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Alternating obstacle wall " + row, p, x, 1.6f, z,
                // 步骤 22：执行 21f, 3.2f, 0.7f, row % 2 == 0 ? "WarmLimestone" : "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                21f, 3.2f, 0.7f, row % 2 == 0 ? "WarmLimestone" : "Ivory");
            // 步骤 23：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Obstacle gilded top", p, x, 3.27f, z, 21f, 0.18f, 0.9f, "PaleGold", false);
            // 步骤 24：按 int j = 0; j < 4; j++ 变化索引；循环体依次处理每个位置。
            for (int j = 0; j < 4; j++)
            {
                // 步骤 25：计算 x - 8f + j * 5.3f，把结果存进 px（px）；之后引用 px 时用的就是这次结果。
                float px = x - 8f + j * 5.3f;
                // 步骤 26：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Obstacle buttress", p, px, z, 0f, 3.55f, 0.29f, "BlueStone");
                // 步骤 27：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                Orb("Wall candle", p, new Vector3(px, 3.65f, z),
                    // 步骤 28：执行 Vector3.one * 0.17f, "Candle");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    Vector3.one * 0.17f, "Candle");
            }
            // 步骤 29：用三元表达式为 beaconX 二选一：问号前条件为真取前值，为假取冒号后的值。
            float beaconX = eastGap ? 23f : 3f;
            // 步骤 30：计算 z - 1.25f，把结果存进 beaconZ（beaconz）；之后引用 beaconZ 时用的就是这次结果。
            float beaconZ = z - 1.25f;
            // 步骤 31：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Beacon altar", p, beaconX, beaconZ, 0f, 1.15f, 0.55f, "Ivory");
            // 步骤 32：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Beacon gold ring", p, beaconX, beaconZ, 1.1f, 0.1f, 0.64f,
                // 步骤 33：执行 "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "PaleGold", false);
            // 步骤 34：把计算结果 Orb("Awakenable light " + (row + 1), p, 保存为light；下一条判断或调用会读取这个具体值。
            Transform light = Orb("Awakenable light " + (row + 1), p,
                // 步骤 35：用 new 创建 Vector3(beaconX, 1.45f, beaconZ), Vector3.one * 0.45f,；构造参数决定新对象的初始内容。
                new Vector3(beaconX, 1.45f, beaconZ), Vector3.one * 0.45f,
                // 步骤 36：执行 "Dormant").transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Dormant").transform;
            // 步骤 37：把括号里的新元素加入 beacons 集合；后续会按集合顺序使用它。
            beacons.Add(light);
        }
        // Small solid stones add depth without removing the clear 4.3 m walking lane.
        // 步骤 38：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++)
        {
            // 步骤 39：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = i % 2 == 0 ? 8f : 18f;
            // 步骤 40：计算 51.5f + i * 2.25f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 51.5f + i * 2.25f;
            // 步骤 41：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Garden sculpted stone", p, x, z, 0f, 0.65f, 0.34f, "WarmLimestone");
            // 步骤 42：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Sculpted stone crown", p, new Vector3(x, 0.8f, z),
                // 步骤 43：执行 Vector3.one * 0.3f, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * 0.3f, "PaleGold");
        }
        // 步骤 44：按 int x = 2; x <= 24; x += 4 变化索引；循环体依次处理每个位置。
        for (int x = 2; x <= 24; x += 4)
            // 步骤 45：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Garden last floor ray", p, x, 0.129f, 68.25f,
                // 步骤 46：执行 0.12f, 0.02f, 1.3f, "Candle", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.12f, 0.02f, 1.3f, "Candle", false);
        // 步骤 47：把计算结果 Box("Sanctuary door", p, 13f, 2.05f, 69.8f, 5.55f, 4.1f, 0.48f, 保存为gate；下一条判断或调用会读取这个具体值。
        gate = Box("Sanctuary door", p, 13f, 2.05f, 69.8f, 5.55f, 4.1f, 0.48f,
            // 步骤 48：执行 "BlueStone").transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "BlueStone").transform;
        // 步骤 49：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++)
        {
            // 步骤 50：计算 i * Mathf.PI / 4f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * Mathf.PI / 4f;
            // 步骤 51：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Door constellation", gate,
                // 步骤 52：继续填写上一行调用的参数：gate.position + new Vector3(Mathf.Cos(a) * 1.2f, Mathf.Sin(a) * 1.2f, -0.32f)；参数位置决定它表示的坐标、速度或选项。
                gate.position + new Vector3(Mathf.Cos(a) * 1.2f, Mathf.Sin(a) * 1.2f, -0.32f),
                // 步骤 53：执行 Vector3.one * 0.18f, "Candle");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * 0.18f, "Candle");
        }
        // 步骤 54：逐个读取 Transform part in gate.GetComponentsInChildren<Transform>() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform part in gate.GetComponentsInChildren<Transform>())
            // 步骤 55：调用 SetStaticEditorFlags：设置StaticEditorFlags。
            GameObjectUtility.SetStaticEditorFlags(part.gameObject, 0);
        // 步骤 56：调用 Arch：Arch。
        Arch(p, "Garden crown", new Vector3(13f, 0f, 69.75f), 3f, 3.7f, true, "PaleGold");
        // 步骤 57：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Reward platform", p, 13f, 0.19f, 73f, 6f, 0.38f, 5f, "Ivory");
        // 步骤 58：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Reward gold disc", p, 13f, 73f, 0.38f, 0.035f, 1.5f, "PaleGold", false);
        // 步骤 59：新建用于reward的对象或数值结构，并把构造参数保存进去。
        reward = Orb("Sanctuary completion star", p, new Vector3(13f, 2.15f, 73f),
            // 步骤 60：执行 Vector3.one * 0.95f, "Candle").transform;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.one * 0.95f, "Candle").transform;
        // 步骤 61：调用 SetStaticEditorFlags：设置StaticEditorFlags。
        GameObjectUtility.SetStaticEditorFlags(reward.gameObject, 0);
        // 步骤 62：调用 AddLight：添加灯光。
        AddLight(p, "Final arch glow", new Vector3(13f, 5f, 69f),
            // 步骤 63：用 new 创建 Color(1f, 0.75f, 0.43f), 1.8f, 8f);；构造参数决定新对象的初始内容。
            new Color(1f, 0.75f, 0.43f), 1.8f, 8f);
    }
    // ！！！功能结束：Labyrinth！！！

    // ！！！功能：WestCloister——WestCloister！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void WestCloister()
    {
        // 步骤 1：计算 Group("05 - West cloister and five alcoves")，把结果存进 p（p）；之后引用 p 时用的就是这次结果。
        Transform p = Group("05 - West cloister and five alcoves");
        // 步骤 2：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("West cloister floor", p, -13f, 0.055f, 38f, 30f, 0.12f, 26f, "WarmLimestone");
        // Individual static inlays give the long hall a readable scale in VR.
        // 步骤 3：按 int ix = 0; ix < 15; ix++ 变化索引；循环体依次处理每个位置。
        for (int ix = 0; ix < 15; ix++)
            // 步骤 4：按 int iz = 0; iz < 13; iz++ 变化索引；循环体依次处理每个位置。
            for (int iz = 0; iz < 13; iz++)
            {
                // 步骤 5：计算 -27f + ix * 2f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
                float x = -27f + ix * 2f;
                // 步骤 6：计算 26f + iz * 2f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
                float z = 26f + iz * 2f;
                // 步骤 7：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
                Box("Cloister geometric floor tile", p, x, 0.127f, z, 1.8f, 0.014f, 1.8f,
                    // 步骤 8：执行 (ix + iz) % 4 == 0 ? "MistTile" : "IvoryTile", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    (ix + iz) % 4 == 0 ? "MistTile" : "IvoryTile", false);
                // 步骤 9：检查条件 (ix + iz) % 5 == 0；只有为真才运行下面的步骤，否则沿原流程继续。
                if ((ix + iz) % 5 == 0)
                    // 步骤 10：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                    Orb("Cloister tile star", p, new Vector3(x, 0.144f, z),
                        // 步骤 11：执行 Vector3.one * 0.09f, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                        Vector3.one * 0.09f, "PaleGold");
            }
        // 步骤 12：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister west outer wall", p, -28.1f, 3.6f, 38f, 0.65f, 7.2f, 26f, "Ivory");
        // 步骤 13：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister south outer wall", p, -13f, 3.6f, 25f, 30f, 7.2f, 0.65f, "Ivory");
        // 步骤 14：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister north outer wall", p, -13f, 3.6f, 51f, 30f, 7.2f, 0.65f, "Ivory");
        // 步骤 15：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister east wall south", p, 2f, 3.6f, 29.5f, 0.65f, 7.2f, 9f, "Ivory");
        // 步骤 16：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister east wall north", p, 2f, 3.6f, 46.5f, 0.65f, 7.2f, 9f, "Ivory");
        // 步骤 17：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister entrance lintel", p, 2f, 7.6f, 38f, 0.8f, 1.4f, 8f, "Ivory");
        // 步骤 18：调用 Arch：Arch。
        Arch(p, "Cloister doorway", new Vector3(1.64f, 0f, 38f), 3.6f, 3.4f, false, "PaleGold");
        // 步骤 19：按 int row = 0; row < 2; row++ 变化索引；循环体依次处理每个位置。
        for (int row = 0; row < 2; row++)
        {
            // 步骤 20：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = row == 0 ? -22f : -4.5f;
            // 步骤 21：按 int i = 0; i < 7; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < 7; i++)
            {
                // 步骤 22：计算 27f + i * 3.6f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
                float z = 27f + i * 3.6f;
                // 步骤 23：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Cloister fluted column", p, x, z, 0f, 7.4f, 0.35f, "Ivory");
                // 步骤 24：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Cloister blue base", p, x, z, 0f, 0.32f, 0.49f, "BlueStone");
                // 步骤 25：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Cloister gilded capital", p, x, z, 7.1f, 0.34f, 0.51f,
                    // 步骤 26：执行 "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    "PaleGold", false);
                // 步骤 27：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
                if (i % 2 == 0) Lamp(p, "Cloister lamp", x + (row == 0 ? 1.1f : -1.1f),
                    // 步骤 28：执行 z, 1.7f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    z, 1.7f);
            }
        }
        // 步骤 29：按 int bay = 0; bay < 5; bay++ 变化索引；循环体依次处理每个位置。
        for (int bay = 0; bay < 5; bay++)
        {
            // 步骤 30：计算 28f + bay * 5f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 28f + bay * 5f;
            // 步骤 31：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Alcove azure backdrop", p, -27.68f, 3.4f, z, 0.08f, 4.8f, 3.6f,
                // 步骤 32：执行 "BlueStone", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "BlueStone", false);
            // 步骤 33：调用 Arch：Arch。
            Arch(p, "Alcove golden arch", new Vector3(-27.5f, 0f, z), 1.65f,
                // 步骤 34：执行 4.4f, false, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                4.4f, false, "PaleGold");
            // 步骤 35：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Alcove resting bench", p, -25.4f, 0.5f, z, 2.8f, 0.34f, 2f,
                // 步骤 36：执行 "HoneyWood");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "HoneyWood");
            // 步骤 37：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Alcove woven cushion", p, -25.4f, 0.72f, z, 2.55f, 0.1f, 1.7f,
                // 步骤 38：执行 "Banner", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Banner", false);
            // 步骤 39：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Alcove floating flame", p, new Vector3(-27f, 3.7f, z),
                // 步骤 40：执行 Vector3.one * 0.4f, "Candle");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * 0.4f, "Candle");
            // 步骤 41：按 int j = 0; j < 8; j++ 变化索引；循环体依次处理每个位置。
            for (int j = 0; j < 8; j++)
            {
                // 步骤 42：计算 z - 1.4f + j * 0.4f，把结果存进 zz（zz）；之后引用 zz 时用的就是这次结果。
                float zz = z - 1.4f + j * 0.4f;
                // 步骤 43：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
                Box("Alcove wall carving", p, -27.55f, 2.1f, zz,
                    // 步骤 44：执行 0.1f, 0.9f, 0.06f, "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0.1f, 0.9f, 0.06f, "PaleGold", false);
            }
        }
        // 步骤 45：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister south ceiling", p, -13f, 8.2f, 29f, 30f, 0.4f, 8f,
            // 步骤 46：执行 "Ivory", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Ivory", false);
        // 步骤 47：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister north ceiling", p, -13f, 8.2f, 47f, 30f, 0.4f, 8f,
            // 步骤 48：执行 "Ivory", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Ivory", false);
        // 步骤 49：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister west ceiling", p, -25f, 8.2f, 38f, 6f, 0.4f, 26f,
            // 步骤 50：执行 "Ivory", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Ivory", false);
        // 步骤 51：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Cloister east ceiling", p, -1f, 8.2f, 38f, 6f, 0.4f, 26f,
            // 步骤 52：执行 "Ivory", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Ivory", false);
        // 步骤 53：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++)
        {
            // 步骤 54：计算 27f + i * 3.1f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 27f + i * 3.1f;
            // 步骤 55：调用 Beam：Beam。
            Beam("Cloister cross vault", p, new Vector3(-22f, 7.8f, z),
                // 步骤 56：用 new 创建 Vector3(-4.5f, 7.8f, z), 0.22f, "PaleGold");；构造参数决定新对象的初始内容。
                new Vector3(-4.5f, 7.8f, z), 0.22f, "PaleGold");
        }
        // 步骤 57：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Cloister moon basin rim", p, -13f, 38f, 0.14f, 0.13f, 2.8f,
            // 步骤 58：执行 "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "PaleGold", false);
        // 步骤 59：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Cloister moon basin", p, -13f, 38f, 0.27f, 0.015f, 2.4f,
            // 步骤 60：执行 "Water", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Water", false);
        // 步骤 61：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
        Orb("Cloister moon", p, new Vector3(-13f, 3.1f, 38f),
            // 步骤 62：执行 Vector3.one * 0.78f, "MoonLight");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.one * 0.78f, "MoonLight");
        // 步骤 63：调用 AddLight：添加灯光。
        AddLight(p, "Cloister moon glow", new Vector3(-13f, 5.7f, 38f),
            // 步骤 64：用 new 创建 Color(0.55f, 0.82f, 1f), 1.8f, 12f);；构造参数决定新对象的初始内容。
            new Color(0.55f, 0.82f, 1f), 1.8f, 12f);
    }
    // ！！！功能结束：WestCloister！！！

    // ！！！功能：GrandCourt——GrandCourt！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void GrandCourt()
    {
        // 步骤 1：计算 Group("06 - Grand northern court and sky gate")，把结果存进 p（p）；之后引用 p 时用的就是这次结果。
        Transform p = Group("06 - Grand northern court and sky gate");
        // 步骤 2：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Grand northern court floor", p, 7.5f, 0.055f, 84f,
            // 步骤 3：执行 55f, 0.12f, 28f, "WarmLimestone");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            55f, 0.12f, 28f, "WarmLimestone");
        // 步骤 4：按 int ix = 0; ix < 27; ix++ 变化索引；循环体依次处理每个位置。
        for (int ix = 0; ix < 27; ix++)
            // 步骤 5：按 int iz = 0; iz < 14; iz++ 变化索引；循环体依次处理每个位置。
            for (int iz = 0; iz < 14; iz++)
            {
                // 步骤 6：计算 -18.5f + ix * 2f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
                float x = -18.5f + ix * 2f;
                // 步骤 7：计算 71f + iz * 2f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
                float z = 71f + iz * 2f;
                // 步骤 8：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
                Box("Court sunstone floor tile", p, x, 0.127f, z,
                    // 步骤 9：继续填写上一行调用的参数：1.82f, 0.014f, 1.82f；参数位置决定它表示的坐标、速度或选项。
                    1.82f, 0.014f, 1.82f,
                    // 步骤 10：执行 (ix + iz) % 6 == 0 ? "Mist" : "Ivory", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    (ix + iz) % 6 == 0 ? "Mist" : "Ivory", false);
                // 步骤 11：这里用 && 要求两边条件同时成立：ix % 4 == 0 && iz % 4 == 0；任意一边为假就跳过本分支。
                if (ix % 4 == 0 && iz % 4 == 0)
                    // 步骤 12：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                    Orb("Court floor constellation", p, new Vector3(x, 0.15f, z),
                        // 步骤 13：执行 Vector3.one * 0.11f, "Candle");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                        Vector3.one * 0.11f, "Candle");
            }
        // 步骤 14：按 int side = -1; side <= 1; side += 2 变化索引；循环体依次处理每个位置。
        for (int side = -1; side <= 1; side += 2)
        {
            // 步骤 15：计算 7.5f + side * 27.6f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 7.5f + side * 27.6f;
            // 步骤 16：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Court side balustrade", p, x, 0.9f, 84f,
                // 步骤 17：执行 0.55f, 1.8f, 28f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.55f, 1.8f, 28f, "Ivory");
            // 步骤 18：按 int i = 0; i <= 9; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i <= 9; i++)
            {
                // 步骤 19：计算 70f + i * 3.1f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
                float z = 70f + i * 3.1f;
                // 步骤 20：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
                Pillar("Court side obelisk", p, x, z, 0f, 2.4f,
                    // 步骤 21：执行 0.32f, "BlueStone");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0.32f, "BlueStone");
                // 步骤 22：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                Orb("Court obelisk jewel", p, new Vector3(x, 2.6f, z),
                    // 步骤 23：执行 Vector3.one * 0.34f, "Candle");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    Vector3.one * 0.34f, "Candle");
            }
        }
        // 步骤 24：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Court north balustrade left", p, -9.5f, 0.9f, 98f,
            // 步骤 25：执行 21f, 1.8f, 0.55f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            21f, 1.8f, 0.55f, "Ivory");
        // 步骤 26：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Court north balustrade right", p, 24.5f, 0.9f, 98f,
            // 步骤 27：执行 21f, 1.8f, 0.55f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            21f, 1.8f, 0.55f, "Ivory");
        // 步骤 28：按 int i = 0; i < 18; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 18; i++)
        {
            // 步骤 29：计算 i * Mathf.PI * 2f / 18f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * Mathf.PI * 2f / 18f;
            // 步骤 30：计算 6f + Mathf.Cos(a) * 11.5f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 6f + Mathf.Cos(a) * 11.5f;
            // 步骤 31：计算 84f + Mathf.Sin(a) * 11.5f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 84f + Mathf.Sin(a) * 11.5f;
            // 步骤 32：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Court circular colonnade", p, x, z, 0f, 6.9f,
                // 步骤 33：执行 0.37f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.37f, "Ivory");
            // 步骤 34：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Court gold capital", p, x, z, 6.6f, 0.4f,
                // 步骤 35：执行 0.52f, "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.52f, "PaleGold", false);
            // 步骤 36：计算 (i + 1) * Mathf.PI * 2f / 18f，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
            float b = (i + 1) * Mathf.PI * 2f / 18f;
            // 步骤 37：调用 Beam：Beam。
            Beam("Court ring architrave", p, new Vector3(x, 7.2f, z),
                // 步骤 38：用 new 创建 Vector3(6f + Mathf.Cos(b) * 11.5f, 7.2f,；构造参数决定新对象的初始内容。
                new Vector3(6f + Mathf.Cos(b) * 11.5f, 7.2f,
                    // 步骤 39：执行 84f + Mathf.Sin(b) * 11.5f), 0.28f, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    84f + Mathf.Sin(b) * 11.5f), 0.28f, "PaleGold");
            // 步骤 40：检查条件 i % 3 == 0；只有为真才运行下面的步骤，否则沿原流程继续。
            if (i % 3 == 0)
            {
                // 步骤 41：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                Orb("Court suspended lantern", p,
                    // 步骤 42：用 new 创建 Vector3(6f + Mathf.Cos(a) * 9f, 5f,；构造参数决定新对象的初始内容。
                    new Vector3(6f + Mathf.Cos(a) * 9f, 5f,
                        // 步骤 43：继续填写上一行调用的参数：84f + Mathf.Sin(a) * 9f)；参数位置决定它表示的坐标、速度或选项。
                        84f + Mathf.Sin(a) * 9f),
                    // 步骤 44：执行 Vector3.one * 0.52f, "Candle");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    Vector3.one * 0.52f, "Candle");
            }
        }
        // 步骤 45：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Court central star plinth", p, 6f, 84f, 0.12f,
            // 步骤 46：执行 0.22f, 4.2f, "BlueStone", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            0.22f, 4.2f, "BlueStone", false);
        // 步骤 47：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Court central gold mandala", p, 6f, 84f, 0.35f,
            // 步骤 48：执行 0.025f, 3.5f, "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            0.025f, 3.5f, "PaleGold", false);
        // 步骤 49：按 int i = 0; i < 20; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 20; i++)
        {
            // 步骤 50：计算 i * Mathf.PI / 10f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * Mathf.PI / 10f;
            // 步骤 51：调用 Beam：Beam。
            Beam("Radial sun inlay", p,
                // 步骤 52：用 new 创建 Vector3(6f + Mathf.Cos(a) * 1.4f, 0.4f,；构造参数决定新对象的初始内容。
                new Vector3(6f + Mathf.Cos(a) * 1.4f, 0.4f,
                    // 步骤 53：继续填写上一行调用的参数：84f + Mathf.Sin(a) * 1.4f)；参数位置决定它表示的坐标、速度或选项。
                    84f + Mathf.Sin(a) * 1.4f),
                // 步骤 54：用 new 创建 Vector3(6f + Mathf.Cos(a) * 3.2f, 0.4f,；构造参数决定新对象的初始内容。
                new Vector3(6f + Mathf.Cos(a) * 3.2f, 0.4f,
                    // 步骤 55：继续填写上一行调用的参数：84f + Mathf.Sin(a) * 3.2f)；参数位置决定它表示的坐标、速度或选项。
                    84f + Mathf.Sin(a) * 3.2f),
                // 步骤 56：执行 0.11f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.11f, "Ivory");
        }
        // 步骤 57：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
        Orb("Court levitating star", p, new Vector3(6f, 3.5f, 84f),
            // 步骤 58：执行 Vector3.one * 1.1f, "MoonLight");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.one * 1.1f, "MoonLight");
        // 步骤 59：按 int side = -1; side <= 1; side += 2 变化索引；循环体依次处理每个位置。
        for (int side = -1; side <= 1; side += 2)
        {
            // 步骤 60：计算 6f + side * 20f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 6f + side * 20f;
            // 步骤 61：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Court lily pool rim", p, x, 84f, 0.13f,
                // 步骤 62：执行 0.15f, 4.1f, "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.15f, 4.1f, "PaleGold", false);
            // 步骤 63：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Court lily pool", p, x, 84f, 0.27f,
                // 步骤 64：执行 0.015f, 3.8f, "Water", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.015f, 3.8f, "Water", false);
            // 步骤 65：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < 8; i++)
            {
                // 步骤 66：计算 i * Mathf.PI / 4f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
                float a = i * Mathf.PI / 4f;
                // 步骤 67：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                Orb("Court lily light", p,
                    // 步骤 68：用 new 创建 Vector3(x + Mathf.Cos(a) * 2.7f, 0.35f,；构造参数决定新对象的初始内容。
                    new Vector3(x + Mathf.Cos(a) * 2.7f, 0.35f,
                        // 步骤 69：继续填写上一行调用的参数：84f + Mathf.Sin(a) * 2.7f)；参数位置决定它表示的坐标、速度或选项。
                        84f + Mathf.Sin(a) * 2.7f),
                    // 步骤 70：用 new 创建 Vector3(0.28f, 0.11f, 0.28f), "Candle");；构造参数决定新对象的初始内容。
                    new Vector3(0.28f, 0.11f, 0.28f), "Candle");
            }
        }
        // 步骤 71：调用 AddLight：添加灯光。
        AddLight(p, "Grand star glow", new Vector3(6f, 5.5f, 84f),
            // 步骤 72：用 new 创建 Color(0.62f, 0.88f, 1f), 2f, 15f);；构造参数决定新对象的初始内容。
            new Color(0.62f, 0.88f, 1f), 2f, 15f);
        // 步骤 73：把计算结果 AssetDatabase.LoadAssetAtPath<GameObject>( 保存为gate资源；下一条判断或调用会读取这个具体值。
        GameObject gateAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
            // 步骤 74：执行 "Assets/Project files/3/SkyGateModel.fbx");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Assets/Project files/3/SkyGateModel.fbx");
        // 步骤 75：确认gate资源已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (gateAsset != null)
        {
            // 步骤 76：计算 (GameObject)PrefabUtility.InstantiatePrefab(gateAsset)，把结果存进 monument（monument）；之后引用 monument 时用的就是这次结果。
            GameObject monument = (GameObject)PrefabUtility.InstantiatePrefab(gateAsset);
            // 步骤 77：把计算结果 "Existing Sky Gate - northern monument" 保存为name；下一条判断或调用会读取这个具体值。
            monument.name = "Existing Sky Gate - northern monument";
            // 步骤 78：执行 monument.transform.SetParent(p, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            monument.transform.SetParent(p, false);
            // 步骤 79：调用 FitModel：FitModel。
            FitModel(monument, new Vector3(6f, 0.14f, 96f), 7.5f, 0f);
            // 步骤 80：调用 RemoveModelColliders：移除ModelColliders。
            RemoveModelColliders(monument);
        }
    }
    // ！！！功能结束：GrandCourt！！！

    // ！！！功能：EastAmphitheater——EastAmphitheater！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void EastAmphitheater()
    {
        // 步骤 1：计算 Group("07 - Eastern amphitheater and character court")，把结果存进 p（p）；之后引用 p 时用的就是这次结果。
        Transform p = Group("07 - Eastern amphitheater and character court");
        // 步骤 2：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("East gallery connector", p, 40f, 0.05f, 42f,
            // 步骤 3：执行 8f, 0.12f, 10f, "WarmLimestone");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            8f, 0.12f, 10f, "WarmLimestone");
        // 步骤 4：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Eastern amphitheater floor", p, 47.5f, 0.055f, 65.5f,
            // 步骤 5：执行 35f, 0.12f, 41f, "WarmLimestone");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            35f, 0.12f, 41f, "WarmLimestone");
        // 步骤 6：按 int ix = 0; ix < 17; ix++ 变化索引；循环体依次处理每个位置。
        for (int ix = 0; ix < 17; ix++)
            // 步骤 7：按 int iz = 0; iz < 20; iz++ 变化索引；循环体依次处理每个位置。
            for (int iz = 0; iz < 20; iz++)
            {
                // 步骤 8：计算 31.5f + ix * 2f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
                float x = 31.5f + ix * 2f;
                // 步骤 9：计算 46.5f + iz * 2f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
                float z = 46.5f + iz * 2f;
                // 步骤 10：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
                Box("Amphitheater mosaic tile", p, x, 0.127f, z,
                    // 步骤 11：继续填写上一行调用的参数：1.82f, 0.014f, 1.82f；参数位置决定它表示的坐标、速度或选项。
                    1.82f, 0.014f, 1.82f,
                    // 步骤 12：执行 (ix + iz) % 5 == 0 ? "BlueStone" : "Ivory", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    (ix + iz) % 5 == 0 ? "BlueStone" : "Ivory", false);
                // 步骤 13：检查条件 (ix * 3 + iz) % 9 == 0；只有为真才运行下面的步骤，否则沿原流程继续。
                if ((ix * 3 + iz) % 9 == 0)
                    // 步骤 14：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                    Orb("Amphitheater tile light", p,
                        // 步骤 15：用 new 创建 Vector3(x, 0.15f, z),；构造参数决定新对象的初始内容。
                        new Vector3(x, 0.15f, z),
                        // 步骤 16：执行 Vector3.one * 0.085f, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                        Vector3.one * 0.085f, "PaleGold");
            }
        // Three concentric seating terraces leave the south entrance open.
        // 步骤 17：按 int row = 0; row < 3; row++ 变化索引；循环体依次处理每个位置。
        for (int row = 0; row < 3; row++)
        {
            // 步骤 18：计算 10.8f + row * 2.25f，把结果存进 radius（radius）；之后引用 radius 时用的就是这次结果。
            float radius = 10.8f + row * 2.25f;
            // 步骤 19：按 int segment = 0; segment < 22; segment++ 变化索引；循环体依次处理每个位置。
            for (int segment = 0; segment < 22; segment++)
            {
                // 步骤 20：计算 -0.12f + segment * (Mathf.PI + 0.24f) / 22f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
                float a = -0.12f + segment * (Mathf.PI + 0.24f) / 22f;
                // 步骤 21：计算 -0.12f + (segment + 1) * (Mathf.PI + 0.24f) / 22f，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
                float b = -0.12f + (segment + 1) * (Mathf.PI + 0.24f) / 22f;
                // 步骤 22：新建用于v的对象或数值结构，并把构造参数保存进去。
                Vector3 v = new Vector3(49f + Mathf.Cos(a) * radius,
                    // 步骤 23：执行 0.28f + row * 0.21f, 68f + Mathf.Sin(a) * radius);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0.28f + row * 0.21f, 68f + Mathf.Sin(a) * radius);
                // 步骤 24：新建用于w的对象或数值结构，并把构造参数保存进去。
                Vector3 w = new Vector3(49f + Mathf.Cos(b) * radius,
                    // 步骤 25：执行 0.28f + row * 0.21f, 68f + Mathf.Sin(b) * radius);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0.28f + row * 0.21f, 68f + Mathf.Sin(b) * radius);
                // 步骤 26：调用 Beam：Beam。
                Beam("Curved audience bench", p, v, w,
                    // 步骤 27：用三元表达式为 row 二选一：问号前条件为真取前值，为假取冒号后的值。
                    0.28f, row == 1 ? "HoneyWood" : "BlueStone");
                // 步骤 28：检查条件 segment % 3 == 0；只有为真才运行下面的步骤，否则沿原流程继续。
                if (segment % 3 == 0)
                    // 步骤 29：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
                    Orb("Seat gold rivet", p, w + Vector3.up * 0.2f,
                        // 步骤 30：执行 Vector3.one * 0.13f, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                        Vector3.one * 0.13f, "PaleGold");
            }
        }
        // 步骤 31：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Performance stage", p, 49f, 0.31f, 68f,
            // 步骤 32：执行 15f, 0.62f, 10f, "BlueStone");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            15f, 0.62f, 10f, "BlueStone");
        // 步骤 33：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Stage gilt edge", p, 49f, 0.65f, 63f,
            // 步骤 34：执行 15.4f, 0.12f, 0.3f, "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            15.4f, 0.12f, 0.3f, "PaleGold", false);
        // 步骤 35：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Audience approach step", p, 49f, 0.12f, 62.2f,
            // 步骤 36：执行 8f, 0.24f, 1.5f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            8f, 0.24f, 1.5f, "Ivory");
        // 步骤 37：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Audience middle step", p, 49f, 0.24f, 63.5f,
            // 步骤 38：执行 8f, 0.48f, 1.5f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            8f, 0.48f, 1.5f, "Ivory");
        // 步骤 39：按 int i = 0; i < 18; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 18; i++)
        {
            // 步骤 40：计算 i * Mathf.PI * 2f / 18f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * Mathf.PI * 2f / 18f;
            // 步骤 41：计算 49f + Mathf.Cos(a) * 17f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 49f + Mathf.Cos(a) * 17f;
            // 步骤 42：计算 68f + Mathf.Sin(a) * 17f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 68f + Mathf.Sin(a) * 17f;
            // 步骤 43：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Amphitheater perimeter column", p, x, z, 0f,
                // 步骤 44：执行 6.7f, 0.35f, "Ivory");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                6.7f, 0.35f, "Ivory");
            // 步骤 45：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Perimeter column capital", p, x, z, 6.4f,
                // 步骤 46：执行 0.38f, 0.53f, "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.38f, 0.53f, "PaleGold", false);
            // 步骤 47：检查条件 i % 2 == 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Lamp：Lamp。
            if (i % 2 == 0) Lamp(p, "Gallery torch", x, z, 2f);
        }
        // 步骤 48：按 int i = 0; i < 16; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 16; i++)
        {
            // 步骤 49：计算 i * Mathf.PI / 8f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * Mathf.PI / 8f;
            // 步骤 50：计算 49f + Mathf.Cos(a) * 6f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 49f + Mathf.Cos(a) * 6f;
            // 步骤 51：计算 68f + Mathf.Sin(a) * 3.7f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 68f + Mathf.Sin(a) * 3.7f;
            // 步骤 52：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Stage constellation", p,
                // 步骤 53：用 new 创建 Vector3(x, 0.73f, z),；构造参数决定新对象的初始内容。
                new Vector3(x, 0.73f, z),
                // 步骤 54：执行 Vector3.one * 0.12f, "Candle");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * 0.12f, "Candle");
        }
        // 步骤 55：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        Box("Stage rear azure wall", p, 49f, 3.2f, 73f,
            // 步骤 56：执行 16f, 6.4f, 0.6f, "BlueStone");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            16f, 6.4f, 0.6f, "BlueStone");
        // 步骤 57：调用 Arch：Arch。
        Arch(p, "Performance stage arch", new Vector3(49f, 0f, 72.55f),
            // 步骤 58：执行 6f, 3.2f, true, "PaleGold");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            6f, 3.2f, true, "PaleGold");
        // 步骤 59：按 int i = 0; i < 12; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 12; i++)
        {
            // 步骤 60：计算 42f + i * 1.25f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 42f + i * 1.25f;
            // 步骤 61：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
            Box("Stage rear light slit", p, x, 3.2f, 72.62f,
                // 步骤 62：继续填写上一行调用的参数：0.07f, 3.2f + (i % 3) * 0.4f, 0.07f；参数位置决定它表示的坐标、速度或选项。
                0.07f, 3.2f + (i % 3) * 0.4f, 0.07f,
                // 步骤 63：执行 "Candle", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Candle", false);
        }
        // 步骤 64：调用 AddLight：添加灯光。
        AddLight(p, "Eastern stage glow", new Vector3(49f, 5.5f, 67f),
            // 步骤 65：用 new 创建 Color(1f, 0.74f, 0.46f), 2.1f, 13f);；构造参数决定新对象的初始内容。
            new Color(1f, 0.74f, 0.46f), 2.1f, 13f);
    }
    // ！！！功能结束：EastAmphitheater！！！

    // ！！！功能：CharacterGallery——CharacterGallery！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void CharacterGallery()
    {
        // 步骤 1：计算 Group("08 - Visible character gallery")，把结果存进 p（p）；之后引用 p 时用的就是这次结果。
        Transform p = Group("08 - Visible character gallery");
        // 步骤 2：调用 PlaceCharacter：PlaceCharacter。
        PlaceCharacter(p, "COTL traveller", "Assets/Project files/COTL_rigged.fbx",
            // 步骤 3：用 new 创建 Vector3(-16f, 0.16f, 36f), 1.6f, 240f);；构造参数决定新对象的初始内容。
            new Vector3(-16f, 0.16f, 36f), 1.6f, 240f);
        // 步骤 4：调用 PlaceCharacter：PlaceCharacter。
        PlaceCharacter(p, "Candle pilgrim", "Assets/Project files/1/2021.fbx",
            // 步骤 5：用 new 创建 Vector3(-12f, 0.16f, 45f), 1.6f, 0f);；构造参数决定新对象的初始内容。
            new Vector3(-12f, 0.16f, 45f), 1.6f, 0f);
        // 步骤 6：调用 PlaceCharacter：PlaceCharacter。
        PlaceCharacter(p, "Shibiyama dancer", "Assets/Project files/2/shibiyamaskfb.fbx",
            // 步骤 7：用 new 创建 Vector3(44.5f, 0.65f, 67f), 1.6f, 20f);；构造参数决定新对象的初始内容。
            new Vector3(44.5f, 0.65f, 67f), 1.6f, 20f);
        // 步骤 8：把计算结果 GameObject.Find("skeleton") ?? 保存为originalSkeleton；下一条判断或调用会读取这个具体值。
        GameObject originalSkeleton = GameObject.Find("skeleton") ??
            // 步骤 9：调用 Find：查找。
            GameObject.Find("Skeleton pilgrim - grounded gallery character");
        // 步骤 10：确认originalSkeleton已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (originalSkeleton != null)
        {
            // 步骤 11：把计算结果 "Skeleton pilgrim - grounded gallery character" 保存为name；下一条判断或调用会读取这个具体值。
            originalSkeleton.name = "Skeleton pilgrim - grounded gallery character";
            // 步骤 12：显示这个场景对象及其子对象；界面会立即反映这一状态。
            originalSkeleton.SetActive(true);
            // 步骤 13：调用 FitModel：FitModel。
            FitModel(originalSkeleton, new Vector3(-19f, 0.16f, 44f), 1.6f, 130f);
            // 步骤 14：调用 DetailFlatCharacter：DetailFlatCharacter。
            DetailFlatCharacter(originalSkeleton);
            // 步骤 15：调用 RemoveModelColliders：移除ModelColliders。
            RemoveModelColliders(originalSkeleton);
        }
        // 步骤 16：把计算结果 FindSceneRoot("C1_Player") ?? 保存为legacy玩家；下一条判断或调用会读取这个具体值。
        GameObject legacyPlayer = FindSceneRoot("C1_Player") ??
            // 步骤 17：调用 FindSceneRoot：查找场景Root。
            FindSceneRoot("Original C1 player - visible gallery character");
        // 步骤 18：确认legacy玩家已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (legacyPlayer != null)
        {
            // 步骤 19：逐个读取 MonoBehaviour behaviour in legacyPlayer.GetComponentsInChildren<MonoBehaviour>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (MonoBehaviour behaviour in legacyPlayer.GetComponentsInChildren<MonoBehaviour>(true))
                // 步骤 20：把 behaviour.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                behaviour.enabled = false;
            // 步骤 21：逐个读取 Camera camera in legacyPlayer.GetComponentsInChildren<Camera>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Camera camera in legacyPlayer.GetComponentsInChildren<Camera>(true))
                // 步骤 22：把 camera.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                camera.enabled = false;
            // 步骤 23：逐个读取 AudioListener listener in legacyPlayer.GetComponentsInChildren<AudioListener>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (AudioListener listener in legacyPlayer.GetComponentsInChildren<AudioListener>(true))
                // 步骤 24：把 listener.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                listener.enabled = false;
            // 步骤 25：把计算结果 "Original C1 player - visible gallery character" 保存为name；下一条判断或调用会读取这个具体值。
            legacyPlayer.name = "Original C1 player - visible gallery character";
            // 步骤 26：显示这个场景对象及其子对象；界面会立即反映这一状态。
            legacyPlayer.SetActive(true);
            // 步骤 27：调用 FitModel：FitModel。
            FitModel(legacyPlayer, new Vector3(-12f, 0.16f, 92f), 1.6f, 190f);
            // 步骤 28：调用 DetailFlatCharacter：DetailFlatCharacter。
            DetailFlatCharacter(legacyPlayer);
            // 步骤 29：调用 RemoveModelColliders：移除ModelColliders。
            RemoveModelColliders(legacyPlayer);
        }
        // 步骤 30：把计算结果 GameObject.Find("deer mask") ?? 保存为deerMask；下一条判断或调用会读取这个具体值。
        GameObject deerMask = GameObject.Find("deer mask") ??
            // 步骤 31：调用 Find：查找。
            GameObject.Find("Deer mask relic - grounded exhibit");
        // 步骤 32：确认deerMask已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (deerMask != null)
        {
            // 步骤 33：把计算结果 "Deer mask relic - grounded exhibit" 保存为name；下一条判断或调用会读取这个具体值。
            deerMask.name = "Deer mask relic - grounded exhibit";
            // 步骤 34：显示这个场景对象及其子对象；界面会立即反映这一状态。
            deerMask.SetActive(true);
            // 步骤 35：调用 FitModel：FitModel。
            FitModel(deerMask, new Vector3(-25.4f, 0.8f, 28f), 1.25f, 90f);
            // 步骤 36：调用 RemoveModelColliders：移除ModelColliders。
            RemoveModelColliders(deerMask);
        }
        // 步骤 37：把计算结果 FindSceneRoot("sky_pray_with texture") ?? 保存为original；下一条判断或调用会读取这个具体值。
        GameObject original = FindSceneRoot("sky_pray_with texture") ??
            // 步骤 38：调用 FindSceneRoot：查找场景Root。
            FindSceneRoot("Sky prayer character - stage guardian") ??
            // 步骤 39：调用 FindSceneRoot：查找场景Root。
            FindSceneRoot("Legacy sky prayer source model");
        // 步骤 40：确认original已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (original != null)
        {
            // 步骤 41：把计算结果 "Legacy sky prayer source model" 保存为name；下一条判断或调用会读取这个具体值。
            original.name = "Legacy sky prayer source model";
            // 步骤 42：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
            original.SetActive(false);
        }
        // 步骤 43：调用 PlaceCharacter：PlaceCharacter。
        PlaceCharacter(p, "Sky prayer guardian",
            // 步骤 44：继续填写上一行调用的参数："Assets/Project files/4/sky_pray_with texture.fbx"；参数位置决定它表示的坐标、速度或选项。
            "Assets/Project files/4/sky_pray_with texture.fbx",
            // 步骤 45：用 new 创建 Vector3(53.5f, 0.65f, 67f), 1.6f, 180f);；构造参数决定新对象的初始内容。
            new Vector3(53.5f, 0.65f, 67f), 1.6f, 180f);
        // 步骤 46：逐个读取 Vector3 place in new[] 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Vector3 place in new[]
        {
            // 步骤 47：用 new 创建 Vector3(-16f, 0f, 36f), new Vector3(-12f, 0f, 45f),；构造参数决定新对象的初始内容。
            new Vector3(-16f, 0f, 36f), new Vector3(-12f, 0f, 45f),
            // 步骤 48：用 new 创建 Vector3(44.5f, 0f, 67f), new Vector3(53.5f, 0f, 67f),；构造参数决定新对象的初始内容。
            new Vector3(44.5f, 0f, 67f), new Vector3(53.5f, 0f, 67f),
            // 步骤 49：用 new 创建 Vector3(-19f, 0f, 44f), new Vector3(-12f, 0f, 92f)；构造参数决定新对象的初始内容。
            new Vector3(-19f, 0f, 44f), new Vector3(-12f, 0f, 92f)
        // 步骤 50：这一行补充 })，与相邻行合起来完成同一次计算或对象配置。
        })
        {
            // 步骤 51：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Character grounded exhibit plinth", p, place.x, place.z,
                // 步骤 52：继续填写上一行调用的参数：0f, place.x > 0f ? 0.65f : 0.16f, 1.15f；参数位置决定它表示的坐标、速度或选项。
                0f, place.x > 0f ? 0.65f : 0.16f, 1.15f,
                // 步骤 53：执行 "PaleGold", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "PaleGold", false);
        }
    }
    // ！！！功能结束：CharacterGallery！！！

    // ！！！功能：FindSceneRoot——查找场景Root！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject FindSceneRoot(string name)
    {
        // 步骤 1：逐个读取 GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
            // 步骤 2：检查条件 obj.name == name；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
            if (obj.name == name) return obj;
        // 步骤 3：没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }
    // ！！！功能结束：FindSceneRoot！！！

    // 继续填写上一行调用的参数：private static void PlaceCharacter(Transform parent, string name, string assetPath；参数位置决定它表示的坐标、速度或选项。
    private static void PlaceCharacter(Transform parent, string name, string assetPath,
        // 这一行补充 Vector3 footPoint, float height, float facing)，与相邻行合起来完成同一次计算或对象配置。
        Vector3 footPoint, float height, float facing)
    {
        // 计算 AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)，把结果存进 asset（asset）；之后引用 asset 时用的就是这次结果。
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        // 检查asset是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("Character model is missing: " + assetPath);
        if (asset == null) throw new Exception("Character model is missing: " + assetPath);
        // 计算 (GameObject)PrefabUtility.InstantiatePrefab(asset)，把结果存进 instance（instance）；之后引用 instance 时用的就是这次结果。
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        // 把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        instance.name = name;
        // 执行 instance.transform.SetParent(parent, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        instance.transform.SetParent(parent, true);
        // 检查条件 name == "Shibiyama dancer"；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 RestoreDancerTextures：RestoreDancer贴图。
        if (name == "Shibiyama dancer") RestoreDancerTextures(instance);
        // 这里用 || 表示任一条件成立就处理：name == "COTL traveller" || name == "Candle pilgrim"；用于合并几种停止或跳过原因。 成立时调用 DetailFlatCharacter：DetailFlatCharacter。
        if (name == "COTL traveller" || name == "Candle pilgrim") DetailFlatCharacter(instance);
        // 检查条件 name == "Sky prayer guardian"；只有为真才运行下面的步骤，否则沿原流程继续。
        if (name == "Sky prayer guardian")
        {
            // The FBX bundles a floor, rocks, poles and flags around the actual figure.
            // 逐个读取 Renderer renderer in instance.GetComponentsInChildren<Renderer>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
                if (renderer.name.StartsWith("Plane.") || renderer.name.StartsWith("Cube.") ||
                    // 这一行补充 renderer.name.StartsWith("Cylinder."))，与相邻行合起来完成同一次计算或对象配置。
                    renderer.name.StartsWith("Cylinder."))
                    // 把 renderer.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                    renderer.enabled = false;
        }
        // 调用 FitModel：FitModel。
        FitModel(instance, footPoint, height, facing);
        // 调用 RemoveModelColliders：移除ModelColliders。
        RemoveModelColliders(instance);
    }

    // ！！！功能：RestoreDancerTextures——RestoreDancer贴图！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject instance 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void RestoreDancerTextures(GameObject instance)
    {
        // 步骤 1：计算 "Assets/Project files/2/"，把结果存进 folder（folder）；之后引用 folder 时用的就是这次结果。
        string folder = "Assets/Project files/2/";
        // 步骤 2：这一行补充 Dictionary<string, string> maps = new Dictionary<string, string> {，与相邻行合起来完成同一次计算或对象配置。
        Dictionary<string, string> maps = new Dictionary<string, string> {
            // 步骤 3：继续填写上一行调用的参数：{"Fabric", "shibiyama_Fabric_BaseColor.png"}；参数位置决定它表示的坐标、速度或选项。
            {"Fabric", "shibiyama_Fabric_BaseColor.png"},
            // 步骤 4：继续填写上一行调用的参数：{"Body Boots", "shibiyama_Body_Boots_BaseColor.png"}；参数位置决定它表示的坐标、速度或选项。
            {"Body Boots", "shibiyama_Body_Boots_BaseColor.png"},
            // 步骤 5：继续填写上一行调用的参数：{"mask", "shibiyama_mask_BaseColor.png"}；参数位置决定它表示的坐标、速度或选项。
            {"mask", "shibiyama_mask_BaseColor.png"},
            // 步骤 6：这一行补充 {"Grass", "bg_Grass_BaseColor.png"}，与相邻行合起来完成同一次计算或对象配置。
            {"Grass", "bg_Grass_BaseColor.png"}
        };
        // 步骤 7：逐个读取 KeyValuePair<string, string> pair in maps 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (KeyValuePair<string, string> pair in maps)
        {
            // 步骤 8：计算 MaterialFolder + "/Dancer_" + pair.Key.Replace(" ", "") + ".mat"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
            string path = MaterialFolder + "/Dancer_" + pair.Key.Replace(" ", "") + ".mat";
            // 步骤 9：计算 AssetDatabase.LoadAssetAtPath<Material>(path)，把结果存进 mat（mat）；之后引用 mat 时用的就是这次结果。
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            // 步骤 10：检查mat是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
            if (mat == null)
            {
                // 步骤 11：新建用于mat的对象或数值结构，并把构造参数保存进去。
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                // 步骤 12：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
                AssetDatabase.CreateAsset(mat, path);
            }
            // 步骤 13：计算 AssetDatabase.LoadAssetAtPath<Texture2D>(folder + pair.Value)，把结果存进 texture（texture）；之后引用 texture 时用的就是这次结果。
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + pair.Value);
            // 步骤 14：检查texture是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("Missing dancer texture: " + pair.Value);
            if (texture == null) throw new Exception("Missing dancer texture: " + pair.Value);
            // 步骤 15：调用 SetTexture：设置贴图。
            mat.SetTexture("_BaseMap", texture);
            // 步骤 16：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
            mat.SetColor("_BaseColor", Color.white);
            // 步骤 17：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            mat.SetFloat("_Smoothness", 0.13f);
            // 步骤 18：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(mat);
            // 步骤 19：逐个读取 Renderer renderer in instance.GetComponentsInChildren<Renderer>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                // 步骤 20：计算 renderer.sharedMaterials，把结果存进 slots（slots）；之后引用 slots 时用的就是这次结果。
                Material[] slots = renderer.sharedMaterials;
                // 步骤 21：用 changed 记住此处真假结果（false），供下一条 if 选择执行路径。
                bool changed = false;
                // 步骤 22：按 int i = 0; i < slots.Length; i++ 变化索引；循环体依次处理每个位置。
                for (int i = 0; i < slots.Length; i++)
                    // 步骤 23：检查对象引用是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
                    if (slots[i] != null && slots[i].name == pair.Key)
                    // 步骤 24：这一行补充 { slots[i] = mat; changed = true; }，与相邻行合起来完成同一次计算或对象配置。
                    { slots[i] = mat; changed = true; }
                // 步骤 25：检查条件 changed；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 renderer.sharedMaterials = slots;
                if (changed) renderer.sharedMaterials = slots;
            }
        }
    }
    // ！！！功能结束：RestoreDancerTextures！！！

    // ！！！功能：DetailFlatCharacter——DetailFlatCharacter！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject instance 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void DetailFlatCharacter(GameObject instance)
    {
        // 步骤 1：计算 AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/Cloth_Detail.png")，把结果存进 cloth（cloth）；之后引用 cloth 时用的就是这次结果。
        Texture2D cloth = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/Cloth_Detail.png");
        // 步骤 2：计算 AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/Cloth_Normal.png")，把结果存进 normal（normal）；之后引用 normal 时用的就是这次结果。
        Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/Cloth_Normal.png");
        // 步骤 3：检查cloth、normal是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 throw new Exception("Character detail maps are missing");
        if (cloth == null || normal == null) throw new Exception("Character detail maps are missing");
        // 步骤 4：逐个读取 Renderer renderer in instance.GetComponentsInChildren<Renderer>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            // 步骤 5：计算 renderer.sharedMaterials，把结果存进 slots（slots）；之后引用 slots 时用的就是这次结果。
            Material[] slots = renderer.sharedMaterials;
            // 步骤 6：用 changed 记住此处真假结果（false），供下一条 if 选择执行路径。
            bool changed = false;
            // 步骤 7：按 int i = 0; i < slots.Length; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < slots.Length; i++)
            {
                // 步骤 8：计算 slots[i]，把结果存进 source（反馈跟随的来源物品）；之后引用 source 时用的就是这次结果。
                Material source = slots[i];
                // 步骤 9：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
                if (source == null || source.mainTexture != null ||
                    // 步骤 10：这一行补充 source.name.StartsWith("Actor_") || source.name == "eyes" ||，与相邻行合起来完成同一次计算或对象配置。
                    source.name.StartsWith("Actor_") || source.name == "eyes" ||
                    // 步骤 11：把计算结果 = "head" || source.name == "hair" || 保存为name；下一条判断或调用会读取这个具体值。
                    source.name == "head" || source.name == "hair" ||
                    // 步骤 12：把计算结果 = "rose" || source.name == "mask" || 保存为name；下一条判断或调用会读取这个具体值。
                    source.name == "rose" || source.name == "mask" ||
                    // 步骤 13：把计算结果 = "Material.002" || source.name == "Material.082" || 保存为name；下一条判断或调用会读取这个具体值。
                    source.name == "Material.002" || source.name == "Material.082" ||
                    // 步骤 14：把计算结果 = "Telport Area") continue 保存为name；下一条判断或调用会读取这个具体值。
                    source.name == "Telport Area") continue;
                // 步骤 15：计算 source.name.Replace(" ", "").Replace(".", "_")，把结果存进 safeName（safeName）；之后引用 safeName 时用的就是这次结果。
                string safeName = source.name.Replace(" ", "").Replace(".", "_");
                // 步骤 16：计算 MaterialFolder + "/Actor_" + safeName + ".mat"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
                string path = MaterialFolder + "/Actor_" + safeName + ".mat";
                // 步骤 17：计算 AssetDatabase.LoadAssetAtPath<Material>(path)，把结果存进 detailed（detailed）；之后引用 detailed 时用的就是这次结果。
                Material detailed = AssetDatabase.LoadAssetAtPath<Material>(path);
                // 步骤 18：检查detailed是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
                if (detailed == null)
                {
                    // 步骤 19：新建用于detailed的对象或数值结构，并把构造参数保存进去。
                    detailed = new Material(source);
                    // 步骤 20：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
                    AssetDatabase.CreateAsset(detailed, path);
                }
                // 步骤 21：调用 SetTexture：设置贴图。
                detailed.SetTexture("_BaseMap", cloth);
                // 步骤 22：调用 SetTexture：设置贴图。
                detailed.SetTexture("_BumpMap", normal);
                // 步骤 23：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
                detailed.SetFloat("_BumpScale", 0.18f);
                // 步骤 24：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
                detailed.SetFloat("_Smoothness", 0.12f);
                // 步骤 25：调用 EnableKeyword：EnableKeyword。
                detailed.EnableKeyword("_NORMALMAP");
                // 步骤 26：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
                EditorUtility.SetDirty(detailed);
                // 步骤 27：执行 slots[i] = detailed;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                slots[i] = detailed;
                // 步骤 28：把 changed 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                changed = true;
            }
            // 步骤 29：检查条件 changed；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 renderer.sharedMaterials = slots;
            if (changed) renderer.sharedMaterials = slots;
        }
    }
    // ！！！功能结束：DetailFlatCharacter！！！

    // 继续填写上一行调用的参数：private static void FitModel(GameObject instance, Vector3 footPoint, float desiredHeight；参数位置决定它表示的坐标、速度或选项。
    private static void FitModel(GameObject instance, Vector3 footPoint, float desiredHeight,
        // 这一行补充 float facing)，与相邻行合起来完成同一次计算或对象配置。
        float facing)
    {
        // 执行 instance.transform.rotation = Quaternion.Euler(0f, facing, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        instance.transform.rotation = Quaternion.Euler(0f, facing, 0f);
        // 从目标物体找到所需组件并存为 renderers；使用前应检查是否为 null。
        Renderer[] renderers = Array.FindAll(instance.GetComponentsInChildren<Renderer>(true), r => r.enabled);
        // 检查条件 renderers.Length == 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 throw new Exception("Character has no visible renderers: " + instance.name)
        if (renderers.Length == 0) throw new Exception("Character has no visible renderers: " + instance.name);
        // 计算 renderers[0].bounds，把结果存进 bounds（bounds）；之后引用 bounds 时用的就是这次结果。
        Bounds bounds = renderers[0].bounds;
        // 逐个读取 Renderer renderer in renderers 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        // 检查条件 bounds.size.y < 0.001f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 throw new Exception("Character has zero height: " + instance.name);
        if (bounds.size.y < 0.001f) throw new Exception("Character has zero height: " + instance.name);
        // 计算 desiredHeight / bounds.size.y，把结果存进 factor（factor）；之后引用 factor 时用的就是这次结果。
        float factor = desiredHeight / bounds.size.y;
        // 执行 instance.transform.localScale *= factor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        instance.transform.localScale *= factor;
        // 把计算结果 renderers[0].bounds 保存为bounds；下一条判断或调用会读取这个具体值。
        bounds = renderers[0].bounds;
        // 逐个读取 Renderer renderer in renderers 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        // 继续填写上一行调用的参数：instance.transform.position += new Vector3(footPoint.x - bounds.center.x；参数位置决定它表示的坐标、速度或选项。
        instance.transform.position += new Vector3(footPoint.x - bounds.center.x,
            // 执行 footPoint.y - bounds.min.y, footPoint.z - bounds.center.z);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            footPoint.y - bounds.min.y, footPoint.z - bounds.center.z);
        // 告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(instance);
        // 把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Grounded character " + instance.name + " bottom=" + footPoint.y +
            // 执行 " height=" + desiredHeight + " at " + footPoint);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            " height=" + desiredHeight + " at " + footPoint);
    }

    // ！！！功能：RemoveModelColliders——移除ModelColliders！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject instance 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void RemoveModelColliders(GameObject instance)
    {
        // 步骤 1：逐个读取 Collider collider in instance.GetComponentsInChildren<Collider>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            // 步骤 2：在编辑器里立即移除旧对象；重新搭建时避免上次生成内容重复叠加。
            UnityEngine.Object.DestroyImmediate(collider);
        // Several source FBX files contain 1000-intensity authoring lights.
        // Their copies wash out the entire VR gallery and nearby paths.
        // 步骤 3：逐个读取 Light light in instance.GetComponentsInChildren<Light>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Light light in instance.GetComponentsInChildren<Light>(true))
            // 步骤 4：把 light.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            light.enabled = false;
        // 步骤 5：逐个读取 Camera camera in instance.GetComponentsInChildren<Camera>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Camera camera in instance.GetComponentsInChildren<Camera>(true))
            // 步骤 6：在编辑器里立即移除旧对象；重新搭建时避免上次生成内容重复叠加。
            UnityEngine.Object.DestroyImmediate(camera.gameObject);
    }
    // ！！！功能结束：RemoveModelColliders！！！

    // ！！！功能：Landscape——Landscape！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Landscape()
    {
        // 步骤 1：计算 Group("04 - Floating islands, trees and cloud sea")，把结果存进 p（p）；之后引用 p 时用的就是这次结果。
        Transform p = Group("04 - Floating islands, trees and cloud sea");
        // 步骤 2：按 int i = 0; i < 12; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 12; i++)
        {
            // 步骤 3：计算 -34f - (i % 3) * 3.3f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = -34f - (i % 3) * 3.3f;
            // 步骤 4：计算 28f + (i / 3) * 11.5f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 28f + (i / 3) * 11.5f;
            // 步骤 5：调用 CloudCluster：CloudCluster。
            CloudCluster(p, x, -1.3f, z, 1.4f + (i % 3) * 0.2f, i + 30);
        }
        // 步骤 6：按 int i = 0; i < 9; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 9; i++)
        {
            // 步骤 7：计算 72f + (i % 3) * 3f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 72f + (i % 3) * 3f;
            // 步骤 8：计算 50f + (i / 3) * 8f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 50f + (i / 3) * 8f;
            // 步骤 9：调用 CloudCluster：CloudCluster。
            CloudCluster(p, x, -1.1f, z, 1.3f, i + 50);
        }
        // 步骤 10：按 int i = 0; i < 7; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 7; i++)
        {
            // 步骤 11：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = i % 2 == 0 ? -35f : 75f;
            // 步骤 12：计算 32f + i * 5.4f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 32f + i * 5.4f;
            // 步骤 13：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
            Pillar("Distant floating rock", p, x, z, 2.5f + (i % 3), 0.8f,
                // 步骤 14：执行 0.8f + (i % 2) * 0.3f, "BlueStone", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                0.8f + (i % 2) * 0.3f, "BlueStone", false);
            // 步骤 15：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Floating island soft top", p, new Vector3(x, 3.7f + (i % 3), z),
                // 步骤 16：用 new 创建 Vector3(2.3f, 0.5f, 2.3f), "Mist");；构造参数决定新对象的初始内容。
                new Vector3(2.3f, 0.5f, 2.3f), "Mist");
        }
        // 步骤 17：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++)
        {
            // 步骤 18：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = i % 2 == 0 ? -34f : 74f;
            // 步骤 19：计算 29f + (i / 2) * 11f，把结果存进 z（z）；之后引用 z 时用的就是这次结果。
            float z = 29f + (i / 2) * 11f;
            // 步骤 20：调用 Tree：Tree。
            Tree(p, x, z, i);
        }
    }
    // ！！！功能结束：Landscape！！！

    // ！！！功能：Tree——Tree！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform p, float x, float z, int seed 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Tree(Transform p, float x, float z, int seed)
    {
        // 步骤 1：调用 Pillar：创建场景柱体，并按参数决定高度、半径和位置。
        Pillar("Ivory tree trunk", p, x, z, 0f, 2.4f + (seed % 3) * 0.3f,
            // 步骤 2：执行 0.13f, "IvoryBark", false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            0.13f, "IvoryBark", false);
        // 步骤 3：计算 2.8f + (seed % 3) * 0.3f，把结果存进 top（top）；之后引用 top 时用的就是这次结果。
        float top = 2.8f + (seed % 3) * 0.3f;
        // 步骤 4：按 int i = 0; i < 5; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 5; i++)
        {
            // 步骤 5：计算 i * 1.25f + seed * 0.6f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * 1.25f + seed * 0.6f;
            // 步骤 6：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Soft tree crown", p,
                // 步骤 7：用 new 创建 Vector3(x + Mathf.Cos(a) * 0.65f, top + (i % 2) * 0.35f,；构造参数决定新对象的初始内容。
                new Vector3(x + Mathf.Cos(a) * 0.65f, top + (i % 2) * 0.35f,
                    // 步骤 8：继续填写上一行调用的参数：z + Mathf.Sin(a) * 0.65f)；参数位置决定它表示的坐标、速度或选项。
                    z + Mathf.Sin(a) * 0.65f),
                // 步骤 9：用 new 创建 Vector3(1.35f, 0.85f, 1.25f), "LeafCanopy");；构造参数决定新对象的初始内容。
                new Vector3(1.35f, 0.85f, 1.25f), "LeafCanopy");
        }
    }
    // ！！！功能结束：Tree！！！

    // ！！！功能：CloudCluster——CloudCluster！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform p, float x, float y, float z, float size, int seed 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void CloudCluster(Transform p, float x, float y, float z, float size, int seed)
    {
        // 步骤 1：按 int i = 0; i < 4; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 4; i++)
        {
            // 步骤 2：计算 i * 1.57f + seed * 0.41f，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
            float a = i * 1.57f + seed * 0.41f;
            // 步骤 3：调用 Orb：创建球形装饰或能量体，并配置颜色和尺寸。
            Orb("Cloud bank", p,
                // 步骤 4：用 new 创建 Vector3(x + Mathf.Cos(a) * size * 0.65f,；构造参数决定新对象的初始内容。
                new Vector3(x + Mathf.Cos(a) * size * 0.65f,
                    // 步骤 5：继续填写上一行调用的参数：y + (i % 2) * 0.17f, z + Mathf.Sin(a) * size * 0.5f)；参数位置决定它表示的坐标、速度或选项。
                    y + (i % 2) * 0.17f, z + Mathf.Sin(a) * size * 0.5f),
                // 步骤 6：用 new 创建 Vector3(size * 1.8f, size * 0.65f, size * 1.45f), "Cloud");；构造参数决定新对象的初始内容。
                new Vector3(size * 1.8f, size * 0.65f, size * 1.45f), "Cloud");
        }
    }
    // ！！！功能结束：CloudCluster！！！

    // 继续填写上一行调用的参数：private static void AddLight(Transform parent, string name, Vector3 position；参数位置决定它表示的坐标、速度或选项。
    private static void AddLight(Transform parent, string name, Vector3 position,
        // 这一行补充 Color color, float intensity, float range)，与相邻行合起来完成同一次计算或对象配置。
        Color color, float intensity, float range)
    {
        // 创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name, typeof(Light));
        // 执行 obj.transform.SetParent(parent, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(parent, false);
        // 执行 obj.transform.position = position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.position = position;
        // 从目标物体找到所需组件并存为 light；使用前应检查是否为 null。
        Light light = obj.GetComponent<Light>();
        // 把计算结果 LightType.Point 保存为type；下一条判断或调用会读取这个具体值。
        light.type = LightType.Point;
        // 把计算结果 color 保存为color；下一条判断或调用会读取这个具体值。
        light.color = color;
        // 把计算结果 intensity * 0.7f 保存为intensity；下一条判断或调用会读取这个具体值。
        light.intensity = intensity * 0.7f;
        // 把计算结果 range * 0.82f 保存为range；下一条判断或调用会读取这个具体值。
        light.range = range * 0.82f;
        // 把计算结果 LightShadows.None 保存为shadows；下一条判断或调用会读取这个具体值。
        light.shadows = LightShadows.None;
    }

    // ！！！功能：ValidateBoth——验证Both！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void ValidateBoth()
    {
        // 步骤 1：调用 Validate：验证。
        Validate(Quest);
        // 步骤 2：调用 Validate：验证。
        Validate(Desktop);
    }
    // ！！！功能结束：ValidateBoth！！！

    // ！！！功能：FindNamed——查找Named！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform parent, string name 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Transform FindNamed(Transform parent, string name)
    {
        // 步骤 1：逐个读取 Transform t in parent.GetComponentsInChildren<Transform>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            // 步骤 2：检查条件 t.name == name；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
            if (t.name == name) return t;
        // 步骤 3：没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }
    // ！！！功能结束：FindNamed！！！

    // ！！！功能：CheckGrounded——检查Grounded！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, float expectedBottom 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void CheckGrounded(string name, float expectedBottom)
    {
        // 步骤 1：计算 GameObject.Find(name)，把结果存进 actor（actor）；之后引用 actor 时用的就是这次结果。
        GameObject actor = GameObject.Find(name);
        // 步骤 2：检查actor是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (actor == null || !actor.activeInHierarchy)
            // 步骤 3：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Visible character is missing: " + name);
        // 步骤 4：从目标物体找到所需组件并存为 renderers；使用前应检查是否为 null。
        Renderer[] renderers = Array.FindAll(actor.GetComponentsInChildren<Renderer>(true), r => r.enabled);
        // 步骤 5：检查条件 renderers.Length == 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 throw new Exception("Character has no visible mesh: " + name);
        if (renderers.Length == 0) throw new Exception("Character has no visible mesh: " + name);
        // 步骤 6：计算 renderers[0].bounds，把结果存进 bounds（bounds）；之后引用 bounds 时用的就是这次结果。
        Bounds bounds = renderers[0].bounds;
        // 步骤 7：逐个读取 Renderer renderer in renderers 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        // 步骤 8：检查条件 Mathf.Abs(bounds.min.y - expectedBottom) > 0.12f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (Mathf.Abs(bounds.min.y - expectedBottom) > 0.12f)
            // 步骤 9：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Character is not grounded: " + name +
                // 步骤 10：执行 " bottom=" + bounds.min.y + " expected=" + expectedBottom);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                " bottom=" + bounds.min.y + " expected=" + expectedBottom);
    }
    // ！！！功能结束：CheckGrounded！！！

    // ！！！功能：Validate——验证！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string scenePath 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Validate(string scenePath)
    {
        // 步骤 1：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        // 步骤 2：计算 GameObject.Find(RootName)，把结果存进 built（built）；之后引用 built 时用的就是这次结果。
        GameObject built = GameObject.Find(RootName);
        // 步骤 3：计算 UnityEngine.Object.FindFirstObjectByType<SkyQuestSanctuary>()，把结果存进 experience（experience）；之后引用 experience 时用的就是这次结果。
        SkyQuestSanctuary experience = UnityEngine.Object.FindFirstObjectByType<SkyQuestSanctuary>();
        // 步骤 4：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (built == null || experience == null || experience.beacons == null ||
            // 步骤 5：这一行补充 experience.beacons.Length != 4 || experience.gate == null || experience.reward == null)，与相邻行合起来完成同一次计算或对象配置。
            experience.beacons.Length != 4 || experience.gate == null || experience.reward == null)
            // 步骤 6：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Sanctuary extension is incomplete in " + scenePath);
        // 步骤 7：从目标物体找到所需组件并存为 renderers；使用前应检查是否为 null。
        int renderers = built.GetComponentsInChildren<Renderer>(true).Length;
        // 步骤 8：从目标物体找到所需组件并存为 colliders；使用前应检查是否为 null。
        int colliders = built.GetComponentsInChildren<Collider>(true).Length;
        // 步骤 9：这里用 || 表示任一条件成立就处理：renderers < 2000 || colliders < 170；用于合并几种停止或跳过原因。
        if (renderers < 2000 || colliders < 170)
            // 步骤 10：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Expanded sanctuary geometry unexpectedly small in " + scenePath);
        // 步骤 11：计算 FindNamed(built.transform, "Open sky railing")，把结果存进 railing（railing）；之后引用 railing 时用的就是这次结果。
        Transform railing = FindNamed(built.transform, "Open sky railing");
        // 步骤 12：计算 FindNamed(built.transform, "Railing post")，把结果存进 railingPost（railingPost）；之后引用 railingPost 时用的就是这次结果。
        Transform railingPost = FindNamed(built.transform, "Railing post");
        // 步骤 13：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (railing == null || railing.GetComponent<Collider>() == null ||
            // 步骤 14：从目标 GameObject 查找并保存 railingPost 组件；后面的移动或显示逻辑会直接使用这个缓存。
            railingPost == null || railingPost.GetComponent<Collider>() == null)
            // 步骤 15：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Arrival railing is missing collision in " + scenePath);
        // 步骤 16：定义不可在运行时改变的数值 oldFloorArea（oldFloorArea）；后续计算统一使用这个阈值、速度或尺寸。
        const float oldFloorArea = 22f * 6.5f + 20.5f * 24f + 27f * 22f + 6f * 5f;
        // 步骤 17：把数值 0f 暂存为 newFloorArea；它只在当前方法里参与后续判断或计算。
        float newFloorArea = 0f;
        // 步骤 18：逐个读取 string name in new[] { "West cloister floor", "Grand northern court floor", 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (string name in new[] { "West cloister floor", "Grand northern court floor",
            // 步骤 19：这一行补充 "Eastern amphitheater floor", "East gallery connector" })，与相邻行合起来完成同一次计算或对象配置。
            "Eastern amphitheater floor", "East gallery connector" })
        {
            // 步骤 20：计算 FindNamed(built.transform, name)，把结果存进 floor（floor）；之后引用 floor 时用的就是这次结果。
            Transform floor = FindNamed(built.transform, name);
            // 步骤 21：检查floor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new Exception("Missing expanded floor: " + name);
            if (floor == null) throw new Exception("Missing expanded floor: " + name);
            // 步骤 22：从目标物体找到所需组件并存为 bounds；使用前应检查是否为 null。
            Bounds bounds = floor.GetComponent<Renderer>().bounds;
            // 步骤 23：在newFloorArea原值上增加 bounds.size.x * bounds.size.z；旧状态与本次输入共同决定新状态。
            newFloorArea += bounds.size.x * bounds.size.z;
        }
        // 步骤 24：检查条件 oldFloorArea + newFloorArea < oldFloorArea * 3f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (oldFloorArea + newFloorArea < oldFloorArea * 3f)
            // 步骤 25：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Playable floor area did not reach three times the previous area.");
        // 步骤 26：调用 CheckGrounded：检查Grounded。
        CheckGrounded("COTL traveller", 0.16f);
        // 步骤 27：调用 CheckGrounded：检查Grounded。
        CheckGrounded("Candle pilgrim", 0.16f);
        // 步骤 28：调用 CheckGrounded：检查Grounded。
        CheckGrounded("Shibiyama dancer", 0.65f);
        // 步骤 29：调用 CheckGrounded：检查Grounded。
        CheckGrounded("Sky prayer guardian", 0.65f);
        // 步骤 30：调用 CheckGrounded：检查Grounded。
        CheckGrounded("Skeleton pilgrim - grounded gallery character", 0.16f);
        // 步骤 31：调用 CheckGrounded：检查Grounded。
        CheckGrounded("Original C1 player - visible gallery character", 0.16f);
        // 步骤 32：逐个读取 Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            // 步骤 33：这里用 && 要求两边条件同时成立：light.enabled && light.gameObject.activeInHierarchy && light.intensity > 10f；任意一边为假就跳过本分支。
            if (light.enabled && light.gameObject.activeInHierarchy && light.intensity > 10f)
                // 步骤 34：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                throw new Exception("Imported model light is overexposing the scene: " + light.name);
        // 步骤 35：从目标物体找到所需组件并存为 dancer；使用前应检查是否为 null。
        Renderer dancer = GameObject.Find("Shibiyama dancer").GetComponentInChildren<Renderer>();
        // 步骤 36：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (dancer == null || !Array.Exists(
            // 步骤 37：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            GameObject.Find("Shibiyama dancer").GetComponentsInChildren<Renderer>(true),
            // 步骤 38：把计算结果 > Array.Exists(r.sharedMaterials, 保存为r；下一条判断或调用会读取这个具体值。
            r => Array.Exists(r.sharedMaterials,
                // 步骤 39：把计算结果 > m != null && m.name == "Dancer_Fabric" && m.mainTexture != null))) 保存为m；下一条判断或调用会读取这个具体值。
                m => m != null && m.name == "Dancer_Fabric" && m.mainTexture != null)))
            // 步骤 40：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new Exception("Dancer fabric texture was not restored.");
        // 步骤 41：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Sanctuary validation passed: " + scenePath +
            // 步骤 42：这一行补充 " renderers=" + renderers + " colliders=" + colliders +，与相邻行合起来完成同一次计算或对象配置。
            " renderers=" + renderers + " colliders=" + colliders +
            // 步骤 43：这一行补充 " total-floor-m2=" + (oldFloorArea + newFloorArea) +，与相邻行合起来完成同一次计算或对象配置。
            " total-floor-m2=" + (oldFloorArea + newFloorArea) +
            // 步骤 44：执行 " previous-floor-m2=" + oldFloorArea);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            " previous-floor-m2=" + oldFloorArea);
    }
    // ！！！功能结束：Validate！！！
}
