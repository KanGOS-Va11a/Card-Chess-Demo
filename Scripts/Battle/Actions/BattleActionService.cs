using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using CardChessDemo.Battle.Board;
using CardChessDemo.Battle.Data;
using CardChessDemo.Battle.Presentation;
using CardChessDemo.Battle.Rooms;
using CardChessDemo.Battle.Shared;
using CardChessDemo.Battle.State;
using CardChessDemo.Battle.Visual;

namespace CardChessDemo.Battle.Actions;

public sealed class BattleActionService
{
    public event Action<string>? ActionLogged;
    public event Action<string, string>? EnemyDefeated;
    public const string ArcTerrainId = "arc";
    public const int ArcTerrainDamage = 2;
    public const string FireTerrainId = "fire";
    public const int FireTerrainDamage = 3;

    private readonly BoardState _boardState;
    private readonly BoardObjectRegistry _registry;
    private readonly BoardQueryService _queryService;
    private readonly BoardPathfinder _pathfinder;
    private readonly BattleObjectStateManager _stateManager;
    private readonly BattlePieceViewManager _pieceViewManager;
    private readonly BattleRoomTemplate _room;
    private readonly GlobalGameSession _session;
    private readonly BattleFloatingTextLayer? _floatingTextLayer;
    private readonly SceneTree _sceneTree;
    private double _lastImpactPresentationDurationSeconds;

    public const double MovePresentationDurationSeconds = 0.12d;
    public const double AttackPresentationDurationSeconds = 0.24d;
    public const double ImpactPresentationDurationSeconds = 0.18d;
    public const double DefensePresentationDurationSeconds = 0.22d;
    public const double UtilityPresentationDurationSeconds = 0.24d;
    public const double ImpactFlowWaitRatio = 0.82d;
    public const double KillShatterPresentationDurationSeconds = 0.52d;
    public const double KillWhitenPresentationDurationSeconds = 0.24d;
    public const double KillKnockbackPresentationDurationSeconds = 0.50d;
    public const float KillKnockbackDistancePixels = 18.0f;
    public const double ObstacleBreakShatterPresentationDurationSeconds = 0.58d;
    public const double ObstacleBreakWhitenPresentationDurationSeconds = 0.18d;

    public BattleActionService(
        BoardState boardState,
        BoardObjectRegistry registry,
        BoardQueryService queryService,
        BoardPathfinder pathfinder,
        BattleObjectStateManager stateManager,
        BattlePieceViewManager pieceViewManager,
        BattleRoomTemplate room,
        GlobalGameSession session,
        BattleFloatingTextLayer? floatingTextLayer = null,
        SceneTree? sceneTree = null)
    {
        _boardState = boardState;
        _registry = registry;
        _queryService = queryService;
        _pathfinder = pathfinder;
        _stateManager = stateManager;
        _pieceViewManager = pieceViewManager;
        _room = room;
        _session = session;
        _floatingTextLayer = floatingTextLayer;
        _sceneTree = sceneTree ?? room.GetTree();
    }

    public bool IsPlayerDefeated => _session.PlayerCurrentHp <= 0;

    public double LastImpactPresentationDurationSeconds => _lastImpactPresentationDurationSeconds;

    public bool TryMoveObject(string objectId, Vector2I targetCell, out string failureReason)
    {
        failureReason = string.Empty;
        Vector2I previousCell = Vector2I.Zero;
        if (_registry.TryGet(objectId, out BoardObject? existingObject) && existingObject != null)
        {
            previousCell = existingObject.Cell;
        }

        bool moved = _queryService.TryMoveObject(objectId, targetCell, out failureReason);
        if (!moved)
        {
            return false;
        }

        if (_registry.TryGet(objectId, out BoardObject? movedObject) && movedObject != null)
        {
            ApplyTerrainEffectsForMovement(movedObject, previousCell, targetCell);
        }

        SyncPresentation();
        _pieceViewManager.PlayMove(objectId);
        PublishActionLog($"{ResolveObjectDisplayName(objectId)}->({targetCell.X},{targetCell.Y}) 绉诲姩");
        return true;
    }

