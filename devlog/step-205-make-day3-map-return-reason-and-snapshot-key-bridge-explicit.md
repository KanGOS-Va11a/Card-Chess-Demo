# step-205 make day3 map return reason and snapshot key bridge explicit

date: 2026-04-24

changed:
- `Scripts/Battle/Boundary/MapReturnReason.cs`
  - added explicit map return reason enum for battle-to-map resume flows
- `Scripts/Battle/Boundary/MapResumeContext.cs`
  - added `ReturnReason`
  - added `WithReturnReason()` so battle resolution can promote a pending resume context into a concrete victory or retreat return
- `Scripts/Battle/Shared/GlobalGameSession.cs`
  - battle completion now stamps the pending map resume context with an explicit return reason
  - map resume snapshot mutation now resolves node paths through the runtime snapshot key bridge before editing snapshot entries
- `Scripts/Map/Transitions/MapRuntimeSnapshotHelper.cs`
  - added reserved snapshot metadata key `__node_path_index`
  - capture now records a `node path -> runtime state key` bridge
  - apply and lookup paths now resolve interactables by either runtime state key or scene-relative node path
- `Scripts/Map/Controllers/MapSceneController.cs`
  - battle return log now prints explicit return reason
  - used interactable replay now resolves both runtime state keys and node paths
- `Scripts/Map/Controllers/MazeEnemySpawnController.cs`
  - battle return handling now prefers explicit resume return reason, with battle result as fallback

result:
- battle victory and battle retreat are no longer inferred only from scattered implicit state when returning to the map
- map runtime snapshot edits made through source interactable node paths now land on the correct runtime snapshot entry
- one-shot map objects that persist by runtime state key, such as grid-authored chests, now use the same lookup path as path-based battle cleanup
