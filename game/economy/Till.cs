using System;

/// <summary>Tracks the shop's money. Plain C# with no Godot dependency, so UI and other systems observe it via events.</summary>
public sealed class Till
{
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
