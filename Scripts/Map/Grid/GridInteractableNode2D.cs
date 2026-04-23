using Godot;

namespace CardChessDemo.Map;

[Tool]
public partial class GridInteractableNode2D : GridPlacedNode2D
{
	[Export] public string InteractionId { get; set; } = string.Empty;

	public string ResolveInteractionId(string scenePath)
	{
		if (!string.IsNullOrWhiteSpace(InteractionId))
		{
			return InteractionId.Trim();
		}

		Vector2I cell = ResolveCell();
		string normalizedScenePath = string.IsNullOrWhiteSpace(scenePath) ? "scene" : scenePath.Trim();
		return $"{normalizedScenePath}::interactable::{cell.X},{cell.Y}";
	}

	protected override string BuildEditorLabelText(Vector2I cell)
	{
		return string.IsNullOrWhiteSpace(InteractionId)
			? base.BuildEditorLabelText(cell)
			: $"({cell.X},{cell.Y})\n{InteractionId}";
	}
}
