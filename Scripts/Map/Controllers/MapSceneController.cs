using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class MapSceneController : Node2D
{
	[Export] public NodePath PlayerPath { get; set; } = new("Player");

	private GlobalGameSession? _globalSession;

	public override void _Ready()
	{
		_globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (_globalSession == null)
		{
			return;
		}

		ApplyPendingResumeContext();
		_globalSession.ConsumeLastBattleResult();
	}

	private void ApplyPendingResumeContext()
	{
		if (_globalSession == null)
		{
			return;
		}

		MapResumeContext? resumeContext = _globalSession.PeekPendingMapResumeContext();
		if (resumeContext == null)
		{
			return;
		}

		string currentScenePath = GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath;
		if (!string.Equals(currentScenePath, resumeContext.ScenePath, System.StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		Node2D? player = ResolvePlayerNode();
		if (player == null)
		{
			GD.PushWarning("MapSceneController: failed to resolve player node for map resume.");
			return;
		}

		player.GlobalPosition = resumeContext.PlayerGlobalPosition;
		_globalSession.ConsumePendingMapResumeContext();
	}

	private Node2D? ResolvePlayerNode()
	{
		if (!PlayerPath.IsEmpty && GetNodeOrNull<Node2D>(PlayerPath) is Node2D player)
		{
			return player;
		}

		return GetNodeOrNull<Node2D>("Player");
	}
}
