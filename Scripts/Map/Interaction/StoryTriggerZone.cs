using System;
using System.Linq;
using CardChessDemo.UI.Dialogue;
using Godot;

namespace CardChessDemo.Map;

public partial class StoryTriggerZone : InteractableTemplate
{
	[Export] public Vector2 TriggerSize { get; set; } = new(32.0f, 32.0f);
	[Export] public bool TriggerOnce { get; set; } = true;
	[Export] public bool DisableAfterTrigger { get; set; } = true;
	[Export] public PackedScene? DialoguePanelScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/DialogueSequencePanel.tscn");
	[Export] public Godot.Collections.Array<DialoguePage> DialoguePages { get; set; } = new();
	[Export] public string[] DialogueLineEntries { get; set; } = Array.Empty<string>();
	[Export] public string DialogueScriptId { get; set; } = string.Empty;
	[Export] public NodePath TriggerInteractablePath { get; set; } = new("");
	[Export] public bool StartBattleOnComplete { get; set; } = false;
	[Export] public bool PlayDialogueOnlyOnce { get; set; } = true;
	[Export] public string BattleEncounterId { get; set; } = "scene01_tutorial";
	[Export] public PackedScene? BattleScene { get; set; }
	[Export(PropertyHint.File, "*.tscn")] public string BattleScenePath { get; set; } = "res://Scene/Battle/BattleTutorial.tscn";

	private Area2D _triggerArea = null!;
	private CollisionShape2D _collisionShape = null!;
	private RectangleShape2D _shape = null!;
	private Polygon2D _fillPolygon = null!;
	private Line2D _outline = null!;
	private bool _showingDialogue;
	private bool _hasTriggered;
	private Player? _pendingPlayer;
	private static readonly System.Collections.Generic.Dictionary<string, string[]> DialogueScripts = new(StringComparer.Ordinal)
	{
		["scene03_basic_story"] = new[]
		{
			"\u8352\u5DDD|\u524D\u9762\u90A3\u5BB6\u4F19\u628A\u8DEF\u5835\u6B7B\u4E86\u3002\u5148\u7528\u8FD9\u573A\u6218\u6597\u5B66\u4F1A\u6700\u57FA\u7840\u7684\u52A8\u4F5C\u3002",
			"\u8352\u5DDD|\u8BB0\u4F4F\uFF0C\u79FB\u52A8\u3001\u653B\u51FB\u3001\u9632\u5FA1\u90FD\u5F88\u91CD\u8981\uFF0C\u522B\u53EA\u987E\u7740\u786C\u51B2\u3002",
			"\u4E3B\u89D2|\u660E\u767D\uFF0C\u5148\u628A\u5B83\u89E3\u51B3\u6389\u3002",
		},
		["scene04_escape_story"] = new[]
		{
			"\u4E3B\u89D2|\u540E\u9762\u7684\u8FFD\u5175\u8D8A\u6765\u8D8A\u591A\u4E86\uFF0C\u518D\u7F20\u4E0B\u53BB\u53EA\u4F1A\u88AB\u62D6\u6B7B\u3002",
			"\u8352\u5DDD|\u522B\u604B\u6218\uFF0C\u5148\u5B66\u4F1A\u8131\u79BB\u5305\u56F4\u3002\u80FD\u8DD1\u7684\u65F6\u5019\uFF0C\u5C31\u679C\u65AD\u8DD1\u3002",
			"\u4E3B\u89D2|\u660E\u767D\uFF0C\u5148\u7529\u5F00\u4ED6\u4EEC\u3002",
		},
		["scene04_arakawa_story"] = new[]
		{
			"\u4E3B\u89D2|\u88AB\u5305\u56F4\u4E86\uFF0C\u5DF2\u7ECF\u6CA1\u6709\u9000\u8DEF\u4E86\u3002",
			"\u8352\u5DDD|\u90A3\u5C31\u8BA9\u6211\u6765\u5F00\u8DEF\u3002\u770B\u597D\u8352\u5DDD\u80FD\u529B\uFF0C\u76F4\u63A5\u628A\u5305\u56F4\u6495\u5F00\u3002",
			"\u8352\u5DDD|\u51C6\u5907\u597D\uFF0C\u52A8\u624B\u3002",
		},
	};

