using System;
using Godot;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.Services;

public sealed class SaveGameService
{
	public SaveGameData BuildSaveData(GlobalGameSession session)
	{
		return new SaveGameData
		{
			SessionId = Guid.NewGuid().ToString("N"),
			PlayerSnapshot = session.BuildPlayerSnapshot(),
			CompanionSnapshot = session.BuildCompanionSnapshot(),
			ProgressionSnapshot = session.BuildProgressionSnapshotModel().ToDictionary(),
			DeckBuildSnapshot = session.BuildDeckBuildSnapshotModel().ToDictionary(),
			InventorySnapshot = session.BuildInventorySnapshot(),
			SaveRuntimeSnapshot = session.BuildSaveRuntimeSnapshot(),
		};
	}

	public void ApplySaveData(GlobalGameSession session, SaveGameData saveData)
	{
		session.ApplyPlayerSnapshot(saveData.PlayerSnapshot);
		session.ApplyCompanionSnapshot(saveData.CompanionSnapshot);
		session.ApplyProgressionSnapshot(saveData.ProgressionSnapshot);
		session.ApplyDeckBuildSnapshot(saveData.DeckBuildSnapshot);
		session.ApplyInventorySnapshot(saveData.InventorySnapshot);
		session.ApplySaveRuntimeSnapshot(saveData.SaveRuntimeSnapshot);
	}

	public string SerializeToJson(SaveGameData saveData)
	{
		Godot.Collections.Dictionary root = new()
		{
			["version"] = saveData.Version,
			["session_id"] = saveData.SessionId,
			["player_snapshot"] = saveData.PlayerSnapshot,
			["companion_snapshot"] = saveData.CompanionSnapshot,
			["progression_snapshot"] = saveData.ProgressionSnapshot,
			["deck_build_snapshot"] = saveData.DeckBuildSnapshot,
			["inventory_snapshot"] = saveData.InventorySnapshot,
			["save_runtime_snapshot"] = saveData.SaveRuntimeSnapshot,
		};

		return Json.Stringify(root);
	}

	public bool TryDeserializeFromJson(string json, out SaveGameData? saveData, out string failureReason)
	{
		saveData = null;
		failureReason = string.Empty;

		Json jsonParser = new();
		Error parseError = jsonParser.Parse(json);
		if (parseError != Error.Ok || jsonParser.Data.Obj is not Godot.Collections.Dictionary root)
		{
			failureReason = $"Save json parse failed. error={parseError}";
			return false;
		}

		saveData = new SaveGameData
		{
			Version = root.TryGetValue("version", out Variant version) ? version.AsInt32() : 1,
			SessionId = root.TryGetValue("session_id", out Variant sessionId) ? sessionId.AsString() : string.Empty,
			PlayerSnapshot = root.TryGetValue("player_snapshot", out Variant playerSnapshot) && playerSnapshot.Obj is Godot.Collections.Dictionary rawPlayer ? CloneDictionary(rawPlayer) : new(),
			CompanionSnapshot = root.TryGetValue("companion_snapshot", out Variant companionSnapshot) && companionSnapshot.Obj is Godot.Collections.Dictionary rawCompanion ? CloneDictionary(rawCompanion) : new(),
			ProgressionSnapshot = root.TryGetValue("progression_snapshot", out Variant progressionSnapshot) && progressionSnapshot.Obj is Godot.Collections.Dictionary rawProgression ? CloneDictionary(rawProgression) : new(),
			DeckBuildSnapshot = root.TryGetValue("deck_build_snapshot", out Variant deckBuildSnapshot) && deckBuildSnapshot.Obj is Godot.Collections.Dictionary rawDeck ? CloneDictionary(rawDeck) : new(),
			InventorySnapshot = root.TryGetValue("inventory_snapshot", out Variant inventorySnapshot) && inventorySnapshot.Obj is Godot.Collections.Dictionary rawInventory ? CloneDictionary(rawInventory) : new(),
			SaveRuntimeSnapshot = root.TryGetValue("save_runtime_snapshot", out Variant saveRuntimeSnapshot) && saveRuntimeSnapshot.Obj is Godot.Collections.Dictionary rawSaveRuntime ? CloneDictionary(rawSaveRuntime) : new(),
		};
		return true;
	}

	private static Godot.Collections.Dictionary CloneDictionary(Godot.Collections.Dictionary source)
	{
		Godot.Collections.Dictionary clone = new();
		foreach (Variant key in source.Keys)
		{
			clone[key] = source[key];
		}

		return clone;
	}
}
