# Step 27 - 16px Chinese HUD Layout

## Date

2026-03-21

## Goal

Increase the battle UI font to a clearly legible `16px` size without changing the project resolution, and stop relying on overly aggressive shorthand by restructuring the HUD layout for Chinese text.

## Implemented

- Raised the project UI theme default font size to `16`.
- Rebuilt the battle HUD layout from a compressed two-row strip into:
  - a left-side multi-line information column
  - a right-side vertical control column
- Rewrote the battle HUD display text into normal Chinese wording.
- Rewrote the hover unit panel into a multi-line Chinese status card.

## Behavior

- The bottom HUD now has enough vertical space to render larger glyphs.
- Important state text is displayed with more natural Chinese wording instead of extreme abbreviation.
- The hover panel also follows the larger font and multi-line layout.

## Quick Validation

- Run the battle scene and confirm the HUD font is visibly larger than the previous pass.
- Confirm the bottom HUD remains on-screen at `320x180`.
- Confirm the Chinese wording is readable and no longer excessively abbreviated.

## Result

This step changes the battle UI strategy from “compress text harder” to “allocate the layout needed for a 16px pixel font,” which is the correct approach for low-resolution Chinese UI.
