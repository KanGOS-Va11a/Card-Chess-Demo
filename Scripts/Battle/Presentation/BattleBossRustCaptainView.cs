using Godot;

namespace CardChessDemo.Battle.Presentation;

public partial class BattleBossRustCaptainView : BattleEnemyView
{
    protected override void ConfigureAnimatedSprite(AnimatedSprite2D sprite)
    {
        base.ConfigureAnimatedSprite(sprite);
        sprite.Scale = new Vector2(1.6f, 1.6f);
        sprite.Position += new Vector2(-2.0f, -4.0f);
    }

    protected override Vector2 GetBoardAnchorOffset()
    {
        return base.GetBoardAnchorOffset() + new Vector2(0.0f, 2.0f);
    }
}
