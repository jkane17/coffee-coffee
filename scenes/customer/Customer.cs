using Godot;

/// <summary>A customer that walks to points in the shop, orders a drink, and removes itself once it has left.</summary>
public partial class Customer : Node2D
{
    /// <summary>Emitted when the customer reaches a spot it was sent to with <see cref="WalkTo"/>.</summary>
    [Signal] public delegate void ArrivedEventHandler();

    [Export] public float WalkSpeed { get; set; } = 120f;
    [Export] public Label OrderLabel { get; set; } = null!;

    private Vector2? _target;
    private bool _isLeaving;

    /// <summary>The drink this customer wants, or null if they haven't ordered yet.</summary>
    public DrinkRecipe? Order { get; private set; }

    public void WalkTo(Vector2 globalTarget) => _target = globalTarget;

    public void PlaceOrder(DrinkRecipe drink)
    {
        Order = drink;
        OrderLabel.Text = drink.DisplayName;
        OrderLabel.Visible = true;
    }

    /// <summary>Walk to the exit, then remove this customer from the scene.</summary>
    public void LeaveThrough(Vector2 globalExit)
    {
        _isLeaving = true;
        OrderLabel.Visible = false;
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
