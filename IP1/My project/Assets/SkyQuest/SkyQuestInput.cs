using UnityEngine;
using UnityEngine.XR;

// Input boundary for Quest Touch controllers only.
public static class SkyQuestInput
{
    public static InputDevice Left => InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    public static InputDevice Right => InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

    public static float Trigger(InputDevice device)
    {
        return device.TryGetFeatureValue(CommonUsages.trigger, out float value) ? value : 0f;
    }

    public static float Grip(InputDevice device)
    {
        return device.TryGetFeatureValue(CommonUsages.grip, out float value) ? value : 0f;
    }

    public static bool Primary(InputDevice device)
    {
        return device.TryGetFeatureValue(CommonUsages.primaryButton, out bool value) && value;
    }

    public static bool Secondary(InputDevice device)
    {
        return device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool value) && value;
    }

    public static Vector2 Stick(InputDevice device)
    {
        return device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 value) ? value : Vector2.zero;
    }

    public static void Pulse(InputDevice device, float amplitude = 0.35f, float duration = 0.08f)
    {
        if (device.isValid && device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
            device.SendHapticImpulse(0, Mathf.Clamp01(amplitude), duration);
    }
}

public static class SkyQuestInputExtensions
{
    public static bool StickClick(UnityEngine.XR.InputDevice device)
    {
        return device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool value) && value;
    }
}
