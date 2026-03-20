using Godot;

namespace CardChessDemo.Battle.Rooms;

[GlobalClass]
public partial class BattleRoomPoolDefinition : Resource
{
    [Export] public BattleRoomPoolEntry[] Entries { get; set; } = System.Array.Empty<BattleRoomPoolEntry>();
}
