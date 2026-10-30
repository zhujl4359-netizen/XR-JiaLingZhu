// ！！！文件功能：把古寺模型、材质、陶瓷奖励和先祖摆放整合到 IP1！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：此文件位于 Assets/Editor，仅用于 Unity 编辑器的搭建与验证，玩家安装的 APK 不执行这里的方法。
// 引入 UnityEditor 命名空间；下方可直接使用仅在 Unity 编辑器内使用的资源和菜单接口。
using UnityEditor;
// 引入 UnityEditor.SceneManagement 命名空间；下方可直接使用编辑器内创建、打开、保存场景的接口。
using UnityEditor.SceneManagement;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.SceneManagement 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.SceneManagement;

// Installs the optimized pieces exported from the three supplied Blender projects.
// Existing level geometry and gameplay objects are retained.
// 定义 SkyQuestTempleIntegration 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestTempleIntegration
{
    // 定义固定文字 Base（Base）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string Base = "Assets/SkyQuest/TempleIntegration/";
    // 定义固定文字 RootName（RootName）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string RootName = "Ancient temple complex — A301 + A427";
    // 执行 private static Material stone, statue, sandstone, wood, gold, darkStone;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private static Material stone, statue, sandstone, wood, gold, darkStone;
    // 创建新的 GameObject[] 并存为 ceramics；构造参数决定其初始内容或对象名称。
    private static readonly GameObject[] ceramics = new GameObject[8];

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Frame ancient temple in Scene view")]
    // ！！！功能：FrameAncientTemple——FrameAncientTemple！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void FrameAncientTemple()
    {
        // 步骤 1：计算 SceneView.lastActiveSceneView，把结果存进 view（view）；之后引用 view 时用的就是这次结果。
        SceneView view = SceneView.lastActiveSceneView;
        // 步骤 2：检查view是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (view == null) return;
        // 步骤 3：调用 LookAt：LookAt。
        view.LookAt(new Vector3(56f, 1.8f, 26f), Quaternion.Euler(25f, 180f, 0f), 17f);
        // 步骤 4：调用 Repaint：Repaint。
        view.Repaint();
    }
    // ！！！功能结束：FrameAncientTemple！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Integrate A301 A427 temples and A242 ceramics")]
    // ！！！功能：Integrate——Integrate！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void Integrate()
    {
        // 步骤 1：检查条件 EditorApplication.isPlayingOrWillChangePlaymode；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        // 步骤 2：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.Refresh();
        // 步骤 3：调用 EnsureFolder：确保文件夹。
        EnsureFolder(Base + "Materials");
        // 步骤 4：调用 EnsureFolder：确保文件夹。
        EnsureFolder(Base + "Prefabs");
        // 步骤 5：调用 BuildMaterials：构建材质。
        BuildMaterials();
        // 步骤 6：调用 BuildCeramicPrefabs：构建Ceramic预制体。
        BuildCeramicPrefabs();
        // 步骤 7：调用 IntegrateScene：Integrate场景。
        IntegrateScene("Assets/SkyQuest/SkyQuest3S.unity");
        // 步骤 8：调用 IntegrateScene：Integrate场景。
        IntegrateScene("Assets/SkyQuest/SkyDesktop.unity");
        // 步骤 9：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 10：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("A301 + A427 ancient temple and all eight A242 ceramic rewards installed in both SkyQuest scenes.");
    }
    // ！！！功能结束：Integrate！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Match ancient temple colors to existing buildings")]
    // ！！！功能：MatchTempleColors——MatchTempleColors！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void MatchTempleColors()
    {
        // 步骤 1：调用 BuildTempleMaterials：构建Temple材质。
        BuildTempleMaterials();
        // 步骤 2：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.SaveAssets();
        // 步骤 3：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("Ancient temple stone, timber and trim now use the existing building palette.");
    }
    // ！！！功能结束：MatchTempleColors！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Face COTL and C1 ancestors toward the visitor")]
    // ！！！功能：FaceScreenshotAncestors——Face截图Ancestors！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void FaceScreenshotAncestors()
    {
        // 步骤 1：检查条件 EditorApplication.isPlayingOrWillChangePlaymode；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        // 步骤 2：逐个读取 string path in new[] { 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (string path in new[] {
            // 步骤 3：这一行补充 "Assets/SkyQuest/SkyQuest3S.unity", "Assets/SkyQuest/SkyDesktop.unity" })，与相邻行合起来完成同一次计算或对象配置。
            "Assets/SkyQuest/SkyQuest3S.unity", "Assets/SkyQuest/SkyDesktop.unity" })
        {
            // 步骤 4：计算 EditorSceneManager.OpenScene(path, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            // 步骤 5：计算 Object.FindFirstObjectByType<SkyQuestMagicExchange>()，把结果存进 exchange（exchange）；之后引用 exchange 时用的就是这次结果。
            SkyQuestMagicExchange exchange = Object.FindFirstObjectByType<SkyQuestMagicExchange>();
            // 步骤 6：检查exchange是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new System.InvalidOperationException("Missing ancestor exchange in " 
            if (exchange == null) throw new System.InvalidOperationException("Missing ancestor exchange in " + path);
            // 步骤 7：调用 FaceScreenshotAncestors：Face截图Ancestors。
            FaceScreenshotAncestors(exchange);
            // 步骤 8：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
            EditorSceneManager.MarkSceneDirty(scene);
            // 步骤 9：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
            EditorSceneManager.SaveScene(scene);
        }
        // 步骤 10：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.OpenScene("Assets/SkyQuest/SkyQuest3S.unity", OpenSceneMode.Single);
        // 步骤 11：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
        Debug.Log("COTL and C1 ancestors now face visitors in both scenes.");
    }
    // ！！！功能结束：FaceScreenshotAncestors！！！

    // ！！！功能：FaceScreenshotAncestors——Face截图Ancestors！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 SkyQuestMagicExchange exchange 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void FaceScreenshotAncestors(SkyQuestMagicExchange exchange)
    {
        // The visitor approaches these rear shrines from the south aisle.
        // 步骤 1：创建新的 Vector3 并存为 visitor；构造参数决定其初始内容或对象名称。
        Vector3 visitor = new Vector3(56f, 0f, 10f);
        // 步骤 2：逐个读取 int index in new[] { 2, 5 } 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (int index in new[] { 2, 5 })
        {
            // 步骤 3：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (exchange.ancestors == null || index >= exchange.ancestors.Length ||
                // 步骤 4：这一行补充 exchange.ancestors[index] == null)，与相邻行合起来完成同一次计算或对象配置。
                exchange.ancestors[index] == null)
                // 步骤 5：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                throw new System.InvalidOperationException("Missing gallery ancestor " + index);
            // 步骤 6：计算 exchange.ancestors[index]，把结果存进 spirit（spirit）；之后引用 spirit 时用的就是这次结果。
            Transform spirit = exchange.ancestors[index];
            // 步骤 7：计算 visitor - spirit.position，把结果存进 direction（direction）；之后引用 direction 时用的就是这次结果。
            Vector3 direction = visitor - spirit.position;
            // 步骤 8：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
            direction.y = 0f;
            // COTL faces local -Z. C1's nested visual is already turned 180
            // degrees relative to its root, so its root must face the visitor.
            // 步骤 9：计算 Quaternion.LookRotation(direction.normalized, Vector3.up)，把结果存进 faceVisitor（faceVisitor）；之后引用 faceVisitor 时用的就是这次结果。
            Quaternion faceVisitor = Quaternion.LookRotation(direction.normalized, Vector3.up);
            // 步骤 10：计算 VisibleBoundsOf(spirit.gameObject)，把结果存进 before（before）；之后引用 before 时用的就是这次结果。
            Bounds before = VisibleBoundsOf(spirit.gameObject);
            // 步骤 11：把rotation更新为 index == 2 ?；物体朝向会在本帧改变。
            spirit.rotation = index == 2 ?
                // 步骤 12：执行 faceVisitor * Quaternion.Euler(0f, 180f, 0f) : faceVisitor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                faceVisitor * Quaternion.Euler(0f, 180f, 0f) : faceVisitor;
            // C1's imported pivot sits far from its mesh. Keep the actual figure
            // on its shrine while rotating the root used by interaction logic.
            // 步骤 13：检查条件 index == 5；只有为真才运行下面的步骤，否则沿原流程继续。
            if (index == 5)
            {
                // 步骤 14：计算 spirit.Find("C1")，把结果存进 model（model）；之后引用 model 时用的就是这次结果。
                Transform model = spirit.Find("C1");
                // 步骤 15：检查model是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new System.InvalidOperationException("Missing C1 visual model");
                if (model == null) throw new System.InvalidOperationException("Missing C1 visual model");
                // 步骤 16：计算 before.center - VisibleBoundsOf(spirit.gameObject).center，把结果存进 correction（correction）；之后引用 correction 时用的就是这次结果。
                Vector3 correction = before.center - VisibleBoundsOf(spirit.gameObject).center;
                // 步骤 17：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
                correction.y = 0f;
                // 步骤 18：把position更新为 correction；世界或局部位置会立即影响物体摆放。
                model.position += correction;
                // 步骤 19：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
                EditorUtility.SetDirty(model);
            }
            // 步骤 20：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(spirit);
            // 步骤 21：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (exchange.memorials == null || index >= exchange.memorials.Length ||
                // 步骤 22：这一行补充 exchange.memorials[index] == null)，与相邻行合起来完成同一次计算或对象配置。
                exchange.memorials[index] == null)
                // 步骤 23：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                throw new System.InvalidOperationException("Missing gallery memorial " + index);
            // 步骤 24：计算 exchange.memorials[index]，把结果存进 memorial（memorial）；之后引用 memorial 时用的就是这次结果。
            Transform memorial = exchange.memorials[index];
            // 步骤 25：计算 spirit.position - direction.normalized * .95f，把结果存进 behind（behind）；之后引用 behind 时用的就是这次结果。
            Vector3 behind = spirit.position - direction.normalized * .95f;
            // 步骤 26：新建用于position的对象或数值结构，并把构造参数保存进去。
            memorial.position = new Vector3(behind.x, memorial.position.y, behind.z);
            // 步骤 27：把rotation更新为 faceVisitor；物体朝向会在本帧改变。
            memorial.rotation = faceVisitor;
            // 步骤 28：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(memorial);
        }
    }
    // ！！！功能结束：FaceScreenshotAncestors！！！

    // ！！！功能：EnsureFolder——确保文件夹！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string path 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void EnsureFolder(string path)
    {
        // 步骤 1：计算 path.TrimEnd('/')，把结果存进 parent（parent）；之后引用 parent 时用的就是这次结果。
        string parent = path.TrimEnd('/');
        // 步骤 2：检查条件 AssetDatabase.IsValidFolder(parent)；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (AssetDatabase.IsValidFolder(parent)) return;
        // 步骤 3：计算 parent.LastIndexOf('/')，把结果存进 slash（slash）；之后引用 slash 时用的就是这次结果。
        int slash = parent.LastIndexOf('/');
        // 步骤 4：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
        AssetDatabase.CreateFolder(parent.Substring(0, slash), parent.Substring(slash + 1));
    }
    // ！！！功能结束：EnsureFolder！！！

    // ！！！功能：Texture——贴图！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, bool normal = false 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Texture2D 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Texture2D Texture(string name, bool normal = false)
    {
        // 步骤 1：计算 Base + "Textures/" + name + ".png"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
        string path = Base + "Textures/" + name + ".png";
        // 步骤 2：计算 AssetImporter.GetAtPath(path) as TextureImporter，把结果存进 importer（importer）；之后引用 importer 时用的就是这次结果。
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        // 步骤 3：检查importer是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 { Debug.LogError("Missing temple texture " + path); return null; }
        if (importer == null) { Debug.LogError("Missing temple texture " + path); return null; }
        // 步骤 4：用三元表达式为 desired 二选一：问号前条件为真取前值，为假取冒号后的值。
        TextureImporterType desired = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        // 步骤 5：这里用 || 表示任一条件成立就处理：importer.textureType != desired || importer.maxTextureSize != 1024 || !importer.mipmapEnabled；用于合并几种停止或跳过原因。
        if (importer.textureType != desired || importer.maxTextureSize != 1024 || !importer.mipmapEnabled)
        {
            // 步骤 6：把 desired 写入textureType；TextMeshPro 会把新文字显示给玩家。
            importer.textureType = desired;
            // 步骤 7：把计算结果 1024 保存为max贴图Size；下一条判断或调用会读取这个具体值。
            importer.maxTextureSize = 1024;
            // 步骤 8：把 importer.mipmapEnabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            importer.mipmapEnabled = true;
            // 步骤 9：调用 SaveAndReimport：保存和Reimport。
            importer.SaveAndReimport();
        }
        // 步骤 10：把 AssetDatabase.LoadAssetAtPath<Texture2D>(path) 作为方法结果返回给调用方；return 后本方法立即结束。
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    // ！！！功能结束：Texture！！！

    // ！！！功能：MakeMaterial——Make材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, string albedo, string normal, Color tint, float smoothness 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Material 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Material MakeMaterial(string name, string albedo, string normal, Color tint, float smoothness)
    {
        // 步骤 1：计算 Base + "Materials/" + name + ".mat"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
        string path = Base + "Materials/" + name + ".mat";
        // 步骤 2：计算 AssetDatabase.LoadAssetAtPath<Material>(path)，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        // 步骤 3：检查material是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (material == null)
        {
            // 步骤 4：计算 Shader.Find("Universal Render Pipeline/Lit")，把结果存进 shader（shader）；之后引用 shader 时用的就是这次结果。
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            // 步骤 5：检查shader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Standard");
            // 步骤 6：新建用于material的对象或数值结构，并把构造参数保存进去。
            material = new Material(shader);
            // 步骤 7：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateAsset(material, path);
        }
        // 步骤 8：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
        material.SetColor("_BaseColor", tint);
        // 步骤 9：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
        material.SetColor("_Color", tint);
        // 步骤 10：确认albedo已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (albedo != null)
        {
            // 步骤 11：计算 Texture(albedo)，把结果存进 map（map）；之后引用 map 时用的就是这次结果。
            Texture2D map = Texture(albedo);
            // 步骤 12：调用 SetTexture：设置贴图。
            material.SetTexture("_BaseMap", map);
            // 步骤 13：调用 SetTexture：设置贴图。
            material.SetTexture("_MainTex", map);
        }
        // 步骤 14：确认normal已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (normal != null)
        {
            // 步骤 15：调用 SetTexture：设置贴图。
            material.SetTexture("_BumpMap", Texture(normal, true));
            // 步骤 16：调用 EnableKeyword：EnableKeyword。
            material.EnableKeyword("_NORMALMAP");
        }
        // 步骤 17：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
        material.SetFloat("_Smoothness", smoothness);
        // 步骤 18：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(material);
        // 步骤 19：把 material 作为方法结果返回给调用方；return 后本方法立即结束。
        return material;
    }
    // ！！！功能结束：MakeMaterial！！！

    // ！！！功能：MatchExistingBuildingMaterial——MatchExistingBuilding材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string templeName, string buildingName 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Material 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Material MatchExistingBuildingMaterial(string templeName, string buildingName)
    {
        // 步骤 1：计算 Base + "Materials/" + templeName + ".mat"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
        string path = Base + "Materials/" + templeName + ".mat";
        // 步骤 2：把计算结果 AssetDatabase.LoadAssetAtPath<Material>( 保存为building；下一条判断或调用会读取这个具体值。
        Material building = AssetDatabase.LoadAssetAtPath<Material>(
            // 步骤 3：执行 "Assets/SkyQuest/Environment/Materials/" + buildingName + ".mat");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Assets/SkyQuest/Environment/Materials/" + buildingName + ".mat");
        // 步骤 4：检查building是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (building == null)
            // 步骤 5：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
            throw new System.InvalidOperationException("Missing existing building material: " + buildingName);
        // 步骤 6：计算 AssetDatabase.LoadAssetAtPath<Material>(path)，把结果存进 temple（temple）；之后引用 temple 时用的就是这次结果。
        Material temple = AssetDatabase.LoadAssetAtPath<Material>(path);
        // 步骤 7：检查temple是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (temple == null)
        {
            // 步骤 8：新建用于temple的对象或数值结构，并把构造参数保存进去。
            temple = new Material(building.shader);
            // 步骤 9：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            AssetDatabase.CreateAsset(temple, path);
        }
        // Copy into the temple's own material asset. Existing architecture and
        // the A242 ceramics keep their original materials and appearance.
        // 步骤 10：把计算结果 building.shader 保存为shader；下一条判断或调用会读取这个具体值。
        temple.shader = building.shader;
        // 步骤 11：调用 CopyPropertiesFromMaterial：CopyProperties从材质。
        temple.CopyPropertiesFromMaterial(building);
        // 步骤 12：把计算结果 building.shaderKeywords 保存为shaderKeywords；下一条判断或调用会读取这个具体值。
        temple.shaderKeywords = building.shaderKeywords;
        // 步骤 13：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(temple);
        // 步骤 14：把 temple 作为方法结果返回给调用方；return 后本方法立即结束。
        return temple;
    }
    // ！！！功能结束：MatchExistingBuildingMaterial！！！

    // ！！！功能：BuildTempleMaterials——构建Temple材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void BuildTempleMaterials()
    {
        // 步骤 1：把计算结果 MatchExistingBuildingMaterial("A301 aged ritual stone", "BlueStone") 保存为stone；下一条判断或调用会读取这个具体值。
        stone = MatchExistingBuildingMaterial("A301 aged ritual stone", "BlueStone");
        // 步骤 2：把计算结果 MatchExistingBuildingMaterial("A301 carved statue", "Ivory") 保存为statue；下一条判断或调用会读取这个具体值。
        statue = MatchExistingBuildingMaterial("A301 carved statue", "Ivory");
        // 步骤 3：把计算结果 MatchExistingBuildingMaterial("A427 warm sandstone", "WarmLimestone") 保存为sandstone；下一条判断或调用会读取这个具体值。
        sandstone = MatchExistingBuildingMaterial("A427 warm sandstone", "WarmLimestone");
        // 步骤 4：把计算结果 MatchExistingBuildingMaterial("A427 timber", "HoneyWood") 保存为wood；下一条判断或调用会读取这个具体值。
        wood = MatchExistingBuildingMaterial("A427 timber", "HoneyWood");
        // 步骤 5：把计算结果 MatchExistingBuildingMaterial("Temple bronze trim", "PaleGold") 保存为gold；下一条判断或调用会读取这个具体值。
        gold = MatchExistingBuildingMaterial("Temple bronze trim", "PaleGold");
        // 步骤 6：把计算结果 MatchExistingBuildingMaterial("Temple dark paving", "Slate") 保存为darkStone；下一条判断或调用会读取这个具体值。
        darkStone = MatchExistingBuildingMaterial("Temple dark paving", "Slate");
    }
    // ！！！功能结束：BuildTempleMaterials！！！

    // ！！！功能：BuildMaterials——构建材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void BuildMaterials()
    {
        // 步骤 1：调用 BuildTempleMaterials：构建Temple材质。
        BuildTempleMaterials();
        // 步骤 2：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++)
            // 步骤 3：调用 MakeMaterial：Make材质。
            MakeMaterial("A242 ceramic " + (i+1).ToString("00"),
                // 步骤 4：执行 "Ceramic" + (i+1).ToString("00") + "_Albedo", null, Color.white, .48f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Ceramic" + (i+1).ToString("00") + "_Albedo", null, Color.white, .48f);
    }
    // ！！！功能结束：BuildMaterials！！！

    // ！！！功能：Model——Model！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string asset 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject Model(string asset)
    {
        // 步骤 1：计算 Base + "Models/" + asset + ".fbx"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
        string path = Base + "Models/" + asset + ".fbx";
        // 步骤 2：计算 AssetDatabase.LoadAssetAtPath<GameObject>(path)，把结果存进 prefab（prefab）；之后引用 prefab 时用的就是这次结果。
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        // 步骤 3：检查prefab是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时调用 LogError：LogError。
        if (prefab == null) Debug.LogError("Missing temple model " + path);
        // 步骤 4：把 prefab 作为方法结果返回给调用方；return 后本方法立即结束。
        return prefab;
    }
    // ！！！功能结束：Model！！！

    // ！！！功能：Paint——Paint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject instance, Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Paint(GameObject instance, Material material)
    {
        // 步骤 1：逐个读取 Renderer renderer in instance.GetComponentsInChildren<Renderer>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            // 步骤 2：计算 renderer.sharedMaterials，把结果存进 slots（slots）；之后引用 slots 时用的就是这次结果。
            Material[] slots = renderer.sharedMaterials;
            // 步骤 3：检查条件 slots.Length == 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 slots = new Material[1];
            if (slots.Length == 0) slots = new Material[1];
            // 步骤 4：按 int i = 0; i < slots.Length; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < slots.Length; i++) slots[i] = material;
            // 步骤 5：把计算结果 slots 保存为shared材质；下一条判断或调用会读取这个具体值。
            renderer.sharedMaterials = slots;
        }
    }
    // ！！！功能结束：Paint！！！

    // ！！！功能：BoundsOf——Bounds的！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject instance 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static Bounds BoundsOf(GameObject instance)
    {
        // 步骤 1：从目标物体找到所需组件并存为 renderers；使用前应检查是否为 null。
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        // 步骤 2：检查条件 renderers.Length == 0；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (renderers.Length == 0) return new Bounds(instance.transform.position, Vector3.one);
        // 步骤 3：计算 renderers[0].bounds，把结果存进 bounds（bounds）；之后引用 bounds 时用的就是这次结果。
        Bounds bounds = renderers[0].bounds;
        // 步骤 4：按 int i = 1; i < renderers.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        // 步骤 5：把 bounds 作为方法结果返回给调用方；return 后本方法立即结束。
        return bounds;
    }
    // ！！！功能结束：BoundsOf！！！

    // ！！！功能：VisibleBoundsOf——VisibleBounds的！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject instance 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static Bounds VisibleBoundsOf(GameObject instance)
    {
        // 步骤 1：把计算结果 System.Array.FindAll( 保存为renderers；下一条判断或调用会读取这个具体值。
        Renderer[] renderers = System.Array.FindAll(
            // 步骤 2：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            instance.GetComponentsInChildren<Renderer>(true), renderer => renderer.enabled);
        // 步骤 3：检查条件 renderers.Length == 0；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (renderers.Length == 0) return BoundsOf(instance);
        // 步骤 4：计算 renderers[0].bounds，把结果存进 bounds（bounds）；之后引用 bounds 时用的就是这次结果。
        Bounds bounds = renderers[0].bounds;
        // 步骤 5：按 int i = 1; i < renderers.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        // 步骤 6：把 bounds 作为方法结果返回给调用方；return 后本方法立即结束。
        return bounds;
    }
    // ！！！功能结束：VisibleBoundsOf！！！

    // ！！！功能：Fit——Fit！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject instance, float height 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Fit(GameObject instance, float height)
    {
        // 步骤 1：计算 BoundsOf(instance)，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
        Bounds b = BoundsOf(instance);
        // 步骤 2：计算 height / Mathf.Max(.001f, b.size.y)，把结果存进 factor（factor）；之后引用 factor 时用的就是这次结果。
        float factor = height / Mathf.Max(.001f, b.size.y);
        // 步骤 3：执行 instance.transform.localScale *= factor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        instance.transform.localScale *= factor;
    }
    // ！！！功能结束：Fit！！！

    // ！！！功能：Seat——Seat！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject instance, Vector3 floorPosition 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void Seat(GameObject instance, Vector3 floorPosition)
    {
        // 步骤 1：计算 BoundsOf(instance)，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
        Bounds b = BoundsOf(instance);
        // 步骤 2：这一行补充 instance.transform.position += new Vector3(，与相邻行合起来完成同一次计算或对象配置。
        instance.transform.position += new Vector3(
            // 步骤 3：继续填写上一行调用的参数：floorPosition.x - b.center.x, floorPosition.y - b.min.y；参数位置决定它表示的坐标、速度或选项。
            floorPosition.x - b.center.x, floorPosition.y - b.min.y,
            // 步骤 4：执行 floorPosition.z - b.center.z);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            floorPosition.z - b.center.z);
    }
    // ！！！功能结束：Seat！！！

    // ！！！功能：BuildCeramicPrefabs——构建Ceramic预制体！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void BuildCeramicPrefabs()
    {
        // 步骤 1：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++)
        {
            // 步骤 2：计算 (i+1).ToString("00")，把结果存进 id（id）；之后引用 id 时用的就是这次结果。
            string id = (i+1).ToString("00");
            // 步骤 3：计算 Model("A242_Ceramic" + id)，把结果存进 source（反馈跟随的来源物品）；之后引用 source 时用的就是这次结果。
            GameObject source = Model("A242_Ceramic" + id);
            // 步骤 4：检查反馈跟随的来源物品是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (source == null) continue;
            // 步骤 5：创建新的 var 并存为 root；构造参数决定其初始内容或对象名称。
            var root = new GameObject("A242 ceramic " + id);
            // 步骤 6：计算 (GameObject)PrefabUtility.InstantiatePrefab(source)，把结果存进 mesh（mesh）；之后引用 mesh 时用的就是这次结果。
            GameObject mesh = (GameObject)PrefabUtility.InstantiatePrefab(source);
            // 步骤 7：执行 mesh.transform.SetParent(root.transform, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            mesh.transform.SetParent(root.transform, true);
            // 步骤 8：调用 Unity 资源数据库处理磁盘上的场景、材质或预制体资产，并保存引用。
            Paint(mesh, AssetDatabase.LoadAssetAtPath<Material>(Base + "Materials/A242 ceramic " + id + ".mat"));
            // 步骤 9：调用 Fit：Fit。
            Fit(mesh, .48f);
            // 步骤 10：调用 Seat：Seat。
            Seat(mesh, Vector3.zero);
            // 步骤 11：计算 Base + "Prefabs/A242 ceramic " + id + ".prefab"，把结果存进 path（path）；之后引用 path 时用的就是这次结果。
            string path = Base + "Prefabs/A242 ceramic " + id + ".prefab";
            // 步骤 12：执行 ceramics[i] = PrefabUtility.SaveAsPrefabAsset(root, path);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ceramics[i] = PrefabUtility.SaveAsPrefabAsset(root, path);
            // 步骤 13：在编辑器里立即移除旧对象；重新搭建时避免上次生成内容重复叠加。
            Object.DestroyImmediate(root);
        }
    }
    // ！！！功能结束：BuildCeramicPrefabs！！！

    // ！！！功能：Block——Block！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Transform parent, Vector3 center, Vector3 size, Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject Block(string name, Transform parent, Vector3 center, Vector3 size, Material material)
    {
        // 步骤 1：计算 GameObject.CreatePrimitive(PrimitiveType.Cube)，把结果存进 cube（cube）；之后引用 cube 时用的就是这次结果。
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // 步骤 2：把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        cube.name = name;
        // 步骤 3：执行 cube.transform.SetParent(parent);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        cube.transform.SetParent(parent);
        // 步骤 4：执行 cube.transform.position = center;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        cube.transform.position = center;
        // 步骤 5：执行 cube.transform.localScale = size;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        cube.transform.localScale = size;
        // 步骤 6：调用 Paint：Paint。
        Paint(cube, material);
        // 步骤 7：把 cube 作为方法结果返回给调用方；return 后本方法立即结束。
        return cube;
    }
    // ！！！功能结束：Block！！！

    // ！！！功能：Place——Place！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Transform parent, string asset, Vector3 floor, float height, Material material, float yaw = 0f 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private static GameObject Place(string name, Transform parent, string asset, Vector3 floor, float height, Material material, float yaw = 0f)
    {
        // 步骤 1：计算 Model(asset)，把结果存进 source（反馈跟随的来源物品）；之后引用 source 时用的就是这次结果。
        GameObject source = Model(asset);
        // 步骤 2：检查反馈跟随的来源物品是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (source == null) return null;
        // 步骤 3：计算 (GameObject)PrefabUtility.InstantiatePrefab(source)，把结果存进 instance（instance）；之后引用 instance 时用的就是这次结果。
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        // 步骤 4：把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        instance.name = name;
        // 步骤 5：执行 instance.transform.SetParent(parent, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        instance.transform.SetParent(parent, true);
        // The imported Blender FBX root carries its Z-up to Unity Y-up conversion.
        // Keep that rotation when adding the planned yaw; replacing it lays the
        // statues, columns, gates and temple buildings on their sides.
        // 步骤 6：执行 instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * instance.transform.rotation;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * instance.transform.rotation;
        // 步骤 7：调用 Paint：Paint。
        Paint(instance, material);
        // 步骤 8：调用 Fit：Fit。
        Fit(instance, height);
        // 步骤 9：调用 Seat：Seat。
        Seat(instance, floor);
        // 步骤 10：把 instance 作为方法结果返回给调用方；return 后本方法立即结束。
        return instance;
    }
    // ！！！功能结束：Place！！！

    // ！！！功能：WarmLight——Warm灯光！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Transform parent, Vector3 pos, float range, float intensity 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void WarmLight(string name, Transform parent, Vector3 pos, float range, float intensity)
    {
        // 步骤 1：创建新的 var 并存为 glow；构造参数决定其初始内容或对象名称。
        var glow = new GameObject(name);
        // 步骤 2：执行 glow.transform.SetParent(parent);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        glow.transform.SetParent(parent);
        // 步骤 3：执行 glow.transform.position = pos;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        glow.transform.position = pos;
        // 步骤 4：计算 glow.AddComponent<Light>()，把结果存进 light（light）；之后引用 light 时用的就是这次结果。
        var light = glow.AddComponent<Light>();
        // 步骤 5：把计算结果 LightType.Point 保存为type；下一条判断或调用会读取这个具体值。
        light.type = LightType.Point;
        // 步骤 6：新建用于color的对象或数值结构，并把构造参数保存进去。
        light.color = new Color(1f,.65f,.32f);
        // 步骤 7：把计算结果 range 保存为range；下一条判断或调用会读取这个具体值。
        light.range = range;
        // 步骤 8：把计算结果 intensity 保存为intensity；下一条判断或调用会读取这个具体值。
        light.intensity = intensity;
        // 步骤 9：把计算结果 LightShadows.None 保存为shadows；下一条判断或调用会读取这个具体值。
        light.shadows = LightShadows.None;
    }
    // ！！！功能结束：WarmLight！！！

    // ！！！功能：IntegrateScene——Integrate场景！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string path 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void IntegrateScene(string path)
    {
        // 步骤 1：计算 EditorSceneManager.OpenScene(path, OpenSceneMode.Single)，把结果存进 scene（scene）；之后引用 scene 时用的就是这次结果。
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        // 步骤 2：逐个读取 GameObject old in scene.GetRootGameObjects() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (GameObject old in scene.GetRootGameObjects())
            // 步骤 3：检查条件 old.name == RootName；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 DestroyImmediate：DestroyImmediate。
            if (old.name == RootName) Object.DestroyImmediate(old);

        // 步骤 4：创建新的 var 并存为 complex；构造参数决定其初始内容或对象名称。
        var complex = new GameObject(RootName);
        // 步骤 5：调用 MoveGameObjectToScene：MoveGame物品到场景。
        SceneManager.MoveGameObjectToScene(complex, scene);
        // 步骤 6：取目标对象的 Transform 并存为 root；Transform 提供位置、旋转和父子关系。
        Transform root = complex.transform;
        // 步骤 7：调用 Block：Block。
        Block("Bridge from existing eastern causeway", root, new Vector3(44f,.075f,37f),
            // 步骤 8：用 new 创建 Vector3(7f,.15f,5.5f), sandstone);；构造参数决定新对象的初始内容。
            new Vector3(7f,.15f,5.5f), sandstone);
        // 步骤 9：调用 Block：Block。
        Block("Ancient temple walkable court", root, new Vector3(56f,.055f,27f),
            // 步骤 10：用 new 创建 Vector3(24f,.11f,28f), sandstone);；构造参数决定新对象的初始内容。
            new Vector3(24f,.11f,28f), sandstone);
        // 步骤 11：调用 Block：Block。
        Block("Central ritual path", root, new Vector3(56f,.12f,27f),
            // 步骤 12：用 new 创建 Vector3(4.2f,.018f,22f), darkStone);；构造参数决定新对象的初始内容。
            new Vector3(4.2f,.018f,22f), darkStone);
        // 步骤 13：按 int ix = 0; ix < 4; ix++ 变化索引；循环体依次处理每个位置。
        for (int ix = 0; ix < 4; ix++)
            // 步骤 14：按 int iz = 0; iz < 5; iz++ 变化索引；循环体依次处理每个位置。
            for (int iz = 0; iz < 5; iz++)
                // 步骤 15：调用 Block：Block。
                Block("Temple paving inset", root,
                    // 步骤 16：用 new 创建 Vector3(47.25f + ix * 5.85f,.12f,15.8f + iz * 5.5f),；构造参数决定新对象的初始内容。
                    new Vector3(47.25f + ix * 5.85f,.12f,15.8f + iz * 5.5f),
                    // 步骤 17：用 new 创建 Vector3(5.3f,.015f,4.95f), ix % 2 == iz % 2 ? stone : sandstone);；构造参数决定新对象的初始内容。
                    new Vector3(5.3f,.015f,4.95f), ix % 2 == iz % 2 ? stone : sandstone);
        // 步骤 18：按 int i = 0; i < 2; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 2; i++)
        {
            // 步骤 19：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = i == 0 ? 47f : 65f;
            // 步骤 20：调用 Place：Place。
            Place("A301 stone arch " + (i+1), root, "A301_RitualArch",
                // 步骤 21：用 new 创建 Vector3(x,.13f,34.5f), 5.3f, stone, i == 0 ? 90f : -90f);；构造参数决定新对象的初始内容。
                new Vector3(x,.13f,34.5f), 5.3f, stone, i == 0 ? 90f : -90f);
            // 步骤 22：调用 Place：Place。
            Place("A301 carved column " + (i+1), root, "A301_RitualPillar",
                // 步骤 23：用 new 创建 Vector3(x,.13f,19f), 4.7f, stone);；构造参数决定新对象的初始内容。
                new Vector3(x,.13f,19f), 4.7f, stone);
            // 步骤 24：调用 Place：Place。
            Place("A301 carved column " + (i+3), root, "A301_RitualPillar",
                // 步骤 25：用 new 创建 Vector3(x,.13f,28f), 4.7f, stone);；构造参数决定新对象的初始内容。
                new Vector3(x,.13f,28f), 4.7f, stone);
        }
        // 步骤 26：调用 Place：Place。
        Place("A301 ritual gateway", root, "A301_RitualGate",
            // 步骤 27：用 new 创建 Vector3(56f,.13f,37.5f), 4.9f, stone);；构造参数决定新对象的初始内容。
            new Vector3(56f,.13f,37.5f), 4.9f, stone);
        // 步骤 28：调用 Place：Place。
        Place("A301 ceremonial floor", root, "A301_RitualFloor",
            // 步骤 29：用 new 创建 Vector3(56f,.14f,25.5f), .12f, stone);；构造参数决定新对象的初始内容。
            new Vector3(56f,.14f,25.5f), .12f, stone);
        // 步骤 30：调用 Block：Block。
        Block("Main temple terrace", root, new Vector3(56f,.29f,14.9f),
            // 步骤 31：用 new 创建 Vector3(12.5f,.46f,8.8f), darkStone);；构造参数决定新对象的初始内容。
            new Vector3(12.5f,.46f,8.8f), darkStone);
        // 步骤 32：调用 Block：Block。
        Block("Main terrace bronze edge", root, new Vector3(56f,.53f,19.2f),
            // 步骤 33：用 new 创建 Vector3(12.5f,.05f,.12f), gold);；构造参数决定新对象的初始内容。
            new Vector3(12.5f,.05f,.12f), gold);
        // 步骤 34：调用 Place：Place。
        Place("A427 main Chinese temple", root, "A427_TempleLarge",
            // 步骤 35：用 new 创建 Vector3(56f,.53f,13.8f), 6.7f, sandstone, 180f);；构造参数决定新对象的初始内容。
            new Vector3(56f,.53f,13.8f), 6.7f, sandstone, 180f);
        // 步骤 36：调用 Place：Place。
        Place("A427 west shrine", root, "A427_TempleSmall",
            // 步骤 37：用 new 创建 Vector3(47f,.13f,17.1f), 4.2f, sandstone, 140f);；构造参数决定新对象的初始内容。
            new Vector3(47f,.13f,17.1f), 4.2f, sandstone, 140f);
        // 步骤 38：调用 Place：Place。
        Place("A427 east shrine", root, "A427_TempleSmall",
            // 步骤 39：用 new 创建 Vector3(65f,.13f,17.1f), 4.2f, sandstone, -140f);；构造参数决定新对象的初始内容。
            new Vector3(65f,.13f,17.1f), 4.2f, sandstone, -140f);
        // 步骤 40：调用 Place：Place。
        Place("A427 entry platform", root, "A427_TemplePlatform",
            // 步骤 41：用 new 创建 Vector3(56f,.14f,20f), .3f, wood);；构造参数决定新对象的初始内容。
            new Vector3(56f,.14f,20f), .3f, wood);
        // 步骤 42：调用 Place：Place。
        Place("A427 entry stairs", root, "A427_TempleStairs",
            // 步骤 43：用 new 创建 Vector3(56f,.14f,21f), .34f, wood);；构造参数决定新对象的初始内容。
            new Vector3(56f,.14f,21f), .34f, wood);
        // 步骤 44：调用 Block：Block。
        Block("Ancestral statue pedestal", root, new Vector3(56f,.3f,25.3f),
            // 步骤 45：用 new 创建 Vector3(2.4f,.42f,2.4f), darkStone);；构造参数决定新对象的初始内容。
            new Vector3(2.4f,.42f,2.4f), darkStone);
        // 步骤 46：调用 Place：Place。
        Place("A301 ancestral carved statue", root, "A301_RitualStatue",
            // 步骤 47：用 new 创建 Vector3(56f,.52f,25.3f), 2.8f, statue, 180f);；构造参数决定新对象的初始内容。
            new Vector3(56f,.52f,25.3f), 2.8f, statue, 180f);
        // 步骤 48：按 int i = 0; i < 4; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 4; i++)
        {
            // 步骤 49：计算 47.5f + i * 5.7f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = 47.5f + i * 5.7f;
            // 步骤 50：调用 Place：Place。
            Place("A301 offering post " + (i+1), root, "A301_RitualPost",
                // 步骤 51：用 new 创建 Vector3(x,.13f,39f), 1.45f, stone);；构造参数决定新对象的初始内容。
                new Vector3(x,.13f,39f), 1.45f, stone);
            // 步骤 52：调用 WarmLight：Warm灯光。
            WarmLight("Offering lamp glow", root, new Vector3(x,2.3f,39f), 7f, 1.35f);
        }
        // 步骤 53：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++)
        {
            // 步骤 54：计算 i / 4, col = i % 4，把结果存进 row（row）；之后引用 row 时用的就是这次结果。
            int row = i / 4, col = i % 4;
            // 步骤 55：新建用于display的对象或数值结构，并把构造参数保存进去。
            Vector3 display = new Vector3(51.1f + col * 3.25f, .13f,
                // 步骤 56：把计算结果 = 0 ? 20.8f : 34.5f) 保存为row；下一条判断或调用会读取这个具体值。
                row == 0 ? 20.8f : 34.5f);
            // 步骤 57：调用 Block：Block。
            Block("A242 ceramic display plinth " + (i+1), root,
                // 步骤 58：执行 display + Vector3.up * .42f, new Vector3(.85f,.82f,.85f), darkStone);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                display + Vector3.up * .42f, new Vector3(.85f,.82f,.85f), darkStone);
            // 步骤 59：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (ceramics[i] != null)
            {
                // 步骤 60：计算 (GameObject)PrefabUtility.InstantiatePrefab(ceramics[i])，把结果存进 item（item）；之后引用 item 时用的就是这次结果。
                GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(ceramics[i]);
                // 步骤 61：把计算结果 "A242 ceramic display " + (i+1) 保存为name；下一条判断或调用会读取这个具体值。
                item.name = "A242 ceramic display " + (i+1);
                // 步骤 62：执行 item.transform.SetParent(root, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                item.transform.SetParent(root, true);
                // 步骤 63：执行 item.transform.position = display + Vector3.up * .84f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                item.transform.position = display + Vector3.up * .84f;
                // 步骤 64：执行 item.transform.localScale = Vector3.one * 1.12f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                item.transform.localScale = Vector3.one * 1.12f;
            }
        }
        // The pre-existing mossy SkyGate stood in the middle of the new
        // ceremonial axis, hiding the ancestor statue and main temple.
        // Reuse it as a side relic while keeping the object and its components.
        // 步骤 65：计算 GameObject.Find("SkyGateModel")，把结果存进 skyGate（skyGate）；之后引用 skyGate 时用的就是这次结果。
        GameObject skyGate = GameObject.Find("SkyGateModel");
        // 步骤 66：确认skyGate已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (skyGate != null)
        {
            // 步骤 67：调用 Block：Block。
            Block("Original sky gate relic plinth", root,
                // 步骤 68：用 new 创建 Vector3(42.5f,.26f,30.5f), new Vector3(2.8f,.28f,2.8f), darkStone);；构造参数决定新对象的初始内容。
                new Vector3(42.5f,.26f,30.5f), new Vector3(2.8f,.28f,2.8f), darkStone);
            // 步骤 69：执行 skyGate.transform.rotation = Quaternion.Euler(0f,25f,0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            skyGate.transform.rotation = Quaternion.Euler(0f,25f,0f);
            // 步骤 70：调用 Fit：Fit。
            Fit(skyGate, 3.1f);
            // 步骤 71：调用 Seat：Seat。
            Seat(skyGate, new Vector3(42.5f,.4f,30.5f));
        }
        // 步骤 72：调用 MoveAncestors：MoveAncestors。
        MoveAncestors(root);
        // 步骤 73：调用 ValidateTempleScene：验证Temple场景。
        ValidateTempleScene(root);
        // 步骤 74：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.MarkSceneDirty(scene);
        // 步骤 75：调用 Unity 编辑器的场景管理接口，对编辑中的场景进行创建、载入或保存。
        EditorSceneManager.SaveScene(scene);
    }
    // ！！！功能结束：IntegrateScene！！！

    // ！！！功能：ValidateTempleScene——验证Temple场景！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ValidateTempleScene(Transform root)
    {
        // 步骤 1：逐个读取 string name in new[] { "A301 carved column 1", "A301 ancestral carved statue" } 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (string name in new[] { "A301 carved column 1", "A301 ancestral carved statue" })
        {
            // 步骤 2：计算 root.Find(name)，把结果存进 model（model）；之后引用 model 时用的就是这次结果。
            Transform model = root.Find(name);
            // 步骤 3：检查model是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new System.InvalidOperationException("Missing temple piece: " + name)
            if (model == null) throw new System.InvalidOperationException("Missing temple piece: " + name);
            // 步骤 4：计算 BoundsOf(model.gameObject).size，把结果存进 size（size）；之后引用 size 时用的就是这次结果。
            Vector3 size = BoundsOf(model.gameObject).size;
            // 步骤 5：检查条件 size.y < Mathf.Max(size.x, size.z) * 1.15f；只有为真才运行下面的步骤，否则沿原流程继续。
            if (size.y < Mathf.Max(size.x, size.z) * 1.15f)
                // 步骤 6：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                throw new System.InvalidOperationException(name + " is still sideways: " + size);
        }
        // 步骤 7：逐个读取 string name in new[] { "A427 main Chinese temple", "A427 west shrine", "A427 east shr 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (string name in new[] { "A427 main Chinese temple", "A427 west shrine", "A427 east shrine" })
        {
            // 步骤 8：计算 root.Find(name)，把结果存进 model（model）；之后引用 model 时用的就是这次结果。
            Transform model = root.Find(name);
            // 步骤 9：检查model是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
            if (model == null || BoundsOf(model.gameObject).size.y < 3.5f)
                // 步骤 10：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                throw new System.InvalidOperationException("Temple building is missing or sideways: " + name);
        }
        // 步骤 11：计算 Object.FindFirstObjectByType<SkyQuestMagicExchange>()，把结果存进 exchange（exchange）；之后引用 exchange 时用的就是这次结果。
        SkyQuestMagicExchange exchange = Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        // 步骤 12：检查exchange是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new System.InvalidOperationException("Missing ancestor exchange");
        if (exchange == null) throw new System.InvalidOperationException("Missing ancestor exchange");
        // 步骤 13：按 int i = 0; i < ceramics.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < ceramics.Length; i++)
        {
            // 步骤 14：计算 exchange.ceramicRewardPrefabs[i]，把结果存进 ceramic（ceramic）；之后引用 ceramic 时用的就是这次结果。
            GameObject ceramic = exchange.ceramicRewardPrefabs[i];
            // 步骤 15：检查ceramic是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new System.InvalidOperationException("Missing A242 ceramic " + (i + 1
            if (ceramic == null) throw new System.InvalidOperationException("Missing A242 ceramic " + (i + 1));
            // 步骤 16：从目标物体找到所需组件并存为 renderer；使用前应检查是否为 null。
            Renderer renderer = ceramic.GetComponentInChildren<Renderer>(true);
            // 步骤 17：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (renderer == null || renderer.sharedMaterial == null ||
                // 步骤 18：这一行补充 renderer.sharedMaterial.GetTexture("_BaseMap") == null)，与相邻行合起来完成同一次计算或对象配置。
                renderer.sharedMaterial.GetTexture("_BaseMap") == null)
                // 步骤 19：遇到无法继续的状态时抛出异常，让 Unity 日志指出具体失败位置。
                throw new System.InvalidOperationException("Missing A242 ceramic texture " + (i + 1));
        }
    }
    // ！！！功能结束：ValidateTempleScene！！！

    // ！！！功能：MoveAncestors——MoveAncestors！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform root 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void MoveAncestors(Transform root)
    {
        // 步骤 1：计算 Object.FindFirstObjectByType<SkyQuestMagicExchange>()，把结果存进 exchange（exchange）；之后引用 exchange 时用的就是这次结果。
        SkyQuestMagicExchange exchange = Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        // 步骤 2：检查exchange是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 { Debug.LogError("No SkyQuestMagicExchange found in scene"); return; }
        if (exchange == null) { Debug.LogError("No SkyQuestMagicExchange found in scene"); return; }
        // 步骤 3：把计算结果 { 保存为spots；下一条判断或调用会读取这个具体值。
        Vector3[] spots = {
            // 步骤 4：用 new 创建 Vector3(49f,.14f,30f), new Vector3(63f,.14f,30f),；构造参数决定新对象的初始内容。
            new Vector3(49f,.14f,30f), new Vector3(63f,.14f,30f),
            // 步骤 5：用 new 创建 Vector3(49f,.14f,24f), new Vector3(63f,.14f,24f),；构造参数决定新对象的初始内容。
            new Vector3(49f,.14f,24f), new Vector3(63f,.14f,24f),
            // 步骤 6：用 new 创建 Vector3(51f,.14f,18.3f), new Vector3(61f,.14f,18.3f)；构造参数决定新对象的初始内容。
            new Vector3(51f,.14f,18.3f), new Vector3(61f,.14f,18.3f)
        };
        // 步骤 7：按 int i = 0; i < 6 && i < exchange.ancestors.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 6 && i < exchange.ancestors.Length; i++)
        {
            // 步骤 8：计算 exchange.ancestors[i]，把结果存进 spirit（spirit）；之后引用 spirit 时用的就是这次结果。
            Transform spirit = exchange.ancestors[i];
            // 步骤 9：检查spirit是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (spirit == null) continue;
            // 步骤 10：计算 spots[i]，把结果存进 spot（spot）；之后引用 spot 时用的就是这次结果。
            Vector3 spot = spots[i];
            // 步骤 11：用三元表达式为 platformTop 二选一：问号前条件为真取前值，为假取冒号后的值。
            float platformTop = i < 2 ? .65f : .24f;
            // 步骤 12：调用 Block：Block。
            Block("Ancestor " + (i+1) + " shrine", root,
                // 步骤 13：用 new 创建 Vector3(spot.x, (.14f + platformTop) * .5f, spot.z),；构造参数决定新对象的初始内容。
                new Vector3(spot.x, (.14f + platformTop) * .5f, spot.z),
                // 步骤 14：用 new 创建 Vector3(2.15f, platformTop - .14f, 2.15f), darkStone);；构造参数决定新对象的初始内容。
                new Vector3(2.15f, platformTop - .14f, 2.15f), darkStone);
            // 步骤 15：把position更新为 spot + Vector3.up * platformTop；世界或局部位置会立即影响物体摆放。
            spirit.position = spot + Vector3.up * platformTop;
            // 步骤 16：新建用于rotation的对象或数值结构，并把构造参数保存进去。
            spirit.rotation = Quaternion.LookRotation(new Vector3(56f,0f,29f) -
                // 步骤 17：用 new 创建 Vector3(spot.x,0f,spot.z), Vector3.up);；构造参数决定新对象的初始内容。
                new Vector3(spot.x,0f,spot.z), Vector3.up);
            // 步骤 18：计算 VisibleBoundsOf(spirit.gameObject)，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
            Bounds b = VisibleBoundsOf(spirit.gameObject);
            // 步骤 19：把position更新为 Vector3.up * (platformTop - b.min.y)；世界或局部位置会立即影响物体摆放。
            spirit.position += Vector3.up * (platformTop - b.min.y);
            // 步骤 20：检查exchange的memorials是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (exchange.memorials != null && i < exchange.memorials.Length && exchange.memorials[i] != null)
            {
                // 步骤 21：计算 exchange.memorials[i]，把结果存进 memorial（memorial）；之后引用 memorial 时用的就是这次结果。
                Transform memorial = exchange.memorials[i];
                // 步骤 22：把原三维朝向投影到地面平面并存为 forward；行走或转向不受头部俯仰影响。
                Vector3 forward = Vector3.ProjectOnPlane(spirit.forward, Vector3.up).normalized;
                // 步骤 23：把position更新为 spirit.position - forward * .95f；世界或局部位置会立即影响物体摆放。
                memorial.position = spirit.position - forward * .95f;
                // 步骤 24：新建用于position的对象或数值结构，并把构造参数保存进去。
                memorial.position = new Vector3(memorial.position.x, spot.y, memorial.position.z);
                // 步骤 25：把rotation更新为 spirit.rotation；物体朝向会在本帧改变。
                memorial.rotation = spirit.rotation;
            }
        }
        // Keep the two screenshot ancestors facing the visitor after rebuilding
        // the court. Their memorials were already placed behind this direction.
        // 步骤 26：调用 FaceScreenshotAncestors：Face截图Ancestors。
        FaceScreenshotAncestors(exchange);
        // 步骤 27：按 int i = 0; i < 8; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 8; i++) exchange.ceramicRewardPrefabs[i] = ceramics[i];
        // 步骤 28：把计算结果 4.5f 保存为proximity；下一条判断或调用会读取这个具体值。
        exchange.proximity = 4.5f;
        // 步骤 29：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
        EditorUtility.SetDirty(exchange);

        // 步骤 30：计算 Object.FindFirstObjectByType<SkyQuestAncestor>()，把结果存进 legacy（legacy）；之后引用 legacy 时用的就是这次结果。
        SkyQuestAncestor legacy = Object.FindFirstObjectByType<SkyQuestAncestor>();
        // 步骤 31：检查legacy、legacy的ancestor是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (legacy != null && legacy.ancestor != null)
        {
            // 步骤 32：创建新的 Vector3 并存为 center；构造参数决定其初始内容或对象名称。
            Vector3 center = new Vector3(56f,.14f,31f);
            // 步骤 33：调用 Block：Block。
            Block("First ancestor central shrine", root,
                // 步骤 34：执行 center + Vector3.up * .11f, new Vector3(2.4f,.2f,2.4f), darkStone);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                center + Vector3.up * .11f, new Vector3(2.4f,.2f,2.4f), darkStone);
            // 步骤 35：执行 legacy.ancestor.position = center + Vector3.up * .21f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            legacy.ancestor.position = center + Vector3.up * .21f;
            // 步骤 36：计算 VisibleBoundsOf(legacy.ancestor.gameObject)，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
            Bounds b = VisibleBoundsOf(legacy.ancestor.gameObject);
            // 步骤 37：执行 legacy.ancestor.position += Vector3.up * (.24f - b.min.y);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            legacy.ancestor.position += Vector3.up * (.24f - b.min.y);
            // 步骤 38：执行 legacy.ancestor.rotation = Quaternion.Euler(0f,180f,0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            legacy.ancestor.rotation = Quaternion.Euler(0f,180f,0f);
            // 步骤 39：确认legacy的memorialStone已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (legacy.memorialStone != null)
            {
                // 步骤 40：这一行补充 legacy.memorialStone.position = new Vector3(，与相邻行合起来完成同一次计算或对象配置。
                legacy.memorialStone.position = new Vector3(
                    // 步骤 41：继续填写上一行调用的参数：legacy.ancestor.position.x；参数位置决定它表示的坐标、速度或选项。
                    legacy.ancestor.position.x,
                    // 步骤 42：继续填写上一行调用的参数：center.y；参数位置决定它表示的坐标、速度或选项。
                    center.y,
                    // 步骤 43：执行 legacy.ancestor.position.z + .95f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    legacy.ancestor.position.z + .95f);
                // 步骤 44：执行 legacy.memorialStone.rotation = legacy.ancestor.rotation;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                legacy.memorialStone.rotation = legacy.ancestor.rotation;
            }
            // 步骤 45：告知 Unity 当前资源已修改；保存项目时才会把这些修改写回磁盘。
            EditorUtility.SetDirty(legacy);
        }
    }
    // ！！！功能结束：MoveAncestors！！！
}
