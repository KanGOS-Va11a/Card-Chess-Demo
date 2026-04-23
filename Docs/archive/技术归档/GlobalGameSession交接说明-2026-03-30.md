# GlobalGameSession 交接说明

## 1. 文档目标

这份文档用于给队友快速交接当前 `GlobalGameSession` 的真实职责、内部结构、对外接口和使用边界。

它不是抽象设计稿，而是基于当前代码现状整理的“可协作说明书”。

核心文件：

- [GlobalGameSession.cs](D:/Godot/newproject/Card-Chess-Demo/Scripts/Battle/Shared/GlobalGameSession.cs)

## 2. 它在当前项目中的定位

`GlobalGameSession` 当前不是一个“可选工具类”，而是 battle / map 主链的核心共享状态真源。

它同时承担三种角色：

1. 全局状态容器
- 保存主角、荒川、成长、构筑、背包、装备、存档相关状态

2. 跨场景边界外观层
- 提供 battle 入场和 battle 结算的统一入口

3. 快照聚合入口
- 提供 `BuildPlayerSnapshot()`、`BuildProgressionSnapshotModel()`、`BuildDeckBuildSnapshotModel()` 等快照方法

一句话理解：

- 当前所有“应该跨地图和战斗持续存在”的状态，正式主链都应优先走 `GlobalGameSession`

## 3. 它不是什么

它不是：

- 旧 `GameSession` 的同义替代名
- UI 专用数据容器
- 只给 battle 单独用的临时状态对象
- 最终规则中心

尤其要避免混淆：

- `GlobalGameSession`：当前 battle / map 主链接口真源
- `GameSession`：旧地图原型链仍在用的旧会话对象

后续新功能不要再继续接到 `GameSession` 上。

## 4. 当前内部状态结构

当前 `GlobalGameSession` 内部已经挂接的正式状态对象有：

### 4.1 `PartyRuntimeState`

管理：

- 主角基础运行时状态
- 荒川基础运行时状态

其中主角当前包含：

- 显示名
- 基础生命上限
- 当前生命
- 基础移动力
- 攻击范围
- 基础攻击力

荒川当前包含：

- 名称
- 成长等级
- 当前能量
- 最大能量

### 4.2 `ProgressionRuntimeState`

管理：

- 主角等级
- 主角经验
- 专精点
- 荒川成长等级
- 已解锁天赋 id
- 已解锁荒川能力 id
- 已解锁卡牌 id
- 天赋分支 tag
- 构筑规则修正项

### 4.3 `DeckBuildState`

管理：

- 当前局外构筑牌组名
- 当前构筑卡牌 id 列表
- 当前构筑遗物 id 列表

### 4.4 `InventoryRuntimeState`

管理：

- 物品计数表

### 4.5 `EquipmentLoadoutState`

管理：

- 武器槽装备 id
- 护甲槽装备 id
- 饰品槽装备 id

这是近期新增的状态分层之一，目的是把装备槽状态从 `GlobalGameSession` 的平铺字段中抽出来。

### 4.6 `SaveRuntimeState`

管理：

- 最近 checkpoint
- 最近手动存档
- 自动存档槽位
- checkpoint 场景和地图上下文

## 5. 当前仍保留的平铺字段

虽然已经开始分层，但当前为了兼容主链和 Godot 导出字段，`GlobalGameSession` 仍保留了一批平铺字段，例如：

- `PlayerMaxHp`
- `PlayerCurrentHp`
- `PlayerAttackDamage`
- `PlayerLevel`
- `PlayerExperience`
- `DeckCardIds`
- `EquippedWeaponItemId`

这些字段的作用主要是：

- 导出到 Godot inspector
- 兼容现有代码
- 作为状态对象同步镜像

不要把这些平铺字段理解成“以后长期就应该这样保留”。  
当前正确理解是：

- 对外仍可用
- 内部已经开始逐步转成“状态对象 + 同步镜像”

## 6. 当前新增的分层服务

