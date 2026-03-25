using System;
using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Encounters;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.Services;

public sealed class BattleRequestBuilder
{
	private readonly GlobalGameSession _session;
	private readonly BattleEncounterResolver? _encounterResolver;

	public BattleRequestBuilder(GlobalGameSession session, BattleEncounterResolver? encounterResolver = null)
	{
		_session = session;
		_encounterResolver = encounterResolver;
	}

	public BattleRequest Build(
		string encounterId,
		int randomSeed = 0,
		Godot.Collections.Dictionary? deckRuntimeInitOverrides = null,
		Godot.Collections.Dictionary? runtimeModifiers = null)
	{
		string resolvedEncounterId = encounterId?.Trim() ?? string.Empty;
		if (_encounterResolver != null
			&& _encounterResolver.TryResolve(resolvedEncounterId, out BattleEncounterResolution? resolution, out _)
			&& resolution != null)
		{
			resolvedEncounterId = resolution.EncounterId;
		}

		return new BattleRequest(
			requestId: Guid.NewGuid().ToString("N"),
			encounterId: resolvedEncounterId,
			randomSeed: randomSeed,
			playerSnapshot: _session.BuildPlayerSnapshot(),
			companionSnapshot: _session.BuildCompanionSnapshot(),
			progressionSnapshot: _session.BuildProgressionSnapshotModel().ToDictionary(),
			deckBuildSnapshot: _session.BuildDeckBuildSnapshotModel().ToDictionary(),
			deckRuntimeInitOverrides: deckRuntimeInitOverrides,
			runtimeModifiers: runtimeModifiers);
	}
}
