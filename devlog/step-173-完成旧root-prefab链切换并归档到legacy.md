# step-173 完成旧 root prefab 链切换并归档到 Legacy

时间：2026-04-07

本次目标：
- 继续按“归档旧链路、保留 donor 新主链路”的原则推进清理
- 把仍然残留在工作区里的旧 root prefab 链彻底切掉

本次先做了什么：

1. 先把活跃 prefab 全切到 `Scripts/Map/*`
- `Scene/InteractableItem/Door.tscn`
  - 已切到 `Scripts/Map/Interaction/SceneDoor.cs`
- `Scene/InteractableItem/HealingStation.tscn`
  - 已切到 `Scripts/Map/Interaction/HealStation.cs`
- `Scene/Character/Npc.tscn`
  - 已切到 `Scripts/Map/Interaction/Npc.cs`

这一步完成后，当前 donor 地图主链路中的门 / 回血站 / NPC 已不再依赖旧 root 脚本链。

2. 归档旧 root prefab 链
- 新归档目录：
  - `Legacy/2026-04-07-premerge-root-prefab-chain/`

归档内容包括：
- 旧 root scene prefab：
  - `Scene/Chest.tscn`
  - `Scene/Door.tscn`
  - `Scene/HealingStation.tscn`
  - `Scene/Npc.tscn`
- 旧 root 基类：
  - `IInteractable.cs`
  - `InteractableTemplate.cs`
- 旧 root 配套脚本：
  - `Scripts/Character/Player.cs`
  - `Scripts/Character/Npc.cs`
  - `Scripts/Item/Chest.cs`
  - `Scripts/Item/Door.cs`
  - `Scripts/Item/HealStation.cs`
- 以及对应 `.uid`

中途处理的一个点：
- 旧 root 链归档后，`HubEntrySavePoint.cs` 还残留在全局命名空间写法
- 已顺手修正为：
  - `namespace CardChessDemo.Map`
  - 显式使用 `CardChessDemo.Map.InteractableTemplate`
  - 这样旧 root 基类归档后也不会再拖住编译

结果：
- 当前工作区中的地图交互主链路，已经基本统一为：
  - `Scripts/Map/*`
- 旧 root 地图链已经完整退出当前工作区主链路
- 之后继续 merge / 清理时，不需要再围绕旧 root prefab 链做托底兼容

验证：
- 执行 `dotnet build`
- 结果：通过，0 error

当前状态总结：
- donor 新地图链路继续可编译
- donor 活跃 prefab 已切入 `Scripts/Map/*`
- 合并前旧地图布局与旧 root prefab 链都已经进入 `Legacy`

下一步建议：
1. 继续处理更深层的接口冲突
2. 重点看 donor 地图与 battle 之间剩余的状态边界是否还有冗余适配
3. 如果需要，也可以开始对 `Legacy` 内容做一轮文档化说明，方便队友看懂哪些东西已退役
