# step-220 tune battle phase overlay position and font size

date: 2026-04-24

changed:
- `Scripts/UI/AreaTitleOverlay.cs`
  - added an optional `VerticalOffsetPixels` export so overlays can shift up or down without changing the shared scene layout
- `Scripts/Battle/BattleSceneController.cs`
  - battle phase overlays now use 16 px font size
  - battle phase overlays now shift upward slightly to sit closer to the visual center

result:
- battle side-transition text is smaller and visually more centered
- map area-title overlays keep their existing default placement unless explicitly configured
