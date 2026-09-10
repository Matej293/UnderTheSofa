using System;
using UnityEngine;

public sealed class RingWallet : MonoBehaviour
{
    public event Action<int> RingsChanged;

    public int Rings { get; private set; }

    public void AddRings(int amount)
    {
        Rings += Mathf.Max(0, amount);
        RingsChanged?.Invoke(Rings);
    }

    public bool TrySpendRings(int amount)
    {
        if (amount <= 0 || Rings < amount)
        {
            return false;
        }

        Rings -= amount;
        RingsChanged?.Invoke(Rings);
        return true;
    }
}
