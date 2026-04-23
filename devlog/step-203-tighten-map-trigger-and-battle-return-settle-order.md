# step-203 tighten map trigger and battle return settle order

date: 2026-04-23

changed:
- `Scripts/Map/Actors/Player.cs`
  - added `IsGridMoving` so map-side trigger code can wait for a fully settled player
  - added `GetStableGridPosition()` to expose the stable grid destination for external systems
  - added `SettleToWorldPosition()` to place the player while also clearing movement state, velocity, and animation
  - fixed `_PhysicsProcess()` animation timing so a move that ends this frame switches to `idle` immediately
- `Scripts/Map/Interaction/StoryTriggerZone.cs`
  - grid trigger zones now wait until the player is no longer moving between cells
  - before dialogue starts, the player is settled to the stable grid position and forced back to `idle`
- `Scripts/Map/Controllers/MapSceneController.cs`
  - added unified `PlacePlayer()` placement path
  - battle return, scene spawn, and pending restore now all use the unified placement path instead of writing `GlobalPosition` directly
- `Scripts/Map/Transitions/MapBattleTransitionHelper.cs`
  - battle entry now stores the player's stable grid position as the map return point
- `Scripts/Map/Interaction/SceneDoor.cs`
  - in-scene teleports now also use the unified player settle path

result:
- entering a story-trigger cell now finishes the step first, then returns the player to `idle`, then opens dialogue
- battle return now places the player back on a stable grid position instead of a half-cell position
- direct external player placement paths are now centralized, which reduces repeat bugs of "position updated but movement state not cleared"
