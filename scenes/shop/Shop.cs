using System.Collections.Generic;
using Godot;

/// <summary>The shop floor: spawns customers at the door on a timer, lines them up at the counter, and serves them.</summary>
public partial class Shop : Node2D
{
    private static readonly StringName ServeAction = "serve";

    [Export] public PackedScene CustomerScene { get; set; } = null!;
    [Export] public Marker2D Door { get; set; } = null!;
    [Export] public Marker2D Counter { get; set; } = null!;
    [Export] public Timer SpawnTimer { get; set; } = null!;

    [ExportGroup("Queue")]
    [Export(PropertyHint.Range, "1,20,1")] public int MaxQueueLength { get; set; } = 5;
    [Export] public float QueueSpacing { get; set; } = 60f;

    private readonly List<Customer> _queue = new();
    private int _servedCount;

    public override void _Ready()
    {
        SpawnTimer.Timeout += OnSpawnTimerTimeout;
        SpawnCustomer();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(ServeAction))
        {
            ServeNextCustomer();
        }
    }

    private void OnSpawnTimerTimeout()
    {
        if (_queue.Count < MaxQueueLength)
        {
            SpawnCustomer();
        }
    }

    private void SpawnCustomer()
    {
        Customer customer = CustomerScene.Instantiate<Customer>();
        AddChild(customer);
        customer.GlobalPosition = Door.GlobalPosition;
        customer.WalkTo(GetQueueSlotPosition(_queue.Count));
        _queue.Add(customer);
    }

    /// <summary>Serve the customer at the front of the line, if they've reached the counter.</summary>
    private void ServeNextCustomer()
    {
        if (_queue.Count == 0 || _queue[0].IsWalking)
        {
            return;
        }

        Customer served = _queue[0];
        _queue.RemoveAt(0);
        served.LeaveThrough(Door.GlobalPosition);
        _servedCount++;
        GD.Print($"Served customer #{_servedCount} ({_queue.Count} still waiting).");

        MoveQueueForward();
    }

    private void MoveQueueForward()
    {
        for (int i = 0; i < _queue.Count; i++)
        {
            _queue[i].WalkTo(GetQueueSlotPosition(i));
        }
    }

    /// <summary>Slot 0 is at the counter; each later slot is one step further back down the line.</summary>
    private Vector2 GetQueueSlotPosition(int index)
    {
        return Counter.GlobalPosition + Vector2.Down * QueueSpacing * index;
    }
}
