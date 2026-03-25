# Step 49 - 地图域结构整理与文档迭代

## 日期

2026-03-25

## 目标

基于当前项目实际结构，对地图域代码做一次更明确的收束，并把文档修正到与代码一致的状态；同时确定后续变更统一用中文记录到 `devlog`。

## 本次改动

### 代码结构

- 新增 `Scripts/Map` 目录，并把地图域脚本按职责迁移到：
  - `Actors`
  - `Interaction`
  - `Controllers`
  - `Transitions`
- 地图域脚本统一进入 `CardChessDemo.Map` 命名空间。
- 主地图 `Scene/Mainlevel.tscn` 的根节点接入 `MapSceneController`，由它负责地图恢复。

### 职责整理

- `Player` 不再负责消费 `MapResumeContext` 和 `BattleResult`，只保留移动、交互、治疗接收。
- `MapSceneController` 成为地图恢复的唯一入口，并在找不到玩家节点时保留 pending 状态，不提前消费。
- `MapBattleTransitionHelper` 在切场景失败时会调用 `GlobalGameSession.CancelPendingBattleTransition()`，避免留下脏的 pending 战斗状态。

### 交互对象统一

- `Chest` 改为继承 `InteractableTemplate`，不再自己重复实现整套交互接口。
- `HealStation` 改为继承 `InteractableTemplate`，治疗逻辑直接调用 `player.ReceiveHeal(...)`。
- `InteractableTemplate` 新增统一的交互脉冲反馈方法，NPC、宝箱、治疗站可复用。
- `BattleEncounterEnemy` 去掉未实际使用的 `EnemyTypeId` 导出字段。

### 场景同步

- 所有地图原型场景的脚本引用已更新到 `Scripts/Map/...`
- `Mainlevel` 已显式配置 `PlayerPath = "Player"`

### 文档

- 重写并更新：
  - `README.md`
  - `Docs/项目总体架构设计.md`
  - `Docs/项目总体需求表.md`
- 新增：
  - `Docs/项目目录结构说明.md`
  - `devlog/README.md`
- 后续变更记录从本条开始改为中文。

## 验证

- 已执行 `dotnet build`
- 结果：构建通过，`0` 错误
- 当前仍存在一批旧的 `CS8632` nullable 警告，本次没有顺带处理

## 结果

这一步的价值不在于新增玩法，而在于把地图域从“脚本散落在根目录、职责交叉”收束为“目录明确、边界明确、恢复链路单点负责”的状态。后续继续迭代地图或战斗时，入口位置和维护约定都会更清晰。
