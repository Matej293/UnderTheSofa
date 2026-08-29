using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(VehicleGrounding))]
public sealed class VehicleStabilizer : MonoBehaviour
{
    [SerializeField] private Vector3 centerOfMass = new(0f, -0.22f, 0f);
    [SerializeField, Min(0f)] private float uprightTorque = 16f;
    [SerializeField, Min(0f)] private float groundedAngularDamping = 1.5f;

    private Rigidbody body;
    private VehicleGrounding grounding;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.centerOfMass = centerOfMass;
        grounding = GetComponent<VehicleGrounding>();
    }

    private void FixedUpdate()
    {
        if (!grounding.IsGrounded)
        {
            return;
        }

        Vector3 correctionAxis = Vector3.Cross(transform.up, grounding.GroundNormal);
        body.AddTorque(correctionAxis * uprightTorque, ForceMode.Acceleration);
        body.AddTorque(-body.angularVelocity * groundedAngularDamping, ForceMode.Acceleration);
    }
}
