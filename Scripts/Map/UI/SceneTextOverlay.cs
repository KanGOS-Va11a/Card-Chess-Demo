using Godot;
using CardChessDemo.Audio;

namespace CardChessDemo.Map;

public static class SceneTextOverlay
{
	private const string PanelPath = "TutorialUI/TutorialTipPanel";
	private const string LabelPath = "TutorialUI/TutorialTipPanel/TutorialTipLabel";
	private const string ActiveMetaKey = "scene_text_overlay_active";

	public static bool IsVisible(Node context)
	{
		Control? panel = ResolvePanel(context);
		return panel != null
			&& panel.Visible
			&& panel.HasMeta(ActiveMetaKey)
			&& panel.GetMeta(ActiveMetaKey).AsBool();
	}

	public static void Hide(Node context)
	{
		Control? panel = ResolvePanel(context);
		if (panel != null)
		{
			panel.SetMeta(ActiveMetaKey, false);
			panel.Visible = false;
			GameAudio.Instance?.PlayUiCancel();
		}
	}

	public static bool Show(Node context, string content)
	{
		Control? panel = ResolvePanel(context);
		Label? label = ResolveLabel(context);
		if (panel == null || label == null)
		{
			return false;
		}

		label.Text = string.IsNullOrWhiteSpace(content) ? "..." : content.Trim();
		panel.SetMeta(ActiveMetaKey, true);
		panel.Visible = true;
		GameAudio.Instance?.PlayDialoguePopup();
		return true;
	}

	private static Control? ResolvePanel(Node context)
	{
		Node? currentScene = context?.GetTree()?.CurrentScene;
		return currentScene?.GetNodeOrNull<Control>(PanelPath);
	}

	private static Label? ResolveLabel(Node context)
	{
		Node? currentScene = context?.GetTree()?.CurrentScene;
		return currentScene?.GetNodeOrNull<Label>(LabelPath);
	}
}
