using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Enemies;

namespace CardChessDemo.Battle.Services;

public sealed class BattleRewardResolver
{
	private const string Scene01EncounterId = "grunt_debug";
	private const string DefaultEnemyLibraryPath = "res://Resources/Battle/Enemies/DefaultBattleEnemyLibrary.tres";

	private static readonly IReadOnlyDictionary<string, int> EncounterClearBonusByEncounterId =
		new Dictionary<string, int>(StringComparer.Ordinal)
		{
			[Scene01EncounterId] = 10,
		};
	private readonly BattleEnemyLibrary? _enemyLibrary;

	public BattleRewardResolver(BattleEnemyLibrary? enemyLibrary = null)
	{
		_enemyLibrary = enemyLibrary ?? GD.Load<BattleEnemyLibrary>(DefaultEnemyLibraryPath);
	}

	public RewardBundle Resolve(BattleRewardContext context)
	{
		RewardBundle bundle = new();
		if (context.Outcome != BattleOutcome.Victory
			&& context.Outcome != BattleOutcome.Retreat)
		{
			return bundle;
		}

		bundle.RuntimeFlags["reward_context_encounter_id"] = context.EncounterId;
		bundle.RuntimeFlags["reward_context_room_layout_id"] = context.RoomLayoutId;
		AppendEncounteredEnemyFlags(bundle, context);

		AppendDefeatedEnemyRewards(bundle, context);

		if (context.Outcome == BattleOutcome.Victory
			&& IsAllEnemiesDefeated(context)
			&& TryGetEncounterClearBonus(context.EncounterId, out int encounterClearBonus)
			&& encounterClearBonus > 0)
		{
			bundle.ProgressionDelta.ExperienceDelta += encounterClearBonus;
			bundle.AddRewardEntry(new BattleRewardEntry(
				rewardType: "encounter_clear_bonus",
				rewardId: string.IsNullOrWhiteSpace(context.EncounterId) ? "battle_clear_bonus" : context.EncounterId,
				amount: encounterClearBonus,
				metadata: new Godot.Collections.Dictionary
				{
					["encounter_id"] = context.EncounterId,
				}));
		}

		if (context.Outcome == BattleOutcome.Victory)
		{
			AppendLearningRewards(bundle, context);
		}

		return bundle;
	}

	private static void AppendEncounteredEnemyFlags(RewardBundle bundle, BattleRewardContext context)
	{
		if (!context.RuntimeFlags.TryGetValue("encountered_enemy_definition_ids", out Variant encounteredEnemyIdsVariant)
			|| encounteredEnemyIdsVariant.Obj is not Godot.Collections.Array encounteredEnemyIds)
		{
			return;
		}

		Godot.Collections.Array<string> uniqueEncounteredIds = new();
		HashSet<string> seenIds = new(StringComparer.Ordinal);
		foreach (Variant encounteredEnemyIdVariant in encounteredEnemyIds)
		{
			string encounteredEnemyId = encounteredEnemyIdVariant.AsString();
			if (string.IsNullOrWhiteSpace(encounteredEnemyId) || !seenIds.Add(encounteredEnemyId))
			{
				continue;
			}

			uniqueEncounteredIds.Add(encounteredEnemyId);
		}

		if (uniqueEncounteredIds.Count > 0)
		{
			bundle.RuntimeFlags["encountered_enemy_definition_ids"] = uniqueEncounteredIds;
		}
	}

	private void AppendDefeatedEnemyRewards(RewardBundle bundle, BattleRewardContext context)
	{
		foreach (string defeatedEnemyDefinitionId in EnumerateDefeatedEnemyDefinitionIds(context.RuntimeFlags))
		{
			if (!TryGetEnemyExperience(defeatedEnemyDefinitionId, out int enemyExperience)
				|| enemyExperience <= 0)
			{
				continue;
			}

			bundle.ProgressionDelta.ExperienceDelta += enemyExperience;
			bundle.AddRewardEntry(new BattleRewardEntry(
				rewardType: "enemy_defeat_exp",
				rewardId: defeatedEnemyDefinitionId,
				amount: enemyExperience,
				metadata: new Godot.Collections.Dictionary
				{
					["enemy_definition_id"] = defeatedEnemyDefinitionId,
					["battle_outcome"] = context.Outcome.ToString(),
				}));
		}
	}

	private static void AppendLearningRewards(RewardBundle bundle, BattleRewardContext context)
	{
		if (!context.RuntimeFlags.TryGetValue("learned_card_ids", out Variant learnedCardIdsVariant)
			|| learnedCardIdsVariant.Obj is not Godot.Collections.Array learnedCardIds)
		{
			return;
		}

		List<string> unlockedCardIds = new();
		foreach (Variant learnedCardIdVariant in learnedCardIds)
		{
			string learnedCardId = learnedCardIdVariant.AsString();
			if (string.IsNullOrWhiteSpace(learnedCardId))
			{
				continue;
			}

			unlockedCardIds.Add(learnedCardId);
			bundle.AddRewardEntry(new BattleRewardEntry(
				rewardType: "learned_card",
				rewardId: learnedCardId,
				amount: 1,
				metadata: new Godot.Collections.Dictionary
				{
					["source_enemy_definition_id"] = string.Empty,
				}));
		}

		bundle.ProgressionDelta.UnlockedCardIds = unlockedCardIds
			.Distinct(StringComparer.Ordinal)
			.ToArray();
		bundle.RuntimeFlags["learning_rewards_granted"] = unlockedCardIds.Count > 0;
	}

	private static IEnumerable<string> EnumerateDefeatedEnemyDefinitionIds(Godot.Collections.Dictionary runtimeFlags)
	{
		if (!runtimeFlags.TryGetValue("defeated_enemy_definition_ids", out Variant defeatedEnemyIdsVariant)
			|| defeatedEnemyIdsVariant.Obj is not Godot.Collections.Array defeatedEnemyIds)
		{
			yield break;
		}

		foreach (Variant defeatedEnemyIdVariant in defeatedEnemyIds)
		{
			string defeatedEnemyId = defeatedEnemyIdVariant.AsString();
			if (!string.IsNullOrWhiteSpace(defeatedEnemyId))
			{
				yield return defeatedEnemyId;
			}
		}
	}

	private static bool IsAllEnemiesDefeated(BattleRewardContext context)
	{
		return context.RuntimeFlags.TryGetValue("all_enemies_defeated", out Variant allEnemiesDefeatedVariant)
			&& allEnemiesDefeatedVariant.AsBool();
	}

	private bool TryGetEnemyExperience(string enemyDefinitionId, out int experience)
	{
		experience = _enemyLibrary?.FindEntry(enemyDefinitionId ?? string.Empty)?.DefeatExperience ?? 0;
		return experience > 0;
	}

	private static bool TryGetEncounterClearBonus(string encounterId, out int experience)
	{
		return EncounterClearBonusByEncounterId.TryGetValue(encounterId ?? string.Empty, out experience);
	}
}
