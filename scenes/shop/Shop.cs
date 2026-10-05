using System.Collections.Generic;
using Godot;

/// <summary>The shop floor: spawns customers at the door on a timer and lines them up at the counter.</summary>
public partial class Shop : Node2D
{
    [Export] public PackedScene CustomerScene { get; set; } = null!;
    [Export] public Marker2D Door { get; set; } = null!;
    [Export] public Marker2D Counter { get; set; } = null!;
    [Export] public Timer SpawnTimer { get; set; } = null!;

    [ExportGroup("Queue")]
    [Export(PropertyHint.Range, "1,20,1")] public int MaxQueueLength { get; set; } = 5;
    [Export] public float QueueSpacing { get; set; } = 60f;

    private readonly List<Customer> _queue = new();

    public override void _Ready()
    {
        SpawnTimer.Timeout += OnSpawnTimerTimeout;
        SpawnCustomer();
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
        customer.Arrived += OnCustomerArrived;
        customer.WalkTo(GetQueueSlotPosition(_queue.Count));
        _queue.Add(customer);
    }

    /// <summary>Slot 0 is at the counter; each later slot is one step further back down the line.</summary>
    private Vector2 GetQueueSlotPosition(int index)
    {
        return Counter.GlobalPosition + Vector2.Down * QueueSpacing * index;
    }

    private void OnCustomerArrived()
    {
        GD.Print($"A customer joined the queue ({_queue.Count} waiting).");
    }
}
