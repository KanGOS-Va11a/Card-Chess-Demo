# Step 51 - 切换主场景到 Scene1

## 日期

2026-03-25

## 目标

将项目默认启动入口切换到 `Scene/Scene1.tscn`。

## 改动

- 修改 `project.godot`
- 将 `run/main_scene` 从原配置改为显式路径：
  - `res://Scene/Scene1.tscn`

## 说明

- 当前仓库里存在场景 UID 复用迹象，单纯依赖 UID 可能导致 Godot 选中错误场景。
- 这次直接写成显式路径，避免主场景解析歧义。

## 结果

- 项目启动时将默认进入 `Scene1`
