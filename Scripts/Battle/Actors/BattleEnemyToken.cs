using Godot;

namespace CardChessDemo.Battle.Actors;

public partial class BattleEnemyToken : BattleActorTokenBase
{
    public override void _Ready()
    {
        AccentColor = new Color(1.0f, 0.38f, 0.32f, 1.0f);
        base._Ready();
    }
}