    public async Task<bool> TryMoveObjectAsync(string objectId, Vector2I targetCell)
    {
        if (!_registry.TryGet(objectId, out BoardObject? movingObject) || movingObject == null)
        {
            return false;
        }

        Vector2I previousCell = movingObject.Cell;
        IReadOnlyList<Vector2I> path = BuildMovePath(objectId, movingObject.Cell, targetCell);
        bool moved = _queryService.TryMoveObject(objectId, targetCell, out _);
        if (!moved)
        {
            return false;
        }

        bool animated = await _pieceViewManager.PlayMovePathAsync(objectId, path, _room, MovePresentationDurationSeconds);
        SyncPresentation();
        if (!animated)
        {
            _pieceViewManager.PlayMove(objectId);
            await WaitSeconds(MovePresentationDurationSeconds);
        }

        if (_registry.TryGet(objectId, out BoardObject? movedObject) && movedObject != null)
        {
            ApplyTerrainEffectsForMovement(movedObject, previousCell, targetCell);
        }

        PublishActionLog($"{ResolveObjectDisplayName(objectId)}->({targetCell.X},{targetCell.Y}) 绉诲姩");

        return true;
    }

    public bool TryAttackObject(string attackerId, string targetId, out bool wasDestroyed, out string failureReason, bool allowKillKnockback = false)
    {
        failureReason = string.Empty;
        wasDestroyed = false;

        if (!_registry.TryGet(attackerId, out BoardObject? attacker) || attacker == null)
        {
            failureReason = $"Attacker {attackerId} was not found.";
            return false;
        }

        if (!_registry.TryGet(targetId, out BoardObject? target) || target == null)
        {
            failureReason = $"Target {targetId} was not found.";
            return false;
        }

        BattleObjectState? attackerState = _stateManager.Get(attackerId);
        if (attackerState == null)
        {
            failureReason = $"Attacker state {attackerId} was not found.";
            return false;
        }

        if (!CanAttack(attacker, target, attackerState.AttackRange, out failureReason))
        {
            return false;
        }

        Vector2 attackDirection = new(target.Cell.X - attacker.Cell.X, target.Cell.Y - attacker.Cell.Y);
        PlayAttackPresentation(attacker, target, attackDirection);
        DamageApplicationResult result = ApplyDamageToTarget(target, attackerState.AttackDamage, attackDirection, allowKillKnockback);
        wasDestroyed = target.IsDestroyed;
        PublishActionLog($"{ResolveObjectDisplayName(attacker.ObjectId)}->{ResolveObjectDisplayName(target.ObjectId)} 鏀诲嚮{SumDamageImpactAmount(result)}");
        SyncPresentation();
        return true;
    }

    public async Task<bool> TryAttackObjectAsync(string attackerId, string targetId, bool allowKillKnockback = false)
    {
        bool attacked = TryAttackObject(attackerId, targetId, out _, out _, allowKillKnockback);
        if (!attacked)
        {
            return false;
        }

        await WaitSeconds(Math.Max(AttackPresentationDurationSeconds, GetEffectiveImpactPresentationDurationSeconds()));
        return true;
    }

    public DamageApplicationResult ApplyDamageToTarget(string targetId, int amount, Vector2? knockbackDirection, out bool wasDestroyed, out string failureReason, bool allowKillKnockback = false)
    {
        failureReason = string.Empty;
        wasDestroyed = false;

        if (!_registry.TryGet(targetId, out BoardObject? target) || target == null)
        {
            failureReason = $"Target {targetId} was not found.";
            return new DamageApplicationResult();
        }

        DamageApplicationResult result = ApplyDamageToTarget(target, amount, knockbackDirection ?? Vector2.Zero, allowKillKnockback);
        wasDestroyed = target.IsDestroyed;
        return result;
    }

