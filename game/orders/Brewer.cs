using System;

/// <summary>Brews one drink at a time. The owner advances it by calling <see cref="Tick"/> every frame.</summary>
public sealed class Brewer
{
    private double _elapsedSeconds;

    /// <summary>The drink being brewed, or null when idle.</summary>
    public DrinkRecipe? CurrentDrink { get; private set; }

    public bool IsBusy => CurrentDrink is not null;

    /// <summary>How far along the current brew is, from 0 to 1. Zero when idle.</summary>
    public double Progress => CurrentDrink is null
        ? 0
        : Math.Clamp(_elapsedSeconds / CurrentDrink.BrewSeconds, 0, 1);

    public event Action<DrinkRecipe>? BrewStarted;
    public event Action<DrinkRecipe>? BrewFinished;

    public void Start(DrinkRecipe drink)
    {
        if (IsBusy)
        {
            throw new InvalidOperationException("The brewer is already brewing a drink.");
        }

        CurrentDrink = drink;
        _elapsedSeconds = 0;
        BrewStarted?.Invoke(drink);
    }

    public void Tick(double deltaSeconds)
    {
        if (CurrentDrink is not DrinkRecipe drink)
        {
            return;
        }

        _elapsedSeconds += deltaSeconds;

        if (_elapsedSeconds >= drink.BrewSeconds)
        {
            CurrentDrink = null;
            _elapsedSeconds = 0;
            BrewFinished?.Invoke(drink);
        }
    }
}
