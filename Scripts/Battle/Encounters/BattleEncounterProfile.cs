using System;
using Godot;

namespace CardChessDemo.Battle.Encounters;

[GlobalClass]
public partial class BattleEncounterProfile : Resource
{
    [Export] public string EncounterId { get; set; } = "debug_encounter";
    [Export] public string DisplayName { get; set; } = "Debug Encounter";
    [Export] public string PrimaryEnemyDefinitionId { get; set; } = "battle_enemy";
    [Export] public string[] EnemyTypeIds { get; set; } = Array.Empty<string>();
    [Export] public string PreferredRoomPoolId { get; set; } = string.Empty;
}
