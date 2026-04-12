using Godot;

namespace CardChessDemo.Map;

public partial class HealStation : InteractableTemplate
{
	[Export] public int HealAmount = 30;
	[Export] public string HealText = "\u6062\u590D\u4E86\u751F\u547D\u3002";

	public override void _Ready()
	{
		if (Mathf.IsZeroApprox(CooldownSeconds))
		{
			CooldownSeconds = 1.5f;
		}
	}

	public override string GetInteractText(Player player)
	{
		return CanInteract(player)
			? string.IsNullOrWhiteSpace(PromptText) ? "\u6CBB\u7597" : PromptText
			: "\u51B7\u5374\u4E2D";
	}

	protected override void OnInteract(Player player)
	{
		player.ReceiveHeal(HealAmount);
		SceneTextOverlay.Show(this, BuildHealMessage());
		PlayInteractionPulse();
	}

	private string BuildHealMessage()
	{
		if (!string.IsNullOrWhiteSpace(HealText))
		{
			return HealText;
		}

		return $"\u6062\u590D\u4E86 {HealAmount} \u70B9\u751F\u547D\u3002";
	}
}
