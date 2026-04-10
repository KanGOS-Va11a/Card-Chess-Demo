using System;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Board;

namespace CardChessDemo.Battle.AI.Strategies;

public sealed class SupportHealerEnemyAiStrategy : IEnemyAiStrategy
{
    public string AiId => "support_healer";

    public EnemyAiDecision Decide(EnemyAiContext context)
    {
        BoardObject? supportTarget = FindBestSupportTarget(context);
        if (supportTarget != null && GetManhattanDistance(context.Self.Cell, supportTarget.Cell) <= 2)
        {
            return EnemyAiDecision.Support(supportTarget.ObjectId, healingAmount: 3, shieldAmount: 2);
        }

        BoardObject? attackTarget = context.ActionService
            .FindAttackableTargetsInRange(context.Self.ObjectId, context.Self.Cell, context.SelfState.AttackRange)
            .OrderBy(target => GetManhattanDistance(context.Self.Cell, target.Cell))
            .FirstOrDefault();
        if (attackTarget != null)
        {
            return EnemyAiDecision.Attack(attackTarget.ObjectId);
        }

        if (supportTarget != null)
        {
            Vector2I? supportMove = context.Pathfinder
                .FindReachableCells(context.Self.ObjectId, context.Self.Cell, context.SelfState.MovePointsPerTurn)
                .Where(cell => cell != context.Self.Cell)
                .OrderBy(cell => GetManhattanDistance(cell, supportTarget.Cell))
                .ThenBy(cell => cell.Y)
                .ThenBy(cell => cell.X)
                .Select(cell => (Vector2I?)cell)
                .FirstOrDefault();
            if (supportMove.HasValue)
            {
                return EnemyAiDecision.Move(supportMove.Value);
            }
        }

        BoardObject? nearestOpponent = FindNearestOpponent(context);
        if (nearestOpponent == null)
        {
            return EnemyAiDecision.Wait();
        }

        Vector2I? nextCell = context.Pathfinder
            .FindReachableCells(context.Self.ObjectId, context.Self.Cell, context.SelfState.MovePointsPerTurn)
            .Where(cell => cell != context.Self.Cell)
            .OrderBy(cell => Math.Abs(GetManhattanDistance(cell, nearestOpponent.Cell) - Math.Max(2, context.SelfState.AttackRange)))
            .ThenBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .Select(cell => (Vector2I?)cell)
            .FirstOrDefault();

        return nextCell.HasValue ? EnemyAiDecision.Move(nextCell.Value) : EnemyAiDecision.Wait();
    }

    private static BoardObject? FindBestSupportTarget(EnemyAiContext context)
    {
        return context.Registry.AllObjects
            .Where(boardObject => boardObject.ObjectId != context.Self.ObjectId)
            .Where(boardObject => boardObject.Faction == context.Self.Faction)
            .Where(boardObject => boardObject.ObjectType == BoardObjectType.Unit || boardObject.ObjectType == BoardObjectType.Obstacle)
            .Select(boardObject => new
            {
                Object = boardObject,
                State = context.StateManager.Get(boardObject.ObjectId),
            })
            .Where(candidate => candidate.State != null)
            .Where(candidate => candidate.State!.CurrentHp < candidate.State.MaxHp || candidate.State.CurrentShield < candidate.State.MaxShield)
            .OrderBy(candidate => GetManhattanDistance(context.Self.Cell, candidate.Object.Cell))
            .ThenBy(candidate => candidate.State!.CurrentHp)
            .ThenBy(candidate => candidate.State!.CurrentShield)
            .Select(candidate => candidate.Object)
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
