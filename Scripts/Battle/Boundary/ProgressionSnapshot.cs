using System;
using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class ProgressionSnapshot
{
	public int PlayerLevel { get; set; } = 1;

	public int PlayerExperience { get; set; }

	public int PlayerMasteryPoints { get; set; }

	public int ArakawaGrowthLevel { get; set; } = 1;

	public string[] TalentIds { get; set; } = Array.Empty<string>();

	public string[] ArakawaUnlockIds { get; set; } = Array.Empty<string>();

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
