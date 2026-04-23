using System;
using System.Linq;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;
using CardChessDemo.UI.Dialogue;
using Godot;

namespace CardChessDemo.Map;

public partial class Scene01TutorialController : Node
{
	[Export] public NodePath PlayerCharacterPath = new("MainPlayer/Player");
	[Export] public NodePath EnemyNodePath = new("Enemy");
	[Export] public NodePath VisionMaskControllerPath = new("PlayerVisionMaskController");
	[Export] public NodePath FollowPointLightPath = new("MainPlayer/PointLight2D");
	[Export] public Vector2 FollowPointLightOffset = Vector2.Zero;
	[Export(PropertyHint.Range, "32,512,1")] public float EnemySightDistance = 176.0f;
	[Export] public bool UseTileBasedEnemySight = true;
	[Export(PropertyHint.Range, "1,12,1")] public int EnemySightTiles = 4;
	[Export] public bool TriggerEnemyDialogByVisionMask = true;
	[Export(PropertyHint.Range, "0.50,1.20,0.01")] public float VisionTriggerRadiusFactor = 0.90f;
	[Export] public string TutorialEncounterId = "grunt_debug";
	[Export] public PackedScene? DialoguePanelScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/DialogueSequencePanel.tscn");
	[Export] public string[] IntroDialogLines =
	{
		"\u65C1\u767D\uff1A\u4F60\u4ECE\u6DF7\u7740\u661F\u5C18\u4E0E\u673A\u6CB9\u5473\u7684\u5E9F\u6599\u4E2D\u9192\u6765\u3002\u8231\u4F53\u6DF1\u5904\u4F20\u6765\u4EE4\u4EBA\u7259\u9178\u7684\u91D1\u5C5E\u8F70\u9E23\u3002",
		"\u65C1\u767D\uff1A\u8BB0\u5FC6\u50CF\u88AB\u751F\u751F\u6316\u7A7A\uFF0C\u53EA\u5269\u4E0B\u65E7\u5927\u8863\u3001\u7EA2\u65B9\u5DFE\u548C\u90A3\u53EA\u6C89\u91CD\u7684\u9ED1\u8272\u624B\u63D0\u7BB1\u8FD8\u5728\u4F60\u8EAB\u8FB9\u3002",
		"\u65C1\u767D\uff1A\u8FD9\u91CC\u4E0D\u662F\u5B89\u5168\u7684\u5730\u65B9\u3002\u5148\u987A\u7740\u4EAE\u7740\u7684\u8231\u9053\u8D70\uFF0C\u627E\u5230\u79BB\u5F00\u8FD9\u91CC\u7684\u8DEF\u3002",
	};
	[Export] public string[] EnemySightedLines =
	{
		"\u4E3B\u89D2\uff1A\u90A3\u662F\u2026\u2026\u4E00\u4E2A\u602A\u7269\u3002",
		"\u672A\u77E5\u751F\u7269\uff1A\u5420\u2026\u2026",
		"\u4E3B\u89D2\uff1A\u770B\u6765\u53EA\u80FD\u5148\u6253\u5012\u5B83\u4E86\u3002",
	};

	private CharacterBody2D? _player;
	private Node2D? _enemy;
	private PlayerVisionMaskController? _visionMaskController;
	private PointLight2D? _followPointLight;
	private GlobalGameSession? _globalSession;
	private bool _showingDialog;
	private bool _introCompleted;
	private bool _enemyDialogCompleted;

	private enum DialogFlow
	{
		None = 0,
		Intro = 1,
		EnemySighted = 2,
	}

	public bool IsDialogBlockingInput => _showingDialog;

