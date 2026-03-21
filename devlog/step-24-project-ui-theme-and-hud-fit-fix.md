# Step 24 - Project UI Theme And HUD Fit Fix

## Date

2026-03-21

## Goal

Fix low-resolution HUD clipping, make the battle HUD use the intended pixel font through project-wide theme configuration, and tighten the encounter-driven battle setup flow.

## Implemented

- Added `Resources/UI/ProjectUiTheme.tres`.
- Set project-wide GUI theme in `project.godot`.
- Switched the global project UI font to `Assets/Fonts/unifont_t-17.0.04.otf`.
- Removed local HUD font binding so the battle HUD now follows project theme.
- Reduced battle HUD font sizing and tightened panel margins / spacing.
- Shortened status label text to fit `320x180` more reliably.
- Reduced debug button width and simplified button labels.
- Kept preview path hidden when the hovered cell is outside reachable range.

## Behavior

- The project now has a single intended UI font source instead of only scene-local overrides.
- The battle status bar uses less horizontal space and is less likely to clip at `320x180`.
- Hover status remains available, but the main bottom HUD is less visually crowded.

## Quick Validation

- Run the battle scene and confirm the font now matches the intended project pixel font.
- Confirm the bottom HUD stays fully on-screen at `320x180`.
- Confirm the hover panel still appears on unit hover.
- Confirm path preview only appears inside reachable cells.

## Result

This step moves the battle UI from ad-hoc local font overrides toward a proper project-level font setup and reduces label overflow risk in the low-resolution layout.
