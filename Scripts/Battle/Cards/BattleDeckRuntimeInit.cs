using System;

namespace CardChessDemo.Battle.Cards;

public sealed class BattleDeckRuntimeInit
{
	public BattleCardDefinition[] BuildCards { get; init; } = Array.Empty<BattleCardDefinition>();

	public BattleCardDefinition[] StartingHandCards { get; init; } = Array.Empty<BattleCardDefinition>();

	public BattleCardDefinition[] StartingDrawPileCards { get; init; } = Array.Empty<BattleCardDefinition>();

	public BattleCardDefinition[] StartingDiscardPileCards { get; init; } = Array.Empty<BattleCardDefinition>();

	public BattleCardDefinition[] StartingExhaustPileCards { get; init; } = Array.Empty<BattleCardDefinition>();

	public int HandSizeOverride { get; init; } = -1;

	public int MaxEnergyOverride { get; init; } = -1;

	public int InitialEnergy { get; init; } = -1;

	public int OpeningDrawCount { get; init; } = -1;

	public bool HasExplicitPiles =>
		StartingHandCards.Length > 0
		|| StartingDrawPileCards.Length > 0
		|| StartingDiscardPileCards.Length > 0
		|| StartingExhaustPileCards.Length > 0;

	public bool HasExplicitStartingHand => StartingHandCards.Length > 0;
}
