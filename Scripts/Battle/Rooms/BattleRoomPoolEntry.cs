using Godot;

namespace CardChessDemo.Battle.Rooms;

[GlobalClass]
public partial class BattleRoomPoolEntry : Resource
{
	[Export] public string EnemyTypeId { get; set; } = "default";
	[Export] public PackedScene[] RoomScenes { get; set; } = System.Array.Empty<PackedScene>();
}
