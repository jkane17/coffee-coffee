using Godot;

/// <summary>A drink on the menu. Create one as a .tres file per drink under resources/drinks/.</summary>
[GlobalClass]
public partial class DrinkRecipe : Resource
{
    [Export] public string DisplayName { get; set; } = "";
    [Export(PropertyHint.Range, "1,100,1")] public int Price { get; set; } = 3;
    [Export(PropertyHint.Range, "0.1,30,0.1,suffix:s")] public float BrewSeconds { get; set; } = 2f;
    [Export] public Texture2D? Icon { get; set; }
}
