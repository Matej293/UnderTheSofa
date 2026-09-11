using UnityEngine;

public sealed class VehicleGrounding : MonoBehaviour
{
    [SerializeField] private Transform groundCheck;
    [SerializeField, Min(0.01f)] private float castRadius = 0.28f;
    [SerializeField, Min(0.01f)] private float castDistance = 0.75f;
    [SerializeField, Range(0f, 89f)] private float maximumGroundSlope = 55f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Collider vehicleCollider;

    public bool IsGrounded { get; private set; }
    public RaycastHit GroundHit { get; private set; }
    public Vector3 GroundNormal => IsGrounded ? GroundHit.normal : Vector3.up;
    public LayerMask GroundLayers => groundLayers;

    private void Awake()
    {
        vehicleCollider = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        Vector3 origin = GetCastOrigin();
        RaycastHit[] hits = Physics.SphereCastAll(origin, castRadius, Vector3.down, GetCastDistance(), groundLayers, QueryTriggerInteraction.Ignore);

        IsGrounded = false;
        float closestDistance = float.PositiveInfinity;
        float minimumGroundUpDot = Mathf.Cos(maximumGroundSlope * Mathf.Deg2Rad);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform)
                || Vector3.Dot(hit.normal, Vector3.up) < minimumGroundUpDot
                || hit.distance >= closestDistance)
            {
                continue;
            }

            GroundHit = hit;
            closestDistance = hit.distance;
            IsGrounded = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = GetCastOrigin();
        Gizmos.color = IsGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(origin, castRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * GetCastDistance());
    }

    private Vector3 GetCastOrigin()
    {
        if (vehicleCollider != null)
        {
            Bounds bounds = vehicleCollider.bounds;
            return bounds.center + Vector3.up * 0.6f;
        }

        return (groundCheck != null ? groundCheck.position : transform.position) + Vector3.up * 0.6f;
    }

    private float GetCastDistance()
    {
        return vehicleCollider != null
            ? vehicleCollider.bounds.extents.y + 0.6f + castDistance
            : castDistance;
    }
}
