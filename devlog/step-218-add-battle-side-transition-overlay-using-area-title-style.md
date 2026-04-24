# step-218 add battle side transition overlay using area title style

date: 2026-04-24

changed:
- `Scripts/Battle/BattleSceneController.cs`
  - added a reusable battle phase overlay helper that instantiates the existing `AreaTitleOverlay` scene
  - enemy turn start now shows `——敌方行动——`
  - enemy turn end / player turn start now shows `——玩家行动——`
  - added a small overlap guard so a new phase banner clears the previous one before showing

result:
- battle now shows a clear full-screen phase transition prompt when control switches between player side and enemy side
- the visual style is aligned with the map's existing key-area title overlay instead of introducing a separate battle-only prompt system
