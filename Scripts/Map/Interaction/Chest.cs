using Godot;

namespace CardChessDemo.Map;

public partial class Chest : InteractableTemplate
{
	[Export] public string ChestName = "Sword Chest";
	[Export] public string ItemDescription = "你发现了一把生锈的剑。";

	private bool _isOpened;
	private bool _isOpening;
	private Sprite2D? _sprite;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
	}

	public override string GetInteractText(Player player)
	{
		if (_isOpening)
		{
			return "打开中...";
		}

		return _isOpened ? "已打开" : string.IsNullOrWhiteSpace(PromptText) ? "打开宝箱" : PromptText;
	}

	public override bool CanInteract(Player player)
	{
		return !_isOpened && !_isOpening && base.CanInteract(player);
	}

	protected override void OnInteract(Player player)
	{
		_isOpening = true;
		_isOpened = true;
		GD.Print($"{ChestName}：{ItemDescription}");

		if (HasNode("AnimationPlayer"))
		{
			GetNode<AnimationPlayer>("AnimationPlayer").Play("open");
		}

		PlayInteractionPulse();
		Tween tween = CreateTween();
		if (_sprite != null)
		{
			tween.TweenProperty(_sprite, "modulate", new Color(0.75f, 0.75f, 0.75f), 0.15f);
		}

		tween.Finished += () => _isOpening = false;
	}
}
