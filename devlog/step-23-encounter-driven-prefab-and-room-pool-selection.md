# Step 23 - Encounter Driven Prefab And Room Pool Selection

## Date

2026-03-21

## Goal

Tighten the relationship between enemy prefab selection and room-pool selection so battle setup can be driven by an encounter id instead of hardcoded debug assumptions.

## Implemented

- Added `BattleEncounterProfile` and `BattleEncounterLibrary`.
- `BattleSceneController` now resolves:
  - `EncounterId`
  - enemy type ids
  - primary enemy definition id
- Battle room pool expansion now actually uses `BattleRoomPoolEntry.EnemyTypeId` as a filter.
- `BattleRoomTemplate.BuildLayoutDefinition(...)` now accepts the resolved enemy definition id.
- Enemy spawn markers in the room template now instantiate the resolved enemy definition instead of always forcing `battle_enemy`.
- Added `Resources/Battle/Encounters/DebugBattleEncounterLibrary.tres`.
- Wired `Scene/Battle/Battle.tscn` to use the debug encounter library and `grunt_debug`.
- Expanded the debug grunt room pool to include both debug grunt rooms.

## UI Changes

- Switched battle HUD to the newer pixel font in `Assets/Fonts/unifont-17.0.04.otf`.
- Reduced HUD font sizing and tightened panel spacing for `320x180`.
- Preview path now hides when the mouse is outside the player's reachable range.

## Behavior

- The current battle test scene is no longer tied only to a hardcoded enemy prefab assumption.
- One encounter id now controls:
  - which enemy prefab definition gets spawned into enemy markers
  - which room-pool entries are considered valid
- Room presets remain responsible for layout and enemy slot positions, not for choosing the encounter id itself.

## Quick Validation

- Run the battle scene and confirm `grunt_debug` still loads a valid grunt room.
- Move the mouse outside reachable cells and confirm the preview path disappears.
- Confirm the HUD fits better at low resolution with the updated font and spacing.
- Change `EncounterId` in `Battle.tscn` to another future profile and confirm battle setup pivots through the new encounter data instead of direct prefab edits.

## Result

This step makes the battle side much closer to the intended map-to-battle flow: the map only needs to decide which encounter id to launch, while the battle scene resolves enemy prefab and room selection from data.
