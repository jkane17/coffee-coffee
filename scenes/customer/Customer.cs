using Godot;

/// <summary>A customer that walks to points in the shop, and removes itself once it has left.</summary>
public partial class Customer : Node2D
{
    /// <summary>Emitted when the customer reaches a spot it was sent to with <see cref="WalkTo"/>.</summary>
    [Signal] public delegate void ArrivedEventHandler();

    [Export] public float WalkSpeed { get; set; } = 120f;

    private Vector2? _target;
    private bool _isLeaving;

    public bool IsWalking => _target is not null;

    public void WalkTo(Vector2 globalTarget) => _target = globalTarget;

    /// <summary>Walk to the exit, then remove this customer from the scene.</summary>
    public void LeaveThrough(Vector2 globalExit)
    {
        _isLeaving = true;
        WalkTo(globalExit);
    }

    public override void _Process(double delta)
    {
        if (_target is not Vector2 target)
        {
            return;
        }

        GlobalPosition = GlobalPosition.MoveToward(target, WalkSpeed * (float)delta);

        if (GlobalPosition == target)
        {
            _target = null;

            if (_isLeaving)
            {
                QueueFree();
                return;
            }

            EmitSignal(SignalName.Arrived);
        }
    }
}
