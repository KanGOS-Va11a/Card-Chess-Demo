# Step 33 - Player Attack Params Source Fix

## Date

2026-03-21

## Goal

Close the missing source-of-truth link for player attack parameters so the runtime state manager no longer references fields that do not exist on `GlobalGameSession`.

## Implemented

- Added the missing player attack fields to `GlobalGameSession`:
  - `PlayerAttackRange`
  - `PlayerAttackDamage`
- Added those fields to:
  - player snapshot export
  - player snapshot import
- Updated docs so `GlobalGameSession` now documents the attack fields as part of the actual player runtime source.

## Behavior

- Player attack range and attack damage now have a clear runtime source.
- `BattleObjectStateManager` can safely mirror attack values from `GlobalGameSession` without compile errors.

## Quick Validation

- Run `dotnet build` and confirm there are no missing-member errors for `PlayerAttackRange` / `PlayerAttackDamage`.
- Open `GlobalGameSession.cs` and confirm the attack fields are present beside the other player runtime fields.

## Result

This step fixes a broken system connection: player attack behavior now has a proper global runtime source instead of an incomplete mirror path.
