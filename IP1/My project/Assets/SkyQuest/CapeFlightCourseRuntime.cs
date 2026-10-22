using UnityEngine;

/// <summary>Checkpoint and recovery for the standalone flight practice scene.</summary>
public sealed class CapeFlightCourseRuntime : MonoBehaviour
{
    public Vector3 startingCheckpoint = new Vector3(0f, 0.55f, 0f);
    public float recoveryHeight = -3.4f;
    private Vector3 checkpoint;
    private CapeFlightController flight;
    private CapeEnergySystem energy;
    private float nextRecovery;
    private bool finished;
    private bool suppressSafeStop;

    private void Start()
    {
        checkpoint = startingCheckpoint;
        flight = GetComponent<CapeFlightController>();
        energy = GetComponent<CapeEnergySystem>();
        if (flight != null) flight.OnSafeStop += SafeStop;
    }

    private void Update()
    {
        if (transform.position.y < recoveryHeight && Time.time >= nextRecovery)
            ReturnToCheckpoint();
    }

    public void ReachCheckpoint(Vector3 position)
    {
        checkpoint = position;
        Debug.Log("Cape flight checkpoint reached.");
    }

    public void Finish()
    {
        finished = true;
        GameObject sign = GameObject.Find("Finish instructions");
        if (sign != null && sign.TryGetComponent(out TextMesh text))
            text.text = "COURSE COMPLETE\nR CLICK: RESTART";
        Debug.Log("Cape flight course completed. Right stick click restarts the course.");
        energy?.RestoreEnergy(energy.maxEnergy);
    }

    private void SafeStop()
    {
        if (suppressSafeStop) return;
        if (finished)
        {
            finished = false;
            checkpoint = startingCheckpoint;
            foreach (CapeFlightCourseMarker marker in FindObjectsByType<CapeFlightCourseMarker>(FindObjectsSortMode.None))
                marker.ResetMarker();
            GameObject sign = GameObject.Find("Finish instructions");
            if (sign != null && sign.TryGetComponent(out TextMesh text))
                text.text = "LAND HERE TO COMPLETE\nFALLS RETURN TO CHECKPOINT";
            ReturnToCheckpoint();
            return;
        }
        if (transform.position.y < -1f) ReturnToCheckpoint();
    }

    private void ReturnToCheckpoint()
    {
        nextRecovery = Time.time + 1f;
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        transform.position = checkpoint;
        if (controller != null) controller.enabled = true;
        suppressSafeStop = true;
        flight?.SafeStop();
        suppressSafeStop = false;
        energy?.RestoreEnergy(energy.maxEnergy);
        Debug.Log("Cape flight recovery: returned to checkpoint with full energy.");
    }

    private void OnDestroy()
    {
        if (flight != null) flight.OnSafeStop -= SafeStop;
    }
}
