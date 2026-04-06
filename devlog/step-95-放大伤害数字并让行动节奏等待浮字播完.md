# Step 95 - 放大伤害数字并让行动节奏等待浮字播完

## 日期

2026-03-31

## 目标

优化 battle 中的数值反馈节奏，让玩家更容易看清每一步行动结果：

- 伤害数字更大
- 停留更久
- 护盾伤害与生命伤害之间拉开一点间隔
- 数字没有基本播完前，不要马上进入下一位角色行动

## 本次改动

### 1. 调整浮字默认参数

更新：

- `Scripts/Battle/Visual/BattleFloatingTextLayer.cs`

主要调整：

- `FontSize`
  - `16 -> 22`
- `LifetimeSeconds`
  - `0.65 -> 1.05`
- `RiseDistancePixels`
  - `18 -> 24`
- `HorizontalSpreadPixels`
  - `11 -> 13`
- `ImpactStaggerSeconds`
  - `0.045 -> 0.12`
- `OutlineSize`
  - `2 -> 3`

结果：

- 数字更大、更明显
- 停留时间更长
- 上升与淡出更慢，更适合观察行动结果

### 2. 增加“不同类型 impact 之间的额外间隔”

更新：

- `Scripts/Battle/Visual/BattleFloatingTextLayer.cs`

新增参数：

- `ImpactTypeChangeExtraStaggerSeconds = 0.06`
- `SequenceEndPaddingSeconds = 0.08`

效果：

- 先掉护盾再掉生命时，两组数字之间会更清楚地错开
- 不会再几乎重叠成一团

### 3. 浮字层可返回“这一组 impact 需要播放多久”

更新：

- `Scripts/Battle/Visual/BattleFloatingTextLayer.cs`

新增方法：

- `GetImpactSequenceDurationSeconds(IReadOnlyList<CombatImpact> impacts)`

意义：

- 行动流程不再只依赖写死的 `ImpactPresentationDurationSeconds`
- 可以改成按实际浮字数量和错峰间隔来等待

### 4. ActionService 记录最近一次 impact 展示时长

更新：

- `Scripts/Battle/Actions/BattleActionService.cs`

新增：

- `LastImpactPresentationDurationSeconds`

并在：

- `ApplyDamageToTarget(...)`
- `OnNonDamageImpactsApplied(...)`

中根据浮字层实际序列时长更新它。

### 5. 攻击 / 防御异步动作开始等待真实浮字时长

更新：

- `Scripts/Battle/Actions/BattleActionService.cs`

处理：

- `TryAttackObjectAsync(...)`
- `ApplyDefenseActionAsync(...)`

现在不再只等固定的动作常量时间，而是会取：

- 动作最小表现时间
- 与最近一次浮字序列时间

中的较大值

### 6. 敌人回合推进等待浮字播完

更新：

- `Scripts/Battle/AI/EnemyTurnResolver.cs`

处理：

- 每个敌人行动后，等待时间从固定 `PostActionDelaySeconds`
- 改为：
  - `max(PostActionDelaySeconds, actionService.LastImpactPresentationDurationSeconds)`

这样敌人 A 打完产生浮字后，不会立刻切到敌人 B 行动。

### 7. 玩家行动结束后进入敌方回合前也等待浮字播完

更新：

- `Scripts/Battle/BattleSceneController.cs`

处理：

- `ResolveTurnPostPhase()` 中，玩家行动后的缓冲时间从固定值改为：
  - `max(PlayerActionResolveBufferSeconds, actionService.LastImpactPresentationDurationSeconds)`

这样玩家普通攻击、出伤害牌、加盾、治疗后，battle 不会在浮字刚冒出来时就立刻把回合推进掉。

## 结果

现在 battle 中的数字反馈更接近“先看懂结果，再进入下一步”的节奏：

- 数字更大
- 数字更耐看
- 护盾与生命的分段更清楚
- 玩家和敌人的行动推进都更愿意等浮字播完

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍保留项目历史 nullable warnings，本次未处理
