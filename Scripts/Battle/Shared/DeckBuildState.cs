using System;

namespace CardChessDemo.Battle.Shared;

public sealed class DeckBuildState
{
	public string[] CardIds { get; set; } = Array.Empty<string>();

	public string[] RelicIds { get; set; } = Array.Empty<string>();
}
