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

        return EnemyAiTactics.DecideChasePlayerOrBreakBlockingObstacle(
            context,
            nearestOpponent,
            desiredMaxRange: context.SelfState.AttackRange,
            desiredMinRange: 1,
            preferFlank: false);
    }
}
