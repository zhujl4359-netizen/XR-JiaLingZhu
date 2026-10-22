using TMPro;
using UnityEngine;

public class SkyQuestWorld : MonoBehaviour
{
    public SkyQuestLocomotion locomotion;
    public SkyQuestStatus status;
    public Transform head;
    public Transform ancestor;
    public int EnergyCollected { get; private set; }

    private readonly Vector3[] windOffsets =
    {
        new Vector3(-4f, 0f, -10f), new Vector3(3f, 0f, -18f), new Vector3(-5f, 0f, -26f)
    };
    private readonly Vector3[] energyOffsets =
    {
        new Vector3(0f, 2f, -5f), new Vector3(-3f, 3f, -11f),
        new Vector3(2f, 4f, -16f), new Vector3(-2f, 3f, -21f),
        new Vector3(1f, 4f, -26f), new Vector3(-1f, 2f, -31f)
    };
    private GameObject[] energyOrbs;
    private bool[] collected;
    private Vector3[] windPositions;
    private TextMeshPro counter;
    private float nextWindMessage;
    private float nextWindHaptic;

    void Start()
    {
        Vector3 origin = transform.position;
        windPositions = new Vector3[windOffsets.Length];
        energyOrbs = new GameObject[energyOffsets.Length];
        collected = new bool[energyOffsets.Length];
        for (int i = 0; i < windOffsets.Length; i++)
        {
            windPositions[i] = origin + windOffsets[i];
            GameObject zone = SkyQuestVisuals.Primitive("Wind current " + (i + 1),
                PrimitiveType.Sphere, null, windPositions[i] + Vector3.up * 1.5f,
                Vector3.one * 0.35f, new Color(0.35f, 0.8f, 1f), true);
            SkyQuestVisuals.RemoveCollider(zone);
            if (locomotion != null && !locomotion.desktopControlled)
            {
                GameObject recharge = new GameObject("Cape light recharge trigger " + (i + 1));
                recharge.transform.position = windPositions[i] + Vector3.up * 1.5f;
                SphereCollider trigger = recharge.AddComponent<SphereCollider>();
                trigger.radius = 2.2f;
                trigger.isTrigger = true;
                recharge.AddComponent<CapeRechargeZone>().rechargePerSecond = 2f;
            }
        }
        for (int i = 0; i < energyOffsets.Length; i++)
        {
            energyOrbs[i] = SkyQuestVisuals.Primitive("Sky energy " + (i + 1),
                PrimitiveType.Sphere, null, origin + energyOffsets[i],
                Vector3.one * 0.25f, new Color(1f, 0.9f, 0.4f), true);
            SkyQuestVisuals.RemoveCollider(energyOrbs[i]);
        }
        if (head != null && locomotion != null && locomotion.desktopControlled)
            counter = SkyQuestVisuals.Text("Sky light counter", "Sky lights 0 / 6", head,
                new Vector3(0f, 0.37f, 1.2f), 2f, Color.white, 0.9f);
    }

    void Update()
    {
        if (head == null || locomotion == null) return;
        for (int i = 0; i < windPositions.Length; i++)
        {
            Vector3 playerPosition = locomotion.desktopControlled ? transform.position : head.position;
            Vector2 distance = new Vector2(playerPosition.x - windPositions[i].x,
                playerPosition.z - windPositions[i].z);
            if (distance.magnitude < 2.2f && locomotion.IsFlying)
            {
                locomotion.AddWindLift(3f * Time.deltaTime);
                if (Time.time > nextWindHaptic)
                {
                    SkyQuestInput.Pulse(SkyQuestInput.Left, 0.14f, 0.06f);
                    SkyQuestInput.Pulse(SkyQuestInput.Right, 0.14f, 0.06f);
                    nextWindHaptic = Time.time + 0.55f;
                }
                if (Time.time > nextWindMessage)
                {
                    status?.Show(locomotion.desktopControlled ? "Wind current lifts you." :
                        "Wind current lifts you and recharges your cape.");
                    nextWindMessage = Time.time + 5f;
                }
            }
        }
        for (int i = 0; i < energyOrbs.Length; i++)
        {
            if (collected[i] || energyOrbs[i] == null) continue;
            energyOrbs[i].transform.Rotate(Vector3.up, 40f * Time.deltaTime, Space.World);
            Vector3 playerPosition = locomotion.desktopControlled ?
                transform.position + Vector3.up * 1.2f : head.position;
            if (Vector3.Distance(playerPosition, energyOrbs[i].transform.position) > 0.9f) continue;
            collected[i] = true;
            EnergyCollected++;
            Destroy(energyOrbs[i]);
            if (!locomotion.desktopControlled)
                GetComponent<CapeEnergySystem>()?.RestoreEnergy(2f);
            if (counter != null) counter.text = "Sky lights " + EnergyCollected + " / " + energyOrbs.Length;
            status?.Show("Sky energy collected: " + EnergyCollected + " / " + energyOrbs.Length);
            SkyQuestInput.Pulse(SkyQuestInput.Left, 0.35f, 0.08f);
        }
    }
}
