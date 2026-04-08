using Godot;

namespace CardChessDemo.Battle.Boundary;

public sealed class MapResumeContext
{
    public MapResumeContext(
        string scenePath,
        Vector2 playerGlobalPosition,
        Godot.Collections.Dictionary? mapRuntimeSnapshot = null,
        string sourceInteractablePath = "",
        string encounterId = "")
    {
        ScenePath = scenePath ?? string.Empty;
        PlayerGlobalPosition = playerGlobalPosition;
        MapRuntimeSnapshot = mapRuntimeSnapshot ?? new Godot.Collections.Dictionary();
        SourceInteractablePath = sourceInteractablePath?.Trim() ?? string.Empty;
        EncounterId = encounterId?.Trim() ?? string.Empty;
    }

    public string ScenePath { get; }

    public Vector2 PlayerGlobalPosition { get; }

    public Godot.Collections.Dictionary MapRuntimeSnapshot { get; }

    public string SourceInteractablePath { get; }

    public string EncounterId { get; }
}
