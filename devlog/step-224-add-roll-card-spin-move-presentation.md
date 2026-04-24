# step-224 add roll card spin move presentation

date: 2026-04-24

changed:
- `Scripts/Battle/Presentation/BattleAnimatedViewBase.cs`
  - added a reusable move tween that rotates the sprite one full turn while traveling to a target board position
  - the spin temporarily recenters the sprite around its visual center so the rotation reads as a true self-spin instead of an orbit
- `Scripts/Battle/Presentation/BattlePieceViewManager.cs`
  - added a dedicated `PlayRollMoveAsync(...)` presentation path for roll movement
- `Scripts/Battle/BattleSceneController.cs`
  - `翻滚` no longer uses the default instant move presentation
  - roll now updates board state first, then plays a locked presentation task:
    - move to target cell
    - rotate sprite one full turn
    - resync board presentation only after the roll animation finishes
  - deferred the usual immediate state-to-view sync for `card_roll` so the custom animation is not overwritten by a teleport

result:
- `翻滚` now reads as a distinct action instead of a normal step move
- the player sprite spins once around its own center while traveling to the destination cell
- battle input stays locked during the roll presentation, so quick follow-up actions do not cut the animation off
