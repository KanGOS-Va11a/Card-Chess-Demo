# Step 91 - 落地 GlobalGameSession 分层骨架并打通最小运行链路

## 日期

2026-03-30

## 目标

按上一条方案文档开始真正拆分 `GlobalGameSession` 的内部职责，但不破坏 battle / map 主链，至少做到：

- 接口结构搭好
- 最小运行闭环打通
- 暂未正式资源化的部分用注释说明后续方向

## 本次新增结构

### 1. 状态层

新增：

- `Scripts/Battle/Shared/EquipmentLoadoutState.cs`

作用：

- 把装备槽状态从 `GlobalGameSession` 的平铺字段中抽出为独立状态对象

### 2. 装备定义与服务层

新增：

- `Scripts/Battle/Equipment/EquipmentSlotIds.cs`
- `Scripts/Battle/Equipment/EquipmentModifierDefinition.cs`
- `Scripts/Battle/Equipment/EquipmentDefinition.cs`
- `Scripts/Battle/Equipment/EquipmentCatalog.cs`
- `Scripts/Battle/Equipment/EquipmentService.cs`

作用：

- 让“装备有哪些”“能不能穿”“怎么查槽位”开始从 session 本体中分离

说明：

- `EquipmentCatalog.CreateFromConfiguredResources()` 当前仍回退到 `CreateDemoFallback()`
- 方法体内已写注释，明确后续应替换为资源 / 表驱动加载

### 3. 成长规则层

新增：

- `Scripts/Battle/Progression/ProgressionRuleSet.cs`

作用：

- 把经验曲线从 `GlobalGameSession` 中抽出来

说明：

- `CreateFromConfiguredRules()` 当前仍回退到 demo 线性曲线
- 方法体内已写注释，明确后续应换成正式规则资源

### 4. 属性解析层

新增：

- `Scripts/Battle/Stats/ResolvedPlayerStats.cs`
- `Scripts/Battle/Stats/PlayerStatResolver.cs`

作用：

- 把“主角基础状态 + 天赋 + 装备 = battle 可读数值”这件事集中到 resolver

## 本次改动

### 1. `GlobalGameSession` 改为委托型外观层

更新：

- `Scripts/Battle/Shared/GlobalGameSession.cs`

处理：

- 新增：
  - `EquipmentLoadoutState`
  - `RuntimeEquipmentCatalog`
  - `RuntimeProgressionRuleSet`
- 内部新增：
  - `EquipmentService`
  - `PlayerStatResolver`
- 原有这些对外方法保留，但内部已开始委托新层：
  - `GetResolvedPlayerMaxHp()`
  - `GetResolvedPlayerMovePointsPerTurn()`
  - `GetResolvedPlayerAttackDamage()`
  - `GetResolvedPlayerDefenseDamageReductionPercent()`
  - `GetResolvedPlayerDefenseShieldGain()`
  - `TryEquipItem(...)`
  - `UnequipItem(...)`
  - `GetExperienceRequiredForNextLevel()`
  - `GetExperienceProgressWithinLevel()`
  - `GetExperienceNeededToLevelUp()`

新增外观接口：

- `ResolvePlayerStats()`
- `FindEquipmentDefinition(string itemId)`
- `GetEquipmentDefinitionsForSlot(string slotId)`

### 2. battle 读取层开始统一依赖解析结果

更新：

- `Scripts/Battle/State/BattleObjectStateManager.cs`

处理：

- 改为调用 `GlobalGameSession.ResolvePlayerStats()`
- 玩家生命、移动、攻击不再由 state manager 自己拼装

### 3. 测试 UI 开始依赖共享装备定义入口

更新：

- `Scripts/Map/UI/SystemFeatureLabController.cs`

处理：

- 状态页装备列表改为通过：
  - `GetEquipmentDefinitionsForSlot(...)`
  - `FindEquipmentDefinition(...)`
  读取共享装备定义入口
- 角色状态页、穿脱装备链路仍保持可运行

## 当前结果

当前已经实现的不是“完整正式资源化系统”，而是：

- `GlobalGameSession` 不再自己直接承载全部规则
- 装备、成长、解析已经开始拆层
- 但对外接口名尽量保持稳定

这意味着后续继续扩展时：

- 可以继续优化内部实现
- 不需要马上打断现有 battle / map 对接

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍有项目历史 nullable warnings，本次未处理

## 残留问题

- `SystemFeatureLabController.cs` 内仍残留一份旧的本地示范装备定义结构，当前已不再作为主链读取源，后续应清理
- 装备与成长规则当前仍使用 demo fallback 数据，不是正式资源化实现
