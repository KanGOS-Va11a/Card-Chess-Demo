# step-188 接入格子交互锚点并为主流程 NPC 与教程剧情区启用格子化摆放

时间：2026-04-23

## 本次目标

- 在第一轮格子交互基础设施之上，继续把两类显式节点对象接入格子化 authoring：
  - `Npc`
  - `StoryTriggerZone`

目标不是一次性迁完所有地图交互，而是先把主流程里最关键、最容易落地的一批节点型对象切到格子模式。

## 本次改动

### 1. 新增格子交互锚点容器

新增文件：

- `Scripts/Map/Grid/GridInteractableAnchor.cs`

作用：

- 让包装型场景（例如 `NpcScene` 这类 `Node2D -> 交互体子节点` 结构）也能拥有：
  - `UseGridPlacement`
  - `Cell`
  - `InteractionId`
- 并把稳定交互键传递给真正的 `InteractableTemplate`

这一步是为后续继续迁移更多“有 root 包装层的交互预制件”做准备。

### 2. `InteractableTemplate` 增补显式 `InteractionId`

修改：

- `Scripts/Map/Interaction/InteractableTemplate.cs`

结果：

- 交互物现在支持直接导出 `InteractionId`
- `BuildRuntimeStateKey(...)` 的优先级调整为：
  1. `InteractionId`
  2. 父级 `GridInteractableNode2D`
  3. 场景路径 + 格子
  4. 节点路径兜底

这样后续即使地图层级调整，主流程交互对象也可以拥有稳定状态键。

### 3. `StoryTriggerZone` 新增纯格子触发模式

修改：

- `Scripts/Map/Interaction/StoryTriggerZone.cs`

新增能力：

- `UseGridTriggerPlacement`
- `OriginCell`
- `TriggerSizeCells`
- `GridTileSize`

行为变化：

- 启用格子模式后，运行时不再依赖 `Area2D.BodyEntered`
- 改为每帧读取玩家当前格，检测是否进入触发格覆盖范围
- 进入时触发一次 `Interact(player)`

同时：

- 编辑器下会继续按格刷新可视化矩形大小
- 运行时会把触发区根节点位置对齐到 `OriginCell`

说明：

- 旧 `Area2D` 触发链路仍保留给未迁移实例
- 因此当前处于“格子触发与旧触发并行存在，但新实例可以开始切换”的状态

### 4. `Npc` 场景接入格子锚点

修改：

- `Scene/Character/Npc.tscn`
- `Scene/Character/Npc05.tscn`

结果：

- 两个 NPC 预制体的 root `NpcScene` 现在挂上了 `GridInteractableAnchor`
- 后续地图实例可以直接在 root 上配置：
  - `UseGridPlacement`
  - `Cell`
  - `InteractionId`

### 5. 主流程地图中的第一批实例落地

修改：

- `Scene/Maps/Scene03.tscn`
- `Scene/Maps/Scene04.tscn`
- `Scene/Maps/Scene05.tscn`

具体落地：

#### `Scene03`

- `TutorialBasicTrigger` 已启用 `UseGridTriggerPlacement`
- 使用显式：
  - `OriginCell`
  - `TriggerSizeCells`

#### `Scene04`

- `TutorialEscapeTrigger` 已启用格子触发
- `TutorialArakawaTrigger` 已启用格子触发

#### `Scene05`

- 两个港口 NPC 实例已启用：
  - `UseGridPlacement`
  - `Cell`
  - `InteractionId`

这意味着主流程里已经出现了第一批真正使用格子 authoring 的节点型地图交互物。

## 本次未做

- 未迁移 `HealStation`
- 未迁移 `BattleEncounterEnemy`
- 未把 `Scene05 / Scene06` 中位置明显未对齐格子的剧情区全部切成格子触发
- 未处理 `InteractionLayer` 里仍由 TileSet 承担的历史箱子 / 柜子实例

这些留给下一轮继续推进。

## 当前阶段结论

到这一步为止，地图格子化交互已经不再只停留在“基础设施存在”：

- 玩家显式交互主链已经走格子
- `Npc` 已具备格子化摆放能力
- `StoryTriggerZone` 已具备格子化触发能力
- `Scene03 / Scene04 / Scene05` 中已有第一批主流程实例落地

这说明后续继续迁移 `HealStation` 和更多剧情 / 教程节点时，不需要再重新设计 authoring 方式。

## 静态验证

- 已执行 `dotnet build`
- 结果：通过

## 运行时验证

- 本次未执行 Godot 运行测试
- 按当前阶段约定，运行时内容由开发者本人审查
