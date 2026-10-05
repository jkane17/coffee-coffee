using Godot;

/// <summary>Pauses the game on the "pause" action and offers Resume or Quit to Title.
/// The node's Process Mode must be Always so it keeps handling input while the tree is paused.</summary>
public partial class PauseMenu : CanvasLayer
{
    private static readonly StringName PauseAction = "pause";

    /// <summary>Loaded by path rather than as a PackedScene export, because the title scene already references
    /// the game scene and two scenes exporting each other is a circular reference.</summary>
    [Export(PropertyHint.File, "*.tscn")] public string TitleScenePath { get; set; } = "";
    [Export] public Button ResumeButton { get; set; } = null!;
    [Export] public Button QuitToTitleButton { get; set; } = null!;

    public override void _Ready()
    {
        Visible = false;
        ResumeButton.Pressed += Resume;
        QuitToTitleButton.Pressed += OnQuitToTitlePressed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed(PauseAction))
        {
            return;
        }

        if (GetTree().Paused)
        {
            Resume();
        }
        else
        {
            Pause();
        }

        GetViewport().SetInputAsHandled();
    }

    private void Pause()
    {
        GetTree().Paused = true;
        Visible = true;
        ResumeButton.GrabFocus();
    }

    private void Resume()
    {
        GetTree().Paused = false;
        Visible = false;
    }

    private void OnQuitToTitlePressed()
    {
        // Pausing belongs to the SceneTree, not the scene, so it would carry over and freeze the title screen.
        GetTree().Paused = false;

        Error error = GetTree().ChangeSceneToFile(TitleScenePath);
        if (error != Error.Ok)
        {
            GD.PushError($"Couldn't open the title scene '{TitleScenePath}': {error}");
        }
    }
}
