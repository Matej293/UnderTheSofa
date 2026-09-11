using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(VehicleGrounding), typeof(VehicleInput))]
public sealed class VehicleObstacleAssist : MonoBehaviour
{
    [Header("Low Edge Assist")]
    [SerializeField] private Vector3 forwardProbeLocal = new(0.79f, -0.12f, 3f);
    [SerializeField, Min(0.01f)] private float probeRadius = 0.16f;
    [SerializeField, Min(0.01f)] private float probeDistance = 0.45f;
    [SerializeField, Min(0f)] private float minimumForwardSpeed = 1.5f;
    [SerializeField, Min(0f)] private float maximumForwardSpeed = 12f;
    [SerializeField, Min(0f)] private float maximumObstacleHeight = 0.35f;
    [SerializeField, Range(0f, 1f)] private float maximumFaceUpDot = 0.3f;
    [SerializeField, Range(0f, 1f)] private float minimumFrontFaceDot = 0.5f;
    [SerializeField, Range(0f, 1f)] private float minimumTopUpDot = 0.7f;
    [SerializeField, Min(0f)] private float topSurfaceForwardOffset = 0.12f;
    [SerializeField, Min(0f)] private float topSurfaceClearance = 0.1f;
    [SerializeField, Min(0f)] private float upwardVelocityChange = 1.8f;
    [SerializeField, Min(0f)] private float assistCooldown = 0.2f;

    private Rigidbody body;
    private VehicleGrounding grounding;
    private VehicleInput input;
    private VehicleResetter resetter;
    private float nextAssistTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grounding = GetComponent<VehicleGrounding>();
        input = GetComponent<VehicleInput>();
        resetter = GetComponent<VehicleResetter>();
    }

    private void FixedUpdate()
    {
        if ((resetter != null && resetter.IsVehicleControlLocked)
            || Time.fixedTime < nextAssistTime
            || !grounding.IsGrounded
            || input.DriftHeld
            || input.Drive.y <= 0.1f)
        {
            return;
        }

        float forwardSpeed = Vector3.Dot(Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up), transform.forward);
        if (forwardSpeed < minimumForwardSpeed || forwardSpeed > maximumForwardSpeed || !TryGetObstacleFace(out RaycastHit faceHit))
        {
            return;
        }

        if (Vector3.Dot(faceHit.normal, Vector3.up) > maximumFaceUpDot
            || Vector3.Dot(-faceHit.normal, transform.forward) < minimumFrontFaceDot
            || !TryGetObstacleTop(faceHit, out RaycastHit topHit)
            || Vector3.Dot(topHit.normal, Vector3.up) < minimumTopUpDot)
        {
            return;
        }

        float obstacleHeight = topHit.point.y - grounding.GroundHit.point.y;
        if (obstacleHeight < 0f || obstacleHeight > maximumObstacleHeight)
        {
            return;
        }

        body.AddForce(Vector3.up * upwardVelocityChange, ForceMode.VelocityChange);
        nextAssistTime = Time.fixedTime + assistCooldown;
    }

    private bool TryGetObstacleFace(out RaycastHit closestHit)
    {
        Vector3 origin = transform.TransformPoint(forwardProbeLocal);
        return TryGetNearestHit(
            Physics.SphereCastAll(origin, probeRadius, transform.forward, probeDistance, grounding.GroundLayers, QueryTriggerInteraction.Ignore),
            out closestHit);
    }

    private bool TryGetObstacleTop(RaycastHit faceHit, out RaycastHit topHit)
    {
        Vector3 origin = faceHit.point + transform.forward * topSurfaceForwardOffset
                       + Vector3.up * (maximumObstacleHeight + topSurfaceClearance);
        float distance = maximumObstacleHeight + topSurfaceClearance;
        return TryGetNearestHit(
            Physics.RaycastAll(origin, Vector3.down, distance, grounding.GroundLayers, QueryTriggerInteraction.Ignore),
            out topHit);
    }

    private bool TryGetNearestHit(RaycastHit[] hits, out RaycastHit closestHit)
    {
        closestHit = default;
        float closestDistance = float.PositiveInfinity;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform) || hit.distance >= closestDistance)
            {
                continue;
            }

            closestHit = hit;
            closestDistance = hit.distance;
        }

        return closestDistance < float.PositiveInfinity;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.TransformPoint(forwardProbeLocal);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, probeRadius);
        Gizmos.DrawLine(origin, origin + transform.forward * probeDistance);
    }
}
