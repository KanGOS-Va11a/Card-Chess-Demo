using System;
using CardChessDemo.UI.Dialogue;
using Godot;

namespace CardChessDemo.Map;

public sealed class MapDialogueRequest
{
	public PackedScene? PanelScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/DialogueSequencePanel.tscn");
	public DialoguePage[] Pages { get; set; } = Array.Empty<DialoguePage>();
	public bool LockPlayerInput { get; set; } = true;
	public bool RejectIfAnotherDialogueVisible { get; set; } = true;
	public string SourceId { get; set; } = string.Empty;
	public MapDialogueFollowUpAction[] CompletedFollowUpActions { get; set; } = Array.Empty<MapDialogueFollowUpAction>();
	public MapDialogueFollowUpAction[] ClosedFollowUpActions { get; set; } = Array.Empty<MapDialogueFollowUpAction>();
}
