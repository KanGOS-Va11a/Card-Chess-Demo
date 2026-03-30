using System;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Equipment;
using CardChessDemo.Battle.Shared;
using SharedPlayerRuntimeState = CardChessDemo.Battle.Shared.PlayerRuntimeState;
using SharedProgressionRuntimeState = CardChessDemo.Battle.Shared.ProgressionRuntimeState;
using SharedEquipmentLoadoutState = CardChessDemo.Battle.Shared.EquipmentLoadoutState;

namespace CardChessDemo.Battle.Stats;

public sealed class PlayerStatResolver
{
	private readonly EquipmentCatalog _equipmentCatalog;

	public PlayerStatResolver(EquipmentCatalog equipmentCatalog)
	{
		_equipmentCatalog = equipmentCatalog;
	}

	public ResolvedPlayerStats Resolve(SharedPlayerRuntimeState playerState, SharedProgressionRuntimeState progressionState, SharedEquipmentLoadoutState loadoutState, int defenseDamageReductionPercent, int defenseShieldGain)
	{
		if (playerState == null)
		{
			return new ResolvedPlayerStats();
		}

		int attackBonus = SumTalentScalarBonuses(progressionState, "stat.attack_bonus.") + SumEquipmentIntModifiers(loadoutState, "player.attack_bonus");
		int defenseReductionBonus = SumTalentScalarBonuses(progressionState, "stat.defense_reduction_bonus.") + SumEquipmentIntModifiers(loadoutState, "player.defense_reduction_bonus");
		int defenseShieldBonus = SumTalentScalarBonuses(progressionState, "stat.defense_shield_bonus.") + SumEquipmentIntModifiers(loadoutState, "player.defense_shield_bonus");
		int maxHpBonus = SumEquipmentIntModifiers(loadoutState, "player.max_hp_bonus");
		int moveBonus = SumEquipmentIntModifiers(loadoutState, "player.move_bonus");

		return new ResolvedPlayerStats
		{
			MaxHp = Math.Max(1, playerState.MaxHp + maxHpBonus),
			CurrentHp = Math.Max(0, playerState.CurrentHp),
			MovePointsPerTurn = Math.Max(0, playerState.MovePointsPerTurn + moveBonus),
			AttackRange = Math.Max(0, playerState.AttackRange),
			AttackDamage = Math.Max(0, playerState.AttackDamage + attackBonus),
			DefenseDamageReductionPercent = Mathf.Clamp(defenseDamageReductionPercent + defenseReductionBonus, 0, 100),
			DefenseShieldGain = Math.Max(0, defenseShieldGain + defenseShieldBonus),
		};
	}

	private static int SumTalentScalarBonuses(SharedProgressionRuntimeState? progressionState, string prefix)
	{
		if (progressionState == null || string.IsNullOrWhiteSpace(prefix))
		{
			return 0;
		}

		int total = 0;
		foreach (string talentId in progressionState.TalentIds)
		{
			if (string.IsNullOrWhiteSpace(talentId) || !talentId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			string valueText = talentId[prefix.Length..];
			if (int.TryParse(valueText, out int value))
			{
				total += value;
			}
		}

		return total;
	}

	private int SumEquipmentIntModifiers(SharedEquipmentLoadoutState? loadoutState, string modifierTypeId)
	{
		if (loadoutState == null || string.IsNullOrWhiteSpace(modifierTypeId))
		{
			return 0;
		}

		return GetEquippedDefinitions(loadoutState)
			.SelectMany(definition => definition.Modifiers)
			.Where(modifier => string.Equals(modifier.ModifierTypeId, modifierTypeId, StringComparison.Ordinal))
			.Sum(modifier => modifier.IntValue);
	}

	private EquipmentDefinition[] GetEquippedDefinitions(SharedEquipmentLoadoutState loadoutState)
	{
		return new[]
		{
			_equipmentCatalog.FindDefinition(loadoutState.WeaponItemId),
			_equipmentCatalog.FindDefinition(loadoutState.ArmorItemId),
			_equipmentCatalog.FindDefinition(loadoutState.AccessoryItemId),
		}
		.Where(definition => definition != null)
		.Cast<EquipmentDefinition>()
		.ToArray();
	}
}
