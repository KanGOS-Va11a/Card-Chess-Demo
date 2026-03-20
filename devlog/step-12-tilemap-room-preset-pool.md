# Step 12 - TileMap Room Preset Pool

## Date

2026-03-20

## Goal

把 battleRoom 工作流固定为“TileMap 房间预设 -> 保存为场景 -> 加入随机房间池资源”的流程。

## Implemented

- 新增房间池资源：`Resources/Battle/Rooms/DebugBattleRoomPool.tres`
- 该资源使用 `BattleRoomPoolDefinition` / `BattleRoomPoolEntry`
- 当前把两张测试房间都挂在 `grunt` 敌人类型下：
  - `Scene/Battle/Rooms/GruntDebugRoomA.tscn`
  - `Scene/Battle/Rooms/GruntDebugRoomB.tscn`
- `Scene/Battle/Battle.tscn` 已接入该房间池资源

## Result

- 现在房间本身就是可保存的预制件
- 你可以直接复制一个 battleRoom 场景，修改 FloorLayer 和 MarkerLayer 的绘制内容，然后把它加入房间池资源
- 战斗初始化会根据敌人类型从对应房间池中抽取一个房间使用
