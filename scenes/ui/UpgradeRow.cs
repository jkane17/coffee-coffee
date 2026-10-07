using System;
using Godot;

/// <summary>One upgrade on the upgrades page: its name, what it does, and a button showing whether it can be bought.</summary>
public partial class UpgradeRow : PanelContainer
{
    [Export] public Label NameLabel { get; set; } = null!;
    [Export] public Label DescriptionLabel { get; set; } = null!;
    [Export] public Button BuyButton { get; set; } = null!;

    private Upgrade _upgrade = null!;

    public event Action<Upgrade>? BuyPressed;

    public override void _Ready()
    {
        BuyButton.Pressed += OnBuyButtonPressed;
    }

    /// <summary>Call once, after the row is added to the tree.</summary>
    public void SetUpgrade(Upgrade upgrade)
    {
        _upgrade = upgrade;
        NameLabel.Text = upgrade.DisplayName;
        DescriptionLabel.Text = upgrade.Description;
    }

    /// <summary>Update the button for whether the upgrade is owned, locked, or affordable.</summary>
    public void Refresh(UpgradeBook upgrades, Till till)
    {
        if (upgrades.IsOwned(_upgrade))
        {
            BuyButton.Text = "Owned";
            BuyButton.Disabled = true;
        }
        else if (!upgrades.IsUnlocked(_upgrade))
        {
            BuyButton.Text = $"Needs {_upgrade.Requires!.DisplayName}";
            BuyButton.Disabled = true;
        }
        else
        {
            BuyButton.Text = $"Buy  {_upgrade.Cost}";
            BuyButton.Disabled = !till.CanAfford(_upgrade.Cost);
        }
    }

    private void OnBuyButtonPressed()
    {
        BuyPressed?.Invoke(_upgrade);
    }
}
