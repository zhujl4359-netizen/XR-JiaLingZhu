using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class SkyQuestLocomotion : MonoBehaviour
{
    [Header("Tracked rig")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public GameObject avatar;

    [Header("Movement")]
    public float groundSpeed = 2.7f;
    public float launchSpeed = 2.8f;
    public float glideSpeed = 3.2f;
    public float terminalFallSpeed = 1.2f;
    public float launchHandSpeed = 0.9f;
    public float snapAngle = 20f;
    public float playRadius = 85f;
    public float groundHeight = 0f;
    public float stepHeight = 0.38f;
    [Range(1f, 4f)] public float movementScale = 2f;

    [Header("Comfort")]
    public float thirdPersonDistance = 4.5f;
    public float perspectiveBlendSpeed = 3f;
    public float vignetteAtSpeed = 4f;
    public bool useComfortVignette = true;
    public bool desktopControlled;

    public bool IsFlying { get; private set; }
    public bool ThirdPerson { get; private set; }
    public float CurrentSpeed { get; private set; }

    private float verticalSpeed;
    private float smoothTurnVelocity;
    private const float SmoothTurnDegreesPerSecond = 85f;
    private const float ComfortTurnDegreesPerSecond = 55f;
    private const float SmoothTurnDeadzone = 0.18f;
    private const float SmoothTurnResponse = 12f;
    private bool previousViewButton;
    private Vector3 avatarStartPosition;
    private Renderer[] avatarRenderers;
    private Material vignetteMaterial;
    private Transform vignette;
    private Vector3 horizontalVelocity;
    private Vector3 desktopFacing = Vector3.forward;
    private CharacterController bodyController;
    private CapeFlightController capeFlight;
    private CapeEnergySystem capeEnergy;
    private CapeEnergyFanHUD fanHUD;
    private SkyQuestCapeVisual capeVisual;
    private float nextDesktopFlightCost;
    private Transform vrSpeedTrack;
    private Transform vrSpeedBacking;
    private Transform vrSpeedFill;
    private Transform vrSpeedKnob;
    private TMPro.TextMeshPro vrSpeedLabel;
    private TMPro.TextMeshPro vrSpeedHint;
    private bool previousSpeedTrigger;
    private bool speedSliderCapturesTrigger;
    private readonly RaycastHit[] avatarCastHits = new RaycastHit[32];
    private readonly Collider[] avatarOverlapHits = new Collider[64];
    private const float DesktopEyeHeight = 1.65f;
    private const float DesktopCameraRadius = 0.22f;
    private const float MinVrAvatarDistance = 1.2f;

    void Awake()
    {
#if UNITY_EDITOR
        // The Quest scene can also be previewed in the editor. Without a
        // tracked headset its VR action map has no movement source, so use
        // the same character movement path as the keyboard scene.
        if (!desktopControlled && !HasTrackedHeadset())
        {
            desktopControlled = true;
            EnableEditorDesktopControls();
        }
#endif
    }

#if UNITY_EDITOR
    private void EnableEditorDesktopControls()
    {
        // The XR pose driver also writes the camera transform just before render.
        // In an untracked Editor preview that write cancels mouse look (and the
        // third-person camera offset), even though the keyboard script ran.
        if (head != null)
            foreach (Behaviour component in head.GetComponents<Behaviour>())
                if (component.GetType().Name == "TrackedPoseDriver")
                    component.enabled = false;

        SkyQuestAncestor ancestor = GetComponent<SkyQuestAncestor>();
        SkyQuestMagicExchange magic = GetComponent<SkyQuestMagicExchange>();
        SkyQuestSocial social = GetComponent<SkyQuestSocial>();
        SkyQuestProps props = GetComponent<SkyQuestProps>();
        SkyQuestCommunication communication = GetComponent<SkyQuestCommunication>();
        SkyQuestStatus status = GetComponent<SkyQuestStatus>();
        SkyQuestImmersion immersion = GetComponent<SkyQuestImmersion>();

        if (ancestor != null) ancestor.desktopMode = true;
        if (magic != null) magic.desktopMode = true;
        if (social != null) social.desktopMode = true;
        if (props != null) props.desktopMode = true;
        if (communication != null) communication.desktopMode = true;
        if (status != null) status.desktopMode = true;
        if (immersion != null) immersion.desktopMode = true;

        SkyQuestDesktopControls controls = GetComponent<SkyQuestDesktopControls>();
        if (controls == null) controls = gameObject.AddComponent<SkyQuestDesktopControls>();
        controls.head = head;
        controls.locomotion = this;
        controls.ancestor = ancestor;
        controls.magicExchange = magic;
        controls.social = social;
        controls.props = props;
        controls.communication = communication;
        controls.status = status;
    }

    private static bool HasTrackedHeadset()
    {
        var headsets = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, headsets);
        foreach (UnityEngine.XR.InputDevice device in headsets)
            if (device.isValid && device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) && tracked)
                return true;
        return false;
    }
