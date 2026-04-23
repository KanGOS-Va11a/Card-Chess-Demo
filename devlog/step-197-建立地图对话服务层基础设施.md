# step-197 建立地图对话服务层基础设施

日期：2026-04-23

本次改动：

- 新增 Day 2 第一阶段的地图对话服务层基础设施：
  - `MapDialogueCompletionStatus.cs`
  - `MapDialogueRequest.cs`
  - `MapDialogueResult.cs`
  - `MapDialogueService.cs`
- `MapDialogueService` 统一负责：
  - 检查当前是否已有对话正在播放
  - 实例化 `DialogueSequencePanel`
  - 统一锁定/恢复玩家输入
  - 返回标准化的对话完成结果
- 当前阶段尚未改动 `StoryTriggerZone / Npc / StoryDialogueEnemy` 的调用方逻辑。

结果：

- Day 2 的“统一对话入口”已经具备基础结构。
- 后续调用方不需要再自己直接 `Instantiate` 对话面板，只需要改成提交 `MapDialogueRequest`。
