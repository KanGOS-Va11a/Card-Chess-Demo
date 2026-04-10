using System;

namespace CardChessDemo.Battle.Progression;

public sealed class ProgressionRuleSet
{
	private readonly int[] _experienceRequirementsPerLevel;

	public ProgressionRuleSet(int[] experienceRequirementsPerLevel, int softLevelCap, int masteryPointsPerLevelUpBeforeSoftCap)
	{
		_experienceRequirementsPerLevel = experienceRequirementsPerLevel ?? Array.Empty<int>();
		if (_experienceRequirementsPerLevel.Length == 0)
		{
			throw new ArgumentException("ProgressionRuleSet requires at least one level requirement.", nameof(experienceRequirementsPerLevel));
		}

		for (int index = 0; index < _experienceRequirementsPerLevel.Length; index++)
		{
			_experienceRequirementsPerLevel[index] = Math.Max(1, _experienceRequirementsPerLevel[index]);
		}

		SoftLevelCap = Math.Max(2, softLevelCap);
		MasteryPointsPerLevelUpBeforeSoftCap = Math.Max(0, masteryPointsPerLevelUpBeforeSoftCap);
	}

	// level N 的 requirement 表示从 Lv.N 升到 Lv.N+1 所需经验。
	// Demo 现阶段采用手工分段曲线，方便直接对齐当前敌人掉落口径。
	public int GetExperienceRequirementForLevel(int level)
	{
		int normalizedLevel = Math.Max(1, level);
		int index = Math.Min(normalizedLevel - 1, _experienceRequirementsPerLevel.Length - 1);
		return _experienceRequirementsPerLevel[index];
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

	public int ResolveLevelFromExperience(int experience)
	{
		int normalizedExperience = Math.Max(0, experience);
		int resolvedLevel = 1;
		while (normalizedExperience >= GetAccumulatedExperienceForLevel(resolvedLevel + 1))
		{
			resolvedLevel++;
		}

		return resolvedLevel;
	}

	public int GetMasteryPointsAwardForLevel(int levelReached)
	{
		return levelReached <= SoftLevelCap ? MasteryPointsPerLevelUpBeforeSoftCap : 0;
	}

	public int GetMasteryPointsAwardBetweenLevels(int fromLevelExclusive, int toLevelInclusive)
	{
		int total = 0;
		for (int level = Math.Max(2, fromLevelExclusive + 1); level <= Math.Max(fromLevelExclusive, toLevelInclusive); level++)
		{
			total += GetMasteryPointsAwardForLevel(level);
		}

		return total;
	}

	public int SoftLevelCap { get; }

	public int MasteryPointsPerLevelUpBeforeSoftCap { get; }

	public static ProgressionRuleSet CreateFromConfiguredRules()
	{
		return CreateDefaultDemoRuleSet();
	}

	public static ProgressionRuleSet CreateDefaultDemoRuleSet()
	{
		// 当前敌人经验大致为：
		// 普通敌人 20~26、教学敌人 40、精英 48~50、Boss 120。
		// 这条曲线保证：
		// 1. 教学战后就能看到第一次升级；
		// 2. 中段 2~3 场普通战 / 1 场精英战有稳定成长反馈；
		// 3. Demo 全流程大致落在 Lv.5~Lv.6，Lv.6 之后进入软上限区。
		return new ProgressionRuleSet(
			experienceRequirementsPerLevel: new[]
			{
				40,  // Lv.1 -> Lv.2
				55,  // Lv.2 -> Lv.3
				75,  // Lv.3 -> Lv.4
				100, // Lv.4 -> Lv.5
				130, // Lv.5 -> Lv.6
				220, // Lv.6 -> Lv.7 软上限后明显变慢
				300, // Lv.7 -> Lv.8
				420, // Lv.8 -> Lv.9
				560, // Lv.9 -> Lv.10
			},
			softLevelCap: 6,
			masteryPointsPerLevelUpBeforeSoftCap: 1);
	}
}
