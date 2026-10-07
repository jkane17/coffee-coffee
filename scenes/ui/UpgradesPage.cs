using System;
using System.Collections.Generic;
using Godot;

/// <summary>Lists every upgrade under its category's tab. Only observes the upgrades and the till: buying is passed on
/// through <see cref="PurchaseRequested"/>, and the rows update when the upgrades or the balance change.</summary>
public partial class UpgradesPage : Control
{
    [Export] public PackedScene RowScene { get; set; } = null!;
    [Export] public Container BaristaList { get; set; } = null!;
    [Export] public Container EquipmentList { get; set; } = null!;
    [Export] public Container ShopList { get; set; } = null!;
    [Export] public Button CloseButton { get; set; } = null!;

    private readonly List<UpgradeRow> _rows = new();
    private UpgradeBook? _upgrades;
    private Till? _till;

    public event Action<Upgrade>? PurchaseRequested;
    public event Action? Closed;

    public override void _Ready()
    {
        Visible = false;
        CloseButton.Pressed += Close;
    }

    public void Open(UpgradeBook upgrades, Till till)
    {
        StopObserving();
        if (upgrades != _upgrades)
        {
            BuildRows(upgrades);
        }

        _upgrades = upgrades;
        _till = till;
        _upgrades.Changed += Refresh;
        _till.BalanceChanged += OnBalanceChanged;
        Refresh();

        Visible = true;
        CloseButton.GrabFocus();
    }

    public override void _ExitTree()
    {
        StopObserving();
    }

    private void Close()
    {
        StopObserving();
        Visible = false;
        Closed?.Invoke();
    }

    private void BuildRows(UpgradeBook upgrades)
    {
        foreach (UpgradeRow row in _rows)
        {
            row.QueueFree();
        }

        _rows.Clear();

        foreach (Upgrade upgrade in upgrades.Catalog)
        {
            UpgradeRow row = RowScene.Instantiate<UpgradeRow>();
            ListFor(upgrade.Category).AddChild(row);
            row.SetUpgrade(upgrade);
            row.BuyPressed += OnBuyPressed;
            _rows.Add(row);
        }
    }

    private Container ListFor(UpgradeCategory category) => category switch
    {
        UpgradeCategory.Barista => BaristaList,
        UpgradeCategory.Equipment => EquipmentList,
        _ => ShopList,
    };

    private void Refresh()
    {
        if (_upgrades is null || _till is null)
        {
            return;
        }

        foreach (UpgradeRow row in _rows)
        {
            row.Refresh(_upgrades, _till);
        }
    }

    private void OnBalanceChanged(Money balance) => Refresh();

    private void OnBuyPressed(Upgrade upgrade)
    {
        PurchaseRequested?.Invoke(upgrade);
    }

    /// <summary>The upgrades and till are plain C# and outlive the page being open; stop listening to them while it's closed.</summary>
    private void StopObserving()
    {
        if (_upgrades is not null)
        {
            _upgrades.Changed -= Refresh;
        }

        if (_till is not null)
        {
            _till.BalanceChanged -= OnBalanceChanged;
        }
    }
}
