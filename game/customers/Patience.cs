using System;

/// <summary>Counts down how long a customer is willing to wait. The owner advances it by calling <see cref="Tick"/>.</summary>
public sealed class Patience
{
    public Patience(double totalSeconds)
    {
        if (totalSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalSeconds), totalSeconds, "Patience must be longer than zero seconds.");
        }

        TotalSeconds = totalSeconds;
        RemainingSeconds = totalSeconds;
    }

    public double TotalSeconds { get; }
    public double RemainingSeconds { get; private set; }

    /// <summary>1 when fully patient, 0 when patience has run out.</summary>
    public double Fraction => RemainingSeconds / TotalSeconds;

    public bool HasRunOut => RemainingSeconds <= 0;

    /// <summary>Raised once, when the remaining time reaches zero.</summary>
    public event Action? RanOut;

    public void Tick(double deltaSeconds)
    {
        if (HasRunOut)
        {
            return;
        }

        RemainingSeconds = Math.Max(0, RemainingSeconds - deltaSeconds);

        if (HasRunOut)
        {
            RanOut?.Invoke();
        }
    }
}
