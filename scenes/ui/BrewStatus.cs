using Godot;

/// <summary>Shows what the kettle is doing (filling, boiling, boiled) and how far along it is. Hidden while the kettle is idle.</summary>
public partial class BrewStatus : VBoxContainer
{
    [Export] public Label StatusLabel { get; set; } = null!;
    [Export] public ProgressBar BrewBar { get; set; } = null!;

    private Kettle? _kettle;

    /// <summary>Start displaying the given kettle, replacing any kettle shown before.</summary>
    public void ShowKettle(Kettle kettle)
    {
        _kettle = kettle;
    }

    public override void _Process(double delta)
    {
        // The kettle's state and progress change over time, so poll them rather than listening for events.
        string? status = _kettle?.State switch
        {
            KettleState.Filling => "Filling the kettle...",
            KettleState.Boiling => "Boiling...",
            KettleState.Boiled => "Kettle boiled",
            _ => null,
        };

        Visible = status is not null;
        if (_kettle is not null && status is not null)
        {
            StatusLabel.Text = status;
            BrewBar.Value = _kettle.Progress * BrewBar.MaxValue;
        }
    }
}
