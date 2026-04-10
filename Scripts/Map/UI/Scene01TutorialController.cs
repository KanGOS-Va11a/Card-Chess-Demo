using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class Scene01TutorialController : Node
{
	[Export] public NodePath PlayerCharacterPath = new("MainPlayer/Player");
	[Export] public NodePath EnemyNodePath = new("Enemy");
	[Export] public NodePath VisionMaskControllerPath = new("PlayerVisionMaskController");
	[Export] public NodePath IntroDialogPanelPath = new("TutorialUI/TutorialTipPanel");
	[Export] public NodePath IntroDialogLabelPath = new("TutorialUI/TutorialTipPanel/TutorialTipLabel");
	[Export] public NodePath GuideLabelPath = new("TutorialUI/GuideLabel");
	[Export] public NodePath FollowPointLightPath = new("MainPlayer/PointLight2D");
	[Export] public Vector2 FollowPointLightOffset = Vector2.Zero;
	[Export] public float DialogSlideOffset = 86.0f;
	[Export(PropertyHint.Range, "2,30,1")] public float GuideVisibleSeconds = 10.0f;
	[Export(PropertyHint.Range, "32,512,1")] public float EnemySightDistance = 176.0f;
	[Export] public bool UseTileBasedEnemySight = true;
	[Export(PropertyHint.Range, "1,12,1")] public int EnemySightTiles = 4;
	[Export] public bool TriggerEnemyDialogByVisionMask = true;
	[Export(PropertyHint.Range, "0.50,1.20,0.01")] public float VisionTriggerRadiusFactor = 0.90f;
	[Export] public string TutorialEncounterId = "grunt_debug";
	[Export] public string[] IntroDialogLines =
	{
		"这......这是哪里？（按e继续）",
        "先顺着有光的地方看看......或许能有一些线索"
	};

	private CharacterBody2D _player;
	private Node2D _enemy;
	private PlayerVisionMaskController _visionMaskController;
	private Panel _introPanel;
	private Label _introLabel;
	private Label _guideLabel;
	private PointLight2D _followPointLight;
	private GlobalGameSession _globalSession;
	private static readonly string[] DefaultIntroDialogLines =
	{
		"这......是哪里？(按e继续)",
        "先顺着有光的地方看看......或许能有一些线索"
	};

	private readonly string[] _enemySightedLines =
	{
		"我：那是......一个怪物？",
		"未知生物：嗷......",
        "我：看来我需要打倒他才能通过这里"
	};

	private enum DialogFlow
	{
		None,
		Intro,
		EnemySighted,
	}

	private int _lineIndex;
	private bool _isDialogActive;
	private bool _introCompleted;
	private bool _enemyDialogCompleted;
	private DialogFlow _currentDialogFlow = DialogFlow.None;
	private string[] _activeDialogLines = null;
	private ulong _guideTicket;
	private float _panelVisibleTop;
	private float _panelVisibleBottom;

	public bool IsDialogBlockingInput => _isDialogActive;

	public override void _Ready()
	{
		SetProcessInput(true);
		SetProcessUnhandledInput(true);
		_player = ResolvePlayerNode();
		_enemy = ResolveEnemyNode();
		_visionMaskController = VisionMaskControllerPath.IsEmpty ? null : GetNodeOrNull<PlayerVisionMaskController>(VisionMaskControllerPath);
		_introPanel = IntroDialogPanelPath.IsEmpty ? null : GetNodeOrNull<Panel>(IntroDialogPanelPath);
		_introLabel = IntroDialogLabelPath.IsEmpty ? null : GetNodeOrNull<Label>(IntroDialogLabelPath);
		_guideLabel = GuideLabelPath.IsEmpty ? null : GetNodeOrNull<Label>(GuideLabelPath);
		_followPointLight = FollowPointLightPath.IsEmpty ? null : GetNodeOrNull<PointLight2D>(FollowPointLightPath);
		_globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (IsTutorialEncounterAlreadyCleared())
		{
			_introCompleted = true;
			_enemyDialogCompleted = true;
			_isDialogActive = false;
			_enemy?.QueueFree();
		}

		SyncFollowPointLight();

		bool resumedFromBattle = ApplyPendingResumeContext();
		_globalSession?.ConsumeLastBattleResult();

		if (resumedFromBattle)
		{
			_introCompleted = true;
			_enemyDialogCompleted = true;
			_isDialogActive = false;
			if (_introPanel != null)
			{
				_introPanel.Visible = false;
			}

			SetPlayerInputEnabled(true);
			return;
		}

		if (_guideLabel != null)
		{
			_guideLabel.Visible = false;
			_guideLabel.Text = string.Empty;
		}

		if (_introPanel == null || _introLabel == null)
		{
			GD.PushWarning("Scene01TutorialController: 缺少教程对话UI节点，跳过开场对话。 ");
			_introCompleted = true;
			ShowGuide("使用WASD进行移动\n按E和环境物品交互");
			return;
		}

		_panelVisibleTop = _introPanel.OffsetTop;
		_panelVisibleBottom = _introPanel.OffsetBottom;

		// 先隐藏到屏幕外，再滑入。
		_introPanel.OffsetTop = _panelVisibleTop + DialogSlideOffset;
		_introPanel.OffsetBottom = _panelVisibleBottom + DialogSlideOffset;
		_introPanel.Visible = true;

		BeginDialog(DialogFlow.Intro, ResolveIntroDialogLines());
	}

	private string[] ResolveIntroDialogLines()
	{
		if (IntroDialogLines == null || IntroDialogLines.Length == 0)
		{
			return DefaultIntroDialogLines;
		}

		int validCount = 0;
		foreach (string line in IntroDialogLines)
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				validCount++;
			}
		}

		if (validCount == 0)
		{
			return DefaultIntroDialogLines;
		}

		string[] resolvedLines = new string[validCount];
		int index = 0;
		foreach (string line in IntroDialogLines)
		{
			if (!string.IsNullOrWhiteSpace(line))
			{
				resolvedLines[index] = line.Trim();
				index++;
			}
		}

		return resolvedLines;
	}

	private bool ApplyPendingResumeContext()
	{
		if (_globalSession == null)
		{
			return false;
		}

		MapResumeContext resumeContext = _globalSession.PeekPendingMapResumeContext();
		if (resumeContext == null)
		{
			return false;
		}

		string currentScenePath = GetTree().CurrentScene?.SceneFilePath ?? SceneFilePath;
		if (!string.Equals(currentScenePath, resumeContext.ScenePath, System.StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		if (_player != null)
		{
			_player.GlobalPosition = resumeContext.PlayerGlobalPosition;
		}

		Node sceneRoot = GetTree().CurrentScene ?? this;
		MapRuntimeSnapshotHelper.ApplyToScene(sceneRoot, resumeContext.MapRuntimeSnapshot);
		_globalSession.ConsumePendingMapResumeContext();
		return true;
	}

	private bool IsTutorialEncounterAlreadyCleared()
	{
		return _globalSession != null
			&& !string.IsNullOrWhiteSpace(TutorialEncounterId)
			&& _globalSession.ClearedEncounters.Contains(new StringName(TutorialEncounterId));
	}

	public override void _Process(double delta)
	{
		SyncFollowPointLight();

		if (!_introCompleted || _enemyDialogCompleted || _isDialogActive)
		{
			return;
		}

		if (!IsInstanceValid(_enemy))
		{
			_enemy = EnemyNodePath.IsEmpty ? GetNodeOrNull<Node2D>("Enemy") : GetNodeOrNull<Node2D>(EnemyNodePath);
			if (!IsInstanceValid(_enemy))
			{
				return;
			}
		}

		if (_player == null)
		{
			return;
		}

		if (!CanTriggerEnemyDialog())
		{
			return;
		}

		BeginDialog(DialogFlow.EnemySighted, _enemySightedLines);
	}

	private void SyncFollowPointLight()
	{
		if (!GodotObject.IsInstanceValid(_player))
		{
			_player = PlayerCharacterPath.IsEmpty
				? GetNodeOrNull<CharacterBody2D>("MainPlayer/Player")
				: GetNodeOrNull<CharacterBody2D>(PlayerCharacterPath);
		}

		if (!GodotObject.IsInstanceValid(_followPointLight) && !FollowPointLightPath.IsEmpty)
		{
			_followPointLight = GetNodeOrNull<PointLight2D>(FollowPointLightPath);
		}

		if (!GodotObject.IsInstanceValid(_player) || !GodotObject.IsInstanceValid(_followPointLight))
		{
			return;
		}

		_followPointLight.GlobalPosition = _player.GlobalPosition + FollowPointLightOffset;
	}

	private bool CanTriggerEnemyDialog()
	{
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
		bool insideVisionRadius = playerScreen.DistanceTo(enemyScreen) <= visionRadiusPixels;

		// 允许二选一：进入可视圈或进入配置好的地砖触发距离。
		return insideVisionRadius || worldDistance <= distanceThreshold;
	}

	private CharacterBody2D ResolvePlayerNode()
	{
		if (!PlayerCharacterPath.IsEmpty && GetNodeOrNull<CharacterBody2D>(PlayerCharacterPath) is CharacterBody2D playerFromPath)
		{
			return playerFromPath;
		}

		return GetNodeOrNull<CharacterBody2D>("MainPlayer/Player")
			?? GetNodeOrNull<CharacterBody2D>("Player");
	}

	private Node2D ResolveEnemyNode()
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

	public override void _Input(InputEvent @event)
	{
		HandleDialogAdvanceInput(@event);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		HandleDialogAdvanceInput(@event);
	}

	private void HandleDialogAdvanceInput(InputEvent @event)
	{
		if (!_isDialogActive)
		{
			return;
		}

		if (!@event.IsActionPressed("interact"))
		{
			return;
		}

		_lineIndex++;
		if (_activeDialogLines != null && _lineIndex < _activeDialogLines.Length)
		{
			_introLabel.Text = _activeDialogLines[_lineIndex];
			GetViewport().SetInputAsHandled();
			return;
		}

		_isDialogActive = false;
		SlideDialogOutAndHide();
		HandleDialogFinished();
		GetViewport().SetInputAsHandled();
	}

	private void BeginDialog(DialogFlow flow, string[] lines)
	{
		if (_introPanel == null || _introLabel == null || lines == null || lines.Length == 0)
		{
			return;
		}

		_currentDialogFlow = flow;
		_activeDialogLines = lines;
		_lineIndex = 0;
		_isDialogActive = true;
		_introLabel.Text = _activeDialogLines[_lineIndex];

		_introPanel.OffsetTop = _panelVisibleTop + DialogSlideOffset;
		_introPanel.OffsetBottom = _panelVisibleBottom + DialogSlideOffset;
		_introPanel.Visible = true;

		SetPlayerInputEnabled(false);
		SlideDialogIn();
	}

	private void HandleDialogFinished()
	{
		switch (_currentDialogFlow)
		{
			case DialogFlow.Intro:
				_introCompleted = true;
				SetPlayerInputEnabled(true);
				ShowGuide("使用WASD进行移动\n按E和环境物品交互");
				break;

			case DialogFlow.EnemySighted:
				_enemyDialogCompleted = true;
				SetPlayerInputEnabled(true);
				ShowGuide("靠近怪物按下e进入战斗");
				break;

			default:
				SetPlayerInputEnabled(true);
				break;
		}

		_currentDialogFlow = DialogFlow.None;
		_activeDialogLines = null;
	}

	private void ShowGuide(string text)
	{
		if (!GodotObject.IsInstanceValid(_guideLabel))
		{
			return;
		}

		Label guideLabel = _guideLabel;
		guideLabel.Text = text;
		guideLabel.Visible = true;

		_guideTicket++;
		ulong currentTicket = _guideTicket;
		SceneTreeTimer timer = GetTree().CreateTimer(Mathf.Max(GuideVisibleSeconds, 0.1f));
		timer.Timeout += () =>
		{
			if (!GodotObject.IsInstanceValid(this) || currentTicket != _guideTicket)
			{
				return;
			}

			if (!GodotObject.IsInstanceValid(_guideLabel))
			{
				return;
			}

			_guideLabel.Visible = false;
			_guideLabel.Text = string.Empty;
		};
	}

	private void SlideDialogIn()
	{
		if (_introPanel == null)
		{
			return;
		}

		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.Out);
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(_introPanel, "offset_top", _panelVisibleTop, 0.20f);
		tween.TweenProperty(_introPanel, "offset_bottom", _panelVisibleBottom, 0.20f);
	}

	private void SlideDialogOutAndHide()
	{
		if (_introPanel == null)
		{
			return;
		}

		float hiddenTop = _panelVisibleTop + DialogSlideOffset;
		float hiddenBottom = _panelVisibleBottom + DialogSlideOffset;

		Tween tween = CreateTween();
		tween.SetEase(Tween.EaseType.In);
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(_introPanel, "offset_top", hiddenTop, 0.18f);
		tween.TweenProperty(_introPanel, "offset_bottom", hiddenBottom, 0.18f);
		tween.Finished += () =>
		{
			if (GodotObject.IsInstanceValid(_introPanel))
			{
				_introPanel.Visible = false;
			}
		};
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
