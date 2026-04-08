# step-162 按 merge 版 Scene01 修正地图交互合并基准并补齐快照与门箱 NPC 接口

时间：2026-04-07

本次目标：
- 纠正地图层 merge 的错误参考基准
- 统一以后续 donor 工程 `Merge/Card-Chess-Demo/Scene/Maps/Scene01.tscn` 为场景节点结构基准
- 继续在不破坏 battle 主干的前提下，把地图交互层能力补齐

本次确认的重要事实：
- 后续地图 merge 的参考 Scene01，不是先前误用的旧理解版本
- 真实玩家节点路径应按 merge 工程 Scene01 处理，为 `MainPlayer/Player`
- 后续你的 `C` 键综合菜单接入，也必须围绕这套 Scene01 结构来设计挂点

本次修改内容：

1. 修正地图恢复时的玩家节点解析
- 文件：`Scripts/Map/Controllers/MapSceneController.cs`
- 修改：
  - 默认 `PlayerPath` 改为 `MainPlayer/Player`
  - fallback 改为先找 `MainPlayer/Player`，再找旧式 `Player`
- 目的：
  - 让 donor Scene01 结构下，战后回图与玩家位置恢复不再因默认路径错误失效

2. 把 Scene01 实际接上地图恢复控制器
- 文件：`Scene/Maps/Scene01.tscn`
- 修改：
  - 新增子节点 `MapSceneController`
  - 显式设置 `PlayerPath = ../MainPlayer/Player`
- 目的：
  - 避免 Scene01 因根节点已经挂教程脚本，导致地图恢复逻辑虽然存在但根本没有执行入口

3. 补回地图进入战斗时的运行时快照
- 文件：`Scripts/Map/Transitions/MapBattleTransitionHelper.cs`
- 修改：
  - 进入战斗前解析当前真实场景路径
  - 捕获当前地图交互对象快照 `MapRuntimeSnapshot`
  - 将快照写入 `MapResumeContext`
  - 保留已有异步转场 overlay 逻辑
- 目的：
  - donor 地图的宝箱、门、NPC 等交互状态可以在战后回图时继续恢复

4. 合并 BattleEncounterEnemy 的 donor 行为
- 文件：`Scripts/Map/Interaction/BattleEncounterEnemy.cs`
- 修改：
  - 增加 `DisableAfterInteract`
  - 当战斗启动失败时回滚禁用状态
- 目的：
  - 兼容 donor 地图中“交互后应失活”的遭遇敌人需求

5. 合并 donor 宝箱行为并接入快照恢复
- 文件：`Scripts/Map/Interaction/Chest.cs`
- 修改：
  - 改为使用 `AnimatedSprite2D` 的 `closed/open` 动画状态
  - 打开后记录 `_isOpened`
  - 实现 `BuildRuntimeSnapshot/ApplyRuntimeSnapshot`
- 目的：
  - 让 donor 风格宝箱在战后回图时维持已打开状态，而不是重置

6. 合并 donor NPC 对话表现
- 文件：`Scripts/Map/Interaction/Npc.cs`
- 修改：
  - 交互从 `GD.Print` 改为 `GalDialogueOverlay.Show(...)`
- 目的：
  - donor 地图中 NPC 不再只是打印日志，而是能直接出对话面板

7. 补齐 Door / SceneDoor 的 donor 同场景传送能力
- 文件：
  - `Scripts/Item/Door.cs`
  - `Scripts/Map/Interaction/SceneDoor.cs`
- 修改：
  - 支持 `UsesInSceneTeleport`
  - 支持 `TeleportTargetPath / TeleportOffset`
  - 支持黑屏淡入淡出与临时锁输入
- 目的：
  - donor 地图里需要“同图门传送”的地方可以直接接进当前工程，而不是只能切整张场景

本次没有做的事：
- 没有改 `project.godot` 的主场景入口
- 没有动 battle 核心脚本
- 没有把 donor 的全局单例直接并进来
- 没有开始改 `C` 键综合菜单本体，只是明确了后续必须依附的 Scene01 结构

验证：
- 执行 `dotnet build`
- 结果：通过，0 error

当前收益：
- donor Scene01 的真实节点结构已经被写回当前合并逻辑
- 地图 -> 战斗 -> 回地图 这条链路的状态恢复边界更加完整
- 宝箱 / NPC / 门这三类最常见地图交互，已经开始按 donor 需求并轨

下一步建议：
1. 继续处理 `SystemFeatureLabController.cs`
2. 逐张 donor 地图核对 Door / Enemy / Chest 的 Inspector 参数
3. 在 Scene01 的 `MainPlayer/UI` 周边规划 `C` 键综合菜单挂载位置