    public DamageApplicationResult ApplyShieldGainToTarget(string targetId, int amount, out string failureReason)
    {
        failureReason = string.Empty;

        if (!_registry.TryGet(targetId, out BoardObject? target) || target == null)
        {
            failureReason = $"Target {targetId} was not found.";
            return new DamageApplicationResult();
        }

        DamageApplicationResult result = target.GainShield(amount);
        OnNonDamageImpactsApplied(target, result);
        SyncPresentation();
        return result;
    }

    public DamageApplicationResult ApplyHealingToTarget(string targetId, int amount, out string failureReason)
    {
        failureReason = string.Empty;

        if (!_registry.TryGet(targetId, out BoardObject? target) || target == null)
        {
            failureReason = $"Target {targetId} was not found.";
            return new DamageApplicationResult();
        }

        DamageApplicationResult result = target.RestoreHealth(amount);
        OnNonDamageImpactsApplied(target, result);
        SyncPresentation();
        return result;
    }

    public DamageApplicationResult ApplyDefenseAction(string objectId, DefenseActionDefinition definition, int currentTurnIndex, out string failureReason)
    {
        failureReason = string.Empty;

        if (!_registry.TryGet(objectId, out BoardObject? target) || target == null)
        {
            failureReason = $"Target {objectId} was not found.";
            return new DamageApplicationResult();
        }

        DamageApplicationResult result = target.EnterDefenseStance(currentTurnIndex, definition.DamageReductionPercent, definition.ShieldGain);
        _pieceViewManager.PlayCue(objectId, "defend");
        OnNonDamageImpactsApplied(target, result);
        SyncPresentation();
        return result;
    }

    public bool TryCreateArakawaBarrier(Vector2I targetCell, out string createdObjectId, out string failureReason)
    {
        BoardObjectSpawnDefinition spawn = new()
        {
            ObjectId = $"arakawa_wall_{Guid.NewGuid():N}"[..21],
            DefinitionId = "battle_obstacle_wall",
            ObjectType = BoardObjectType.Obstacle,
            Cell = targetCell,
            Faction = BoardObjectFaction.World,
            // 鑽掑窛閫犵墿闇€瑕侀樆鎸＄珯浣嶏紝浣嗕粛搴斿厑璁歌鏀诲嚮鎷嗛櫎锛岄伩鍏嶆棤闄愬牭姝绘晫浜哄鑷村崱鍏炽€?            Tags = new[] { "obstacle", "destructible", "arakawa_construct" },
            MaxHp = 3,
            CurrentHp = 3,
            BlocksMovement = true,
            BlocksLineOfSight = true,
            StackableWithUnit = false,
        };

        return TrySpawnBoardObject(spawn, out createdObjectId, out failureReason);
    }

    public bool TryCreateIndestructibleObstacle(Vector2I targetCell, out string createdObjectId, out string failureReason)
    {
        // 兼容旧调用点。当前荒川造墙已经统一为可破坏障碍物实现。
        return TryCreateArakawaBarrier(targetCell, out createdObjectId, out failureReason);
    }

    public bool TrySpawnBoardObject(BoardObjectSpawnDefinition spawn, out string createdObjectId, out string failureReason)
    {
        createdObjectId = string.Empty;
        failureReason = string.Empty;

        if (!_boardState.ContainsCell(spawn.Cell))
        {
            failureReason = $"Cell {spawn.Cell} is outside the board.";
            return false;
        }

        BoardObject boardObject = BoardObject.FromSpawn(spawn);
        if (!OccupancyRules.CanPlaceObject(_boardState, _registry, boardObject, spawn.Cell, out failureReason))
        {
            return false;
        }

        if (!_registry.Register(boardObject))
        {
            failureReason = $"Object {boardObject.ObjectId} could not be registered.";
            return false;
        }

        _boardState.PlaceObject(boardObject);
        SyncPresentation();
        _pieceViewManager.PlayTintPulse(boardObject.ObjectId, ResolveSpawnPulseColor(boardObject));
        createdObjectId = boardObject.ObjectId;
        return true;
    }

