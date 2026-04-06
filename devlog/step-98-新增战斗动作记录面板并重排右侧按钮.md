# Step 98 - 新增战斗动作记录面板并重排右侧按钮

## 日期

2026-03-31

## 目标

在 battle HUD 中补一个简单但可用的动作记录面板，并把当前挤在右侧的一排按钮重排成更紧凑的双列布局，避免遮挡手牌区域。

## 本次改动

### 1. 新增动作记录面板

更新：

- `Scene/Battle/UI/BattleHud.tscn`
- `Scripts/Battle/UI/BattleHudController.cs`

新增内容：

- `ActionLogButton`
- `ActionLogDismissButton`
- `ActionLogPopup`
- `ActionLogPopup/Margin/VBox/Header/TitleLabel`
- `ActionLogPopup/Margin/VBox/Header/CloseButton`
- `ActionLogPopup/Margin/VBox/BodyScroll/BodyText`

行为：

- 右侧新增 `Log` 按钮
- 点击后打开动作记录面板
- 点击空白遮罩可关闭
- 点击单独的 `关闭` 按钮也可关闭
- 面板正文为可滚动样式

### 2. 动作记录显示当前回合与上一回合

更新：

- `Scripts/Battle/UI/BattleHudController.cs`
- `Scripts/Battle/BattleSceneController.cs`

处理：

- HUD 新增：
  - `SetActionLogState(...)`
- controller 维护：
  - 当前回合动作列表
  - 上一回合动作列表
- 每当回合推进到下一回合时：
  - 当前回合动作转存为上一回合
  - 新回合动作列表清空

当前展示结构：

- 本回合 `Tn`
- 上一回合 `Tn-1`

### 3. 动作记录来源接入 battle 逻辑层

更新：

- `Scripts/Battle/Actions/BattleActionService.cs`
- `Scripts/Battle/BattleSceneController.cs`

处理：

- `BattleActionService` 新增：
  - `ActionLogged` 事件
- 目前会自动记录：
  - 普通攻击
  - 移动

示例格式：

- `Player->Enemy 攻击3`
- `Enemy->(6,4) 移动`

- `BattleSceneController` 另外补记：
  - 冥想
  - 防御
  - 逃跑
  - 荒川造墙
  - 荒川强化
  - 卡牌造成的攻击 / 护盾 / 治疗

示例格式：

- `Player->Player 冥想`
- `Player->Player 护盾2`
- `Player->Enemy 攻击4`
- `Player->Player 治疗3`
- `荒川->(5,3) 造墙`

### 4. 右侧按钮改为双列紧凑布局

更新：

- `Scene/Battle/UI/BattleHud.tscn`

处理：

- `RightControls` 从竖排 `VBoxContainer`
  - 改成 `GridContainer`
  - `columns = 2`
- 右侧按钮全部缩小：
  - 宽度缩到 `32`
  - 字号改为 `14`
- 新增 `Log` 按钮也纳入右侧双列

结果：

- 右侧操作区整体更紧凑
- 更不容易压到下方手牌区域

### 5. 统一瞬时弹窗关闭逻辑

更新：

- `Scripts/Battle/UI/BattleHudController.cs`

处理：

- 新增 `CloseTransientPanels()`
- 在攻击、防御、冥想、撤退、回合结束、荒川轮盘等操作前统一关闭：
  - 牌堆弹窗
  - 动作记录弹窗

避免多个弹层互相遮挡。

## 结果

当前 battle HUD 已经具备一个最小可用的战斗日志面板：

- 能查看本回合和上一回合的动作
- 能滚动
- 能点击空白或关闭按钮关闭

同时右侧操作按钮已经改成双列紧凑布局，比之前更不容易挡住手牌。

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍保留项目历史 nullable warnings，本次未处理
