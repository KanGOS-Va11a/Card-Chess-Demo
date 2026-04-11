using Godot;

namespace CardChessDemo.Battle.Presentation;

public partial class BattlePlayerView : BattleAnimatedViewBase
{
	private const string IdleDownSheetPath = "res://Assets/Character/MainPlayer/Idle/Idle_Down-export.png";
	private const string IdleDownFallbackSheetPath = "res://Assets/Character/MainPlayer/Idle/Idle_Down.png";
	private const string IdleUpSheetPath = "res://Assets/Character/MainPlayer/Idle/Idle_Up-export.png";
	private const string IdleUpFallbackSheetPath = "res://Assets/Character/MainPlayer/Idle/Idle_Up.png";
	private const string IdleRightSheetPath = "res://Assets/Character/MainPlayer/Idle/Idle_Right_Down-export.png";
	private const string IdleRightFallbackSheetPath = "res://Assets/Character/MainPlayer/Idle/Idle_Right_Down.png";
	private const string WalkDownSheetPath = "res://Assets/Character/MainPlayer/Walk/walk_Down-export.png";
	private const string WalkDownFallbackSheetPath = "res://Assets/Character/MainPlayer/Walk/walk_Down.png";
	private const string WalkUpSheetPath = "res://Assets/Character/MainPlayer/Walk/walk_Up-export.png";
	private const string WalkUpFallbackSheetPath = "res://Assets/Character/MainPlayer/Walk/walk_Up.png";
	private const string WalkRightSheetPath = "res://Assets/Character/MainPlayer/Walk/walk_Right_Down-export.png";
	private const string WalkRightFallbackSheetPath = "res://Assets/Character/MainPlayer/Walk/walk_Right_Down.png";

	[Export] public Texture2D? IdleSpriteSheet { get; set; }
	[Export] public Texture2D? IdleDownSpriteSheet { get; set; }
	[Export] public Texture2D? IdleUpSpriteSheet { get; set; }
	[Export] public Texture2D? WalkSpriteSheet { get; set; }
	[Export] public Texture2D? WalkDownSpriteSheet { get; set; }
	[Export] public Texture2D? WalkUpSpriteSheet { get; set; }
	[Export(PropertyHint.Range, "1,256,1")] public int FrameWidth { get; set; } = 48;
	[Export(PropertyHint.Range, "1,256,1")] public int FrameHeight { get; set; } = 64;
	[Export(PropertyHint.Range, "1,64,1")] public int FrameCount { get; set; } = 7;
	[Export(PropertyHint.Range, "0,32,1")] public int CellFootOffsetY { get; set; } = 8;
	[Export] public Vector2 SpriteDrawOffset { get; set; } = new(-24.0f, -48.0f);

	private string _directionKey = "down";

	public void PlayDefend()
	{
		PlayNamedAnimation("defend");
	}

	public void PlayCustom(StringName animationName)
	{
		PlayNamedAnimation(animationName.ToString());
	}

	public override void PlayIdle()
	{
		PlayCustom($"idle_{_directionKey}");
	}

	public override void PlayMove()
	{
		PlayCustom($"move_{_directionKey}");
	}

	public override void PlayAction()
	{
		PlayCustom("action");
	}

	public override void FaceDirection(Vector2 direction)
	{
		_directionKey = ResolveDirectionKey(direction);
		base.FaceDirection(direction);
	}

	protected override SpriteFrames BuildFallbackFrames()
	{
		if (ResolvePreferredTexture(IdleSpriteSheet, IdleRightSheetPath, IdleRightFallbackSheetPath) != null)
		{
			return BuildPlayerSheetFrames();
		}

		return CreateFrames(new Color(0.24f, 0.78f, 1.0f), new Color(0.8f, 0.96f, 1.0f));
	}

	protected override void ConfigureAnimatedSprite(AnimatedSprite2D sprite)
	{
		sprite.Centered = false;
		sprite.Position = SpriteDrawOffset;
	}

	protected override Vector2 GetBoardAnchorOffset()
	{
		return new Vector2(0.0f, CellFootOffsetY);
	}

	protected override int GetSourceArtFacingSign()
	{
		return 1;
	}

	protected override int GetDefaultFacingSign()
	{
		return 1;
	}

