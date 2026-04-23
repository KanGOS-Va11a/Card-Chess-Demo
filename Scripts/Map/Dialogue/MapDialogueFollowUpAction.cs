using Godot;

namespace CardChessDemo.Map;

public sealed class MapDialogueFollowUpAction
{
	public MapDialogueFollowUpKind Kind { get; set; } = MapDialogueFollowUpKind.None;
	public NodePath TargetNodePath { get; set; } = new("");
	public string BattleEncounterId { get; set; } = string.Empty;
	public PackedScene? BattleScene { get; set; }
	public string BattleScenePath { get; set; } = string.Empty;
	public string NextScenePath { get; set; } = string.Empty;
	public string NextSceneSpawnId { get; set; } = string.Empty;
}
