# Step 34 - Card Loop And Targeting Integration

## Date

2026-03-21

## Goal

Extend the current turn-state tree so the player action phase can drive both basic attacks and card play, then wire in a minimal playable card loop.

## Implemented

- Extended `TurnActionState` with:
  - `CardTargeting`
  - `SelectedCardInstanceId`
  - quick-action handling via `QuickChainCount`
- Added a minimal card runtime:
  - card definitions
  - deck / hand / discard piles
  - per-turn energy refill
  - draw-to-hand-size on new turns
  - discard reshuffle when the draw pile is empty
- Wired cards into `BattleSceneController`:
  - hand click enters card targeting or resolves instantly
  - non-target cards resolve immediately
  - targeted cards validate against board rules before spending the card
- Added a prototype deck:
  - `Slash`
  - `Shot`
  - `Meditate`
- Reused `BoardTargetingService` for straight-line card targeting.
- Expanded the HUD with:
  - turn label
  - energy / draw / discard counters
  - clickable hand buttons

## Behavior

- The player still gets one main non-quick action per turn.
- `AttackTargeting` and `CardTargeting` now coexist under the player action phase.
- `Slash` uses adjacent enemy targeting.
- `Shot` uses straight-line enemy targeting.
- `Meditate` is a `Quick` card:
  - it draws 1 card
  - restores 1 energy
  - does not end the turn

## Quick Validation

- Run `dotnet build` and confirm the battle scripts compile cleanly.
- Start the battle scene and confirm the HUD shows hand cards and current energy.
- Click `Shot` and confirm only straight-line enemies are highlighted.
- Click `Meditate` and confirm the turn does not end after the card resolves.
- Play a non-quick card and confirm the turn advances into post-turn processing.

## Result

This step moves the prototype from “basic attack only” into a real action phase that can host both attacks and cards, while keeping the system intentionally small and readable for the next round of combat work.
