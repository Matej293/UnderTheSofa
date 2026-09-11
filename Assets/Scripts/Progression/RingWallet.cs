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

}
