# 卡牌系统与局外构筑 README

## 1. 当前卡牌链路回顾

当前 battle 卡牌链路分为三层：

### 1.1 运行时定义

核心运行时类型：

- `Scripts/Battle/Cards/BattleCardDefinition.cs`
- `Scripts/Battle/Cards/BattleCardInstance.cs`
- `Scripts/Battle/Cards/BattleDeckState.cs`
- `Scripts/Battle/Cards/BattleDeckRuntimeInit.cs`

职责：

- `BattleCardDefinition`
  - 描述一张卡在 battle 内真正结算用的参数
- `BattleCardInstance`
  - 一张进入牌堆后的实例
- `BattleDeckState`
  - 管理抽牌堆 / 手牌 / 弃牌堆 / 消耗堆
- `BattleDeckRuntimeInit`
  - 控制 battle 开局时各牌堆如何初始化

### 1.2 数据模板层

新增的资源模板：

- `Scripts/Battle/Cards/BattleCardTemplate.cs`
- `Scripts/Battle/Cards/BattleCardLibrary.cs`
- `Scripts/Battle/Cards/BattleDeckBuildRules.cs`
- `Scripts/Battle/Cards/BattleDeckValidationResult.cs`
- `Scripts/Battle/Cards/BattleDeckConstructionService.cs`

职责：

- `BattleCardTemplate`
  - 编辑器里配置卡牌参数和文本
- `BattleCardLibrary`
  - 管理全局卡牌模板列表
- `BattleDeckBuildRules`
  - 定义最少 / 最多卡数、点数预算、同名卡上限
- `BattleDeckConstructionService`
  - 校验局外构筑是否合法，并生成可带入 battle 的运行时定义

### 1.3 战斗控制层

关键文件：

- `Scripts/Battle/BattleSceneController.cs`

当前结算逻辑：

1. 从 deck build snapshot 解析卡牌 id
2. 解析开局牌堆初始化参数
3. `BattleDeckState` 构建运行时牌堆
4. 玩家在 HUD 中出牌
5. `TryPlayCard(...)` 根据卡牌参数执行：
   - 伤害
   - 治疗
   - 抽牌
   - 回能
   - 护盾

## 2. 目前卡牌参数定义

`BattleCardTemplate` / `BattleCardDefinition` 当前支持的主要参数：

- `CardId`
- `DisplayName`
- `Description`
- `Category`
- `TargetingMode`
- `Cost`
- `Range`
- `Damage`
- `HealingAmount`
- `DrawCount`
- `EnergyGain`
- `ShieldGain`
- `IsQuick`
- `ExhaustsOnPlay`

局外构筑相关参数：

- `BuildPoints`
- `MaxCopiesInDeck`
- `UnlockedByDefault`
- `RequiredPlayerLevel`
- `RequiredTalentIds`
- `RequiredBranchTags`

## 3. 现在如何新增一张“普通参数卡”

如果只是新增一张现有功能范围内的卡，比如：

- 造成伤害
- 抽牌
- 回能
- 加护盾
- 治疗

那后续应该尽量只改模板参数，不改控制器逻辑。

推荐流程：

1. 在编辑器里创建一个 `BattleCardTemplate` 资源
2. 填写：
   - `CardId`
   - `DisplayName`
   - `Description`
   - 费用、范围、伤害、治疗等参数
3. 把这个模板加入 `BattleCardLibrary`
4. 把卡牌 id 加入可选牌库 / 构筑系统
5. 如果有卡图，放到：
   - `Assets/Cards/{card_id}.png`
   或
   - `Assets/Battle/Cards/{card_id}.png`

### 3.1 示例：新增一张护盾卡

例如：

- `CardId = steady_guard`
- `DisplayName = 稳守`
- `Description = 得 4 盾`
- `Category = Skill`
- `TargetingMode = None`
- `Cost = 1`
- `ShieldGain = 4`
- 其余数值留 0

这种卡不需要改代码。

### 3.2 示例：新增一张治疗卡

例如：

- `CardId = field_patch`
- `DisplayName = 现场包扎`
- `Description = 2 格内友方回复 3 点生命`
- `Category = Skill`
- `TargetingMode = FriendlyUnit`
- `Cost = 1`
- `Range = 2`
- `HealingAmount = 3`

这种卡在当前版本也不需要改控制器逻辑，因为：

- `FriendlyUnit` 已经接入目标选择
- `HealingAmount` 已经接入 `BattleActionService.ApplyHealingToTarget(...)`

## 4. 现在如何新增“新类型”的卡

如果新卡仍属于这几类参数组合：

- 伤害
- 治疗
- 抽牌
- 回能
- 护盾

只要当前参数够表达，就不该再写新逻辑。

只有在出现下面这种情况时，才需要扩展代码：

- 自定义形状范围
- 位移
- 召唤
- 多段效果
- 持续 buff / debuff
- 延迟触发
- 依赖地形或房间标签

### 4.1 例子：实现“自定义范围的治疗”

当前“范围治疗”已属于现有系统支持的类型。

