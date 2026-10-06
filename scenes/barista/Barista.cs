using System;
using Godot;

/// <summary>The player's barista: walks where they're sent, then does what was asked on arrival, and shows what they're carrying.</summary>
public partial class Barista : Node2D
{
    [Export] public Walker Walker { get; set; } = null!;
    [Export] public Sprite2D HeldItemSprite { get; set; } = null!;

    private Action? _onArrived;

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

    /// <summary>Show the item being carried, or nothing when <paramref name="item"/> is null.
    /// The barista only displays it; what they're actually holding is tracked by <see cref="CoffeeBar"/>.</summary>
    public void Hold(Texture2D? item)
    {
        HeldItemSprite.Texture = item;
        HeldItemSprite.Visible = item is not null;
    }

    private void OnWalkerArrived()
    {
        // Clear before invoking, so the action can safely start a new walk.
        Action? onArrived = _onArrived;
        _onArrived = null;
        onArrived?.Invoke();
    }
}
