using Godot;
using System.Threading.Tasks;

namespace CardChessDemo.Map;

public static class MapSceneTransitionHelper
{
	private const string DefaultTransitionOverlayScenePath = "res://Scene/Transitions/MapBattleTransitionOverlay.tscn";

	public static bool TryChangeSceneWithDissolve(
		Node contextNode,
		PackedScene? nextScene,
		string nextScenePath,
		float dissolveOutSeconds,
		float holdSeconds,
		float restoreSeconds,
		out string failureReason,
		System.Action<string>? deferredFailureCallback = null)
	{
		failureReason = string.Empty;
		if (nextScene == null && string.IsNullOrWhiteSpace(nextScenePath))
		{
			failureReason = "Target scene is not configured.";
			return false;
		}

		_ = ExecuteSceneTransitionAsync(
			contextNode,
			nextScene,
			nextScenePath,
			dissolveOutSeconds,
			holdSeconds,
			restoreSeconds,
			deferredFailureCallback);
		return true;
	}

	private static async Task ExecuteSceneTransitionAsync(
		Node contextNode,
		PackedScene? nextScene,
		string nextScenePath,
		float dissolveOutSeconds,
		float holdSeconds,
		float restoreSeconds,
		System.Action<string>? deferredFailureCallback)
	{
		if (!GodotObject.IsInstanceValid(contextNode))
		{
			deferredFailureCallback?.Invoke("Scene transition context node is no longer valid.");
			return;
		}

		SceneTree tree = contextNode.GetTree();
		MapBattleTransitionOverlay? overlay = await SpawnTransitionOverlayAsync(tree);
		if (overlay != null)
		{
			overlay.TransitionDurationSeconds = Mathf.Max(0.01f, dissolveOutSeconds);
			overlay.RevealDurationSeconds = Mathf.Max(0.01f, restoreSeconds);
			overlay.EndHoldSeconds = 0.0f;
			await overlay.PlayDissolveOutAsync();
		}

		if (holdSeconds > 0.0f)
		{
			await contextNode.ToSignal(tree.CreateTimer(holdSeconds, false, false, true), SceneTreeTimer.SignalName.Timeout);
		}

		Error result = nextScene != null
			? tree.ChangeSceneToPacked(nextScene)
			: tree.ChangeSceneToFile(nextScenePath.Trim());

		if (result != Error.Ok)
		{
			overlay?.QueueFree();
			string reason = $"Scene change failed, error={result}.";
			deferredFailureCallback?.Invoke(reason);
			GD.PushError($"MapSceneTransitionHelper: {reason}");
			return;
		}

		if (overlay == null)
		{
			return;
		}

		await overlay.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		await overlay.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		await overlay.PlayRestoreAsync();
		overlay.QueueFree();
	}

	private static async Task<MapBattleTransitionOverlay?> SpawnTransitionOverlayAsync(SceneTree tree)
	{
		PackedScene? overlayScene = GD.Load<PackedScene>(DefaultTransitionOverlayScenePath);
		if (overlayScene == null)
		{
			GD.PushWarning($"MapSceneTransitionHelper: failed to load transition overlay scene at {DefaultTransitionOverlayScenePath}.");
			return null;
		}

		MapBattleTransitionOverlay? overlay = overlayScene.Instantiate<MapBattleTransitionOverlay>();
		if (overlay == null)
		{
			GD.PushWarning("MapSceneTransitionHelper: failed to instantiate transition overlay.");
			return null;
		}

		tree.Root.AddChild(overlay);
		await overlay.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
		return overlay;
	}
}