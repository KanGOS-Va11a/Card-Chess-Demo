using System;
using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Enemies;

[GlobalClass]
public partial class BattleEnemyLibrary : Resource
{
	// 当前项目敌人的统一资源入口。
	// 战斗房间生成、敌人状态解析、奖励与学习逻辑都可从这里取定义。
	[Export] public BattleEnemyDefinition[] Entries { get; set; } = Array.Empty<BattleEnemyDefinition>();

	public BattleEnemyDefinition? FindEntry(string definitionId)
	{
		return Entries.FirstOrDefault(entry => string.Equals(entry.DefinitionId, definitionId, StringComparison.Ordinal));
	}
}
