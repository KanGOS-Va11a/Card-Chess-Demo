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
		// TODO: 后续正式版本应从 Resource/CSV/JSON 装备表加载，避免继续把装备定义硬编码在运行时逻辑里。
		// 当前竞赛版先保留 demo fallback，确保 state / service / resolver 的边界先跑通。
		return CreateDemoFallback();
	}

	public static EquipmentCatalog CreateDemoFallback()
	{
		return new EquipmentCatalog(new[]
		{
			new EquipmentDefinition(
				"equip_magnetic_scabbard",
				"磁锁刀鞘",
				EquipmentSlotIds.Weapon,
				"近战过渡武器。当前版本先落实普通攻击加成，并保留对护盾目标增伤的设计语义。",
				new EquipmentModifierDefinition("player.attack_bonus", 1, "攻击 +1")),
			new EquipmentDefinition(
				"equip_arc_pipe",
				"电弧金属管",
				EquipmentSlotIds.Weapon,
				"荒川首次强化的武器。当前版本先落实基础攻击加成，后续再补电弧地形联动的额外伤害。",
				new EquipmentModifierDefinition("player.attack_bonus", 1, "攻击 +1"),
				new EquipmentModifierDefinition("player.attack_range_bonus", 1, "射程 +1")),
			new EquipmentDefinition(
				"equip_target_lens",
				"校准瞄具",
				EquipmentSlotIds.Accessory,
				"远程过渡装备。强化直线攻击射程判定，并以生命代价换取更稳定的远程输出。",
				new EquipmentModifierDefinition("player.attack_range_bonus", 1, "射程 +1"),
				new EquipmentModifierDefinition("player.max_hp_bonus", -2, "生命 -2")),
			new EquipmentDefinition(
				"drawn_revolver",
				"Drawn Revolver",
				EquipmentSlotIds.Weapon,
				"Battle-only temporary sidearm profile used by draw_revolver.",
				new EquipmentModifierDefinition("player.attack_bonus", 2, "Attack +2"),
				new EquipmentModifierDefinition("player.attack_range_bonus", 1, "Range +1")),
			new EquipmentDefinition(
				"equip_old_coat",
				"旧大衣",
				EquipmentSlotIds.Armor,
				"主角初始护甲。提供稳定的基础生存空间。",
				new EquipmentModifierDefinition("player.max_hp_bonus", 3, "生命 +3"),
				new EquipmentModifierDefinition("player.defense_reduction_bonus", 5, "减伤 +5%")),
			new EquipmentDefinition(
				"equip_phase_boots",
				"护相短靴",
				EquipmentSlotIds.Armor,
				"创造 / 机动向过渡装备。当前版本先落实移动力提升。",
				new EquipmentModifierDefinition("player.move_bonus", 1, "移动 +1")),
			new EquipmentDefinition(
				"equip_red_scarf",
				"红色方巾",
				EquipmentSlotIds.Accessory,
				"主角身份符号。当前版本先作为正式饰品定义接入，首次近战抽牌效果待补事件触发器。"),
			new EquipmentDefinition(
				"equip_archive_probe",
				"档案探针",
				EquipmentSlotIds.Accessory,
				"学习流核心辅助装备。当前版本先接入正式定义，后续补学习窗口延长与专用槽位效果。"),
			new EquipmentDefinition(
				"equip_parallel_battery",
				"并联电池组",
				EquipmentSlotIds.Armor,
				"高费与学习牌承载器。当前版本先接入正式定义，后续补初始能量与构筑承载联动。"),
			new EquipmentDefinition(
				"equip_forbidden_patch",
				"禁区补丁",
				EquipmentSlotIds.Accessory,
				"高风险实验型装备。当前版本先接入正式定义，后续补超规槽位与额外代价。"),
			new EquipmentDefinition(
				"equip_insulated_cloak",
				"绝缘披肩",
				EquipmentSlotIds.Armor,
				"创造流后期稳定器。当前版本先接入正式定义，后续补首次电弧伤害免疫与 flex 修正。"),
		});
	}
}
