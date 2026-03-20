# Step 14 - Room TileSet Preservation And Hover Fix

## Date

2026-03-20

## Problem

测试中出现了三个直接症状：

- 载入 `GruntDebugRoomA` 后地板 tile 不显示
- 移动范围和路径预览不显示
- 鼠标悬停高亮边框有缺边

## Root Cause

### 1. Room TileSet 被运行时覆盖

房间场景本身已经保存了 `TileSet + tile_map_data`，但 `BattleRoomTemplate` 在进入场景时仍然强制重建并覆盖 TileSet，导致：

- 手工绘制的 atlas tile 坐标失配
- marker 层中 scene tile 的 ID 失配
- 房间能画出来的内容和运行时代码解析到的内容不再一致

### 2. Marker Tile ID 硬编码错误

代码之前假设 marker 的 tile id 是 `0/1/2`，而场景里保存的 scene tile 实际是 `1/2/3`。

这导致：

- 玩家出生点无法被正确解析
- 运行时没有生成 player object
- 后续可达域和路径预览自然也都没有显示

### 3. Hover 描边落在缩放边界上

高亮边框原先直接沿格子外框绘制，在当前 Camera2D 缩放下容易出现 1px 描边被裁掉的视觉现象。

## Fix

- `BattleRoomTemplate` 现在只有在房间没有 TileSet 时才自动创建测试 TileSet
- 已保存的房间 TileSet 不会再被运行时覆盖
- marker 相关 ID 改为显式导出配置：
  - `MarkerSourceId`
  - `PlayerMarkerTileId`
  - `EnemyMarkerTileId`
  - `ObstacleMarkerTileId`
- 当前默认值已对齐现有房间：
  - source = `1`
  - player = `1`
  - enemy = `2`
  - obstacle = `3`
- hover 描边改为向内收 1px 后再绘制，避免缺边

## Result

- TileMap 房间会优先使用你在场景中保存好的 TileSet 和绘制结果
- 玩家/敌人/障碍物 marker 能再次正确解析
- 路径和可达域显示链路恢复
- 高亮边框在当前测试镜头下更稳定
