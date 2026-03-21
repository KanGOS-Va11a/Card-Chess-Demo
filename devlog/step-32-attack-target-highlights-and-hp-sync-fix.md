# Step 32 - Attack Target Highlights And HP Sync Fix

## Date

2026-03-21

## Goal

Make basic attack targeting easier to read, and fix the incorrect initial enemy HP display that showed `0/MaxHp` at battle start.

## Implemented

- Fixed `BattleObjectStateManager` HP sync so units without explicit spawned HP no longer get overwritten to `0` during state synchronization.
- Added attack target highlight support to `BattleBoardOverlay`.
- During attack-targeting mode:
  - movement highlight is hidden
  - valid enemy targets inside attack range are highlighted

## Behavior

- Enemies now start with the expected full HP from prefab/default values when the room data does not explicitly override HP.
- Entering attack mode gives a clearer red target highlight instead of leaving the board in movement-highlight mode.

## Quick Validation

- Start a battle and confirm enemy hover cards show full HP instead of `0/MaxHp`.
- Click `Atk` and confirm enemy targets in range are highlighted.
- Attack an enemy and confirm HP updates immediately.

## Result

This step fixes a state sync bug that made enemies look dead on spawn, and it improves attack readability by giving attack mode its own target highlight pass.
