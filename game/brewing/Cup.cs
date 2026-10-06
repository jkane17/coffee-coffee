using System;

/// <summary>A take-away cup: empty, with coffee granules in it, or holding a finished drink.</summary>
public sealed class Cup
{
    public bool HasGranules { get; private set; }

    /// <summary>The finished drink in the cup, or null if it isn't made yet.</summary>
    public DrinkRecipe? Drink { get; private set; }

    public bool IsEmpty => !HasGranules && Drink is null;

    public void AddGranules()
    {
        if (!IsEmpty)
        {
            throw new InvalidOperationException("Granules only go into an empty cup.");
        }

        HasGranules = true;
    }

    /// <summary>Pour hot water onto the granules, which makes <paramref name="drink"/>.</summary>
    public void AddHotWater(DrinkRecipe drink)
    {
        if (!HasGranules)
        {
            throw new InvalidOperationException("Hot water goes onto granules.");
        }

        HasGranules = false;
        Drink = drink;
    }

    /// <summary>Tip everything out, leaving the cup empty.</summary>
    public void TipOut()
    {
        HasGranules = false;
        Drink = null;
    }
}
