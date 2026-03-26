using CardChessDemo.Battle.Boundary;

namespace CardChessDemo.Battle.Services;

public sealed class BattleResolutionPlan
{
	public BattleResult Result { get; init; } = null!;

	public RewardBundle RewardBundle { get; init; } = new();

	public SaveSlotDecision SaveDecision { get; init; } = new();

	public bool ShouldClearEncounter { get; init; }

	public string ClearedEncounterId { get; init; } = string.Empty;

	public bool ShouldReturnToMap { get; init; } = true;
}
