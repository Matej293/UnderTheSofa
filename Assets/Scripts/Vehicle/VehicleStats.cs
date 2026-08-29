using UnityEngine;

public sealed class VehicleStats : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float accelerationMultiplier = 1f;

    public float AccelerationMultiplier => accelerationMultiplier;

    public void AddAccelerationMultiplier(float amount)
    {
        accelerationMultiplier += amount;
    }
}
