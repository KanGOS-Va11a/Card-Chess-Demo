# step-225 switch map scene transitions to pixel reshape overlay

date: 2026-04-24

changed:
- `Shaders/Transitions/MapScenePixelReshape.gdshader`
  - added a screen-space pixelation shader that increases mosaic block size with transition progress
- `Scene/Transitions/MapScenePixelTransitionOverlay.tscn`
  - added a dedicated map-scene transition overlay that uses the shared transition script with the new pixel-reshape shader
- `Scripts/Map/Transitions/MapSceneTransitionHelper.cs`
  - switched map scene transitions to the new pixel-reshape overlay scene
- `Scripts/Map/Interaction/SceneDoor.cs`
  - map scene doors now use `MapSceneTransitionHelper` instead of direct `ChangeSceneTo*`
- `Scripts/Map/Interaction/StoryTriggerZone.cs`
  - configured scene changes after trigger completion now use the shared map transition helper
- `Scripts/Map/Dialogue/MapDialogueService.cs`
  - dialogue follow-up scene changes now use the shared map transition helper
- `Scripts/Map/UI/Scene04To05CutsceneController.cs`
  - fallback scene jump path now also uses the shared map transition helper

result:
- map scene-to-scene transitions now pixelate the current screen into larger mosaic blocks before the scene swap
- after the next scene loads, the mosaic resolves back down to the original pixel density
- doors, dialogue-driven scene jumps, and trigger-driven scene jumps now share the same transition behavior instead of bypassing it
