using Godot;

namespace CardChessDemo.Map;

public partial class BattleEncounterEnemy : InteractableTemplate
{
	[Export] public string EnemyDisplayName = "Wanderer";
	[Export] public string BattleEncounterId = "grunt_debug";
	[Export] public PackedScene? BattleScene;
	[Export(PropertyHint.File, "*.tscn")] public string BattleScenePath = "res://Scene/Battle/Battle.tscn";
	[Export] public string BusyText = "\u6218\u6597\u4E2D...";
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
			return "\u65E0\u6CD5\u4EA4\u6218";
		}

		return string.IsNullOrWhiteSpace(PromptText) ? $"\u6311\u6218 {EnemyDisplayName}" : PromptText;
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

	public bool TriggerEncounterDirect(Player player)
	{
		if (!CanInteract(player))
		{
			return false;
		}

		return TryStartEncounter(player);
	}

	protected override void OnInteract(Player player)
	{
		TryStartEncounter(player);
	}

	private bool TryStartEncounter(Player player)
	{
		_isTransitioning = true;
		PromptText = BusyText;
		if (!MapBattleTransitionHelper.TryEnterBattle(this, player, BattleScene, BattleScenePath, BattleEncounterId, out string failureReason, HandleDeferredBattleFailure))
		{
			_isTransitioning = false;
			PromptText = $"\u5931\u8D25: {failureReason}";
			GD.PushError($"BattleEncounterEnemy: {failureReason}");
			return false;
		}

		if (DisableAfterInteract)
		{
			IsDisabled = true;
		}

		return true;
	}

	private void HandleDeferredBattleFailure(string failureReason)
	{
		_isTransitioning = false;
		if (DisableAfterInteract)
		{
			IsDisabled = false;
		}

		PromptText = $"\u5931\u8D25: {failureReason}";
		GD.PushError($"BattleEncounterEnemy: {failureReason}");
	}

	public override Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		Godot.Collections.Dictionary snapshot = base.BuildRuntimeSnapshot();
		snapshot["remove_from_scene"] = false;
		snapshot["disable_when_session_used"] = true;
		snapshot["remove_when_session_used"] = true;
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
		if (this is CanvasItem canvasItem)
		{
			canvasItem.Visible = false;
		}

		IsDisabled = true;
		CallDeferred(MethodName.QueueFree);
	}
}