	public override void _Ready()
	{
		base._Ready();
		_triggerArea = GetNode<Area2D>("TriggerArea");
		_collisionShape = GetNode<CollisionShape2D>("TriggerArea/CollisionShape2D");
		_fillPolygon = GetNode<Polygon2D>("EditorFill");
		_outline = GetNode<Line2D>("EditorOutline");
		_shape = _collisionShape.Shape as RectangleShape2D ?? new RectangleShape2D();
		_collisionShape.Shape = _shape;
		_triggerArea.BodyEntered += OnTriggerBodyEntered;
		RefreshShape();
		SetProcess(Engine.IsEditorHint());
		UpdateEditorVisual();
	}

	public override void _Process(double delta)
	{
		if (!Engine.IsEditorHint())
		{
			SetProcess(false);
			return;
		}

		RefreshShape();
		UpdateEditorVisual();
	}

	public override bool CanInteract(Player player)
	{
		return !_showingDialogue
			&& !IsDisabled
			&& !IsOnCooldown
			&& (!TriggerOnce || !_hasTriggered || (PlayDialogueOnlyOnce && HasFollowUpAction()))
			&& (HasDialogueContent() || HasFollowUpAction());
	}

	public override string GetInteractText(Player player)
	{
		return string.Empty;
	}

	public override void SetInteractionHighlight(bool highlighted)
	{
	}

	protected override void OnInteract(Player player)
	{
		_pendingPlayer = player;
		if (!HasDialogueContent() || (_hasTriggered && PlayDialogueOnlyOnce))
		{
			HandleDialogueCompleted();
			return;
		}

		if (DialoguePanelScene?.Instantiate() is not DialogueSequencePanel panel)
		{
			return;
		}

		Node currentScene = GetTree().CurrentScene ?? this;
		currentScene.AddChild(panel);
		_showingDialogue = true;
		_hasTriggered = true;
		SetPlayerInputEnabled(false);
		panel.Present(
			BuildDialoguePages(),
			onCompleted: HandleDialogueCompleted,
			onClosed: () =>
			{
				_showingDialogue = false;
				SetPlayerInputEnabled(true);
				_pendingPlayer = null;
				if (TriggerOnce && DisableAfterTrigger && !HasFollowUpAction())
				{
					IsDisabled = true;
				}
			});
	}

