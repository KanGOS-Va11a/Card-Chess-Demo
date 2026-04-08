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

        foreach (Variant key in snapshot.Keys)
        {
            string relativePath = key.AsString();
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                continue;
            }

            InteractableTemplate? interactable = sceneRoot.GetNodeOrNull<InteractableTemplate>(relativePath);
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

    private static void CaptureRecursive(Node current, Node sceneRoot, Godot.Collections.Dictionary snapshot)
    {
        if (current is InteractableTemplate interactable)
        {
            string relativePath = sceneRoot.GetPathTo(interactable).ToString();
            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                snapshot[relativePath] = interactable.BuildRuntimeSnapshot();
            }
        }

        foreach (Node child in current.GetChildren())
        {
            CaptureRecursive(child, sceneRoot, snapshot);
        }
    }
}