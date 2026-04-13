using System;
using Godot;
using CardChessDemo.Audio;

namespace CardChessDemo.UI;

public partial class EscapeSettingsOverlay : CanvasLayer
{
	public static EscapeSettingsOverlay? Instance { get; private set; }

	private ColorRect _dim = null!;
	private Panel _panel = null!;
	private Label _titleLabel = null!;
	private Button _closeButton = null!;
	private HSlider _masterSlider = null!;
	private Label _masterValueLabel = null!;
	private Button _masterMuteButton = null!;
	private HSlider _musicSlider = null!;
	private Label _musicValueLabel = null!;
	private Button _musicMuteButton = null!;
	private HSlider _sfxSlider = null!;
	private Label _sfxValueLabel = null!;
	private Button _sfxMuteButton = null!;
	private Label _hintLabel = null!;
	private bool _isOpen;
	private bool _isRefreshing;

	public override void _Ready()
	{
		Instance = this;
		ProcessMode = ProcessModeEnum.Always;
		Layer = 500;
		SetProcessUnhandledInput(true);
		Visible = true;
		BuildUi();
		SetMenuVisible(false, playSound: false);
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		if (keyEvent.Keycode != Key.Escape)
		{
			return;
		}

		SetMenuVisible(!_isOpen, playSound: true);
		GetViewport().SetInputAsHandled();
	}

