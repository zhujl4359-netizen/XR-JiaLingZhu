using TMPro;
using UnityEngine;

public class SkyQuestProps : MonoBehaviour
{
    public Transform head;
    public Transform rightHand;
    public SkyQuestAncestor itemTree;
    public SkyQuestCommunication communication;
    public SkyQuestStatus status;
    public bool desktopMode;
    public LayerMask placementLayers = ~0;

    private GameObject preview;
    private bool wasSecondary;
    private bool wasTrigger;
    private bool validPlacement;
    private Vector3 placementPoint;
    private TextMeshPro hint;
    private SkyQuestLocomotion locomotion;

    void Start()
    {
        locomotion = FindFirstObjectByType<SkyQuestLocomotion>();
        if (head != null)
        {
            hint = SkyQuestVisuals.Text("Prop instructions", "", head,
                new Vector3(0f, -0.36f, 1.1f), 2f, Color.white, 1.5f);
        }
    }

    void Update()
    {
        if (hint != null && !desktopMode && locomotion != null)
            hint.gameObject.SetActive(!locomotion.ThirdPerson);
        if (desktopMode)
        {
            UpdateDesktopPreview();
            return;
        }
        if (rightHand == null) return;
        if (communication != null && communication.IsOpen)
        {
            if (preview != null) { Destroy(preview); preview = null; }
            if (hint != null) hint.text = "";
            wasTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
            wasSecondary = SkyQuestInput.Secondary(SkyQuestInput.Left);
            return;
        }
        bool button = SkyQuestInput.Secondary(SkyQuestInput.Left);
        if (button && !wasSecondary && (itemTree == null || !itemTree.IsMenuOpen)) TogglePreview();
        wasSecondary = button;
        if (preview == null)
        {
            if (hint != null) hint.text = "Y: place tea table";
            wasTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
            return;
        }
        if (hint != null) hint.text = "Aim at ground. Right trigger: place; Y: cancel";
        Ray ray = new Ray(rightHand.position, rightHand.forward);
        validPlacement = Physics.Raycast(ray, out RaycastHit hit, 7f, placementLayers,
            QueryTriggerInteraction.Ignore) && Vector3.Dot(hit.normal, Vector3.up) > 0.65f;
        if (validPlacement) placementPoint = hit.point;
        else placementPoint = rightHand.position + rightHand.forward * 2f;
        preview.transform.position = placementPoint;
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        if (trigger && !wasTrigger && validPlacement)
        {
            preview.name = "Placed tea table";
            SetOpacity(preview, 1f);
            preview.AddComponent<SkyQuestTeaTable>();
            preview = null;
            status?.Show("Tea table placed. Point and click it to chat.");
            SkyQuestInput.Pulse(SkyQuestInput.Right);
        }
        wasTrigger = trigger;
    }

    private void TogglePreview()
    {
        if (preview != null) { Destroy(preview); preview = null; return; }
        preview = CreateTable();
        SetOpacity(preview, 0.5f);
        status?.Show(desktopMode ? "Aim toward the ground and press R to place a tea table." :
            "Aim at the ground and press the right trigger to place a tea table.", 5f);
    }

    private void UpdateDesktopPreview()
    {
        if (head == null) return;
        if (hint != null) hint.text = preview == null ? "P: preview tea table" :
            "Arrow Up/Down: aim    R: place    P: cancel";
        if (preview == null) return;
        Ray ray = new Ray(head.position, head.forward);
        validPlacement = Physics.Raycast(ray, out RaycastHit hit, 8f, placementLayers,
            QueryTriggerInteraction.Ignore) && Vector3.Dot(hit.normal, Vector3.up) > 0.65f;
        if (!validPlacement)
        {
            Vector3 ahead = head.position + Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized * 2f;
            validPlacement = Physics.Raycast(ahead + Vector3.up * 2f, Vector3.down, out hit,
                6f, placementLayers, QueryTriggerInteraction.Ignore) &&
                Vector3.Dot(hit.normal, Vector3.up) > 0.65f;
        }
        preview.transform.position = validPlacement ? hit.point : head.position + head.forward * 2f;
    }

    public void DesktopTogglePreview()
    {
        if (desktopMode) TogglePreview();
    }

    public void DesktopPlace()
    {
        if (!desktopMode || preview == null || !validPlacement) return;
        preview.name = "Placed tea table";
        SetOpacity(preview, 1f);
        preview.AddComponent<SkyQuestTeaTable>();
        preview = null;
        status?.Show("Tea table placed. Click it to chat.");
    }

    private GameObject CreateTable()
    {
        GameObject table = new GameObject("Tea table preview");
        SkyQuestVisuals.Primitive("Table top", PrimitiveType.Cylinder, table.transform,
            new Vector3(0f, 0.62f, 0f), new Vector3(0.55f, 0.05f, 0.55f), new Color(0.5f, 0.34f, 0.2f));
        for (int i = 0; i < 4; i++)
        {
            float x = (i % 2 == 0 ? -1f : 1f) * 0.38f;
            float z = (i < 2 ? -1f : 1f) * 0.38f;
            SkyQuestVisuals.Primitive("Table leg", PrimitiveType.Cylinder, table.transform,
                new Vector3(x, 0.3f, z), new Vector3(0.05f, 0.3f, 0.05f), new Color(0.45f, 0.28f, 0.15f));
        }
        SkyQuestVisuals.Primitive("Teapot", PrimitiveType.Sphere, table.transform,
            new Vector3(0f, 0.78f, 0f), new Vector3(0.18f, 0.12f, 0.18f), new Color(0.92f, 0.8f, 0.55f));
        return table;
    }

    private void SetOpacity(GameObject item, float opacity)
    {
        foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
        {
            Material material = renderer.material;
            if (material.HasProperty("_Surface"))
            {
                bool transparent = opacity < 1f;
                material.SetFloat("_Surface", transparent ? 1f : 0f);
                material.SetFloat("_SrcBlend", (float)(transparent ? UnityEngine.Rendering.BlendMode.SrcAlpha : UnityEngine.Rendering.BlendMode.One));
                material.SetFloat("_DstBlend", (float)(transparent ? UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha : UnityEngine.Rendering.BlendMode.Zero));
                material.SetFloat("_ZWrite", transparent ? 0f : 1f);
                if (transparent) material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                else material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = transparent ? 3000 : -1;
            }
            Color color = material.color;
            color.a = opacity;
            material.color = color;
            Collider collider = renderer.GetComponent<Collider>();
            if (collider != null) collider.enabled = opacity >= 1f;
        }
    }
}

// A marker on placed tea tables lets either mouse clicks or Quest rays open chat.
public sealed class SkyQuestTeaTable : MonoBehaviour
{
    public float readyAt { get; private set; }

    private void Awake()
    {
        // The trigger that places a table must not also click it in that frame.
        readyAt = Time.unscaledTime + .2f;
    }
}
