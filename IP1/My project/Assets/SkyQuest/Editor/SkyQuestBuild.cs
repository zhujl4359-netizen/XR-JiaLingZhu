using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using UnityEngine.XR.OpenXR.Features.CompositionLayers;

public static class SkyQuestBuild
{
    private const string SourceScene = "Assets/Scenes/SampleScene.unity";
    private const string QuestScene = "Assets/SkyQuest/SkyQuest3S.unity";
    private const string DesktopScene = "Assets/SkyQuest/SkyDesktop.unity";
    private const string RigPrefab = "Assets/Samples/XR Interaction Toolkit/3.6.0/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

    [MenuItem("SKY/Install Magic exchange trees in both scenes")]
    public static void InstallMagicTreesInBothScenes()
    {
        InstallMagicTree(QuestScene);
        InstallMagicTree(DesktopScene);
        AssetDatabase.SaveAssets();
        Debug.Log("Magic exchange trees installed for both plaza ancestors in Quest and desktop scenes.");
    }

    [MenuItem("SKY/Install ancestor memorial in both scenes")]
    public static void InstallAncestorMemorialBothScenes()
    {
        InstallAncestorMemorial(QuestScene);
        InstallAncestorMemorial(DesktopScene);
        AssetDatabase.SaveAssets();
        Debug.Log("Ancestor memorial installed in Quest and keyboard scenes.");
    }

    private static void InstallAncestorMemorial(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        SkyQuestAncestor tree = UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>();
        if (tree == null || tree.ancestor == null)
            throw new Exception("Original ancestor missing in " + scenePath);
        if (tree.memorialStone != null)
            UnityEngine.Object.DestroyImmediate(tree.memorialStone.gameObject);
        tree.memorialStone = BuildAncestorMemorial(tree.ancestor);
        EditorUtility.SetDirty(tree);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
            throw new Exception("Could not save memorial in " + scenePath);
    }

    private static Transform BuildAncestorMemorial(Transform ancestor)
    {
        Transform root = new GameObject("Ancestor memorial stone").transform;
        root.position = MemorialPosition(ancestor);
        Vector3 forward = VisualForward(ancestor);
        root.rotation = Quaternion.LookRotation(forward, Vector3.up);
        Material blue = AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/BlueStone.mat");
        Material limestone = AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/WarmLimestone.mat");
        Material gold = AssetDatabase.LoadAssetAtPath<Material>("Assets/SkyQuest/Environment/Materials/PaleGold.mat");
        MemorialPart(root, "Limestone footing", new Vector3(0f, 0.08f, 0f), new Vector3(1.18f, 0.16f, 0.52f), limestone);
        MemorialPart(root, "Blue stone tablet", new Vector3(0f, 1.12f, 0f), new Vector3(0.89f, 2.05f, 0.32f), blue);
        MemorialPart(root, "Carved crown", new Vector3(0f, 2.18f, 0f), new Vector3(1.04f, 0.12f, 0.39f), limestone, false);
        MemorialPart(root, "Left border", new Vector3(-0.39f, 1.13f, 0.17f), new Vector3(0.035f, 1.8f, 0.025f), limestone, false);
        MemorialPart(root, "Right border", new Vector3(0.39f, 1.13f, 0.17f), new Vector3(0.035f, 1.8f, 0.025f), limestone, false);
        MemorialPart(root, "Top carved line", new Vector3(0f, 1.91f, 0.17f), new Vector3(0.63f, 0.025f, 0.025f), gold, false);
        MemorialPart(root, "Lower carved line", new Vector3(0f, 0.37f, 0.17f), new Vector3(0.63f, 0.025f, 0.025f), gold, false);
        GameObject diamond = MemorialPart(root, "Memorial star", new Vector3(0f, 1.52f, 0.185f),
            new Vector3(0.23f, 0.23f, 0.025f), gold, false);
        diamond.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        MemorialPart(root, "Star upper ray", new Vector3(0f, 1.77f, 0.185f), new Vector3(0.018f, 0.16f, 0.02f), gold, false);
        MemorialPart(root, "Star lower ray", new Vector3(0f, 1.27f, 0.185f), new Vector3(0.018f, 0.16f, 0.02f), gold, false);
        MemorialPart(root, "Star left ray", new Vector3(-0.25f, 1.52f, 0.185f), new Vector3(0.16f, 0.018f, 0.02f), gold, false);
        MemorialPart(root, "Star right ray", new Vector3(0.25f, 1.52f, 0.185f), new Vector3(0.16f, 0.018f, 0.02f), gold, false);
        return root;
    }

