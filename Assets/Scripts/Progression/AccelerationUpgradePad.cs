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

        CoinWallet wallet = other.GetComponentInParent<CoinWallet>();
        VehicleStats stats = other.GetComponentInParent<VehicleStats>();
        if (wallet == null || stats == null || !wallet.TrySpend(cost))
        {
            return;
        }

        stats.AddAccelerationMultiplier(accelerationBonus);
        purchased = true;
        gameObject.SetActive(false);
    }
}
