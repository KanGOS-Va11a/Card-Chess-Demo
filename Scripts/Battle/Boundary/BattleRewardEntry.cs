using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class BattleRewardEntry
{
	public BattleRewardEntry(
		string rewardType = "",
		string rewardId = "",
		int amount = 0,
		Godot.Collections.Dictionary? metadata = null)
	{
		RewardType = rewardType?.Trim() ?? string.Empty;
		RewardId = rewardId?.Trim() ?? string.Empty;
		Amount = amount;
		Metadata = CloneDictionary(metadata);
	}

	public string RewardType { get; }

	public string RewardId { get; }

	public int Amount { get; }

	public Godot.Collections.Dictionary Metadata { get; }

	public BattleRewardEntry Clone()
	{
		return new BattleRewardEntry(RewardType, RewardId, Amount, Metadata);
	}

	public Godot.Collections.Dictionary ToDictionary()
	{
		return new Godot.Collections.Dictionary
		{
			["reward_type"] = RewardType,
			["reward_id"] = RewardId,
			["amount"] = Amount,
			["metadata"] = CloneDictionary(Metadata),
		};
	}

	public static BattleRewardEntry FromDictionary(Godot.Collections.Dictionary dictionary)
	{
		string rewardType = dictionary.TryGetValue("reward_type", out Variant rewardTypeValue)
			? rewardTypeValue.AsString()
			: string.Empty;
		string rewardId = dictionary.TryGetValue("reward_id", out Variant rewardIdValue)
			? rewardIdValue.AsString()
			: string.Empty;
		int amount = dictionary.TryGetValue("amount", out Variant amountValue)
			? amountValue.AsInt32()
			: 0;
		Godot.Collections.Dictionary metadata = dictionary.TryGetValue("metadata", out Variant metadataValue)
			&& metadataValue.Obj is Godot.Collections.Dictionary rawMetadata
			? CloneDictionary(rawMetadata)
			: new Godot.Collections.Dictionary();

		return new BattleRewardEntry(rewardType, rewardId, amount, metadata);
	}

	private static Godot.Collections.Dictionary CloneDictionary(Godot.Collections.Dictionary? source)
	{
		Godot.Collections.Dictionary clone = new();
		if (source == null)
		{
			return clone;
		}

		foreach (Variant key in source.Keys)
		{
			clone[key] = source[key];
		}

		return clone;
	}
}
