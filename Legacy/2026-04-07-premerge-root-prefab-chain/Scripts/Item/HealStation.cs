using Godot;
using CardChessDemo.Battle.Shared;

public partial class HealStation : StaticBody2D, IInteractable
{
	[Export] public int HealAmount = 30;
	[Export] public float CooldownSeconds = 1.5f;

	private ulong _nextAvailableTimeMs;

	public string GetInteractText(Player player)
	{
		return CanInteract(player) ? "治疗" : "冷却中";
	}

	public bool CanInteract(Player player)
	{
		return Time.GetTicksMsec() >= _nextAvailableTimeMs;
	}

	public void Interact(Player player)
	{
		if (!CanInteract(player))
		{
			return;
		}

		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (session == null)
		{
			GD.PushWarning("治疗站：未找到 GlobalGameSession，无法恢复状态。");
			return;
		}

		int playerHpMax = Mathf.Max(1, session.GetResolvedPlayerMaxHp());
		int playerHpDelta = playerHpMax - session.PlayerCurrentHp;
		if (playerHpDelta != 0)
		{
			session.ApplyResourceDelta("player_hp", playerHpDelta, 0, playerHpMax);
		}

		int partnerEnergyCap = Mathf.Max(1, session.ArakawaMaxEnergy);
		int partnerEnergyDelta = partnerEnergyCap - session.ArakawaCurrentEnergy;
		if (partnerEnergyDelta != 0)
		{
			session.ApplyResourceDelta("arakawa_energy", partnerEnergyDelta, 0, partnerEnergyCap);
		}

		GD.Print($"治疗站：已回满主角 HP({session.PlayerCurrentHp}/{playerHpMax}) 与伙伴 EN({session.ArakawaCurrentEnergy}/{partnerEnergyCap})。");

		_nextAvailableTimeMs = Time.GetTicksMsec() + (ulong)(CooldownSeconds * 1000.0f);

		Vector2 baseScale = Scale;
		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		tween.TweenProperty(this, "scale", baseScale * 1.08f, 0.08f);
		tween.TweenProperty(this, "scale", baseScale, 0.10f);
	}
}
