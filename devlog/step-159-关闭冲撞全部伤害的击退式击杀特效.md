# step-159 关闭冲撞全部伤害的击退式击杀特效

时间：2026-04-06

本次目标：
- 按最新要求，把 `冲撞` 的所有伤害来源都改成不触发击退式击杀特效。

本次实际改动：
- 文件：
  - `Scripts/Battle/BattleSceneController.cs`
- 内容：
  - `冲撞` 本体 `3` 点伤害改为 `allowKillKnockback: false`
  - `冲撞` 目标撞击追加 `4` 点伤害改为 `allowKillKnockback: false`
  - `冲撞` 被撞对象追加 `4` 点伤害改为 `allowKillKnockback: false`

当前结果：
- `冲撞` 仍然保留：
  - 主角补位
  - 目标逻辑击退
  - 碰撞双方伤害
- 但如果敌人是被 `冲撞` 或其碰撞伤害打死，只会原地白化碎裂，不会再触发击退式击杀。

验证：
- 执行 `dotnet build`
- 结果：编译通过，0 error
