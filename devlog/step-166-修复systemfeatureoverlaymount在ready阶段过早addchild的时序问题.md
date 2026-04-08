# step-166 修复 SystemFeatureOverlayMount 在 _Ready 阶段过早 add_child 的时序问题

时间：2026-04-07

问题现象：
- donor 地图启动时控制台报错：
  - `Parent node is busy setting up children, add_child() failed`
  - `Invalid owner. Owner must be an ancestor in the tree`

定位结果：
- 问题点在 `SystemFeatureOverlayMount.cs`
- 原因不是接口错了，也不是挂载目标路径错了
- 而是挂载时机过早：
  - donor 地图当前还在 `_Ready` 建树阶段
  - 此时 `sceneRoot.AddChild(overlay)` 会触发 Godot 的 `blocked > 0` 限制
  - `AddChild` 失败后继续设 `Owner`，就会连带出现 `Invalid owner`

本次修改：
- 文件：`Scripts/Map/UI/SystemFeatureOverlayMount.cs`
- 调整内容：
  - 将 `_Ready()` 改为 `async`
  - 在真正挂载 `overlay` 之前，先 `await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame)`
  - 等 donor 地图完成当前帧建树后，再执行 `AddChild`
  - 若等待后 `sceneRoot` 已无效，则安全回收 `overlay/sourceRoot`

修改后的效果：
- `SystemFeatureOverlayMount` 不再在父节点忙于建树时强插 `add_child`
- `Owner` 设置也发生在合法入树之后

验证：
- 执行 `dotnet build`
- 结果：通过，0 error

备注：
- 这次修复没有改变菜单接入方案
- 仍然保持“从 `SystemFeatureLab.tscn` 提取 `SystemUI` 子树，再挂到 donor 新版地图”的方案不变
