using UnityEngine;

public sealed class ChaseCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody targetBody;
    [SerializeField, Min(0f)] private float closeDistance = 4.5f;
    [SerializeField, Min(0f)] private float farDistance = 7.5f;
    [SerializeField, Min(0.01f)] private float speedForFarDistance = 16f;
    [SerializeField] private float height = 2.1f;
    [SerializeField, Min(0.01f)] private float positionSmoothTime = 0.14f;
    [SerializeField, Min(0.01f)] private float rotationSharpness = 10f;

    private Vector3 positionVelocity;
    private Vector3 heading;

    private void Awake()
    {
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
        return target.position + Vector3.up * height - heading * distance;
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
