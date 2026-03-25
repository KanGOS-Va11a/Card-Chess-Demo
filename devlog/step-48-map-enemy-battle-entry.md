# Step 48 - Map Enemy Battle Entry

## Date

2026-03-25

## Goal

Add a real map-side enemy interaction object that enters battle through the existing encounter -> enemy type -> room pool -> room selection chain, instead of relying only on a generic battle door.

## Implemented

- Added `MapBattleTransitionHelper` to centralize the shared map-side battle entry flow:
  - build `BattleRequest`
  - store pending encounter id
  - store `MapResumeContext`
  - change to the battle scene
- Updated `SceneDoor` to reuse the same shared helper for battle-entry mode.
- Added `BattleEncounterEnemy` as a dedicated interactable map enemy script.
- Added `Scene/BattleEncounterEnemy.tscn` as a reusable map enemy scene.
- Configured the new map enemy to use the existing:
  - `grunt_debug` encounter id
  - `grunt` enemy type path already present in the encounter library
  - existing battle room pool and debug room resources selected by `BattleSceneController`
- Placed one interactable battle enemy instance into `Scene/Mainlevel.tscn` for direct testing.

## Behavior

- Interacting with the new map enemy now enters battle through the same encounter-driven room-selection path as the current battle prototype.
- The enemy object is now a clearer test entry than using a door for all battle starts.
- `SceneDoor` and map enemies no longer duplicate the low-level request / resume setup logic.

## Quick Validation

- Run `dotnet build` and confirm the project compiles cleanly.
- Start `Mainlevel`, interact with the new battle enemy, and confirm battle starts.
- Confirm the selected battle still uses the existing encounter resource and room-pool selection chain.
- Lose the battle and confirm the map return flow still restores the player position correctly.

## Result

This step adds the first dedicated map enemy battle-entry object and wires it into the existing encounter and room-pool flow, which makes the map-side gameplay loop closer to the intended “touch enemy -> resolve encounter -> return to map” structure without introducing a separate duplicate battle bootstrap path.