    public async Task<bool> TryCreateArakawaBarrierAsync(Vector2I targetCell)
    {
        bool created = TryCreateArakawaBarrier(targetCell, out _, out _);
        if (!created)
        {
            return false;
        }

        await WaitSeconds(UtilityPresentationDurationSeconds);
        return true;
    }

    public async Task<bool> TryCreateIndestructibleObstacleAsync(Vector2I targetCell)
    {
        // 兼容旧调用点。当前荒川造墙已经统一为可破坏障碍物实现。
        return await TryCreateArakawaBarrierAsync(targetCell);
    }

    public bool TryCreateArcTerrain(Vector2I centerCell, out string failureReason)
    {
        failureReason = string.Empty;
        bool changedAnyCell = false;
        foreach (Vector2I cell in EnumerateArcTerrainCells(centerCell))
        {
            if (!_boardState.ContainsCell(cell))
            {
                continue;
            }

            BoardCellState cellState = _boardState.GetCell(cell);
            if (string.Equals(cellState.TerrainId, ArcTerrainId, StringComparison.Ordinal))
            {
                continue;
            }

            _boardState.SetTerrain(cell, ArcTerrainId);
            changedAnyCell = true;
        }

        if (!changedAnyCell)
        {
            failureReason = "No cells were changed.";
            return false;
        }

        SyncPresentation();
        return true;
    }

    public async Task<bool> TryCreateArcTerrainAsync(Vector2I centerCell)
    {
        bool created = TryCreateArcTerrain(centerCell, out _);
        if (!created)
        {
            return false;
        }

        await WaitSeconds(UtilityPresentationDurationSeconds);
        return true;
    }

    public async Task<bool> TrySpawnBoardObjectAsync(BoardObjectSpawnDefinition spawn)
    {
        bool created = TrySpawnBoardObject(spawn, out _, out _);
        if (!created)
        {
            return false;
        }

        await WaitSeconds(UtilityPresentationDurationSeconds);
        return true;
    }

    public async Task ApplyDefenseActionAsync(string objectId, DefenseActionDefinition definition, int currentTurnIndex)
    {
        ApplyDefenseAction(objectId, definition, currentTurnIndex, out _);
        await WaitSeconds(Math.Max(DefensePresentationDurationSeconds, GetEffectiveImpactPresentationDurationSeconds()));
    }

    public void ResolveTurnStart(BoardObjectFaction activeFaction, int activeTurnIndex)
    {
        foreach (BoardObject boardObject in _registry.AllObjects)
        {
            if (boardObject.Faction == activeFaction)
            {
                boardObject.ResolveTurnStart(activeFaction, activeTurnIndex);
            }
        }

        foreach (BoardObject boardObject in _registry.AllObjects.Where(obj => obj.Faction == activeFaction && obj.ObjectType == BoardObjectType.Unit).ToArray())
        {
            ApplyArcTerrainStayEffect(boardObject);
        }

        SyncPresentation();
    }

    public void ResolveTurnEnd(BoardObjectFaction endingFaction, int endingTurnIndex)
    {
        foreach (BoardObject boardObject in _registry.AllObjects.Where(obj => obj.Faction == endingFaction && obj.ObjectType == BoardObjectType.Unit).ToArray())
        {
            ApplyFireTerrainStayEffect(boardObject);
        }

        SyncPresentation();
    }

    public BoardObject? GetAttackableObjectAtCell(string sourceObjectId, Vector2I targetCell)
    {
        if (!_registry.TryGet(sourceObjectId, out BoardObject? sourceObject) || sourceObject == null)
        {
            return null;
        }

        foreach (BoardObject boardObject in _queryService.GetObjectsAtCell(targetCell))
        {
            if (boardObject.ObjectId == sourceObjectId)
            {
                continue;
            }

            if (IsAttackable(sourceObject, boardObject))
            {
                return boardObject;
            }
        }

        return null;
    }

