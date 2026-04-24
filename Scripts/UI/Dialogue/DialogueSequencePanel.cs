using System;
using System.Linq;
using CardChessDemo.Audio;
using Godot;

namespace CardChessDemo.UI.Dialogue;

public partial class DialogueSequencePanel : CanvasLayer
{
	private const float MinPanelWidth = 236.0f;
	private const float MaxPanelWidth = 300.0f;
	private const float MinPanelHeight = 58.0f;
	private const float HorizontalPadding = 22.0f;
	private const float VerticalPadding = 20.0f;
	private const float ScreenSideMargin = 10.0f;
	private const float ScreenBottomMargin = 6.0f;
	private const float ScreenTopMargin = 8.0f;

	private Panel _panel = null!;
	private Label _speakerLabel = null!;
	private Label _contentLabel = null!;

	private DialoguePage[] _pages = Array.Empty<DialoguePage>();
	private int _pageIndex;
	private Action? _onCompleted;
	private Action? _onClosed;
	private bool _isClosing;

	public static bool IsVisible(Node? context)
	{
		if (context == null)
		{
			return false;
		}

		Node? currentScene = context.GetTree()?.CurrentScene;
		if (currentScene == null)
		{
			return false;
		}

		return currentScene
			.GetChildren()
			.OfType<DialogueSequencePanel>()
			.Any(panel => GodotObject.IsInstanceValid(panel) && panel.Visible && !panel._isClosing);
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Layer = 430;
		_panel = GetNode<Panel>("Panel");
		_speakerLabel = GetNode<Label>("Panel/Margin/VBox/SpeakerLabel");
		_contentLabel = GetNode<Label>("Panel/Margin/VBox/ContentLabel");
		ConfigureLayout();
		UpdatePanelSize();
		SetProcessUnhandledInput(true);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_isClosing)
		{
			return;
		}

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.E || keyEvent.Keycode == Key.Space || keyEvent.Keycode == Key.Enter)
			{
				Advance();
				GetViewport().SetInputAsHandled();
			}
		}
	}

	public void Present(DialoguePage[] pages, Action? onCompleted = null, Action? onClosed = null)
	{
		_pages = pages ?? Array.Empty<DialoguePage>();
		_pageIndex = 0;
		_onCompleted = onCompleted;
		_onClosed = onClosed;
		_isClosing = false;
		Visible = true;
		UpdatePage();
		GameAudio.Instance?.PlayDialoguePopup();
	}

	private void Advance()
	{
		if (_pageIndex < _pages.Length - 1)
		{
			_pageIndex++;
			UpdatePage();
			GameAudio.Instance?.PlayUiConfirm();
			return;
		}

		Close(true);
	}

	private void UpdatePage()
	{
		if (_pages.Length == 0)
		{
			_speakerLabel.Text = "\u65C1\u767D";
			_contentLabel.Text = "...";
			return;
		}

		DialoguePage page = _pages[Mathf.Clamp(_pageIndex, 0, _pages.Length - 1)];
		_speakerLabel.Text = string.IsNullOrWhiteSpace(page.Speaker) ? "\u65C1\u767D" : page.Speaker.Trim();

		string content = string.IsNullOrWhiteSpace(page.Content) ? "..." : page.Content.Trim();
		if (_pageIndex == 0)
		{
			content = $"{content}\n\n\uff08\u6309 E \u7FFB\u9875\uff09";
		}

		_contentLabel.Text = content;
		UpdatePanelSize();
	}

	private void UpdatePanelSize()
	{
		Vector2 viewportSize = GetViewport()?.GetVisibleRect().Size ?? Vector2.Zero;
		float panelWidth = Mathf.Clamp(viewportSize.X - ScreenSideMargin * 2.0f, MinPanelWidth, MaxPanelWidth);
		float contentWidth = Mathf.Max(80.0f, panelWidth - HorizontalPadding);

		Font? contentFont = _contentLabel.GetThemeFont("font");
		Font? speakerFont = _speakerLabel.GetThemeFont("font");
		if (contentFont == null || speakerFont == null)
		{
			return;
		}

		int speakerFontSize = _speakerLabel.GetThemeFontSize("font_size");
		int contentFontSize = _contentLabel.GetThemeFontSize("font_size");
		Vector2 speakerSize = speakerFont.GetMultilineStringSize(
			_speakerLabel.Text ?? string.Empty,
			HorizontalAlignment.Left,
			contentWidth,
			speakerFontSize);
		Vector2 contentSize = contentFont.GetMultilineStringSize(
			_contentLabel.Text ?? string.Empty,
			HorizontalAlignment.Left,
			contentWidth,
			contentFontSize);

		_speakerLabel.CustomMinimumSize = new Vector2(contentWidth, 0.0f);
		_contentLabel.CustomMinimumSize = new Vector2(contentWidth, 0.0f);

		float speakerHeight = Mathf.Max(speakerFontSize + 4.0f, speakerSize.Y);
		float maxHeight = Mathf.Max(MinPanelHeight, viewportSize.Y - ScreenTopMargin - ScreenBottomMargin);
		float desiredHeight = Mathf.Clamp(contentSize.Y + speakerHeight + VerticalPadding, MinPanelHeight, maxHeight);
		float panelBottom = viewportSize.Y - ScreenBottomMargin;
		_panel.OffsetLeft = ScreenSideMargin;
		_panel.OffsetRight = ScreenSideMargin + panelWidth;
		_panel.OffsetBottom = panelBottom;
		_panel.OffsetTop = panelBottom - desiredHeight;
	}

	private void ConfigureLayout()
	{
		_speakerLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_contentLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_contentLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_contentLabel.CustomMinimumSize = new Vector2(0.0f, 24.0f);
		_contentLabel.ClipText = false;

		if (GetNodeOrNull<Control>("Panel/Margin/VBox") is Control vbox)
		{
			vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			vbox.AnchorRight = 1.0f;
			vbox.AnchorBottom = 1.0f;
		}
	}

	private void Close(bool completed)
	{
		if (_isClosing)
		{
			return;
		}

		_isClosing = true;
		Visible = false;
		if (completed)
		{
			GameAudio.Instance?.PlayUiConfirm();
			_onCompleted?.Invoke();
		}
		else
		{
			GameAudio.Instance?.PlayUiCancel();
			_onClosed?.Invoke();
		}

		QueueFree();
	}
}
