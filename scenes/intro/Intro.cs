using System;
using Godot;

/// <summary>The new-game story. A relative shows the player the shop they've inherited, they unpack the coffee things,
/// and making the relative a coffee doubles as the tutorial. Raises <see cref="Finished"/> when it's time to open for Day 1.</summary>
public partial class Intro : Node
{
    [Export] public Dialogue Welcome { get; set; } = null!;
    [Export] public Dialogue Request { get; set; } = null!;
    [Export] public Dialogue Verdict { get; set; } = null!;

    [ExportGroup("Relative")]
    /// <summary>Used in tutorial hints. Keep it matching the speaker name in the dialogues.</summary>
    [Export] public string RelativeName { get; set; } = "Aunt Bea";
    [Export] public Texture2D RelativeLook { get; set; } = null!;

    private Shop _shop = null!;
    private Hud _hud = null!;
    private Customer? _relative;
    private bool _isTutorialRunning;

    public event Action? Finished;

    public void Play(Shop shop, Hud hud)
    {
        _shop = shop;
        _hud = hud;

        _shop.PackAwayStations();
        _relative = _shop.AdmitCustomer(SetUpRelative, thinksOnArrival: false)
            ?? throw new InvalidOperationException("There was no free spot at the counter for the relative.");
        _hud.DialogueBox.Play(Welcome, OnWelcomeFinished);
    }

    public override void _Process(double delta)
    {
        // Poll, because any click can change which step comes next.
        if (_isTutorialRunning && _relative is not null)
        {
            _hud.ShowHint(CoffeeTutorial.NextHint(_relative.State, _shop.CoffeeBar, RelativeName));
        }
    }

    /// <summary>Runs before the relative enters the scene tree, so their _Ready already sees these.</summary>
    private void SetUpRelative(Customer relative)
    {
        relative.Looks = new Godot.Collections.Array<Texture2D> { RelativeLook };
        relative.HasUnlimitedPatience = true;
        relative.StaysAfterServed = true;
    }

    private void OnWelcomeFinished()
    {
        _hud.ShowHint("Click the boxes to see what's inside.");
        _shop.StationsUnpacked += OnStationsUnpacked;
    }

    private void OnStationsUnpacked()
    {
        _shop.StationsUnpacked -= OnStationsUnpacked;
        _hud.ShowHint(null);
        _hud.DialogueBox.Play(Request, OnRequestFinished);
    }

    private void OnRequestFinished()
    {
        _isTutorialRunning = true;
        _shop.CustomerServed += OnCustomerServed;
        _relative?.StartThinking();
    }

    private void OnCustomerServed(Customer customer)
    {
        if (customer != _relative)
        {
            return;
        }

        _shop.CustomerServed -= OnCustomerServed;
        _isTutorialRunning = false;
        _hud.ShowHint(null);
        _hud.DialogueBox.Play(Verdict, OnVerdictFinished);
    }

    /// <summary>The relative heads out; the intro finishes once they've gone through the door.</summary>
    private void OnVerdictFinished()
    {
        if (_relative is null)
        {
            Finished?.Invoke();
            return;
        }

        // A customer frees itself after walking out of the door, which takes it out of the scene tree.
        _relative.TreeExited += OnRelativeLeft;
        _relative.LeaveThrough(_shop.ExitPosition);
    }

    private void OnRelativeLeft()
    {
        _relative = null;
        Finished?.Invoke();
    }

    public override void _ExitTree()
    {
        // When the whole game scene is freed (e.g. quitting to the title), don't react to the relative being freed with it.
        if (_relative is not null && IsInstanceValid(_relative))
        {
            _relative.TreeExited -= OnRelativeLeft;
        }
    }
}
