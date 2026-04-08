# step-180 接入 Scene01 敌人学习奖励与 debug 卡牌解锁

时间：2026-04-08

本次目标：
- 以 `Scene01` 当前敌人为样板，做出第一条真正可测的学习奖励链
- 修正构筑 / 图鉴中看不到卡牌的问题
- 保持地图接口不变，只改 battle 与 session 侧

本次完成内容：

1. Scene01 遭遇正式指向独立敌人定义
- `grunt_debug` 遭遇不再继续吃泛用 `battle_enemy`
- 现在改为：
  - `PrimaryEnemyDefinitionId = "scene01_tutorial_enemy"`
- 文件：
  - `Resources/Battle/Encounters/DebugBattleEncounterLibrary.tres`

2. Scene01 敌人独特行为接入
- 新增 `scene01_learning` AI
- 行为特点：
  - 默认按侦察 / 绕侧思路行动
  - 生命值低于一半时，移动力额外 `+1`
- 文件：
  - `Scripts/Battle/AI/Strategies/Scene01LearningEnemyAiStrategy.cs`
  - `Scripts/Battle/AI/EnemyAiRegistry.cs`
  - `Scripts/Battle/Rooms/BattleRoomTemplate.cs`

3. Scene01 两张学习奖励牌正式加入卡牌库
- 普通学习奖励牌：
  - `card_patrol_strike`
  - 名称：`巡猎突刺`
- 特色学习奖励牌：
  - `card_last_chase`
  - 名称：`穷追`
- 两张牌都作为 `IsLearnedCard = true` 接入，默认不参与 debug 全解锁
- 文件：
  - `Resources/Battle/Cards/DefaultBattleCardLibrary.tres`

4. 战后奖励脚本正式能回写卡牌解锁
- `ProgressionDelta` 新增：
  - `UnlockedCardIds`
- `GlobalGameSession.ApplyProgressionDelta(...)` 现在会把解锁卡牌合并回 `ProgressionState.UnlockedCardIds`
- `GlobalGameSession.CompleteBattle(...)` 现在会调用 `BattleResolutionService`
- `BattleRewardResolver` 对 `grunt_debug` 胜利时发放：
  - `40` 点经验
  - `card_patrol_strike`
  - `card_last_chase`
- 文件：
  - `Scripts/Battle/Boundary/ProgressionDelta.cs`
  - `Scripts/Battle/Shared/GlobalGameSession.cs`
  - `Scripts/Battle/Services/BattleRewardResolver.cs`

5. 修正 debug 阶段的卡牌可见性
- `SystemFeatureLabController` 新增 debug 解锁逻辑
- 当前会把卡牌库中所有 `非学习奖励牌` 自动并入已解锁卡池
- 同时补齐三系分支标签：
  - `melee`
  - `ranged`
  - `flex`
- 目的：
  - 让构筑与图鉴在 debug 阶段能直接看到当前已设计好的卡牌
  - 但不提前显示学习奖励牌，保留测试空间
- 文件：
  - `Scripts/Map/UI/SystemFeatureLabController.cs`

6. Scene01 教程敌人的显示定义补齐
- `scene01_tutorial_enemy` 已加入 battle prefab 库
- 文件：
  - `Resources/Battle/Presentation/DefaultBattlePrefabLibrary.tres`

当前结论：
- 卡牌系统不是整体坏掉
- 当前“图鉴和构筑看不到卡牌”的主要问题在于：
  - 已解锁卡池没有被正确喂给 session
  - 分支标签要求没有在 debug 阶段补齐
- 这次已经用 debug 解锁逻辑兜上

当前建议测试路径：
1. 打开 `Scene01`
2. 按 `C` 查看图鉴 / 构筑
3. 先确认普通设计牌是否出现
4. 与 `Scene01` 敌人战斗并获胜
5. 返回地图后再看是否新增：
   - `巡猎突刺`
   - `穷追`
