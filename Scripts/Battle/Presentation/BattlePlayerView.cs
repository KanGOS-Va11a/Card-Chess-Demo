using Godot;
using CardChessDemo.Battle.State;

namespace CardChessDemo.Battle.Presentation;

public partial class BattlePlayerView : BattleAnimatedViewBase
{
	public void PlayDefend()
	{
		// 当前只有防御动画入口，真正的防御结算系统尚未实现。
		PlayNamedAnimation("defend");
	}

	public void PlayCustom(StringName animationName)
	{
		PlayNamedAnimation(animationName.ToString());
	}

	public override void PlayAction()
	{
		PlayCustom("action");
	}

	protected override SpriteFrames BuildFallbackFrames()
	{
		return CreateFrames(new Color(0.24f, 0.78f, 1.0f), new Color(0.8f, 0.96f, 1.0f));
	}
}
