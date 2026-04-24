# step-212 start day5 scene03 learning flow and menu page tutorials

date: 2026-04-24

changed:
- `Scripts/Map/UI/Scene03PostBattleTutorialController.cs`
  - rewrote the first post-battle summary popup into a shorter C-menu overview
  - added `scene03_menu_page_tutorials_unlocked` flag output so page-specific menu tutorials only become available after the first battle summary has finished
- `Scripts/Map/UI/Scene03MenuPageTutorialController.cs`
  - added a new Scene03-specific menu tutorial controller
  - after the first battle summary, it now shows one-time tutorials the first time the player enters:
    - status tab
    - inventory tab
    - talent tab
    - codex tab
    - deck tab
  - deck tab also supports a second one-time tutorial after the learning talent/card has been unlocked, specifically for carrying the learning card and saving the build
- `Scripts/Map/UI/SystemFeatureLabController.cs`
  - added minimal public menu state helpers:
    - menu visibility
    - current tab index
    - tutorial focus helpers for talent and deck
- `Scripts/Battle/UI/BattleTutorialIntroOverlay.cs`
  - rewrote the basic battle tutorial pages to be shorter and more player-order oriented
  - added a new `learning_battle` tutorial preset for the second Scene03 teaching fight
- `Scene/Battle/BattleTutorialLearning.tscn`
  - added a dedicated battle scene for the second Scene03 learning tutorial fight
- `Resources/Battle/Encounters/DebugBattleEncounterLibrary.tres`
  - added `scene03_learning_enemy` encounter id that still uses the same tutorial scavenger enemy definition and room pool
- `Scripts/Map/Interaction/StoryTriggerZone.cs`
  - shortened the first Scene03 corridor intro text
  - added `scene03_learning_story` for the second tutorial enemy setup
- `Scene/Character/EnemyStoryDialogue.tscn`
  - rewrote the first enemy pre-battle dialogue to match the revised basic combat teaching flow
- `Scene/Maps/Scene03.tscn`
  - added a second tutorial enemy using the same tutorial scavenger enemy type
  - added a second grid story trigger for the learning fight lead-in
  - mounted the new menu page tutorial controller into Scene03
  - wired the second enemy to `BattleTutorialLearning.tscn`

result:
- Scene03 now has a second dedicated teaching enemy for the learning system while still using the same tutorial enemy type as the first fight
- first-battle C-menu overview and later page-specific tutorials are now split into separate layers instead of being forced into one popup
- the player can now be guided in order:
  - first battle basics
  - C menu overview
  - first-time page tutorials
  - learning talent unlock
  - learning card carry/save
  - second battle learning usage tutorial
