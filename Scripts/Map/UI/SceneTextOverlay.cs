using Godot;
using CardChessDemo.Audio;

namespace CardChessDemo.Map;

public static class SceneTextOverlay
{
	private const string OverlayLayerName = "TutorialUI";
	private const string OverlayPanelName = "TutorialTipPanel";
	private const string OverlayLabelName = "TutorialTipLabel";
	private const string PanelPath = "TutorialUI/TutorialTipPanel";
	private const string LabelPath = "TutorialUI/TutorialTipPanel/TutorialTipLabel";
	private const string ActiveMetaKey = "scene_text_overlay_active";
	private const string DefaultFontPath = "res://Assets/Fonts/unifont_t-17.0.04.otf";

	public static bool IsVisible(Node context)
	{
		Control? panel = ResolvePanel(context, ensureExists: false);
		return panel != null
			&& panel.Visible
			&& panel.HasMeta(ActiveMetaKey)
			&& panel.GetMeta(ActiveMetaKey).AsBool();
	}

	public static void Hide(Node context)
	{
		Control? panel = ResolvePanel(context, ensureExists: false);
		if (panel != null)
		{
			panel.SetMeta(ActiveMetaKey, false);
			panel.Visible = false;
			GameAudio.Instance?.PlayUiCancel();
		}
	}

	public static bool Show(Node context, string content)
	{
		Control? panel = ResolvePanel(context, ensureExists: true);
		Label? label = ResolveLabel(context, ensureExists: true);
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

	private static Control? ResolvePanel(Node context, bool ensureExists)
	{
		Node? currentScene = context?.GetTree()?.CurrentScene;
		if (currentScene == null)
		{
			return null;
		}

		Control? panel = currentScene.GetNodeOrNull<Control>(PanelPath);
		if (panel == null && ensureExists)
		{
			panel = EnsureOverlayNodes(currentScene).panel;
		}

		return panel;
	}

	private static Label? ResolveLabel(Node context, bool ensureExists)
	{
		Node? currentScene = context?.GetTree()?.CurrentScene;
		if (currentScene == null)
		{
			return null;
		}

		Label? label = currentScene.GetNodeOrNull<Label>(LabelPath);
		if (label == null && ensureExists)
		{
			label = EnsureOverlayNodes(currentScene).label;
		}

		return label;
	}

	private static (CanvasLayer layer, Panel panel, Label label) EnsureOverlayNodes(Node currentScene)
	{
		CanvasLayer? layer = currentScene.GetNodeOrNull<CanvasLayer>(OverlayLayerName);
		if (layer == null)
		{
			layer = new CanvasLayer
			{
				Name = OverlayLayerName,
				Layer = 40,
			};
			currentScene.AddChild(layer);
		}

		Panel? panel = layer.GetNodeOrNull<Panel>(OverlayPanelName);
		if (panel == null)
		{
			panel = new Panel
			{
				Name = OverlayPanelName,
				Visible = false,
				ZIndex = 30,
				AnchorLeft = 0.5f,
				AnchorTop = 1.0f,
				AnchorRight = 0.5f,
				AnchorBottom = 1.0f,
				OffsetLeft = -110.0f,
				OffsetTop = -66.0f,
				OffsetRight = 110.0f,
				OffsetBottom = -18.0f,
				MouseFilter = Control.MouseFilterEnum.Ignore,
			};
			layer.AddChild(panel);
		}

		Label? label = panel.GetNodeOrNull<Label>(OverlayLabelName);
		if (label == null)
		{
			label = new Label
			{
				Name = OverlayLabelName,
				AnchorLeft = 0.0f,
				AnchorTop = 0.0f,
				AnchorRight = 1.0f,
				AnchorBottom = 1.0f,
				OffsetLeft = 10.0f,
				OffsetTop = 6.0f,
				OffsetRight = -10.0f,
				OffsetBottom = -6.0f,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				MouseFilter = Control.MouseFilterEnum.Ignore,
			};

			FontFile? font = GD.Load<FontFile>(DefaultFontPath);
			if (font != null)
			{
				label.AddThemeFontOverride("font", font);
			}

			label.AddThemeFontSizeOverride("font_size", 16);
			panel.AddChild(label);
		}

		return (layer, panel, label);
	}
}
