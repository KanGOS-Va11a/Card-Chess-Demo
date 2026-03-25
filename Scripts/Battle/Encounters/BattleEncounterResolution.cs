using System;

namespace CardChessDemo.Battle.Encounters;

public sealed class BattleEncounterResolution
{
	public string EncounterId { get; init; } = string.Empty;

	public string DisplayName { get; init; } = string.Empty;

	public string PrimaryEnemyDefinitionId { get; init; } = "battle_enemy";

	public string[] EnemyTypeIds { get; init; } = Array.Empty<string>();

	public string PreferredRoomPoolId { get; init; } = string.Empty;
}
