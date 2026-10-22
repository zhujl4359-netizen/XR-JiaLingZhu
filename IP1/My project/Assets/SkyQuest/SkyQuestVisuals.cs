using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class SkyQuestVisuals
{
    private static readonly Dictionary<int, Material> SharedMaterials = new Dictionary<int, Material>();
    private static Shader sharedShader;
    private static TMP_FontAsset chineseFont;

    public static void EnsureChineseFont()
    {
        if (chineseFont != null) return;
        Font source = Resources.Load<Font>("NotoSansCJKsc-Regular");
        if (source == null)
        {
            Debug.LogWarning("SkyQuest Chinese font was not imported.");
            return;
        }
        chineseFont = TMP_FontAsset.CreateFontAsset(source, 64, 8,
            GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        if (chineseFont != null && TMP_Settings.fallbackFontAssets != null &&
            !TMP_Settings.fallbackFontAssets.Contains(chineseFont))
            TMP_Settings.fallbackFontAssets.Add(chineseFont);
    }

    public static Material Material(Color color, bool glow = false)
    {
        Color32 compact = color;
        int key = compact.r | (compact.g << 8) | (compact.b << 16) | (compact.a << 24);
        key = (key * 397) ^ (glow ? 1 : 0);
        if (SharedMaterials.TryGetValue(key, out Material cached) && cached != null) return cached;

        if (sharedShader == null) sharedShader = Shader.Find("Universal Render Pipeline/Lit");
        if (sharedShader == null) sharedShader = Shader.Find("Standard");
        Material material = new Material(sharedShader);
        material.color = color;
        if (glow)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.5f);
        }
        SharedMaterials[key] = material;
        return material;
    }

    public static GameObject Primitive(string name, PrimitiveType type, Transform parent,
        Vector3 localPosition, Vector3 localScale, Color color, bool glow = false)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localScale = localScale;
        obj.GetComponent<Renderer>().sharedMaterial = Material(color, glow);
        return obj;
    }

    public static TextMeshPro Text(string name, string value, Transform parent, Vector3 localPosition,
        float fontSize, Color color, float width = 3f)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        // World-space labels use a compact scale so guidance stays readable in VR
        // and does not cover the entire view.
        obj.transform.localScale = Vector3.one * 0.12f;
        TextMeshPro text = obj.AddComponent<TextMeshPro>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(width, 1.3f);
        return text;
    }

    public static void RemoveCollider(GameObject obj)
    {
        Collider collider = obj.GetComponent<Collider>();
        if (collider == null) return;
        // Destroy is deferred until frame end; disable now so newly created
        // decoration cannot lift the player or interrupt the camera cast.
        collider.enabled = false;
        Object.Destroy(collider);
    }
}
