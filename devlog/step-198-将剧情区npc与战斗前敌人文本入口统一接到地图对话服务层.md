# step-198 将剧情区、NPC 与战斗前敌人文本入口统一接到地图对话服务层

日期：2026-04-23

本次改动：

- `StoryTriggerZone.cs`
  - 改为通过 `MapDialogueService.PresentAsync(...)` 播放剧情文本
  - 不再直接实例化 `DialogueSequencePanel`
  - 对话完成后仍按原逻辑继续：
    - 触发目标交互
    - 开始战斗
    - 切场景
- `Npc.cs`
  - 改为通过 `MapDialogueService.PresentAsync(...)` 播放 NPC 对话
  - 不再自行锁定/恢复玩家输入
- `StoryDialogueEnemy.cs`
  - 改为通过 `MapDialogueService.PresentAsync(...)` 播放战斗前文本
  - 对话完成后再进入 `base.OnInteract(player)` 启动原有战斗逻辑

结果：

- 当前主流程中的三类地图文本入口已经进入同一条运行时服务链路。
- 地图脚本不再各自直接：
  - `Instantiate()` 对话面板
  - `AddChild(panel)`
  - 手动锁/解锁玩家输入
- Day 2 的统一对话链路已经从“服务层存在”推进到“主调用方接入”。
