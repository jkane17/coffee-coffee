using System;
using Godot;

/// <summary>The player's barista: walks where they're sent, then does what was asked on arrival, and can carry one drink.</summary>
public partial class Barista : Node2D
{
    [Export] public float WalkSpeed { get; set; } = 220f;
    [Export] public Sprite2D HeldDrinkSprite { get; set; } = null!;

    private Vector2? _target;
    private Action? _onArrived;

    /// <summary>The drink the barista is carrying, or null if their hands are empty.</summary>
    public DrinkRecipe? HeldDrink { get; private set; }

    /// <summary>Walk to a point, then run <paramref name="onArrived"/>. A new walk replaces any unfinished one, along with its action.</summary>
    public void WalkTo(Vector2 globalTarget, Action? onArrived = null)
    {
        _target = globalTarget;
        _onArrived = onArrived;
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

    public override void _Process(double delta)
    {
        if (_target is not Vector2 target)
        {
            return;
        }

        GlobalPosition = GlobalPosition.MoveToward(target, WalkSpeed * (float)delta);

        if (GlobalPosition == target)
        {
            _target = null;

            // Clear before invoking, so the action can safely start a new walk.
            Action? onArrived = _onArrived;
            _onArrived = null;
            onArrived?.Invoke();
        }
    }
}
