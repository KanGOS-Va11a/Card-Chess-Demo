using System;
using System.Collections.Generic;
using System.Linq;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Cards;
using CardChessDemo.Battle.Shared;
using Godot;

namespace CardChessDemo.Map;

public static class InteractableRewardResolver
{
	private static BattleCardLibrary? _cardLibrary;

	public static bool ApplyConfiguredRewards(GlobalGameSession? session, IEnumerable<(string ItemId, int Amount)> rewards)
	{
		if (session == null || rewards == null)
		{
			return false;
		}

		InventoryDelta inventoryDelta = new();
		ProgressionDelta progressionDelta = new();
		HashSet<string> unlockedCardIds = new(StringComparer.Ordinal);

		foreach ((string itemId, int amount) in rewards)
		{
			string normalizedId = itemId?.Trim() ?? string.Empty;
			if (string.IsNullOrWhiteSpace(normalizedId) || amount <= 0)
			{
				continue;
			}

			if (IsCardUnlockReward(normalizedId))
			{
				unlockedCardIds.Add(normalizedId);
				continue;
			}

			inventoryDelta.ItemDeltas[normalizedId] = inventoryDelta.ItemDeltas.TryGetValue(normalizedId, out int currentAmount)
				? currentAmount + amount
				: amount;
		}

		if (unlockedCardIds.Count > 0)
		{
			progressionDelta.UnlockedCardIds = unlockedCardIds.ToArray();
			session.ApplyProgressionDelta(progressionDelta);
		}

		if (inventoryDelta.ItemDeltas.Count > 0 || inventoryDelta.KeyItemUnlockIds.Length > 0)
		{
			session.ApplyInventoryDelta(inventoryDelta);
		}

		return unlockedCardIds.Count > 0 || inventoryDelta.ItemDeltas.Count > 0;
	}

	public static bool IsCardUnlockReward(string rewardId)
	{
		if (string.IsNullOrWhiteSpace(rewardId))
		{
			return false;
		}

		string normalizedId = rewardId.Trim();
		if (normalizedId.StartsWith("card_", StringComparison.Ordinal))
		{
			return true;
		}

		return ResolveCardLibrary().FindTemplate(normalizedId) != null;
	}

	public static string ResolveRewardDisplayName(GlobalGameSession? session, string rewardId)
	{
		if (string.IsNullOrWhiteSpace(rewardId))
		{
			return string.Empty;
		}

		string normalizedId = rewardId.Trim();
		if (session?.FindEquipmentDefinition(normalizedId) is { } equipmentDefinition)
		{
			return equipmentDefinition.DisplayName;
		}

		BattleCardTemplate? cardTemplate = ResolveCardLibrary().FindTemplate(normalizedId);
		if (cardTemplate != null)
		{
			return string.IsNullOrWhiteSpace(cardTemplate.DisplayName) ? normalizedId : cardTemplate.DisplayName;
		}

		return normalizedId;
	}

	private static BattleCardLibrary ResolveCardLibrary()
	{
		_cardLibrary ??= GD.Load<BattleCardLibrary>("res://Resources/Battle/Cards/DefaultBattleCardLibrary.tres")
			?? new BattleCardLibrary();
		return _cardLibrary;
	}
}
