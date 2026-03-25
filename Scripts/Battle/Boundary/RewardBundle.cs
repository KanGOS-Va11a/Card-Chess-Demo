using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class RewardBundle
{
	public ProgressionDelta ProgressionDelta { get; set; } = new();

	public InventoryDelta InventoryDelta { get; set; } = new();

	public Godot.Collections.Array<Godot.Collections.Dictionary> RewardEntries { get; } = new();

	public Godot.Collections.Dictionary RuntimeFlags { get; set; } = new();

	public void AddRewardEntry(BattleRewardEntry entry)
	{
		RewardEntries.Add(entry.ToDictionary());
	}

	public void AddRewardEntries(IEnumerable<BattleRewardEntry> entries)
	{
		foreach (BattleRewardEntry entry in entries)
		{
			AddRewardEntry(entry);
		}
	}
}
