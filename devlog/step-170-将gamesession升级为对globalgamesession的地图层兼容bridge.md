# step-170 将 GameSession 升级为对 GlobalGameSession 的地图层兼容 bridge

时间：2026-04-07

本次目标：
- 开始处理 donor 地图层对接中的核心冲突
- 解决 donor 地图脚本依赖 `GameSession`，而当前 battle 主干依赖 `GlobalGameSession` 的双真源问题

问题背景：
- donor 地图层中，以下内容仍在直接读取 `/root/GameSession`：
  - `MapFlowController`
  - `HubEntrySavePoint`
  - `HealStation`
  - 旧地图侧 Player / InventoryPanel 等脚本
- 但当前 battle 主干的真实全局状态已经在：
  - `GlobalGameSession`

如果不处理：
- donor 地图层会继续读写一套和 battle 脱节的旧数据
- battle 结束后、地图恢复后、背包与生命能量等状态会越来越容易分裂

本次方案：
- 不改 donor 地图层脚本对 `GameSession` 的接口依赖
- 直接把 `AutoLoad/GameSession.cs` 升级为 facade / bridge
- 让它继续暴露 donor 熟悉的 snake_case 契约
- 但底层同步到 `GlobalGameSession`

本次修改文件：
- `AutoLoad/GameSession.cs`

本次桥接的内容：

1. donor 契约字段继续保留
- `session_id`
- `current_map_id`
- `current_map_spawn_id`
- `player_runtime`
- `deck_state`
- `inventory_state`
- `arakawa_state`
- `suitcase_state`
- `world_flags`
- `cleared_encounters`
- `used_interactables`

2. 从 `GlobalGameSession` 镜像回 donor facade 的内容
- 玩家当前生命
- 玩家最大生命
- 牌组卡牌列表
- 背包物品数量
- 荒川当前能量 / 上限
- 荒川成长等级
- 荒川解锁列表

3. donor facade 主动写回 `GlobalGameSession` 的内容
- `apply_resource_delta("player_hp", ...)`
- `apply_resource_delta("arakawa_energy", ...)`
- `apply_inventory_delta(...)`
- `start_new_session(...)` 的基础重置同步

4. 仍旧只保留在 donor facade 本地的数据
- `world_flags`
- `scan_risk`
- `cleared_encounters`
- `used_interactables`
- `suitcase_state`
- donor 老式的战后位置恢复字段

这样处理的原因：
- 这些内容目前主要还是 donor 地图层在用
- 不需要为了现阶段 demo 去把所有 donor 世界状态直接塞进 `GlobalGameSession`
- 但生命、能量、背包、牌组这些与 battle 已经强相关的状态，必须开始统一

结果：
- donor 地图层仍可继续使用 `/root/GameSession`
- battle 主干仍继续使用 `/root/GlobalGameSession`
- 两者的关系从“冲突双真源”收敛成：
  - `GlobalGameSession` = battle 真源
  - `GameSession` = donor 地图层兼容 facade

验证：
- 执行 `dotnet build`
- 结果：通过，0 error

下一步建议：
1. 继续检查 donor 地图 flow / 存档点 / HealStation 对 `GameSession` 的剩余读取是否还缺桥接字段
2. 再处理 `GlobalBattleContext` / donor Core-BattleRequest 这类已经次一级的冲突
