using System;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Board;

namespace CardChessDemo.Battle.AI.Strategies;

public sealed class ObstacleBomberEnemyAiStrategy : IEnemyAiStrategy
{
    public string AiId => "obstacle_bomber";

    public EnemyAiDecision Decide(EnemyAiContext context)
    {
        BoardObject? obstacleTarget = FindNearestDestructibleObstacle(context);
        if (obstacleTarget != null
            && GetManhattanDistance(context.Self.Cell, obstacleTarget.Cell) <= context.SelfState.AttackRange)
        {
            return EnemyAiDecision.Attack(obstacleTarget.ObjectId);
        }

        BoardObject? attackTarget = context.ActionService
            .FindAttackableTargetsInRange(context.Self.ObjectId, context.Self.Cell, context.SelfState.AttackRange)
            .OrderBy(target => target.ObjectType == BoardObjectType.Obstacle ? 0 : 1)
            .ThenBy(target => GetManhattanDistance(context.Self.Cell, target.Cell))
            .FirstOrDefault();
        if (attackTarget != null)
        {
            return EnemyAiDecision.Attack(attackTarget.ObjectId);
        }

        BoardObject? moveTarget = obstacleTarget ?? FindNearestOpponent(context);
        if (moveTarget == null)
        {
            return EnemyAiDecision.Wait();
        }

        Vector2I? nextCell = context.Pathfinder
            .FindReachableCells(context.Self.ObjectId, context.Self.Cell, context.SelfState.MovePointsPerTurn)
            .Where(cell => cell != context.Self.Cell)
            .OrderBy(cell => GetManhattanDistance(cell, moveTarget.Cell))
            .ThenBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .Select(cell => (Vector2I?)cell)
            .FirstOrDefault();

        return nextCell.HasValue ? EnemyAiDecision.Move(nextCell.Value) : EnemyAiDecision.Wait();
    }

    private static BoardObject? FindNearestDestructibleObstacle(EnemyAiContext context)
    {
        return context.Registry.AllObjects
            .Where(boardObject => boardObject.ObjectType == BoardObjectType.Obstacle && boardObject.HasTag("destructible"))
            .OrderBy(boardObject => GetManhattanDistance(context.Self.Cell, boardObject.Cell))
            .ThenBy(boardObject => boardObject.ObjectId, StringComparer.Ordinal)
            .FirstOrDefault();
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
