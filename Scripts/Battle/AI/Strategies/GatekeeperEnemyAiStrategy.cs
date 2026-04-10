using System;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Board;

namespace CardChessDemo.Battle.AI.Strategies;

public sealed class GatekeeperEnemyAiStrategy : IEnemyAiStrategy
{
    public string AiId => "gatekeeper_guard";

    public EnemyAiDecision Decide(EnemyAiContext context)
    {
        BoardObject? attackTarget = context.ActionService
            .FindAttackableTargetsInRange(context.Self.ObjectId, context.Self.Cell, context.SelfState.AttackRange)
            .FirstOrDefault();
        if (attackTarget != null)
        {
            return EnemyAiDecision.Attack(attackTarget.ObjectId);
        }

        BoardObject? nearestOpponent = FindNearestOpponent(context);
        if (nearestOpponent == null)
        {
            return EnemyAiDecision.Wait();
        }

        Vector2I? nextCell = context.Pathfinder
            .FindReachableCells(context.Self.ObjectId, context.Self.Cell, context.SelfState.MovePointsPerTurn)
            .Where(cell => cell != context.Self.Cell)
            .Select(cell => new
            {
                Cell = cell,
                Distance = GetManhattanDistance(cell, nearestOpponent.Cell),
                AdjacentObstacle = HasAdjacentObstacle(context, cell) ? 1 : 0,
            })
            .OrderByDescending(candidate => candidate.AdjacentObstacle)
            .ThenBy(candidate => candidate.Distance)
            .ThenBy(candidate => candidate.Cell.Y)
            .ThenBy(candidate => candidate.Cell.X)
            .Select(candidate => (Vector2I?)candidate.Cell)
            .FirstOrDefault();

        return nextCell.HasValue ? EnemyAiDecision.Move(nextCell.Value) : EnemyAiDecision.Wait();
    }

    private static bool HasAdjacentObstacle(EnemyAiContext context, Vector2I cell)
    {
        return BoardTopology.CardinalDirections
            .Select(direction => cell + direction)
            .Any(neighbor => context.Registry.AllObjects.Any(boardObject =>
                boardObject.Cell == neighbor && boardObject.ObjectType == BoardObjectType.Obstacle));
    }

    private static BoardObject? FindNearestOpponent(EnemyAiContext context)
    {
        return context.Registry.AllObjects
            .Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit)
            .Where(boardObject => boardObject.ObjectId != context.Self.ObjectId)
            .Where(boardObject => boardObject.Faction != context.Self.Faction)
            .OrderBy(boardObject => GetManhattanDistance(context.Self.Cell, boardObject.Cell))
            .ThenBy(boardObject => boardObject.ObjectId, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static int GetManhattanDistance(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }
}
