using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Map;

public partial class MazeEnemyChaser : Enemy
{
	[Export] public NodePath PlayerPath { get; set; } = new("../../../WorldObjects/MainPlayer/Player");
	[Export] public NodePath WallLayerPath { get; set; } = new("../../../WallLayer");
	[Export] public NodePath GroundLayerPath { get; set; } = new("../../../GroundLayer");
	[Export(PropertyHint.Range, "1,16,1")] public int VisionRangeTiles { get; set; } = 5;
	[Export(PropertyHint.Range, "1,20,1")] public int ChaseRangeTiles { get; set; } = 8;
	[Export(PropertyHint.Range, "0,2,1")] public int StopDistanceTiles { get; set; } = 1;
	[Export(PropertyHint.Range, "0,2,1")] public int AutoBattleDistanceTiles { get; set; } = 1;
	[Export(PropertyHint.Range, "0.05,1.0,0.01")] public float StepDurationSeconds { get; set; } = 0.35f;
	[Export(PropertyHint.Range, "0.00,1.0,0.01")] public float StepPauseSeconds { get; set; } = 0.12f;
	[Export(PropertyHint.Range, "0.5,20.0,0.1")] public float MaxChaseDurationSeconds { get; set; } = 5.0f;
	[Export] public bool RequireLineOfSight { get; set; } = true;
	[Export(PropertyHint.Layers2DPhysics)] public uint VisionObstacleMask { get; set; } = 1u << 1;
	[Export(PropertyHint.Layers2DPhysics)] public uint BlockingCollisionMask { get; set; } = 1u << 1;
	[Export(PropertyHint.Range, "8,128,1")] public int GridTileSize { get; set; } = 16;

	private Player? _player;
	private TileMapLayer? _wallLayer;
	private TileMapLayer? _groundLayer;
	private readonly HashSet<Vector2I> _groundCells = new();
	private bool _isMoving;
	private Vector2 _moveStart = Vector2.Zero;
	private Vector2 _moveTarget = Vector2.Zero;
	private Vector2I _moveTargetCell = new(int.MinValue, int.MinValue);
	private double _moveElapsed;
	private double _stepCooldown;
	private bool _isChasing;
	private double _chaseElapsed;

	public override void _Ready()
	{
		base._Ready();
		ResolveReferences();
		GlobalPosition = SnapToGrid(GlobalPosition);
	}

