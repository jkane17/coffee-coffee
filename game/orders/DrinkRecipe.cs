using Godot;

/// <summary>A drink on the menu. Create one as a .tres file per drink under resources/drinks/.</summary>
[GlobalClass]
public partial class DrinkRecipe : Resource
{
    [Export] public string DisplayName { get; set; } = "";
    [Export(PropertyHint.Range, "1,100,1")] public int Price { get; set; } = 3;
}
