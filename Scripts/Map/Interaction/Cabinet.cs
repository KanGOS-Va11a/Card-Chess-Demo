using System.Collections.Generic;
using CardChessDemo.Audio;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;
using Godot;

namespace CardChessDemo.Map;

public partial class Cabinet : InteractableTemplate, IConfigurableLootInteractable
{
	[Export] public string CabinetName { get; set; } = "储物柜";
	[Export] public string ItemDescription { get; set; } = "找到了可用物资。";
	[Export] public string EmptyDescription { get; set; } = "这个柜子已经被搜空了。";
	[Export] public string LootItemId { get; set; } = "steel_scrap";
	[Export(PropertyHint.Range, "1,99,1")] public int LootAmount { get; set; } = 1;
	[Export] public string InteractableSessionKey { get; set; } = string.Empty;
	[Export] public string[] InteractionTexts { get; set; } = System.Array.Empty<string>();
	[Export] public Godot.Collections.Array<InteractableItemGrant> GrantedItems { get; set; } = new();
	[Export] public Color OpenedTint { get; set; } = new(0.72f, 0.72f, 0.72f, 1.0f);

	private bool _isOpened;
	private bool _isSearching;
	private int _interactionCount;
	private Sprite2D? _sprite;
	private Color _defaultTint = Colors.White;
	private GlobalGameSession? _session;

	string IConfigurableLootInteractable.DisplayName
	{
		get => CabinetName;
		set => CabinetName = value;
	}

	string IConfigurableLootInteractable.PromptText
	{
		get => PromptText;
		set => PromptText = value;
	}

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
			_interactionCount = System.Math.Max(_interactionCount, 1);
		}

		ApplyVisualState();
	}

	public override string GetInteractText(Player player)
	{
		if (_isSearching)
		{
			return "搜索中...";
		}

		if (_isOpened)
		{
			return "检查柜子";
		}

		return string.IsNullOrWhiteSpace(PromptText) ? "搜索柜子" : PromptText;
	}

	public override bool CanInteract(Player player)
	{
		return !_isSearching && base.CanInteract(player);
	}

	protected override void OnInteract(Player player)
	{
		_isSearching = true;
		bool grantedLoot = false;
		if (!_isOpened)
		{
			grantedLoot = TryGrantLoot();
		}

		SceneTextOverlay.Show(this, ResolveInteractionMessage(grantedLoot));
		PlayInteractionPulse();
		GameAudio.Instance?.PlayUiConfirm();
		_isSearching = false;
		_interactionCount++;
	}

	public override Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		Godot.Collections.Dictionary snapshot = base.BuildRuntimeSnapshot();
		snapshot["is_opened"] = _isOpened;
		snapshot["loot_amount"] = LootAmount;
		snapshot["interaction_count"] = _interactionCount;
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

		if (snapshot.TryGetValue("interaction_count", out Variant interactionCount))
		{
			_interactionCount = interactionCount.AsInt32();
		}

		if (WasAlreadyOpenedInSession())
		{
			_isOpened = true;
			LootAmount = 0;
			_interactionCount = System.Math.Max(_interactionCount, 1);
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
		if (_session == null)
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

		bool grantedLoot = InteractableRewardResolver.ApplyConfiguredRewards(_session, EnumerateGrantedItems());

		_isOpened = true;
		LootAmount = 0;
		_session.MarkInteractableUsed(interactableId);
		ApplyVisualState();
		return grantedLoot;
	}

	private string ResolveInteractionMessage(bool granted)
	{
		if (InteractionTexts.Length > 0)
		{
			int index = Mathf.Clamp(_interactionCount, 0, InteractionTexts.Length - 1);
			string configured = InteractionTexts[index]?.Trim() ?? string.Empty;
			if (!string.IsNullOrWhiteSpace(configured))
			{
				return configured;
			}
		}

		if (granted)
		{
			return InteractableItemTextResolver.BuildLootSummary(_session, EnumerateGrantedItems());
		}

		return string.IsNullOrWhiteSpace(EmptyDescription) ? "这个柜子已经被搜空了。" : EmptyDescription;
	}

	private IEnumerable<(string ItemId, int Amount)> EnumerateGrantedItems()
	{
		HashSet<string> yielded = new(System.StringComparer.Ordinal);
		foreach (InteractableItemGrant? entry in GrantedItems)
		{
			if (entry == null || string.IsNullOrWhiteSpace(entry.ItemId) || entry.Amount <= 0)
			{
				continue;
			}

			string itemId = entry.ItemId.Trim();
			yielded.Add(itemId);
			yield return (itemId, entry.Amount);
		}

		if (!string.IsNullOrWhiteSpace(LootItemId) && LootAmount > 0)
		{
			string legacyItemId = LootItemId.Trim();
			if (!yielded.Contains(legacyItemId))
			{
				yield return (legacyItemId, LootAmount);
			}
		}
	}
}
