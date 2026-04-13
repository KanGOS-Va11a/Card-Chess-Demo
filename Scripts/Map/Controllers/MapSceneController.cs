using System.Collections.Generic;
using System.Linq;
using CardChessDemo.Audio;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;
using Godot;

namespace CardChessDemo.Map;

public partial class MapSceneController : Node2D
{
	private const string PlayerSpawnLayerName = "PlayerSpawnLayer";

	[Export] public NodePath PlayerPath { get; set; } = new("MainPlayer/Player");
	[Export] public NodePath PlayerContainerPath { get; set; } = new("WorldObjects");
	[Export] public PackedScene? PlayerScene { get; set; } = GD.Load<PackedScene>("res://Scene/Character/MainPlayer.tscn");

	private static readonly Dictionary<string, string> DoorRouteOverrides = new(System.StringComparer.OrdinalIgnoreCase)
	{
		["res://Scene/Maps/Scene01.tscn"] = "res://Scene/Maps/Scene03.tscn",
		["res://Scene/Maps/Scene03.tscn"] = "res://Scene/Maps/Scene04.tscn",
		["res://Scene/Maps/Scene04.tscn"] = "res://Scene/Maps/Scene05.tscn",
		["res://Scene/Maps/Scene05.tscn"] = "res://Scene/Maps/Scene06.tscn",
	};

	private GlobalGameSession? _globalSession;

	public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		string currentScenePath = GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath;

		Node2D? player = EnsurePlayerNodePresent();
		HideAllSpawnMarkers();
		ApplyDoorRouteOverrides();

		if (_globalSession != null
			&& _globalSession.TryConsumePendingBattleReturn(currentScenePath, out MapResumeContext? resumeContext)
			&& resumeContext != null)
		{
			GD.Print($"MapSceneController: applying battle return for '{currentScenePath}', playerPos={resumeContext.PlayerGlobalPosition}");
			MapRuntimeSnapshotHelper.ApplyToScene(GetTree().CurrentScene ?? this, resumeContext.MapRuntimeSnapshot);
			if (player != null)
			{
				player.GlobalPosition = resumeContext.PlayerGlobalPosition;
			}
		}
		else
		{
			string targetSpawnId = string.Empty;
			if (_globalSession != null && _globalSession.TryConsumePendingSceneTransfer(currentScenePath, out string pendingSpawnId))
			{
				targetSpawnId = pendingSpawnId;
			}

			GD.Print($"MapSceneController: applying scene entry spawn for '{currentScenePath}', targetSpawnId='{targetSpawnId}'");
			ApplySpawnMarkerPosition(player, targetSpawnId);
		}

