using System;
using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class ProgressionDelta
{
	public int ExperienceDelta { get; set; }

	public int MasteryPointDelta { get; set; }

	public int PlayerLevelDelta { get; set; }

	public int ArakawaGrowthLevelDelta { get; set; }

	public string[] TalentUnlockIds { get; set; } = Array.Empty<string>();

	public string[] ArakawaUnlockIds { get; set; } = Array.Empty<string>();

	public string[] UnlockedCardIds { get; set; } = Array.Empty<string>();

	public bool TryValidate(out string failureReason)
	{
		if (PlayerLevelDelta < 0)
		{
			failureReason = "ProgressionDelta.PlayerLevelDelta cannot be negative.";
			return false;
		}

		if (ArakawaGrowthLevelDelta < 0)
		{
			failureReason = "ProgressionDelta.ArakawaGrowthLevelDelta cannot be negative.";
			return false;
		}

		failureReason = string.Empty;
		return true;
	}

	public Godot.Collections.Dictionary ToDictionary()
	{
		return new Godot.Collections.Dictionary
		{
			["experience_delta"] = ExperienceDelta,
			["mastery_point_delta"] = MasteryPointDelta,
			["player_level_delta"] = PlayerLevelDelta,
			["arakawa_growth_level_delta"] = ArakawaGrowthLevelDelta,
			["talent_unlock_ids"] = ToVariantArray(TalentUnlockIds),
			["arakawa_unlock_ids"] = ToVariantArray(ArakawaUnlockIds),
			["unlocked_card_ids"] = ToVariantArray(UnlockedCardIds),
		};
	}

	public static ProgressionDelta FromDictionary(Godot.Collections.Dictionary? dictionary)
	{
		if (dictionary == null)
		{
			return new ProgressionDelta();
		}

		return new ProgressionDelta
		{
			ExperienceDelta = dictionary.TryGetValue("experience_delta", out Variant experienceDelta) ? experienceDelta.AsInt32() : 0,
			MasteryPointDelta = dictionary.TryGetValue("mastery_point_delta", out Variant masteryPointDelta) ? masteryPointDelta.AsInt32() : 0,
			PlayerLevelDelta = dictionary.TryGetValue("player_level_delta", out Variant playerLevelDelta) ? playerLevelDelta.AsInt32() : 0,
			ArakawaGrowthLevelDelta = dictionary.TryGetValue("arakawa_growth_level_delta", out Variant growthDelta) ? growthDelta.AsInt32() : 0,
			TalentUnlockIds = dictionary.TryGetValue("talent_unlock_ids", out Variant talentUnlockIds) ? ToStringArray(talentUnlockIds) : Array.Empty<string>(),
			ArakawaUnlockIds = dictionary.TryGetValue("arakawa_unlock_ids", out Variant arakawaUnlockIds) ? ToStringArray(arakawaUnlockIds) : Array.Empty<string>(),
			UnlockedCardIds = dictionary.TryGetValue("unlocked_card_ids", out Variant unlockedCardIds) ? ToStringArray(unlockedCardIds) : Array.Empty<string>(),
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
