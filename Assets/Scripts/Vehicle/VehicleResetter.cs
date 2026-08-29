using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(VehicleInput), typeof(VehicleGrounding))]
public sealed class VehicleResetter : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float minimumUprightDot = 0.8f;
    [SerializeField, Range(0f, 1f)] private float minimumGroundUpDot = 0.65f;
    [SerializeField] private float minimumSafeHeight = -2f;
    [SerializeField] private float fallResetHeight = -8f;
    [SerializeField, Min(0f)] private float maximumSafeSpeed = 12f;
    [SerializeField, Min(0f)] private float safePoseUpdateInterval = 0.25f;
    [SerializeField] private float resetLift = 0.4f;
    [SerializeField] private ChaseCamera chaseCamera;

    private Rigidbody body;
    private VehicleInput input;
    private VehicleGrounding grounding;
    private Vector3 safePosition;
    private Quaternion safeRotation;
    private bool hasSafePose;
    private float nextSafePoseUpdateTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        input = GetComponent<VehicleInput>();
        grounding = GetComponent<VehicleGrounding>();
        safePosition = transform.position;
        safeRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
    }

    private void FixedUpdate()
    {
        if (Time.time >= nextSafePoseUpdateTime && CanRecordSafePose())
        {
            Vector3 groundNormal = grounding.GroundNormal;
            Vector3 safeForward = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
            if (safeForward.sqrMagnitude < 0.001f)
            {
                safeForward = Vector3.ProjectOnPlane(Vector3.forward, groundNormal).normalized;
            }

            safePosition = grounding.GroundHit.point + groundNormal * resetLift;
            safeRotation = Quaternion.LookRotation(safeForward, groundNormal);
            hasSafePose = true;
            nextSafePoseUpdateTime = Time.time + safePoseUpdateInterval;
        }

        if (input.ConsumeReset() || transform.position.y < fallResetHeight)
        {
            ResetVehicle();
        }
    }

    private bool CanRecordSafePose()
    {
        return grounding.IsGrounded
            && grounding.GroundHit.collider != null
            && !grounding.GroundHit.collider.isTrigger
            && transform.position.y >= minimumSafeHeight
            && Vector3.Dot(grounding.GroundNormal, Vector3.up) >= minimumGroundUpDot
            && Vector3.Dot(transform.up, grounding.GroundNormal) >= minimumUprightDot
            && body.linearVelocity.sqrMagnitude <= maximumSafeSpeed * maximumSafeSpeed;
    }

    public void ResetVehicle()
    {
        body.position = safePosition;
        body.rotation = safeRotation;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.WakeUp();
        chaseCamera?.SnapToTarget();
    }
}
