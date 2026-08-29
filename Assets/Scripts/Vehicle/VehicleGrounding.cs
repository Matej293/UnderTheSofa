using UnityEngine;

public sealed class VehicleGrounding : MonoBehaviour
{
    [SerializeField] private Transform groundCheck;
    [SerializeField, Min(0.01f)] private float castRadius = 0.28f;
    [SerializeField, Min(0.01f)] private float castDistance = 0.75f;
    [SerializeField] private LayerMask groundLayers = ~0;

    public bool IsGrounded { get; private set; }
    public RaycastHit GroundHit { get; private set; }
    public Vector3 GroundNormal => IsGrounded ? GroundHit.normal : Vector3.up;

    private void FixedUpdate()
    {
        Vector3 origin = (groundCheck != null ? groundCheck.position : transform.position) + Vector3.up * 0.6f;
        RaycastHit[] hits = Physics.SphereCastAll(origin, castRadius, Vector3.down, castDistance, groundLayers, QueryTriggerInteraction.Ignore);

        IsGrounded = false;
        float closestDistance = float.PositiveInfinity;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform) || hit.distance >= closestDistance)
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
        Vector3 origin = (groundCheck != null ? groundCheck.position : transform.position) + Vector3.up * 0.6f;
        Gizmos.color = IsGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(origin, castRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * castDistance);
    }
}
