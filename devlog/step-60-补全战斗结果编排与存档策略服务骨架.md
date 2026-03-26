# Step 60 - 补全战斗结果编排与存档策略服务骨架

## 日期

2026-03-26

## 目标

继续把 battle 的对外交互框架从“状态对象 + 请求结果 DTO”推进到“战斗结束后可交由外部系统消费的流程编排层”。

## 本次改动

### 1. 继续结构化的正式模型

新增：

- `Scripts/Battle/Boundary/ProgressionSnapshot.cs`
- `Scripts/Battle/Boundary/DeckBuildSnapshot.cs`
- `Scripts/Battle/Boundary/BattleRewardContext.cs`
- `Scripts/Battle/Boundary/RewardBundle.cs`

### 2. 服务边界层

新增：

- `Scripts/Battle/Encounters/BattleEncounterResolution.cs`
- `Scripts/Battle/Encounters/BattleEncounterResolver.cs`
- `Scripts/Battle/Services/BattleRequestBuilder.cs`
- `Scripts/Battle/Services/BattleRewardResolver.cs`
- `Scripts/Battle/Services/SaveSlotDecision.cs`
- `Scripts/Battle/Services/SaveSlotPolicy.cs`
- `Scripts/Battle/Services/BattleResolutionPlan.cs`
- `Scripts/Battle/Services/BattleResolutionService.cs`
- `Scripts/Battle/Services/SaveGameData.cs`
- `Scripts/Battle/Services/SaveGameService.cs`

### 3. SaveRuntimeState 扩展

补充：

- 自动存档槽 id
- checkpoint scene / map / spawn 信息
- 自动存档时间戳
- 首选回滚槽类型

### 4. GlobalGameSession 扩展

新增能力：

- 构建正式 `ProgressionSnapshot`
- 构建正式 `DeckBuildSnapshot`
- 构建保存用 `SaveRuntimeSnapshot`
- 应用背包快照
- 应用保存运行时快照

## 当前结果

现在 battle 主链已经具备：

1. DTO / 结构体层
2. 全局状态层
3. 基础校验层
4. 服务边界层
5. 结果编排层

这意味着后续地图、成长、背包、存档系统再接 battle 时，已经不需要直接对着 `BattleSceneController` 或 `GlobalGameSession` 的细节乱接。

## 当前限制

- `BattleRewardResolver` 还没有正式奖励表
- `BattleResolutionService` 还没有真正接到地图侧结果消费链
- `SaveGameService` 还没有做文件槽位管理
- `SaveSlotPolicy` 目前只有最小策略，不包含更复杂规则

## 验证

- 已执行 `dotnet build`
- 构建通过，`0` 错误
