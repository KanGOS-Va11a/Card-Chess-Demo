# step-214 fix learning reward display unlock persistence and menu input lock

date: 2026-04-24

changed:
- `Scripts/Battle/BattleSceneController.cs`
  - battle result summary now resolves learned card reward ids to localized card display names instead of showing raw english card ids
- `Scripts/Map/UI/SystemFeatureLabController.cs`
  - talent recompute no longer overwrites `UnlockedCardIds` with talent-only unlocks
  - learned cards and other previously unlocked cards are now preserved when talent state is recomputed
  - player resolution for menu input lock now falls back to finding the actual player node in the current scene if `PlayerPath` is empty
- `Scripts/Map/UI/SystemFeatureOverlayAdapter.cs`
  - menu input lock now resolves the player with the same fallback strategy, so opening the C menu correctly disables map movement even when overlay instances do not carry an explicit `PlayerPath`
- `Scripts/Map/UI/MapTextBlocker.cs`
  - blocking text detection now uses `MapDialogueService.IsDialogueVisible(...)` instead of only the blocking counter, so story dialogue panels also prevent the C menu from opening reliably

result:
- learned card rewards now appear with proper chinese card names on the battle result screen
- learned cards now remain actually unlocked for codex and deck availability after returning to map and recomputing talent state
- story dialogue panels now correctly block C-menu opening
- once the C menu is open, map player movement is correctly disabled even during talent-page viewport navigation
