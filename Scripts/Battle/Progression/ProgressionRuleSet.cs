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
		return new ProgressionRuleSet(
			experienceRequirementsPerLevel: new[]
			{
				90,
				120,
				160,
				210,
				280,
				380,
				520,
				700,
				900,
			},
			softLevelCap: 6,
			masteryPointsPerLevelUpBeforeSoftCap: 1);
	}
}
