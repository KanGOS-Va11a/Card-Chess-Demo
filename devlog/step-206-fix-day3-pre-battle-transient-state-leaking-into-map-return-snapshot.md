# step-206 fix day3 pre-battle transient state leaking into map return snapshot

date: 2026-04-24

changed:
- `Scripts/Map/Transitions/MapRuntimeSnapshotHelper.cs`
  - added `UpsertInteractableSnapshot()` so a map interactable can push its final persistent runtime state back into the pending battle return snapshot after battle transition has already been started
- `Scripts/Map/Interaction/BattleEncounterEnemy.cs`
  - moved `IsDisabled = true` to after successful `TryEnterBattle()`
  - this prevents the temporary pre-battle disabled state from being captured into the battle return snapshot on retreat
- `Scripts/Map/Interaction/StoryTriggerZone.cs`
  - when a trigger becomes permanently disabled or removed during a battle-starting flow, it now syncs its latest snapshot back into the pending battle return snapshot
  - direct `StartBattleOnComplete` triggers now also register self cleanup for retreat when `RemoveSelfOnBattleRetreat` is enabled

result:
- battle encounter enemies no longer come back from retreat already disabled just because they were temporarily locked during transition
- story trigger zones no longer lose their post-trigger disabled state when battle return replays an earlier snapshot
- direct battle-start story zones now honor retreat cleanup consistently instead of only the trigger-interactable branch doing so