    private static GameObject MemorialPart(Transform parent, string name, Vector3 position,
        Vector3 scale, Material material, bool collision = true)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        if (!collision) UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
        return part;
    }

    private static Vector3 MemorialPosition(Transform ancestor)
    {
        Bounds bounds = VisibleBounds(ancestor);
        float ground = bounds.size.y > 0.01f ? bounds.min.y : ancestor.position.y;
        Vector3 forward = VisualForward(ancestor);
        Vector3 behind = ancestor.position - forward * 0.95f;
        return new Vector3(behind.x, ground, behind.z);
    }

    private static Vector3 VisualForward(Transform ancestor)
    {
        // The COTL figure faces local -Z. C1 has a nested 180-degree visual,
        // so its root's +Z is the visible forward direction.
        bool reversed = ancestor.name == "COTL traveller";
        Vector3 axis = reversed ? -ancestor.forward : ancestor.forward;
        return Vector3.ProjectOnPlane(axis, Vector3.up).normalized;
    }

    private static void ValidateMemorial(SkyQuestAncestor tree)
    {
        ValidateMemorial(tree.ancestor, tree.memorialStone);
    }

    private static void ValidateMemorial(Transform ancestor, Transform memorial)
    {
        Vector3 forward = VisualForward(ancestor);
        Vector3 offset = Vector3.ProjectOnPlane(memorial.position - ancestor.position, Vector3.up);
        if (Mathf.Abs(Vector3.Dot(offset, forward) + 0.95f) > 0.05f ||
            Vector3.Cross(offset, forward).magnitude > 0.05f ||
            Vector3.Dot(memorial.forward, forward) < 0.99f)
            throw new Exception("Memorial is not centered directly behind its ancestor.");
        Transform tablet = memorial.Find("Blue stone tablet");
        if (tablet == null || tablet.GetComponent<Renderer>().sharedMaterial == null)
            throw new Exception("Memorial stone material is missing.");
    }

    private static void InstallMagicTree(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        SkyQuestAncestor original = UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>();
        SkyQuestStatus status = UnityEngine.Object.FindFirstObjectByType<SkyQuestStatus>();
        string[] names = { "Shibiyama dancer", "Sky prayer guardian", "COTL traveller",
            "Candle pilgrim", "Skeleton pilgrim - grounded gallery character",
            "Original C1 player - visible gallery character" };
        if (locomotion == null || original == null || status == null)
            throw new Exception("Cannot install Magic exchange; a required actor or gameplay system is missing in " + scenePath);
        SkyQuestMagicExchange magic = locomotion.GetComponent<SkyQuestMagicExchange>();
        if (magic == null) magic = locomotion.gameObject.AddComponent<SkyQuestMagicExchange>();
        magic.head = locomotion.head;
        magic.leftHand = locomotion.leftHand;
        magic.rightHand = locomotion.rightHand;
        magic.avatar = locomotion.avatar.transform;
        magic.status = status;
        magic.legacyTree = original;
        magic.desktopMode = locomotion.desktopControlled;
        magic.ancestors = new Transform[names.Length];
        magic.memorials = new Transform[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            GameObject actor = GameObject.Find(names[i]);
            if (actor == null) throw new Exception("Gallery actor missing: " + names[i]);
            magic.ancestors[i] = actor.transform;
            GameObject oldStone = GameObject.Find("Gallery memorial - " + names[i]);
            if (oldStone != null) UnityEngine.Object.DestroyImmediate(oldStone);
            Transform stone = BuildAncestorMemorial(actor.transform);
            stone.name = "Gallery memorial - " + names[i];
            magic.memorials[i] = stone;
        }
        original.magicExchange = magic;
        SkyQuestDesktopControls controls = locomotion.GetComponent<SkyQuestDesktopControls>();
        if (controls != null) controls.magicExchange = magic;
        EditorUtility.SetDirty(magic);
        EditorUtility.SetDirty(original);
        if (controls != null) EditorUtility.SetDirty(controls);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Could not save " + scenePath);
    }

    [MenuItem("SKY/Build and validate Quest + Keyboard scenes")]
    public static void BuildAndValidateScenes()
    {
        CreateQuestScene();
        CreateDesktopScene();
        SkyQuestEnvironmentBuilder.BuildBoth();
        InstallMagicTreesInBothScenes();
        SkyQuestEnvironmentBuilder.ValidateBoth();
        Debug.Log("SKY scenes rebuilt and validated.");
    }

    public static void CreateQuestScene()
    {
        ConfigureQuestProject();
        Scene scene = PrepareEnvironment(out Vector3 spawn, out AncestorExchangeTree oldTree);
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == "XR Origin (XR Rig)") root.SetActive(false);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefab);
        if (prefab == null) throw new Exception("XRI Starter Assets XR Origin prefab was not found.");
        GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rig.name = "Sky Quest 3S XR Origin";
        rig.transform.position = spawn;
        rig.GetComponent<XROrigin>().RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
        foreach (MonoBehaviour behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour == null) continue;
            string typeName = behaviour.GetType().Name;
            if (typeName.Contains("MoveProvider") || typeName.Contains("TurnProvider") ||
                typeName.Contains("TeleportationProvider") || typeName.Contains("JumpProvider") ||
                typeName.Contains("ClimbProvider") || typeName.Contains("GrabMoveProvider") ||
                typeName.Contains("GravityProvider") || behaviour.gameObject.name == "Gravity")
                behaviour.enabled = false;
        }
        Camera xrCamera = rig.GetComponentInChildren<Camera>(true);
        if (xrCamera == null) throw new Exception("XR Origin has no camera.");
        xrCamera.gameObject.SetActive(true);
        xrCamera.tag = "MainCamera";
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
            if (camera != xrCamera) UnityEngine.Object.DestroyImmediate(camera);
        Transform head = xrCamera.transform;
        Transform left = FindChild(rig.transform, "Left Controller");
        Transform right = FindChild(rig.transform, "Right Controller");
        if (left == null || right == null) throw new Exception("XR Origin has no controller transforms.");
        AttachGameplay(rig, head, left, right, oldTree.transform, false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, QuestScene);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(QuestScene, true),
            new EditorBuildSettingsScene(DesktopScene, false),
            new EditorBuildSettingsScene(SourceScene, false)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("Sky Quest 3S scene created: " + QuestScene);
    }

    private static Scene PrepareEnvironment(out Vector3 spawn, out AncestorExchangeTree oldTree)
    {
        Scene scene = EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);
        GameObject desktopPlayer = GameObject.Find("C1_Player");
        spawn = desktopPlayer != null ? desktopPlayer.transform.position : new Vector3(35f, 0f, 35f);
        spawn.y = 0f;
        if (desktopPlayer != null) desktopPlayer.SetActive(false);
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
            camera.gameObject.SetActive(false);
        oldTree = UnityEngine.Object.FindFirstObjectByType<AncestorExchangeTree>(FindObjectsInactive.Include);
        if (oldTree == null) throw new Exception("The source scene has no AncestorExchangeTree.");
        oldTree.enabled = false;
        ResizeAncestorForSanctuary(oldTree.transform, 1.6f);
        foreach (MaskInteraction oldMask in UnityEngine.Object.FindObjectsByType<MaskInteraction>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None)) oldMask.enabled = false;
        foreach (MaskThrowController oldThrow in UnityEngine.Object.FindObjectsByType<MaskThrowController>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None)) oldThrow.enabled = false;
        return scene;
    }

    // The imported ancestor is authored much larger than the surrounding Sky-style architecture.
    // Keep the feet at the original ground height while reducing the whole visible character.
    private static void ResizeAncestorForSanctuary(Transform root, float desiredHeight)
    {
        if (root == null) return;
        Bounds before = VisibleBounds(root);
        if (before.size.y > 0.01f)
            root.localScale *= desiredHeight / before.size.y;
        Bounds after = VisibleBounds(root);
        if (before.size.y > 0.01f && after.size.y > 0.01f)
            root.position += Vector3.up * (before.min.y - after.min.y);
    }

    private static Bounds VisibleBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        Bounds bounds = new Bounds(root.position, Vector3.zero);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled) continue;
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    private static void AttachGameplay(GameObject rig, Transform head, Transform left,
        Transform right, Transform ancestor, bool desktop)
    {
        GameObject avatar = BuildAvatar(rig.transform, desktop);
        SkyQuestStatus status = rig.AddComponent<SkyQuestStatus>();
        status.head = head;
        status.desktopMode = desktop;
        SkyQuestLocomotion locomotion = rig.AddComponent<SkyQuestLocomotion>();
        locomotion.head = head;
        locomotion.leftHand = left;
        locomotion.rightHand = right;
        locomotion.avatar = avatar;
        locomotion.desktopControlled = desktop;
        locomotion.thirdPersonDistance = desktop ? 4.5f : 2.8f;
        if (!desktop)
        {
            rig.AddComponent<CapeEnergySystem>();
            CapeFlightController flight = rig.AddComponent<CapeFlightController>();
            flight.actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
                "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");
            rig.AddComponent<CapeEnergyUI>();
            rig.AddComponent<CapeFlightFeedback>();
            rig.AddComponent<CapeRechargeReceiver>();
        }
        SkyQuestAncestor tree = rig.AddComponent<SkyQuestAncestor>();
        tree.head = head;
        tree.leftHand = left;
        tree.rightHand = right;
        tree.ancestor = ancestor;
        tree.memorialStone = BuildAncestorMemorial(ancestor);
        tree.avatar = avatar.transform;
        tree.locomotion = locomotion;
        tree.status = status;
        tree.desktopMode = desktop;
        tree.maskPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Project files/mask/deer mask ult sky cotl.obj");
        SkyQuestMagicExchange magic = rig.AddComponent<SkyQuestMagicExchange>();
        magic.head = head;
        magic.leftHand = left;
        magic.rightHand = right;
        magic.avatar = avatar.transform;
        magic.status = status;
        magic.legacyTree = tree;
        magic.desktopMode = desktop;
        magic.ancestors[0] = ancestor;
        tree.magicExchange = magic;
        SkyQuestSocial social = rig.AddComponent<SkyQuestSocial>();
        social.head = head;
        social.leftHand = left;
        social.rightHand = right;
        social.ancestor = ancestor;
        social.itemTree = tree;
        social.status = status;
        social.desktopMode = desktop;
        SkyQuestProps props = rig.AddComponent<SkyQuestProps>();
        props.head = head;
        props.rightHand = right;
        props.itemTree = tree;
        props.status = status;
        props.desktopMode = desktop;
        SkyQuestWorld world = rig.AddComponent<SkyQuestWorld>();
        world.locomotion = locomotion;
        world.status = status;
        world.head = head;
        world.ancestor = ancestor;
        SkyQuestCommunication communication = rig.AddComponent<SkyQuestCommunication>();
        communication.head = head;
        communication.rightHand = right;
        communication.itemTree = tree;
        communication.status = status;
        communication.desktopMode = desktop;
        props.communication = communication;
        SkyQuestImmersion immersion = rig.AddComponent<SkyQuestImmersion>();
        immersion.head = head;
        immersion.leftHand = left;
        immersion.rightHand = right;
        immersion.locomotion = locomotion;
        immersion.sanctuary = UnityEngine.Object.FindFirstObjectByType<SkyQuestSanctuary>();
        immersion.status = status;
        immersion.desktopMode = desktop;
        if (desktop)
        {
            SkyQuestDesktopControls controls = rig.AddComponent<SkyQuestDesktopControls>();
            controls.head = head;
            controls.locomotion = locomotion;
            controls.ancestor = tree;
            controls.magicExchange = magic;
            controls.social = social;
            controls.props = props;
            controls.communication = communication;
            controls.status = status;
        }
    }

    public static void CreateDesktopScene()
    {
        Scene scene = PrepareEnvironment(out Vector3 spawn, out AncestorExchangeTree oldTree);
        GameObject rig = new GameObject("Sky keyboard rig");
        rig.transform.position = spawn;
        GameObject headObject = new GameObject("Desktop head camera");
        headObject.transform.SetParent(rig.transform, false);
        headObject.transform.localPosition = new Vector3(0f, 1.65f, 0f);
        headObject.tag = "MainCamera";
        Camera camera = headObject.AddComponent<Camera>();
        camera.stereoTargetEye = StereoTargetEyeMask.None;
        headObject.AddComponent<AudioListener>();
        Transform left = new GameObject("Desktop left hand").transform;
        left.SetParent(headObject.transform, false);
        left.localPosition = new Vector3(-0.28f, -0.24f, 0.4f);
        Transform right = new GameObject("Desktop right hand").transform;
        right.SetParent(headObject.transform, false);
        right.localPosition = new Vector3(0.28f, -0.24f, 0.4f);
        AttachGameplay(rig, headObject.transform, left, right, oldTree.transform, true);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, DesktopScene);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(QuestScene, true),
            new EditorBuildSettingsScene(DesktopScene, false),
            new EditorBuildSettingsScene(SourceScene, false)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("Sky keyboard scene created: " + DesktopScene);
    }

    private static Transform FindChild(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child;
        return null;
    }

    private static GameObject BuildAvatar(Transform rig, bool desktop)
    {
        GameObject avatar = new GameObject("Sky avatar");
        avatar.transform.SetParent(rig, false);
        avatar.transform.localPosition = desktop ? Vector3.zero : new Vector3(0f, 0f, 2.8f);
        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Project files/COTL_rigged.fbx");
        if (modelAsset != null)
        {
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            model.transform.SetParent(avatar.transform, false);
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            foreach (Camera embeddedCamera in model.GetComponentsInChildren<Camera>(true))
                UnityEngine.Object.DestroyImmediate(embeddedCamera.gameObject);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
                if (bounds.size.y > 0.01f)
                {
                    float scale = 1.6f / bounds.size.y;
                    model.transform.localScale = Vector3.one * scale;
                    model.transform.position -= new Vector3(bounds.center.x - avatar.transform.position.x,
                        bounds.min.y - avatar.transform.position.y, bounds.center.z - avatar.transform.position.z) * scale;
                }
            }
            foreach (Collider collider in model.GetComponentsInChildren<Collider>(true)) UnityEngine.Object.DestroyImmediate(collider);
        }
        else
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Sky avatar fallback";
            body.transform.SetParent(avatar.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            body.transform.localScale = new Vector3(0.4f, 0.8f, 0.4f);
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());
        }
        return avatar;
    }

    public static void ValidateQuestScene()
    {
        Scene scene = EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Single);
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        SkyQuestAncestor tree = UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>();
        SkyQuestSocial social = UnityEngine.Object.FindFirstObjectByType<SkyQuestSocial>();
        SkyQuestCommunication communication = UnityEngine.Object.FindFirstObjectByType<SkyQuestCommunication>();
        SkyQuestProps props = UnityEngine.Object.FindFirstObjectByType<SkyQuestProps>();
        SkyQuestWorld world = UnityEngine.Object.FindFirstObjectByType<SkyQuestWorld>();
        SkyQuestMagicExchange magic = UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        if (!scene.IsValid() || locomotion == null || tree == null || tree.memorialStone == null || social == null ||
            communication == null || props == null || world == null || magic == null)
            throw new Exception("Quest scene is missing one or more core systems.");
        if (locomotion.head == null || locomotion.leftHand == null || locomotion.rightHand == null ||
            locomotion.avatar == null || tree.ancestor == null || tree.avatar == null ||
            communication.head == null || props.rightHand == null || props.communication == null || world.head == null ||
            magic.legacyTree != tree)
            throw new Exception("Quest scene has a missing device or gameplay reference.");
        ValidateGallery(magic);
        ValidateMemorial(tree);
        int activeCameras = 0;
        string activeCameraNames = "";
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
            if (camera.gameObject.activeInHierarchy && camera.enabled)
            {
                activeCameras++;
                activeCameraNames += camera.name + "[" + camera.transform.root.name + "; " +
                    AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(camera)) + "] ";
            }
        if (activeCameras != 1) throw new Exception("Quest scene should have exactly one active camera, found " +
            activeCameras + ": " + activeCameraNames);
        XRGeneralSettings general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
        if (general == null || general.Manager == null || !general.Manager.automaticLoading ||
            !general.Manager.automaticRunning)
            throw new Exception("OpenXR is not configured to start automatically on Android.");
        Debug.Log("Sky Quest 3S scene validation passed.");
    }

    public static void ValidateDesktopScene()
    {
        Scene scene = EditorSceneManager.OpenScene(DesktopScene, OpenSceneMode.Single);
        SkyQuestDesktopControls controls = UnityEngine.Object.FindFirstObjectByType<SkyQuestDesktopControls>();
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        SkyQuestAncestor ancestor = UnityEngine.Object.FindFirstObjectByType<SkyQuestAncestor>();
        SkyQuestSocial social = UnityEngine.Object.FindFirstObjectByType<SkyQuestSocial>();
        SkyQuestProps props = UnityEngine.Object.FindFirstObjectByType<SkyQuestProps>();
        SkyQuestCommunication communication = UnityEngine.Object.FindFirstObjectByType<SkyQuestCommunication>();
        SkyQuestMagicExchange magic = UnityEngine.Object.FindFirstObjectByType<SkyQuestMagicExchange>();
        if (!scene.IsValid() || controls == null || locomotion == null || ancestor == null ||
            ancestor.memorialStone == null ||
            social == null || props == null || communication == null || magic == null ||
            !locomotion.desktopControlled || !ancestor.desktopMode || !social.desktopMode ||
            !props.desktopMode || !communication.desktopMode || controls.head == null ||
            controls.locomotion != locomotion || controls.ancestor != ancestor ||
            controls.social != social || controls.props != props ||
            controls.communication != communication || controls.magicExchange != magic ||
            !magic.desktopMode || magic.legacyTree != ancestor)
            throw new Exception("Desktop scene has missing or incorrect keyboard controls.");
        ValidateGallery(magic);
        ValidateMemorial(ancestor);
        int cameras = 0;
        foreach (Camera camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
            if (camera.enabled && camera.gameObject.activeInHierarchy) cameras++;
        if (cameras != 1) throw new Exception("Desktop scene needs one active camera, found " + cameras);
        Debug.Log("Sky keyboard scene validation passed.");
    }

    private static void ValidateGallery(SkyQuestMagicExchange magic)
    {
        string[] names = { "Shibiyama dancer", "Sky prayer guardian", "COTL traveller",
            "Candle pilgrim", "Skeleton pilgrim - grounded gallery character",
            "Original C1 player - visible gallery character" };
        if (magic.ancestors == null || magic.memorials == null ||
            magic.ancestors.Length != names.Length || magic.memorials.Length != names.Length)
            throw new Exception("Gallery ancestor or memorial list is incomplete.");
        for (int i = 0; i < names.Length; i++)
        {
            if (magic.ancestors[i] == null || magic.ancestors[i].name != names[i] || magic.memorials[i] == null)
                throw new Exception("Gallery ancestor missing: " + names[i]);
            ValidateMemorial(magic.ancestors[i], magic.memorials[i]);
        }
    }

    public static void BuildQuestApk()
    {
        ConfigureQuestProject();
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            throw new Exception("Unity could not switch to Android build target.");
        string outputDirectory = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "SkyQuest3S-Builds");
        System.IO.Directory.CreateDirectory(outputDirectory);
        string output = System.IO.Path.Combine(outputDirectory, "SkyQuest3S.apk");
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { QuestScene },
            locationPathName = output,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Quest APK build failed: " + report.summary.result +
                " (" + report.summary.totalErrors + " errors)");
        Debug.Log("Quest APK built: " + output);
    }

    [MenuItem("SKY/Build Quest 3S APK")]
    public static void BuildQuestApkMenu()
    {
        BuildQuestApk();
    }

    [MenuItem("SKY/Validate gameplay references")]
    public static void ValidateGameplayReferences()
    {
        ValidateQuestScene();
        ValidateDesktopScene();
        SkyQuestEnvironmentBuilder.ValidateBoth();
        Debug.Log("SKY gameplay references and environment validation passed.");
    }

    [MenuItem("SKY/Build keyboard desktop demo")]
    public static void BuildDesktopDemo()
    {
        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
                BuildTarget.StandaloneOSX))
            throw new Exception("Unity could not switch to the desktop build target.");
        string output = "Build/SkyDesktopDemo.app";
        System.IO.Directory.CreateDirectory("Build");
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { DesktopScene },
            locationPathName = output,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("Desktop demo build failed: " + report.summary.result);
        Debug.Log("Desktop demo built: " + output);
    }

    [MenuItem("SKY/Configure Quest 3S project")]
    public static void ConfigureQuestProject()
    {
        const string settingsPath = "Assets/XRI/Settings/SkyQuestXRSettings.asset";
        XRGeneralSettingsPerBuildTarget buildSettings =
            AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(settingsPath);
        if (buildSettings == null)
        {
            buildSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(buildSettings, settingsPath);
        }
        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildSettings, true);
        if (!buildSettings.HasSettingsForBuildTarget(BuildTargetGroup.Android))
            buildSettings.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
        if (!buildSettings.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))
            buildSettings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        XRGeneralSettings general = buildSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
        general.InitManagerOnStart = true;
        general.Manager.automaticLoading = true;
        general.Manager.automaticRunning = true;
        if (!XRPackageMetadataStore.AssignLoader(general.Manager, typeof(OpenXRLoader).FullName,
                BuildTargetGroup.Android))
            throw new Exception("Could not assign the OpenXR Android loader.");
        OpenXRSettings openXR = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        if (openXR != null)
        {
            openXR.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;
            OculusTouchControllerProfile touch = openXR.GetFeature<OculusTouchControllerProfile>();
            if (touch != null) { touch.enabled = true; EditorUtility.SetDirty(touch); }
            MetaQuestTouchPlusControllerProfile touchPlus =
                openXR.GetFeature<MetaQuestTouchPlusControllerProfile>();
            if (touchPlus != null) { touchPlus.enabled = true; EditorUtility.SetDirty(touchPlus); }
            MetaQuestFeature meta = openXR.GetFeature<MetaQuestFeature>();
            if (meta != null) { meta.enabled = true; EditorUtility.SetDirty(meta); }
            OpenXRCompositionLayersFeature layers = openXR.GetFeature<OpenXRCompositionLayersFeature>();
            if (layers != null) { layers.enabled = true; EditorUtility.SetDirty(layers); }
            EditorUtility.SetDirty(openXR);
        }
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;
        PlayerSettings.Android.optimizedFramePacing = true;
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.jialingzhu.skyquestxr");
        PlayerSettings.productName = "Sky Quest XR";
        UniversalRenderPipelineAsset mobilePipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
            "Assets/Settings/Mobile_RPAsset.asset");
        if (mobilePipeline != null)
        {
            mobilePipeline.msaaSampleCount = 4;
            EditorUtility.SetDirty(mobilePipeline);
        }
        ConfigureExistingQuestRig();
        EditorUtility.SetDirty(buildSettings);
        EditorUtility.SetDirty(general);
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureExistingQuestRig()
    {
        Scene scene = SceneManager.GetSceneByPath(QuestScene);
        bool openedHere = !scene.isLoaded;
        if (openedHere) scene = EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Additive);
        try
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (XROrigin origin in root.GetComponentsInChildren<XROrigin>(true))
            {
                if (origin.gameObject.name != "Sky Quest 3S XR Origin" ||
                    origin.RequestedTrackingOriginMode == XROrigin.TrackingOriginMode.Floor) continue;
                origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
                EditorUtility.SetDirty(origin);
                EditorSceneManager.MarkSceneDirty(scene);
            }
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }
        finally
        {
            if (openedHere) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
