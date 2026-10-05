using System;

/// <summary>Tracks the shop's money. Plain C# with no Godot dependency, so UI and other systems observe it via events.</summary>
public sealed class Till
{
    public Till(int startingBalance = 0)
    {
        if (startingBalance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startingBalance), startingBalance, "The till can't start with a negative balance.");
        }

        Balance = startingBalance;
    }

    public int Balance { get; private set; }

    /// <summary>Raised with the new balance whenever it changes.</summary>
    public event Action<int>? BalanceChanged;

    public void AddSale(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "A sale must be worth more than zero.");
        }

        Balance += amount;
        BalanceChanged?.Invoke(Balance);
    }
}
