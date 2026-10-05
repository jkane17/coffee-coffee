using Godot;

/// <summary>The first screen: starts the game or quits.</summary>
public partial class TitleScreen : Control
{
    /// <summary>The scene to switch to when Play is pressed (the Main scene).</summary>
    [Export] public PackedScene GameScene { get; set; } = null!;
    [Export] public Button PlayButton { get; set; } = null!;
    [Export] public Button QuitButton { get; set; } = null!;

    public override void _Ready()
    {
        PlayButton.Pressed += OnPlayPressed;
        QuitButton.Pressed += OnQuitPressed;

        // Focus Play so Enter/Space starts the game without touching the mouse.
        PlayButton.GrabFocus();
    }

    private void OnPlayPressed()
    {
        Error error = GetTree().ChangeSceneToPacked(GameScene);
        if (error != Error.Ok)
        {
            GD.PushError($"Couldn't start the game scene: {error}");
        }
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
