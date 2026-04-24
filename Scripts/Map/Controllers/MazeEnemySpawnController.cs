using System;
using System.Collections.Generic;
using System.Linq;
using CardChessDemo.Battle.Boundary;
using Godot;

namespace CardChessDemo.Map;

public partial class MazeEnemySpawnController : Node
{
	[Export] public NodePath PlayerPath { get; set; } = new("../WorldObjects/MainPlayer/Player");
	[Export] public NodePath GroundLayerPath { get; set; } = new("../GroundLayer");
	[Export] public Vector2I RegionSizeTiles { get; set; } = new(12, 12);
	[Export(PropertyHint.Range, "0,3,1")] public int ActiveRegionRadius { get; set; } = 1;
	[Export(PropertyHint.Range, "1,12,1")] public int MaxActiveEnemiesTotal { get; set; } = 4;
	[Export(PropertyHint.Range, "1,6,1")] public int MaxActiveEnemiesPerRegion { get; set; } = 2;
	[Export(PropertyHint.Range, "0.10,10.0,0.05")] public float SpawnCheckIntervalSeconds { get; set; } = 0.75f;
	[Export(PropertyHint.Range, "0.5,120.0,0.5")] public float VictoryRespawnCooldownSeconds { get; set; } = 12.0f;
	[Export(PropertyHint.Range, "0.5,120.0,0.5")] public float RetreatRespawnCooldownSeconds { get; set; } = 6.0f;
	[Export(PropertyHint.Range, "0,64,1")] public int MinSpawnDistanceTiles { get; set; } = 6;
	[Export(PropertyHint.Range, "0,512,1")] public int ViewportPaddingPixels { get; set; } = 48;
	[Export(PropertyHint.Range, "0,8,1")] public int FallbackBattleSourceDistanceTiles { get; set; } = 2;
	[Export] public bool HideAnchorsAtRuntime { get; set; } = true;

	private readonly List<AnchorState> _anchors = new();
	private readonly RandomNumberGenerator _rng = new();
	private Player? _player;
	private TileMapLayer? _groundLayer;
	private double _spawnCheckRemaining;
	private bool _initialized;

