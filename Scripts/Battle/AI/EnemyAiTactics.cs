using System;
using System.Collections.Generic;
using System.Linq;
using CardChessDemo.Battle.Board;
using Godot;

namespace CardChessDemo.Battle.AI;

internal static class EnemyAiTactics
{
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

    public static Vector2I? FindBestApproachCell(
        EnemyAiContext context,
        BoardObject target,
        int desiredMaxRange,
        int desiredMinRange = 1,
        bool preferFlank = true,
        int? moveBudgetOverride = null)
    {
        int moveBudget = moveBudgetOverride ?? context.SelfState.MovePointsPerTurn;
        IEnumerable<Vector2I> reachableCells = context.Pathfinder
            .FindReachableCells(context.Self.ObjectId, context.Self.Cell, moveBudget)
            .Where(cell => cell != context.Self.Cell);

        Vector2I[] desiredCells = EnumerateDesiredRangeCells(target.Cell, desiredMinRange, desiredMaxRange).ToArray();
        int searchBudget = 64;

        return reachableCells
            .Select(cell => BuildApproachCandidate(context, cell, target.Cell, desiredCells, desiredMinRange, desiredMaxRange, preferFlank, searchBudget))
            .Where(candidate => candidate.HasFutureRoute)
            .OrderByDescending(candidate => candidate.InDesiredRange)
            .ThenByDescending(candidate => candidate.FlankScore)
            .ThenBy(candidate => candidate.FuturePathCost)
            .ThenBy(candidate => candidate.RangeDeviation)
            .ThenBy(candidate => candidate.Cell.Y)
            .ThenBy(candidate => candidate.Cell.X)
            .Select(candidate => (Vector2I?)candidate.Cell)
            .FirstOrDefault();
    }

    public static int GetManhattanDistance(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }

    private static ApproachCandidate BuildApproachCandidate(
        EnemyAiContext context,
        Vector2I cell,
        Vector2I targetCell,
        IReadOnlyList<Vector2I> desiredCells,
        int desiredMinRange,
        int desiredMaxRange,
        bool preferFlank,
        int searchBudget)
    {
        int distance = GetManhattanDistance(cell, targetCell);
        bool inDesiredRange = distance >= desiredMinRange && distance <= desiredMaxRange;
        int flankScore = preferFlank && cell.X != targetCell.X && cell.Y != targetCell.Y ? 1 : 0;
        int targetDistance = Mathf.Clamp(distance, desiredMinRange, desiredMaxRange);
        int rangeDeviation = Mathf.Abs(distance - targetDistance);
        int futurePathCost = ResolveFuturePathCost(context, cell, desiredCells, searchBudget);
        bool hasFutureRoute = futurePathCost < int.MaxValue;
        return new ApproachCandidate(cell, distance, flankScore, inDesiredRange, rangeDeviation, futurePathCost, hasFutureRoute);
    }

    private static int ResolveFuturePathCost(EnemyAiContext context, Vector2I startCell, IReadOnlyList<Vector2I> desiredCells, int searchBudget)
    {
        int bestCost = int.MaxValue;
        foreach (Vector2I desiredCell in desiredCells)
        {
            if (startCell == desiredCell)
            {
                return 0;
            }

            if (context.Pathfinder.TryFindPath(context.Self.ObjectId, startCell, desiredCell, searchBudget, out _, out int totalCost))
            {
                bestCost = Math.Min(bestCost, totalCost);
            }
        }

        return bestCost;
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

    private readonly record struct ApproachCandidate(
        Vector2I Cell,
        int Distance,
        int FlankScore,
        bool InDesiredRange,
        int RangeDeviation,
        int FuturePathCost,
        bool HasFutureRoute);
}
