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

            EnemyAiTactics.PathBlockAnalysis playerRouteAnalysis = EnemyAiTactics.AnalyzePathToTarget(
                context,
                nearestOpponent,
                desiredMaxRange: context.SelfState.AttackRange,
                desiredMinRange: Math.Min(2, context.SelfState.AttackRange),
                preferFlank: true);
            if (playerRouteAnalysis.HasOpenRoute)
            {
                return playerRouteAnalysis.MoveCell.HasValue
                    ? EnemyAiDecision.Move(playerRouteAnalysis.MoveCell.Value, nearestOpponent.ObjectId)
                    : EnemyAiDecision.Wait();
            }

            if (playerRouteAnalysis.BlockingObstacle != null
                && GetManhattanDistance(context.Self.Cell, playerRouteAnalysis.BlockingObstacle.Cell) <= context.SelfState.AttackRange)
            {
                return EnemyAiDecision.Attack(playerRouteAnalysis.BlockingObstacle.ObjectId);
            }

            if (playerRouteAnalysis.BlockingObstacle != null)
            {
                Vector2I? obstacleApproachCell = EnemyAiTactics.FindBestApproachCell(
                    context,
                    playerRouteAnalysis.BlockingObstacle,
                    desiredMaxRange: context.SelfState.AttackRange,
                    desiredMinRange: 1,
                    preferFlank: false);
                if (obstacleApproachCell.HasValue)
                {
                    return EnemyAiDecision.Move(obstacleApproachCell.Value, playerRouteAnalysis.BlockingObstacle.ObjectId);
                }
            }
        }

        BoardObject? obstacleTarget = FindNearestDestructibleObstacle(context);
        if (obstacleTarget == null)
        {
            return EnemyAiDecision.Wait();
        }

        if (GetManhattanDistance(context.Self.Cell, obstacleTarget.Cell) <= context.SelfState.AttackRange)
        {
            return EnemyAiDecision.Attack(obstacleTarget.ObjectId);
        }

        Vector2I? nextCell = EnemyAiTactics.FindBestApproachCell(
            context,
            obstacleTarget,
            desiredMaxRange: context.SelfState.AttackRange,
            desiredMinRange: 1,
            preferFlank: false);

        return nextCell.HasValue ? EnemyAiDecision.Move(nextCell.Value, obstacleTarget.ObjectId) : EnemyAiDecision.Wait();
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
