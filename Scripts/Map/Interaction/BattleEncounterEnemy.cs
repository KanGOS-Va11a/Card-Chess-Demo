using Godot;

namespace CardChessDemo.Map;

public partial class BattleEncounterEnemy : InteractableTemplate
{
	[Export] public string EnemyDisplayName = "Wanderer";
	[Export] public string BattleEncounterId = "grunt_debug";
	[Export] public PackedScene? BattleScene;
	[Export(PropertyHint.File, "*.tscn")] public string BattleScenePath = "res://Scene/Battle/Battle.tscn";
	[Export] public string BusyText = "战斗中...";
	[Export] public bool DisableAfterInteract = true;

	private bool _isTransitioning;

	public override string GetInteractText(Player player)
	{
		if (_isTransitioning)
		{
			return BusyText;
		}

		if (!CanInteract(player))
		{
			return "无法交战";
		}

		return string.IsNullOrWhiteSpace(PromptText) ? $"挑战 {EnemyDisplayName}" : PromptText;
	}

	public override bool CanInteract(Player player)
	{
		if (_isTransitioning)
		{
			return false;
		}

		return base.CanInteract(player)
			&& (BattleScene != null || !string.IsNullOrWhiteSpace(BattleScenePath))
			&& !string.IsNullOrWhiteSpace(BattleEncounterId);
	}

	protected override void OnInteract(Player player)
	{
		bool disableAfterInteract = DisableAfterInteract;
		if (disableAfterInteract)
		{
			IsDisabled = true;
		}

		_isTransitioning = true;
		PromptText = BusyText;
		if (!MapBattleTransitionHelper.TryEnterBattle(this, player, BattleScene, BattleScenePath, BattleEncounterId, out string failureReason, HandleDeferredBattleFailure))
		{
			_isTransitioning = false;
			if (disableAfterInteract)
			{
				IsDisabled = false;
			}

			PromptText = $"失败: {failureReason}";
			GD.PushError($"BattleEncounterEnemy: {failureReason}");
		}
	}

	private void HandleDeferredBattleFailure(string failureReason)
	{
		_isTransitioning = false;
		if (DisableAfterInteract)
		{
			IsDisabled = false;
		}

		PromptText = $"失败: {failureReason}";
		GD.PushError($"BattleEncounterEnemy: {failureReason}");
	}

	public override Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		Godot.Collections.Dictionary snapshot = base.BuildRuntimeSnapshot();
		snapshot["remove_from_scene"] = false;
		return snapshot;
	}

	public override void ApplyRuntimeSnapshot(Godot.Collections.Dictionary snapshot)
	{
		base.ApplyRuntimeSnapshot(snapshot);
		if (snapshot == null || snapshot.Count == 0)
		{
			return;
		}

		if (snapshot.TryGetValue("remove_from_scene", out Variant removeVariant) && removeVariant.AsBool())
		{
			HideAndRemoveEncounterRoot();
		}
	}

	private void HideAndRemoveEncounterRoot()
	{
		Node rootToRemove = GetParent() ?? this;
		if (rootToRemove is CanvasItem canvasItem)
		{
			canvasItem.Visible = false;
		}

		IsDisabled = true;
		CallDeferred(MethodName.QueueFree);
		if (rootToRemove != this)
		{
			rootToRemove.CallDeferred(MethodName.QueueFree);
		}
	}
}
