using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(VehicleInput), typeof(VehicleGrounding))]
public sealed class VehicleResetter : MonoBehaviour
{
    private struct VehicleSnapshot
    {
        public float Time;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    private struct SafePose
    {
        public float Time;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 GroundNormal;
    }

    [Header("Rewind")]
    [SerializeField, Min(0.1f)] private float rewindSeconds = 3f;
    [SerializeField, Min(0.01f)] private float rewindPlaybackDuration = 1.5f;

    [Header("Fall Recovery")]
    [SerializeField, Range(0f, 1f)] private float minimumUprightDot = 0.8f;
    [SerializeField, Range(0f, 1f)] private float minimumGroundUpDot = 0.65f;
    [SerializeField] private float minimumSafeHeight = -2f;
    [SerializeField] private float fallResetHeight = -8f;
    [SerializeField, Min(0f)] private float maximumSafeSpeed = 12f;
    [SerializeField, Min(0f)] private float safePoseUpdateInterval = 0.25f;
    [SerializeField] private float resetLift = 0.4f;
    [SerializeField] private ChaseCamera chaseCamera;

    [Header("Unstuck")]
    [SerializeField, Min(0f)] private float unstuckMinimumBacktrackDistance = 1.5f;
    [SerializeField, Range(0f, 1f)] private float unstuckMinimumGroundUpDot = 0.85f;
    [SerializeField, Min(0f)] private float unstuckClearance = 0.5f;
    [SerializeField, Min(0f)] private float unstuckCooldown = 0.5f;

    private Rigidbody body;
    private VehicleInput input;
    private VehicleGrounding grounding;
    private Vector3 safePosition;
    private Quaternion safeRotation;
    private bool hasSafePose;
    private float nextSafePoseUpdateTime;
    private readonly List<VehicleSnapshot> rewindHistory = new();
    private readonly List<SafePose> safePoseHistory = new();
    private Collider[] vehicleColliders;
    private bool[] colliderEnabledStates;
    private bool collisionsEnabledBeforeRewind;
    private float rewindStartTime;
    private float rewindEndTime;
    private float rewindElapsed;
    private VehicleSnapshot rewindEndpoint;
    private float controlLockUntilFixedTime;
    private float nextUnstuckTime;
    private SafePose initialSafePose;

    public bool IsRewinding { get; private set; }
    public bool IsVehicleControlLocked => IsRewinding || Time.fixedTime <= controlLockUntilFixedTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        input = GetComponent<VehicleInput>();
        grounding = GetComponent<VehicleGrounding>();
        vehicleColliders = GetComponentsInChildren<Collider>(true);
        colliderEnabledStates = new bool[vehicleColliders.Length];
        safePosition = transform.position;
        safeRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        initialSafePose = new SafePose
        {
            Time = Time.fixedTime,
            Position = safePosition,
            Rotation = safeRotation,
            GroundNormal = Vector3.up
        };
    }

    private void FixedUpdate()
    {
        if (IsRewinding)
        {
            UpdateRewind();
            return;
        }

        RecordSnapshot();

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
            RecordSafePose(safePosition, safeRotation, groundNormal);
            nextSafePoseUpdateTime = Time.time + safePoseUpdateInterval;
        }

