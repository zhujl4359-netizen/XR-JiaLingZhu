using UnityEngine;

// Shared proximity interactions for the keyboard and Quest scenes.
public class SkyQuestSanctuary : MonoBehaviour
{
    public Transform[] beacons;
    public Transform gate;
    public Transform reward;
    public float activationDistance = 1.45f;
    public float completionDistance = 2.2f;

    private Transform head;
    private SkyQuestLocomotion locomotion;
    private SkyQuestStatus status;
    private bool[] lit;
    private int litCount;
    private bool completed;
    private Vector3 gateClosedPosition;
    private Material[] beaconMaterials;

    public bool IsComplete => completed;
    public Transform Gate => gate;

    void Start()
    {
        Camera camera = Camera.main;
        head = camera != null ? camera.transform : null;
        locomotion = FindFirstObjectByType<SkyQuestLocomotion>();
        status = FindFirstObjectByType<SkyQuestStatus>();
        lit = new bool[beacons != null ? beacons.Length : 0];
        beaconMaterials = new Material[lit.Length];
        for (int i = 0; i < lit.Length; i++)
        {
            Renderer renderer = beacons[i] != null ? beacons[i].GetComponent<Renderer>() : null;
            if (renderer != null) beaconMaterials[i] = renderer.material;
        }
        if (gate != null) gateClosedPosition = gate.position;
        if (reward != null) reward.gameObject.SetActive(false);
    }

    void Update()
    {
        if (head == null) return;
        for (int i = 0; i < lit.Length; i++)
        {
            if (lit[i] || beacons[i] == null ||
                Vector3.Distance(PlayerPosition, beacons[i].position) > activationDistance) continue;
            ActivateBeacon(i, "proximity");
        }
        if (gate != null && lit.Length > 0 && litCount == lit.Length)
        {
            gate.position = Vector3.MoveTowards(gate.position,
                gateClosedPosition + Vector3.up * 4.3f, 1.4f * Time.deltaTime);
            if (reward != null && !reward.gameObject.activeSelf) reward.gameObject.SetActive(true);
            if (!completed && reward != null &&
                Vector3.Distance(PlayerPosition, reward.position) < completionDistance)
            {
                completed = true;
                status?.Show("Sanctuary complete — the sky remembers your light.", 8f);
                SkyQuestInput.Pulse(SkyQuestInput.Left, 0.45f, 0.18f);
                SkyQuestInput.Pulse(SkyQuestInput.Right, 0.45f, 0.18f);
            }
        }
        if (reward != null && reward.gameObject.activeSelf)
            reward.Rotate(Vector3.up, 24f * Time.deltaTime, Space.World);
    }

    private Vector3 PlayerPosition => locomotion != null && locomotion.desktopControlled ?
        locomotion.transform.position + Vector3.up * 1.2f : head.position;

    public bool ActivateBeacon(int index, string source = "interaction")
    {
        if (index < 0 || index >= lit.Length || lit[index] || beacons[index] == null) return false;
        lit[index] = true;
        litCount++;
        if (beaconMaterials[index] != null)
        {
            Color gold = new Color(1f, 0.75f, 0.29f);
            beaconMaterials[index].color = gold;
            beaconMaterials[index].EnableKeyword("_EMISSION");
            beaconMaterials[index].SetColor("_EmissionColor", gold * 2f);
        }
        status?.Show("Light awakened: " + litCount + " / " + lit.Length, 5f);
        SkyQuestInput.Pulse(SkyQuestInput.Left, 0.22f, 0.09f);
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.22f, 0.09f);
        return true;
    }
}
