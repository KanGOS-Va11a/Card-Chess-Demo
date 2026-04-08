using Godot;
using System;
using CardChessDemo.Battle.Shared;

public partial class GameSession : Node
{
	// Interface-contract fields (snake_case)
	public string session_id { get; set; } = Guid.NewGuid().ToString("N");
	public StringName current_map_id { get; set; } = new StringName("scene1");
	public StringName current_map_spawn_id { get; set; } = new StringName("");
	public StringName player_profile_id { get; set; } = new StringName("default_player");

	public PlayerRuntimeState player_runtime { get; set; } = new PlayerRuntimeState();
	public DeckState deck_state { get; set; } = new DeckState();
	public InventoryState inventory_state { get; set; } = new InventoryState();
	public ArakawaState arakawa_state { get; set; } = new ArakawaState();
	public SuitcaseState suitcase_state { get; set; } = new SuitcaseState();

	public Godot.Collections.Dictionary<StringName, Variant> world_flags { get; set; }
		= new Godot.Collections.Dictionary<StringName, Variant>();
	public int scan_risk { get; set; }
	public Godot.Collections.Array<StringName> cleared_encounters { get; set; }
		= new Godot.Collections.Array<StringName>();
	public Godot.Collections.Array<StringName> used_interactables { get; set; }
		= new Godot.Collections.Array<StringName>();

	// Legacy battle return position fields still used by older map-side scripts.
	public bool should_restore_player_position { get; set; }
	public Vector2 pending_restore_player_position { get; set; } = Vector2.Zero;

	private GlobalGameSession? _globalSession;

	public override void _Ready()
	{
		if (string.IsNullOrWhiteSpace(session_id))
		{
			session_id = Guid.NewGuid().ToString("N");
		}

		_globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		SyncFromGlobalGameSession();
	}

	public override void _Process(double delta)
	{
		if (_globalSession == null)
		{
			_globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		}

		SyncFromGlobalGameSession();
	}

	public void start_new_session(StringName map_id, StringName spawn_id)
	{
		session_id = Guid.NewGuid().ToString("N");
		world_flags.Clear();
		cleared_encounters.Clear();
		used_interactables.Clear();
		scan_risk = 0;
		should_restore_player_position = false;
		pending_restore_player_position = Vector2.Zero;
		current_map_id = map_id;
		current_map_spawn_id = spawn_id;

		if (_globalSession == null)
		{
			return;
		}

		_globalSession.SessionId = session_id;
		_globalSession.SetCurrentMapContext(map_id.ToString(), spawn_id.ToString(), player_profile_id.ToString());
		_globalSession.WorldFlags.Clear();
		_globalSession.ClearedEncounters.Clear();
		_globalSession.UsedInteractables.Clear();
		_globalSession.ScanRisk = 0;
		_globalSession.ClearPendingRestorePlayerPosition();
		_globalSession.ApplyInventorySnapshot(new Godot.Collections.Dictionary());
		_globalSession.SetPlayerCurrentHp(Math.Max(0, player_runtime.hp_current));
		_globalSession.SetArakawaCurrentEnergy(Math.Max(0, arakawa_state.energy_current));
		SyncFromGlobalGameSession();
	}

	public void set_flag(StringName key, Variant value)
	{
		world_flags[key] = value;
		_globalSession?.SetFlag(key, value);
	}

	public void mark_encounter_cleared(StringName encounter_id)
	{
		if (!cleared_encounters.Contains(encounter_id))
		{
			cleared_encounters.Add(encounter_id);
		}

		_globalSession?.MarkEncounterCleared(encounter_id);
	}

	public void mark_interactable_used(StringName interactable_id)
	{
		if (!used_interactables.Contains(interactable_id))
		{
			used_interactables.Add(interactable_id);
		}

		_globalSession?.MarkInteractableUsed(interactable_id);
	}

