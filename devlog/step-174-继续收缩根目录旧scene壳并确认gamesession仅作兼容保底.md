# step-174 继续收缩根目录旧 Scene 壳并确认 GameSession 仅作兼容保底

时间：2026-04-07

本次目标：
- 在旧布局链和旧 root prefab 链已经归档的基础上，继续收缩工作区里的旧 Scene 壳
- 让当前工作区的场景入口更集中到 donor 新主链路

本次处理内容：

1. 归档未被活跃链路引用的旧 root Scene 文件
- 新归档目录：
  - `Legacy/2026-04-07-unused-root-scenes/`

归档内容包括：
- `Scene/BaseInteractable.tscn`
- `Scene/Battle.tscn`
- `Scene/BattleEncounterEnemy.tscn`
- `Scene/DeckBuilder.tscn`
- `Scene/Enemy.tscn`
- `Scene/GridFloorTemplate.tscn`
- `Scene/Healing.tscn`
- `Scene/MapDoor.tscn`
- `Scene/NpcScene.tscn`
- `Scene/SceneDoor.tscn`
- `Scene/人物待机.gif`

结果：
- 当前工作区 `Scene/` 根目录中的旧 root Scene 壳已经基本清空
- 当前主链路场景结构更明确地集中在：
  - `Scene/Maps/*`
  - `Scene/Character/*`
  - `Scene/InteractableItem/*`
  - `Scene/UI/*`

2. 再次确认 `GameSession` 当前定位
- 当前非 Legacy / 非 Merge 的活跃脚本里，已经没有直接 `/root/GameSession` 依赖
- 这说明前几轮的迁移与清理已经起效
- 但 `project.godot` 中仍保留：
  - `GameSession="*res://AutoLoad/GameSession.cs"`

当前判断：
- 现在的 `GameSession` 已经基本不是业务主干
- 它更像是：
  - donor 旧接口兼容层
  - 运行时兜底 facade

为什么这一步没有直接移除 autoload：
- 当前虽然已无显式活跃脚本依赖
- 但还没有对所有运行路径做一次完整编辑器实测
- 为避免出现“文本搜索没引用，但某个工具场景 / 隐藏入口仍依赖”的情况，先不贸然移除

验证：
- 执行 `dotnet build`
- 结果：通过，0 warning，0 error

当前状态：
- 当前工作区中的旧 Scene 壳已大幅减少
- donor 新地图链路已经明显成为唯一主链路
- `GameSession` 仍保留，但更多是兼容保底层，而不再是业务主轴

下一步建议：
1. 如果继续深收缩，可以考虑下一轮评估：
   - `GameSession` 是否还能从 autoload 降级
2. 或者改为开始做剩余 battle / map 状态边界的进一步统一
