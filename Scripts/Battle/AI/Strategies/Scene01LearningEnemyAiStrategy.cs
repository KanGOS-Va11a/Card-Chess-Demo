using System;
using CardChessDemo.Battle.Board;
using Godot;

namespace CardChessDemo.Battle.AI.Strategies;

public sealed class Scene01LearningEnemyAiStrategy : IEnemyAiStrategy
{
    public string AiId => "scene01_learning";

    public EnemyAiDecision Decide(EnemyAiContext context)
    {
        BoardObject? nearestOpponent = EnemyAiTactics.FindNearestOpponentUnit(context);
        if (nearestOpponent == null)
        {
            return EnemyAiDecision.Wait();
        }

        BoardObject? attackTarget = EnemyAiTactics.FindOpponentAttackTargetInRange(context);
        if (attackTarget != null)
        {
            return EnemyAiDecision.Attack(attackTarget.ObjectId);
        }

        bool isHalfHpPhase = context.SelfState.MaxHp > 0 && context.SelfState.CurrentHp * 2 <= context.SelfState.MaxHp;
        int effectiveMovePoints = context.SelfState.MovePointsPerTurn + (isHalfHpPhase ? 1 : 0);

        return EnemyAiTactics.DecideChasePlayerOrBreakBlockingObstacle(
            context,
            nearestOpponent,
            desiredMaxRange: context.SelfState.AttackRange,
            desiredMinRange: 1,
            preferFlank: true,
            moveBudgetOverride: effectiveMovePoints);
    }
}
