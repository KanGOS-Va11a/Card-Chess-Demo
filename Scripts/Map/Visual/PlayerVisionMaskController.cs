using Godot;

namespace CardChessDemo.Map;

/// <summary>
/// 在地图场景创建一个屏幕空间的昏暗遮罩，只保留玩家周围的圆形可视范围。
/// </summary>
public partial class PlayerVisionMaskController : Node
{
	[Export] public NodePath PlayerPath = new NodePath("MainPlayer/Player");
	[Export] public NodePath HintTargetPath = new NodePath("Enemy");
	[Export(PropertyHint.Range, "0.05,0.80,0.01")] public float VisibleRadius = 0.22f;
	[Export(PropertyHint.Range, "0.01,0.40,0.01")] public float EdgeSoftness = 0.12f;
	[Export] public Color DarknessColor = new Color(0.04f, 0.07f, 0.10f, 0.86f);
	[Export(PropertyHint.Range, "0.00,1.00,0.01")] public float VignetteStrength = 0.22f;
	[Export] public bool EnableSubtleFlicker = true;
	[Export(PropertyHint.Range, "0.000,0.100,0.001")] public float FlickerAmplitude = 0.010f;
	[Export(PropertyHint.Range, "0.100,12.000,0.050")] public float FlickerSpeed = 2.2f;
	[Export] public bool EnableHintLight = false;
	[Export(PropertyHint.Range, "0.02,0.40,0.01")] public float HintRadius = 0.10f;
	[Export(PropertyHint.Range, "0.01,0.40,0.01")] public float HintSoftness = 0.08f;
	[Export(PropertyHint.Range, "0.00,1.00,0.01")] public float HintStrength = 0.35f;
	[Export] public int OverlayLayer = 0;

	private Node2D _player;
	private Node2D _hintTarget;
	private CanvasLayer _overlayLayer;
	private ColorRect _overlayRect;
	private ShaderMaterial _overlayMaterial;
	private float _baseVisibleRadius;
	private float _currentVisibleRadius;

	private const string OverlayLayerName = "VisionMaskLayer";
	private const string OverlayRectName = "VisionMaskRect";

	private const string ShaderCode = @"shader_type canvas_item;
uniform vec2 center_uv = vec2(0.5, 0.5);
uniform float visible_radius = 0.22;
uniform float edge_softness = 0.12;
uniform vec4 darkness_color : source_color = vec4(0.04, 0.07, 0.10, 0.86);
uniform float vignette_strength = 0.22;
uniform vec2 hint_uv = vec2(-1.0, -1.0);
uniform float hint_radius = 0.10;
uniform float hint_softness = 0.08;
uniform float hint_strength = 0.35;

void fragment() {
	float dist = distance(UV, center_uv);
	float radial = smoothstep(visible_radius, visible_radius + edge_softness, dist);
	float vignette = smoothstep(0.38, 1.05, distance(UV, vec2(0.5)));
	float alpha = clamp(radial + vignette * vignette_strength, 0.0, 1.0) * darkness_color.a;

	if (hint_uv.x >= 0.0 && hint_uv.y >= 0.0) {
		float hint_dist = distance(UV, hint_uv);
		float hint_light = 1.0 - smoothstep(hint_radius, hint_radius + hint_softness, hint_dist);
		alpha = clamp(alpha - hint_light * hint_strength, 0.0, 1.0);
	}

	COLOR = vec4(darkness_color.rgb, alpha);
}";

	public override void _Ready()
	{
		_baseVisibleRadius = VisibleRadius;
		_currentVisibleRadius = VisibleRadius;
		_player = ResolvePlayer();
		_hintTarget = ResolveHintTarget();
		CallDeferred(nameof(InitializeOverlayDeferred));
	}

	public float GetCurrentVisibleRadiusPixels()
	{
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f)
		{
			return 0.0f;
		}

