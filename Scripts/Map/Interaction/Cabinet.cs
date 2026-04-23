using System;

namespace CardChessDemo.Map;

// Cabinet remains as a compatibility wrapper so old scene references and tile sources
// keep working while behavior is unified on Chest.
public partial class Cabinet : Chest
{
	public override void _Ready()
	{
		ChestName = string.IsNullOrWhiteSpace(ChestName) || ChestName == "宝箱" ? "储物柜" : ChestName;
		PromptText = string.IsNullOrWhiteSpace(PromptText) ? "搜索柜子" : PromptText;
		EmptyDescription = string.IsNullOrWhiteSpace(EmptyDescription) || EmptyDescription == "箱子是空的。"
			? "这个柜子已经被搜空了。"
			: EmptyDescription;
		if (string.IsNullOrWhiteSpace(LootItemId) && string.IsNullOrWhiteSpace(GrantedItemId))
		{
			LootItemId = "steel_scrap";
			LootAmount = Math.Max(1, LootAmount);
		}

		base._Ready();
	}
}
