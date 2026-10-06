using System;
using System.Collections.Generic;
using Godot;

/// <summary>The shop floor: runs the working day, spawns customers while open, gives each a spot at the service counter,
/// and turns the player's clicks into barista actions (taking orders, using the brew counter's stations, and serving).</summary>
public partial class Shop : Node2D
{
    [Export] public PackedScene CustomerScene { get; set; } = null!;
    [Export] public Marker2D Door { get; set; } = null!;
    [Export] public Timer SpawnTimer { get; set; } = null!;
    [Export] public Godot.Collections.Array<DrinkRecipe> Menu { get; set; } = new();

    [ExportGroup("Barista")]
    [Export] public Barista Barista { get; set; } = null!;
    /// <summary>How far to the right of a brew counter station the barista stands to use it.</summary>
    [Export] public float StationReach { get; set; } = 48f;
    /// <summary>Where the barista stands to serve the customer at the first counter spot. Later spots are spaced like the customers' spots.</summary>
    [Export] public Marker2D ServeSpot { get; set; } = null!;
    /// <summary>The area behind the service counter the barista can walk in. Floor clicks outside it are clamped into it.</summary>
    [Export] public Control WorkArea { get; set; } = null!;

    [ExportGroup("Brew counter")]
    [Export] public Area2D KettleArea { get; set; } = null!;
    [Export] public Area2D SinkArea { get; set; } = null!;
    [Export] public Area2D CupStackArea { get; set; } = null!;
    [Export] public Area2D GranulesArea { get; set; } = null!;
    /// <summary>The boxes the coffee things are packed in on a new game. Clicking them unpacks the <see cref="PackedStations"/>.</summary>
    [Export] public Area2D BoxesArea { get; set; } = null!;
    /// <summary>Stations that <see cref="PackAwayStations"/> hides in the boxes until the barista unpacks them.</summary>
    [Export] public Godot.Collections.Array<Node2D> PackedStations { get; set; } = new();
    /// <summary>The kettle sprite on its base. Hidden while the barista carries it, and its texture is reused for the carried kettle.</summary>
    [Export] public Sprite2D KettleOnBase { get; set; } = null!;
    /// <summary>The drink made by pouring hot water onto granules.</summary>
    [Export] public DrinkRecipe InstantCoffee { get; set; } = null!;
    [Export(PropertyHint.Range, "1,10,1")] public int KettleCapacityCups { get; set; } = 1;
    [Export(PropertyHint.Range, "0.5,30,0.5,suffix:s")] public float KettleFillSeconds { get; set; } = 3f;
    [Export(PropertyHint.Range, "0.5,60,0.5,suffix:s")] public float KettleBoilSeconds { get; set; } = 6f;

    [ExportGroup("Held item art")]
    [Export] public Texture2D EmptyCupTexture { get; set; } = null!;
    [Export] public Texture2D GranulesCupTexture { get; set; } = null!;

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

    /// <summary>What each brew counter station does when clicked, keyed by its click area. Built in _Ready.</summary>
    private readonly Dictionary<Area2D, StationActions> _stations = new();
    private DayStats _today = new(0);
    private int _daysCompleted;
    private bool _isDayRunning;
    private Clickable? _highlighted;
    private bool _isPacked;

    /// <summary>The shop's money. Exposed so UI can observe it. Replaced by <see cref="RestoreProgress"/>.</summary>
    public Till Till { get; private set; } = new();

    /// <summary>The coffee-making stations and what the barista is carrying. Created in _Ready; exposed so UI can observe the kettle.</summary>
    public CoffeeBar CoffeeBar { get; private set; } = null!;

    /// <summary>Opening hours for the current day. Created in _Ready from the Day exports.</summary>
    public DayClock Clock { get; private set; } = null!;

    /// <summary>How many full days the shop has finished, including any from a loaded save.</summary>
    public int DaysCompleted => _daysCompleted;

    /// <summary>The number the next call to <see cref="StartDay"/> will give the day.</summary>
    public int NextDayNumber => _daysCompleted + 1;

