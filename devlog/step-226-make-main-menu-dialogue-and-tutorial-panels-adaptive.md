# step-226 make main menu dialogue and tutorial panels adaptive

date: 2026-04-24

changed:
- `Scripts/UI/MainMenuController.cs`
  - added responsive main-menu panel width handling
  - main menu title / subtitle / hint labels now reserve wrapped text width instead of relying on a narrow fixed panel
- `Scene/UI/MainMenu.tscn`
  - enabled wrapping and horizontal fill for menu text labels
- `Scripts/UI/Dialogue/DialogueSequencePanel.cs`
  - dialogue panel now measures speaker + content text against the current viewport
  - panel width and height now expand within screen-safe bounds instead of using a hard fixed height cap
- `Scripts/UI/PagedTutorialPopup.cs`
  - tutorial popup now recalculates panel width, height, and body area based on viewport size and page content
  - body text falls back to scroll mode only when content exceeds the screen-safe maximum height
- `Scene/UI/PagedTutorialPopup.tscn`
  - enabled wrapped title behavior for the popup header

result:
- long menu titles no longer spill outside the main-menu panel
- dialogue boxes and tutorial popups now add more vertical slack automatically for longer text
- text stays inside the panel boundary much more reliably on export builds and scaled desktop windows
