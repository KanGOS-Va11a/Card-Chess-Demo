# GlobalGameSession 解耦与状态分层方案

## 1. 文档目标

本方案只解决一个问题：

- `GlobalGameSession` 目前承担了过多职责，后续如果继续把装备、成长、背包、构筑、局外 UI 逻辑直接堆进去，会越来越难维护、越来越难 merge、也越来越难与地图层和成长层稳定对接。

本方案的目标不是“完全去掉 `GlobalGameSession`”，而是：

- 保留它作为全局状态真源与跨场景边界入口
- 把不该长期放在里面的“规则”“定义”“解析逻辑”拆出去
- 在不破坏现有 battle / map 主链的前提下完成渐进式重构

## 2. 当前问题

当前 `GlobalGameSession` 里已经同时混入了四类内容：

1. 全局运行时状态
- 主角当前生命
- 荒川当前能量
- 当前等级 / 经验 / 专精点
- 当前牌组构筑
- 当前背包
- 当前装备槽

2. 边界编排职责
- 暂存 `BattleRequest`
- 暂存 `BattleResult`
- 暂存 `MapResumeContext`

3. 规则逻辑
- 经验曲线
- 装备槽位合法性判定
- 装备加成计算
- 天赋数值叠加解析

4. 数据定义
- 当前 demo 装备效果映射
- 槽位字符串约定

真正的问题不在于 `GlobalGameSession` 大，而在于它同时扮演了：

- 状态容器
- 规则中心
- 配置表
- 属性解析器

这会带来几个直接风险：

- merge 时共享文件冲突频率极高
- 地图 / UI / battle 都会被迫依赖同一个“大类”
- battle 运行时和局外系统更难分层
- 后续资源化装备和成长时必须改动核心共享文件

## 3. 解耦原则

本方案采用以下原则：

### 3.1 保留单一状态真源

`GlobalGameSession` 仍然保留，且继续作为：

- 跨场景共享状态容器
- battle 与 map 的边界入口
- 存档序列化入口

不建议在当前项目里拆成多个 Autoload 去抢状态真源。

### 3.2 状态、规则、定义、解析分层

后续明确拆成四层：

1. 状态层
- 只保存“现在是什么”

2. 定义层
- 只保存“有哪些装备 / 成长规则 / 天赋规则 / 词条”

3. 规则层
- 只保存“如何计算”

4. 解析层
- 只负责把状态 + 定义 + 规则组合成 battle 和局外要读的结果

### 3.3 边界层稳定优先

后续重构时，优先保持这些外部接口稳定：

- `BattleRequest`
- `BattleResult`
- `GlobalGameSession.BeginBattle(...)`
- `GlobalGameSession.CompleteBattle(...)`
- `GlobalGameSession.BuildPlayerSnapshot()`
- `GlobalGameSession.BuildProgressionSnapshotModel()`

也就是说：

- 可以改内部实现
- 尽量不改 battle / map 对外接口名

### 3.4 不硬编码无必要内容

以下内容后续不应继续硬编码在 `GlobalGameSession` 或测试 UI 控制器里：

- 装备条目定义
- 装备效果映射
- 经验曲线参数
- 槽位到效果的详细关系
- 天赋数值规则表

真正允许保留硬编码的只应是：

- 最小边界方法名
- 少量稳定常量
- demo 过渡期兼容代码

## 4. 目标分层结构

建议最终形成如下结构：

```text
GlobalGameSession
  ├─ PartyRuntimeState
  ├─ ProgressionRuntimeState
  ├─ DeckBuildState
  ├─ InventoryRuntimeState
  ├─ EquipmentLoadoutState
  ├─ SaveRuntimeState
  ├─ BattleBoundaryState
  │    ├─ PendingBattleRequest
  │    ├─ LastBattleResult
  │    └─ PendingMapResumeContext
  ├─ Query facade
  │    ├─ BuildPlayerSnapshot()
  │    ├─ BuildProgressionSnapshotModel()
  │    └─ BuildDeckBuildSnapshotModel()
  └─ Thin state mutation facade
	   ├─ ApplyProgressionDelta(...)
	   ├─ ApplyInventoryDelta(...)
	   ├─ Equip / Unequip
	   └─ Begin / CompleteBattle

EquipmentCatalog
  └─ 装备定义资源集合

ProgressionRuleSet
  └─ 经验曲线 / 升级规则资源

TalentRuleCatalog
  └─ 天赋数值修正与 granted tags / granted effects 定义

PlayerStatResolver
  ├─ 输入：session state + catalogs
  └─ 输出：ResolvedPlayerStats

EquipmentService
  ├─ 装备合法性校验
  └─ 穿脱装备流程
```

## 5. 各层职责划分

### 5.1 `GlobalGameSession`

应该保留的职责：

