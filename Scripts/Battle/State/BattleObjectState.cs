using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Board;

namespace CardChessDemo.Battle.State;

public sealed class BattleObjectState
{
    private readonly Dictionary<string, int> _specialSkillCooldowns = new(StringComparer.Ordinal);
    private readonly HashSet<string> _runtimeFlags = new(StringComparer.Ordinal);

    public BattleObjectState(string objectId, string definitionId, string aiId, string displayName, BoardObjectType objectType, BoardObjectFaction faction)
    {
        ObjectId = objectId;
        DefinitionId = definitionId;
        AiId = aiId;
        DisplayName = displayName;
        ObjectType = objectType;
        Faction = faction;
    }

    public string ObjectId { get; }
    public string DefinitionId { get; }
    public string AiId { get; }
    public string DisplayName { get; set; }
    public BoardObjectType ObjectType { get; }
    public BoardObjectFaction Faction { get; }
    public Vector2I Cell { get; set; }
    public int MaxHp { get; set; }
    public int CurrentHp { get; set; }
    public int MaxShield { get; set; }
    public int CurrentShield { get; set; }
    public bool HasDefenseStance { get; set; }
    public int DefenseDamageReductionPercent { get; set; }
    public int MovePointsPerTurn { get; set; }
    public int AttackRange { get; set; }
    public int AttackDamage { get; set; }
    public Vector2 InitialFacing { get; set; } = Vector2.Right;
    public string CurrentAnimation { get; set; } = "idle";
    public bool IsPlayer { get; set; }
    public string PendingSpecialSkillId { get; set; } = string.Empty;
    public string PendingSpecialTargetObjectId { get; set; } = string.Empty;
    public Vector2I PendingSpecialTargetCell { get; set; } = new(int.MinValue, int.MinValue);
    public Vector2I[] PendingSpecialCells { get; set; } = Array.Empty<Vector2I>();
    public bool IsTelegraphing => !string.IsNullOrWhiteSpace(PendingSpecialSkillId) && PendingSpecialCells.Length > 0;

    public int GetSpecialSkillCooldown(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return 0;
        }

        return _specialSkillCooldowns.GetValueOrDefault(skillId, 0);
    }

    public void SetSpecialSkillCooldown(string skillId, int turns)
    {
        if (string.IsNullOrWhiteSpace(skillId))
        {
            return;
        }

        if (turns <= 0)
        {
            _specialSkillCooldowns.Remove(skillId);
            return;
        }

        _specialSkillCooldowns[skillId] = turns;
    }

    public void TickSpecialSkillCooldowns()
    {
        foreach (string skillId in _specialSkillCooldowns.Keys.ToArray())
        {
            int remaining = Math.Max(0, _specialSkillCooldowns[skillId] - 1);
            if (remaining <= 0)
            {
                _specialSkillCooldowns.Remove(skillId);
            }
            else
            {
                _specialSkillCooldowns[skillId] = remaining;
            }
        }
    }

    public bool HasRuntimeFlag(string flagId)
    {
        return !string.IsNullOrWhiteSpace(flagId) && _runtimeFlags.Contains(flagId);
    }

    public void SetRuntimeFlag(string flagId, bool enabled = true)
    {
        if (string.IsNullOrWhiteSpace(flagId))
        {
            return;
        }

        if (enabled)
        {
            _runtimeFlags.Add(flagId);
        }
        else
        {
            _runtimeFlags.Remove(flagId);
        }
    }

    public void ClearPendingSpecial()
    {
        PendingSpecialSkillId = string.Empty;
        PendingSpecialTargetObjectId = string.Empty;
        PendingSpecialTargetCell = new Vector2I(int.MinValue, int.MinValue);
        PendingSpecialCells = Array.Empty<Vector2I>();
    }
}
