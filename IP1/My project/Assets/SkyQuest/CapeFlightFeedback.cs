using UnityEngine;

public sealed class CapeFlightFeedback : MonoBehaviour
{
    private CapeFlightController flight;
    private CapeEnergySystem energy;
    private AudioSource audioSource;
    private AudioClip boostClip;
    private AudioClip deniedClip;
    private AudioClip rechargeClip;

    public void Initialize(CapeFlightController controller, CapeEnergySystem owner, Transform head)
    {
        flight = controller;
        energy = owner;
        audioSource = head.gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.32f;
        boostClip = Tone("Cape boost", 420f, 650f, 0.16f);
        deniedClip = Tone("Cape empty", 210f, 160f, 0.12f);
        rechargeClip = Tone("Cape full", 540f, 800f, 0.18f);
        flight.OnBoostSucceeded += Boosted;
        flight.OnBoostRejected += Rejected;
        energy.OnEnergyFull += Filled;
    }

    private void Boosted()
    {
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.48f, 0.12f);
        if (audioSource != null) audioSource.PlayOneShot(boostClip);
    }

    private void Rejected()
    {
        SkyQuestInput.Pulse(SkyQuestInput.Right, 0.12f, 0.035f);
        if (audioSource != null) audioSource.PlayOneShot(deniedClip);
    }

    private void Filled()
    {
        SkyQuestInput.Pulse(SkyQuestInput.Left, 0.2f, 0.08f);
        if (audioSource != null) audioSource.PlayOneShot(rechargeClip);
    }

    private static AudioClip Tone(string name, float start, float end, float duration)
    {
        const int sampleRate = 22050;
        int count = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create(name, count, 1, sampleRate, false);
        float[] samples = new float[count];
        float phase = 0f;
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            phase += 2f * Mathf.PI * Mathf.Lerp(start, end, t) / sampleRate;
            samples[i] = Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * t) * 0.35f;
        }
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        if (flight != null) { flight.OnBoostSucceeded -= Boosted; flight.OnBoostRejected -= Rejected; }
        if (energy != null) energy.OnEnergyFull -= Filled;
    }
}
