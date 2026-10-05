using Godot;

/// <summary>An Area2D the player can click. Outlines a sprite while highlighted, using outline.gdshader.</summary>
public partial class Clickable : Area2D
{
    private static readonly StringName OutlineEnabledParameter = "enabled";

    /// <summary>The visual to outline. Its Material must be a ShaderMaterial using outline.gdshader.</summary>
    [Export] public CanvasItem OutlineTarget { get; set; } = null!;

    public void SetHighlighted(bool highlighted)
    {
        if (OutlineTarget.Material is ShaderMaterial material)
        {
            material.SetShaderParameter(OutlineEnabledParameter, highlighted);
        }
        else
        {
            GD.PushWarning($"{GetPath()}: OutlineTarget has no ShaderMaterial, so it can't be outlined.");
        }
    }
}
