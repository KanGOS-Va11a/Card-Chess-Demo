using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.Services;

public sealed class SaveSlotDecision
{
	public SaveSlotKind SlotKind { get; set; } = SaveSlotKind.Unknown;

	public bool ShouldWriteSave { get; set; }

	public bool ShouldRollbackOnLoad { get; set; }

	public string SlotId { get; set; } = string.Empty;

	public string Reason { get; set; } = string.Empty;
}
