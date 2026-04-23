# step-196 为地图格子交互物与剧情触发区补编辑器实时坐标显示

日期：2026-04-23

本次改动：

- 新增 `MapEditorDrawHelper.cs`，统一负责在 2D 编辑器中绘制格子坐标标签。
- `GridPlacedNode2D` 新增编辑器实时坐标标签能力：
  - `ShowEditorCellLabel`
  - `EditorLabelOffset`
  - `EditorLabelFontSize`
  - `EditorLabelColor`
- `GridPlacedNode2D` 在编辑器 `_Process` 中增加 `QueueRedraw()`，修改 `Cell` 后会实时重绘。
- `GridInteractableNode2D` 的编辑器标签会额外显示 `InteractionId`。
- `InteractableContainerConfig` 标记为 `[Tool]`，使 `Chest / Cabinet` 这类交互物在编辑器里修改 `Cell` 时可以实时移动和显示坐标。
- `StoryTriggerZone` 标记为 `[Tool]`，并在编辑器中实时显示：
  - `OriginCell`
  - `TriggerSizeCells` 或 `TriggerCellOffsets` 数量
  - 覆盖格子的可视矩形

结果：

- 现在在编辑器里修改 `Cell / OriginCell / TriggerSizeCells / TriggerCellOffsets` 时：
  - 交互物位置会实时更新
  - 剧情触发范围会实时重绘
  - 对应的格子坐标会直接显示在场景视图里
