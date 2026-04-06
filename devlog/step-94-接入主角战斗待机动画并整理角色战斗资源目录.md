# Step 94 - 接入主角战斗待机动画并整理角色战斗资源目录

## 日期

2026-03-31

## 目标

把新的主角 battle idle sprite sheet 接入战斗玩家待机表现，同时整理角色战斗资源目录，并让大体型角色在 16x16 棋盘格上按“下半身留在格内、上半身可交叠”的方式显示。

## 本次资源整理

新增目录：

- `Assets/Character/Battle/Player/Idle`

迁移资源：

- `Idle_Right_Down-Sheet-sheet_alt.png`
- 配套 `.import`

当前资源归位后，世界地图原型仍继续使用：

- `Assets/Character/Idle/idle_001.png`
- `Assets/Character/Idle/idle_002.png`

因此这次整理是“把 battle 主角专用待机资源从通用 idle 目录拆出来”，而不是把全部角色资源都强行挪走。

## 本次改动

### 1. battle 玩家视图支持真实 sprite sheet

更新：

- `Scripts/Battle/Presentation/BattlePlayerView.cs`

处理：

- 新增导出字段：
  - `IdleSpriteSheet`
  - `FrameWidth`
  - `FrameHeight`
  - `FrameCount`
  - `CellFootOffsetY`
  - `SpriteDrawOffset`
- 当 `IdleSpriteSheet` 可用时，玩家不再使用 fallback 纯色方块帧
- 改为从 7 帧 `48x64` sheet 动态切出动画帧
- 为：
  - `idle`
  - `move`
  - `action`
  - `hit`
  - `defend`
  - `defeat`
  生成最小可运行动画集合

说明：

- 当前真正有美术意义的是 `idle`
- 其他动画先复用同一张 sheet，保证 battle 逻辑切换动画名时不会掉回纯方块

### 2. battle 视图基类增加朝向与底脚点机制

更新：

- `Scripts/Battle/Presentation/BattleAnimatedViewBase.cs`

处理：

- 新增 `FaceDirection(Vector2 direction)`
- 通过 `AnimatedSprite2D.FlipH` 实现左右镜像
- 新增 `ConfigureAnimatedSprite(...)`
- 新增 `GetBoardAnchorOffset()`
- 视图最终位置改为：
  - 棋盘锚点
  - + 底脚点偏移
  - + 动作位移偏移

这样大体型角色不再被强行以“中心点贴格”方式摆放。

### 3. 主角动作时根据左右方向镜像

更新：

- `Scripts/Battle/Presentation/BattlePieceViewManager.cs`

处理：

- 移动路径播放时，根据每一步的 X 方向更新朝向
- 攻击动作播放时，根据攻击方向更新朝向

当前规则：

- 向右动作：保持原图方向
- 向左动作：镜像反转
- 纯上下动作：保持上一次左右朝向

### 4. battle 场景开启 Y Sort

更新：

- `Scene/Battle/Battle.tscn`
- `Scene/Battle/SystemFeatureLabBattle.tscn`

处理：

- `PieceRoot` 开启 `y_sort_enabled = true`

意义：

- 玩家角色原点现在更接近脚底
- 因此和其他棋盘对象的上下遮挡会更自然

### 5. 玩家 prefab 绑定新资源

更新：

- `Scene/Battle/Tiles/BattlePlayerToken.tscn`

处理：

- 绑定新的 idle sheet 资源路径
- 设置：
  - `FrameWidth = 48`
  - `FrameHeight = 64`
  - `FrameCount = 7`
  - `CellFootOffsetY = 8`
  - `SpriteDrawOffset = (-24, -48)`

当前这组参数的含义是：

- battle view 的原点放在格子底边附近
- sprite 本体向上抬起显示
- 角色下半身进入格子，上半身可以超出格子并参与 Y 排序

## 结果

现在 battle 中的玩家待机表现已经不再是默认占位方块，而是会读取你提供的 7 帧 idle sheet。

同时，battle 玩家显示的空间语义也已经更符合大体型角色在 16x16 格上的表现方式：

- 脚下落在格子内
- 上半身可以和周围对象正常交叠
- 左右动作时会自动镜像切换方向

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍保留项目历史 nullable warnings，本次未处理

## 后续建议

- 如果后续补上：
  - 左 / 右独立动作帧
  - 攻击专用动作 sheet
  - 受击 / 防御专用动作 sheet

则 `BattlePlayerView` 当前结构可以继续直接扩展，不需要再重做 battle 表现层架构。
