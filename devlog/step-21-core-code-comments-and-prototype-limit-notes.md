# Step 21 - Core Code Comments And Prototype Limit Notes

## Date

2026-03-21

## Goal

Add clear comments at the important runtime code locations so the current project flow is easier to onboard into, and mark prototype-only behavior or unimplemented systems directly in code.

## Implemented

- Added high-signal comments to the main battle runtime controller:
  - startup order
  - per-frame state sync intent
  - turn advancement behavior
  - camera framing intent
  - prototype reachability and preview-path limitations
- Added comments to board/runtime services:
  - `BoardInitializer`
  - `BoardQueryService`
  - `OccupancyRules`
  - `BattleObjectStateManager`
  - `TurnActionState`
- Added comments to presentation and UI code:
  - `BattleAnimatedViewBase`
  - `BattlePieceViewManager`
  - `BattlePlayerView`
  - `BattleHudController`
  - `GlobalGameSession`
- Documented unimplemented or prototype-only behavior inline, including:
  - no map-battle loop
  - no formal combat pipeline
  - no real defend resolution
  - no action queue or enemy turn flow
  - simplified move range and preview path logic
  - HUD still being debug-oriented rather than final battle UI

## Behavior

- Important classes now explain what layer they belong to and why they exist.
- Prototype shortcuts are marked where they happen, instead of being left implicit.
- Future work boundaries are easier to spot directly in code while reading.

## Quick Validation

- Open the battle controller, board services, state sync layer, and HUD files and confirm the main flow can be followed through comments.
- Confirm the notes about unimplemented systems match the current runtime behavior.
- Run `dotnet build` to ensure comment-only edits did not affect compilation.

## Result

This step improves maintainability and onboarding by making the current architecture and prototype limits visible right where developers read the code.
