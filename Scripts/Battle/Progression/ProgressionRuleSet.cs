using System;

namespace CardChessDemo.Battle.Progression;

public sealed class ProgressionRuleSet
{
	public ProgressionRuleSet(int baseExperienceForLevel, int linearGrowthPerLevel)
	{
		BaseExperienceForLevel = Math.Max(1, baseExperienceForLevel);
		LinearGrowthPerLevel = Math.Max(0, linearGrowthPerLevel);
	}

	public int BaseExperienceForLevel { get; }

	public int LinearGrowthPerLevel { get; }

	public int GetExperienceRequirementForLevel(int level)
	{
		return Math.Max(BaseExperienceForLevel, BaseExperienceForLevel + (Math.Max(1, level) - 1) * LinearGrowthPerLevel);
	}

	public int GetAccumulatedExperienceForLevel(int level)
	{
		int total = 0;
		for (int current = 1; current < Math.Max(1, level); current++)
		{
			total += GetExperienceRequirementForLevel(current);
		}

		return total;
	}

	public static ProgressionRuleSet CreateFromConfiguredRules()
	{
		// TODO: 后续正式版应改为从资源或数值表读取成长曲线。
		// 当前竞赛版先固化一条最小线性曲线，保证状态接口与局外 UI 先稳定。
		return CreateDefaultDemoRuleSet();
	}

	public static ProgressionRuleSet CreateDefaultDemoRuleSet()
	{
		return new ProgressionRuleSet(baseExperienceForLevel: 10, linearGrowthPerLevel: 5);
	}
}
