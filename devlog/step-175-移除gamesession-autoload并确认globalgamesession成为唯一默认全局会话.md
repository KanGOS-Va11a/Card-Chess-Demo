# step-175 移除 GameSession autoload 并确认 GlobalGameSession 成为唯一默认全局会话

时间：2026-04-07

本次目标：
- 在前几轮持续清理后，进一步收缩运行时全局状态节点
- 让当前运行时默认只保留一个真正的全局会话节点

本次前提确认：
- 当前非 `Legacy` / 非 `Merge` 的活跃代码中，已经没有任何地方再直接依赖：
  - `/root/GameSession`
- 这意味着：
  - `GameSession` 已经不再是当前运行时主链路必需节点
  - 它继续作为 autoload 保留，只会让“全局双节点”长期存在

本次修改：
- 文件：
  - `project.godot`
- 处理：
  - 移除：
    - `GameSession="*res://AutoLoad/GameSession.cs"`
  - 保留：
    - `GlobalGameSession="*res://Scripts/Battle/Shared/GlobalGameSession.cs"`

结果：
- 当前默认运行时全局会话节点只剩：
  - `GlobalGameSession`
- `GameSession.cs` 文件本体仍保留在工作区
  - 作为兼容储备
  - 但不再默认注入根节点

为什么保留文件但移除 autoload：
- 现在它已经不再是主链路运行时依赖
- 但仍然有价值作为：
  - donor 老接口兼容参考
  - 回溯旧流程的桥接代码
- 因此现阶段不需要直接删文件

验证：
- 执行 `dotnet build`
- 结果：通过，0 warning，0 error

当前结论：
- 当前项目运行时的全局状态真源已经正式收敛为：
  - `GlobalGameSession`
- 从结构上看，之前的“GameSession vs GlobalGameSession”双全局掣肘已经基本解除

下一步建议：
1. 转去做运行时实测，验证 donor 地图 -> 战斗 -> 回图在没有 `GameSession` autoload 的情况下是否完全正常
2. 若实测通过，再考虑是否把 `GameSession.cs` 本体也移入 `Legacy`
