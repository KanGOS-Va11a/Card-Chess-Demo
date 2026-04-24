# step-223 add distinct battle card visual affixes for quick and exhaust

date: 2026-04-24

changed:
- `Scripts/Battle/UI/BattleCardAffixOverlay.cs`
  - added a dedicated overlay control for card affix rendering
  - exhaust cards now draw a dashed outer frame
  - removed the temporary quick-side marker after readability review
- `Scene/Battle/UI/BattleCardView.tscn`
  - added the affix overlay as a top-layer child of the card view
- `Scripts/Battle/UI/BattleCardView.cs`
  - bound `IsQuick` and `ExhaustsOnPlay` state into the overlay during card refresh
  - quick cards now use a blue title banner instead of a small side accent
  - quick title text now switches to a light font color for readability

result:
- players can now identify quick cards from the title strip and exhaust cards from the dashed frame without reading keyword text first
