# step-190 迁移 Scene01 与 Scene05 的第一批箱柜和回复点实例到显式格子节点

时间：2026-04-23

## 本次目标

- 从“预制具备格子化入口”推进到“主流程地图中已有第一批真实实例完成迁移”
- 先处理能够静态确认格子位置、且最容易落地的交互对象：
  - `Scene01` 的柜子
  - `Scene05` 的箱子
  - `Scene05` 的回复点

## 本次改动

### 1. `Scene01` 迁出旧 `InteractionLayer` 柜子实例

修改：

- `Scene/Maps/Scene01.tscn`

结果：

- 原 `InteractionLayer` 中的单个柜子 scene tile 已清空
- 在 `WorldObjects` 下新增：
  - `GridInteractables`
- 显式添加：
  - `Cabinet01`

配置：

- `UseGridPlacement = true`
- `Cell = (1, -5)`
- `InteractionId = "scene01_cabinet_01"`

这意味着：

- `Scene01` 中这只柜子已不再依赖 `InteractionLayer` 的 scene tile 实例
- 现在由显式节点承载位置和状态键

### 2. `Scene05` 迁出旧 `InteractionLayer` 的箱子与回复点

修改：

- `Scene/Maps/Scene05.tscn`

结果：

- 原 `InteractionLayer` 中的两项交互 scene tile 已清空
- 在 `WorldObjects` 下新增：
  - `GridInteractables`

并显式添加：

- `Chest03`
  - `UseGridPlacement = true`
  - `Cell = (18, 14)`
  - `InteractionId = "scene05_chest_01"`

- `HealingStation`
  - `UseGridPlacement = true`
  - `Cell = (22, -4)`
  - `InteractionId = "scene05_healstation_01"`

说明：

- 这两个格子坐标来自现有 `InteractionLayer.tile_map_data` 的静态解码，不是运行时试摆

### 3. 主流程地图开始出现“显式格子交互节点容器”

本次后，以下地图已经出现新的显式格子交互容器：

- `Scene01 -> WorldObjects/GridInteractables`
- `Scene05 -> WorldObjects/GridInteractables`

这标志着地图交互实例迁移已经从“脚本和预制基础”进入“真实地图场景落地”阶段。

## 本次未做

- 未迁移 `Scene03` / `Scene04` / `Scene06` 中 `InteractionLayer` 的旧 scene tile 实例
- 未处理 `Scene03` 里可能存在的旧箱柜 scene tile
- 未迁移 `Cabinet01.tscn / Cabinet02.tscn` 的场景脚本引用到 `Chest.cs`
- 未迁移更多 `HealStation` 实例（`Scene09/10/11` 仍为旧方式）

## 当前阶段结论

到这一步，地图格子化交互已经完成了从“基础设施存在”到“主流程地图有真实迁移实例”的跨越：

- 玩家交互主链已改成按格查询
- `Npc` 与 `StoryTriggerZone` 有了格子化入口
- `Chest / Cabinet / HealStation` 预制已具备格子 authoring 入口
- `Scene01` 和 `Scene05` 已经开始脱离旧 `InteractionLayer` scene tile 方案

下一轮优先级应为：

1. 继续迁 `Scene03 / Scene04` 中的第一批旧 `InteractionLayer` 实例
2. 再决定是否把 `Cabinet01 / Cabinet02` 预制正式切到同一脚本引用

## 静态验证

- 已执行 `dotnet build`
- 结果：通过

## 运行时验证

- 本次未执行 Godot 运行测试
- 按当前阶段约定，运行时内容由开发者本人审查
