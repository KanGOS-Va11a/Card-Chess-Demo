using Godot;

namespace CardChessDemo.Map;

public partial class SystemFeatureLabController
{
	private void UpdateStatusHint()
	{
		if (_statusLabel == null || _panelRoot == null)
		{
			return;
		}

		Player? player = PlayerPath.IsEmpty ? null : GetNodeOrNull<Player>(PlayerPath);
		if (player == null)
		{
			_statusLabel.Text = "未找到玩家节点";
			return;
		}

		if (TryGetPlayerInteractionHint(player, out string interactionHint))
		{
			_statusLabel.Text = interactionHint;
			return;
		}

		_statusLabel.Text = _panelRoot.Visible
			? "系统面板已打开，按 C 关闭"
			: "靠近可交互目标后按 E 交互";
	}

	private void ApplyReadableStatusHint()
	{
		UpdateStatusHint();
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
}
