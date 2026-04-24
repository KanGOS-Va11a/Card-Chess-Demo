# step-210 add saved deck revision to ui and battle init chain

date: 2026-04-24

changed:
- `Scripts/Battle/Shared/DeckBuildState.cs`
  - added formal deck build `Revision`
- `Scripts/Battle/Boundary/DeckBuildSnapshot.cs`
  - added snapshot `Revision`
  - deck snapshot dictionaries now serialize and deserialize revision
- `Scripts/Battle/Shared/GlobalGameSession.cs`
  - added exported `DeckBuildRevision`
  - formal deck snapshots now carry revision
  - applying a saved deck snapshot now restores or increments the formal revision
  - composite state sync now keeps revision in sync with exported session fields
- `Scripts/Map/UI/SystemFeatureLabController.cs`
  - main deck builder now shows the formal saved deck revision in summary and validation text
- `Scripts/Battle/UI/BattleDeckBuilderController.cs`
  - standalone deck builder now also shows the formal saved deck revision
- `Scripts/Battle/BattleSceneController.cs`
  - battle initialization now logs the saved deck revision, saved card count, and build name read from the incoming battle request

result:
- saved deck state now has an explicit revision that can be tracked across save, load, builder UI, and battle initialization
- deck UI can now clearly distinguish “working copy not saved” from the current formal saved build revision
- battle startup now exposes which saved build revision it actually consumed, making day4 acceptance much easier to verify manually
