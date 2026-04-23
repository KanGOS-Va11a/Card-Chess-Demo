using Godot;

namespace CardChessDemo.Map;

[Tool]
public partial class InteractableContainerConfig : GridInteractableNode2D
{
	[Export] public NodePath TargetNodePath { get; set; } = new("Chest");
	[Export] public string DisplayName { get; set; } = string.Empty;
	[Export] public string PromptText { get; set; } = string.Empty;
	[Export] public string InteractableSessionKey { get; set; } = string.Empty;
	[Export] public string[] InteractionTexts { get; set; } = System.Array.Empty<string>();
	[Export] public Godot.Collections.Array<InteractableItemGrant> GrantedItems { get; set; } = new();

	public override void _Ready()
	{
		base._Ready();
		if (GetNodeOrNull(TargetNodePath) is not IConfigurableLootInteractable interactable)
		{
			return;
		}

		if (!string.IsNullOrWhiteSpace(DisplayName))
		{
			interactable.DisplayName = DisplayName;
		}

		if (!string.IsNullOrWhiteSpace(PromptText))
		{
			interactable.PromptText = PromptText;
		}

		if (!string.IsNullOrWhiteSpace(InteractableSessionKey))
		{
			interactable.InteractableSessionKey = InteractableSessionKey;
		}
		else if (!string.IsNullOrWhiteSpace(InteractionId))
		{
			interactable.InteractableSessionKey = InteractionId.Trim();
		}

		if (InteractionTexts.Length > 0)
		{
			interactable.InteractionTexts = (string[])InteractionTexts.Clone();
		}

		if (GrantedItems.Count > 0)
		{
			Godot.Collections.Array<InteractableItemGrant> cloned = new();
			foreach (InteractableItemGrant? item in GrantedItems)
			{
				if (item != null)
				{
					cloned.Add(item);
				}
			}

			interactable.GrantedItems = cloned;
		}
	}
}
