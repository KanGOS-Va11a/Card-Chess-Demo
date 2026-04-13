using System;
using System.Collections.Generic;
using Godot;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float Friction = 0.2f;
	[Export(PropertyHint.Range, "8,128,1")] public int GridTileSize = 16;
	[Export] public bool SnapToGridOnReady = true;
	[Export] public NodePath AnimatedSpritePath = "AnimatedSprite2D";
	[Export(PropertyHint.Range, "16,128,1")] public int SpriteFrameWidth = 48;
	[Export(PropertyHint.Range, "16,128,1")] public int SpriteFrameHeight = 64;
	[Export(PropertyHint.Range, "1,24,1")] public int SpriteFps = 10;
	[Export(PropertyHint.Range, "16,240,1")] public float InteractionRange = 96.0f;
	[Export] public NodePath InteractionHintLabelPath = "../UI/InteractionHintLabel";
	[Export(PropertyHint.Range, "10,89,1")] public float ViewConeHalfAngleDeg = 45.0f;
	[Export] public bool RequireLineOfSight = false;
	[Export] public uint InteractionObstacleMask = uint.MaxValue;
	[Export] public float InteractionStickyWindow = 0.35f;
	[Export] public float InteractionStickyBonus = 0.08f;
	[Export] public bool ShowInteractionGizmo = false;
	[Export] public Color GizmoRangeColor = new Color(0.2f, 0.85f, 1.0f, 0.35f);
	[Export] public Color GizmoConeColor = new Color(0.3f, 1.0f, 0.6f, 0.85f);
	[Export] public Color GizmoForwardColor = new Color(1.0f, 0.85f, 0.25f, 0.95f);

	private Vector2 _lastFacingDirection = Vector2.Down;
	private Area2D _interactionArea = null!;
	private Area2D? _lastInteractedArea;
	private ulong _lastInteractTimeMs;
	private GlobalGameSession? _globalSession;
	private AnimatedSprite2D? _animatedSprite;
	private Label? _interactionHintLabel;
	private string _activeAnimation = string.Empty;
	private bool _isGridMoving;
	private Vector2 _gridMoveTarget = Vector2.Zero;
	private bool _preferHorizontalInput = true;
	private readonly HashSet<InteractableTemplate> _highlightedInteractables = new();

	public override void _Ready()
	{
		_globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		_interactionArea = GetNode<Area2D>("InteractionArea");
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>(AnimatedSpritePath);
		_interactionHintLabel = GetNodeOrNull<Label>(InteractionHintLabelPath);
		if (SnapToGridOnReady)
		{
			GlobalPosition = SnapToGrid(GlobalPosition);
		}
		SetupMainPlayerAnimations();

		float areaRadius = GetInteractionAreaRadius();
		if (areaRadius > 0.0f && InteractionRange < areaRadius)
		{
			InteractionRange = areaRadius;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		UpdateInputPriority();

		if (_isGridMoving)
		{
			ContinueGridMove((float)delta);
			UpdateMovementAnimation(_lastFacingDirection);
			return;
		}

		Vector2 inputDirection = ReadCardinalInput();
		if (inputDirection != Vector2.Zero)
		{
			TryBeginGridMove(inputDirection);
			if (_isGridMoving)
			{
				ContinueGridMove((float)delta);
				UpdateMovementAnimation(_lastFacingDirection);
				return;
			}
		}

		Velocity = Vector2.Zero;
		UpdateMovementAnimation(Vector2.Zero);
	}

	public override void _Process(double delta)
	{
		UpdateNearbyInteractableHighlights();

		if (ShowInteractionGizmo)
		{
			QueueRedraw();
		}
	}

	public override void _ExitTree()
	{
		ClearNearbyInteractableHighlights();
	}

	public override void _Draw()
	{
		if (!ShowInteractionGizmo)
		{
			return;
		}

		Vector2 facing = _lastFacingDirection == Vector2.Zero ? Vector2.Down : _lastFacingDirection.Normalized();
		facing = facing.Rotated(-GlobalRotation);

		float halfAngleRad = Mathf.DegToRad(ViewConeHalfAngleDeg);
		float startAngle = facing.Angle() - halfAngleRad;
		float endAngle = facing.Angle() + halfAngleRad;

		DrawArc(Vector2.Zero, InteractionRange, 0.0f, Mathf.Tau, 64, GizmoRangeColor, 1.5f, true);
		DrawArc(Vector2.Zero, InteractionRange, startAngle, endAngle, 32, GizmoConeColor, 2.0f, true);

		Vector2 leftEdge = facing.Rotated(-halfAngleRad) * InteractionRange;
		Vector2 rightEdge = facing.Rotated(halfAngleRad) * InteractionRange;
		Vector2 forward = facing * InteractionRange;

		DrawLine(Vector2.Zero, leftEdge, GizmoConeColor, 2.0f, true);
		DrawLine(Vector2.Zero, rightEdge, GizmoConeColor, 2.0f, true);
		DrawLine(Vector2.Zero, forward, GizmoForwardColor, 2.5f, true);

		if (IsInstanceValid(_lastInteractedArea))
		{
			Vector2 targetLocal = ToLocal(_lastInteractedArea!.GlobalPosition);
			DrawLine(Vector2.Zero, targetLocal, new Color(1.0f, 0.4f, 0.2f, 0.8f), 1.5f, true);
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!@event.IsActionPressed("interact"))
		{
			return;
		}

		if (SceneTextOverlay.IsVisible(this))
		{
			SceneTextOverlay.Hide(this);
			GetViewport()?.SetInputAsHandled();
			return;
		}

		if (GalDialogueOverlay.IsVisible(this))
		{
			GalDialogueOverlay.Hide(this);
			GetViewport()?.SetInputAsHandled();
			return;
		}

		Node? currentScene = GetTree().CurrentScene;
		Control? tutorialPanel = currentScene?.GetNodeOrNull<Control>("TutorialUI/TutorialTipPanel");
		if (tutorialPanel != null && tutorialPanel.Visible)
		{
			return;
		}

		if (!TryGetFacingTileTarget(out IInteractable? bestTarget, out Area2D? bestArea) || bestTarget == null || bestArea == null)
		{
			return;
		}

		ulong nowMs = Time.GetTicksMsec();
		_lastInteractedArea = bestArea;
		_lastInteractTimeMs = nowMs;
		bestTarget.Interact(this);
		GetViewport()?.SetInputAsHandled();
	}

	private bool TryGetFacingTileTarget(out IInteractable? bestTarget, out Area2D? bestArea)
	{
		bestTarget = null;
		bestArea = null;

		Vector2 facing = _lastFacingDirection;
		if (facing == Vector2.Zero)
		{
			facing = Vector2.Down;
		}

		if (Mathf.Abs(facing.X) > Mathf.Abs(facing.Y))
		{
			facing = new Vector2(Mathf.Sign(facing.X), 0.0f);
		}
		else
		{
			facing = new Vector2(0.0f, Mathf.Sign(facing.Y));
		}

		float tileSize = Mathf.Max(1.0f, GridTileSize);
		Vector2 origin = SnapToGrid(GlobalPosition);
		Vector2 side = new Vector2(-facing.Y, facing.X);
		float bestScore = float.PositiveInfinity;

		foreach (Area2D area in _interactionArea.GetOverlappingAreas())
		{
			if (area.GetParent() is not IInteractable item || !item.CanInteract(this))
			{
				continue;
			}

			if (!HasLineOfSight(area))
			{
				continue;
			}

			Vector2 toTarget = area.GlobalPosition - origin;
			float along = toTarget.Dot(facing);
			float sideways = Mathf.Abs(toTarget.Dot(side));

			if (along < tileSize * 0.35f || along > tileSize * 1.65f)
			{
				continue;
			}

			if (sideways > tileSize * 0.55f)
			{
				continue;
			}

			float score = Mathf.Abs(along - tileSize) + sideways * 0.35f;
			if (score >= bestScore)
			{
				continue;
			}

			bestScore = score;
			bestTarget = item;
			bestArea = area;
		}

		return bestTarget != null;
	}

	public void ReceiveHeal(int amount)
	{
		if (amount <= 0 || _globalSession == null)
		{
			return;
		}

		_globalSession.SetPlayerCurrentHp(_globalSession.PlayerCurrentHp + amount);
	}

	private void SetupMainPlayerAnimations()
	{
		if (_animatedSprite == null)
		{
			GD.PushWarning("Player: missing AnimatedSprite2D, skip player animation setup.");
			return;
		}

		SpriteFrames frames = new();
		string idleDownPath = ResolvePreferredAnimationStripPath("res://Assets/Character/MainPlayer/Idle/Idle_Down-export.png", "res://Assets/Character/MainPlayer/Idle/Idle_Down.png");
		string idleUpPath = ResolvePreferredAnimationStripPath("res://Assets/Character/MainPlayer/Idle/Idle_Up-export.png", "res://Assets/Character/MainPlayer/Idle/Idle_Up.png");
		string idleRightPath = ResolvePreferredAnimationStripPath("res://Assets/Character/MainPlayer/Idle/Idle_Right_Down-export.png", "res://Assets/Character/MainPlayer/Idle/Idle_Right_Down.png");
		string walkDownPath = ResolvePreferredAnimationStripPath("res://Assets/Character/MainPlayer/Walk/walk_Down-export.png", "res://Assets/Character/MainPlayer/Walk/walk_Down.png");
		string walkUpPath = ResolvePreferredAnimationStripPath("res://Assets/Character/MainPlayer/Walk/walk_Up-export.png", "res://Assets/Character/MainPlayer/Walk/walk_Up.png");
		string walkRightPath = ResolvePreferredAnimationStripPath("res://Assets/Character/MainPlayer/Walk/walk_Right_Down-export.png", "res://Assets/Character/MainPlayer/Walk/walk_Right_Down.png");

		AddDirectionalStripAnimation(frames, "idle_down", idleDownPath, SpriteFps * 0.5f);
		AddDirectionalStripAnimation(frames, "idle_up", idleUpPath, SpriteFps * 0.5f);
		AddDirectionalStripAnimation(frames, "idle_left_down", idleRightPath);
		AddDirectionalStripAnimation(frames, "idle_right_down", idleRightPath);
		AddDirectionalStripAnimation(frames, "walk_down", walkDownPath);
		AddDirectionalStripAnimation(frames, "walk_up", walkUpPath);
		AddDirectionalStripAnimation(frames, "walk_left_down", walkRightPath);
		AddDirectionalStripAnimation(frames, "walk_right_down", walkRightPath);

		if (!frames.HasAnimation("idle_down"))
		{
			GD.PushWarning("Player: missing idle_down animation frames.");
			return;
		}

		_animatedSprite.SpriteFrames = frames;
		_activeAnimation = "idle_down";
		_animatedSprite.FlipH = false;
		_animatedSprite.Play(_activeAnimation);
	}

	private static string ResolvePreferredAnimationStripPath(string preferredPath, string fallbackPath)
	{
		return ResourceLoader.Exists(preferredPath) ? preferredPath : fallbackPath;
	}

	private void AddDirectionalStripAnimation(SpriteFrames frames, string animationName, string texturePath, float? fpsOverride = null)
	{
		Texture2D? stripTexture = ResourceLoader.Load<Texture2D>(texturePath);
		if (stripTexture == null)
		{
			GD.PushWarning($"Player: missing animation strip. path='{texturePath}'");
			return;
		}

		int frameWidth = ResolveFrameWidth(stripTexture);
		if (frameWidth <= 0)
		{
			return;
		}

		int frameHeight = SpriteFrameHeight > 0 ? Mathf.Min(SpriteFrameHeight, stripTexture.GetHeight()) : stripTexture.GetHeight();
		int frameCount = Mathf.Max(1, stripTexture.GetWidth() / frameWidth);

		frames.AddAnimation(animationName);
		frames.SetAnimationLoop(animationName, true);
		frames.SetAnimationSpeed(animationName, Mathf.Max(1.0f, fpsOverride ?? SpriteFps));

		for (int i = 0; i < frameCount; i++)
		{
			AtlasTexture frameTexture = new()
			{
				Atlas = stripTexture,
				Region = new Rect2(i * frameWidth, 0, frameWidth, frameHeight)
			};
			frames.AddFrame(animationName, frameTexture);
		}
	}

	private int ResolveFrameWidth(Texture2D stripTexture)
	{
		int width = stripTexture.GetWidth();
		if (SpriteFrameWidth > 0 && width % SpriteFrameWidth == 0)
		{
			return SpriteFrameWidth;
		}

		if (SpriteFrameHeight > 0 && width % SpriteFrameHeight == 0)
		{
			return SpriteFrameHeight;
		}

		GD.PushWarning($"Player: cannot auto-split animation strip. path='{stripTexture.ResourcePath}', width={width}");
		return width;
	}

	private void UpdateMovementAnimation(Vector2 inputDirection)
	{
		if (_animatedSprite == null || _animatedSprite.SpriteFrames == null)
		{
			return;
		}

		bool isMoving = inputDirection != Vector2.Zero;
		Vector2 direction = isMoving ? inputDirection.Normalized() : _lastFacingDirection;
		string directionKey = ResolveDirectionKey(direction);
		string nextAnimation = (isMoving ? "walk_" : "idle_") + directionKey;
		bool shouldFlipHorizontally = directionKey == "left_down";

		if (!_animatedSprite.SpriteFrames.HasAnimation(nextAnimation))
		{
			return;
		}

		_animatedSprite.FlipH = shouldFlipHorizontally;
		if (_activeAnimation == nextAnimation)
		{
			return;
		}

		_activeAnimation = nextAnimation;
		_animatedSprite.Play(_activeAnimation);
	}

	private static string ResolveDirectionKey(Vector2 direction)
	{
		if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
		{
			return direction.X < 0.0f ? "left_down" : "right_down";
		}

		if (Mathf.Abs(direction.Y) < 0.001f)
		{
			return "down";
		}

		return direction.Y < 0.0f ? "up" : "down";
	}

	private void UpdateInputPriority()
	{
		if (Input.IsActionJustPressed("move_left") || Input.IsActionJustPressed("move_right"))
		{
			_preferHorizontalInput = true;
		}

		if (Input.IsActionJustPressed("move_up") || Input.IsActionJustPressed("move_down"))
		{
			_preferHorizontalInput = false;
		}
	}

	private Vector2 ReadCardinalInput()
	{
		int horizontal = 0;
		if (Input.IsActionPressed("move_left"))
		{
			horizontal--;
		}
		if (Input.IsActionPressed("move_right"))
		{
			horizontal++;
		}

		int vertical = 0;
		if (Input.IsActionPressed("move_up"))
		{
			vertical--;
		}
		if (Input.IsActionPressed("move_down"))
		{
			vertical++;
		}

		if (horizontal != 0 && vertical != 0)
		{
			return _preferHorizontalInput
				? new Vector2(Mathf.Sign(horizontal), 0.0f)
				: new Vector2(0.0f, Mathf.Sign(vertical));
		}

		if (horizontal != 0)
		{
			return new Vector2(Mathf.Sign(horizontal), 0.0f);
		}

		if (vertical != 0)
		{
			return new Vector2(0.0f, Mathf.Sign(vertical));
		}

		return Vector2.Zero;
	}

	private void TryBeginGridMove(Vector2 inputDirection)
	{
		Vector2 direction = inputDirection == Vector2.Zero ? Vector2.Zero : inputDirection.Normalized();
		if (direction == Vector2.Zero)
		{
			return;
		}

		_lastFacingDirection = new Vector2(Mathf.Round(direction.X), Mathf.Round(direction.Y));
		Vector2 startPosition = SnapToGrid(GlobalPosition);
		GlobalPosition = startPosition;

		float tileSize = Mathf.Max(1.0f, GridTileSize);
		Vector2 motion = _lastFacingDirection * tileSize;
		if (TestMove(GlobalTransform, motion))
		{
			Velocity = Vector2.Zero;
			return;
		}

		_gridMoveTarget = startPosition + motion;
		_isGridMoving = true;
	}

	private void ContinueGridMove(float delta)
	{
		Vector2 toTarget = _gridMoveTarget - GlobalPosition;
		float remainingDistance = toTarget.Length();
		if (remainingDistance <= 0.01f)
		{
			GlobalPosition = _gridMoveTarget;
			Velocity = Vector2.Zero;
			_isGridMoving = false;
			return;
		}

		float speed = Mathf.Max(1.0f, Speed);
		float moveDistance = Mathf.Min(speed * delta, remainingDistance);
		Vector2 direction = toTarget / remainingDistance;
		Velocity = direction * speed;

		KinematicCollision2D? collision = MoveAndCollide(direction * moveDistance);
		if (collision != null)
		{
			GlobalPosition = SnapToGrid(GlobalPosition);
			Velocity = Vector2.Zero;
			_isGridMoving = false;
			return;
		}

		if ((_gridMoveTarget - GlobalPosition).Length() <= 0.1f)
		{
			GlobalPosition = _gridMoveTarget;
			Velocity = Vector2.Zero;
			_isGridMoving = false;
		}
	}

	private Vector2 SnapToGrid(Vector2 position)
	{
		float tileSize = Mathf.Max(1.0f, GridTileSize);
		float halfTile = tileSize * 0.5f;
		return new Vector2(
			Mathf.Round((position.X - halfTile) / tileSize) * tileSize + halfTile,
			Mathf.Round((position.Y - halfTile) / tileSize) * tileSize + halfTile);
	}

	private bool HasLineOfSight(Area2D targetArea)
	{
		if (!RequireLineOfSight)
		{
			return true;
		}

		PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create(GlobalPosition, targetArea.GlobalPosition, InteractionObstacleMask);
		query.CollideWithAreas = true;
		query.CollideWithBodies = true;
		query.Exclude = new Godot.Collections.Array<Rid> { GetRid(), _interactionArea.GetRid() };

		Godot.Collections.Dictionary hit = GetWorld2D().DirectSpaceState.IntersectRay(query);
		if (hit.Count == 0)
		{
			return true;
		}

		if (!hit.ContainsKey("collider"))
		{
			return false;
		}

		GodotObject collider = hit["collider"].AsGodotObject();
		return collider == targetArea || collider == targetArea.GetParent();
	}

	private void UpdateNearbyInteractableHighlights()
	{
		if (_interactionArea == null)
		{
			UpdateInteractionPrompt(null);
			return;
		}

		HashSet<InteractableTemplate> currentInteractables = new();
		InteractableTemplate? promptInteractable = null;
		float promptDistanceSquared = float.PositiveInfinity;
		foreach (Area2D area in _interactionArea.GetOverlappingAreas())
		{
			if (area.GetParent() is not InteractableTemplate interactable)
			{
				continue;
			}

			if (!interactable.CanInteract(this))
			{
				continue;
			}

			currentInteractables.Add(interactable);
			float distanceSquared = GlobalPosition.DistanceSquaredTo(interactable.GlobalPosition);
			if (distanceSquared < promptDistanceSquared)
			{
				promptDistanceSquared = distanceSquared;
				promptInteractable = interactable;
			}
			if (_highlightedInteractables.Contains(interactable))
			{
				continue;
			}

			interactable.SetInteractionHighlight(true);
		}

		List<InteractableTemplate> staleInteractables = new();
		foreach (InteractableTemplate interactable in _highlightedInteractables)
		{
			if (!GodotObject.IsInstanceValid(interactable) || currentInteractables.Contains(interactable))
			{
				continue;
			}

			staleInteractables.Add(interactable);
		}

		foreach (InteractableTemplate interactable in staleInteractables)
		{
			interactable.SetInteractionHighlight(false);
			_highlightedInteractables.Remove(interactable);
		}

		foreach (InteractableTemplate interactable in currentInteractables)
		{
			_highlightedInteractables.Add(interactable);
		}

		UpdateInteractionPrompt(promptInteractable);
	}

	private void ClearNearbyInteractableHighlights()
	{
		foreach (InteractableTemplate interactable in _highlightedInteractables)
		{
			if (GodotObject.IsInstanceValid(interactable))
			{
				interactable.SetInteractionHighlight(false);
			}
		}

		_highlightedInteractables.Clear();
		UpdateInteractionPrompt(null);
	}

	private void UpdateInteractionPrompt(InteractableTemplate? interactable)
	{
		if (_interactionHintLabel == null)
		{
			return;
		}

		if (interactable == null || !GodotObject.IsInstanceValid(interactable))
		{
			_interactionHintLabel.Visible = false;
			_interactionHintLabel.Text = string.Empty;
			return;
		}

		string prompt = interactable.GetInteractText(this)?.Trim() ?? string.Empty;
		if (string.IsNullOrWhiteSpace(prompt))
		{
			_interactionHintLabel.Visible = false;
			_interactionHintLabel.Text = string.Empty;
			return;
		}

		_interactionHintLabel.Text = $"E {prompt}";
		_interactionHintLabel.Visible = true;
	}

	private float GetInteractionAreaRadius()
	{
		if (_interactionArea == null || !_interactionArea.HasNode("CollisionShape2D"))
		{
			return 0.0f;
		}

		CollisionShape2D shapeNode = _interactionArea.GetNode<CollisionShape2D>("CollisionShape2D");
		if (shapeNode.Shape is CircleShape2D circle)
		{
			return circle.Radius;
		}

		if (shapeNode.Shape is RectangleShape2D rectangle)
		{
			return rectangle.Size.Length() * 0.5f;
		}

		return 0.0f;
	}
}
