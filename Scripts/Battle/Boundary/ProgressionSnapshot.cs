using System;
using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class ProgressionSnapshot
{
	private const string MeleeMasteryPrefix = "mastery.melee.";
	private const string RangedMasteryPrefix = "mastery.ranged.";
	private const string FlexMasteryPrefix = "mastery.flex.";
	private const string OverlimitSlotBonusPrefix = "stat.overlimit_slots_bonus.";
	private const string OverlimitCostReductionPrefix = "stat.overlimit_cost_reduction.";
	private const string OverlimitEffectBonusPrefix = "stat.overlimit_effect_bonus_percent.";

	public int PlayerLevel { get; set; } = 1;

	public int PlayerExperience { get; set; }

	public int PlayerMasteryPoints { get; set; }

	public int ArakawaGrowthLevel { get; set; } = 1;

	public string[] TalentIds { get; set; } = Array.Empty<string>();

	public string[] ArakawaUnlockIds { get; set; } = Array.Empty<string>();

	public string[] UnlockedCardIds { get; set; } = Array.Empty<string>();

	public string[] TalentBranchTags { get; set; } = Array.Empty<string>();

	public int DeckPointBudgetBonus { get; set; }

	public int DeckMinCardCountDelta { get; set; }

	public int DeckMaxCardCountDelta { get; set; }

	public int DeckMaxCopiesPerCardBonus { get; set; }

	public int MeleeMastery => GetTalentScalarBonus(MeleeMasteryPrefix);

	public int RangedMastery => GetTalentScalarBonus(RangedMasteryPrefix);

	public int FlexMastery => GetTalentScalarBonus(FlexMasteryPrefix);

	public int OverlimitCarrySlotBonus => GetTalentScalarBonus(OverlimitSlotBonusPrefix);

	public int OverlimitCostPenaltyReduction => GetTalentScalarBonus(OverlimitCostReductionPrefix);

	public int OverlimitEffectBonusPercent => GetTalentScalarBonus(OverlimitEffectBonusPrefix);

	public bool TryValidate(out string failureReason)
	{
		if (PlayerLevel <= 0)
		{
			failureReason = "ProgressionSnapshot.PlayerLevel must be greater than 0.";
			return false;
		}

		if (ArakawaGrowthLevel <= 0)
		{
			failureReason = "ProgressionSnapshot.ArakawaGrowthLevel must be greater than 0.";
			return false;
		}

		failureReason = string.Empty;
		return true;
	}

	public bool HasTalent(string talentId)
	{
		if (string.IsNullOrWhiteSpace(talentId))
		{
			return false;
		}

		return TalentIds.Contains(talentId, StringComparer.Ordinal);
	}

	public int GetBranchMastery(string branchId)
	{
		return branchId?.Trim().ToLowerInvariant() switch
		{
			"melee" => MeleeMastery,
			"ranged" => RangedMastery,
			"flex" => FlexMastery,
			_ => 0,
		};
	}

	public int GetTalentScalarBonus(string prefix)
	{
		if (string.IsNullOrWhiteSpace(prefix))
		{
			return 0;
		}

		int total = 0;
		foreach (string talentId in TalentIds)
		{
			if (string.IsNullOrWhiteSpace(talentId) || !talentId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			string valueText = talentId[prefix.Length..];
			if (int.TryParse(valueText, out int parsedValue))
			{
				total += parsedValue;
			}
		}

		return total;
	}

	public Godot.Collections.Dictionary ToDictionary()
	{
		return new Godot.Collections.Dictionary
		{
			["player_level"] = PlayerLevel,
			["player_experience"] = PlayerExperience,
			["player_mastery_points"] = PlayerMasteryPoints,
			["arakawa_growth_level"] = ArakawaGrowthLevel,
			["talent_ids"] = ToVariantArray(TalentIds),
			["arakawa_unlock_ids"] = ToVariantArray(ArakawaUnlockIds),
			["unlocked_card_ids"] = ToVariantArray(UnlockedCardIds),
			["talent_branch_tags"] = ToVariantArray(TalentBranchTags),
			["deck_point_budget_bonus"] = DeckPointBudgetBonus,
			["deck_min_card_count_delta"] = DeckMinCardCountDelta,
			["deck_max_card_count_delta"] = DeckMaxCardCountDelta,
			["deck_max_copies_per_card_bonus"] = DeckMaxCopiesPerCardBonus,
		};
	}

	public static ProgressionSnapshot FromDictionary(Godot.Collections.Dictionary? dictionary)
	{
		if (dictionary == null)
		{
			return new ProgressionSnapshot();
		}

		return new ProgressionSnapshot
		{
			PlayerLevel = dictionary.TryGetValue("player_level", out Variant playerLevel) ? Math.Max(1, playerLevel.AsInt32()) : 1,
			PlayerExperience = dictionary.TryGetValue("player_experience", out Variant playerExperience) ? Math.Max(0, playerExperience.AsInt32()) : 0,
			PlayerMasteryPoints = dictionary.TryGetValue("player_mastery_points", out Variant playerMasteryPoints) ? Math.Max(0, playerMasteryPoints.AsInt32()) : 0,
			ArakawaGrowthLevel = dictionary.TryGetValue("arakawa_growth_level", out Variant arakawaGrowthLevel) ? Math.Max(1, arakawaGrowthLevel.AsInt32()) : 1,
			TalentIds = dictionary.TryGetValue("talent_ids", out Variant talentIds) ? ToStringArray(talentIds) : Array.Empty<string>(),
			ArakawaUnlockIds = dictionary.TryGetValue("arakawa_unlock_ids", out Variant arakawaUnlockIds) ? ToStringArray(arakawaUnlockIds) : Array.Empty<string>(),
			UnlockedCardIds = dictionary.TryGetValue("unlocked_card_ids", out Variant unlockedCardIds) ? ToStringArray(unlockedCardIds) : Array.Empty<string>(),
			TalentBranchTags = dictionary.TryGetValue("talent_branch_tags", out Variant talentBranchTags) ? ToStringArray(talentBranchTags) : Array.Empty<string>(),
			DeckPointBudgetBonus = dictionary.TryGetValue("deck_point_budget_bonus", out Variant deckPointBudgetBonus) ? deckPointBudgetBonus.AsInt32() : 0,
			DeckMinCardCountDelta = dictionary.TryGetValue("deck_min_card_count_delta", out Variant minDelta) ? minDelta.AsInt32() : 0,
			DeckMaxCardCountDelta = dictionary.TryGetValue("deck_max_card_count_delta", out Variant maxDelta) ? maxDelta.AsInt32() : 0,
			DeckMaxCopiesPerCardBonus = dictionary.TryGetValue("deck_max_copies_per_card_bonus", out Variant maxCopiesBonus) ? maxCopiesBonus.AsInt32() : 0,
		};
	}

	private static Godot.Collections.Array<string> ToVariantArray(string[] values)
	{
		Godot.Collections.Array<string> array = new();
		foreach (string value in values.Where(value => !string.IsNullOrWhiteSpace(value)))
		{
			array.Add(value);
		}

		return array;
	}

	private static string[] ToStringArray(Variant value)
	{
		if (value.Obj is not Godot.Collections.Array rawArray)
		{
			return Array.Empty<string>();
		}

		return rawArray
			.Select(item => item.AsString())
			.Where(item => !string.IsNullOrWhiteSpace(item))
			.ToArray();
	}
}
