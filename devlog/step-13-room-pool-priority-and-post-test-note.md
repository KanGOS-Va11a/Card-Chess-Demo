# Step 13 - Room Pool Priority And Post-Test Note

## Date

2026-03-20

## Goal

修正 battle 房间来源优先级，避免测试阶段同时使用“直接场景列表”和“房间池”导致抽房间结果不符合预期。

## Change

- `BattleSceneController.ExpandRoomScenePool()` 已改为：
  - 若 `BattleRoomPools` 中存在可用房间，则只使用房间池中的场景
  - 只有在房间池为空时，才回退到 `BattleRoomScenes`

## Result

- 现在如果你在 `BattleRoomPool` 里只保留 `GruntDebugRoomA`，运行时就不会再从 `BattleRoomScenes` 混入 `B`
- `BattleRoomScenes` 现在只作为无房间池时的测试回退入口

## Follow-up Note

- 当前约定保留：测试阶段结束后，继续对 battle 域代码做一轮结构完善与清理
- 重点会包括：
  - 房间扫描与运行时对象同步的职责边界
  - TileMap 标记层与运行时表现层解耦
  - 正式路径搜索 / 可达域 / 选中状态系统
  - BattleRequest 驱动的正式初始化链路
