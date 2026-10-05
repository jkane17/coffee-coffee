using System;
using Godot;

/// <summary>The player's barista: walks where they're sent, then does what was asked on arrival, and can carry one drink.</summary>
public partial class Barista : Node2D
{
    [Export] public Walker Walker { get; set; } = null!;
    [Export] public Sprite2D HeldDrinkSprite { get; set; } = null!;

    private Action? _onArrived;

    /// <summary>The drink the barista is carrying, or null if their hands are empty.</summary>
    public DrinkRecipe? HeldDrink { get; private set; }

    public override void _Ready()
    {
        Walker.Arrived += OnWalkerArrived;
    }

    /// <summary>Walk to a point, then run <paramref name="onArrived"/>. A new walk replaces any unfinished one, along with its action.</summary>
    public void WalkTo(Vector2 globalTarget, Action? onArrived = null)
    {
        _onArrived = onArrived;
        Walker.WalkTo(globalTarget);
    }

    public void PickUp(DrinkRecipe drink)
    {
        HeldDrink = drink;
        HeldDrinkSprite.Texture = drink.Icon;
        HeldDrinkSprite.Visible = true;
    }

    public void HandOver()
    {
        HeldDrink = null;
        HeldDrinkSprite.Visible = false;
    }

    private void OnWalkerArrived()
    {
        // Clear before invoking, so the action can safely start a new walk.
        Action? onArrived = _onArrived;
        _onArrived = null;
        onArrived?.Invoke();
    }
}
