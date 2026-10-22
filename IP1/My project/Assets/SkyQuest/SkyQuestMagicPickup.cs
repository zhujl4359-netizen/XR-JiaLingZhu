using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Interaction for every physical reward pulled from an ancestor tree.
// Z picks up/drops one nearby item. Holding Q previews the shared ballistic
// arc; release launches portable items or places setup items at its landing.
[DefaultExecutionOrder(200)]
public class SkyQuestMagicPickup : MonoBehaviour
{
    public Transform desktopHead;
    public Transform desktopHand;
    public Transform desktopAvatar;
    public bool desktopMode;
    public bool held;
    public bool placeable;
    public bool restoresCapeEnergy;
    public bool displayPottery;
    public float pickupRange = 2.8f;
    public float throwSpeed = 9f;

    // Keep every desktop throw in the same short, low style.  The last
    // preset still gives the player a useful choice without producing the
    // very tall 45/60 degree arcs used by the old version.
    private static readonly float[] AnglePresets = { 10f, 13f, 16f, 19f };
    private const float MaximumLaunchSpeed = 9f;
    private const float MaximumPreviewDistance = 8f;
    private static int lastZFrame = -1;
    private static int lastEFrame = -1;
    private static bool lastEConsumed;
    private static bool questGripWasDown;
    private static bool questTriggerWasDown;
    private static bool questTriggerHeldAtPress;
    private static SkyQuestMagicPickup questTriggerHeldItem;
    private static SkyQuestMagicPickup questTriggerPendingTable;
    private static float questTriggerStarted;
    private static float questEatNearFaceSince = -1f;
    private static LineRenderer questPickupArc;
    private static Canvas questTooltipCanvas;
    private static TextMeshProUGUI questAimProgress;

    private Rigidbody body;
    private Collider hitCollider;
    private CharacterController playerCollider;
    private Transform handHint;
    private LineRenderer aimArc;
    private LineRenderer landingRing;
    private TextMeshPro aimLabel;
    private Transform carryAnchor;
    private Transform firstPersonAnchor;
    private GameObject firstPersonHand;
    private GameObject firstPersonForearm;
    private SkyQuestLocomotion locomotion;
    private SkyQuestAvatarWalk walk;
    private Vector3 looseScale;
    private bool consuming;
    private float consumeStarted;
    private string consumeName;
    private static SkyQuestMagicPickup consumingItem;
    private const float ConsumeDuration = .82f;
    private bool aiming;
    private float aimStarted;
    private float selectedAngle = 16f;
    private Vector3 aimLandingPoint;
    private Vector3 aimLandingNormal = Vector3.up;
    private bool hasAimLanding;
    private float aimLandingTime;
    private bool inFlight;

