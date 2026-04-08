# step-172 归档合并前旧地图布局并保留旧 Player 脚本作为活跃 prefab 编译托底

时间：2026-04-07

本次目标：
- 把合并前使用的旧地图布局归进 `Legacy`
- 保持 donor 新地图主链路可编译

本次先确认了什么：
- 旧布局链本身已经不再属于 donor 新地图主链路
- 但并不是整条旧 root 链都能一刀切走
- 当前 donor 仍有部分活跃 prefab 还挂着旧 root 脚本：
  - `Scene/InteractableItem/Door.tscn`
  - `Scene/InteractableItem/HealingStation.tscn`
  - `Scene/Character/Npc.tscn`

这意味着：
- 旧地图布局场景可以归档
- 但旧 root 交互链的部分脚本暂时还不能一起完全清空

本次归档到 Legacy 的内容：
- `Scene/Scene1.tscn`
- `Scene/Scene2.tscn`
- `Scene/Scene3.tscn`
- `Scene/Mainlevel.tscn`
- `Scene/GridTest30x20.tscn`
- `Scene/SystemFeatureLab.tscn`
- `Scene/MainPlayer.tscn`
- 配套旧脚本：
  - `Scripts/Character/Player.cs`
  - `Scripts/Character/Player.cs.uid`

归档目录：
- `Legacy/2026-04-07-premerge-map-layouts/`

中途出现的问题：
- 归档 `Scripts/Character/Player.cs` 后，当前工程编译失败
- 根因不是 donor 新地图直接在用旧地图布局
- 而是 donor 当前活跃 prefab 的旧 root 脚本签名仍然依赖全局命名空间下的 `Player` 类型

本次处理方式：
- 旧地图布局场景继续留在 `Legacy`
- 但为保证当前主链路继续编译运行，已将：
  - `Scripts/Character/Player.cs`
  - `Scripts/Character/Player.cs.uid`
  临时恢复回工作区

这样做的结论：
- 旧地图布局已经从工作区主入口移走
- 旧 `Player` 脚本当前仅作为“活跃旧 prefab 编译托底”保留
- 后续若继续深清，应先做一件事：
  - 把 `Scene/InteractableItem/Door.tscn`
  - `Scene/InteractableItem/HealingStation.tscn`
  - `Scene/Character/Npc.tscn`
  这类活跃 prefab 的脚本切到 `Scripts/Map/*`

验证：
- 执行 `dotnet build`
- 结果：通过，0 error

当前状态：
- donor 新地图主链路继续可用
- 合并前旧地图布局已归档
- 剩余需要继续处理的，不再是旧地图布局本身，而是旧 root prefab 脚本链

下一步建议：
1. 继续把活跃 prefab 从旧 root 脚本链切到 `Scripts/Map/*`
2. 完成后再彻底归档：
   - `Scripts/Item/*`
   - `Scripts/Character/Npc.cs`
   - `IInteractable.cs`
   - `InteractableTemplate.cs`
