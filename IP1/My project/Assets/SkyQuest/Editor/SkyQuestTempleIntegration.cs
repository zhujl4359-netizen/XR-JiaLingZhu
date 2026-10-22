using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Installs the optimized pieces exported from the three supplied Blender projects.
// Existing level geometry and gameplay objects are retained.
public static class SkyQuestTempleIntegration
{
    private const string Base = "Assets/SkyQuest/TempleIntegration/";
    private const string RootName = "Ancient temple complex — A301 + A427";
    private static Material stone, statue, sandstone, wood, gold, darkStone;
    private static readonly GameObject[] ceramics = new GameObject[8];

    [MenuItem("SKY/Frame ancient temple in Scene view")]
    public static void FrameAncientTemple()
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view == null) return;
        view.LookAt(new Vector3(56f, 1.8f, 26f), Quaternion.Euler(25f, 180f, 0f), 17f);
        view.Repaint();
    }

    [MenuItem("SKY/Integrate A301 A427 temples and A242 ceramics")]
    public static void Integrate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        AssetDatabase.Refresh();
        EnsureFolder(Base + "Materials");
        EnsureFolder(Base + "Prefabs");
        BuildMaterials();
        BuildCeramicPrefabs();
        IntegrateScene("Assets/SkyQuest/SkyQuest3S.unity");
        IntegrateScene("Assets/SkyQuest/SkyDesktop.unity");
        AssetDatabase.SaveAssets();
        Debug.Log("A301 + A427 ancient temple and all eight A242 ceramic rewards installed in both SkyQuest scenes.");
    }

    [MenuItem("SKY/Match ancient temple colors to existing buildings")]
    public static void MatchTempleColors()
    {
        BuildTempleMaterials();
        AssetDatabase.SaveAssets();
        Debug.Log("Ancient temple stone, timber and trim now use the existing building palette.");
    }

    [MenuItem("SKY/Face COTL and C1 ancestors toward the visitor")]
    public static void FaceScreenshotAncestors()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        foreach (string path in new[] {
            "Assets/SkyQuest/SkyQuest3S.unity", "Assets/SkyQuest/SkyDesktop.unity" })
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            SkyQuestMagicExchange exchange = Object.FindFirstObjectByType<SkyQuestMagicExchange>();
            if (exchange == null) throw new System.InvalidOperationException("Missing ancestor exchange in " + path);
            FaceScreenshotAncestors(exchange);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        EditorSceneManager.OpenScene("Assets/SkyQuest/SkyQuest3S.unity", OpenSceneMode.Single);
        Debug.Log("COTL and C1 ancestors now face visitors in both scenes.");
    }

    private static void FaceScreenshotAncestors(SkyQuestMagicExchange exchange)
    {
        // The visitor approaches these rear shrines from the south aisle.
        Vector3 visitor = new Vector3(56f, 0f, 10f);
        foreach (int index in new[] { 2, 5 })
        {
            if (exchange.ancestors == null || index >= exchange.ancestors.Length ||
                exchange.ancestors[index] == null)
                throw new System.InvalidOperationException("Missing gallery ancestor " + index);
            Transform spirit = exchange.ancestors[index];
            Vector3 direction = visitor - spirit.position;
            direction.y = 0f;
            // COTL faces local -Z. C1's nested visual is already turned 180
            // degrees relative to its root, so its root must face the visitor.
            Quaternion faceVisitor = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Bounds before = VisibleBoundsOf(spirit.gameObject);
            spirit.rotation = index == 2 ?
                faceVisitor * Quaternion.Euler(0f, 180f, 0f) : faceVisitor;
            // C1's imported pivot sits far from its mesh. Keep the actual figure
            // on its shrine while rotating the root used by interaction logic.
            if (index == 5)
            {
                Transform model = spirit.Find("C1");
                if (model == null) throw new System.InvalidOperationException("Missing C1 visual model");
                Vector3 correction = before.center - VisibleBoundsOf(spirit.gameObject).center;
                correction.y = 0f;
                model.position += correction;
                EditorUtility.SetDirty(model);
            }
            EditorUtility.SetDirty(spirit);
            if (exchange.memorials == null || index >= exchange.memorials.Length ||
                exchange.memorials[index] == null)
                throw new System.InvalidOperationException("Missing gallery memorial " + index);
            Transform memorial = exchange.memorials[index];
            Vector3 behind = spirit.position - direction.normalized * .95f;
            memorial.position = new Vector3(behind.x, memorial.position.y, behind.z);
            memorial.rotation = faceVisitor;
            EditorUtility.SetDirty(memorial);
        }
    }

    private static void EnsureFolder(string path)
    {
        string parent = path.TrimEnd('/');
        if (AssetDatabase.IsValidFolder(parent)) return;
        int slash = parent.LastIndexOf('/');
        AssetDatabase.CreateFolder(parent.Substring(0, slash), parent.Substring(slash + 1));
    }

    private static Texture2D Texture(string name, bool normal = false)
    {
        string path = Base + "Textures/" + name + ".png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) { Debug.LogError("Missing temple texture " + path); return null; }
        TextureImporterType desired = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        if (importer.textureType != desired || importer.maxTextureSize != 1024 || !importer.mipmapEnabled)
        {
            importer.textureType = desired;
            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Material MakeMaterial(string name, string albedo, string normal, Color tint, float smoothness)
    {
        string path = Base + "Materials/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", tint);
        material.SetColor("_Color", tint);
        if (albedo != null)
        {
            Texture2D map = Texture(albedo);
            material.SetTexture("_BaseMap", map);
            material.SetTexture("_MainTex", map);
        }
        if (normal != null)
        {
            material.SetTexture("_BumpMap", Texture(normal, true));
            material.EnableKeyword("_NORMALMAP");
        }
        material.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material MatchExistingBuildingMaterial(string templeName, string buildingName)
    {
        string path = Base + "Materials/" + templeName + ".mat";
        Material building = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/SkyQuest/Environment/Materials/" + buildingName + ".mat");
        if (building == null)
            throw new System.InvalidOperationException("Missing existing building material: " + buildingName);
        Material temple = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (temple == null)
        {
            temple = new Material(building.shader);
            AssetDatabase.CreateAsset(temple, path);
        }
        // Copy into the temple's own material asset. Existing architecture and
        // the A242 ceramics keep their original materials and appearance.
        temple.shader = building.shader;
        temple.CopyPropertiesFromMaterial(building);
        temple.shaderKeywords = building.shaderKeywords;
        EditorUtility.SetDirty(temple);
        return temple;
    }

    private static void BuildTempleMaterials()
    {
        stone = MatchExistingBuildingMaterial("A301 aged ritual stone", "BlueStone");
        statue = MatchExistingBuildingMaterial("A301 carved statue", "Ivory");
        sandstone = MatchExistingBuildingMaterial("A427 warm sandstone", "WarmLimestone");
        wood = MatchExistingBuildingMaterial("A427 timber", "HoneyWood");
        gold = MatchExistingBuildingMaterial("Temple bronze trim", "PaleGold");
        darkStone = MatchExistingBuildingMaterial("Temple dark paving", "Slate");
    }

    private static void BuildMaterials()
    {
        BuildTempleMaterials();
        for (int i = 0; i < 8; i++)
            MakeMaterial("A242 ceramic " + (i+1).ToString("00"),
                "Ceramic" + (i+1).ToString("00") + "_Albedo", null, Color.white, .48f);
    }

    private static GameObject Model(string asset)
    {
        string path = Base + "Models/" + asset + ".fbx";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) Debug.LogError("Missing temple model " + path);
        return prefab;
    }

    private static void Paint(GameObject instance, Material material)
    {
        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            Material[] slots = renderer.sharedMaterials;
            if (slots.Length == 0) slots = new Material[1];
            for (int i = 0; i < slots.Length; i++) slots[i] = material;
            renderer.sharedMaterials = slots;
        }
    }

    private static Bounds BoundsOf(GameObject instance)
    {
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(instance.transform.position, Vector3.one);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static Bounds VisibleBoundsOf(GameObject instance)
    {
        Renderer[] renderers = System.Array.FindAll(
            instance.GetComponentsInChildren<Renderer>(true), renderer => renderer.enabled);
        if (renderers.Length == 0) return BoundsOf(instance);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static void Fit(GameObject instance, float height)
    {
        Bounds b = BoundsOf(instance);
        float factor = height / Mathf.Max(.001f, b.size.y);
        instance.transform.localScale *= factor;
    }

    private static void Seat(GameObject instance, Vector3 floorPosition)
    {
        Bounds b = BoundsOf(instance);
        instance.transform.position += new Vector3(
            floorPosition.x - b.center.x, floorPosition.y - b.min.y,
            floorPosition.z - b.center.z);
    }

    private static void BuildCeramicPrefabs()
    {
        for (int i = 0; i < 8; i++)
        {
            string id = (i+1).ToString("00");
            GameObject source = Model("A242_Ceramic" + id);
            if (source == null) continue;
            var root = new GameObject("A242 ceramic " + id);
            GameObject mesh = (GameObject)PrefabUtility.InstantiatePrefab(source);
            mesh.transform.SetParent(root.transform, true);
            Paint(mesh, AssetDatabase.LoadAssetAtPath<Material>(Base + "Materials/A242 ceramic " + id + ".mat"));
            Fit(mesh, .48f);
            Seat(mesh, Vector3.zero);
            string path = Base + "Prefabs/A242 ceramic " + id + ".prefab";
            ceramics[i] = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }
    }

    private static GameObject Block(string name, Transform parent, Vector3 center, Vector3 size, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = center;
        cube.transform.localScale = size;
        Paint(cube, material);
        return cube;
    }

    private static GameObject Place(string name, Transform parent, string asset, Vector3 floor, float height, Material material, float yaw = 0f)
    {
        GameObject source = Model(asset);
        if (source == null) return null;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = name;
        instance.transform.SetParent(parent, true);
        // The imported Blender FBX root carries its Z-up to Unity Y-up conversion.
        // Keep that rotation when adding the planned yaw; replacing it lays the
        // statues, columns, gates and temple buildings on their sides.
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * instance.transform.rotation;
        Paint(instance, material);
        Fit(instance, height);
        Seat(instance, floor);
        return instance;
    }

    private static void WarmLight(string name, Transform parent, Vector3 pos, float range, float intensity)
    {
        var glow = new GameObject(name);
        glow.transform.SetParent(parent);
        glow.transform.position = pos;
        var light = glow.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f,.65f,.32f);
        light.range = range;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
    }

    private static void IntegrateScene(string path)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        foreach (GameObject old in scene.GetRootGameObjects())
            if (old.name == RootName) Object.DestroyImmediate(old);

        var complex = new GameObject(RootName);
        SceneManager.MoveGameObjectToScene(complex, scene);
        Transform root = complex.transform;
        Block("Bridge from existing eastern causeway", root, new Vector3(44f,.075f,37f),
            new Vector3(7f,.15f,5.5f), sandstone);
        Block("Ancient temple walkable court", root, new Vector3(56f,.055f,27f),
            new Vector3(24f,.11f,28f), sandstone);
        Block("Central ritual path", root, new Vector3(56f,.12f,27f),
            new Vector3(4.2f,.018f,22f), darkStone);
        for (int ix = 0; ix < 4; ix++)
            for (int iz = 0; iz < 5; iz++)
                Block("Temple paving inset", root,
                    new Vector3(47.25f + ix * 5.85f,.12f,15.8f + iz * 5.5f),
                    new Vector3(5.3f,.015f,4.95f), ix % 2 == iz % 2 ? stone : sandstone);
        for (int i = 0; i < 2; i++)
        {
            float x = i == 0 ? 47f : 65f;
            Place("A301 stone arch " + (i+1), root, "A301_RitualArch",
                new Vector3(x,.13f,34.5f), 5.3f, stone, i == 0 ? 90f : -90f);
            Place("A301 carved column " + (i+1), root, "A301_RitualPillar",
                new Vector3(x,.13f,19f), 4.7f, stone);
            Place("A301 carved column " + (i+3), root, "A301_RitualPillar",
                new Vector3(x,.13f,28f), 4.7f, stone);
        }
        Place("A301 ritual gateway", root, "A301_RitualGate",
            new Vector3(56f,.13f,37.5f), 4.9f, stone);
        Place("A301 ceremonial floor", root, "A301_RitualFloor",
            new Vector3(56f,.14f,25.5f), .12f, stone);
        Block("Main temple terrace", root, new Vector3(56f,.29f,14.9f),
            new Vector3(12.5f,.46f,8.8f), darkStone);
        Block("Main terrace bronze edge", root, new Vector3(56f,.53f,19.2f),
            new Vector3(12.5f,.05f,.12f), gold);
        Place("A427 main Chinese temple", root, "A427_TempleLarge",
            new Vector3(56f,.53f,13.8f), 6.7f, sandstone, 180f);
        Place("A427 west shrine", root, "A427_TempleSmall",
            new Vector3(47f,.13f,17.1f), 4.2f, sandstone, 140f);
        Place("A427 east shrine", root, "A427_TempleSmall",
            new Vector3(65f,.13f,17.1f), 4.2f, sandstone, -140f);
        Place("A427 entry platform", root, "A427_TemplePlatform",
            new Vector3(56f,.14f,20f), .3f, wood);
        Place("A427 entry stairs", root, "A427_TempleStairs",
            new Vector3(56f,.14f,21f), .34f, wood);
        Block("Ancestral statue pedestal", root, new Vector3(56f,.3f,25.3f),
            new Vector3(2.4f,.42f,2.4f), darkStone);
        Place("A301 ancestral carved statue", root, "A301_RitualStatue",
            new Vector3(56f,.52f,25.3f), 2.8f, statue, 180f);
        for (int i = 0; i < 4; i++)
        {
            float x = 47.5f + i * 5.7f;
            Place("A301 offering post " + (i+1), root, "A301_RitualPost",
                new Vector3(x,.13f,39f), 1.45f, stone);
            WarmLight("Offering lamp glow", root, new Vector3(x,2.3f,39f), 7f, 1.35f);
        }
        for (int i = 0; i < 8; i++)
        {
            int row = i / 4, col = i % 4;
            Vector3 display = new Vector3(51.1f + col * 3.25f, .13f,
                row == 0 ? 20.8f : 34.5f);
            Block("A242 ceramic display plinth " + (i+1), root,
                display + Vector3.up * .42f, new Vector3(.85f,.82f,.85f), darkStone);
            if (ceramics[i] != null)
            {
                GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(ceramics[i]);
                item.name = "A242 ceramic display " + (i+1);
                item.transform.SetParent(root, true);
                item.transform.position = display + Vector3.up * .84f;
                item.transform.localScale = Vector3.one * 1.12f;
            }
        }
        // The pre-existing mossy SkyGate stood in the middle of the new
        // ceremonial axis, hiding the ancestor statue and main temple.
        // Reuse it as a side relic while keeping the object and its components.
        GameObject skyGate = GameObject.Find("SkyGateModel");
        if (skyGate != null)
        {
            Block("Original sky gate relic plinth", root,
                new Vector3(42.5f,.26f,30.5f), new Vector3(2.8f,.28f,2.8f), darkStone);
            skyGate.transform.rotation = Quaternion.Euler(0f,25f,0f);
            Fit(skyGate, 3.1f);
            Seat(skyGate, new Vector3(42.5f,.4f,30.5f));
        }
        MoveAncestors(root);
        ValidateTempleScene(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ValidateTempleScene(Transform root)
    {
        foreach (string name in new[] { "A301 carved column 1", "A301 ancestral carved statue" })
        {
            Transform model = root.Find(name);
            if (model == null) throw new System.InvalidOperationException("Missing temple piece: " + name);
            Vector3 size = BoundsOf(model.gameObject).size;
            if (size.y < Mathf.Max(size.x, size.z) * 1.15f)
                throw new System.InvalidOperationException(name + " is still sideways: " + size);
        }
        foreach (string name in new[] { "A427 main Chinese temple", "A427 west shrine", "A427 east shrine" })
        {
            Transform model = root.Find(name);
            if (model == null || BoundsOf(model.gameObject).size.y < 3.5f)
                throw new System.InvalidOperationException("Temple building is missing or sideways: " + name);
        }
        SkyQuestMagicExchange exchange = Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        if (exchange == null) throw new System.InvalidOperationException("Missing ancestor exchange");
        for (int i = 0; i < ceramics.Length; i++)
        {
            GameObject ceramic = exchange.ceramicRewardPrefabs[i];
            if (ceramic == null) throw new System.InvalidOperationException("Missing A242 ceramic " + (i + 1));
            Renderer renderer = ceramic.GetComponentInChildren<Renderer>(true);
            if (renderer == null || renderer.sharedMaterial == null ||
                renderer.sharedMaterial.GetTexture("_BaseMap") == null)
                throw new System.InvalidOperationException("Missing A242 ceramic texture " + (i + 1));
        }
    }

    private static void MoveAncestors(Transform root)
    {
        SkyQuestMagicExchange exchange = Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        if (exchange == null) { Debug.LogError("No SkyQuestMagicExchange found in scene"); return; }
        Vector3[] spots = {
            new Vector3(49f,.14f,30f), new Vector3(63f,.14f,30f),
            new Vector3(49f,.14f,24f), new Vector3(63f,.14f,24f),
            new Vector3(51f,.14f,18.3f), new Vector3(61f,.14f,18.3f)
        };
        for (int i = 0; i < 6 && i < exchange.ancestors.Length; i++)
        {
            Transform spirit = exchange.ancestors[i];
            if (spirit == null) continue;
            Vector3 spot = spots[i];
            float platformTop = i < 2 ? .65f : .24f;
            Block("Ancestor " + (i+1) + " shrine", root,
                new Vector3(spot.x, (.14f + platformTop) * .5f, spot.z),
                new Vector3(2.15f, platformTop - .14f, 2.15f), darkStone);
            spirit.position = spot + Vector3.up * platformTop;
            spirit.rotation = Quaternion.LookRotation(new Vector3(56f,0f,29f) -
                new Vector3(spot.x,0f,spot.z), Vector3.up);
            Bounds b = VisibleBoundsOf(spirit.gameObject);
            spirit.position += Vector3.up * (platformTop - b.min.y);
            if (exchange.memorials != null && i < exchange.memorials.Length && exchange.memorials[i] != null)
            {
                Transform memorial = exchange.memorials[i];
                Vector3 forward = Vector3.ProjectOnPlane(spirit.forward, Vector3.up).normalized;
                memorial.position = spirit.position - forward * .95f;
                memorial.position = new Vector3(memorial.position.x, spot.y, memorial.position.z);
                memorial.rotation = spirit.rotation;
            }
        }
        // Keep the two screenshot ancestors facing the visitor after rebuilding
        // the court. Their memorials were already placed behind this direction.
        FaceScreenshotAncestors(exchange);
        for (int i = 0; i < 8; i++) exchange.ceramicRewardPrefabs[i] = ceramics[i];
        exchange.proximity = 4.5f;
        EditorUtility.SetDirty(exchange);

        SkyQuestAncestor legacy = Object.FindFirstObjectByType<SkyQuestAncestor>();
        if (legacy != null && legacy.ancestor != null)
        {
            Vector3 center = new Vector3(56f,.14f,31f);
            Block("First ancestor central shrine", root,
                center + Vector3.up * .11f, new Vector3(2.4f,.2f,2.4f), darkStone);
            legacy.ancestor.position = center + Vector3.up * .21f;
            Bounds b = VisibleBoundsOf(legacy.ancestor.gameObject);
            legacy.ancestor.position += Vector3.up * (.24f - b.min.y);
            legacy.ancestor.rotation = Quaternion.Euler(0f,180f,0f);
            if (legacy.memorialStone != null)
            {
                legacy.memorialStone.position = new Vector3(
                    legacy.ancestor.position.x,
                    center.y,
                    legacy.ancestor.position.z + .95f);
                legacy.memorialStone.rotation = legacy.ancestor.rotation;
            }
            EditorUtility.SetDirty(legacy);
        }
    }
}
