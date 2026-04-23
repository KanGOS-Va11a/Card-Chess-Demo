# step-187 建立地图格子交互基础设施并切断玩家对 Area 重叠查询的依赖

时间：2026-04-23

## 本次目标

- 为地图层格子化交互重构建立第一批基础设施
- 让玩家的显式交互入口不再依赖 `Area2D.GetOverlappingAreas()`
- 先把状态键和一个代表性交互物（`Chest`）接到新的稳定格子链路上

## 本次改动

### 1. 新增地图格子基础设施

新增文件：

- `Scripts/Map/Grid/MapGridService.cs`
- `Scripts/Map/Grid/GridPlacedNode2D.cs`
- `Scripts/Map/Grid/GridInteractableNode2D.cs`
- `Scripts/Map/Grid/GridTriggerNode2D.cs`

作用：

- 提供世界坐标与格子坐标之间的统一换算
- 提供面朝方向转格子偏移的统一逻辑
- 提供后续格子化节点摆放、稳定交互键与触发区覆盖范围的基础类型

### 2. 玩家交互主链切到格子查询

修改：

- `Scripts/Map/Actors/Player.cs`

结果：

- 显式交互不再通过玩家自身的 `InteractionArea` 扫描重叠区域来决定目标
- 改为根据：
  - 玩家当前格
  - 玩家面朝方向
  - 交互物所占格子

直接查询面朝目标格

同时：

- 附近高亮与提示也改为基于格子距离 / 面朝格判断
- 交互完成后记录的“最后交互对象”从 `Area2D` 改成实际 `InteractableTemplate`

说明：

- 这一步还没有把自动触发式剧情区全部迁完
- 但玩家主动按 `E` 的交互主链已经脱离 `Area2D` 重叠依赖

### 3. 运行时状态键开始从节点路径转向稳定键

修改：

- `Scripts/Map/Interaction/InteractableTemplate.cs`
- `Scripts/Map/Transitions/MapRuntimeSnapshotHelper.cs`

结果：

- `InteractableTemplate` 新增：
  - 交互主格解析
  - 按格判断是否占用交互格
  - `BuildRuntimeStateKey(...)`

- `MapRuntimeSnapshotHelper` 不再依赖“从快照 key 反查节点路径”
- 改为：
  - 扫描当前场景内的交互物
  - 为每个交互物建立稳定状态键
  - 用稳定键做快照匹配

这为后续把状态保存从“节点路径耦合”迁到“显式 `InteractionId` / 场景+格子”打下了基础。

### 4. `Chest` 接入稳定交互键

修改：

- `Scripts/Map/Interaction/Chest.cs`
- `Scripts/Map/Interaction/InteractableContainerConfig.cs`

结果：

- `InteractableContainerConfig` 现在继承 `GridInteractableNode2D`
- 以后箱子 root 容器可以直接持有：
  - `UseGridPlacement`
  - `Cell`
  - `InteractionId`

- `Chest` 的 session / runtime key 生成不再优先依赖 `GetPath()`
- 当存在格子化配置时，会优先使用稳定交互键

说明：

- 当前这一步还没有把所有箱子场景批量迁成显式格子 authoring
- 但 `Chest` 已经具备接入新 authoring 模式的代码基础

## 本次未做

- 未迁移 `Npc`
- 未迁移 `HealStation`
- 未迁移 `StoryTriggerZone`
- 未把 `InteractionLayer` 从主链中彻底退出
- 未改自动触发剧情区的底层判定

这些留给下一轮继续推进。

## 当前阶段结论

这次改动把地图格子化交互从“纯文档设计”推进到了“已有基础设施、已有主链切换、已有状态键支撑”的阶段。

当前可以认为已经完成了执行版文档中的前两步半：

1. 建格子基础设施
2. 切玩家显式交互主链
3. `Chest` 的稳定键接入一半

## 静态验证

- 已执行 `dotnet build`
- 结果：通过

## 运行时验证

- 本次未执行 Godot 运行测试
- 按当前阶段约定，运行时内容由开发者本人审查
