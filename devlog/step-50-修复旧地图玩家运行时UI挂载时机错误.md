# Step 50 - 修复旧地图玩家运行时 UI 挂载时机错误

## 日期

2026-03-25

## 目标

修复朋友合并进来的旧地图原型在运行时创建交互提示 UI 时，因为在 `_Ready()` 内同步调用 `add_sibling()` 而触发的节点树阻塞错误。

## 改动

- 修改 `Scripts/Character/Player.cs`
- 当未找到 `InteractionHintLabel` 时：
  - 仍然创建兜底 `CanvasLayer + Label`
  - 不再在 `_Ready()` 中调用 `AddSibling(...)`
  - 改为对父节点执行 `CallDeferred("add_child", fallbackUi)`

## 原因

Godot 在节点仍处于“正在建立子节点”的阶段时，不允许同步插入兄弟节点；旧实现会触发：

- `Parent node is busy setting up children`
- `add_sibling() failed`

这不是逻辑错误，而是节点挂载时机错误。

## 结果

- 旧原型场景继续可以保留兜底交互提示 UI
- 不会再因为 `_Ready()` 时机问题触发 `add_sibling()` 运行时报错

## 验证

- 已重新执行 `dotnet build`
- 构建通过，`0` 错误
