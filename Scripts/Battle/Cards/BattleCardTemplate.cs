using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Boundary;

namespace CardChessDemo.Battle.Cards;

[GlobalClass]
public partial class BattleCardTemplate : Resource
{
	private static readonly HashSet<string> HighImpactCardIds = new(StringComparer.Ordinal)
	{
		"draw_revolver",
		"card_learning",
		"card_pressure_breach",
		"card_magnetic_hunt",
		"card_overclock_beam",
	};

	[Export] public string CardId { get; set; } = "card_id";
	[Export] public string DisplayName { get; set; } = "新卡牌";
	[Export(PropertyHint.MultilineText)] public string Description { get; set; } = string.Empty;
	[Export] public BattleCardCategory Category { get; set; } = BattleCardCategory.Skill;
	[Export] public BattleCardTargetingMode TargetingMode { get; set; } = BattleCardTargetingMode.None;
	[Export] public int Cost { get; set; }
	[Export] public int Range { get; set; }
	[Export] public int Damage { get; set; }
	[Export] public int HealingAmount { get; set; }
	[Export] public int DrawCount { get; set; }
	[Export] public int EnergyGain { get; set; }
	[Export] public int ShieldGain { get; set; }
	[Export] public bool IsQuick { get; set; }
	[Export] public bool ExhaustsOnPlay { get; set; }
	[Export] public int BuildPoints { get; set; } = 1;
	[Export] public int MaxCopiesInDeck { get; set; } = 3;
	[Export] public int DefaultStarterCopies { get; set; } = 0;
	[Export] public bool UnlockedByDefault { get; set; } = true;
	[Export] public int RequiredPlayerLevel { get; set; } = 1;
	[Export] public string[] RequiredTalentIds { get; set; } = Array.Empty<string>();
	[Export] public string[] RequiredBranchTags { get; set; } = Array.Empty<string>();
	[Export] public int RequiredMeleeMastery { get; set; }
	[Export] public int RequiredRangedMastery { get; set; }
	[Export] public int RequiredFlexMastery { get; set; }
	[Export] public string[] CycleTags { get; set; } = Array.Empty<string>();
	[Export] public bool IsLearnedCard { get; set; }
	[Export] public bool DisallowOverlimitCarry { get; set; }
	[Export] public int OverlimitCostPenalty { get; set; } = 1;
	[Export(PropertyHint.Range, "0,1,0.05")] public float OverlimitEffectMultiplier { get; set; } = 0.8f;
	[Export] public int OverlimitExtraBuildPoints { get; set; } = 1;

	public BattleCardDefinition BuildRuntimeDefinition(bool applyOverlimitPenalty = false, ProgressionSnapshot? snapshot = null)
	{
		int cost = Cost;
		int damage = Damage;
		int healingAmount = HealingAmount;
		int drawCount = DrawCount;
		int energyGain = EnergyGain;
		int shieldGain = ShieldGain;
		string displayName = DisplayName;
		string description = Description;

		if (applyOverlimitPenalty)
		{
			int adjustedCostPenalty = GetAdjustedOverlimitCostPenalty(snapshot);
			float multiplier = GetAdjustedOverlimitEffectMultiplier(snapshot);
			cost += adjustedCostPenalty;
			damage = ScalePositiveValue(damage, multiplier);
			healingAmount = ScalePositiveValue(healingAmount, multiplier);
			drawCount = ScalePositiveValue(drawCount, multiplier);
			energyGain = ScalePositiveValue(energyGain, multiplier);
			shieldGain = ScalePositiveValue(shieldGain, multiplier);
			displayName = string.IsNullOrWhiteSpace(displayName) ? "\u8D85\u89C4\u5361\u724C" : $"{displayName} [\u8D85\u89C4]";
			int preservedPercent = Mathf.RoundToInt(multiplier * 100.0f);
			string penaltyText = adjustedCostPenalty > 0
				? $"\u8D39\u7528+{adjustedCostPenalty}\uFF0C\u6548\u679C\u4FDD\u7559{preservedPercent}%"
				: $"\u6548\u679C\u4FDD\u7559{preservedPercent}%";
			description = string.IsNullOrWhiteSpace(description)
				? $"\u4EE5\u8D85\u89C4\u65B9\u5F0F\u643A\u5E26\u3002{penaltyText}"
				: $"{description} / \u8D85\u89C4\u643A\u5E26\uFF1A{penaltyText}";
		}

		return new BattleCardDefinition(
			cardId: CardId,
			displayName: displayName,
			description: description,
			cost: cost,
			category: Category,
			targetingMode: TargetingMode,
			range: Range,
			damage: damage,
			healingAmount: healingAmount,
			drawCount: drawCount,
			energyGain: energyGain,
			shieldGain: shieldGain,
			isQuick: IsQuick,
			exhaustsOnPlay: ExhaustsOnPlay);
	}

