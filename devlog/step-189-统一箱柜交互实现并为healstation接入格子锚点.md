# step-189 统一箱柜交互实现并为 HealStation 接入格子锚点

时间：2026-04-23

## 本次目标

- 把 `Chest` 与 `Cabinet` 的交互行为统一到同一套实现上
- 避免后续在两套箱柜逻辑上继续并行返工
- 让 `HealStation` 的预制根节点也具备格子化 authoring 入口

## 本次改动

### 1. `Chest` 扩展为通用箱柜交互实现

修改：

- `Scripts/Map/Interaction/Chest.cs`

新增 / 调整：

- 增加兼容字段：
  - `LootItemId`
  - `LootAmount`
  - `OpenedTint`
- `Chest` 现在既支持：
  - `AnimatedSprite2D` 的开箱动画
  - 也支持 `Sprite2D` 的静态开箱变色
- 奖励枚举逻辑现在可同时兼容：
  - `GrantedItems`
  - `GrantedItemId`
  - `LootItemId + LootAmount`

结果：

- `Chest` 已不再只是“宝箱脚本”
- 它已经可以承载“柜子式静态贴图交互”和“开箱式动画交互”两种形态

### 2. `Cabinet` 退化为兼容壳

修改：

- `Scripts/Map/Interaction/Cabinet.cs`

结果：

- `Cabinet` 不再维护独立交互逻辑
- 改为继承 `Chest`
- 只在 `_Ready()` 中补默认语义：
  - 默认显示名改成“储物柜”
  - 默认提示改成“搜索柜子”
  - 默认空柜提示改成柜子语义
  - 如无显式奖励配置，则默认给 `steel_scrap`

说明：

- 这是一个过渡方案
- 目的是先统一行为实现，再决定后续是否把场景资源也全部切到同一脚本引用

### 3. `HealStation` 预制根节点接入格子锚点

修改：

- `Scene/InteractableItem/HealingStation.tscn`

结果：

- `HealingStationRoot` 已挂上 `GridInteractableAnchor`
- 并指定 `TargetNodePath = "HealingStation"`

这意味着：

- 后续地图中的回复点实例可以直接使用：
  - `UseGridPlacement`
  - `Cell`
  - `InteractionId`

## 本次未做

- 未把 `Cabinet01.tscn / Cabinet02.tscn` 正式切换到 `Chest.cs` 场景引用
- 未迁移主流程地图中的 `InteractionLayer` 箱柜实例到显式节点
- 未迁移主流程中的 `HealStation` 实例（当前主流程地图本身还没有在用）

## 当前阶段结论

到这一步，地图格子化交互的“箱柜与补给点预制基础”已经具备：

- 箱子和柜子的行为实现不再分叉
- 回复点也有了格子化 authoring 入口

下一轮应优先开始做的，不再是继续扩脚本，而是：

1. 在主流程地图中把第一批 `Chest` / `Cabinet` 实例从 `InteractionLayer` 迁到显式节点
2. 再视情况接入主流程地图中的 `HealStation`

## 静态验证

- 已执行 `dotnet build`
- 结果：通过

## 运行时验证

- 本次未执行 Godot 运行测试
- 按当前阶段约定，运行时内容由开发者本人审查
