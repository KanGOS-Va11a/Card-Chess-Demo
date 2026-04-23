using System;
using System.Linq;
using CardChessDemo.Battle.Boundary;

namespace CardChessDemo.Battle.Cards;

public sealed class BattleDeckResolvedCard
{
	public BattleDeckResolvedCard(BattleCardTemplate template, ProgressionSnapshot progression, bool usesOverlimitCarry, int appliedBuildPoints)
	{
		Template = template ?? throw new ArgumentNullException(nameof(template));
		Progression = progression ?? throw new ArgumentNullException(nameof(progression));
		UsesOverlimitCarry = usesOverlimitCarry;
		AppliedBuildPoints = Math.Max(0, appliedBuildPoints);
		CycleTags = template.GetNormalizedCycleTags().ToArray();
	}

	public BattleCardTemplate Template { get; }

	public ProgressionSnapshot Progression { get; }

	public bool UsesOverlimitCarry { get; }

	public int AppliedBuildPoints { get; }

	public string[] CycleTags { get; }

	public BattleCardDefinition BuildRuntimeDefinition()
	{
		return Template.BuildRuntimeDefinition(UsesOverlimitCarry, Progression);
	}
}
