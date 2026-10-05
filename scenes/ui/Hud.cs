using System;
using Godot;

/// <summary>On-screen overlay. Observes gameplay systems and displays their state; never changes them.</summary>
public partial class Hud : CanvasLayer
{
    private const int ClockStepMinutes = 10;

    [Export] public Label MoneyLabel { get; set; } = null!;
    [Export] public Label ClockLabel { get; set; } = null!;
    [Export] public BrewStatus BrewStatus { get; set; } = null!;
    [Export] public DaySummary DaySummary { get; set; } = null!;

    private Till? _till;
    private DayClock? _clock;

    /// <summary>Raised when the player asks to start the next day from the summary panel.</summary>
    public event Action? NextDayRequested;

    public override void _Ready()
    {
        DaySummary.NextDayPressed += OnNextDayPressed;
    }

    public void ShowBrewer(Brewer brewer) => BrewStatus.ShowBrewer(brewer);

    public void ShowDaySummary(DayStats stats) => DaySummary.ShowSummary(stats);

    /// <summary>Start displaying the given clock's time of day.</summary>
    public void ShowClock(DayClock clock) => _clock = clock;

    /// <summary>Start displaying the given till's balance, replacing any till shown before.</summary>
    public void ShowTill(Till till)
    {
        StopShowingTill();
        _till = till;
        _till.BalanceChanged += OnBalanceChanged;
        OnBalanceChanged(_till.Balance);
    }

    public override void _Process(double delta)
    {
        // The time changes every frame, so poll it rather than listening for an event.
        if (_clock is not null)
        {
            ClockLabel.Text = _clock.IsOpen ? FormatTime(_clock.TimeOfDay) : "Closed";
        }
    }

    public override void _ExitTree()
    {
        // Till is plain C# and could outlive this node; unsubscribe so it never calls into a freed HUD.
        StopShowingTill();
    }

    /// <summary>Shows the time in 10-minute steps, like a shop clock, rather than a ticking digital one.</summary>
    private static string FormatTime(TimeSpan time)
    {
        int minutes = time.Minutes / ClockStepMinutes * ClockStepMinutes;
        return $"{time.Hours:00}:{minutes:00}";
    }

    private void StopShowingTill()
    {
        if (_till is not null)
        {
            _till.BalanceChanged -= OnBalanceChanged;
            _till = null;
        }
    }

    private void OnBalanceChanged(int balance)
    {
        MoneyLabel.Text = $"$ {balance}";
    }

    private void OnNextDayPressed()
    {
        NextDayRequested?.Invoke();
    }
}
