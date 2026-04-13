using System.Linq;
using CardChessDemo.UI.Dialogue;
using Godot;

namespace CardChessDemo.Map;

public partial class StoryDialogueEnemy : Enemy
{
	[Export] public PackedScene? DialoguePanelScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/DialogueSequencePanel.tscn");
	[Export] public Godot.Collections.Array<DialoguePage> DialoguePages { get; set; } = new();

	private bool _showingDialogue;

	public override string GetInteractText(Player player)
	{
		if (_showingDialogue)
		{
			return "对话中...";
		}

		return base.GetInteractText(player);
	}

	public override bool CanInteract(Player player)
	{
		return !_showingDialogue && base.CanInteract(player);
	}

	protected override void OnInteract(Player player)
	{
		if (_showingDialogue || DialoguePages.Count == 0)
		{
			base.OnInteract(player);
			return;
		}

		if (DialoguePanelScene?.Instantiate() is not DialogueSequencePanel panel)
		{
			base.OnInteract(player);
			return;
		}

		_showingDialogue = true;
		(GetTree().CurrentScene ?? this).AddChild(panel);
		panel.Present(
			DialoguePages.ToArray(),
			onCompleted: () =>
			{
				_showingDialogue = false;
				base.OnInteract(player);
			},
			onClosed: () =>
			{
				_showingDialogue = false;
			});
	}
}
