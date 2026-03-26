using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.Services;

public sealed class BattleResolutionService
{
	private readonly BattleRewardResolver _rewardResolver;
	private readonly SaveSlotPolicy _saveSlotPolicy;

	public BattleResolutionService(
		BattleRewardResolver? rewardResolver = null,
		SaveSlotPolicy? saveSlotPolicy = null)
	{
		_rewardResolver = rewardResolver ?? new BattleRewardResolver();
		_saveSlotPolicy = saveSlotPolicy ?? new SaveSlotPolicy();
	}

	public BattleResolutionPlan Resolve(
		GlobalGameSession session,
		BattleResult result,
		string roomLayoutId = "")
	{
		BattleRewardContext rewardContext = new()
		{
			EncounterId = result.EncounterId,
			RoomLayoutId = roomLayoutId,
			Outcome = result.Outcome,
			RuntimeFlags = CloneDictionary(result.RuntimeFlags),
		};

		RewardBundle rewardBundle = _rewardResolver.Resolve(rewardContext);
		SaveSlotDecision saveDecision = _saveSlotPolicy.Evaluate(session, result);
		bool shouldClearEncounter = result.Outcome == BattleOutcome.Victory
			&& !string.IsNullOrWhiteSpace(result.ClearedEncounterId);

		return new BattleResolutionPlan
		{
			Result = result,
			RewardBundle = rewardBundle,
			SaveDecision = saveDecision,
			ShouldClearEncounter = shouldClearEncounter,
			ClearedEncounterId = shouldClearEncounter ? result.ClearedEncounterId : string.Empty,
			ShouldReturnToMap = true,
		};
	}

	public void Apply(GlobalGameSession session, BattleResolutionPlan plan)
	{
		if (plan.RewardBundle.ProgressionDelta.TryValidate(out _))
		{
			session.ApplyProgressionDelta(plan.RewardBundle.ProgressionDelta);
		}

		if (plan.RewardBundle.InventoryDelta.TryValidate(out _))
		{
			session.ApplyInventoryDelta(plan.RewardBundle.InventoryDelta);
		}

		if (plan.ShouldClearEncounter)
		{
			Godot.Collections.Dictionary rewardFlags = plan.RewardBundle.RuntimeFlags;
			rewardFlags["cleared_encounter_id"] = plan.ClearedEncounterId;
		}
	}

	private static Godot.Collections.Dictionary CloneDictionary(Godot.Collections.Dictionary source)
	{
		Godot.Collections.Dictionary clone = new();
		foreach (Variant key in source.Keys)
		{
			clone[key] = source[key];
		}

		return clone;
	}
}
