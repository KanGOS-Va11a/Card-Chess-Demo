using Godot;

namespace CardChessDemo.Battle.Presentation;

public partial class BattleBossRustCaptainView : BattleEnemyView
{
	private static readonly Texture2D BossSpriteSheet = GD.Load<Texture2D>("res://Assets/Character/Battle/Enemy/Boss/boss.png");

	public BattleBossRustCaptainView()
	{
		IdleSpriteSheet = BossSpriteSheet;
		FrameWidth = 48;
		FrameHeight = 48;
		FrameCount = 1;
		CellFootOffsetY = 7;
		SpriteDrawOffset = new Vector2(-12.0f, -22.0f);
	}

	protected override void ConfigureAnimatedSprite(AnimatedSprite2D sprite)
	{
		base.ConfigureAnimatedSprite(sprite);
		sprite.Scale = new Vector2(0.5f, 0.5f);
	}

	protected override Vector2 GetBoardAnchorOffset()
	{
		return base.GetBoardAnchorOffset() + new Vector2(0.0f, 1.0f);
	}
}
