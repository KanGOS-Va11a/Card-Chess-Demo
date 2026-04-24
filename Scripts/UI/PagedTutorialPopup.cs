using System;
using System.Linq;
using CardChessDemo.Audio;
using Godot;

namespace CardChessDemo.UI;

public partial class PagedTutorialPopup : CanvasLayer
{
	private const float PanelHorizontalMargin = 12.0f;
	private const float PanelTopMargin = 8.0f;
	private const float PanelBottomMargin = 8.0f;
	private const float PanelMaxWidth = 296.0f;
	private const float PanelMinWidth = 236.0f;
	private const float PanelMarginPadding = 20.0f;
	private const float ContentMinHeight = 72.0f;
	private const float HeaderHeightEstimate = 18.0f;
	private const float FooterHeightEstimate = 20.0f;
	private const float ContainerSpacing = 12.0f;
	private const float HeaderFooterPadding = 24.0f;

	private Control _root = null!;
	private Panel _panel = null!;
	private Label _titleLabel = null!;
	private RichTextLabel _contentLabel = null!;
	private Button _prevButton = null!;
	private Button _nextButton = null!;
	private Button _closeButton = null!;

	private string[] _pages = Array.Empty<string>();
	private int _pageIndex;
	private Action? _onCompleted;
	private Action? _onClosed;
	private bool _isClosing;
	private bool _completed;

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
			.OfType<PagedTutorialPopup>()
			.Any(panel => GodotObject.IsInstanceValid(panel) && panel.Visible && !panel._isClosing);
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Layer = 420;
		_root = GetNode<Control>("Root");
		_panel = GetNode<Panel>("Root/Panel");
		_titleLabel = GetNode<Label>("Root/Panel/Margin/Content/Header/Title");
		_contentLabel = GetNode<RichTextLabel>("Root/Panel/Margin/Content/Body");
		_contentLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_contentLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_contentLabel.CustomMinimumSize = new Vector2(264.0f, ContentMinHeight);
		_prevButton = GetNode<Button>("Root/Panel/Margin/Content/Footer/PrevButton");
		_nextButton = GetNode<Button>("Root/Panel/Margin/Content/Footer/NextButton");
		_closeButton = GetNode<Button>("Root/Panel/Margin/Content/Header/CloseButton");
		_titleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;

		_prevButton.Pressed += OnPrevPressed;
		_nextButton.Pressed += OnNextPressed;
		_closeButton.Pressed += OnClosePressed;
		_root.Resized += QueueLayoutRefresh;
	}

	public override void _ExitTree()
	{
		if (!IsNodeReady())
		{
			return;
		}

		_root.Resized -= QueueLayoutRefresh;
	}

	public void Present(string title, string[] pages, Action? onCompleted = null, Action? onClosed = null)
	{
		_pages = pages ?? Array.Empty<string>();
		_pageIndex = 0;
		_onCompleted = onCompleted;
		_onClosed = onClosed;
		_isClosing = false;
		_completed = false;
		_titleLabel.Text = string.IsNullOrWhiteSpace(title) ? "\u6559\u7A0B" : title.Trim();
		Visible = true;
		UpdatePage();
		GameAudio.Instance?.PlayDialoguePopup();
	}

	private void OnPrevPressed()
	{
		if (_isClosing || _pageIndex <= 0)
		{
			return;
		}

		_pageIndex--;
		UpdatePage();
		GameAudio.Instance?.PlayUiConfirm();
	}

	private void OnNextPressed()
	{
		if (_isClosing)
		{
			return;
		}

		if (_pageIndex < _pages.Length - 1)
		{
			_pageIndex++;
			UpdatePage();
			GameAudio.Instance?.PlayUiConfirm();
			return;
		}

		_completed = true;
		CloseInternal(invokeCompleted: true);
	}

	private void OnClosePressed()
	{
		if (_isClosing)
		{
			return;
		}

		CloseInternal(invokeCompleted: false);
	}

	private void UpdatePage()
	{
		string text = _pages.Length == 0
			? "..."
			: _pages[Mathf.Clamp(_pageIndex, 0, _pages.Length - 1)] ?? string.Empty;
		_contentLabel.Text = string.IsNullOrWhiteSpace(text) ? "..." : text.Trim();
		_prevButton.Disabled = _pageIndex <= 0;
		_nextButton.Text = _pageIndex >= _pages.Length - 1 ? "\u5B8C\u6210" : "\u4E0B\u4E00\u9875";
		QueueLayoutRefresh();
	}

	private void QueueLayoutRefresh()
	{
		if (!IsNodeReady())
		{
			return;
		}

		CallDeferred(nameof(UpdatePanelLayout));
	}

	private void UpdatePanelLayout()
	{
		Vector2 viewportSize = GetViewport()?.GetVisibleRect().Size ?? Vector2.Zero;
		float panelWidth = Mathf.Clamp(viewportSize.X - PanelHorizontalMargin * 2.0f, PanelMinWidth, PanelMaxWidth);
		float bodyWidth = Mathf.Max(96.0f, panelWidth - PanelMarginPadding);

		_panel.OffsetLeft = Mathf.Floor((viewportSize.X - panelWidth) * 0.5f);
		_panel.OffsetRight = _panel.OffsetLeft + panelWidth;
		_titleLabel.CustomMinimumSize = new Vector2(bodyWidth - 28.0f, 0.0f);
		_contentLabel.CustomMinimumSize = new Vector2(bodyWidth, 0.0f);

		Font? bodyFont = _contentLabel.GetThemeFont("normal_font");
		Font? titleFont = _titleLabel.GetThemeFont("font");
		if (bodyFont == null || titleFont == null)
		{
			return;
		}

		int bodyFontSize = _contentLabel.GetThemeFontSize("normal_font_size");
		int titleFontSize = _titleLabel.GetThemeFontSize("font_size");
		float measuredBodyHeight = bodyFont.GetMultilineStringSize(
			_contentLabel.Text ?? string.Empty,
			HorizontalAlignment.Left,
			bodyWidth,
			bodyFontSize).Y + 4.0f;
		float measuredTitleHeight = titleFont.GetMultilineStringSize(
			_titleLabel.Text ?? string.Empty,
			HorizontalAlignment.Left,
			bodyWidth - 28.0f,
			titleFontSize).Y;

		float maxPanelHeight = Mathf.Max(96.0f, viewportSize.Y - PanelTopMargin - PanelBottomMargin);
		float fixedHeight = HeaderHeightEstimate + FooterHeightEstimate + HeaderFooterPadding + ContainerSpacing;
		float maxBodyHeight = Mathf.Max(ContentMinHeight, maxPanelHeight - fixedHeight);
		float bodyHeight = Mathf.Min(Mathf.Max(ContentMinHeight, measuredBodyHeight), maxBodyHeight);
		_contentLabel.CustomMinimumSize = new Vector2(bodyWidth, bodyHeight);
		_contentLabel.ScrollActive = measuredBodyHeight > maxBodyHeight + 0.5f;

		float desiredHeight = Mathf.Min(
			maxPanelHeight,
			measuredTitleHeight + bodyHeight + HeaderFooterPadding + ContainerSpacing);
		float top = Mathf.Clamp((viewportSize.Y - desiredHeight) * 0.38f, PanelTopMargin, viewportSize.Y - desiredHeight - PanelBottomMargin);
		_panel.OffsetTop = top;
		_panel.OffsetBottom = top + desiredHeight;
	}

	private void CloseInternal(bool invokeCompleted)
	{
		_isClosing = true;
		Visible = false;
		if (invokeCompleted && _completed)
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
