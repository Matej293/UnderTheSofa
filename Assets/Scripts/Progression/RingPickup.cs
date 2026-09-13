using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class RingPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;
    [SerializeField, Min(0f)] private float spinSpeed = 0f;

    [Header("Collection Feedback")]
    [SerializeField] private Collider pickupTrigger;
    [SerializeField] private Renderer[] pickupVisuals;
    [SerializeField] private ParticleSystem ambientParticles;
    [SerializeField] private ParticleSystem pickupBurst;
    [SerializeField] private AudioSource pickupAudio;

    private bool isCollected;

    private void Awake()
    {
        if (pickupTrigger == null)
        {
            pickupTrigger = GetComponent<Collider>();
        }
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected)
        {
            return;
        }

        RingWallet wallet = other.GetComponentInParent<RingWallet>();
        if (wallet == null)
        {
            return;
        }

        isCollected = true;
        if (pickupTrigger != null)
        {
            pickupTrigger.enabled = false;
        }

        SetPickupVisualsVisible(false);
        wallet.AddRings(value);

        ambientParticles?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        pickupBurst?.Play(true);
        pickupAudio?.Play();

        StartCoroutine(DeactivateAfterFeedback());
    }

    private void SetPickupVisualsVisible(bool visible)
    {
        if (pickupVisuals == null)
        {
            return;
        }

        foreach (Renderer pickupVisual in pickupVisuals)
        {
            if (pickupVisual != null)
            {
                pickupVisual.enabled = visible;
            }
        }
    }

    private IEnumerator DeactivateAfterFeedback()
    {
        float waitDuration = 0f;
        if (pickupBurst != null)
        {
            ParticleSystem.MainModule main = pickupBurst.main;
            waitDuration = main.duration + main.startLifetime.constantMax;
        }

        if (pickupAudio != null && pickupAudio.clip != null)
        {
            waitDuration = Mathf.Max(waitDuration, pickupAudio.clip.length);
        }

        if (waitDuration > 0f)
        {
            yield return new WaitForSeconds(waitDuration);
        }

        gameObject.SetActive(false);
    }
}
