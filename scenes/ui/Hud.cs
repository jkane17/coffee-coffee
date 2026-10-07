using System;
using Godot;

/// <summary>On-screen overlay. Observes gameplay systems and displays their state; never changes them.</summary>
public partial class Hud : CanvasLayer
{
    private const int ClockStepMinutes = 10;

    [Export] public Label MoneyLabel { get; set; } = null!;
    [Export] public Label ClockLabel { get; set; } = null!;
    [Export] public BrewStatus BrewStatus { get; set; } = null!;
    [Export] public DaySummary DaySummary { get; set; } = null!;
    [Export] public UpgradesPage UpgradesPage { get; set; } = null!;
    [Export] public DialogueBox DialogueBox { get; set; } = null!;
    /// <summary>Shown with <see cref="ShowHint"/>; hidden when there's no hint.</summary>
    [Export] public Control HintPanel { get; set; } = null!;
    [Export] public Label HintLabel { get; set; } = null!;

    [ExportGroup("Day banner")]
    /// <summary>Big "Day N" text shown at the start of each day.</summary>
    [Export] public Label DayBanner { get; set; } = null!;
    [Export(PropertyHint.Range, "0.5,10,0.5,suffix:s")] public float DayBannerSeconds { get; set; } = 2f;
    [Export(PropertyHint.Range, "0,3,0.1,suffix:s")] public float DayBannerFadeSeconds { get; set; } = 0.5f;

    private Till? _till;
    private DayClock? _clock;
    private UpgradeBook? _upgrades;

    /// <summary>Raised when the player asks to start the next day from the summary panel.</summary>
    public event Action? NextDayRequested;

    /// <summary>Raised when the player tries to buy an upgrade on the upgrades page.</summary>
    public event Action<Upgrade>? UpgradePurchaseRequested;

    public override void _Ready()
    {
        DaySummary.NextDayPressed += OnNextDayPressed;
        DaySummary.UpgradesPressed += OnUpgradesPressed;
        UpgradesPage.PurchaseRequested += OnUpgradePurchaseRequested;
        UpgradesPage.Closed += DaySummary.Reopen;
        ShowHint(null);
        DayBanner.Visible = false;
    }

    /// <summary>Fade in "Day N", hold it, fade it out, then call <paramref name="onFinished"/>.</summary>
    public void ShowDayBanner(int dayNumber, Action onFinished)
    {
        string modulate = CanvasItem.PropertyName.Modulate.ToString();
        DayBanner.Text = $"Day {dayNumber}";
        DayBanner.Modulate = Colors.Transparent;
        DayBanner.Visible = true;

        // Tween steps run one after another by default; TweenInterval just waits.
        Tween tween = CreateTween();
        tween.TweenProperty(DayBanner, modulate, Colors.White, DayBannerFadeSeconds);
        tween.TweenInterval(DayBannerSeconds);
        tween.TweenProperty(DayBanner, modulate, Colors.Transparent, DayBannerFadeSeconds);
        tween.TweenCallback(Callable.From(() =>
        {
            DayBanner.Visible = false;
            onFinished();
        }));
    }

    /// <summary>Show or hide the shop's money and clock, e.g. hidden during the intro before there's a shop to run.</summary>
    public void SetShopInfoVisible(bool visible)
    {
        MoneyLabel.Visible = visible;
        ClockLabel.Visible = visible;
    }

    /// <summary>Show a hint at the top of the screen, or hide it with null.</summary>
    public void ShowHint(string? hint)
    {
        HintPanel.Visible = hint is not null;
        HintLabel.Text = hint ?? "";
    }

    public void ShowKettle(Kettle kettle) => BrewStatus.ShowKettle(kettle);

    public void ShowDaySummary(DayStats stats) => DaySummary.ShowSummary(stats);

    /// <summary>The upgrades listed on the upgrades page, opened from the day summary.</summary>
    public void ShowUpgrades(UpgradeBook upgrades) => _upgrades = upgrades;

    /// <summary>Start displaying the given clock's time of day.</summary>
    public void ShowClock(DayClock clock) => _clock = clock;

    /// <summary>Start displaying the given till's balance, replacing any till shown before.</summary>
    public void ShowTill(Till till)
    {
        StopShowingTill();
        _till = till;
        _till.BalanceChanged += OnBalanceChanged;
        OnBalanceChanged(_till.Balance);
    }

    public override void _Process(double delta)
    {
        // The time changes every frame, so poll it rather than listening for an event.
        if (_clock is not null)
        {
            ClockLabel.Text = _clock.IsOpen ? FormatTime(_clock.TimeOfDay) : "Closed";
        }
    }

    public override void _ExitTree()
    {
        // Till is plain C# and could outlive this node; unsubscribe so it never calls into a freed HUD.
        StopShowingTill();
    }

    /// <summary>Shows the time in 10-minute steps, like a shop clock, rather than a ticking digital one.</summary>
    private static string FormatTime(TimeSpan time)
    {
        int minutes = time.Minutes / ClockStepMinutes * ClockStepMinutes;
        return $"{time.Hours:00}:{minutes:00}";
    }

    private void StopShowingTill()
    {
        if (_till is not null)
        {
            _till.BalanceChanged -= OnBalanceChanged;
            _till = null;
        }
    }

    private void OnBalanceChanged(int balance)
    {
        MoneyLabel.Text = $"$ {balance}";
    }

    private void OnNextDayPressed()
    {
        NextDayRequested?.Invoke();
    }

    private void OnUpgradesPressed()
    {
        if (_upgrades is null || _till is null)
        {
            GD.PushError("The upgrades page needs ShowUpgrades and ShowTill to be called first.");
            DaySummary.Reopen();
            return;
        }

        UpgradesPage.Open(_upgrades, _till);
    }

    private void OnUpgradePurchaseRequested(Upgrade upgrade)
    {
        UpgradePurchaseRequested?.Invoke(upgrade);
    }
}
