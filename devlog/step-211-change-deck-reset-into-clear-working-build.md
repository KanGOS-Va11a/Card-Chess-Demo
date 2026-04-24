# step-211 change deck reset into clear working build

date: 2026-04-24

changed:
- `Scripts/Map/UI/SystemFeatureLabController.cs`
  - changed the deck tab reset button label from "恢复会话" to "清空构筑"
  - clear action now empties the current working deck instead of reloading the saved deck from session
  - clear button availability now depends on whether the working deck currently has cards
- `Scripts/Battle/UI/BattleDeckBuilderController.cs`
  - aligned the standalone builder reset button semantics to "Clear Deck"
  - clear action now empties the current working deck instead of restoring session state
  - clear button availability now depends on whether the working deck currently has cards

result:
- players can now quickly wipe the working build and rebuild from scratch
- the formal saved build in session is still untouched until the player explicitly presses save
- this keeps the day4 "save after effect" rule intact while making deck iteration faster
