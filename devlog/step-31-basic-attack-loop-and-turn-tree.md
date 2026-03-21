# Step 31 - Basic Attack Loop And Turn Tree

## Date

2026-03-21

## Goal

Add a minimal playable attack loop with turn-state-tree support, while keeping enemy AI and enemy actions out of scope for now.

## Implemented

- Expanded `TurnActionState` into a minimal turn-state tree:
  - `PlayerMove`
  - `PlayerAction`
  - `TurnPost`
  - `EnemyTurn`
- Added attack-targeting input mode.
- Added configurable attack stats to runtime state:
  - attack range
  - attack damage
- Added an `Atk` button to the fixed HUD.
- Implemented player basic attack flow:
  - enter attack targeting
  - click enemy in Manhattan range
  - apply damage immediately
  - remove target on zero HP
- Updated runtime sync so removed units also disappear from:
  - state manager
  - piece views
- Kept an explicit enemy-turn phase in the loop, but left its action body empty.

## Behavior

- The player can skip movement and attack directly.
- Moving ends the move phase and enters the remaining player action phase.
- Attacking ends the player action phase and advances into turn-post processing.
- Turn-post processing advances through the empty enemy-turn placeholder and then starts the next turn.

## Quick Validation

- Start the battle scene and click `Atk`.
- Click an enemy within Manhattan range and confirm damage is applied.
- Continue attacking until the enemy reaches zero HP and confirm it disappears.
- Confirm the next turn begins automatically after the player action resolves.

## Result

This step moves the prototype from “movement-only turn test” to a minimal but real combat loop with attack resolution, death cleanup, and a usable turn-state-tree foundation for later card actions and enemy AI.
