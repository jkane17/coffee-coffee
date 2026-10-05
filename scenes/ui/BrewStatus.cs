using Godot;

/// <summary>Shows what the brewer is making and how far along it is. Hidden while the brewer is idle.</summary>
public partial class BrewStatus : VBoxContainer
{
    [Export] public Label DrinkLabel { get; set; } = null!;
    [Export] public ProgressBar BrewBar { get; set; } = null!;

    private Brewer? _brewer;

    /// <summary>Start displaying the given brewer, replacing any brewer shown before.</summary>
    public void ShowBrewer(Brewer brewer)
    {
        StopShowingBrewer();
        _brewer = brewer;
        _brewer.BrewStarted += OnBrewStarted;
        _brewer.BrewFinished += OnBrewFinished;

        if (_brewer.CurrentDrink is DrinkRecipe drink)
        {
            OnBrewStarted(drink);
        }
        else
        {
            Visible = false;
        }
    }

    public override void _Process(double delta)
    {
        // Progress changes every frame, so poll it rather than raising an event per frame.
        if (_brewer is { IsBusy: true })
        {
            BrewBar.Value = _brewer.Progress * BrewBar.MaxValue;
        }
    }

    public override void _ExitTree()
    {
        StopShowingBrewer();
    }

    private void StopShowingBrewer()
    {
        if (_brewer is not null)
        {
            _brewer.BrewStarted -= OnBrewStarted;
            _brewer.BrewFinished -= OnBrewFinished;
            _brewer = null;
        }
    }

    private void OnBrewStarted(DrinkRecipe drink)
    {
        DrinkLabel.Text = $"Brewing {drink.DisplayName}...";
        BrewBar.Value = 0;
        Visible = true;
    }

    private void OnBrewFinished(DrinkRecipe drink)
    {
        Visible = false;
    }
}
