using Godot;

namespace CardChessDemo.Map;

public partial class DecorLightBulb : Node2D
{
	[Export] public NodePath LightPath { get; set; } = new("PointLight2D");
	[Export] public float BaseEnergy { get; set; } = 1.1f;
	[Export] public float FlickerAmplitude { get; set; } = 0.08f;
	[Export] public float FlickerSpeed { get; set; } = 4.0f;
	[Export] public bool EnableFlicker { get; set; } = true;

	private PointLight2D? _light;
	private float _timeOffset;

	public override void _Ready()
	{
		_light = GetNodeOrNull<PointLight2D>(LightPath);
		_timeOffset = GD.Randf() * 100.0f;
		ApplyEnergy();
	}

	public override void _Process(double delta)
	{
		if (!EnableFlicker || _light == null)
		{
			return;
		}

		float t = (float)(Time.GetTicksMsec() / 1000.0) + _timeOffset;
		float wave = Mathf.Sin(t * FlickerSpeed) * 0.5f + Mathf.Sin(t * FlickerSpeed * 2.37f) * 0.25f;
		_light.Energy = Mathf.Max(0.01f, BaseEnergy + wave * FlickerAmplitude);
	}

	private void ApplyEnergy()
	{
		if (_light == null)
		{
			return;
		}

		_light.Energy = BaseEnergy;
	}
}
