namespace CardChessDemo.Battle.Stats;

public sealed class ResolvedPlayerStats
{
	public int MaxHp { get; set; }

	public int CurrentHp { get; set; }

	public int MovePointsPerTurn { get; set; }

	public int AttackRange { get; set; }

	public int AttackDamage { get; set; }

	public int DefenseDamageReductionPercent { get; set; }

	public int DefenseShieldGain { get; set; }
}
