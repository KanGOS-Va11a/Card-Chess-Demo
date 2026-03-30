using System;
using System.Linq;

namespace CardChessDemo.Battle.Equipment;

public sealed class EquipmentDefinition
{
	public EquipmentDefinition(
		string itemId,
		string displayName,
		string slotId,
		string description,
		params EquipmentModifierDefinition[] modifiers)
	{
		ItemId = itemId?.Trim() ?? string.Empty;
		DisplayName = displayName?.Trim() ?? string.Empty;
		SlotId = slotId?.Trim().ToLowerInvariant() ?? string.Empty;
		Description = description?.Trim() ?? string.Empty;
		Modifiers = modifiers ?? Array.Empty<EquipmentModifierDefinition>();
	}

	public string ItemId { get; }

	public string DisplayName { get; }

	public string SlotId { get; }

	public string Description { get; }

	public EquipmentModifierDefinition[] Modifiers { get; }

	public string BuildModifierSummary()
	{
		return Modifiers.Length == 0
			? "无"
			: string.Join(" / ", Modifiers.Select(modifier => modifier.SummaryText).Where(text => !string.IsNullOrWhiteSpace(text)));
	}
}
