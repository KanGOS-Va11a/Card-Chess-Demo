# Step 59 - 补齐遭遇解析、奖励解析与存档服务边界骨架

## 日期

2026-03-25

## 目标

继续把 battle 对外交互框架从“状态对象 + DTO”推进到“真正可对接的服务边界层”。

## 本次改动

### 1. 正式结构体继续补全

新增：

- `Scripts/Battle/Boundary/ProgressionSnapshot.cs`
- `Scripts/Battle/Boundary/DeckBuildSnapshot.cs`
- `Scripts/Battle/Boundary/ProgressionDelta.cs`
- `Scripts/Battle/Boundary/InventoryDelta.cs`
- `Scripts/Battle/Boundary/BattleRewardContext.cs`
- `Scripts/Battle/Boundary/RewardBundle.cs`

### 2. 遭遇解析服务

新增：

- `Scripts/Battle/Encounters/BattleEncounterResolution.cs`
- `Scripts/Battle/Encounters/BattleEncounterResolver.cs`

用途：

- 让外部系统不直接依赖 `BattleEncounterLibrary` 的资源结构
- 先通过 resolver 拿到标准 resolution

### 3. 请求构建服务

新增：

- `Scripts/Battle/Services/BattleRequestBuilder.cs`

用途：

- 统一由 battle 主链根据：
  - `GlobalGameSession`
  - `EncounterId`
  - 运行时修正
  构建 `BattleRequest`

### 4. 奖励解析服务

新增：

- `Scripts/Battle/Services/BattleRewardResolver.cs`

用途：

- 提供 battle 外部奖励解析入口
- 当前只返回最小空壳与上下文标记，不在这里硬编码正式奖励表

### 5. 存档服务

新增：

- `Scripts/Battle/Services/SaveGameData.cs`
- `Scripts/Battle/Services/SaveGameService.cs`

用途：

- 从 `GlobalGameSession` 构建可保存数据
- 从保存数据回写 `GlobalGameSession`
- 提供 JSON 序列化 / 反序列化入口

### 6. SaveRuntimeState 扩展

扩展：

- `AutoSaveSlotId`
- `LastCheckpointScenePath`
- `LastCheckpointMapId`
- `LastCheckpointSpawnId`
- `LastAutoSaveTimestampUtc`
- `PreferredRollbackSlotKind`

## 当前结果

battle 主链现在已经具备 3 层基础：

1. DTO / 结构体层
2. 全局状态层
3. 服务边界层

也就是说，后续地图、成长、奖励、存档系统已经不需要直接对着 `BattleSceneController` 对接。

## 当前限制

- `BattleRewardResolver` 还没有正式奖励表逻辑
- `SaveGameService` 目前只做到数据构建和 JSON 串转换，还没有实际槽位文件管理
- `BattleRequestBuilder` 还没有和地图侧真正接线
- `BattleEncounterResolver` 当前还没有权重、条件过滤、禁用规则

## 验证

- 已执行 `dotnet build`
- 构建通过，`0` 错误