    public static SkyQuestMagicPickup Held { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetQuestInput()
    {
        Held = null;
        consumingItem = null;
        questGripWasDown = false;
        questTriggerWasDown = false;
        questTriggerHeldAtPress = false;
        questTriggerHeldItem = null;
        questTriggerPendingTable = null;
        questTriggerStarted = 0f;
        questEatNearFaceSince = -1f;
        questPickupArc = null;
        questTooltipCanvas = null;
        questAimProgress = null;
    }

    public static void HandleQuestControls(Transform hand, Transform head,
        bool speedSliderCapturesTrigger = false)
    {
        if (hand == null || head == null) return;
        bool grip = SkyQuestInput.Grip(SkyQuestInput.Right) > .72f;
        if (grip && !questGripWasDown && consumingItem == null && questTriggerHeldItem == null &&
            SkyQuestInput.Grip(SkyQuestInput.Left) < .65f && !ExchangeMenuOpen())
        {
            if (Held != null && Held.held) Held.Drop(false);
            else
            {
                SkyQuestMagicPickup nearby = FindNearestLoose(hand, 1.15f);
                if (nearby != null) nearby.SetHeld(hand);
            }
        }
        questGripWasDown = grip;

        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f;
        SkyQuestCommunication chat = FindFirstObjectByType<SkyQuestCommunication>();
        bool menuOpen = ExchangeMenuOpen();
        bool chatOpen = chat != null && chat.IsOpen;
        bool canEatHeld = Held != null && Held.held && !Held.displayPottery &&
            consumingItem == null && !speedSliderCapturesTrigger && !menuOpen && !chatOpen;
        bool nearFace = canEatHeld && IsHeldRewardNearFace(hand, head, Held);
        if (!trigger || !nearFace) questEatNearFaceSince = -1f;
        else if (questEatNearFaceSince < 0f) questEatNearFaceSince = Time.unscaledTime;

        // A reward may be grabbed with the trigger already down. Keep checking
        // while it is held, so moving it to the mouth can still consume it.
        if (trigger && nearFace && (!questTriggerWasDown ||
            Time.unscaledTime - questEatNearFaceSince >= .22f))
        {
            if (questTriggerHeldItem != null)
            {
                questTriggerHeldItem.aiming = false;
                questTriggerHeldItem.SetAimVisible(false);
            }
            Held.BeginConsume();
            questTriggerHeldItem = null;
            questTriggerPendingTable = null;
            questTriggerHeldAtPress = false;
            questEatNearFaceSince = -1f;
            ShowQuestPickupArc(hand, head, null, false);
            questTriggerWasDown = true;
            return;
        }

        bool blocked = speedSliderCapturesTrigger || menuOpen || consumingItem != null ||
            nearFace || chatOpen;
        if (blocked && questTriggerHeldItem != null)
        {
            questTriggerHeldItem.aiming = false;
            questTriggerHeldItem.SetAimVisible(false);
            questTriggerHeldItem = null;
        }
        if (blocked) questTriggerPendingTable = null;

        // All loose rewards, including chat tables, share the same pointer.
        // A short table click opens chat; holding the trigger selects it.
        SkyQuestMagicPickup aimed = !blocked && (Held == null || !Held.held) ? FindAimedLoose(hand) : null;
        ShowQuestPickupArc(hand, head, aimed,
            !menuOpen && !chatOpen && consumingItem == null);
        if (trigger && !questTriggerWasDown)
        {
            questTriggerHeldAtPress = Held != null && Held.held;
            if (!blocked)
            {
                if (aimed != null)
                {
                    if (aimed.GetComponent<SkyQuestTeaTable>() != null)
                        questTriggerPendingTable = aimed;
                    else
                    {
                        aimed.SetHeld(hand);
                        questTriggerHeldItem = aimed;
                        SkyQuestInput.Pulse(SkyQuestInput.Right, .25f, .05f);
                    }
                }
                else if (Held != null && Held.held) questTriggerHeldItem = Held;
                questTriggerStarted = Time.unscaledTime;
            }
        }
        // A tree exchange can place a reward in the hand while the trigger is
        // already depressed. Begin its release/aim session when the menu closes.
        if (trigger && questTriggerWasDown && !blocked && questTriggerHeldItem == null &&
            questTriggerPendingTable == null && Held != null && Held.held)
        {
            questTriggerHeldItem = Held;
            questTriggerHeldAtPress = true;
            questTriggerStarted = Time.unscaledTime;
        }
        if (trigger && questTriggerPendingTable != null &&
            Time.unscaledTime - questTriggerStarted >= .32f)
        {
            if (!questTriggerPendingTable.held)
            {
                questTriggerPendingTable.SetHeld(hand);
                questTriggerHeldItem = questTriggerPendingTable;
                SkyQuestInput.Pulse(SkyQuestInput.Right, .25f, .05f);
            }
            questTriggerPendingTable = null;
        }
        if (trigger && questTriggerHeldItem != null && questTriggerHeldItem.held)
        {
            float heldTime = Time.unscaledTime - questTriggerStarted;
            if (heldTime >= .32f)
            {
                questTriggerHeldItem.aiming = true;
                int preset = Mathf.Clamp(Mathf.FloorToInt((heldTime - .32f) / .65f),
                    0, AnglePresets.Length - 1);
                questTriggerHeldItem.selectedAngle = AnglePresets[preset];
                questTriggerHeldItem.UpdateAimPreview(hand, head, "Release trigger");
            }
        }
        if (!trigger && questTriggerWasDown)
        {
            if (questTriggerHeldItem != null && questTriggerHeldItem.held)
            {
                if (questTriggerHeldItem.aiming)
                {
                    if (questTriggerHeldItem.placeable) questTriggerHeldItem.LaunchPlaceable(hand);
                    else questTriggerHeldItem.LaunchFromQuest(hand);
                }
                else if (questTriggerHeldAtPress) questTriggerHeldItem.Drop(false);
                else questTriggerHeldItem.SetAimVisible(false);
            }
            questTriggerHeldItem = null;
            questTriggerPendingTable = null;
            questTriggerHeldAtPress = false;
        }
        questTriggerWasDown = trigger;
    }

    private static bool IsHeldRewardNearFace(Transform hand, Transform head,
        SkyQuestMagicPickup reward)
    {
        Vector3 mouth = head.position + head.forward * .09f - head.up * .12f;
        float handDistance = Vector3.Distance(hand.position, mouth);
        Vector3 handFromMouth = hand.position - mouth;
        bool handInFront = Vector3.Dot(handFromMouth, head.forward) > -.2f;
        float handHeight = Vector3.Dot(handFromMouth, head.up);
        if (handInFront && handHeight > -.24f && handDistance <= .48f) return true;

        // Different reward meshes extend different distances beyond the grip.
        // Let their visible surface reach the mouth, but require the hand to
        // remain nearby so large tables cannot be eaten from across the room.
        if (!handInFront || handHeight < -.55f || handDistance > .75f || reward == null)
            return false;
        foreach (Renderer visual in reward.GetComponentsInChildren<Renderer>())
        {
            if (!visual.enabled || visual is ParticleSystemRenderer || visual is LineRenderer)
                continue;
            if (Vector3.Distance(visual.bounds.ClosestPoint(mouth), mouth) <= .16f)
                return true;
        }
        return false;
    }

    private static SkyQuestMagicPickup FindAimedLoose(Transform hand)
    {
        SkyQuestMagicPickup nearest = null;
        float best = 10f;
        RaycastHit[] hits = Physics.RaycastAll(hand.position, hand.forward,
            10f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.GetComponentInParent<SkyQuestLocomotion>() != null) continue;
            SkyQuestMagicPickup item = hit.collider.GetComponentInParent<SkyQuestMagicPickup>();
            if (item == null) break; // a wall or other prop occludes the reward
            if (item.held || item.inFlight || hit.distance >= best) continue;
            nearest = item;
            best = hit.distance;
            break;
        }
        return nearest;
    }

