using System;
using UnityEngine;

public sealed class CoinWallet : MonoBehaviour
{
    public event Action<int> CoinsChanged;

    public int Coins { get; private set; }

    public void AddCoins(int amount)
    {
        Coins += Mathf.Max(0, amount);
        CoinsChanged?.Invoke(Coins);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || Coins < amount)
        {
            return false;
        }

        Coins -= amount;
        CoinsChanged?.Invoke(Coins);
        return true;
    }
}
