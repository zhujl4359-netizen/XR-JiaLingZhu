using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class SkyQuestFlameFeedbackAsset
{
    [MenuItem("SKY/Prepare flight flame collection material")]
    public static void Prepare()
    {
        const string folder = "Assets/SkyQuest/Resources";
        const string path = folder + "/FlightFlameCollection.mat";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/SkyQuest", "Resources");
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) throw new InvalidOperationException("URP particle shader is unavailable.");
        Material material = new Material(shader) { name = "Flight flame collection" };
        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)RenderQueue.Transparent;
        AssetDatabase.CreateAsset(material, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Flight flame collection material ready: " + path);
    }
}
