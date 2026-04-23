# step-199 将场景教程与过场控制器文本入口统一接到地图对话服务层

日期：2026-04-23

本次改动：

- `Scene01TutorialController.cs`
  - 开场苏醒文本与首个敌人前置文本改为通过 `MapDialogueService.PresentAsync(...)` 播放
  - 不再直接实例化 `DialogueSequencePanel`
- `Scene04To05CutsceneController.cs`
  - 过场文本改为通过 `MapDialogueService.PresentAsync(...)` 播放
  - 不再直接实例化 `DialogueSequencePanel`

结果：

- 地图主流程中的教程文本、剧情区文本、NPC 文本、战斗前文本、过场文本，已经全部进入统一的地图对话服务层。
- Day 2 当前阶段的“统一文本播放入口”目标已基本完成。
