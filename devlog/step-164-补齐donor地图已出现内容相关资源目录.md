# step-164 补齐 donor 地图已出现内容相关资源目录

时间：2026-04-07

本次目标：
- 把当前 donor 地图里已经出现，或者相关 prefab 已经明确在地图侧使用的资源目录继续补充 merge 到主项目
- 避免出现“场景文件已经引用，但工作目录里其实没有资源目录”的空心状态

本次检查结果：

1. 地图场景与角色场景的直接引用已整理
- `Scene01~Scene06` 主要引用：
  - `ArtResource/resource/地图/*`
  - `CosmicLegacy_PetricakeGamesPNG.png`
  - `Scene/Character/MainPlayer.tscn`
  - `Scene/Character/Enemy.tscn`
  - `Scene/InteractableItem/Chest.tscn`

2. 进一步检查角色场景时发现的资源空洞
- `Scene/Character/MainPlayer.tscn` 已引用：
  - `Assets/Character/MainPlayer/Idle/*`
  - `Assets/Character/MainPlayer/Walk/*`
- `Scene/Character/Enemy.tscn` 已引用：
  - `Assets/Character/EnemyGif/frames/*`
- 但工作目录里此前缺少：
  - `Assets/Character/MainPlayer`
  - `Assets/Character/EnemyGif`
  - `Assets/TutorialResource`

这意味着地图逻辑虽然已经迁进来了，但资源层并不完整。

本次实际补充 merge 的资源：

1. 主角地图动作资源
- 迁入：
  - `Assets/Character/MainPlayer/*`
- 内容包含：
  - Idle 方向图
  - Walk 方向图
  - 主角阴影资源

2. 地图敌人动态图资源
- 迁入：
  - `Assets/Character/EnemyGif/*`
- 内容包含：
  - `enemy_preview.png`
  - `frames/enemy_000.png`
  - `frames/enemy_001.png`
  - `frames/enemy_002.png`

3. 地图通用动作资源补充
- 迁入：
  - `Assets/Character/Walk/*`
  - `Assets/Character/Shadow.png`

4. 教程相关资源
- 迁入：
  - `Assets/TutorialResource/*`
- 内容包含：
  - `enemy_cowboy.png`
  - `heal_station_icon.png`
  - `tile_01.png`
  - `tile_10.png`

本次没有动的内容：
- 没有去覆盖 battle 专用角色资源目录
- 没有改地图逻辑
- 没有强行替换回血站 prefab 视觉，因为 donor 当前 prefab 本身仍旧使用老的 `icon.svg`

验证：
- 检查关键引用路径：
  - `res://Assets/Character/MainPlayer/Idle/Idle_Down.png`
  - `res://Assets/Character/MainPlayer/Idle/Idle_Up.png`
  - `res://Assets/Character/EnemyGif/frames/enemy_000.png`
  - `res://Assets/TutorialResource/enemy_cowboy.png`
  - 结果：均已存在
- 执行 `dotnet build`
  - 结果：通过，0 warning，0 error

当前收益：
- donor 地图目前已经出现的角色与教程内容，其相关资源目录在主项目中已经补齐
- 后续把 `SystemFeatureLabController` 或新版地图 UI 接回 donor 地图时，不需要再为这些基础资源回补一次

下一步建议：
1. 开始把 `SystemFeatureLabController` 的功能拆到 donor 新版地图
2. 在 donor 新版地图里重新规划 `C` 键综合菜单的挂载与显示范围
3. 最后统一修 map -> battle -> map 的衔接细节
