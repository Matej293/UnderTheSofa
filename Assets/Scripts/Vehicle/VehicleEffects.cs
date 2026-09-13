using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(VehicleGrounding), typeof(VehicleBoost))]
public sealed class VehicleEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody body;
    [SerializeField] private VehicleGrounding grounding;
    [SerializeField] private VehicleBoost boost;
    [SerializeField] private ParticleSystem dustParticles;
    [SerializeField] private ParticleSystem boostParticles;

    [Header("Ground Dust")]
    [SerializeField, Min(0f)] private float minimumDustSpeed = 2.5f;
    [SerializeField, Min(0.01f)] private float fullDustSpeed = 18f;
    [SerializeField, Min(0f)] private float maximumDustEmissionRate = 8f;
    [SerializeField] private Color dustColor = new(0.58f, 0.52f, 0.45f, 0.28f);
    [SerializeField] private Vector2 dustSizeRange = new(0.08f, 0.16f);
    [SerializeField] private Vector2 dustLifetimeRange = new(0.35f, 0.65f);

    [Header("Boost Streaks")]
    [SerializeField, Min(0f)] private float boostEmissionRate = 32f;
    [SerializeField] private Color boostColor = new(1f, 0.52f, 0.06f, 0.85f);
    [SerializeField] private Vector2 boostSizeRange = new(0.035f, 0.075f);
    [SerializeField] private Vector2 boostLifetimeRange = new(0.12f, 0.22f);

    private VehicleResetter resetter;

    private void Awake()
    {
        body ??= GetComponent<Rigidbody>();
        grounding ??= GetComponent<VehicleGrounding>();
        boost ??= GetComponent<VehicleBoost>();
        resetter = GetComponent<VehicleResetter>();

        ConfigureParticles(dustParticles, dustColor, dustSizeRange, dustLifetimeRange);
        ConfigureParticles(boostParticles, boostColor, boostSizeRange, boostLifetimeRange);
    }

    private void FixedUpdate()
    {
        bool controlsLocked = resetter != null && resetter.IsVehicleControlLocked;
        float speed = body != null ? body.linearVelocity.magnitude : 0f;

        float dustRate = 0f;
        if (!controlsLocked && grounding != null && grounding.IsGrounded && speed > minimumDustSpeed)
        {
            float speedRange = Mathf.Max(0.01f, fullDustSpeed - minimumDustSpeed);
            dustRate = Mathf.Clamp01((speed - minimumDustSpeed) / speedRange) * maximumDustEmissionRate;
        }

        SetEmissionRate(dustParticles, dustRate);

        bool shouldEmitBoost = !controlsLocked
            && grounding != null
            && grounding.IsGrounded
            && boost != null
            && boost.IsBoosting
            && boost.Energy > 0f;
        SetEmissionRate(boostParticles, shouldEmitBoost ? boostEmissionRate : 0f);
    }

    private static void ConfigureParticles(
        ParticleSystem particles,
        Color color,
        Vector2 sizeRange,
        Vector2 lifetimeRange)
    {
        if (particles == null)
        {
            return;
        }

        ParticleSystem.MainModule main = particles.main;
        main.startColor = color;
        main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeRange.x, lifetimeRange.y);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = true;

        SetEmissionRate(particles, 0f);
        particles.Play(true);
    }

    private static void SetEmissionRate(ParticleSystem particles, float rate)
    {
        if (particles == null)
        {
            return;
        }

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = rate > 0f;
        emission.rateOverTime = rate;
    }

    private void OnDisable()
    {
        dustParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        boostParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
