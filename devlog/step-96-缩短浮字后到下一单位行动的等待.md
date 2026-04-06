# Step 96 - 缩短浮字后到下一单位行动的等待

## 日期

2026-03-31

## 目标

在保留“浮字足够清楚、护盾和生命分段明显”的前提下，略微缩短数值跳出后到下一位单位行动的等待时间，减少整体节奏的拖沓感。

## 本次改动

### 1. 缩短浮字序列尾部缓冲

更新：

- `Scripts/Battle/Visual/BattleFloatingTextLayer.cs`

调整：

- `SequenceEndPaddingSeconds`
  - `0.08 -> 0.03`

效果：

- 不再硬等浮字完全淡到最后一丝才继续
- 但仍保留最基础的结尾观察时间

### 2. 行动流程等待时长改为“按浮字时长打折”

更新：

- `Scripts/Battle/Actions/BattleActionService.cs`

新增：

- `ImpactFlowWaitRatio = 0.82`

处理：

- `GetEffectiveImpactPresentationDurationSeconds()` 不再直接全额使用最近一次浮字序列时长
- 改为按 `0.82` 倍进行流程等待

结果：

- 行动推进仍会尊重浮字节奏
- 但不会硬等到浮字接近完全消失

### 3. 玩家和敌人回合推进自动吃到这次缩短

受影响逻辑：

- 玩家行动后进入 turn post phase 的等待
- 敌人 A 行动后切到敌人 B 的等待
- 攻击 / 防御异步动作自己的等待

因为这些逻辑本来就已经统一依赖 `BattleActionService.LastImpactPresentationDurationSeconds`，所以这次不用再分别改三套时序。

## 结果

现在节奏已经从“等浮字几乎播完才继续”调整成：

- 先确保玩家看清主要反馈
- 再略早一点进入下一位单位行动

相比上一版：

- 可读性还在
- 拖沓感下降

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍保留项目历史 nullable warnings，本次未处理
