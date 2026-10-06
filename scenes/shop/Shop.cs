using System;
using System.Collections.Generic;
using Godot;

/// <summary>The shop floor: runs the working day, spawns customers while open, gives each a spot at the service counter,
/// and turns the player's clicks into barista actions (taking orders, brewing and serving).</summary>
public partial class Shop : Node2D
{
    [Export] public PackedScene CustomerScene { get; set; } = null!;
    [Export] public Marker2D Door { get; set; } = null!;
    [Export] public Timer SpawnTimer { get; set; } = null!;
    [Export] public Godot.Collections.Array<DrinkRecipe> Menu { get; set; } = new();

    [ExportGroup("Barista")]
    [Export] public Barista Barista { get; set; } = null!;
    [Export] public Area2D BrewStationArea { get; set; } = null!;
    /// <summary>Where the barista stands to use the brew station.</summary>
    [Export] public Marker2D BrewSpot { get; set; } = null!;
    /// <summary>Where the barista stands to serve the customer at the first counter spot. Later spots are spaced like the customers' spots.</summary>
    [Export] public Marker2D ServeSpot { get; set; } = null!;
    /// <summary>The area behind the service counter the barista can walk in. Floor clicks outside it are clamped into it.</summary>
    [Export] public Control WorkArea { get; set; } = null!;

    [ExportGroup("Queue")]
    /// <summary>Where the first customer stands, near the bottom of the service counter. Later spots run up the counter from here.</summary>
    [Export] public Marker2D QueueStart { get; set; } = null!;
    [Export(PropertyHint.Range, "1,20,1")] public int MaxQueueLength { get; set; } = 4;
    [Export] public float QueueSpacing { get; set; } = 60f;

    [ExportGroup("Day")]
    [Export(PropertyHint.Range, "10,600,5,suffix:s")] public float DayLengthSeconds { get; set; } = 120f;
    [Export(PropertyHint.Range, "0,23,1")] public int OpenHour { get; set; } = 8;
    [Export(PropertyHint.Range, "1,24,1")] public int CloseHour { get; set; } = 17;

    /// <summary>Who is standing at each spot along the service counter. Customers keep their spot until they leave.</summary>
    private Customer?[] _spots = Array.Empty<Customer?>();

    /// <summary>Taken orders that haven't been brewed yet, oldest first.</summary>
    private readonly List<Customer> _orderTickets = new();

    /// <summary>Who the drink being brewed or carried is for.</summary>
    private Customer? _drinkFor;
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

        _spots = new Customer?[MaxQueueLength];
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

