# step-158 显式拆分击杀特效来源并关闭冲撞击退式击杀

时间：2026-04-06

本次目标：
- 明确区分“哪些击杀会触发击退式击杀特效，哪些不会”
- 修正地形击杀仍会出现击退的问题
- 按最新要求关闭 `冲撞` 造成的击退式击杀特效

本次实际改动：

1. 击杀特效来源改为显式布尔开关
- 文件：
  - `Scripts/Battle/Actions/BattleActionService.cs`
- 内容：
  - `ApplyDamageToTarget(...)` 增加 `allowKillKnockback`
  - 不再依赖“击杀方向是否为零”去隐式判断
  - 现在是显式决定：
    - 是否允许击退式击杀
    - 是否只做原地白化碎裂

2. 主角普通攻击 / 敌人攻击 / 地形击杀分流
- 文件：
  - `Scripts/Battle/Actions/BattleActionService.cs`
  - `Scripts/Battle/AI/EnemyTurnResolver.cs`
  - `Scripts/Battle/BattleSceneController.cs`
  - `Scripts/Battle/Presentation/BattleAnimatedViewBase.cs`
- 当前规则：
  - 主角普通攻击：允许击退式击杀
  - 主角普通攻击类直伤卡：允许击退式击杀
  - 敌人攻击：不允许击退式击杀
  - 电弧 / 火焰地形：不允许击退式击杀
  - `BattleAnimatedViewBase` 在不允许击退时，会原地白化碎裂，不做位移

3. `冲撞` 的伤害全部关闭击退式击杀
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - `冲撞` 的：
    - 本体 `3` 点伤害
    - 目标撞击追加 `4` 点伤害
    - 被撞对象追加 `4` 点伤害
  - 现在都显式传入 `allowKillKnockback: false`
- 结果：
  - `冲撞` 仍然保留逻辑位移和碰撞伤害
  - 但如果冲撞本身把敌人打死，只会原地白化碎裂，不会触发击退式击杀

验证：
- 执行 `dotnet build`
- 结果：编译通过，0 error
