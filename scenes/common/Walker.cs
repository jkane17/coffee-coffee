using System;
using Godot;

/// <summary>Component that walks its parent Node2D to a point at a steady speed, playing a walk animation while moving.
/// Add it as a child of the character to move.</summary>
public partial class Walker : Node
{
    private static readonly StringName WalkAnimation = "walk";

    [Export] public float Speed { get; set; } = 120f;

    /// <summary>Optional. Plays "walk" while moving; stopping returns to the animation's first frame, so key the resting pose at 0.</summary>
    [Export] public AnimationPlayer? AnimationPlayer { get; set; }

    private Node2D _mover = null!;
    private Vector2? _target;

    public bool IsWalking => _target is not null;

    /// <summary>Raised when the walker reaches the point it was sent to.</summary>
    public event Action? Arrived;

    public override void _Ready()
    {
        _mover = GetParent<Node2D>();
    }

    public void WalkTo(Vector2 globalTarget)
    {
        _target = globalTarget;

        if (AnimationPlayer is not null && AnimationPlayer.CurrentAnimation != WalkAnimation)
        {
            AnimationPlayer.Play(WalkAnimation);
        }
    }

    public override void _Process(double delta)
    {
        if (_target is not Vector2 target)
        {
            return;
        }

        _mover.GlobalPosition = _mover.GlobalPosition.MoveToward(target, Speed * (float)delta);

        if (_mover.GlobalPosition == target)
        {
            _target = null;
            AnimationPlayer?.Stop();
            Arrived?.Invoke();
        }
    }
}
