using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class AccelerationUpgradePad : MonoBehaviour
{
    [SerializeField, Min(1)] private int cost = 10;
    [SerializeField, Min(0.01f)] private float accelerationBonus = 0.25f;

    private bool purchased;

    private void OnTriggerEnter(Collider other)
    {
        if (purchased)
        {
            return;
        }

        RingWallet wallet = other.GetComponentInParent<RingWallet>();
        VehicleStats stats = other.GetComponentInParent<VehicleStats>();
        if (wallet == null || stats == null || !wallet.TrySpendRings(cost))
        {
            return;
        }

        stats.AddAccelerationMultiplier(accelerationBonus);
        purchased = true;
        gameObject.SetActive(false);
    }
}