    /// <summary>Send the barista to whatever was clicked: the brew station, a customer (to take their order or serve them), or a spot on the floor.</summary>
    private void OnClick(Vector2 globalPoint)
    {
        // The barista stays at the brew station until the drink is ready.
        if (Brewer.IsBusy)
        {
            return;
        }

        Area2D? clicked = FindAreaAt(globalPoint);

        if (clicked == BrewStationArea)
        {
            Barista.WalkTo(BrewSpot.GlobalPosition, UseBrewStation);
        }
        else if (clicked?.GetParent() is Customer customer)
        {
            int spot = Array.IndexOf(_spots, customer);
            if (spot >= 0)
            {
                Barista.WalkTo(GetServePosition(spot), () => AttendTo(customer));
            }
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
        if (area == BrewStationArea)
        {
            return CanUseBrewStation();
        }

        return area.GetParent() is Customer customer && (CanTakeOrder(customer) || CanServe(customer));
    }

    /// <summary>The brew station is usable when it's free, the barista's hands are empty, and there's an order waiting to be made.</summary>
    private bool CanUseBrewStation()
    {
        return !Brewer.IsBusy && Barista.HeldDrink is null && _orderTickets.Count > 0;
    }

    /// <summary>An order can be taken from any customer at the counter once they've decided what they want.</summary>
    private bool CanTakeOrder(Customer customer)
    {
        return customer.State == CustomerState.ReadyToOrder;
    }

    /// <summary>A customer can be served when the barista is carrying the drink that was made for them.</summary>
    private bool CanServe(Customer customer)
    {
        return Barista.HeldDrink is not null && customer == _drinkFor;
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

    /// <summary>Brew the oldest order that's still waiting, if the barista's hands are free.</summary>
    private void UseBrewStation()
    {
        if (!CanUseBrewStation())
        {
            return;
        }

        Customer customer = _orderTickets[0];
        _orderTickets.RemoveAt(0);
        _drinkFor = customer;
        customer.StartBeingServed();
        Brewer.Start(customer.Order!);
    }

    private void OnBrewFinished(DrinkRecipe drink)
    {
        Barista.PickUp(drink);
    }

    /// <summary>Do whatever the customer needs right now: take their order, or hand over their drink.
    /// Checked on arrival, because things may have changed while the barista was walking over.</summary>
    private void AttendTo(Customer customer)
    {
        // They may have given up and walked out while the barista was on the way.
        if (!IsInstanceValid(customer))
        {
            return;
        }

        if (CanTakeOrder(customer))
        {
            TakeOrderFrom(customer);
        }
        else
        {
            ServeCustomer(customer);
        }
    }

    private void TakeOrderFrom(Customer customer)
    {
        if (Menu.Count == 0)
        {
            return;
        }

        customer.TakeOrder(Menu[Random.Shared.Next(Menu.Count)]);
        _orderTickets.Add(customer);
    }

    /// <summary>Hand the held drink to the customer, if it was made for them.</summary>
    private void ServeCustomer(Customer customer)
    {
        if (!CanServe(customer))
        {
            return;
        }

        DrinkRecipe drink = Barista.HeldDrink!;
        Barista.HandOver();
        _drinkFor = null;
        FreeSpotOf(customer);
        customer.LeaveThrough(Door.GlobalPosition);
        Till.AddSale(drink.Price);
        _today.RecordSale(drink.Price);
        GD.Print($"Served a {drink.DisplayName} for $ {drink.Price}.");

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
        if (_isDayRunning && !Clock.IsOpen && CountWaitingCustomers() == 0)
        {
            _isDayRunning = false;
            _daysCompleted = _today.DayNumber;
            DayEnded?.Invoke(_today);
        }
    }

    private void OnSpawnTimerTimeout()
    {
        SpawnCustomer();
    }

    /// <summary>Send a new customer in to the first free spot at the counter. Does nothing if every spot is taken.</summary>
    private void SpawnCustomer()
    {
        int spot = Array.IndexOf(_spots, null);
        if (spot < 0)
        {
            return;
        }

        Customer customer = CustomerScene.Instantiate<Customer>();
        AddChild(customer);
        customer.GlobalPosition = Door.GlobalPosition;
        // Everyone starts deciding what to order as soon as they reach their spot.
        customer.Arrived += customer.StartThinking;
        customer.GaveUp += () => OnCustomerGaveUp(customer);
        customer.WalkTo(GetSpotPosition(spot));
        _spots[spot] = customer;
    }

    /// <summary>An impatient customer leaves without paying, and their spot and any order ticket are cleared.</summary>
    private void OnCustomerGaveUp(Customer customer)
    {
        FreeSpotOf(customer);
        _orderTickets.Remove(customer);
        customer.LeaveThrough(Door.GlobalPosition);
        _today.RecordLostCustomer();
        GD.Print($"A customer gave up waiting ({CountWaitingCustomers()} still waiting).");

        EndDayIfFinished();
    }

    private void FreeSpotOf(Customer customer)
    {
        int spot = Array.IndexOf(_spots, customer);
        if (spot >= 0)
        {
            _spots[spot] = null;
        }
    }

    private int CountWaitingCustomers()
    {
        int count = 0;
        foreach (Customer? customer in _spots)
        {
            if (customer is not null)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Spot 0 is near the bottom of the service counter; each later spot is one step further up it.</summary>
    private Vector2 GetSpotPosition(int index)
    {
        return QueueStart.GlobalPosition + Vector2.Up * QueueSpacing * index;
    }

    /// <summary>Where the barista stands, across the counter from a customer's spot.</summary>
    private Vector2 GetServePosition(int index)
    {
        return ServeSpot.GlobalPosition + Vector2.Up * QueueSpacing * index;
    }
}
