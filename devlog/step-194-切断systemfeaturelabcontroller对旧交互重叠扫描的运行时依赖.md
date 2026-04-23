# step-194 切断 SystemFeatureLabController 对旧交互重叠扫描的运行时依赖

日期：2026-04-23

本次改动：

- 恢复了 `Scripts/Map/UI/SystemFeatureLabController.cs` 的 `HEAD` 版本，避免继续在已被编码污染的临时版本上修改。
- 将 `UpdateStatusHint()` 的运行时实现切到新系统：
  - 不再依赖 `InteractionArea.GetOverlappingAreas()`
  - 改为直接读取玩家当前交互提示标签内容
- 将 `ApplyReadableStatusHint()` 的运行时实现切到新系统：
  - 直接复用 `UpdateStatusHint()`
- 旧的重叠扫描实现被包裹到 `#if false` 中，已不再参与编译和运行。
- 保持 `SystemFeatureOverlayAdapter.cs` 同样读取玩家当前提示文本，确保地图 UI 与调试/实验 UI 使用同一条格子交互链路。

结果：

- 地图层运行时交互提示已不再通过球形碰撞重叠扫描获得。
- 当前项目中地图 UI 主链已经统一改为基于玩家当前格子交互状态。
- `dotnet build` 通过，未引入新的编译错误。
