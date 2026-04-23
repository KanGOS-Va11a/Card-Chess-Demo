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

	protected override async void OnInteract(Player player)
	{
		if (_showingDialogue || DialoguePages.Count == 0)
		{
			base.OnInteract(player);
			return;
		}

		if (DialoguePanelScene == null)
		{
			base.OnInteract(player);
			return;
		}

		_showingDialogue = true;
		await MapDialogueService.PresentAsync(
			this,
			new MapDialogueRequest
			{
				PanelScene = DialoguePanelScene,
				Pages = DialoguePages.ToArray(),
				LockPlayerInput = true,
				RejectIfAnotherDialogueVisible = true,
				SourceId = BuildRuntimeStateKey(),
				CompletedFollowUpActions = new[]
				{
					new MapDialogueFollowUpAction
					{
						Kind = MapDialogueFollowUpKind.StartBattle,
						BattleEncounterId = EncounterId,
						BattleScene = BattleScene,
						BattleScenePath = BattleScenePath,
					},
				},
			},
			player);

		_showingDialogue = false;
	}
}
