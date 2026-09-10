using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class RingPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;
    [SerializeField, Min(0f)] private float spinSpeed = 0f;

    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        RingWallet wallet = other.GetComponentInParent<RingWallet>();
        if (wallet == null)
        {
            return;
        }

        wallet.AddRings(value);
        gameObject.SetActive(false);
    }
}
