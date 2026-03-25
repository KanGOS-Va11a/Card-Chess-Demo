using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class InventoryDelta
{
	public Dictionary<string, int> ItemDeltas { get; } = new(StringComparer.Ordinal);

	public string[] KeyItemUnlockIds { get; set; } = Array.Empty<string>();

	public bool TryValidate(out string failureReason)
	{
		if (ItemDeltas.Keys.Any(string.IsNullOrWhiteSpace))
		{
			failureReason = "InventoryDelta contains an empty item id.";
			return false;
		}

		failureReason = string.Empty;
		return true;
	}

	public Godot.Collections.Dictionary ToDictionary()
	{
		Godot.Collections.Dictionary itemDeltas = new();
		foreach ((string itemId, int amount) in ItemDeltas)
		{
			itemDeltas[itemId] = amount;
		}

		return new Godot.Collections.Dictionary
		{
			["item_deltas"] = itemDeltas,
			["key_item_unlock_ids"] = ToVariantArray(KeyItemUnlockIds),
		};
	}

	public static InventoryDelta FromDictionary(Godot.Collections.Dictionary? dictionary)
	{
		InventoryDelta delta = new();
		if (dictionary == null)
		{
			return delta;
		}

		if (dictionary.TryGetValue("item_deltas", out Variant itemDeltasVariant)
			&& itemDeltasVariant.Obj is Godot.Collections.Dictionary rawItemDeltas)
		{
			foreach (Variant key in rawItemDeltas.Keys)
			{
				string itemId = key.AsString();
				if (!string.IsNullOrWhiteSpace(itemId))
				{
					delta.ItemDeltas[itemId] = rawItemDeltas[key].AsInt32();
				}
			}
		}

		if (dictionary.TryGetValue("key_item_unlock_ids", out Variant keyItemUnlockIds))
		{
			delta.KeyItemUnlockIds = ToStringArray(keyItemUnlockIds);
		}

		return delta;
	}

	private static Godot.Collections.Array<string> ToVariantArray(string[] values)
	{
		Godot.Collections.Array<string> array = new();
		foreach (string value in values.Where(value => !string.IsNullOrWhiteSpace(value)))
		{
			array.Add(value);
		}

		return array;
	}

	private static string[] ToStringArray(Variant value)
	{
		if (value.Obj is not Godot.Collections.Array rawArray)
		{
			return Array.Empty<string>();
		}

		return rawArray
			.Select(item => item.AsString())
			.Where(item => !string.IsNullOrWhiteSpace(item))
			.ToArray();
	}
}
