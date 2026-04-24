using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Equipment;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Cards;
using CardChessDemo.Battle.Progression;
using CardChessDemo.Battle.Services;
using CardChessDemo.Battle.Stats;
using CardChessDemo.Map;

namespace CardChessDemo.Battle.Shared;

public partial class GlobalGameSession : Node
{
	public const string PrimarySaveSlotId = "slot_01";
	private const string PrimarySaveFilePath = "user://saves/slot_01.json";
	private const string DefaultNewGameScenePath = "res://Scene/Maps/Scene01.tscn";

	[Signal] public delegate void PlayerRuntimeChangedEventHandler();
	[Signal] public delegate void ArakawaRuntimeChangedEventHandler();

	[Export] public string PlayerDisplayName { get; set; } = "Traveler";
	[Export] public int PlayerMaxHp { get; set; } = 12;
	[Export] public int PlayerCurrentHp { get; set; } = 12;
	[Export] public int PlayerMovePointsPerTurn { get; set; } = 4;
	[Export] public int PlayerAttackRange { get; set; } = 1;
	[Export] public int PlayerAttackDamage { get; set; } = 2;
	[Export] public int PlayerDefenseDamageReductionPercent { get; set; } = 50;
	[Export] public int PlayerDefenseShieldGain { get; set; } = 0;
	[Export] public int ArakawaMaxEnergy { get; set; } = 3;
	[Export] public int ArakawaCurrentEnergy { get; set; } = 3;
	[Export] public int PlayerLevel { get; set; } = 1;
	[Export] public int PlayerExperience { get; set; } = 0;
	[Export] public int PlayerMasteryPoints { get; set; } = 2;
	[Export] public int ArakawaGrowthLevel { get; set; } = 1;
	[Export] public string[] TalentIds { get; set; } = Array.Empty<string>();
	[Export] public string[] ArakawaUnlockIds { get; set; } = Array.Empty<string>();
	[Export] public string[] UnlockedCardIds { get; set; } = Array.Empty<string>();
	[Export] public string[] TalentBranchTags { get; set; } = Array.Empty<string>();
	[Export] public int DeckPointBudgetBonus { get; set; } = 0;
	[Export] public int DeckMinCardCountDelta { get; set; } = 0;
	[Export] public int DeckMaxCardCountDelta { get; set; } = 0;
	[Export] public int DeckMaxCopiesPerCardBonus { get; set; } = 0;
	[Export] public string DeckBuildName { get; set; } = "default";
	[Export] public string[] DeckCardIds { get; set; } = Array.Empty<string>();
	[Export] public string[] DeckRelicIds { get; set; } = Array.Empty<string>();
	[Export] public int DeckBuildRevision { get; set; } = 0;
	[Export] public string EquippedWeaponItemId { get; set; } = string.Empty;
	[Export] public string EquippedArmorItemId { get; set; } = string.Empty;
	[Export] public string EquippedAccessoryItemId { get; set; } = string.Empty;
	[Export] public string[] InventoryKeyItemIds { get; set; } = Array.Empty<string>();
	[Export] public string LastCheckpointSaveId { get; set; } = string.Empty;
	[Export] public string LastManualSaveId { get; set; } = string.Empty;
	[Export] public string AutoSaveSlotId { get; set; } = "autosave";
	[Export] public string LastCheckpointScenePath { get; set; } = string.Empty;
	[Export] public string LastCheckpointMapId { get; set; } = string.Empty;
	[Export] public string LastCheckpointSpawnId { get; set; } = string.Empty;
	[Export] public string LastAutoSaveTimestampUtc { get; set; } = string.Empty;
	[Export] public string SessionId { get; set; } = string.Empty;
	[Export] public string CurrentMapId { get; set; } = "scene1";
	[Export] public string CurrentMapSpawnId { get; set; } = string.Empty;
	[Export] public string PlayerProfileId { get; set; } = "default_player";
	[Export] public int ScanRisk { get; set; } = 0;
	[Export] public bool ShouldRestorePlayerPosition { get; set; } = false;
	[Export] public Vector2 PendingRestorePlayerPosition { get; set; } = Vector2.Zero;
	[Export] public string PendingSceneTransferScenePath { get; set; } = string.Empty;
	[Export] public string PendingSceneTransferSpawnId { get; set; } = string.Empty;

	public BattleRequest? PendingBattleRequest { get; private set; }
	public BattleResult? LastBattleResult { get; private set; }
	public MapResumeContext? PendingBattleReturnContext { get; private set; }
	public string PendingBattleEncounterId { get; private set; } = string.Empty;
	public string PendingSceneRuntimeScenePath { get; private set; } = string.Empty;
	public Godot.Collections.Dictionary PendingSceneRuntimeSnapshot { get; private set; } = new();
	public PartyRuntimeState PartyState { get; } = new();
	public ProgressionRuntimeState ProgressionState { get; } = new();
	public DeckBuildState DeckBuildState { get; } = new();
	public InventoryRuntimeState InventoryState { get; } = new();
	public EquipmentLoadoutState EquipmentLoadoutState { get; } = new();
	public SaveRuntimeState SaveState { get; } = new();
	public EquipmentCatalog RuntimeEquipmentCatalog { get; } = EquipmentCatalog.CreateFromConfiguredResources();
	public ProgressionRuleSet RuntimeProgressionRuleSet { get; } = ProgressionRuleSet.CreateFromConfiguredRules();
	public Godot.Collections.Dictionary InventoryItemCounts => InventoryState.ItemCounts;
	public Godot.Collections.Array<string> InventoryKeyItems => InventoryState.KeyItemIds;
	public Godot.Collections.Dictionary<StringName, Variant> WorldFlags { get; } = new();
	public Godot.Collections.Array<StringName> ClearedEncounters { get; } = new();
	public Godot.Collections.Array<StringName> UsedInteractables { get; } = new();

	private EquipmentService _equipmentService = null!;
	private PlayerStatResolver _playerStatResolver = null!;
	private readonly List<string> _pendingRetreatCleanupInteractablePaths = new();
	private readonly SaveGameService _saveGameService = new();
	private Godot.Collections.Dictionary _defaultPlayerSnapshot = new();
	private Godot.Collections.Dictionary _defaultCompanionSnapshot = new();
	private Godot.Collections.Dictionary _defaultProgressionSnapshot = new();
	private Godot.Collections.Dictionary _defaultDeckBuildSnapshot = new();
	private Godot.Collections.Dictionary _defaultInventorySnapshot = new();
	private Godot.Collections.Dictionary _defaultSaveRuntimeSnapshot = new();
	private Godot.Collections.Dictionary _defaultMapRuntimeSnapshot = new();

	public override void _Ready()
	{
		if (string.IsNullOrWhiteSpace(SessionId))
		{
			SessionId = Guid.NewGuid().ToString("N");
		}

		EnsureCompositionServices();
		SyncCompositeStateFromFields();
		CaptureDefaultSnapshots();
	}

	public bool HasPrimarySave()
	{
		return FileAccess.FileExists(PrimarySaveFilePath);
	}

	public bool DeletePrimarySave(out string failureReason)
	{
		failureReason = string.Empty;
		if (!HasPrimarySave())
		{
			return true;
		}

		Error error = DirAccess.RemoveAbsolute(PrimarySaveFilePath);
		if (error != Error.Ok)
		{
			failureReason = $"Delete save failed. error={error}, path='{PrimarySaveFilePath}'";
			return false;
		}

		return true;
	}

