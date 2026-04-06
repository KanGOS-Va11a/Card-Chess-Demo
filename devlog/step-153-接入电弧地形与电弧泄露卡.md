# step-153 接入电弧地形与电弧泄露卡

时间：2026-04-06

本次目标：
- 接入战斗中的电弧地形规则。
- 实现设计文档中的“创造电弧地形”卡牌。
- 先补最小可玩视觉：棋盘上以蓝色高亮显示电弧格。

本次实际改动：

1. 棋盘支持电弧地形
- 文件：
  - `Scripts/Battle/Board/BoardState.cs`
  - `Scripts/Battle/Data/RoomLayoutDefinition.cs`
  - `Scripts/Battle/Board/BoardInitializer.cs`
  - `Scripts/Battle/Rooms/BattleRoomTemplate.cs`
- 内容：
  - 为房间布局增加 `ArcTerrainCells` 数据。
  - 初始化棋盘时，会把对应格子的 `TerrainId` 标成 `arc`。
  - `BoardState` 增加 `SetTerrain(...)`，用于战斗内动态改地形。

2. 电弧地形正式参与战斗规则
- 文件：
  - `Scripts/Battle/Actions/BattleActionService.cs`
- 内容：
  - 新增常量：
    - `ArcTerrainId = "arc"`
    - `ArcTerrainDamage = 2`
  - 单位移动时：
    - 离开电弧格会受一次伤害
    - 进入电弧格会再受一次伤害
  - 单位回合开始时：
    - 若停留在电弧格上，再受一次伤害
  - 电弧伤害复用现有伤害结算与飘字系统，不新开另一套逻辑。

3. 新增可制造电弧地形的卡牌 `电弧泄露`
- 文件：
  - `Scripts/Battle/Cards/BattleCardDefinition.cs`
  - `Scripts/Battle/BattleSceneController.cs`
  - `Scripts/Battle/Shared/GlobalGameSession.cs`
- 内容：
  - 新增地格目标类型 `BattleCardTargetingMode.Cell`。
  - 新增运行时卡牌 `card_arc_leak`：
    - 名称：`电弧泄露`
    - 费用：`1`
    - 射程：`3`
    - 目标：地格
    - 效果：对目标格及其上下左右相邻格施加电弧地形
  - 当前 debug 默认牌组中，只要存在 `debug_finisher`，就会自动插入 `card_arc_leak`，方便测试。

4. 棋盘可视化增加电弧高亮
- 文件：
  - `Scripts/Battle/Visual/BattleBoardOverlay.cs`
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - `BattleBoardOverlay` 新增 `ArcTerrainColor`
  - 新增 `SetArcTerrainCells(...)`
  - 当前会把所有 `TerrainId == "arc"` 的格子绘制为蓝色半透明高亮边框
  - 暂时不加粒子、电流闪烁或 shader 特效

当前实现范围说明：
- 这次做的是“规则版电弧地形”，不是完整编辑器版。
- 现在已经可以：
  - 通过卡牌制造电弧格
  - 在棋盘上看到蓝色高亮
  - 进入 / 离开 / 停留其上时受伤
- 还没有做：
  - 更复杂的电弧特效
  - 纯机械敌人额外伤害
  - 通过房间编辑层批量绘制电弧地形的正式工作流

验证：
- 执行 `dotnet build`
- 结果：编译通过，0 error
