# step-193 门、固定敌人与剧情触发区接入格子 authoring

日期：2026-04-23

本次改动：

- 为 `Door02.tscn`、`Door03.tscn` 根节点接入 `GridInteractableAnchor`，使门实例支持：
  - `UseGridPlacement`
  - `Cell`
  - `InteractionId`
- 将当前主流程地图中的门实例切到格子 authoring：
  - `Scene01`
  - `Scene03`
  - `Scene04`
  - `Scene05`
  - `Scene06`
- 将固定敌人/教程敌人切到格子锚点 authoring：
  - `Scene03` 首个教程敌人
  - `Scene04` 逃跑教程敌人
  - `Scene04` 荒川教程敌人
  - `Scene08` 固定敌人
- 固定敌人进入锚点容器后，修正了对应剧情触发器的 `TriggerInteractablePath` 和 `editable path`。
- 将仍在使用旧 `position/scale/TriggerSize` 的剧情触发区改为格子区域触发：
  - `Scene05` 到达剧情
  - `Scene06` Boss 战前剧情
  - `Scene04To05Cutscene` 登船过场
- 统一清理格子触发器上的旧缩放干扰，避免运行时和编辑器里继续依赖手摆缩放。
- `StoryTriggerZone.cs` 新增格子偏移列表能力：
  - 在矩形区域之外，可额外通过 `TriggerCellOffsets` 定义非规则格子触发区
  - 编辑器下使用格子绘制预览，而不是继续依赖旧矩形碰撞感知
- `SystemFeatureOverlayAdapter.cs` 改为读取玩家当前交互提示文本，不再依赖 `InteractionArea.GetOverlappingAreas()`。

结果：

- 主流程地图里的门、固定敌人和剧情触发区，已经基本进入“显式格子坐标 authoring”链路。
- 仍残留的旧 `Area2D` 读取主要集中在 `SystemFeatureLabController.cs` 这类调试/UI 实验层，不影响当前主流程格子交互。
