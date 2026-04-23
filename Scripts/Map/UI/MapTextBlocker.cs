using Godot;
using CardChessDemo.UI.Dialogue;
using CardChessDemo.UI;

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

		if (MapDialogueService.HasBlockingDialogue())
		{
			return true;
		}

		if (PagedTutorialPopup.IsVisible(context))
		{
			return true;
		}

		return false;
	}
}
