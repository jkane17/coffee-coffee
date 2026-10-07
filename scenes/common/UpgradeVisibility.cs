using Godot;

/// <summary>Component that shows or hides its parent depending on whether an upgrade is owned,
/// e.g. a cobweb that disappears after a deep clean. Add it as a child of the sprite.</summary>
public partial class UpgradeVisibility : Node
{
    /// <summary>Every UpgradeVisibility joins this group, so the shop can find them all to refresh.</summary>
    public static readonly StringName GroupName = "upgrade_visibility";

    [Export] public Upgrade Upgrade { get; set; } = null!;
    /// <summary>On: the parent appears once the upgrade is owned. Off: it disappears.</summary>
    [Export] public bool VisibleWhenOwned { get; set; } = true;

    public override void _EnterTree()
    {
        AddToGroup(GroupName);
    }

    public void Refresh(UpgradeBook upgrades)
    {
        GetParent<CanvasItem>().Visible = upgrades.IsOwned(Upgrade) == VisibleWhenOwned;
    }
}
