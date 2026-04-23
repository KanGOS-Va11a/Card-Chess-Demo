using CardChessDemo.Audio;
using CardChessDemo.Battle.Shared;
using CardChessDemo.UI;
using Godot;

namespace CardChessDemo.Map;

public partial class HealStation : InteractableTemplate
{
	[Export] public string ChoiceTitleText { get; set; } = "\u6CBB\u7597\u7AD9";
	[Export] public string SaveSuccessText { get; set; } = "\u5DF2\u8BB0\u5F55\u5230\u6700\u8FD1\u5B58\u6863\u70B9\u3002";
	[Export] public string SaveFailureText { get; set; } = "\u5B58\u6863\u5931\u8D25\u3002";
	[Export] public string HealText { get; set; } = "\u751F\u547D\u4E0E\u8352\u5DDD\u80FD\u91CF\u5DF2\u56DE\u6EE1\u3002";
	[Export] public string AlreadyFullText { get; set; } = "\u5F53\u524D\u72B6\u6001\u5DF2\u7ECF\u662F\u6EE1\u7684\u3002";

	private GlobalGameSession? _session;
	private AnimatedSprite2D? _animatedSprite;
	private bool _choiceOpen;

	public override void _Ready()
	{
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		SetupAnimationFrames();
	}

	public override string GetInteractText(Player player)
	{
		if (_choiceOpen)
		{
			return string.Empty;
		}

		return CanInteract(player) ? "\u5B58\u6863 / \u6CBB\u7597" : "\u51B7\u5374\u4E2D";
	}

	public override bool CanInteract(Player player)
	{
		return !_choiceOpen && base.CanInteract(player);
	}

	protected override async void OnInteract(Player player)
	{
		if (_session == null)
		{
			SceneTextOverlay.Show(this, SaveFailureText);
			return;
		}

		ChoiceOverlay overlay = new();
		Node currentScene = GetTree().CurrentScene ?? this;
		currentScene.AddChild(overlay);
		_choiceOpen = true;
		SetPlayerInputEnabled(player, false);

		int choice = await overlay.PresentAsync(
			ChoiceTitleText,
			"\u5B58\u6863",
			"\u6CBB\u7597");

		_choiceOpen = false;
		SetPlayerInputEnabled(player, true);

		switch (choice)
		{
			case 0:
				HandleSaveChoice(player);
				break;
			case 1:
				HandleHealChoice();
				break;
			default:
				_nextAvailableTimeMs = 0;
				break;
		}
	}

	private void HandleSaveChoice(Player player)
	{
		string scenePath = GetTree().CurrentScene?.SceneFilePath ?? string.Empty;
		Godot.Collections.Dictionary sceneRuntimeSnapshot = MapRuntimeSnapshotHelper.CaptureFromScene(GetTree().CurrentScene);
		if (_session == null)
		{
			SceneTextOverlay.Show(this, SaveFailureText);
			return;
		}

		if (!_session.TrySavePrimaryCheckpoint(scenePath, player.GlobalPosition, sceneRuntimeSnapshot, out string failureReason))
		{
			GD.PushError($"HealStation: save failed. {failureReason}");
			SceneTextOverlay.Show(this, SaveFailureText);
			return;
		}

		SceneTextOverlay.Show(this, SaveSuccessText);
		PlayInteractionPulse();
		GameAudio.Instance?.PlayUiConfirm();
	}

	private void HandleHealChoice()
	{
		bool restored = RestorePartyState();
		SceneTextOverlay.Show(this, restored ? HealText : AlreadyFullText);
		PlayInteractionPulse();
		_animatedSprite?.Play("idle");
		GameAudio.Instance?.PlayHealing();
	}

	private void SetupAnimationFrames()
	{
		if (_animatedSprite == null)
		{
			GD.PushWarning("HealStation: AnimatedSprite2D not found.");
			return;
		}

		SpriteFrames frames = new();
		frames.AddAnimation("idle");
		frames.SetAnimationLoop("idle", true);
		frames.SetAnimationSpeed("idle", 8.0f);
		for (int frameIndex = 1; frameIndex <= 8; frameIndex++)
		{
			string framePath = $"res://ArtResource/resource/\u9053\u5177/\u56DE\u8840\u7AD9/\u56DE\u8840\u7AD9{frameIndex}.png";
			if (GD.Load<Texture2D>(framePath) is not Texture2D texture)
			{
				continue;
			}

			frames.AddFrame("idle", texture);
		}

		if (frames.GetFrameCount("idle") == 0)
		{
			GD.PushWarning("HealStation: no heal station frames were loaded.");
			return;
		}

		_animatedSprite.SpriteFrames = frames;
		_animatedSprite.Animation = "idle";
		_animatedSprite.Play("idle");
	}

	private bool RestorePartyState()
	{
		if (_session == null)
		{
			return false;
		}

		int targetHp = _session.GetResolvedPlayerMaxHp();
		int targetEnergy = _session.ArakawaMaxEnergy;
		bool changed = _session.PlayerCurrentHp != targetHp || _session.ArakawaCurrentEnergy != targetEnergy;
		_session.SetPlayerCurrentHp(targetHp);
		_session.SetArakawaCurrentEnergy(targetEnergy);
		return changed;
	}

	private static void SetPlayerInputEnabled(Player player, bool enabled)
	{
		if (!GodotObject.IsInstanceValid(player))
		{
			return;
		}

		player.SetPhysicsProcess(enabled);
		player.SetProcess(enabled);
		player.SetProcessInput(enabled);
		player.SetProcessUnhandledInput(enabled);
	}
}