    private static void ShowQuestPickupArc(Transform hand, Transform head,
        SkyQuestMagicPickup item, bool showHeldItem)
    {
        if (questPickupArc == null)
        {
            GameObject arc = new GameObject("Right controller pickup arc", typeof(LineRenderer));
            questPickupArc = arc.GetComponent<LineRenderer>();
            questPickupArc.sharedMaterial = SkyQuestVisuals.Material(new Color(.66f, .92f, 1f), true);
            questPickupArc.positionCount = 18;
            questPickupArc.startWidth = .018f;
            questPickupArc.endWidth = .009f;
            questPickupArc.useWorldSpace = true;
        }
        questPickupArc.enabled = item != null;
        SkyQuestMagicPickup selected = item != null ? item : questTriggerHeldItem;
        if (selected == null && showHeldItem && Held != null && Held.held) selected = Held;
        if (selected == null) { HideQuestAimProgress(); return; }
        if (item != null)
        {
            Vector3 start = hand.position;
            Vector3 end = item.hitCollider != null ? item.hitCollider.bounds.center : item.transform.position;
            for (int i = 0; i < questPickupArc.positionCount; i++)
            {
                float t = i / (float)(questPickupArc.positionCount - 1);
                questPickupArc.SetPosition(i, Vector3.Lerp(start, end, t) +
                    Vector3.up * (.18f * 4f * t * (1f - t)));
            }
        }
        if (questAimProgress == null) BuildQuestTooltip(head);
        questTooltipCanvas.gameObject.SetActive(true);
        bool hasBounds = selected.hitCollider != null && selected.hitCollider.enabled;
        Vector3 tooltipPosition = hasBounds ?
            selected.hitCollider.bounds.center : selected.transform.position;
        tooltipPosition.y = hasBounds ?
            selected.hitCollider.bounds.max.y + .18f :
            selected.transform.position.y + (selected.held ? .28f : .72f);
        if (selected.held)
        {
            Vector3 fromHead = tooltipPosition - head.position;
            if (fromHead.sqrMagnitude < 1f)
                tooltipPosition = head.position +
                    (fromHead.sqrMagnitude > .0001f ? fromHead.normalized : head.forward);
        }
        Transform tooltip = questTooltipCanvas.transform;
        tooltip.position = tooltipPosition;
        tooltip.rotation = Quaternion.LookRotation(tooltipPosition - head.position, Vector3.up);
        tooltip.localScale = Vector3.one * (selected.held ? .00085f :
            .0015f * Mathf.Clamp(Vector3.Distance(head.position, tooltipPosition) / 3f, 1f, 3f));
        questAimProgress.text = selected.displayPottery ?
            "GRAB: RIGHT TRIGGER\nTHROW: HOLD RIGHT TRIGGER, RELEASE" :
            (selected.restoresCapeEnergy ? "EAT +1 ENERGY" : "EAT +5 CANDLES") +
            ": RIGHT TRIGGER NEAR FACE\nTHROW: HOLD RIGHT TRIGGER, RELEASE";
    }

