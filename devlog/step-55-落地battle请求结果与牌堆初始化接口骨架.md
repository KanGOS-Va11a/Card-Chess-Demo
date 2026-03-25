# Step 55 - 落地 battle 请求、结果与牌堆初始化接口骨架

## 日期

2026-03-25

## 目标

把 battle 的对外交互方案从“文档设计”推进到“可编译的代码骨架”，先落 battle 自己最关键的三块边界：

- `BattleRequest`
- `BattleResult`
- `BattleDeckState` 初始化入口

## 本次改动

### 1. Battle 边界 DTO

新增：

- `Scripts/Battle/Boundary/BattleOutcome.cs`
- `Scripts/Battle/Boundary/BattleRewardEntry.cs`

扩展：

- `Scripts/Battle/Boundary/BattleRequest.cs`
- `Scripts/Battle/Boundary/BattleResult.cs`

`BattleRequest` 现在已具备：

- `RequestId`
- `EncounterId`
- `RandomSeed`
- `PlayerSnapshot`
- `CompanionSnapshot`
- `ProgressionSnapshot`
- `DeckBuildSnapshot`
- `DeckRuntimeInitOverrides`
- `RuntimeModifiers`

`BattleResult` 现在已具备：

- `RequestId`
- `EncounterId`
- `Outcome`
- `PlayerSnapshot`
- `CompanionSnapshot`
- `ProgressionDelta`
- `InventoryDelta`
- `RewardEntries`
- `ClearedEncounterId`
- `RuntimeFlags`

### 2. GlobalGameSession

扩展 `Scripts/Battle/Shared/GlobalGameSession.cs`，加入 battle 主链需要的统一外观接口：

- 成长相关字段
- 构筑相关字段
- 背包计数字典
- `BuildCompanionSnapshot()`
- `BuildProgressionSnapshot()`
- `BuildDeckBuildSnapshot()`
- `BuildInventorySnapshot()`
- `ApplyCompanionSnapshot(...)`
- `ApplyProgressionSnapshot(...)`
- `ApplyDeckBuildSnapshot(...)`
- `ApplyProgressionDelta(...)`
- `ApplyInventoryDelta(...)`

这一步的目标不是把成长 / 背包系统做完，而是先把 battle 主链对外该依赖的接口固定出来。

### 3. BattleDeckState

新增：

- `Scripts/Battle/Cards/BattleDeckRuntimeInit.cs`

扩展：

- `Scripts/Battle/Cards/BattleDeckState.cs`

现在 `BattleDeckState` 已支持：

- 自定义 build cards
- 自定义起手牌
- 自定义抽牌堆
- 自定义弃牌堆
- 自定义消耗堆
- 自定义手牌上限
- 自定义最大能量
- 自定义初始能量
- 自定义开局抽牌数量

### 4. BattleSceneController

扩展 `Scripts/Battle/BattleSceneController.cs`：

- 记录当前 `BattleRequest`
- 从 `BattleRequest` 中消费：
  - `EncounterId`
  - `RandomSeed`
  - `DeckBuildSnapshot`
  - `DeckRuntimeInitOverrides`
- 生成 `BattleDeckRuntimeInit`
- 用请求中的 deck snapshot 初始化 battle runtime deck
- 在结算时把 `RequestId` / `EncounterId` 回写进 `BattleResult`

## 当前结果

battle 主链现在已经具备了后续真正和地图 / 成长 / 构筑 / 奖励对接时最核心的接口骨架，不再只有“玩家快照 + 失败标记”这种最小边界。

## 当前限制

- `ProgressionDelta` / `InventoryDelta` 目前还是字典增量骨架
- 奖励解析器还没有正式落地
- 构筑快照当前仍主要靠 card id 数组
- deck runtime 初始化还没有接入正式卡牌资源库

## 验证

- 已执行 `dotnet build`
- 构建通过，`0` 错误

## 风险

- 当前项目仍存在大量历史 `CS8632` nullable 警告，这次没有顺手清理
- 现在的接口骨架已经适合继续迭代，但还不适合直接拿来当最终存档格式
