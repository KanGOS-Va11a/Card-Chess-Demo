using System;
using Godot;

namespace CardChessDemo.Map;

/// <summary>
/// 无贴图灯泡版本：玩家进入触发区后，点亮一个矩形范围的 2D 光照。
/// </summary>
public partial class AutoTriggerRectLight : Node2D
{
	[Export] public NodePath LightPath { get; set; } = new("RectLight2D");
	[Export] public NodePath TriggerAreaPath { get; set; } = new("TriggerArea");
	[Export] public bool StartLit { get; set; } = false;
	[Export] public bool TriggerOnce { get; set; } = true;
	[Export] public bool TurnOffWhenPlayerLeaves { get; set; } = false;
	[Export] public bool SyncTriggerShapeToLightSize { get; set; } = true;
	[Export] public Vector2 LightSize { get; set; } = new Vector2(320.0f, 192.0f);
	[Export(PropertyHint.Range, "0.00,0.49,0.01")] public float EdgeSoftness { get; set; } = 0.18f;
	[Export] public Color LightColor { get; set; } = new Color(1.0f, 0.96f, 0.85f, 1.0f);
	[Export(PropertyHint.Range, "0.01,8.00,0.01")] public float LightEnergy { get; set; } = 1.6f;

	private PointLight2D _light = null!;
	private Area2D _triggerArea = null!;
	private bool _isLit;
	private bool _hasTriggered;
	private int _playerInsideCount;

	public override void _Ready()
	{
		_light = GetNodeOrNull<PointLight2D>(LightPath)!;
		_triggerArea = GetNodeOrNull<Area2D>(TriggerAreaPath)!;

		if (_light == null)
		{
			GD.PushWarning($"AutoTriggerRectLight({Name}): missing PointLight2D at path '{LightPath}'.");
		}
		else
		{
			ConfigureRectLight();
		}

		if (_triggerArea == null)
		{
			GD.PushWarning($"AutoTriggerRectLight({Name}): missing Area2D at path '{TriggerAreaPath}'.");
		}
		else
		{
			_triggerArea.BodyEntered += OnTriggerBodyEntered;
			_triggerArea.BodyExited += OnTriggerBodyExited;
			if (SyncTriggerShapeToLightSize)
			{
				SyncTriggerShape();
			}
		}

		SetLit(StartLit);
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

	private void ConfigureRectLight()
	{
		if (_light == null)
		{
			return;
		}

		Vector2 clampedSize = new Vector2(
			Mathf.Max(32.0f, LightSize.X),
			Mathf.Max(32.0f, LightSize.Y));

		ImageTexture rectTexture = BuildRectTexture((int)Mathf.Round(clampedSize.X), (int)Mathf.Round(clampedSize.Y));
		_light.Texture = rectTexture;
		_light.TextureScale = 1.0f;
		_light.Color = LightColor;
		_light.Energy = LightEnergy;
	}

	private ImageTexture BuildRectTexture(int width, int height)
	{
		int texWidth = Mathf.Clamp(width, 16, 2048);
		int texHeight = Mathf.Clamp(height, 16, 2048);
		float softness = Mathf.Clamp(EdgeSoftness, 0.0f, 0.49f);

		Image image = Image.Create(texWidth, texHeight, false, Image.Format.Rgba8);
		for (int y = 0; y < texHeight; y++)
		{
			float v = (y + 0.5f) / texHeight;
			float ny = Mathf.Abs(v * 2.0f - 1.0f);
			for (int x = 0; x < texWidth; x++)
			{
				float u = (x + 0.5f) / texWidth;
				float nx = Mathf.Abs(u * 2.0f - 1.0f);
				float edge = Mathf.Max(nx, ny);

				float alpha;
				if (softness <= 0.0001f)
				{
					alpha = edge <= 1.0f ? 1.0f : 0.0f;
				}
				else
				{
					float start = 1.0f - softness;
					alpha = 1.0f - Mathf.SmoothStep(start, 1.0f, edge);
				}

				image.SetPixel(x, y, new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp(alpha, 0.0f, 1.0f)));
			}
		}

		return ImageTexture.CreateFromImage(image);
	}

	private void SyncTriggerShape()
	{
		if (_triggerArea == null)
		{
			return;
		}

		CollisionShape2D shapeNode = _triggerArea.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		if (shapeNode == null)
		{
			return;
		}

		if (shapeNode.Shape is RectangleShape2D rect)
		{
			rect.Size = new Vector2(Mathf.Max(16.0f, LightSize.X), Mathf.Max(16.0f, LightSize.Y));
		}
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
		if (!TurnOffWhenPlayerLeaves || TriggerOnce || _playerInsideCount > 0)
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
		_isLit = lit;
		if (_light == null)
		{
			return;
		}

		_light.Enabled = _isLit;
		if (_isLit)
		{
			_light.Energy = LightEnergy;
		}
	}
}