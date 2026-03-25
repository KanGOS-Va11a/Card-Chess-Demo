# Step 57 - 修复移动终点重复显示可达范围

## 日期

2026-03-25

## 问题

在 battle 中改成慢速逐格移动后，单位到达路径终点时会短暂再次显示一次移动范围。

## 原因

- 逻辑坐标在移动开始时就已经更新到目标格
- 但 `TurnActionState.MarkMoved()` 要等移动补间结束后才执行
- 这导致补间期间 `_Process()` 仍把玩家视为“还能移动”，于是按终点格又刷新了一次可达范围

## 修复

- 修改 `Scripts/Battle/BattleSceneController.cs`
- 在 `_isPlayerMoveResolving == true` 时，直接清空：
  - reachable overlay
  - attack overlay
  - support overlay
  - preview path

## 结果

- 玩家移动补间期间不会再在路径终点闪出一次新的移动范围
- overlay 行为和动作状态现在保持一致
