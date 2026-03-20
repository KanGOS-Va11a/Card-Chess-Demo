# Step 08 - Room Selection And Board Initialization Pipeline

## Date

2026-03-20

## Goal

实现从战斗入口选择房间、实例化房间、扫描房间 TileMap、生成运行时棋盘状态的初始化链路。

## Implemented

- `BattleSceneController` 现在负责：
  - 根据敌人类型从房间池中筛选房间
  - 随机抽取一个可用房间
  - 实例化房间到 `RoomContainer`
  - 从房间 TileMap 扫描出 `RoomLayoutDefinition`
  - 调用 `BoardInitializer` 初始化运行时 board
  - 将运行时对象同步回 `MarkerLayer`

## Selection Rule

- 优先使用 `ForcedBattleRoomScene`
- 否则从 `BattleRoomScenes` 和 `BattleRoomPools` 中筛选
- 优先选“完全匹配当前敌人类型集合”的房间
- 若没有完全匹配，则回退到“部分匹配”
- 再没有则使用未声明敌人类型的通用房间

## Result

战斗初始化已经从“硬编码布局”切换到了“房间 prefab -> 运行时 layout -> board state”的流程。
