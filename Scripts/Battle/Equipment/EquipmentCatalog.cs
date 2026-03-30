using System;
using System.Collections.Generic;
using System.Linq;

namespace CardChessDemo.Battle.Equipment;

public sealed class EquipmentCatalog
{
	private readonly Dictionary<string, EquipmentDefinition> _definitionsById;

	public EquipmentCatalog(IEnumerable<EquipmentDefinition> definitions)
	{
		_definitionsById = definitions?
			.Where(definition => definition != null && !string.IsNullOrWhiteSpace(definition.ItemId))
			.GroupBy(definition => definition.ItemId, StringComparer.Ordinal)
			.ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal)
			?? new Dictionary<string, EquipmentDefinition>(StringComparer.Ordinal);
	}

	public IEnumerable<EquipmentDefinition> AllDefinitions => _definitionsById.Values;

	public EquipmentDefinition? FindDefinition(string itemId)
	{
		if (string.IsNullOrWhiteSpace(itemId))
		{
			return null;
		}

		return _definitionsById.GetValueOrDefault(itemId.Trim());
	}

	public EquipmentDefinition[] GetDefinitionsForSlot(string slotId)
	{
		string normalizedSlotId = slotId?.Trim().ToLowerInvariant() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(normalizedSlotId))
		{
			return Array.Empty<EquipmentDefinition>();
		}

		return _definitionsById.Values
			.Where(definition => string.Equals(definition.SlotId, normalizedSlotId, StringComparison.Ordinal))
			.OrderBy(definition => definition.DisplayName, StringComparer.Ordinal)
			.ToArray();
	}

	public bool CanEquipInSlot(string itemId, string slotId)
	{
		EquipmentDefinition? definition = FindDefinition(itemId);
		if (definition == null)
		{
			return false;
		}

		return string.Equals(definition.SlotId, slotId?.Trim().ToLowerInvariant(), StringComparison.Ordinal);
	}

	public static EquipmentCatalog CreateFromConfiguredResources()
	{
		// TODO: 后续正式版应从 Resource/CSV/JSON 装备表加载，避免把装备定义继续硬编码在运行时逻辑里。
		// 当前竞赛版先回退到 demo catalog，保证 state/service/resolver 分层先跑通。
		return CreateDemoFallback();
	}

	public static EquipmentCatalog CreateDemoFallback()
	{
		return new EquipmentCatalog(new[]
		{
			new EquipmentDefinition(
				"rusted_blade",
				"旧钢刀",
				EquipmentSlotIds.Weapon,
				"拆船废料打磨成的近战武器，稳定提升攻击。",
				new EquipmentModifierDefinition("player.attack_bonus", 1, "攻击 +1")),
			new EquipmentDefinition(
				"ion_pistol",
				"脉冲短铳",
				EquipmentSlotIds.Weapon,
				"便携式脉冲副武器，输出更高但更依赖供能。",
				new EquipmentModifierDefinition("player.attack_bonus", 2, "攻击 +2")),
			new EquipmentDefinition(
				"patched_coat",
				"补丁风衣",
				EquipmentSlotIds.Armor,
				"缝补过的旧大衣，提供更扎实的生存空间。",
				new EquipmentModifierDefinition("player.max_hp_bonus", 4, "生命 +4"),
				new EquipmentModifierDefinition("player.defense_reduction_bonus", 5, "减伤 +5%")),
			new EquipmentDefinition(
				"reactive_plate",
				"反应护甲片",
				EquipmentSlotIds.Armor,
				"临时拼装的护甲片，强化防御姿态。",
				new EquipmentModifierDefinition("player.defense_reduction_bonus", 10, "减伤 +10%"),
				new EquipmentModifierDefinition("player.defense_shield_bonus", 1, "防御附盾 +1")),
			new EquipmentDefinition(
				"signal_charm",
				"信号挂饰",
				EquipmentSlotIds.Accessory,
				"轻量化挂饰，改善战场移动调度。",
				new EquipmentModifierDefinition("player.move_bonus", 1, "移动 +1")),
			new EquipmentDefinition(
				"tactical_chip",
				"战术芯片",
				EquipmentSlotIds.Accessory,
				"旧时代战术分析模组，补强防御判断。",
				new EquipmentModifierDefinition("player.defense_reduction_bonus", 5, "减伤 +5%")),
		});
	}
}
