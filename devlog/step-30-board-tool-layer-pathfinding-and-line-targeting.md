# Step 30 - Board Tool Layer Pathfinding And Line Targeting

## Date

2026-03-21

## Goal

Add reusable board-level tools for later card targeting and enemy AI, while keeping the current gameplay integration limited to movement/highlight/path preview.

## Implemented

- Added `BoardTopology` utility support for:
  - cardinal directions
  - cardinal neighbor enumeration
  - cardinal direction normalization
- Added `BoardPathfinder`
  - reachable-cell cost search
  - A* path query
- Added `BoardTargetingService`
  - straight-line enemy query by direction
  - four-direction convenience query
- Added `BoardQueryService.CanOccupyCell(...)`.
- Updated `BattleSceneController` to create:
  - `Pathfinder`
  - `TargetingService`
- Updated reachable highlight and preview path to use the pathfinding tool layer instead of ad-hoc Manhattan-only logic.

## Behavior

- Reachable cells are now derived from path query results rather than raw distance checks.
- Preview paths are now generated through A*.
- Straight-line enemy detection now exists as a reusable board tool, but it is not yet wired into attacks, cards, or enemy logic.

## Quick Validation

- Run the battle scene and confirm obstacle or blocked cells are excluded from reachable highlights.
- Hover reachable cells and confirm the preview path follows valid board traversal.
- Confirm the project still builds cleanly after the new board tools are added.

## Result

This step establishes the first real board utility layer that future card targeting, enemy AI, and stricter movement rules can reuse instead of duplicating logic inside scene controllers.
