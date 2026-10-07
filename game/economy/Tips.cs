using System;

/// <summary>How much a served customer tips: up to a share of the drink's price, scaled by how much patience they had left.
/// A customer served straight away tips the full share; one kept waiting until they give up tips nothing.</summary>
public static class Tips
{
    /// <summary>Tips are rounded to this many cents, so balances stay simple multiples of 5 cents.</summary>
    public const int RoundToCents = 5;

    /// <param name="price">What the customer paid for their drink.</param>
    /// <param name="patienceLeft">Their patience when served, from 0 (about to give up) to 1 (full).</param>
    /// <param name="maxShare">The biggest tip as a share of the price, e.g. 0.5 for half the price.</param>
    public static Money For(Money price, double patienceLeft, double maxShare)
    {
        double cents = price.Cents * maxShare * Math.Clamp(patienceLeft, 0, 1);
        return new Money((int)Math.Round(cents)).RoundToNearest(RoundToCents);
    }
}