    private static void BuildQuestTooltip(Transform head)
    {
        GameObject root = new GameObject("Selected item tooltip",
            typeof(RectTransform), typeof(Canvas));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(590f, 106f);
        questTooltipCanvas = root.GetComponent<Canvas>();
        questTooltipCanvas.renderMode = RenderMode.WorldSpace;
        questTooltipCanvas.worldCamera = head.GetComponent<Camera>();
        questTooltipCanvas.overrideSorting = true;
        questTooltipCanvas.sortingOrder = 90;

        GameObject plate = new GameObject("Bright interaction plate",
            typeof(RectTransform), typeof(Image));
        RectTransform plateRect = plate.GetComponent<RectTransform>();
        plateRect.SetParent(rect, false);
        plateRect.anchorMin = Vector2.zero;
        plateRect.anchorMax = Vector2.one;
        plateRect.offsetMin = plateRect.offsetMax = Vector2.zero;
        Image background = plate.GetComponent<Image>();
        background.color = new Color(1f, .9f, .12f, .95f);
        background.raycastTarget = false;

        GameObject textObject = new GameObject("Reward description",
            typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(rect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15f, 5f);
        textRect.offsetMax = new Vector2(-15f, -5f);
        questAimProgress = textObject.GetComponent<TextMeshProUGUI>();
        questAimProgress.fontSize = 25f;
        questAimProgress.alignment = TextAlignmentOptions.Center;
        questAimProgress.color = Color.black;
        questAimProgress.enableWordWrapping = false;
        questAimProgress.raycastTarget = false;
        root.SetActive(false);
    }

    private static void HideQuestAimProgress()
    {
        if (questTooltipCanvas != null) questTooltipCanvas.gameObject.SetActive(false);
    }

    private static bool ExchangeMenuOpen()
    {
        SkyQuestMagicExchange magic = FindFirstObjectByType<SkyQuestMagicExchange>();
        SkyQuestAncestor ancestor = FindFirstObjectByType<SkyQuestAncestor>();
        return magic != null && magic.IsMenuOpen || ancestor != null && ancestor.IsMenuOpen;
    }

    public static bool TryConsumeWithE(Transform viewer)
    {
        if (lastEFrame == Time.frameCount) return lastEConsumed;
        lastEFrame = Time.frameCount;
        lastEConsumed = false;
        if (consumingItem != null) return lastEConsumed = true;

        SkyQuestCommunication communication = FindFirstObjectByType<SkyQuestCommunication>();
        if (communication != null && communication.IsOpen) return false;

        SkyQuestMagicPickup target = Held != null && Held.held ? Held : FindNearestLoose(viewer);
        if (target == null || target.displayPottery) return false;
        target.BeginConsume();
        lastEConsumed = true;
        return true;
    }

    private void BeginConsume()
    {
        if (!held) SetHeld(desktopHand != null ? desktopHand : desktopHead);
        consumeName = gameObject.name;
        const string rewardSuffix = " reward";
        if (consumeName.EndsWith(rewardSuffix))
            consumeName = consumeName.Substring(0, consumeName.Length - rewardSuffix.Length);
        consuming = true;
        consumingItem = this;
        consumeStarted = Time.time;
        aiming = false;
        if (Held == this) Held = null;
        SetAimVisible(false);
        walk?.PlayEatGesture();
        FindFirstObjectByType<SkyQuestStatus>()?.PlayCrunch();
    }

    private void FinishConsume()
    {
        SkyQuestStatus status = FindFirstObjectByType<SkyQuestStatus>();
        if (restoresCapeEnergy)
        {
            CapeEnergySystem energy = FindFirstObjectByType<CapeEnergySystem>();
            if (energy != null)
            {
                energy.RestoreEnergy(1f);
                status?.Show(consumeName + " consumed. +1 fly energy (" +
                    Mathf.RoundToInt(energy.CurrentEnergy) + "/" + Mathf.RoundToInt(energy.maxEnergy) + ").", 4f);
            }
        }
        else
        {
            SkyQuestCandleWallet.Add(5);
            status?.Show(consumeName + " consumed. +5 candles (" + SkyQuestCandleWallet.Balance + ").", 4f);
        }
        if (consumingItem == this) consumingItem = null;
        Consume();
    }

    void Awake()
    {
        looseScale = transform.localScale;
        body = gameObject.GetComponent<Rigidbody>();
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        body.mass = .35f;
        body.linearDamping = .24f;
        body.angularDamping = .3f;
        body.useGravity = true;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        hitCollider = gameObject.GetComponent<Collider>();
        if (hitCollider is MeshCollider)
        {
            hitCollider.enabled = false;
            hitCollider = null;
        }
        if (hitCollider == null) hitCollider = gameObject.AddComponent<BoxCollider>();
        if (hitCollider is BoxCollider box)
            FitVisibleBounds(box);
        // Imported ceramic models can carry small nested colliders that
        // intercept the pointer or interfere with rigidbody throws. The one
        // fitted root collider handles every shape consistently.
        foreach (Collider child in GetComponentsInChildren<Collider>(true))
            if (child != hitCollider) child.enabled = false;
        // Interaction and aiming visuals are built lazily. Creating all of
        // their primitives during a GUI drag caused a visible one-frame hitch.
    }

    private void FitVisibleBounds(BoxCollider box)
    {
        Vector3 minimum = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 maximum = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        bool found = false;
        foreach (Renderer visual in GetComponentsInChildren<Renderer>(true))
        {
            if (visual is ParticleSystemRenderer) continue;
            Bounds bounds = visual.bounds;
            for (int corner = 0; corner < 8; corner++)
            {
                Vector3 world = new Vector3((corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (corner & 4) == 0 ? bounds.min.z : bounds.max.z);
                Vector3 local = transform.InverseTransformPoint(world);
                minimum = Vector3.Min(minimum, local);
                maximum = Vector3.Max(maximum, local);
                found = true;
            }
        }
        if (!found)
        {
            box.center = new Vector3(0f, .22f, 0f);
            box.size = new Vector3(.62f, .72f, .62f);
            return;
        }
        box.center = (minimum + maximum) * .5f;
        Vector3 size = maximum - minimum + Vector3.one * .12f;
        box.size = new Vector3(Mathf.Max(.5f, size.x), Mathf.Max(.65f, size.y),
            Mathf.Max(.5f, size.z));
    }

    public void AnchorOnDisplay()
    {
        if (body == null) return;
        displayPottery = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        body.useGravity = false;
    }

    public void SetHeld(Transform hand)
    {
        if (Held != null && Held != this) Held.Drop(true);
        Held = this;
        held = true;
        aiming = false;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        body.useGravity = false;
        body.detectCollisions = false;
        if (hitCollider != null) hitCollider.enabled = false;
        if (locomotion == null && desktopHead != null)
            locomotion = desktopHead.GetComponentInParent<SkyQuestLocomotion>();
        if (walk == null && desktopAvatar != null)
            walk = desktopAvatar.GetComponent<SkyQuestAvatarWalk>();
        Transform target = ResolveCarryTarget(hand);
        if (target != null)
        {
            transform.SetParent(target, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(7f, 0f, -8f);
        }
        UpdateHeldVisual();
        SetHintVisible(false);
        SetAimVisible(false);
    }

    public void Release(bool throwForward)
    {
        if (throwForward) Launch(selectedAngle);
        else Drop(false);
    }

    public void TogglePickup(Transform hand)
    {
        if (held) Drop(false); else SetHeld(hand);
    }

    private void Drop(bool rollAway)
    {
        if (!held) return;
        HideFirstPersonHand();
        transform.SetParent(null, true);
        transform.localScale = looseScale;
        held = false;
        aiming = false;
        body.isKinematic = false;
        body.useGravity = true;
        body.detectCollisions = true;
        if (hitCollider != null) hitCollider.enabled = true;
        IgnorePlayerCollision();
        if (rollAway)
        {
            Vector3 right = desktopHead != null ? desktopHead.right : Vector3.right;
            Vector3 forward = desktopHead != null ? Vector3.ProjectOnPlane(desktopHead.forward, Vector3.up).normalized : Vector3.forward;
            body.linearVelocity = right * .9f + forward * .45f + Vector3.up * .3f;
            body.angularVelocity = new Vector3(2.5f, 1.5f, -4.5f);
        }
        else
        {
            body.linearVelocity = Vector3.down * .15f;
            body.angularVelocity = new Vector3(.4f, 0f, -.8f);
        }
        if (Held == this) Held = null;
        SetAimVisible(false);
    }

    private void Launch(float angle)
    {
        if (desktopHead != null) LaunchWithForward(desktopHead.forward, angle);
    }

    private void LaunchFromQuest(Transform hand)
    {
        if (hand != null) LaunchWithForward(hand.forward, selectedAngle);
    }

    // Setup props travel along their previewed arc as well, then settle
    // upright at the marked point so tables remain usable for chat.
    private void LaunchPlaceable(Transform source)
    {
        if (!held || source == null) return;
        if (!hasAimLanding || aimLandingNormal.y < .45f)
        {
            Drop(false);
            return;
        }
        Vector3 forward = Vector3.ProjectOnPlane(source.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < .01f) forward = transform.forward;
        float radians = selectedAngle * Mathf.Deg2Rad;
        Vector3 velocity = (forward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) *
            EffectiveThrowSpeed();
        Vector3 start = transform.position;
        HideFirstPersonHand();
        transform.SetParent(null, true);
        transform.localScale = looseScale;
        held = false;
        aiming = false;
        inFlight = true;
        if (Held == this) Held = null;
        body.isKinematic = true;
        body.useGravity = false;
        body.detectCollisions = false;
        if (hitCollider != null) hitCollider.enabled = false;
        SetAimVisible(false);
        StartCoroutine(FlyPlaceable(start, velocity, Mathf.Clamp(aimLandingTime, .25f, 2.3f)));
    }

    private IEnumerator FlyPlaceable(Vector3 start, Vector3 velocity, float duration)
    {
        float elapsed = 0f;
        Vector3 landing = aimLandingPoint + aimLandingNormal * .04f;
        while (elapsed < duration)
        {
            elapsed = Mathf.Min(duration, elapsed + Time.deltaTime);
            Vector3 ballistic = start + velocity * elapsed + Physics.gravity * (.5f * elapsed * elapsed);
            float finish = Mathf.InverseLerp(duration * .85f, duration, elapsed);
            transform.position = Vector3.Lerp(ballistic, landing, finish * finish);
            yield return null;
        }
        PlaceAtAim();
        inFlight = false;
    }

    private void LaunchWithForward(Vector3 direction, float angle)
    {
        if (!held) return;
        HideFirstPersonHand();
        transform.SetParent(null, true);
        transform.localScale = looseScale;
        held = false;
        aiming = false;
        body.isKinematic = false;
        body.useGravity = true;
        body.detectCollisions = true;
        // The preview is a ballistic arc, so disable drag while airborne to
        // keep the actual throw on that same curve for every reward shape.
        body.linearDamping = 0f;
        if (hitCollider != null) hitCollider.enabled = true;
        IgnorePlayerCollision();
        Vector3 flatForward = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < .01f) flatForward = transform.forward;
        float radians = angle * Mathf.Deg2Rad;
        body.linearVelocity = (flatForward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) * EffectiveThrowSpeed();
        body.angularVelocity = new Vector3(2.2f, 4f, -1.4f);
        if (Held == this) Held = null;
        SetAimVisible(false);
    }

    void Update()
    {
        if (consuming) return;
        if (desktopHead == null) return;
        if (!desktopMode)
        {
            if (!held) UpdateLooseHint();
            return;
        }
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame)
            TryConsumeWithE(desktopAvatar != null ? desktopAvatar : desktopHead);

        if (keyboard.zKey.wasPressedThisFrame && lastZFrame != Time.frameCount)
        {
            lastZFrame = Time.frameCount;
            if (Held != null) Held.Drop(false);
            else PickNearest(desktopAvatar != null ? desktopAvatar : desktopHead);
        }

        if (Held == this)
        {
            if (keyboard.qKey.wasPressedThisFrame)
            {
                aiming = true;
                aimStarted = Time.time;
                selectedAngle = AnglePresets[0];
            }
            if (aiming && keyboard.qKey.isPressed)
            {
                int preset = Mathf.Clamp(Mathf.FloorToInt((Time.time - aimStarted) / .65f), 0, AnglePresets.Length - 1);
                selectedAngle = AnglePresets[preset];
                UpdateAimPreview();
            }
            if (aiming && keyboard.qKey.wasReleasedThisFrame)
            {
                if (placeable) LaunchPlaceable(desktopHead);
                else Launch(selectedAngle);
            }
        }
        else
        {
            SetAimVisible(false);
            UpdateLooseHint();
        }
    }

    private void UpdateLooseHint()
    {
        Transform viewer = desktopAvatar != null ? desktopAvatar : desktopHead;
        SetHintVisible(Held == null && FindNearestLoose(viewer) == this);
        if (handHint != null && handHint.gameObject.activeSelf)
        {
            handHint.position = transform.position + Vector3.up * HintHeight();
            handHint.rotation = Quaternion.LookRotation(handHint.position - desktopHead.position, Vector3.up);
        }
    }

    private void IgnorePlayerCollision()
    {
        if (hitCollider == null) return;
        if (playerCollider == null && locomotion != null)
            playerCollider = locomotion.GetComponent<CharacterController>();
        if (playerCollider == null) return;
        // Unity resets IgnoreCollision when a collider is disabled. Reapply it
        // each time this prop leaves the hand. A loose reward is collectible,
        // never a step that should lift the player or snag the camera.
        Physics.IgnoreCollision(hitCollider, playerCollider, true);
    }

    void LateUpdate()
    {
        if (!held) return;
        UpdateHeldVisual();
        if (consuming && Time.time - consumeStarted >= ConsumeDuration)
            FinishConsume();
    }

    private static void PickNearest(Transform viewer)
    {
        SkyQuestMagicPickup nearest = FindNearestLoose(viewer);
        if (nearest != null) nearest.SetHeld(nearest.desktopHand != null ? nearest.desktopHand : viewer);
    }

    private void Consume()
    {
        held = false;
        aiming = false;
        HideFirstPersonHand();
        if (Held == this) Held = null;
        if (consumingItem == this) consumingItem = null;
        SetHintVisible(false);
        SetAimVisible(false);
        Destroy(gameObject);
    }

    private static SkyQuestMagicPickup FindNearestLoose(Transform viewer) =>
        FindNearestLoose(viewer, float.PositiveInfinity);

    private static SkyQuestMagicPickup FindNearestLoose(Transform viewer, float maxDistance)
    {
        if (viewer == null) return null;
        SkyQuestMagicPickup nearest = null;
        float best = float.PositiveInfinity;
        foreach (SkyQuestMagicPickup item in FindObjectsByType<SkyQuestMagicPickup>(FindObjectsSortMode.None))
        {
            if (item == null || item.held || item.inFlight) continue;
            float distance = Vector3.Distance(item.transform.position, viewer.position);
            if (distance <= Mathf.Min(item.pickupRange, maxDistance) && distance < best)
            {
                best = distance;
                nearest = item;
            }
        }
        return nearest;
    }

    private void PlaceAtAim()
    {
        if (desktopHead == null || !hasAimLanding || aimLandingNormal.y < .45f)
        {
            Drop(false);
            return;
        }
        HideFirstPersonHand();
        transform.SetParent(null, true);
        transform.localScale = looseScale;
        transform.position = aimLandingPoint + aimLandingNormal * .04f;
        transform.rotation = Quaternion.Euler(0f, desktopHead.eulerAngles.y, 0f);
        held = false;
        aiming = false;
        body.isKinematic = false;
        body.useGravity = true;
        body.detectCollisions = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        if (hitCollider != null) hitCollider.enabled = true;
        IgnorePlayerCollision();
        if (Held == this) Held = null;
        SetAimVisible(false);
    }

    private void CreateHandHint()
    {
        if (handHint != null) return;
        handHint = new GameObject("Pickup hand hint").transform;
        handHint.SetParent(transform, false);
        handHint.localScale = Vector3.one * .82f;
        Color white = new Color(1f, .98f, .9f);
        AddHintPart("Palm", PrimitiveType.Sphere, new Vector3(0f, -.02f, 0f), new Vector3(.18f, .22f, .055f), white);
        for (int i = 0; i < 4; i++)
            AddHintPart("Finger " + i, PrimitiveType.Capsule, new Vector3(-.105f + i * .07f, .18f + Mathf.Abs(1.5f - i) * .018f, 0f), new Vector3(.035f, .105f + (i == 1 || i == 2 ? .035f : 0f), .035f), white);
        GameObject thumb = AddHintPart("Thumb", PrimitiveType.Capsule, new Vector3(-.17f, .02f, 0f), new Vector3(.04f, .1f, .04f), white);
        thumb.transform.localRotation = Quaternion.Euler(0f, 0f, 58f);
        handHint.gameObject.SetActive(false);
    }

    // The desktop tracked hand is a camera child. A separate first-person
    // anchor keeps the prop in the lower-right of the view; third person uses
    // the visible model's wrist after the walking animation has posed it.
    private Transform ResolveCarryTarget(Transform fallback)
    {
        if (desktopMode && locomotion != null && !locomotion.ThirdPerson && desktopHead != null)
        {
            if (firstPersonAnchor == null) CreateFirstPersonAnchor();
            return firstPersonAnchor;
        }
        if (desktopAvatar != null && locomotion != null && locomotion.ThirdPerson)
        {
            if (carryAnchor == null)
            {
                carryAnchor = new GameObject("Visible right-hand prop anchor").transform;
                carryAnchor.SetParent(desktopAvatar, false);
            }
            return carryAnchor;
        }
        return fallback != null ? fallback : desktopHand != null ? desktopHand : desktopHead;
    }

    private void UpdateDesktopCarryAnchor()
    {
        if (carryAnchor == null || desktopAvatar == null) return;
        Vector3 facing = Vector3.ProjectOnPlane(-desktopAvatar.forward, Vector3.up).normalized;
        if (facing.sqrMagnitude < .01f) facing = Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, facing).normalized;
        Vector3 rest = walk != null && walk.RightWrist != null
            ? walk.RightWrist.position + facing * .035f
            : desktopAvatar.position + Vector3.up * .9f + right * .34f + facing * .1f;
        if (consuming && (walk == null || walk.RightWrist == null))
        {
            float reach = Mathf.SmoothStep(0f, 1f,
                Mathf.Clamp01((Time.time - consumeStarted) / .35f));
            Vector3 mouth = desktopAvatar.position + Vector3.up * 1.36f + facing * .2f;
            rest = Vector3.Lerp(rest, mouth, reach);
        }
        carryAnchor.position = rest;
        carryAnchor.rotation = Quaternion.LookRotation(facing, Vector3.up);
    }

