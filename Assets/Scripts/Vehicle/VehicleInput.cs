using UnityEngine;
using UnityEngine.InputSystem;

public sealed class VehicleInput : MonoBehaviour
{
    [SerializeField] private InputActionAsset controls;

    private InputActionMap vehicleMap;
    private InputAction driveAction;
    private InputAction hopAction;
    private InputAction driftAction;
    private InputAction boostAction;
    private InputAction resetAction;
    private InputAction airRollAction;
    private bool hopQueued;
    private bool resetQueued;

    public Vector2 Drive => driveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public float AirRoll => airRollAction?.ReadValue<float>() ?? 0f;
    public bool DriftHeld => driftAction != null && driftAction.ReadValue<float>() > 0.5f;
    public bool BoostHeld => boostAction != null && boostAction.ReadValue<float>() > 0.5f;

    private void Awake()
    {
        CacheActions();
    }

    private void OnEnable()
    {
        CacheActions();
        vehicleMap?.Enable();
    }

    private void OnDisable()
    {
        vehicleMap?.Disable();
    }

    private void CacheActions()
    {
        if (controls == null)
        {
            return;
        }

        vehicleMap = controls.FindActionMap("Vehicle", true);
        driveAction = vehicleMap.FindAction("Drive", true);
        hopAction = vehicleMap.FindAction("Hop", true);
        driftAction = vehicleMap.FindAction("Drift", true);
        boostAction = vehicleMap.FindAction("Boost", true);
        resetAction = vehicleMap.FindAction("Reset", true);
        airRollAction = vehicleMap.FindAction("AirRoll", true);
    }

    private void Update()
    {
        hopQueued |= hopAction != null && hopAction.WasPressedThisFrame();
        resetQueued |= resetAction != null && resetAction.WasPressedThisFrame();
    }

    public bool ConsumeHop()
    {
        bool pressed = hopQueued;
        hopQueued = false;
        return pressed;
    }

    public bool ConsumeReset()
    {
        bool pressed = resetQueued;
        resetQueued = false;
        return pressed;
    }
}
