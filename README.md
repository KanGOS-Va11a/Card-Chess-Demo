# Card-Chess-Demo

当前仓库是一个基于 Godot 4.6 + C# 的独立策略 RPG 原型工程，当前阶段目标是把项目收束为一个“完整区域闭合循环”的高完成度可交付切片。

当前不再默认由助手执行 Godot 运行时测试；运行时内容由开发者本人在编辑器内审查。助手后续只做静态审查、代码修改、资源链检查和 `dotnet build`。

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
- 仅在代码功能发生变化时写 `devlog`
- 纯文档新增、归档、规范整理不写 `devlog`

## 当前文档入口

- `Docs/README.md`
- `Docs/guides/开发规范-现行.md`
- `Docs/guides/文档规范-现行.md`
- `Docs/guides/一周开发计划-2026-04-23.md`
- `Docs/design/打磨阶段开发主任务-2026-04-23.md`

## 参考文档

- `Docs/guides/项目目录结构说明.md`
- `Docs/guides/项目接口文档.md`
- `Docs/guides/战斗对外交互接口方案.md`
- `Docs/guides/战斗机制规则书.md`
- `Docs/guides/卡牌系统与局外构筑README.md`
- `Docs/guides/天赋成长与卡组构筑正式方案.md`
- `Docs/guides/卡组构筑平衡规则方案.md`
- `Docs/设计模板/README.md`
- `Docs/设计示范-标准内容/README.md`
- `Docs/design/场景脚本文稿-序章与世界观整合版.md`