	private SpriteFrames BuildPlayerSheetFrames()
	{
		SpriteFrames frames = new();
		Texture2D? idleRight = ResolvePreferredTexture(IdleSpriteSheet, IdleRightSheetPath, IdleRightFallbackSheetPath);
		Texture2D? idleDown = ResolvePreferredTexture(IdleDownSpriteSheet, IdleDownSheetPath, IdleDownFallbackSheetPath);
		Texture2D? idleUp = ResolvePreferredTexture(IdleUpSpriteSheet, IdleUpSheetPath, IdleUpFallbackSheetPath);
		Texture2D? walkRight = ResolvePreferredTexture(WalkSpriteSheet, WalkRightSheetPath, WalkRightFallbackSheetPath);
		Texture2D? walkDown = ResolvePreferredTexture(WalkDownSpriteSheet, WalkDownSheetPath, WalkDownFallbackSheetPath);
		Texture2D? walkUp = ResolvePreferredTexture(WalkUpSpriteSheet, WalkUpSheetPath, WalkUpFallbackSheetPath);

		AddSheetAnimation(frames, "idle_down", idleDown ?? idleRight, 3.5d, true);
		AddSheetAnimation(frames, "idle_up", idleUp ?? idleRight, 3.5d, true);
		AddSheetAnimation(frames, "idle_right_down", idleRight, 7.0d, true);
		AddSheetAnimation(frames, "idle_left_down", idleRight, 7.0d, true);
		AddSheetAnimation(frames, "move_down", walkDown ?? idleDown ?? idleRight, 8.0d, true);
		AddSheetAnimation(frames, "move_up", walkUp ?? idleUp ?? idleRight, 8.0d, true);
		AddSheetAnimation(frames, "move_right_down", walkRight ?? idleRight, 8.0d, true);
		AddSheetAnimation(frames, "move_left_down", walkRight ?? idleRight, 8.0d, true);

		// Fallback generic clips for existing action flow.
		AddSheetAnimation(frames, "idle", idleDown ?? idleRight, 7.0d, true);
		AddSheetAnimation(frames, "move", walkRight ?? idleRight, 8.0d, true);
		AddSheetAnimation(frames, "action", idleRight, 9.0d, false);
		AddSheetAnimation(frames, "hit", idleRight, 10.0d, false, 0, 3);
		AddSheetAnimation(frames, "defend", idleRight, 8.0d, false, 0, 4);
		AddSheetAnimation(frames, "defeat", idleRight, 6.0d, false, 0, Mathf.Min(2, FrameCount));
		return frames;
	}

	private void AddSheetAnimation(SpriteFrames frames, string animationName, Texture2D? spriteSheet, double fps, bool loop, int startFrame = 0, int? frameLimit = null)
	{
		frames.AddAnimation(animationName);
		frames.SetAnimationSpeed(animationName, fps);
		frames.SetAnimationLoop(animationName, loop);

		if (spriteSheet == null || FrameCount <= 0)
		{
			return;
		}

		int lastFrameExclusive = Mathf.Clamp(frameLimit ?? FrameCount, 1, FrameCount);
		for (int frameIndex = Mathf.Clamp(startFrame, 0, FrameCount - 1); frameIndex < lastFrameExclusive; frameIndex++)
		{
			AtlasTexture atlas = new()
			{
				Atlas = spriteSheet,
				Region = new Rect2(frameIndex * FrameWidth, 0, FrameWidth, FrameHeight),
			};
			frames.AddFrame(animationName, atlas);
		}
	}

	private Texture2D? ResolvePreferredTexture(Texture2D? assigned, string preferredPath, string fallbackPath)
	{
		if (assigned != null)
		{
			return assigned;
		}

		if (ResourceLoader.Exists(preferredPath))
		{
			return ResourceLoader.Load<Texture2D>(preferredPath);
		}

		return ResourceLoader.Load<Texture2D>(fallbackPath);
	}

	private static string ResolveDirectionKey(Vector2 direction)
	{
		if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
		{
			return direction.X < 0.0f ? "left_down" : "right_down";
		}

		if (Mathf.Abs(direction.Y) < 0.001f)
		{
			return "down";
		}

		return direction.Y < 0.0f ? "up" : "down";
	}
}
