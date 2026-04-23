using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Battle.Cards;

[GlobalClass]
public partial class BattleCardLibrary : Resource
{
	[Export] public BattleCardTemplate[] Entries { get; set; } = Array.Empty<BattleCardTemplate>();

	private BattleCardTemplate[]? _allEntries;

	public IReadOnlyList<BattleCardTemplate> AllEntries => GetAllEntries();

	public BattleCardTemplate? FindTemplate(string cardId)
	{
		return GetAllEntries().FirstOrDefault(entry => string.Equals(entry.CardId, cardId, StringComparison.Ordinal));
	}

	public BattleCardDefinition[] BuildDefinitions()
	{
		return GetAllEntries()
			.Where(entry => entry != null)
			.Select(entry => entry.BuildRuntimeDefinition())
			.ToArray();
	}

	public string[] BuildStarterDeckCardIds()
	{
		List<string> cardIds = new();
		foreach (BattleCardTemplate template in GetAllEntries().Where(entry => entry != null))
		{
			int copies = Math.Max(0, template.DefaultStarterCopies);
			for (int copyIndex = 0; copyIndex < copies; copyIndex++)
			{
				cardIds.Add(template.CardId);
			}
		}

		return cardIds.ToArray();
	}

	private BattleCardTemplate[] GetAllEntries()
	{
		if (_allEntries != null)
		{
			return _allEntries;
		}

		List<BattleCardTemplate> mergedEntries = Entries
			.Where(entry => entry != null)
			.ToList();

		foreach (BattleCardTemplate template in CreateSupplementalTemplates())
		{
			if (mergedEntries.Any(existing => string.Equals(existing.CardId, template.CardId, StringComparison.Ordinal)))
			{
				continue;
			}

			mergedEntries.Add(template);
		}

		foreach (BattleCardTemplate template in mergedEntries)
		{
			ApplyImplicitBalanceMetadata(template);
		}

		_allEntries = mergedEntries.ToArray();
		return _allEntries;
	}

	private static void ApplyImplicitBalanceMetadata(BattleCardTemplate template)
	{
		switch (template.CardId)
		{
			case "card_plunder":
				template.RequiredMeleeMastery = Math.Max(template.RequiredMeleeMastery, 1);
				break;
			case "draw_revolver":
				template.RequiredRangedMastery = Math.Max(template.RequiredRangedMastery, 1);
				break;
			case "card_repair":
				template.RequiredFlexMastery = Math.Max(template.RequiredFlexMastery, 1);
				break;
			case "card_pressure_breach":
				template.RequiredMeleeMastery = Math.Max(template.RequiredMeleeMastery, 2);
				template.RequiredFlexMastery = Math.Max(template.RequiredFlexMastery, 1);
				template.OverlimitEffectMultiplier = Math.Max(template.OverlimitEffectMultiplier, 0.85f);
				break;
			case "card_ranging_barrage":
				template.RequiredRangedMastery = Math.Max(template.RequiredRangedMastery, 2);
				break;
			case "card_chain_detonation":
				template.RequiredFlexMastery = Math.Max(template.RequiredFlexMastery, 2);
				break;
			case "card_magnetic_hunt":
				template.RequiredMeleeMastery = Math.Max(template.RequiredMeleeMastery, 2);
				template.RequiredFlexMastery = Math.Max(template.RequiredFlexMastery, 1);
				break;
			case "card_field_patch_plus":
				template.RequiredFlexMastery = Math.Max(template.RequiredFlexMastery, 1);
				break;
			case "card_optimize":
			case "card_contemplate":
				template.RequiredFlexMastery = Math.Max(template.RequiredFlexMastery, 1);
				break;
		}
	}

	private static BattleCardTemplate[] CreateSupplementalTemplates()
	{
		return new[]
		{
			new BattleCardTemplate
			{
				CardId = "card_momentum_slice",
				DisplayName = "\u987A\u52BF\u5207\u5F00",
				Description = "\u5BF9\u76F8\u90BB\u654C\u4EBA\u9020\u6210 5 \u70B9\u4F24\u5BB3\uFF0CQuick\uFF0C\u6D88\u8017\u3002",
				Category = BattleCardCategory.Attack,
				TargetingMode = BattleCardTargetingMode.EnemyUnit,
				Cost = 1,
				Range = 1,
				Damage = 5,
				IsQuick = true,
				ExhaustsOnPlay = true,
				BuildPoints = 4,
				MaxCopiesInDeck = 1,
				UnlockedByDefault = false,
				RequiredMeleeMastery = 1,
				RequiredFlexMastery = 1,
			},
			new BattleCardTemplate
			{
				CardId = "card_salvage_focus",
				DisplayName = "\u56DE\u6536\u4E13\u6CE8",
				Description = "\u62BD 2 \u5F20\u724C\u5E76\u56DE\u590D 1 \u70B9\u80FD\u91CF\uFF0CQuick\uFF0C\u6D88\u8017\u3002",
				Category = BattleCardCategory.Skill,
				TargetingMode = BattleCardTargetingMode.None,
				Cost = 1,
				DrawCount = 2,
				EnergyGain = 1,
				IsQuick = true,
				ExhaustsOnPlay = true,
				BuildPoints = 4,
				MaxCopiesInDeck = 1,
				UnlockedByDefault = false,
				RequiredFlexMastery = 2,
			},
			new BattleCardTemplate
			{
				CardId = "card_overclock_beam",
				DisplayName = "\u8FC7\u8F7D\u5149\u675F",
				Description = "\u5BF9\u76F4\u7EBF\u9996\u4E2A\u654C\u4EBA\u9020\u6210 10 \u70B9\u4F24\u5BB3\uFF0C\u6D88\u8017\u3002",
				Category = BattleCardCategory.Attack,
				TargetingMode = BattleCardTargetingMode.StraightLineEnemy,
				Cost = 3,
				Range = 4,
				Damage = 10,
				ExhaustsOnPlay = true,
				BuildPoints = 5,
				MaxCopiesInDeck = 1,
				UnlockedByDefault = false,
				RequiredRangedMastery = 2,
				DisallowOverlimitCarry = true,
			},
		};
	}
}
