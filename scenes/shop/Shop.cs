using System;
using System.Collections.Generic;
using Godot;

/// <summary>The shop floor: runs the working day, spawns customers while open, lines them up,
/// and turns the player's clicks into barista actions (brewing and serving).</summary>
public partial class Shop : Node2D
{
    [Export] public PackedScene CustomerScene { get; set; } = null!;
    [Export] public Marker2D Door { get; set; } = null!;
    [Export] public Marker2D Counter { get; set; } = null!;
    [Export] public Timer SpawnTimer { get; set; } = null!;
    [Export] public Godot.Collections.Array<DrinkRecipe> Menu { get; set; } = new();

    [ExportGroup("Barista")]
    [Export] public Barista Barista { get; set; } = null!;
    [Export] public Area2D CoffeeMachineArea { get; set; } = null!;
    /// <summary>Where the barista stands to use the coffee machine.</summary>
    [Export] public Marker2D MachineSpot { get; set; } = null!;
    /// <summary>Where the barista stands to serve the front customer.</summary>
    [Export] public Marker2D ServeSpot { get; set; } = null!;
    /// <summary>The area behind the counter the barista can walk in. Floor clicks outside it are clamped into it.</summary>
    [Export] public Control WorkArea { get; set; } = null!;

    [ExportGroup("Queue")]
    [Export(PropertyHint.Range, "1,20,1")] public int MaxQueueLength { get; set; } = 5;
    [Export] public float QueueSpacing { get; set; } = 60f;

    [ExportGroup("Day")]
    [Export(PropertyHint.Range, "10,600,5,suffix:s")] public float DayLengthSeconds { get; set; } = 120f;
    [Export(PropertyHint.Range, "0,23,1")] public int OpenHour { get; set; } = 8;
    [Export(PropertyHint.Range, "1,24,1")] public int CloseHour { get; set; } = 17;

    private readonly List<Customer> _queue = new();
    private DayStats _today = new(0);
    private int _daysCompleted;
    private bool _isDayRunning;
    private Clickable? _highlighted;

    /// <summary>The shop's money. Exposed so UI can observe it. Replaced by <see cref="RestoreProgress"/>.</summary>
    public Till Till { get; private set; } = new();

    /// <summary>Brews the front customer's order. Exposed so UI can observe it.</summary>
    public Brewer Brewer { get; } = new();

    /// <summary>Opening hours for the current day. Created in _Ready from the Day exports.</summary>
    public DayClock Clock { get; private set; } = null!;

    /// <summary>How many full days the shop has finished, including any from a loaded save.</summary>
    public int DaysCompleted => _daysCompleted;

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
        UpdateHighlight();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            OnClick(GetGlobalMousePosition());
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>Continue from saved progress. Must be called before the first day starts and before anything observes <see cref="Till"/>.</summary>
    public void RestoreProgress(SaveData save)
    {
        if (_isDayRunning || _daysCompleted > 0)
        {
            throw new InvalidOperationException("Progress can only be restored before the first day starts.");
        }

        Till = new Till(save.Money);
        _daysCompleted = save.DaysCompleted;
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
        _today = new DayStats(_daysCompleted + 1);
        Clock.Open();
        SpawnTimer.Start();
        SpawnCustomer();
        GD.Print($"Day {_today.DayNumber}: the shop is open.");
    }

    /// <summary>Send the barista to whatever was clicked: the coffee machine, a customer, or a spot on the floor.</summary>
    private void OnClick(Vector2 globalPoint)
    {
        // The barista stays at the machine until the drink is ready.
        if (Brewer.IsBusy)
        {
            return;
        }

        Area2D? clicked = FindAreaAt(globalPoint);

        if (clicked == CoffeeMachineArea)
        {
            Barista.WalkTo(MachineSpot.GlobalPosition, UseCoffeeMachine);
        }
        else if (clicked?.GetParent() is Customer customer)
        {
            Barista.WalkTo(ServeSpot.GlobalPosition, () => ServeCustomer(customer));
        }
        else
        {
            Rect2 workArea = WorkArea.GetGlobalRect();
            Barista.WalkTo(globalPoint.Clamp(workArea.Position, workArea.End));
        }
    }

    /// <summary>Outline whatever is under the mouse if clicking it would do something right now, and show a hand cursor.
    /// Checked every frame, because what's usable changes even when the mouse doesn't move (e.g. a brew finishing).</summary>
    private void UpdateHighlight()
    {
        Clickable? hovered = FindAreaAt(GetGlobalMousePosition()) as Clickable;
        Clickable? usable = hovered is not null && CanUse(hovered) ? hovered : null;

        if (usable == _highlighted)
        {
            return;
        }

        // The previous target may be a customer who has since been freed.
        if (_highlighted is not null && IsInstanceValid(_highlighted))
        {
            _highlighted.SetHighlighted(false);
        }

        _highlighted = usable;
        _highlighted?.SetHighlighted(true);
        Input.SetDefaultCursorShape(_highlighted is null ? Input.CursorShape.Arrow : Input.CursorShape.PointingHand);
    }

    private bool CanUse(Area2D area)
    {
        if (area == CoffeeMachineArea)
        {
            return CanUseCoffeeMachine();
        }

        return area.GetParent() is Customer customer && CanServe(customer);
    }

    /// <summary>The machine is usable when it's free, the barista's hands are empty, and the front customer has ordered.</summary>
    private bool CanUseCoffeeMachine()
    {
        return !Brewer.IsBusy && Barista.HeldDrink is null && _queue.Count > 0 && _queue[0].Order is not null;
    }

    /// <summary>A customer can be served when they're at the front and the barista is holding what they ordered.</summary>
    private bool CanServe(Customer customer)
    {
        return _queue.Count > 0 && _queue[0] == customer && Barista.HeldDrink is not null && customer.Order == Barista.HeldDrink;
    }

    /// <summary>Ask the physics engine which Area2D, if any, is under a point.</summary>
    private Area2D? FindAreaAt(Vector2 globalPoint)
    {
        PhysicsPointQueryParameters2D query = new()
        {
            Position = globalPoint,
            CollideWithAreas = true,
            CollideWithBodies = false,
        };

        foreach (Godot.Collections.Dictionary hit in GetWorld2D().DirectSpaceState.IntersectPoint(query))
        {
            if (hit["collider"].AsGodotObject() is Area2D area)
            {
                return area;
            }
        }

        return null;
    }

    /// <summary>Brew the front customer's order, if they've ordered and the barista's hands are free.</summary>
    private void UseCoffeeMachine()
    {
        if (!CanUseCoffeeMachine())
        {
            return;
        }

        _queue[0].StartBeingServed();
        Brewer.Start(_queue[0].Order!);
    }

    private void OnBrewFinished(DrinkRecipe drink)
    {
        Barista.PickUp(drink);
    }

    /// <summary>Hand the held drink to the customer, if they're at the front and it's what they ordered.</summary>
    private void ServeCustomer(Customer customer)
    {
        if (!CanServe(customer))
        {
            return;
        }

        DrinkRecipe drink = Barista.HeldDrink!;
        Barista.HandOver();
        _queue.RemoveAt(0);
        customer.LeaveThrough(Door.GlobalPosition);
        Till.AddSale(drink.Price);
        _today.RecordSale(drink.Price);
        GD.Print($"Served a {drink.DisplayName} for $ {drink.Price}.");

        MoveQueueForward();
        EndDayIfFinished();
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
            _daysCompleted = _today.DayNumber;
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
