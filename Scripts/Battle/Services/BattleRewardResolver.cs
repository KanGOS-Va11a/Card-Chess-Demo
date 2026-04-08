using Godot;
using CardChessDemo.Battle.Boundary;

namespace CardChessDemo.Battle.Services;

public sealed class BattleRewardResolver
{
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
		return bundle;
	}
}
