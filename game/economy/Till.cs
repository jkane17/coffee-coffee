using System;

/// <summary>Tracks the shop's money. Plain C# with no Godot dependency, so UI and other systems observe it via events.</summary>
public sealed class Till
{
    public Till(Money startingBalance = default)
    {
        if (startingBalance < Money.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(startingBalance), startingBalance, "The till can't start with a negative balance.");
        }

        Balance = startingBalance;
    }

    public Money Balance { get; private set; }

    /// <summary>Raised with the new balance whenever it changes.</summary>
    public event Action<Money>? BalanceChanged;

    /// <summary>Take payment for a sale, including any tip.</summary>
    public void AddSale(Money amount)
    {
        if (amount <= Money.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "A sale must be worth more than zero.");
        }

        Balance += amount;
        BalanceChanged?.Invoke(Balance);
    }

    public bool CanAfford(Money amount) => amount <= Balance;

    public void Spend(Money amount)
    {
        if (amount <= Money.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Spending must be more than zero.");
        }

        if (!CanAfford(amount))
        {
            throw new InvalidOperationException($"Can't spend {amount} with only {Balance} in the till.");
        }

        Balance -= amount;
        BalanceChanged?.Invoke(Balance);
    }
}
