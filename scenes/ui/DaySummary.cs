using System;
using Godot;

/// <summary>End-of-day panel showing the day's results, with a button to start the next day.</summary>
public partial class DaySummary : PanelContainer
{
    [Export] public Label TitleLabel { get; set; } = null!;
    [Export] public Label StatsLabel { get; set; } = null!;
    [Export] public Button NextDayButton { get; set; } = null!;

    public event Action? NextDayPressed;

    public override void _Ready()
    {
        Visible = false;
        NextDayButton.Pressed += OnNextDayButtonPressed;
    }

    public void ShowSummary(DayStats stats)
    {
        TitleLabel.Text = $"Day {stats.DayNumber} is over!";
        StatsLabel.Text =
            $"Customers served: {stats.CustomersServed}\n" +
            $"Customers lost: {stats.CustomersLost}\n" +
            $"Earnings: $ {stats.Earnings}";
        Visible = true;

        // Give the button keyboard focus so Space/Enter presses it.
        NextDayButton.GrabFocus();
    }

    private void OnNextDayButtonPressed()
    {
        Visible = false;
        NextDayPressed?.Invoke();
    }
}
