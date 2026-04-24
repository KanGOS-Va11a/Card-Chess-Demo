using System;
using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Map;

/// <summary>
/// 通用交互物基类，所有交互物（宝箱、NPC、回血站等）推荐继承此类。
/// 只需实现 OnInteract 方法即可，冷却、禁用等逻辑自动处理。
/// </summary>
public abstract partial class InteractableTemplate : StaticBody2D, IInteractable
{
	private static readonly Shader InteractionOutlineShader = GD.Load<Shader>("res://Shaders/Battle/ActiveTurnOutline.gdshader");
	private const int DefaultGridTileSize = 16;
	[Export] public string InteractionId { get; set; } = string.Empty;
	[Export] public string DisplayName = "交互物";
	[Export] public string PromptText = "交互";
	[Export] public float CooldownSeconds = 0.0f;
	[Export] public bool IsDisabled = false;

	protected ulong _nextAvailableTimeMs = 0;
	protected bool IsOnCooldown => Time.GetTicksMsec() < _nextAvailableTimeMs;
	private readonly Dictionary<CanvasItem, Material?> _originalHighlightMaterials = new();
	private readonly Dictionary<CanvasItem, ShaderMaterial> _highlightMaterials = new();

	/// <summary>
	/// 获取交互提示文本。
	/// 默认返回 PromptText，禁用或冷却中会返回对应提示。
	/// 可被子类覆盖以实现自定义逻辑（如 "已打开" / "打开宝箱"）。
	/// </summary>
	public virtual string GetInteractText(Player player)
	{
		if (IsDisabled)
		{
			return "（已禁用）";
		}

		if (IsOnCooldown)
		{
			return "（冷却中）";
		}

		return PromptText;
	}

	/// <summary>
	/// 判断是否可以交互。
	/// 默认检查禁用状态和冷却状态。
	/// 可被子类覆盖以实现额外逻辑（如 "已打开的箱子不能再开"）。
	/// </summary>
	public virtual bool CanInteract(Player player)
	{
		if (IsDisabled)
		{
			return false;
		}

		return Time.GetTicksMsec() >= _nextAvailableTimeMs;
	}

	/// <summary>
	/// 接口实现：交互入口。
	/// 自动检查可交互性，然后调用子类的 OnInteract，最后应用冷却。
	/// 子类不应覆盖此方法，应覆盖 OnInteract。
	/// </summary>
	public void Interact(Player player)
	{
		if (!CanInteract(player))
		{
			return;
		}

		OnInteract(player);
		ApplyCooldown();
	}

	/// <summary>
	/// 子类实现具体交互行为。
	/// 例如：开箱、对话、回血等。
	/// </summary>
	protected abstract void OnInteract(Player player);

	/// <summary>
	/// 应用冷却。
	/// 可被子类覆盖以自定义冷却逻辑。
	/// </summary>
	protected virtual void ApplyCooldown()
	{
		if (CooldownSeconds > 0.0f)
		{
			_nextAvailableTimeMs = Time.GetTicksMsec() + (ulong)(CooldownSeconds * 1000.0f);
		}
	}

	protected void PlayInteractionPulse(Node2D? targetNode = null, float scaleFactor = 1.08f)
	{
		Node2D pulseTarget = targetNode ?? this;
		Vector2 baseScale = pulseTarget.Scale;
		Tween tween = pulseTarget.CreateTween();
		tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		tween.TweenProperty(pulseTarget, "scale", baseScale * scaleFactor, 0.08f);
		tween.TweenProperty(pulseTarget, "scale", baseScale, 0.10f);
	}

	public virtual void SetInteractionHighlight(bool highlighted)
	{
		List<CanvasItem> targets = GetHighlightTargets();
		if (targets.Count == 0)
		{
			return;
		}

		foreach (CanvasItem target in targets)
		{
			if (highlighted)
			{
				ApplyInteractionHighlight(target);
			}
			else
			{
				ClearInteractionHighlight(target);
			}
		}
	}

	private List<CanvasItem> GetHighlightTargets()
	{
		List<CanvasItem> targets = new();
		CollectHighlightTargets(this, targets);
		return targets;
	}

