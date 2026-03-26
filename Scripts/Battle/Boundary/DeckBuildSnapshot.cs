using System;
using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class DeckBuildSnapshot
{
	public string[] CardIds { get; set; } = Array.Empty<string>();

	public string[] RelicIds { get; set; } = Array.Empty<string>();

	public string BuildName { get; set; } = "default";

	public bool TryValidate(out string failureReason)
	{
		if (CardIds.Any(string.IsNullOrWhiteSpace))
		{
			failureReason = "DeckBuildSnapshot contains an empty card id.";
			return false;
		}

		if (RelicIds.Any(string.IsNullOrWhiteSpace))
		{
			failureReason = "DeckBuildSnapshot contains an empty relic id.";
			return false;
		}

		failureReason = string.Empty;
		return true;
	}

	public Godot.Collections.Dictionary ToDictionary()
	{
		return new Godot.Collections.Dictionary
		{
			["build_name"] = BuildName,
			["card_ids"] = ToVariantArray(CardIds),
			["relic_ids"] = ToVariantArray(RelicIds),
		};
	}

	public static DeckBuildSnapshot FromDictionary(Godot.Collections.Dictionary? dictionary)
	{
		if (dictionary == null)
		{
			return new DeckBuildSnapshot();
		}

		return new DeckBuildSnapshot
		{
			BuildName = dictionary.TryGetValue("build_name", out Variant buildName) ? buildName.AsString() : "default",
			CardIds = dictionary.TryGetValue("card_ids", out Variant cardIds) ? ToStringArray(cardIds) : Array.Empty<string>(),
			RelicIds = dictionary.TryGetValue("relic_ids", out Variant relicIds) ? ToStringArray(relicIds) : Array.Empty<string>(),
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
