# step-178 整理 Day 1 详细实现清单并对齐设计 wiki 条目

时间：2026-04-07

本次目标：
- 在正式进入 Day 1 开发前，先把当天范围收紧成可执行清单
- 明确今天究竟实现设计 wiki 里的哪些卡牌、敌人、装备和状态影响
- 避免今天开发过程中继续扩散到学习闭环、Boss、音频、存档等非今日重点

本次新增文档：
- `Docs/design/Day1详细实现清单-2026-04-07.md`

本次整理出的 Day 1 必做范围：

1. 卡牌
- `card_stance`
- `card_heavy_blow`
- `card_quick_shot`
- `card_concussion_shot`
- `card_weathering`

2. 敌人
- `pirate_blocker`
- `pirate_scout`
- `pirate_shocker`
- 若进度允许，再补 `pirate_gunner`

3. 装备
- `equip_old_coat`
- `equip_magnetic_scabbard`
- `equip_phase_boots`
- 若进度允许，再补 `equip_red_scarf`
- 若进度允许，再补 `equip_target_lens`

4. 状态边界
- 普通攻击受武器影响
- 防御减伤比例受天赋影响
- 移动力受装备或天赋影响
- `拔枪` 临时武器覆盖链路不被破坏

明确延后到后续日期的内容：
- `学习` 机制完整闭环
- 精英 / Boss 学习奖励
- Boss 战正式实现
- 完整战后结算深化
- 音频系统
- 存档系统深化

这份清单的作用：
- 作为 Day 1 的正式执行边界
- 让后续实现时能逐项勾掉，不会继续被新想法冲散
