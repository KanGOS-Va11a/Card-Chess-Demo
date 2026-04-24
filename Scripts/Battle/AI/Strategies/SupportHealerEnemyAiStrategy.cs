using System;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Board;

namespace CardChessDemo.Battle.AI.Strategies;

public sealed class SupportHealerEnemyAiStrategy : IEnemyAiStrategy
{
    private const string EmergencyRepairSkillId = "emergency_repair";
    private const int EmergencyRepairRange = 2;

    public string AiId => "support_healer";

    public EnemyAiDecision Decide(EnemyAiContext context)
    {
        if (string.Equals(context.SelfState.PendingSpecialSkillId, EmergencyRepairSkillId, StringComparison.Ordinal))
        {
            return EnemyAiDecision.Special(
                EmergencyRepairSkillId,
                context.SelfState.PendingSpecialTargetObjectId,
                context.SelfState.PendingSpecialTargetCell,
                context.SelfState.PendingSpecialCells);
        }

        BoardObject? emergencyRepairTarget = EnemyAiTactics.FindBestSupportTargetInRange(context, EmergencyRepairRange);
        if (emergencyRepairTarget != null && context.SelfState.GetSpecialSkillCooldown(EmergencyRepairSkillId) <= 0)
        {
            return EnemyAiDecision.Telegraph(
                EmergencyRepairSkillId,
                emergencyRepairTarget.ObjectId,
                emergencyRepairTarget.Cell,
                new[] { emergencyRepairTarget.Cell });
        }

        BoardObject? supportTarget = FindBestSupportTarget(context);
        if (supportTarget != null && GetManhattanDistance(context.Self.Cell, supportTarget.Cell) <= 2)
        {
            return EnemyAiDecision.Support(supportTarget.ObjectId, healingAmount: 3, shieldAmount: 2);
        }

        BoardObject? attackTarget = EnemyAiTactics.FindOpponentAttackTargetInRange(context);
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

        BoardObject? nearestOpponent = EnemyAiTactics.FindNearestOpponentUnit(context);
        if (nearestOpponent == null)
        {
            return EnemyAiDecision.Wait();
        }

        return EnemyAiTactics.DecideChasePlayerOrBreakBlockingObstacle(
            context,
            nearestOpponent,
            desiredMaxRange: context.SelfState.AttackRange,
            desiredMinRange: Math.Min(2, context.SelfState.AttackRange),
            preferFlank: true);
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

    private static int GetManhattanDistance(Vector2I a, Vector2I b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }
}