做法不是新写一个 `HealCardController`，而是：

- 新建卡牌模板
- `TargetingMode = FriendlyUnit`
- `Range = 你想要的距离`
- `HealingAmount = 你想要的治疗量`

如果你以后要做“直线治疗”“十字治疗”“范围群疗”，那才需要继续扩展：

- `BattleCardTargetingMode`
- `BattleSceneController` 的目标高亮与目标解析逻辑

## 5. 当前已接入的目标类型

`BattleCardTargetingMode` 当前包括：

- `None`
  - 不需要指定目标
- `EnemyUnit`
  - 指定范围内敌方单位
- `StraightLineEnemy`
  - 直线首个敌人
- `FriendlyUnit`
  - 指定范围内友方单位，包含自己

## 6. 局外卡牌构筑系统怎么掌控

局外构筑当前推荐通过下面三层掌控：

### 6.1 可选牌库

由：

- `BattleCardLibrary`
- `ProgressionSnapshot`
- `BattleDeckConstructionService.GetAvailableCardPool(...)`

共同决定。

也就是说：

- 卡牌模板定义“这张卡是什么”
- 成长快照定义“玩家目前解锁了什么”
- 构筑服务决定“当前可选牌库是什么”

### 6.2 实际携带牌组

由：

- `DeckBuildSnapshot`

表达。

当前最重要字段：

- `BuildName`
- `CardIds`
- `RelicIds`

其中：

- `CardIds` 就是这次带进 battle 的实际牌组列表

### 6.3 规则约束

由：

- `BattleDeckBuildRules`
- `BattleDeckConstructionService.ValidateDeck(...)`

共同控制。

当前已经能做的约束包括：

- 最少携带牌数
- 最多携带牌数
- 点数预算
- 同名卡数量上限
- 某张卡自己的最大可携带张数
- 玩家是否已经解锁该卡

## 7. 如何与成长系统挂钩

当前推荐通过 `ProgressionSnapshot` 里的这些字段挂钩：

- `PlayerLevel`
- `PlayerExperience`
- `PlayerMasteryPoints`
- `TalentIds`
- `ArakawaUnlockIds`
- `UnlockedCardIds`
- `TalentBranchTags`
- `DeckPointBudgetBonus`
- `DeckMinCardCountDelta`
- `DeckMaxCardCountDelta`
- `DeckMaxCopiesPerCardBonus`

## 7.1 解锁牌库

推荐两种方式并存：

### 方式 A：显式解锁

成长系统直接把某张卡的 id 写入：

- `UnlockedCardIds`

适合：

- 剧情奖励
- 固定天赋节点奖励
- 商店购买

### 方式 B：条件解锁

卡牌模板本身写上：

- `RequiredPlayerLevel`
- `RequiredTalentIds`
- `RequiredBranchTags`

适合：

- 分支玩法
- 职业 / 流派限制

## 7.2 点数预算系统

推荐做法：

- 每张卡模板有 `BuildPoints`
- 构筑规则定义 `BasePointBudget`
- 成长系统通过：
  - `DeckPointBudgetBonus`
  影响总预算

例如：

- 基础预算 `18`
- 某天赋 `+2`
- 实际预算 `20`

这样就能限制“只带几张超强卡”的问题。

## 7.3 最少 / 最多卡数限制

推荐做法：

- `BattleDeckBuildRules.MinDeckSize`
- `BattleDeckBuildRules.MaxDeckSize`

再加成长修正：

- `DeckMinCardCountDelta`
- `DeckMaxCardCountDelta`

例如：

- 基础最少 `8` 张
- 基础最多 `18` 张
- 某天赋让最大上限 `+2`

## 7.4 同名卡上限

推荐双层限制：

1. 全局规则：
   - `BaseMaxCopiesPerCard`
2. 卡牌模板自己的局部规则：
   - `MaxCopiesInDeck`

最终取两者中更严格的一层。

这样可以防止：

- 某张极强卡被无限堆叠
- 少牌 high roll 导致你不希望的无限循环

## 8. 推荐的成长分支挂钩方式

你提到以后可能有：

- 近战
- 远程
- 灵活

这非常适合通过：

- `TalentBranchTags`
- `RequiredBranchTags`

来挂钩。

例如：

- 某张近战牌要求 `RequiredBranchTags = ["melee"]`
- 某张远程牌要求 `RequiredBranchTags = ["ranged"]`
- 某张灵活牌要求 `RequiredBranchTags = ["flex"]`

这样天赋树和卡牌解锁天然能接起来。

## 9. 当前剩余限制

虽然框架已具备，但当前还没做完这些：

- battle 外构筑 UI
- 正式卡牌资源库 `.tres` 批量落地
- 更复杂的卡牌效果类型
- 构筑保存 / 多套牌切换
- 构筑与奖励系统自动联动

## 10. 结论

从现在开始，如果你要新增一张普通卡：

- 优先新建 `BattleCardTemplate`
- 改参数
- 填文本
- 加入 `BattleCardLibrary`

只有当卡牌效果已经超出现有参数表达能力时，才值得继续扩代码。
