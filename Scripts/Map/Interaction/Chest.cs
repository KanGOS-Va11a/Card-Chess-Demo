using System.Collections.Generic;
using CardChessDemo.Audio;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;
using Godot;

namespace CardChessDemo.Map;

public partial class Chest : InteractableTemplate, IConfigurableLootInteractable
{
	[Export] public string ChestName { get; set; } = "宝箱";
	[Export] public string GrantedItemId { get; set; } = string.Empty;
	[Export] public string LootItemId { get; set; } = string.Empty;
	[Export(PropertyHint.Range, "0,99,1")] public int LootAmount { get; set; } = 0;
	[Export] public string ItemDescription { get; set; } = "获得了物品。";
	[Export] public string EmptyDescription { get; set; } = "箱子是空的。";
	[Export] public string InteractableSessionKey { get; set; } = string.Empty;
	[Export] public string[] InteractionTexts { get; set; } = System.Array.Empty<string>();
	[Export] public Godot.Collections.Array<InteractableItemGrant> GrantedItems { get; set; } = new();
	[Export] public Color OpenedTint { get; set; } = new(0.72f, 0.72f, 0.72f, 1.0f);

	private const string ClosedAnimationName = "closed";
	private const string OpenAnimationName = "open";

	private bool _isOpened;
	private bool _isOpening;
	private int _interactionCount;
	private AnimatedSprite2D? _animatedSprite;
	private Sprite2D? _staticSprite;
	private Color _defaultTint = Colors.White;
	private GlobalGameSession? _session;

	string IConfigurableLootInteractable.DisplayName
	{
		get => ChestName;
		set => ChestName = value;
	}

	string IConfigurableLootInteractable.PromptText
	{
		get => PromptText;
		set => PromptText = value;
	}

	public override void _Ready()
	{
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_staticSprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		if (_staticSprite != null)
		{
			_defaultTint = _staticSprite.Modulate;
		}

		ApplyOpenedStateFromSession();
		RefreshAnimationState();

		if (_animatedSprite != null)
		{
			_animatedSprite.AnimationFinished += OnAnimatedChestFinished;
		}
	}

	public override string GetInteractText(Player player)
	{
		if (_isOpening)
		{
			return "打开中...";
		}

		if (_isOpened)
		{
			return "检查箱子";
		}

		return string.IsNullOrWhiteSpace(PromptText) ? "打开宝箱" : PromptText;
	}

	public override bool CanInteract(Player player)
	{
		return !_isOpening && base.CanInteract(player);
	}

	protected override void OnInteract(Player player)
	{
		bool grantedLoot = false;
		if (!_isOpened)
		{
			_isOpening = true;
			_isOpened = true;
			grantedLoot = GrantConfiguredItems();
			MarkOpenedInSession();
		}

		SceneTextOverlay.Show(this, ResolveInteractionMessage(grantedLoot));
		PlayInteractionPulse();
		GameAudio.Instance?.PlayUiConfirm();
		_interactionCount++;

		if (_animatedSprite != null
			&& _animatedSprite.SpriteFrames != null
			&& _animatedSprite.SpriteFrames.HasAnimation(OpenAnimationName))
		{
			_animatedSprite.Play(OpenAnimationName);
			return;
		}

		_isOpening = false;
	}

	public override Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		Godot.Collections.Dictionary snapshot = base.BuildRuntimeSnapshot();
		snapshot["is_opened"] = _isOpened;
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

		if (snapshot.TryGetValue("interaction_count", out Variant interactionCount))
		{
			_interactionCount = interactionCount.AsInt32();
		}

		_isOpening = false;
		ApplyOpenedStateFromSession();
		RefreshAnimationState();
	}

	private void RefreshAnimationState()
	{
		if (_animatedSprite != null && _animatedSprite.SpriteFrames != null)
		{
			if (_isOpened && _animatedSprite.SpriteFrames.HasAnimation(OpenAnimationName))
			{
				_animatedSprite.Play(OpenAnimationName);
				int frameCount = _animatedSprite.SpriteFrames.GetFrameCount(OpenAnimationName);
				_animatedSprite.Frame = Mathf.Max(0, frameCount - 1);
				_animatedSprite.Stop();
			}
			else if (_animatedSprite.SpriteFrames.HasAnimation(ClosedAnimationName))
			{
				_animatedSprite.Play(ClosedAnimationName);
				_animatedSprite.Stop();
			}
		}

		if (_staticSprite != null)
		{
			_staticSprite.Modulate = _isOpened ? OpenedTint : _defaultTint;
		}
	}

	private void ApplyOpenedStateFromSession()
	{
		if (WasAlreadyOpenedInSession())
		{
			_isOpened = true;
			_interactionCount = System.Math.Max(_interactionCount, 1);
		}
	}

	private bool GrantConfiguredItems()
	{
		if (_session == null)
		{
			GD.PushWarning($"Chest '{Name}' could not find /root/GlobalGameSession. Item grant skipped.");
			return false;
		}

		return InteractableRewardResolver.ApplyConfiguredRewards(_session, EnumerateGrantedItems());
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

		if (!string.IsNullOrWhiteSpace(GrantedItemId))
		{
			string legacyId = GrantedItemId.Trim();
			if (!yielded.Contains(legacyId))
			{
				yield return (legacyId, 1);
			}
		}

		if (!string.IsNullOrWhiteSpace(LootItemId) && LootAmount > 0)
		{
			string legacyLootId = LootItemId.Trim();
			if (!yielded.Contains(legacyLootId))
			{
				yield return (legacyLootId, LootAmount);
			}
		}
	}

	private string ResolveInteractionMessage(bool grantedLoot)
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

		if (grantedLoot)
		{
			return InteractableItemTextResolver.BuildLootSummary(_session, EnumerateGrantedItems());
		}

		return string.IsNullOrWhiteSpace(EmptyDescription) ? "箱子是空的。" : EmptyDescription;
	}

	private bool WasAlreadyOpenedInSession()
	{
		return _session != null && _session.UsedInteractables.Contains(BuildInteractableSessionId());
	}

	private void MarkOpenedInSession()
	{
		_session?.MarkInteractableUsed(BuildInteractableSessionId());
	}

	private StringName BuildInteractableSessionId()
	{
		if (!string.IsNullOrWhiteSpace(InteractableSessionKey))
		{
			return new StringName(InteractableSessionKey.Trim());
		}

		return new StringName(BuildRuntimeStateKey(GetTree().CurrentScene));
	}

	private void OnAnimatedChestFinished()
	{
		if (_animatedSprite == null || _animatedSprite.Animation != OpenAnimationName)
		{
			_isOpening = false;
			return;
		}

		if (_animatedSprite.SpriteFrames != null)
		{
			int frameCount = _animatedSprite.SpriteFrames.GetFrameCount(OpenAnimationName);
			_animatedSprite.Frame = Mathf.Max(0, frameCount - 1);
		}

		_animatedSprite.Stop();
		_isOpening = false;
	}
}
