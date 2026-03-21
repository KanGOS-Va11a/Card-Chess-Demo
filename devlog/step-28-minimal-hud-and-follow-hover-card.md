# Step 28 - Minimal HUD And Follow Hover Card

## Date

2026-03-21

## Goal

Remove the oversized bottom HUD panel, keep only the end-turn button, and move the unit status display to a compact hover card that follows the hovered grid area without going off-screen.

## Implemented

- Deleted the large bottom battle status panel layout.
- Kept only one fixed battle UI control:
  - `结束 / 下一`
- Rebuilt the hover unit status display as a small floating card.
- Hover card now follows the hovered tile area through screen-space positioning near the cursor.
- Added screen-edge clamping so the hover card does not render outside the visible viewport.

## Behavior

- The board is less obstructed because the persistent bottom HUD panel is gone.
- The only fixed UI element is the end-turn button.
- Unit state is now contextual instead of always occupying screen space.

## Quick Validation

- Run the battle scene and confirm only the end-turn button remains fixed on screen.
- Hover units near each edge of the board and confirm the status card stays fully visible.
- Move the cursor off a unit and confirm the hover card hides.

## Result

This step changes the battle UI from a persistent dashboard to a minimal fixed control plus contextual hover information, which is a better fit for the limited `320x180` screen space.
