using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class BattleRewardContext
{
	public string EncounterId { get; set; } = string.Empty;

	public string RoomLayoutId { get; set; } = string.Empty;

	public BattleOutcome Outcome { get; set; } = BattleOutcome.Unknown;

	public Godot.Collections.Dictionary RuntimeFlags { get; set; } = new();
}
