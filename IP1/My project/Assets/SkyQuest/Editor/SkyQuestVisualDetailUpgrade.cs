using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Improves the authored sanctuary without rebuilding it or changing gameplay colliders.
[InitializeOnLoad]
public static class SkyQuestVisualDetailUpgrade
{
    private const string ScenePath = "Assets/SkyQuest/SkyQuest3S.unity";
    private const string MarkerName = "SkyQuest visual detail v2";
    private const string MeshFolder = "Assets/SkyQuest/Environment/Meshes";
    private const string Source = "Assets/Project files/";
    private const string Materials = "Assets/SkyQuest/Environment/Materials/";
    private static bool attempted;

    static SkyQuestVisualDetailUpgrade()
    {
        EditorApplication.update += UpgradeWhenReady;
    }

    private static void UpgradeWhenReady()
    {
        if (attempted || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            return;
        if (SceneManager.GetActiveScene().path != ScenePath) return;
        attempted = true;
        EditorApplication.update -= UpgradeWhenReady;
        if (GameObject.Find(MarkerName) != null &&
            AssetDatabase.LoadAssetAtPath<Mesh>(MeshFolder + "/BeveledArchitecture.asset") != null)
            return;
        UpgradeOpenScene();
    }

    [MenuItem("SKY/Improve visual detail in open scene")]
    public static void UpgradeOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath && scene.path != "Assets/SkyQuest/SkyDesktop.unity") return;
        bool alreadyUpgraded = GameObject.Find(MarkerName) != null;

