using Godot;
using CardChessDemo.Battle.Boundary;

namespace CardChessDemo.Battle.Services;

public sealed class BattleRewardResolver
{
	private const string Scene01EncounterId = "grunt_debug";

	public RewardBundle Resolve(BattleRewardContext context)
	{
		RewardBundle bundle = new();
		if (context.Outcome != BattleOutcome.Victory)
		{
			return bundle;
		}

		// 当前只提供服务边界和最小可扩展骨架，不在这里硬编码正式奖励表。
		bundle.RuntimeFlags["reward_context_encounter_id"] = context.EncounterId;
		bundle.RuntimeFlags["reward_context_room_layout_id"] = context.RoomLayoutId;

		if (string.Equals(context.EncounterId, Scene01EncounterId, System.StringComparison.Ordinal))
		{
			bundle.ProgressionDelta.ExperienceDelta += 40;
			if (context.RuntimeFlags.TryGetValue("learned_card_ids", out Variant learnedCardIdsVariant)
				&& learnedCardIdsVariant.Obj is Godot.Collections.Array learnedCardIds)
			{
				System.Collections.Generic.List<string> unlockedCardIds = new();
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
							["source_enemy_definition_id"] = "scene01_tutorial_enemy",
						}));
				}

				bundle.ProgressionDelta.UnlockedCardIds = unlockedCardIds.ToArray();
				bundle.RuntimeFlags["scene01_learning_rewards_granted"] = unlockedCardIds.Count > 0;
			}
		}

		return bundle;
	}
}
