# Step 97 - 把荒川造墙正式改为可破坏障碍物

## 日期

2026-03-31

## 目标

避免荒川造墙在 battle 中形成永久堵死敌人的不可解局面。

要求：

- 造出来的障碍物仍然挡路
- 但必须能被敌人和玩家攻击拆掉

## 本次改动

### 1. 荒川能力调用切到新名字

更新：

- `Scripts/Battle/BattleSceneController.cs`

处理：

- 荒川造墙能力现在调用：
  - `TryCreateArakawaBarrierAsync(targetCell)`

而不是继续直接调用旧语义的：

- `TryCreateIndestructibleObstacleAsync(...)`

### 2. 实际生成对象改为可破坏 barrier

更新：

- `Scripts/Battle/Actions/BattleActionService.cs`

新增正式入口：

- `TryCreateArakawaBarrier(...)`
- `TryCreateArakawaBarrierAsync(...)`

生成内容现在为：

- `DefinitionId = "battle_obstacle_wall"`
- `Tags = ["obstacle", "destructible", "arakawa_construct"]`
- `MaxHp = 3`
- `CurrentHp = 3`
- `BlocksMovement = true`
- `BlocksLineOfSight = true`

这意味着它仍然是挡路的墙，但已经不再是不可破坏物。

### 3. 旧接口名保留兼容壳

同文件中仍保留：

- `TryCreateIndestructibleObstacle(...)`
- `TryCreateIndestructibleObstacleAsync(...)`

但它们内部现在只会转调新 barrier 实现。

这样做的原因是：

- 避免旧调用点因为方法重命名直接失效
- 但保证即使走旧名，也不会再生成真正不可破坏的障碍

### 4. 敌人当前确实能把它当作攻击目标

确认点：

- `BattleActionService.IsAttackable(...)`
  - 对 obstacle 的判定是 `HasTag("destructible")`
- `MeleeBasicEnemyAiStrategy`
  - 使用 `FindAttackableTargetsInRange(...)` 选择攻击目标

因此：

- 荒川 barrier 现在不只是“标签上可破坏”
- 也已经符合敌人当前 AI 的可攻击对象规则

## 结果

现在荒川造墙的玩法语义变成：

- 可以短时间改造地形、阻断路线
- 但不能无限堆叠成永久封锁
- 敌人如果被挡住，理论上可以选择攻击障碍物推进

## 验证

- `dotnet build`
  - 结果：`0 warnings`
  - `0 errors`
