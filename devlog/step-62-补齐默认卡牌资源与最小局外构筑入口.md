# Step 62 - 补齐默认卡牌资源与最小局外构筑入口

## 日期

2026-03-26

## 目标

落实默认卡牌资源，避免 battle 在未显式配置卡牌资源时因为空引用或空牌组崩溃；同时提供一个最小但可运行的局外构筑入口。

## 本次改动

### 1. 默认资源

新增：

- `Resources/Battle/Cards/DefaultBattleCardLibrary.tres`
- `Resources/Battle/Cards/DefaultBattleDeckBuildRules.tres`

内容包括：

- 当前原型牌组对应的卡牌模板
- 默认 starter deck 复制数
- 默认构筑规则：
  - 最少牌数
  - 最多牌数
  - 构筑点数预算
  - 同名卡上限

### 2. 默认资源回退加载

扩展：

- `Scripts/Battle/BattleSceneController.cs`

现在 battle 在 `_Ready()` 中会：

- 自动加载默认卡牌库
- 自动加载默认构筑规则
- 在 session 牌组为空时自动初始化 starter deck

### 3. 最小局外构筑入口

新增：

- `Scripts/Battle/UI/BattleDeckBuilderController.cs`
- `Scene/DeckBuilder.tscn`

当前最小构筑入口已支持：

- 查看当前可选牌库
- 查看当前已选牌组
- 查看单卡详情
- 添加卡牌
- 移除卡牌
- 恢复当前会话中的构筑
- 重置为默认 starter deck
- 按规则校验当前构筑
- 保存到 `GlobalGameSession`

### 4. 卡牌与构筑 README

新增：

- `Docs/卡牌系统与局外构筑README.md`

内容包括：

- 当前卡牌链路回顾
- 如何新增模板卡
- 如何做范围治疗卡
- 如何和成长系统挂钩
- 如何做点数预算 / 牌数上下限 / 同名卡限制

## 结果

当前就算没有额外配置卡牌资源，battle 主链也能自动回退到默认卡牌库和默认构筑规则，不会因为缺资源直接失效。

同时已经有了一个独立的、局外的最小构筑入口场景，后续可以继续在这个基础上补完整构筑 UI。

## 验证

- 已执行 `dotnet build`
- 构建通过，`0` 错误
