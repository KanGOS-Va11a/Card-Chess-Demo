# Step 61 - 补齐卡牌模板与局外构筑规则骨架

## 日期

2026-03-26

## 目标

把当前卡牌系统从“battle controller 内置原型牌组”推进到“模板资源 + 局外构筑规则骨架”的阶段，并补一份可操作的 README。

## 本次改动

### 1. 卡牌运行时能力扩展

扩展：

- `Scripts/Battle/Cards/BattleCardDefinition.cs`
- `Scripts/Battle/Cards/BattleCardEnhancementDefinition.cs`

新增能力：

- `HealingAmount`
- `FriendlyUnit` 目标类型
- `HealingDelta`

### 2. 数据驱动模板层

新增：

- `Scripts/Battle/Cards/BattleCardTemplate.cs`
- `Scripts/Battle/Cards/BattleCardLibrary.cs`
- `Scripts/Battle/Cards/BattleDeckBuildRules.cs`
- `Scripts/Battle/Cards/BattleDeckValidationResult.cs`
- `Scripts/Battle/Cards/BattleDeckConstructionService.cs`

### 3. 局外构筑与成长挂钩

扩展：

- `Scripts/Battle/Boundary/ProgressionSnapshot.cs`
- `Scripts/Battle/Shared/ProgressionRuntimeState.cs`
- `Scripts/Battle/Shared/GlobalGameSession.cs`
- `Scripts/Battle/Boundary/DeckBuildSnapshot.cs`

新增用于构筑约束的字段：

- `UnlockedCardIds`
- `TalentBranchTags`
- `DeckPointBudgetBonus`
- `DeckMinCardCountDelta`
- `DeckMaxCardCountDelta`
- `DeckMaxCopiesPerCardBonus`

### 4. Battle 控制器接线

扩展：

- `Scripts/Battle/BattleSceneController.cs`
- `Scripts/Battle/UI/BattleCardView.cs`

当前 battle 已支持：

- `FriendlyUnit` 高亮与目标选择
- `HealingAmount` 结算
- 优先使用 `BattleCardLibrary` 构建运行时卡牌目录
- 若未配置卡牌库，则回退到内置原型牌组

### 5. README

新增：

- `Docs/卡牌系统与局外构筑README.md`

内容包括：

- 当前卡牌链路回顾
- 如何通过模板新增卡牌
- 如何做范围治疗卡
- 如何通过成长系统扩大可选牌库
- 如何通过点数预算、牌数上下限、同名卡限制约束构筑
- 如何通过天赋分支与卡牌解锁挂钩

## 当前结果

卡牌系统现在已经具备：

1. battle 运行时定义层
2. 模板资源层
3. 构筑规则层
4. 成长挂钩字段层
5. README 指导层

## 当前限制

- 还没有 battle 外构筑 UI
- 还没有默认卡牌库资源 `.tres` 正式落地一整套牌表
- `BattleDeckConstructionService` 已能校验，但尚未接成完整编辑流程

## 验证

- 已执行 `dotnet build`
- 构建通过，`0` 错误
