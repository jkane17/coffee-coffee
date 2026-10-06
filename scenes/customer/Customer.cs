using System;
using Godot;

/// <summary>A customer that walks to points in the shop, decides what to order, loses patience while waiting, and removes itself once it has left.</summary>
public partial class Customer : Node2D
{
    private static readonly Color ImpatientTint = new(1f, 0.35f, 0.35f);

    /// <summary>Emitted when the customer reaches a spot it was sent to with <see cref="WalkTo"/>.</summary>
    [Signal] public delegate void ArrivedEventHandler();

    /// <summary>Emitted when the customer runs out of patience before being served.</summary>
    [Signal] public delegate void GaveUpEventHandler();

    [Export] public Walker Walker { get; set; } = null!;
    [Export] public Label OrderLabel { get; set; } = null!;
    [Export] public TextureRect OrderIcon { get; set; } = null!;
    [Export] public Control OrderBubble { get; set; } = null!;

    /// <summary>Shown while the customer is deciding what to order.</summary>
    [Export] public CanvasItem ThinkingIndicator { get; set; } = null!;

    /// <summary>Shown once the customer has decided and wants the barista to take their order.</summary>
    [Export] public CanvasItem ReadyIndicator { get; set; } = null!;

    /// <summary>The customer's sprite. Gets a random look on spawn and is tinted towards red as patience runs out.</summary>
    [Export] public Sprite2D Body { get; set; } = null!;

    /// <summary>Textures to pick from so customers don't all look the same.</summary>
    [Export] public Godot.Collections.Array<Texture2D> Looks { get; set; } = new();

    [ExportGroup("Ordering")]
    [Export(PropertyHint.Range, "0,10,0.5,suffix:s")] public float MinThinkSeconds { get; set; } = 1.5f;
    [Export(PropertyHint.Range, "0,10,0.5,suffix:s")] public float MaxThinkSeconds { get; set; } = 4f;

    [ExportGroup("Patience")]
    [Export(PropertyHint.Range, "1,120,1,suffix:s")] public float MinPatienceSeconds { get; set; } = 15f;
    [Export(PropertyHint.Range, "1,120,1,suffix:s")] public float MaxPatienceSeconds { get; set; } = 30f;
    /// <summary>Patience given back when the barista takes the order.</summary>
    [Export(PropertyHint.Range, "0,60,1,suffix:s")] public float OrderTakenPatienceBoost { get; set; } = 5f;

    private Patience _patience = null!;
    private double _thinkSecondsLeft;

    public CustomerState State { get; private set; } = CustomerState.Queueing;

    /// <summary>The drink this customer wants, or null if their order hasn't been taken yet.</summary>
    public DrinkRecipe? Order { get; private set; }

    public override void _Ready()
    {
        if (Looks.Count > 0)
        {
            Body.Texture = Looks[Random.Shared.Next(Looks.Count)];
        }

        double patienceSeconds = Mathf.Lerp(MinPatienceSeconds, MaxPatienceSeconds, Random.Shared.NextSingle());
        _patience = new Patience(patienceSeconds);
        _patience.RanOut += OnPatienceRanOut;
        Walker.Arrived += OnWalkerArrived;

        OrderBubble.Visible = false;
        ThinkingIndicator.Visible = false;
        ReadyIndicator.Visible = false;
    }

    public void WalkTo(Vector2 globalTarget) => Walker.WalkTo(globalTarget);

    /// <summary>They've reached their spot at the counter: spend a moment deciding, then ask to order. Ignored if they're already past queueing.</summary>
    public void StartThinking()
    {
        if (State != CustomerState.Queueing)
        {
            return;
        }

        State = CustomerState.Thinking;
        _thinkSecondsLeft = Mathf.Lerp(MinThinkSeconds, MaxThinkSeconds, Random.Shared.NextSingle());
        ThinkingIndicator.Visible = true;
    }

    /// <summary>The barista takes their order: show it, and give back a little patience for the attention.</summary>
    public void TakeOrder(DrinkRecipe drink)
    {
        if (State != CustomerState.ReadyToOrder)
        {
            throw new InvalidOperationException($"Can't take an order from a customer who is {State}.");
        }

        State = CustomerState.Ordered;
        Order = drink;
        OrderLabel.Text = drink.DisplayName;
        OrderIcon.Texture = drink.Icon;
        ReadyIndicator.Visible = false;
        OrderBubble.Visible = true;
        _patience.Restore(OrderTakenPatienceBoost);
    }

    /// <summary>Their drink is being made: stop losing patience.</summary>
    public void StartBeingServed()
    {
        State = CustomerState.BeingServed;
    }

    /// <summary>Walk to the exit, then remove this customer from the scene.</summary>
    public void LeaveThrough(Vector2 globalExit)
    {
        State = CustomerState.Leaving;
        OrderBubble.Visible = false;
        ThinkingIndicator.Visible = false;
        ReadyIndicator.Visible = false;
        WalkTo(globalExit);
    }

    public override void _Process(double delta)
    {
        switch (State)
        {
            case CustomerState.Thinking:
                _thinkSecondsLeft -= delta;
                if (_thinkSecondsLeft <= 0)
                {
                    State = CustomerState.ReadyToOrder;
                    ThinkingIndicator.Visible = false;
                    ReadyIndicator.Visible = true;
                }
                break;

            case CustomerState.ReadyToOrder:
            case CustomerState.Ordered:
                _patience.Tick(delta);
                Body.Modulate = Colors.White.Lerp(ImpatientTint, 1f - (float)_patience.Fraction);
                break;
        }
    }

    private void OnWalkerArrived()
    {
        if (State == CustomerState.Leaving)
        {
            QueueFree();
            return;
        }

        EmitSignal(SignalName.Arrived);
    }

    private void OnPatienceRanOut()
    {
        EmitSignal(SignalName.GaveUp);
    }
}
