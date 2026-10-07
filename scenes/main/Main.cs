using System.Linq;
using Godot;

/// <summary>Root of the game. Owns the top-level scenes, connects gameplay to UI so neither has to know about the other,
/// and saves progress at the end of each day.</summary>
public partial class Main : Node
{
    [Export] public Shop Shop { get; set; } = null!;
    [Export] public Hud Hud { get; set; } = null!;
    /// <summary>Plays on a new game, before Day 1.</summary>
    [Export] public Intro Intro { get; set; } = null!;
    /// <summary>Plays before the first day with tips.</summary>
    [Export] public TipJarVisit TipJarVisit { get; set; } = null!;

    private readonly SaveStore _saveStore = new();
    private SaveData? _saveToContinue;

    /// <summary>Continue from a save instead of starting fresh. Call before adding Main to the scene tree.</summary>
    public void ContinueFrom(SaveData save)
    {
        _saveToContinue = save;
    }

    public override void _Ready()
    {
        // Restore before the HUD starts observing the till, so it shows the loaded balance.
        if (_saveToContinue is not null)
        {
            Shop.RestoreProgress(_saveToContinue);
        }

        Hud.ShowTill(Shop.Till);
        Hud.ShowKettle(Shop.CoffeeBar.Kettle);
        Hud.ShowClock(Shop.Clock);
        Hud.ShowUpgrades(Shop.Upgrades);

        Shop.DayEnded += OnDayEnded;
        Hud.NextDayRequested += BeginDay;
        Hud.UpgradePurchaseRequested += OnUpgradePurchaseRequested;

        if (_saveToContinue is null)
        {
            Hud.SetShopInfoVisible(false);
            Intro.Finished += BeginDay;
            Intro.Play(Shop, Hud);
        }
        else
        {
            BeginDay();
        }
    }

    /// <summary>Play any story visit due before the next day, then announce the day with a banner and open the shop once it's gone.</summary>
    private void BeginDay()
    {
        if (!Shop.AreTipsEnabled && Shop.NextDayNumber >= Shop.TipsFromDay)
        {
            TipJarVisit.Play(Shop, Hud, ShowDayBanner);
            return;
        }

        ShowDayBanner();
    }

    private void ShowDayBanner()
    {
        Hud.ShowDayBanner(Shop.NextDayNumber, OpenShop);
    }

    private void OpenShop()
    {
        Hud.SetShopInfoVisible(true);
        Shop.StartDay();
    }

    private void OnDayEnded(DayStats stats)
    {
        SaveProgress();
        Hud.ShowDaySummary(stats);
    }

    /// <summary>Save straight after buying, so quitting before the next day doesn't lose the purchase.</summary>
    private void OnUpgradePurchaseRequested(Upgrade upgrade)
    {
        Shop.BuyUpgrade(upgrade);
        SaveProgress();
    }

    private void SaveProgress()
    {
        _saveStore.Save(new SaveData(SaveData.CurrentVersion, Shop.Till.Balance.Cents, Shop.DaysCompleted, Shop.Upgrades.OwnedIds.ToArray()));
    }
}