`GlobalGameSession` 现在已经不再自己硬扛所有规则，而是开始依赖以下新层：

### 6.1 `EquipmentCatalog`

作用：

- 提供装备定义查询
- 提供按槽位获取装备定义列表

当前仍是 demo fallback 数据，但入口已经独立出来。

### 6.2 `EquipmentService`

作用：

- 判断玩家是否拥有某装备
- 校验装备是否可穿到指定槽位
- 执行穿装备 / 脱装备

### 6.3 `ProgressionRuleSet`

作用：

- 提供经验曲线
- 提供升级所需经验的统一查询

### 6.4 `PlayerStatResolver`

作用：

- 把主角基础状态 + 天赋 + 装备解析为 battle 和局外 UI 真正要读的数值

输出：

- `ResolvedPlayerStats`

也就是说：

- `GlobalGameSession` 现在更接近“状态 + 边界外观”
- 装备规则和成长曲线已经开始往外拆

## 7. 当前最重要的对外方法

### 7.1 battle / map 边界相关

- `BeginBattle(BattleRequest? request = null)`
- `ConsumePendingBattleRequest()`
- `CompleteBattle(BattleResult result)`
- `ConsumeLastBattleResult()`
- `SetPendingMapResumeContext(...)`
- `ConsumePendingMapResumeContext()`
- `SetPendingBattleEncounterId(...)`
- `ConsumePendingBattleEncounterId()`

### 7.2 快照导出相关

- `BuildPlayerSnapshot()`
- `BuildCompanionSnapshot()`
- `BuildProgressionSnapshotModel()`
- `BuildDeckBuildSnapshotModel()`
- `BuildInventorySnapshot()`
- `BuildSaveRuntimeSnapshot()`

### 7.3 状态应用相关

- `ApplyPlayerSnapshot(...)`
- `ApplyCompanionSnapshot(...)`
- `ApplyProgressionSnapshot(...)`
- `ApplyDeckBuildSnapshot(...)`
- `ApplyProgressionDelta(...)`
- `ApplyInventoryDelta(...)`
- `ApplySaveRuntimeSnapshot(...)`

### 7.4 角色解析与装备相关

- `ResolvePlayerStats()`
- `GetResolvedPlayerMaxHp()`
- `GetResolvedPlayerMovePointsPerTurn()`
- `GetResolvedPlayerAttackDamage()`
- `GetResolvedPlayerDefenseDamageReductionPercent()`
- `GetResolvedPlayerDefenseShieldGain()`
- `IsEquipmentOwned(...)`
- `GetEquippedItemId(...)`
- `TryEquipItem(...)`
- `UnequipItem(...)`
- `FindEquipmentDefinition(...)`
- `GetEquipmentDefinitionsForSlot(...)`

### 7.5 经验进度相关

- `GetExperienceRequiredForNextLevel()`
- `GetExperienceProgressWithinLevel()`
- `GetExperienceNeededToLevelUp()`

## 8. 当前 battle 与 map 的衔接方式

当前主链是：

### 8.1 地图进入战斗

地图侧：

1. 准备 `EncounterId`
2. 通过 `MapBattleTransitionHelper` 生成或触发 battle request
3. 写入 `GlobalGameSession`
4. 切换 battle 场景

当前地图层不应该：

- 自己计算装备加成
- 自己计算解析后的玩家攻击
- 自己维护一份 battle 专用玩家状态

### 8.2 战斗开局

battle 侧：

1. `BattleSceneController` 从 `GlobalGameSession` 取 `PendingBattleRequest`
2. request 应用回 session
3. `BattleObjectStateManager` 从 session 读取玩家状态
4. 玩家战斗属性通过 `ResolvePlayerStats()` 获取

### 8.3 战斗结束

1. battle 产出 `BattleResult`
2. `GlobalGameSession.CompleteBattle(...)`
3. result 应用回 session
4. 地图或外部系统消费 result 和 session 状态

## 9. 当前快照语义