    private void CreateFirstPersonAnchor()
    {
        firstPersonAnchor = new GameObject("First-person right-hand prop anchor").transform;
        firstPersonAnchor.SetParent(desktopHead, false);
        firstPersonHand = new GameObject("Visible first-person right hand");
        firstPersonHand.transform.SetParent(firstPersonAnchor, false);
        Color skin = new Color(.86f, .9f, .94f);
        Color sleeve = new Color(.64f, .75f, .88f);
        GameObject palm = SkyQuestVisuals.Primitive("Right palm", PrimitiveType.Sphere,
            firstPersonHand.transform, new Vector3(.025f, -.035f, -.005f),
            new Vector3(.13f, .075f, .12f), skin);
        Collider palmCollider = palm.GetComponent<Collider>();
        if (palmCollider != null) palmCollider.enabled = false;
        SkyQuestVisuals.RemoveCollider(palm);
        firstPersonForearm = SkyQuestVisuals.Primitive("Right forearm", PrimitiveType.Capsule,
            desktopHead, Vector3.zero, new Vector3(.08f, .25f, .08f), sleeve);
        Collider forearmCollider = firstPersonForearm.GetComponent<Collider>();
        if (forearmCollider != null) forearmCollider.enabled = false;
        SkyQuestVisuals.RemoveCollider(firstPersonForearm);
    }

