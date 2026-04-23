using Godot;

namespace CardChessDemo.Map;

[Tool]
public partial class GridInteractableAnchor : GridInteractableNode2D
{
	[Export] public NodePath TargetNodePath { get; set; } = new("");

	public override void _Ready()
	{
		base._Ready();
		if (TargetNodePath.IsEmpty)
		{
			return;
		}

		if (GetNodeOrNull(TargetNodePath) is not InteractableTemplate interactable)
		{
			return;
		}

		if (!string.IsNullOrWhiteSpace(InteractionId))
		{
			interactable.InteractionId = InteractionId.Trim();
		}
	}
}
