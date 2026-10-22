using UnityEngine;
using UnityEngine.InputSystem;

// Keyboard-only controls for the separate desktop scene. Quest controls remain untouched.
public class SkyQuestDesktopControls : MonoBehaviour
{
    public Transform head;
    public SkyQuestLocomotion locomotion;
    public SkyQuestAncestor ancestor;
    public SkyQuestMagicExchange magicExchange;
    public SkyQuestSocial social;
    public SkyQuestProps props;
    public SkyQuestCommunication communication;
    public SkyQuestStatus status;
    public float mouseSensitivity = 0.12f;

    private float yaw;
    private float pitch;
    private bool exchangeWasOpen;
    private Vector2 previousMousePosition;
    private bool hasMousePosition;

    void Start()
    {
        if (head != null)
        {
            yaw = head.localEulerAngles.y;
            pitch = Mathf.DeltaAngle(0f, head.localEulerAngles.x);
        }
        status?.Show("WASD: move   Mouse: look   F: ancestor tree   Z: pick up/drop   Hold Q: aim/launch", 10f);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || locomotion == null || head == null) return;

        if (communication != null && communication.IsOpen)
        {
            Mouse chatMouse = Mouse.current;
            if (chatMouse != null)
            {
                Vector2 look = ReadMouseMovement(chatMouse);
                yaw += look.x * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - look.y * mouseSensitivity, -60f, 60f);
                head.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            }
            Vector2 chatMove = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
            Gamepad chatGamepad = Gamepad.current;
            if (chatGamepad != null && chatGamepad.leftStick.ReadValue().sqrMagnitude > chatMove.sqrMagnitude)
                chatMove = chatGamepad.leftStick.ReadValue();
            locomotion.DesktopMove(chatMove, keyboard.spaceKey.isPressed, keyboard.leftCtrlKey.isPressed,
                keyboard.spaceKey.wasPressedThisFrame);
            if (keyboard.escapeKey.wasPressedThisFrame) communication.DesktopToggle();
            if (keyboard.backspaceKey.wasPressedThisFrame) communication.DesktopDelete();
            if (keyboard.enterKey.wasPressedThisFrame) communication.DesktopSend();
            return;
        }

        bool exchangeOpen = (ancestor != null && ancestor.IsMenuOpen) ||
            (magicExchange != null && magicExchange.IsMenuOpen);
        // Ignore one extra mouse sample after the exchange panel closes. The
        // pointer can move a long distance during a drag, and that accumulated
        // delta must not be applied to the camera on the closing frame.
        bool suppressMouseLook = exchangeOpen || exchangeWasOpen;
        exchangeWasOpen = exchangeOpen;
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            Vector2 delta = ReadMouseMovement(mouse);
            if (!suppressMouseLook)
            {
                yaw += delta.x * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, -60f, 60f);
            }
        }

        if (!exchangeOpen)
        {
            if (keyboard.leftArrowKey.wasPressedThisFrame) yaw -= locomotion.snapAngle;
            if (keyboard.rightArrowKey.wasPressedThisFrame) yaw += locomotion.snapAngle;
            if (keyboard.upArrowKey.wasPressedThisFrame) pitch = Mathf.Clamp(pitch - 10f, -30f, 30f);
            if (keyboard.downArrowKey.wasPressedThisFrame) pitch = Mathf.Clamp(pitch + 10f, -30f, 30f);
        }
        head.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector2 move = new Vector2(
            (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
            (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
        locomotion.DesktopMove(move, keyboard.spaceKey.isPressed, keyboard.leftCtrlKey.isPressed,
            keyboard.spaceKey.wasPressedThisFrame);

        if (!exchangeOpen && keyboard.digit1Key.wasPressedThisFrame) locomotion.SetPerspective(false);
        if (!exchangeOpen && keyboard.digit3Key.wasPressedThisFrame) locomotion.SetPerspective(true);
        if (keyboard.fKey.wasPressedThisFrame)
        {
            bool legacy = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            if (legacy || magicExchange == null || !magicExchange.IsNearAny)
                ancestor?.DesktopToggleMenu();
            else magicExchange.DesktopToggleNearest();
        }
        if (magicExchange != null && magicExchange.IsMenuOpen)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) magicExchange.DesktopSelect(0);
            if (keyboard.digit2Key.wasPressedThisFrame) magicExchange.DesktopSelect(1);
            if (keyboard.digit3Key.wasPressedThisFrame) magicExchange.DesktopSelect(2);
            if (keyboard.digit4Key.wasPressedThisFrame) magicExchange.DesktopSelect(3);
            if (keyboard.digit5Key.wasPressedThisFrame) magicExchange.DesktopSelect(4);
            if (keyboard.enterKey.wasPressedThisFrame) magicExchange.DesktopObtainSelected();
        }
        else if (ancestor != null && ancestor.IsMenuOpen)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) ancestor.DesktopSelect(0);
            if (keyboard.digit2Key.wasPressedThisFrame) ancestor.DesktopSelect(1);
            if (keyboard.digit3Key.wasPressedThisFrame) ancestor.DesktopSelect(2);
            if (keyboard.digit4Key.wasPressedThisFrame) ancestor.DesktopSelect(3);
            if (keyboard.digit5Key.wasPressedThisFrame) ancestor.DesktopSelect(4);
            if (keyboard.enterKey.wasPressedThisFrame) ancestor.DesktopObtainSelected();
        }
        if (keyboard.eKey.wasPressedThisFrame)
        {
            Transform eater = locomotion.avatar != null ? locomotion.avatar.transform : head;
            if (!SkyQuestMagicPickup.TryConsumeWithE(eater)) ancestor?.EquipHeld();
        }
        if (keyboard.hKey.wasPressedThisFrame) social?.DesktopShareHeart();
        if (keyboard.cKey.wasPressedThisFrame) social?.DesktopOfferCandle();
        if (keyboard.gKey.wasPressedThisFrame) social?.DesktopGreet();
        if (keyboard.digit7Key.wasPressedThisFrame) social?.DesktopReadStory(0);
        if (keyboard.digit8Key.wasPressedThisFrame) social?.DesktopReadStory(1);
        if (keyboard.digit9Key.wasPressedThisFrame) social?.DesktopReadStory(2);
        if (keyboard.pKey.wasPressedThisFrame) props?.DesktopTogglePreview();
        if (keyboard.rKey.wasPressedThisFrame) props?.DesktopPlace();
        if (keyboard.tKey.wasPressedThisFrame) communication?.DesktopToggle();
        if (keyboard.kKey.wasPressedThisFrame) communication?.DesktopToggle();
    }

    private Vector2 ReadMouseMovement(Mouse mouse)
    {
        Vector2 position = mouse.position.ReadValue();
        Vector2 movement = mouse.delta.ReadValue();
        if (movement.sqrMagnitude < 0.01f && hasMousePosition)
        {
            // Some Editor input backends report position but no delta while
            // the pointer is free over the Game view.
            Vector2 fallback = position - previousMousePosition;
            if (fallback.sqrMagnitude < 80f * 80f) movement = fallback;
        }
        previousMousePosition = position;
        hasMousePosition = true;
        return movement;
    }
}
