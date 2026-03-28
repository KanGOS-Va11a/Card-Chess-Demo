using Godot;

namespace CardChessDemo.Map;

public partial class FlowNodeGate : Node
{
    [Export] public NodePath FlowControllerPath { get; set; } = new("../MapFlowController");
    [Export] public string NodeId { get; set; } = string.Empty;
    [Export] public Godot.Collections.Array<string> Dependencies { get; set; } = new();
    [Export] public bool AutoSetActiveWhenUnlocked { get; set; } = true;

    private MapFlowController _flow;

    public override void _Ready()
    {
        _flow = ResolveFlowController();
    }

    public bool IsUnlocked()
    {
        if (_flow == null)
        {
            return false;
        }

        if (Dependencies.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < Dependencies.Count; i++)
        {
            if (!_flow.IsNodeCompleted(Dependencies[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void EnterNode()
    {
        if (_flow == null || string.IsNullOrWhiteSpace(NodeId))
        {
            return;
        }

        if (!IsUnlocked())
        {
            return;
        }

        if (AutoSetActiveWhenUnlocked)
        {
            _flow.SetActiveNodeId(NodeId);
        }
    }

    public void CompleteNode()
    {
        if (_flow == null || string.IsNullOrWhiteSpace(NodeId))
        {
            return;
        }

        _flow.MarkNodeCompleted(NodeId);
    }

    private MapFlowController ResolveFlowController()
    {
        if (!FlowControllerPath.IsEmpty)
        {
            return GetNodeOrNull<MapFlowController>(FlowControllerPath);
        }

        return GetNodeOrNull<MapFlowController>("../MapFlowController");
    }
}
