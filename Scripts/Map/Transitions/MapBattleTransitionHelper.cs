using System;
using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;
using CardChessDemo.Audio;
using System.Threading.Tasks;

namespace CardChessDemo.Map;

public static class MapBattleTransitionHelper
{
    private const string DefaultTransitionOverlayScenePath = "res://Scene/Transitions/MapBattleTransitionOverlay.tscn";

    public static bool TryEnterBattle(
        Node contextNode,
        Player player,
        PackedScene? battleScene,
        string battleScenePath,
        string battleEncounterId,
        out string failureReason,
        System.Action<string>? deferredFailureCallback = null)
    {
        failureReason = string.Empty;

        if (battleScene == null && string.IsNullOrWhiteSpace(battleScenePath))
        {
            failureReason = "Battle scene is not configured.";
            return false;
        }

        GlobalGameSession? globalSession = contextNode.GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
        if (globalSession == null)
        {
            failureReason = "GlobalGameSession is missing.";
            return false;
        }

        Node? currentScene = contextNode.GetTree().CurrentScene;
        string currentScenePath = ResolveCurrentScenePath(contextNode, currentScene);
        if (string.IsNullOrWhiteSpace(currentScenePath))
        {
            failureReason = "Current map scene path is empty, cannot resume from battle.";
            return false;
        }

        Godot.Collections.Dictionary mapRuntimeSnapshot = MapRuntimeSnapshotHelper.CaptureFromScene(currentScene);
        string sourceInteractablePath = currentScene != null && currentScene.IsAncestorOf(contextNode)
            ? currentScene.GetPathTo(contextNode).ToString()
            : string.Empty;
        int battleSeed = BuildBattleSeed(battleEncounterId);
        globalSession.BeginBattle(BattleRequest.FromSession(globalSession, battleEncounterId, battleSeed));
        globalSession.SetPendingBattleEncounterId(battleEncounterId);
        globalSession.SetPendingMapResumeContext(new MapResumeContext(
            currentScenePath,
            player.GlobalPosition,
            mapRuntimeSnapshot,
            sourceInteractablePath,
            battleEncounterId));

        _ = ExecuteBattleTransitionAsync(contextNode, battleScene, battleScenePath, globalSession, deferredFailureCallback);
        return true;
    }

    private static int BuildBattleSeed(string battleEncounterId)
    {
        int timeSeed = unchecked((int)(Time.GetTicksUsec() & 0x7fffffff));
        int encounterHash = string.IsNullOrWhiteSpace(battleEncounterId)
            ? 0
            : StringComparer.Ordinal.GetHashCode(battleEncounterId);
        int combined = timeSeed ^ encounterHash;
        return Math.Max(1, combined & 0x7fffffff);
    }

    private static async Task ExecuteBattleTransitionAsync(
        Node contextNode,
        PackedScene? battleScene,
        string battleScenePath,
        GlobalGameSession globalSession,
        System.Action<string>? deferredFailureCallback)
    {
        if (!GodotObject.IsInstanceValid(contextNode))
        {
            globalSession.CancelPendingBattleTransition();
            deferredFailureCallback?.Invoke("Battle transition context node is no longer valid.");
            return;
        }

        SceneTree tree = contextNode.GetTree();
        GameAudio.Instance?.StopMusic(0.22f);
        MapBattleTransitionOverlay? overlay = await SpawnTransitionOverlayAsync(tree);
        if (overlay != null)
        {
            await overlay.PlayAsync();
            overlay.QueueFree();
        }

        Error result = battleScene != null
            ? tree.ChangeSceneToPacked(battleScene)
            : tree.ChangeSceneToFile(battleScenePath.Trim());

        if (result != Error.Ok)
        {
            string failureReason = $"Scene change failed, error={result}.";
            globalSession.CancelPendingBattleTransition();
            deferredFailureCallback?.Invoke(failureReason);
            GD.PushError($"MapBattleTransitionHelper: {failureReason}");
        }
    }

    private static async Task<MapBattleTransitionOverlay?> SpawnTransitionOverlayAsync(SceneTree tree)
    {
        PackedScene? overlayScene = GD.Load<PackedScene>(DefaultTransitionOverlayScenePath);
        if (overlayScene == null)
        {
            GD.PushWarning($"MapBattleTransitionHelper: failed to load transition overlay scene at {DefaultTransitionOverlayScenePath}.");
            return null;
        }

        MapBattleTransitionOverlay? overlay = overlayScene.Instantiate<MapBattleTransitionOverlay>();
        if (overlay == null)
        {
            GD.PushWarning("MapBattleTransitionHelper: failed to instantiate transition overlay.");
            return null;
        }

        tree.Root.AddChild(overlay);
        await overlay.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
        return overlay;
    }

    private static string ResolveCurrentScenePath(Node contextNode, Node? currentScene)
    {
        if (currentScene != null && !string.IsNullOrWhiteSpace(currentScene.SceneFilePath))
        {
            return currentScene.SceneFilePath;
        }

        Node? owner = contextNode;
        while (owner != null)
        {
            if (!string.IsNullOrWhiteSpace(owner.SceneFilePath))
            {
                return owner.SceneFilePath;
            }

            owner = owner.Owner;
        }

        return string.Empty;
    }
}
