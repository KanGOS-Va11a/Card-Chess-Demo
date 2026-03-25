using Godot;

namespace CardChessDemo.Map;

public partial class HealStation : InteractableTemplate
{
	[Export] public int HealAmount = 30;

	public override void _Ready()
	{
		if (Mathf.IsZeroApprox(CooldownSeconds))
		{
			CooldownSeconds = 1.5f;
		}
	}

	public override string GetInteractText(Player player)
	{
		return CanInteract(player) ? string.IsNullOrWhiteSpace(PromptText) ? "治疗" : PromptText : "冷却中";
	}

	protected override void OnInteract(Player player)
	{
		player.ReceiveHeal(HealAmount);
		GD.Print($"治疗站：恢复 {HealAmount} 点生命。");
		PlayInteractionPulse();
	}
}
