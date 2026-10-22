using System.Collections.Generic;
using UnityEngine;

/// <summary>Combines overlapping sources by their highest applicable rate.</summary>
public sealed class CapeRechargeReceiver : MonoBehaviour
{
    private readonly HashSet<CapeRechargeZone> zones = new HashSet<CapeRechargeZone>();
    private CapeEnergySystem energy;
    private CapeFlightController flight;
    private float lastFeedback;

    public void Initialize(CapeEnergySystem owner, CapeFlightController controller)
    {
        energy = owner;
        flight = controller;
    }

    public void Register(CapeRechargeZone zone)
    {
        if (zone != null) zones.Add(zone);
    }

    public void Unregister(CapeRechargeZone zone)
    {
        zones.Remove(zone);
    }

    private void Update() => Step(Time.deltaTime);

    public void Step(float dt)
    {
        if (energy == null) return;
        float rate = 0f;
        zones.RemoveWhere(zone => zone == null || !zone.isActiveAndEnabled ||
            zone.ZoneCollider == null || !zone.ZoneCollider.enabled);
        foreach (CapeRechargeZone zone in zones)
        {
            if (zone.groundOnly && (flight == null || flight.State != CapeFlightState.Grounded)) continue;
            rate = Mathf.Max(rate, zone.rechargePerSecond);
        }
        if (rate <= 0f || energy.CurrentEnergy >= energy.maxEnergy) return;
        energy.RestoreEnergy(rate * Mathf.Max(0f, dt));
        if (Time.time - lastFeedback > 0.65f)
        {
            SkyQuestInput.Pulse(SkyQuestInput.Left, 0.10f, 0.04f);
            lastFeedback = Time.time;
        }
    }
}
