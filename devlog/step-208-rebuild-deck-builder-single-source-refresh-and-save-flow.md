# step-208 rebuild deck builder single-source refresh and save flow

date: 2026-04-24

changed:
- `Scripts/Map/UI/SystemFeatureLabController.cs`
  - rebuilt the deck tab refresh flow so the view is fully re-derived from:
    - current progression snapshot
    - current working deck
    - deck construction service
  - removed deferred follow-up refresh hacks from the main deck tab
  - changed the right-side deck list to grouped entries by card id with visible copy counts
  - made save button state depend on both:
    - validation success
    - dirty working state
  - made reset button depend on dirty working state
  - add/remove operations now only edit `_workingDeck`
  - save is now the only action that writes the formal build back to `GlobalGameSession.DeckBuildState`
  - add button availability now checks real candidate validation instead of only copy limit
- `Scripts/Battle/UI/BattleDeckBuilderController.cs`
  - aligned the standalone builder with the same rules:
    - grouped deck entries
    - dirty-state tracking
    - save-after-validation only
    - working deck separate from formal session state

result:
- the main C-menu deck builder no longer relies on second-pass refresh patches to show correct state
- candidate list, carried list, impact text, and save state now come from the same working deck interpretation
- unsaved changes stay local to the builder UI and do not become battle input until saved
- battle continues to read only the formal saved deck snapshot from session state
