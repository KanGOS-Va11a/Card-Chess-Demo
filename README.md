# Card-Chess-Demo

当前仓库是一个基于 Godot 4.6 + C# 的原型工程，包含两条并行但已接上最小边界的玩法链路：

- 地图探索与交互原型
- 90 度方格战斗原型

目前默认主入口是 `Scene/Mainlevel.tscn`，可直接在地图中测试：

- 玩家移动与交互扇形判定
- NPC / 宝箱 / 治疗站 / 门 / 地图敌人
- 地图进入战斗
- 战斗失败后返回原地图位置

## 目录约定

- `Scripts/Battle`：战斗域脚本
- `Scripts/Map`：地图域脚本
- `Scene/Battle`：战斗场景
- `Scene`：地图与基础原型场景
- `Resources/Battle`：战斗资源
- `Docs`：项目文档
- `devlog`：开发变更记录

## 当前维护约定

- 地图侧新增脚本统一放入 `Scripts/Map/...`
- 战斗侧新增脚本统一放入 `Scripts/Battle/...`
- 文档统一更新到 `Docs`
- 从本次整理开始，后续变更统一以中文记录到 `devlog`

## 参考文档

- `Docs/项目目录结构说明.md`
- `Docs/项目总体架构设计.md`
- `Docs/项目接口文档.md`
- `Docs/项目总体需求表.md`
