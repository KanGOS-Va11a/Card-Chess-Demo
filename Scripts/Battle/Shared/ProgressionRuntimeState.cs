using System;

namespace CardChessDemo.Battle.Shared;

public sealed class ProgressionRuntimeState
{
	public int PlayerLevel { get; set; } = 1;

	public int PlayerExperience { get; set; }

	public int PlayerMasteryPoints { get; set; }

	public int ArakawaGrowthLevel { get; set; } = 1;

	public string[] TalentIds { get; set; } = Array.Empty<string>();

	public string[] ArakawaUnlockIds { get; set; } = Array.Empty<string>();

	public string[] UnlockedCardIds { get; set; } = Array.Empty<string>();

	public string[] TalentBranchTags { get; set; } = Array.Empty<string>();

	public int DeckPointBudgetBonus { get; set; }

	public int DeckMinCardCountDelta { get; set; }

	public int DeckMaxCardCountDelta { get; set; }

	public int DeckMaxCopiesPerCardBonus { get; set; }
}
