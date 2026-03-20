using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Board;
using CardChessDemo.Battle.Data;
using CardChessDemo.Battle.Rooms;
using CardChessDemo.Battle.Turn;
using CardChessDemo.Battle.Visual;

namespace CardChessDemo.Battle;

public partial class BattleSceneController : Node2D
{
	private const int DebugMoveRange = 4;

	[Export] public PackedScene? ForcedBattleRoomScene { get; set; }
	[Export] public PackedScene[] BattleRoomScenes { get; set; } = Array.Empty<PackedScene>();
	[Export] public BattleRoomPoolDefinition? BattleRoomPools { get; set; }
	[Export] public string[] EncounterEnemyTypeIds { get; set; } = { "grunt" };
	[Export] public int RandomSeed { get; set; } = 1337;

	public BoardState? BoardState { get; private set; }

	public BoardObjectRegistry? Registry { get; private set; }

	public BoardQueryService? QueryService { get; private set; }

	public TurnActionState? TurnState { get; private set; }

	public BattleRoomTemplate? CurrentRoom { get; private set; }

	private RandomNumberGenerator _rng = new();

	public override void _Ready()
	{
		_rng.Seed = (ulong)Math.Max(RandomSeed, 1);

		BoardState = new BoardState();
		Registry = new BoardObjectRegistry();
		QueryService = new BoardQueryService(BoardState, Registry);
		TurnState = new TurnActionState();
		TurnState.StartNewTurn(1);

		CurrentRoom = InstantiateSelectedRoom();
		GetNode<Node2D>("RoomContainer").AddChild(CurrentRoom);
		GetNode<Node2D>("RoomContainer").MoveChild(CurrentRoom, 0);

		RoomLayoutDefinition layout = CurrentRoom.BuildLayoutDefinition();
		BoardInitializer initializer = new(BoardState, Registry);
		initializer.InitializeFromLayout(layout);
		CurrentRoom.SyncMarkersFromBoard(Registry);

		BattleBoardOverlay? overlay = GetNodeOrNull<BattleBoardOverlay>("RoomContainer/BoardOverlay");
		overlay?.Bind(CurrentRoom);

		GD.Print($"BattleSceneController: layout={layout.LayoutId}, size={layout.BoardSize}, objects={Registry.Count}");
	}