	public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_rng.Randomize();
		InitializeForScene(null, string.Empty, null, null);
	}

	public override void _Process(double delta)
	{
		if (!_initialized || _player == null || _anchors.Count == 0)
		{
			return;
		}

		for (int index = 0; index < _anchors.Count; index++)
		{
			AnchorState anchor = _anchors[index];
			if (anchor.CooldownRemainingSeconds > 0.0d)
			{
				anchor.CooldownRemainingSeconds = Math.Max(0.0d, anchor.CooldownRemainingSeconds - delta);
			}
		}

		_spawnCheckRemaining -= delta;
		if (_spawnCheckRemaining > 0.0d)
		{
			return;
		}

		_spawnCheckRemaining = Math.Max(0.10f, SpawnCheckIntervalSeconds);
		RefreshSpawns(force: false);
	}

	public void InitializeForScene(Player? player, string currentScenePath, MapResumeContext? resumeContext, BattleResult? lastBattleResult)
	{
		_player = player ?? ResolvePlayer();
		_groundLayer ??= ResolveGroundLayer();
		CollectAnchors();
		_initialized = _anchors.Count > 0;
		if (!_initialized)
		{
			return;
		}

		if (resumeContext != null && lastBattleResult != null)
		{
			HandleBattleReturn(resumeContext, lastBattleResult);
		}

		_spawnCheckRemaining = 0.0d;
		RefreshSpawns(force: true);
	}

	private void CollectAnchors()
	{
		Node sceneRoot = GetTree().CurrentScene ?? this;
		MazeEnemyChaser[] discoveredAnchors = sceneRoot
			.FindChildren("*", "", true, false)
			.OfType<MazeEnemyChaser>()
			.Where(anchor => anchor.SpawnControllerManaged)
			.ToArray();

		if (_anchors.Count == discoveredAnchors.Length
			&& _anchors.All(entry => GodotObject.IsInstanceValid(entry.Anchor))
			&& discoveredAnchors.All(anchor => _anchors.Any(existing => existing.Anchor == anchor)))
		{
			return;
		}

		_anchors.Clear();
		foreach (MazeEnemyChaser anchor in discoveredAnchors)
		{
			anchor.DeactivateManagedSpawn(HideAnchorsAtRuntime);
			anchor.GlobalPosition = anchor.GlobalPosition;
			_anchors.Add(new AnchorState
			{
				Anchor = anchor,
				SourcePath = sceneRoot.IsAncestorOf(anchor) ? sceneRoot.GetPathTo(anchor).ToString() : string.Empty,
				AnchorCell = anchor.GetAnchorCell(),
				CooldownRemainingSeconds = 0.0d,
			});
		}
	}

	private void HandleBattleReturn(MapResumeContext resumeContext, BattleResult lastBattleResult)
	{
		MapReturnReason returnReason = ResolveReturnReason(resumeContext, lastBattleResult);
		if (returnReason != MapReturnReason.BattleVictory && returnReason != MapReturnReason.BattleRetreat)
		{
			return;
		}

		AnchorState? resolvedAnchor = ResolveBattleSourceAnchor(resumeContext);
		if (resolvedAnchor == null)
		{
			return;
		}

		resolvedAnchor.Anchor.DeactivateManagedSpawn(HideAnchorsAtRuntime);
		resolvedAnchor.CooldownRemainingSeconds = Math.Max(
			resolvedAnchor.CooldownRemainingSeconds,
			returnReason == MapReturnReason.BattleRetreat ? RetreatRespawnCooldownSeconds : VictoryRespawnCooldownSeconds);
		resolvedAnchor.AnchorCell = resolvedAnchor.Anchor.GetAnchorCell();
	}

	private AnchorState? ResolveBattleSourceAnchor(MapResumeContext resumeContext)
	{
		if (!string.IsNullOrWhiteSpace(resumeContext.SourceInteractablePath))
		{
			AnchorState? exactMatch = _anchors.FirstOrDefault(anchor => string.Equals(anchor.SourcePath, resumeContext.SourceInteractablePath, StringComparison.Ordinal));
			if (exactMatch != null)
			{
				return exactMatch;
			}
		}

		if (_player == null)
		{
			return null;
		}

		Vector2I playerCell = WorldToCell(_player.GlobalPosition);
		return _anchors
			.Where(anchor => string.IsNullOrWhiteSpace(resumeContext.EncounterId) || string.Equals(anchor.Anchor.EncounterId, resumeContext.EncounterId, StringComparison.Ordinal))
			.OrderBy(anchor => EnemyAiDistance(anchor.AnchorCell, playerCell))
			.ThenBy(anchor => anchor.SourcePath, StringComparer.Ordinal)
			.FirstOrDefault(anchor => EnemyAiDistance(anchor.AnchorCell, playerCell) <= Math.Max(0, FallbackBattleSourceDistanceTiles));
	}

	private static MapReturnReason ResolveReturnReason(MapResumeContext resumeContext, BattleResult lastBattleResult)
	{
		if (resumeContext.ReturnReason != MapReturnReason.None && resumeContext.ReturnReason != MapReturnReason.PendingBattle)
		{
			return resumeContext.ReturnReason;
		}

		if (lastBattleResult.DidPlayerRetreat)
		{
			return MapReturnReason.BattleRetreat;
		}

		if (lastBattleResult.DidPlayerWin)
		{
			return MapReturnReason.BattleVictory;
		}

		return MapReturnReason.None;
	}

	private void RefreshSpawns(bool force)
	{
		if (_player == null || _groundLayer == null || _anchors.Count == 0)
		{
			return;
		}

		Vector2I playerCell = WorldToCell(_player.GlobalPosition);
		Vector2I playerRegion = GetRegionId(playerCell);

		for (int index = 0; index < _anchors.Count; index++)
		{
			AnchorState anchor = _anchors[index];
			anchor.AnchorCell = anchor.Anchor.GetAnchorCell();
			if (!anchor.Anchor.IsManagedSpawnActive)
			{
				continue;
			}

			if (IsRegionWithinRadius(GetRegionId(anchor.AnchorCell), playerRegion, ActiveRegionRadius + 1))
			{
				continue;
			}

			if (IsInsideVisibleWorldRect(anchor.Anchor.GlobalPosition))
			{
				continue;
			}

			anchor.Anchor.DeactivateManagedSpawn(HideAnchorsAtRuntime);
		}

		Dictionary<Vector2I, int> activeCountsByRegion = _anchors
			.Where(anchor => anchor.Anchor.IsManagedSpawnActive)
			.GroupBy(anchor => GetRegionId(anchor.AnchorCell))
			.ToDictionary(group => group.Key, group => group.Count());
		int activeTotal = _anchors.Count(anchor => anchor.Anchor.IsManagedSpawnActive);
		if (activeTotal >= Math.Max(1, MaxActiveEnemiesTotal))
		{
			return;
		}

		List<Vector2I> orderedRegions = BuildOrderedRegions(playerRegion);
		bool spawnedAny = true;
		while (activeTotal < Math.Max(1, MaxActiveEnemiesTotal) && spawnedAny)
		{
			spawnedAny = false;
			foreach (Vector2I region in orderedRegions)
			{
				int activeInRegion = activeCountsByRegion.TryGetValue(region, out int value) ? value : 0;
				if (activeInRegion >= Math.Max(1, MaxActiveEnemiesPerRegion))
				{
					continue;
				}

				List<AnchorState> eligibleAnchors = _anchors
					.Where(anchor => !anchor.Anchor.IsManagedSpawnActive)
					.Where(anchor => anchor.CooldownRemainingSeconds <= 0.0d)
					.Where(anchor => GetRegionId(anchor.AnchorCell) == region)
					.Where(anchor => IsAnchorEligibleForSpawn(anchor, playerCell, force))
					.ToList();
				if (eligibleAnchors.Count == 0)
				{
					continue;
				}

				AnchorState chosenAnchor = eligibleAnchors[(int)_rng.RandiRange(0, eligibleAnchors.Count - 1)];
				string encounterId = ChooseEncounterId(chosenAnchor.Anchor);
				if (string.IsNullOrWhiteSpace(encounterId))
				{
					continue;
				}

				chosenAnchor.Anchor.ActivateManagedSpawn(encounterId);
				chosenAnchor.AnchorCell = chosenAnchor.Anchor.GetAnchorCell();
				activeCountsByRegion[region] = activeInRegion + 1;
				activeTotal += 1;
				spawnedAny = true;
				if (activeTotal >= Math.Max(1, MaxActiveEnemiesTotal))
				{
					break;
				}
			}
		}
	}

	private bool IsAnchorEligibleForSpawn(AnchorState anchor, Vector2I playerCell, bool force)
	{
		if (EnemyAiDistance(anchor.AnchorCell, playerCell) < Math.Max(0, MinSpawnDistanceTiles))
		{
			return false;
		}

		if (!force && IsInsideVisibleWorldRect(anchor.Anchor.GlobalPosition))
		{
			return false;
		}

		return true;
	}

	private string ChooseEncounterId(MazeEnemyChaser anchor)
	{
		string[] candidates = anchor.GetEncounterCandidates();
		if (candidates.Length == 0)
		{
			return string.Empty;
		}

		return candidates[(int)_rng.RandiRange(0, candidates.Length - 1)];
	}

	private List<Vector2I> BuildOrderedRegions(Vector2I playerRegion)
	{
		List<Vector2I> regions = new();
		for (int y = -ActiveRegionRadius; y <= ActiveRegionRadius; y++)
		{
			for (int x = -ActiveRegionRadius; x <= ActiveRegionRadius; x++)
			{
				Vector2I region = new(playerRegion.X + x, playerRegion.Y + y);
				if (!IsRegionWithinRadius(region, playerRegion, ActiveRegionRadius))
				{
					continue;
				}

				regions.Add(region);
			}
		}

		return regions
			.OrderBy(region => EnemyAiDistance(region, playerRegion))
			.ThenBy(region => region.Y)
			.ThenBy(region => region.X)
			.ToList();
	}

	private bool IsInsideVisibleWorldRect(Vector2 worldPosition)
	{
		Rect2 screenRect = GetViewport().GetVisibleRect();
		Transform2D screenToWorld = GetViewport().GetCanvasTransform().AffineInverse();
		Vector2 a = screenToWorld * screenRect.Position;
		Vector2 b = screenToWorld * screenRect.End;
		float left = Mathf.Min(a.X, b.X) - ViewportPaddingPixels;
		float right = Mathf.Max(a.X, b.X) + ViewportPaddingPixels;
		float top = Mathf.Min(a.Y, b.Y) - ViewportPaddingPixels;
		float bottom = Mathf.Max(a.Y, b.Y) + ViewportPaddingPixels;
		return worldPosition.X >= left && worldPosition.X <= right && worldPosition.Y >= top && worldPosition.Y <= bottom;
	}

	private Player? ResolvePlayer()
	{
		if (!PlayerPath.IsEmpty && GetNodeOrNull<Player>(PlayerPath) is Player playerFromPath)
		{
			return playerFromPath;
		}

		return GetTree().CurrentScene?.FindChild("Player", true, false) as Player;
	}

	private TileMapLayer? ResolveGroundLayer()
	{
		if (!GroundLayerPath.IsEmpty && GetNodeOrNull<TileMapLayer>(GroundLayerPath) is TileMapLayer groundLayerFromPath)
		{
			return groundLayerFromPath;
		}

		return GetTree().CurrentScene?.FindChild("GroundLayer", true, false) as TileMapLayer;
	}

	private Vector2I WorldToCell(Vector2 worldPosition)
	{
		if (_groundLayer != null)
		{
			return _groundLayer.LocalToMap(_groundLayer.ToLocal(worldPosition));
		}

		return new Vector2I(Mathf.RoundToInt(worldPosition.X / 16.0f), Mathf.RoundToInt(worldPosition.Y / 16.0f));
	}

	private Vector2I GetRegionId(Vector2I cell)
	{
		int width = Math.Max(1, RegionSizeTiles.X);
		int height = Math.Max(1, RegionSizeTiles.Y);
		return new Vector2I(
			Mathf.FloorToInt((float)cell.X / width),
			Mathf.FloorToInt((float)cell.Y / height));
	}

	private static bool IsRegionWithinRadius(Vector2I region, Vector2I center, int radius)
	{
		return EnemyAiDistance(region, center) <= Math.Max(0, radius);
	}

	private static int EnemyAiDistance(Vector2I a, Vector2I b)
	{
		return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
	}

	private sealed class AnchorState
	{
		public MazeEnemyChaser Anchor { get; init; } = null!;
		public string SourcePath { get; init; } = string.Empty;
		public Vector2I AnchorCell { get; set; }
		public double CooldownRemainingSeconds { get; set; }
	}
}

