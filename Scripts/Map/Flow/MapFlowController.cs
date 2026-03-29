using Godot;
using System;

namespace CardChessDemo.Map;

public partial class MapFlowController : Node
{
    [Export] public string ActiveNodeKey { get; set; } = "flow.active_node";
    [Export] public string CompletedPrefix { get; set; } = "flow.completed.";
    [Export] public string DefaultActiveNodeId { get; set; } = "intro_wakeup";

    private GameSession _session;

    public override void _Ready()
    {
        _session = GetNodeOrNull<GameSession>("/root/GameSession");
        EnsureDefaultActiveNode();
    }

    public string GetActiveNodeId()
    {
        if (_session == null)
        {
            return string.Empty;
        }

        StringName key = new StringName(ActiveNodeKey);
        if (!_session.world_flags.TryGetValue(key, out Variant value))
        {
            return string.Empty;
        }

        return value.AsString();
    }

    public void SetActiveNodeId(string nodeId)
    {
        if (_session == null || string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        _session.set_flag(new StringName(ActiveNodeKey), nodeId.Trim());
    }

    public bool IsNodeCompleted(string nodeId)
    {
        if (_session == null || string.IsNullOrWhiteSpace(nodeId))
        {
            return false;
        }

        StringName key = BuildCompletedKey(nodeId);
        if (!_session.world_flags.TryGetValue(key, out Variant value))
        {
            return false;
        }

        return value.AsBool();
    }

    public void MarkNodeCompleted(string nodeId)
    {
        if (_session == null || string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        _session.set_flag(BuildCompletedKey(nodeId), true);
    }

    public bool AreAllNodesCompleted(string[] nodeIds)
    {
        if (nodeIds == null || nodeIds.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < nodeIds.Length; i++)
        {
            if (!IsNodeCompleted(nodeIds[i]))
            {
                return false;
            }
        }

        return true;
    }

    private void EnsureDefaultActiveNode()
    {
        if (_session == null)
        {
            return;
        }

        StringName key = new StringName(ActiveNodeKey);
        if (_session.world_flags.ContainsKey(key))
        {
            return;
        }

        _session.set_flag(key, DefaultActiveNodeId);
    }

    private StringName BuildCompletedKey(string nodeId)
    {
        return new StringName($"{CompletedPrefix}{nodeId.Trim()}");
    }
}