    private void HideFirstPersonHand()
    {
        if (firstPersonHand != null) firstPersonHand.SetActive(false);
        if (firstPersonForearm != null) firstPersonForearm.SetActive(false);
    }

    private void UpdateHeldVisual()
    {
        if (!held) return;
        Transform target = ResolveCarryTarget(desktopHand);
        if (target == null) return;
        if (transform.parent != target)
            transform.SetParent(target, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(7f, 0f, -8f);

        bool firstPerson = desktopMode && target == firstPersonAnchor;
        if (!firstPerson) HideFirstPersonHand();
        if (firstPerson)
        {
            if (firstPersonHand != null) firstPersonHand.SetActive(true);
            if (firstPersonForearm != null) firstPersonForearm.SetActive(true);
            float reach = consuming ? Mathf.SmoothStep(0f, 1f,
                Mathf.Clamp01((Time.time - consumeStarted) / .37f)) : 0f;
            firstPersonAnchor.localPosition = Vector3.Lerp(
                new Vector3(.39f, -.42f, .82f), new Vector3(.12f, -.15f, .7f), reach);
            firstPersonAnchor.localRotation = Quaternion.Euler(-12f * reach, -8f * reach, 20f * reach);
            float shrink = consuming ? Mathf.Lerp(1f, .38f,
                Mathf.Clamp01((Time.time - consumeStarted - .52f) / .28f)) : 1f;
            transform.localScale = looseScale * (.48f * Mathf.Lerp(1f, .66f, reach) * shrink);
            if (firstPersonForearm != null)
            {
                Vector3 basePoint = new Vector3(.65f, -.81f, .52f);
                Vector3 handPoint = firstPersonAnchor.localPosition + new Vector3(.025f, -.04f, 0f);
                Vector3 direction = handPoint - basePoint;
                firstPersonForearm.transform.localPosition = (basePoint + handPoint) * .5f;
                firstPersonForearm.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
                firstPersonForearm.transform.localScale = new Vector3(.08f, direction.magnitude * .5f, .08f);
            }
        }
        else if (target == carryAnchor)
        {
            UpdateDesktopCarryAnchor();
            float shrink = consuming ? Mathf.Lerp(1f, .2f,
                Mathf.Clamp01((Time.time - consumeStarted - .54f) / .25f)) : 1f;
            transform.localScale = looseScale * (.55f * shrink);
        }
        else
        {
            transform.localScale = looseScale * .55f;
            if (consuming && desktopHead != null)
            {
                float reach = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01((Time.time - consumeStarted) / .38f));
                Vector3 mouth = desktopHead.position + desktopHead.forward * .36f - desktopHead.up * .18f;
                transform.position = Vector3.Lerp(target.position, mouth, reach);
            }
        }
    }

