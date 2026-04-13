using CardChessDemo.Audio;
using CardChessDemo.Battle.Shared;
using Godot;

namespace CardChessDemo.Map;

public partial class HealStation : InteractableTemplate
{
	[Export] public string HealText { get; set; } = "生命与荒川能量已经回满。";
	[Export] public string AlreadyFullText { get; set; } = "当前状态已经是满的。";

	private GlobalGameSession? _session;
	private AnimatedSprite2D? _animatedSprite;
	private static readonly string[] HealFramePaths =
	{
		"res://ArtResource/resource/道具/回血站/回血站1.png",
		"res://ArtResource/resource/道具/回血站/回血站2.png",
		"res://ArtResource/resource/道具/回血站/回血站3.png",
		"res://ArtResource/resource/道具/回血站/回血站4.png",
		"res://ArtResource/resource/道具/回血站/回血站5.png",
		"res://ArtResource/resource/道具/回血站/回血站6.png",
		"res://ArtResource/resource/道具/回血站/回血站7.png",
		"res://ArtResource/resource/道具/回血站/回血站8.png",
	};

	public override void _Ready()
	{
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		SetupAnimationFrames();
		if (Mathf.IsZeroApprox(CooldownSeconds))
		{
			CooldownSeconds = 1.5f;
		}
	}

	public override string GetInteractText(Player player)
	{
		return CanInteract(player)
			? string.IsNullOrWhiteSpace(PromptText) ? "恢复状态" : PromptText
			: "冷却中";
	}

	protected override void OnInteract(Player player)
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
		foreach (string framePath in HealFramePaths)
		{
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
}
