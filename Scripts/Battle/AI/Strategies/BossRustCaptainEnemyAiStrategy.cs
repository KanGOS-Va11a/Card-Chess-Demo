using System;
using System.Linq;
using CardChessDemo.Battle.Board;
using CardChessDemo.Battle.State;

namespace CardChessDemo.Battle.AI.Strategies;

public sealed class BossRustCaptainEnemyAiStrategy : IEnemyAiStrategy
{
    private const string CallCrewUsedFlagId = "boss_call_crew_used";
    private const string MagneticHuntSkillId = "magnetic_hunt";
    private const string CaptainBashSkillId = "captain_bash";
    private const string FlameGridSkillId = "flame_grid";
    private const string CallCrewSkillId = "call_crew";
    private const int MagneticHuntRange = 5;
    private const int CaptainBashRange = 2;
    private const int FlameGridRange = 4;

    public string AiId => "boss_rust_captain";

    public EnemyAiDecision Decide(EnemyAiContext context)
    {
        if (string.Equals(context.SelfState.PendingSpecialSkillId, MagneticHuntSkillId, StringComparison.Ordinal))
        {
            return EnemyAiDecision.Special(
                MagneticHuntSkillId,
                context.SelfState.PendingSpecialTargetObjectId,
                context.SelfState.PendingSpecialTargetCell,
                context.SelfState.PendingSpecialCells);
        }

        if (string.Equals(context.SelfState.PendingSpecialSkillId, CaptainBashSkillId, StringComparison.Ordinal))
        {
            return EnemyAiDecision.Special(
                CaptainBashSkillId,
                context.SelfState.PendingSpecialTargetObjectId,
                context.SelfState.PendingSpecialTargetCell,
                context.SelfState.PendingSpecialCells);
        }

        if (string.Equals(context.SelfState.PendingSpecialSkillId, FlameGridSkillId, StringComparison.Ordinal))
        {
            return EnemyAiDecision.Special(
                FlameGridSkillId,
                context.SelfState.PendingSpecialTargetObjectId,
                context.SelfState.PendingSpecialTargetCell,
                context.SelfState.PendingSpecialCells);
        }

        if (string.Equals(context.SelfState.PendingSpecialSkillId, CallCrewSkillId, StringComparison.Ordinal))
        {
            return EnemyAiDecision.Special(
                CallCrewSkillId,
                context.SelfState.PendingSpecialTargetObjectId,
                context.SelfState.PendingSpecialTargetCell,
                context.SelfState.PendingSpecialCells);
        }

        if (context.SelfState.GetSpecialSkillCooldown(MagneticHuntSkillId) <= 0
            && TryFindMagneticHuntTarget(context, out BoardObject? lineTarget, out Godot.Vector2I[] lineCells)
            && lineTarget != null)
        {
            return EnemyAiDecision.Telegraph(
                MagneticHuntSkillId,
                lineTarget.ObjectId,
                lineTarget.Cell,
                lineCells);
        }

        BoardObject? nearestOpponent = EnemyAiTactics.FindNearestOpponentUnit(context);
        if (nearestOpponent == null)
        {
            return EnemyAiDecision.Wait();
        }

        if (context.SelfState.GetSpecialSkillCooldown(CaptainBashSkillId) <= 0
            && EnemyAiTactics.GetManhattanDistance(context.Self.Cell, nearestOpponent.Cell) <= CaptainBashRange)
        {
            return EnemyAiDecision.Telegraph(
                CaptainBashSkillId,
                nearestOpponent.ObjectId,
                nearestOpponent.Cell,
                BuildCaptainBashCells(nearestOpponent.Cell));
        }

        BoardObject? attackTarget = EnemyAiTactics.FindPreferredAttackTarget(context);
        if (attackTarget != null)
        {
            return EnemyAiDecision.Attack(attackTarget.ObjectId);
        }

        if (context.SelfState.GetSpecialSkillCooldown(FlameGridSkillId) <= 0
            && EnemyAiTactics.GetManhattanDistance(context.Self.Cell, nearestOpponent.Cell) <= FlameGridRange)
        {
            Godot.Vector2I[] terrainCells = BuildFlameGridCells(nearestOpponent.Cell);
            if (terrainCells.Length > 0)
            {
                return EnemyAiDecision.Telegraph(
                    FlameGridSkillId,
                    nearestOpponent.ObjectId,
                    nearestOpponent.Cell,
                    terrainCells);
            }
        }

        bool isBelowHalfHp = context.SelfState.MaxHp > 0
            && context.SelfState.CurrentHp * 2 <= context.SelfState.MaxHp;
        if (isBelowHalfHp
            && !context.SelfState.HasRuntimeFlag(CallCrewUsedFlagId)
            && context.SelfState.GetSpecialSkillCooldown(CallCrewSkillId) <= 0)
        {
            Godot.Vector2I[] summonCells = FindSummonCells(context);
            if (summonCells.Length > 0 && CountEnemyUnits(context) < 4)
            {
                return EnemyAiDecision.Telegraph(
                    CallCrewSkillId,
                    string.Empty,
                    context.Self.Cell,
                    summonCells);
            }
        }

        Godot.Vector2I? nextCell = EnemyAiTactics.FindBestApproachCell(
            context,
            nearestOpponent,
            desiredMaxRange: 1,
            desiredMinRange: 1,
            preferFlank: false);

        return nextCell.HasValue
            ? EnemyAiDecision.Move(nextCell.Value)
            : EnemyAiDecision.Wait();
    }