这是当前最容易误解、也最重要的部分。

### 9.1 `BuildPlayerSnapshot()` 现在有两层语义

它当前同时输出：

#### 基础字段

- `base_max_hp`
- `base_move_points_per_turn`
- `base_attack_damage`
- `base_defense_damage_reduction_percent`
- `base_defense_shield_gain`

#### 解析字段

- `max_hp`
- `move_points_per_turn`
- `attack_damage`

### 9.2 为什么要分基础字段和解析字段

因为：

- battle 运行时需要吃解析后的数值
- 但 session 在 request / result / save 回写时，不能把这些解析后数值再当作基础值写回

否则会出现：

- 装备加成重复结算
- 天赋加成重复结算
- 攻击 / 生命 / 移动被无限污染

### 9.3 当前规则

- battle 可继续依赖解析字段
- `ApplyPlayerSnapshot()` 回写时优先恢复基础字段
- 只有旧快照缺少基础字段时才回退兼容旧字段

## 10. 当前最容易出问题的点

### 10.1 不要混用 `GlobalGameSession` 和 `GameSession`

当前仓库里两者并存，这是历史遗留。

规则：

- 新 battle / map / 成长功能接 `GlobalGameSession`
- 不要再往 `GameSession` 补主链功能

### 10.2 不要把解析值当基础值缓存

例如：

- 不要把 `attack_damage` 直接当成长后的基础攻击缓存到地图脚本
- 不要自己把解析后的生命上限再写回基础生命字段

### 10.3 不要在 UI 层重算装备和天赋

UI 层应该读：

- `ResolvePlayerStats()`
- `GetEquipmentDefinitionsForSlot(...)`

而不是自己再写一套“装备 + 天赋 = 属性”的逻辑。

### 10.4 merge 时最该看的文件

- `Scripts/Battle/Shared/GlobalGameSession.cs`
- `Scripts/Battle/Boundary/BattleRequest.cs`
- `Scripts/Battle/Boundary/BattleResult.cs`
- `Scripts/Battle/State/BattleObjectStateManager.cs`

尤其检查：

- snapshot 是否仍区分 base 字段和解析字段
- `ApplyPlayerSnapshot()` 是否仍优先写回 base 字段
- battle 是否仍统一通过 `ResolvePlayerStats()` 读值

## 11. 目前还没完全完成的部分

虽然结构已经开始解耦，但以下内容仍是过渡实现：

- `EquipmentCatalog` 当前仍走 demo fallback 数据
- `ProgressionRuleSet` 当前仍走 demo fallback 曲线
- `SystemFeatureLabController` 中仍残留一份旧的本地示范装备定义结构，虽然主链已不再依赖它

这意味着：

- 结构方向已经对了
- 但正式资源化还没彻底完成

## 12. 对队友的直接建议

如果队友后续要接成长、背包、装备、地图：

### 可以直接依赖的

- `GlobalGameSession`
- `BattleRequest`
- `BattleResult`
- `ResolvePlayerStats()`
- `TryEquipItem(...)`
- `ApplyProgressionDelta(...)`
- `ApplyInventoryDelta(...)`

### 不要继续新增的

- 再做一个平行的角色全局状态容器
- 在地图层自己维护“battle 用攻击力”
- 在 UI 里硬编码装备效果
- 把新规则继续直接塞回 `GlobalGameSession`

### 正确扩展方向

- 装备定义继续扩到 `EquipmentCatalog`
- 成长规则继续扩到 `ProgressionRuleSet`
- 属性结算继续扩到 `PlayerStatResolver`
- `GlobalGameSession` 只保留状态和边界外观职责

## 13. 当前一句话结论

`GlobalGameSession` 现在是：

- 当前 battle / map 主链的全局状态真源
- 一个已经开始分层、但仍处于过渡期的边界外观层

理解这一点，后续和队友协作时就不会再把它当成“什么都往里塞的大杂烩”，也不会错误地去绕开它另建一套主链状态。
