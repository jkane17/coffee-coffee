using Godot;

/// <summary>The first screen: continues a saved game, starts a new one, or quits.</summary>
public partial class TitleScreen : Control
{
    /// <summary>The scene to switch to when starting the game (the Main scene).</summary>
    [Export] public PackedScene GameScene { get; set; } = null!;
    [Export] public Button ContinueButton { get; set; } = null!;
    [Export] public Button NewGameButton { get; set; } = null!;
    [Export] public Button QuitButton { get; set; } = null!;
    [Export] public ConfirmationDialog NewGameConfirm { get; set; } = null!;

    private SaveData? _save;

    public override void _Ready()
    {
        _save = new SaveStore().Load();

        ContinueButton.Visible = _save is not null;
        if (_save is not null)
        {
            ContinueButton.Text = $"Continue (Day {_save.DaysCompleted + 1})";
        }

        ContinueButton.Pressed += OnContinuePressed;
        NewGameButton.Pressed += OnNewGamePressed;
        QuitButton.Pressed += OnQuitPressed;
        NewGameConfirm.Confirmed += OnNewGameConfirmed;
        NewGameConfirm.Canceled += OnNewGameCanceled;

        // Focus the most likely choice so Enter/Space works without the mouse.
        (_save is not null ? ContinueButton : NewGameButton).GrabFocus();
    }

    private void OnContinuePressed()
    {
        StartGame(_save);
    }

    private void OnNewGamePressed()
    {
        if (_save is null)
        {
            StartGame(null);
        }
        else
        {
            NewGameConfirm.PopupCentered();
        }
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void OnNewGameConfirmed()
    {
        StartGame(null);
    }

    private void OnNewGameCanceled()
    {
        ContinueButton.GrabFocus();
    }

    /// <summary>Swap this screen for the game scene by hand, rather than with ChangeSceneToPacked,
    /// so the save can be handed to Main before its _Ready runs.</summary>
    private void StartGame(SaveData? save)
    {
        Main main = GameScene.Instantiate<Main>();
        if (save is not null)
        {
            main.ContinueFrom(save);
        }

        SceneTree tree = GetTree();
        tree.Root.AddChild(main);
        tree.CurrentScene = main;
        QueueFree();
    }
}
