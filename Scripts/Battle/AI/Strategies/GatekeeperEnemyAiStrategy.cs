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
        BoardObject? attackTarget = EnemyAiTactics.FindOpponentAttackTargetInRange(context);
        if (attackTarget != null)
        {
            return EnemyAiDecision.Attack(attackTarget.ObjectId);
        }

        BoardObject? nearestOpponent = EnemyAiTactics.FindNearestOpponentUnit(context);
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
                Distance = EnemyAiTactics.GetManhattanDistance(cell, nearestOpponent.Cell),
                InDesiredRange = EnemyAiTactics.GetManhattanDistance(cell, nearestOpponent.Cell) <= context.SelfState.AttackRange,
                FlankScore = cell.X != nearestOpponent.Cell.X && cell.Y != nearestOpponent.Cell.Y ? 1 : 0,
                AdjacentObstacle = HasAdjacentObstacle(context, cell) ? 1 : 0,
            })
            .OrderByDescending(candidate => candidate.InDesiredRange)
            .ThenByDescending(candidate => candidate.FlankScore)
            .ThenByDescending(candidate => candidate.AdjacentObstacle)
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

}
