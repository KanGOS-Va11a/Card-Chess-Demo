# step-215 tighten day5 active tutorial copy and fix current visible typos

date: 2026-04-24

changed:
- `Scripts/Map/UI/Scene03PostBattleTutorialController.cs`
  - rewrote the first post-battle C-menu overview copy to be shorter and more action-oriented
- `Scripts/Map/UI/Scene03MenuPageTutorialController.cs`
  - tightened the first-open page tutorial copy for:
    - status tab
    - inventory tab
    - talent tab
    - codex tab
    - deck tab
    - deck learning follow-up tab
  - copy now follows the intended player order more closely:
    - understand the page
    - know what to do next
    - move toward learning setup
- `Scripts/Battle/UI/BattleTutorialIntroOverlay.cs`
  - tightened the first battle tutorial pages
  - tightened the second learning battle tutorial pages
  - learning tutorial now explicitly explains:
    - this is the last chance to learn from this enemy
    - learning records the enemy's actual last-turn action
    - normal vs signature learning outcomes
- `Scripts/Map/Interaction/StoryTriggerZone.cs`
  - rewrote Scene03 corridor trigger text to better match the ship corridor worldbuilding
  - rewrote the second Scene03 learning trigger text to fit the intended learning setup
- `Scene/Character/EnemyStoryDialogue.tscn`
  - rewrote the first enemy pre-battle dialogue to be shorter and fit the updated tutorial flow
- `Resources/Battle/Enemies/DefaultBattleEnemyLibrary.tres`
  - renamed the active tutorial enemy display name to `搭船客拾荒兵`
- `Scripts/Map/UI/SystemFeatureLabController.cs`
  - fixed the active visible typo `掠船客重压者` -> `搭船客重压者`
  - updated the active codex entry for `scene01_tutorial_enemy` to match the current corridor teaching role and worldbuilding language

result:
- the active Day5 teaching text is now shorter, clearer, and closer to the intended player cognition order
- current visible naming is now aligned better with the worldbuilding docs and enemy faction language
- the most obvious active typo and wording drift in codex and tutorial-facing content have been corrected
