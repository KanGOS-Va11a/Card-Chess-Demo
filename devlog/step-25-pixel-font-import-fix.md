# Step 25 - Pixel Font Import Fix

## Date

2026-03-21

## Goal

Fix the blurry project UI font by correcting the font import settings instead of treating it as a generic texture filtering problem.

## Implemented

- Updated the import settings for:
  - `Assets/Fonts/unifont_t-17.0.04.otf`
  - `Assets/Fonts/unifont-17.0.04.otf`
- Changed the font import configuration to better suit pixel-style UI rendering:
  - disabled antialiasing
  - re-enabled embedded bitmaps
  - disabled hinting
  - disabled subpixel positioning

## Behavior

- The project UI font should now render with crisper edges after Godot reimports the font assets.
- This change targets font rasterization itself; it is not a canvas texture filter fix.

## Quick Validation

- Reopen or reload the project so Godot reimports the font assets.
- Check the battle HUD and hover panel text at `320x180`.
- Confirm the font looks sharp rather than soft/blurred.

## Result

This step fixes the actual cause of the blurry font: unsuitable dynamic font import settings for a pixel-oriented UI font.
