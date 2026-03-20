using Godot;

namespace CardChessDemo.Battle.Actors;

public partial class BattleObstacleToken : BattleActorTokenBase
{
    public override void _Ready()
    {
        AccentColor = new Color(0.75f, 0.74f, 0.67f, 1.0f);
        FloatAmplitude = 0.0f;
        base._Ready();
    }
}