	public override void _Ready()
	{
		_player = ResolvePlayerNode();
		_enemy = ResolveEnemyNode();
		_visionMaskController = VisionMaskControllerPath.IsEmpty ? null : GetNodeOrNull<PlayerVisionMaskController>(VisionMaskControllerPath);
		_followPointLight = FollowPointLightPath.IsEmpty ? null : GetNodeOrNull<PointLight2D>(FollowPointLightPath);
		_globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");

		if (IsTutorialEncounterAlreadyCleared())
		{
			_introCompleted = true;
			_enemyDialogCompleted = true;
			_showingDialog = false;
			_enemy?.QueueFree();
		}

		SyncFollowPointLight();

		if (!_introCompleted)
		{
			PresentDialog(DialogFlow.Intro, ResolveDialogLines(IntroDialogLines));
		}
	}

	public override void _Process(double delta)
	{
		SyncFollowPointLight();

		if (!_introCompleted || _enemyDialogCompleted || _showingDialog || _player == null)
		{
			return;
		}

		if (!GodotObject.IsInstanceValid(_enemy))
		{
			_enemy = ResolveEnemyNode();
			if (!GodotObject.IsInstanceValid(_enemy))
			{
				return;
			}
		}

		if (!CanTriggerEnemyDialog())
		{
			return;
		}

		PresentDialog(DialogFlow.EnemySighted, ResolveDialogLines(EnemySightedLines));
	}

	private string[] ResolveDialogLines(string[] sourceLines)
	{
		if (sourceLines == null)
		{
			return Array.Empty<string>();
		}

		return sourceLines
			.Where(line => !string.IsNullOrWhiteSpace(line))
			.Select(line => line.Trim())
			.ToArray();
	}

	private async void PresentDialog(DialogFlow flow, string[] lines)
	{
		DialoguePage[] pages = lines.Select(BuildDialoguePage).ToArray();
		if (DialoguePanelScene == null || pages.Length == 0)
		{
			HandleDialogFinished(flow);
			return;
		}

		_showingDialog = true;
		MapDialogueResult result = await MapDialogueService.PresentAsync(
			this,
			new MapDialogueRequest
			{
				PanelScene = DialoguePanelScene,
				Pages = pages,
				LockPlayerInput = true,
				RejectIfAnotherDialogueVisible = false,
				SourceId = $"scene01_tutorial_{flow}",
				CompletedFollowUpActions = BuildCompletedFollowUpActions(flow),
				ClosedFollowUpActions = BuildClosedFollowUpActions(flow),
			},
			_player as Player);

		if (result.IsCompleted || result.IsClosed || result.IsFailed)
		{
			HandleDialogFinished(flow);
		}
	}

	private static DialoguePage BuildDialoguePage(string line)
	{
		DialoguePage page = new();
		int separatorIndex = line.IndexOf('\uFF1A');
		if (separatorIndex < 0)
		{
			separatorIndex = line.IndexOf(':');
		}

		if (separatorIndex > 0)
		{
			page.Speaker = line[..separatorIndex].Trim();
			page.Content = line[(separatorIndex + 1)..].Trim();
		}
		else
		{
			page.Speaker = "\u65C1\u767D";
			page.Content = line.Trim();
		}

		return page;
	}

	private void HandleDialogFinished(DialogFlow flow)
	{
		_showingDialog = false;
		switch (flow)
		{
			case DialogFlow.Intro:
				_introCompleted = true;
				break;
			case DialogFlow.EnemySighted:
				_enemyDialogCompleted = true;
				break;
		}

		SetPlayerInputEnabled(true);
	}

	private MapDialogueFollowUpAction[] BuildCompletedFollowUpActions(DialogFlow flow)
	{
		return flow switch
		{
			DialogFlow.Intro => Array.Empty<MapDialogueFollowUpAction>(),
			DialogFlow.EnemySighted => Array.Empty<MapDialogueFollowUpAction>(),
			_ => Array.Empty<MapDialogueFollowUpAction>(),
		};
	}

