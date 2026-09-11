using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(VehicleInput), typeof(VehicleGrounding))]
public sealed class ArcadeVehicleController : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField, Min(0f)] private float acceleration = 45f;
    [SerializeField, Min(0f)] private float reverseAcceleration = 24f;
    [SerializeField, Min(0f)] private float brakeAcceleration = 65f;
    [SerializeField, Min(0f)] private float topSpeed = 22f;
    [SerializeField, Min(0f)] private float reverseSpeed = 9f;
    [SerializeField, Min(0f)] private float frontLateralGrip = 18f;
    [SerializeField, Min(0f)] private float rearLateralGrip = 14f;
    [SerializeField] private Vector3 frontAxleLocal = new(0f, -0.22f, 0.72f);
    [SerializeField] private Transform rearDrivePoint;

    [Header("Steering")]
    [SerializeField, Min(0f)] private float minimumSteeringSpeed = 0.75f;
    [SerializeField, Min(0.01f)] private float lowSpeedSteeringFullSpeed = 4f;
    [SerializeField, Min(0f)] private float lowSpeedSteeringDegreesPerSecond = 280f;
    [SerializeField, Min(0f)] private float highSpeedSteeringDegreesPerSecond = 140f;
    [SerializeField, Range(0f, 45f)] private float visualSteeringAngle = 30f;
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;
    [SerializeField, Min(0.01f)] private float visualWheelRadius = 0.28f;
    [SerializeField] private Transform cockpitModelRoot;
    [SerializeField, Min(0f)] private float cockpitSteeringWheelAngle = 110f;

    [Header("Drift")]
    [SerializeField, Min(0f)] private float driftEngageSpeed = 3f;
    [SerializeField, Min(0f)] private float driftFullSpeed = 18f;
    [SerializeField, Range(0f, 1f)] private float driftSteerThreshold = 0.25f;
    [SerializeField, Min(0f)] private float lowSpeedDriftRearGrip = 4f;
    [SerializeField, Min(0f)] private float highSpeedDriftRearGrip = 0.35f;
    [SerializeField, Range(0f, 1f)] private float handbrakeThrottleMultiplier = 0f;
    [SerializeField, Range(0f, 1f)] private float handbrakeBoostMultiplier = 0f;
    [SerializeField, Min(0f)] private float lowSpeedHandbrakeDeceleration = 7f;
    [SerializeField, Min(0f)] private float highSpeedHandbrakeDeceleration = 15f;

    [Header("Hop and Air")]
    [SerializeField, Min(0f)] private float hopImpulse = 5.5f;
    [SerializeField, Min(0f)] private float airPitchTorque = 1f;
    [SerializeField, Min(0f)] private float airYawTorque = 4f;
    [SerializeField, Min(0f)] private float airRollTorque = 4.5f;

    private Rigidbody body;
    private VehicleInput input;
    private VehicleGrounding grounding;
    private VehicleResetter resetter;
    private Quaternion frontLeftWheelBaseRotation;
    private Quaternion frontRightWheelBaseRotation;
    private Vector3 frontLeftWheelBasePosition;
    private Vector3 frontRightWheelBasePosition;
    private Vector3 frontLeftWheelPivotLocal;
    private Vector3 frontRightWheelPivotLocal;
    private Quaternion rearLeftWheelBaseRotation;
    private Quaternion rearRightWheelBaseRotation;
    private Vector3 rearLeftWheelBasePosition;
    private Vector3 rearRightWheelBasePosition;
    private Vector3 rearLeftWheelPivotLocal;
    private Vector3 rearRightWheelPivotLocal;
    private float visualWheelSpinAngle;
    private readonly List<CockpitSteeringWheelPart> cockpitSteeringWheelParts = new();
    private Vector3 cockpitSteeringWheelPivotLocal;
    private Vector3 cockpitSteeringWheelAxisLocal = Vector3.forward;

    private struct CockpitSteeringWheelPart
    {
        public Transform Transform;
        public Vector3 BasePosition;
        public Quaternion BaseRotation;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        input = GetComponent<VehicleInput>();
        grounding = GetComponent<VehicleGrounding>();
        resetter = GetComponent<VehicleResetter>();
        frontLeftWheel ??= transform.Find("WHEEL_FL");
        frontRightWheel ??= transform.Find("WHEEL_FR");
        rearLeftWheel ??= transform.Find("test/WHEEL_BL");
        rearRightWheel ??= transform.Find("test/WHEEL_BR");
        frontLeftWheelBaseRotation = frontLeftWheel != null ? frontLeftWheel.localRotation : Quaternion.identity;
        frontRightWheelBaseRotation = frontRightWheel != null ? frontRightWheel.localRotation : Quaternion.identity;
        frontLeftWheelBasePosition = frontLeftWheel != null ? frontLeftWheel.localPosition : Vector3.zero;
        frontRightWheelBasePosition = frontRightWheel != null ? frontRightWheel.localPosition : Vector3.zero;
        frontLeftWheelPivotLocal = GetWheelPivotLocal(frontLeftWheel);
        frontRightWheelPivotLocal = GetWheelPivotLocal(frontRightWheel);
        rearLeftWheelBaseRotation = rearLeftWheel != null ? rearLeftWheel.localRotation : Quaternion.identity;
        rearRightWheelBaseRotation = rearRightWheel != null ? rearRightWheel.localRotation : Quaternion.identity;
        rearLeftWheelBasePosition = rearLeftWheel != null ? rearLeftWheel.localPosition : Vector3.zero;
        rearRightWheelBasePosition = rearRightWheel != null ? rearRightWheel.localPosition : Vector3.zero;
        rearLeftWheelPivotLocal = GetWheelPivotLocal(rearLeftWheel);
        rearRightWheelPivotLocal = GetWheelPivotLocal(rearRightWheel);
        CacheCockpitSteeringWheel();
    }

    private void Update()
    {
        float steeringAngle = input.Drive.x * visualSteeringAngle;
        float forwardSpeed = Vector3.Dot(body.linearVelocity, transform.forward);
        visualWheelSpinAngle -= forwardSpeed / visualWheelRadius * Mathf.Rad2Deg * Time.deltaTime;
        visualWheelSpinAngle = Mathf.Repeat(visualWheelSpinAngle, 360f);

        ApplyWheelVisual(frontLeftWheel, frontLeftWheelBasePosition, frontLeftWheelBaseRotation, frontLeftWheelPivotLocal, steeringAngle);
        ApplyWheelVisual(frontRightWheel, frontRightWheelBasePosition, frontRightWheelBaseRotation, frontRightWheelPivotLocal, steeringAngle);
        ApplyWheelVisual(rearLeftWheel, rearLeftWheelBasePosition, rearLeftWheelBaseRotation, rearLeftWheelPivotLocal, 0f);
        ApplyWheelVisual(rearRightWheel, rearRightWheelBasePosition, rearRightWheelBaseRotation, rearRightWheelPivotLocal, 0f);
        ApplyCockpitSteeringWheel(-input.Drive.x * cockpitSteeringWheelAngle);
    }

    private void CacheCockpitSteeringWheel()
    {
        cockpitSteeringWheelParts.Clear();
        cockpitModelRoot ??= transform.Find("2001-acura-integra-type-r");
        if (cockpitModelRoot == null)
        {
            return;
        }

        Vector3 pivotWorld = Vector3.zero;
        Transform steeringWheelPlastic = null;
        foreach (Transform part in cockpitModelRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!part.name.StartsWith("acu_integrar_01_cockpit_steering_wheel_"))
            {
                continue;
            }

            cockpitSteeringWheelParts.Add(new CockpitSteeringWheelPart
            {
                Transform = part,
                BasePosition = part.localPosition,
                BaseRotation = part.localRotation
            });
            pivotWorld += GetMeshCenterWorld(part);

            if (part.name.EndsWith("steering_wheel_plastic_"))
            {
                steeringWheelPlastic = part;
            }
        }

        if (cockpitSteeringWheelParts.Count > 0)
        {
            cockpitSteeringWheelPivotLocal = transform.InverseTransformPoint(pivotWorld / cockpitSteeringWheelParts.Count);
            cockpitSteeringWheelAxisLocal = GetMeshNormalLocal(steeringWheelPlastic);
        }
    }

    private void ApplyCockpitSteeringWheel(float steeringAngle)
    {
        Vector3 pivotWorld = transform.TransformPoint(cockpitSteeringWheelPivotLocal);
        foreach (CockpitSteeringWheelPart part in cockpitSteeringWheelParts)
        {
            if (part.Transform == null)
            {
                continue;
            }

            part.Transform.localPosition = part.BasePosition;
            part.Transform.localRotation = part.BaseRotation;
            part.Transform.RotateAround(pivotWorld, transform.TransformDirection(cockpitSteeringWheelAxisLocal), steeringAngle);
        }
    }

    private Vector3 GetWheelPivotLocal(Transform wheel)
    {
        if (wheel == null)
        {
            return Vector3.zero;
        }

        return transform.InverseTransformPoint(GetMeshCenterWorld(wheel));
    }

    private static Vector3 GetMeshCenterWorld(Transform meshTransform)
    {
        MeshFilter meshFilter = meshTransform.GetComponent<MeshFilter>();
        return meshFilter != null && meshFilter.sharedMesh != null
            ? meshTransform.TransformPoint(meshFilter.sharedMesh.bounds.center)
            : meshTransform.position;
    }

    private Vector3 GetMeshNormalLocal(Transform meshTransform)
    {
        if (meshTransform == null)
        {
            return Vector3.forward;
        }

        MeshFilter meshFilter = meshTransform.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
        if (mesh == null)
        {
            return Vector3.forward;
        }

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        float largestNormalSqrMagnitude = 0f;
        Vector3 largestNormal = Vector3.zero;
        for (int triangle = 0; triangle < triangles.Length; triangle += 3)
        {
            Vector3 normal = Vector3.Cross(
                vertices[triangles[triangle + 1]] - vertices[triangles[triangle]],
                vertices[triangles[triangle + 2]] - vertices[triangles[triangle]]);
            if (normal.sqrMagnitude > largestNormalSqrMagnitude)
            {
                largestNormalSqrMagnitude = normal.sqrMagnitude;
                largestNormal = normal;
            }
        }

        if (largestNormalSqrMagnitude <= 0f)
        {
            return Vector3.forward;
        }

        Vector3 worldAxis = meshTransform.TransformDirection(largestNormal / Mathf.Sqrt(largestNormalSqrMagnitude));
        return transform.InverseTransformDirection(worldAxis).normalized;
    }

    private void ApplyWheelVisual(
        Transform wheel,
        Vector3 basePosition,
        Quaternion baseRotation,
        Vector3 pivotLocal,
        float steeringAngle)
    {
        if (wheel == null)
        {
            return;
        }

        wheel.localPosition = basePosition;
        wheel.localRotation = baseRotation;
        Vector3 wheelPivot = transform.TransformPoint(pivotLocal);
        wheel.RotateAround(wheelPivot, transform.up, steeringAngle);
        wheel.RotateAround(wheelPivot, wheel.right, visualWheelSpinAngle);
    }

    private void FixedUpdate()
    {
        if (resetter != null && resetter.IsVehicleControlLocked)
        {
            return;
        }

        Vector2 drive = input.Drive;
        if (grounding.IsGrounded)
        {
            ApplyGroundMovement(drive);
            if (input.ConsumeHop())
            {
                body.AddForce(Vector3.up * hopImpulse, ForceMode.Impulse);
            }
        }
        else
        {
            ApplyAirControl(drive, input.AirRoll);
        }
    }

    private void ApplyGroundMovement(Vector2 drive)
    {
        Vector3 groundNormal = grounding.GroundNormal;
        Vector3 surfaceForward = GetSurfaceForward(groundNormal);
        Vector3 surfaceRight = Vector3.Cross(groundNormal, surfaceForward).normalized;
        Vector3 planarVelocity = Vector3.ProjectOnPlane(body.linearVelocity, groundNormal);
        float forwardSpeed = Vector3.Dot(planarVelocity, surfaceForward);
        float throttle = drive.y;

        bool handbrakeHeld = input.DriftHeld;
        bool driftActive = IsDrifting;
        float driveMultiplier = handbrakeHeld ? handbrakeThrottleMultiplier : 1f;
        if (throttle > 0f && forwardSpeed < topSpeed)
        {
            ApplyRearDriveForce(surfaceForward * throttle * acceleration * driveMultiplier);
        }
        else if (throttle < 0f)
        {
            float force = forwardSpeed > 0.5f ? brakeAcceleration : reverseAcceleration;
            float speedLimit = forwardSpeed > 0.5f ? topSpeed : reverseSpeed;
            if (Mathf.Abs(forwardSpeed) < speedLimit)
            {
                ApplyRearDriveForce(surfaceForward * throttle * force * driveMultiplier);
            }
        }

        float absoluteForwardSpeed = Mathf.Abs(forwardSpeed);
        if (absoluteForwardSpeed >= minimumSteeringSpeed)
        {
            float speedFactor = Mathf.InverseLerp(minimumSteeringSpeed, topSpeed, absoluteForwardSpeed);
            float direction = forwardSpeed < 0f ? -1f : 1f;
            float turnRate = Mathf.Lerp(lowSpeedSteeringDegreesPerSecond, highSpeedSteeringDegreesPerSecond, speedFactor);
            float lowSpeedResponse = Mathf.InverseLerp(minimumSteeringSpeed, lowSpeedSteeringFullSpeed, absoluteForwardSpeed);
            turnRate *= lowSpeedResponse;
            float turnAmount = drive.x * turnRate * direction * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.AngleAxis(turnAmount, groundNormal));
        }

        Vector3 frontAxlePosition = transform.TransformPoint(frontAxleLocal);
        Vector3 rearAxlePosition = GetRearDrivePosition();
        float rearGrip = rearLateralGrip;
        if (driftActive)
        {
            float driftSpeed = Mathf.InverseLerp(driftEngageSpeed, driftFullSpeed, absoluteForwardSpeed);
            rearGrip = Mathf.Lerp(lowSpeedDriftRearGrip, highSpeedDriftRearGrip, driftSpeed);
        }

        ApplyAxleGrip(frontAxlePosition, surfaceRight, frontLateralGrip);
        ApplyAxleGrip(rearAxlePosition, surfaceRight, rearGrip);
        if (handbrakeHeld)
        {
            float rearForwardSpeed = Vector3.Dot(body.GetPointVelocity(rearAxlePosition), surfaceForward);
            if (Mathf.Abs(rearForwardSpeed) > 0.01f)
            {
                float brakeFactor = Mathf.InverseLerp(0f, driftFullSpeed, Mathf.Abs(rearForwardSpeed));
                float brakeAcceleration = Mathf.Lerp(lowSpeedHandbrakeDeceleration, highSpeedHandbrakeDeceleration, brakeFactor);
                body.AddForceAtPosition(-surfaceForward * Mathf.Sign(rearForwardSpeed) * brakeAcceleration, rearAxlePosition, ForceMode.Acceleration);
            }
        }
    }

    private void ApplyRearDriveForce(Vector3 force)
    {
        body.AddForceAtPosition(force, GetRearDrivePosition(), ForceMode.Acceleration);
    }

    public void ApplyBoostAcceleration(float accelerationForce)
    {
        float multiplier = input != null && input.DriftHeld ? handbrakeBoostMultiplier : 1f;
        Vector3 driveForward = grounding != null && grounding.IsGrounded
            ? GetSurfaceForward(grounding.GroundNormal)
            : transform.forward;
        ApplyRearDriveForce(driveForward * accelerationForce * multiplier);
    }

    private bool IsDrifting => input != null
                               && input.DriftHeld
                               && Mathf.Abs(input.Drive.x) >= driftSteerThreshold
                               && grounding != null
                               && grounding.IsGrounded
                               && Mathf.Abs(Vector3.Dot(
                                   Vector3.ProjectOnPlane(body.linearVelocity, grounding.GroundNormal),
                                   GetSurfaceForward(grounding.GroundNormal))) >= driftEngageSpeed;

    private Vector3 GetRearDrivePosition()
    {
        return rearDrivePoint != null
            ? rearDrivePoint.position
            : transform.TransformPoint(0f, -0.22f, -0.75f);
    }

    private Vector3 GetSurfaceForward(Vector3 groundNormal)
    {
        Vector3 surfaceForward = Vector3.ProjectOnPlane(transform.forward, groundNormal);
        return surfaceForward.sqrMagnitude > 0.001f ? surfaceForward.normalized : transform.forward;
    }

    private void ApplyAxleGrip(Vector3 axlePosition, Vector3 surfaceRight, float grip)
    {
        float lateralSpeed = Vector3.Dot(body.GetPointVelocity(axlePosition), surfaceRight);
        body.AddForceAtPosition(-surfaceRight * lateralSpeed * grip, axlePosition, ForceMode.Acceleration);
    }

    private void ApplyAirControl(Vector2 drive, float roll)
    {
        Vector3 torque = transform.right * (drive.y * airPitchTorque)
                       + Vector3.up * (drive.x * airYawTorque)
                       + transform.forward * (-roll * airRollTorque);
        body.AddTorque(torque, ForceMode.Acceleration);
    }
}
