# step-160 整理地图层项目合并方案文档

时间：2026-04-06

本次目标：
- 对 `Merge/Card-Chess-Demo` 中的地图层工程进行结构审查
- 给出后续如何和当前主工程进行 merge 的实际方案
- 将方案整理成独立文档，放入 `Docs/merge`

新增文件：
- `Docs/merge/地图层项目合并方案-2026-04-06.md`

文档内容重点：
- 审查当前主工程和 merge 工程的目录结构差异
- 识别高风险模块：
  - `project.godot`
  - `AutoLoad/GameSession.cs`
  - `AutoLoad/GlobalBattleContext.cs`
  - 对方 `Scripts/Core/BattleRequest.cs` / `BattleResult.cs`
- 识别必须手工合并的 `Scripts/Map` 分叉文件
- 识别可以优先迁入的地图资源、视觉和教程模块
- 给出推荐的三步 merge 路线：
  1. 先迁独有地图资源和独有脚本
  2. 再手工合并地图核心脚本
  3. 最后再把地图接回 battle

本次结论摘要：
- 当前不适合做整工程覆盖式 merge
- 最合理的方案是：
  - 以当前工程为主干
  - 把对方项目当作地图资源与演出模块 donor
  - battle 真源、全局单例、battle boundary 继续保留当前主干版本
