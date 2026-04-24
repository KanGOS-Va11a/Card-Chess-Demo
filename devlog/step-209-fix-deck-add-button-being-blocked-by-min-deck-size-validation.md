# step-209 fix deck add button being blocked by min deck size validation

date: 2026-04-24

changed:
- `Scripts/Battle/Cards/BattleDeckValidationResult.cs`
  - added explicit violation flags for deck validation categories
  - added `CanAddCards` to represent whether a candidate deck can continue growing even if it is still below minimum deck size
- `Scripts/Battle/Cards/BattleDeckConstructionService.cs`
  - now fills detailed validation category flags
  - now records effective max deck size
  - now validates maximum deck size explicitly
- `Scripts/Map/UI/SystemFeatureLabController.cs`
  - deck add availability now uses `validation.CanAddCards` instead of full `validation.IsValid`
- `Scripts/Battle/UI/BattleDeckBuilderController.cs`
  - standalone builder add availability now also uses `validation.CanAddCards`

result:
- deck add buttons are no longer permanently disabled just because the current or candidate build is still below the minimum required deck size
- add remains blocked for real hard constraints such as:
  - copy limit
  - max deck size
  - point budget
  - overlimit slot overflow
  - cycle-rule overflow
  - unavailable cards
