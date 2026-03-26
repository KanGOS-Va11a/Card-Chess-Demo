using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.Services;

public sealed class SaveSlotPolicy
{
	public SaveSlotDecision Evaluate(GlobalGameSession session, BattleResult result)
	{
		return result.Outcome switch
		{
			BattleOutcome.Victory => BuildVictoryDecision(session),
			BattleOutcome.Defeat => BuildDefeatDecision(session),
			BattleOutcome.Retreat => BuildRetreatDecision(session),
			_ => new SaveSlotDecision
			{
				SlotKind = SaveSlotKind.Unknown,
				ShouldWriteSave = false,
				ShouldRollbackOnLoad = false,
				Reason = "No save policy is defined for this outcome.",
			},
		};
	}

	private static SaveSlotDecision BuildVictoryDecision(GlobalGameSession session)
	{
		return new SaveSlotDecision
		{
			SlotKind = SaveSlotKind.Auto,
			ShouldWriteSave = true,
			ShouldRollbackOnLoad = false,
			SlotId = session.SaveState.AutoSaveSlotId,
			Reason = "Victory defaults to auto-save progression.",
		};
	}

	private static SaveSlotDecision BuildDefeatDecision(GlobalGameSession session)
	{
		SaveSlotKind preferredRollback = session.SaveState.PreferredRollbackSlotKind;
		string slotId = preferredRollback switch
		{
			SaveSlotKind.Manual => session.SaveState.LastManualSaveId,
			SaveSlotKind.Auto => session.SaveState.AutoSaveSlotId,
			_ => session.SaveState.LastCheckpointSaveId,
		};

		return new SaveSlotDecision
		{
			SlotKind = preferredRollback,
			ShouldWriteSave = false,
			ShouldRollbackOnLoad = true,
			SlotId = slotId,
			Reason = "Defeat rolls back through save runtime policy.",
		};
	}

	private static SaveSlotDecision BuildRetreatDecision(GlobalGameSession session)
	{
		return new SaveSlotDecision
		{
			SlotKind = SaveSlotKind.Unknown,
			ShouldWriteSave = false,
			ShouldRollbackOnLoad = false,
			SlotId = session.SaveState.AutoSaveSlotId,
			Reason = "Retreat keeps runtime state but does not auto-save by default.",
		};
	}
}
