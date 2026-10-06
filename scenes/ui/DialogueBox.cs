using System;
using Godot;

/// <summary>Shows a <see cref="Dialogue"/> one line at a time, typing each line out. Click, Enter or Space finishes the line, then moves on.
/// The root fills the screen and stops mouse clicks, so the shop can't be used while someone is talking.</summary>
public partial class DialogueBox : Control
{
    private static readonly StringName AdvanceAction = "ui_accept";

    [Export] public Label SpeakerLabel { get; set; } = null!;
    [Export] public Label TextLabel { get; set; } = null!;
    [Export(PropertyHint.Range, "5,200,5,suffix:chars/s")] public float CharactersPerSecond { get; set; } = 40f;

    private Dialogue? _dialogue;
    private int _lineIndex;
    private Tween? _typing;
    private Action? _onFinished;

    public bool IsPlaying => _dialogue is not null;

    public override void _Ready()
    {
        Visible = false;
    }

    /// <summary>Show the dialogue from its first line, then hide and call <paramref name="onFinished"/> after the last one.</summary>
    public void Play(Dialogue dialogue, Action? onFinished = null)
    {
        if (dialogue.Lines.Count == 0)
        {
            onFinished?.Invoke();
            return;
        }

        _dialogue = dialogue;
        _onFinished = onFinished;
        _lineIndex = 0;
        Visible = true;
        ShowLine();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            Advance();
            AcceptEvent();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (IsPlaying && @event.IsActionPressed(AdvanceAction))
        {
            Advance();
            GetViewport().SetInputAsHandled();
        }
    }

    private void ShowLine()
    {
        DialogueLine line = _dialogue!.Lines[_lineIndex];
        SpeakerLabel.Text = line.Speaker;
        TextLabel.Text = line.Text;

        // Type the line out by tweening how much of the text is visible from 0 to 1.
        TextLabel.VisibleRatio = 0;
        _typing?.Kill();
        _typing = CreateTween();
        _typing.TweenProperty(TextLabel, Label.PropertyName.VisibleRatio.ToString(), 1.0, line.Text.Length / CharactersPerSecond);
    }

    private void Advance()
    {
        if (_dialogue is null)
        {
            return;
        }

        // The first press finishes a line that's still typing; the next one moves on.
        if (_typing is not null && _typing.IsRunning())
        {
            _typing.Kill();
            TextLabel.VisibleRatio = 1;
            return;
        }

        _lineIndex++;
        if (_lineIndex < _dialogue.Lines.Count)
        {
            ShowLine();
            return;
        }

        _dialogue = null;
        Visible = false;

        // Clear before invoking, so the callback can start another dialogue.
        Action? onFinished = _onFinished;
        _onFinished = null;
        onFinished?.Invoke();
    }
}
