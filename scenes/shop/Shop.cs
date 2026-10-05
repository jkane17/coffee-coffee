using System;
using System.Collections.Generic;
using Godot;

/// <summary>The shop floor: runs the working day, spawns customers while open, lines them up, takes their orders and serves them.</summary>
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

    [ExportGroup("Day")]
    [Export(PropertyHint.Range, "10,600,5,suffix:s")] public float DayLengthSeconds { get; set; } = 120f;
    [Export(PropertyHint.Range, "0,23,1")] public int OpenHour { get; set; } = 8;
    [Export(PropertyHint.Range, "1,24,1")] public int CloseHour { get; set; } = 17;

    private readonly List<Customer> _queue = new();
    private DayStats _today = new(0);
    private bool _isDayRunning;

    /// <summary>The shop's money. Exposed so UI can observe it.</summary>
    public Till Till { get; } = new();

    /// <summary>Brews the front customer's order. Exposed so UI can observe it.</summary>
    public Brewer Brewer { get; } = new();

    /// <summary>Opening hours for the current day. Created in _Ready from the Day exports.</summary>
    public DayClock Clock { get; private set; } = null!;

    /// <summary>Raised once the shop has closed and the last customer has left the line.</summary>
    public event Action<DayStats>? DayEnded;

    public override void _Ready()
    {
        if (Menu.Count == 0)
        {
            GD.PushError("The shop's Menu is empty. Add DrinkRecipe resources to it in the Inspector.");
        }

        Clock = new DayClock(DayLengthSeconds, OpenHour, CloseHour);
        Clock.Closed += OnClosingTime;
        SpawnTimer.Timeout += OnSpawnTimerTimeout;
        Brewer.BrewFinished += OnBrewFinished;
    }

    public override void _Process(double delta)
    {
        Clock.Tick(delta);
        Brewer.Tick(delta);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(ServeAction))
        {
            StartBrewingNextOrder();
        }
    }

    /// <summary>Open the shop for a new day.</summary>
    public void StartDay()
    {
        if (_isDayRunning)
        {
            GD.PushWarning("Can't start a new day while the current one is still running.");
            return;
        }

        _isDayRunning = true;
        _today = new DayStats(_today.DayNumber + 1);
        Clock.Open();
        SpawnTimer.Start();
        SpawnCustomer();
        GD.Print($"Day {_today.DayNumber}: the shop is open.");
    }

    private void OnClosingTime()
    {
        SpawnTimer.Stop();
        GD.Print("Closing time. Serving the last customers in line.");
        EndDayIfFinished();
    }

    /// <summary>The day ends once the shop is closed and nobody is left waiting.</summary>
    private void EndDayIfFinished()
    {
        if (_isDayRunning && !Clock.IsOpen && _queue.Count == 0)
        {
            _isDayRunning = false;
            DayEnded?.Invoke(_today);
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
        _today.RecordSale(drink.Price);
        GD.Print($"Served a {drink.DisplayName} for $ {drink.Price}.");

        MoveQueueForward();
        EndDayIfFinished();
    }

    /// <summary>An impatient customer leaves without paying, and everyone behind them moves up.</summary>
    private void OnCustomerGaveUp(Customer customer)
    {
        _queue.Remove(customer);
        customer.LeaveThrough(Door.GlobalPosition);
        _today.RecordLostCustomer();
        GD.Print($"A customer gave up waiting ({_queue.Count} still waiting).");

        MoveQueueForward();
        EndDayIfFinished();
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
