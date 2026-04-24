using System.Linq;
using CardChessDemo.Battle.Shared;
using Godot;

namespace CardChessDemo.Map;

public partial class Scene03LearningFlowController : Node
{
	[Export] public NodePath LearningEnemyPath { get; set; } = new("WorldObjects/Enemies/LearningEnemyAnchor/LearningEnemy");
	[Export] public NodePath LearningTriggerPath { get; set; } = new("WorldObjects/TutorialLearningTrigger");
	[Export] public string MenuTutorialsUnlockedFlagId { get; set; } = "scene03_menu_page_tutorials_unlocked";
	[Export] public string LearningTalentId { get; set; } = "talent_flex_learning";
	[Export] public string LearningCardId { get; set; } = "card_learning";
	[Export] public string LearningReadyFlagId { get; set; } = "scene03_learning_battle_ready";

	private Enemy? _learningEnemy;
	private StoryTriggerZone? _learningTrigger;
	private bool _lastReadyState;

	public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_learningEnemy = GetNodeOrNull<Enemy>(LearningEnemyPath);
		_learningTrigger = GetNodeOrNull<StoryTriggerZone>(LearningTriggerPath);
		SetProcess(true);
		ApplyLearningGate();
	}

	public override void _Process(double delta)
	{
		ApplyLearningGate();
	}

	private void ApplyLearningGate()
	{
		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (session == null)
		{
			return;
		}

		bool ready = IsLearningBattleReady(session);
		if (ready != _lastReadyState)
		{
			_lastReadyState = ready;
		}

		if (!string.IsNullOrWhiteSpace(LearningReadyFlagId))
		{
			session.SetFlag(new StringName(LearningReadyFlagId), ready);
		}

		if (_learningEnemy != null && GodotObject.IsInstanceValid(_learningEnemy))
		{
			_learningEnemy.IsDisabled = !ready;
			if (ready)
			{
				_learningEnemy.PromptText = "发起学习战斗";
			}
		}

		if (_learningTrigger != null && GodotObject.IsInstanceValid(_learningTrigger))
		{
			_learningTrigger.IsDisabled = !ready;
		}
	}

	private bool IsLearningBattleReady(GlobalGameSession session)
	{
		if (!GetFlag(session, MenuTutorialsUnlockedFlagId))
		{
			return false;
		}

		bool learningTalentUnlocked = session.ProgressionState.TalentIds.Contains(LearningTalentId, System.StringComparer.Ordinal);
		bool learningCardSaved = session.DeckBuildState.CardIds.Contains(LearningCardId, System.StringComparer.Ordinal);
		return learningTalentUnlocked && learningCardSaved;
	}

	private static bool GetFlag(GlobalGameSession session, string flagId)
	{
		if (string.IsNullOrWhiteSpace(flagId))
		{
			return false;
		}

		return session.TryGetFlag(new StringName(flagId), out Variant value) && value.AsBool();
	}
}
