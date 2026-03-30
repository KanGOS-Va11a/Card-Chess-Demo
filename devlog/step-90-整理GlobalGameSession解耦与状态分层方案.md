# Step 90 - 整理 GlobalGameSession 解耦与状态分层方案

## 日期

2026-03-30

## 目标

在不继续硬编码无必要内容的前提下，先把 `GlobalGameSession` 后续怎么解耦这件事整理成一份正式方案文档，避免后面一边开发一边临时拍脑袋拆。

## 本次产出

新增文档：

- `Docs/GlobalGameSession解耦与状态分层方案.md`

## 文档核心结论

### 1. 不删除 `GlobalGameSession`

方案明确：

- `GlobalGameSession` 继续保留
- 它仍然是全局状态真源
- 它仍然是 battle / map / save 的边界外观层

也就是说，后续不是把它拆没，而是把它“收瘦”。

### 2. 真正需要拆出去的是三类内容

- 规则
- 定义
- 解析

当前最不该继续硬编码在 `GlobalGameSession` 里的内容包括：

- 装备定义表
- 装备效果映射
- 经验曲线
- 天赋数值规则

### 3. 明确了目标结构

文档里正式提出后续应拆成：

- `GlobalGameSession`
- `EquipmentLoadoutState`
- `EquipmentCatalog`
- `ProgressionRuleSet`
- `PlayerStatResolver`
- `EquipmentService`
- `ResolvedPlayerStats`

### 4. 采用渐进式迁移，不破坏 battle / map 主链

文档把后续拆分分成四阶段：

1. 先补状态对象
2. 再外移解析逻辑
3. 再资源化规则与定义
4. 最后让 battle / UI 全部统一走解析层

这样可以尽量保证：

- `BattleRequest`
- `BattleResult`
- `BuildPlayerSnapshot()`
- battle / map 对接入口

这些已有边界不被随意打断。

## 结果

从这一步开始，`GlobalGameSession` 的后续演进方向已经明确：

- 它保留
- 但不再继续无节制扩张
- 后续新增系统优先加到资源、规则类和解析服务，而不是继续把逻辑硬塞进 session
