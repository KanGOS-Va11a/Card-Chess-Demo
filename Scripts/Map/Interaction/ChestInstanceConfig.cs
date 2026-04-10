using Godot;

namespace CardChessDemo.Map;

public partial class ChestInstanceConfig : Node2D
{
	[Export] public string ChestName { get; set; } = "宝箱";
	[Export] public string GrantedItemId { get; set; } = string.Empty;
	[Export] public string ItemDescription { get; set; } = "获得了物品。";
	[Export] public string EmptyDescription { get; set; } = "箱子是空的。";
	[Export] public string PromptText { get; set; } = "打开宝箱";

	public override void _Ready()
	{
		Chest? chest = GetNodeOrNull<Chest>("Chest");
		if (chest == null)
		{
			return;
		}

		chest.ChestName = ChestName;
		chest.GrantedItemId = GrantedItemId;
		chest.ItemDescription = ItemDescription;
		chest.EmptyDescription = EmptyDescription;
		chest.PromptText = PromptText;
	}
}
