using System;
using Godot;

/// <summary>End-of-day panel showing the day's results, with buttons to visit the upgrades page and to start the next day.</summary>
public partial class DaySummary : PanelContainer
{
    [Export] public Label TitleLabel { get; set; } = null!;
    [Export] public Label StatsLabel { get; set; } = null!;
    [Export] public Button NextDayButton { get; set; } = null!;
    [Export] public Button UpgradesButton { get; set; } = null!;

    public event Action? NextDayPressed;
    public event Action? UpgradesPressed;

    public override void _Ready()
    {
        Visible = false;
        NextDayButton.Pressed += OnNextDayButtonPressed;
        UpgradesButton.Pressed += OnUpgradesButtonPressed;
    }

    public void ShowSummary(DayStats stats)
    {
        TitleLabel.Text = $"Day {stats.DayNumber} is over!";
        StatsLabel.Text =
            $"Customers served: {stats.CustomersServed}\n" +
            $"Customers lost: {stats.CustomersLost}\n" +
            $"Earnings: $ {stats.Earnings}";
        Reopen();
    }

    /// <summary>Show the panel again with the same results, e.g. after coming back from the upgrades page.</summary>
    public void Reopen()
    {
        Visible = true;

        // Give the button keyboard focus so Space/Enter presses it.
        NextDayButton.GrabFocus();
    }

    private void OnNextDayButtonPressed()
    {
        Visible = false;
        NextDayPressed?.Invoke();
    }

    private void OnUpgradesButtonPressed()
    {
        Visible = false;
        UpgradesPressed?.Invoke();
    }
}
