using TMPro;
using UnityEngine;

// A screen-space counterpart to the VR wrist meter. Both read the same energy source.
public sealed class CapeEnergyFanHUD : MonoBehaviour
{
    public CapeEnergySystem energy;
    public Transform head;
    public bool desktopMode;
    private Transform vrFan;
    private Renderer[] vrSegments;
    private TextMeshPro vrCount;
    private GUIStyle jewelStyle;
    private GUIStyle countStyle;

    public void Initialize()
    {
        if (desktopMode || energy == null || head == null || vrFan != null) return;
        vrFan = new GameObject("Cape energy fan HUD").transform;
        vrFan.SetParent(head, false);
        vrFan.localPosition = new Vector3(0f, .30f, 1.05f);
        vrFan.localRotation = Quaternion.identity;
        vrSegments = new Renderer[15];
        for (int i = 0; i < vrSegments.Length; i++)
        {
            float angle = Mathf.Lerp(-68f, 68f, i / 14f);
            float radians = angle * Mathf.Deg2Rad;
            GameObject bar = SkyQuestVisuals.Primitive("Energy ray " + (i + 1),
                PrimitiveType.Cube, vrFan,
                new Vector3(Mathf.Sin(radians) * .092f, -Mathf.Cos(radians) * .092f, 0f),
                new Vector3(.009f, .052f, .004f), new Color(1f, .84f, .34f), true);
            bar.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            SkyQuestVisuals.RemoveCollider(bar);
            vrSegments[i] = bar.GetComponent<Renderer>();
        }
        GameObject jewel = SkyQuestVisuals.Primitive("Energy jewel", PrimitiveType.Cube,
            vrFan, new Vector3(0f, .004f, 0f), new Vector3(.025f, .025f, .005f),
            new Color(1f, .96f, .75f), true);
        jewel.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        SkyQuestVisuals.RemoveCollider(jewel);
        vrCount = SkyQuestVisuals.Text("Energy remaining", "15 / 15", vrFan,
            new Vector3(0f, -.159f, -.006f), 1.8f, new Color(1f, .94f, .75f), .65f);
        energy.OnEnergyChanged += RefreshVr;
        RefreshVr(energy.CurrentEnergy, energy.maxEnergy);
    }

    public void SetVisible(bool visible)
    {
        if (vrFan != null && vrFan.gameObject.activeSelf != visible)
            vrFan.gameObject.SetActive(visible);
    }

    private void RefreshVr(float current, float maximum)
    {
        if (vrSegments == null) return;
        int lit = Mathf.Clamp(Mathf.CeilToInt(current / Mathf.Max(1f, maximum) * 15f - .001f), 0, 15);
        for (int i = 0; i < vrSegments.Length; i++)
            vrSegments[i].sharedMaterial = SkyQuestVisuals.Material(i < lit ?
                new Color(1f, .84f, .34f) : new Color(.22f, .27f, .30f), i < lit);
        if (vrCount != null) vrCount.text = Mathf.CeilToInt(current) + " / " + Mathf.RoundToInt(maximum);
    }

    private void OnDestroy()
    {
        if (energy != null) energy.OnEnergyChanged -= RefreshVr;
        if (vrFan != null) Destroy(vrFan.gameObject);
    }

    private void OnGUI()
    {
        if (energy == null || !desktopMode) return;
        if (jewelStyle == null)
        {
            jewelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fontSize = 23,
                fontStyle = FontStyle.Bold
            };
            jewelStyle.normal.textColor = new Color(1f, .98f, .85f);
            countStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            countStyle.normal.textColor = new Color(1f, .95f, .78f);
        }

        int filled = Mathf.Clamp(Mathf.CeilToInt(energy.CurrentEnergy - .001f), 0, 15);
        float cx = Screen.width * .5f;
        float top = 32f;
        Vector2 pivot = new Vector2(cx, top + 12f);
        Matrix4x4 oldMatrix = GUI.matrix;
        Color oldColor = GUI.color;
        for (int i = 0; i < 15; i++)
        {
            GUI.matrix = oldMatrix;
            GUIUtility.RotateAroundPivot(Mathf.Lerp(-78f, 78f, i / 14f), pivot);
            GUI.color = i < filled ? new Color(1f, .88f, .32f, .98f) :
                new Color(.18f, .23f, .28f, .7f);
            // Start the bars away from the jewel so adjacent energy marks remain
            // visibly separate, including at the narrow inner edge of the fan.
            GUI.DrawTexture(new Rect(cx - 3f, top + 55f, 6f, 34f), Texture2D.whiteTexture);
        }
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
        GUI.Label(new Rect(cx - 14f, top - 4f, 28f, 28f), "◆", jewelStyle);
        GUI.Label(new Rect(cx - 36f, top + 103f, 72f, 21f),
            Mathf.RoundToInt(energy.CurrentEnergy) + "/15", countStyle);
    }
}
