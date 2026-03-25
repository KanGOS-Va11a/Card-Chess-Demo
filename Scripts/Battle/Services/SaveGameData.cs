using Godot;

namespace CardChessDemo.Battle.Services;

public sealed class SaveGameData
{
	public int Version { get; set; } = 1;

	public string SessionId { get; set; } = string.Empty;

	public Godot.Collections.Dictionary PlayerSnapshot { get; set; } = new();

	public Godot.Collections.Dictionary CompanionSnapshot { get; set; } = new();

	public Godot.Collections.Dictionary ProgressionSnapshot { get; set; } = new();

	public Godot.Collections.Dictionary DeckBuildSnapshot { get; set; } = new();

	public Godot.Collections.Dictionary InventorySnapshot { get; set; } = new();

	public Godot.Collections.Dictionary SaveRuntimeSnapshot { get; set; } = new();
}
