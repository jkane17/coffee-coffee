using Godot;

/// <summary>A drink on the menu. Create one as a .tres file per drink under resources/drinks/.</summary>
[GlobalClass]
public partial class DrinkRecipe : Resource
{
    [Export] public string DisplayName { get; set; } = "";
    /// <summary>In dollars, in steps of 5 cents so tips and balances stay multiples of 5 cents.</summary>
    [Export(PropertyHint.Range, "0.05,100,0.05,prefix:$")] public float PriceDollars { get; set; } = 3f;
    [Export(PropertyHint.Range, "0.1,30,0.1,suffix:s")] public float BrewSeconds { get; set; } = 2f;
    [Export] public Texture2D? Icon { get; set; }

    public Money Price => Money.FromDollars(PriceDollars);
}
