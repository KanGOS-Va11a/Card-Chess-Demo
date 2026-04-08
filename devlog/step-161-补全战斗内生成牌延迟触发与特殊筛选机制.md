# step-161 补全战斗内生成牌、延迟触发与特殊筛选机制

时间：2026-04-07

本次目标：
- 补全之前识别出的三类“小机制缺口”
- 不碰 `学习` 这条大战后链
- 让设计 wiki 中更多卡牌可以直接落地到当前战斗系统

本次实际改动：

## 1. 战斗内生成临时牌

文件：
- `Scripts/Battle/Cards/BattleDeckState.cs`
- `Scripts/Battle/BattleSceneController.cs`

内容：
- `BattleDeckState` 新增：
  - `AddTemporaryCardToHand(...)`
  - `AddTemporaryCardToDrawPileTop(...)`
- 当前已经用于：
  - `瞄准` -> 生成 1 张临时 `狙击` 到手牌

效果：
- 战斗中可以动态生成临时卡，而不需要预先写死在整副牌组里

## 2. 延迟触发效果

文件：
- `Scripts/Battle/BattleSceneController.cs`

内容：
- 新增轻量延迟效果结构 `PendingDelayedCardEffect`
- 当前支持：
  - `AlertStrike`
- 新增结算入口：
  - `ResolvePendingDelayedCardEffectsForTurnStart(...)`
  - 在玩家新回合开始时触发

当前已落地卡牌：
- `戒备`
  - 出牌后登记一个下回合开始触发的效果
  - 到下回合开始时，对范围 2 格内全部敌人造成 6 伤害

## 3. 特殊筛选目标

文件：
- `Scripts/Battle/BattleSceneController.cs`

内容：
- 新增：
  - `FindLowestHpEnemyInRange(...)`
  - `TryResolveRollCallHit(...)`

当前已落地卡牌：
- `点名`
  - 自动选取距离 2 格内生命最低的敌人
  - 造成 3 伤害
  - 若击杀，则再重复一次

## 4. 新增代表牌

当前已注入运行时牌库：
- `瞄准`
- `狙击`
- `戒备`
- `点名`

同时，当前 debug 测试链也会把以下牌加入开局手牌：
- `处决`
- `拔枪`
- `电弧泄露`
- `冲撞`
- `瞄准`
- `戒备`
- `点名`

说明：
- 这轮做的是“机制补口 + 代表牌验证”
- 不是一次性把 wiki 全部卡牌都录完
- 但现在已经说明：
  - 战斗内生成牌可用
  - 延迟到下回合触发可用
  - 特殊筛选目标可用

结论：
- 除了 `学习` 及更完整的 buff / 状态体系外
- 当前设计 wiki 中大部分卡牌已经不再缺底层机制

验证：
- 执行 `dotnet build`
- 结果：编译通过，0 error
