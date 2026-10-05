using System;

/// <summary>An in-game clock that runs from opening to closing time over a fixed number of real seconds.
/// The owner advances it by calling <see cref="Tick"/>.</summary>
public sealed class DayClock
{
    private readonly double _dayLengthSeconds;
    private readonly int _openHour;
    private readonly int _closeHour;
    private double _elapsedSeconds;

    public DayClock(double dayLengthSeconds, int openHour, int closeHour)
    {
        if (dayLengthSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dayLengthSeconds), dayLengthSeconds, "A day must last longer than zero seconds.");
        }

        if (openHour < 0 || closeHour > 24 || openHour >= closeHour)
        {
            throw new ArgumentException($"Opening hour ({openHour}) must be before closing hour ({closeHour}), within 0–24.");
        }

        _dayLengthSeconds = dayLengthSeconds;
        _openHour = openHour;
        _closeHour = closeHour;
    }

    public bool IsOpen { get; private set; }

    /// <summary>How far through the day it is, from 0 at opening to 1 at closing.</summary>
    public double Progress => Math.Clamp(_elapsedSeconds / _dayLengthSeconds, 0, 1);

    /// <summary>The in-game time of day.</summary>
    public TimeSpan TimeOfDay => TimeSpan.FromHours(_openHour + (_closeHour - _openHour) * Progress);

    public event Action? Closed;

    /// <summary>Start a new day at opening time.</summary>
    public void Open()
    {
        _elapsedSeconds = 0;
        IsOpen = true;
    }

    public void Tick(double deltaSeconds)
    {
        if (!IsOpen)
        {
            return;
        }

        _elapsedSeconds += deltaSeconds;

        if (_elapsedSeconds >= _dayLengthSeconds)
        {
            _elapsedSeconds = _dayLengthSeconds;
            IsOpen = false;
            Closed?.Invoke();
        }
    }
}
