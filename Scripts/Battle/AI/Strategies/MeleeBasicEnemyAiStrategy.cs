using System;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Board;

namespace CardChessDemo.Battle.AI.Strategies;

public sealed class MeleeBasicEnemyAiStrategy : IEnemyAiStrategy
{
    public string AiId => "melee_basic";

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

        return EnemyAiTactics.DecideChasePlayerOrBreakBlockingObstacle(
            context,
            nearestOpponent,
            desiredMaxRange: context.SelfState.AttackRange,
            desiredMinRange: 1,
            preferFlank: true);
    }
}
