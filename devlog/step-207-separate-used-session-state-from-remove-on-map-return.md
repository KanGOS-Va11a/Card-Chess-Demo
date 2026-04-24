# step-207 separate used session state from remove-on-map-return

date: 2026-04-24

changed:
- `Scripts/Map/Interaction/InteractableTemplate.cs`
  - added default runtime snapshot flags:
    - `disable_when_session_used`
    - `remove_when_session_used`
- `Scripts/Map/Interaction/Enemy.cs`
  - marked enemies as `disable_when_session_used = true`
  - marked enemies as `remove_when_session_used = true`
- `Scripts/Map/Interaction/BattleEncounterEnemy.cs`
  - marked battle encounter enemies as `disable_when_session_used = true`
  - marked battle encounter enemies as `remove_when_session_used = true`
- `Scripts/Map/Interaction/StoryTriggerZone.cs`
  - marked story trigger zones as `disable_when_session_used = true`
  - marked story trigger zones as `remove_when_session_used = true`
- `Scripts/Map/Controllers/MapSceneController.cs`
  - `ApplyUsedInteractableRemovals()` no longer removes every object found in `UsedInteractables`
  - map return now only disables/removes interactables that explicitly opt into session-used removal behavior

result:
- chests and cabinets that only use `UsedInteractables` to remember opened state are no longer removed or disabled after battle return
- enemies and one-shot story triggers still keep their intended persistent removal behavior on map reload
- map interaction highlight and prompt behavior for already-opened containers can continue to work after returning from battle
