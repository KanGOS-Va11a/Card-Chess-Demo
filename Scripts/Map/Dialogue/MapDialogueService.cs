using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardChessDemo.Battle.Shared;
using CardChessDemo.UI.Dialogue;
using Godot;

namespace CardChessDemo.Map;

public static class MapDialogueService
{
	private static int _activeBlockingDialogueCount;

	private readonly struct PlayerProcessState
	{
		public PlayerProcessState(bool physics, bool process, bool input, bool unhandledInput)
		{
			Physics = physics;
			Process = process;
			Input = input;
			UnhandledInput = unhandledInput;
		}

		public bool Physics { get; }
		public bool Process { get; }
		public bool Input { get; }
		public bool UnhandledInput { get; }
	}

	public static bool IsDialogueVisible(Node? context)
	{
		return _activeBlockingDialogueCount > 0 || DialogueSequencePanel.IsVisible(context);
	}

	public static bool HasBlockingDialogue()
	{
		return _activeBlockingDialogueCount > 0;
	}

	public static async Task<MapDialogueResult> PresentAsync(Node context, MapDialogueRequest request, Player? player = null)
	{
		if (context == null)
		{
			return new MapDialogueResult(MapDialogueCompletionStatus.Failed, 0, "context_missing");
		}

		if (request == null)
		{
			return new MapDialogueResult(MapDialogueCompletionStatus.Failed, 0, "request_missing");
		}

		DialoguePage[] pages = request.Pages ?? Array.Empty<DialoguePage>();
		if (pages.Length == 0)
		{
			return new MapDialogueResult(MapDialogueCompletionStatus.Failed, 0, "pages_missing");
		}

		if (request.RejectIfAnotherDialogueVisible && DialogueSequencePanel.IsVisible(context))
		{
			return new MapDialogueResult(MapDialogueCompletionStatus.Blocked, pages.Length, "dialogue_already_visible");
		}

		Node? currentScene = context.GetTree()?.CurrentScene;
		if (currentScene == null)
		{
			return new MapDialogueResult(MapDialogueCompletionStatus.Failed, pages.Length, "scene_missing");
		}

		if (request.PanelScene?.Instantiate() is not DialogueSequencePanel panel)
		{
			return new MapDialogueResult(MapDialogueCompletionStatus.Failed, pages.Length, "panel_instantiate_failed");
		}

		player ??= ResolveCurrentScenePlayer(currentScene);
		PlayerProcessState? previousPlayerState = null;
		bool registeredAsBlocking = false;
		if (request.LockPlayerInput && player != null)
		{
			previousPlayerState = CapturePlayerProcessState(player);
			ApplyPlayerInputEnabled(player, false);
			_activeBlockingDialogueCount++;
			registeredAsBlocking = true;
		}

		TaskCompletionSource<MapDialogueResult> completionSource = new();

		void Complete(MapDialogueResult result)
		{
			if (previousPlayerState.HasValue && player != null)
			{
				ApplyPlayerProcessState(player, previousPlayerState.Value);
			}

			if (registeredAsBlocking && _activeBlockingDialogueCount > 0)
			{
				_activeBlockingDialogueCount--;
				registeredAsBlocking = false;
			}

			MapDialogueFollowUpResult? followUpResult = null;
			if (result.Status == MapDialogueCompletionStatus.Completed && request.CompletedFollowUpActions.Length > 0)
			{
				followUpResult = ExecuteFollowUpActions(context, player, request.CompletedFollowUpActions);
			}
			else if (result.Status == MapDialogueCompletionStatus.Closed && request.ClosedFollowUpActions.Length > 0)
			{
				followUpResult = ExecuteFollowUpActions(context, player, request.ClosedFollowUpActions);
			}

			completionSource.TrySetResult(new MapDialogueResult(result.Status, result.PageCount, result.FailureReason, followUpResult));
		}

		currentScene.AddChild(panel);
		panel.Present(
			pages,
			onCompleted: () => Complete(new MapDialogueResult(MapDialogueCompletionStatus.Completed, pages.Length)),
			onClosed: () => Complete(new MapDialogueResult(MapDialogueCompletionStatus.Closed, pages.Length)));

		return await completionSource.Task;
	}

	private static Player? ResolveCurrentScenePlayer(Node currentScene)
	{
		return currentScene.FindChild("Player", true, false) as Player;
	}

	public static MapDialogueFollowUpResult ExecuteFollowUpActions(Node context, Player? player, params MapDialogueFollowUpAction[] actions)
	{
		if (actions == null || actions.Length == 0)
		{
			return new MapDialogueFollowUpResult(false, true);
		}

		bool executedAny = false;
		foreach (MapDialogueFollowUpAction? action in actions)
		{
			if (action == null || action.Kind == MapDialogueFollowUpKind.None)
			{
				continue;
			}

			executedAny = true;
			if (!TryExecuteFollowUpAction(context, player, action, out string failureReason))
			{
				return new MapDialogueFollowUpResult(true, false, failureReason);
			}
		}

		return new MapDialogueFollowUpResult(executedAny, true);
	}

