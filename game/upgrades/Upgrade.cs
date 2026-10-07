using Godot;

/// <summary>Something the shop can buy between days. Create one as a .tres file per upgrade under resources/upgrades/,
/// and add it to the <see cref="UpgradeCatalog"/>.</summary>
[GlobalClass]
public partial class Upgrade : Resource
{
    /// <summary>Stored in the save file to remember the upgrade was bought, so don't change it once players have saves.</summary>
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public UpgradeCategory Category { get; set; }
    [Export(PropertyHint.Range, "0.05,1000,0.05,prefix:$")] public float CostDollars { get; set; } = 10f;
    /// <summary>An upgrade that must be bought first, e.g. comfy shoes before running shoes. Leave empty for none.</summary>
    [Export] public Upgrade? Requires { get; set; }

    [ExportGroup("Effect")]
    [Export] public UpgradeEffect Effect { get; set; }
    [Export] public int Amount { get; set; }

    public Money Cost => Money.FromDollars(CostDollars);
}
