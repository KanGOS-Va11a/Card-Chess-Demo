using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Map;

public static class MapRuntimeSnapshotHelper
{
    public const string NodePathIndexKey = "__node_path_index";

    public static Godot.Collections.Dictionary CaptureFromScene(Node? sceneRoot)
    {
        Godot.Collections.Dictionary snapshot = new();
        if (sceneRoot == null)
        {
            return snapshot;
        }

        Godot.Collections.Dictionary pathIndex = new();
        CaptureRecursive(sceneRoot, sceneRoot, snapshot, pathIndex);
        if (pathIndex.Count > 0)
        {
            snapshot[NodePathIndexKey] = pathIndex;
        }

        return snapshot;
    }

    public static void ApplyToScene(Node? sceneRoot, Godot.Collections.Dictionary? snapshot)
    {
        if (sceneRoot == null || snapshot == null || snapshot.Count == 0)
        {
            return;
        }

        Dictionary<string, InteractableTemplate> interactableLookup = BuildInteractableLookup(sceneRoot);

        foreach (Variant key in snapshot.Keys)
        {
            string stateKey = key.AsString();
            if (string.IsNullOrWhiteSpace(stateKey) || IsReservedSnapshotKey(stateKey))
            {
                continue;
            }

            InteractableTemplate? interactable = ResolveInteractable(sceneRoot, stateKey, interactableLookup);
            if (interactable == null)
            {
                continue;
            }

            if (snapshot[key].Obj is not Godot.Collections.Dictionary interactableSnapshot)
            {
                continue;
            }

            interactable.ApplyRuntimeSnapshot(interactableSnapshot);
        }
    }

    public static InteractableTemplate? ResolveInteractable(Node? sceneRoot, string stateKeyOrPath)
    {
        if (sceneRoot == null || string.IsNullOrWhiteSpace(stateKeyOrPath))
        {
            return null;
        }

        Dictionary<string, InteractableTemplate> interactableLookup = BuildInteractableLookup(sceneRoot);
        return ResolveInteractable(sceneRoot, stateKeyOrPath, interactableLookup);
    }

    public static string ResolveSnapshotKey(Godot.Collections.Dictionary? snapshot, string stateKeyOrPath)
    {
        if (snapshot == null || string.IsNullOrWhiteSpace(stateKeyOrPath))
        {
            return string.Empty;
        }

        if (snapshot.TryGetValue(NodePathIndexKey, out Variant pathIndexVariant)
            && pathIndexVariant.Obj is Godot.Collections.Dictionary pathIndex
            && pathIndex.TryGetValue(stateKeyOrPath, out Variant mappedVariant))
        {
            string mappedKey = mappedVariant.AsString();
            if (!string.IsNullOrWhiteSpace(mappedKey))
            {
                return mappedKey;
            }
        }

        return stateKeyOrPath;
    }

    public static void UpsertInteractableSnapshot(Node? sceneRoot, InteractableTemplate? interactable, Godot.Collections.Dictionary? snapshot)
    {
        if (sceneRoot == null || interactable == null || snapshot == null)
        {
            return;
        }

        string stateKey = interactable.BuildRuntimeStateKey(sceneRoot);
        if (string.IsNullOrWhiteSpace(stateKey))
        {
            return;
        }

        snapshot[stateKey] = interactable.BuildRuntimeSnapshot();

        string relativePath = sceneRoot.IsAncestorOf(interactable)
            ? sceneRoot.GetPathTo(interactable).ToString()
            : string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return;
        }

        Godot.Collections.Dictionary pathIndex;
        if (snapshot.TryGetValue(NodePathIndexKey, out Variant pathIndexVariant)
            && pathIndexVariant.Obj is Godot.Collections.Dictionary existingIndex)
        {
            pathIndex = existingIndex;
        }
        else
        {
            pathIndex = new Godot.Collections.Dictionary();
            snapshot[NodePathIndexKey] = pathIndex;
        }

        pathIndex[relativePath] = stateKey;
    }

    private static void CaptureRecursive(Node current, Node sceneRoot, Godot.Collections.Dictionary snapshot, Godot.Collections.Dictionary pathIndex)
    {
        if (current is InteractableTemplate interactable)
        {
            string stateKey = interactable.BuildRuntimeStateKey(sceneRoot);
            if (!string.IsNullOrWhiteSpace(stateKey))
            {
                snapshot[stateKey] = interactable.BuildRuntimeSnapshot();
            }

            string relativePath = sceneRoot.IsAncestorOf(interactable)
                ? sceneRoot.GetPathTo(interactable).ToString()
                : string.Empty;
            if (!string.IsNullOrWhiteSpace(relativePath) && !string.IsNullOrWhiteSpace(stateKey))
            {
                pathIndex[relativePath] = stateKey;
            }
        }

        foreach (Node child in current.GetChildren())
        {
            CaptureRecursive(child, sceneRoot, snapshot, pathIndex);
        }
    }

    private static Dictionary<string, InteractableTemplate> BuildInteractableLookup(Node sceneRoot)
    {
        Dictionary<string, InteractableTemplate> lookup = new(System.StringComparer.Ordinal);
        BuildInteractableLookupRecursive(sceneRoot, sceneRoot, lookup);
        return lookup;
    }

    private static void BuildInteractableLookupRecursive(Node current, Node sceneRoot, Dictionary<string, InteractableTemplate> lookup)
    {
        if (current is InteractableTemplate interactable)
        {
            string stateKey = interactable.BuildRuntimeStateKey(sceneRoot);
            if (!string.IsNullOrWhiteSpace(stateKey))
            {
                lookup[stateKey] = interactable;
            }
        }

        foreach (Node child in current.GetChildren())
        {
            BuildInteractableLookupRecursive(child, sceneRoot, lookup);
        }
    }

    private static InteractableTemplate? ResolveInteractable(Node sceneRoot, string stateKeyOrPath, Dictionary<string, InteractableTemplate> interactableLookup)
    {
        if (interactableLookup.TryGetValue(stateKeyOrPath, out InteractableTemplate? interactable) && interactable != null)
        {
            return interactable;
        }

        return sceneRoot.GetNodeOrNull<InteractableTemplate>(stateKeyOrPath);
    }

    private static bool IsReservedSnapshotKey(string key)
    {
        return string.Equals(key, NodePathIndexKey, System.StringComparison.Ordinal);
    }
}
