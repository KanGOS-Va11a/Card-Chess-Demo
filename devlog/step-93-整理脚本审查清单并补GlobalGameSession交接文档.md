# Step 93 - 整理脚本审查清单并补 GlobalGameSession 交接文档

## 日期

2026-03-30

## 目标

1. 审查当前脚本结构，迁移已经脱离主链的旧脚本到 `legacy`
2. 明确哪些旧脚本仍然被旧场景引用，因此这次只标记不搬
3. 补一份可直接给队友交接的 `GlobalGameSession` 说明文档

## 本次迁移

### 已迁移到 `legacy/2026-03-30-script-audit`

- `AutoLoad/GlobalBattleContext.cs`
- `AutoLoad/PartnerManager.cs`
- `Scripts/Battle/BattleQuickExitController.cs`
- `Scripts/Core/BattleRequest.cs`
- `Scripts/Core/BattleResult.cs`
- `Scripts/Character/Partner.cs`
- 配套旧场景：
  - `Scene/Battle.tscn`
  - `Scene/Partner.tscn`

### 同步处理

- `project.godot` 移除：
  - `GlobalBattleContext`
  - `PartnerManager`
- `newproject.csproj` 新增：
  - `Compile Remove=\"legacy\\**\\*.cs\"`

作用：

- 避免 legacy 归档脚本继续参与主工程编译
- 减少旧链路继续干扰当前主工程

## 本次新增文档

- `Docs/脚本审查与legacy迁移清单-2026-03-30.md`
- `Docs/GlobalGameSession交接说明-2026-03-30.md`

## 文档重点

### 1. 脚本审查清单

明确区分了：

- 当前主链
- 已确认冗余并迁移到 legacy 的脚本
- 虽然旧，但因为 `Scene1` 等场景仍直接引用，所以暂时不能搬的旧链脚本

### 2. GlobalGameSession 交接文档

正式写清了：

- `GlobalGameSession` 当前定位
- 它不是旧 `GameSession`
- 它当前内部状态结构
- 它依赖的新分层服务
- battle / map / save 的衔接方式
- 当前 player snapshot 的 base / resolved 双层语义
- 队友后续扩展时应该怎么接，哪些事情不要再做

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍有项目历史 nullable warnings，本次未处理
