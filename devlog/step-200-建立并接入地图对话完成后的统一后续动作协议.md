# step-200 建立并接入地图对话完成后的统一后续动作协议

日期：2026-04-23

本次改动：

- 新增地图对话完成后的统一动作协议：
  - `MapDialogueFollowUpKind.cs`
  - `MapDialogueFollowUpAction.cs`
  - `MapDialogueFollowUpResult.cs`
- `MapDialogueRequest.cs` 增加：
  - `CompletedFollowUpActions`
  - `ClosedFollowUpActions`
- `MapDialogueService.cs` 增加统一动作执行能力：
  - `ExecuteFollowUpActions(...)`
  - 统一支持：
    - `TriggerInteractable`
    - `StartBattle`
    - `ChangeScene`
- `StoryTriggerZone.cs`
  - 完成剧情后不再自己分散调用“触发对象/开战斗/切场景”
  - 改为先构造统一动作，再交给 `MapDialogueService.ExecuteFollowUpActions(...)`
- `StoryDialogueEnemy.cs`
  - 对话完成后通过统一动作协议进入战斗
- `Scene04To05CutsceneController.cs`
  - 对话完成或关闭后通过统一动作协议切场景

结果：

- Day 2 的“文本结束后的后续动作”已经不再是调用方各自散写的回调逻辑。
- 当前主流程中已经有统一的动作定义和执行入口。
- `dotnet build` 通过，未引入新的编译错误。
