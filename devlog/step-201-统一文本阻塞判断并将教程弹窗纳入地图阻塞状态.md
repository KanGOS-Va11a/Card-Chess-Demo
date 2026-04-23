# step-201 统一文本阻塞判断并将教程弹窗纳入地图阻塞状态

日期：2026-04-23

本次改动：

- `MapDialogueService.cs`
  - 新增活跃阻塞对话计数
  - 统一提供 `HasBlockingDialogue()` / `IsDialogueVisible(...)`
- `MapTextBlocker.cs`
  - 改为优先依赖 `MapDialogueService` 的阻塞状态
  - 不再直接依赖 `DialogueSequencePanel.IsVisible(...)` 作为唯一判断来源
  - 将 `PagedTutorialPopup` 纳入地图输入阻塞判断
- `PagedTutorialPopup.cs`
  - 新增 `IsVisible(...)`

结果：

- 地图侧文本阻塞判断已开始从“枚举具体面板类型”收敛到“依赖统一服务状态”。
- 教程弹窗也进入了统一阻塞判断链路。
- `dotnet build` 通过，未引入新的编译错误。
