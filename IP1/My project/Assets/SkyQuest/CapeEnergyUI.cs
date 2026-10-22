using TMPro;
using UnityEngine;

/// <summary>Fifteen cape energy segments fixed to the left controller.</summary>
public sealed class CapeEnergyUI : MonoBehaviour
{
    public bool alwaysVisible = true;
    public Vector3 wristOffset = new Vector3(0f, 0.10f, 0.14f);
    public Vector3 wristRotation = new Vector3(55f, 0f, 0f);
    private CapeEnergySystem energy;
    private Transform panel;
    private Transform[] fills;
    private Renderer[] fillRenderers;
    private TextMeshPro label;

    public void Initialize(CapeEnergySystem owner, Transform leftHand)
    {
        energy = owner;
        if (energy == null || leftHand == null) return;
        panel = new GameObject("Cape energy wrist display").transform;
        panel.SetParent(leftHand, false);
        panel.localPosition = wristOffset;
        panel.localRotation = Quaternion.Euler(wristRotation);
        GameObject backing = MakePart("Dark backing", panel, new Vector3(0f, 0f, 0f),
            new Vector3(0.25f, 0.085f, 0.006f), new Color(0.035f, 0.07f, 0.10f));
        fills = new Transform[15];
        fillRenderers = new Renderer[15];
        for (int i = 0; i < fills.Length; i++)
        {
            float x = -0.102f + i * 0.0145f;
            MakePart("Empty segment " + (i + 1), panel, new Vector3(x, 0.009f, -0.005f),
                new Vector3(0.012f, 0.037f, 0.003f), new Color(0.18f, 0.23f, 0.27f));
            GameObject fill = MakePart("Energy segment " + (i + 1), panel,
                new Vector3(x, 0.009f, -0.008f), new Vector3(0.011f, 0.034f, 0.004f),
                new Color(1f, 0.82f, 0.32f));
            fills[i] = fill.transform;
            fillRenderers[i] = fill.GetComponent<Renderer>();
        }
        label = SkyQuestVisuals.Text("Cape energy label", "CAPE 15 / 15", panel,
            new Vector3(0f, -0.027f, -0.009f), 0.7f, Color.white, 0.22f);
        label.alignment = TextAlignmentOptions.Center;
        energy.OnEnergyChanged += Refresh;
        Refresh(energy.CurrentEnergy, energy.maxEnergy);
    }

    private static GameObject MakePart(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        part.GetComponent<Renderer>().sharedMaterial = SkyQuestVisuals.Material(color);
        return part;
    }

    private void Refresh(float current, float maximum)
    {
        if (fills == null) return;
        float segmentCapacity = maximum / fills.Length;
        for (int i = 0; i < fills.Length; i++)
        {
            float fraction = segmentCapacity > 0f ? Mathf.Clamp01((current - i * segmentCapacity) / segmentCapacity) : 0f;
            fills[i].gameObject.SetActive(fraction > 0.001f);
            fills[i].localScale = new Vector3(0.011f * fraction, 0.034f, 0.004f);
            fills[i].localPosition = new Vector3(-0.102f + i * 0.0145f - 0.0055f * (1f - fraction), 0.009f, -0.008f);
            fillRenderers[i].sharedMaterial = SkyQuestVisuals.Material(current <= maximum * 0.2f ?
                new Color(1f, 0.35f, 0.22f) : new Color(1f, 0.82f, 0.32f));
        }
        if (label != null) label.text = "CAPE " + current.ToString("0.#") + " / " + maximum.ToString("0.#");
    }

    private void OnDestroy()
    {
        if (energy != null) energy.OnEnergyChanged -= Refresh;
        if (panel != null) Destroy(panel.gameObject);
    }
}
