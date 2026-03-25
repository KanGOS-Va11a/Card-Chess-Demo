# Step 52 - 修复 Scene1 敌人交互战斗入口链路

## 日期

2026-03-25

## 目标

修复 `Scene1` 中敌人交互后无法正常进入战斗的问题。

## 原因

`Scene1` 当前主线使用的是朋友合并进来的旧原型链：

- `MainPlayer` -> `GameSession`
- `Enemy` -> `GlobalBattleContext`
- `Scene/Battle.tscn`

但 `Scene/Enemy.tscn` 在之前整理过程中被切到了新链脚本 `Scripts/Map/Interaction/Enemy.cs`，该脚本依赖的是：

- `GlobalGameSession`
- 新版地图/战斗边界层

结果就是：

- `Scene1` 的玩家和自动加载器还在旧链
- `Scene1` 的敌人却跑到新链
- 交互时无法走通同一套战斗入口

## 改动

- 修改 `Scene/Enemy.tscn`
- 将敌人脚本恢复为旧链脚本：
  - `res://Scripts/Character/Enemy.cs`
- 显式写回旧链战斗场景路径：
  - `res://Scene/Battle.tscn`

## 结果

`Scene1` 中的敌人、玩家、战斗上下文和占位战斗场景重新回到同一条旧原型链，敌人交互应可再次正常进入战斗。

## 验证建议

- 从 `Scene1` 启动项目
- 靠近敌人并按交互键
- 确认能够切入 `Scene/Battle.tscn`
