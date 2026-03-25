using Godot;
using CardChessDemo.Battle.Shared;

public partial class Enemy : InteractableTemplate
{
	[Export] public string EncounterId = "debug_grunt";
	[Export(PropertyHint.File, "*.tscn")] public string BattleScenePath = "res://Scene/Battle/Battle.tscn";
	[Export] public string BusyText = "进入战斗中...";
	[Export] public bool DisableAfterInteract = true;

	private bool _isTransitioning = false;

	public override string GetInteractText(Player player)
	{
		if (_isTransitioning)
		{
			return BusyText;
		}

		if (!CanInteract(player))
		{
			return "已清理";
		}

		return string.IsNullOrWhiteSpace(PromptText) ? "发起战斗" : PromptText;
	}

	public override bool CanInteract(Player player)
	{
		if (_isTransitioning)
		{
			return false;
		}

		return base.CanInteract(player);
	}

	protected override void OnInteract(Player player)
	{
		if (string.IsNullOrWhiteSpace(BattleScenePath))
		{
			GD.PushError("Enemy: BattleScenePath 为空，无法切换战斗场景。");
			return;
		}

		_isTransitioning = true;

		GlobalGameSession session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		Node currentScene = GetTree().CurrentScene;
		string returnScenePath = currentScene?.SceneFilePath ?? string.Empty;
		Vector2 returnPlayerPosition = player?.GlobalPosition ?? Vector2.Zero;
		session?.SetPendingEncounterContext(EncounterId, returnScenePath, returnPlayerPosition);

		Error result = GetTree().ChangeSceneToFile(BattleScenePath.Trim());
		if (result != Error.Ok)
		{
			_isTransitioning = false;
			GD.PushError($"Enemy: 切换战斗场景失败，错误码={result}");
			return;
		}

		if (DisableAfterInteract)
		{
			IsDisabled = true;
		}
	}
}
