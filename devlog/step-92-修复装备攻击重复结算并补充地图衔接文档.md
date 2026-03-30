# Step 92 - 修复装备攻击重复结算并补充地图衔接文档

## 日期

2026-03-30

## 目标

修复装备武器后玩家攻击在 battle 中被重复结算的问题，并把上一次 `GlobalGameSession` 分层改动正式补写到文档里，尤其说明它对 map / battle 衔接的实际影响。

## 本次修复

更新：

- `Scripts/Battle/Shared/GlobalGameSession.cs`

### 问题原因

`BuildPlayerSnapshot()` 输出的是解析后攻击值，而 `ApplyPlayerSnapshot()` 又把它按基础攻击写回 session，随后 resolver 再把装备加成叠一次，导致：

- 玩家攻击加成重复结算
- 打敌人时护甲和生命伤害都被异常放大

### 修复方式

`BuildPlayerSnapshot()` 现在同时输出：

- 基础字段
  - `base_max_hp`
  - `base_move_points_per_turn`
  - `base_attack_damage`
  - `base_defense_damage_reduction_percent`
  - `base_defense_shield_gain`
- 解析字段
  - `max_hp`
  - `move_points_per_turn`
  - `attack_damage`

`ApplyPlayerSnapshot()` 现在会：

- 优先回写 base 字段
- 只有在旧快照缺失 base 字段时，才回退兼容旧字段

这样 battle request / battle result / save 的回写语义就被修正了。

## 文档补充

新增交接补充：

- `Docs/交接记录/2026-03-30-角色状态与装备接口同步/04-快照语义与地图衔接补充.md`

内容重点：

- 解释这次重复结算 bug 的真实根因
- 明确快照里“基础字段”和“解析字段”的区别
- 明确地图层为什么不需要跟着大改
- 明确 merge 时最该检查的 battle / map 边界文件

## 结果

当前 battle 与 map 的接口结构没有被推翻，但 battle 内部对玩家数值快照的恢复语义已经更正确：

- 地图层仍旧按原边界进入 battle
- battle 层不再把解析后攻击写回成基础攻击
- 装备与天赋加成不会再在这条链路里重复结算

## 验证

- `dotnet build`
  - 结果：`0 errors`
  - 仍保留项目历史 nullable warnings，本次未处理
