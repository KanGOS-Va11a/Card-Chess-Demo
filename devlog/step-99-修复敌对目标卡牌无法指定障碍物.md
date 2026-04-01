# Step 99 - 修复敌对目标卡牌无法指定障碍物

## 日期

2026-04-01

## 目标

修复当前 battle 中“人物卡牌技能打不到障碍物”的问题。

正确规则应为：

- 可破坏障碍物应当和敌对单位一样，属于敌对目标卡牌的合法目标
- 只要该障碍物满足当前攻击 / 直线卡牌的目标条件，就应允许选中并结算

## 问题原因

本次定位到有两层拦截：

### 1. 卡牌目标解析层只认敌方单位

`BattleSceneController.GetCardTargetAtCell(...)` 中：

- `EnemyUnit`
- `StraightLineEnemy`

都先经过了 `GetEnemyUnitAtCell(...)`

这一步把障碍物直接过滤掉了。

### 2. 卡牌目标校验层写死了“只有单位能被卡牌指定”

`BattleSceneController.TryResolveCardTarget(...)` 中存在旧逻辑：

- `targetObject.ObjectType != BoardObjectType.Unit`
  - 直接判定为非法卡牌目标

这会让可破坏障碍物即使在逻辑上本应可攻击，也无法作为卡牌目标通过校验。

### 3. 直线目标服务只找敌方单位

`BoardTargetingService.TryFindFirstEnemyInDirection(...)` 里：

- 单位会被当作候选目标
- 障碍物只会作为挡视线物处理

所以直线敌对卡也打不到可破坏障碍。

## 本次改动

### 1. 放开敌对目标卡的格子目标解析

更新：

- `Scripts/Battle/BattleSceneController.cs`

处理：

- `BattleCardTargetingMode.EnemyUnit`
  - 改为从 `GetAttackableObjectAtCell(...)` 取目标
- `BattleCardTargetingMode.StraightLineEnemy`
  - 也改为从 `GetAttackableObjectAtCell(...)` 取目标

这样可破坏障碍物不再在第一层就被过滤掉。

### 2. 放开卡牌目标校验中的“单位限定”

更新：

- `Scripts/Battle/BattleSceneController.cs`

处理：

- 删除“只有单位能被卡牌指定”的统一硬限制
- 对于：
  - `EnemyUnit`
  - `StraightLineEnemy`
  现在改为依赖：
  - `BattleActionService.IsAttackable(attacker, targetObject)`

这意味着：

- 敌对单位可指定
- 可破坏障碍物也可指定
- 非法目标仍会被挡住

### 3. 直线目标服务允许命中可破坏障碍物

更新：

- `Scripts/Battle/Board/BoardTargetingService.cs`

处理：

- 在沿直线扫描目标时：
  - 如果遇到 `destructible` obstacle
  - 现在会把它作为目标返回

而不是像之前那样只把它当作挡线物直接拦掉。

## 当前结果

现在敌对目标卡牌的规则已经变成：

- 可攻击敌人单位
- 可攻击可破坏障碍物
- 友方单位类卡牌仍只允许指定友方单位

也就是说：

- 普通攻击可以打障碍物
- 敌对卡牌技能现在也可以打障碍物

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍保留项目历史 nullable warnings，本次未处理
