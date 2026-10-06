using System;

/// <summary>The rules for making instant coffee at the brew counter's stations (sink, kettle, cups, granules),
/// and what the barista is carrying between them. Each station has a CanUse check and a Use action that does whatever makes sense right now.
/// The owner advances it by calling <see cref="Tick"/>.</summary>
public sealed class CoffeeBar
{
    private readonly DrinkRecipe _instantCoffee;

    public CoffeeBar(Kettle kettle, DrinkRecipe instantCoffee)
    {
        Kettle = kettle;
        _instantCoffee = instantCoffee;
    }

    public Kettle Kettle { get; }

    /// <summary>True while the barista is carrying the kettle (including while it fills at the sink); otherwise it's on its base.</summary>
    public bool IsHoldingKettle { get; private set; }

    /// <summary>The cup the barista is carrying, if any.</summary>
    public Cup? HeldCup { get; private set; }

    public bool HandsEmpty => !IsHoldingKettle && HeldCup is null;

    /// <summary>The finished drink the barista is carrying, if any.</summary>
    public DrinkRecipe? HeldDrink => HeldCup?.Drink;

    /// <summary>True while the barista has to stay put: holding the kettle under the tap.</summary>
    public bool IsBusy => Kettle.State == KettleState.Filling;

    /// <summary>Raised when what the barista is carrying, or where the kettle is, changes.</summary>
    public event Action? Changed;

    public void Tick(double deltaSeconds) => Kettle.Tick(deltaSeconds);

    // Kettle base: lift the empty kettle, put a filled one back to boil, or pour boiled water onto granules.
    public bool CanUseKettle => CanLiftKettle || CanPutKettleBack || CanPourWater;
    private bool CanLiftKettle => HandsEmpty && Kettle.State == KettleState.Empty;
    private bool CanPutKettleBack => IsHoldingKettle && Kettle.State == KettleState.Full;
    private bool CanPourWater => !IsHoldingKettle && Kettle.State == KettleState.Boiled && HeldCup is { HasGranules: true };

    public void UseKettle()
    {
        if (CanLiftKettle)
        {
            IsHoldingKettle = true;
        }
        else if (CanPutKettleBack)
        {
            IsHoldingKettle = false;
            Kettle.StartBoiling();
        }
        else if (CanPourWater)
        {
            Kettle.PourCup();
            HeldCup!.AddHotWater(_instantCoffee);
        }
        else
        {
            return;
        }

        Changed?.Invoke();
    }

    // Sink: fill the empty kettle, or tip out a cup that has something in it.
    public bool CanUseSink => CanFillKettle || CanTipOutCup;
    private bool CanFillKettle => IsHoldingKettle && Kettle.State == KettleState.Empty;
    private bool CanTipOutCup => HeldCup is { IsEmpty: false };

    public void UseSink()
    {
        if (CanFillKettle)
        {
            Kettle.StartFilling();
        }
        else if (CanTipOutCup)
        {
            HeldCup!.TipOut();
        }
        else
        {
            return;
        }

        Changed?.Invoke();
    }

    // Cup stack: grab a cup with empty hands, or put an empty one back.
    public bool CanUseCupStack => HandsEmpty || HeldCup is { IsEmpty: true };

    public void UseCupStack()
    {
        if (HandsEmpty)
        {
            HeldCup = new Cup();
        }
        else if (HeldCup is { IsEmpty: true })
        {
            HeldCup = null;
        }
        else
        {
            return;
        }

        Changed?.Invoke();
    }

    // Granules jar: spoon granules into an empty cup.
    public bool CanUseGranules => HeldCup is { IsEmpty: true };

    public void UseGranules()
    {
        if (!CanUseGranules)
        {
            return;
        }

        HeldCup!.AddGranules();
        Changed?.Invoke();
    }

    /// <summary>Give the finished drink to a customer, emptying the barista's hands.</summary>
    public DrinkRecipe HandOverDrink()
    {
        DrinkRecipe drink = HeldDrink ?? throw new InvalidOperationException("The barista isn't carrying a finished drink.");
        HeldCup = null;
        Changed?.Invoke();
        return drink;
    }
}
