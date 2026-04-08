# step-163 统一补挂 donor 地图基础控制层并确认暂不接入 MapFlow 系统

时间：2026-04-07

本次目标：
- 继续推进 donor 地图控制 / UI / 演出效果接线
- 先处理 donor 地图真正实际使用到的基础控制层
- 不把 donor 工程里“存在但尚未落地”的流程系统硬塞进新版地图

本次检查结论：

1. donor 地图里真正已经在用的控制层
- `MapResolutionController`
- `MapSceneController`
- `Scene01TutorialController`
- `PlayerVisionMaskController`
- `WallLayerCollisionBuilder`

2. donor 工程里虽然有，但 donor 地图场景并没有实际挂载使用的内容
- `MapFlowController`
- `FlowNodeGate`
- `HubEntrySavePoint`

这意味着当前正确做法不是把 `MapFlow` 先行接进新版地图，而是先把 donor 地图真正依赖的基础控制器统一挂上。

本次修改内容：

1. 六张 donor 地图统一补挂 `MapResolutionController`
- 文件：
  - `Scene/Maps/Scene01.tscn`
  - `Scene/Maps/Scene02.tscn`
  - `Scene/Maps/Scene03.tscn`
  - `Scene/Maps/Scene04.tscn`
  - `Scene/Maps/Scene05.tscn`
  - `Scene/Maps/Scene06.tscn`
- 目的：
  - 统一 donor 地图探索时的逻辑分辨率
  - 后续切图时不用再靠单张图临时修窗口尺度

2. 给尚未具备恢复入口的 donor 地图补上 `MapSceneController`
- 文件：
  - `Scene/Maps/Scene03.tscn`
  - `Scene/Maps/Scene04.tscn`
  - `Scene/Maps/Scene05.tscn`
  - `Scene/Maps/Scene06.tscn`
- 另外：
  - `Scene01.tscn` 上一轮已经补过 `MapSceneController`
  - `Scene02.tscn` 原本 root 已挂 `MapSceneController`，这轮只补 `MapResolutionController`
- 目的：
  - 让新版 donor 地图统一具备战后回图恢复入口
  - 避免只有个别地图能正确恢复玩家位置与交互状态

3. 明确本阶段不把 `MapFlow` 接入 donor 地图
- 原因：
  - donor 的 `Scene01~Scene06` 里目前并没有实际挂 `MapFlowController / FlowNodeGate / HubEntrySavePoint`
  - 提前硬接只会制造额外分叉，不是真正尊重 donor 地图
- 处理方式：
  - 脚本保留
  - 等后续 demo 流程需要时，再按真实地图节点去落

对后续的意义：
- donor 新版地图现在已经有了统一的基础地图控制层
- 这样下一步把 `SystemFeatureLabController` 的内容迁到新版地图时，就不需要再顺手修一遍分辨率和回图恢复
- 也避免把 donor 尚未正式采用的流程系统过早绑死

验证：
- 执行 `dotnet build`
- 结果：通过，0 warning，0 error

下一步建议：
1. 开始把 `SystemFeatureLabController` 的功能拆成适合 donor 新版地图的接入方式
2. 优先规划它在 `Scene01` 或单独测试地图中的挂载结构
3. 完成后再集中修正 battle 与地图的最终衔接
