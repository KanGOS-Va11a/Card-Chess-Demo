using Godot;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class SystemFeatureOverlayAdapter : Node
{
	private SystemFeatureLabController? _controller;
	private Control? _panelRoot;
	private ColorRect? _dimBlocker;
	private Label? _hintLabel;
	private Label? _statusLabel;
	private TabContainer? _tabs;
	private GlobalGameSession? _session;

	public override void _Ready()
	{
		SetProcessUnhandledInput(true);
		_controller = GetParent<SystemFeatureLabController>();
		_panelRoot = _controller?.GetNodeOrNull<Control>("PanelRoot");
		_dimBlocker = _controller?.GetNodeOrNull<ColorRect>("PanelRoot/Dim");
		_hintLabel = _controller?.GetNodeOrNull<Label>("HintLabel");
		_statusLabel = _controller?.GetNodeOrNull<Label>("StatusLabel");
		_tabs = _controller?.GetNodeOrNull<TabContainer>("PanelRoot/Window/Margin/Root/Tabs");
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");

		ApplyStaticUiText();
		if (_dimBlocker != null)
		{
			_dimBlocker.GuiInput += OnDimGuiInput;
		}
	}

	public override void _Process(double delta)
	{
		if (_panelRoot == null)
		{
			return;
		}

		if (_session?.PendingBattleRequest != null && _panelRoot.Visible)
		{
			SetMenuVisible(false);
			return;
		}

		UpdateStatusHint();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (_panelRoot == null)
		{
			return;
		}

		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		if (MapTextBlocker.IsBlockingTextVisible(this))
		{
			GetViewport().SetInputAsHandled();
			return;
		}

		if (keyEvent.Keycode == Key.C)
		{
			SetMenuVisible(!_panelRoot.Visible);
			GetViewport().SetInputAsHandled();
			return;
		}

		if (keyEvent.Keycode == Key.Escape && _panelRoot.Visible)
		{
			SetMenuVisible(false);
			GetViewport().SetInputAsHandled();
		}
	}

	private void ApplyStaticUiText()
	{
		if (_hintLabel != null)
		{
			_hintLabel.Text = "WASD Move  E Interact  C System";
		}

		if (_statusLabel != null)
		{
			_statusLabel.Text = "Approach an interactable target and press E";
		}

		if (_tabs != null)
		{
			_tabs.SetTabTitle(0, "Status");
			_tabs.SetTabTitle(1, "Bag");
			_tabs.SetTabTitle(2, "Talent");
			_tabs.SetTabTitle(3, "Codex");
			_tabs.SetTabTitle(4, "Deck");
		}
	}

	private void UpdateStatusHint()
	{
		if (_controller == null || _statusLabel == null || _panelRoot == null)
		{
			return;
		}

		Player? player = ResolvePlayer();
		if (player == null)
		{
			_statusLabel.Text = "Player node was not found";
			return;
		}

		if (TryGetPlayerInteractionHint(player, out string interactionHint))
		{
			_statusLabel.Text = interactionHint;
			return;
		}

		_statusLabel.Text = _panelRoot.Visible
			? "System menu is open. Press C or Esc to close"
			: "Approach an interactable target and press E. Press C for the system menu";
	}

	private static bool TryGetPlayerInteractionHint(Player player, out string hintText)
	{
		hintText = string.Empty;
		Label? hintLabel = player.GetNodeOrNull<Label>(player.InteractionHintLabelPath);
		if (hintLabel == null || !hintLabel.Visible)
		{
			return false;
		}

		string text = hintLabel.Text?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}

		hintText = text;
		return true;
	}

	private void OnDimGuiInput(InputEvent @event)
	{
		if (_panelRoot == null || !_panelRoot.Visible)
		{
			return;
		}

		if (@event is InputEventMouseButton mouseButton
			&& mouseButton.Pressed
			&& mouseButton.ButtonIndex == MouseButton.Left)
		{
			SetMenuVisible(false);
			GetViewport().SetInputAsHandled();
		}
	}

	private void SetMenuVisible(bool visible)
	{
		if (_panelRoot == null || _hintLabel == null || _controller == null)
		{
			return;
		}

		_panelRoot.Visible = visible;
		_hintLabel.Visible = !visible;
		SetPlayerInputEnabled(!visible);

		if (visible)
		{
			_controller.CallDeferred("RefreshAll");
		}
	}

	private void SetPlayerInputEnabled(bool enabled)
	{
		Player? player = ResolvePlayer();
		if (player == null)
		{
			return;
		}

		player.SetPhysicsProcess(enabled);
		player.SetProcess(enabled);
		player.SetProcessInput(enabled);
		player.SetProcessUnhandledInput(enabled);
	}

	private Player? ResolvePlayer()
	{
		if (_controller == null)
		{
			return null;
		}

		if (!_controller.PlayerPath.IsEmpty && _controller.GetNodeOrNull<Player>(_controller.PlayerPath) is Player byPath)
		{
			return byPath;
		}

		Node? sceneRoot = _controller.GetTree().CurrentScene ?? _controller;
		return sceneRoot.FindChild("Player", true, false) as Player;
	}
}
