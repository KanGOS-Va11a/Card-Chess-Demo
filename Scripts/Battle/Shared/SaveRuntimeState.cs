namespace CardChessDemo.Battle.Shared;

public sealed class SaveRuntimeState
{
	public string LastCheckpointSaveId { get; set; } = string.Empty;

	public string LastManualSaveId { get; set; } = string.Empty;

	public string AutoSaveSlotId { get; set; } = "autosave";

	public string LastCheckpointScenePath { get; set; } = string.Empty;

	public string LastCheckpointMapId { get; set; } = string.Empty;

	public string LastCheckpointSpawnId { get; set; } = string.Empty;

	public string LastAutoSaveTimestampUtc { get; set; } = string.Empty;

	public SaveSlotKind PreferredRollbackSlotKind { get; set; } = SaveSlotKind.Checkpoint;
}
