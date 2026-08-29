using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CoinPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;
    [SerializeField, Min(0f)] private float spinSpeed = 180f;

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        CoinWallet wallet = other.GetComponentInParent<CoinWallet>();
        if (wallet == null)
        {
            return;
        }

        wallet.AddCoins(value);
        gameObject.SetActive(false);
    }
}