		ApplyUsedInteractableRemovals();
		_globalSession?.ConsumeLastBattleResult();
		GameAudio.Instance?.PlayMapMusic();
	}

	private Node2D? EnsurePlayerNodePresent()
	{
		Node2D? existingPlayer = ResolvePlayerNode();
		if (existingPlayer != null)
		{
			GD.Print($"MapSceneController: found existing player at {existingPlayer.GlobalPosition}");
			return existingPlayer;
		}

		if (PlayerScene?.Instantiate() is not Node playerRoot)
		{
			GD.Print("MapSceneController: failed to instantiate PlayerScene.");
			return null;
		}

		playerRoot.Name = "MainPlayer";
		Node playerContainer = ResolvePlayerContainerNode();
		playerContainer.AddChild(playerRoot);

		Node2D? spawnedPlayer = playerRoot.GetNodeOrNull<Node2D>("Player")
			?? ResolvePlayerNode();
		if (spawnedPlayer == null)
		{
			GD.Print($"MapSceneController: instantiated player root under '{playerContainer.GetPath()}', but failed to resolve Player child.");
		}
		else
		{
			GD.Print($"MapSceneController: instantiated player under '{playerContainer.GetPath()}', currentPos={spawnedPlayer.GlobalPosition}");
		}

		return spawnedPlayer;
	}

	private void ApplySpawnMarkerPosition(Node2D? player, string targetSpawnId)
	{
		if (player == null)
		{
			GD.Print("MapSceneController: ApplySpawnMarkerPosition skipped because player is null.");
			return;
		}

		SpawnPoint[] spawnPoints = CollectSpawnPoints(GetTree().CurrentScene ?? this);
		if (spawnPoints.Length == 0)
		{
			GD.Print($"MapSceneController: no spawn points found for '{GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath}', player stays at {player.GlobalPosition}");
			return;
		}

		SpawnPoint spawnPoint = spawnPoints[0];
		if (!string.IsNullOrWhiteSpace(targetSpawnId))
		{
			int matchedIndex = System.Array.FindIndex(
				spawnPoints,
				candidate => string.Equals(candidate.SpawnId, targetSpawnId, System.StringComparison.OrdinalIgnoreCase));
			if (matchedIndex >= 0)
			{
				spawnPoint = spawnPoints[matchedIndex];
			}
		}

		player.GlobalPosition = spawnPoint.WorldPosition;
		GD.Print($"MapSceneController: selected spawn '{spawnPoint.SpawnId}' at {spawnPoint.WorldPosition}");
	}

	private void HideAllSpawnMarkers()
	{
		foreach (TileMapLayer spawnLayer in EnumeratePlayerSpawnLayers(GetTree().CurrentScene ?? this))
		{
			spawnLayer.Visible = false;
		}

		foreach (CanvasItem markerVisual in EnumeratePlayerSpawnMarkers(GetTree().CurrentScene ?? this).OfType<CanvasItem>())
		{
			markerVisual.Visible = false;
		}
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
			if (sceneDoor.NextScene != null || !string.IsNullOrWhiteSpace(sceneDoor.NextScenePath))
			{
				continue;
			}

			sceneDoor.NextScenePath = targetScenePath;
		}
	}

	private void ApplyUsedInteractableRemovals()
	{
		if (_globalSession == null)
		{
			return;
		}

		Node sceneRoot = GetTree().CurrentScene ?? this;
		foreach (StringName interactableId in _globalSession.UsedInteractables)
		{
			string relativePath = interactableId.ToString();
			if (string.IsNullOrWhiteSpace(relativePath))
			{
				continue;
			}

			InteractableTemplate? interactable = sceneRoot.GetNodeOrNull<InteractableTemplate>(relativePath);
			if (interactable == null)
			{
				continue;
			}

			Godot.Collections.Dictionary snapshot = interactable.BuildRuntimeSnapshot();
			snapshot["is_disabled"] = true;
			snapshot["remove_from_scene"] = true;
			interactable.ApplyRuntimeSnapshot(snapshot);
		}
	}

	private Node2D? ResolvePlayerNode()
	{
		if (!PlayerPath.IsEmpty && GetNodeOrNull<Node2D>(PlayerPath) is Node2D byPath)
		{
			return byPath;
		}

		return GetNodeOrNull<Node2D>("WorldObjects/MainPlayer/Player")
			?? GetNodeOrNull<Node2D>("MainPlayer/Player")
			?? GetNodeOrNull<Node2D>("Player")
			?? (GetTree().CurrentScene?.FindChild("Player", true, false) as Node2D);
	}

	private Node ResolvePlayerContainerNode()
	{
		if (!PlayerContainerPath.IsEmpty && GetNodeOrNull<Node>(PlayerContainerPath) is Node container)
		{
			return container;
		}

		return GetNodeOrNull<Node>("WorldObjects")
			?? (GetTree().CurrentScene ?? this);
	}

	private static IEnumerable<PlayerSpawnMarker> EnumeratePlayerSpawnMarkers(Node root)
	{
		if (root is PlayerSpawnMarker marker)
		{
			yield return marker;
		}

		foreach (Node child in root.GetChildren())
		{
			foreach (PlayerSpawnMarker nestedMarker in EnumeratePlayerSpawnMarkers(child))
			{
				yield return nestedMarker;
			}
		}
	}

	private static IEnumerable<TileMapLayer> EnumeratePlayerSpawnLayers(Node root)
	{
		if (root is TileMapLayer tileMapLayer && string.Equals(tileMapLayer.Name.ToString(), PlayerSpawnLayerName, System.StringComparison.Ordinal))
		{
			yield return tileMapLayer;
		}

		foreach (Node child in root.GetChildren())
		{
			foreach (TileMapLayer nestedLayer in EnumeratePlayerSpawnLayers(child))
			{
				yield return nestedLayer;
			}
		}
	}

	private static SpawnPoint[] CollectSpawnPoints(Node root)
	{
		SpawnPoint[] explicitMarkers = EnumeratePlayerSpawnMarkers(root)
			.Select(marker => new SpawnPoint(
				marker.GetResolvedSpawnId(),
				marker.GetSpawnWorldPosition()))
			.OrderBy(point => point.WorldPosition.Y)
			.ThenBy(point => point.WorldPosition.X)
			.ToArray();
		if (explicitMarkers.Length > 0)
		{
			return explicitMarkers;
		}

		return EnumeratePlayerSpawnLayers(root)
			.SelectMany(layer => layer.GetUsedCells().Select(cell =>
			{
				Vector2 localPosition = layer.MapToLocal(cell);
				return new SpawnPoint(
					$"cell:{cell.X},{cell.Y}",
					layer.ToGlobal(localPosition));
			}))
			.OrderBy(point => point.WorldPosition.Y)
			.ThenBy(point => point.WorldPosition.X)
			.ToArray();
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

	private readonly record struct SpawnPoint(string SpawnId, Vector2 WorldPosition);
}
