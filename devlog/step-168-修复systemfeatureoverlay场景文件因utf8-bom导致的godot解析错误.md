# step-168 修复 SystemFeatureOverlay 场景文件因 UTF-8 BOM 导致的 Godot 解析错误

时间：2026-04-07

问题现象：
- 进入 donor 地图时控制台报错：
  - `res://Scene/UI/SystemFeatureOverlay.tscn:1 - Parse Error: Expected '['`
- 报错位置在 `SystemFeatureOverlayMount` 尝试加载 `SystemFeatureOverlay.tscn` 时

根因定位：
- `SystemFeatureOverlay.tscn` 的文本内容本身没有坏
- 但文件头存在 UTF-8 BOM：
  - 十六进制前导字节为 `EF BB BF`
- Godot 在这里没有把 BOM 正常忽略，导致第 1 行开头不是直接读到 `[`，从而报 `Expected '['`

本次处理：
- 将 `Scene/UI/SystemFeatureOverlay.tscn` 重写为无 BOM 的 UTF-8 文本
- 修复后文件头前 8 个字节变为：
  - `5B 67 64 5F 73 63 65 6E`
  - 对应正常的 `[gd_scen`

验证：
- 再次检查文件头，确认 BOM 已移除
- 执行 `dotnet build`
- 结果：通过，0 warning，0 error

备注：
- 这次问题不是场景结构错，也不是挂载逻辑错
- 是由文件编码引起的 Godot 文本场景解析问题
- 后续如果再通过脚本批量生成 `.tscn` / `.tres`，要优先使用无 BOM 的 UTF-8