- 保存当前玩家状态
- 保存当前成长状态
- 保存当前背包状态
- 保存当前装备槽已装备的 item id
- 保存 battle 请求 / 结果边界状态
- 保存存档相关状态
- 提供快照导出方法
- 提供状态应用方法

不应长期保留的职责：

- 装备效果数值表
- 经验曲线算法细节
- 天赋修正解析细节
- 装备槽位判定规则

### 5.2 `EquipmentCatalog`

职责：

- 维护装备定义资源
- 每件装备的：
  - `ItemId`
  - 显示名
  - 槽位
  - 描述
  - 附加效果列表
  - 限制条件

建议形式：

- `Resource` 资源集合
- 或目录扫描后的资源库

不应放在这里的内容：

- 当前玩家是否拥有
- 当前玩家是否已装备

### 5.3 `ProgressionRuleSet`

职责：

- 升级经验曲线
- 专精点授予策略
- 等级上限
- 特殊成长阈值

建议形式：

- 一个集中资源
- 支持后续竞赛版与正式版切换

### 5.4 `PlayerStatResolver`

职责：

- 读取：
  - 基础主角状态
  - 已解锁天赋
  - 已装备装备
  - 成长规则
- 输出：
  - 解析后的生命
  - 解析后的移动
  - 解析后的攻击
  - 解析后的防御减伤
  - 解析后的防御附盾

它应成为 battle 和局外 UI 都可共用的统一解析入口。

### 5.5 `EquipmentService`

职责：

- 校验装备是否存在
- 校验玩家是否拥有
- 校验槽位是否匹配
- 执行穿装备 / 脱装备
- 返回失败原因

也就是说：

- `GlobalGameSession` 保存结果
- `EquipmentService` 负责流程和规则

## 6. 推荐新增的数据结构

### 6.1 `EquipmentLoadoutState`

建议新增一个独立状态对象，而不是继续把三条装备字段平铺在 `GlobalGameSession` 根部：

```csharp
public sealed class EquipmentLoadoutState
{
	public string WeaponItemId { get; set; } = string.Empty;
	public string ArmorItemId { get; set; } = string.Empty;
	public string AccessoryItemId { get; set; } = string.Empty;
}
```

短期兼容策略：

- `GlobalGameSession` 先继续保留当前三条导出字段
- 内部开始同步到 `EquipmentLoadoutState`
- 外部迁移完成后，再考虑把根部字段降级为兼容镜像

### 6.2 `ResolvedPlayerStats`

建议新增 battle / UI 共用的解析结果对象：

```csharp
public sealed class ResolvedPlayerStats
{
	public int MaxHp { get; set; }
	public int MovePointsPerTurn { get; set; }
	public int AttackRange { get; set; }
	public int AttackDamage { get; set; }
	public int DefenseDamageReductionPercent { get; set; }
	public int DefenseShieldGain { get; set; }
}
```

这样可以避免：

- battle 读一个方法
- UI 再读另一个方法
- 两边各自拼装一份结果

### 6.3 `EquipmentDefinition`

当前已经在测试 UI 里有 demo 版定义，但应正式迁移为资源定义：

```csharp
public partial class EquipmentDefinition : Resource
{
	[Export] public string ItemId { get; set; } = string.Empty;
	[Export] public string DisplayName { get; set; } = string.Empty;
	[Export] public string SlotId { get; set; } = string.Empty;
	[Export] public EquipmentModifierDefinition[] Modifiers { get; set; } = Array.Empty<EquipmentModifierDefinition>();
}
```

### 6.4 `EquipmentModifierDefinition`

不要继续写成：

- 某个具体 item id 就返回 `+2 攻击`

而应写成：

```csharp
public partial class EquipmentModifierDefinition : Resource
{
	[Export] public string ModifierTypeId { get; set; } = string.Empty;
	[Export] public int IntValue { get; set; }
	[Export] public float FloatValue { get; set; }
	[Export] public string StringValue { get; set; } = string.Empty;
}
```

这样可以避免“装备效果规则完全硬编码”。

## 7. 边界不变、内部解耦的迁移路线

本方案采用四阶段迁移。

### 第 1 阶段：状态对象补齐

目标：

- 新增 `EquipmentLoadoutState`
- `GlobalGameSession` 内部从“平铺字段”转为“状态对象 + 兼容镜像”

这一阶段不改 battle / map 外部接口。

### 第 2 阶段：解析逻辑外移

目标：

- 新增 `PlayerStatResolver`
- 把 `GetResolvedPlayerMaxHp()` 等方法内部实现改为委托给 resolver

这一阶段外部方法名仍保持不变。

### 第 3 阶段：规则与定义资源化

目标：

- 新增 `EquipmentCatalog`
- 新增 `ProgressionRuleSet`
- 逐步删除 `GlobalGameSession` 内部的硬编码装备效果和经验曲线

这一阶段最关键的是“替换实现，不替换边界”。