	public void apply_resource_delta(StringName resource_key, int delta, int? clamp_min = null, int? clamp_max = null)
	{
		int value;
		switch (resource_key)
		{
			case "player_hp":
				value = player_runtime.hp_current + delta;
				player_runtime.hp_current = clamp_optional(value, clamp_min, clamp_max);
				if (_globalSession != null)
				{
					_globalSession.SetPlayerCurrentHp(player_runtime.hp_current);
				}
				break;
			case "arakawa_energy":
				value = arakawa_state.energy_current + delta;
				arakawa_state.energy_current = clamp_optional(value, clamp_min, clamp_max);
				if (_globalSession != null)
				{
					_globalSession.SetArakawaCurrentEnergy(arakawa_state.energy_current);
				}
				break;
			case "suitcase_fuel":
				value = suitcase_state.fuel_current + delta;
				suitcase_state.fuel_current = clamp_optional(value, clamp_min, clamp_max);
				break;
			case "scan_risk":
				value = scan_risk + delta;
				scan_risk = clamp_optional(value, clamp_min, clamp_max);
				if (_globalSession != null)
				{
					_globalSession.ScanRisk = scan_risk;
				}
				break;
			default:
				GD.PushWarning($"GameSession: unknown resource key '{resource_key}'.");
				break;
		}
	}

	public void apply_inventory_delta(StringName item_id, int amount)
	{
		int current = 0;
		if (inventory_state.items.TryGetValue(item_id, out int value))
		{
			current = value;
		}

		int next = current + amount;
		if (next <= 0)
		{
			inventory_state.items.Remove(item_id);
		}
		else
		{
			inventory_state.items[item_id] = next;
		}

		if (_globalSession != null)
		{
			Godot.Collections.Dictionary delta = new()
			{
				[item_id.ToString()] = amount,
			};
			_globalSession.ApplyInventoryDelta(delta);
		}
	}

	private void SyncFromGlobalGameSession()
	{
		if (_globalSession == null)
		{
			return;
		}

		session_id = string.IsNullOrWhiteSpace(_globalSession.SessionId) ? session_id : _globalSession.SessionId;
		current_map_id = new StringName(_globalSession.CurrentMapId);
		current_map_spawn_id = new StringName(_globalSession.CurrentMapSpawnId);
		player_profile_id = new StringName(_globalSession.PlayerProfileId);
		scan_risk = _globalSession.ScanRisk;
		should_restore_player_position = _globalSession.ShouldRestorePlayerPosition;
		pending_restore_player_position = _globalSession.PendingRestorePlayerPosition;

		player_runtime.hp_current = _globalSession.PlayerCurrentHp;
		player_runtime.hp_max = _globalSession.GetResolvedPlayerMaxHp();

		deck_state.deck_list = ToStringNameArray(_globalSession.DeckBuildState.CardIds);
		inventory_state.items = ToItemDictionary(_globalSession.InventoryItemCounts);
		inventory_state.key_items = ToStringNameArray(_globalSession.InventoryKeyItems);
		arakawa_state.energy_current = _globalSession.ArakawaCurrentEnergy;
		arakawa_state.energy_cap = _globalSession.ArakawaMaxEnergy;
		arakawa_state.growth_level = _globalSession.ArakawaGrowthLevel;
		arakawa_state.unlocks = ToStringNameArray(_globalSession.ArakawaUnlockIds);
		world_flags = _globalSession.WorldFlags;
		cleared_encounters = _globalSession.ClearedEncounters;
		used_interactables = _globalSession.UsedInteractables;
	}

