using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Keeps the same hand-built course in the desktop and Quest versions of the sanctuary.
public static class SkyQuestFlightFlameInstaller
{
    private const string DesktopScene = "Assets/SkyQuest/SkyDesktop.unity";
    private const string QuestScene = "Assets/SkyQuest/SkyQuest3S.unity";
    private const string MaterialFolder = "Assets/SkyQuest/FlightMaterials";
    private const string FlameMeshPath = MaterialFolder + "/Sky flight flame.asset";
    private const string HaloMeshPath = MaterialFolder + "/Sky flight halo.asset";
    private static readonly Vector3[] FlamePositions =
    {
        new Vector3(43f, 3.4f, 44f), new Vector3(47f, 4.2f, 47f),
        new Vector3(52f, 5.0f, 49f), new Vector3(57f, 5.6f, 46f),
        new Vector3(62f, 6.1f, 43f), new Vector3(67f, 6.4f, 47f),
        new Vector3(64f, 7.0f, 53f), new Vector3(58f, 6.5f, 56f),
        new Vector3(52f, 5.8f, 59f), new Vector3(46f, 5.2f, 57f),
        new Vector3(41f, 4.6f, 53f), new Vector3(39f, 3.7f, 47f)
    };

    [MenuItem("SKY/Install flying flame game in both scenes")]
    public static void Install()
    {
        string previous = SceneManager.GetActiveScene().path;
        if (SceneManager.GetActiveScene().isDirty)
            EditorSceneManager.SaveOpenScenes();
        EnsureAssets(out Mesh flameMesh, out Material outer, out Material inner,
            out Material spark, out Material start);
        Mesh haloMesh = HaloMesh();
        InstallInScene(DesktopScene, flameMesh, haloMesh, outer, inner, spark, start);
        InstallInScene(QuestScene, flameMesh, haloMesh, outer, inner, spark, start);
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        Debug.Log("Flying flame game installed in desktop and Quest sanctuary scenes.");
    }

