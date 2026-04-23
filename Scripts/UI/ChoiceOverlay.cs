using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardChessDemo.Audio;
using Godot;

namespace CardChessDemo.UI;

public sealed partial class ChoiceOverlay : CanvasLayer
{
	private readonly List<Button> _buttons = new();
	private readonly TaskCompletionSource<int> _completionSource = new();
	private string _pendingTitle = string.Empty;
	private string[] _pendingOptions = Array.Empty<string>();

	private ColorRect _dim = null!;
	private Panel _panel = null!;
	private Label _titleLabel = null!;
	private VBoxContainer _buttonContainer = null!;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Layer = 460;
		SetProcessUnhandledInput(true);
		BuildUi();
		ApplyPendingContent();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		if (keyEvent.Keycode == Key.Escape)
		{
			CompleteSelection(-1, playCancelSound: true);
			GetViewport().SetInputAsHandled();
		}
	}

	public Task<int> PresentAsync(string title, params string[] options)
	{
		_pendingTitle = title ?? string.Empty;
		_pendingOptions = options?.Where(option => !string.IsNullOrWhiteSpace(option)).ToArray() ?? Array.Empty<string>();
		ApplyPendingContent();
		return _completionSource.Task;
	}

	private void BuildUi()
	{
		_dim = new ColorRect
		{
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			Color = new Color(0.02f, 0.03f, 0.05f, 0.74f),
			MouseFilter = Control.MouseFilterEnum.Stop,
			ProcessMode = ProcessModeEnum.Always,
		};
		_dim.GuiInput += OnDimGuiInput;
		AddChild(_dim);

		_panel = new Panel
		{
			CustomMinimumSize = new Vector2(176.0f, 96.0f),
			Position = new Vector2(72.0f, 42.0f),
			MouseFilter = Control.MouseFilterEnum.Stop,
			ProcessMode = ProcessModeEnum.Always,
		};
		StyleBoxFlat style = new()
		{
			BgColor = new Color(0.08f, 0.10f, 0.14f, 0.97f),
			BorderColor = new Color(0.80f, 0.83f, 0.88f, 1.0f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 3,
			CornerRadiusTopRight = 3,
			CornerRadiusBottomLeft = 3,
			CornerRadiusBottomRight = 3,
		};
		_panel.AddThemeStyleboxOverride("panel", style);
		AddChild(_panel);

		MarginContainer margin = new()
		{
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			OffsetLeft = 10.0f,
			OffsetTop = 8.0f,
			OffsetRight = -10.0f,
			OffsetBottom = -8.0f,
		};
		_panel.AddChild(margin);

		VBoxContainer root = new();
		root.AddThemeConstantOverride("separation", 8);
		margin.AddChild(root);

		_titleLabel = new Label
		{
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
		};
		_titleLabel.AddThemeFontSizeOverride("font_size", 16);
		root.AddChild(_titleLabel);

		_buttonContainer = new VBoxContainer();
		_buttonContainer.AddThemeConstantOverride("separation", 6);
		root.AddChild(_buttonContainer);
	}

	private void ApplyPendingContent()
	{
		if (_titleLabel == null || _buttonContainer == null)
		{
			return;
		}

		_titleLabel.Text = string.IsNullOrWhiteSpace(_pendingTitle) ? "\u9009\u62E9\u64CD\u4F5C" : _pendingTitle;

		foreach (Node child in _buttonContainer.GetChildren())
		{
			child.QueueFree();
		}

		_buttons.Clear();
		if (_pendingOptions.Length == 0)
		{
			_pendingOptions = new[] { "\u5173\u95ED" };
		}

		for (int index = 0; index < _pendingOptions.Length; index++)
		{
			int resolvedIndex = index;
			Button button = new()
			{
				Text = _pendingOptions[index],
				CustomMinimumSize = new Vector2(0.0f, 24.0f),
				FocusMode = Control.FocusModeEnum.All,
				ProcessMode = ProcessModeEnum.Always,
			};
			button.AddThemeFontSizeOverride("font_size", 16);
			button.Pressed += () => CompleteSelection(resolvedIndex, playCancelSound: false);
			_buttonContainer.AddChild(button);
			_buttons.Add(button);
		}

		if (_buttons.Count > 0)
		{
			_buttons[0].GrabFocus();
		}
	}

	private void OnDimGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton
			&& mouseButton.Pressed
			&& mouseButton.ButtonIndex == MouseButton.Left)
		{
			CompleteSelection(-1, playCancelSound: true);
			GetViewport().SetInputAsHandled();
		}
	}

	private void CompleteSelection(int selectedIndex, bool playCancelSound)
	{
		if (_completionSource.Task.IsCompleted)
		{
			return;
		}

		if (selectedIndex >= 0)
		{
			GameAudio.Instance?.PlayUiConfirm();
		}
		else if (playCancelSound)
		{
			GameAudio.Instance?.PlayUiCancel();
		}

		_completionSource.SetResult(selectedIndex);
		QueueFree();
	}
}
