using CardChessDemo.Audio;
using Godot;

namespace CardChessDemo.UI;

public partial class MainMenuController : Control
{
	private const string Scene01Path = "res://Scene/Maps/Scene01.tscn";

	private Button _continueButton = null!;
	private Button _newGameButton = null!;
	private Button _settingsButton = null!;

	public override void _Ready()
	{
		GetTree().Paused = false;
		_continueButton = GetNode<Button>("Center/Panel/Margin/Content/Buttons/ContinueButton");
		_newGameButton = GetNode<Button>("Center/Panel/Margin/Content/Buttons/NewGameButton");
		_settingsButton = GetNode<Button>("Center/Panel/Margin/Content/Buttons/SettingsButton");

		_continueButton.Disabled = true;
		_continueButton.TooltipText = "存档系统接入后可从这里继续游戏。";
		_continueButton.Pressed += OnContinuePressed;
		_newGameButton.Pressed += OnNewGamePressed;
		_settingsButton.Pressed += OnSettingsPressed;

		GameAudio.Instance?.PlayMapMusic();
	}

	private void OnContinuePressed()
	{
		GameAudio.Instance?.PlayUiCancel();
	}

	private void OnNewGamePressed()
	{
		GameAudio.Instance?.PlayUiConfirm();
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
}
