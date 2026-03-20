using Godot;

public partial class HealStation : StaticBody2D, IInteractable
{
	[Export] public int HealAmount = 30;
	[Export] public float CooldownSeconds = 1.5f;

	private ulong _nextAvailableTimeMs = 0;

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

		if (player.HasMethod("ReceiveHeal"))
		{
			player.Call("ReceiveHeal", HealAmount);
			GD.Print($"治疗站：恢复 {HealAmount} 点生命。");
		}
		else
		{
			GD.Print("治疗站：玩家尚未实现 ReceiveHeal(amount) 方法。");
		}

		_nextAvailableTimeMs = Time.GetTicksMsec() + (ulong)(CooldownSeconds * 1000.0f);

		Vector2 baseScale = Scale;
		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		tween.TweenProperty(this, "scale", baseScale * 1.08f, 0.08f);
		tween.TweenProperty(this, "scale", baseScale, 0.10f);
	}
}


// using Godot;

// public partial class HealingStation : StaticBody2D, IInteractable
// {
//     // 永远可以回血，或者你可以加个冷却变量
//     public bool CanInteract(Player player) => true;

//     public string GetInteractText(Player player) => "恢复生命值";

//     public void Interact(Player player)
//     {
//         GD.Print("回血站激活：玩家状态已恢复！");
//         // 视觉反馈：闪烁一下绿色
//         var sprite = GetNode<Sprite2D>("Sprite2D");
//         sprite.Modulate = Colors.Green;
		
//         // 1秒后恢复原色
//         GetTree().CreateTimer(1.0).Timeout += () => sprite.Modulate = Colors.White;
//     }
// }
