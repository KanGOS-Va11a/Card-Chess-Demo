using System;
using System.Linq;
using CardChessDemo.Audio;
using Godot;

namespace CardChessDemo.UI.Dialogue;

public partial class DialogueSequencePanel : CanvasLayer
{
	private const float PanelWidth = 300.0f;
	private const float MinPanelHeight = 58.0f;
	private const float MaxPanelHeight = 108.0f;
	private const float PanelBottom = 174.0f;
	private const float HorizontalPadding = 20.0f;
	private const float VerticalPadding = 20.0f;

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
		Font? contentFont = _contentLabel.GetThemeFont("font");
		if (contentFont == null)
		{
			return;
		}

		int speakerFontSize = _speakerLabel.GetThemeFontSize("font_size");
		int contentFontSize = _contentLabel.GetThemeFontSize("font_size");
		float contentWidth = Mathf.Max(32.0f, PanelWidth - HorizontalPadding);
		Vector2 contentSize = contentFont.GetMultilineStringSize(
			_contentLabel.Text ?? string.Empty,
			HorizontalAlignment.Left,
			contentWidth,
			contentFontSize);

		float speakerHeight = speakerFontSize + 4.0f;
		float desiredHeight = Mathf.Clamp(contentSize.Y + speakerHeight + VerticalPadding, MinPanelHeight, MaxPanelHeight);
		_panel.OffsetBottom = PanelBottom;
		_panel.OffsetTop = PanelBottom - desiredHeight;
	}

	private void ConfigureLayout()
	{
		float contentWidth = Mathf.Max(32.0f, PanelWidth - HorizontalPadding);
		_contentLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_contentLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_contentLabel.CustomMinimumSize = new Vector2(contentWidth, 24.0f);
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
