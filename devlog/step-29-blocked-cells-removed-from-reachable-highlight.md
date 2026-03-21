# Step 29 - Blocked Cells Removed From Reachable Highlight

## Date

2026-03-21

## Goal

Stop blocked obstacle cells from appearing in the player's reachable highlight, and extract a reusable occupancy query that later pathfinding and targeting systems can build on.

## Implemented

- Added `BoardQueryService.CanOccupyCell(...)`.
- Updated `BattleSceneController.BuildReachableCells(...)` to filter candidate cells through board occupancy rules instead of only checking Manhattan distance.
- Updated move target validation to use the same filtered reachable-cell list.

## Behavior

- Obstacle cells that reject unit occupancy no longer appear as reachable movement cells.
- Enemy-occupied or otherwise non-stackable destination cells are also excluded from the movement highlight.
- The current movement range is still Manhattan-based; this step fixes destination validity, not full pathfinding.

## Quick Validation

- Run the battle scene and confirm obstacle tiles are no longer highlighted as valid destinations.
- Hover or click cells blocked by obstacles and confirm they are not accepted as reachable movement cells.
- Confirm ordinary open floor cells inside the player's move range are still highlighted.

## Result

This step fixes a visible rules inconsistency and introduces a reusable board-level occupancy query that will be useful when A* pathfinding and straight-line targeting are added next.
