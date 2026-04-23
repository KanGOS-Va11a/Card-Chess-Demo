# step-195 将 SystemFeatureLabController 的运行时提示逻辑切到干净 partial 文件

日期：2026-04-23

本次改动：

- 在 `Scripts/Map/UI/SystemFeatureLabController.StatusHints.cs` 中新增干净的运行时提示实现：
  - `UpdateStatusHint()`
  - `ApplyReadableStatusHint()`
  - `TryGetPlayerInteractionHint(...)`
- 将 `SystemFeatureLabController.cs` 中原有的旧提示方法重命名为：
  - `LegacyUpdateStatusHint_DoNotUse()`
  - `LegacyApplyReadableStatusHint_DoNotUse()`

结果：

- `SystemFeatureLabController` 的运行时状态提示主链已经不再依赖旧的碰撞重叠扫描实现。
- 后续维护 Day 1 收口后的提示逻辑时，只需要查看 `SystemFeatureLabController.StatusHints.cs`。
- `dotnet build` 通过，未引入新的编译错误。

说明：

- 旧方法体仍保留在原控制器中，原因是该文件历史上存在编码污染和大体量混合逻辑，直接在原文件内部大范围删改风险过高。
- 当前方案优先保证：
  1. 运行时主链切换成功
  2. 后续维护入口干净
  3. 不破坏既有系统面板主体功能