    /// <summary>Where customers come in and leave.</summary>
    public Vector2 ExitPosition => Door.GlobalPosition;

    /// <summary>Raised once the shop has closed and the last customer has left the line.</summary>
    public event Action<DayStats>? DayEnded;

    /// <summary>Raised when the barista unpacks the boxes.</summary>
    public event Action? StationsUnpacked;

    /// <summary>Raised when a customer is handed their drink.</summary>
    public event Action<Customer>? CustomerServed;

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

        CoffeeBar = new CoffeeBar(new Kettle(KettleCapacityCups, KettleFillSeconds, KettleBoilSeconds), InstantCoffee);
        CoffeeBar.Changed += UpdateHeldItem;
        // Nothing on the brew counter can be used while the coffee things are still in the boxes.
        _stations[KettleArea] = new StationActions(() => !_isPacked && CoffeeBar.CanUseKettle, CoffeeBar.UseKettle);
        _stations[SinkArea] = new StationActions(() => !_isPacked && CoffeeBar.CanUseSink, CoffeeBar.UseSink);
        _stations[CupStackArea] = new StationActions(() => !_isPacked && CoffeeBar.CanUseCupStack, CoffeeBar.UseCupStack);
        _stations[GranulesArea] = new StationActions(() => !_isPacked && CoffeeBar.CanUseGranules, CoffeeBar.UseGranules);
        _stations[BoxesArea] = new StationActions(() => _isPacked, Unpack);
        UpdateHeldItem();
    }

    public override void _Process(double delta)
    {
        Clock.Tick(delta);
        CoffeeBar.Tick(delta);
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

    /// <summary>Hide the <see cref="PackedStations"/> in the boxes until the barista unpacks them. Used by the new-game intro.</summary>
    public void PackAwayStations()
    {
        _isPacked = true;
        foreach (Node2D station in PackedStations)
        {
            station.Visible = false;
        }
    }

    /// <summary>Bring in one customer, e.g. outside opening hours. <paramref name="setUp"/> runs before they enter the scene tree.
    /// Returns null if every spot at the counter is taken.</summary>
    public Customer? AdmitCustomer(Action<Customer>? setUp = null, bool thinksOnArrival = true)
    {
        int spot = Array.IndexOf(_spots, null);
        if (spot < 0)
        {
            return null;
        }

        Customer customer = CustomerScene.Instantiate<Customer>();
        setUp?.Invoke(customer);
        AddChild(customer);
        customer.GlobalPosition = Door.GlobalPosition;
        if (thinksOnArrival)
        {
            customer.Arrived += customer.StartThinking;
        }

        customer.GaveUp += () => OnCustomerGaveUp(customer);
        customer.WalkTo(GetSpotPosition(spot));
        _spots[spot] = customer;
        return customer;
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
        _today = new DayStats(NextDayNumber);
        Clock.Open();
        SpawnTimer.Start();
        SpawnCustomer();
        GD.Print($"Day {_today.DayNumber}: the shop is open.");
    }

    /// <summary>Send the barista to whatever was clicked: a brew counter station, a customer (to take their order or serve them), or a spot on the floor.</summary>
    private void OnClick(Vector2 globalPoint)
    {
        // The barista stays at the sink until the kettle is full.
        if (CoffeeBar.IsBusy)
        {
            return;
        }

        Area2D? clicked = FindAreaAt(globalPoint);

        if (clicked is not null && _stations.TryGetValue(clicked, out StationActions station))
        {
            // Checked again on arrival, because things may have changed on the way (e.g. the kettle finished boiling).
            Barista.WalkTo(GetStandPosition(clicked), () =>
            {
                if (station.CanUse())
                {
                    station.Use();
                }
            });
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
            Barista.WalkTo(ClampToWorkArea(globalPoint));
        }
    }

    /// <summary>Where the barista stands to use a station: just to its right, inside the work area.</summary>
    private Vector2 GetStandPosition(Area2D station)
    {
        return ClampToWorkArea(station.GlobalPosition + Vector2.Right * StationReach);
    }

    private Vector2 ClampToWorkArea(Vector2 globalPoint)
    {
        Rect2 workArea = WorkArea.GetGlobalRect();
        return globalPoint.Clamp(workArea.Position, workArea.End);
    }

    /// <summary>Show what the barista is carrying, and whether the kettle is on its base.</summary>
    private void UpdateHeldItem()
    {
        KettleOnBase.Visible = !CoffeeBar.IsHoldingKettle;

        Texture2D? held = null;
        if (CoffeeBar.IsHoldingKettle)
        {
            held = KettleOnBase.Texture;
        }
        else if (CoffeeBar.HeldCup is Cup cup)
        {
            held = cup.Drink?.Icon ?? (cup.HasGranules ? GranulesCupTexture : EmptyCupTexture);
        }

        Barista.Hold(held);
    }

    /// <summary>Outline whatever is under the mouse if clicking it would do something right now, and show a hand cursor.
    /// Checked every frame, because what's usable changes even when the mouse doesn't move (e.g. the kettle boiling).</summary>
    private void UpdateHighlight()
    {
        // Nothing in the shop counts as hovered while the mouse is over UI that blocks clicks, like the dialogue box.
        Clickable? hovered = GetViewport().GuiGetHoveredControl() is null ? FindAreaAt(GetGlobalMousePosition()) as Clickable : null;
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
        if (CoffeeBar.IsBusy)
        {
            return false;
        }

        if (_stations.TryGetValue(area, out StationActions station))
        {
            return station.CanUse();
        }

        return area.GetParent() is Customer customer && (CanTakeOrder(customer) || CanServe(customer));
    }

    /// <summary>An order can be taken from any customer at the counter once they've decided what they want.</summary>
    private bool CanTakeOrder(Customer customer)
    {
        return customer.State == CustomerState.ReadyToOrder;
    }

    /// <summary>A customer can be served when the barista is carrying the drink they ordered.</summary>
    private bool CanServe(Customer customer)
    {
        return customer.State == CustomerState.Ordered && CoffeeBar.HeldDrink is not null && CoffeeBar.HeldDrink == customer.Order;
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
    }

    /// <summary>Hand the held drink to the customer, if it's what they ordered.</summary>
    private void ServeCustomer(Customer customer)
    {
        if (!CanServe(customer))
        {
            return;
        }

        DrinkRecipe drink = CoffeeBar.HandOverDrink();
        FreeSpotOf(customer);
        customer.ReceiveDrink();
        if (!customer.StaysAfterServed)
        {
            customer.LeaveThrough(Door.GlobalPosition);
        }

        // Outside opening hours (the intro), drinks are on the house.
        if (_isDayRunning)
        {
            Till.AddSale(drink.Price);
            _today.RecordSale(drink.Price);
            GD.Print($"Served a {drink.DisplayName} for $ {drink.Price}.");
        }

        CustomerServed?.Invoke(customer);

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
        AdmitCustomer();
    }

    /// <summary>Take the coffee things out of the boxes, popping each station into place one after another.</summary>
    private void Unpack()
    {
        _isPacked = false;

        // A parallel tween runs all its steps at once; the growing delays stagger them.
        Tween tween = CreateTween().SetParallel();
        for (int i = 0; i < PackedStations.Count; i++)
        {
            Node2D station = PackedStations[i];
            Vector2 fullScale = station.Scale;
            station.Scale = Vector2.Zero;
            station.Visible = true;
            tween.TweenProperty(station, Node2D.PropertyName.Scale.ToString(), fullScale, 0.35)
                .SetDelay(i * 0.15)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        }

        StationsUnpacked?.Invoke();
    }

    /// <summary>An impatient customer leaves without paying, freeing their spot.</summary>
    private void OnCustomerGaveUp(Customer customer)
    {
        FreeSpotOf(customer);
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

    /// <summary>A brew counter station's rules: whether clicking it does anything right now, and what it does.</summary>
    private readonly record struct StationActions(Func<bool> CanUse, Action Use);
}