        if (input.ConsumeUnstuck())
        {
            TryUnstuck();
        }
        else if (input.ConsumeReset())
        {
            BeginRewind();
        }
        else if (transform.position.y < fallResetHeight)
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
        rewindHistory.Clear();
        RecordSnapshot();
    }

    private void RecordSafePose(Vector3 position, Quaternion rotation, Vector3 groundNormal)
    {
        safePoseHistory.Add(new SafePose
        {
            Time = Time.fixedTime,
            Position = position,
            Rotation = rotation,
            GroundNormal = groundNormal
        });

        float oldestRequiredTime = Time.fixedTime - rewindSeconds - Time.fixedDeltaTime;
        while (safePoseHistory.Count > 1 && safePoseHistory[1].Time < oldestRequiredTime)
        {
            safePoseHistory.RemoveAt(0);
        }
    }

    private void TryUnstuck()
    {
        if (Time.fixedTime < nextUnstuckTime)
        {
            return;
        }

        float minimumDistanceSqr = unstuckMinimumBacktrackDistance * unstuckMinimumBacktrackDistance;
        for (int index = safePoseHistory.Count - 1; index >= 0; index--)
        {
            SafePose candidate = safePoseHistory[index];
            if (Vector3.Dot(candidate.GroundNormal, Vector3.up) < unstuckMinimumGroundUpDot)
            {
                continue;
            }

            Vector3 offset = candidate.Position - body.position;
            offset.y = 0f;
            if (offset.sqrMagnitude >= minimumDistanceSqr)
            {
                MoveToSafePose(candidate);
                return;
            }
        }

        MoveToSafePose(initialSafePose);
    }

    private void MoveToSafePose(SafePose pose)
    {
        input.ClearQueuedActions();
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = pose.Position + Vector3.up * unstuckClearance;
        body.rotation = pose.Rotation;
        body.WakeUp();
        chaseCamera?.SnapToTarget();

        nextUnstuckTime = Time.fixedTime + unstuckCooldown;
        controlLockUntilFixedTime = Time.fixedTime + Time.fixedDeltaTime;
        rewindHistory.Clear();
        RecordSnapshot();
    }

    private void RecordSnapshot()
    {
        rewindHistory.Add(new VehicleSnapshot
        {
            Time = Time.fixedTime,
            Position = body.position,
            Rotation = body.rotation
        });

        float oldestRequiredTime = Time.fixedTime - rewindSeconds - Time.fixedDeltaTime;
        while (rewindHistory.Count > 1 && rewindHistory[1].Time < oldestRequiredTime)
        {
            rewindHistory.RemoveAt(0);
        }
    }

    private void BeginRewind()
    {
        if (rewindHistory.Count == 0)
        {
            return;
        }

        rewindStartTime = rewindHistory[^1].Time;
        rewindEndTime = Mathf.Max(rewindHistory[0].Time, rewindStartTime - rewindSeconds);
        rewindEndpoint = GetSnapshotAt(rewindEndTime);
        rewindElapsed = 0f;
        IsRewinding = true;

        input.ClearQueuedActions();
        collisionsEnabledBeforeRewind = body.detectCollisions;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.isKinematic = true;
        body.detectCollisions = false;
        SetVehicleCollidersEnabled(false);
    }

    private void UpdateRewind()
    {
        rewindElapsed += Time.fixedDeltaTime;
        float progress = Mathf.Clamp01(rewindElapsed / rewindPlaybackDuration);
        float sampleTime = Mathf.Lerp(rewindStartTime, rewindEndTime, progress);
        VehicleSnapshot snapshot = GetSnapshotAt(sampleTime);
        body.position = snapshot.Position;
        body.rotation = snapshot.Rotation;

        if (progress >= 1f)
        {
            CompleteRewind();
        }
    }

    private VehicleSnapshot GetSnapshotAt(float sampleTime)
    {
        for (int index = 1; index < rewindHistory.Count; index++)
        {
            VehicleSnapshot next = rewindHistory[index];
            if (next.Time < sampleTime)
            {
                continue;
            }

            VehicleSnapshot previous = rewindHistory[index - 1];
            float interval = next.Time - previous.Time;
            float progress = interval > 0f ? Mathf.Clamp01((sampleTime - previous.Time) / interval) : 0f;
            return new VehicleSnapshot
            {
                Time = sampleTime,
                Position = Vector3.Lerp(previous.Position, next.Position, progress),
                Rotation = Quaternion.Slerp(previous.Rotation, next.Rotation, progress)
            };
        }

        return rewindHistory[^1];
    }

    private void CompleteRewind()
    {
        body.position = rewindEndpoint.Position;
        body.rotation = rewindEndpoint.Rotation;
        body.detectCollisions = collisionsEnabledBeforeRewind;
        SetVehicleCollidersEnabled(true);
        body.isKinematic = false;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.WakeUp();

        IsRewinding = false;
        controlLockUntilFixedTime = Time.fixedTime + Time.fixedDeltaTime;
        input.ClearQueuedActions();
        rewindHistory.Clear();
        RecordSnapshot();
    }

    private void SetVehicleCollidersEnabled(bool enabled)
    {
        for (int index = 0; index < vehicleColliders.Length; index++)
        {
            Collider collider = vehicleColliders[index];
            if (collider == null)
            {
                continue;
            }

            if (!enabled)
            {
                colliderEnabledStates[index] = collider.enabled;
                collider.enabled = false;
            }
            else
            {
                collider.enabled = colliderEnabledStates[index];
            }
        }
    }
}
