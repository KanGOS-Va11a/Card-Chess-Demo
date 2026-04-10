using Godot;

namespace CardChessDemo.Map;

public static class MapTextBlocker
{
	public static bool IsBlockingTextVisible(Node context)
	{
		if (context == null)
		{
			return false;
		}

		if (GalDialogueOverlay.IsVisible(context))
		{
			return true;
		}

		Node? currentScene = context.GetTree()?.CurrentScene;
		if (currentScene is Scene01TutorialController tutorialController && tutorialController.IsDialogBlockingInput)
		{
			return true;
		}

		return false;
	}
}
