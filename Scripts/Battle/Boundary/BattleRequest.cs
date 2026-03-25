using System;
using Godot;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.Boundary;

public sealed class BattleRequest
{
	public BattleRequest(
		Godot.Collections.Dictionary? playerSnapshot = null,
		bool _ = false)
		: this(
			requestId: string.Empty,
			encounterId: string.Empty,
			randomSeed: 0,
			playerSnapshot: playerSnapshot)
	{
	}

	public BattleRequest(
		string requestId = "",
		string encounterId = "",
		int randomSeed = 0,
		Godot.Collections.Dictionary? playerSnapshot = null,
		Godot.Collections.Dictionary? companionSnapshot = null,
		Godot.Collections.Dictionary? progressionSnapshot = null,
		Godot.Collections.Dictionary? deckBuildSnapshot = null,
		Godot.Collections.Dictionary? deckRuntimeInitOverrides = null,
		Godot.Collections.Dictionary? runtimeModifiers = null)
	{
		RequestId = string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString("N") : requestId.Trim();
		EncounterId = encounterId?.Trim() ?? string.Empty;
		RandomSeed = randomSeed;
		PlayerSnapshot = CloneDictionary(playerSnapshot);
		CompanionSnapshot = CloneDictionary(companionSnapshot);
		ProgressionSnapshot = CloneDictionary(progressionSnapshot);
		DeckBuildSnapshot = CloneDictionary(deckBuildSnapshot);
		DeckRuntimeInitOverrides = CloneDictionary(deckRuntimeInitOverrides);
		RuntimeModifiers = CloneDictionary(runtimeModifiers);
	}

	public string RequestId { get; }

	public string EncounterId { get; }

	public int RandomSeed { get; }

	public Godot.Collections.Dictionary PlayerSnapshot { get; }

	public Godot.Collections.Dictionary CompanionSnapshot { get; }

	public Godot.Collections.Dictionary ProgressionSnapshot { get; }

	public Godot.Collections.Dictionary DeckBuildSnapshot { get; }

	public Godot.Collections.Dictionary DeckRuntimeInitOverrides { get; }

	public Godot.Collections.Dictionary RuntimeModifiers { get; }

	public bool TryValidate(out string failureReason)
	{
		if (string.IsNullOrWhiteSpace(RequestId))
		{
			failureReason = "BattleRequest.RequestId is required.";
			return false;
		}

		if (!PlayerSnapshot.TryGetValue("current_hp", out _))
		{
			failureReason = "BattleRequest.PlayerSnapshot.current_hp is required.";
			return false;
		}

		if (!PlayerSnapshot.TryGetValue("max_hp", out _))
		{
			failureReason = "BattleRequest.PlayerSnapshot.max_hp is required.";
			return false;
		}

		failureReason = string.Empty;
		return true;
	}

	public static BattleRequest FromSession(GlobalGameSession session, string encounterId = "", int randomSeed = 0)
	{
		return new BattleRequest(
			requestId: Guid.NewGuid().ToString("N"),
			encounterId: encounterId,
			randomSeed: randomSeed,
			playerSnapshot: session.BuildPlayerSnapshot(),
			companionSnapshot: session.BuildCompanionSnapshot(),
			progressionSnapshot: session.BuildProgressionSnapshot(),
			deckBuildSnapshot: session.BuildDeckBuildSnapshot());
	}

	public void ApplyToSession(GlobalGameSession session)
	{
		session.ApplyPlayerSnapshot(PlayerSnapshot);
		session.ApplyCompanionSnapshot(CompanionSnapshot);
		session.ApplyProgressionSnapshot(ProgressionSnapshot);
		session.ApplyDeckBuildSnapshot(DeckBuildSnapshot);
	}

	private static Godot.Collections.Dictionary CloneDictionary(Godot.Collections.Dictionary? source)
	{
		Godot.Collections.Dictionary clone = new();
		if (source == null)
		{
			return clone;
		}

		foreach (Variant key in source.Keys)
		{
			clone[key] = source[key];
		}

		return clone;
	}
}