	private static void CollectHighlightTargets(Node root, List<CanvasItem> targets)
	{
		foreach (Node child in root.GetChildren())
		{
			if (child is Sprite2D sprite && sprite.Texture != null)
			{
				targets.Add(sprite);
			}
			else if (child is AnimatedSprite2D animatedSprite && animatedSprite.SpriteFrames != null)
			{
				targets.Add(animatedSprite);
			}

			CollectHighlightTargets(child, targets);
		}
	}

	private void ApplyInteractionHighlight(CanvasItem target)
	{
		if (!_highlightMaterials.TryGetValue(target, out ShaderMaterial? highlightMaterial))
		{
			highlightMaterial = new ShaderMaterial
			{
				Shader = InteractionOutlineShader,
			};
			highlightMaterial.SetShaderParameter("outline_color", new Color(1.0f, 1.0f, 1.0f, 0.95f));
			highlightMaterial.SetShaderParameter("outline_size", 1.0f);
			_highlightMaterials[target] = highlightMaterial;
		}

		if (!_originalHighlightMaterials.ContainsKey(target))
		{
			_originalHighlightMaterials[target] = target.Material;
		}

		target.Material = highlightMaterial;
	}

	private void ClearInteractionHighlight(CanvasItem target)
	{
		if (_originalHighlightMaterials.TryGetValue(target, out Material? originalMaterial))
		{
			target.Material = originalMaterial;
			_originalHighlightMaterials.Remove(target);
			return;
		}

		if (_highlightMaterials.TryGetValue(target, out ShaderMaterial? highlightMaterial) && target.Material == highlightMaterial)
		{
			target.Material = null;
		}
	}

	public virtual Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		return new Godot.Collections.Dictionary
		{
			["is_disabled"] = IsDisabled,
			["next_available_time_ms"] = (long)_nextAvailableTimeMs,
			["disable_when_session_used"] = false,
			["remove_when_session_used"] = false,
		};
	}

	public virtual void ApplyRuntimeSnapshot(Godot.Collections.Dictionary snapshot)
	{
		if (snapshot == null || snapshot.Count == 0)
		{
			return;
		}

		if (snapshot.TryGetValue("is_disabled", out Variant disabledValue))
		{
			IsDisabled = disabledValue.AsBool();
		}

		if (snapshot.TryGetValue("next_available_time_ms", out Variant nextAvailableValue))
		{
			_nextAvailableTimeMs = (ulong)Math.Max(0L, nextAvailableValue.AsInt64());
		}
	}

	public virtual bool TryGetPrimaryInteractionCell(int tileSize, out Vector2I cell)
	{
		if (GetParent() is GridPlacedNode2D gridParent)
		{
			cell = gridParent.ResolveCell();
			return true;
		}

		cell = MapGridService.WorldToCell(GlobalPosition, tileSize <= 0 ? DefaultGridTileSize : tileSize);
		return true;
	}

	public virtual bool OccupiesInteractionCell(Vector2I cell, int tileSize)
	{
		return TryGetPrimaryInteractionCell(tileSize, out Vector2I resolvedCell) && resolvedCell == cell;
	}

	public virtual string BuildRuntimeStateKey(Node? sceneRoot = null)
	{
		string scenePath = sceneRoot?.GetTree()?.CurrentScene?.SceneFilePath
			?? GetTree()?.CurrentScene?.SceneFilePath
			?? string.Empty;
		if (!string.IsNullOrWhiteSpace(InteractionId))
		{
			return InteractionId.Trim();
		}

		if (GetParent() is GridInteractableNode2D gridParent)
		{
			return gridParent.ResolveInteractionId(scenePath);
		}

		if (TryGetPrimaryInteractionCell(DefaultGridTileSize, out Vector2I cell))
		{
			string normalizedScenePath = string.IsNullOrWhiteSpace(scenePath) ? "scene" : scenePath.Trim();
			return $"{normalizedScenePath}::{GetType().Name}::{cell.X},{cell.Y}";
		}

		if (sceneRoot != null && sceneRoot.IsAncestorOf(this))
		{
			return sceneRoot.GetPathTo(this).ToString();
		}

		return GetPath().ToString();
	}
}
