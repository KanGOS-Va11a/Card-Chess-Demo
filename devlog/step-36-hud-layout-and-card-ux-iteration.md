# Step 36 - HUD Layout And Card UX Iteration

## Date

2026-03-22

## Goal

Make the battle HUD more testable at `320x180`, reduce card/board overlap, and tighten the card interaction flow around selection, cancellation, and pile inspection.

## Implemented

- Reworked the HUD layout so it is split into:
  - a compact top status strip
  - a right-side debug/tool button column
  - a bottom hand strip
- Replaced the old hand-button layout with tighter parallel card stacking.
- Added hover card descriptions through the custom hover panel instead of relying on built-in tooltips.
- Added draw / discard / exhaust pile inspection buttons and a popup list view.
- Fixed card-target cancel behavior so cancelling an uncommitted selection can return to the move phase.
- Added card-use fly-out animation and reduced bottom-hand visual blocking.

## Behavior

- Selecting a card now lifts it and marks it as the active selection.
- Clicking a non-target card a second time resolves it.
- Cancelling selection before moving returns the player to `PlayerMove`.
- Card hover text now comes from one custom panel rather than a duplicated tooltip path.

## Quick Validation

- Run `dotnet build` and confirm the HUD scripts compile cleanly.
- Start the battle scene and confirm the hand is visible along the bottom edge.
- Confirm the right-side buttons are clickable and no longer overlap with the hand.
- Select and cancel a card before moving and confirm the turn returns to `PlayerMove`.

## Result

This step turns the HUD from a temporary prototype strip into a more controlled battle test UI that fits the project resolution and keeps card interactions readable.
