# Step 53 - 切回 Battle 主场景并补对外交互接口方案

## 日期

2026-03-25

## 目标

从这一刻开始，暂时只专注我们自己的战斗链路与对外接口，不再继续追着朋友的地图实现做兼容。

## 改动

### 运行入口

- 修改 `project.godot`
- 将主场景切换为：
  - `res://Scene/Battle/Battle.tscn`

### 全局依赖

- 在 `project.godot` 中重新加入：
  - `GlobalGameSession="*res://Scripts/Battle/Shared/GlobalGameSession.cs"`

原因是我们的 `BattleSceneController` 启动时依赖 `/root/GlobalGameSession`。

### 文档

- 新增 `Docs/战斗对外交互接口方案.md`
- 用于固定战斗与地图、全局成长之间的边界职责和推荐数据结构

## 当前策略

后续一段时间内，优先保证：

- battle 内部交互可持续迭代
- battle 对外输入输出接口稳定
- 地图和成长系统只通过标准边界对接

不再为了兼容朋友当前的中间态实现，频繁把 battle 主链改来改去。

## 结果

- 默认启动将进入我们的 battle 场景
- 我们的 battle 主链重新具备所需的全局会话依赖
- 地图 / battle / 成长三方的对接方案已有明确文档底稿