	private void SyncToGlobalGameSession()
	{
		if (_globalSession == null)
		{
			return;
		}

		_globalSession.SetPlayerCurrentHp(player_runtime.hp_current);
		_globalSession.SetArakawaCurrentEnergy(arakawa_state.energy_current);
		_globalSession.ArakawaGrowthLevel = arakawa_state.growth_level;
		_globalSession.ArakawaUnlockIds = ToStringArray(arakawa_state.unlocks);
		_globalSession.PlayerLevel = Math.Max(1, _globalSession.PlayerLevel);
		_globalSession.SessionId = session_id;
		_globalSession.SetCurrentMapContext(current_map_id.ToString(), current_map_spawn_id.ToString(), player_profile_id.ToString());
		_globalSession.ScanRisk = scan_risk;
		_globalSession.ShouldRestorePlayerPosition = should_restore_player_position;
		_globalSession.PendingRestorePlayerPosition = pending_restore_player_position;
		_globalSession.WorldFlags.Clear();
		foreach (StringName key in world_flags.Keys)
		{
			_globalSession.WorldFlags[key] = world_flags[key];
		}
		_globalSession.ClearedEncounters.Clear();
		foreach (StringName encounterId in cleared_encounters)
		{
			_globalSession.ClearedEncounters.Add(encounterId);
		}
		_globalSession.UsedInteractables.Clear();
		foreach (StringName interactableId in used_interactables)
		{
			_globalSession.UsedInteractables.Add(interactableId);
		}
		_globalSession.ApplyInventorySnapshot(ToVariantInventorySnapshot(inventory_state.items));
		_globalSession.InventoryKeyItems.Clear();
		foreach (StringName keyItem in inventory_state.key_items)
		{
			_globalSession.InventoryKeyItems.Add(keyItem.ToString());
		}
	}

	private static Godot.Collections.Array<StringName> ToStringNameArray(string[] values)
	{
		Godot.Collections.Array<StringName> result = new();
		foreach (string value in values)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				result.Add(new StringName(value));
			}
		}

		return result;
	}

	private static Godot.Collections.Array<StringName> ToStringNameArray(Godot.Collections.Array<string> values)
	{
		Godot.Collections.Array<StringName> result = new();
		foreach (string value in values)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				result.Add(new StringName(value));
			}
		}

		return result;
	}

	private static string[] ToStringArray(Godot.Collections.Array<StringName> values)
	{
		string[] result = new string[values.Count];
		for (int i = 0; i < values.Count; i++)
		{
			result[i] = values[i].ToString();
		}

		return result;
	}

	private static Godot.Collections.Dictionary<StringName, int> ToItemDictionary(Godot.Collections.Dictionary values)
	{
		Godot.Collections.Dictionary<StringName, int> result = new();
		foreach (Variant key in values.Keys)
		{
			result[new StringName(key.AsString())] = values[key].AsInt32();
		}

		return result;
	}

	private static Godot.Collections.Dictionary ToVariantInventorySnapshot(Godot.Collections.Dictionary<StringName, int> values)
	{
		Godot.Collections.Dictionary result = new();
		foreach (StringName key in values.Keys)
		{
			result[key.ToString()] = values[key];
		}

		return result;
	}

	private static int clamp_optional(int value, int? min, int? max)
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
}

public class PlayerRuntimeState
{
	public int hp_current { get; set; } = 100;
	public int hp_max { get; set; } = 100;
	public Godot.Collections.Dictionary<StringName, Variant> stat_modifiers { get; set; }
		= new Godot.Collections.Dictionary<StringName, Variant>();
	public Godot.Collections.Dictionary<StringName, Variant> status_runtime { get; set; }
		= new Godot.Collections.Dictionary<StringName, Variant>();
}

public class DeckState
{
	public Godot.Collections.Array<StringName> deck_list { get; set; } = new Godot.Collections.Array<StringName>();
	public int build_version { get; set; } = 1;
	public int deck_runtime_seed { get; set; }
}

public class InventoryState
{
	public Godot.Collections.Dictionary<StringName, int> items { get; set; }
		= new Godot.Collections.Dictionary<StringName, int>();
	public Godot.Collections.Array<StringName> key_items { get; set; }
		= new Godot.Collections.Array<StringName>();
}

public class ArakawaState
{
	public int energy_current { get; set; } = 100;
	public int energy_cap { get; set; } = 100;
	public int growth_level { get; set; } = 1;
	public Godot.Collections.Array<StringName> unlocks { get; set; } = new Godot.Collections.Array<StringName>();
}

public class SuitcaseState
{
	public int fuel_current { get; set; } = 10;
	public int fuel_cap { get; set; } = 100;
	public Godot.Collections.Dictionary<StringName, Variant> overload_state { get; set; }
		= new Godot.Collections.Dictionary<StringName, Variant>();
	public Godot.Collections.Array<StringName> modules { get; set; } = new Godot.Collections.Array<StringName>();
}
