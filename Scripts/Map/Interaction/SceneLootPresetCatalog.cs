using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CardChessDemo.Map;

public static class SceneLootPresetCatalog
{
	private static readonly Dictionary<string, IReadOnlyList<LootPreset>> PresetsByScene = new(StringComparer.OrdinalIgnoreCase)
	{
		["res://Scene/Maps/Scene01.tscn"] = new[]
		{
			Loot("equip_arc_pipe"),
			Loot("arakawa_battery"),
		},
		["res://Scene/Maps/Scene03.tscn"] = new[]
		{
			Loot("card_tactical_shift"),
			Loot("equip_magnetic_scabbard"),
			Loot("card_alert_guard"),
			Loot("card_structural_boost"),
			Loot("charged_core"),
			Loot("medical_gel"),
			Loot("arakawa_battery"),
			Loot("card_momentum_slice"),
		},
		["res://Scene/Maps/Scene04.tscn"] = Array.Empty<LootPreset>(),
		["res://Scene/Maps/Scene05.tscn"] = new[]
		{
			Loot("equip_target_lens"),
			Loot("card_optimize"),
			Loot("card_contemplate"),
			Loot("charged_core", 2),
		},
		["res://Scene/Maps/Scene06.tscn"] = new[]
		{
			Loot("equip_phase_boots"),
			Loot("equip_archive_probe"),
			Loot("card_salvage_focus"),
			Loot("card_momentum_slice"),
			Loot("equip_parallel_battery"),
			Loot("equip_forbidden_patch"),
			Loot("equip_insulated_cloak"),
			Loot("arakawa_battery", 2),
			Loot("medical_gel", 2),
			Loot("charged_core", 2),
		},
	};

	public static void ApplyToScene(Node sceneRoot, string scenePath)
	{
		if (sceneRoot == null || string.IsNullOrWhiteSpace(scenePath))
		{
			return;
		}

		if (!PresetsByScene.TryGetValue(scenePath, out IReadOnlyList<LootPreset>? presets) || presets.Count == 0)
		{
			return;
		}

		IConfigurableLootInteractable[] interactables = EnumerateInteractables(sceneRoot)
			.OrderBy(interactable => GetSortPosition(interactable).Y)
			.ThenBy(interactable => GetSortPosition(interactable).X)
			.ThenBy(interactable => (interactable as Node)?.Name.ToString() ?? string.Empty, StringComparer.Ordinal)
			.ToArray();

		for (int index = 0; index < interactables.Length && index < presets.Count; index++)
		{
			ApplyPreset(interactables[index], presets[index]);
		}
	}

	private static IEnumerable<IConfigurableLootInteractable> EnumerateInteractables(Node root)
	{
		if (root is IConfigurableLootInteractable interactable)
		{
			yield return interactable;
		}

		foreach (Node child in root.GetChildren())
		{
			foreach (IConfigurableLootInteractable nested in EnumerateInteractables(child))
			{
				yield return nested;
			}
		}
	}

	private static Vector2 GetSortPosition(IConfigurableLootInteractable interactable)
	{
		if (interactable is Node2D node2D)
		{
			return node2D.GlobalPosition;
		}

		return Vector2.Zero;
	}

	private static void ApplyPreset(IConfigurableLootInteractable interactable, LootPreset preset)
	{
		if (interactable == null)
		{
			return;
		}

		interactable.GrantedItems = BuildGrantArray(preset.Grants);
		interactable.InteractionTexts = Array.Empty<string>();

		if (interactable is Chest chest)
		{
			chest.GrantedItemId = string.Empty;
			chest.ItemDescription = string.Empty;
			chest.EmptyDescription = "\u7BB1\u5B50\u662F\u7A7A\u7684\u3002";
		}
		else if (interactable is Cabinet cabinet)
		{
			cabinet.LootItemId = string.Empty;
			cabinet.LootAmount = 0;
			cabinet.ItemDescription = string.Empty;
			cabinet.EmptyDescription = "\u8FD9\u4E2A\u67DC\u5B50\u5DF2\u7ECF\u88AB\u641C\u7A7A\u4E86\u3002";
		}
	}

	private static Godot.Collections.Array<InteractableItemGrant> BuildGrantArray(IReadOnlyList<LootGrant> grants)
	{
		Godot.Collections.Array<InteractableItemGrant> array = new();
		foreach (LootGrant grant in grants)
		{
			if (string.IsNullOrWhiteSpace(grant.ItemId) || grant.Amount <= 0)
			{
				continue;
			}

			array.Add(new InteractableItemGrant
			{
				ItemId = grant.ItemId,
				Amount = grant.Amount,
			});
		}

		return array;
	}

	private static LootPreset Loot(string itemId, int amount = 1)
	{
		return new LootPreset(new[] { new LootGrant(itemId, amount) });
	}

	private readonly record struct LootGrant(string ItemId, int Amount);

	private readonly record struct LootPreset(IReadOnlyList<LootGrant> Grants);
}
