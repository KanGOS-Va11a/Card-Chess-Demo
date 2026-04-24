# step-221 block turn progress on phase banner and fix battle move facing

date: 2026-04-24

changed:
- `Scripts/UI/AreaTitleOverlay.cs`
  - added a `PlaybackFinished` signal so callers can wait for the overlay to fully finish before continuing game flow
- `Scripts/Battle/BattleSceneController.cs`
  - battle now treats phase banners as blocking transitions
  - enemy turns do not begin until `——敌方行动——` has fully faded out
  - player turns do not reopen until `——玩家行动——` has fully faded out
  - initial battle start banner also locks battle input during its startup delay and playback
  - cleared battle interaction input while phase transition overlays are active
- `Scripts/Battle/Presentation/BattlePieceViewManager.cs`
  - battle movement now faces the next step direction before replaying the move animation for that step

result:
- phase switch text now behaves like a real transition instead of an overlapping prompt
- players cannot act before the phase banner fully disappears
- battle movement no longer looks like units are walking one tile with the previous direction animation