	public override Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		Godot.Collections.Dictionary snapshot = base.BuildRuntimeSnapshot();
		snapshot["has_triggered"] = _hasTriggered;
		return snapshot;
	}

	public override void ApplyRuntimeSnapshot(Godot.Collections.Dictionary snapshot)
	{
		base.ApplyRuntimeSnapshot(snapshot);
		if (snapshot == null || snapshot.Count == 0)
		{
			return;
		}

		if (snapshot.TryGetValue("has_triggered", out Variant hasTriggeredVariant))
		{
			_hasTriggered = hasTriggeredVariant.AsBool();
		}
	}

	private void OnTriggerBodyEntered(Node2D body)
	{
		if (body is not Player player)
		{
			return;
		}

		Interact(player);
	}

	private void HandleDialogueCompleted()
	{
		_showingDialogue = false;
		Player? player = _pendingPlayer;
		SetPlayerInputEnabled(true);
		_pendingPlayer = null;

		bool actionStarted = false;
		if (player != null)
		{
			actionStarted = TryTriggerTargetInteractable(player) || TryStartConfiguredBattle(player);
		}

		if (TriggerOnce && DisableAfterTrigger && (actionStarted || !HasFollowUpAction()))
		{
			IsDisabled = true;
		}
	}

	private bool TryTriggerTargetInteractable(Player player)
	{
		if (TriggerInteractablePath.IsEmpty)
		{
			return false;
		}

		Node currentScene = GetTree().CurrentScene ?? this;
		Node? targetNode = GetNodeOrNull(TriggerInteractablePath);
		targetNode ??= currentScene.GetNodeOrNull(TriggerInteractablePath);
		if (targetNode == null)
		{
			string pathText = TriggerInteractablePath.ToString();
			if (pathText.StartsWith("../", StringComparison.Ordinal) && GetParent() != null)
			{
				targetNode = GetParent()?.GetNodeOrNull(pathText[3..]);
			}
		}
		if (targetNode is Enemy enemy)
		{
			return enemy.TriggerEncounterDirect(player);
		}

		if (targetNode is BattleEncounterEnemy battleEncounterEnemy)
		{
			return battleEncounterEnemy.TriggerEncounterDirect(player);
		}

		if (targetNode is InteractableTemplate interactable)
		{
			interactable.Interact(player);
			return true;
		}

		GD.PushError($"StoryTriggerZone: failed to resolve trigger target '{TriggerInteractablePath}'.");
		return false;
	}

	private bool TryStartConfiguredBattle(Player player)
	{
		if (!StartBattleOnComplete)
		{
			return false;
		}

		if (!MapBattleTransitionHelper.TryEnterBattle(this, player, BattleScene, BattleScenePath, BattleEncounterId, out string failureReason))
		{
			GD.PushError($"StoryTriggerZone: {failureReason}");
			return false;
		}

		return true;
	}

	private bool HasFollowUpAction()
	{
		return !TriggerInteractablePath.IsEmpty || StartBattleOnComplete;
	}

	private bool HasDialogueContent()
	{
		return DialoguePages.Count > 0
			|| (DialogueLineEntries?.Length ?? 0) > 0
			|| (!string.IsNullOrWhiteSpace(DialogueScriptId) && DialogueScripts.ContainsKey(DialogueScriptId));
	}

	private DialoguePage[] BuildDialoguePages()
	{
		if (DialoguePages.Count > 0)
		{
			return DialoguePages.ToArray();
		}

		if (!string.IsNullOrWhiteSpace(DialogueScriptId) && DialogueScripts.TryGetValue(DialogueScriptId, out string[]? scriptedLines))
		{
			return scriptedLines
				.Where(entry => !string.IsNullOrWhiteSpace(entry))
				.Select(ParseDialogueLineEntry)
				.ToArray();
		}

		if (DialogueLineEntries == null || DialogueLineEntries.Length == 0)
		{
			return Array.Empty<DialoguePage>();
		}

		return DialogueLineEntries
			.Where(entry => !string.IsNullOrWhiteSpace(entry))
			.Select(ParseDialogueLineEntry)
			.ToArray();
	}

	private static DialoguePage ParseDialogueLineEntry(string entry)
	{
		string trimmed = entry.Trim();
		int separatorIndex = trimmed.IndexOf('|');
		if (separatorIndex < 0)
		{
			separatorIndex = trimmed.IndexOf('：');
		}

		if (separatorIndex < 0)
		{
			separatorIndex = trimmed.IndexOf(':');
		}

		if (separatorIndex <= 0)
		{
			return new DialoguePage
			{
				Speaker = "旁白",
				Content = trimmed,
			};
		}

		return new DialoguePage
		{
			Speaker = trimmed[..separatorIndex].Trim(),
			Content = trimmed[(separatorIndex + 1)..].Trim(),
		};
	}

	private void SetPlayerInputEnabled(bool enabled)
	{
		if (_pendingPlayer == null || !GodotObject.IsInstanceValid(_pendingPlayer))
		{
			return;
		}

		_pendingPlayer.SetPhysicsProcess(enabled);
		_pendingPlayer.SetProcess(enabled);
		_pendingPlayer.SetProcessInput(enabled);
		_pendingPlayer.SetProcessUnhandledInput(enabled);
	}

	private void RefreshShape()
	{
		Vector2 safeSize = new(Mathf.Max(16.0f, TriggerSize.X), Mathf.Max(16.0f, TriggerSize.Y));
		if (_shape.Size == safeSize)
		{
			return;
		}

		_shape.Size = safeSize;
	}

	private void UpdateEditorVisual()
	{
		bool visibleInEditor = Engine.IsEditorHint();
		_fillPolygon.Visible = visibleInEditor;
		_outline.Visible = visibleInEditor;

		Vector2 half = _shape.Size * 0.5f;
		Vector2[] points =
		{
			new(-half.X, -half.Y),
			new(half.X, -half.Y),
			new(half.X, half.Y),
			new(-half.X, half.Y),
		};

		_fillPolygon.Polygon = points;
		_outline.Points = new[]
		{
			points[0],
			points[1],
			points[2],
			points[3],
			points[0],
		};
	}
}
