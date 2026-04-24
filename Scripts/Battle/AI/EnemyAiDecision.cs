using CardChessDemo.Battle.Data;
using Godot;

namespace CardChessDemo.Battle.AI;

public enum EnemyAiDecisionType
{
    Wait = 0,
    Move = 1,
    Attack = 2,
    Spawn = 3,
    Support = 4,
    Telegraph = 5,
    Special = 6,
}

public sealed class EnemyAiDecision
{
    private EnemyAiDecision(
        EnemyAiDecisionType decisionType,
        Vector2I moveCell,
        string targetObjectId,
        BoardObjectSpawnDefinition? spawnDefinition,
        int healingAmount,
        int shieldAmount,
        string specialSkillId,
        Vector2I specialTargetCell,
        Vector2I[] specialCells)
    {
        DecisionType = decisionType;
        MoveCell = moveCell;
        TargetObjectId = targetObjectId;
        SpawnDefinition = spawnDefinition;
        HealingAmount = healingAmount;
        ShieldAmount = shieldAmount;
        SpecialSkillId = specialSkillId;
        SpecialTargetCell = specialTargetCell;
        SpecialCells = specialCells ?? System.Array.Empty<Vector2I>();
    }

    public EnemyAiDecisionType DecisionType { get; }
    public Vector2I MoveCell { get; }
    public string TargetObjectId { get; }
    public BoardObjectSpawnDefinition? SpawnDefinition { get; }
    public int HealingAmount { get; }
    public int ShieldAmount { get; }
    public string SpecialSkillId { get; }
    public Vector2I SpecialTargetCell { get; }
    public Vector2I[] SpecialCells { get; }

    public static EnemyAiDecision Wait()
    {
        return new EnemyAiDecision(EnemyAiDecisionType.Wait, Vector2I.Zero, string.Empty, null, 0, 0, string.Empty, Vector2I.Zero, System.Array.Empty<Vector2I>());
    }

    public static EnemyAiDecision Move(Vector2I targetCell, string targetObjectId = "")
    {
        return new EnemyAiDecision(EnemyAiDecisionType.Move, targetCell, targetObjectId, null, 0, 0, string.Empty, Vector2I.Zero, System.Array.Empty<Vector2I>());
    }

    public static EnemyAiDecision Attack(string targetObjectId)
    {
        return new EnemyAiDecision(EnemyAiDecisionType.Attack, Vector2I.Zero, targetObjectId, null, 0, 0, string.Empty, Vector2I.Zero, System.Array.Empty<Vector2I>());
    }

    public static EnemyAiDecision Spawn(BoardObjectSpawnDefinition spawnDefinition)
    {
        return new EnemyAiDecision(EnemyAiDecisionType.Spawn, Vector2I.Zero, string.Empty, spawnDefinition, 0, 0, string.Empty, Vector2I.Zero, System.Array.Empty<Vector2I>());
    }

    public static EnemyAiDecision Support(string targetObjectId, int healingAmount = 0, int shieldAmount = 0)
    {
        return new EnemyAiDecision(EnemyAiDecisionType.Support, Vector2I.Zero, targetObjectId, null, healingAmount, shieldAmount, string.Empty, Vector2I.Zero, System.Array.Empty<Vector2I>());
    }

    public static EnemyAiDecision Telegraph(string skillId, string targetObjectId, Vector2I targetCell, Vector2I[] cells)
    {
        return new EnemyAiDecision(EnemyAiDecisionType.Telegraph, Vector2I.Zero, targetObjectId, null, 0, 0, skillId, targetCell, cells);
    }

    public static EnemyAiDecision Special(string skillId, string targetObjectId, Vector2I targetCell, Vector2I[] cells)
    {
        return new EnemyAiDecision(EnemyAiDecisionType.Special, Vector2I.Zero, targetObjectId, null, 0, 0, skillId, targetCell, cells);
    }
}
