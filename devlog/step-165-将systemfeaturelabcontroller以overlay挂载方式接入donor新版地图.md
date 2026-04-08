# step-165 将 SystemFeatureLabController 以 overlay 挂载方式接入 donor 新版地图

时间：2026-04-07

本次目标：
- 开始把 `SystemFeatureLabController` 的内容接到 donor 新版地图
- 让 donor 地图也具备 `C` 键综合菜单入口
- 不把旧实验场景整张图直接塞进 donor 地图

本次采用的方案：

不是直接把 `Scene/SystemFeatureLab.tscn` 实例化到 donor 地图里。

原因：
- 那张旧实验场景里除了 `SystemUI` 之外，还带有：
  - 旧测试背景
  - 旧测试玩家
  - 旧训练敌人
  - 旧测试地图控制节点
- 如果整张图直接接到 donor 地图，会把测试内容一并带进来，污染 donor 新版地图

因此本次改为：
- 新增一个轻量挂载脚本 `SystemFeatureOverlayMount`
- 运行时只从 `Scene/SystemFeatureLab.tscn` 里提取 `SystemUI` 子树
- 再把这个 `SystemUI` 接到 donor 地图根节点

这样做的好处：
- 复用现有 `SystemFeatureLabController` 与完整 UI 结构
- 不把旧测试地图内容带进 donor 地图
- 后续如果要做正式版 `C` 菜单，只要把挂载源替换成正式 overlay 场景即可

本次新增文件：
- `Scripts/Map/UI/SystemFeatureOverlayMount.cs`

脚本职责：
- 加载 `Scene/SystemFeatureLab.tscn`
- 提取其中的 `SystemUI`
- 将 `SystemUI` 挂到当前 donor 地图根节点
- 在挂载前，把 `PlayerPath` 改成 donor 地图真实玩家路径 `../MainPlayer/Player`
- 如果场景中已经存在 `SystemUI`，则默认跳过，防止重复挂载

本次接入的 donor 地图：
- `Scene/Maps/Scene01.tscn`
- `Scene/Maps/Scene02.tscn`
- `Scene/Maps/Scene03.tscn`
- `Scene/Maps/Scene04.tscn`
- `Scene/Maps/Scene05.tscn`
- `Scene/Maps/Scene06.tscn`

每张图都新增了：
- `SystemFeatureOverlayMount` 节点
- 并统一设置 `PlayerPath = ../MainPlayer/Player`

这一步的意义：
- donor 新版地图已经开始拥有 `C` 键综合菜单能力
- 下一步可以直接在 donor 地图上继续改菜单显示、布局与功能边界
- 不需要再依赖旧测试图作为唯一入口

本次没有做的事：
- 没有重写 `SystemFeatureLabController`
- 没有把 `SystemUI` 正式拆成独立 `.tscn` overlay 文件
- 没有开始改 battle 与菜单之间的细节边界
- 没有开始做 donor 地图里的菜单视觉本地化清理

验证：
- 执行 `dotnet build`
- 结果：通过，0 error
- 说明：
  - 本次新增挂载脚本与地图场景引用已经编译通过
  - 目前的主要剩余验证应当在编辑器里实际进 donor 地图测试 `C` 键菜单显示与交互

下一步建议：
1. 实际进入 donor 地图验证 `C` 键菜单是否正常弹出
2. 若弹出正常，再按 donor 地图需求优化菜单布局与提示文本
3. 之后再集中修正菜单与 battle / map 状态衔接细节
