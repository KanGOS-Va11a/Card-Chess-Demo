using Godot;

namespace CardChessDemo.Battle.Presentation;

public partial class BattleObstacleView : BattleAnimatedViewBase
{
    protected override SpriteFrames BuildFallbackFrames()
    {
        if (State?.DefinitionId == "battle_obstacle_wall")
        {
            return CreateFrames(new Color(0.42f, 0.44f, 0.48f), new Color(0.60f, 0.63f, 0.68f));
        }

        if (State?.DefinitionId == "battle_obstacle_slow")
        {
            return CreateFrames(new Color(0.62f, 0.48f, 0.26f), new Color(0.80f, 0.68f, 0.36f));
        }

        return CreateFrames(new Color(0.72f, 0.26f, 0.24f), new Color(0.92f, 0.58f, 0.44f));
    }
}
