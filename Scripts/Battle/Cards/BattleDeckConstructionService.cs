using System;
using System.Collections.Generic;
using System.Linq;
using CardChessDemo.Battle.Boundary;

namespace CardChessDemo.Battle.Cards;

public sealed class BattleDeckConstructionService
{
	private readonly BattleCardLibrary _library;
	private readonly BattleDeckBuildRules _rules;

	public BattleDeckConstructionService(BattleCardLibrary library, BattleDeckBuildRules rules)
	{
		_library = library;
		_rules = rules;
	}

	public IReadOnlyList<BattleCardTemplate> GetAvailableCardPool(ProgressionSnapshot snapshot)
	{
		return _library.Entries
			.Where(template => template != null && template.IsUnlocked(snapshot))
			.OrderBy(template => template.CardId, StringComparer.Ordinal)
			.ToArray();
	}

	public BattleDeckValidationResult ValidateDeck(DeckBuildSnapshot snapshot, ProgressionSnapshot progression)
	{
		BattleDeckValidationResult result = new()
		{
			EffectiveMinDeckSize = Math.Max(1, _rules.MinDeckSize + progression.DeckMinCardCountDelta),
			EffectiveMaxDeckSize = Math.Max(1, _rules.MaxDeckSize + progression.DeckMaxCardCountDelta),
			EffectivePointBudget = Math.Max(0, _rules.BasePointBudget + progression.DeckPointBudgetBonus),
			EffectiveMaxCopiesPerCard = Math.Max(1, _rules.BaseMaxCopiesPerCard + progression.DeckMaxCopiesPerCardBonus),
		};

		Dictionary<string, int> copyCounts = new(StringComparer.Ordinal);
		foreach (string cardId in snapshot.CardIds)
		{
			BattleCardTemplate? template = _library.FindTemplate(cardId);
			if (template == null)
			{
				result.Errors.Add($"Card '{cardId}' was not found in BattleCardLibrary.");
				continue;
			}

			if (!template.IsUnlocked(progression))
			{
				result.Errors.Add($"Card '{cardId}' is not unlocked for the current progression snapshot.");
				continue;
			}

			result.ResolvedTemplates.Add(template);
			copyCounts.TryGetValue(cardId, out int currentCopies);
			copyCounts[cardId] = currentCopies + 1;
			result.TotalBuildPoints += Math.Max(0, template.BuildPoints);
		}

		result.TotalCardCount = result.ResolvedTemplates.Count;

		if (result.TotalCardCount < result.EffectiveMinDeckSize)
		{
			result.Errors.Add($"Deck size is below minimum. Need {result.EffectiveMinDeckSize}, got {result.TotalCardCount}.");
		}

		if (result.TotalCardCount > result.EffectiveMaxDeckSize)
		{
			result.Errors.Add($"Deck size exceeds maximum. Max {result.EffectiveMaxDeckSize}, got {result.TotalCardCount}.");
		}

		if (result.TotalBuildPoints > result.EffectivePointBudget)
		{
			result.Errors.Add($"Deck build points exceed budget. Budget {result.EffectivePointBudget}, got {result.TotalBuildPoints}.");
		}

		foreach ((string cardId, int copies) in copyCounts)
		{
			BattleCardTemplate template = _library.FindTemplate(cardId)!;
			int maxCopies = Math.Min(result.EffectiveMaxCopiesPerCard, Math.Max(1, template.MaxCopiesInDeck));
			if (copies > maxCopies)
			{
				result.Errors.Add($"Card '{cardId}' exceeds copy limit. Max {maxCopies}, got {copies}.");
			}
		}

		if (!snapshot.TryValidate(out string failureReason))
		{
			result.Errors.Add(failureReason);
		}

		if (!progression.TryValidate(out failureReason))
		{
			result.Errors.Add(failureReason);
		}

		return result;
	}

	public BattleCardDefinition[] BuildRuntimeDefinitions(DeckBuildSnapshot snapshot, ProgressionSnapshot progression, out BattleDeckValidationResult validationResult)
	{
		validationResult = ValidateDeck(snapshot, progression);
		if (!validationResult.IsValid)
		{
			return Array.Empty<BattleCardDefinition>();
		}

		return validationResult.ResolvedTemplates
			.Select(template => template.BuildRuntimeDefinition())
			.ToArray();
	}
}
