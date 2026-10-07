using System;
using System.Globalization;

/// <summary>An amount of money, stored as whole cents so sums never pick up floating-point errors (0.1 + 0.2 isn't exactly 0.3 as a double).
/// Shown as dollars and cents, e.g. "$ 2.05".</summary>
public readonly record struct Money(int Cents) : IComparable<Money>
{
    public static readonly Money Zero = new(0);

    /// <summary>Convert a dollar amount from the Inspector (e.g. 2.5) to money, rounded to the nearest cent.</summary>
    public static Money FromDollars(double dollars) => new((int)Math.Round(dollars * 100, MidpointRounding.AwayFromZero));

    /// <summary>Round to the nearest multiple of <paramref name="stepCents"/>, e.g. 5 for the nearest 5 cents.</summary>
    public Money RoundToNearest(int stepCents)
    {
        return new Money((int)Math.Round((double)Cents / stepCents, MidpointRounding.AwayFromZero) * stepCents);
    }

    public static Money operator +(Money a, Money b) => new(a.Cents + b.Cents);
    public static Money operator -(Money a, Money b) => new(a.Cents - b.Cents);
    public static bool operator <(Money a, Money b) => a.Cents < b.Cents;
    public static bool operator >(Money a, Money b) => a.Cents > b.Cents;
    public static bool operator <=(Money a, Money b) => a.Cents <= b.Cents;
    public static bool operator >=(Money a, Money b) => a.Cents >= b.Cents;

    public int CompareTo(Money other) => Cents.CompareTo(other.Cents);

    public override string ToString() => "$ " + (Cents / 100m).ToString("0.00", CultureInfo.InvariantCulture);
}