### 第 4 阶段：UI 与 battle 统一走解析层

目标：

- battle 统一读 `ResolvedPlayerStats`
- 局外状态页也统一读 `ResolvedPlayerStats`
- 测试场景控制器里移除 demo 级装备定义表

这一阶段完成后，状态、规则、解析才算真正分层。

## 8. 与 battle / map 衔接的影响控制

为了避免重构过程中影响 battle 和地图对接，必须遵守：

### 8.1 battle 入场边界保持不变

保持：

- `BuildPlayerSnapshot()`
- `BuildProgressionSnapshotModel()`
- `BuildDeckBuildSnapshotModel()`

继续存在。

### 8.2 地图侧不直接碰解析器细节

地图层仍然只依赖：

- `GlobalGameSession`
- `BattleRequest`
- `BattleResult`

不要让地图层直接知道 `EquipmentCatalog` 或 `PlayerStatResolver` 的内部细节。

### 8.3 存档入口仍然从 session 收口

即使未来拆出更多服务，存档仍建议继续以 `GlobalGameSession` 为聚合导出入口。

## 9. merge 风险与控制策略

如果不解耦，后续最容易冲突的仍然是：

- `GlobalGameSession.cs`
- `SystemFeatureLabController.cs`
- 各种局外 UI 控制器

采用本方案后，未来改动会分散到：

- 状态类
- 规则类
- 目录化资源
- resolver / service

这样 merge 冲突会显著减少。

建议的控制策略：

- 共享状态字段改动优先少做
- 新效果优先加资源，不优先改 `GlobalGameSession`
- UI 页面改动不要再顺手写规则逻辑

## 10. 本方案下哪些内容仍然允许暂时保留在 `GlobalGameSession`

竞赛周期很短，因此以下内容允许暂时保留在 `GlobalGameSession`，但要标注为过渡实现：

- `GetResolved...()` 这组对外方法
- 兼容旧代码的平铺字段
- 简单的 battle 边界编排逻辑

不建议继续新增到 `GlobalGameSession` 的内容：

- 新装备定义表
- 新经验曲线逻辑
- 新天赋数值映射
- 新构筑规则表

## 11. 推荐的近期执行顺序

如果后续开始落地，建议顺序是：

1. 新增 `EquipmentLoadoutState`
2. 新增 `ResolvedPlayerStats`
3. 新增 `PlayerStatResolver`
4. 让 `GlobalGameSession.GetResolved...()` 转调 resolver
5. 新增 `ProgressionRuleSet`
6. 新增 `EquipmentCatalog`
7. 把测试 UI 的示范装备定义迁出控制器

这样能在最小风险下先把“状态 / 解析”分开，再慢慢把“规则 / 定义”挪出去。

## 12. 最终结论

`GlobalGameSession` 不应该被删除。

它必须继续承担：

- 全局状态真源
- battle / map 边界入口
- 存档聚合入口

真正需要解耦出去的是：

- 规则
- 定义
- 解析

因此后续正确方向不是：

- “把 `GlobalGameSession` 拆没”

而是：

- “把 `GlobalGameSession` 收敛成状态容器 + 边界外观层”

这是当前项目在竞赛工期内最稳、也最适合继续协作推进的方案。

## 13. 当前已落地状态

截至 2026-03-30，本方案已经落地到以下程度：

### 已落地

- 新增 `EquipmentLoadoutState`
- 新增 `EquipmentCatalog`
- 新增 `EquipmentService`
- 新增 `ProgressionRuleSet`
- 新增 `ResolvedPlayerStats`
- 新增 `PlayerStatResolver`
- `GlobalGameSession` 已开始把：
  - 装备合法性判断
  - 穿脱装备流程
  - 经验曲线读取
  - 主角解析属性计算
  委托给新层实现
- `BattleObjectStateManager` 已开始读取 `ResolvePlayerStats()` 结果，而不是自己拼装玩家数值
- `SystemFeatureLabController` 已开始通过 session 查询正式装备定义入口，而不是继续直接依赖局部硬编码定义作为主数据源

### 仍属过渡实现

- `EquipmentCatalog` 当前仍通过 `CreateDemoFallback()` 提供 demo 装备数据
- `ProgressionRuleSet` 当前仍通过 `CreateDefaultDemoRuleSet()` 提供 demo 经验曲线
- `SystemFeatureLabController` 文件内仍残留一份旧的本地示范装备定义结构，当前已不再作为主链读取源，后续应清理

### 当前阶段判断

这意味着本方案当前已经完成了：

- 第 1 阶段：状态对象补齐
- 第 2 阶段的一部分：解析逻辑外移

但还没有完成：

- 正式资源化装备定义
- 正式资源化成长曲线
- UI 侧残留过渡定义的最终清理

所以当前正确理解应是：

- 接口骨架已经搭好并最小跑通
- 但定义数据层仍处于 demo 过渡态
