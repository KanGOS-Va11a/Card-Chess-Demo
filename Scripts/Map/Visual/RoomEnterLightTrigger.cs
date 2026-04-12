using System;
using Godot;

namespace CardChessDemo.Map;

/// <summary>
/// 玩家进入房间触发区后，让目标房间节点从暗色渐亮。
/// </summary>
public partial class RoomEnterLightTrigger : Node2D
{
	[Export] public NodePath TriggerAreaPath { get; set; } = new("TriggerArea");
	[Export] public NodePath RoomRootPath { get; set; } = new("../RoomRoot");
	[Export] public bool StartDimmed { get; set; } = true;
	[Export] public bool TriggerOnce { get; set; } = true;
	[Export] public bool DimWhenPlayerLeaves { get; set; } = false;
	[Export(PropertyHint.Range, "0.01,3.00,0.01")] public float TransitionSeconds { get; set; } = 0.35f;
	[Export] public Color DimColor { get; set; } = new Color(0.45f, 0.45f, 0.50f, 1.0f);
	[Export] public Color LitColor { get; set; } = Colors.White;

	private Area2D _triggerArea = null!;
	private CanvasItem _roomRoot = null!;
	private Tween _transitionTween;
	private bool _hasTriggered;
	private bool _isLit;
	private int _playerInsideCount;

	public override void _Ready()
	{
		_triggerArea = GetNodeOrNull<Area2D>(TriggerAreaPath)!;
		_roomRoot = GetNodeOrNull<CanvasItem>(RoomRootPath)!;

		if (_triggerArea == null)
		{
			GD.PushWarning($"RoomEnterLightTrigger({Name}): missing Area2D at path '{TriggerAreaPath}'.");
		}
		else
		{
			_triggerArea.BodyEntered += OnTriggerBodyEntered;
			_triggerArea.BodyExited += OnTriggerBodyExited;
		}

		if (_roomRoot == null)
		{
			GD.PushWarning($"RoomEnterLightTrigger({Name}): missing CanvasItem at path '{RoomRootPath}'.");
			return;
		}

		if (StartDimmed)
		{
			SetRoomColor(DimColor, false);
			_isLit = false;
		}
		else
		{
			SetRoomColor(LitColor, false);
			_isLit = true;
		}
	}

	public override void _ExitTree()
	{
		if (_triggerArea == null)
		{
			return;
		}

		_triggerArea.BodyEntered -= OnTriggerBodyEntered;
		_triggerArea.BodyExited -= OnTriggerBodyExited;
	}

	private void OnTriggerBodyEntered(Node2D body)
	{
		if (!IsPlayerBody(body))
		{
			return;
		}

		_playerInsideCount = Math.Max(1, _playerInsideCount + 1);
		if (TriggerOnce && _hasTriggered)
		{
			return;
		}

		_hasTriggered = true;
		SetLit(true);
	}

	private void OnTriggerBodyExited(Node2D body)
	{
		if (!IsPlayerBody(body))
		{
			return;
		}

		_playerInsideCount = Math.Max(0, _playerInsideCount - 1);
		if (!DimWhenPlayerLeaves || TriggerOnce || _playerInsideCount > 0)
		{
			return;
		}

		SetLit(false);
	}

	private bool IsPlayerBody(Node2D body)
	{
		if (body is Player)
		{
			return true;
		}

		Node parent = body.GetParent();
		return parent is Player;
	}

	private void SetLit(bool lit)
	{
		if (_roomRoot == null || _isLit == lit)
		{
			return;
		}

		_isLit = lit;
		SetRoomColor(_isLit ? LitColor : DimColor, true);
	}

	private void SetRoomColor(Color targetColor, bool animate)
	{
		if (_roomRoot == null)
		{
			return;
		}

		_transitionTween?.Kill();
		_transitionTween = null;

		if (!animate || TransitionSeconds <= 0.0f)
		{
			_roomRoot.Modulate = targetColor;
			return;
		}

		_transitionTween = CreateTween();
		_transitionTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		_transitionTween.TweenProperty(_roomRoot, "modulate", targetColor, TransitionSeconds);
	}
}