	public bool TrySavePrimaryCheckpoint(string scenePath, Vector2 checkpointPosition, Godot.Collections.Dictionary? sceneRuntimeSnapshot, out string failureReason)
	{
		failureReason = string.Empty;
		string resolvedScenePath = string.IsNullOrWhiteSpace(scenePath) ? DefaultNewGameScenePath : scenePath.Trim();

		SaveState.LastCheckpointSaveId = PrimarySaveSlotId;
		SaveState.LastManualSaveId = PrimarySaveSlotId;
		SaveState.LastCheckpointScenePath = resolvedScenePath;
		SaveState.LastCheckpointMapId = resolvedScenePath;
		SaveState.LastCheckpointSpawnId = string.Empty;
		SaveState.LastAutoSaveTimestampUtc = DateTime.UtcNow.ToString("O");
		SaveState.PreferredRollbackSlotKind = SaveSlotKind.Checkpoint;
		SyncFieldsFromCompositeState();

		SaveGameData saveData = _saveGameService.BuildSaveData(this);
		saveData.MapRuntimeSnapshot = CloneDictionary(BuildMapRuntimeSnapshot());
		saveData.MapRuntimeSnapshot["should_restore_player_position"] = true;
		saveData.MapRuntimeSnapshot["pending_restore_player_position"] = checkpointPosition;
		saveData.SceneRuntimeScenePath = resolvedScenePath;
		saveData.SceneRuntimeSnapshot = CloneDictionary(sceneRuntimeSnapshot);

		string saveDirectory = PrimarySaveFilePath.GetBaseDir();
		if (!string.IsNullOrWhiteSpace(saveDirectory))
		{
			DirAccess.MakeDirRecursiveAbsolute(saveDirectory);
		}

		FileAccess? file = FileAccess.Open(PrimarySaveFilePath, FileAccess.ModeFlags.Write);
		if (file == null)
		{
			failureReason = $"Open save file failed. error={FileAccess.GetOpenError()}, path='{PrimarySaveFilePath}'";
			return false;
		}

		file.StoreString(_saveGameService.SerializeToJson(saveData));
		file.Flush();
		file.Close();
		return true;
	}

	public bool TryLoadPrimarySave(out string scenePath, out string failureReason)
	{
		scenePath = DefaultNewGameScenePath;
		failureReason = string.Empty;
		if (!HasPrimarySave())
		{
			failureReason = "Primary save file was not found.";
			return false;
		}

		FileAccess? file = FileAccess.Open(PrimarySaveFilePath, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			failureReason = $"Open save file failed. error={FileAccess.GetOpenError()}, path='{PrimarySaveFilePath}'";
			return false;
		}

		string json = file.GetAsText();
		file.Close();
		if (!_saveGameService.TryDeserializeFromJson(json, out SaveGameData? saveData, out failureReason) || saveData == null)
		{
			return false;
		}

		ResetToNewGameDefaults();
		_saveGameService.ApplySaveData(this, saveData);
		SetPendingSceneRuntimeSnapshot(saveData.SceneRuntimeScenePath, saveData.SceneRuntimeSnapshot);
		scenePath = string.IsNullOrWhiteSpace(SaveState.LastCheckpointScenePath)
			? DefaultNewGameScenePath
			: SaveState.LastCheckpointScenePath;
		return true;
	}

	public void ResetToNewGameDefaults()
	{
		CancelPendingBattleTransition();
		ClearPendingBattleReturnContext();
		ConsumeLastBattleResult();
		ClearPendingRestorePlayerPosition();
		ClearPendingSceneRuntimeSnapshot();
		PendingSceneTransferScenePath = string.Empty;
		PendingSceneTransferSpawnId = string.Empty;

		ApplyPlayerSnapshot(CloneDictionary(_defaultPlayerSnapshot));
		ApplyCompanionSnapshot(CloneDictionary(_defaultCompanionSnapshot));
		ApplyProgressionSnapshot(CloneDictionary(_defaultProgressionSnapshot));
		ApplyDeckBuildSnapshot(CloneDictionary(_defaultDeckBuildSnapshot));
		ApplyInventorySnapshot(CloneDictionary(_defaultInventorySnapshot));
		ApplySaveRuntimeSnapshot(CloneDictionary(_defaultSaveRuntimeSnapshot));
		ApplyMapRuntimeSnapshot(CloneDictionary(_defaultMapRuntimeSnapshot));

		SessionId = Guid.NewGuid().ToString("N");
		LastBattleResult = null;
	}

	private void CaptureDefaultSnapshots()
	{
		_defaultPlayerSnapshot = CloneDictionary(BuildPlayerSnapshot());
		_defaultCompanionSnapshot = CloneDictionary(BuildCompanionSnapshot());
		_defaultProgressionSnapshot = CloneDictionary(BuildProgressionSnapshot());
		_defaultDeckBuildSnapshot = CloneDictionary(BuildDeckBuildSnapshot());
		_defaultInventorySnapshot = CloneDictionary(BuildInventorySnapshot());
		_defaultSaveRuntimeSnapshot = CloneDictionary(BuildSaveRuntimeSnapshot());
		_defaultMapRuntimeSnapshot = CloneDictionary(BuildMapRuntimeSnapshot());
	}

	public void SetPendingSceneRuntimeSnapshot(string scenePath, Godot.Collections.Dictionary? snapshot)
	{
		PendingSceneRuntimeScenePath = scenePath?.Trim() ?? string.Empty;
		PendingSceneRuntimeSnapshot = CloneDictionary(snapshot);
	}

