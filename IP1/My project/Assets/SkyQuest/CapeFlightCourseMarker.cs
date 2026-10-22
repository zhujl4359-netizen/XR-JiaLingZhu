using UnityEngine;

public sealed class CapeFlightCourseMarker : MonoBehaviour
{
    public bool finish;
    public Vector3 checkpointPosition;
    private bool activated;

    public void ResetMarker() => activated = false;

    private void Awake() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        CapeFlightCourseRuntime course = other.GetComponentInParent<CapeFlightCourseRuntime>();
        if (course == null || activated) return;
        activated = true;
        if (finish) course.Finish();
        else course.ReachCheckpoint(checkpointPosition);
    }
}
