using Godot;
using CardChessDemo.Map;

public partial class Door : InteractableTemplate
{
	[Export] public PackedScene? NextScene;
	[Export(PropertyHint.File, "*.tscn")] public string NextScenePath = string.Empty;
	[Export] public bool UsesInSceneTeleport = false;
	[Export] public NodePath TeleportTargetPath = new("");
	[Export] public Vector2 TeleportOffset = Vector2.Zero;
	[Export(PropertyHint.Range, "0.05,1.50,0.01")] public float FadeOutSeconds = 0.22f;
	[Export(PropertyHint.Range, "0.00,1.50,0.01")] public float BlackScreenHoldSeconds = 0.05f;
	[Export(PropertyHint.Range, "0.05,1.50,0.01")] public float FadeInSeconds = 0.22f;
	[Export] public bool LockPlayerInputDuringTeleport = true;
	[Export] public string BusyText = "切换中...";

	private bool _isTransitioning;

	public override void _Ready()
	{
		if (!HasValidDestination())
		{
			GD.PushWarning("Door: 未配置有效目标，请设置同场景传送目标或 NextScene/NextScenePath。");
		}
	}

	public override string GetInteractText(Player player)
	{
		if (_isTransitioning)
		{
			return BusyText;
		}

		if (!CanInteract(player))
		{
			return "无法进入";
		}

		return string.IsNullOrWhiteSpace(PromptText) ? "进入下一场景" : PromptText;
	}

	public override bool CanInteract(Player player)
	{
		if (_isTransitioning)
		{
			return false;
		}

		return base.CanInteract(player) && HasValidDestination();
	}

	protected override void OnInteract(Player player)
	{
		if (UsesInSceneTeleport)
		{
			TeleportWithinScene(player);
			return;
		}

		if (NextScene == null && string.IsNullOrWhiteSpace(NextScenePath))
		{
			GD.PushError("Door: 目标场景未配置，请在 Inspector 设置 NextScene 或 NextScenePath。");
			return;
		}

		_isTransitioning = true;
		Error result = ChangeToConfiguredScene();
		if (result != Error.Ok)
		{
			_isTransitioning = false;
			GD.PushError($"Door: 切换场景失败，错误码={result}，NextScenePath='{NextScenePath}'");
		}
	}

	private Error ChangeToConfiguredScene()
	{
		if (NextScene != null)
		{
			return GetTree().ChangeSceneToPacked(NextScene);
		}

		string rawPath = NextScenePath?.Trim() ?? string.Empty;
		if (string.IsNullOrEmpty(rawPath))
		{
			return Error.InvalidParameter;
		}

		if (rawPath.StartsWith("uid://"))
		{
			PackedScene? byUid = ResourceLoader.Load<PackedScene>(rawPath);
			if (byUid == null)
			{
				return Error.CantOpen;
			}

			return GetTree().ChangeSceneToPacked(byUid);
		}

		string normalizedPath = NormalizeLegacyScenePath(rawPath);
		if (!ResourceLoader.Exists(normalizedPath))
		{
			return Error.CantOpen;
		}

		return GetTree().ChangeSceneToFile(normalizedPath);
	}

	private static string NormalizeLegacyScenePath(string path)
	{
		if (path.StartsWith("res://Scene(garbage)/"))
		{
			return path.Replace("res://Scene(garbage)/", "res://Scene/");
		}

		return path;
	}

	private bool HasValidDestination()
	{
		if (UsesInSceneTeleport)
		{
			if (!TeleportTargetPath.IsEmpty)
			{
				return ResolveTeleportTarget() != null;
			}

			return TeleportOffset != Vector2.Zero;
		}

		return NextScene != null || !string.IsNullOrWhiteSpace(NextScenePath);
	}

	private async void TeleportWithinScene(Player player)
	{
		if (_isTransitioning)
		{
			return;
		}

		_isTransitioning = true;
		bool previousPhysics = player.IsPhysicsProcessing();
		bool previousInput = player.IsProcessingUnhandledInput();
		if (LockPlayerInputDuringTeleport)
		{
			player.SetPhysicsProcess(false);
			player.SetProcessUnhandledInput(false);
		}

		ColorRect fadeRect = EnsureFadeOverlay();
		fadeRect.Visible = true;
		fadeRect.Modulate = new Color(0f, 0f, 0f, 0f);

		await ToSignal(FadeOverlayTo(fadeRect, 1f, Mathf.Max(0.01f, FadeOutSeconds)), Tween.SignalName.Finished);

		if (BlackScreenHoldSeconds > 0f)
		{
			await ToSignal(GetTree().CreateTimer(BlackScreenHoldSeconds), SceneTreeTimer.SignalName.Timeout);
		}

		Node2D? targetNode = ResolveTeleportTarget();
		if (targetNode != null)
		{
			player.GlobalPosition = targetNode.GlobalPosition + TeleportOffset;
		}
		else
		{
			player.GlobalPosition += TeleportOffset;
		}

		await ToSignal(FadeOverlayTo(fadeRect, 0f, Mathf.Max(0.01f, FadeInSeconds)), Tween.SignalName.Finished);
		fadeRect.Visible = false;

		if (LockPlayerInputDuringTeleport)
		{
			player.SetPhysicsProcess(previousPhysics);
			player.SetProcessUnhandledInput(previousInput);
		}

		_isTransitioning = false;
	}

	private Node2D? ResolveTeleportTarget()
	{
		if (TeleportTargetPath.IsEmpty)
		{
			return null;
		}

		Node sceneRoot = GetTree().CurrentScene ?? GetTree().Root;
		return sceneRoot.GetNodeOrNull<Node2D>(TeleportTargetPath);
	}

	private static Tween FadeOverlayTo(ColorRect fadeRect, float targetAlpha, float duration)
	{
		Tween tween = fadeRect.CreateTween();
		tween.SetEase(Tween.EaseType.InOut);
		tween.SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(fadeRect, "modulate:a", targetAlpha, duration);
		return tween;
	}

	private ColorRect EnsureFadeOverlay()
	{
		Node sceneRoot = GetTree().CurrentScene ?? GetTree().Root;
		CanvasLayer? fadeLayer = sceneRoot.GetNodeOrNull<CanvasLayer>("DoorFadeLayer");
		if (fadeLayer == null)
		{
			fadeLayer = new CanvasLayer
			{
				Name = "DoorFadeLayer",
				Layer = 90,
			};
			sceneRoot.AddChild(fadeLayer);
		}

		ColorRect? fadeRect = fadeLayer.GetNodeOrNull<ColorRect>("DoorFadeRect");
		if (fadeRect == null)
		{
			fadeRect = new ColorRect
			{
				Name = "DoorFadeRect",
				AnchorLeft = 0f,
				AnchorTop = 0f,
				AnchorRight = 1f,
				AnchorBottom = 1f,
				OffsetLeft = 0f,
				OffsetTop = 0f,
				OffsetRight = 0f,
				OffsetBottom = 0f,
				MouseFilter = Control.MouseFilterEnum.Ignore,
				Color = Colors.Black,
				Visible = false,
			};
			fadeLayer.AddChild(fadeRect);
		}

		return fadeRect;
	}
}
