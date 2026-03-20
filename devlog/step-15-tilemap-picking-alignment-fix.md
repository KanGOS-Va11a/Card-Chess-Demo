# Step 15 - TileMap Picking Alignment Fix

## Date

2026-03-20

## Problem

鼠标定位存在固定偏移，点击某个格子时实际落到它左上角的格子。

## Root Cause

之前战场坐标获取使用的是手写的：

- `ToLocal(globalPosition)`
- 再做 `x / CellSize` 与 `y / CellSize`

这在当前房间是 `TileMapLayer` 驱动、并且带有相机缩放与场景层级偏移时，不够稳。

## Fix

- `BattleRoomTemplate.TryScreenToCell(...)` 改为使用 `TileMapLayer.LocalToMap(...)`
- `BattleRoomTemplate.CellToLocalCenter(...)` 改为使用 `TileMapLayer.MapToLocal(...)`
- `BattleRoomTemplate.GetCellRect(...)` 改为基于 TileMap 映射后的中心点构建
- 点击与悬停统一都使用 `GetGlobalMousePosition()` 作为输入坐标源

## Result

战场坐标转换现在以 `TileMapLayer` 自身的 map/local 规则为准，后续继续实现：

- 可达域显示
- 路径预览
- 单位选择
- 正式移动命令

都可以直接复用这套坐标基线。
