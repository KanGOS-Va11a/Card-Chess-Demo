# Step 58 - 补全全局状态子结构与请求结果基础校验

## 日期

2026-03-25

## 目标

继续把 battle 对外交互框架从“字段扩展”推进到“有基本约束的可依赖基础设施”。

## 本次改动

### 1. 全局状态子结构

新增：

- `Scripts/Battle/Shared/PartyRuntimeState.cs`
- `Scripts/Battle/Shared/ProgressionRuntimeState.cs`
- `Scripts/Battle/Shared/DeckBuildState.cs`
- `Scripts/Battle/Shared/InventoryRuntimeState.cs`
- `Scripts/Battle/Shared/SaveRuntimeState.cs`

`GlobalGameSession` 现在内部已经挂接：

- `PartyState`
- `ProgressionState`
- `DeckBuildState`
- `InventoryState`
- `SaveState`

### 2. 兼容策略

当前没有直接删除旧字段，而是采用：

- 子状态对象作为 battle 主链内部结构
- 旧字段继续保留作镜像
- 通过同步方法维持兼容

这样做的原因是：

- 当前已有控制器和资源仍依赖旧字段
- 这一步先把结构搭起来，不在同一轮里做大拆

### 3. 请求 / 结果基础校验

`BattleRequest` 新增：

- `TryValidate(out string failureReason)`

`BattleResult` 新增：

- `TryValidate(out string failureReason)`

`GlobalGameSession` 现在会在：

- `BeginBattle(...)`
- `CompleteBattle(...)`

这两个入口执行校验，并在非法时拒绝继续流转。

## 当前约束范围

### `BattleRequest`

当前最小校验：

- `RequestId` 必须存在
- `PlayerSnapshot.current_hp` 必须存在
- `PlayerSnapshot.max_hp` 必须存在

### `BattleResult`

当前最小校验：

- `Outcome` 不能是 `Unknown`
- `PlayerSnapshot.current_hp` 必须存在

## 结果

battle 主链现在已经不只是“有接口字段”，而是开始具备：

- 全局状态结构分层
- 入口校验
- 基本的非法请求 / 非法结果拦截

## 当前限制

- 校验目前仍是最小版本，没有做更细的字段一致性检查
- 子状态对象与旧字段目前还是双轨并存
- 这一步还没有继续拆出单独的 save service / reward resolver / progression service

## 验证

- 已执行 `dotnet build`
- 构建通过，`0` 错误
