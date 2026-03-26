using Godot;

namespace CardChessDemo.Battle.Cards;

[GlobalClass]
public partial class BattleDeckBuildRules : Resource
{
	[Export] public int MinDeckSize { get; set; } = 8;
	[Export] public int MaxDeckSize { get; set; } = 18;
	[Export] public int BasePointBudget { get; set; } = 18;
	[Export] public int BaseMaxCopiesPerCard { get; set; } = 2;
}
