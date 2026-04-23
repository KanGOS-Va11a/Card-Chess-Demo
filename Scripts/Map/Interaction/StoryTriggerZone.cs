using System;
using System.Collections.Generic;
using System.Linq;
using CardChessDemo.Battle.Shared;
using CardChessDemo.UI.Dialogue;
using Godot;

namespace CardChessDemo.Map;

[Tool]
public partial class StoryTriggerZone : InteractableTemplate
{
	[Export] public bool UseGridTriggerPlacement { get; set; }
	[Export] public Vector2I OriginCell { get; set; } = Vector2I.Zero;
	[Export] public Vector2I TriggerSizeCells { get; set; } = new(2, 2);
	[Export] public Godot.Collections.Array<Vector2I> TriggerCellOffsets { get; set; } = new();
	[Export(PropertyHint.Range, "8,128,1")] public int GridTileSize { get; set; } = 16;
	[Export] public Vector2 TriggerSize { get; set; } = new(32.0f, 32.0f);
	[Export] public bool TriggerOnce { get; set; } = true;
	[Export] public bool DisableAfterTrigger { get; set; } = true;
	[Export] public PackedScene? DialoguePanelScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/DialogueSequencePanel.tscn");
	[Export] public Godot.Collections.Array<DialoguePage> DialoguePages { get; set; } = new();
	[Export] public string[] DialogueLineEntries { get; set; } = Array.Empty<string>();
	[Export] public string DialogueScriptId { get; set; } = string.Empty;
	[Export] public NodePath TriggerInteractablePath { get; set; } = new("");
	[Export(PropertyHint.File, "*.tscn")] public string NextScenePath { get; set; } = string.Empty;
	[Export] public string NextSceneSpawnId { get; set; } = string.Empty;
	[Export] public bool StartBattleOnComplete { get; set; } = false;
	[Export] public bool PlayDialogueOnlyOnce { get; set; } = true;
	[Export] public bool RemoveSelfOnBattleRetreat { get; set; } = false;
	[Export] public bool RemoveTargetInteractableOnBattleRetreat { get; set; } = false;
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
	private bool _removeFromScene;
	private Player? _pendingPlayer;
	private bool _wasPlayerInsideGridZone;

