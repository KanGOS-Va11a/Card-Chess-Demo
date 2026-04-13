using Godot;

namespace CardChessDemo.Map;

[GlobalClass]
public partial class InteractableItemGrant : Resource
{
	[Export] public string ItemId { get; set; } = string.Empty;
	[Export(PropertyHint.Range, "1,999,1")] public int Amount { get; set; } = 1;
}
