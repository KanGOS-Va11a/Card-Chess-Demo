using Godot;

namespace CardChessDemo.Map;

public interface IConfigurableLootInteractable
{
	string DisplayName { get; set; }
	string PromptText { get; set; }
	string InteractableSessionKey { get; set; }
	string[] InteractionTexts { get; set; }
	Godot.Collections.Array<InteractableItemGrant> GrantedItems { get; set; }
}
