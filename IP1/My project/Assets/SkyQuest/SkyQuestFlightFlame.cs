using UnityEngine;

// A single collectible in the sanctuary flight challenge.
public sealed class SkyQuestFlightFlame : MonoBehaviour
{
    public float collectRadius = 1.15f;
    public float bobHeight = 0.14f;
    public float bobSpeed = 1.8f;
    private Vector3 restingPosition;
    private float phase;
    private Transform[] wisps;
    private Vector3[] wispScales;
    private Quaternion[] wispRotations;

    private void Awake()
    {
        restingPosition = transform.position;
        phase = transform.GetSiblingIndex() * 0.7f;
        MeshFilter[] parts = GetComponentsInChildren<MeshFilter>(true);
        int count = 0;
        foreach (MeshFilter part in parts)
            if (part.name.Contains("tongue") || part.name.Contains("wisp")) count++;
        wisps = new Transform[count];
        wispScales = new Vector3[count];
        wispRotations = new Quaternion[count];
        int index = 0;
        foreach (MeshFilter part in parts)
        {
            if (!part.name.Contains("tongue") && !part.name.Contains("wisp")) continue;
            wisps[index] = part.transform;
            wispScales[index] = part.transform.localScale;
            wispRotations[index] = part.transform.localRotation;
            index++;
        }
    }

    private void Update()
    {
        transform.position = restingPosition + Vector3.up *
            (Mathf.Sin(Time.time * bobSpeed + phase) * bobHeight);
        transform.Rotate(Vector3.up, 24f * Time.deltaTime, Space.World);
        for (int i = 0; i < wisps.Length; i++)
        {
            float pulse = Mathf.Sin(Time.time * (2.3f + i * .19f) + phase + i * 1.4f);
            wisps[i].localScale = wispScales[i] * (1f + pulse * .065f);
            wisps[i].localRotation = wispRotations[i] * Quaternion.Euler(0f, 0f, pulse * 5f);
        }
    }
}