    private GameObject AddHintPart(string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject part = SkyQuestVisuals.Primitive(name, type, handHint, localPosition, localScale, color, true);
        SkyQuestVisuals.RemoveCollider(part);
        return part;
    }

    private float HintHeight()
    {
        return hitCollider != null ? Mathf.Max(.7f, hitCollider.bounds.extents.y + .42f) : .9f;
    }

    private void SetHintVisible(bool value)
    {
        if (value && handHint == null) CreateHandHint();
        if (handHint != null && handHint.gameObject.activeSelf != value) handHint.gameObject.SetActive(value);
    }

    private void CreateAimPreview()
    {
        if (aimArc != null) return;
        GameObject arcObject = new GameObject("Launch angle preview");
        aimArc = arcObject.AddComponent<LineRenderer>();
        aimArc.material = SkyQuestVisuals.Material(new Color(1f, .9f, .55f), true);
        aimArc.startWidth = .035f;
        aimArc.endWidth = .012f;
        aimArc.positionCount = 56;
        aimArc.useWorldSpace = true;
        aimArc.numCapVertices = 3;

        GameObject ringObject = new GameObject("Launch landing ring");
        landingRing = ringObject.AddComponent<LineRenderer>();
        landingRing.material = SkyQuestVisuals.Material(new Color(1f, .9f, .55f), true);
        landingRing.startWidth = .075f;
        landingRing.endWidth = .075f;
        landingRing.positionCount = 49;
        landingRing.loop = true;
        landingRing.useWorldSpace = true;
        landingRing.numCapVertices = 3;
        aimLabel = SkyQuestVisuals.Text("Launch angle label", "", null, Vector3.zero, 2.5f, new Color(1f, .96f, .78f), 2.4f);
        SetAimVisible(false);
    }

    private void UpdateAimPreview()
    {
        UpdateAimPreview(desktopHead, desktopHead, "Q release");
    }

