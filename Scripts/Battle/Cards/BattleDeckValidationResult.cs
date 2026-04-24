using System.Collections.Generic;

namespace CardChessDemo.Battle.Cards;

public sealed class BattleDeckValidationResult
{
	public bool IsValid => Errors.Count == 0;

	public bool CanAddCards => !HasMaxDeckSizeViolation
		&& !HasPointBudgetViolation
		&& !HasOverlimitCarryViolation
		&& !HasCycleLimitViolation
		&& !HasQuickCycleLimitViolation
		&& !HasEnergyPositiveLimitViolation
		&& !HasAvailabilityViolation
		&& !HasCopyLimitViolation
		&& !HasSnapshotSchemaViolation;

	public List<string> Errors { get; } = new();

	public List<string> Warnings { get; } = new();

	public int TotalCardCount { get; set; }

	public int TotalBuildPoints { get; set; }

	public int EffectiveMinDeckSize { get; set; }

	public int EffectiveMaxDeckSize { get; set; }

	public int EffectivePointBudget { get; set; }

	public int EffectiveMaxCopiesPerCard { get; set; }

	public int EffectiveCycleCardLimit { get; set; }

	public int EffectiveQuickCycleCardLimit { get; set; }

	public int EffectiveEnergyPositiveCardLimit { get; set; }

	public int EffectiveOverlimitCarrySlots { get; set; }

	public int UsedOverlimitCarrySlots { get; set; }

	public bool HasMinDeckSizeViolation { get; set; }

	public bool HasMaxDeckSizeViolation { get; set; }

	public bool HasPointBudgetViolation { get; set; }

	public bool HasOverlimitCarryViolation { get; set; }

	public bool HasCycleLimitViolation { get; set; }

	public bool HasQuickCycleLimitViolation { get; set; }

	public bool HasEnergyPositiveLimitViolation { get; set; }

	public bool HasAvailabilityViolation { get; set; }

	public bool HasCopyLimitViolation { get; set; }

	public bool HasSnapshotSchemaViolation { get; set; }

	public List<BattleCardTemplate> ResolvedTemplates { get; } = new();

	public List<BattleDeckResolvedCard> ResolvedCards { get; } = new();
}
