# step-184 为当前行动敌人添加红色描边高亮

时间：2026-04-08

本次目标：
- 给当前正在行动的敌人增加明确的红色边框视觉提示
- 不新增额外 UI，直接挂在单位表现层上

本次修改：
- `Scripts/Battle/Presentation/BattleAnimatedViewBase.cs`
- `Scripts/Battle/Presentation/BattlePieceViewManager.cs`
- `Scripts/Battle/AI/EnemyTurnResolver.cs`
- `Scripts/Battle/BattleSceneController.cs`

实现方式：
1. 在单位表现基类中新增 `Line2D` 描边
- 默认隐藏
- 当前行动敌人时显示红色描边

2. 在 `BattlePieceViewManager` 中新增活动单位切换接口
- 切换时会自动清掉上一个单位的描边

3. 在 `EnemyTurnResolver` 中接入当前行动敌人的回调
- 每个敌人开始行动时高亮
- 行动结束后取消

当前预期：
- 敌人回合中，当前执行移动 / 攻击的敌人会有红色边框
- 便于玩家快速读懂是谁在行动
