# Step 11 - Battle Test Camera

## Date

2026-03-20

## Goal

给战斗测试场景增加一个默认摄像机，让当前 16x8、16x16 像素棋盘在 320x180 视口里能居中并放大查看。

## Change

- 在 `Scene/Battle/Battle.tscn` 下新增 `Camera2D`
- 位置设置为视口中心 `Vector2(160, 90)`
- `zoom` 设置为 `Vector2(0.85, 0.85)`
- `enabled = true`

## Result

- 运行项目时会直接看到居中的战斗测试场景
- 当前棋盘会比无摄像机时稍微放大，更适合测试 TileMap、标记层和悬停高亮
- 后续如果要接地图快照背景或战斗转场，这个 Camera2D 也可以继续保留作为战斗专用镜头
