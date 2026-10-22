using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

// Builds the same spatial extension into the desktop and Quest scenes.
public static class SkyQuestEnvironmentBuilder
{
    private const string Quest = "Assets/SkyQuest/SkyQuest3S.unity";
    private const string Desktop = "Assets/SkyQuest/SkyDesktop.unity";
    private const string MaterialFolder = "Assets/SkyQuest/Environment/Materials";
    private const string TextureFolder = "Assets/SkyQuest/Environment/Textures";
    private const string RootName = "Sky Sanctuary Extension";
    private static Transform root;
    private static readonly Dictionary<string, Material> palette = new Dictionary<string, Material>();
    private static readonly List<Transform> beacons = new List<Transform>();
    private static Transform gate;
    private static Transform reward;

    public static void BuildBoth()
    {
        BuildScene(Quest);
        BuildScene(Desktop);
        Debug.Log("Sky Sanctuary extension built in both scenes.");
    }

    public static void BuildScene(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject old = GameObject.Find(RootName);
        if (old != null) UnityEngine.Object.DestroyImmediate(old);
        palette.Clear();
        beacons.Clear();
        EnsurePalette();
        // Keep material detail visible in the open courtyards and prevent white clipping.
        Light sun = GameObject.Find("Directional Light")?.GetComponent<Light>();
        if (sun != null) { sun.intensity = 1.05f; EditorUtility.SetDirty(sun); }
        RenderSettings.ambientIntensity = 0.8f;
        root = new GameObject(RootName).transform;
        Courtyard();
        Hall();
        Labyrinth();
        WestCloister();
        GrandCourt();
        EastAmphitheater();
        CharacterGallery();
        SkyQuestMagicExchange magic = UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        if (magic != null)
        {
            GameObject dancer = GameObject.Find("Shibiyama dancer");
            GameObject guardian = GameObject.Find("Sky prayer guardian");
            if (dancer == null || guardian == null)
                throw new Exception("A plaza Magic exchange ancestor is missing from the scene.");
            magic.ancestors[0] = dancer.transform;
            magic.ancestors[1] = guardian.transform;
            EditorUtility.SetDirty(magic);
        }
        Landscape();
        // Imported source models left several standalone authoring lights in the scene.
        // Keep the main sun and the sanctuary's controlled accent lights only.
        foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (light != sun && !light.transform.IsChildOf(root)) light.enabled = false;
        SkyQuestSanctuary experience = root.gameObject.AddComponent<SkyQuestSanctuary>();
        experience.beacons = beacons.ToArray();
        experience.gate = gate;
        experience.reward = reward;
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        if (locomotion != null) { locomotion.playRadius = 130f; EditorUtility.SetDirty(locomotion); }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save " + scenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Sky Sanctuary built: " + scenePath + "; beacons=" + beacons.Count +
            "; objects=" + root.GetComponentsInChildren<Transform>(true).Length);
    }

    private static void EnsurePalette()
    {
        if (!AssetDatabase.IsValidFolder("Assets/SkyQuest/Environment"))
            AssetDatabase.CreateFolder("Assets/SkyQuest", "Environment");
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/SkyQuest/Environment", "Materials");
        AddMaterial("WarmLimestone", new Color(0.72f, 0.66f, 0.56f), 0.15f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        AddMaterial("Ivory", new Color(0.87f, 0.82f, 0.72f), 0.24f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        AddMaterial("IvoryTile", new Color(0.60f, 0.57f, 0.51f), 0.17f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        AddMaterial("MistTile", new Color(0.50f, 0.63f, 0.67f), 0.12f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        AddMaterial("PaleGold", new Color(0.83f, 0.68f, 0.36f), 0.44f, 0f, "Metal_Detail.png", "Metal_Normal.jpg", 0.58f);
        AddMaterial("DeepGold", new Color(0.54f, 0.39f, 0.21f), 0.37f, 0f, "Metal_Detail.png", "Metal_Normal.jpg", 0.5f);
        AddMaterial("BlueStone", new Color(0.43f, 0.67f, 0.76f), 0.18f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        AddMaterial("Slate", new Color(0.28f, 0.45f, 0.54f), 0.17f, 0f, "Limestone_Detail.png", "Limestone_Normal.png");
        AddMaterial("Mist", new Color(0.7f, 0.79f, 0.81f), 0.08f, 0f, "Cloud_Detail.png");
        AddMaterial("Cloud", new Color(0.89f, 0.91f, 0.86f), 0.04f, 0f, "Cloud_Detail.png");
        AddMaterial("Candle", new Color(1f, 0.72f, 0.29f), 0.1f, 1.5f);
        AddMaterial("MoonLight", new Color(0.43f, 0.85f, 1f), 0.1f, 1.2f);
        AddMaterial("Dormant", new Color(0.27f, 0.52f, 0.61f), 0.25f, 0.2f);
        AddMaterial("Water", new Color(0.39f, 0.69f, 0.72f), 0.82f);
        AddMaterial("Banner", new Color(0.76f, 0.88f, 0.84f), 0.12f, 0f, "Cloth_Detail.png", "Cloth_Normal.png");
        AddMaterial("HoneyWood", new Color(0.87f, 0.77f, 0.64f), 0.29f, 0f, "Wood_Color.jpg", "Wood_Normal.jpg");
        AddMaterial("IvoryBark", new Color(0.83f, 0.78f, 0.66f), 0.12f, 0f, "Bark_Detail.png", "Bark_Normal.png");
        AddMaterial("LeafCanopy", new Color(0.68f, 0.82f, 0.70f), 0.08f, 0f, "Leaf_Detail.png", "Leaf_Normal.png");
    }

    private static void AddMaterial(string name, Color color, float smoothness, float emission = 0f,
        string colorMap = null, string normalMap = null, float metallic = 0f)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (colorMap != null)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/" + colorMap);
            if (tex == null) throw new Exception("Missing environment texture: " + colorMap);
            material.SetTexture("_BaseMap", tex);
        }
        if (normalMap != null)
        {
            string normalPath = TextureFolder + "/" + normalMap;
            TextureImporter importer = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            if (importer == null) throw new Exception("Missing environment normal map: " + normalMap);
            if (importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.maxTextureSize = 1024;
                importer.SaveAndReimport();
            }
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_BumpScale", 0.4f);
        }
        if (colorMap != null)
        {
            Vector2 tiling = name == "HoneyWood" ? Vector2.one : new Vector2(2f, 2f);
            material.SetTextureScale("_BaseMap", tiling);
            if (normalMap != null) material.SetTextureScale("_BumpMap", tiling);
        }
        if (emission > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * emission);
        }
        EditorUtility.SetDirty(material);
        palette[name] = material;
    }

    private static Transform Group(string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(root, false);
        return obj.transform;
    }

    private static GameObject Shape(string name, PrimitiveType type, Transform parent,
        Vector3 position, Vector3 scale, string material, bool solid = true,
        Quaternion? rotation = null)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        obj.transform.rotation = rotation ?? Quaternion.identity;
        obj.GetComponent<Renderer>().sharedMaterial = palette[material];
        if (!solid) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        GameObjectUtility.SetStaticEditorFlags(obj, StaticEditorFlags.BatchingStatic);
        return obj;
    }

    private static GameObject Box(string name, Transform parent, float x, float y, float z,
        float width, float height, float depth, string material, bool solid = true)
    {
        return Shape(name, PrimitiveType.Cube, parent, new Vector3(x, y, z),
            new Vector3(width, height, depth), material, solid);
    }

    private static GameObject Pillar(string name, Transform parent, float x, float z,
        float yBase, float height, float radius, string material, bool solid = true)
    {
        return Shape(name, PrimitiveType.Cylinder, parent,
            new Vector3(x, yBase + height * 0.5f, z),
            new Vector3(radius * 2f, height * 0.5f, radius * 2f), material, solid);
    }

    private static GameObject Orb(string name, Transform parent, Vector3 position,
        Vector3 diameter, string material, bool solid = false)
    {
        return Shape(name, PrimitiveType.Sphere, parent, position, diameter, material, solid);
    }

    private static void Beam(string name, Transform parent, Vector3 a, Vector3 b,
        float width, string material)
    {
        Vector3 delta = b - a;
        Shape(name, PrimitiveType.Cube, parent, (a + b) * 0.5f,
            new Vector3(width, width, delta.magnitude), material, false,
            Quaternion.LookRotation(delta.normalized, Vector3.up));
    }

    private static void Arch(Transform parent, string name, Vector3 center,
        float radius, float baseHeight, bool alongX, string material)
    {
        for (int i = 0; i < 12; i++)
        {
            float a = Mathf.PI * i / 12f;
            float b = Mathf.PI * (i + 1) / 12f;
            Vector3 p = new Vector3(-radius * Mathf.Cos(a), baseHeight + radius * Mathf.Sin(a), 0f);
            Vector3 q = new Vector3(-radius * Mathf.Cos(b), baseHeight + radius * Mathf.Sin(b), 0f);
            if (!alongX) { p = new Vector3(0f, p.y, p.x); q = new Vector3(0f, q.y, q.x); }
            Beam(name + " voussoir " + i, parent, center + p, center + q, 0.25f, material);
        }
    }

    private static void Lamp(Transform parent, string name, float x, float z, float height)
    {
        Pillar(name + " plinth", parent, x, z, 0f, 0.32f, 0.35f, "BlueStone");
        Pillar(name + " stem", parent, x, z, 0.32f, height - 0.5f, 0.085f, "DeepGold", false);
        Orb(name + " flame", parent, new Vector3(x, height, z),
            new Vector3(0.28f, 0.43f, 0.28f), "Candle");
        Orb(name + " halo", parent, new Vector3(x, height + 0.12f, z),
            Vector3.one * 0.52f, "Ivory");
    }

    private static void Courtyard()
    {
        Transform p = Group("01 - Arrival causeway and cloud garden");
        Box("Causeway limestone deck", p, 31f, 0.05f, 38f, 22f, 0.14f, 6.5f, "WarmLimestone");
        for (int i = 0; i < 10; i++)
        {
            float x = 22.5f + i * 1.9f;
            Box("Inlaid path slab " + i, p, x, 0.135f, 38f, 1.6f, 0.025f, 4.5f,
                i % 2 == 0 ? "IvoryTile" : "MistTile", false);
        }
        for (int side = -1; side <= 1; side += 2)
        {
            float z = 38f + side * 3.1f;
            Box("Open sky railing", p, 31f, 0.5f, z, 22f, 0.2f, 0.2f, "Ivory");
            for (int i = 0; i <= 7; i++)
            {
                float x = 21f + i * 2.85f;
                Pillar("Railing post", p, x, z, 0f, 0.95f, 0.16f, "WarmLimestone");
                Orb("Gold finial", p, new Vector3(x, 1.02f, z), Vector3.one * 0.18f, "PaleGold");
                if (i % 2 == 0) Lamp(p, "Way lantern", x, z + side * 0.5f, 1.75f);
            }
        }
        Pillar("Arrival medallion", p, 39.5f, 38f, 0.08f, 0.03f, 2.3f, "PaleGold", false);
        Pillar("Arrival inner circle", p, 39.5f, 38f, 0.115f, 0.02f, 1.75f, "IvoryTile", false);
        for (int i = 0; i < 12; i++)
        {
            float a = i * Mathf.PI / 6f;
            Orb("Compass star", p, new Vector3(39.5f + Mathf.Sin(a) * 1.45f, 0.14f,
                38f + Mathf.Cos(a) * 1.45f), Vector3.one * 0.12f, "PaleGold");
        }
        for (int i = 0; i < 16; i++)
        {
            float a = i * 2.4f;
            float x = 25f + (i % 4) * 3.9f;
            float z = i % 2 == 0 ? 44f + (i / 4) * 1.8f : 32f - (i / 4) * 1.8f;
            CloudCluster(p, x, -0.35f - (i % 3) * 0.1f, z, 1f + (i % 4) * 0.2f, i);
        }
    }

    private static void Hall()
    {
        Transform p = Group("02 - Interior Hall of Flight");
        Box("Polished sanctuary floor", p, 12f, 0.06f, 38f, 20.5f, 0.13f, 24f, "WarmLimestone");
        Box("East entrance north wing", p, 22.3f, 3.3f, 45.1f, 0.55f, 6.6f, 8.1f, "Ivory");
        Box("East entrance south wing", p, 22.3f, 3.3f, 30.9f, 0.55f, 6.6f, 8.1f, "Ivory");
        Box("Entrance lintel", p, 22.3f, 7.5f, 38f, 0.7f, 1.5f, 6.8f, "Ivory");
        Arch(p, "East doorway arch", new Vector3(22.62f, 0f, 38f), 3.25f, 3.95f, false, "PaleGold");
        for (int i = 0; i < 2; i++)
        {
            float z = i == 0 ? 30.6f : 45.4f;
            Box("Entrance azure relief", p, 22.64f, 3.45f, z, 0.07f, 3.2f, 3.5f,
                "BlueStone", false);
            Box("Relief top gilding", p, 22.7f, 5.12f, z, 0.12f, 0.13f, 3.7f,
                "PaleGold", false);
            Box("Relief foot gilding", p, 22.7f, 1.78f, z, 0.12f, 0.13f, 3.7f,
                "PaleGold", false);
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Relief side gilding", p, 22.7f, 3.45f, z + side * 1.8f,
                    0.12f, 3.45f, 0.1f, "PaleGold", false);
                Beam("Relief wing", p,
                    new Vector3(22.78f, 3.2f, z),
                    new Vector3(22.78f, 3.8f, z + side * 1.2f),
                    0.12f, "PaleGold");
            }
            Orb("Relief moon", p, new Vector3(22.82f, 3.85f, z),
                new Vector3(0.16f, 0.5f, 0.5f), "Candle");
        }
        Orb("Entrance crown star", p, new Vector3(22.8f, 7.8f, 38f),
            new Vector3(0.16f, 0.62f, 0.62f), "Candle");
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side < 0 ? 2f : 22f;
            for (int bay = 0; bay < 5; bay++)
            {
                float z = 28f + bay * 5f;
                if (bay == 1 || bay == 2) continue;
                Box("Window parapet", p, x, 0.7f, z, 0.55f, 1.4f, 4.7f, "Ivory");
                Box("Window crown", p, x, 6.1f, z, 0.65f, 1.3f, 4.7f, "Ivory");
                Box("Window tracer", p, x, 3.7f, z, 0.08f, 4.2f, 0.1f, "PaleGold", false);
                Orb("Window jewel", p, new Vector3(x + (side < 0 ? 0.32f : -0.32f), 4.9f, z),
                    Vector3.one * 0.22f, "MoonLight");
            }
            for (int i = 0; i <= 6; i++)
            {
                float z = 26.5f + i * 3.8f;
                if (z > 34f && z < 42f) continue;
                Pillar("Fluted wall column", p, x, z, 0f, 6.8f, 0.23f, "Ivory");
                Pillar("Column capital", p, x, z, 6.5f, 0.4f, 0.38f, "PaleGold", false);
            }
        }
        Box("Southern mural wall", p, 12f, 3.2f, 26.4f, 20.5f, 6.4f, 0.65f, "Ivory");
        Box("North threshold left", p, 6.7f, 3.15f, 49.5f, 9.9f, 6.3f, 0.6f, "Ivory");
        Box("North threshold right", p, 18.3f, 3.15f, 49.5f, 7.8f, 6.3f, 0.6f, "Ivory");
        Box("North threshold crown", p, 12.5f, 7.6f, 49.5f, 20f, 1.7f, 0.75f, "Ivory");
        Arch(p, "North portal arch", new Vector3(12.5f, 0f, 49.12f), 2.65f, 3.8f, true, "PaleGold");
        for (int i = 0; i < 5; i++)
        {
            float z = 28f + i * 5f;
            for (int side = -1; side <= 1; side += 2)
            {
                float x = 12f + side * 7.2f;
                Pillar("Nave column", p, x, z, 0f, 6.8f, 0.36f, "Ivory");
                Pillar("Column footing", p, x, z, 0f, 0.3f, 0.53f, "BlueStone");
                Pillar("Column gilded capital", p, x, z, 6.5f, 0.4f, 0.52f, "PaleGold", false);
                if (i % 2 == 0) Lamp(p, "Aisle candle", x + side * 1.1f, z + 1.3f, 1.6f);
            }
            VaultRib(p, z);
        }
        // Split ceiling leaves a luminous central skylight; the hall remains clearly indoors.
        Box("West vault ceiling", p, 4.2f, 8.2f, 38f, 4.8f, 0.4f, 23.7f, "Ivory", false);
        Box("East vault ceiling", p, 19.5f, 8.2f, 38f, 5.5f, 0.4f, 23.7f, "Ivory", false);
        Box("South ceiling beam", p, 12f, 8.2f, 29f, 11f, 0.36f, 5.5f, "Ivory", false);
        Box("North ceiling beam", p, 12f, 8.2f, 46.2f, 11f, 0.36f, 7.3f, "Ivory", false);
        for (int i = 0; i < 9; i++)
        {
            float z = 28f + i * 2.5f;
            Box("Floor gold seam", p, 12f, 0.143f, z, 17.5f, 0.016f, 0.045f, "PaleGold", false);
        }
        Pillar("Reflecting pool rim", p, 12f, 38f, 0.13f, 0.17f, 2.45f, "PaleGold", false);
        Pillar("Reflecting pool water", p, 12f, 38f, 0.27f, 0.015f, 2.1f, "Water", false);
        Orb("Light over the pool", p, new Vector3(12f, 2.5f, 38f),
            new Vector3(0.65f, 0.65f, 0.65f), "MoonLight");
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 0.25f;
            float x = 12f + Mathf.Cos(angle) * 3.4f;
            float z = 38f + Mathf.Sin(angle) * 3.4f;
            Orb("Pool constellation", p, new Vector3(x, 0.19f, z), Vector3.one * 0.11f, "Candle");
        }
        for (int i = 0; i < 6; i++)
        {
            float z = 30f + i * 3.1f;
            Box("Hanging silk banner", p, 6.7f, 5.45f, z, 0.04f, 2.4f, 0.7f, "Banner", false);
            Box("Hanging silk banner", p, 17.3f, 5.45f, z, 0.04f, 2.4f, 0.7f, "Banner", false);
        }
        AddLight(p, "Pool glow", new Vector3(12f, 5.3f, 38f), new Color(0.55f, 0.86f, 1f), 2.1f, 13f);
        AddLight(p, "Portal glow", new Vector3(21f, 4f, 38f), new Color(1f, 0.72f, 0.39f), 1.5f, 8f);
    }

    private static void VaultRib(Transform p, float z)
    {
        for (int i = 0; i < 12; i++)
        {
            float a = Mathf.PI * i / 12f;
            float b = Mathf.PI * (i + 1) / 12f;
            Vector3 v = new Vector3(12f - 8.5f * Mathf.Cos(a), 6.9f + 1.1f * Mathf.Sin(a), z);
            Vector3 w = new Vector3(12f - 8.5f * Mathf.Cos(b), 6.9f + 1.1f * Mathf.Sin(b), z);
            Beam("Vault rib", p, v, w, 0.18f, "PaleGold");
        }
    }

    private static void Labyrinth()
    {
        Transform p = Group("03 - Garden of four lights and obstacle route");
        Box("Garden limestone base", p, 13f, 0.055f, 60f, 27f, 0.12f, 22f, "BlueStone");
        for (int i = 0; i < 10; i++)
            Box("Garden paving band", p, 13f, 0.125f, 50.8f + i * 2.1f,
                25f, 0.02f, 0.09f, "PaleGold", false);
        // Perimeter walls and alternating solid barriers are navigable on foot.
        for (int side = -1; side <= 1; side += 2)
        {
            float x = 13f + side * 13.4f;
            Box("Garden outer wall", p, x, 1.6f, 59.7f, 0.55f, 3.2f, 19.5f, "Ivory");
            Box("Garden outer cornice", p, x, 3.25f, 59.7f, 0.9f, 0.25f, 19.5f, "PaleGold", false);
            for (int i = 0; i < 6; i++)
                Pillar("Garden wall buttress", p, x, 50.7f + i * 3.5f, 0f, 3.5f,
                    0.28f, "WarmLimestone");
        }
        Box("Garden south wall left", p, 5f, 1.6f, 50.2f, 10f, 3.2f, 0.55f, "Ivory");
        Box("Garden south wall right", p, 21f, 1.6f, 50.2f, 10f, 3.2f, 0.55f, "Ivory");
        Box("Garden far wall left", p, 5f, 1.6f, 69.8f, 10f, 3.2f, 0.55f, "Ivory");
        Box("Garden far wall right", p, 21f, 1.6f, 69.8f, 10f, 3.2f, 0.55f, "Ivory");
        for (int row = 0; row < 4; row++)
        {
            float z = 54f + row * 4f;
            bool eastGap = row % 2 == 0;
            float x = eastGap ? 10.5f : 15.5f;
            Box("Alternating obstacle wall " + row, p, x, 1.6f, z,
                21f, 3.2f, 0.7f, row % 2 == 0 ? "WarmLimestone" : "Ivory");
            Box("Obstacle gilded top", p, x, 3.27f, z, 21f, 0.18f, 0.9f, "PaleGold", false);
            for (int j = 0; j < 4; j++)
            {
                float px = x - 8f + j * 5.3f;
                Pillar("Obstacle buttress", p, px, z, 0f, 3.55f, 0.29f, "BlueStone");
                Orb("Wall candle", p, new Vector3(px, 3.65f, z),
                    Vector3.one * 0.17f, "Candle");
            }
            float beaconX = eastGap ? 23f : 3f;
            float beaconZ = z - 1.25f;
            Pillar("Beacon altar", p, beaconX, beaconZ, 0f, 1.15f, 0.55f, "Ivory");
            Pillar("Beacon gold ring", p, beaconX, beaconZ, 1.1f, 0.1f, 0.64f,
                "PaleGold", false);
            Transform light = Orb("Awakenable light " + (row + 1), p,
                new Vector3(beaconX, 1.45f, beaconZ), Vector3.one * 0.45f,
                "Dormant").transform;
            beacons.Add(light);
        }
        // Small solid stones add depth without removing the clear 4.3 m walking lane.
        for (int i = 0; i < 8; i++)
        {
            float x = i % 2 == 0 ? 8f : 18f;
            float z = 51.5f + i * 2.25f;
            Pillar("Garden sculpted stone", p, x, z, 0f, 0.65f, 0.34f, "WarmLimestone");
            Orb("Sculpted stone crown", p, new Vector3(x, 0.8f, z),
                Vector3.one * 0.3f, "PaleGold");
        }
        for (int x = 2; x <= 24; x += 4)
            Box("Garden last floor ray", p, x, 0.129f, 68.25f,
                0.12f, 0.02f, 1.3f, "Candle", false);
        gate = Box("Sanctuary door", p, 13f, 2.05f, 69.8f, 5.55f, 4.1f, 0.48f,
            "BlueStone").transform;
        for (int i = 0; i < 8; i++)
        {
            float a = i * Mathf.PI / 4f;
            Orb("Door constellation", gate,
                gate.position + new Vector3(Mathf.Cos(a) * 1.2f, Mathf.Sin(a) * 1.2f, -0.32f),
                Vector3.one * 0.18f, "Candle");
        }
        foreach (Transform part in gate.GetComponentsInChildren<Transform>())
            GameObjectUtility.SetStaticEditorFlags(part.gameObject, 0);
        Arch(p, "Garden crown", new Vector3(13f, 0f, 69.75f), 3f, 3.7f, true, "PaleGold");
        Box("Reward platform", p, 13f, 0.19f, 73f, 6f, 0.38f, 5f, "Ivory");
        Pillar("Reward gold disc", p, 13f, 73f, 0.38f, 0.035f, 1.5f, "PaleGold", false);
        reward = Orb("Sanctuary completion star", p, new Vector3(13f, 2.15f, 73f),
            Vector3.one * 0.95f, "Candle").transform;
        GameObjectUtility.SetStaticEditorFlags(reward.gameObject, 0);
        AddLight(p, "Final arch glow", new Vector3(13f, 5f, 69f),
            new Color(1f, 0.75f, 0.43f), 1.8f, 8f);
    }

    private static void WestCloister()
    {
        Transform p = Group("05 - West cloister and five alcoves");
        Box("West cloister floor", p, -13f, 0.055f, 38f, 30f, 0.12f, 26f, "WarmLimestone");
        // Individual static inlays give the long hall a readable scale in VR.
        for (int ix = 0; ix < 15; ix++)
            for (int iz = 0; iz < 13; iz++)
            {
                float x = -27f + ix * 2f;
                float z = 26f + iz * 2f;
                Box("Cloister geometric floor tile", p, x, 0.127f, z, 1.8f, 0.014f, 1.8f,
                    (ix + iz) % 4 == 0 ? "MistTile" : "IvoryTile", false);
                if ((ix + iz) % 5 == 0)
                    Orb("Cloister tile star", p, new Vector3(x, 0.144f, z),
                        Vector3.one * 0.09f, "PaleGold");
            }
        Box("Cloister west outer wall", p, -28.1f, 3.6f, 38f, 0.65f, 7.2f, 26f, "Ivory");
        Box("Cloister south outer wall", p, -13f, 3.6f, 25f, 30f, 7.2f, 0.65f, "Ivory");
        Box("Cloister north outer wall", p, -13f, 3.6f, 51f, 30f, 7.2f, 0.65f, "Ivory");
        Box("Cloister east wall south", p, 2f, 3.6f, 29.5f, 0.65f, 7.2f, 9f, "Ivory");
        Box("Cloister east wall north", p, 2f, 3.6f, 46.5f, 0.65f, 7.2f, 9f, "Ivory");
        Box("Cloister entrance lintel", p, 2f, 7.6f, 38f, 0.8f, 1.4f, 8f, "Ivory");
        Arch(p, "Cloister doorway", new Vector3(1.64f, 0f, 38f), 3.6f, 3.4f, false, "PaleGold");
        for (int row = 0; row < 2; row++)
        {
            float x = row == 0 ? -22f : -4.5f;
            for (int i = 0; i < 7; i++)
            {
                float z = 27f + i * 3.6f;
                Pillar("Cloister fluted column", p, x, z, 0f, 7.4f, 0.35f, "Ivory");
                Pillar("Cloister blue base", p, x, z, 0f, 0.32f, 0.49f, "BlueStone");
                Pillar("Cloister gilded capital", p, x, z, 7.1f, 0.34f, 0.51f,
                    "PaleGold", false);
                if (i % 2 == 0) Lamp(p, "Cloister lamp", x + (row == 0 ? 1.1f : -1.1f),
                    z, 1.7f);
            }
        }
        for (int bay = 0; bay < 5; bay++)
        {
            float z = 28f + bay * 5f;
            Box("Alcove azure backdrop", p, -27.68f, 3.4f, z, 0.08f, 4.8f, 3.6f,
                "BlueStone", false);
            Arch(p, "Alcove golden arch", new Vector3(-27.5f, 0f, z), 1.65f,
                4.4f, false, "PaleGold");
            Box("Alcove resting bench", p, -25.4f, 0.5f, z, 2.8f, 0.34f, 2f,
                "HoneyWood");
            Box("Alcove woven cushion", p, -25.4f, 0.72f, z, 2.55f, 0.1f, 1.7f,
                "Banner", false);
            Orb("Alcove floating flame", p, new Vector3(-27f, 3.7f, z),
                Vector3.one * 0.4f, "Candle");
            for (int j = 0; j < 8; j++)
            {
                float zz = z - 1.4f + j * 0.4f;
                Box("Alcove wall carving", p, -27.55f, 2.1f, zz,
                    0.1f, 0.9f, 0.06f, "PaleGold", false);
            }
        }
        Box("Cloister south ceiling", p, -13f, 8.2f, 29f, 30f, 0.4f, 8f,
            "Ivory", false);
        Box("Cloister north ceiling", p, -13f, 8.2f, 47f, 30f, 0.4f, 8f,
            "Ivory", false);
        Box("Cloister west ceiling", p, -25f, 8.2f, 38f, 6f, 0.4f, 26f,
            "Ivory", false);
        Box("Cloister east ceiling", p, -1f, 8.2f, 38f, 6f, 0.4f, 26f,
            "Ivory", false);
        for (int i = 0; i < 8; i++)
        {
            float z = 27f + i * 3.1f;
            Beam("Cloister cross vault", p, new Vector3(-22f, 7.8f, z),
                new Vector3(-4.5f, 7.8f, z), 0.22f, "PaleGold");
        }
        Pillar("Cloister moon basin rim", p, -13f, 38f, 0.14f, 0.13f, 2.8f,
            "PaleGold", false);
        Pillar("Cloister moon basin", p, -13f, 38f, 0.27f, 0.015f, 2.4f,
            "Water", false);
        Orb("Cloister moon", p, new Vector3(-13f, 3.1f, 38f),
            Vector3.one * 0.78f, "MoonLight");
        AddLight(p, "Cloister moon glow", new Vector3(-13f, 5.7f, 38f),
            new Color(0.55f, 0.82f, 1f), 1.8f, 12f);
    }

    private static void GrandCourt()
    {
        Transform p = Group("06 - Grand northern court and sky gate");
        Box("Grand northern court floor", p, 7.5f, 0.055f, 84f,
            55f, 0.12f, 28f, "WarmLimestone");
        for (int ix = 0; ix < 27; ix++)
            for (int iz = 0; iz < 14; iz++)
            {
                float x = -18.5f + ix * 2f;
                float z = 71f + iz * 2f;
                Box("Court sunstone floor tile", p, x, 0.127f, z,
                    1.82f, 0.014f, 1.82f,
                    (ix + iz) % 6 == 0 ? "Mist" : "Ivory", false);
                if (ix % 4 == 0 && iz % 4 == 0)
                    Orb("Court floor constellation", p, new Vector3(x, 0.15f, z),
                        Vector3.one * 0.11f, "Candle");
            }
        for (int side = -1; side <= 1; side += 2)
        {
            float x = 7.5f + side * 27.6f;
            Box("Court side balustrade", p, x, 0.9f, 84f,
                0.55f, 1.8f, 28f, "Ivory");
            for (int i = 0; i <= 9; i++)
            {
                float z = 70f + i * 3.1f;
                Pillar("Court side obelisk", p, x, z, 0f, 2.4f,
                    0.32f, "BlueStone");
                Orb("Court obelisk jewel", p, new Vector3(x, 2.6f, z),
                    Vector3.one * 0.34f, "Candle");
            }
        }
        Box("Court north balustrade left", p, -9.5f, 0.9f, 98f,
            21f, 1.8f, 0.55f, "Ivory");
        Box("Court north balustrade right", p, 24.5f, 0.9f, 98f,
            21f, 1.8f, 0.55f, "Ivory");
        for (int i = 0; i < 18; i++)
        {
            float a = i * Mathf.PI * 2f / 18f;
            float x = 6f + Mathf.Cos(a) * 11.5f;
            float z = 84f + Mathf.Sin(a) * 11.5f;
            Pillar("Court circular colonnade", p, x, z, 0f, 6.9f,
                0.37f, "Ivory");
            Pillar("Court gold capital", p, x, z, 6.6f, 0.4f,
                0.52f, "PaleGold", false);
            float b = (i + 1) * Mathf.PI * 2f / 18f;
            Beam("Court ring architrave", p, new Vector3(x, 7.2f, z),
                new Vector3(6f + Mathf.Cos(b) * 11.5f, 7.2f,
                    84f + Mathf.Sin(b) * 11.5f), 0.28f, "PaleGold");
            if (i % 3 == 0)
            {
                Orb("Court suspended lantern", p,
                    new Vector3(6f + Mathf.Cos(a) * 9f, 5f,
                        84f + Mathf.Sin(a) * 9f),
                    Vector3.one * 0.52f, "Candle");
            }
        }
        Pillar("Court central star plinth", p, 6f, 84f, 0.12f,
            0.22f, 4.2f, "BlueStone", false);
        Pillar("Court central gold mandala", p, 6f, 84f, 0.35f,
            0.025f, 3.5f, "PaleGold", false);
        for (int i = 0; i < 20; i++)
        {
            float a = i * Mathf.PI / 10f;
            Beam("Radial sun inlay", p,
                new Vector3(6f + Mathf.Cos(a) * 1.4f, 0.4f,
                    84f + Mathf.Sin(a) * 1.4f),
                new Vector3(6f + Mathf.Cos(a) * 3.2f, 0.4f,
                    84f + Mathf.Sin(a) * 3.2f),
                0.11f, "Ivory");
        }
        Orb("Court levitating star", p, new Vector3(6f, 3.5f, 84f),
            Vector3.one * 1.1f, "MoonLight");
        for (int side = -1; side <= 1; side += 2)
        {
            float x = 6f + side * 20f;
            Pillar("Court lily pool rim", p, x, 84f, 0.13f,
                0.15f, 4.1f, "PaleGold", false);
            Pillar("Court lily pool", p, x, 84f, 0.27f,
                0.015f, 3.8f, "Water", false);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4f;
                Orb("Court lily light", p,
                    new Vector3(x + Mathf.Cos(a) * 2.7f, 0.35f,
                        84f + Mathf.Sin(a) * 2.7f),
                    new Vector3(0.28f, 0.11f, 0.28f), "Candle");
            }
        }
        AddLight(p, "Grand star glow", new Vector3(6f, 5.5f, 84f),
            new Color(0.62f, 0.88f, 1f), 2f, 15f);
        GameObject gateAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Project files/3/SkyGateModel.fbx");
        if (gateAsset != null)
        {
            GameObject monument = (GameObject)PrefabUtility.InstantiatePrefab(gateAsset);
            monument.name = "Existing Sky Gate - northern monument";
            monument.transform.SetParent(p, false);
            FitModel(monument, new Vector3(6f, 0.14f, 96f), 7.5f, 0f);
            RemoveModelColliders(monument);
        }
    }

    private static void EastAmphitheater()
    {
        Transform p = Group("07 - Eastern amphitheater and character court");
        Box("East gallery connector", p, 40f, 0.05f, 42f,
            8f, 0.12f, 10f, "WarmLimestone");
        Box("Eastern amphitheater floor", p, 47.5f, 0.055f, 65.5f,
            35f, 0.12f, 41f, "WarmLimestone");
        for (int ix = 0; ix < 17; ix++)
            for (int iz = 0; iz < 20; iz++)
            {
                float x = 31.5f + ix * 2f;
                float z = 46.5f + iz * 2f;
                Box("Amphitheater mosaic tile", p, x, 0.127f, z,
                    1.82f, 0.014f, 1.82f,
                    (ix + iz) % 5 == 0 ? "BlueStone" : "Ivory", false);
                if ((ix * 3 + iz) % 9 == 0)
                    Orb("Amphitheater tile light", p,
                        new Vector3(x, 0.15f, z),
                        Vector3.one * 0.085f, "PaleGold");
            }
        // Three concentric seating terraces leave the south entrance open.
        for (int row = 0; row < 3; row++)
        {
            float radius = 10.8f + row * 2.25f;
            for (int segment = 0; segment < 22; segment++)
            {
                float a = -0.12f + segment * (Mathf.PI + 0.24f) / 22f;
                float b = -0.12f + (segment + 1) * (Mathf.PI + 0.24f) / 22f;
                Vector3 v = new Vector3(49f + Mathf.Cos(a) * radius,
                    0.28f + row * 0.21f, 68f + Mathf.Sin(a) * radius);
                Vector3 w = new Vector3(49f + Mathf.Cos(b) * radius,
                    0.28f + row * 0.21f, 68f + Mathf.Sin(b) * radius);
                Beam("Curved audience bench", p, v, w,
                    0.28f, row == 1 ? "HoneyWood" : "BlueStone");
                if (segment % 3 == 0)
                    Orb("Seat gold rivet", p, w + Vector3.up * 0.2f,
                        Vector3.one * 0.13f, "PaleGold");
            }
        }
        Box("Performance stage", p, 49f, 0.31f, 68f,
            15f, 0.62f, 10f, "BlueStone");
        Box("Stage gilt edge", p, 49f, 0.65f, 63f,
            15.4f, 0.12f, 0.3f, "PaleGold", false);
        Box("Audience approach step", p, 49f, 0.12f, 62.2f,
            8f, 0.24f, 1.5f, "Ivory");
        Box("Audience middle step", p, 49f, 0.24f, 63.5f,
            8f, 0.48f, 1.5f, "Ivory");
        for (int i = 0; i < 18; i++)
        {
            float a = i * Mathf.PI * 2f / 18f;
            float x = 49f + Mathf.Cos(a) * 17f;
            float z = 68f + Mathf.Sin(a) * 17f;
            Pillar("Amphitheater perimeter column", p, x, z, 0f,
                6.7f, 0.35f, "Ivory");
            Pillar("Perimeter column capital", p, x, z, 6.4f,
                0.38f, 0.53f, "PaleGold", false);
            if (i % 2 == 0) Lamp(p, "Gallery torch", x, z, 2f);
        }
        for (int i = 0; i < 16; i++)
        {
            float a = i * Mathf.PI / 8f;
            float x = 49f + Mathf.Cos(a) * 6f;
            float z = 68f + Mathf.Sin(a) * 3.7f;
            Orb("Stage constellation", p,
                new Vector3(x, 0.73f, z),
                Vector3.one * 0.12f, "Candle");
        }
        Box("Stage rear azure wall", p, 49f, 3.2f, 73f,
            16f, 6.4f, 0.6f, "BlueStone");
        Arch(p, "Performance stage arch", new Vector3(49f, 0f, 72.55f),
            6f, 3.2f, true, "PaleGold");
        for (int i = 0; i < 12; i++)
        {
            float x = 42f + i * 1.25f;
            Box("Stage rear light slit", p, x, 3.2f, 72.62f,
                0.07f, 3.2f + (i % 3) * 0.4f, 0.07f,
                "Candle", false);
        }
        AddLight(p, "Eastern stage glow", new Vector3(49f, 5.5f, 67f),
            new Color(1f, 0.74f, 0.46f), 2.1f, 13f);
    }

    private static void CharacterGallery()
    {
        Transform p = Group("08 - Visible character gallery");
        PlaceCharacter(p, "COTL traveller", "Assets/Project files/COTL_rigged.fbx",
            new Vector3(-16f, 0.16f, 36f), 1.6f, 240f);
        PlaceCharacter(p, "Candle pilgrim", "Assets/Project files/1/2021.fbx",
            new Vector3(-12f, 0.16f, 45f), 1.6f, 0f);
        PlaceCharacter(p, "Shibiyama dancer", "Assets/Project files/2/shibiyamaskfb.fbx",
            new Vector3(44.5f, 0.65f, 67f), 1.6f, 20f);
        GameObject originalSkeleton = GameObject.Find("skeleton") ??
            GameObject.Find("Skeleton pilgrim - grounded gallery character");
        if (originalSkeleton != null)
        {
            originalSkeleton.name = "Skeleton pilgrim - grounded gallery character";
            originalSkeleton.SetActive(true);
            FitModel(originalSkeleton, new Vector3(-19f, 0.16f, 44f), 1.6f, 130f);
            DetailFlatCharacter(originalSkeleton);
            RemoveModelColliders(originalSkeleton);
        }
        GameObject legacyPlayer = FindSceneRoot("C1_Player") ??
            FindSceneRoot("Original C1 player - visible gallery character");
        if (legacyPlayer != null)
        {
            foreach (MonoBehaviour behaviour in legacyPlayer.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = false;
            foreach (Camera camera in legacyPlayer.GetComponentsInChildren<Camera>(true))
                camera.enabled = false;
            foreach (AudioListener listener in legacyPlayer.GetComponentsInChildren<AudioListener>(true))
                listener.enabled = false;
            legacyPlayer.name = "Original C1 player - visible gallery character";
            legacyPlayer.SetActive(true);
            FitModel(legacyPlayer, new Vector3(-12f, 0.16f, 92f), 1.6f, 190f);
            DetailFlatCharacter(legacyPlayer);
            RemoveModelColliders(legacyPlayer);
        }
        GameObject deerMask = GameObject.Find("deer mask") ??
            GameObject.Find("Deer mask relic - grounded exhibit");
        if (deerMask != null)
        {
            deerMask.name = "Deer mask relic - grounded exhibit";
            deerMask.SetActive(true);
            FitModel(deerMask, new Vector3(-25.4f, 0.8f, 28f), 1.25f, 90f);
            RemoveModelColliders(deerMask);
        }
        GameObject original = FindSceneRoot("sky_pray_with texture") ??
            FindSceneRoot("Sky prayer character - stage guardian") ??
            FindSceneRoot("Legacy sky prayer source model");
        if (original != null)
        {
            original.name = "Legacy sky prayer source model";
            original.SetActive(false);
        }
        PlaceCharacter(p, "Sky prayer guardian",
            "Assets/Project files/4/sky_pray_with texture.fbx",
            new Vector3(53.5f, 0.65f, 67f), 1.6f, 180f);
        foreach (Vector3 place in new[]
        {
            new Vector3(-16f, 0f, 36f), new Vector3(-12f, 0f, 45f),
            new Vector3(44.5f, 0f, 67f), new Vector3(53.5f, 0f, 67f),
            new Vector3(-19f, 0f, 44f), new Vector3(-12f, 0f, 92f)
        })
        {
            Pillar("Character grounded exhibit plinth", p, place.x, place.z,
                0f, place.x > 0f ? 0.65f : 0.16f, 1.15f,
                "PaleGold", false);
        }
    }

    private static GameObject FindSceneRoot(string name)
    {
        foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
            if (obj.name == name) return obj;
        return null;
    }

    private static void PlaceCharacter(Transform parent, string name, string assetPath,
        Vector3 footPoint, float height, float facing)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (asset == null) throw new Exception("Character model is missing: " + assetPath);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        instance.name = name;
        instance.transform.SetParent(parent, true);
        if (name == "Shibiyama dancer") RestoreDancerTextures(instance);
        if (name == "COTL traveller" || name == "Candle pilgrim") DetailFlatCharacter(instance);
        if (name == "Sky prayer guardian")
        {
            // The FBX bundles a floor, rocks, poles and flags around the actual figure.
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
                if (renderer.name.StartsWith("Plane.") || renderer.name.StartsWith("Cube.") ||
                    renderer.name.StartsWith("Cylinder."))
                    renderer.enabled = false;
        }
        FitModel(instance, footPoint, height, facing);
        RemoveModelColliders(instance);
    }

    private static void RestoreDancerTextures(GameObject instance)
    {
        string folder = "Assets/Project files/2/";
        Dictionary<string, string> maps = new Dictionary<string, string> {
            {"Fabric", "shibiyama_Fabric_BaseColor.png"},
            {"Body Boots", "shibiyama_Body_Boots_BaseColor.png"},
            {"mask", "shibiyama_mask_BaseColor.png"},
            {"Grass", "bg_Grass_BaseColor.png"}
        };
        foreach (KeyValuePair<string, string> pair in maps)
        {
            string path = MaterialFolder + "/Dancer_" + pair.Key.Replace(" ", "") + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(folder + pair.Value);
            if (texture == null) throw new Exception("Missing dancer texture: " + pair.Value);
            mat.SetTexture("_BaseMap", texture);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0.13f);
            EditorUtility.SetDirty(mat);
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                Material[] slots = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < slots.Length; i++)
                    if (slots[i] != null && slots[i].name == pair.Key)
                    { slots[i] = mat; changed = true; }
                if (changed) renderer.sharedMaterials = slots;
            }
        }
    }

    private static void DetailFlatCharacter(GameObject instance)
    {
        Texture2D cloth = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/Cloth_Detail.png");
        Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/Cloth_Normal.png");
        if (cloth == null || normal == null) throw new Exception("Character detail maps are missing");
        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            Material[] slots = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < slots.Length; i++)
            {
                Material source = slots[i];
                if (source == null || source.mainTexture != null ||
                    source.name.StartsWith("Actor_") || source.name == "eyes" ||
                    source.name == "head" || source.name == "hair" ||
                    source.name == "rose" || source.name == "mask" ||
                    source.name == "Material.002" || source.name == "Material.082" ||
                    source.name == "Telport Area") continue;
                string safeName = source.name.Replace(" ", "").Replace(".", "_");
                string path = MaterialFolder + "/Actor_" + safeName + ".mat";
                Material detailed = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (detailed == null)
                {
                    detailed = new Material(source);
                    AssetDatabase.CreateAsset(detailed, path);
                }
                detailed.SetTexture("_BaseMap", cloth);
                detailed.SetTexture("_BumpMap", normal);
                detailed.SetFloat("_BumpScale", 0.18f);
                detailed.SetFloat("_Smoothness", 0.12f);
                detailed.EnableKeyword("_NORMALMAP");
                EditorUtility.SetDirty(detailed);
                slots[i] = detailed;
                changed = true;
            }
            if (changed) renderer.sharedMaterials = slots;
        }
    }

    private static void FitModel(GameObject instance, Vector3 footPoint, float desiredHeight,
        float facing)
    {
        instance.transform.rotation = Quaternion.Euler(0f, facing, 0f);
        Renderer[] renderers = Array.FindAll(instance.GetComponentsInChildren<Renderer>(true), r => r.enabled);
        if (renderers.Length == 0) throw new Exception("Character has no visible renderers: " + instance.name);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        if (bounds.size.y < 0.001f) throw new Exception("Character has zero height: " + instance.name);
        float factor = desiredHeight / bounds.size.y;
        instance.transform.localScale *= factor;
        bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        instance.transform.position += new Vector3(footPoint.x - bounds.center.x,
            footPoint.y - bounds.min.y, footPoint.z - bounds.center.z);
        EditorUtility.SetDirty(instance);
        Debug.Log("Grounded character " + instance.name + " bottom=" + footPoint.y +
            " height=" + desiredHeight + " at " + footPoint);
    }

    private static void RemoveModelColliders(GameObject instance)
    {
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            UnityEngine.Object.DestroyImmediate(collider);
        // Several source FBX files contain 1000-intensity authoring lights.
        // Their copies wash out the entire VR gallery and nearby paths.
        foreach (Light light in instance.GetComponentsInChildren<Light>(true))
            light.enabled = false;
        foreach (Camera camera in instance.GetComponentsInChildren<Camera>(true))
            UnityEngine.Object.DestroyImmediate(camera.gameObject);
    }

    private static void Landscape()
    {
        Transform p = Group("04 - Floating islands, trees and cloud sea");
        for (int i = 0; i < 12; i++)
        {
            float x = -34f - (i % 3) * 3.3f;
            float z = 28f + (i / 3) * 11.5f;
            CloudCluster(p, x, -1.3f, z, 1.4f + (i % 3) * 0.2f, i + 30);
        }
        for (int i = 0; i < 9; i++)
        {
            float x = 72f + (i % 3) * 3f;
            float z = 50f + (i / 3) * 8f;
            CloudCluster(p, x, -1.1f, z, 1.3f, i + 50);
        }
        for (int i = 0; i < 7; i++)
        {
            float x = i % 2 == 0 ? -35f : 75f;
            float z = 32f + i * 5.4f;
            Pillar("Distant floating rock", p, x, z, 2.5f + (i % 3), 0.8f,
                0.8f + (i % 2) * 0.3f, "BlueStone", false);
            Orb("Floating island soft top", p, new Vector3(x, 3.7f + (i % 3), z),
                new Vector3(2.3f, 0.5f, 2.3f), "Mist");
        }
        for (int i = 0; i < 8; i++)
        {
            float x = i % 2 == 0 ? -34f : 74f;
            float z = 29f + (i / 2) * 11f;
            Tree(p, x, z, i);
        }
    }

    private static void Tree(Transform p, float x, float z, int seed)
    {
        Pillar("Ivory tree trunk", p, x, z, 0f, 2.4f + (seed % 3) * 0.3f,
            0.13f, "IvoryBark", false);
        float top = 2.8f + (seed % 3) * 0.3f;
        for (int i = 0; i < 5; i++)
        {
            float a = i * 1.25f + seed * 0.6f;
            Orb("Soft tree crown", p,
                new Vector3(x + Mathf.Cos(a) * 0.65f, top + (i % 2) * 0.35f,
                    z + Mathf.Sin(a) * 0.65f),
                new Vector3(1.35f, 0.85f, 1.25f), "LeafCanopy");
        }
    }

    private static void CloudCluster(Transform p, float x, float y, float z, float size, int seed)
    {
        for (int i = 0; i < 4; i++)
        {
            float a = i * 1.57f + seed * 0.41f;
            Orb("Cloud bank", p,
                new Vector3(x + Mathf.Cos(a) * size * 0.65f,
                    y + (i % 2) * 0.17f, z + Mathf.Sin(a) * size * 0.5f),
                new Vector3(size * 1.8f, size * 0.65f, size * 1.45f), "Cloud");
        }
    }

    private static void AddLight(Transform parent, string name, Vector3 position,
        Color color, float intensity, float range)
    {
        GameObject obj = new GameObject(name, typeof(Light));
        obj.transform.SetParent(parent, false);
        obj.transform.position = position;
        Light light = obj.GetComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity * 0.7f;
        light.range = range * 0.82f;
        light.shadows = LightShadows.None;
    }

    public static void ValidateBoth()
    {
        Validate(Quest);
        Validate(Desktop);
    }

    private static Transform FindNamed(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private static void CheckGrounded(string name, float expectedBottom)
    {
        GameObject actor = GameObject.Find(name);
        if (actor == null || !actor.activeInHierarchy)
            throw new Exception("Visible character is missing: " + name);
        Renderer[] renderers = Array.FindAll(actor.GetComponentsInChildren<Renderer>(true), r => r.enabled);
        if (renderers.Length == 0) throw new Exception("Character has no visible mesh: " + name);
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        if (Mathf.Abs(bounds.min.y - expectedBottom) > 0.12f)
            throw new Exception("Character is not grounded: " + name +
                " bottom=" + bounds.min.y + " expected=" + expectedBottom);
    }

    private static void Validate(string scenePath)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject built = GameObject.Find(RootName);
        SkyQuestSanctuary experience = UnityEngine.Object.FindFirstObjectByType<SkyQuestSanctuary>();
        if (built == null || experience == null || experience.beacons == null ||
            experience.beacons.Length != 4 || experience.gate == null || experience.reward == null)
            throw new Exception("Sanctuary extension is incomplete in " + scenePath);
        int renderers = built.GetComponentsInChildren<Renderer>(true).Length;
        int colliders = built.GetComponentsInChildren<Collider>(true).Length;
        if (renderers < 2000 || colliders < 170)
            throw new Exception("Expanded sanctuary geometry unexpectedly small in " + scenePath);
        Transform railing = FindNamed(built.transform, "Open sky railing");
        Transform railingPost = FindNamed(built.transform, "Railing post");
        if (railing == null || railing.GetComponent<Collider>() == null ||
            railingPost == null || railingPost.GetComponent<Collider>() == null)
            throw new Exception("Arrival railing is missing collision in " + scenePath);
        const float oldFloorArea = 22f * 6.5f + 20.5f * 24f + 27f * 22f + 6f * 5f;
        float newFloorArea = 0f;
        foreach (string name in new[] { "West cloister floor", "Grand northern court floor",
            "Eastern amphitheater floor", "East gallery connector" })
        {
            Transform floor = FindNamed(built.transform, name);
            if (floor == null) throw new Exception("Missing expanded floor: " + name);
            Bounds bounds = floor.GetComponent<Renderer>().bounds;
            newFloorArea += bounds.size.x * bounds.size.z;
        }
        if (oldFloorArea + newFloorArea < oldFloorArea * 3f)
            throw new Exception("Playable floor area did not reach three times the previous area.");
        CheckGrounded("COTL traveller", 0.16f);
        CheckGrounded("Candle pilgrim", 0.16f);
        CheckGrounded("Shibiyama dancer", 0.65f);
        CheckGrounded("Sky prayer guardian", 0.65f);
        CheckGrounded("Skeleton pilgrim - grounded gallery character", 0.16f);
        CheckGrounded("Original C1 player - visible gallery character", 0.16f);
        foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (light.enabled && light.gameObject.activeInHierarchy && light.intensity > 10f)
                throw new Exception("Imported model light is overexposing the scene: " + light.name);
        Renderer dancer = GameObject.Find("Shibiyama dancer").GetComponentInChildren<Renderer>();
        if (dancer == null || !Array.Exists(
            GameObject.Find("Shibiyama dancer").GetComponentsInChildren<Renderer>(true),
            r => Array.Exists(r.sharedMaterials,
                m => m != null && m.name == "Dancer_Fabric" && m.mainTexture != null)))
            throw new Exception("Dancer fabric texture was not restored.");
        Debug.Log("Sanctuary validation passed: " + scenePath +
            " renderers=" + renderers + " colliders=" + colliders +
            " total-floor-m2=" + (oldFloorArea + newFloorArea) +
            " previous-floor-m2=" + oldFloorArea);
    }
}
