# Step 22 - Hovered Unit Status Panel

## Date

2026-03-21

## Goal

Show a compact status UI when the mouse hovers a unit on the battle board, and verify that the enemy prefab path is already ready for test content.

## Implemented

- Added hovered-unit lookup in `BattleSceneController`.
- Wired the hovered unit state into `BattleHudController`.
- Added a compact `HoverPanel` to `Scene/Battle/UI/BattleHud.tscn`.
- The hover panel now shows:
  - unit display name
  - role / faction
  - HP
  - move points
  - cell position
  - current animation name

## Enemy Prefab Status

- The enemy prefab path is already wired and usable:
  - `DefaultBattlePrefabLibrary.tres`
  - `Scene/Battle/Tiles/BattleEnemyToken.tscn`
  - `Scripts/Battle/Presentation/BattleEnemyView.cs`
- No extra enemy-prefab-side script work was required for the hover-status feature.

## Behavior

- Moving the mouse over any unit cell now opens the hover status panel.
- Moving off a unit or off the board hides the panel.
- The panel supports both player and enemy units because it reads shared `BattleObjectState`.

## Quick Validation

- Run the battle scene and hover the player.
- Hover each enemy and confirm the panel updates per unit.
- Move a unit and confirm its cell value and animation text update in the hover panel.
- Move the cursor away from units and confirm the panel hides.

## Result

This step makes it much easier to inspect unit runtime state in the prototype without clicking or opening debug logs, and it confirms that the current enemy prefab pipeline is already ready for more test samples.
