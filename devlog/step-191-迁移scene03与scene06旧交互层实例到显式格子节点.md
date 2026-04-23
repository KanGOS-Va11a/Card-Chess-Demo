# step-191 迁移 Scene03 与 Scene06 旧交互层实例到显式格子节点

日期：2026-04-23

本次改动：

- 将 `Scene03.tscn` 中旧 `InteractionLayer` 残留的 8 个交互体迁移为 `WorldObjects/GridInteractables` 下的显式实例节点：
  - `Cabinet01` x2
  - `Cabinet02` x1
  - `Chest` x4
  - `HealingStation` x1
- 将 `Scene06.tscn` 中旧 `InteractionLayer` 残留的 4 个交互体迁移为 `WorldObjects/GridInteractables` 下的显式实例节点：
  - `Cabinet01` x3
  - `HealingStation` x1
- 为所有迁移后的交互体补充稳定的 `InteractionId`，接入新的格子交互与运行时状态链路。
- 将两张场景里的旧 `InteractionLayer.tile_map_data` 清空，避免旧 scene-tile 交互与新显式节点重复生效。

说明：

- 本次仅处理地图交互物的格子化迁移，不涉及战斗房间的障碍物/地形 tileset。
- 旧 `InteractionLayer` 的场景 tile 数据通过交互 tileset 映射和现有已迁样例进行了保守还原，优先保证对象类型、格子坐标和状态键稳定。
