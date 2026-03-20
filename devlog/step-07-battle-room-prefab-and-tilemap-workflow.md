# Step 07 - Battle Room Prefab And TileMap Workflow

## Date

2026-03-20

## Goal

把战斗房间改成可设计的 `battleRoom` 场景预制件，而不是继续使用纯代码调试格子。

## Implemented

- 新增 `BattleRoomTemplate`，作为房间预制件基类
- 房间内部统一使用两个 `TileMapLayer`
  - `FloorLayer`：绘制 16x8 的 16x16 地板格
  - `MarkerLayer`：放置玩家、敌人、障碍物标记
- 新增两张测试房间：
  - `Scene/Battle/Rooms/GruntDebugRoomA.tscn`
  - `Scene/Battle/Rooms/GruntDebugRoomB.tscn`

## Notes

- 房间脚本会自动为 TileMap 构建可编辑 TileSet
- 当前 TileSet 包含：
  - 1 个地板 atlas tile
  - 1 个玩家 scene tile
  - 1 个敌人 scene tile
  - 1 个障碍物 scene tile
- 若房间为空，会自动填充一版测试地板和测试标记，保证运行时可立即验证
