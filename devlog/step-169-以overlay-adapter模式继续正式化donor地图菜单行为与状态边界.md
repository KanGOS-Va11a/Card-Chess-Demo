# step-169 以 overlay adapter 模式继续正式化 donor 地图菜单行为与状态边界

时间：2026-04-07

本次目标：
- 继续 donor 地图中的菜单布局细修与 battle / 菜单 / 地图状态边界修正
- 但不再持续直接深改 `SystemFeatureLabController.cs`

为什么调整实现策略：
- `SystemFeatureLabController.cs` 文件体量很大
- 历史上它就存在编码污染风险
- 这一轮尝试在控制器本体中继续塞 donor 地图菜单专属逻辑时，曾引发一次大范围字符串损坏
- 虽然已回退恢复，但说明这份文件不适合继续作为“高频修改入口”

因此本次改为：
- 把 donor 地图专属的菜单细修逻辑放到独立适配层
- 由 overlay adapter 补行为，而不是继续侵入式修改大控制器

本次新增文件：
- `Scripts/Map/UI/SystemFeatureOverlayAdapter.cs`

本次结构调整：
- `Scene/UI/SystemFeatureOverlay.tscn`
  - 已新增 `OverlayAdapter` 子节点
  - 该节点挂载 `SystemFeatureOverlayAdapter.cs`

adapter 当前承担的职责：

1. donor 地图专属菜单静态文本修正
- 顶部提示
- 默认状态行
- Tab 标题

2. donor 地图菜单状态行修正
- 基于 `PlayerPath` 和 `InteractionArea` 重新计算当前可交互对象
- 菜单打开时显示“按 C 或 Esc 关闭”
- 菜单关闭时显示“靠近可交互对象后按 E 交互”

3. donor 地图菜单关闭边界
- 支持 `Esc` 关闭菜单
- 点击遮罩层可关闭菜单

4. battle / 菜单状态边界
- 若 `GlobalGameSession.PendingBattleRequest` 已存在且菜单仍开着，则自动关闭菜单
- 避免进入战斗切换前菜单仍悬挂在 donor 地图状态里

5. 菜单关闭时恢复玩家输入
- adapter 自己负责同步：
  - `PhysicsProcess`
  - `Process`
  - `ProcessInput`
  - `ProcessUnhandledInput`

本次还做了什么：
- `SystemFeatureLabController.cs` 已再次恢复为 donor 稳定版本
- 当前 donor 地图菜单行为不再依赖继续深改该大控制器

验证：
- 执行 `dotnet build`
- 结果：通过，0 error

当前结论：
- donor 地图上的 `C` 键菜单已经进入“正式 overlay + adapter 细修”阶段
- 后续如果要继续调菜单行为、状态行、关闭逻辑或 donor 地图专属提示，应优先改：
  - `Scene/UI/SystemFeatureOverlay.tscn`
  - `Scripts/Map/UI/SystemFeatureOverlayAdapter.cs`

下一步建议：
1. 进入 donor 地图实际验证：
  - `C` 开关菜单
  - `Esc` 关闭
  - 点击遮罩关闭
  - 打开菜单后地图移动是否锁住
2. 若以上通过，再继续微调菜单窗口宽高与各页布局
3. 最后再继续压细 map / menu / battle 状态衔接
