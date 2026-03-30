using System;
using Godot;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.Equipment;

public sealed class EquipmentService
{
	private readonly EquipmentCatalog _catalog;

	public EquipmentService(EquipmentCatalog catalog)
	{
		_catalog = catalog;
	}

	public bool IsOwned(InventoryRuntimeState inventoryState, string itemId)
	{
		if (inventoryState == null || string.IsNullOrWhiteSpace(itemId))
		{
			return false;
		}

		return inventoryState.ItemCounts.TryGetValue(itemId.Trim(), out Variant amount) && amount.AsInt32() > 0;
	}

	public string GetEquippedItemId(EquipmentLoadoutState loadoutState, string slotId)
	{
		if (loadoutState == null)
		{
			return string.Empty;
		}

		return NormalizeSlotId(slotId) switch
		{
			EquipmentSlotIds.Weapon => loadoutState.WeaponItemId,
			EquipmentSlotIds.Armor => loadoutState.ArmorItemId,
			EquipmentSlotIds.Accessory => loadoutState.AccessoryItemId,
			_ => string.Empty,
		};
	}

	public bool TryEquipItem(EquipmentLoadoutState loadoutState, InventoryRuntimeState inventoryState, string slotId, string itemId, out string failureReason)
	{
		string normalizedSlotId = NormalizeSlotId(slotId);
		if (string.IsNullOrWhiteSpace(normalizedSlotId))
		{
			failureReason = "unknown_slot";
			return false;
		}

		string normalizedItemId = itemId?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(normalizedItemId))
		{
			failureReason = "missing_item";
			return false;
		}

		if (!IsOwned(inventoryState, normalizedItemId))
		{
			failureReason = "item_not_owned";
			return false;
		}

		if (!_catalog.CanEquipInSlot(normalizedItemId, normalizedSlotId))
		{
			failureReason = "slot_mismatch";
			return false;
		}

		SetEquippedItemId(loadoutState, normalizedSlotId, normalizedItemId);
		failureReason = string.Empty;
		return true;
	}

	public void UnequipItem(EquipmentLoadoutState loadoutState, string slotId)
	{
		string normalizedSlotId = NormalizeSlotId(slotId);
		if (string.IsNullOrWhiteSpace(normalizedSlotId))
		{
			return;
		}

		SetEquippedItemId(loadoutState, normalizedSlotId, string.Empty);
	}

	private static string NormalizeSlotId(string? slotId)
	{
		return slotId?.Trim().ToLowerInvariant() switch
		{
			EquipmentSlotIds.Weapon => EquipmentSlotIds.Weapon,
			EquipmentSlotIds.Armor => EquipmentSlotIds.Armor,
			EquipmentSlotIds.Accessory => EquipmentSlotIds.Accessory,
			_ => string.Empty,
		};
	}

	private static void SetEquippedItemId(EquipmentLoadoutState loadoutState, string slotId, string itemId)
	{
		if (loadoutState == null)
		{
			return;
		}

		switch (slotId)
		{
			case EquipmentSlotIds.Weapon:
				loadoutState.WeaponItemId = itemId;
				break;
			case EquipmentSlotIds.Armor:
				loadoutState.ArmorItemId = itemId;
				break;
			case EquipmentSlotIds.Accessory:
				loadoutState.AccessoryItemId = itemId;
				break;
		}
	}
}
