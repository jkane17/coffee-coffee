using Godot;

/// <summary>A conversation: lines shown one after another in a <see cref="DialogueBox"/>.</summary>
[GlobalClass]
public partial class Dialogue : Resource
{
    [Export] public Godot.Collections.Array<DialogueLine> Lines { get; set; } = new();
}
