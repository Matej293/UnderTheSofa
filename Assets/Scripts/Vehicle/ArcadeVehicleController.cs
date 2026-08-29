using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(VehicleInput), typeof(VehicleGrounding))]
[RequireComponent(typeof(VehicleStats))]
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
    [SerializeField] private Transform rearDrivePoint;

    [Header("Steering")]
    [SerializeField, Min(0f)] private float minimumSteeringSpeed = 0.75f;
    [SerializeField, Min(0.01f)] private float lowSpeedSteeringFullSpeed = 4f;
    [SerializeField, Min(0f)] private float lowSpeedSteeringDegreesPerSecond = 280f;
    [SerializeField, Min(0f)] private float highSpeedSteeringDegreesPerSecond = 140f;
    [SerializeField, Range(0f, 45f)] private float visualSteeringAngle = 30f;
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;

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
    private VehicleStats stats;
    private Quaternion frontLeftWheelBaseRotation;
    private Quaternion frontRightWheelBaseRotation;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        input = GetComponent<VehicleInput>();
        grounding = GetComponent<VehicleGrounding>();
        stats = GetComponent<VehicleStats>();
        frontLeftWheel ??= transform.Find("Wheel_FL");
        frontRightWheel ??= transform.Find("Wheel_FR");
        frontLeftWheelBaseRotation = frontLeftWheel != null ? frontLeftWheel.localRotation : Quaternion.identity;
        frontRightWheelBaseRotation = frontRightWheel != null ? frontRightWheel.localRotation : Quaternion.identity;
    }

    private void Update()
    {
        float steeringAngle = input.Drive.x * visualSteeringAngle;
        if (frontLeftWheel != null)
        {
            frontLeftWheel.localRotation = frontLeftWheelBaseRotation * Quaternion.Euler(0f, steeringAngle, 0f);
        }

        if (frontRightWheel != null)
        {
            frontRightWheel.localRotation = frontRightWheelBaseRotation * Quaternion.Euler(0f, steeringAngle, 0f);
        }
    }

    private void FixedUpdate()
    {
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
        Vector3 planarVelocity = Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up);
        float forwardSpeed = Vector3.Dot(planarVelocity, transform.forward);
        float throttle = drive.y;

        bool handbrakeHeld = input.DriftHeld;
        bool driftActive = IsDrifting;
        float driveMultiplier = handbrakeHeld ? handbrakeThrottleMultiplier : 1f;
        if (throttle > 0f && forwardSpeed < topSpeed)
        {
            ApplyRearDriveForce(transform.forward * throttle * acceleration * stats.AccelerationMultiplier * driveMultiplier);
        }
        else if (throttle < 0f)
        {
            float force = forwardSpeed > 0.5f ? brakeAcceleration : reverseAcceleration;
            float speedLimit = forwardSpeed > 0.5f ? topSpeed : reverseSpeed;
            if (Mathf.Abs(forwardSpeed) < speedLimit)
            {
                ApplyRearDriveForce(transform.forward * throttle * force * stats.AccelerationMultiplier * driveMultiplier);
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
            body.MoveRotation(body.rotation * Quaternion.AngleAxis(turnAmount, Vector3.up));
        }

        Vector3 frontAxlePosition = transform.TransformPoint(0f, -0.22f, 0.72f);
        Vector3 rearAxlePosition = GetRearDrivePosition();
        float rearGrip = rearLateralGrip;
        if (driftActive)
        {
            float driftSpeed = Mathf.InverseLerp(driftEngageSpeed, driftFullSpeed, absoluteForwardSpeed);
            rearGrip = Mathf.Lerp(lowSpeedDriftRearGrip, highSpeedDriftRearGrip, driftSpeed);
        }

        ApplyAxleGrip(frontAxlePosition, frontLateralGrip);
        ApplyAxleGrip(rearAxlePosition, rearGrip);
        if (handbrakeHeld)
        {
            float rearForwardSpeed = Vector3.Dot(body.GetPointVelocity(rearAxlePosition), transform.forward);
            if (Mathf.Abs(rearForwardSpeed) > 0.01f)
            {
                float brakeFactor = Mathf.InverseLerp(0f, driftFullSpeed, Mathf.Abs(rearForwardSpeed));
                float brakeAcceleration = Mathf.Lerp(lowSpeedHandbrakeDeceleration, highSpeedHandbrakeDeceleration, brakeFactor);
                body.AddForceAtPosition(-transform.forward * Mathf.Sign(rearForwardSpeed) * brakeAcceleration, rearAxlePosition, ForceMode.Acceleration);
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
        ApplyRearDriveForce(transform.forward * accelerationForce * multiplier);
    }

    private bool IsDrifting => input != null
                               && input.DriftHeld
                               && Mathf.Abs(input.Drive.x) >= driftSteerThreshold
                               && Mathf.Abs(Vector3.Dot(Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up), transform.forward)) >= driftEngageSpeed;

    private Vector3 GetRearDrivePosition()
    {
        return rearDrivePoint != null
            ? rearDrivePoint.position
            : transform.TransformPoint(0f, -0.22f, -0.75f);
    }

    private void ApplyAxleGrip(Vector3 axlePosition, float grip)
    {
        float lateralSpeed = Vector3.Dot(body.GetPointVelocity(axlePosition), transform.right);
        body.AddForceAtPosition(-transform.right * lateralSpeed * grip, axlePosition, ForceMode.Acceleration);
    }

    private void ApplyAirControl(Vector2 drive, float roll)
    {
        Vector3 torque = transform.right * (drive.y * airPitchTorque)
                       + Vector3.up * (drive.x * airYawTorque)
                       + transform.forward * (-roll * airRollTorque);
        body.AddTorque(torque, ForceMode.Acceleration);
    }
}
