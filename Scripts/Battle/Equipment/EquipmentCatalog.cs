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
		// TODO: 鍚庣画姝ｅ紡鐗堟湰搴斾粠 Resource/CSV/JSON 瑁呭琛ㄥ姞杞斤紝閬垮厤缁х画鎶婅澶囧畾涔夌‖缂栫爜鍦ㄨ繍琛屾椂閫昏緫閲屻€?
		// 褰撳墠绔炶禌鐗堝厛淇濈暀 demo fallback锛岀‘淇?state / service / resolver 鐨勮竟鐣屽厛璺戦€氥€?
		return CreateDemoFallback();
	}

	public static EquipmentCatalog CreateDemoFallback()
	{
		return new EquipmentCatalog(new[]
		{
			new EquipmentDefinition(
				"equip_magnetic_scabbard",
				"\u78C1\u9501\u5200\u9798",
				EquipmentSlotIds.Weapon,
				"\u8FD1\u6218\u8FC7\u6E21\u6B66\u5668\u3002\u5F53\u524D\u7248\u672C\u5148\u843D\u5B9E\u666E\u901A\u653B\u51FB\u52A0\u6210\u3002",
				new EquipmentModifierDefinition("player.attack_bonus", 1, "\u653B\u51FB +1")),
			new EquipmentDefinition(
				"equip_arc_pipe",
				"\u7535\u5F27\u91D1\u5C5E\u7BA1",
				EquipmentSlotIds.Weapon,
				"\u8352\u5DDD\u9996\u6B21\u5F3A\u5316\u7684\u6B66\u5668\u3002\u5F53\u524D\u7248\u672C\u5148\u843D\u5B9E\u653B\u51FB\u548C\u5C04\u7A0B\u52A0\u6210\u3002",
				new EquipmentModifierDefinition("player.attack_bonus", 1, "\u653B\u51FB +1"),
				new EquipmentModifierDefinition("player.attack_range_bonus", 1, "\u5C04\u7A0B +1")),
			new EquipmentDefinition(
				"equip_target_lens",
				"\u6821\u51C6\u7784\u5177",
				EquipmentSlotIds.Accessory,
				"\u8FDC\u7A0B\u8FC7\u6E21\u88C5\u5907\u3002\u4EE5\u751F\u547D\u4EE3\u4EF7\u6362\u53D6\u66F4\u7A33\u5B9A\u7684\u8FDC\u7A0B\u8F93\u51FA\u3002",
				new EquipmentModifierDefinition("player.attack_range_bonus", 1, "\u5C04\u7A0B +1"),
				new EquipmentModifierDefinition("player.max_hp_bonus", -2, "\u751F\u547D -2")),
			new EquipmentDefinition(
				"drawn_revolver",
				"\u5DF2\u62D4\u5DE6\u8F6E",
				EquipmentSlotIds.Weapon,
				"\u62D4\u67AA\u540E\u4E34\u65F6\u4F7F\u7528\u7684\u5DE6\u8F6E\u6863\u6848\u3002",
				new EquipmentModifierDefinition("player.attack_bonus", 2, "\u653B\u51FB +2"),
				new EquipmentModifierDefinition("player.attack_range_bonus", 1, "\u5C04\u7A0B +1")),
			new EquipmentDefinition(
				"equip_old_coat",
				"\u65E7\u5927\u8863",
				EquipmentSlotIds.Armor,
				"\u4E3B\u89D2\u521D\u59CB\u62A4\u5177\u3002\u63D0\u4F9B\u57FA\u7840\u751F\u5B58\u7A7A\u95F4\u3002",
				new EquipmentModifierDefinition("player.max_hp_bonus", 3, "\u751F\u547D +3"),
				new EquipmentModifierDefinition("player.defense_reduction_bonus", 5, "\u51CF\u4F24 +5%")),
			new EquipmentDefinition(
				"equip_phase_boots",
				"\u76F8\u4F4D\u77ED\u9774",
				EquipmentSlotIds.Armor,
				"\u504F\u673A\u52A8\u53D6\u5411\u7684\u62A4\u5177\u3002",
				new EquipmentModifierDefinition("player.move_bonus", 1, "\u79FB\u52A8 +1")),
			new EquipmentDefinition(
				"equip_red_scarf",
				"\u7EA2\u8272\u65B9\u5DFE",
				EquipmentSlotIds.Accessory,
				"\u4E3B\u89D2\u8EAB\u4EFD\u7B26\u53F7\u3002\u5F53\u524D\u4F5C\u4E3A\u5360\u4F4D\u9970\u54C1\u3002"),
			new EquipmentDefinition(
				"equip_archive_probe",
				"\u6863\u6848\u63A2\u9488",
				EquipmentSlotIds.Accessory,
				"\u5B66\u4E60\u6D41\u6838\u5FC3\u8F85\u52A9\u88C5\u5907\u3002\u5F53\u524D\u4F5C\u4E3A\u6B63\u5F0F\u5B9A\u4E49\u63A5\u5165\u3002"),
			new EquipmentDefinition(
				"equip_parallel_battery",
				"\u5E76\u8054\u7535\u6C60\u7EC4",
				EquipmentSlotIds.Armor,
				"\u9AD8\u8D1F\u8F7D\u4F9B\u80FD\u7EC4\u4EF6\u3002\u5F53\u524D\u4F5C\u4E3A\u5360\u4F4D\u88C5\u5907\u3002"),
			new EquipmentDefinition(
				"equip_forbidden_patch",
				"\u7981\u533A\u8865\u4E01",
				EquipmentSlotIds.Accessory,
				"\u9AD8\u98CE\u9669\u5B9E\u9A8C\u578B\u88C5\u5907\u3002\u5F53\u524D\u4F5C\u4E3A\u5360\u4F4D\u9970\u54C1\u3002"),
			new EquipmentDefinition(
				"equip_insulated_cloak",
				"\u7EDD\u7F18\u62AB\u80A9",
				EquipmentSlotIds.Armor,
				"\u540E\u671F\u6297\u5F02\u5E38\u62A4\u5177\u3002\u5F53\u524D\u4F5C\u4E3A\u5360\u4F4D\u88C5\u5907\u3002"),
		});
	}
}