	public bool IsOwned(ProgressionSnapshot snapshot)
	{
		return UnlockedByDefault || snapshot.UnlockedCardIds.Contains(CardId, StringComparer.Ordinal);
	}

	public bool MeetsCarryRequirements(ProgressionSnapshot snapshot)
	{
		if (snapshot.PlayerLevel < Math.Max(1, RequiredPlayerLevel))
		{
			return false;
		}

		if (RequiredTalentIds.Any(requiredTalentId => !snapshot.TalentIds.Contains(requiredTalentId, StringComparer.Ordinal)))
		{
			return false;
		}

		if (RequiredBranchTags.Any(requiredBranchTag => !snapshot.TalentBranchTags.Contains(requiredBranchTag, StringComparer.Ordinal)))
		{
			return false;
		}

		if (snapshot.MeleeMastery < Math.Max(0, RequiredMeleeMastery))
		{
			return false;
		}

		if (snapshot.RangedMastery < Math.Max(0, RequiredRangedMastery))
		{
			return false;
		}

		if (snapshot.FlexMastery < Math.Max(0, RequiredFlexMastery))
		{
			return false;
		}

		return true;
	}

	public bool CanCarryNormally(ProgressionSnapshot snapshot)
	{
		if (!MeetsCarryRequirements(snapshot))
		{
			return false;
		}

		return IsOwned(snapshot);
	}

	public bool CanCarryOverlimit(ProgressionSnapshot snapshot)
	{
		if (DisallowOverlimitCarry)
		{
			return false;
		}

		return IsOwned(snapshot) && !CanCarryNormally(snapshot);
	}

	public bool IsUnlocked(ProgressionSnapshot snapshot)
	{
		return CanCarryNormally(snapshot);
	}

	public string[] GetNormalizedCycleTags()
	{
		return CycleTags
			.Where(tag => !string.IsNullOrWhiteSpace(tag))
			.Select(tag => tag.Trim().ToLowerInvariant())
			.Distinct(StringComparer.Ordinal)
			.ToArray();
	}

	public int GetEffectiveBuildPoints()
	{
		int lowered = Math.Max(1, BuildPoints - 1);
		return HighImpactCardIds.Contains(CardId)
			? Math.Max(4, lowered)
			: lowered;
	}

	public int GetAppliedBuildPoints(bool usesOverlimitCarry)
	{
		return GetEffectiveBuildPoints() + (usesOverlimitCarry ? Math.Max(0, OverlimitExtraBuildPoints) : 0);
	}

	public int GetAdjustedOverlimitCostPenalty(ProgressionSnapshot? snapshot)
	{
		int reduction = snapshot?.OverlimitCostPenaltyReduction ?? 0;
		return Math.Max(0, OverlimitCostPenalty - reduction);
	}

	public float GetAdjustedOverlimitEffectMultiplier(ProgressionSnapshot? snapshot)
	{
		float bonus = (snapshot?.OverlimitEffectBonusPercent ?? 0) / 100.0f;
		return Mathf.Clamp(OverlimitEffectMultiplier + bonus, 0.0f, 1.0f);
	}

	private static int ScalePositiveValue(int value, float multiplier)
	{
		if (value <= 0)
		{
			return 0;
		}

		return Math.Max(1, Mathf.FloorToInt(value * multiplier));
	}
}


