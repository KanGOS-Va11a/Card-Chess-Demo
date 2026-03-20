# Step 10 - Battle Test Entry As Main Scene

## Date

2026-03-20

## Goal

将项目主场景切到战斗测试入口，并隔离当前非战斗脚本，避免地图侧合并中的错误阻塞战斗开发。

## Changes

- `project.godot` 的 `run/main_scene` 已切到 `Scene/Battle/Battle.tscn`
- `newproject.csproj` 已改为：
  - 仅编译 `Scripts/Battle/**/*.cs`
  - 排除 `Scripts` 根目录旧探索脚本
  - 排除 `Card-Chess-Demo-main/**/*.cs`

## Result

- 现在项目启动直接进入战斗测试场景
- 朋友那边地图脚本即便继续处于整理中，也不会影响当前 battle 域的编译和调试
- 本轮完成后 `dotnet build` 结果为 `0 warnings / 0 errors`
