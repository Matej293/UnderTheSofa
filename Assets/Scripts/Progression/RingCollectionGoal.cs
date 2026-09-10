using UnityEngine;

public sealed class RingCollectionGoal : MonoBehaviour
{
    [SerializeField] private RingWallet wallet;
    [SerializeField] private LivingRoomGoal goalToComplete;
    [SerializeField, Min(1)] private int requiredRings = 15;

    private void Awake()
    {
        if (wallet == null)
        {
            wallet = FindFirstObjectByType<RingWallet>();
        }
    }

    private void OnEnable()
    {
        if (wallet != null)
        {
            wallet.RingsChanged += Evaluate;
            Evaluate(wallet.Rings);
        }
    }

    private void OnDisable()
    {
        if (wallet != null)
        {
            wallet.RingsChanged -= Evaluate;
        }
    }

    private void Evaluate(int ringCount)
    {
        if (ringCount >= requiredRings && goalToComplete != null)
        {
            goalToComplete.Complete();
        }
    }
}
