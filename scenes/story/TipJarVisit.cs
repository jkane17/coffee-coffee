using System;
using Godot;

/// <summary>Before the day tips start, the relative pops in to ask how the first day went, brings a tip jar, explains tipping,
/// and suggests cleaning the place up if it hasn't been. Calls back once they've left, so the day can begin.</summary>
public partial class TipJarVisit : Node
{
    /// <summary>Asking how it went, up to handing over the jar.</summary>
    [Export] public Dialogue Greeting { get; set; } = null!;
    /// <summary>How tipping works, said once the jar is on the counter.</summary>
    [Export] public Dialogue Explanation { get; set; } = null!;
    /// <summary>Cleaning up makes customers more patient, so they tip more. Skipped if <see cref="CleanUp"/> is already owned.</summary>
    [Export] public Dialogue CleanUpSuggestion { get; set; } = null!;
    [Export] public Upgrade CleanUp { get; set; } = null!;
    /// <summary>Goodbyes, before the relative leaves.</summary>
    [Export] public Dialogue Farewell { get; set; } = null!;
    [Export] public Texture2D RelativeLook { get; set; } = null!;

    private Shop _shop = null!;
    private Hud _hud = null!;
    private Customer? _relative;
    private Action? _onFinished;

    public void Play(Shop shop, Hud hud, Action onFinished)
    {
        _shop = shop;
        _hud = hud;
        _onFinished = onFinished;

        _relative = _shop.AdmitCustomer(SetUpRelative, thinksOnArrival: false);
        _hud.DialogueBox.Play(Greeting, OnGreetingFinished);
    }

    /// <summary>Runs before the relative enters the scene tree, so their _Ready already sees these.</summary>
    private void SetUpRelative(Customer relative)
    {
        relative.Looks = new Godot.Collections.Array<Texture2D> { RelativeLook };
        relative.HasUnlimitedPatience = true;
    }

    private void OnGreetingFinished()
    {
        _shop.EnableTips();
        _hud.DialogueBox.Play(Explanation, OnExplanationFinished);
    }

    private void OnExplanationFinished()
    {
        if (_shop.Upgrades.IsOwned(CleanUp))
        {
            _hud.DialogueBox.Play(Farewell, OnFarewellFinished);
            return;
        }

        _hud.DialogueBox.Play(CleanUpSuggestion, () => _hud.DialogueBox.Play(Farewell, OnFarewellFinished));
    }

    private void OnFarewellFinished()
    {
        if (_relative is null)
        {
            Finish();
            return;
        }

        // A customer frees itself after walking out of the door, which takes it out of the scene tree.
        _relative.TreeExited += OnRelativeLeft;
        _relative.LeaveThrough(_shop.ExitPosition);
    }

    private void OnRelativeLeft()
    {
        _relative = null;
        Finish();
    }

    private void Finish()
    {
        Action? onFinished = _onFinished;
        _onFinished = null;
        onFinished?.Invoke();
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
