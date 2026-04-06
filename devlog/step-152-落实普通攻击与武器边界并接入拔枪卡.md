# step-152 落实普通攻击与武器边界并接入拔枪卡

时间：2026-04-06

本次目标：
- 先完成战斗 P0 里“普通攻击与武器边界做实”的第一步。
- 实现 `拔枪` 卡，让它不再只是文档概念，而是真正影响普通攻击。
- 把 `拔枪` 放进当前 debug 默认牌组，并与 `处决` 靠前放置，方便测试。

本次实际改动：

1. 普通攻击接入更明确的武器结算边界
- 文件：
  - `Scripts/Battle/Stats/PlayerStatResolver.cs`
  - `Scripts/Battle/Shared/GlobalGameSession.cs`
  - `Scripts/Battle/State/BattleObjectStateManager.cs`
  - `Scripts/Battle/Equipment/EquipmentCatalog.cs`
- 内容：
  - `PlayerStatResolver` 现在支持读取武器射程修正，不再只看攻击力。
  - 同时支持传入“战斗内临时武器覆盖 itemId”，用于把战斗中的临时武器和局外正式装备分开。
  - `BattleObjectStateManager` 新增战斗内临时武器覆盖状态：
    - 当前覆盖武器 id
    - 剩余普通攻击次数
  - 玩家普通攻击属性现在按这条链路刷新：
    - 角色基础属性
    - 正式装备修正
    - 战斗内临时武器覆盖
    - 战斗内临时攻击加成（例如荒川强化武器）

2. 实现 `拔枪` 卡
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
  - `Scripts/Battle/Equipment/EquipmentCatalog.cs`
- 内容：
  - 新增 `draw_revolver` 的运行时卡牌定义。
  - 出牌后会给玩家施加一个战斗内临时武器覆盖：`drawn_revolver`。
  - 该临时武器当前规则：
    - 普通攻击射程改为 `2`
    - 普通攻击伤害改为 `4`（通过武器修正落地）
    - 可进行 `6` 次普通攻击
  - 每次成功进行一次普通攻击后，都会消耗一次左轮次数。
  - 次数耗尽后，自动清除临时武器覆盖，恢复原本武器边界。

3. 默认 debug 牌组自动插入 `拔枪`
- 文件：
  - `Scripts/Battle/Shared/GlobalGameSession.cs`
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - 若当前 deck 初始化或已有 debug 牌组中存在 `debug_finisher`，则会自动把 `draw_revolver` 插入到默认牌组前部。
  - 当前策略是让它和 `处决` 都位于调试牌组前段，方便直接测试。
  - 即使资源库 `.tres` 尚未补入该卡，战斗运行时牌库也会补注入 `draw_revolver`，确保当前版本可用。

4. 额外修复
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - 修复了这次改动过程中暴露出来的旧字符串/原型牌组文本损坏问题。
  - 清理并重写了 `BuildPrototypePlayerDeck()` 中损坏的原型卡定义，恢复为可编译状态。

当前结果：
- `拔枪` 已经不是纯文档项，而是可在战斗中使用。
- 普通攻击与武器之间已经具备明确边界，后续继续做：
  - 更多正式武器
  - 战斗内临时换武器技能
  - 装备影响普通攻击风格
  都不需要再推翻这次实现。

验证：
- 执行 `dotnet build`
- 结果：编译通过，0 error

说明：
- 由于当前资源文件编辑在工具层出现过刷新异常，这次采用“运行时代码注入 `拔枪` 进可用牌库和 debug 牌组”的方式先确保功能可用。
- 这不影响当前战斗测试；如果后续要让 `拔枪` 出现在所有局外构筑与资源视图中，再单独补资源表即可。
