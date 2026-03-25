using System;

namespace CardChessDemo.Battle.Encounters;

public sealed class BattleEncounterResolver
{
	private readonly BattleEncounterLibrary _library;

	public BattleEncounterResolver(BattleEncounterLibrary library)
	{
		_library = library;
	}

	public bool TryResolve(string encounterId, out BattleEncounterResolution? resolution, out string failureReason)
	{
		resolution = null;
		failureReason = string.Empty;

		if (string.IsNullOrWhiteSpace(encounterId))
		{
			failureReason = "EncounterId is required.";
			return false;
		}

		BattleEncounterProfile? profile = _library.FindEntry(encounterId);
		if (profile == null)
		{
			failureReason = $"Encounter '{encounterId}' was not found.";
			return false;
		}

		resolution = new BattleEncounterResolution
		{
			EncounterId = profile.EncounterId,
			DisplayName = profile.DisplayName,
			PrimaryEnemyDefinitionId = profile.PrimaryEnemyDefinitionId,
			EnemyTypeIds = profile.EnemyTypeIds ?? Array.Empty<string>(),
			PreferredRoomPoolId = profile.PreferredRoomPoolId,
		};
		return true;
	}
}
