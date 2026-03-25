using System.Collections.Generic;
using Godot;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.Boundary;

public sealed class BattleResult
{
	public BattleResult(Godot.Collections.Dictionary? playerSnapshot = null, bool didPlayerFail = false)
		: this(
			requestId: string.Empty,
			encounterId: string.Empty,
			outcome: didPlayerFail ? BattleOutcome.Defeat : BattleOutcome.Victory,
			playerSnapshot: playerSnapshot)
	{
	}

	public BattleResult(
		string requestId = "",
		string encounterId = "",
		BattleOutcome outcome = BattleOutcome.Unknown,
		Godot.Collections.Dictionary? playerSnapshot = null,
		Godot.Collections.Dictionary? companionSnapshot = null,
		Godot.Collections.Dictionary? progressionDelta = null,
		Godot.Collections.Dictionary? inventoryDelta = null,
		IEnumerable<BattleRewardEntry>? rewardEntries = null,
		string clearedEncounterId = "",
		Godot.Collections.Dictionary? runtimeFlags = null)
	{
		RequestId = requestId?.Trim() ?? string.Empty;
		EncounterId = encounterId?.Trim() ?? string.Empty;
		Outcome = outcome;
		PlayerSnapshot = CloneDictionary(playerSnapshot);
		CompanionSnapshot = CloneDictionary(companionSnapshot);
		ProgressionDelta = CloneDictionary(progressionDelta);
		InventoryDelta = CloneDictionary(inventoryDelta);
		ClearedEncounterId = clearedEncounterId?.Trim() ?? string.Empty;
		RuntimeFlags = CloneDictionary(runtimeFlags);
		RewardEntries = CloneRewardEntries(rewardEntries);
	}

	public string RequestId { get; }

	public string EncounterId { get; }

	public BattleOutcome Outcome { get; }

	public Godot.Collections.Dictionary PlayerSnapshot { get; }

	public Godot.Collections.Dictionary CompanionSnapshot { get; }

	public Godot.Collections.Dictionary ProgressionDelta { get; }

	public Godot.Collections.Dictionary InventoryDelta { get; }

	public Godot.Collections.Array<Godot.Collections.Dictionary> RewardEntries { get; }

	public string ClearedEncounterId { get; }

	public Godot.Collections.Dictionary RuntimeFlags { get; }

	public bool DidPlayerFail => Outcome == BattleOutcome.Defeat;

	public bool DidPlayerWin => Outcome == BattleOutcome.Victory;

	public bool DidPlayerRetreat => Outcome == BattleOutcome.Retreat;

	public ProgressionDelta GetProgressionDeltaModel()
	{
		return CardChessDemo.Battle.Boundary.ProgressionDelta.FromDictionary(ProgressionDelta);
	}

	public InventoryDelta GetInventoryDeltaModel()
	{
		return CardChessDemo.Battle.Boundary.InventoryDelta.FromDictionary(InventoryDelta);
	}

	public bool TryValidate(out string failureReason)
	{
		if (Outcome == BattleOutcome.Unknown)
		{
			failureReason = "BattleResult.Outcome must be resolved before completion.";
			return false;
		}

		if (!PlayerSnapshot.TryGetValue("current_hp", out _))
		{
			failureReason = "BattleResult.PlayerSnapshot.current_hp is required.";
			return false;
		}

		if (!GetProgressionDeltaModel().TryValidate(out failureReason))
		{
			return false;
		}

		if (!GetInventoryDeltaModel().TryValidate(out failureReason))
		{
			return false;
		}

		failureReason = string.Empty;
		return true;
	}

	public static BattleResult FromSession(
		GlobalGameSession session,
		bool didPlayerFail,
		string requestId = "",
		string encounterId = "",
		string clearedEncounterId = "")
	{
		return new BattleResult(
			requestId: requestId,
			encounterId: encounterId,
			outcome: didPlayerFail ? BattleOutcome.Defeat : BattleOutcome.Victory,
			playerSnapshot: session.BuildPlayerSnapshot(),
			companionSnapshot: session.BuildCompanionSnapshot(),
			clearedEncounterId: clearedEncounterId);
	}

	public static BattleResult FromSession(
		GlobalGameSession session,
		BattleOutcome outcome,
		string requestId = "",
		string encounterId = "",
		string clearedEncounterId = "",
		Godot.Collections.Dictionary? runtimeFlags = null)
	{
		Godot.Collections.Dictionary flags = CloneDictionary(runtimeFlags);
		if (outcome == BattleOutcome.Retreat)
		{
			flags["retreat_succeeded"] = true;
		}

		return new BattleResult(
			requestId: requestId,
			encounterId: encounterId,
			outcome: outcome,
			playerSnapshot: session.BuildPlayerSnapshot(),
			companionSnapshot: session.BuildCompanionSnapshot(),
			clearedEncounterId: clearedEncounterId,
			runtimeFlags: flags);
	}

	public void ApplyToSession(GlobalGameSession session)
	{
		session.ApplyPlayerSnapshot(PlayerSnapshot);
		session.ApplyCompanionSnapshot(CompanionSnapshot);
		session.ApplyProgressionDelta(ProgressionDelta);
		session.ApplyInventoryDelta(InventoryDelta);
	}

	private static Godot.Collections.Array<Godot.Collections.Dictionary> CloneRewardEntries(IEnumerable<BattleRewardEntry>? source)
	{
		Godot.Collections.Array<Godot.Collections.Dictionary> clone = new();
		if (source == null)
		{
			return clone;
		}

		foreach (BattleRewardEntry entry in source)
		{
			if (entry != null)
			{
				clone.Add(entry.ToDictionary());
			}
		}

		return clone;
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
