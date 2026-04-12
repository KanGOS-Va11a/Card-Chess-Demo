using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class Chest : InteractableTemplate
{
	[Export] public string ChestName = "宝箱";
	[Export] public string GrantedItemId = string.Empty;
	[Export] public string ItemDescription = "获得了物品。";
	[Export] public string EmptyDescription = "箱子是空的。";

	private const string ClosedAnimationName = "closed";
	private const string OpenAnimationName = "open";

	private bool _isOpened;
	private bool _isOpening;
	private AnimatedSprite2D? _animatedSprite;

	public override void _Ready()
	{
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (_animatedSprite == null)
		{
			return;
		}

		if (_animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation(ClosedAnimationName))
		{
			_animatedSprite.Play(ClosedAnimationName);
			_animatedSprite.Stop();
		}
		else if (_animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation(OpenAnimationName))
		{
			_animatedSprite.Play(OpenAnimationName);
			_animatedSprite.Frame = 0;
			_animatedSprite.Stop();
		}

		_animatedSprite.AnimationFinished += OnAnimatedChestFinished;
	}

	public override string GetInteractText(Player player)
	{
		if (_isOpening)
		{
			return "打开中...";
		}

		if (_isOpened)
		{
			return "箱子是空的";
		}

		return string.IsNullOrWhiteSpace(PromptText) ? "打开宝箱" : PromptText;
	}

	public override bool CanInteract(Player player)
	{
		return !_isOpening && base.CanInteract(player);
	}

	protected override void OnInteract(Player player)
	{
		if (_isOpened)
		{
			SceneTextOverlay.Show(this, EmptyDescription);
			PlayInteractionPulse();
			return;
		}

		_isOpening = true;
		_isOpened = true;
		GrantConfiguredItem();
		SceneTextOverlay.Show(this, ItemDescription);
		PlayInteractionPulse();

		bool hasOpenAnimation = _animatedSprite != null
			&& _animatedSprite.SpriteFrames != null
			&& _animatedSprite.SpriteFrames.HasAnimation(OpenAnimationName);
		if (hasOpenAnimation)
		{
			_animatedSprite!.Play(OpenAnimationName);
		}
		else
		{
			_isOpening = false;
		}
	}

	private void GrantConfiguredItem()
	{
		if (string.IsNullOrWhiteSpace(GrantedItemId))
		{
			return;
		}

		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (session == null)
		{
			GD.PushWarning($"Chest '{Name}' could not find /root/GlobalGameSession. Item grant skipped.");
			return;
		}

		InventoryDelta delta = new();
		delta.ItemDeltas[GrantedItemId.Trim()] = 1;
		session.ApplyInventoryDelta(delta);
	}

	private void OnAnimatedChestFinished()
	{
		if (_animatedSprite == null)
		{
			_isOpening = false;
			return;
		}

		if (_animatedSprite.Animation != OpenAnimationName)
		{
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

	public override Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		Godot.Collections.Dictionary snapshot = base.BuildRuntimeSnapshot();
		snapshot["is_opened"] = _isOpened;
		return snapshot;
	}

	public override void ApplyRuntimeSnapshot(Godot.Collections.Dictionary snapshot)
	{
		base.ApplyRuntimeSnapshot(snapshot);

		if (snapshot.TryGetValue("is_opened", out Variant isOpened))
		{
			_isOpened = isOpened.AsBool();
		}

		_isOpening = false;
		if (_animatedSprite == null)
		{
			return;
		}

		if (_isOpened && _animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation(OpenAnimationName))
		{
			_animatedSprite.Play(OpenAnimationName);
			int frameCount = _animatedSprite.SpriteFrames.GetFrameCount(OpenAnimationName);
			_animatedSprite.Frame = Mathf.Max(0, frameCount - 1);
			_animatedSprite.Stop();
			return;
		}

		if (_animatedSprite.SpriteFrames != null && _animatedSprite.SpriteFrames.HasAnimation(ClosedAnimationName))
		{
			_animatedSprite.Play(ClosedAnimationName);
			_animatedSprite.Stop();
		}
	}
}
