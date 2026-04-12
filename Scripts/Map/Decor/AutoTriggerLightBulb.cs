using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Map;

/// <summary>
/// 玩家进入触发区域后自动点亮的灯泡。
/// 默认一次触发后常亮，也可配置离开后熄灭。
/// </summary>
public partial class AutoTriggerLightBulb : Node2D
{
	[Export] public NodePath LightPath { get; set; } = new("PointLight2D");
	[Export] public NodePath TriggerAreaPath { get; set; } = new("TriggerArea");
	[Export] public bool StartLit { get; set; } = false;
	[Export] public bool TriggerOnce { get; set; } = true;
	[Export] public bool TurnOffWhenPlayerLeaves { get; set; } = false;
	[Export] public bool RequireLineOfSight { get; set; } = true;
	[Export(PropertyHint.Layers2DPhysics)] public uint ObstacleMask { get; set; } = 1u << 1;
	[Export] public float BaseEnergy { get; set; } = 1.1f;
	[Export] public float FlickerAmplitude { get; set; } = 0.08f;
	[Export] public float FlickerSpeed { get; set; } = 4.0f;
	[Export] public bool EnableFlicker { get; set; } = true;

	private PointLight2D _light = null!;
	private Area2D _triggerArea = null!;
	private float _timeOffset;
	private bool _isLit;
	private bool _hasTriggered;
	private readonly HashSet<Player> _playersInTrigger = new();

	public override void _Ready()
	{
		_light = GetNodeOrNull<PointLight2D>(LightPath)!;
		_triggerArea = GetNodeOrNull<Area2D>(TriggerAreaPath)!;
		_timeOffset = GD.Randf() * 100.0f;

		if (_light == null)
		{
			GD.PushWarning($"AutoTriggerLightBulb({Name}): missing PointLight2D at path '{LightPath}'.");
		}

		if (_triggerArea == null)
		{
			GD.PushWarning($"AutoTriggerLightBulb({Name}): missing Area2D at path '{TriggerAreaPath}'.");
		}
		else
		{
			_triggerArea.BodyEntered += OnTriggerBodyEntered;
			_triggerArea.BodyExited += OnTriggerBodyExited;
		}

		SetLit(StartLit, false);
		_hasTriggered = StartLit && TriggerOnce;
	}

	public override void _ExitTree()
	{
		if (_triggerArea == null)
		{
			return;
		}

		_triggerArea.BodyEntered -= OnTriggerBodyEntered;
		_triggerArea.BodyExited -= OnTriggerBodyExited;
		_playersInTrigger.Clear();
	}

	public override void _PhysicsProcess(double delta)
	{
		EvaluateTriggerState(allowPulse: true);
	}

	public override void _Process(double delta)
	{
		if (!_isLit || !EnableFlicker || _light == null)
		{
			return;
		}

		float t = (float)(Time.GetTicksMsec() / 1000.0) + _timeOffset;
		float wave = Mathf.Sin(t * FlickerSpeed) * 0.5f + Mathf.Sin(t * FlickerSpeed * 2.37f) * 0.25f;
		_light.Energy = Mathf.Max(0.01f, BaseEnergy + wave * FlickerAmplitude);
	}

	private void OnTriggerBodyEntered(Node2D body)
	{
		if (body is not Player player)
		{
			return;
		}

		_playersInTrigger.Add(player);
		EvaluateTriggerState(allowPulse: true);
	}

	private void OnTriggerBodyExited(Node2D body)
	{
		if (body is not Player player)
		{
			return;
		}

		_playersInTrigger.Remove(player);
		EvaluateTriggerState(allowPulse: false);
	}

	private void EvaluateTriggerState(bool allowPulse)
	{
		RemoveInvalidTrackedPlayers();

		if (HasVisiblePlayerInTrigger())
		{
			if (TriggerOnce && _hasTriggered)
			{
				return;
			}

			if (_isLit)
			{
				return;
			}

			_hasTriggered = true;
			SetLit(true, allowPulse);
			return;
		}

		if (!TurnOffWhenPlayerLeaves || TriggerOnce || !_isLit)
		{
			return;
		}

		SetLit(false, false);
	}

	private void RemoveInvalidTrackedPlayers()
	{
		_playersInTrigger.RemoveWhere(static player =>
			!GodotObject.IsInstanceValid(player) || player.IsQueuedForDeletion());
	}

	private bool HasVisiblePlayerInTrigger()
	{
		foreach (Player player in _playersInTrigger)
		{
			if (HasLineOfSightToPlayer(player))
			{
				return true;
			}
		}

		return false;
	}

	private bool HasLineOfSightToPlayer(Player player)
	{
		if (!RequireLineOfSight)
		{
			return true;
		}

		if (ObstacleMask == 0)
		{
			return true;
		}

		PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(GlobalPosition, player.GlobalPosition, ObstacleMask);
		query.CollideWithAreas = false;
		query.CollideWithBodies = true;
		query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };

		if (_triggerArea != null)
		{
			query.Exclude.Add(_triggerArea.GetRid());
		}

		Godot.Collections.Dictionary hit = GetWorld2D().DirectSpaceState.IntersectRay(query);
		return hit.Count == 0;
	}

	private void SetLit(bool lit, bool pulse)
	{
		_isLit = lit;
		if (_light == null)
		{
			return;
		}

		_light.Enabled = _isLit;
		if (!_isLit)
		{
			return;
		}

		_light.Energy = BaseEnergy;
		if (pulse)
		{
			PlayPulse();
		}
	}

	private void PlayPulse()
	{
		Vector2 baseScale = Scale;
		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		tween.TweenProperty(this, "scale", baseScale * 1.05f, 0.08f);
		tween.TweenProperty(this, "scale", baseScale, 0.10f);
	}
}