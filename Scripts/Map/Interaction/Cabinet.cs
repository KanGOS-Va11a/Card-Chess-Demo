using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class Cabinet : InteractableTemplate
{
	[Export] public string CabinetName = "\u50A8\u7269\u67DC";
	[Export] public string ItemDescription = "\u627E\u5230\u4E86\u4E00\u4E9B\u53EF\u7528\u7269\u8D44\u3002";
	[Export] public string EmptyDescription = "\u8FD9\u4E2A\u67DC\u5B50\u5DF2\u7ECF\u88AB\u641C\u7A7A\u4E86\u3002";
	[Export] public string LootItemId = "steel_scrap";
	[Export(PropertyHint.Range, "1,99,1")] public int LootAmount = 1;
	[Export] public string InteractableSessionKey = string.Empty;
	[Export] public Color OpenedTint = new(0.72f, 0.72f, 0.72f, 1.0f);

	private bool _isOpened;
	private bool _isSearching;
	private Sprite2D? _sprite;
	private Color _defaultTint = Colors.White;
	private GlobalGameSession? _session;

	public override void _Ready()
	{
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (_sprite != null)
		{
			_defaultTint = _sprite.Modulate;
		}

		if (WasAlreadyOpenedInSession())
		{
			_isOpened = true;
			LootAmount = 0;
		}

		ApplyVisualState();
	}

	public override string GetInteractText(Player player)
	{
		if (_isSearching)
		{
			return "\u641C\u7D22\u4E2D...";
		}

		if (_isOpened || LootAmount <= 0 || string.IsNullOrWhiteSpace(LootItemId))
		{
			return "\u67DC\u5B50\u5DF2\u88AB\u641C\u7D22";
		}

		return string.IsNullOrWhiteSpace(PromptText) ? "\u641C\u7D22\u67DC\u5B50" : PromptText;
	}

	public override bool CanInteract(Player player)
	{
		return !_isSearching && base.CanInteract(player);
	}

	protected override void OnInteract(Player player)
	{
		if (_isOpened || LootAmount <= 0 || string.IsNullOrWhiteSpace(LootItemId))
		{
			ShowText(EmptyDescription);
			PlayInteractionPulse();
			return;
		}

		_isSearching = true;
		int grantedAmount = LootAmount;
		bool granted = TryGrantLoot();
		string message = granted ? BuildGainMessage(grantedAmount) : EmptyDescription;
		ShowText(message);
		PlayInteractionPulse();
		_isSearching = false;
	}

	public override Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		Godot.Collections.Dictionary snapshot = base.BuildRuntimeSnapshot();
		snapshot["is_opened"] = _isOpened;
		snapshot["loot_amount"] = LootAmount;
		return snapshot;
	}

	public override void ApplyRuntimeSnapshot(Godot.Collections.Dictionary snapshot)
	{
		base.ApplyRuntimeSnapshot(snapshot);

		if (snapshot.TryGetValue("is_opened", out Variant isOpened))
		{
			_isOpened = isOpened.AsBool();
		}

		if (snapshot.TryGetValue("loot_amount", out Variant lootAmount))
		{
			LootAmount = lootAmount.AsInt32();
		}

		if (WasAlreadyOpenedInSession())
		{
			_isOpened = true;
			LootAmount = 0;
		}

		_isSearching = false;
		ApplyVisualState();
	}

	private bool WasAlreadyOpenedInSession()
	{
		return _session != null && _session.UsedInteractables.Contains(BuildInteractableSessionId());
	}

	private StringName BuildInteractableSessionId()
	{
		if (!string.IsNullOrWhiteSpace(InteractableSessionKey))
		{
			return new StringName(InteractableSessionKey.Trim());
		}

		string scenePath = GetTree().CurrentScene?.SceneFilePath ?? string.Empty;
		return new StringName($"{scenePath}::{GetPath()}");
	}

	private void ApplyVisualState()
	{
		if (_sprite == null)
		{
			return;
		}

		_sprite.Modulate = _isOpened ? OpenedTint : _defaultTint;
	}

	private bool TryGrantLoot()
	{
		if (_session == null || string.IsNullOrWhiteSpace(LootItemId) || LootAmount <= 0)
		{
			return false;
		}

		StringName interactableId = BuildInteractableSessionId();
		if (_session.UsedInteractables.Contains(interactableId))
		{
			_isOpened = true;
			LootAmount = 0;
			ApplyVisualState();
			return false;
		}

		InventoryDelta delta = new();
		delta.ItemDeltas[LootItemId.Trim()] = LootAmount;
		_session.ApplyInventoryDelta(delta);

		_isOpened = true;
		LootAmount = 0;
		_session.MarkInteractableUsed(interactableId);
		ApplyVisualState();
		return true;
	}

	private string BuildGainMessage(int amount)
	{
		if (!string.IsNullOrWhiteSpace(ItemDescription))
		{
			return ItemDescription;
		}

		return $"\u83B7\u5F97 {LootItemId} x{amount}";
	}

	private void ShowText(string message)
	{
		SceneTextOverlay.Show(this, message);
	}
}
