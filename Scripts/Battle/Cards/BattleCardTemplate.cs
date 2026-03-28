using System;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Boundary;

namespace CardChessDemo.Battle.Cards;

[GlobalClass]
public partial class BattleCardTemplate : Resource
{
	[Export] public string CardId { get; set; } = "card_id";
	[Export] public string DisplayName { get; set; } = "新卡牌";
	[Export(PropertyHint.MultilineText)] public string Description { get; set; } = string.Empty;
	[Export] public BattleCardCategory Category { get; set; } = BattleCardCategory.Skill;
	[Export] public BattleCardTargetingMode TargetingMode { get; set; } = BattleCardTargetingMode.None;
	[Export] public int Cost { get; set; }
	[Export] public int Range { get; set; }
	[Export] public int Damage { get; set; }
	[Export] public int HealingAmount { get; set; }
	[Export] public int DrawCount { get; set; }
	[Export] public int EnergyGain { get; set; }
	[Export] public int ShieldGain { get; set; }
	[Export] public bool IsQuick { get; set; }
	[Export] public bool ExhaustsOnPlay { get; set; }
	[Export] public int BuildPoints { get; set; } = 1;
	[Export] public int MaxCopiesInDeck { get; set; } = 3;
	[Export] public int DefaultStarterCopies { get; set; } = 0;
	[Export] public bool UnlockedByDefault { get; set; } = true;
	[Export] public int RequiredPlayerLevel { get; set; } = 1;
	[Export] public string[] RequiredTalentIds { get; set; } = Array.Empty<string>();
	[Export] public string[] RequiredBranchTags { get; set; } = Array.Empty<string>();

	public BattleCardDefinition BuildRuntimeDefinition()
	{
		return new BattleCardDefinition(
			cardId: CardId,
			displayName: DisplayName,
			description: Description,
			cost: Cost,
			category: Category,
			targetingMode: TargetingMode,
			range: Range,
			damage: Damage,
			healingAmount: HealingAmount,
			drawCount: DrawCount,
			energyGain: EnergyGain,
			shieldGain: ShieldGain,
			isQuick: IsQuick,
			exhaustsOnPlay: ExhaustsOnPlay);
	}

	public bool IsUnlocked(ProgressionSnapshot snapshot)
	{
		if (UnlockedByDefault)
		{
			return true;
		}

		if (snapshot.UnlockedCardIds.Contains(CardId, StringComparer.Ordinal))
		{
			return true;
		}

		if (snapshot.PlayerLevel < Math.Max(1, RequiredPlayerLevel))
		{
			return false;
		}

		if (RequiredTalentIds.Any(requiredTalentId => !snapshot.TalentIds.Contains(requiredTalentId, StringComparer.Ordinal)))
		{
			return false;
		}

		if (RequiredBranchTags.Any(requiredBranchTag => !snapshot.TalentBranchTags.Contains(requiredBranchTag, StringComparer.Ordinal)))
		{
			return false;
		}

		return RequiredTalentIds.Length > 0 || RequiredBranchTags.Length > 0 || RequiredPlayerLevel > 1;
	}
}
