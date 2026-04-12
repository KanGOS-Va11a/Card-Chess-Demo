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
        BoardObject? nearestOpponent = EnemyAiTactics.FindNearestOpponentUnit(context);
        if (nearestOpponent != null)
        {
            BoardObject? playerAttackTarget = EnemyAiTactics.FindOpponentAttackTargetInRange(context);
            if (playerAttackTarget != null)
            {
                return EnemyAiDecision.Attack(playerAttackTarget.ObjectId);
            }

            Vector2I? playerApproachCell = EnemyAiTactics.FindBestApproachCell(
                context,
                nearestOpponent,
                desiredMaxRange: context.SelfState.AttackRange,
                desiredMinRange: Math.Min(2, context.SelfState.AttackRange),
                preferFlank: true);
            if (playerApproachCell.HasValue)
            {
                return EnemyAiDecision.Move(playerApproachCell.Value);
            }
        }

        BoardObject? obstacleTarget = FindNearestDestructibleObstacle(context);
        if (obstacleTarget != null
            && GetManhattanDistance(context.Self.Cell, obstacleTarget.Cell) <= context.SelfState.AttackRange)
        {
            return EnemyAiDecision.Attack(obstacleTarget.ObjectId);
        }

        BoardObject? attackTarget = EnemyAiTactics.FindPreferredAttackTarget(context);
        if (attackTarget != null)
        {
            return EnemyAiDecision.Attack(attackTarget.ObjectId);
        }

        BoardObject? moveTarget = nearestOpponent ?? obstacleTarget;
        if (moveTarget == null)
        {
            return EnemyAiDecision.Wait();
        }

        Vector2I? nextCell = moveTarget.ObjectType == BoardObjectType.Unit
            ? EnemyAiTactics.FindBestApproachCell(
                context,
                moveTarget,
                desiredMaxRange: context.SelfState.AttackRange,
                desiredMinRange: Math.Min(2, context.SelfState.AttackRange),
                preferFlank: true)
            : context.Pathfinder
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

    private static int GetManhattanDistance(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }
}
