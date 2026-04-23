# step-204 fix map walk animation reset between grid steps

date: 2026-04-23

changed:
- `Scripts/Map/Actors/Player.cs`
  - restored continuous map walk animation playback during chained grid movement
  - while the player is inside the movement branch, animation now stays on the current walk direction instead of switching to `idle` on each tile boundary frame

result:
- holding movement across multiple cells no longer resets the walk animation every tile
- story trigger settle and battle return settle behavior from step-203 remain intact because forced `idle` still comes from `SettleToWorldPosition()`
