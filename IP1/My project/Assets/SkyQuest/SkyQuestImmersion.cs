using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Runtime immersion layer: positional audio, low-cost environmental motion,
// tactile lantern/candle interactions and a comfort-friendly optional teleport mode.
public class SkyQuestImmersion : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public SkyQuestLocomotion locomotion;
    public SkyQuestSanctuary sanctuary;
    public SkyQuestStatus status;
    public bool desktopMode;
    public bool enableProceduralAudio = true;
    public bool enablePhysicalInteractions = true;

    private readonly List<Transform> swayObjects = new List<Transform>();
    private readonly List<Vector3> swayBase = new List<Vector3>();
    private readonly List<GameObject> lanterns = new List<GameObject>();
    private readonly List<GameObject> candles = new List<GameObject>();
    private readonly List<bool> candleLit = new List<bool>();
    private AudioSource windSource;
    private AudioSource waterSource;
    private AudioSource fireSource;
    private AudioSource ancestorSource;
    private ParticleSystem cloudParticles;
    private TextMeshPro comfortHint;
    private bool previousLeftTrigger;
    private float leftTriggerStarted;
    private bool leftTriggerStartedWithItem;
    private bool previousTeleportToggle;
    private bool previousCalibrate;
    private float nextHaptic;
    private float nextTeleport;
    private Vector3 gateStart;
    private bool teleportMode;
    private bool previousTeleportTrigger;

    void Start()
    {
        if (head == null) return;
        FindSwayObjects();
        if (enableProceduralAudio) CreateAudioField();
        CreateCloudParticles();
        CreatePhysicalObjects();
        if (sanctuary != null && sanctuary.Gate != null) gateStart = sanctuary.Gate.position;
        if (!desktopMode)
            comfortHint = SkyQuestVisuals.Text("Comfort hint", "", head,
                new Vector3(0f, -0.38f, 1.05f), 1.4f,
                new Color(0.78f, 0.9f, 1f), 3.5f);
        ShowComfortHint();
    }

    void Update()
    {
        if (head == null) return;
        AnimateEnvironment();
        UpdateAudio();
        UpdateCloudParticles();
        if (comfortHint != null && locomotion != null)
            comfortHint.gameObject.SetActive(desktopMode || !locomotion.ThirdPerson);
        if (!desktopMode && enablePhysicalInteractions) UpdatePhysicalInteractions();
        UpdateComfortTeleport();
    }

    private void FindSwayObjects()
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            string n = t.name.ToLowerInvariant();
            if (!n.Contains("banner") && !n.Contains("cloth") && !n.Contains("lantern") && !n.Contains("candle")) continue;
            if (t.IsChildOf(transform)) continue;
            swayObjects.Add(t);
            swayBase.Add(t.localPosition);
            if (swayObjects.Count >= 36) break;
        }
    }

    private void AnimateEnvironment()
    {
        float wind = Mathf.Sin(Time.time * 0.72f) * 0.035f;
        for (int i = 0; i < swayObjects.Count; i++)
        {
            Transform t = swayObjects[i];
            if (t == null) continue;
            t.localPosition = swayBase[i] + new Vector3(wind * Mathf.Sin(i * 1.7f),
                wind * 0.25f, wind * Mathf.Cos(i * 1.3f));
            t.localRotation = Quaternion.Euler(0f, wind * 18f, wind * 24f);
        }
    }

    private void CreateAudioField()
    {
        windSource = MakeLoop("Spatial wind", head.position + new Vector3(-6f, 1.6f, 8f),
            MakeNoise("Wind", 1.4f, 0.12f), 0.22f, 10f, 42f);
        waterSource = MakeLoop("Spatial water", head.position + new Vector3(7f, 0.2f, 4f),
            MakeNoise("Water", 1.1f, 0.08f), 0.18f, 4f, 22f);
        fireSource = MakeLoop("Spatial candle fire", head.position + new Vector3(2f, 1.2f, 6f),
            MakeTone("Fire", 188f, 1.1f, 0.06f), 0.16f, 3f, 15f);
        ancestorSource = MakeLoop("Spatial ancestor hum", head.position + new Vector3(0f, 1.5f, -5f),
            MakeTone("Ancestor", 132f, 2f, 0.045f), 0.12f, 3f, 18f);
        GameObject zoneObject = new GameObject("Sanctuary room acoustics");
        zoneObject.transform.position = head.position + Vector3.forward * 4f;
        AudioReverbZone zone = zoneObject.AddComponent<AudioReverbZone>();
        zone.reverbPreset = AudioReverbPreset.Hallway;
        zone.minDistance = 5f;
        zone.maxDistance = 24f;
    }

    private AudioSource MakeLoop(string name, Vector3 position, AudioClip clip, float volume,
        float minDistance, float maxDistance)
    {
        GameObject sourceObject = new GameObject(name);
        sourceObject.transform.position = position;
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = true;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.spread = 65f;
        source.Play();
        return source;
    }

    private AudioClip MakeTone(string name, float frequency, float seconds, float amplitude)
    {
        const int rate = 22050;
        int samples = Mathf.CeilToInt(rate * seconds);
        AudioClip clip = AudioClip.Create(name, samples, 1, rate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)rate;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip MakeNoise(string name, float seconds, float amplitude)
    {
        const int rate = 22050;
        int samples = Mathf.CeilToInt(rate * seconds);
        AudioClip clip = AudioClip.Create(name, samples, 1, rate, false);
        float[] data = new float[samples];
        float filtered = 0f;
        for (int i = 0; i < samples; i++)
        {
            filtered = Mathf.Lerp(filtered, Random.Range(-1f, 1f), 0.08f);
            data[i] = filtered * amplitude;
        }
        clip.SetData(data, 0);
        return clip;
    }

    private void CreateCloudParticles()
    {
        GameObject particleObject = new GameObject("Cloud flight particles");
        particleObject.transform.SetParent(head, false);
        particleObject.transform.localPosition = new Vector3(0f, 0f, 2.4f);
        cloudParticles = particleObject.AddComponent<ParticleSystem>();
        var main = cloudParticles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = 0.7f;
        main.startSpeed = 0.35f;
        main.startSize = 0.018f;
        main.startColor = new Color(0.72f, 0.9f, 1f, 0.34f);
        main.maxParticles = 28;
        var emission = cloudParticles.emission;
        emission.rateOverTime = 0f;
        var shape = cloudParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.8f;
        // Use the same Quest-compatible glow as the cape. The default particle
        // material renders as magenta squares in the Android/URP flight view.
        Material capeGlow = Resources.Load<Material>("SkyCapeGlow");
        if (capeGlow != null)
            particleObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = capeGlow;
        cloudParticles.Stop();
    }

    private void UpdateCloudParticles()
    {
        if (cloudParticles == null || locomotion == null) return;
        var emission = cloudParticles.emission;
        bool visible = locomotion.IsFlying && (desktopMode || !locomotion.ThirdPerson);
        emission.rateOverTime = visible ? 18f : 0f;
        if (visible && !cloudParticles.isPlaying) cloudParticles.Play();
        if (!visible && cloudParticles.isPlaying) cloudParticles.Stop();
    }

    private void CreatePhysicalObjects()
    {
        if (sanctuary != null && sanctuary.beacons != null)
        {
            for (int i = 0; i < sanctuary.beacons.Length; i++)
            {
                Transform beacon = sanctuary.beacons[i];
                if (beacon == null) continue;
                GameObject lantern = CreateGlowObject("Touchable lantern " + (i + 1),
                    beacon.position + Vector3.up * 0.42f, new Color(0.35f, 0.8f, 1f), 0.22f);
                lantern.GetComponent<SkyQuestPhysicalObject>().index = i;
                // Lanterns share the exact same distant trigger pickup and
                // ballistic release as every ancestor reward and display jar.
                SphereCollider solid = lantern.GetComponent<SphereCollider>();
                if (solid != null) solid.isTrigger = false;
                SkyQuestMagicPickup pickup = lantern.AddComponent<SkyQuestMagicPickup>();
                pickup.desktopHead = head;
                pickup.desktopHand = rightHand;
                pickup.desktopAvatar = locomotion != null && locomotion.avatar != null ?
                    locomotion.avatar.transform : null;
                pickup.desktopMode = desktopMode;
                pickup.displayPottery = true;
                pickup.AnchorOnDisplay();
                lanterns.Add(lantern);
            }
        }
        Vector3[] candlePositions = { new Vector3(30f, 1.05f, 12f), new Vector3(44f, 1.05f, 12f), new Vector3(55f, 1.05f, -4f) };
        for (int i = 0; i < candlePositions.Length; i++)
        {
            GameObject candle = CreateGlowObject("Touchable candle " + (i + 1), candlePositions[i],
                new Color(1f, 0.65f, 0.22f), 0.08f);
            candle.GetComponent<SkyQuestPhysicalObject>().index = i;
            candle.GetComponent<SkyQuestPhysicalObject>().candle = true;
            candles.Add(candle);
            candleLit.Add(false);
        }
    }

    private GameObject CreateGlowObject(string name, Vector3 position, Color color, float size)
    {
        GameObject obj = SkyQuestVisuals.Primitive(name, PrimitiveType.Sphere, null, position,
            Vector3.one * size, color, true);
        SphereCollider collider = obj.GetComponent<SphereCollider>();
        if (collider != null) { collider.isTrigger = true; collider.radius = 1.6f; }
        SkyQuestPhysicalObject physical = obj.AddComponent<SkyQuestPhysicalObject>();
        physical.owner = this;
        return obj;
    }

    private void UpdatePhysicalInteractions()
    {
        if (leftHand == null || rightHand == null) return;
        bool leftTrigger = SkyQuestInput.Trigger(SkyQuestInput.Left) > 0.7f;
        if (leftTrigger && !previousLeftTrigger)
        {
            leftTriggerStarted = Time.unscaledTime;
            leftTriggerStartedWithItem = SkyQuestMagicPickup.Held != null;
        }
        bool shortTap = !leftTrigger && previousLeftTrigger &&
            Time.unscaledTime - leftTriggerStarted < .6f && !leftTriggerStartedWithItem;
        for (int i = 0; i < candles.Count && shortTap; i++)
        {
            if (candleLit[i] || candles[i] == null) continue;
            if (Vector3.Distance(leftHand.position, candles[i].transform.position) > 0.42f) continue;
            candleLit[i] = true;
            candles[i].GetComponent<Renderer>().material.color = new Color(1f, 0.95f, 0.55f);
            status?.Show("Candle lit. A warm light answers your hand.", 3f);
            SkyQuestInput.Pulse(SkyQuestInput.Left, 0.3f, 0.1f);
        }
        UpdateGatePush();
        previousLeftTrigger = leftTrigger;
    }

    private void UpdateGatePush()
    {
        if (sanctuary == null || sanctuary.Gate == null) return;
        float gateDistance = Vector3.Distance((leftHand.position + rightHand.position) * 0.5f,
            sanctuary.Gate.position);
        bool pushing = gateDistance < 2.6f && SkyQuestInput.Grip(SkyQuestInput.Left) > 0.65f &&
            SkyQuestInput.Grip(SkyQuestInput.Right) > 0.65f;
        if (!pushing) return;
        sanctuary.Gate.position = Vector3.MoveTowards(sanctuary.Gate.position,
            gateStart + Vector3.up * 4.3f, 0.85f * Time.deltaTime);
        if (Time.time > nextHaptic)
        {
            status?.Show("Stone gate moved by both hands.", 2f);
            SkyQuestInput.Pulse(SkyQuestInput.Left, 0.25f, 0.08f);
            SkyQuestInput.Pulse(SkyQuestInput.Right, 0.25f, 0.08f);
            nextHaptic = Time.time + 0.5f;
        }
    }

    private void UpdateAudio()
    {
        if (windSource == null) return;
        float flying = locomotion != null && locomotion.IsFlying ? 1f : 0.45f;
        windSource.volume = 0.18f + flying * 0.12f;
        fireSource.volume = 0.12f + (Mathf.Sin(Time.time * 2.3f) + 1f) * 0.018f;
    }

    private void UpdateComfortTeleport()
    {
        bool toggle = false;
        bool calibrate = false;
        bool toggleHeld = false;
        bool calibrateHeld = false;
        if (desktopMode)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null) { toggle = keyboard.tKey.wasPressedThisFrame; calibrate = keyboard.f1Key.wasPressedThisFrame; }
        }
        else
        {
            toggleHeld = SkyQuestInput.Grip(SkyQuestInput.Left) > 0.75f &&
                SkyQuestInput.Secondary(SkyQuestInput.Right);
            calibrateHeld = SkyQuestInputExtensions.StickClick(SkyQuestInput.Left) &&
                SkyQuestInputExtensions.StickClick(SkyQuestInput.Right);
            toggle = toggleHeld && !previousTeleportToggle;
            calibrate = calibrateHeld && !previousCalibrate;
        }
        if (toggle)
        {
            teleportMode = !teleportMode;
            status?.Show(teleportMode ?
                (desktopMode ? "Teleport ON: look at the ground and press V." :
                    "Teleport ON: aim at the ground and press right trigger.") :
                "Comfort teleport OFF.", 4f);
        }
        if (calibrate)
        {
            if (head != null) transform.position -= Vector3.up * (head.localPosition.y - 1.65f);
            status?.Show("Seated height calibrated.", 3f);
        }
        previousTeleportToggle = toggleHeld;
        previousCalibrate = calibrateHeld;
        bool vrTrigger = !desktopMode && SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        bool vrFire = vrTrigger && !previousTeleportTrigger;
        previousTeleportTrigger = vrTrigger;
        if (!teleportMode || locomotion == null || locomotion.IsFlying) return;
        if (desktopMode)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.vKey.wasPressedThisFrame && Time.time > nextTeleport &&
                Physics.Raycast(head.position, head.forward, out RaycastHit desktopHit, 12f,
                    ~0, QueryTriggerInteraction.Ignore) && desktopHit.normal.y > 0.65f)
                TeleportTo(desktopHit.point);
        }
        else if (rightHand != null && vrFire && Time.time > nextTeleport &&
            Physics.Raycast(rightHand.position, rightHand.forward, out RaycastHit hit, 12f,
                ~0, QueryTriggerInteraction.Ignore) && hit.normal.y > 0.65f)
        {
            TeleportTo(hit.point);
            SkyQuestInput.Pulse(SkyQuestInput.Right, 0.35f, 0.08f);
        }
        ShowComfortHint();
    }

    private void TeleportTo(Vector3 destination)
    {
        transform.position = destination + Vector3.up * 0.05f -
            (desktopMode ? Vector3.zero :
                Vector3.ProjectOnPlane(head.position - transform.position, Vector3.up));
        nextTeleport = Time.time + 0.5f;
        status?.Show("Teleported.", 2f);
    }

    private void ShowComfortHint()
    {
        if (comfortHint == null) return;
        comfortHint.text = "Left grip+B: teleport  •  both stick clicks: calibrate";
    }
}

public class SkyQuestPhysicalObject : MonoBehaviour
{
    [HideInInspector] public SkyQuestImmersion owner;
    [HideInInspector] public int index;
    [HideInInspector] public bool candle;
}
