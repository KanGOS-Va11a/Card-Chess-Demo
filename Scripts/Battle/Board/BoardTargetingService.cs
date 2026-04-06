using System;
using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Battle.Board;

public sealed class BoardTargetingService
{
    private readonly BoardTopology _topology;
    private readonly BoardObjectRegistry _registry;
    private readonly BoardQueryService _queryService;

    public BoardTargetingService(BoardTopology topology, BoardObjectRegistry registry, BoardQueryService queryService)
    {
        _topology = topology;
        _registry = registry;
        _queryService = queryService;
    }

    public bool TryFindFirstEnemyInDirection(
        string sourceObjectId,
        Vector2I direction,
        int maxRange,
        out BoardObject? targetObject,
        out IReadOnlyList<Vector2I> traversedCells)
    {
        targetObject = null;
        List<Vector2I> lineCells = new();
        traversedCells = lineCells;

        if (maxRange <= 0)
        {
            return false;
        }

        if (!_registry.TryGet(sourceObjectId, out BoardObject? sourceObject) || sourceObject == null)
        {
            return false;
        }

        if (!_topology.TryNormalizeCardinalDirection(direction, out Vector2I normalizedDirection))
        {
            return false;
        }

        Vector2I currentCell = sourceObject.Cell;
        for (int step = 0; step < maxRange; step++)
        {
            currentCell += normalizedDirection;
            if (!_topology.IsInsideBoard(currentCell))
            {
                break;
            }

            lineCells.Add(currentCell);

            foreach (BoardObject boardObject in _queryService.GetObjectsAtCell(currentCell))
            {
                if (boardObject.ObjectId == sourceObjectId)
                {
                    continue;
                }

                if (boardObject.ObjectType == BoardObjectType.Unit)
                {
                    if (IsEnemy(sourceObject, boardObject))
                    {
                        targetObject = boardObject;
                        return true;
                    }

                    return false;
                }

                if (boardObject.ObjectType == BoardObjectType.Obstacle)
                {
                    if (boardObject.HasTag("destructible"))
                    {
                        targetObject = boardObject;
                        return true;
                    }

                    if (boardObject.BlocksLineOfSight)
                    {
                        return false;
                    }
                }

                if (boardObject.BlocksLineOfSight)
                {
                    return false;
                }
            }
        }

        return false;
    }

    public IReadOnlyDictionary<Vector2I, BoardObject> FindEnemiesInStraightLines(string sourceObjectId, int maxRange)
    {
        Dictionary<Vector2I, BoardObject> targetsByDirection = new();

        foreach (Vector2I direction in BoardTopology.CardinalDirections)
        {
            if (TryFindFirstEnemyInDirection(sourceObjectId, direction, maxRange, out BoardObject? targetObject, out _)
                && targetObject != null)
            {
                targetsByDirection[direction] = targetObject;
            }
        }

        return targetsByDirection;
    }

    private static bool IsEnemy(BoardObject sourceObject, BoardObject targetObject)
    {
        if (sourceObject.Faction == BoardObjectFaction.None || targetObject.Faction == BoardObjectFaction.None)
        {
            return false;
        }

        return sourceObject.Faction != targetObject.Faction;
    }
}
