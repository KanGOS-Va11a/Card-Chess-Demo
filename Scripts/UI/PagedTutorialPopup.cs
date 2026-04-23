using System;
using System.Linq;
using CardChessDemo.Audio;
using Godot;

namespace CardChessDemo.UI;

public partial class PagedTutorialPopup : CanvasLayer
{
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
		_titleLabel = GetNode<Label>("Root/Panel/Margin/Content/Header/Title");
		_contentLabel = GetNode<RichTextLabel>("Root/Panel/Margin/Content/Body");
		_contentLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		_contentLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_contentLabel.CustomMinimumSize = new Vector2(264.0f, 72.0f);
		_prevButton = GetNode<Button>("Root/Panel/Margin/Content/Footer/PrevButton");
		_nextButton = GetNode<Button>("Root/Panel/Margin/Content/Footer/NextButton");
		_closeButton = GetNode<Button>("Root/Panel/Margin/Content/Header/CloseButton");

		_prevButton.Pressed += OnPrevPressed;
		_nextButton.Pressed += OnNextPressed;
		_closeButton.Pressed += OnClosePressed;
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
