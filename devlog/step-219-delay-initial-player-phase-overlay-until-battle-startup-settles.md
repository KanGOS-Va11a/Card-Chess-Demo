# step-219 delay initial player phase overlay until battle startup settles

date: 2026-04-24

changed:
- `Scripts/Battle/BattleSceneController.cs`
  - added an initial battle phase overlay bootstrap for the opening player turn
  - the first `——玩家行动——` banner now waits for startup to settle before showing
  - if a tutorial popup is already visible at battle start, the phase banner waits until the popup is gone, then applies a short extra delay before appearing

result:
- the opening turn now gets the same phase switch prompt as later turn transitions
- the first banner no longer pops too early during battle scene setup
