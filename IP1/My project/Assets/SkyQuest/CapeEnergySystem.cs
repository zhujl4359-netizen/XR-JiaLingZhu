using System;
using UnityEngine;

/// <summary>Owns the cape's energy. Flight physics never changes this value directly.</summary>
public sealed class CapeEnergySystem : MonoBehaviour
{
    [Min(0.01f)] public float maxEnergy = 15f;
    [Min(0f)] public float initialEnergy = 15f;
    [Min(0.01f)] public float energyCostPerBoost = 1f;
    public bool allowPartialBoost;
    [Range(0f, 1f)] public float lowEnergyThreshold = 0.2f;

    public float CurrentEnergy { get; private set; }
    public float Normalized => maxEnergy > 0f ? CurrentEnergy / maxEnergy : 0f;
    public event Action<float, float> OnEnergyChanged;
    public event Action OnEnergyDepleted;
    public event Action OnEnergyFull;
    public event Action OnLowEnergy;

    private void Awake() => ResetEnergy();

    public bool CanBoost() => allowPartialBoost ? CurrentEnergy > 0f : CurrentEnergy >= energyCostPerBoost;

    public bool TryConsumeBoostEnergy()
    {
        if (!CanBoost()) return false;
        SetEnergy(CurrentEnergy - (allowPartialBoost ? Mathf.Min(CurrentEnergy, energyCostPerBoost) : energyCostPerBoost));
        return true;
    }

    public void RestoreEnergy(float amount)
    {
        if (amount > 0f) SetEnergy(CurrentEnergy + amount);
    }

    public float GetCurrentEnergy() => CurrentEnergy;
    public float GetEnergyNormalized() => Normalized;
    public void ResetEnergy() => SetEnergy(initialEnergy);

    public void SetMaximumEnergy(float value)
    {
        maxEnergy = Mathf.Max(0.01f, value);
        SetEnergy(CurrentEnergy);
    }

    private void SetEnergy(float value)
    {
        float previous = CurrentEnergy;
        CurrentEnergy = Mathf.Clamp(value, 0f, maxEnergy);
        if (Mathf.Approximately(previous, CurrentEnergy)) return;
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
        if (CurrentEnergy <= 0f) OnEnergyDepleted?.Invoke();
        if (CurrentEnergy >= maxEnergy) OnEnergyFull?.Invoke();
        if (previous > maxEnergy * lowEnergyThreshold && CurrentEnergy <= maxEnergy * lowEnergyThreshold)
            OnLowEnergy?.Invoke();
    }
}