	public override void SetInteractionHighlight(bool highlighted)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_stepCooldown > 0.0d)
		{
			_stepCooldown = Mathf.Max(0.0d, _stepCooldown - delta);
		}

		if (_isMoving)
		{
			ContinueMove(delta);
			return;
		}

		ResolveReferences();
		if (_player == null || _groundLayer == null || _wallLayer == null)
		{
			return;
		}

		if (_stepCooldown > 0.0d)
		{
			return;
		}

		Vector2I currentCell = WorldToCell(GlobalPosition);
		Vector2I playerCell = WorldToCell(_player.GlobalPosition);
		int distance = Mathf.Abs(playerCell.X - currentCell.X) + Mathf.Abs(playerCell.Y - currentCell.Y);
		bool hasLineOfSight = !RequireLineOfSight || HasLineOfSightToPlayer();

		if (!_isChasing)
		{
			if (distance > VisionRangeTiles || !hasLineOfSight)
			{
				return;
			}

			_isChasing = true;
			_chaseElapsed = 0.0d;
		}
		else
		{
			_chaseElapsed += delta;
			if (!hasLineOfSight || _chaseElapsed >= MaxChaseDurationSeconds || distance > ChaseRangeTiles)
			{
				ResetChaseState();
				return;
			}
		}

		if (distance <= AutoBattleDistanceTiles)
		{
			Interact(_player);
			return;
		}

		if (distance <= StopDistanceTiles)
		{
			return;
		}

		if (!TryGetNextChaseCell(currentCell, playerCell, out Vector2I nextCell))
		{
			return;
		}

		StartMove(nextCell);
	}

	private void ResolveReferences()
	{
		_player ??= ResolvePlayer();
		_wallLayer ??= GetNodeOrNull<TileMapLayer>(WallLayerPath) ?? GetTree().CurrentScene?.FindChild("WallLayer", true, false) as TileMapLayer;
		_groundLayer ??= GetNodeOrNull<TileMapLayer>(GroundLayerPath) ?? GetTree().CurrentScene?.FindChild("GroundLayer", true, false) as TileMapLayer;

		if (_groundLayer != null && _groundCells.Count == 0)
		{
			foreach (Vector2I cell in _groundLayer.GetUsedCells())
			{
				if (_groundLayer.GetCellSourceId(cell) >= 0)
				{
					_groundCells.Add(cell);
				}
			}
		}
	}

	private Player? ResolvePlayer()
	{
		if (!PlayerPath.IsEmpty && GetNodeOrNull<Player>(PlayerPath) is Player playerFromPath)
		{
			return playerFromPath;
		}

		return GetTree().CurrentScene?.FindChild("Player", true, false) as Player;
	}

	private void ContinueMove(double delta)
	{
		if (_player != null)
		{
			Vector2I playerCell = WorldToCell(_player.GlobalPosition);
			if (playerCell == _moveTargetCell || playerCell == WorldToCell(GlobalPosition))
			{
				_isMoving = false;
				_moveElapsed = 0.0d;
				Interact(_player);
				return;
			}
		}

		_moveElapsed += delta;
		double duration = System.Math.Max(0.01d, StepDurationSeconds);
		float t = Mathf.Clamp((float)(_moveElapsed / duration), 0.0f, 1.0f);
		GlobalPosition = _moveStart.Lerp(_moveTarget, t);
		if (t < 1.0f)
		{
			return;
		}

		GlobalPosition = _moveTarget;
		_isMoving = false;
		_moveElapsed = 0.0d;
	}

	private void ResetChaseState()
	{
		_isChasing = false;
		_chaseElapsed = 0.0d;
	}

	private void StartMove(Vector2I nextCell)
	{
		if (_player != null && nextCell == WorldToCell(_player.GlobalPosition))
		{
			Interact(_player);
			return;
		}

		_moveStart = GlobalPosition;
		_moveTarget = CellToWorld(nextCell);
		_moveTargetCell = nextCell;
		_isMoving = true;
		_moveElapsed = 0.0d;
		_stepCooldown = StepPauseSeconds;
	}

	private bool TryGetNextChaseCell(Vector2I startCell, Vector2I playerCell, out Vector2I nextCell)
	{
		nextCell = startCell;
		List<Vector2I> goalCells = BuildGoalCellsAroundPlayer(playerCell);
		if (goalCells.Count == 0)
		{
			return false;
		}

		Queue<Vector2I> frontier = new();
		Dictionary<Vector2I, Vector2I> cameFrom = new();
		HashSet<Vector2I> visited = new();
		frontier.Enqueue(startCell);
		visited.Add(startCell);

		Vector2I[] directions =
		{
			new Vector2I(1, 0),
			new Vector2I(-1, 0),
			new Vector2I(0, 1),
			new Vector2I(0, -1),
		};

		Vector2I reachedGoal = new(int.MinValue, int.MinValue);
		while (frontier.Count > 0)
		{
			Vector2I current = frontier.Dequeue();
			if (goalCells.Contains(current))
			{
				reachedGoal = current;
				break;
			}

			foreach (Vector2I direction in directions)
			{
				Vector2I candidate = current + direction;
				if (visited.Contains(candidate) || !IsWalkable(candidate))
				{
					continue;
				}

				visited.Add(candidate);
				cameFrom[candidate] = current;
				frontier.Enqueue(candidate);
			}
		}

		if (reachedGoal.X == int.MinValue)
		{
			return false;
		}

		Vector2I step = reachedGoal;
		while (cameFrom.TryGetValue(step, out Vector2I previous) && previous != startCell)
		{
			step = previous;
		}

		nextCell = step;
		return nextCell != startCell;
	}

	private List<Vector2I> BuildGoalCellsAroundPlayer(Vector2I playerCell)
	{
		List<Vector2I> goals = new();
		Vector2I[] directions =
		{
			new Vector2I(1, 0),
			new Vector2I(-1, 0),
			new Vector2I(0, 1),
			new Vector2I(0, -1),
		};

		foreach (Vector2I direction in directions)
		{
			Vector2I candidate = playerCell + direction;
			if (IsWalkable(candidate))
			{
				goals.Add(candidate);
			}
		}

		return goals;
	}

	private bool IsWalkable(Vector2I cell)
	{
		if (_groundLayer == null || !_groundCells.Contains(cell))
		{
			return false;
		}

		if (_player != null && cell == WorldToCell(_player.GlobalPosition))
		{
			return false;
		}

		if (_wallLayer != null && _wallLayer.GetCellSourceId(cell) >= 0)
		{
			return false;
		}

		return !HasBlockingBody(cell);
	}

	private bool HasBlockingBody(Vector2I cell)
	{
		PhysicsPointQueryParameters2D query = new()
		{
			Position = CellToWorld(cell),
			CollisionMask = BlockingCollisionMask,
			CollideWithBodies = true,
			CollideWithAreas = false,
		};
		query.Exclude.Add(GetRid());

		Godot.Collections.Array<Godot.Collections.Dictionary> hits = GetWorld2D().DirectSpaceState.IntersectPoint(query, 8);
		foreach (Godot.Collections.Dictionary hit in hits)
		{
			GodotObject collider = hit["collider"].AsGodotObject();
			if (collider == null || collider == this)
			{
				continue;
			}

			return true;
		}

		return false;
	}

	private bool HasLineOfSightToPlayer()
	{
		if (_player == null || VisionObstacleMask == 0)
		{
			return true;
		}

		PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(GlobalPosition, _player.GlobalPosition, VisionObstacleMask);
		query.CollideWithAreas = false;
		query.CollideWithBodies = true;
		query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

		Godot.Collections.Dictionary hit = GetWorld2D().DirectSpaceState.IntersectRay(query);
		return hit.Count == 0;
	}

	private Vector2I WorldToCell(Vector2 worldPosition)
	{
		if (_groundLayer != null)
		{
			return _groundLayer.LocalToMap(_groundLayer.ToLocal(worldPosition));
		}

		float tileSize = Mathf.Max(1.0f, GridTileSize);
		return new Vector2I(
			Mathf.RoundToInt((worldPosition.X - tileSize * 0.5f) / tileSize),
			Mathf.RoundToInt((worldPosition.Y - tileSize * 0.5f) / tileSize));
	}

	private Vector2 CellToWorld(Vector2I cell)
	{
		if (_groundLayer != null)
		{
			return _groundLayer.ToGlobal(_groundLayer.MapToLocal(cell));
		}

		float tileSize = Mathf.Max(1.0f, GridTileSize);
		return new Vector2(
			cell.X * tileSize + tileSize * 0.5f,
			cell.Y * tileSize + tileSize * 0.5f);
	}

	private Vector2 SnapToGrid(Vector2 worldPosition)
	{
		return CellToWorld(WorldToCell(worldPosition));
	}
}