	private static readonly Dictionary<string, string[]> DialogueScripts = new(StringComparer.Ordinal)
	{
		["scene03_basic_story"] = new[]
		{
			"\u65C1\u767D|\u4F60\u63A8\u5F00\u8231\u95E8\uFF0C\u8D70\u5ECA\u91CC\u6EE1\u662F\u66B4\u529B\u7834\u574F\u7559\u4E0B\u7684\u75D5\u8FF9\u3002\u8FD9\u91CC\u663E\u7136\u521A\u7ECF\u5386\u8FC7\u4E00\u573A\u52AB\u63A0\u3002",
			"\u65C1\u767D|\u524D\u65B9\u4F20\u6765\u7C97\u91CD\u7684\u811A\u6B65\u58F0\u3002\u4E00\u540D\u642D\u8239\u5BA2\u4ECE\u7834\u635F\u7BA1\u7EBF\u540E\u6653\u51FA\u6765\uFF0C\u6B63\u6321\u5728\u4F60\u8981\u8D70\u7684\u8DEF\u4E0A\u3002",
			"\u4E3B\u89D2|\u2026\u2026\u5148\u6D3B\u4E0B\u6765\u3002",
		},
		["scene04_escape_story"] = new[]
		{
			"\u65C1\u767D|\u8D70\u5ECA\u5C3D\u5934\u7684\u4EBA\u5F71\u8D8A\u6765\u8D8A\u591A\uFF0C\u540E\u8DEF\u4E5F\u5F00\u59CB\u88AB\u5C01\u6B7B\u3002",
			"\u4E3B\u89D2|\u4E0D\u80FD\u518D\u7F20\u4E0B\u53BB\u4E86\u3002\u518D\u6162\u4E00\u6B65\uFF0C\u5C31\u4F1A\u88AB\u4ED6\u4EEC\u62D6\u6B7B\u5728\u8FD9\u91CC\u3002",
			"\u4E3B\u89D2|\u5148\u51B2\u51FA\u53BB\u3002",
		},
		["scene04_arakawa_story"] = new[]
		{
			"\u4E3B\u89D2|\u2026\u2026\u8DDD\u79BB\u6781\u9650\u5C31\u5DEE\u4E00\u53E3\u6C14\u4E86\u3002",
			"\u65C1\u767D|\u624B\u8FB9\u7684\u9ED1\u8272\u624B\u63D0\u7BB1\u7A81\u7136\u9707\u52A8\u4E86\u4E00\u4E0B\u3002\u51B0\u51B7\u7532\u677F\u7684\u7F1D\u9699\u91CC\u6E17\u51FA\u5E7D\u84DD\u8272\u5149\uFF0C\u50CF\u67D0\u79CD\u4E0D\u8BE5\u5B58\u5728\u7684\u529B\u91CF\u6B63\u5728\u9192\u6765\u3002",
			"\u4E3B\u89D2|\u8FD9\u662F\u4EC0\u4E48\u2026\u2026",
			"\u65C1\u767D|\u73B0\u5728\u6CA1\u65F6\u95F4\u8FFD\u95EE\u7B54\u6848\u3002\u5148\u8BD5\u7740\u501F\u90A3\u80A1\u529B\u91CF\u6539\u5199\u6218\u573A\uFF0C\u7136\u540E\u6D3B\u4E0B\u53BB\u3002",
		},
		["scene04_boarding_story"] = new[]
		{
			"\u65C1\u767D|\u4F60\u7EC8\u4E8E\u6740\u7A7F\u4E86\u8FD9\u6761\u8D70\u5ECA\u3002",
			"\u65C1\u767D|\u5728\u98DE\u8239\u5F00\u59CB\u89E3\u4F53\u3001\u6574\u8258\u8239\u90FD\u50CF\u8981\u88AB\u771F\u7A7A\u6254\u5F00\u7684\u6700\u540E\u65F6\u523B\uFF0C\u4F60\u649E\u8FDB\u4E86\u6700\u8FD1\u7684\u4E34\u65F6\u9003\u751F\u98DE\u8239\u3002",
			"\u65C1\u767D|\u539A\u91CD\u8231\u95E8\u5728\u4F60\u8EAB\u540E\u8F70\u7136\u95ED\u5408\u3002\u4E0B\u4E00\u79D2\uFF0C\u9003\u751F\u98DE\u8239\u8131\u79BB\u6BCD\u8239\uFF0C\u72ED\u7A84\u8231\u5BA4\u91CC\u53EA\u5269\u4E0B\u4F60\u3001\u90A3\u53EA\u9ED1\u8272\u624B\u63D0\u7BB1\uFF0C\u4EE5\u53CA\u4E0D\u65AD\u95EA\u70C1\u7684\u6545\u969C\u706F\u3002",
			"\u65C1\u767D|\u65E0\u6570\u5F69\u8272\u50CF\u7D20\u5757\u6BEB\u65E0\u5F81\u5146\u5730\u95EF\u5165\u89C6\u91CE\uFF0C\u51E0\u4E4E\u541E\u6CA1\u6574\u4E2A\u72ED\u7A84\u8231\u5BA4\u3002",
			"\u672A\u77E5\u58F0\u97F3|\u2026\u2026\u54FC\u3002",
			"\u4E3B\u89D2|\u8C01\u5728\u8BF4\u8BDD\uFF1F",
			"\u8352\u5DDD|\u522B\u627E\u4E86\uFF0C\u4F60\u7684\u4F20\u611F\u5668\u88AB\u6211\u501F\u7528\u4E86\u4E00\u4E0B\u3002\u53EB\u6211\u8352\u5DDD\u3002",
			"\u4E3B\u89D2|\u4F60\u5230\u5E95\u662F\u4EC0\u4E48\u4E1C\u897F\uFF1F",
			"\u8352\u5DDD|\u7B54\u6848\u5F88\u957F\u3002\u4F46\u5BFC\u822A\u7CFB\u7EDF\u5DF2\u7ECF\u5148\u66FF\u6211\u4EEC\u9009\u597D\u4E86\u4E0B\u4E00\u7AD9\u3002",
			"\u8352\u5DDD|\u6700\u8FD1\u7684\u964D\u843D\u70B9\u662F\u4E00\u5EA7\u96B6\u5C5E\u4E8E\u661F\u9645\u8054\u76DF\u7684\u8FB9\u5883\u661F\u6E2F\u3002\u522B\u8BEF\u4F1A\uFF0C\u90A3\u91CC\u4E0D\u4F1A\u662F\u5B89\u5168\u533A\u3002",
			"\u8352\u5DDD|\u5148\u6D3B\u7740\u843D\u4E0B\u53BB\u3002\u5176\u4F59\u7684\u95EE\u9898\uFF0C\u7B49\u4F60\u8FD8\u80FD\u7AD9\u7740\u7684\u65F6\u5019\u518D\u95EE\u6211\u3002",
		},
		["scene05_arrival_story"] = new[]
		{
			"\u65C1\u767D|\u4E34\u65F6\u9003\u751F\u98DE\u8239\u5728\u771F\u7A7A\u4E2D\u6296\u5F97\u50CF\u5FEB\u8981\u6563\u67B6\u7684\u94C1\u76D2\u3002\u71C3\u6599\u8868\u4E00\u8DEF\u8DCC\u8FDB\u7EA2\u533A\uFF0C\u4F60\u53EA\u80FD\u5C31\u8FD1\u5F3A\u884C\u8FEB\u964D\u5728\u8FD9\u5904\u7A7A\u6E2F\u3002",
			"\u65C1\u767D|\u5793\u0034\u0032\u0036\u661F\u6E2F\u7684\u8EAB\u4EFD\u6838\u9A8C\u6BD4\u60F3\u8C61\u4E2D\u66F4\u4E25\u3002\u95F8\u673A\u5373\u5C06\u62A5\u8B66\u7684\u77AC\u95F4\uFF0C\u4F60\u4F53\u5185\u90A3\u80A1\u964C\u751F\u529B\u91CF\u50CF\u96FE\u4E00\u6837\u62B9\u5E73\u4E86\u626B\u63CF\u7ED3\u679C\u3002",
			"\u4E3B\u89D2|\u2026\u2026\u5C45\u7136\u771F\u7684\u6DF7\u8FC7\u6765\u4E86\u3002",
			"\u65C1\u767D|\u81F3\u5C11\u73B0\u5728\uFF0C\u4F60\u8FD8\u80FD\u4EE5\u201C\u6B63\u5E38\u65C5\u5BA2\u201D\u7684\u8EAB\u4EFD\u7AD9\u5728\u8FD9\u91CC\u3002\u5148\u5728\u661F\u6E2F\u91CC\u627E\u5230\u7ACB\u8DB3\u70B9\uFF0C\u518D\u53BB\u8FFD\u7D22\u90A3\u8270\u98DE\u8239\u3001\u90A3\u53EA\u624B\u63D0\u7BB1\uFF0C\u4EE5\u53CA\u4F60\u8EAB\u4E0A\u5230\u5E95\u53D1\u751F\u4E86\u4EC0\u4E48\u3002",
		},
		["scene06_boss_story"] = new[]
		{
			"\u65C1\u767D|\u7A7F\u8FC7\u6700\u540E\u4E00\u6BB5\u5806\u6EE1\u5E9F\u94A2\u4E0E\u71C3\u70E7\u75D5\u8FF9\u7684\u901A\u9053\u540E\uFF0C\u7A7A\u6C14\u91CC\u53EA\u5269\u4E0B\u7126\u707C\u4E0E\u673A\u6CB9\u5473\u3002",
			"\u65C1\u767D|\u524D\u65B9\u90A3\u9053\u9AD8\u5927\u7684\u8EAB\u5F71\u7F13\u7F13\u8F6C\u8FC7\u6765\uFF0C\u50CF\u662F\u65E9\u5C31\u77E5\u9053\u4F60\u4F1A\u627E\u5230\u8FD9\u91CC\u3002",
			"\u9523\u8239\u957F|\u64C5\u95EF\u642D\u8239\u5BA2\u5730\u76D8\u7684\u4EBA\uFF0C\u6CA1\u6709\u4E00\u4E2A\u80FD\u6D3B\u7740\u79BB\u5F00\u3002",
			"\u4E3B\u89D2|\u90A3\u5C31\u522B\u8BA9\u6211\u79BB\u5F00\u3002",
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
		if (UseGridTriggerPlacement)
		{
			ApplyGridPlacement();
			_triggerArea.SetDeferred(Area2D.PropertyName.Monitoring, false);
		}

		SetProcess(true);
		UpdateEditorVisual();
	}

	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint())
		{
			ApplyGridPlacement();
			RefreshShape();
			UpdateEditorVisual();
			QueueRedraw();
			return;
		}