	private void BuildUi()
	{
		_dim = new ColorRect
		{
			Name = "Dim",
			Visible = false,
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			Color = new Color(0.02f, 0.03f, 0.05f, 0.72f),
			MouseFilter = Control.MouseFilterEnum.Stop,
			ProcessMode = ProcessModeEnum.Always,
		};
		_dim.GuiInput += OnDimGuiInput;
		AddChild(_dim);

		_panel = new Panel
		{
			Name = "Panel",
			Visible = false,
			CustomMinimumSize = new Vector2(260.0f, 142.0f),
			Position = new Vector2(30.0f, 19.0f),
			MouseFilter = Control.MouseFilterEnum.Stop,
			ProcessMode = ProcessModeEnum.Always,
		};
		AddChild(_panel);

		StyleBoxFlat panelStyle = new()
		{
			BgColor = new Color(0.08f, 0.10f, 0.14f, 0.96f),
			BorderColor = new Color(0.78f, 0.82f, 0.88f, 1.0f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 3,
			CornerRadiusTopRight = 3,
			CornerRadiusBottomLeft = 3,
			CornerRadiusBottomRight = 3,
		};
		_panel.AddThemeStyleboxOverride("panel", panelStyle);

		MarginContainer margin = new()
		{
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			OffsetLeft = 10.0f,
			OffsetTop = 8.0f,
			OffsetRight = -10.0f,
			OffsetBottom = -8.0f,
			ProcessMode = ProcessModeEnum.Always,
		};
		_panel.AddChild(margin);

		VBoxContainer root = new() { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
		root.AddThemeConstantOverride("separation", 6);
		margin.AddChild(root);

		HBoxContainer titleRow = new();
		titleRow.AddThemeConstantOverride("separation", 6);
		root.AddChild(titleRow);

		_titleLabel = new Label
		{
			Text = "\u8BBE\u7F6E",
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			VerticalAlignment = VerticalAlignment.Center,
		};
		_titleLabel.AddThemeFontSizeOverride("font_size", 16);
		titleRow.AddChild(_titleLabel);

		_closeButton = CreateWindowButton("X");
		_closeButton.Pressed += () => SetMenuVisible(false, playSound: true);
		titleRow.AddChild(_closeButton);

		root.AddChild(CreateAudioRow("\u603B\u97F3\u91CF", out _masterSlider, out _masterValueLabel, out _masterMuteButton));
		root.AddChild(CreateAudioRow("\u97F3\u4E50", out _musicSlider, out _musicValueLabel, out _musicMuteButton));
		root.AddChild(CreateAudioRow("\u97F3\u6548", out _sfxSlider, out _sfxValueLabel, out _sfxMuteButton));

		_hintLabel = new Label
		{
			Text = "Esc \u5173\u95ED",
			HorizontalAlignment = HorizontalAlignment.Right,
		};
		_hintLabel.AddThemeFontSizeOverride("font_size", 16);
		root.AddChild(_hintLabel);

		_masterSlider.ValueChanged += OnMasterVolumeChanged;
		_musicSlider.ValueChanged += OnMusicVolumeChanged;
		_sfxSlider.ValueChanged += OnSfxVolumeChanged;
		_masterMuteButton.Pressed += OnMasterMutePressed;
		_musicMuteButton.Pressed += OnMusicMutePressed;
		_sfxMuteButton.Pressed += OnSfxMutePressed;
	}

	private HBoxContainer CreateAudioRow(string labelText, out HSlider slider, out Label valueLabel, out Button muteButton)
	{
		HBoxContainer row = new();
		row.AddThemeConstantOverride("separation", 6);

		Label label = new()
		{
			Text = labelText,
			CustomMinimumSize = new Vector2(46.0f, 0.0f),
			VerticalAlignment = VerticalAlignment.Center,
		};
		label.AddThemeFontSizeOverride("font_size", 16);
		row.AddChild(label);

		slider = new HSlider
		{
			MinValue = 0.0f,
			MaxValue = 100.0f,
			Step = 1.0f,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			CustomMinimumSize = new Vector2(110.0f, 0.0f),
		};
		row.AddChild(slider);

		valueLabel = new Label
		{
			Text = "100%",
			CustomMinimumSize = new Vector2(40.0f, 0.0f),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Center,
		};
		valueLabel.AddThemeFontSizeOverride("font_size", 16);
		row.AddChild(valueLabel);

		muteButton = CreateSmallButton("\u9759\u97F3");
		row.AddChild(muteButton);
		return row;
	}

	private Button CreateSmallButton(string text)
	{
		Button button = new()
		{
			Text = text,
			CustomMinimumSize = new Vector2(48.0f, 20.0f),
			FocusMode = Control.FocusModeEnum.None,
			ProcessMode = ProcessModeEnum.Always,
		};
		button.AddThemeFontSizeOverride("font_size", 16);
		return button;
	}

	private Button CreateWindowButton(string text)
	{
		Button button = CreateSmallButton(text);
		button.CustomMinimumSize = new Vector2(18.0f, 18.0f);
		return button;
	}

	private void OnDimGuiInput(InputEvent @event)
	{
		if (!_isOpen)
		{
			return;
		}

		if (@event is InputEventMouseButton mouseButton
			&& mouseButton.Pressed
			&& mouseButton.ButtonIndex == MouseButton.Left)
		{
			SetMenuVisible(false, playSound: true);
			GetViewport().SetInputAsHandled();
		}
	}

	private void SetMenuVisible(bool visible, bool playSound)
	{
		if (_isOpen == visible && _panel != null && _panel.Visible == visible)
		{
			return;
		}

		_isOpen = visible;
		_dim.Visible = visible;
		_panel.Visible = visible;
		if (visible)
		{
			RefreshFromAudio();
		}

		GetTree().Paused = visible;
		if (!playSound)
		{
			return;
		}

		if (visible)
		{
			GameAudio.Instance?.PlayUiConfirm();
		}
		else
		{
			GameAudio.Instance?.PlayUiCancel();
		}
	}

	public void ShowMenu(bool playSound = true)
	{
		SetMenuVisible(true, playSound);
	}

	public void HideMenu(bool playSound = true)
	{
		SetMenuVisible(false, playSound);
	}

	public bool IsMenuOpen()
	{
		return _isOpen;
	}

	private void RefreshFromAudio()
	{
		GameAudio? audio = GameAudio.Instance;
		if (audio == null)
		{
			return;
		}

		_isRefreshing = true;
		_masterSlider.Value = Mathf.Round(audio.GetMasterVolumeLinear() * 100.0f);
		_musicSlider.Value = Mathf.Round(audio.GetMusicVolumeLinear() * 100.0f);
		_sfxSlider.Value = Mathf.Round(audio.GetSfxVolumeLinear() * 100.0f);
		UpdateRowVisuals(_masterValueLabel, _masterMuteButton, (float)_masterSlider.Value, audio.IsMasterMuted());
		UpdateRowVisuals(_musicValueLabel, _musicMuteButton, (float)_musicSlider.Value, audio.IsMusicMuted());
		UpdateRowVisuals(_sfxValueLabel, _sfxMuteButton, (float)_sfxSlider.Value, audio.IsSfxMuted());
		_isRefreshing = false;
	}

	private static void UpdateRowVisuals(Label valueLabel, Button muteButton, float value, bool muted)
	{
		valueLabel.Text = $"{Mathf.RoundToInt(value)}%";
		muteButton.Text = muted ? "\u5DF2\u9759\u97F3" : "\u9759\u97F3";
	}

	private void OnMasterVolumeChanged(double value)
	{
		if (_isRefreshing || GameAudio.Instance == null)
		{
			return;
		}

		GameAudio.Instance.SetMasterVolumeLinear((float)value / 100.0f);
		UpdateRowVisuals(_masterValueLabel, _masterMuteButton, (float)value, GameAudio.Instance.IsMasterMuted());
	}

	private void OnMusicVolumeChanged(double value)
	{
		if (_isRefreshing || GameAudio.Instance == null)
		{
			return;
		}

		GameAudio.Instance.SetMusicVolumeLinear((float)value / 100.0f);
		UpdateRowVisuals(_musicValueLabel, _musicMuteButton, (float)value, GameAudio.Instance.IsMusicMuted());
	}

	private void OnSfxVolumeChanged(double value)
	{
		if (_isRefreshing || GameAudio.Instance == null)
		{
			return;
		}

		GameAudio.Instance.SetSfxVolumeLinear((float)value / 100.0f);
		UpdateRowVisuals(_sfxValueLabel, _sfxMuteButton, (float)value, GameAudio.Instance.IsSfxMuted());
	}

	private void OnMasterMutePressed()
	{
		if (GameAudio.Instance == null)
		{
			return;
		}

		bool nextMuted = !GameAudio.Instance.IsMasterMuted();
		if (nextMuted)
		{
			GameAudio.Instance.PlayUiToggleOff();
		}
		GameAudio.Instance.SetMasterMuted(nextMuted);
		if (!nextMuted)
		{
			GameAudio.Instance.PlayUiToggleOn();
		}

		RefreshFromAudio();
	}

	private void OnMusicMutePressed()
	{
		if (GameAudio.Instance == null)
		{
			return;
		}

		bool nextMuted = !GameAudio.Instance.IsMusicMuted();
		if (nextMuted)
		{
			GameAudio.Instance.PlayUiToggleOff();
		}

		GameAudio.Instance.SetMusicMuted(nextMuted);
		if (!nextMuted)
		{
			GameAudio.Instance.PlayUiToggleOn();
		}

		RefreshFromAudio();
	}

	private void OnSfxMutePressed()
	{
		if (GameAudio.Instance == null)
		{
			return;
		}

		bool nextMuted = !GameAudio.Instance.IsSfxMuted();
		if (nextMuted)
		{
			GameAudio.Instance.PlayUiToggleOff();
		}

		GameAudio.Instance.SetSfxMuted(nextMuted);
		if (!nextMuted)
		{
			GameAudio.Instance.PlayUiToggleOn();
		}

		RefreshFromAudio();
	}
}
