using Godot;

namespace CardChessDemo.Battle.Shared;

public sealed class PlayerRuntimeState
{
	public string DisplayName { get; set; } = "Traveler";

	public int MaxHp { get; set; } = 12;

	public int CurrentHp { get; set; } = 12;

	public int MovePointsPerTurn { get; set; } = 4;

	public int AttackRange { get; set; } = 1;

	public int AttackDamage { get; set; } = 2;
}

public sealed class CompanionRuntimeState
{
	public string CompanionId { get; set; } = "arakawa";

	public string DisplayName { get; set; } = "荒川";

	public int GrowthLevel { get; set; } = 1;

	public int MaxEnergy { get; set; } = 3;

	public int CurrentEnergy { get; set; } = 3;
}

public sealed class PartyRuntimeState
{
	public PlayerRuntimeState Player { get; } = new();

	public CompanionRuntimeState Arakawa { get; } = new();

	public Godot.Collections.Array<Godot.Collections.Dictionary> ReserveCompanions { get; } = new();
}