	public override void _Process(double delta)
	{
		if (Registry == null || CurrentRoom == null)
		{
			return;
		}

		BattleBoardOverlay? overlay = GetNodeOrNull<BattleBoardOverlay>("RoomContainer/BoardOverlay");
		if (overlay == null)
		{
			return;
		}

		BoardObject? playerObject = GetPrimaryPlayerObject();
		if (playerObject == null)
		{
			overlay.SetReachableCells(Array.Empty<Vector2I>());
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
			return;
		}

		overlay.SetReachableCells(BuildReachableCells(playerObject.Cell, DebugMoveRange));

		if (CurrentRoom.TryScreenToCell(GetGlobalMousePosition(), out Vector2I hoveredCell))
		{
			overlay.SetPreviewPath(BuildPreviewPath(playerObject.Cell, hoveredCell));
		}
		else
		{
			overlay.SetPreviewPath(Array.Empty<Vector2I>());
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseButton || !mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		if (CurrentRoom == null)
		{
			return;
		}

		BoardObject? playerObject = GetPrimaryPlayerObject();
		if (playerObject == null)
		{
			return;
		}

		if (!CurrentRoom.TryScreenToCell(GetGlobalMousePosition(), out Vector2I targetCell))
		{
			return;
		}

		if (!BuildReachableCells(playerObject.Cell, DebugMoveRange).Contains(targetCell))
		{
			return;
		}

		TryMoveObject(playerObject.ObjectId, targetCell, out _);
	}

	public bool TryMoveObject(string objectId, Vector2I targetCell, out string failureReason)
	{
		failureReason = "BoardQueryService has not been initialized.";

		if (QueryService == null || Registry == null || CurrentRoom == null)
		{
			return false;
		}

		bool moved = QueryService.TryMoveObject(objectId, targetCell, out failureReason);
		if (moved)
		{
			CurrentRoom.SyncMarkersFromBoard(Registry);
		}

		return moved;
	}

	private BattleRoomTemplate InstantiateSelectedRoom()
	{
		PackedScene roomScene = SelectRoomScene();
		return roomScene.Instantiate<BattleRoomTemplate>();
	}

	private PackedScene SelectRoomScene()
	{
		if (ForcedBattleRoomScene != null)
		{
			return ForcedBattleRoomScene;
		}

		List<PackedScene> exactMatches = new();
		List<PackedScene> partialMatches = new();
		List<PackedScene> fallbackMatches = new();

		foreach (PackedScene scene in ExpandRoomScenePool())
		{
			BattleRoomTemplate previewRoom = scene.Instantiate<BattleRoomTemplate>();

			if (previewRoom.SupportedEnemyTypeIds.Length == 0)
			{
				fallbackMatches.Add(scene);
				previewRoom.Free();
				continue;
			}

			bool matches = previewRoom.SupportsEnemyTypes(EncounterEnemyTypeIds, out bool exactMatch);
			previewRoom.Free();

			if (!matches)
			{
				continue;
			}

			if (exactMatch)
			{
				exactMatches.Add(scene);
			}
			else
			{
				partialMatches.Add(scene);
			}
		}

		List<PackedScene> candidatePool = exactMatches.Count > 0
			? exactMatches
			: partialMatches.Count > 0
				? partialMatches
				: fallbackMatches.Count > 0
					? fallbackMatches
					: BattleRoomScenes.ToList();

		if (candidatePool.Count == 0)
		{
			throw new InvalidOperationException("BattleSceneController: no battle room scenes are configured.");
		}

		return candidatePool[_rng.RandiRange(0, candidatePool.Count - 1)];
	}

	private IEnumerable<PackedScene> ExpandRoomScenePool()
	{
		HashSet<PackedScene> pooledScenes = new();

		if (BattleRoomPools != null)
		{
			foreach (BattleRoomPoolEntry entry in BattleRoomPools.Entries)
			{
				foreach (PackedScene scene in entry.RoomScenes)
				{
					if (scene != null)
					{
						pooledScenes.Add(scene);
					}
				}
			}
		}

		if (pooledScenes.Count > 0)
		{
			return pooledScenes;
		}

		HashSet<PackedScene> directScenes = new();
		foreach (PackedScene scene in BattleRoomScenes)
		{
			if (scene != null)
			{
				directScenes.Add(scene);
			}
		}

		return directScenes;
	}

	private BoardObject? GetPrimaryPlayerObject()
	{
		if (Registry == null)
		{
			return null;
		}

		return Registry.AllObjects.FirstOrDefault(
			boardObject => boardObject.ObjectType == BoardObjectType.Unit && boardObject.HasTag("player"));
	}

	private List<Vector2I> BuildReachableCells(Vector2I origin, int moveRange)
	{
		List<Vector2I> cells = new();
		if (BoardState == null)
		{
			return cells;
		}

		for (int y = 0; y < BoardState.Size.Y; y++)
		{
			for (int x = 0; x < BoardState.Size.X; x++)
			{
				Vector2I cell = new(x, y);
				int distance = Mathf.Abs(cell.X - origin.X) + Mathf.Abs(cell.Y - origin.Y);
				if (distance <= moveRange)
				{
					cells.Add(cell);
				}
			}
		}

		return cells;
	}

	private static List<Vector2I> BuildPreviewPath(Vector2I start, Vector2I end)
	{
		List<Vector2I> path = new() { start };
		Vector2I current = start;

		while (current.X != end.X)
		{
			current = new Vector2I(current.X + Math.Sign(end.X - current.X), current.Y);
			path.Add(current);
		}

		while (current.Y != end.Y)
		{
			current = new Vector2I(current.X, current.Y + Math.Sign(end.Y - current.Y));
			path.Add(current);
		}

		return path;
	}
}
