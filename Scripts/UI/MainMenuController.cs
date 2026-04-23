using CardChessDemo.Audio;
using CardChessDemo.Battle.Shared;
using Godot;

namespace CardChessDemo.UI;

public partial class MainMenuController : Control
{
	private const string Scene01Path = "res://Scene/Maps/Scene01.tscn";

	private Button _continueButton = null!;
	private Button _newGameButton = null!;
	private Button _settingsButton = null!;
	private Label _titleLabel = null!;
	private Label _subtitleLabel = null!;
	private Label _hintLabel = null!;
	private GlobalGameSession? _session;

	public override void _Ready()
	{
		GetTree().Paused = false;
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		_titleLabel = GetNode<Label>("Center/Panel/Margin/Content/Title");
		_subtitleLabel = GetNode<Label>("Center/Panel/Margin/Content/Subtitle");
		_hintLabel = GetNode<Label>("Center/Panel/Margin/Content/Hint");
		_continueButton = GetNode<Button>("Center/Panel/Margin/Content/Buttons/ContinueButton");
		_newGameButton = GetNode<Button>("Center/Panel/Margin/Content/Buttons/NewGameButton");
		_settingsButton = GetNode<Button>("Center/Panel/Margin/Content/Buttons/SettingsButton");

		_titleLabel.Text = "\u4EE3\u53F7\uFF1A\u6DF1\u7A7A\uFF0C\u5DE6\u8F6E\u4E0E\u6D41\u6D6A\u8005";
		_subtitleLabel.Visible = false;
		_subtitleLabel.Text = string.Empty;
		_hintLabel.Visible = false;
		_hintLabel.Text = string.Empty;

		_continueButton.Text = "\u7EE7\u7EED\u6E38\u620F";
		_newGameButton.Text = "\u8FDB\u5165\u6E38\u620F";
		_settingsButton.Text = "\u8BBE\u7F6E";
		RefreshContinueAvailability();

		_continueButton.Pressed += OnContinuePressed;
		_newGameButton.Pressed += OnNewGamePressed;
		_settingsButton.Pressed += OnSettingsPressed;

		GameAudio.Instance?.PlayMapMusic();
	}

	private void OnContinuePressed()
	{
		if (_session == null)
		{
			GameAudio.Instance?.PlayUiCancel();
			return;
		}

		if (!_session.TryLoadPrimarySave(out string scenePath, out string failureReason))
		{
			GD.PushError($"MainMenuController: continue failed. {failureReason}");
			RefreshContinueAvailability();
			GameAudio.Instance?.PlayUiCancel();
			return;
		}

		GameAudio.Instance?.PlayUiConfirm();
		GetTree().Paused = false;
		Error error = GetTree().ChangeSceneToFile(scenePath);
		if (error != Error.Ok)
		{
			GD.PushError($"MainMenuController: failed to load save scene '{scenePath}', error={error}");
		}
	}

	private void OnNewGamePressed()
	{
		GameAudio.Instance?.PlayUiConfirm();
		_session?.DeletePrimarySave(out _);
		_session?.ResetToNewGameDefaults();
		if (EscapeSettingsOverlay.Instance is not null && EscapeSettingsOverlay.Instance.IsMenuOpen())
		{
			EscapeSettingsOverlay.Instance.HideMenu(playSound: false);
		}

		GetTree().Paused = false;
		Error error = GetTree().ChangeSceneToFile(Scene01Path);
		if (error != Error.Ok)
		{
			GD.PushError($"MainMenuController: failed to load '{Scene01Path}', error={error}");
		}
	}

	private void OnSettingsPressed()
	{
		if (EscapeSettingsOverlay.Instance == null)
		{
			GameAudio.Instance?.PlayUiCancel();
			return;
		}

		EscapeSettingsOverlay.Instance.ShowMenu(playSound: true);
	}

	private void RefreshContinueAvailability()
	{
		bool hasSave = _session?.HasPrimarySave() == true;
		_continueButton.Disabled = !hasSave;
		_continueButton.TooltipText = hasSave ? string.Empty : "\u6682\u65E0\u5B58\u6863";
	}
}