    public IReadOnlyList<BoardObject> FindAttackableTargetsInRange(string attackerId, Vector2I origin, int attackRange)
    {
        if (!_registry.TryGet(attackerId, out BoardObject? attacker) || attacker == null)
        {
            return Array.Empty<BoardObject>();
        }

        return _registry.AllObjects
            .Where(boardObject => boardObject.ObjectId != attackerId && IsAttackable(attacker, boardObject))
            .Where(boardObject => GetManhattanDistance(origin, boardObject.Cell) <= attackRange)
            .OrderBy(boardObject => GetManhattanDistance(origin, boardObject.Cell))
            .ThenBy(boardObject => boardObject.ObjectId, StringComparer.Ordinal)
            .ToArray();
    }

    public bool CanAttack(string attackerId, string targetId, out string failureReason)
    {
        failureReason = string.Empty;

        if (!_registry.TryGet(attackerId, out BoardObject? attacker) || attacker == null)
        {
            failureReason = $"Attacker {attackerId} was not found.";
            return false;
        }

        if (!_registry.TryGet(targetId, out BoardObject? target) || target == null)
        {
            failureReason = $"Target {targetId} was not found.";
            return false;
        }

        BattleObjectState? attackerState = _stateManager.Get(attackerId);
        if (attackerState == null)
        {
            failureReason = $"Attacker state {attackerId} was not found.";
            return false;
        }

        return CanAttack(attacker, target, attackerState.AttackRange, out failureReason);
    }

    public static bool IsAttackable(BoardObject sourceObject, BoardObject targetObject)
    {
        if (targetObject.ObjectType == BoardObjectType.Unit)
        {
            return sourceObject.Faction != targetObject.Faction;
        }

        if (targetObject.ObjectType == BoardObjectType.Obstacle)
        {
            return targetObject.HasTag("destructible");
        }

        return false;
    }

    private bool CanAttack(BoardObject attacker, BoardObject target, int attackRange, out string failureReason)
    {
        failureReason = string.Empty;

        if (!IsAttackable(attacker, target))
        {
            failureReason = "This target cannot be attacked.";
            return false;
        }

        int distance = GetManhattanDistance(attacker.Cell, target.Cell);
        if (distance > attackRange)
        {
            failureReason = $"Target is out of range. Range={attackRange}, distance={distance}.";
            return false;
        }

        return true;
    }

    private void SyncPresentation()
    {
        _stateManager.SyncAllFromRegistry();
        _pieceViewManager.Sync(_registry, _stateManager, _room);
    }

