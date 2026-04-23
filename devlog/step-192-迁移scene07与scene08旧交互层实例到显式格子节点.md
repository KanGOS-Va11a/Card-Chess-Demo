# step-192 迁移 Scene07 与 Scene08 旧交互层实例到显式格子节点

日期：2026-04-23

本次改动：

- 将 `Scene07.tscn` 中旧 `InteractionLayer` 残留的 6 个交互体迁移为 `WorldObjects/GridInteractables` 下的显式实例节点：
  - `Cabinet01` x4
  - `Chest03` x2
- 将 `Scene08.tscn` 中旧 `InteractionLayer` 残留的 3 个交互体迁移为 `WorldObjects/GridInteractables` 下的显式实例节点：
  - `Cabinet01` x3
- 为所有迁移后的交互体补充稳定的 `InteractionId`，接入新的格子交互与运行时状态链路。
- 将两张场景里的旧 `InteractionLayer.tile_map_data` 清空，避免旧 scene-tile 交互与新显式节点重复生效。

说明：

- 本次是地图格子交互重构的收尾批次，目标是把迷宫场景中仍残留的旧交互层实例彻底移除。
