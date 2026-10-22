using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CapeInstantRechargePickup : MonoBehaviour
{
    [Min(0f)] public float amount = 2f;
    public bool singleUse = true;
    private bool collected;

    private void Awake() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        CapeEnergySystem energy = other.GetComponentInParent<CapeEnergySystem>();
        if (energy == null) return;
        energy.RestoreEnergy(amount);
        SkyQuestInput.Pulse(SkyQuestInput.Left, 0.3f, 0.08f);
        if (singleUse)
        {
            collected = true;
            gameObject.SetActive(false);
        }
    }
}