	private MapDialogueFollowUpAction[] BuildClosedFollowUpActions(DialogFlow flow)
	{
		return flow switch
		{
			DialogFlow.Intro => Array.Empty<MapDialogueFollowUpAction>(),
			DialogFlow.EnemySighted => Array.Empty<MapDialogueFollowUpAction>(),
			_ => Array.Empty<MapDialogueFollowUpAction>(),
		};
	}

	private bool IsTutorialEncounterAlreadyCleared()
	{
		return _globalSession != null
			&& !string.IsNullOrWhiteSpace(TutorialEncounterId)
			&& _globalSession.ClearedEncounters.Contains(new StringName(TutorialEncounterId));
	}

	private void SyncFollowPointLight()
	{
		if (!GodotObject.IsInstanceValid(_player))
		{
			_player = ResolvePlayerNode();
		}

		if (!GodotObject.IsInstanceValid(_followPointLight) && !FollowPointLightPath.IsEmpty)
		{
			_followPointLight = GetNodeOrNull<PointLight2D>(FollowPointLightPath);
		}

		if (!GodotObject.IsInstanceValid(_player) || !GodotObject.IsInstanceValid(_followPointLight))
		{
			return;
		}

		_followPointLight!.GlobalPosition = _player!.GlobalPosition + FollowPointLightOffset;
	}

	private bool CanTriggerEnemyDialog()
	{
		if (_player == null || _enemy == null)
		{
			return false;
		}

		float distanceThreshold = ResolveEnemySightDistance();
		float worldDistance = _player.GlobalPosition.DistanceTo(_enemy.GlobalPosition);
		if (!TriggerEnemyDialogByVisionMask)
		{
			return worldDistance <= distanceThreshold;
		}

		if (_visionMaskController == null)
		{
			_visionMaskController = VisionMaskControllerPath.IsEmpty
				? GetNodeOrNull<PlayerVisionMaskController>("PlayerVisionMaskController")
				: GetNodeOrNull<PlayerVisionMaskController>(VisionMaskControllerPath);
		}

		if (_visionMaskController == null)
		{
			return worldDistance <= distanceThreshold;
		}

		float visionRadiusPixels = _visionMaskController.GetCurrentVisibleRadiusPixels() * Mathf.Max(0.1f, VisionTriggerRadiusFactor);
		if (visionRadiusPixels <= 0.0f)
		{
			return false;
		}

		Vector2 playerScreen = GetViewport().GetCanvasTransform() * _player.GlobalPosition;
		Vector2 enemyScreen = GetViewport().GetCanvasTransform() * _enemy.GlobalPosition;
		return playerScreen.DistanceTo(enemyScreen) <= visionRadiusPixels
			|| worldDistance <= distanceThreshold;
	}

	private CharacterBody2D? ResolvePlayerNode()
	{
		if (!PlayerCharacterPath.IsEmpty && GetNodeOrNull<CharacterBody2D>(PlayerCharacterPath) is CharacterBody2D playerFromPath)
		{
			return playerFromPath;
		}

		return GetNodeOrNull<CharacterBody2D>("MainPlayer/Player")
			?? GetNodeOrNull<CharacterBody2D>("Player");
	}

	private Node2D? ResolveEnemyNode()
	{
		if (!EnemyNodePath.IsEmpty && GetNodeOrNull<Node2D>(EnemyNodePath) is Node2D enemyFromPath)
		{
			return enemyFromPath;
		}

		return GetNodeOrNull<Node2D>("Enemy");
	}

	private float ResolveEnemySightDistance()
	{
		if (!UseTileBasedEnemySight)
		{
			return EnemySightDistance;
		}

		int tileSize = 16;
		if (_player is Player mapPlayer)
		{
			tileSize = Mathf.Max(1, mapPlayer.GridTileSize);
		}

		return Mathf.Max(1, EnemySightTiles) * tileSize;
	}

	private void SetPlayerInputEnabled(bool enabled)
	{
		if (_player == null)
		{
			return;
		}

		_player.SetPhysicsProcess(enabled);
		_player.SetProcessUnhandledInput(enabled);
	}
}
