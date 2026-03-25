# Step 54 - 补充战斗与地图、成长、构筑、奖励、存档接口总方案

## 日期

2026-03-25

## 目标

在暂时只专注我们 battle 主链的前提下，把 battle 与地图、全局成长、构筑系统、奖励系统、存档系统之间的接口边界一次性规划清楚。

## 本次产出

- 大幅扩充 `Docs/战斗对外交互接口方案.md`

## 这次文档明确了什么

### 1. 顶层状态管理

- 需要一个统一全局入口脚本
- 建议继续由 `GlobalGameSession` 承担外观层职责
- 但不要把它做成巨型平铺脚本，而是挂接：
  - `PartyRuntimeState`
  - `ProgressionRuntimeState`
  - `DeckBuildState`
  - `InventoryRuntimeState`
  - `PendingBattleState`
  - `SaveRuntimeState`

### 2. 地图 -> 战斗

- 地图只传 `EncounterId`、`MapResumeContext`、`BattleRequest`
- 房间池选择仍由 battle 根据 encounter 解释
- 地图不直接传房间实例给 battle

### 3. 战斗初始化

- 战斗初始化不只要玩家快照，还需要考虑：
  - 荒川 / 队友快照
  - 成长快照
  - 构筑快照
  - 特殊起手牌堆初始化参数

### 4. 战斗结果

- 战斗结束只产出标准 `BattleResult`
- 胜利奖励、成长应用、背包入库、存档落盘都不直接在 battle 场景内执行

### 5. 奖励与成长

- 奖励应由 battle 外部解析器根据：
  - `EncounterId`
  - `RoomLayoutId`
  - `BattleOutcome`
  - `RuntimeFlags`
  来决定

### 6. 存档

- 推荐独立 `SaveGameService`
- 支持：
  - 多个手动存档槽
  - 1 个自动存档槽
- 失败时由 save / session 层决定回退，不由 battle 直接决定

## 当前最建议先做的实现方向

1. 扩展 `BattleRequest`
2. 扩展 `BattleResult`
3. 扩展 `GlobalGameSession`
4. 让 `BattleDeckState` 支持外部初始化参数

## 特别标注的风险

- 双系统并存导致脚本和 autoload 串线
- 奖励逻辑硬塞进 battle controller
- 构筑牌组和 runtime 牌堆混淆
- battle 场景直接参与存档策略判断

## 结果

从这一步开始，battle 主链后续不再只是“能打”，而是开始按最终可对接系统的标准来收边界。