	private static bool TryExecuteFollowUpAction(Node context, Player? player, MapDialogueFollowUpAction action, out string failureReason)
	{
		failureReason = string.Empty;
		switch (action.Kind)
		{
			case MapDialogueFollowUpKind.None:
				return true;
			case MapDialogueFollowUpKind.TriggerInteractable:
				return TryTriggerInteractable(context, player, action, out failureReason);
			case MapDialogueFollowUpKind.StartBattle:
				return TryStartBattle(context, player, action, out failureReason);
			case MapDialogueFollowUpKind.ChangeScene:
				return TryChangeScene(context, action, out failureReason);
			default:
				failureReason = $"unsupported_follow_up_kind:{action.Kind}";
				return false;
		}
	}

	private static bool TryTriggerInteractable(Node context, Player? player, MapDialogueFollowUpAction action, out string failureReason)
	{
		failureReason = string.Empty;
		if (player == null)
		{
			failureReason = "player_missing";
			return false;
		}

		if (!TryResolveNodePath(context, action.TargetNodePath, out Node? targetNode))
		{
			failureReason = $"trigger_target_missing:{action.TargetNodePath}";
			return false;
		}

		if (targetNode is Enemy enemy)
		{
			if (!enemy.TriggerEncounterDirect(player))
			{
				failureReason = "enemy_trigger_failed";
				return false;
			}

			return true;
		}

		if (targetNode is BattleEncounterEnemy battleEncounterEnemy)
		{
			if (!battleEncounterEnemy.TriggerEncounterDirect(player))
			{
				failureReason = "battle_enemy_trigger_failed";
				return false;
			}

			return true;
		}

		if (targetNode is InteractableTemplate interactable)
		{
			interactable.Interact(player);
			return true;
		}

		failureReason = $"unsupported_trigger_target:{targetNode.GetType().Name}";
		return false;
	}

	private static bool TryStartBattle(Node context, Player? player, MapDialogueFollowUpAction action, out string failureReason)
	{
		failureReason = string.Empty;
		if (player == null)
		{
			failureReason = "player_missing";
			return false;
		}

		if (string.IsNullOrWhiteSpace(action.BattleEncounterId))
		{
			failureReason = "battle_encounter_missing";
			return false;
		}

		return MapBattleTransitionHelper.TryEnterBattle(
			context,
			player,
			action.BattleScene,
			action.BattleScenePath,
			action.BattleEncounterId.Trim(),
			out failureReason);
	}

	private static bool TryChangeScene(Node context, MapDialogueFollowUpAction action, out string failureReason)
	{
		failureReason = string.Empty;
		if (string.IsNullOrWhiteSpace(action.NextScenePath))
		{
			failureReason = "next_scene_missing";
			return false;
		}

		GlobalGameSession? session = context.GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		session?.ClearPendingBattleReturnContext();
		session?.ConsumeLastBattleResult();
		session?.ClearPendingRestorePlayerPosition();
		session?.SetPendingSceneTransfer(action.NextScenePath.Trim(), action.NextSceneSpawnId);

		if (!MapSceneTransitionHelper.TryChangeSceneWithDissolve(
			context,
			null,
			action.NextScenePath,
			0.22f,
			0.05f,
			0.22f,
			out string transitionFailureReason,
			reason => GD.PushError($"MapDialogueService: {reason}")))
		{
			failureReason = transitionFailureReason;
			return false;
		}

		return true;
	}

	private static bool TryResolveNodePath(Node context, NodePath path, out Node? targetNode)
	{
		targetNode = null;
		if (path.IsEmpty)
		{
			return false;
		}

		Node? currentScene = context.GetTree()?.CurrentScene;
		if (currentScene != null)
		{
			targetNode = currentScene.GetNodeOrNull(path);
		}

		targetNode ??= context.GetNodeOrNull(path);
		if (targetNode != null)
		{
			return true;
		}

		string pathText = path.ToString();
		if (pathText.StartsWith("../", StringComparison.Ordinal) && context.GetParent() != null)
		{
			targetNode = context.GetParent()?.GetNodeOrNull(pathText[3..]);
		}

		return targetNode != null;
	}

	private static PlayerProcessState CapturePlayerProcessState(Player player)
	{
		return new PlayerProcessState(
			player.IsPhysicsProcessing(),
			player.IsProcessing(),
			player.IsProcessingInput(),
			player.IsProcessingUnhandledInput());
	}

	private static void ApplyPlayerProcessState(Player player, PlayerProcessState state)
	{
		if (!GodotObject.IsInstanceValid(player))
		{
			return;
		}

		player.SetPhysicsProcess(state.Physics);
		player.SetProcess(state.Process);
		player.SetProcessInput(state.Input);
		player.SetProcessUnhandledInput(state.UnhandledInput);
	}

	private static void ApplyPlayerInputEnabled(Player player, bool enabled)
	{
		if (!GodotObject.IsInstanceValid(player))
		{
			return;
		}

		player.SetPhysicsProcess(enabled);
		player.SetProcess(enabled);
		player.SetProcessInput(enabled);
		player.SetProcessUnhandledInput(enabled);
	}
}