	public bool TryConsumePendingSceneRuntimeSnapshot(string currentScenePath, out Godot.Collections.Dictionary snapshot)
	{
		snapshot = new Godot.Collections.Dictionary();
		if (string.IsNullOrWhiteSpace(PendingSceneRuntimeScenePath)
			|| !string.Equals(PendingSceneRuntimeScenePath, currentScenePath ?? string.Empty, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		snapshot = CloneDictionary(PendingSceneRuntimeSnapshot);
		ClearPendingSceneRuntimeSnapshot();
		return true;
	}

	public void ClearPendingSceneRuntimeSnapshot()
	{
		PendingSceneRuntimeScenePath = string.Empty;
		PendingSceneRuntimeSnapshot = new Godot.Collections.Dictionary();
	}

	public void SetCurrentMapContext(string mapId, string spawnId = "", string playerProfileId = "")
	{
		if (!string.IsNullOrWhiteSpace(mapId))
		{
			CurrentMapId = mapId.Trim();
		}

		CurrentMapSpawnId = spawnId?.Trim() ?? string.Empty;
		if (!string.IsNullOrWhiteSpace(playerProfileId))
		{
			PlayerProfileId = playerProfileId.Trim();
		}
	}

	public void SetFlag(StringName key, Variant value)
	{
		WorldFlags[key] = value;
	}

	public bool TryGetFlag(StringName key, out Variant value)
	{
		if (WorldFlags.TryGetValue(key, out value))
		{
			return true;
		}

		value = default;
		return false;
	}

	public void MarkEncounterCleared(StringName encounterId)
	{
		if (!ClearedEncounters.Contains(encounterId))
		{
			ClearedEncounters.Add(encounterId);
		}
	}

	public void MarkInteractableUsed(StringName interactableId)
	{
		if (!UsedInteractables.Contains(interactableId))
		{
			UsedInteractables.Add(interactableId);
		}
	}

	public void SetPendingRestorePlayerPosition(Vector2 position)
	{
		ShouldRestorePlayerPosition = true;
		PendingRestorePlayerPosition = position;
	}

	public void ClearPendingRestorePlayerPosition()
	{
		ShouldRestorePlayerPosition = false;
		PendingRestorePlayerPosition = Vector2.Zero;
	}

	public void SetPendingSceneTransfer(string scenePath, string spawnId = "")
	{
		PendingSceneTransferScenePath = scenePath?.Trim() ?? string.Empty;
		PendingSceneTransferSpawnId = spawnId?.Trim() ?? string.Empty;
	}

	public bool TryConsumePendingSceneTransfer(string currentScenePath, out string spawnId)
	{
		spawnId = string.Empty;
		if (string.IsNullOrWhiteSpace(PendingSceneTransferScenePath)
			|| !string.Equals(PendingSceneTransferScenePath, currentScenePath ?? string.Empty, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		spawnId = PendingSceneTransferSpawnId;
		PendingSceneTransferScenePath = string.Empty;
		PendingSceneTransferSpawnId = string.Empty;
		return true;
	}

	public void ApplyResourceDelta(StringName resourceKey, int delta, int? clampMin = null, int? clampMax = null)
	{
		int value;
		switch (resourceKey.ToString())
		{
			case "player_hp":
				value = PartyState.Player.CurrentHp + delta;
				SetPlayerCurrentHp(ClampOptional(value, clampMin, clampMax));
				break;
			case "arakawa_energy":
				value = PartyState.Arakawa.CurrentEnergy + delta;
				SetArakawaCurrentEnergy(ClampOptional(value, clampMin, clampMax));
				break;
			case "scan_risk":
				ScanRisk = ClampOptional(ScanRisk + delta, clampMin, clampMax);
				break;
			default:
				GD.PushWarning($"GlobalGameSession.ApplyResourceDelta: unknown resource key '{resourceKey}'.");
				break;
		}
	}

	public void SetPlayerCurrentHp(int value)
	{
		PartyState.Player.CurrentHp = Mathf.Clamp(value, 0, GetResolvedPlayerMaxHp());
		SyncFieldsFromCompositeState();
		EmitSignal(SignalName.PlayerRuntimeChanged);
	}

	public void SetPlayerMovePointsPerTurn(int value)
	{
		PartyState.Player.MovePointsPerTurn = Mathf.Max(0, value);
		SyncFieldsFromCompositeState();
		EmitSignal(SignalName.PlayerRuntimeChanged);
	}

	public void SetArakawaCurrentEnergy(int value)
	{
		PartyState.Arakawa.CurrentEnergy = Mathf.Clamp(value, 0, Math.Max(PartyState.Arakawa.MaxEnergy, 0));
		SyncFieldsFromCompositeState();
		EmitSignal(SignalName.ArakawaRuntimeChanged);
	}

	public bool TrySpendArakawaEnergy(int amount)
	{
		if (amount <= 0)
		{
			return true;
		}

		if (PartyState.Arakawa.CurrentEnergy < amount)
		{
			return false;
		}

		SetArakawaCurrentEnergy(PartyState.Arakawa.CurrentEnergy - amount);
		return true;
	}

	public void RestoreArakawaEnergy(int amount)
	{
		if (amount <= 0)
		{
			return;
		}

		SetArakawaCurrentEnergy(ArakawaCurrentEnergy + amount);
	}

	public bool TryUseInventoryItem(string itemId, out string resultMessage)
	{
		resultMessage = string.Empty;
		if (string.IsNullOrWhiteSpace(itemId))
		{
			resultMessage = "\u672A\u9009\u62E9\u7269\u54C1";
			return false;
		}

		if (!InventoryItemCounts.TryGetValue(itemId, out Variant amountValue) || amountValue.AsInt32() <= 0)
		{
			resultMessage = "\u8BE5\u7269\u54C1\u4E0D\u5728\u80CC\u5305\u4E2D";
			return false;
		}

		if (string.Equals(itemId, "arakawa_battery", StringComparison.Ordinal))
		{
			if (ArakawaCurrentEnergy >= ArakawaMaxEnergy)
			{
				resultMessage = "\u8352\u5DDD\u80FD\u91CF\u5DF2\u6EE1\uff0C\u4E0D\u53EF\u4F7F\u7528";
				return false;
			}

			RestoreArakawaEnergy(1);
			int nextAmount = Math.Max(0, amountValue.AsInt32() - 1);
			if (nextAmount <= 0)
			{
				InventoryItemCounts.Remove(itemId);
			}
			else
			{
				InventoryItemCounts[itemId] = nextAmount;
			}

			resultMessage = "\u8352\u5DDD\u80FD\u91CF +1";
			return true;
		}

		resultMessage = "\u5F53\u524D\u6682\u65E0\u53EF\u4F7F\u7528\u6548\u679C";
		return false;
	}

	public void ApplyMovePointDelta(int delta)
	{
		SetPlayerMovePointsPerTurn(PlayerMovePointsPerTurn + delta);
	}

	public int GetResolvedPlayerAttackDamage()
	{
		return ResolvePlayerStats().AttackDamage;
	}

	public int GetResolvedPlayerAttackRange()
	{
		return ResolvePlayerStats().AttackRange;
	}

	public int GetResolvedPlayerDefenseDamageReductionPercent()
	{
		return ResolvePlayerStats().DefenseDamageReductionPercent;
	}

	public int GetResolvedPlayerDefenseShieldGain()
	{
		return ResolvePlayerStats().DefenseShieldGain;
	}

	public int GetResolvedPlayerMaxHp()
	{
		return ResolvePlayerStats().MaxHp;
	}

	public int GetResolvedPlayerMovePointsPerTurn()
	{
		return ResolvePlayerStats().MovePointsPerTurn;
	}

	public ResolvedPlayerStats ResolvePlayerStats(string? weaponOverrideItemId = null)
	{
		EnsureCompositionServices();
		return _playerStatResolver.Resolve(
			PartyState.Player,
			ProgressionState,
			EquipmentLoadoutState,
			PlayerDefenseDamageReductionPercent,
			PlayerDefenseShieldGain,
			weaponOverrideItemId);
	}

	public bool IsEquipmentOwned(string itemId)
	{
		EnsureCompositionServices();
		return _equipmentService.IsOwned(InventoryState, itemId);
	}

	public string GetEquippedItemId(string slotId)
	{
		EnsureCompositionServices();
		return _equipmentService.GetEquippedItemId(EquipmentLoadoutState, slotId);
	}

	public bool TryEquipItem(string slotId, string itemId, out string failureReason)
	{
		EnsureCompositionServices();
		bool equipped = _equipmentService.TryEquipItem(EquipmentLoadoutState, InventoryState, slotId, itemId, out failureReason);
		if (equipped)
		{
			SyncFieldsFromCompositeState();
			EmitSignal(SignalName.PlayerRuntimeChanged);
		}

		return equipped;
	}

	public void UnequipItem(string slotId)
	{
		EnsureCompositionServices();
		_equipmentService.UnequipItem(EquipmentLoadoutState, slotId);
		SyncFieldsFromCompositeState();
		EmitSignal(SignalName.PlayerRuntimeChanged);
	}

	public EquipmentDefinition? FindEquipmentDefinition(string itemId)
	{
		EnsureCompositionServices();
		return RuntimeEquipmentCatalog.FindDefinition(itemId);
	}

	public EquipmentDefinition[] GetEquipmentDefinitionsForSlot(string slotId)
	{
		EnsureCompositionServices();
		return RuntimeEquipmentCatalog.GetDefinitionsForSlot(slotId);
	}

	public int GetExperienceRequiredForNextLevel()
	{
		return RuntimeProgressionRuleSet.GetExperienceRequirementForLevel(PlayerLevel);
	}

	public int GetExperienceProgressWithinLevel()
	{
		int currentLevelFloor = RuntimeProgressionRuleSet.GetAccumulatedExperienceForLevel(PlayerLevel);
		return Math.Max(0, PlayerExperience - currentLevelFloor);
	}

	public int GetExperienceNeededToLevelUp()
	{
		int target = RuntimeProgressionRuleSet.GetAccumulatedExperienceForLevel(PlayerLevel + 1);
		return Math.Max(0, target - PlayerExperience);
	}

	public int GetSoftLevelCap()
	{
		return RuntimeProgressionRuleSet.SoftLevelCap;
	}

	public bool IsAtOrPastSoftLevelCap()
	{
		return PlayerLevel >= RuntimeProgressionRuleSet.SoftLevelCap;
	}

	public void BeginBattle(BattleRequest? request = null)
	{
		BattleRequest resolvedRequest = request ?? BattleRequest.FromSession(this);
		if (!resolvedRequest.TryValidate(out string failureReason))
		{
			GD.PushError($"GlobalGameSession.BeginBattle: invalid request. {failureReason}");
			return;
		}

		PendingBattleRequest = resolvedRequest;
		LastBattleResult = null;
	}

	public void EnsureDeckBuildInitialized(BattleCardLibrary? cardLibrary)
	{
		string[] starterDeck = cardLibrary?.BuildStarterDeckCardIds() ?? Array.Empty<string>();
		if (starterDeck.Length == 0 && DeckBuildState.CardIds.Length == 0)
		{
			return;
		}

		if (DeckBuildState.CardIds.Length == 0)
		{
			DeckBuildState.CardIds = starterDeck;
		}
		else if (LooksLikeLegacyStarterDeckWithRam(DeckBuildState.CardIds, starterDeck))
		{
			DeckBuildState.CardIds = DeckBuildState.CardIds
				.Where(cardId => !string.Equals(cardId, "card_ram", StringComparison.Ordinal))
				.ToArray();
		}

		SyncFieldsFromCompositeState();
	}

	private static bool LooksLikeLegacyStarterDeckWithRam(string[] currentDeck, string[] starterDeck)
	{
		if (currentDeck == null || starterDeck == null || currentDeck.Length <= starterDeck.Length || !currentDeck.Contains("card_ram", StringComparer.Ordinal))
		{
			return false;
		}

		HashSet<string> allowedIds = starterDeck
			.Concat(new[] { "card_ram" })
			.ToHashSet(StringComparer.Ordinal);

		return currentDeck.All(cardId => allowedIds.Contains(cardId));
	}

	public void CancelPendingBattleTransition()
	{
		PendingBattleRequest = null;
		PendingBattleEncounterId = string.Empty;
		PendingBattleReturnContext = null;
		ClearPendingRetreatCleanupInteractables();
	}

	public void SetPendingRetreatCleanupInteractables(IEnumerable<string> interactablePaths)
	{
		_pendingRetreatCleanupInteractablePaths.Clear();
		foreach (string path in interactablePaths)
		{
			string trimmed = path?.Trim() ?? string.Empty;
			if (string.IsNullOrWhiteSpace(trimmed) || _pendingRetreatCleanupInteractablePaths.Any(existing => string.Equals(existing, trimmed, StringComparison.Ordinal)))
			{
				continue;
			}

			_pendingRetreatCleanupInteractablePaths.Add(trimmed);
		}
	}

	public void ClearPendingRetreatCleanupInteractables()
	{
		_pendingRetreatCleanupInteractablePaths.Clear();
	}

	public void SetPendingBattleReturnContext(MapResumeContext? resumeContext)
	{
		PendingBattleReturnContext = resumeContext;
	}

	public MapResumeContext? PeekPendingBattleReturnContext()
	{
		return PendingBattleReturnContext;
	}

	public void ClearPendingBattleReturnContext()
	{
		PendingBattleReturnContext = null;
	}

	public bool TryConsumePendingBattleReturn(string currentScenePath, out MapResumeContext? resumeContext)
	{
		resumeContext = null;
		if (PendingBattleReturnContext == null
			|| string.IsNullOrWhiteSpace(PendingBattleReturnContext.ScenePath)
			|| !string.Equals(PendingBattleReturnContext.ScenePath, currentScenePath ?? string.Empty, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		resumeContext = PendingBattleReturnContext;
		PendingBattleReturnContext = null;
		return true;
	}

	public void SetPendingBattleEncounterId(string encounterId)
	{
		PendingBattleEncounterId = encounterId?.Trim() ?? string.Empty;
	}

	public string ConsumePendingBattleEncounterId()
	{
		string encounterId = PendingBattleEncounterId;
		PendingBattleEncounterId = string.Empty;
		return encounterId;
	}

	public BattleRequest? ConsumePendingBattleRequest()
	{
		BattleRequest? request = PendingBattleRequest;
		PendingBattleRequest = null;
		return request;
	}

	public void CompleteBattle(BattleResult result)
	{
		if (!result.TryValidate(out string failureReason))
		{
			GD.PushError($"GlobalGameSession.CompleteBattle: invalid result. {failureReason}");
			return;
		}

		string roomLayoutId = result.RuntimeFlags.TryGetValue("room_layout_id", out Variant roomLayoutVariant)
			? roomLayoutVariant.AsString()
			: string.Empty;
		BattleResolutionService resolutionService = new();
		BattleResolutionPlan resolutionPlan = resolutionService.Resolve(this, result, roomLayoutId);
		Godot.Collections.Dictionary mergedRuntimeFlags = CloneDictionary(result.RuntimeFlags);
		foreach (Variant key in resolutionPlan.RewardBundle.RuntimeFlags.Keys)
		{
			mergedRuntimeFlags[key] = resolutionPlan.RewardBundle.RuntimeFlags[key];
		}

		ApplyEncounteredEnemyCodexFlags(mergedRuntimeFlags);

		if (resolutionPlan.ShouldClearEncounter)
		{
			mergedRuntimeFlags["cleared_encounter_id"] = resolutionPlan.ClearedEncounterId;
			MarkEncounterCleared(new StringName(resolutionPlan.ClearedEncounterId));
		}

		if (PendingBattleReturnContext != null)
		{
			PendingBattleReturnContext = PendingBattleReturnContext.WithReturnReason(ResolveMapReturnReason(result.Outcome));
		}

		if (result.Outcome == BattleOutcome.Victory && PendingBattleReturnContext != null)
		{
			ApplyBattleVictoryToPendingMapResume(PendingBattleReturnContext);
		}
		else if (result.Outcome == BattleOutcome.Retreat && PendingBattleReturnContext != null)
		{
			ApplyPendingRetreatCleanupToMapResume(PendingBattleReturnContext);
		}

		BattleResult resolvedResult = new(
			requestId: result.RequestId,
			encounterId: result.EncounterId,
			outcome: result.Outcome,
			playerSnapshot: result.PlayerSnapshot,
			companionSnapshot: result.CompanionSnapshot,
			progressionDelta: resolutionPlan.RewardBundle.ProgressionDelta.ToDictionary(),
			inventoryDelta: resolutionPlan.RewardBundle.InventoryDelta.ToDictionary(),
			rewardEntries: resolutionPlan.RewardBundle.RewardEntries.Select(BattleRewardEntry.FromDictionary),
			clearedEncounterId: resolutionPlan.ShouldClearEncounter ? resolutionPlan.ClearedEncounterId : result.ClearedEncounterId,
			runtimeFlags: mergedRuntimeFlags);

		LastBattleResult = resolvedResult;
		resolvedResult.ApplyToSession(this);
		ClearPendingRetreatCleanupInteractables();
	}

	private void ApplyEncounteredEnemyCodexFlags(Godot.Collections.Dictionary runtimeFlags)
	{
		if (!runtimeFlags.TryGetValue("encountered_enemy_definition_ids", out Variant encounteredIdsVariant)
			|| encounteredIdsVariant.Obj is not Godot.Collections.Array encounteredIds)
		{
			return;
		}

		foreach (Variant encounteredIdVariant in encounteredIds)
		{
			string encounteredId = encounteredIdVariant.AsString();
			if (string.IsNullOrWhiteSpace(encounteredId))
			{
				continue;
			}

			SetFlag(new StringName($"enemy_encountered:{encounteredId}"), true);
		}
	}

	public BattleResult? PeekLastBattleResult()
	{
		return LastBattleResult;
	}

	public BattleResult? ConsumeLastBattleResult()
	{
		BattleResult? result = LastBattleResult;
		LastBattleResult = null;
		return result;
	}

	private void ApplyBattleVictoryToPendingMapResume(MapResumeContext resumeContext)
	{
		if (!string.IsNullOrWhiteSpace(resumeContext.EncounterId))
		{
			MarkEncounterCleared(new StringName(resumeContext.EncounterId));
		}

		if (string.IsNullOrWhiteSpace(resumeContext.SourceInteractablePath))
		{
			return;
		}

		ApplyInteractableRemovalToPendingMapResume(resumeContext, resumeContext.SourceInteractablePath);
	}

	private void ApplyPendingRetreatCleanupToMapResume(MapResumeContext resumeContext)
	{
		foreach (string interactablePath in _pendingRetreatCleanupInteractablePaths)
		{
			ApplyInteractableRemovalToPendingMapResume(resumeContext, interactablePath);
		}
	}

	private void ApplyInteractableRemovalToPendingMapResume(MapResumeContext resumeContext, string interactablePath)
	{
		if (string.IsNullOrWhiteSpace(interactablePath))
		{
			return;
		}

		Godot.Collections.Dictionary snapshot = resumeContext.MapRuntimeSnapshot;
		string snapshotKey = MapRuntimeSnapshotHelper.ResolveSnapshotKey(snapshot, interactablePath);
		if (string.IsNullOrWhiteSpace(snapshotKey))
		{
			snapshotKey = interactablePath;
		}

		if (!snapshot.TryGetValue(snapshotKey, out Variant sourceSnapshotVariant)
			|| sourceSnapshotVariant.Obj is not Godot.Collections.Dictionary interactableSnapshot)
		{
			interactableSnapshot = new Godot.Collections.Dictionary();
			snapshot[snapshotKey] = interactableSnapshot;
		}

		bool markUsedOnVictory = !interactableSnapshot.TryGetValue("mark_used_on_battle_victory", out Variant markUsedVariant)
			|| markUsedVariant.AsBool();
		bool removeOnVictory = !interactableSnapshot.TryGetValue("remove_on_battle_victory", out Variant removeOnVictoryVariant)
			|| removeOnVictoryVariant.AsBool();

		if (markUsedOnVictory)
		{
			MarkInteractableUsed(new StringName(interactablePath));
		}

		interactableSnapshot["is_disabled"] = true;
		if (removeOnVictory)
		{
			interactableSnapshot["remove_from_scene"] = true;
		}
	}

	private static MapReturnReason ResolveMapReturnReason(BattleOutcome outcome)
	{
		return outcome switch
		{
			BattleOutcome.Victory => MapReturnReason.BattleVictory,
			BattleOutcome.Retreat => MapReturnReason.BattleRetreat,
			_ => MapReturnReason.PendingBattle,
		};
	}

	public Godot.Collections.Dictionary BuildPlayerSnapshot()
	{
		return new Godot.Collections.Dictionary
		{
			["display_name"] = PartyState.Player.DisplayName,
			["base_max_hp"] = PartyState.Player.MaxHp,
			["max_hp"] = GetResolvedPlayerMaxHp(),
			["current_hp"] = PartyState.Player.CurrentHp,
			["base_move_points_per_turn"] = PartyState.Player.MovePointsPerTurn,
			["move_points_per_turn"] = GetResolvedPlayerMovePointsPerTurn(),
			["attack_range"] = PartyState.Player.AttackRange,
			["base_attack_damage"] = PartyState.Player.AttackDamage,
			["attack_damage"] = GetResolvedPlayerAttackDamage(),
			["base_defense_damage_reduction_percent"] = PlayerDefenseDamageReductionPercent,
			["base_defense_shield_gain"] = PlayerDefenseShieldGain,
			["arakawa_max_energy"] = PartyState.Arakawa.MaxEnergy,
			["arakawa_current_energy"] = PartyState.Arakawa.CurrentEnergy,
		};
	}

	public Godot.Collections.Dictionary BuildCompanionSnapshot()
	{
		return new Godot.Collections.Dictionary
		{
			["companion_id"] = PartyState.Arakawa.CompanionId,
			["display_name"] = PartyState.Arakawa.DisplayName,
			["growth_level"] = PartyState.Arakawa.GrowthLevel,
			["arakawa_max_energy"] = PartyState.Arakawa.MaxEnergy,
			["arakawa_current_energy"] = PartyState.Arakawa.CurrentEnergy,
		};
	}

	public Godot.Collections.Dictionary BuildProgressionSnapshot()
	{
		return BuildProgressionSnapshotModel().ToDictionary();
	}

	public ProgressionSnapshot BuildProgressionSnapshotModel()
	{
		return new ProgressionSnapshot
		{
			PlayerLevel = ProgressionState.PlayerLevel,
			PlayerExperience = ProgressionState.PlayerExperience,
			PlayerMasteryPoints = ProgressionState.PlayerMasteryPoints,
			ArakawaGrowthLevel = ProgressionState.ArakawaGrowthLevel,
			TalentIds = ProgressionState.TalentIds,
			ArakawaUnlockIds = ProgressionState.ArakawaUnlockIds,
			UnlockedCardIds = ProgressionState.UnlockedCardIds,
			TalentBranchTags = ProgressionState.TalentBranchTags,
			DeckPointBudgetBonus = ProgressionState.DeckPointBudgetBonus,
			DeckMinCardCountDelta = ProgressionState.DeckMinCardCountDelta,
			DeckMaxCardCountDelta = ProgressionState.DeckMaxCardCountDelta,
			DeckMaxCopiesPerCardBonus = ProgressionState.DeckMaxCopiesPerCardBonus,
		};
	}

	public Godot.Collections.Dictionary BuildDeckBuildSnapshot()
	{
		return BuildDeckBuildSnapshotModel().ToDictionary();
	}

	public DeckBuildSnapshot BuildDeckBuildSnapshotModel()
	{
		return new DeckBuildSnapshot
		{
			BuildName = DeckBuildState.BuildName,
			CardIds = DeckBuildState.CardIds,
			RelicIds = DeckBuildState.RelicIds,
			Revision = DeckBuildState.Revision,
		};
	}

	public Godot.Collections.Dictionary BuildInventorySnapshot()
	{
		return new Godot.Collections.Dictionary
		{
			["items"] = CloneDictionary(InventoryItemCounts),
			["key_items"] = CloneStringArray(InventoryKeyItems),
		};
	}

	public Godot.Collections.Dictionary BuildSaveRuntimeSnapshot()
	{
		return new Godot.Collections.Dictionary
		{
			["last_checkpoint_save_id"] = SaveState.LastCheckpointSaveId,
			["last_manual_save_id"] = SaveState.LastManualSaveId,
			["auto_save_slot_id"] = SaveState.AutoSaveSlotId,
			["last_checkpoint_scene_path"] = SaveState.LastCheckpointScenePath,
			["last_checkpoint_map_id"] = SaveState.LastCheckpointMapId,
			["last_checkpoint_spawn_id"] = SaveState.LastCheckpointSpawnId,
			["last_auto_save_timestamp_utc"] = SaveState.LastAutoSaveTimestampUtc,
			["preferred_rollback_slot_kind"] = (int)SaveState.PreferredRollbackSlotKind,
		};
	}

	public void ApplyPlayerSnapshot(Godot.Collections.Dictionary snapshot)
	{
		if (snapshot.TryGetValue("display_name", out Variant displayName))
		{
			PartyState.Player.DisplayName = displayName.AsString();
		}

		if (snapshot.TryGetValue("base_max_hp", out Variant baseMaxHp))
		{
			PartyState.Player.MaxHp = baseMaxHp.AsInt32();
		}
		else if (snapshot.TryGetValue("max_hp", out Variant maxHp))
		{
			// 閸忕厧顔愰弮褍鎻╅悡褝绱版俊鍌涚亯濞屸剝婀侀弰鎯х础閸╄櫣顢呴崐纭风礉閸欘亣鍏橀柅鈧崶鐐插煂閸樺棗褰剁€涙顔岄妴?			PartyState.Player.MaxHp = maxHp.AsInt32();
		}

		if (snapshot.TryGetValue("current_hp", out Variant currentHp))
		{
			PartyState.Player.CurrentHp = currentHp.AsInt32();
		}

		if (snapshot.TryGetValue("base_move_points_per_turn", out Variant baseMovePoints))
		{
			PartyState.Player.MovePointsPerTurn = baseMovePoints.AsInt32();
		}
		else if (snapshot.TryGetValue("move_points_per_turn", out Variant movePoints))
		{
			// 閸忕厧顔愰弮褍鎻╅悡褝绱版俊鍌涚亯濞屸剝婀侀弰鎯х础閸╄櫣顢呴崐纭风礉閸欘亣鍏橀柅鈧崶鐐插煂閸樺棗褰剁€涙顔岄妴?			PartyState.Player.MovePointsPerTurn = movePoints.AsInt32();
		}

		if (snapshot.TryGetValue("attack_range", out Variant attackRange))
		{
			PartyState.Player.AttackRange = attackRange.AsInt32();
		}

		if (snapshot.TryGetValue("base_attack_damage", out Variant baseAttackDamage))
		{
			PartyState.Player.AttackDamage = baseAttackDamage.AsInt32();
		}
		else if (snapshot.TryGetValue("attack_damage", out Variant attackDamage))
		{
			// 閸忕厧顔愰弮褍鎻╅悡褝绱版俊鍌涚亯濞屸剝婀侀弰鎯х础閸╄櫣顢呴崐纭风礉閸欘亣鍏橀柅鈧崶鐐插煂閸樺棗褰剁€涙顔岄妴?			PartyState.Player.AttackDamage = attackDamage.AsInt32();
		}

		if (snapshot.TryGetValue("base_defense_damage_reduction_percent", out Variant baseDefenseReduction))
		{
			PlayerDefenseDamageReductionPercent = baseDefenseReduction.AsInt32();
		}

		if (snapshot.TryGetValue("base_defense_shield_gain", out Variant baseDefenseShieldGain))
		{
			PlayerDefenseShieldGain = baseDefenseShieldGain.AsInt32();
		}

		if (snapshot.TryGetValue("arakawa_max_energy", out Variant arakawaMaxEnergy))
		{
			PartyState.Arakawa.MaxEnergy = arakawaMaxEnergy.AsInt32();
		}

		if (snapshot.TryGetValue("arakawa_current_energy", out Variant arakawaCurrentEnergy))
		{
			PartyState.Arakawa.CurrentEnergy = arakawaCurrentEnergy.AsInt32();
		}

		SyncFieldsFromCompositeState();
		EmitSignal(SignalName.PlayerRuntimeChanged);
		EmitSignal(SignalName.ArakawaRuntimeChanged);
	}

	public void ApplyCompanionSnapshot(Godot.Collections.Dictionary snapshot)
	{
		if (snapshot.TryGetValue("growth_level", out Variant growthLevel))
		{
			PartyState.Arakawa.GrowthLevel = Mathf.Max(1, growthLevel.AsInt32());
		}

		if (snapshot.TryGetValue("arakawa_max_energy", out Variant arakawaMaxEnergy))
		{
			PartyState.Arakawa.MaxEnergy = Mathf.Max(0, arakawaMaxEnergy.AsInt32());
		}

		if (snapshot.TryGetValue("arakawa_current_energy", out Variant arakawaCurrentEnergy))
		{
			PartyState.Arakawa.CurrentEnergy = Mathf.Clamp(arakawaCurrentEnergy.AsInt32(), 0, Math.Max(PartyState.Arakawa.MaxEnergy, 0));
		}

		SyncFieldsFromCompositeState();
		EmitSignal(SignalName.ArakawaRuntimeChanged);
	}

	public void ApplyProgressionSnapshot(Godot.Collections.Dictionary snapshot)
	{
		ProgressionSnapshot progression = ProgressionSnapshot.FromDictionary(snapshot);
		ProgressionState.PlayerLevel = progression.PlayerLevel;
		ProgressionState.PlayerExperience = progression.PlayerExperience;
		ProgressionState.PlayerMasteryPoints = progression.PlayerMasteryPoints;
		ProgressionState.ArakawaGrowthLevel = progression.ArakawaGrowthLevel;
		ProgressionState.TalentIds = progression.TalentIds;
		ProgressionState.ArakawaUnlockIds = progression.ArakawaUnlockIds;
		ProgressionState.UnlockedCardIds = progression.UnlockedCardIds;
		ProgressionState.TalentBranchTags = progression.TalentBranchTags;
		ProgressionState.DeckPointBudgetBonus = progression.DeckPointBudgetBonus;
		ProgressionState.DeckMinCardCountDelta = progression.DeckMinCardCountDelta;
		ProgressionState.DeckMaxCardCountDelta = progression.DeckMaxCardCountDelta;
		ProgressionState.DeckMaxCopiesPerCardBonus = progression.DeckMaxCopiesPerCardBonus;

		SyncFieldsFromCompositeState();
	}

	public void ApplyDeckBuildSnapshot(Godot.Collections.Dictionary snapshot)
	{
		DeckBuildSnapshot deckBuild = DeckBuildSnapshot.FromDictionary(snapshot);
		bool changed = !string.Equals(DeckBuildState.BuildName, string.IsNullOrWhiteSpace(deckBuild.BuildName) ? "default" : deckBuild.BuildName, StringComparison.Ordinal)
			|| !DeckBuildState.CardIds.SequenceEqual(deckBuild.CardIds ?? Array.Empty<string>(), StringComparer.Ordinal)
			|| !DeckBuildState.RelicIds.SequenceEqual(deckBuild.RelicIds ?? Array.Empty<string>(), StringComparer.Ordinal);
		int targetRevision = deckBuild.Revision > 0
			? deckBuild.Revision
			: (changed ? Math.Max(1, DeckBuildState.Revision + 1) : DeckBuildState.Revision);
		DeckBuildState.BuildName = string.IsNullOrWhiteSpace(deckBuild.BuildName) ? "default" : deckBuild.BuildName;
		DeckBuildState.CardIds = deckBuild.CardIds;
		DeckBuildState.RelicIds = deckBuild.RelicIds;
		DeckBuildState.Revision = targetRevision;

		SyncFieldsFromCompositeState();
	}

	public void ApplyProgressionDelta(Godot.Collections.Dictionary delta)
	{
		ApplyProgressionDelta(Boundary.ProgressionDelta.FromDictionary(delta));
	}

	public void ApplyProgressionDelta(Boundary.ProgressionDelta delta)
	{
		int previousLevel = ProgressionState.PlayerLevel;
		if (delta.ExperienceDelta != 0)
		{
			ProgressionState.PlayerExperience = Math.Max(0, ProgressionState.PlayerExperience + delta.ExperienceDelta);
			ResolveLevelUpsFromExperience(previousLevel);
			previousLevel = ProgressionState.PlayerLevel;
		}

		if (delta.MasteryPointDelta != 0)
		{
			ProgressionState.PlayerMasteryPoints = Math.Max(0, ProgressionState.PlayerMasteryPoints + delta.MasteryPointDelta);
		}

		if (delta.PlayerLevelDelta != 0)
		{
			ProgressionState.PlayerLevel = Math.Max(1, ProgressionState.PlayerLevel + delta.PlayerLevelDelta);
		}

		if (delta.ArakawaGrowthLevelDelta != 0)
		{
			ProgressionState.ArakawaGrowthLevel = Math.Max(1, ProgressionState.ArakawaGrowthLevel + delta.ArakawaGrowthLevelDelta);
		}

		if (delta.TalentUnlockIds.Length > 0)
		{
			ProgressionState.TalentIds = MergeUniqueStrings(ProgressionState.TalentIds, delta.TalentUnlockIds);
		}

		if (delta.ArakawaUnlockIds.Length > 0)
		{
			ProgressionState.ArakawaUnlockIds = MergeUniqueStrings(ProgressionState.ArakawaUnlockIds, delta.ArakawaUnlockIds);
		}

		if (delta.UnlockedCardIds.Length > 0)
		{
			ProgressionState.UnlockedCardIds = MergeUniqueStrings(ProgressionState.UnlockedCardIds, delta.UnlockedCardIds);
		}

		SyncFieldsFromCompositeState();
	}

	private void ResolveLevelUpsFromExperience(int previousLevel)
	{
		int currentLevel = Math.Max(1, ProgressionState.PlayerLevel);
		while (ProgressionState.PlayerExperience >= RuntimeProgressionRuleSet.GetAccumulatedExperienceForLevel(currentLevel + 1))
		{
			currentLevel++;
		}

		if (currentLevel <= ProgressionState.PlayerLevel)
		{
			return;
		}

		ProgressionState.PlayerLevel = currentLevel;
		int masteryGain = RuntimeProgressionRuleSet.GetMasteryPointsAwardBetweenLevels(previousLevel, currentLevel);
		if (masteryGain > 0)
		{
			ProgressionState.PlayerMasteryPoints = Math.Max(0, ProgressionState.PlayerMasteryPoints + masteryGain);
		}
	}

	public void ApplyInventoryDelta(Godot.Collections.Dictionary delta)
	{
		ApplyInventoryDelta(Boundary.InventoryDelta.FromDictionary(delta));
	}

	public void ApplyInventoryDelta(Boundary.InventoryDelta delta)
	{
		foreach ((string itemId, int amount) in delta.ItemDeltas)
		{
			int currentAmount = InventoryItemCounts.TryGetValue(itemId, out Variant currentValue)
				? currentValue.AsInt32()
				: 0;
			int nextAmount = currentAmount + amount;
			if (nextAmount <= 0)
			{
				InventoryItemCounts.Remove(itemId);
				continue;
			}

			InventoryItemCounts[itemId] = nextAmount;
		}

		if (delta.KeyItemUnlockIds.Length > 0)
		{
			foreach (string keyItemId in delta.KeyItemUnlockIds)
			{
				if (!string.IsNullOrWhiteSpace(keyItemId) && !InventoryState.KeyItemIds.Contains(keyItemId))
				{
					InventoryState.KeyItemIds.Add(keyItemId);
				}
			}
		}

		SyncFieldsFromCompositeState();
		EmitSignal(SignalName.PlayerRuntimeChanged);
	}

	public void ApplyInventorySnapshot(Godot.Collections.Dictionary snapshot)
	{
		if (snapshot.TryGetValue("items", out Variant itemsVariant) && itemsVariant.Obj is Godot.Collections.Dictionary itemDictionary)
		{
			InventoryItemCounts.Clear();
			foreach (Variant key in itemDictionary.Keys)
			{
				InventoryItemCounts[key.AsString()] = itemDictionary[key].AsInt32();
			}
		}
		else
		{
			InventoryItemCounts.Clear();
			foreach (Variant key in snapshot.Keys)
			{
				InventoryItemCounts[key.AsString()] = snapshot[key].AsInt32();
			}
		}

		InventoryKeyItems.Clear();
		if (snapshot.TryGetValue("key_items", out Variant keyItemsVariant) && keyItemsVariant.Obj is Godot.Collections.Array keyItemArray)
		{
			foreach (Variant item in keyItemArray)
			{
				string text = item.AsString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					InventoryKeyItems.Add(text);
				}
			}
		}
	}

	public void ApplySaveRuntimeSnapshot(Godot.Collections.Dictionary snapshot)
	{
		if (snapshot.TryGetValue("last_checkpoint_save_id", out Variant checkpointSaveId))
		{
			SaveState.LastCheckpointSaveId = checkpointSaveId.AsString();
		}

		if (snapshot.TryGetValue("last_manual_save_id", out Variant manualSaveId))
		{
			SaveState.LastManualSaveId = manualSaveId.AsString();
		}

		if (snapshot.TryGetValue("auto_save_slot_id", out Variant autoSaveSlotId))
		{
			SaveState.AutoSaveSlotId = autoSaveSlotId.AsString();
		}

		if (snapshot.TryGetValue("last_checkpoint_scene_path", out Variant checkpointScenePath))
		{
			SaveState.LastCheckpointScenePath = checkpointScenePath.AsString();
		}

		if (snapshot.TryGetValue("last_checkpoint_map_id", out Variant checkpointMapId))
		{
			SaveState.LastCheckpointMapId = checkpointMapId.AsString();
		}

		if (snapshot.TryGetValue("last_checkpoint_spawn_id", out Variant checkpointSpawnId))
		{
			SaveState.LastCheckpointSpawnId = checkpointSpawnId.AsString();
		}

		if (snapshot.TryGetValue("last_auto_save_timestamp_utc", out Variant autoSaveTimestamp))
		{
			SaveState.LastAutoSaveTimestampUtc = autoSaveTimestamp.AsString();
		}

		if (snapshot.TryGetValue("preferred_rollback_slot_kind", out Variant rollbackSlotKind))
		{
			SaveState.PreferredRollbackSlotKind = (SaveSlotKind)rollbackSlotKind.AsInt32();
		}

		SyncFieldsFromCompositeState();
	}

	public Godot.Collections.Dictionary BuildMapRuntimeSnapshot()
	{
		return new Godot.Collections.Dictionary
		{
			["session_id"] = SessionId,
			["current_map_id"] = CurrentMapId,
			["current_map_spawn_id"] = CurrentMapSpawnId,
			["player_profile_id"] = PlayerProfileId,
			["scan_risk"] = ScanRisk,
			["should_restore_player_position"] = ShouldRestorePlayerPosition,
			["pending_restore_player_position"] = PendingRestorePlayerPosition,
			["world_flags"] = CloneDictionary(WorldFlags),
			["cleared_encounters"] = CloneStringNameArray(ClearedEncounters),
			["used_interactables"] = CloneStringNameArray(UsedInteractables),
		};
	}

	public void ApplyMapRuntimeSnapshot(Godot.Collections.Dictionary snapshot)
	{
		if (snapshot.TryGetValue("session_id", out Variant sessionId))
		{
			SessionId = sessionId.AsString();
		}

		if (snapshot.TryGetValue("current_map_id", out Variant currentMapId))
		{
			CurrentMapId = currentMapId.AsString();
		}

		if (snapshot.TryGetValue("current_map_spawn_id", out Variant currentMapSpawnId))
		{
			CurrentMapSpawnId = currentMapSpawnId.AsString();
		}

		if (snapshot.TryGetValue("player_profile_id", out Variant playerProfileId))
		{
			PlayerProfileId = playerProfileId.AsString();
		}

		if (snapshot.TryGetValue("scan_risk", out Variant scanRisk))
		{
			ScanRisk = Math.Max(0, scanRisk.AsInt32());
		}

		if (snapshot.TryGetValue("should_restore_player_position", out Variant shouldRestore))
		{
			ShouldRestorePlayerPosition = shouldRestore.AsBool();
		}

		if (snapshot.TryGetValue("pending_restore_player_position", out Variant pendingRestorePosition))
		{
			PendingRestorePlayerPosition = pendingRestorePosition.AsVector2();
		}

		if (snapshot.TryGetValue("world_flags", out Variant worldFlags) && worldFlags.Obj is Godot.Collections.Dictionary worldFlagsDictionary)
		{
			WorldFlags.Clear();
			foreach (Variant key in worldFlagsDictionary.Keys)
			{
				WorldFlags[new StringName(key.AsString())] = worldFlagsDictionary[key];
			}
		}

		if (snapshot.TryGetValue("cleared_encounters", out Variant clearedEncounters))
		{
			ReplaceStringNameArray(ClearedEncounters, clearedEncounters);
		}

		if (snapshot.TryGetValue("used_interactables", out Variant usedInteractables))
		{
			ReplaceStringNameArray(UsedInteractables, usedInteractables);
		}
	}

	private static string[] MergeUniqueStrings(string[] current, string[] incoming)
	{
		return current
			.Concat(incoming)
			.Where(value => !string.IsNullOrWhiteSpace(value))
			.Distinct(StringComparer.Ordinal)
			.ToArray();
	}

	private static Godot.Collections.Array<string> ToVariantArray(IEnumerable<string> values)
	{
		Godot.Collections.Array<string> array = new();
		foreach (string value in values)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				array.Add(value);
			}
		}

		return array;
	}

	private static string[] ToStringArray(Variant value)
	{
		if (value.VariantType == Variant.Type.Nil)
		{
			return Array.Empty<string>();
		}

		if (value.Obj is Godot.Collections.Array rawArray)
		{
			List<string> values = new();
			foreach (Variant item in rawArray)
			{
				string text = item.AsString();
				if (!string.IsNullOrWhiteSpace(text))
				{
					values.Add(text);
				}
			}

			return values.ToArray();
		}

		return Array.Empty<string>();
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

	private static Godot.Collections.Dictionary CloneDictionary(Godot.Collections.Dictionary<StringName, Variant> source)
	{
		Godot.Collections.Dictionary clone = new();
		foreach (StringName key in source.Keys)
		{
			clone[key] = source[key];
		}

		return clone;
	}

	private static Godot.Collections.Array<StringName> CloneStringNameArray(Godot.Collections.Array<StringName> source)
	{
		Godot.Collections.Array<StringName> clone = new();
		foreach (StringName value in source)
		{
			clone.Add(value);
		}

		return clone;
	}

	private static Godot.Collections.Array<string> CloneStringArray(Godot.Collections.Array<string> source)
	{
		Godot.Collections.Array<string> clone = new();
		foreach (string value in source)
		{
			clone.Add(value);
		}

		return clone;
	}

	private static void ReplaceStringNameArray(Godot.Collections.Array<StringName> target, Variant value)
	{
		target.Clear();
		if (value.Obj is not Godot.Collections.Array rawArray)
		{
			return;
		}

		foreach (Variant item in rawArray)
		{
			string text = item.AsString();
			if (!string.IsNullOrWhiteSpace(text))
			{
				target.Add(new StringName(text));
			}
		}
	}

	private static int ClampOptional(int value, int? min, int? max)
	{
		if (min.HasValue && value < min.Value)
		{
			value = min.Value;
		}

		if (max.HasValue && value > max.Value)
		{
			value = max.Value;
		}

		return value;
	}

	private void EnsureCompositionServices()
	{
		_equipmentService ??= new EquipmentService(RuntimeEquipmentCatalog);
		_playerStatResolver ??= new PlayerStatResolver(RuntimeEquipmentCatalog);
	}

	private void SyncCompositeStateFromFields()
	{
		PartyState.Player.DisplayName = PlayerDisplayName;
		PartyState.Player.MaxHp = PlayerMaxHp;
		PartyState.Player.CurrentHp = PlayerCurrentHp;
		PartyState.Player.MovePointsPerTurn = PlayerMovePointsPerTurn;
		PartyState.Player.AttackRange = PlayerAttackRange;
		PartyState.Player.AttackDamage = PlayerAttackDamage;

		PartyState.Arakawa.CompanionId = "arakawa";
		PartyState.Arakawa.DisplayName = "Arakawa";
		PartyState.Arakawa.GrowthLevel = ArakawaGrowthLevel;
		PartyState.Arakawa.MaxEnergy = ArakawaMaxEnergy;
		PartyState.Arakawa.CurrentEnergy = ArakawaCurrentEnergy;

		ProgressionState.PlayerLevel = PlayerLevel;
		ProgressionState.PlayerExperience = PlayerExperience;
		ProgressionState.PlayerMasteryPoints = PlayerMasteryPoints;
		ProgressionState.ArakawaGrowthLevel = ArakawaGrowthLevel;
		ProgressionState.TalentIds = TalentIds;
		ProgressionState.ArakawaUnlockIds = ArakawaUnlockIds;
		ProgressionState.UnlockedCardIds = UnlockedCardIds;
		ProgressionState.TalentBranchTags = TalentBranchTags;
		ProgressionState.DeckPointBudgetBonus = DeckPointBudgetBonus;
		ProgressionState.DeckMinCardCountDelta = DeckMinCardCountDelta;
		ProgressionState.DeckMaxCardCountDelta = DeckMaxCardCountDelta;
		ProgressionState.DeckMaxCopiesPerCardBonus = DeckMaxCopiesPerCardBonus;

		DeckBuildState.BuildName = DeckBuildName;
		DeckBuildState.CardIds = DeckCardIds;
		DeckBuildState.RelicIds = DeckRelicIds;
		DeckBuildState.Revision = Math.Max(0, DeckBuildRevision);

		EquipmentLoadoutState.WeaponItemId = EquippedWeaponItemId;
		EquipmentLoadoutState.ArmorItemId = EquippedArmorItemId;
		EquipmentLoadoutState.AccessoryItemId = EquippedAccessoryItemId;
		InventoryState.KeyItemIds.Clear();
		foreach (string keyItemId in InventoryKeyItemIds)
		{
			if (!string.IsNullOrWhiteSpace(keyItemId))
			{
				InventoryState.KeyItemIds.Add(keyItemId);
			}
		}

		SaveState.LastCheckpointSaveId = LastCheckpointSaveId;
		SaveState.LastManualSaveId = LastManualSaveId;
		SaveState.AutoSaveSlotId = AutoSaveSlotId;
		SaveState.LastCheckpointScenePath = LastCheckpointScenePath;
		SaveState.LastCheckpointMapId = LastCheckpointMapId;
		SaveState.LastCheckpointSpawnId = LastCheckpointSpawnId;
		SaveState.LastAutoSaveTimestampUtc = LastAutoSaveTimestampUtc;
	}

	private void SyncFieldsFromCompositeState()
	{
		PlayerDisplayName = PartyState.Player.DisplayName;
		PlayerMaxHp = PartyState.Player.MaxHp;
		PlayerCurrentHp = PartyState.Player.CurrentHp;
		PlayerMovePointsPerTurn = PartyState.Player.MovePointsPerTurn;
		PlayerAttackRange = PartyState.Player.AttackRange;
		PlayerAttackDamage = PartyState.Player.AttackDamage;

		ArakawaMaxEnergy = PartyState.Arakawa.MaxEnergy;
		ArakawaCurrentEnergy = PartyState.Arakawa.CurrentEnergy;
		ArakawaGrowthLevel = PartyState.Arakawa.GrowthLevel;

		PlayerLevel = ProgressionState.PlayerLevel;
		PlayerExperience = ProgressionState.PlayerExperience;
		PlayerMasteryPoints = ProgressionState.PlayerMasteryPoints;
		TalentIds = ProgressionState.TalentIds;
		ArakawaUnlockIds = ProgressionState.ArakawaUnlockIds;
		UnlockedCardIds = ProgressionState.UnlockedCardIds;
		TalentBranchTags = ProgressionState.TalentBranchTags;
		DeckPointBudgetBonus = ProgressionState.DeckPointBudgetBonus;
		DeckMinCardCountDelta = ProgressionState.DeckMinCardCountDelta;
		DeckMaxCardCountDelta = ProgressionState.DeckMaxCardCountDelta;
		DeckMaxCopiesPerCardBonus = ProgressionState.DeckMaxCopiesPerCardBonus;

		DeckBuildName = DeckBuildState.BuildName;
		DeckCardIds = DeckBuildState.CardIds;
		DeckRelicIds = DeckBuildState.RelicIds;
		DeckBuildRevision = DeckBuildState.Revision;
		EquippedWeaponItemId = EquipmentLoadoutState.WeaponItemId;
		EquippedArmorItemId = EquipmentLoadoutState.ArmorItemId;
		EquippedAccessoryItemId = EquipmentLoadoutState.AccessoryItemId;
		InventoryKeyItemIds = InventoryState.KeyItemIds.ToArray();

		LastCheckpointSaveId = SaveState.LastCheckpointSaveId;
		LastManualSaveId = SaveState.LastManualSaveId;
		AutoSaveSlotId = SaveState.AutoSaveSlotId;
		LastCheckpointScenePath = SaveState.LastCheckpointScenePath;
		LastCheckpointMapId = SaveState.LastCheckpointMapId;
		LastCheckpointSpawnId = SaveState.LastCheckpointSpawnId;
		LastAutoSaveTimestampUtc = SaveState.LastAutoSaveTimestampUtc;
	}
}
