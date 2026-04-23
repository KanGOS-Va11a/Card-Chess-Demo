using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Map;

public static class MapRuntimeSnapshotHelper
{
    public static Godot.Collections.Dictionary CaptureFromScene(Node? sceneRoot)
    {
        Godot.Collections.Dictionary snapshot = new();
        if (sceneRoot == null)
        {
            return snapshot;
        }

        CaptureRecursive(sceneRoot, sceneRoot, snapshot);
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
            if (string.IsNullOrWhiteSpace(stateKey))
            {
                continue;
            }

            if (!interactableLookup.TryGetValue(stateKey, out InteractableTemplate? interactable) || interactable == null)
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

    private static void CaptureRecursive(Node current, Node sceneRoot, Godot.Collections.Dictionary snapshot)
    {
        if (current is InteractableTemplate interactable)
        {
            string stateKey = interactable.BuildRuntimeStateKey(sceneRoot);
            if (!string.IsNullOrWhiteSpace(stateKey))
            {
                snapshot[stateKey] = interactable.BuildRuntimeSnapshot();
            }
        }

        foreach (Node child in current.GetChildren())
        {
            CaptureRecursive(child, sceneRoot, snapshot);
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
}