    private static Godot.Vector2I[] BuildCaptainBashCells(Godot.Vector2I centerCell)
    {
        return new[]
        {
            centerCell,
            centerCell + Godot.Vector2I.Left,
            centerCell + Godot.Vector2I.Right,
            centerCell + Godot.Vector2I.Up,
            centerCell + Godot.Vector2I.Down,
        };
    }

    private static Godot.Vector2I[] BuildFlameGridCells(Godot.Vector2I centerCell)
    {
        return new[]
        {
            centerCell,
            centerCell + Godot.Vector2I.Left,
            centerCell + Godot.Vector2I.Right,
            centerCell + Godot.Vector2I.Up,
            centerCell + Godot.Vector2I.Down,
        };
    }

    private static Godot.Vector2I[] FindSummonCells(EnemyAiContext context)
    {
        return new[]
            {
                context.Self.Cell + Godot.Vector2I.Left,
                context.Self.Cell + Godot.Vector2I.Right,
                context.Self.Cell + Godot.Vector2I.Up,
                context.Self.Cell + Godot.Vector2I.Down,
                context.Self.Cell + new Godot.Vector2I(-2, 0),
                context.Self.Cell + new Godot.Vector2I(2, 0),
            }
            .Where(cell => context.Room.Topology.IsInsideBoard(cell))
            .Where(cell => context.QueryService.CanOccupyCell(context.Self.ObjectId, cell, out _))
            .Take(2)
            .ToArray();
    }

    private static int CountEnemyUnits(EnemyAiContext context)
    {
        return context.Registry.AllObjects.Count(boardObject =>
            boardObject.ObjectType == BoardObjectType.Unit
            && boardObject.Faction == BoardObjectFaction.Enemy);
    }

    private static bool TryFindMagneticHuntTarget(
        EnemyAiContext context,
        out BoardObject? target,
        out Godot.Vector2I[] lineCells)
    {
        target = null;
        lineCells = Array.Empty<Godot.Vector2I>();

        foreach (Godot.Vector2I direction in BoardTopology.CardinalDirections)
        {
            BoardObject? playerTarget = null;
            bool blocked = false;

            for (int step = 1; step <= MagneticHuntRange; step++)
            {
                Godot.Vector2I cell = context.Self.Cell + direction * step;
                if (!context.Room.Topology.IsInsideBoard(cell))
                {
                    break;
                }

                foreach (BoardObject boardObject in context.QueryService.GetObjectsAtCell(cell))
                {
                    if (boardObject.ObjectId == context.Self.ObjectId || boardObject.Faction == BoardObjectFaction.Enemy)
                    {
                        continue;
                    }

                    if (boardObject.ObjectType == BoardObjectType.Obstacle || boardObject.BlocksLineOfSight)
                    {
                        blocked = true;
                        break;
                    }

                    if (boardObject.ObjectType == BoardObjectType.Unit)
                    {
                        BattleObjectState? boardObjectState = context.StateManager.Get(boardObject.ObjectId);
                        if (boardObjectState?.IsPlayer == true)
                        {
                            playerTarget = boardObject;
                            break;
                        }
                    }
                }

                if (blocked || playerTarget != null)
                {
                    break;
                }
            }

            if (blocked || playerTarget == null)
            {
                continue;
            }

            target = playerTarget;
            lineCells = Enumerable.Range(1, MagneticHuntRange)
                .Select(step => context.Self.Cell + direction * step)
                .Where(cell => context.Room.Topology.IsInsideBoard(cell))
                .ToArray();
            return true;
        }

        return false;
    }
}
