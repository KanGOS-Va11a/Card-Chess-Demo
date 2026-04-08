# step-167 将 donor 地图的 C 键系统菜单正式化为独立 overlay 场景

时间：2026-04-07

本次目标：
- 把 donor 新版地图里的 `C` 键系统菜单，从临时挂载方案升级成正式资源
- 不再依赖“从旧 `SystemFeatureLab.tscn` 运行时拆 `SystemUI`”的方式
- 顺手修正菜单窗口尺寸与关键静态文案

这次做了什么：

1. 生成正式 overlay 场景
- 新文件：
  - `Scene/UI/SystemFeatureOverlay.tscn`
- 来源：
  - 以旧 `Scene/SystemFeatureLab.tscn` 的 `SystemUI` 子树为基础抽出
- 结果：
  - donor 地图的菜单现在有了正式的独立 UI 场景资源

2. 切换挂载脚本到正式 overlay 资源
- 文件：
  - `Scripts/Map/UI/SystemFeatureOverlayMount.cs`
- 修改：
  - `SourceScenePath` 改为 `res://Scene/UI/SystemFeatureOverlay.tscn`
  - 不再去旧实验场景里找 `SystemUI` 节点
  - 直接实例化 `CanvasLayer` overlay
  - 保留延后一帧挂载，避免 Godot 建树时机报错
- 结果：
  - donor 地图接入 `C` 菜单的方式更稳定、更正式

3. 收窄菜单窗口尺寸
- 文件：
  - `Scene/UI/SystemFeatureOverlay.tscn`
- 修改：
  - 菜单窗口上下边距略收
  - `Tabs` 最小高度由 `258` 调整为 `244`
- 目的：
  - 更贴合 donor 地图探索时的窗口尺寸
  - 避免菜单显得过于“实验场景化”

4. 修正 overlay 中关键静态文案
- 文件：
  - `Scene/UI/SystemFeatureOverlay.tscn`
- 已整理内容包括：
  - 顶部操作提示
  - 状态行默认提示
  - 菜单标题
  - 角色 / 装备 / 天赋 / 图鉴 / 构筑相关主要标签
  - 构筑按钮文案
- 结果：
  - 菜单不再直接显示旧实验场景里那批乱码式标题

5. 恢复 `SystemFeatureLabController.cs` 到可编译稳定版本
- 说明：
  - 中途对控制器做中文化时，受历史编码影响，文件出现了大范围字符串损坏
  - 为避免继续在不稳定基线上叠改，已直接恢复为 donor 版本的稳定可编译内容
- 结论：
  - 当前正式化工作的重心转回 overlay 场景本身
  - 暂不在这一步继续深改 `SystemFeatureLabController.cs` 内部字符串

验证：
- 执行 `dotnet build`
- 结果：通过，0 warning，0 error

当前状态总结：
- donor 地图已经拥有正式的 `C` 键系统菜单 overlay 资源
- 地图挂载脚本也已经切到正式 overlay
- 后续如果要继续调整布局、面板、标签或加入 donor 地图专属入口，不需要再回到旧测试场景

下一步建议：
1. 进入 donor 地图实际验证 `C` 菜单的弹出、关闭、玩家输入锁定是否正常
2. 根据 donor 地图实际分辨率，再继续微调 overlay 的窗口宽高与提示条位置
3. 在菜单稳定后，再继续修 battle 与菜单 / donor 地图之间的细节衔接
