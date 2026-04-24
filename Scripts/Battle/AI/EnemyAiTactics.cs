using System;
using System.Collections.Generic;
using System.Linq;
using CardChessDemo.Battle.Board;
using Godot;

namespace CardChessDemo.Battle.AI;

internal static class EnemyAiTactics
{
    public readonly record struct PathBlockAnalysis(
        bool HasOpenRoute,
        Vector2I? MoveCell,
        BoardObject? BlockingObstacle);

    public static bool TryFindStraightLineTarget(
        EnemyAiContext context,
        int maxRange,
        out BoardObject? target,
        out Vector2I direction,
        out Vector2I[] traversedCells)
    {
        target = null;
        direction = Vector2I.Zero;
        traversedCells = Array.Empty<Vector2I>();

        foreach (Vector2I candidateDirection in BoardTopology.CardinalDirections)
        {
            if (context.TargetingService.TryFindFirstEnemyInDirection(context.Self.ObjectId, candidateDirection, maxRange, out BoardObject? candidateTarget, out IReadOnlyList<Vector2I> cells)
                && candidateTarget != null)
            {
                target = candidateTarget;
                direction = candidateDirection;
                traversedCells = cells.ToArray();
                return true;
            }
        }

        return false;
    }

    public static BoardObject? FindNearestOpponentUnit(EnemyAiContext context)
    {
        return context.Registry.AllObjects
            .Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit)
            .Where(boardObject => boardObject.ObjectId != context.Self.ObjectId)
            .Where(boardObject => boardObject.Faction != context.Self.Faction)
            .OrderBy(boardObject => GetManhattanDistance(context.Self.Cell, boardObject.Cell))
            .ThenBy(boardObject => boardObject.ObjectId, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static BoardObject? FindPreferredAttackTarget(EnemyAiContext context)
    {
        IReadOnlyList<BoardObject> targets = context.ActionService.FindAttackableTargetsInRange(
            context.Self.ObjectId,
            context.Self.Cell,
            context.SelfState.AttackRange);

        return targets
            .OrderBy(target => target.ObjectType == BoardObjectType.Unit ? 0 : 1)
            .ThenBy(target => GetManhattanDistance(context.Self.Cell, target.Cell))
            .ThenBy(target => target.ObjectId, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static BoardObject? FindOpponentAttackTargetInRange(EnemyAiContext context)
    {
        IReadOnlyList<BoardObject> targets = context.ActionService.FindAttackableTargetsInRange(
            context.Self.ObjectId,
            context.Self.Cell,
            context.SelfState.AttackRange);

        return targets
            .Where(target => target.ObjectType == BoardObjectType.Unit)
            .OrderBy(target => GetManhattanDistance(context.Self.Cell, target.Cell))
            .ThenBy(target => target.ObjectId, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static BoardObject? FindBestSupportTargetInRange(EnemyAiContext context, int maxRange)
    {
        return context.Registry.AllObjects
            .Where(boardObject => boardObject.ObjectId != context.Self.ObjectId)
            .Where(boardObject => boardObject.Faction == context.Self.Faction)
            .Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit || boardObject.ObjectType == BoardObjectType.Obstacle)
            .Select(boardObject => new
            {
                Object = boardObject,
                State = context.StateManager.Get(boardObject.ObjectId),
                Distance = GetManhattanDistance(context.Self.Cell, boardObject.Cell),
            })
            .Where(candidate => candidate.State != null)
            .Where(candidate => candidate.Distance <= maxRange)
            .Where(candidate => candidate.State!.CurrentHp < candidate.State.MaxHp || candidate.State.CurrentShield < candidate.State.MaxShield)
            .OrderBy(candidate => candidate.Distance)
            .ThenBy(candidate => candidate.State!.CurrentHp)
            .ThenBy(candidate => candidate.State!.CurrentShield)
            .Select(candidate => candidate.Object)
            .FirstOrDefault();
    }

    public static Vector2I? FindBestApproachCell(
        EnemyAiContext context,
        BoardObject target,
        int desiredMaxRange,
        int desiredMinRange = 1,
        bool preferFlank = true,
        int? moveBudgetOverride = null)
    {
        PathBlockAnalysis analysis = AnalyzePathToTarget(
            context,
            target,
            desiredMaxRange,
            desiredMinRange,
            preferFlank,
            moveBudgetOverride);

        return analysis.HasOpenRoute ? analysis.MoveCell : null;
    }

    public static PathBlockAnalysis AnalyzePathToTarget(
        EnemyAiContext context,
        BoardObject target,
        int desiredMaxRange,
        int desiredMinRange = 1,
        bool preferFlank = true,
        int? moveBudgetOverride = null)
    {
        int moveBudget = moveBudgetOverride ?? context.SelfState.MovePointsPerTurn;
        Vector2I[] desiredCells = EnumerateDesiredRangeCells(target.Cell, desiredMinRange, desiredMaxRange).ToArray();
        if (TryFindPathToDesiredCells(
                context,
                context.Self.Cell,
                target.Cell,
                desiredCells,
                ignoreDestructibleObstacles: false,
                preferFlank,
                out IReadOnlyList<Vector2I> openPath))
        {
            return new PathBlockAnalysis(
                HasOpenRoute: true,
                MoveCell: ResolveMoveDestinationWithinBudget(context, openPath, moveBudget),
                BlockingObstacle: null);
        }

        if (TryFindPathToDesiredCells(
                context,
                context.Self.Cell,
                target.Cell,
                desiredCells,
                ignoreDestructibleObstacles: true,
                preferFlank,
                out IReadOnlyList<Vector2I> softPath))
        {
            return new PathBlockAnalysis(
                HasOpenRoute: false,
                MoveCell: null,
                BlockingObstacle: FindFirstDestructibleObstacleOnPath(context, softPath));
        }

        return new PathBlockAnalysis(false, null, null);
    }

    public static int GetManhattanDistance(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    public static EnemyAiDecision DecideChasePlayerOrBreakBlockingObstacle(
        EnemyAiContext context,
        BoardObject target,
        int desiredMaxRange,
        int desiredMinRange = 1,
        bool preferFlank = true,
        int? moveBudgetOverride = null)
    {
        PathBlockAnalysis analysis = AnalyzePathToTarget(
            context,
            target,
            desiredMaxRange,
            desiredMinRange,
            preferFlank,
            moveBudgetOverride);

        if (analysis.HasOpenRoute)
        {
            return analysis.MoveCell.HasValue
                ? EnemyAiDecision.Move(analysis.MoveCell.Value, target.ObjectId)
                : EnemyAiDecision.Wait();
        }

        if (analysis.BlockingObstacle == null)
        {
            return EnemyAiDecision.Wait();
        }

        if (GetManhattanDistance(context.Self.Cell, analysis.BlockingObstacle.Cell) <= context.SelfState.AttackRange)
        {
            return EnemyAiDecision.Attack(analysis.BlockingObstacle.ObjectId);
        }

        Vector2I? obstacleApproachCell = FindBestApproachCell(
            context,
            analysis.BlockingObstacle,
            desiredMaxRange: context.SelfState.AttackRange,
            desiredMinRange: 1,
            preferFlank: false,
            moveBudgetOverride: moveBudgetOverride);

        return obstacleApproachCell.HasValue
            ? EnemyAiDecision.Move(obstacleApproachCell.Value, analysis.BlockingObstacle.ObjectId)
            : EnemyAiDecision.Wait();
    }

    private static bool TryFindPathToDesiredCells(
        EnemyAiContext context,
        Vector2I startCell,
        Vector2I targetCell,
        IReadOnlyList<Vector2I> desiredCells,
        bool ignoreDestructibleObstacles,
        bool preferFlank,
        out IReadOnlyList<Vector2I> bestPath)
    {
        bestPath = Array.Empty<Vector2I>();
        int bestCost = int.MaxValue;
        int bestFlankScore = int.MinValue;
        Vector2I bestDestination = Vector2I.Zero;

        foreach (Vector2I desiredCell in desiredCells)
        {
            if (!TryFindPathForAnalysis(context, startCell, desiredCell, ignoreDestructibleObstacles, out IReadOnlyList<Vector2I> candidatePath, out int candidateCost))
            {
                continue;
            }

            int flankScore = preferFlank && desiredCell.X != targetCell.X && desiredCell.Y != targetCell.Y ? 1 : 0;
            if (candidateCost < bestCost
                || (candidateCost == bestCost && flankScore > bestFlankScore)
                || (candidateCost == bestCost && flankScore == bestFlankScore && CompareCells(desiredCell, bestDestination) < 0))
            {
                bestCost = candidateCost;
                bestFlankScore = flankScore;
                bestDestination = desiredCell;
                bestPath = candidatePath;
            }
        }

        return bestPath.Count > 0;
    }

    private static bool TryFindPathForAnalysis(
        EnemyAiContext context,
        Vector2I startCell,
        Vector2I targetCell,
        bool ignoreDestructibleObstacles,
        out IReadOnlyList<Vector2I> path,
        out int totalCost)
    {
        path = Array.Empty<Vector2I>();
        totalCost = 0;
        if (!context.Room.Topology.IsInsideBoard(startCell) || !context.Room.Topology.IsInsideBoard(targetCell))
        {
            return false;
        }

        if (startCell == targetCell)
        {
            path = new[] { startCell };
            return true;
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
                path = ReconstructPath(cameFrom, currentCell);
                totalCost = CalculatePathCost(context, path, ignoreDestructibleObstacles);
                return true;
            }

            int currentCost = gScore[currentCell];
            foreach (Vector2I neighborCell in context.Room.Topology.EnumerateCardinalNeighbors(currentCell))
            {
                if (!CanTraverseCellForAnalysis(context, neighborCell, ignoreDestructibleObstacles))
                {
                    continue;
                }

                int nextCost = currentCost + GetTraversalStepCostForAnalysis(context, startCell, currentCell, neighborCell, ignoreDestructibleObstacles);
                if (gScore.TryGetValue(neighborCell, out int existingCost) && existingCost <= nextCost)
                {
                    continue;
                }

                gScore[neighborCell] = nextCost;
                cameFrom[neighborCell] = currentCell;
                frontier.Enqueue(neighborCell, nextCost + EstimateRemainingCost(neighborCell, targetCell));
            }
        }

        return false;
    }

    private static IEnumerable<Vector2I> EnumerateDesiredRangeCells(Vector2I targetCell, int desiredMinRange, int desiredMaxRange)
    {
        for (int y = -desiredMaxRange; y <= desiredMaxRange; y++)
        {
            for (int x = -desiredMaxRange; x <= desiredMaxRange; x++)
            {
                Vector2I cell = targetCell + new Vector2I(x, y);
                int distance = GetManhattanDistance(cell, targetCell);
                if (distance >= desiredMinRange && distance <= desiredMaxRange)
                {
                    yield return cell;
                }
            }
        }
    }

    private static Vector2I? ResolveMoveDestinationWithinBudget(EnemyAiContext context, IReadOnlyList<Vector2I> path, int moveBudget)
    {
        if (path.Count <= 1 || moveBudget <= 0)
        {
            return null;
        }

        int accumulatedCost = 0;
        Vector2I originCell = path[0];
        Vector2I currentCell = originCell;
        Vector2I lastReachableCell = originCell;
        bool moved = false;
        for (int index = 1; index < path.Count; index++)
        {
            Vector2I nextCell = path[index];
            int stepCost = GetTraversalStepCostForAnalysis(context, originCell, currentCell, nextCell, ignoreDestructibleObstacles: false);
            if (accumulatedCost + stepCost > moveBudget)
            {
                break;
            }

            accumulatedCost += stepCost;
            currentCell = nextCell;
            lastReachableCell = nextCell;
            moved = true;
        }

        return moved ? lastReachableCell : null;
    }

    private static BoardObject? FindFirstDestructibleObstacleOnPath(EnemyAiContext context, IReadOnlyList<Vector2I> path)
    {
        for (int index = 1; index < path.Count; index++)
        {
            Vector2I cell = path[index];
            BoardObject? obstacle = context.QueryService.GetObjectsAtCell(cell)
                .Where(boardObject => boardObject.ObjectType == BoardObjectType.Obstacle)
                .Where(boardObject => boardObject.HasTag("destructible"))
                .OrderBy(boardObject => boardObject.ObjectId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (obstacle != null)
            {
                return obstacle;
            }
        }

        return null;
    }

    private static bool CanTraverseCellForAnalysis(EnemyAiContext context, Vector2I cell, bool ignoreDestructibleObstacles)
    {
        if (!context.Room.Topology.IsInsideBoard(cell))
        {
            return false;
        }

        foreach (BoardObject occupant in context.QueryService.GetObjectsAtCell(cell))
        {
            if (occupant.ObjectId == context.Self.ObjectId)
            {
                continue;
            }

            if (occupant.ObjectType == BoardObjectType.Unit)
            {
                return false;
            }

            if (occupant.ObjectType == BoardObjectType.Obstacle)
            {
                if (ignoreDestructibleObstacles && occupant.HasTag("destructible"))
                {
                    continue;
                }

                if (occupant.BlocksMovement || !occupant.StackableWithUnit)
                {
                    return false;
                }
            }
            else if (!occupant.StackableWithUnit)
            {
                return false;
            }
        }

        return true;
    }

    private static int GetTraversalStepCostForAnalysis(
        EnemyAiContext context,
        Vector2I originCell,
        Vector2I currentCell,
        Vector2I nextCell,
        bool ignoreDestructibleObstacles)
    {
        int nextCellCost = GetMoveCostForAnalysis(context, nextCell, ignoreDestructibleObstacles);
        if (currentCell != originCell)
        {
            return nextCellCost;
        }

        return nextCellCost + Math.Max(0, GetMoveCostForAnalysis(context, originCell, ignoreDestructibleObstacles) - 1);
    }

    private static int GetMoveCostForAnalysis(EnemyAiContext context, Vector2I cell, bool ignoreDestructibleObstacles)
    {
        int moveCost = 1;
        foreach (BoardObject occupant in context.QueryService.GetObjectsAtCell(cell))
        {
            if (occupant.ObjectType == BoardObjectType.Obstacle && ignoreDestructibleObstacles && occupant.HasTag("destructible"))
            {
                continue;
            }

            moveCost += occupant.MoveCostModifier;
        }

        return Math.Max(0, moveCost);
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

    private static int CalculatePathCost(EnemyAiContext context, IReadOnlyList<Vector2I> path, bool ignoreDestructibleObstacles)
    {
        if (path.Count <= 1)
        {
            return 0;
        }

        int totalCost = Math.Max(0, GetMoveCostForAnalysis(context, path[0], ignoreDestructibleObstacles) - 1);
        for (int index = 1; index < path.Count; index++)
        {
            totalCost += GetMoveCostForAnalysis(context, path[index], ignoreDestructibleObstacles);
        }

        return totalCost;
    }

    private static int EstimateRemainingCost(Vector2I currentCell, Vector2I targetCell)
    {
        return Mathf.Abs(targetCell.X - currentCell.X) + Mathf.Abs(targetCell.Y - currentCell.Y);
    }

    private static int CompareCells(Vector2I a, Vector2I b)
    {
        int yCompare = a.Y.CompareTo(b.Y);
        return yCompare != 0 ? yCompare : a.X.CompareTo(b.X);
    }
}
