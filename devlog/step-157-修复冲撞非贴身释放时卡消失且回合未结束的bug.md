# step-157 修复冲撞非贴身释放时卡消失且回合未结束的 bug

时间：2026-04-06

问题现象：
- 当玩家不是贴着敌人释放 `冲撞` 时：
  - 卡牌会直接消失
  - 但回合没有正确结束
  - 甚至还能继续打出其他卡牌

根因：
- `冲撞` 在特殊效果内部调用的是 `BattleSceneController.TryMoveObject(...)`
- 这个入口会再次检查“玩家本回合是否还能正常移动”
- 出牌阶段的冲撞补位移动本质上是技能位移，不应该再受普通移动权限约束
- 结果就是：
  - 卡已经在前面 `CommitPlayedCard(...)` 被提交移出手牌
  - 但补位移动失败
  - `TryResolveSpecialCardEffect(...)` 返回失败
  - `TurnState.MarkActed(...)` 没有执行
  - 于是出现“卡没了，但行动还没锁”的脏状态

本次修复：

1. 冲撞补位移动改成强制位移
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - `冲撞` 的补位移动不再走 `BattleSceneController.TryMoveObject(...)`
  - 改为直接走 `_actionService.TryMoveObject(...)`
  - 这样不会再被普通移动权限拦截

2. 增加出牌失败回滚
- 文件：
  - `Scripts/Battle/Cards/BattleDeckState.cs`
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - `BattleDeckState` 新增 `RollbackPlayedCard(...)`
  - 若特殊牌效果在提交后失败，会把卡退回手牌并退还能量
  - 这样以后即使别的特殊牌中途失败，也不会再出现“卡消失但回合未结束”的问题

当前结果：
- `冲撞` 在非贴身释放时，不会再因为补位移动被普通移动权限拦下
- 即使后续某一步失败，卡也会回滚，不会再消失

验证：
- 执行 `dotnet build`
- 结果：编译通过，0 error
