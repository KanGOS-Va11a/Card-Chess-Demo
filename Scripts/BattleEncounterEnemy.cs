using Godot;

public partial class BattleEncounterEnemy : InteractableTemplate
{
    [Export] public string EnemyDisplayName = "Wanderer";
    [Export] public string EnemyTypeId = "grunt";
    [Export] public string BattleEncounterId = "grunt_debug";
    [Export] public PackedScene? BattleScene;
    [Export(PropertyHint.File, "*.tscn")] public string BattleScenePath = "res://Scene/Battle/Battle.tscn";

    private bool _isTransitioning;

    public override string GetInteractText(Player player)
    {
        if (_isTransitioning)
        {
            return "战斗中...";
        }

        if (!CanInteract(player))
        {
            return "无法接战";
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
        _isTransitioning = true;
        if (!MapBattleTransitionHelper.TryEnterBattle(this, player, BattleScene, BattleScenePath, BattleEncounterId, out string failureReason))
        {
            _isTransitioning = false;
            GD.PushError($"BattleEncounterEnemy: {failureReason}");
            return;
        }
    }
}
