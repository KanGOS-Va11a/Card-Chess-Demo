# step-155 实现冲撞卡并加入 debug 初始手牌

时间：2026-04-06

本次目标：
- 实现设计 wiki 中的 `冲撞` 卡牌
- 让它像当前测试用的 `电弧泄露` 一样，直接进入 debug 开局手牌，方便第一回合测试

本次实际改动：

1. 新增运行时卡牌 `冲撞`
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - 新增 `RamCardId = "card_ram"`
  - 当前运行时定义：
    - 名称：`冲撞`
    - 费用：`1`
    - 目标：直线敌方目标
    - 射程：`2`
    - 描述：直线 `2` 格内冲到目标面前，造成 `3` 伤害；若目标背后受阻，再追加 `4` 点撞击伤害

2. 实现 `冲撞` 特殊效果
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - `TryResolveSpecialCardEffect(...)` 现在支持 `card_ram`
  - 新增 `TryApplyRamCard(...)`
  - 当前最小可玩实现逻辑：
    - 只能选直线上的目标
    - 玩家会先移动到目标面前一格
    - 对目标造成 `3` 点伤害
    - 若目标背后是边界或不可占据格，则再追加 `4` 点撞击伤害
  - 当前版本没有做真正的逻辑位移击退，而是先按“碰撞追加伤害”把玩法做实

3. 加入 debug 运行时牌库
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - 在当前运行时牌库注入逻辑中，把 `冲撞` 和 `拔枪`、`电弧泄露` 一起补进可用牌库
  - 即使资源表里暂时没有这张牌，当前战斗测试也能用

4. 强制加入 debug 开局手牌
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - 只要当前 buildCards 中包含 `debug_finisher`
  - 就会确保以下牌进入 `StartingHandCards`
    - `debug_finisher`
    - `draw_revolver`
    - `card_arc_leak`
    - `card_ram`
  - 同时把剩余牌重新放回 `StartingDrawPileCards`
  - 并将 `openingDrawCount` 设为 `0`
  - 这样不会只剩测试手牌而丢失整副牌组

当前结果：
- `冲撞` 已经可以在战斗中使用
- 当前 debug 开局第一手就能直接测试：
  - `处决`
  - `拔枪`
  - `电弧泄露`
  - `冲撞`

验证：
- 执行 `dotnet build`
- 结果：编译通过，0 error

说明：
- 这次 `冲撞` 先按“最小可玩版本”实现，没有单独重构完整逻辑击退系统
- 后续如果要做真正的“目标被击退 2 格、途中撞单位/障碍/边界的全逻辑处理”，再在当前版本基础上扩展即可