    private DamageApplicationResult ApplyDamageToTarget(BoardObject target, int amount, Vector2 knockbackDirection, bool allowKillKnockback = false)
    {
        bool isPlayerTarget = target.HasTag("player");
        DamageApplicationResult result = target.ApplyDamage(amount);
        _lastImpactPresentationDurationSeconds = CalculateImpactPresentationDurationSeconds(result);

        ShowImpacts(target, result);

        if (isPlayerTarget)
        {
            _session.SetPlayerCurrentHp(target.CurrentHp);
        }

        if (target.IsDestroyed)
        {
            if (!isPlayerTarget)
            {
                if (target.ObjectType == BoardObjectType.Unit && target.Faction == BoardObjectFaction.Enemy)
                {
                    EnemyDefeated?.Invoke(target.ObjectId, target.DefinitionId);
                }

                if (target.ObjectType == BoardObjectType.Unit)
                {
                    _ = _pieceViewManager.PlayKillSequenceAsync(
                        target.ObjectId,
                        allowKillKnockback ? knockbackDirection : Vector2.Zero,
                        allowKillKnockback ? KillKnockbackDistancePixels : 0.0f,
                        allowKillKnockback ? KillKnockbackPresentationDurationSeconds : 0.0d,
                        KillWhitenPresentationDurationSeconds,
                        KillShatterPresentationDurationSeconds);
                    _lastImpactPresentationDurationSeconds = Math.Max(
                        _lastImpactPresentationDurationSeconds,
                        (allowKillKnockback ? KillKnockbackPresentationDurationSeconds : 0.0d) + KillShatterPresentationDurationSeconds);
                }
                else if (target.ObjectType == BoardObjectType.Obstacle)
                {
                    _ = _pieceViewManager.PlayObstacleBreakSequenceAsync(
                        target.ObjectId,
                        ObstacleBreakWhitenPresentationDurationSeconds,
                        ObstacleBreakShatterPresentationDurationSeconds);
                    _lastImpactPresentationDurationSeconds = Math.Max(
                        _lastImpactPresentationDurationSeconds,
                        ObstacleBreakWhitenPresentationDurationSeconds + ObstacleBreakShatterPresentationDurationSeconds);
                }
                _boardState.RemoveObject(target);
                _registry.Remove(target.ObjectId);
            }
        }
        else if (result.HasAnyImpact)
        {
            _pieceViewManager.PlayHit(target.ObjectId);
        }

        return result;
    }

    private void OnNonDamageImpactsApplied(BoardObject target, DamageApplicationResult result)
    {
        if (!result.HasAnyImpact)
        {
            _lastImpactPresentationDurationSeconds = 0.0d;
            return;
        }

        _lastImpactPresentationDurationSeconds = CalculateImpactPresentationDurationSeconds(result);

        if (target.HasTag("player"))
        {
            _session.SetPlayerCurrentHp(target.CurrentHp);
        }

        ShowImpacts(target, result);
    }

    private void ShowImpacts(BoardObject target, DamageApplicationResult result)
    {
        if (!result.HasAnyImpact)
        {
            return;
        }

        _floatingTextLayer?.ShowImpacts(
            target.ObjectId,
            _room.CellToLocalCenter(target.Cell) + new Vector2(0.0f, -6.0f),
            result.Impacts);
    }

    private void PlayAttackPresentation(BoardObject attacker, BoardObject target, Vector2 direction)
    {
        _pieceViewManager.PlayAttackExchange(attacker.ObjectId, direction, target.ObjectId);
    }

    private async Task WaitSeconds(double seconds)
    {
        if (seconds <= 0.0d)
        {
            return;
        }

        await _room.ToSignal(_sceneTree.CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);
    }

    private void ApplyTerrainEffectsForMovement(BoardObject boardObject, Vector2I previousCell, Vector2I targetCell)
    {
        if (boardObject.ObjectType != BoardObjectType.Unit || boardObject.IsDestroyed)
        {
            return;
        }

        if (previousCell != targetCell && IsArcTerrain(previousCell))
        {
            ApplyArcTerrainDamage(boardObject, "离开");
        }

        if (!boardObject.IsDestroyed && previousCell != targetCell && IsArcTerrain(targetCell))
        {
            ApplyArcTerrainDamage(boardObject, "进入");
        }
    }

    private void ApplyArcTerrainStayEffect(BoardObject boardObject)
    {
        if (boardObject.ObjectType != BoardObjectType.Unit || boardObject.IsDestroyed)
        {
            return;
        }

        if (IsArcTerrain(boardObject.Cell))
        {
            ApplyArcTerrainDamage(boardObject, "停留");
        }
    }

    private void ApplyFireTerrainStayEffect(BoardObject boardObject)
    {
        if (boardObject.ObjectType != BoardObjectType.Unit || boardObject.IsDestroyed)
        {
            return;
        }

        if (IsFireTerrain(boardObject.Cell))
        {
            ApplyFireTerrainDamage(boardObject);
        }
    }

