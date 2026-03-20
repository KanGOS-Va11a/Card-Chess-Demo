using Godot;

namespace CardChessDemo.Battle.Actors;

public partial class BattlePlayerToken : BattleActorTokenBase
{
    public override void _Ready()
    {
        AccentColor = new Color(0.24f, 0.78f, 1.0f, 1.0f);
        base._Ready();
    }
}