    [MenuItem("SKY/Polish flying flames in both scenes")]
    public static void PolishExistingFlames()
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)
        {
            Debug.LogError("Stop Play mode and save the current scene before polishing the flying flames.");
            return;
        }
        string previous = SceneManager.GetActiveScene().path;
        EnsureAssets(out Mesh flameMesh, out Material outer, out Material inner,
            out Material spark, out _);
        Mesh refined = BuildFlameMesh();
        flameMesh.Clear();
        flameMesh.vertices = refined.vertices;
        flameMesh.triangles = refined.triangles;
        flameMesh.uv = refined.uv;
        flameMesh.RecalculateNormals();
        flameMesh.RecalculateBounds();
        UnityEngine.Object.DestroyImmediate(refined);
        EditorUtility.SetDirty(flameMesh);
        Mesh haloMesh = HaloMesh();
        PolishScene(DesktopScene, flameMesh, haloMesh, inner, spark);
        PolishScene(QuestScene, flameMesh, haloMesh, inner, spark);
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
        Debug.Log("Flying flames polished in desktop and Quest scenes.");
    }

    private static void PolishScene(string path, Mesh flameMesh, Mesh haloMesh,
        Material inner, Material spark)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        GameObject challenge = GameObject.Find("Flying flame challenge");
        if (challenge == null) throw new Exception("Flying flame challenge missing from " + path);
        SkyQuestFlightFlame[] flames = challenge.GetComponentsInChildren<SkyQuestFlightFlame>(true);
        if (flames.Length != FlamePositions.Length)
            throw new Exception("Expected " + FlamePositions.Length + " flames in " + path);
        foreach (SkyQuestFlightFlame flame in flames)
        {
            Transform parent = flame.transform;
            foreach (MeshFilter filter in parent.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != haloMesh) filter.sharedMesh = flameMesh;
            AddRefinedDetails(parent, flameMesh, haloMesh, inner, spark);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, path))
            throw new Exception("Could not save polished flames in " + path);
    }

    private static void InstallInScene(string path, Mesh mesh, Mesh haloMesh, Material outer,
        Material inner, Material spark, Material start)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        GameObject existing = GameObject.Find("Flying flame challenge");
        if (existing != null) UnityEngine.Object.DestroyImmediate(existing);
        GameObject root = new GameObject("Flying flame challenge");
        SkyQuestFlightFlameGame game = root.AddComponent<SkyQuestFlightFlameGame>();
        game.roundSeconds = 90f;
        game.pointsPerFlame = 10;

        float ground = GroundHeight(40.5f, 41.5f);
        GameObject startRoot = new GameObject("Blue flight challenge start ring");
        startRoot.transform.SetParent(root.transform, false);
        startRoot.transform.position = new Vector3(40.5f, ground + .08f, 41.5f);
        game.startMarker = startRoot.transform;
        BuildStartRing(startRoot.transform, start);

        game.flames = new SkyQuestFlightFlame[FlamePositions.Length];
        for (int i = 0; i < FlamePositions.Length; i++)
        {
            Vector3 position = ClearPosition(FlamePositions[i]);
            GameObject flameRoot = new GameObject("Flight flame " + (i + 1).ToString("00"));
            flameRoot.transform.SetParent(root.transform, false);
            flameRoot.transform.position = position;
            SkyQuestFlightFlame flame = flameRoot.AddComponent<SkyQuestFlightFlame>();
            flame.collectRadius = 1.2f;
            FlamePart(flameRoot.transform, "Amber flame", mesh, outer,
                Vector3.zero, Vector3.one);
            FlamePart(flameRoot.transform, "Golden inner flame", mesh, inner,
                new Vector3(0f, -.12f, -.02f), Vector3.one * .55f);
            for (int tongue = 0; tongue < 3; tongue++)
            {
                float tongueAngle = tongue * 120f * Mathf.Deg2Rad;
                FlamePart(flameRoot.transform, "Visible golden flame tongue", mesh, inner,
                    new Vector3(Mathf.Cos(tongueAngle) * .32f, -.12f,
                        Mathf.Sin(tongueAngle) * .32f), Vector3.one * .52f);
            }
            for (int sparkIndex = 0; sparkIndex < 3; sparkIndex++)
            {
                float angle = (sparkIndex * 120f + i * 25f) * Mathf.Deg2Rad;
                GameObject sparkObject = Decor(PrimitiveType.Sphere,
                    "Floating flame spark", flameRoot.transform, spark);
                sparkObject.transform.localPosition = new Vector3(Mathf.Cos(angle) * .39f,
                    (sparkIndex - 1) * .22f, Mathf.Sin(angle) * .39f);
                sparkObject.transform.localScale = Vector3.one * .09f;
            }
            AddRefinedDetails(flameRoot.transform, mesh, haloMesh, inner, spark);
            game.flames[i] = flame;
        }
        game.player = FindScenePlayer(scene, path == DesktopScene);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, path))
            throw new Exception("Could not save flight game into " + path);
    }

    private static SkyQuestLocomotion FindScenePlayer(Scene scene, bool desktop)
    {
        SkyQuestLocomotion fallback = null;
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (SkyQuestLocomotion candidate in root.GetComponentsInChildren<SkyQuestLocomotion>(true))
            {
                if (fallback == null) fallback = candidate;
                if (candidate.desktopControlled == desktop) return candidate;
            }
        return fallback;
    }

    private static void BuildStartRing(Transform parent, Material start)
    {
        const int count = 20;
        for (int i = 0; i < count; i++)
        {
            float a = i * Mathf.PI * 2f / count;
            float b = (i + 1) * Mathf.PI * 2f / count;
            Vector3 p = new Vector3(Mathf.Cos(a) * 1.25f, 0f, Mathf.Sin(a) * 1.25f);
            Vector3 q = new Vector3(Mathf.Cos(b) * 1.25f, 0f, Mathf.Sin(b) * 1.25f);
            GameObject segment = Decor(PrimitiveType.Cube, "Start ring edge", parent, start);
            segment.transform.localPosition = (p + q) * .5f;
            segment.transform.localRotation = Quaternion.FromToRotation(Vector3.right, (q - p).normalized);
            segment.transform.localScale = new Vector3(Vector3.Distance(p, q) + .03f, .06f, .12f);
        }
        GameObject beacon = Decor(PrimitiveType.Sphere, "Start beacon", parent, start);
        beacon.transform.localPosition = Vector3.up * 1.45f;
        beacon.transform.localScale = Vector3.one * .24f;
        GameObject stem = Decor(PrimitiveType.Cylinder, "Start beacon ray", parent, start);
        stem.transform.localPosition = Vector3.up * .75f;
        stem.transform.localScale = new Vector3(.035f, .7f, .035f);
    }

    private static GameObject FlamePart(Transform parent, string name, Mesh mesh, Material material,
        Vector3 localPosition, Vector3 localScale)
    {
        GameObject obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = localScale;
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        obj.GetComponent<MeshRenderer>().sharedMaterial = material;
        return obj;
    }

    private static void AddRefinedDetails(Transform parent, Mesh flameMesh, Mesh haloMesh,
        Material inner, Material spark)
    {
        if (parent.Find("White-hot heart") == null)
            FlamePart(parent, "White-hot heart", flameMesh, spark,
                new Vector3(0f, -.16f, 0f), new Vector3(.28f, .53f, .28f));
        for (int i = 0; i < 2; i++)
        {
            string name = "Fine gold wisp " + (i + 1);
            if (parent.Find(name) != null) continue;
            float side = i == 0 ? -1f : 1f;
            GameObject wisp = FlamePart(parent, name, flameMesh, inner,
                new Vector3(side * .19f, -.19f, i == 0 ? -.1f : .1f),
                new Vector3(.22f, .53f, .22f));
            wisp.transform.localRotation = Quaternion.Euler(0f, 0f, side * 18f);
        }
        if (parent.Find("Golden base halo") == null)
        {
            GameObject halo = FlamePart(parent, "Golden base halo", haloMesh, spark,
                new Vector3(0f, -.35f, 0f), Vector3.one * .61f);
            halo.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
        }
    }

    private static GameObject Decor(PrimitiveType type, string name, Transform parent, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        return obj;
    }

    private static Vector3 ClearPosition(Vector3 position)
    {
        Physics.SyncTransforms();
        for (int attempt = 0; attempt < 7; attempt++)
        {
            bool blocked = false;
            foreach (Collider hit in Physics.OverlapSphere(position, .9f, ~0, QueryTriggerInteraction.Ignore))
                if (hit != null && hit.gameObject.scene.IsValid()) { blocked = true; break; }
            if (!blocked) return position;
            position.y += .7f;
        }
        return position;
    }

    private static float GroundHeight(float x, float z)
    {
        float best = float.NegativeInfinity;
        foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(x, 18f, z), Vector3.down, 24f,
                     ~0, QueryTriggerInteraction.Ignore))
            if (hit.normal.y > .6f && hit.point.y < 2.5f && hit.point.y > best)
                best = hit.point.y;
        return float.IsNegativeInfinity(best) ? 0f : best;
    }

    private static void EnsureAssets(out Mesh mesh, out Material outer, out Material inner,
        out Material spark, out Material start)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/SkyQuest", "FlightMaterials");
        mesh = AssetDatabase.LoadAssetAtPath<Mesh>(FlameMeshPath);
        if (mesh == null)
        {
            mesh = BuildFlameMesh();
            AssetDatabase.CreateAsset(mesh, FlameMeshPath);
        }
        outer = MaterialAsset("Flight flame amber", new Color(1f, .56f, .12f));
        inner = MaterialAsset("Flight flame gold", new Color(1f, .91f, .3f));
        spark = MaterialAsset("Flight flame sparks", new Color(1f, .96f, .58f));
        start = MaterialAsset("Flight game start blue", new Color(.16f, .82f, 1f));
    }

    private static Material MaterialAsset(string name, Color color)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            material.color = color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2f);
            EditorUtility.SetDirty(material);
            return material;
        }
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        material = new Material(shader) { name = name, color = color };
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", color * 2f);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Mesh BuildFlameMesh()
    {
        const int sides = 16;
        float[] heights = { -.44f, -.37f, -.23f, -.02f, .17f, .34f, .52f, .68f, .81f };
        float[] radii = { .12f, .22f, .32f, .34f, .29f, .22f, .15f, .075f, .012f };
        int rings = heights.Length;
        Vector3[] vertices = new Vector3[sides * rings];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[sides * (rings - 1) * 6];
        for (int ring = 0; ring < rings; ring++)
        {
            float progress = (float)ring / (rings - 1);
            for (int side = 0; side < sides; side++)
            {
                float angle = side * Mathf.PI * 2f / sides;
                float ripple = 1f + .09f * Mathf.Sin(angle * 3f + progress * 5f);
                float bend = .12f * progress * progress;
                int index = ring * sides + side;
                vertices[index] = new Vector3(Mathf.Cos(angle) * radii[ring] * ripple + bend,
                    heights[ring], Mathf.Sin(angle) * radii[ring] * ripple - bend * .35f);
                uv[index] = new Vector2((float)side / sides, progress);
                if (ring == rings - 1) continue;
                int next = (side + 1) % sides;
                int t = (ring * sides + side) * 6;
                triangles[t] = index;
                triangles[t + 1] = index + sides;
                triangles[t + 2] = ring * sides + next;
                triangles[t + 3] = ring * sides + next;
                triangles[t + 4] = index + sides;
                triangles[t + 5] = (ring + 1) * sides + next;
            }
        }
        Mesh result = new Mesh { name = "Sky flight flame" };
        result.vertices = vertices;
        result.triangles = triangles;
        result.uv = uv;
        result.RecalculateNormals();
        result.RecalculateBounds();
        return result;
    }

    private static Mesh HaloMesh()
    {
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(HaloMeshPath);
        if (mesh != null) return mesh;
        const int segments = 24;
        const int sides = 5;
        Vector3[] vertices = new Vector3[segments * sides];
        int[] triangles = new int[segments * sides * 6];
        for (int i = 0; i < segments; i++)
        for (int j = 0; j < sides; j++)
        {
            float around = i * Mathf.PI * 2f / segments;
            float cross = j * Mathf.PI * 2f / sides;
            float radius = .52f + .025f * Mathf.Cos(cross);
            int v = i * sides + j;
            vertices[v] = new Vector3(Mathf.Cos(around) * radius,
                .025f * Mathf.Sin(cross), Mathf.Sin(around) * radius);
            int nextAround = ((i + 1) % segments) * sides;
            int nextCross = (j + 1) % sides;
            int t = v * 6;
            triangles[t] = v;
            triangles[t + 1] = nextAround + j;
            triangles[t + 2] = i * sides + nextCross;
            triangles[t + 3] = i * sides + nextCross;
            triangles[t + 4] = nextAround + j;
            triangles[t + 5] = nextAround + nextCross;
        }
        mesh = new Mesh { name = "Sky flight halo", vertices = vertices, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, HaloMeshPath);
        return mesh;
    }
}
