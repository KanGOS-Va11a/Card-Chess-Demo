using Godot;

namespace CardChessDemo.Map;

[GlobalClass]
public partial class PlayerSpawnMarker : Node2D
{
	[Export] public string SpawnId { get; set; } = string.Empty;
	[Export] public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

	public Vector2 GetSpawnWorldPosition()
	{
		return GlobalPosition + SpawnOffset;
	}

	public string GetResolvedSpawnId()
	{
		return string.IsNullOrWhiteSpace(SpawnId) ? Name : SpawnId.Trim();
	}
}