        if (!AssetDatabase.IsValidFolder(MeshFolder))
            AssetDatabase.CreateFolder("Assets/SkyQuest/Environment", "Meshes");
        Mesh smooth = LoadOrCreateMesh("SmoothCylinder96", CreateCylinder(false));
        Mesh fluted = LoadOrCreateMesh("FlutedColumn96", CreateCylinder(true));
        Mesh sphere = LoadOrCreateMesh("SmoothSphere96", CreateSphere());
        Mesh bevel = LoadOrCreateMesh("BeveledArchitecture", CreateBeveledCube());
        if (alreadyUpgraded)
        {
            int blocks = UpgradeArchitecture(scene, bevel);
            if (blocks > 0) EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"SkyQuest visual detail refreshed; {blocks} architectural blocks refined.");
            return;
        }
        int cylinders = 0, spheres = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            Mesh old = filter.sharedMesh;
            if (old == null) continue;
            Vector3 size = filter.GetComponent<Renderer>()?.bounds.size ?? Vector3.zero;
            if (old.name == "Cylinder" && (size.y > 1.2f || Mathf.Max(size.x, size.z) > 0.45f))
            {
                filter.sharedMesh = size.y > 1.2f ? fluted : smooth;
                EditorUtility.SetDirty(filter);
                cylinders++;
            }
            else if (old.name == "Sphere" && Mathf.Max(size.x, size.y, size.z) > 0.5f)
            {
                filter.sharedMesh = sphere;
                EditorUtility.SetDirty(filter);
                spheres++;
            }
        }

        int architecture = UpgradeArchitecture(scene, bevel);

        UpgradeMaterials();
        var marker = new GameObject(MarkerName);
        SceneManager.MoveGameObjectToScene(marker, scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"SkyQuest visual detail upgraded: {cylinders} columns/discs, {spheres} large orbs, {architecture} architectural blocks; existing 8K source maps imported with Quest texture limits.");
    }

    private static int UpgradeArchitecture(Scene scene, Mesh bevel)
    {
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name.Contains("player") || root.name.Contains("Origin") || root.name.Contains("Ancestor"))
                continue;
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null || filter.sharedMesh.name != "Cube") continue;
                Renderer renderer = filter.GetComponent<Renderer>();
                if (renderer == null) continue;
                Vector3 size = renderer.bounds.size;
                if (Mathf.Max(size.x, size.y, size.z) < 1.5f) continue;
                filter.sharedMesh = bevel;
                EditorUtility.SetDirty(filter);
                count++;
            }
        }
        if (count > 0) EditorSceneManager.MarkSceneDirty(scene);
        return count;
    }

    private static Mesh LoadOrCreateMesh(string name, Mesh mesh)
    {
        mesh.name = name;
        string path = MeshFolder + "/" + name + ".asset";
        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(mesh, existing);
            UnityEngine.Object.DestroyImmediate(mesh);
            EditorUtility.SetDirty(existing);
            return existing;
        }
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Mesh CreateBeveledCube()
    {
        // Six smooth chamfered faces: 108 triangles versus the primitive cube's 12.
        float[] edge = { -0.5f, -0.46f, 0.46f, 0.5f };
        Vector3[] outward = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down };
        Vector3[] right = { Vector3.right, Vector3.left, Vector3.back, Vector3.forward, Vector3.right, Vector3.right };
        Vector3[] up = { Vector3.up, Vector3.up, Vector3.up, Vector3.up, Vector3.back, Vector3.forward };
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uv = new List<Vector2>();
        var triangles = new List<int>();
        for (int face = 0; face < 6; face++)
        {
            int offset = vertices.Count;
            for (int row = 0; row < 4; row++)
            for (int col = 0; col < 4; col++)
            {
                Vector3 point = outward[face] * 0.5f + right[face] * edge[col] + up[face] * edge[row];
                Vector3 core = new Vector3(Mathf.Clamp(point.x, -0.46f, 0.46f),
                    Mathf.Clamp(point.y, -0.46f, 0.46f), Mathf.Clamp(point.z, -0.46f, 0.46f));
                Vector3 normal = (point - core).normalized;
                vertices.Add(core + normal * 0.04f);
                normals.Add(normal);
                uv.Add(new Vector2(edge[col] + 0.5f, edge[row] + 0.5f));
                if (row == 0 || col == 0) continue;
                int a = offset + (row - 1) * 4 + col - 1;
                int b = a + 1, c = a + 4, d = c + 1;
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(b); triangles.Add(d); triangles.Add(c);
            }
        }
        var mesh = new Mesh();
        mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uv);
        mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateCylinder(bool flute)
    {
        const int sides = 96, rings = 5;
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var triangles = new List<int>();
        for (int ring = 0; ring <= rings; ring++)
        for (int i = 0; i <= sides; i++)
        {
            float t = i / (float)sides, angle = t * Mathf.PI * 2f;
            float r = 0.5f * (flute ? 1f + 0.024f * Mathf.Cos(angle * 16f) : 1f);
            vertices.Add(new Vector3(Mathf.Cos(angle) * r, -1f + 2f * ring / rings, Mathf.Sin(angle) * r));
            uv.Add(new Vector2(t, ring / (float)rings));
            if (ring == 0 || i == 0) continue;
            int a = (ring - 1) * (sides + 1) + i;
            int b = a + sides + 1;
            triangles.Add(a - 1); triangles.Add(b - 1); triangles.Add(a);
            triangles.Add(a); triangles.Add(b - 1); triangles.Add(b);
        }
        for (int cap = 0; cap < 2; cap++)
        {
            float y = cap == 0 ? -1f : 1f;
            int center = vertices.Count;
            vertices.Add(new Vector3(0f, y, 0f)); uv.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= sides; i++)
            {
                float angle = i * Mathf.PI * 2f / sides;
                float r = 0.5f * (flute ? 1f + 0.024f * Mathf.Cos(angle * 16f) : 1f);
                vertices.Add(new Vector3(Mathf.Cos(angle) * r, y, Mathf.Sin(angle) * r));
                uv.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
                if (i == 0) continue;
                triangles.Add(center); triangles.Add(center + i + (cap == 0 ? 0 : 1));
                triangles.Add(center + i + (cap == 0 ? 1 : 0));
            }
        }
        var mesh = new Mesh();
        mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateSphere()
    {
        const int longitude = 96, latitude = 48;
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var triangles = new List<int>();
        for (int y = 0; y <= latitude; y++)
        for (int x = 0; x <= longitude; x++)
        {
            float u = x / (float)longitude, v = y / (float)latitude;
            float a = u * Mathf.PI * 2f, b = v * Mathf.PI;
            vertices.Add(new Vector3(Mathf.Sin(b) * Mathf.Cos(a), Mathf.Cos(b),
                Mathf.Sin(b) * Mathf.Sin(a)) * 0.5f);
            uv.Add(new Vector2(u, v));
            if (x == 0 || y == 0) continue;
            int current = y * (longitude + 1) + x;
            int previous = current - (longitude + 1);
            triangles.Add(previous - 1); triangles.Add(previous); triangles.Add(current - 1);
            triangles.Add(previous); triangles.Add(current); triangles.Add(current - 1);
        }
        var mesh = new Mesh();
        mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    private static void UpgradeMaterials()
    {
        string wood = Source + "material/wood/Wood050_8K-JPG_";
        string metal = Source + "material/metal/Metal047A_8K-JPG_";
        string ground = Source + "land/Ground110_8K-JPG_";
        AssignMaps("HoneyWood", wood + "Color.jpg", wood + "NormalGL.jpg", 0.27f);
        AssignMaps("PaleGold", null, metal + "NormalGL.jpg", 0.18f);
        AssignMaps("DeepGold", null, metal + "NormalGL.jpg", 0.24f);
        Texture2D gravelColor = ImportMap(ground + "Color.jpg", false);
        Texture2D gravelNormal = ImportMap(ground + "NormalGL.jpg", true);
        Material gravel = AssetDatabase.LoadAssetAtPath<Material>(Materials + "GardenGravel.mat");
        if (gravel == null)
        {
            gravel = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(gravel, Materials + "GardenGravel.mat");
        }
        gravel.SetColor("_BaseColor", new Color(0.76f, 0.82f, 0.82f));
        gravel.SetTexture("_BaseMap", gravelColor);
        gravel.SetTexture("_BumpMap", gravelNormal);
        gravel.SetFloat("_BumpScale", 0.2f);
        gravel.EnableKeyword("_NORMALMAP");
        gravel.SetTextureScale("_BaseMap", new Vector2(6f, 6f));
        gravel.SetTextureScale("_BumpMap", new Vector2(6f, 6f));
        EditorUtility.SetDirty(gravel);
        GameObject garden = GameObject.Find("Garden limestone base");
        if (garden != null)
        {
            var renderer = garden.GetComponent<Renderer>();
            if (renderer != null) { renderer.sharedMaterial = gravel; EditorUtility.SetDirty(renderer); }
        }
    }

    private static void AssignMaps(string name, string colorPath, string normalPath, float normalStrength)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(Materials + name + ".mat");
        if (material == null) return;
        if (colorPath != null) material.SetTexture("_BaseMap", ImportMap(colorPath, false));
        material.SetTexture("_BumpMap", ImportMap(normalPath, true));
        material.SetFloat("_BumpScale", normalStrength);
        material.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(material);
    }

    private static Texture2D ImportMap(string path, bool normal)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new Exception("SkyQuest texture missing: " + path);
        bool changed = false;
        TextureImporterType type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        if (importer.textureType != type) { importer.textureType = type; changed = true; }
        if (importer.maxTextureSize != 4096) { importer.maxTextureSize = 4096; changed = true; }
        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
        if (!android.overridden || android.maxTextureSize != 1024)
        {
            android.name = "Android"; android.overridden = true; android.maxTextureSize = 1024;
            importer.SetPlatformTextureSettings(android); changed = true;
        }
        if (changed) importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