    private void UpdateAimPreview(Transform source, Transform viewer, string releaseHint)
    {
        if (aimArc == null) CreateAimPreview();
        if (aimArc == null || aimLabel == null || source == null || viewer == null) return;
        SetAimVisible(true);
        const int maxPoints = 48;
        const float step = .05f;
        aimArc.positionCount = maxPoints;
        Vector3 flatForward = Vector3.ProjectOnPlane(source.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < .01f) flatForward = transform.forward;
        float radians = selectedAngle * Mathf.Deg2Rad;
        Vector3 velocity = (flatForward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) * EffectiveThrowSpeed();
        Vector3 start = transform.position;
        Vector3 previous = start;
        bool foundLanding = false;
        RaycastHit landing = default;
        int usedPoints = 1;
        aimArc.SetPosition(0, start);
        for (int i = 1; i < maxPoints; i++)
        {
            float t = i * step;
            Vector3 next = start + velocity * t + Physics.gravity * (.5f * t * t);
            if (TryTrajectoryHit(previous, next, out landing))
            {
                aimArc.SetPosition(i, landing.point + landing.normal * .025f);
                usedPoints = i + 1;
                foundLanding = true;
                aimLandingTime = t;
                break;
            }
            if (Vector3.ProjectOnPlane(next - start, Vector3.up).magnitude >= MaximumPreviewDistance)
            {
                aimArc.SetPosition(i, next);
                usedPoints = i + 1;
                if (TryFindGroundBelow(next, out landing))
                {
                    foundLanding = true;
                    aimLandingTime = t;
                }
                break;
            }
            aimArc.SetPosition(i, next);
            usedPoints = i + 1;
            previous = next;
        }
        aimArc.positionCount = usedPoints;
        if (foundLanding && landing.normal.y < .45f)
            foundLanding = TryFindGroundBelow(landing.point, out landing);
        hasAimLanding = foundLanding && landing.normal.y >= .45f;
        if (hasAimLanding)
        {
            aimLandingPoint = landing.point;
            aimLandingNormal = landing.normal;
        }
        SetLandingRing(foundLanding, landing.point, landing.normal);
        aimLabel.text = releaseHint + " · " + Mathf.RoundToInt(selectedAngle) + "°" + (placeable ? " · place" : "");
        aimLabel.transform.position = start + Vector3.up * .45f;
        aimLabel.transform.rotation = Quaternion.LookRotation(aimLabel.transform.position - viewer.position, Vector3.up);
    }

    private void SetAimVisible(bool value)
    {
        if (aimArc != null) aimArc.enabled = value;
        if (landingRing != null && !value) landingRing.enabled = false;
        if (aimLabel != null) aimLabel.gameObject.SetActive(value);
    }

    private bool TryTrajectoryHit(Vector3 from, Vector3 to, out RaycastHit bestHit)
    {
        bestHit = default;
        Vector3 segment = to - from;
        float length = segment.magnitude;
        if (length < .001f) return false;
        float closest = float.PositiveInfinity;
        foreach (RaycastHit hit in Physics.RaycastAll(from, segment / length, length, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == null || hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            if (desktopAvatar != null && hit.transform.IsChildOf(desktopAvatar)) continue;
            if (hit.distance >= closest) continue;
            closest = hit.distance;
            bestHit = hit;
        }
        return closest < float.PositiveInfinity;
    }

    private float EffectiveThrowSpeed()
    {
        return Mathf.Clamp(throwSpeed, 5f, MaximumLaunchSpeed);
    }

    // Resolve a proper walkable surface for the target marker.  This second
    // ground query keeps the circle off walls, props and mid-air endpoints.
    private bool TryFindGroundBelow(Vector3 point, out RaycastHit bestHit)
    {
        bestHit = default;
        Vector3 origin = point + Vector3.up * 3f;
        float closest = float.PositiveInfinity;
        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 9f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == null || hit.transform == transform || hit.transform.IsChildOf(transform) || hit.normal.y < .45f) continue;
            if (desktopAvatar != null && hit.transform.IsChildOf(desktopAvatar)) continue;
            if (hit.distance >= closest) continue;
            closest = hit.distance;
            bestHit = hit;
        }
        return closest < float.PositiveInfinity;
    }

    private void SetLandingRing(bool visible, Vector3 point, Vector3 normal)
    {
        if (landingRing == null) return;
        landingRing.enabled = visible;
        if (!visible) return;
        Vector3 up = normal.sqrMagnitude > .01f ? normal.normalized : Vector3.up;
        Vector3 tangent = Vector3.Cross(up, Vector3.forward);
        if (tangent.sqrMagnitude < .01f) tangent = Vector3.Cross(up, Vector3.right);
        tangent.Normalize();
        Vector3 bitangent = Vector3.Cross(up, tangent).normalized;
        float pulse = 1.35f + Mathf.Sin(Time.time * 5f) * .035f;
        Vector3 center = point + up * .018f;
        for (int i = 0; i < landingRing.positionCount; i++)
        {
            float angle = i / (float)(landingRing.positionCount - 1) * Mathf.PI * 2f;
            landingRing.SetPosition(i, center + (tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle)) * pulse);
        }
    }

    void OnDestroy()
    {
        if (Held == this) Held = null;
        if (consumingItem == this) consumingItem = null;
        if (aimArc != null) Destroy(aimArc.gameObject);
        if (landingRing != null) Destroy(landingRing.gameObject);
        if (aimLabel != null) Destroy(aimLabel.gameObject);
        if (carryAnchor != null) Destroy(carryAnchor.gameObject);
        if (firstPersonAnchor != null) Destroy(firstPersonAnchor.gameObject);
        if (firstPersonForearm != null) Destroy(firstPersonForearm);
    }

    public static bool IsAnyHeld() => Held != null && Held.held;
}
