using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;
using CardChessDemo.Audio;
using System.Collections.Generic;

namespace CardChessDemo.Map;

public partial class MapSceneController : Node2D
{
	[Export] public NodePath PlayerPath { get; set; } = new("MainPlayer/Player");

	private static readonly Dictionary<string, string> DoorRouteOverrides = new(System.StringComparer.OrdinalIgnoreCase)
	{
		["res://Scene/Maps/Scene01.tscn"] = "res://Scene/Maps/Scene03.tscn",
		["res://Scene/Maps/Scene03.tscn"] = "res://Scene/Maps/Scene04.tscn",
		["res://Scene/Maps/Scene04.tscn"] = "res://Scene/Maps/Scene05.tscn",
		["res://Scene/Maps/Scene05.tscn"] = "res://Scene/Maps/Scene06.tscn",
	};

	private GlobalGameSession? _globalSession;

	public override void _Ready()
	{
		GameAudio.Instance?.PlayMapMusic();
		_globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		ApplyDoorRouteOverrides();
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
		MapRuntimeSnapshotHelper.ApplyToScene(GetTree().CurrentScene ?? this, resumeContext.MapRuntimeSnapshot);
		_globalSession.ConsumePendingMapResumeContext();
	}

	private void ApplyDoorRouteOverrides()
	{
		string currentScenePath = GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath;
		if (!DoorRouteOverrides.TryGetValue(currentScenePath, out string? targetScenePath))
		{
			return;
		}

		Node sceneRoot = GetTree().CurrentScene ?? this;
		foreach (SceneDoor sceneDoor in EnumerateSceneDoors(sceneRoot))
		{
			// Keep explicitly configured destinations intact and only fill donor default route doors.
			if (sceneDoor.NextScene != null || !string.IsNullOrWhiteSpace(sceneDoor.NextScenePath))
			{
				continue;
			}

			sceneDoor.NextScene = null;
			sceneDoor.NextScenePath = targetScenePath;
		}
	}

	private static IEnumerable<SceneDoor> EnumerateSceneDoors(Node root)
	{
		if (root is SceneDoor sceneDoor)
		{
			yield return sceneDoor;
		}

		foreach (Node child in root.GetChildren())
		{
			foreach (SceneDoor nestedDoor in EnumerateSceneDoors(child))
			{
				yield return nestedDoor;
			}
		}
	}

	private Node2D? ResolvePlayerNode()
	{
		if (!PlayerPath.IsEmpty && GetNodeOrNull<Node2D>(PlayerPath) is Node2D player)
		{
			return player;
		}

		return GetNodeOrNull<Node2D>("MainPlayer/Player")
			?? GetNodeOrNull<Node2D>("Player");
	}
}
