using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Creates an isolated, primitive-only Quest flight practice scene.</summary>
public static class CapeFlightTestLevelBuilder
{
    private const string ScenePath = "Assets/SkyQuest/CapeFlightTest.unity";
    private const string RigPath = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";
    private const string MaterialFolder = "Assets/SkyQuest/FlightMaterials";

    [MenuItem("SKY/Build cape flight test level")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigPath);
        if (prefab == null) throw new Exception("XRI XR Origin prefab is missing.");
        GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rig.name = "Cape Flight XR Origin";
        rig.transform.position = new Vector3(0f, 0.55f, 0f);
        foreach (MonoBehaviour behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour == null) continue;
            string name = behaviour.GetType().Name;
            if (name.Contains("MoveProvider") || name.Contains("TurnProvider") ||
                name.Contains("TeleportationProvider") || name.Contains("JumpProvider") ||
                name.Contains("ClimbProvider") || name.Contains("GrabMoveProvider") ||
                name.Contains("GravityProvider") || behaviour.gameObject.name == "Gravity")
                behaviour.enabled = false;
        }
        Camera head = rig.GetComponentInChildren<Camera>(true);
        Transform left = Find(rig.transform, "Left Controller");
        Transform right = Find(rig.transform, "Right Controller");
        if (head == null || left == null || right == null) throw new Exception("XR rig camera/controllers missing.");
        head.gameObject.SetActive(true);
        head.tag = "MainCamera";
        SkyQuestLocomotion locomotion = rig.AddComponent<SkyQuestLocomotion>();
        locomotion.head = head.transform;
        locomotion.leftHand = left;
        locomotion.rightHand = right;
        locomotion.groundHeight = -4.3f;
        locomotion.playRadius = 90f;
        rig.AddComponent<CapeEnergySystem>();
        CapeFlightController flight = rig.AddComponent<CapeFlightController>();
        flight.actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
            "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");
        rig.AddComponent<CapeEnergyUI>();
        rig.AddComponent<CapeFlightFeedback>();
        rig.AddComponent<CapeRechargeReceiver>();
        rig.AddComponent<CapeFlightCourseRuntime>();

        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/SkyQuest", "FlightMaterials");
        Material stone = GetMaterial("Training stone", new Color(0.20f, 0.35f, 0.43f));
        Material safe = GetMaterial("Safety floor", new Color(0.07f, 0.17f, 0.20f));
        Material light = GetMaterial("Recharge gold", new Color(1f, 0.80f, 0.20f));
        Material goal = GetMaterial("Goal cyan", new Color(0.15f, 0.92f, 0.72f));
        GameObject root = new GameObject("Cape flight training course");
        Platform(root.transform, "1 Starting platform", new Vector3(0f, -0.30f, 0f), new Vector3(6f, 0.6f, 6f), stone);
        Platform(root.transform, "2 Flight training A", new Vector3(0f, -0.10f, 6f), new Vector3(4f, 0.6f, 4f), stone);
        Platform(root.transform, "2 Flight training B", new Vector3(1f, 0.25f, 12f), new Vector3(4f, 0.6f, 4f), stone);
        Platform(root.transform, "3 Ground recharge platform", new Vector3(1f, 0.50f, 18f), new Vector3(5f, 0.6f, 5f), stone);
        Platform(root.transform, "4 Planning platform A", new Vector3(-1f, 0.70f, 25f), new Vector3(4f, 0.6f, 4f), stone);
        Platform(root.transform, "4 Planning platform B", new Vector3(1f, 0.85f, 31f), new Vector3(4f, 0.6f, 4f), stone);
        Platform(root.transform, "5 Final destination", new Vector3(0f, 1.05f, 38f), new Vector3(6f, 0.6f, 6f), goal);
        Platform(root.transform, "Recovery floor", new Vector3(0f, -4.60f, 18f), new Vector3(26f, 0.6f, 50f), safe);
        Recharge(root.transform, "Air light source", new Vector3(0f, 2.2f, 10f), 2f, false, light);
        Recharge(root.transform, "Ground recharge", new Vector3(1f, 1.35f, 18f), 1f, true, light);
        Recharge(root.transform, "Challenge light source", new Vector3(0f, 2.1f, 28f), 2f, false, light);
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "Instant recharge +2";
        pickup.transform.SetParent(root.transform);
        pickup.transform.position = new Vector3(1f, 1.5f, 18f);
        pickup.transform.localScale = Vector3.one * 0.45f;
        pickup.GetComponent<Renderer>().sharedMaterial = light;
        pickup.AddComponent<CapeInstantRechargePickup>();
        Marker(root.transform, "Recharge checkpoint", new Vector3(1f, 1.25f, 18f),
            new Vector3(1f, 1.2f, 18f), false, light);
        Marker(root.transform, "Finish checkpoint", new Vector3(0f, 1.8f, 38f),
            new Vector3(0f, 1.85f, 38f), true, goal);
        Sign(root.transform, "Controls", new Vector3(-1.1f, 1.55f, 2.5f),
            "A: TAKE OFF / BOOST\nLEFT STICK: STEER\nRIGHT STICK: TURN\nR CLICK: SAFE STOP");
        Sign(root.transform, "Recharge instructions", new Vector3(-1.1f, 2.3f, 18f),
            "GOLD LIGHT  RECHARGES CAPE\nLAND HERE TO RECHARGE SLOWLY");
        Sign(root.transform, "Finish instructions", new Vector3(0f, 2.8f, 38f),
            "LAND HERE TO COMPLETE\nFALLS RETURN TO CHECKPOINT");
        GameObject sun = new GameObject("Sun", typeof(Light));
        sun.GetComponent<Light>().type = LightType.Directional;
        sun.GetComponent<Light>().intensity = 1.1f;
        sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        RenderSettings.ambientLight = new Color(0.45f, 0.55f, 0.65f);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new Exception("Could not save cape flight scene.");
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!buildScenes.Exists(item => item.path == ScenePath))
        {
            buildScenes.Add(new EditorBuildSettingsScene(ScenePath, false));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }
        Debug.Log("Cape flight test level built at " + ScenePath);
    }

    private static Material GetMaterial(string name, Color color)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        material = new Material(shader) { color = color };
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject Platform(Transform parent, string name, Vector3 center, Vector3 size, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = center;
        obj.transform.localScale = size;
        obj.GetComponent<Renderer>().sharedMaterial = material;
        return obj;
    }

    private static void Recharge(Transform parent, string name, Vector3 center, float rate, bool groundOnly, Material material)
    {
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.name = name;
        glow.transform.SetParent(parent);
        glow.transform.position = center;
        glow.transform.localScale = Vector3.one * 0.55f;
        glow.GetComponent<Renderer>().sharedMaterial = material;
        SphereCollider trigger = glow.GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = groundOnly ? 5f : 4f;
        CapeRechargeZone zone = glow.AddComponent<CapeRechargeZone>();
        zone.rechargePerSecond = rate;
        zone.groundOnly = groundOnly;
    }

    private static void Marker(Transform parent, string name, Vector3 center, Vector3 checkpoint,
        bool finish, Material material)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = name;
        marker.transform.SetParent(parent);
        marker.transform.position = center;
        marker.transform.localScale = new Vector3(0.8f, 0.03f, 0.8f);
        marker.GetComponent<Renderer>().sharedMaterial = material;
        UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
        BoxCollider trigger = marker.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(2f, 40f, 2f);
        CapeFlightCourseMarker courseMarker = marker.AddComponent<CapeFlightCourseMarker>();
        courseMarker.finish = finish;
        courseMarker.checkpointPosition = checkpoint;
    }

    private static void Sign(Transform parent, string name, Vector3 position, string message)
    {
        GameObject obj = new GameObject(name, typeof(TextMesh), typeof(MeshRenderer));
        obj.transform.SetParent(parent);
        obj.transform.position = position;
        TextMesh text = obj.GetComponent<TextMesh>();
        text.text = message;
        text.fontSize = 48;
        text.characterSize = 0.0085f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(0.08f, 0.16f, 0.20f);
        obj.AddComponent<CapeFlightCourseSign>();
    }

    private static Transform Find(Transform parent, string childName)
    {
        if (parent.name == childName) return parent;
        foreach (Transform child in parent)
        {
            Transform found = Find(child, childName);
            if (found != null) return found;
        }
        return null;
    }
}
