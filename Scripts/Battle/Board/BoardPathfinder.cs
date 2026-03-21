using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Board;

public sealed class BoardPathfinder
{
    private readonly BoardTopology _topology;
    private readonly BoardQueryService _queryService;

    public BoardPathfinder(BoardTopology topology, BoardQueryService queryService)
    {
        _topology = topology;
        _queryService = queryService;
    }

    public IReadOnlyDictionary<Vector2I, int> ComputeReachableCosts(string objectId, Vector2I originCell, int moveBudget)
    {
        Dictionary<Vector2I, int> bestCosts = new();
        PriorityQueue<Vector2I, int> frontier = new();

        bestCosts[originCell] = 0;
        frontier.Enqueue(originCell, 0);

        while (frontier.Count > 0)
        {
            frontier.TryDequeue(out Vector2I currentCell, out int currentCost);

            if (!bestCosts.TryGetValue(currentCell, out int bestKnownCost) || currentCost > bestKnownCost)
            {
                continue;
            }

            foreach (Vector2I neighborCell in _topology.EnumerateCardinalNeighbors(currentCell))
            {
                if (!_queryService.CanOccupyCell(objectId, neighborCell, out _))
                {
                    continue;
                }

                int moveCost = _queryService.GetMoveCost(neighborCell);
                int nextCost = currentCost + moveCost;
                if (nextCost > moveBudget)
                {
                    continue;
                }

                if (bestCosts.TryGetValue(neighborCell, out int existingCost) && existingCost <= nextCost)
                {
                    continue;
                }

                bestCosts[neighborCell] = nextCost;
                frontier.Enqueue(neighborCell, nextCost);
            }
        }

        return bestCosts;
    }

    public IReadOnlyList<Vector2I> FindReachableCells(string objectId, Vector2I originCell, int moveBudget)
    {
        return ComputeReachableCosts(objectId, originCell, moveBudget)
            .Keys
            .OrderBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .ToArray();
    }

    public bool TryFindPath(
        string objectId,
        Vector2I startCell,
        Vector2I targetCell,
        int moveBudget,
        out IReadOnlyList<Vector2I> path,
        out int totalCost)
    {
        path = Array.Empty<Vector2I>();
        totalCost = 0;

        if (!_topology.IsInsideBoard(startCell) || !_topology.IsInsideBoard(targetCell))
        {
            return false;
        }

        if (startCell == targetCell)
        {
            path = new[] { startCell };
            return true;
        }

        if (!_queryService.CanOccupyCell(objectId, targetCell, out _))
        {
            return false;
        }

        Dictionary<Vector2I, int> gScore = new();
        Dictionary<Vector2I, Vector2I> cameFrom = new();
        PriorityQueue<Vector2I, int> frontier = new();

        gScore[startCell] = 0;
        frontier.Enqueue(startCell, EstimateRemainingCost(startCell, targetCell));

        while (frontier.Count > 0)
        {
            frontier.TryDequeue(out Vector2I currentCell, out _);

            if (currentCell == targetCell)
            {
                totalCost = gScore[currentCell];
                path = ReconstructPath(cameFrom, currentCell);
                return totalCost <= moveBudget;
            }

            int currentCost = gScore[currentCell];

            foreach (Vector2I neighborCell in _topology.EnumerateCardinalNeighbors(currentCell))
            {
                if (!_queryService.CanOccupyCell(objectId, neighborCell, out _))
                {
                    continue;
                }

                int nextCost = currentCost + _queryService.GetMoveCost(neighborCell);
                if (nextCost > moveBudget)
                {
                    continue;
                }

                if (gScore.TryGetValue(neighborCell, out int existingCost) && existingCost <= nextCost)
                {
                    continue;
                }

                gScore[neighborCell] = nextCost;
                cameFrom[neighborCell] = currentCell;
                int priority = nextCost + EstimateRemainingCost(neighborCell, targetCell);
                frontier.Enqueue(neighborCell, priority);
            }
        }

        return false;
    }

    private static IReadOnlyList<Vector2I> ReconstructPath(Dictionary<Vector2I, Vector2I> cameFrom, Vector2I currentCell)
    {
        List<Vector2I> path = new() { currentCell };

        while (cameFrom.TryGetValue(currentCell, out Vector2I previousCell))
        {
            currentCell = previousCell;
            path.Add(currentCell);
        }

        path.Reverse();
        return path;
    }

    private static int EstimateRemainingCost(Vector2I currentCell, Vector2I targetCell)
    {
        return Mathf.Abs(targetCell.X - currentCell.X) + Mathf.Abs(targetCell.Y - currentCell.Y);
    }
}
