using Godot;

namespace CardChessDemo.UI.Dialogue;

[GlobalClass]
public partial class DialoguePage : Resource
{
	[Export] public string Speaker { get; set; } = string.Empty;
	[Export(PropertyHint.MultilineText)] public string Content { get; set; } = string.Empty;
}
