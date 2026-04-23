# step-202 将 Npc 与 Scene01TutorialController 补成统一后续动作协议入口

日期：2026-04-23

本次改动：

- `Npc.cs`
  - 在提交 `MapDialogueRequest` 时显式附带：
    - `CompletedFollowUpActions`
    - `ClosedFollowUpActions`
  - 当前默认返回空动作数组，以保持 NPC 现有流程表现不变
- `Scene01TutorialController.cs`
  - 在教程文本请求中显式附带：
    - `CompletedFollowUpActions`
    - `ClosedFollowUpActions`
  - 按 `DialogFlow` 统一生成 follow-up actions
  - 当前 `Intro / EnemySighted` 默认都返回空动作数组，以保持当前教程行为不变

结果：

- 当前地图主流程中的文本入口已经全部进入统一后续动作协议框架。
- 即使某些入口当前没有后续动作，也不再绕开协议本身。
