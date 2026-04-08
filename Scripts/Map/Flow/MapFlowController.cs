using Godot;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class MapFlowController : Node
{
	[Export] public string ActiveNodeKey { get; set; } = "flow.active_node";
	[Export] public string CompletedPrefix { get; set; } = "flow.completed.";
	[Export] public string DefaultActiveNodeId { get; set; } = "intro_wakeup";

	private GlobalGameSession? _session;

	public override void _Ready()
	{
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		EnsureDefaultActiveNode();
	}

	public string GetActiveNodeId()
	{
		if (_session == null)
		{
			return string.Empty;
		}

		return _session.TryGetFlag(new StringName(ActiveNodeKey), out Variant value)
			? value.AsString()
			: string.Empty;
	}

	public void SetActiveNodeId(string nodeId)
	{
		if (_session == null || string.IsNullOrWhiteSpace(nodeId))
		{
			return;
		}

		_session.SetFlag(new StringName(ActiveNodeKey), nodeId.Trim());
	}

	public bool IsNodeCompleted(string nodeId)
	{
		if (_session == null || string.IsNullOrWhiteSpace(nodeId))
		{
			return false;
		}

		return _session.TryGetFlag(BuildCompletedKey(nodeId), out Variant value) && value.AsBool();
	}

	public void MarkNodeCompleted(string nodeId)
	{
		if (_session == null || string.IsNullOrWhiteSpace(nodeId))
		{
			return;
		}

		_session.SetFlag(BuildCompletedKey(nodeId), true);
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

		if (_session.TryGetFlag(new StringName(ActiveNodeKey), out _))
		{
			return;
		}

		_session.SetFlag(new StringName(ActiveNodeKey), DefaultActiveNodeId);
	}

	private StringName BuildCompletedKey(string nodeId)
	{
		return new StringName($"{CompletedPrefix}{nodeId.Trim()}");
	}
}
