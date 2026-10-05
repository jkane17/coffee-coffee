using System;
using Godot;

/// <summary>A customer that walks to points in the shop, orders a drink, loses patience while waiting, and removes itself once it has left.</summary>
public partial class Customer : Node2D
{
    private static readonly Color ImpatientTint = new(1f, 0.35f, 0.35f);

    /// <summary>Emitted when the customer reaches a spot it was sent to with <see cref="WalkTo"/>.</summary>
    [Signal] public delegate void ArrivedEventHandler();

    /// <summary>Emitted when the customer runs out of patience before being served.</summary>
    [Signal] public delegate void GaveUpEventHandler();

    [Export] public float WalkSpeed { get; set; } = 120f;
    [Export] public Label OrderLabel { get; set; } = null!;

    /// <summary>Tinted towards red as patience runs out.</summary>
    [Export] public CanvasItem Body { get; set; } = null!;

    [ExportGroup("Patience")]
    [Export(PropertyHint.Range, "1,120,1,suffix:s")] public float MinPatienceSeconds { get; set; } = 15f;
    [Export(PropertyHint.Range, "1,120,1,suffix:s")] public float MaxPatienceSeconds { get; set; } = 30f;

    private Vector2? _target;
    private bool _isLeaving;
    private bool _isBeingServed;
    private Patience _patience = null!;

    /// <summary>The drink this customer wants, or null if they haven't ordered yet.</summary>
    public DrinkRecipe? Order { get; private set; }

    public override void _Ready()
    {
        double patienceSeconds = Mathf.Lerp(MinPatienceSeconds, MaxPatienceSeconds, Random.Shared.NextSingle());
        _patience = new Patience(patienceSeconds);
        _patience.RanOut += OnPatienceRanOut;
    }

    public void WalkTo(Vector2 globalTarget) => _target = globalTarget;

    public void PlaceOrder(DrinkRecipe drink)
    {
        Order = drink;
        OrderLabel.Text = drink.DisplayName;
        OrderLabel.Visible = true;
    }

    /// <summary>Their drink is being made: stop losing patience.</summary>
    public void StartBeingServed()
    {
        _isBeingServed = true;
        Body.Modulate = Colors.White;
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
        if (!_isLeaving && !_isBeingServed)
        {
            _patience.Tick(delta);
            Body.Modulate = Colors.White.Lerp(ImpatientTint, 1f - (float)_patience.Fraction);
        }

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

    private void OnPatienceRanOut()
    {
        EmitSignal(SignalName.GaveUp);
    }
}