#endif

    void Start()
    {
        ThirdPerson = true;
        const string speedKey = "SkyQuestMovementScaleV2";
        if (!PlayerPrefs.HasKey(speedKey))
        {
            movementScale = 2f;
            PlayerPrefs.SetFloat(speedKey, movementScale);
            PlayerPrefs.Save();
        }
        else movementScale = Mathf.Clamp(PlayerPrefs.GetFloat(speedKey, movementScale), 1f, 4f);
        bodyController = GetComponent<CharacterController>();
        if (bodyController == null) bodyController = gameObject.AddComponent<CharacterController>();
        bodyController.radius = 0.25f;
        bodyController.skinWidth = 0.035f;
        bodyController.slopeLimit = 48f;
        bodyController.stepOffset = stepHeight;
        bodyController.minMoveDistance = 0f;
        UpdateBodyHeight();
        capeEnergy = GetComponent<CapeEnergySystem>();
        if (capeEnergy == null) capeEnergy = gameObject.AddComponent<CapeEnergySystem>();
        capeEnergy.maxEnergy = 15f;
        capeEnergy.initialEnergy = 15f;
        capeEnergy.ResetEnergy();
        fanHUD = GetComponent<CapeEnergyFanHUD>();
        if (fanHUD == null) fanHUD = gameObject.AddComponent<CapeEnergyFanHUD>();
        fanHUD.energy = capeEnergy;
        fanHUD.head = head;
        fanHUD.desktopMode = desktopControlled;
        fanHUD.Initialize();
        SkyQuestItemHoverInfo hoverInfo = GetComponent<SkyQuestItemHoverInfo>();
        if (hoverInfo == null) hoverInfo = gameObject.AddComponent<SkyQuestItemHoverInfo>();
        hoverInfo.head = head;
        hoverInfo.rightHand = rightHand;
        hoverInfo.desktopMode = desktopControlled;
        if (!desktopControlled)
        {
            capeFlight = GetComponent<CapeFlightController>();
            if (capeFlight == null)
            {
                capeFlight = gameObject.AddComponent<CapeFlightController>();
                capeFlight.groundSpeed = groundSpeed;
            }
            capeFlight.Initialize(capeEnergy);
            // The left stick should move toward the direction the player is
            // looking, even when their tracked head is turned inside the rig.
            capeFlight.headRelativeSteering = true;
            CapeEnergyUI energyUI = GetComponent<CapeEnergyUI>();
            if (energyUI == null) energyUI = gameObject.AddComponent<CapeEnergyUI>();
            energyUI.Initialize(capeEnergy, leftHand);
            CapeFlightFeedback feedback = GetComponent<CapeFlightFeedback>();
            if (feedback == null) feedback = gameObject.AddComponent<CapeFlightFeedback>();
            feedback.Initialize(capeFlight, capeEnergy, head);
            CapeRechargeReceiver receiver = GetComponent<CapeRechargeReceiver>();
            if (receiver == null) receiver = gameObject.AddComponent<CapeRechargeReceiver>();
            receiver.Initialize(capeEnergy, capeFlight);
        }
        if (avatar != null)
        {
            avatarStartPosition = avatar.transform.localPosition;
            avatarRenderers = avatar.GetComponentsInChildren<Renderer>(true);
            SkyQuestAvatarWalk walk = avatar.GetComponent<SkyQuestAvatarWalk>();
            if (walk == null) walk = avatar.AddComponent<SkyQuestAvatarWalk>();
            walk.Initialize(this);
            capeVisual = avatar.GetComponent<SkyQuestCapeVisual>();
            if (capeVisual == null) capeVisual = avatar.AddComponent<SkyQuestCapeVisual>();
            capeVisual.Initialize(this, capeEnergy, capeFlight);
            // Include the generated cloth, stars and flight effects in the
            // existing first/third-person visibility control.
            avatarRenderers = avatar.GetComponentsInChildren<Renderer>(true);
        }
        desktopFacing = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (desktopFacing.sqrMagnitude < 0.01f) desktopFacing = Vector3.forward;
        CreateVignette();
        if (!desktopControlled)
        {
            FixRightControllerMaterials();
            CreateVrSpeedSlider();
        }
    }

    private void FixRightControllerMaterials()
    {
        if (rightHand == null) return;
        // The Starter Assets controller mesh contains a material that renders
        // as a purple error block in the Quest URP build. Keep the controller
        // model and tracking, but use the same runtime-safe material as our
        // other world primitives for its mesh parts.
        Material neutral = SkyQuestVisuals.Material(new Color(.76f, .82f, .88f));
        foreach (Renderer renderer in rightHand.GetComponentsInChildren<Renderer>(true))
        {
            // XR hands can instantiate their visual children after Start. Never
            // recolor a reward once it has been attached to the hand.
            if (renderer.GetComponentInParent<SkyQuestMagicPickup>() != null) continue;
            Material[] slots = renderer.sharedMaterials;
            if (slots.Length == 0) slots = new Material[1];
            for (int i = 0; i < slots.Length; i++) slots[i] = neutral;
            renderer.sharedMaterials = slots;
        }
    }

    private float nextMaterialRepair;

    private void RepairQuestErrorMaterials()
    {
        if (Time.unscaledTime < nextMaterialRepair) return;
        nextMaterialRepair = Time.unscaledTime + 2f;
        FixRightControllerMaterials();
        Material neutral = SkyQuestVisuals.Material(new Color(.82f, .86f, .92f));
        Material particle = Resources.Load<Material>("SkyCapeGlow");
        foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            Material[] slots = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < slots.Length; i++)
            {
                Material slot = slots[i];
                if (slot != null && slot.shader != null && slot.shader.name != "Hidden/InternalErrorShader") continue;
                slots[i] = renderer is ParticleSystemRenderer && particle != null ? particle : neutral;
                changed = true;
                Debug.LogWarning("SkyQuest repaired unsupported Quest material on " + renderer.name);
            }
            if (changed) renderer.sharedMaterials = slots;
        }
    }

    void Update()
    {
        if (head == null || leftHand == null || rightHand == null) return;
        float dt = Mathf.Max(Time.deltaTime, 0.001f);
        if (desktopControlled)
        {
            return;
        }
        UpdateBodyHeight();
        RepairQuestErrorMaterials();
        UpdateVrSpeedSlider();
        SkyQuestMagicPickup.HandleQuestControls(rightHand, head, speedSliderCapturesTrigger);
        var right = SkyQuestInput.Right;

        bool viewButton = SkyQuestInput.Secondary(right) &&
            SkyQuestInput.Grip(SkyQuestInput.Left) <= 0.75f;
        if (viewButton && !previousViewButton) TogglePerspective();
        previousViewButton = viewButton;
        UpdatePerspectiveVisuals();

        Vector2 turn = capeFlight != null ? capeFlight.ReadTurn() : Vector2.zero;
        float turnAmount = Mathf.Sign(turn.x) *
            Mathf.Clamp01((Mathf.Abs(turn.x) - SmoothTurnDeadzone) / (1f - SmoothTurnDeadzone));
        float turnRate = capeFlight != null && capeFlight.comfortMode ?
            ComfortTurnDegreesPerSecond : SmoothTurnDegreesPerSecond;
        float turnDt = Mathf.Min(dt, 0.05f);
        float targetTurnVelocity = turnAmount * turnRate;
        smoothTurnVelocity = Mathf.Lerp(smoothTurnVelocity, targetTurnVelocity,
            1f - Mathf.Exp(-SmoothTurnResponse * turnDt));
        if (Mathf.Abs(smoothTurnVelocity) > 0.01f)
            transform.RotateAround(head.position, Vector3.up, smoothTurnVelocity * turnDt);

        Vector3 displacement = capeFlight != null && capeFlight.enabled ?
            capeFlight.Tick(dt, transform.forward, head.forward, movementScale) : Vector3.zero;
        MoveRig(displacement, dt);
        IsFlying = capeFlight != null && capeFlight.IsFlying;
        UpdateAvatar(dt);
        UpdateVignette();
    }

    private void MoveRig(Vector3 displacement, float dt)
    {
        Vector3 next = transform.position + displacement;
        Vector2 radial = new Vector2(next.x, next.z);
        if (radial.magnitude > playRadius)
        {
            radial = radial.normalized * playRadius;
            next.x = radial.x;
            next.z = radial.y;
        }
        Vector3 before = transform.position;
        if (bodyController != null && bodyController.enabled)
        {
            CollisionFlags flags = bodyController.Move(next - before);
            if (!desktopControlled && capeFlight != null)
            {
                capeFlight.ReportCollision(flags);
                IsFlying = capeFlight.IsFlying;
            }
            if (desktopControlled && (flags & CollisionFlags.Below) != 0)
            {
                IsFlying = false;
                verticalSpeed = 0f;
            }
            if (desktopControlled && (flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
        }
        else transform.position = next;
        if (transform.position.y < groundHeight - 0.1f)
        {
            transform.position = new Vector3(transform.position.x, groundHeight, transform.position.z);
            if (desktopControlled) { IsFlying = false; verticalSpeed = 0f; }
            else { capeFlight?.ReportCollision(CollisionFlags.Below); IsFlying = false; }
        }
        CurrentSpeed = Vector3.Distance(before, transform.position) / dt;
    }

    private void UpdateBodyHeight()
    {
        if (bodyController == null || head == null) return;
        float height = Mathf.Clamp(head.position.y - transform.position.y + 0.1f, 1.2f, 2.2f);
        bodyController.height = height;
        Vector3 headLocal = transform.InverseTransformPoint(head.position);
        bodyController.center = new Vector3(headLocal.x, height * 0.5f + 0.035f, headLocal.z);
        bodyController.stepOffset = Mathf.Min(stepHeight, height - 0.05f);
    }

    private void OnGUI()
    {
        if (!desktopControlled) return;
        Rect panel = new Rect(Screen.width - 326f, Mathf.Max(110f, Screen.height * .4f - 58f),
            310f, 116f);
        GUI.Box(panel, "MOVEMENT SPEED  " + movementScale.ToString("0.0") + "x");
        if (GUI.Button(new Rect(panel.x + 14f, panel.y + 40f, 37f, 35f), "-"))
            SetMovementScale(Mathf.Round((movementScale - .1f) * 10f) / 10f);
        float selected = GUI.HorizontalSlider(new Rect(panel.x + 62f, panel.y + 45f, 187f, 30f),
            movementScale, 1f, 4f);
        SetMovementScale(selected);
        if (GUI.Button(new Rect(panel.x + 259f, panel.y + 40f, 37f, 35f), "+"))
            SetMovementScale(Mathf.Round((movementScale + .1f) * 10f) / 10f);
        GUI.Label(new Rect(panel.x + 61f, panel.y + 82f, 190f, 22f),
            "Slow 1.0x                         Fast 4.0x");
    }

    private void CreateVrSpeedSlider()
    {
        if (head == null) return;
        // Scale the backing, text, slider and hit area together around the HUD.
        Transform speedHud = new GameObject("VR speed control panel").transform;
        speedHud.SetParent(head, false);
        // Center below the Left grip+B comfort hint, with space for its text.
        speedHud.localPosition = new Vector3(0f, -.55f, 1.05f);
        speedHud.localScale = Vector3.one * .7225f;
        GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backing.name = "VR speed control backing";
        vrSpeedBacking = backing.transform;
        vrSpeedBacking.SetParent(speedHud, false);
        vrSpeedBacking.localPosition = new Vector3(0f, -.035f, .055f);
        vrSpeedBacking.localScale = new Vector3(.48f, .19f, .008f);
        SkyQuestVisuals.RemoveCollider(backing);
        backing.GetComponent<Renderer>().sharedMaterial =
            SkyQuestVisuals.Material(new Color(.065f, .12f, .19f));

        GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
        track.name = "VR movement speed slider";
        vrSpeedTrack = track.transform;
        vrSpeedTrack.SetParent(speedHud, false);
        vrSpeedTrack.localPosition = new Vector3(0f, -.037f, 0f);
        vrSpeedTrack.localScale = new Vector3(.34f, .027f, .022f);
        BoxCollider speedHitArea = track.GetComponent<BoxCollider>();
        speedHitArea.size = new Vector3(1.16f, 4.4f, 3f);
        speedHitArea.isTrigger = true;
        track.GetComponent<Renderer>().sharedMaterial =
            SkyQuestVisuals.Material(new Color(.34f, .48f, .59f));

        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "Speed selected range";
        vrSpeedFill = fill.transform;
        vrSpeedFill.SetParent(vrSpeedTrack, false);
        SkyQuestVisuals.RemoveCollider(fill);
        fill.GetComponent<Renderer>().sharedMaterial =
            SkyQuestVisuals.Material(new Color(.44f, .86f, .91f), true);

        GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knob.name = "VR speed slider handle";
        vrSpeedKnob = knob.transform;
        vrSpeedKnob.SetParent(vrSpeedTrack, false);
        vrSpeedKnob.localScale = new Vector3(.16f, 2f, 2f);
        SkyQuestVisuals.RemoveCollider(knob);
        knob.GetComponent<Renderer>().sharedMaterial =
            SkyQuestVisuals.Material(new Color(1f, 0.78f, 0.35f));
        vrSpeedLabel = SkyQuestVisuals.Text("Speed label", "", speedHud,
            new Vector3(0f, .025f, -.01f), 2.2f, Color.white, 3.3f);
        vrSpeedLabel.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        vrSpeedHint = SkyQuestVisuals.Text("Speed slider instructions",
            "1.0x     HOLD RIGHT TRIGGER TO ADJUST     4.0x", speedHud,
            new Vector3(0f, -.105f, -.01f), 1.15f,
            new Color(.82f, .94f, 1f), 3.8f);
        vrSpeedHint.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        RefreshVrSpeedSlider();
    }

    private void UpdateVrSpeedSlider()
    {
        if (vrSpeedTrack == null || rightHand == null) return;
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        if (!trigger)
        {
            speedSliderCapturesTrigger = false;
            previousSpeedTrigger = false;
            return;
        }
        if (!previousSpeedTrigger && Physics.Raycast(rightHand.position, rightHand.forward,
            out RaycastHit hit, 3f) && hit.collider != null &&
            hit.collider.transform == vrSpeedTrack)
        {
            speedSliderCapturesTrigger = true;
            SkyQuestInput.Pulse(SkyQuestInput.Right, .18f, .04f);
        }
        if (speedSliderCapturesTrigger)
        {
            Plane face = new Plane(vrSpeedTrack.forward, vrSpeedTrack.position);
            Ray pointer = new Ray(rightHand.position, rightHand.forward);
            if (face.Raycast(pointer, out float distance))
            {
                float x = vrSpeedTrack.InverseTransformPoint(pointer.GetPoint(distance)).x;
                SetMovementScale(Mathf.Lerp(1f, 4f, Mathf.InverseLerp(-.5f, .5f, x)));
            }
        }
        previousSpeedTrigger = trigger;
    }

    private void RefreshVrSpeedSlider()
    {
        float progress = Mathf.InverseLerp(1f, 4f, movementScale);
        if (vrSpeedFill != null)
        {
            vrSpeedFill.localScale = new Vector3(Mathf.Max(.01f, progress), 1.08f, 1.25f);
            vrSpeedFill.localPosition = new Vector3(-.5f + progress * .5f, 0f, -.53f);
        }
        if (vrSpeedKnob != null)
            vrSpeedKnob.localPosition = new Vector3(Mathf.Lerp(-.5f, .5f, progress), 0f, -.9f);
        if (vrSpeedLabel != null)
            vrSpeedLabel.text = "MOVE SPEED  " + movementScale.ToString("0.0") + "x";
    }

    private void UpdatePerspectiveVisuals()
    {
        if (desktopControlled) return;
        bool firstPerson = !ThirdPerson;
        if (vrSpeedTrack != null && vrSpeedTrack.gameObject.activeSelf != firstPerson)
            vrSpeedTrack.gameObject.SetActive(firstPerson);
        if (vrSpeedBacking != null && vrSpeedBacking.gameObject.activeSelf != firstPerson)
            vrSpeedBacking.gameObject.SetActive(firstPerson);
        if (vrSpeedLabel != null && vrSpeedLabel.gameObject.activeSelf != firstPerson)
            vrSpeedLabel.gameObject.SetActive(firstPerson);
        if (vrSpeedHint != null && vrSpeedHint.gameObject.activeSelf != firstPerson)
            vrSpeedHint.gameObject.SetActive(firstPerson);
        fanHUD?.SetVisible(firstPerson);
    }

    private void SetMovementScale(float selected)
    {
        selected = Mathf.Clamp(selected, 1f, 4f);
        if (Mathf.Abs(selected - movementScale) < 0.001f) return;
        movementScale = selected;
        PlayerPrefs.SetFloat("SkyQuestMovementScaleV2", movementScale);
        RefreshVrSpeedSlider();
    }

    void LateUpdate()
    {
        if (!desktopControlled || head == null) return;
        UpdateAvatar(Time.deltaTime);
        UpdateVignette();
    }

    public void DesktopMove(Vector2 axes, bool ascend, bool descend, bool launch)
    {
        if (!desktopControlled || head == null) return;
        float dt = Mathf.Max(Time.deltaTime, 0.001f);
        if (launch && !IsFlying)
        {
            if (capeEnergy != null && capeEnergy.TryConsumeBoostEnergy())
            {
                IsFlying = true;
                verticalSpeed = 1.5f;
                nextDesktopFlightCost = Time.time + .8f;
                capeVisual?.PlayBoost();
            }
        }
        if (IsFlying && ascend && Time.time >= nextDesktopFlightCost)
        {
            if (capeEnergy != null && capeEnergy.TryConsumeBoostEnergy())
            {
                nextDesktopFlightCost = Time.time + .8f;
                capeVisual?.PlayBoost();
            }
            else ascend = false;
        }
        // The third-person avatar belongs to the player rig. A tracked head
        // rotation only changes the view; it must not orbit the character.
        Vector3 forward = Vector3.ProjectOnPlane(ThirdPerson ? transform.forward : head.forward,
            Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 target = Vector3.ClampMagnitude(forward * axes.y + right * axes.x, 1f) *
            (IsFlying ? 2.3f : groundSpeed) * movementScale;
        if (target.sqrMagnitude > 0.0025f)
            desktopFacing = target.normalized;
        if (axes.sqrMagnitude < 0.001f && !IsFlying)
            horizontalVelocity = Vector3.zero;
        else
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, 12f * dt);
        if (IsFlying)
            verticalSpeed = Mathf.MoveTowards(verticalSpeed,
                ascend && capeEnergy != null && capeEnergy.CanBoost() ? 1.4f :
                descend ? -1.2f : -0.45f, 2.5f * dt);
        MoveRig((horizontalVelocity + Vector3.up * (IsFlying ? verticalSpeed : -2f)) * dt, dt);
    }

    public void TogglePerspective()
    {
        ThirdPerson = !ThirdPerson;
        SkyQuestInput.Pulse(SkyQuestInput.Right);
    }

    public void SetPerspective(bool thirdPerson)
    {
        ThirdPerson = thirdPerson;
    }

    public void AddWindLift(float amount)
    {
        if (!desktopControlled && capeFlight != null) { capeFlight.AddWindLift(amount); return; }
        if (IsFlying) verticalSpeed = Mathf.Min(verticalSpeed + amount, launchSpeed);
    }

    public void ShowEquippedAppearance()
    {
        ThirdPerson = true;
        if (avatar != null) avatarRenderers = avatar.GetComponentsInChildren<Renderer>(true);
    }

    private void UpdateAvatar(float dt)
    {
        if (avatar == null) return;
        if (desktopControlled)
        {
            UpdateDesktopCameraAndAvatar(dt);
            return;
        }
        Vector3 forward = Vector3.ProjectOnPlane(
            ThirdPerson ? transform.forward : head.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
        // Room-scale movement changes the real eye position without moving the rig root.
        Vector3 viewAnchor = ThirdPerson ? transform.position :
            new Vector3(head.position.x, transform.position.y, head.position.z);
        float avatarDistance = ThirdPerson ? thirdPersonDistance : 0f;
        if (ThirdPerson)
        {
            int hitCount = Physics.SphereCastNonAlloc(viewAnchor + Vector3.up * 0.9f,
                0.55f, forward, avatarCastHits, thirdPersonDistance,
                ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = avatarCastHits[i];
                if (hit.collider == null || hit.collider.transform == transform ||
                    hit.collider.transform.IsChildOf(transform) ||
                    hit.collider.transform.IsChildOf(avatar.transform) ||
                    IsRewardCollider(hit.collider)) continue;
                avatarDistance = Mathf.Min(avatarDistance, Mathf.Max(0f, hit.distance - 0.55f));
            }

            // Keep the full avatar capsule clear of columns, rails, and walls.
            // Search backward from the desired camera distance until the body fits.
            float safeDistance = avatarDistance;
            for (int i = 0; i < 10 && !AvatarFits(viewAnchor + forward * safeDistance +
                Vector3.up * avatarStartPosition.y); i++)
                safeDistance = Mathf.Max(0f, safeDistance - 0.22f);
            avatarDistance = safeDistance;
        }
        bool clearThirdPersonView = !ThirdPerson || avatarDistance >= MinVrAvatarDistance;
        Vector3 target = viewAnchor + forward * avatarDistance +
            Vector3.up * avatarStartPosition.y;
        if (clearThirdPersonView)
            avatar.transform.position = Vector3.MoveTowards(avatar.transform.position, target,
                Mathf.Max(perspectiveBlendSpeed, 10f) * dt);
        // Keep the third-person avatar still when the player merely turns
        // their head. During a walk, face the actual stick-driven direction.
        Vector3 facing = forward;
        if (ThirdPerson && capeFlight != null)
        {
            Vector3 walking = Vector3.ProjectOnPlane(capeFlight.Velocity, Vector3.up);
            if (walking.sqrMagnitude > .09f) facing = walking.normalized;
        }
        // The imported COTL avatar faces local -Z in both scenes.
        avatar.transform.rotation = Quaternion.Slerp(avatar.transform.rotation,
            Quaternion.LookRotation(-facing, Vector3.up), Mathf.Clamp01(8f * dt));
        if (avatarRenderers == null) return;
        float visibleDistance = Vector3.ProjectOnPlane(avatar.transform.position - head.position, Vector3.up).magnitude;
        bool showAvatar = ThirdPerson && clearThirdPersonView && visibleDistance >= MinVrAvatarDistance;
        foreach (Renderer renderer in avatarRenderers) if (renderer != null) renderer.enabled = showAvatar;
    }

    private void UpdateDesktopCameraAndAvatar(float dt)
    {
        // The rig and its CharacterController are the player's actual position.
        // Keep the visual character there and move only the camera for third person.
        avatar.transform.position = transform.position + Vector3.up * avatarStartPosition.y;
        Vector3 facing = desktopFacing.sqrMagnitude > 0.01f ? desktopFacing : transform.forward;
        avatar.transform.rotation = Quaternion.LookRotation(-facing, Vector3.up);

        Vector3 viewForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        if (viewForward.sqrMagnitude < 0.01f) viewForward = transform.forward;
        float distance = ThirdPerson ? Mathf.Max(3.8f, thirdPersonDistance) : 0f;
        if (ThirdPerson)
        {
            Vector3 origin = transform.position + Vector3.up * DesktopEyeHeight;
            int count = Physics.SphereCastNonAlloc(origin, DesktopCameraRadius,
                -viewForward, avatarCastHits, distance, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = avatarCastHits[i];
                if (hit.collider == null || hit.collider.transform == transform ||
                    hit.collider.transform.IsChildOf(transform) ||
                    IsRewardCollider(hit.collider)) continue;
                distance = Mathf.Min(distance, Mathf.Max(0.35f, hit.distance - DesktopCameraRadius));
            }
        }
        head.position = transform.position + Vector3.up * DesktopEyeHeight - viewForward * distance;
        if (avatarRenderers == null) return;
        foreach (Renderer renderer in avatarRenderers)
            if (renderer != null) renderer.enabled = ThirdPerson;
    }

    private bool AvatarFits(Vector3 position)
    {
        // Keep the feet above the support surface; the old capsule crossed the floor
        // and treated every otherwise clear avatar position as blocked.
        Vector3 bottom = position + Vector3.up * 0.55f;
        Vector3 top = position + Vector3.up * 1.55f;
        int count = Physics.OverlapCapsuleNonAlloc(bottom, top, 0.43f, avatarOverlapHits,
            ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            Collider collider = avatarOverlapHits[i];
            if (collider == null || collider.transform == transform ||
                collider.transform.IsChildOf(transform) ||
                (avatar != null && collider.transform.IsChildOf(avatar.transform)) ||
                IsRewardCollider(collider)) continue;
            return false;
        }
        return true;
    }

    private static bool IsRewardCollider(Collider collider)
    {
        return collider.GetComponentInParent<SkyQuestMagicPickup>() != null;
    }

    private void CreateVignette()
    {
        if (head == null) return;
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) return;
        vignetteMaterial = new Material(shader) { color = new Color(0f, 0f, 0f, 0f) };
        if (vignetteMaterial.HasProperty("_Surface"))
        {
            vignetteMaterial.SetFloat("_Surface", 1f);
            vignetteMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            vignetteMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            vignetteMaterial.SetFloat("_ZWrite", 0f);
            vignetteMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            vignetteMaterial.renderQueue = 3000;
        }
        GameObject ring = new GameObject("Motion comfort vignette", typeof(MeshFilter), typeof(MeshRenderer));
        vignette = ring.transform;
        vignette.SetParent(head, false);
        vignette.localPosition = new Vector3(0f, 0f, 0.52f);
        vignette.localRotation = Quaternion.identity;
        Mesh mesh = new Mesh();
        const int segments = 64;
        Vector3[] vertices = new Vector3[segments * 2];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices[i * 2] = new Vector3(Mathf.Cos(angle) * 0.36f, Mathf.Sin(angle) * 0.36f, 0f);
            vertices[i * 2 + 1] = new Vector3(Mathf.Cos(angle) * 0.75f, Mathf.Sin(angle) * 0.75f, 0f);
            colors[i * 2] = new Color(0f, 0f, 0f, 0f);
            colors[i * 2 + 1] = Color.white;
            int j = (i + 1) % segments;
            int k = i * 6;
            triangles[k] = i * 2; triangles[k + 1] = j * 2; triangles[k + 2] = i * 2 + 1;
            triangles[k + 3] = i * 2 + 1; triangles[k + 4] = j * 2; triangles[k + 5] = j * 2 + 1;
        }
        mesh.vertices = vertices; mesh.colors = colors; mesh.triangles = triangles;
        ring.GetComponent<MeshFilter>().sharedMesh = mesh;
        ring.GetComponent<MeshRenderer>().sharedMaterial = vignetteMaterial;
    }

    private void UpdateVignette()
    {
        if (vignetteMaterial == null) return;
        float amount = useComfortVignette && !ThirdPerson ?
            Mathf.Clamp01((CurrentSpeed - 2f) / Mathf.Max(0.1f, vignetteAtSpeed)) * 0.55f : 0f;
        vignetteMaterial.color = new Color(0f, 0f, 0f, amount);
    }
}
