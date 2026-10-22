using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CapeFlightState { Grounded, Takeoff, Ascending, Gliding, Falling, Landing }

/// <summary>Calculates one rig displacement per frame; SkyQuestLocomotion remains the only mover.</summary>
public sealed class CapeFlightController : MonoBehaviour
{
    [Header("Controller input")]
    public InputActionAsset actions;
    public bool headRelativeSteering;
    public bool comfortMode;
    [Header("Flight tuning")]
    public float takeoffVelocityChange = 3f;
    public float airBoostVelocityChange = 2.2f;
    public float forwardBoostVelocityChange = 1.2f;
    public float maxVerticalSpeed = 5f;
    public float maxHorizontalSpeed = 6f;
    public float glideGravityMultiplier = 0.35f;
    public float flightCooldown = 0.6f;
    public float horizontalDrag = 0.3f;
    public float groundSpeed = 2.7f;
    public float groundAcceleration = 8f;
    public float airAcceleration = 3f;

    public CapeFlightState State { get; private set; } = CapeFlightState.Grounded;
    public bool IsFlying => State != CapeFlightState.Grounded && State != CapeFlightState.Landing;
    public Vector3 Velocity => horizontalVelocity + Vector3.up * verticalVelocity;
    public event Action<CapeFlightState> OnStateChanged;
    public event Action OnBoostSucceeded;
    public event Action OnBoostRejected;
    public event Action OnSafeStop;

    private CapeEnergySystem energy;
    private InputAction boostAction;
    private InputAction moveAction;
    private InputAction turnAction;
    private InputAction safeStopAction;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float nextBoostTime;
    private float takeoffTime;
    private bool glideEnabled = true;
    private float lastGroundContact;

    public void Initialize(CapeEnergySystem owner)
    {
        energy = owner;
        lastGroundContact = Time.time;
        if (actions == null) actions = Resources.Load<InputActionAsset>("CapeFlightActions");
        if (actions == null)
        {
            Debug.LogError("CapeFlightActions input asset is missing from Resources.", this);
            enabled = false;
            return;
        }
        InputActionMap map = actions.FindActionMap("Cape Flight", true);
        boostAction = map.FindAction("Boost", true);
        moveAction = map.FindAction("Move", true);
        turnAction = map.FindAction("Snap Turn", true);
        safeStopAction = map.FindAction("Safe Stop", true);
        map.Enable();
    }

    private void OnEnable()
    {
        if (actions != null && energy != null) actions.FindActionMap("Cape Flight")?.Enable();
    }

    private void OnDisable()
    {
        if (actions != null) actions.FindActionMap("Cape Flight")?.Disable();
    }

    public Vector2 ReadMove() => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    public Vector2 ReadTurn() => turnAction != null ? turnAction.ReadValue<Vector2>() : Vector2.zero;

    public Vector3 Tick(float dt, Vector3 bodyForward, Vector3 headForward, float movementScale)
    {
        if (energy == null) return Vector3.zero;
        if (safeStopAction != null && safeStopAction.WasPressedThisFrame()) SafeStop();
        if (boostAction != null && boostAction.WasPressedThisFrame()) TryBoost();

        Vector3 forward = Vector3.ProjectOnPlane(headRelativeSteering ? headForward : bodyForward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector2 stick = ReadMove();
        // Backward input allows retreat without matching forward flight acceleration.
        Vector3 direction = Vector3.ClampMagnitude(forward * (stick.y < 0f ? stick.y * 0.55f : stick.y) + right * stick.x, 1f);
        float speedCap = Mathf.Min(maxHorizontalSpeed, comfortMode ? 3.5f : maxHorizontalSpeed);
        float targetSpeed = Mathf.Min(speedCap, (IsFlying ? 3.2f : groundSpeed) * movementScale);
        Vector3 target = direction * targetSpeed;
        float acceleration = IsFlying ? airAcceleration : groundAcceleration;
        if (comfortMode) acceleration *= 0.65f;
        if (!IsFlying || stick.sqrMagnitude >= 0.01f)
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, acceleration * dt);
        else
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, horizontalDrag * dt);
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, speedCap);

        if (IsFlying)
        {
            float gravity = 9.81f * (glideEnabled ? glideGravityMultiplier : 1f);
            verticalVelocity = Mathf.Max(-4f, verticalVelocity - gravity * dt);
            if (State == CapeFlightState.Takeoff && Time.time - takeoffTime > 0.12f)
                ChangeState(CapeFlightState.Ascending);
            if (State == CapeFlightState.Ascending && verticalVelocity <= 0f)
                ChangeState(glideEnabled ? CapeFlightState.Gliding : CapeFlightState.Falling);
        }
        else verticalVelocity = -2f;
        return (horizontalVelocity + Vector3.up * verticalVelocity) * dt;
    }

    public bool TryBoost()
    {
        if (energy == null || Time.time < nextBoostTime || !energy.TryConsumeBoostEnergy())
        {
            OnBoostRejected?.Invoke();
            return false;
        }
        bool grounded = State == CapeFlightState.Grounded || State == CapeFlightState.Landing;
        verticalVelocity = Mathf.Min(maxVerticalSpeed, Mathf.Max(0f, verticalVelocity) +
            (grounded ? takeoffVelocityChange : airBoostVelocityChange));
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity + forward * forwardBoostVelocityChange,
            maxHorizontalSpeed);
        nextBoostTime = Time.time + flightCooldown;
        if (grounded) { takeoffTime = Time.time; ChangeState(CapeFlightState.Takeoff); }
        else ChangeState(CapeFlightState.Ascending);
        OnBoostSucceeded?.Invoke();
        return true;
    }

    public void ReportCollision(CollisionFlags flags)
    {
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
        if ((flags & CollisionFlags.Sides) != 0)
            horizontalVelocity *= 0.5f;
        if ((flags & CollisionFlags.Below) != 0 && verticalVelocity <= 0f)
        {
            lastGroundContact = Time.time;
            if (IsFlying) ChangeState(CapeFlightState.Landing);
            else ChangeState(CapeFlightState.Grounded);
            verticalVelocity = -2f;
        }
        else if ((flags & CollisionFlags.Below) == 0 && State == CapeFlightState.Grounded &&
            Time.time - lastGroundContact > 0.08f)
            ChangeState(glideEnabled ? CapeFlightState.Gliding : CapeFlightState.Falling);
    }

    public void AddWindLift(float amount)
    {
        if (IsFlying) verticalVelocity = Mathf.Min(maxVerticalSpeed, verticalVelocity + amount);
    }

    public void SafeStop()
    {
        horizontalVelocity = Vector3.zero;
        verticalVelocity = -0.5f;
        glideEnabled = true;
        if (IsFlying) ChangeState(CapeFlightState.Gliding);
        OnSafeStop?.Invoke();
    }

    public void SetGliding(bool enabled)
    {
        glideEnabled = enabled;
        if (!IsFlying || State == CapeFlightState.Ascending || State == CapeFlightState.Takeoff) return;
        ChangeState(enabled ? CapeFlightState.Gliding : CapeFlightState.Falling);
    }

    private void ChangeState(CapeFlightState next)
    {
        if (State == next) return;
        State = next;
        OnStateChanged?.Invoke(next);
    }
}
