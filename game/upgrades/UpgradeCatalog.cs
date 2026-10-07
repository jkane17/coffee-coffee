using Godot;

/// <summary>Every upgrade in the game, in the order the upgrades page lists them.</summary>
[GlobalClass]
public partial class UpgradeCatalog : Resource
{
    [Export] public Godot.Collections.Array<Upgrade> Upgrades { get; set; } = new();
}
