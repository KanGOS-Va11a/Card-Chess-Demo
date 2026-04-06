# step-154 接入火焰地形并在 roomB 放置默认火焰格

时间：2026-04-06

本次目标：
- 增加第二种危险地形：火焰地形
- 当前仅在 `roomB` 的空白格子上放置若干火焰格
- 视觉先使用橙色高亮，不加特效
- 规则改为：单位在自己回合结束时若仍停留在火焰格上，则受到 `3` 点伤害

本次实际改动：

1. 房间布局支持火焰地形
- 文件：
  - `Scripts/Battle/Data/RoomLayoutDefinition.cs`
  - `Scripts/Battle/Rooms/BattleRoomTemplate.cs`
  - `Scripts/Battle/Board/BoardInitializer.cs`
- 内容：
  - 增加 `FireTerrainCells`
  - 战斗初始化时把对应格子的 `TerrainId` 写成 `fire`

2. roomB 放置默认火焰格
- 文件：
  - `Scene/Battle/Rooms/GruntDebugRoomB.tscn`
- 当前放置坐标：
  - `(3, 1)`
  - `(7, 2)`
  - `(11, 5)`
- 这几格当前都选在 roomB 的空白位置，避免直接压住默认单位和障碍物

3. 棋盘覆盖层增加火焰高亮
- 文件：
  - `Scripts/Battle/Visual/BattleBoardOverlay.cs`
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - 增加 `FireTerrainColor`
  - 增加火焰格绘制列表与橙色高亮显示

4. 火焰地形规则接入回合流
- 文件：
  - `Scripts/Battle/Actions/BattleActionService.cs`
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - 增加：
    - `FireTerrainId = "fire"`
    - `FireTerrainDamage = 3`
  - 新增 `ResolveTurnEnd(...)`
  - 在玩家回合结束、敌方回合结束时，结算该阵营单位是否停留在火焰格上
  - 若停留，则造成 `3` 点伤害
  - 火焰伤害也复用当前伤害飘字与动作日志

当前规则和电弧地形的区别：
- 电弧地形：
  - 进入 / 离开 / 停留都会受伤
- 火焰地形：
  - 只在回合结束时，若仍停留其上才受伤

验证：
- 执行 `dotnet build`
- 结果：编译通过，0 error
