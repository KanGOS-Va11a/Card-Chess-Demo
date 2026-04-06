# Step 100 - 加入战斗相机边缘推镜与 Y 键重置

## 日期

2026-04-01

## 目标

为当前 battle 场景加入最小可用的相机移动系统，满足：

- 鼠标移到屏幕边缘时，相机向该方向轻微移动
- 不改变当前缩放倍率
- 相机移动受边界框限制
- 不允许棋盘超过一半内容离开屏幕
- 按 `Y` 可重置相机到初始 battle 位置
- 如果当前处于 battle 运镜中，则不允许 `Y` 立即重置

## 本次改动

### 1. 扩展 battle 相机导出参数

更新：

- `Scripts/Battle/BattleSceneController.cs`

新增导出参数：

- `CameraEdgePanMarginPixels`
- `CameraResetDurationMs`
- `CameraPanPixelsPerSecond`
- `CameraMinBoardVisibleRatio`

作用：

- 边缘触发区宽度
- 重置回位时长
- 推镜速度
- 最少保持多少比例的棋盘仍在屏幕里

### 2. battle 相机初始化时缓存“休息位”和移动边界

更新：

- `Scripts/Battle/BattleSceneController.cs`

新增内部状态：

- `_battleCamera`
- `_cameraRestPosition`
- `_cameraPanBounds`

处理：

- `ConfigureCameraForBattle()` 在计算初始相机位置后
  - 记录初始 battle 相机位置为休息位
  - 根据当前棋盘尺寸、viewport 尺寸和缩放倍率计算允许平移的边界框

当前边界规则：

- 基于 `CameraMinBoardVisibleRatio`
- 保证棋盘至少保留一半内容仍在可见区域内

### 3. `_Process()` 中加入鼠标边缘推镜

更新：

- `Scripts/Battle/BattleSceneController.cs`

新增：

- `UpdateBattleCameraPan(double delta)`

行为：

- 鼠标靠近左 / 右 / 上 / 下边缘时，计算一个 0~1 的推动系数
- 相机按这个方向持续小幅移动
- 最终位置会被钳制到 `_cameraPanBounds` 内

因此：

- 不会无限把棋盘拖出屏幕
- 只是做一个有限度的“观察式平移”

### 4. `Y` 键重置相机

更新：

- `Scripts/Battle/BattleSceneController.cs`

新增：

- `TryResetBattleCamera()`

行为：

- 按 `Y` 后，将 battle 相机 tween 回 `_cameraRestPosition`
- 使用 `CameraResetDurationMs` 控制回位时长

### 5. 为后续焦点运镜预留锁定机制

更新：

- `Scripts/Battle/BattleSceneController.cs`

新增：

- `_cameraCinematicTween`
- `_isCameraCinematicBusy`
- `PlayBattleCameraFocusAsync(...)`

当前说明：

- 这条方法是为后续攻击 / 技能焦点镜头预留的正式入口
- 目前还没有主动在 battle 行为中触发
- 但重置逻辑已经会在 `_isCameraCinematicBusy` 时拒绝 `Y` 键回位

也就是说：

- 现在已经把“运镜时锁住手动重置”的规则提前搭好了

## 结果

当前 battle 相机已具备最小可用的观察能力：

- 不改缩放
- 鼠标边缘轻推
- 有边界框
- 支持 `Y` 回中
- 为后续焦点运镜保留正式接口和锁定规则

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍保留项目历史 nullable warnings，本次未处理
