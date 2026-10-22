using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CapeRechargeZone : MonoBehaviour
{
    [Min(0f)] public float rechargePerSecond = 2f;
    public bool groundOnly;
    private readonly HashSet<CapeRechargeReceiver> receivers = new HashSet<CapeRechargeReceiver>();
    public Collider ZoneCollider => GetComponent<Collider>();

    private void Awake()
    {
        ZoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => Add(other);
    private void OnTriggerStay(Collider other) => Add(other);

    private void Add(Collider other)
    {
        CapeRechargeReceiver receiver = other.GetComponentInParent<CapeRechargeReceiver>();
        if (receiver == null || !receiver.isActiveAndEnabled) return;
        if (receivers.Add(receiver)) receiver.Register(this);
    }

    private void OnTriggerExit(Collider other)
    {
        CapeRechargeReceiver receiver = other.GetComponentInParent<CapeRechargeReceiver>();
        if (receiver != null && receivers.Remove(receiver)) receiver.Unregister(this);
    }

    private void OnDisable()
    {
        foreach (CapeRechargeReceiver receiver in receivers)
            if (receiver != null) receiver.Unregister(this);
        receivers.Clear();
    }
}
