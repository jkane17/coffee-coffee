using Godot;

/// <summary>A customer that walks to a point in the shop and reports when it arrives.</summary>
public partial class Customer : Node2D
{
    [Signal] public delegate void ArrivedEventHandler();

    [Export] public float WalkSpeed { get; set; } = 120f;

    private Vector2? _target;

    public void WalkTo(Vector2 globalTarget) => _target = globalTarget;

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
            EmitSignal(SignalName.Arrived);
        }
    }
}
