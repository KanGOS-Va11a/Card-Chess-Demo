# Step 09 - Board Visualization, Coordinate Mapping, And Hover Overlay

## Date

2026-03-20

## Goal

把战场可视化从调试画线升级到可交互的 TileMap 棋盘，并补齐从视图坐标到网格坐标的转换。

## Implemented

- 新增 `BoardTopology`
  - 负责棋盘尺寸和 cell 尺寸定义
  - 提供 `TryLocalToCell` / `CellToLocalCenter` / `GetCellRect`
- `BattleRoomTemplate` 提供：
  - `TryScreenToCell(...)`
  - `CellToLocalCenter(...)`
  - `GetCellRect(...)`
- 新增 `BattleBoardOverlay`
  - 鼠标悬停格高亮
  - 可达范围预留显示
  - 路径预览预留显示

## Test Behavior

- 鼠标移动到棋盘格上时会高亮当前格
- 以玩家为起点，会显示调试用的可达范围和预览路径
- 左键点击目标格时，玩家会尝试移动到该格，方便验证坐标和视觉同步
