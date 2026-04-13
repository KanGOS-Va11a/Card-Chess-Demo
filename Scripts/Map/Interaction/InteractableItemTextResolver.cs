using System.Collections.Generic;
using System.Linq;
using CardChessDemo.Battle.Shared;
using CardChessDemo.Battle.Equipment;

namespace CardChessDemo.Map;

public static class InteractableItemTextResolver
{
	public static string BuildLootSummary(GlobalGameSession? session, IEnumerable<(string ItemId, int Amount)> items)
	{
		List<string> parts = items
			.Where(item => !string.IsNullOrWhiteSpace(item.ItemId) && item.Amount > 0)
			.Select(item => $"{GetItemDisplayName(session, item.ItemId)} x{item.Amount}")
			.ToList();

		if (parts.Count == 0)
		{
			return "什么也没有。";
		}

		return $"获得了 {string.Join("、", parts)}";
	}

	public static string GetItemDisplayName(GlobalGameSession? session, string itemId)
	{
		if (string.IsNullOrWhiteSpace(itemId))
		{
			return string.Empty;
		}

		if (session?.FindEquipmentDefinition(itemId.Trim()) is EquipmentDefinition equipmentDefinition)
		{
			return equipmentDefinition.DisplayName;
		}

		return itemId.Trim() switch
		{
			"steel_scrap" => "钢铁碎片",
			"charged_core" => "充能核心",
			"arakawa_battery" => "荒川充能电池",
			"medical_gel" => "医疗凝胶",
			"optical_part" => "光学零件",
			_ => itemId.Trim(),
		};
	}
}
