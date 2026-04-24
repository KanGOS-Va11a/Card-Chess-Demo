# step-213 gate scene03 learning fight until learning talent and card are ready

date: 2026-04-24

changed:
- `Scripts/Map/UI/Scene03LearningFlowController.cs`
  - added a Scene03-specific learning phase controller
  - second tutorial enemy and second tutorial trigger now stay disabled until:
    - the first battle post-summary has unlocked menu tutorials
    - the learning talent has been unlocked
    - the learning card is already present in the formal saved deck
- `Scene/Maps/Scene03.tscn`
  - mounted the new Scene03 learning flow controller

result:
- the player can no longer skip ahead into the second learning fight before finishing the intended learning-preparation steps
- Scene03 now enforces the intended order:
  - first basic battle
  - C menu overview
  - menu page tutorials
  - learning talent unlock
  - learning card carry and save
  - second learning fight
