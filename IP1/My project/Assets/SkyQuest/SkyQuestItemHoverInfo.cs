using UnityEngine;
using UnityEngine.InputSystem;

// Describes physical exchange rewards after the pointer rests on one for three seconds.
public sealed class SkyQuestItemHoverInfo : MonoBehaviour
{
    public Transform head;
    public Transform rightHand;
    public bool desktopMode;

    private SkyQuestMagicPickup current;
    private float hoverStarted;
    private float lastControllerAimTime = -10f;
    private Vector2 screenPoint;
    private GUIStyle tooltipStyle;
    private SkyQuestLocomotion locomotion;

    private void Awake() { locomotion = GetComponent<SkyQuestLocomotion>(); }

    private void Update()
    {
        SkyQuestMagicExchange exchange = GetComponent<SkyQuestMagicExchange>();
        SkyQuestAncestor ancestor = GetComponent<SkyQuestAncestor>();
        if ((exchange != null && exchange.IsMenuOpen) ||
            (ancestor != null && ancestor.IsMenuOpen)) { SetTarget(null); return; }
        if (head == null) { SetTarget(null); return; }

        Camera camera = head.GetComponent<Camera>();
        Ray ray;
        if (desktopMode && camera != null)
        {
            if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().sqrMagnitude > .01f)
                lastControllerAimTime = Time.unscaledTime;
            bool useControllerCursor = Gamepad.current != null &&
                Time.unscaledTime - lastControllerAimTime <= 4f;
            screenPoint = useControllerCursor || Mouse.current == null ?
                new Vector2(Screen.width * .5f, Screen.height * .5f) :
                Mouse.current.position.ReadValue();
            ray = camera.ScreenPointToRay(screenPoint);
        }
        else if (rightHand != null)
        {
            ray = new Ray(rightHand.position, rightHand.forward);
            screenPoint = new Vector2(Screen.width * .5f, Screen.height * .5f);
        }
        else if (camera != null)
        {
            screenPoint = new Vector2(Screen.width * .5f, Screen.height * .5f);
            ray = camera.ViewportPointToRay(new Vector3(.5f, .5f));
        }
        else { SetTarget(null); return; }

        SkyQuestMagicPickup nearest = null;
        float best = 12f;
        foreach (RaycastHit hit in Physics.RaycastAll(ray, 12f, ~0, QueryTriggerInteraction.Ignore))
        {
            SkyQuestMagicPickup item = hit.collider.GetComponentInParent<SkyQuestMagicPickup>();
            if (item == null || item.held || hit.distance >= best) continue;
            best = hit.distance;
            nearest = item;
        }
        SetTarget(nearest);
    }

    private void SetTarget(SkyQuestMagicPickup target)
    {
        if (target == current) return;
        current = target;
        hoverStarted = Time.unscaledTime;
    }

    private void OnGUI()
    {
        // Quest already shows the same hint as a world-space panel above the
        // selected item; a second screen-space label would obscure the view.
        if (!desktopMode) return;
        if (current == null) return;
        if (tooltipStyle == null)
        {
            tooltipStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter, fontSize = 16,
                fontStyle = FontStyle.Bold, wordWrap = true
            };
            tooltipStyle.normal.textColor = Color.black;
        }
        string message = current.displayPottery ?
            (desktopMode ? "Z: PICK UP / DROP  ·  HOLD Q, RELEASE: THROW" :
                "HOLD RIGHT TRIGGER: AIM  ·  RELEASE: THROW") :
            desktopMode ?
                (current.restoresCapeEnergy ? "E: EAT +1 FLY ENERGY" :
                    "E: EAT +5 CANDLES") +
                "  ·  Z: PICK UP / DROP  ·  HOLD Q, RELEASE: " +
                (current.placeable ? "PLACE" : "THROW") :
                (current.restoresCapeEnergy ? "+1 FLY ENERGY  ·  " : "+5 CANDLES  ·  ") +
                "HOLD RIGHT TRIGGER: AIM  ·  RELEASE: THROW  ·  NEAR FACE + RIGHT TRIGGER: EAT";
        float width = desktopMode ? 384f : 480f;
        float height = desktopMode ? 48f : 76f;
        float x = Mathf.Clamp(screenPoint.x - width * .5f, 12f, Screen.width - width - 12f);
        float y = Mathf.Clamp(Screen.height - screenPoint.y + 28f, 12f, Screen.height - height - 12f);
        Rect panel = new Rect(x, y, width, height);
        Color previous = GUI.color;
        GUI.color = new Color(1f, .9f, .12f, .95f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = previous;
        GUI.Label(panel, message, tooltipStyle);
    }
}
