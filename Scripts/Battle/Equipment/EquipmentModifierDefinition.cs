using System;

namespace CardChessDemo.Battle.Equipment;

public sealed class EquipmentModifierDefinition
{
	public EquipmentModifierDefinition(string modifierTypeId, int intValue, string summaryText)
	{
		ModifierTypeId = modifierTypeId?.Trim() ?? string.Empty;
		IntValue = intValue;
		SummaryText = summaryText?.Trim() ?? string.Empty;
	}

	public string ModifierTypeId { get; }

	public int IntValue { get; }

	public string SummaryText { get; }
}
