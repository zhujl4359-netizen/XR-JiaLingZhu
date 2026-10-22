using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public static class CapeFlightSceneInstaller
{
    private const string QuestScene = "Assets/SkyQuest/SkyQuest3S.unity";
    private const string ActionsPath = "Assets/SkyQuest/Resources/CapeFlightActions.inputactions";

    [MenuItem("SKY/Install cape flight in Quest scene")]
    public static void Install()
    {
        Scene scene = EditorSceneManager.OpenScene(QuestScene, OpenSceneMode.Single);
        SkyQuestLocomotion locomotion = UnityEngine.Object.FindFirstObjectByType<SkyQuestLocomotion>();
        if (locomotion == null || locomotion.desktopControlled)
            throw new Exception("Quest scene VR locomotion rig was not found.");
        GameObject rig = locomotion.gameObject;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root == rig || root.name != "XR Origin (XR Rig)" || !root.activeSelf) continue;
            Undo.RecordObject(root, "Disable duplicate XR Origin");
            root.SetActive(false);
            EditorUtility.SetDirty(root);
        }
        GetOrAdd<CapeEnergySystem>(rig);
        CapeFlightController flight = rig.GetComponent<CapeFlightController>();
        if (flight == null)
        {
            flight = Undo.AddComponent<CapeFlightController>(rig);
            flight.groundSpeed = locomotion.groundSpeed;
        }
        flight.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ActionsPath);
        if (flight.actions == null) throw new Exception("Cape flight input asset was not imported.");
        GetOrAdd<CapeEnergyUI>(rig);
        GetOrAdd<CapeFlightFeedback>(rig);
        GetOrAdd<CapeRechargeReceiver>(rig);
        EditorUtility.SetDirty(flight);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new Exception("Quest scene could not be saved.");
        Debug.Log("Cape flight components installed on the existing Quest VR rig.");
    }

    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(gameObject);
    }
}