		return Mathf.Min(viewportSize.X, viewportSize.Y) * _currentVisibleRadius;
	}

	public override void _Process(double delta)
	{
		if (!IsInstanceValid(_player))
		{
			_player = ResolvePlayer();
		}

		if (!IsInstanceValid(_hintTarget))
		{
			_hintTarget = ResolveHintTarget();
		}

		UpdateFlicker();

		UpdateCenterUv();
		UpdateHintUv();
	}

	private Node2D ResolvePlayer()
	{
		if (PlayerPath != null && !PlayerPath.IsEmpty)
		{
			Node2D byPath = GetNodeOrNull<Node2D>(PlayerPath);
			if (byPath != null)
			{
				return byPath;
			}
		}

		Node currentScene = GetTree().CurrentScene;
		if (currentScene == null)
		{
			return null;
		}

		Node candidate = currentScene.FindChild("Player", true, false);
		return candidate as Node2D;
	}

	private Node2D ResolveHintTarget()
	{
		if (HintTargetPath != null && !HintTargetPath.IsEmpty)
		{
			Node2D byPath = GetNodeOrNull<Node2D>(HintTargetPath);
			if (byPath != null)
			{
				return byPath;
			}
		}

		Node currentScene = GetTree().CurrentScene;
		if (currentScene == null)
		{
			return null;
		}

		Node candidate = currentScene.FindChild("Enemy", true, false);
		return candidate as Node2D;
	}

	private void EnsureOverlay()
	{
		Node currentScene = GetTree().CurrentScene;
		if (currentScene == null)
		{
			return;
		}

		_overlayLayer = currentScene.GetNodeOrNull<CanvasLayer>(OverlayLayerName);
		if (_overlayLayer == null)
		{
			_overlayLayer = new CanvasLayer
			{
				Name = OverlayLayerName,
				Layer = OverlayLayer,
			};
			currentScene.AddChild(_overlayLayer);
		}
		else
		{
			_overlayLayer.Layer = OverlayLayer;
		}

		_overlayRect = _overlayLayer.GetNodeOrNull<ColorRect>(OverlayRectName);
		if (_overlayRect == null)
		{
			_overlayRect = new ColorRect
			{
				Name = OverlayRectName,
				AnchorLeft = 0.0f,
				AnchorTop = 0.0f,
				AnchorRight = 1.0f,
				AnchorBottom = 1.0f,
				OffsetLeft = 0.0f,
				OffsetTop = 0.0f,
				OffsetRight = 0.0f,
				OffsetBottom = 0.0f,
				MouseFilter = Control.MouseFilterEnum.Ignore,
			};
			_overlayLayer.AddChild(_overlayRect);
		}

		_overlayMaterial = _overlayRect.Material as ShaderMaterial;
		if (_overlayMaterial == null)
		{
			Shader shader = new Shader();
			shader.Code = ShaderCode;
			_overlayMaterial = new ShaderMaterial { Shader = shader };
			_overlayRect.Material = _overlayMaterial;
		}
	}

	private void InitializeOverlayDeferred()
	{
		EnsureOverlay();
		ApplyStaticParams();
		UpdateCenterUv();
	}

	private void ApplyStaticParams()
	{
		if (_overlayMaterial == null)
		{
			return;
		}

		_overlayMaterial.SetShaderParameter("visible_radius", _baseVisibleRadius);
		_overlayMaterial.SetShaderParameter("edge_softness", EdgeSoftness);
		_overlayMaterial.SetShaderParameter("darkness_color", DarknessColor);
		_overlayMaterial.SetShaderParameter("vignette_strength", VignetteStrength);
		_overlayMaterial.SetShaderParameter("hint_radius", HintRadius);
		_overlayMaterial.SetShaderParameter("hint_softness", HintSoftness);
		_overlayMaterial.SetShaderParameter("hint_strength", HintStrength);
	}

	private void UpdateFlicker()
	{
		if (_overlayMaterial == null)
		{
			return;
		}

		if (!EnableSubtleFlicker || FlickerAmplitude <= 0.0f)
		{
			_currentVisibleRadius = _baseVisibleRadius;
			_overlayMaterial.SetShaderParameter("visible_radius", _currentVisibleRadius);
			return;
		}

		float t = (float)Time.GetTicksMsec() * 0.001f;
		float subtleWave = Mathf.Sin(t * FlickerSpeed);
		_currentVisibleRadius = Mathf.Max(0.02f, _baseVisibleRadius + subtleWave * FlickerAmplitude);
		_overlayMaterial.SetShaderParameter("visible_radius", _currentVisibleRadius);
	}

	private void UpdateCenterUv()
	{
		if (_overlayMaterial == null || !IsInstanceValid(_player))
		{
			return;
		}

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f)
		{
			return;
		}

		Vector2 screenPos = GetViewport().GetCanvasTransform() * _player.GlobalPosition;
		Vector2 centerUv = new Vector2(
			Mathf.Clamp(screenPos.X / viewportSize.X, 0.0f, 1.0f),
			Mathf.Clamp(screenPos.Y / viewportSize.Y, 0.0f, 1.0f));

		_overlayMaterial.SetShaderParameter("center_uv", centerUv);
	}

	private void UpdateHintUv()
	{
		if (_overlayMaterial == null)
		{
			return;
		}

		if (!EnableHintLight || HintStrength <= 0.0f)
		{
			_overlayMaterial.SetShaderParameter("hint_uv", new Vector2(-1.0f, -1.0f));
			return;
		}

		if (!IsInstanceValid(_hintTarget) || !IsInstanceValid(_player))
		{
			_overlayMaterial.SetShaderParameter("hint_uv", new Vector2(-1.0f, -1.0f));
			return;
		}

		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f)
		{
			return;
		}

		Transform2D canvasTransform = GetViewport().GetCanvasTransform();
		Vector2 hintScreenPos = canvasTransform * _hintTarget.GlobalPosition;
		if (hintScreenPos.X < 0.0f || hintScreenPos.X > viewportSize.X
			|| hintScreenPos.Y < 0.0f || hintScreenPos.Y > viewportSize.Y)
		{
			_overlayMaterial.SetShaderParameter("hint_uv", new Vector2(-1.0f, -1.0f));
			return;
		}

		Vector2 playerScreenPos = canvasTransform * _player.GlobalPosition;
		float currentRadiusPixels = GetCurrentVisibleRadiusPixels();
		if (currentRadiusPixels > 0.0f && playerScreenPos.DistanceTo(hintScreenPos) > currentRadiusPixels)
		{
			_overlayMaterial.SetShaderParameter("hint_uv", new Vector2(-1.0f, -1.0f));
			return;
		}

		Vector2 hintUv = new Vector2(
			hintScreenPos.X / viewportSize.X,
			hintScreenPos.Y / viewportSize.Y);

		_overlayMaterial.SetShaderParameter("hint_uv", hintUv);
	}
}
