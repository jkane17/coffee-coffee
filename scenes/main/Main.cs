using Godot;

/// <summary>Root of the game. Owns the top-level scenes and connects gameplay to UI so neither has to know about the other.</summary>
public partial class Main : Node
{
    [Export] public Shop Shop { get; set; } = null!;
    [Export] public Hud Hud { get; set; } = null!;

    public override void _Ready()
    {
        Hud.ShowTill(Shop.Till);
        Hud.ShowBrewer(Shop.Brewer);
        Hud.ShowClock(Shop.Clock);

        Shop.DayEnded += Hud.ShowDaySummary;
        Hud.NextDayRequested += Shop.StartDay;

        Shop.StartDay();
    }
}
