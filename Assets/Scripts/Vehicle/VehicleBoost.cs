using UnityEngine;

[RequireComponent(typeof(VehicleInput), typeof(ArcadeVehicleController))]
public sealed class VehicleBoost : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float capacity = 4f;
    [SerializeField, Min(0f)] private float boostAcceleration = 38f;
    [SerializeField, Min(0f)] private float drainPerSecond = 1f;
    [SerializeField, Min(0f)] private float rechargePerSecond = 0.6f;
    [SerializeField] private bool startFull = true;

    private VehicleInput input;
    private ArcadeVehicleController vehicle;
    private VehicleResetter resetter;

    public float Energy { get; private set; }
    public float NormalizedEnergy => capacity > 0f ? Energy / capacity : 0f;
    public bool IsBoosting { get; private set; }

    private void Awake()
    {
        input = GetComponent<VehicleInput>();
        vehicle = GetComponent<ArcadeVehicleController>();
        resetter = GetComponent<VehicleResetter>();
        Energy = startFull ? capacity : 0f;
    }

    private void FixedUpdate()
    {
        if (resetter != null && resetter.IsVehicleControlLocked)
        {
            IsBoosting = false;
            return;
        }

        if (input.BoostHeld)
        {
            IsBoosting = Energy > 0f;
            if (IsBoosting)
            {
                Energy = Mathf.Max(0f, Energy - drainPerSecond * Time.fixedDeltaTime);
                vehicle.ApplyBoostAcceleration(boostAcceleration);
            }
        }
        else
        {
            IsBoosting = false;
            Energy = Mathf.Min(capacity, Energy + rechargePerSecond * Time.fixedDeltaTime);
        }
    }
}
