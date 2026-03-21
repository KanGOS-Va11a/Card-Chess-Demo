# Step 26 - Larger Pixel Font Size For Legibility

## Date

2026-03-21

## Goal

Increase the pixel font size so glyphs remain readable at low resolution without changing the project screen size.

## Implemented

- Increased the project UI default font size from `6` to `8`.
- Increased the battle HUD local font override from `6` to `8`.
- Increased the hover panel local font override from `6` to `8`.
- Increased HUD and hover panel vertical space to better accommodate the larger glyphs.
- Kept the shortened HUD text layout introduced earlier so the larger font still has room to fit.

## Behavior

- The pixel font should now render larger and more legibly.
- The layout still targets `320x180`, so the larger font is balanced by tighter panel text and compact control widths.

## Quick Validation

- Open the battle scene and confirm the font is larger than the previous pass.
- Check that letters are fully formed instead of looking clipped or undersampled.
- Confirm the bottom HUD and hover panel still remain on-screen.

## Result

This step trades some HUD density for legibility, which is the correct direction for a low-resolution pixel UI when the previous size was too small to form glyphs cleanly.
