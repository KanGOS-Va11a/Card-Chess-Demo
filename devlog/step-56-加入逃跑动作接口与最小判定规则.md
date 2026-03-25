# Step 56 - 加入逃跑动作接口与最小判定规则

## 日期

2026-03-25

## 目标

把“逃跑”从一个接口设想升级为 battle 主链中的正式动作入口，并落地最小可运行判定。

## 本次改动

### HUD 与状态机

- `BattleHudController` 新增：
  - `RetreatRequested` 信号
  - `Run` 按钮入口
- `TurnActionState` 新增：
  - `CanRetreat`

### 战斗控制器

- `BattleSceneController` 新增逃跑动作处理：
  - 点击后原地结束本回合
  - 记录逃跑时玩家 `HP`
  - 敌方回合结束后检查玩家 `HP` 是否下降
  - 若未下降，则以 `BattleOutcome.Retreat` 结束战斗

### 战斗结果

- `BattleResult` 新增：
  - `DidPlayerRetreat`
- 当结果为 `Retreat` 时，会在 `RuntimeFlags` 中写入：
  - `retreat_succeeded = true`

### 文档

同步更新：

- `Docs/战斗对外交互接口方案.md`
- `Docs/项目接口文档.md`
- `Docs/战斗机制规则书.md`

## 当前规则

- 逃跑与 `Atk` / `Def` / `Meditate` 同级
- 点击 `Run` 后立即结束本回合
- 若该回合内玩家只损失护盾、不损失 `HP`，则逃跑成功
- 若玩家 `HP` 下降，则逃跑失败，战斗继续
- 逃跑成功默认不视为胜利，不自动清 encounter，不按胜利流程结算奖励

## 风险与注意

- 地图系统未来如果需要“逃跑后敌人是否保留、是否追击、是否触发额外代价”，应在 battle 外部根据 `Outcome = Retreat` 再做处理
- 不要把 `Retreat` 当作 `Victory`
