using System;
using System.Collections.Generic;
using Godot;

/// <summary>The shop floor: spawns customers on a timer, lines them up, takes their orders and serves them.</summary>
public partial class Shop : Node2D
{
    private static readonly StringName ServeAction = "serve";

    [Export] public PackedScene CustomerScene { get; set; } = null!;
    [Export] public Marker2D Door { get; set; } = null!;
    [Export] public Marker2D Counter { get; set; } = null!;
    [Export] public Timer SpawnTimer { get; set; } = null!;
    [Export] public Godot.Collections.Array<DrinkRecipe> Menu { get; set; } = new();

    [ExportGroup("Queue")]
    [Export(PropertyHint.Range, "1,20,1")] public int MaxQueueLength { get; set; } = 5;
    [Export] public float QueueSpacing { get; set; } = 60f;

    private readonly List<Customer> _queue = new();

    /// <summary>The shop's money. Exposed so UI can observe it.</summary>
    public Till Till { get; } = new();

    /// <summary>Brews the front customer's order. Exposed so UI can observe it.</summary>
    public Brewer Brewer { get; } = new();

    public override void _Ready()
    {
        if (Menu.Count == 0)
        {
            GD.PushError("The shop's Menu is empty. Add DrinkRecipe resources to it in the Inspector.");
        }

        SpawnTimer.Timeout += OnSpawnTimerTimeout;
        Brewer.BrewFinished += OnBrewFinished;
        SpawnCustomer();
    }

    public override void _Process(double delta)
    {
        Brewer.Tick(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(ServeAction))
        {
            StartBrewingNextOrder();
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
        customer.Arrived += () => OnCustomerArrived(customer);
        customer.GaveUp += () => OnCustomerGaveUp(customer);
        customer.WalkTo(GetQueueSlotPosition(_queue.Count));
        _queue.Add(customer);
    }

    /// <summary>A customer who reaches the front of the line places their order.</summary>
    private void OnCustomerArrived(Customer customer)
    {
        bool isAtFront = _queue.Count > 0 && _queue[0] == customer;
        if (isAtFront && customer.Order is null && Menu.Count > 0)
        {
            customer.PlaceOrder(Menu[Random.Shared.Next(Menu.Count)]);
        }
    }

    /// <summary>Start brewing the front customer's order, if they've ordered and the brewer is free.</summary>
    private void StartBrewingNextOrder()
    {
        if (Brewer.IsBusy || _queue.Count == 0 || _queue[0].Order is not DrinkRecipe order)
        {
            return;
        }

        _queue[0].StartBeingServed();
        Brewer.Start(order);
    }

    /// <summary>The finished drink goes to the front customer, who pays and leaves.</summary>
    private void OnBrewFinished(DrinkRecipe drink)
    {
        Customer served = _queue[0];
        _queue.RemoveAt(0);
        served.LeaveThrough(Door.GlobalPosition);
        Till.AddSale(drink.Price);
        GD.Print($"Served a {drink.DisplayName} for $ {drink.Price}.");

        MoveQueueForward();
    }

    /// <summary>An impatient customer leaves without paying, and everyone behind them moves up.</summary>
    private void OnCustomerGaveUp(Customer customer)
    {
        _queue.Remove(customer);
        customer.LeaveThrough(Door.GlobalPosition);
        GD.Print($"A customer gave up waiting ({_queue.Count} still waiting).");

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