    private void ApplyArcTerrainDamage(BoardObject boardObject, string triggerLabel)
    {
        DamageApplicationResult result = ApplyDamageToTarget(boardObject, ArcTerrainDamage, Vector2.Zero, allowKillKnockback: false);
        int damageAmount = result.Impacts
            .Where(impact => impact.ImpactType is CombatImpactType.HealthDamage or CombatImpactType.ShieldDamage)
            .Sum(impact => impact.Amount);
        if (damageAmount > 0)
        {
            PublishActionLog($"{ResolveObjectDisplayName(boardObject.ObjectId)}->电弧{triggerLabel}{damageAmount}");
        }
    }

    private void ApplyFireTerrainDamage(BoardObject boardObject)
    {
        DamageApplicationResult result = ApplyDamageToTarget(boardObject, FireTerrainDamage, Vector2.Zero, allowKillKnockback: false);
        int damageAmount = result.Impacts
            .Where(impact => impact.ImpactType is CombatImpactType.HealthDamage or CombatImpactType.ShieldDamage)
            .Sum(impact => impact.Amount);
        if (damageAmount > 0)
        {
            PublishActionLog($"{ResolveObjectDisplayName(boardObject.ObjectId)}->火焰停留{damageAmount}");
        }
    }

    private bool IsArcTerrain(Vector2I cell)
    {
        return _boardState.TryGetCell(cell, out BoardCellState? cellState)
            && cellState != null
            && string.Equals(cellState.TerrainId, ArcTerrainId, StringComparison.Ordinal);
    }

    private bool IsFireTerrain(Vector2I cell)
    {
        return _boardState.TryGetCell(cell, out BoardCellState? cellState)
            && cellState != null
            && string.Equals(cellState.TerrainId, FireTerrainId, StringComparison.Ordinal);
    }

    private static IEnumerable<Vector2I> EnumerateArcTerrainCells(Vector2I centerCell)
    {
        yield return centerCell;
        foreach (Vector2I direction in BoardTopology.CardinalDirections)
        {
            yield return centerCell + direction;
        }
    }

    private static int GetManhattanDistance(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    private IReadOnlyList<Vector2I> BuildMovePath(string objectId, Vector2I startCell, Vector2I targetCell)
    {
        if (_pathfinder.TryFindPath(objectId, startCell, targetCell, int.MaxValue / 8, out IReadOnlyList<Vector2I> path, out _)
            && path.Count > 0)
        {
            return path;
        }

        return new[] { startCell, targetCell };
    }

    private static Color ResolveSpawnPulseColor(BoardObject boardObject)
    {
        return boardObject.Faction switch
        {
            BoardObjectFaction.Player => new Color(0.36f, 0.92f, 0.58f, 1.0f),
            BoardObjectFaction.Enemy => new Color(0.96f, 0.36f, 0.36f, 1.0f),
            _ => new Color(0.26f, 0.74f, 1.0f, 1.0f),
        };
    }

    private double GetEffectiveImpactPresentationDurationSeconds()
    {
        return Math.Max(ImpactPresentationDurationSeconds, _lastImpactPresentationDurationSeconds * ImpactFlowWaitRatio);
    }

    private double CalculateImpactPresentationDurationSeconds(DamageApplicationResult result)
    {
        if (!result.HasAnyImpact)
        {
            return 0.0d;
        }

        return _floatingTextLayer?.GetImpactSequenceDurationSeconds(result.Impacts)
            ?? ImpactPresentationDurationSeconds;
    }

    private void PublishActionLog(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return;
        }

        ActionLogged?.Invoke(line);
    }

    private string ResolveObjectDisplayName(string objectId)
    {
        return _stateManager.Get(objectId)?.DisplayName ?? objectId;
    }

    private static int SumDamageImpactAmount(DamageApplicationResult result)
    {
        return result.Impacts
            .Where(impact => impact.ImpactType is CombatImpactType.HealthDamage or CombatImpactType.ShieldDamage)
            .Sum(impact => impact.Amount);
    }
}

