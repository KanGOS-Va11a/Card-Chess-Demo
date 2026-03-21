# Step 35 - Card Piles And Visual Hand

## Date

2026-03-21

## Goal

Replace the temporary hand buttons with real card visuals, and expand the runtime from a minimal hand-only loop into a clearer card pile system.

## Implemented

- Expanded `BattleDeckState` with:
  - draw pile
  - hand
  - discard pile
  - exhaust pile
  - end-turn hand discard
- Added richer `BattleCardDefinition` data:
  - card category
  - `Quick`
  - `Exhaust`
- Reworked the battle HUD hand into reusable card views.
- Added `BattleCardView` as a stylized prototype card:
  - energy badge at top-left
  - placeholder card art at the upper center
  - keyword row
  - description label
- Added a simple card play animation so played cards briefly fly forward before fading.
- Replaced the prototype deck with a broader test set:
  - melee attack cards
  - line attack cards
  - draw cards
  - energy cards
  - `Quick` cards
  - `Exhaust` cards

## Behavior

- Ending the player turn now discards the remaining hand.
- Exhaust cards now leave the normal discard loop and go to the exhaust pile.
- The HUD now shows:
  - energy
  - draw count
  - discard count
  - exhaust count
- Targeted cards still reuse board validation from the battle controller.

## Quick Validation

- Run `dotnet build` and confirm the project compiles cleanly.
- Start the battle scene and confirm the hand is rendered as full cards rather than text buttons.
- Play a `Quick` card and confirm the turn remains in player action.
- Play an `Exhaust` card and confirm the exhaust count increases instead of the discard count.
- End the turn and confirm the remaining hand moves into the discard pile before the next draw.

## Result

This step turns the card prototype into something that is much closer to a readable combat UI: the player now sees actual cards, pile counts, and keyword differences, while the runtime can distinguish discard flow from exhaust flow.
