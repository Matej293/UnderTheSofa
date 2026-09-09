using UnityEngine;

public sealed class ChaseCamera : MonoBehaviour
{
    private const int MaxObstructionHits = 16;

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody targetBody;

    [Header("Distance")]
    [SerializeField, Min(0f)] private float closeDistance = 4.5f;
    [SerializeField, Min(0f)] private float farDistance = 7.5f;
    [SerializeField, Min(0.01f)] private float speedForFarDistance = 16f;
    [SerializeField, Min(0f)] private float boostDistanceBonus = 1f;
    [SerializeField, Min(0f)] private float airborneDistanceBonus = 1.25f;
    [SerializeField] private float height = 2.1f;

    [Header("Obstruction")]
    [SerializeField] private LayerMask obstructionLayers = ~0;
    [SerializeField, Min(0.01f)] private float obstructionRadius = 0.25f;
    [SerializeField, Min(0f)] private float obstructionClearance = 0.1f;
    [SerializeField, Min(0f)] private float minimumDistance = 1.25f;

    [Header("Smoothing")]
    [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.14f;
    [SerializeField, Min(0.01f)] private float rotationSharpness = 10f;

    private Vector3 positionVelocity;
    private Vector3 heading;
    private VehicleGrounding grounding;
    private VehicleBoost boost;
    private readonly RaycastHit[] obstructionHits = new RaycastHit[MaxObstructionHits];

    private void Awake()
    {
        if (targetBody != null)
        {
            grounding = targetBody.GetComponent<VehicleGrounding>();
            boost = targetBody.GetComponent<VehicleBoost>();
        }

        if (target != null)
        {
            heading = Flatten(target.forward);
        }
    }

    private void LateUpdate()
    {
        Follow(Time.deltaTime);
    }

    public void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        heading = Flatten(target.forward);
        Vector3 desiredPosition = GetDesiredPosition();
        transform.SetPositionAndRotation(desiredPosition, GetLookRotation(desiredPosition));
        positionVelocity = Vector3.zero;
    }

    private void Follow(float deltaTime)
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetHeading = Flatten(target.forward);
        heading = Vector3.Slerp(heading, targetHeading, 1f - Mathf.Exp(-rotationSharpness * deltaTime));
        Vector3 desiredPosition = GetDesiredPosition();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, positionSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, GetLookRotation(transform.position), 1f - Mathf.Exp(-rotationSharpness * deltaTime));
    }

    private Vector3 GetDesiredPosition()
    {
        float speed = targetBody != null ? targetBody.linearVelocity.magnitude : 0f;
        float distance = Mathf.Lerp(closeDistance, farDistance, Mathf.Clamp01(speed / speedForFarDistance));
        if (boost != null && boost.IsBoosting)
        {
            distance += boostDistanceBonus;
        }

        if (grounding != null && !grounding.IsGrounded)
        {
            distance += airborneDistanceBonus;
        }

        Vector3 pivot = target.position + Vector3.up * height;
        Vector3 desiredPosition = pivot - heading * distance;
        return GetObstructionAdjustedPosition(pivot, desiredPosition);
    }

    private Vector3 GetObstructionAdjustedPosition(Vector3 pivot, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - pivot;
        float desiredDistance = direction.magnitude;
        if (desiredDistance <= 0.001f)
        {
            return desiredPosition;
        }

        direction /= desiredDistance;
        int hitCount = Physics.SphereCastNonAlloc(
            pivot,
            obstructionRadius,
            direction,
            obstructionHits,
            desiredDistance,
            obstructionLayers,
            QueryTriggerInteraction.Ignore);

        float closestHitDistance = float.PositiveInfinity;
        for (int index = 0; index < hitCount; index++)
        {
            RaycastHit hit = obstructionHits[index];
            if (hit.collider == null || IsTargetCollider(hit.collider))
            {
                continue;
            }

            closestHitDistance = Mathf.Min(closestHitDistance, hit.distance);
        }

        if (float.IsPositiveInfinity(closestHitDistance))
        {
            return desiredPosition;
        }

        float correctedDistance = Mathf.Max(minimumDistance, closestHitDistance - obstructionClearance);
        return pivot + direction * Mathf.Min(correctedDistance, desiredDistance);
    }

    private bool IsTargetCollider(Collider collider)
    {
        return targetBody != null && collider.transform.IsChildOf(targetBody.transform);
    }

    private Quaternion GetLookRotation(Vector3 cameraPosition)
    {
        Vector3 lookDirection = target.position - cameraPosition;
        return lookDirection.sqrMagnitude > 0.001f ? Quaternion.LookRotation(lookDirection, Vector3.up) : transform.rotation;
    }

    private static Vector3 Flatten(Vector3 direction)
    {
        Vector3 flat = Vector3.ProjectOnPlane(direction, Vector3.up);
        return flat.sqrMagnitude > 0.001f ? flat.normalized : Vector3.forward;
    }
}
