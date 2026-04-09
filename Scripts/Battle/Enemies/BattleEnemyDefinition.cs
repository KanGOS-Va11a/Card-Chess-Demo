using Godot;

namespace CardChessDemo.Battle.Enemies;

/// <summary>
/// 单个敌人的统一定义资源。
/// 后续调整敌人的表现、基础数值、AI、经验与学习奖励时，优先修改这里。
/// </summary>
[GlobalClass]
public partial class BattleEnemyDefinition : Resource
{
	// 唯一主键。战斗房间、奖励结算、学习逻辑都通过它索引敌人定义。
	[Export] public string DefinitionId { get; set; } = string.Empty;

	// UI、日志、悬浮信息里显示的敌人名称。
	[Export] public string DisplayName { get; set; } = "Enemy";

	// 该敌人在战斗中的表现预制体；为空时回退到旧的 BattlePrefabLibrary。
	[Export] public PackedScene? PrefabScene { get; set; }

	// 当前敌人绑定的 AI 模板 Id，例如 melee_basic / ranged_line / scene01_learning。
	[Export] public string AiId { get; set; } = "melee_basic";

	// 基础生命与初始护盾。房间生成敌人时会以这里为默认值。
	[Export] public int MaxHp { get; set; } = 1;
	[Export] public int StartingShield { get; set; } = 0;

	// 基础战斗能力。BattleObjectStateManager 会优先从这里读取敌人的移动、射程和攻击。
	[Export] public int MovePointsPerTurn { get; set; } = 3;
	[Export] public int AttackRange { get; set; } = 1;
	[Export] public int AttackDamage { get; set; } = 1;

	// 战后逐个敌人结算时使用的经验值。
	[Export] public int DefeatExperience { get; set; } = 0;

	// 学习奖励。普通奖励与特色奖励都放在这里，避免分散硬编码在 BattleSceneController。
	[Export] public string NormalLearnCardId { get; set; } = string.Empty;
	[Export] public string SignatureLearnCardId { get; set; } = string.Empty;

	// 特色学习条件：
	// 1. AtHalfHpOrBelow: 生命降到一半或以下时可学习特色牌。
	// 2. RequiresRuntimeFlag: 依赖运行时状态标记，适合精英/Boss 特殊动作窗口。
	[Export] public bool SignatureLearnAvailableAtHalfHpOrBelow { get; set; } = false;
	[Export] public bool SignatureLearnRequiresRuntimeFlag { get; set; } = false;
}