		if (UseGridTriggerPlacement)
		{
			EvaluateGridTrigger();
		}
	}

	public override void _Draw()
	{
		if (!Engine.IsEditorHint() || !UseGridTriggerPlacement)
		{
			return;
		}

		string labelText = TriggerCellOffsets.Count > 0
			? $"Origin ({OriginCell.X},{OriginCell.Y})\nCells {TriggerCellOffsets.Count}"
			: $"Origin ({OriginCell.X},{OriginCell.Y})\nSize {Mathf.Max(1, TriggerSizeCells.X)}x{Mathf.Max(1, TriggerSizeCells.Y)}";
		MapEditorDrawHelper.DrawLabel(
			this,
			new Vector2(-18.0f, -20.0f),
			labelText,
			12,
			new Color(1.0f, 0.96f, 0.78f, 1.0f),
			new Color(0.12f, 0.09f, 0.03f, 0.9f));
	}

	public override bool CanInteract(Player player)
	{
		return !_showingDialogue
			&& !IsDisabled
			&& !_removeFromScene
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

	protected override async void OnInteract(Player player)
	{
		player.SettleToWorldPosition(player.GetStableGridPosition());
		_pendingPlayer = player;
		if (!HasDialogueContent() || (_hasTriggered && PlayDialogueOnlyOnce))
		{
			HandleDialogueCompleted();
			return;
		}

		_showingDialogue = true;
		_hasTriggered = true;
		MapDialogueResult result = await MapDialogueService.PresentAsync(
			this,
			new MapDialogueRequest
			{
				PanelScene = DialoguePanelScene,
				Pages = BuildDialoguePages(),
				LockPlayerInput = true,
				RejectIfAnotherDialogueVisible = true,
				SourceId = BuildRuntimeStateKey(),
			},
			player);

		switch (result.Status)
		{
			case MapDialogueCompletionStatus.Completed:
				HandleDialogueCompleted();
				break;
			case MapDialogueCompletionStatus.Closed:
				HandleDialogueClosedWithoutAction();
				break;
			default:
				_showingDialogue = false;
				_pendingPlayer = null;
				break;
		}
	}

	public override Godot.Collections.Dictionary BuildRuntimeSnapshot()
	{
		Godot.Collections.Dictionary snapshot = base.BuildRuntimeSnapshot();
		snapshot["has_triggered"] = _hasTriggered;
		snapshot["remove_from_scene"] = _removeFromScene;
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

		if (snapshot.TryGetValue("remove_from_scene", out Variant removeVariant) && removeVariant.AsBool())
		{
			HideAndRemoveTrigger();
		}
	}

	private void OnTriggerBodyEntered(Node2D body)
	{
		if (UseGridTriggerPlacement)
		{
			return;
		}

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
		_pendingPlayer = null;

		bool actionStarted = false;
		if (player != null)
		{
			MapDialogueFollowUpAction? action = BuildPrimaryFollowUpAction();
			if (action != null)
			{
				if (action.Kind == MapDialogueFollowUpKind.TriggerInteractable
					&& TryResolveTriggerTargetNode(out Node? targetNode)
					&& targetNode is InteractableTemplate targetInteractable
					&& (targetNode is Enemy || targetNode is BattleEncounterEnemy))
				{
					RegisterRetreatCleanupIfNeeded(targetInteractable);
				}

				MapDialogueFollowUpResult followUpResult = MapDialogueService.ExecuteFollowUpActions(this, player, action);
				actionStarted = followUpResult.Executed && followUpResult.Succeeded;
				if (!followUpResult.Succeeded)
				{
					ClearRetreatCleanup();
					GD.PushError($"StoryTriggerZone follow-up failed: {followUpResult.FailureReason}");
				}
			}
		}

		if (TriggerOnce && DisableAfterTrigger && (actionStarted || !HasFollowUpAction()))
		{
			IsDisabled = true;
		}
	}

	private void HandleDialogueClosedWithoutAction()
	{
		_showingDialogue = false;
		_pendingPlayer = null;
		if (TriggerOnce && DisableAfterTrigger && !HasFollowUpAction())
		{
			IsDisabled = true;
		}
	}

	private MapDialogueFollowUpAction? BuildPrimaryFollowUpAction()
	{
		if (!TriggerInteractablePath.IsEmpty)
		{
			return new MapDialogueFollowUpAction
			{
				Kind = MapDialogueFollowUpKind.TriggerInteractable,
				TargetNodePath = TriggerInteractablePath,
			};
		}

		if (StartBattleOnComplete)
		{
			return new MapDialogueFollowUpAction
			{
				Kind = MapDialogueFollowUpKind.StartBattle,
				BattleEncounterId = BattleEncounterId,
				BattleScene = BattleScene,
				BattleScenePath = BattleScenePath,
			};
		}

		if (!string.IsNullOrWhiteSpace(NextScenePath))
		{
			return new MapDialogueFollowUpAction
			{
				Kind = MapDialogueFollowUpKind.ChangeScene,
				NextScenePath = NextScenePath,
				NextSceneSpawnId = NextSceneSpawnId,
			};
		}

		return null;
	}

	private bool TryTriggerTargetInteractable(Player player)
	{
		if (!TryResolveTriggerTargetNode(out Node? targetNode))
		{
			GD.PushError($"StoryTriggerZone: failed to resolve trigger target '{TriggerInteractablePath}'.");
			return false;
		}

		if (targetNode is Enemy enemy)
		{
			RegisterRetreatCleanupIfNeeded(enemy);
			bool started = enemy.TriggerEncounterDirect(player);
			if (!started)
			{
				ClearRetreatCleanup();
			}

			return started;
		}

		if (targetNode is BattleEncounterEnemy battleEncounterEnemy)
		{
			RegisterRetreatCleanupIfNeeded(battleEncounterEnemy);
			bool started = battleEncounterEnemy.TriggerEncounterDirect(player);
			if (!started)
			{
				ClearRetreatCleanup();
			}

			return started;
		}

		if (targetNode is InteractableTemplate interactable)
		{
			interactable.Interact(player);
			return true;
		}

		GD.PushError($"StoryTriggerZone: unsupported trigger target '{targetNode.GetType().Name}'.");
		return false;
	}

	private bool TryResolveTriggerTargetNode(out Node? targetNode)
	{
		targetNode = null;
		if (TriggerInteractablePath.IsEmpty)
		{
			return false;
		}

		Node currentScene = GetTree().CurrentScene ?? this;
		targetNode = currentScene.GetNodeOrNull(TriggerInteractablePath);
		targetNode ??= GetNodeOrNull(TriggerInteractablePath);

		if (targetNode != null)
		{
			return true;
		}

		string pathText = TriggerInteractablePath.ToString();
		if (pathText.StartsWith("../", StringComparison.Ordinal) && GetParent() != null)
		{
			targetNode = GetParent()?.GetNodeOrNull(pathText[3..]);
		}

		return targetNode != null;
	}

	private void RegisterRetreatCleanupIfNeeded(InteractableTemplate targetInteractable)
	{
		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (session == null)
		{
			return;
		}

		List<string> paths = new();
		Node currentScene = GetTree().CurrentScene ?? this;
		if (RemoveSelfOnBattleRetreat)
		{
			string selfPath = currentScene.GetPathTo(this).ToString();
			if (!string.IsNullOrWhiteSpace(selfPath))
			{
				paths.Add(selfPath);
			}
		}

		if (RemoveTargetInteractableOnBattleRetreat)
		{
			string targetPath = currentScene.GetPathTo(targetInteractable).ToString();
			if (!string.IsNullOrWhiteSpace(targetPath))
			{
				paths.Add(targetPath);
			}
		}

		if (paths.Count > 0)
		{
			session.SetPendingRetreatCleanupInteractables(paths);
		}
	}

	private void ClearRetreatCleanup()
	{
		GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession")?.ClearPendingRetreatCleanupInteractables();
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

	private bool TryStartConfiguredSceneTransition()
	{
		if (string.IsNullOrWhiteSpace(NextScenePath))
		{
			return false;
		}

		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		session?.ClearPendingBattleReturnContext();
		session?.ConsumeLastBattleResult();
		session?.ClearPendingRestorePlayerPosition();
		session?.SetPendingSceneTransfer(NextScenePath.Trim(), NextSceneSpawnId);

		Error result = GetTree().ChangeSceneToFile(NextScenePath.Trim());
		if (result != Error.Ok)
		{
			GD.PushError($"StoryTriggerZone: scene change failed, error={result}, path='{NextScenePath}'.");
			return false;
		}

		return true;
	}

	private bool HasFollowUpAction()
	{
		return !TriggerInteractablePath.IsEmpty || StartBattleOnComplete || !string.IsNullOrWhiteSpace(NextScenePath);
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
			separatorIndex = trimmed.IndexOf('\uFF1A');
		}

		if (separatorIndex < 0)
		{
			separatorIndex = trimmed.IndexOf(':');
		}

		if (separatorIndex <= 0)
		{
			return new DialoguePage
			{
				Speaker = "\u65C1\u767D",
				Content = trimmed,
			};
		}

		return new DialoguePage
		{
			Speaker = trimmed[..separatorIndex].Trim(),
			Content = trimmed[(separatorIndex + 1)..].Trim(),
		};
	}

	private void HideAndRemoveTrigger()
	{
		_removeFromScene = true;
		IsDisabled = true;
		Visible = false;
		_triggerArea?.SetDeferred(Area2D.PropertyName.Monitoring, false);
		CallDeferred(MethodName.QueueFree);
	}

	private void ApplyGridPlacement()
	{
		if (!UseGridTriggerPlacement)
		{
			return;
		}

		Vector2 localPosition = MapGridService.ResolveLocalPositionFromCell(this, OriginCell, GridTileSize);
		if (Position != localPosition)
		{
			Position = localPosition;
		}
	}

	private void EvaluateGridTrigger()
	{
		if (_showingDialogue || IsDisabled || _removeFromScene)
		{
			_wasPlayerInsideGridZone = false;
			return;
		}

		Player? player = ResolveCurrentScenePlayer();
		if (player == null)
		{
			_wasPlayerInsideGridZone = false;
			return;
		}

		if (player.IsGridMoving)
		{
			return;
		}

		Vector2I playerCell = MapGridService.WorldToCell(player.GlobalPosition, GridTileSize);
		bool isInside = ContainsCell(playerCell);
		if (isInside && !_wasPlayerInsideGridZone)
		{
			Interact(player);
		}

		_wasPlayerInsideGridZone = isInside;
	}

	private bool ContainsCell(Vector2I playerCell)
	{
		if (TriggerCellOffsets.Count > 0)
		{
			foreach (Vector2I offset in TriggerCellOffsets)
			{
				if (playerCell == OriginCell + offset)
				{
					return true;
				}
			}

			return false;
		}

		Vector2I safeSize = new(Math.Max(1, TriggerSizeCells.X), Math.Max(1, TriggerSizeCells.Y));
		return playerCell.X >= OriginCell.X
			&& playerCell.Y >= OriginCell.Y
			&& playerCell.X < OriginCell.X + safeSize.X
			&& playerCell.Y < OriginCell.Y + safeSize.Y;
	}

	private IEnumerable<Vector2I> EnumerateCoveredCells()
	{
		if (TriggerCellOffsets.Count > 0)
		{
			foreach (Vector2I offset in TriggerCellOffsets)
			{
				yield return OriginCell + offset;
			}

			yield break;
		}

		Vector2I safeSize = new(Math.Max(1, TriggerSizeCells.X), Math.Max(1, TriggerSizeCells.Y));
		for (int y = 0; y < safeSize.Y; y++)
		{
			for (int x = 0; x < safeSize.X; x++)
			{
				yield return new Vector2I(OriginCell.X + x, OriginCell.Y + y);
			}
		}
	}

	private Player? ResolveCurrentScenePlayer()
	{
		return GetTree().CurrentScene?.FindChild("Player", true, false) as Player;
	}

	private void RefreshShape()
	{
		Vector2 safeSize;
		if (UseGridTriggerPlacement)
		{
			float safeTileSize = Mathf.Max(1, GridTileSize);
			if (TryGetCoveredCellBounds(out Vector2I minCell, out Vector2I maxCell))
			{
				safeSize = new Vector2(
					(maxCell.X - minCell.X + 1) * safeTileSize,
					(maxCell.Y - minCell.Y + 1) * safeTileSize);
			}
			else
			{
				safeSize = new Vector2(safeTileSize, safeTileSize);
			}
		}
		else
		{
			safeSize = new Vector2(Mathf.Max(16.0f, TriggerSize.X), Mathf.Max(16.0f, TriggerSize.Y));
		}

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

		if (!visibleInEditor)
		{
			return;
		}

		if (UseGridTriggerPlacement && TryGetCoveredCellBounds(out Vector2I minCell, out Vector2I maxCell))
		{
			float safeTileSize = Mathf.Max(1, GridTileSize);
			Vector2I localMin = minCell - OriginCell;
			Vector2I localMax = maxCell - OriginCell;
			Vector2 topLeft = new(localMin.X * safeTileSize - safeTileSize * 0.5f, localMin.Y * safeTileSize - safeTileSize * 0.5f);
			Vector2 topRight = new(localMax.X * safeTileSize + safeTileSize * 0.5f, localMin.Y * safeTileSize - safeTileSize * 0.5f);
			Vector2 bottomRight = new(localMax.X * safeTileSize + safeTileSize * 0.5f, localMax.Y * safeTileSize + safeTileSize * 0.5f);
			Vector2 bottomLeft = new(localMin.X * safeTileSize - safeTileSize * 0.5f, localMax.Y * safeTileSize + safeTileSize * 0.5f);
			Vector2[] points =
			{
				topLeft,
				topRight,
				bottomRight,
				bottomLeft,
			};
			_fillPolygon.Polygon = points;
			_outline.Points = new[]
			{
				topLeft,
				topRight,
				bottomRight,
				bottomLeft,
				topLeft,
			};
			return;
		}

		Vector2 half = _shape.Size * 0.5f;
		Vector2[] legacyPoints =
		{
			new(-half.X, -half.Y),
			new(half.X, -half.Y),
			new(half.X, half.Y),
			new(-half.X, half.Y),
		};

		_fillPolygon.Polygon = legacyPoints;
		_outline.Points = new[]
		{
			legacyPoints[0],
			legacyPoints[1],
			legacyPoints[2],
			legacyPoints[3],
			legacyPoints[0],
		};
	}

	private bool TryGetCoveredCellBounds(out Vector2I minCell, out Vector2I maxCell)
	{
		bool hasAny = false;
		minCell = Vector2I.Zero;
		maxCell = Vector2I.Zero;
		foreach (Vector2I cell in EnumerateCoveredCells())
		{
			if (!hasAny)
			{
				minCell = cell;
				maxCell = cell;
				hasAny = true;
				continue;
			}

			minCell = new Vector2I(Math.Min(minCell.X, cell.X), Math.Min(minCell.Y, cell.Y));
			maxCell = new Vector2I(Math.Max(maxCell.X, cell.X), Math.Max(maxCell.Y, cell.Y));
		}

		return hasAny;
	}
}
