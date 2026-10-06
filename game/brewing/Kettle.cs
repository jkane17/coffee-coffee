using System;

/// <summary>A kettle that is filled at the sink and boiled on its base, and holds enough water for <see cref="CapacityCups"/> cups.
/// The owner advances it by calling <see cref="Tick"/>.</summary>
public sealed class Kettle
{
    private double _elapsedSeconds;

    public Kettle(int capacityCups, double fillSeconds, double boilSeconds)
    {
        if (capacityCups < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacityCups), capacityCups, "A kettle must hold at least one cup.");
        }

        if (fillSeconds <= 0 || boilSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fillSeconds), "Filling and boiling must take longer than zero seconds.");
        }

        CapacityCups = capacityCups;
        FillSeconds = fillSeconds;
        BoilSeconds = boilSeconds;
    }

    public int CapacityCups { get; }
    public double FillSeconds { get; }
    public double BoilSeconds { get; }

    public KettleState State { get; private set; } = KettleState.Empty;

    /// <summary>How many more cups can be poured from the water in the kettle.</summary>
    public int CupsOfWater { get; private set; }

    /// <summary>How far along filling or boiling is, from 0 to 1. Stays at 1 once boiled.</summary>
    public double Progress => State switch
    {
        KettleState.Filling => Math.Clamp(_elapsedSeconds / FillSeconds, 0, 1),
        KettleState.Boiling => Math.Clamp(_elapsedSeconds / BoilSeconds, 0, 1),
        KettleState.Boiled => 1,
        _ => 0,
    };

    public void StartFilling()
    {
        Require(KettleState.Empty, "fill");
        State = KettleState.Filling;
        _elapsedSeconds = 0;
    }

    public void StartBoiling()
    {
        Require(KettleState.Full, "boil");
        State = KettleState.Boiling;
        _elapsedSeconds = 0;
    }

    /// <summary>Pour one cup of hot water. The kettle is empty again once its last cup is poured.</summary>
    public void PourCup()
    {
        Require(KettleState.Boiled, "pour");
        CupsOfWater--;
        if (CupsOfWater == 0)
        {
            State = KettleState.Empty;
        }
    }

    public void Tick(double deltaSeconds)
    {
        if (State is not (KettleState.Filling or KettleState.Boiling))
        {
            return;
        }

        _elapsedSeconds += deltaSeconds;

        if (State == KettleState.Filling && _elapsedSeconds >= FillSeconds)
        {
            State = KettleState.Full;
            CupsOfWater = CapacityCups;
        }
        else if (State == KettleState.Boiling && _elapsedSeconds >= BoilSeconds)
        {
            State = KettleState.Boiled;
        }
    }

    private void Require(KettleState expected, string action)
    {
        if (State != expected)
        {
            throw new InvalidOperationException($"Can't {action} the kettle while it's {State}.");
        }
    }
}
