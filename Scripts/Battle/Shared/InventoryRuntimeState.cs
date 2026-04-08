using Godot;

namespace CardChessDemo.Battle.Shared;

public sealed class InventoryRuntimeState
{
	public Godot.Collections.Dictionary ItemCounts { get; } = new();
}
