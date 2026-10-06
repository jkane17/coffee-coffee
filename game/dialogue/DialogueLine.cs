using Godot;

/// <summary>One thing someone says in a conversation.</summary>
[GlobalClass]
public partial class DialogueLine : Resource
{
    [Export] public string Speaker { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Text { get; set; } = "";
}
