using System;
using System.Linq;
using CardChessDemo.Battle.Shared;
using CardChessDemo.UI;
using Godot;

namespace CardChessDemo.Map;

public partial class Scene03MenuPageTutorialController : Node
{
	[Export] public PackedScene? PopupScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/PagedTutorialPopup.tscn");
	[Export] public NodePath OverlayPath { get; set; } = new("SystemFeatureOverlay");
	[Export] public string TutorialsUnlockedFlagId { get; set; } = "scene03_menu_page_tutorials_unlocked";
	[Export] public string StatusTutorialShownFlagId { get; set; } = "scene03_status_tab_tutorial_shown";
	[Export] public string InventoryTutorialShownFlagId { get; set; } = "scene03_inventory_tab_tutorial_shown";
	[Export] public string TalentTutorialShownFlagId { get; set; } = "scene03_talent_tab_tutorial_shown";
	[Export] public string CodexTutorialShownFlagId { get; set; } = "scene03_codex_tab_tutorial_shown";
	[Export] public string DeckTutorialShownFlagId { get; set; } = "scene03_deck_tab_tutorial_shown";
	[Export] public string DeckLearningTutorialShownFlagId { get; set; } = "scene03_deck_learning_tab_tutorial_shown";
	[Export] public string LearningTalentRootId { get; set; } = "talent_flex_root";
	[Export] public string LearningTalentId { get; set; } = "talent_flex_learning";
	[Export] public string LearningCardId { get; set; } = "card_learning";

	private SystemFeatureLabController? _overlay;
	private bool _wasMenuVisible;
	private int _lastTabIndex = -1;
	private bool _popupActive;

	private static readonly string[] StatusPages =
	{
		"\u8FD9\u91CC\u67E5\u770B\u5F53\u524D\u72B6\u6001\u3002\u751F\u547D\u3001\u8352\u5DDD\u80FD\u91CF\u548C\u57FA\u7840\u80FD\u529B\u503C\u90FD\u4F1A\u76F4\u63A5\u5F71\u54CD\u4F60\u4E0B\u4E00\u573A\u6218\u6597\u3002",
	};

	private static readonly string[] InventoryPages =
	{
		"\u8FD9\u91CC\u653E\u63A2\u7D22\u7269\u8D44\u3001\u56DE\u590D\u9053\u5177\u548C\u88C5\u5907\u3002\u5148\u8BB0\u4F4F\u4F4D\u7F6E\uff0c\u540E\u9762\u62FF\u5230\u4E1C\u897F\u518D\u56DE\u6765\u6574\u7406\u3002",
	};

	private static readonly string[] TalentPages =
	{
		"\u5929\u8D4B\u4E0D\u53EA\u662F\u52A0\u6570\u503C\uff0c\u6709\u4E9B\u8282\u70B9\u4F1A\u76F4\u63A5\u89E3\u9501\u65B0\u7684\u6218\u6597\u65B9\u5F0F\u548C\u65B0\u724C\u3002",
		"\u73B0\u5728\u5148\u8D70\u5230\u521B\u9020\u7EBF\u7684\u300E\u8352\u5DDD\u540C\u8C03\u300F\uff0c\u7136\u540E\u89E3\u9501\u300E\u6218\u6597\u8BB0\u5F55\u534F\u8BAE\u300F\u3002\u5B66\u4E60\u654C\u4EBA\u62DB\u5F0F\u5C31\u4ECE\u8FD9\u6761\u7EBF\u5F00\u59CB\u3002",
	};

	private static readonly string[] CodexPages =
	{
		"\u56FE\u9274\u4F1A\u8BB0\u5F55\u4F60\u89C1\u8FC7\u7684\u654C\u4EBA\u548C\u62FF\u5230\u7684\u5361\u724C\u3002\u4E4B\u540E\u5B66\u5230\u65B0\u62DB\u5F0F\uff0c\u4E5F\u53EF\u4EE5\u56DE\u6765\u8FD9\u91CC\u786E\u8BA4\u3002",
	};

	private static readonly string[] DeckPages =
	{
		"\u8FD9\u91CC\u51B3\u5B9A\u4F60\u4E0B\u4E00\u573A\u6218\u6597\u771F\u6B63\u4F1A\u5E26\u54EA\u4E9B\u724C\u3002\u5DE6\u8FB9\u662F\u53EF\u9009\u724C\u6C60\uff0c\u53F3\u8FB9\u662F\u5F53\u524D\u643A\u5E26\u724C\u7EC4\u3002",
		"\u73B0\u5728\u5148\u53BB\u5929\u8D4B\u9875\u70B9\u51FA\u5B66\u4E60\u76F8\u5173\u8282\u70B9\u3002\u89E3\u9501\u540E\uff0c\u518D\u56DE\u5230\u8FD9\u91CC\u628A\u300E\u5B66\u4E60\u300F\u52A0\u5165\u724C\u7EC4\u3002",
	};

	private static readonly string[] DeckLearningPages =
	{
		"\u300E\u5B66\u4E60\u300F\u73B0\u5728\u5DF2\u7ECF\u89E3\u9501\u4E86\u3002\u628A\u5B83\u52A0\u5165\u5F53\u524D\u6784\u7B51\uff0C\u4E0B\u4E00\u573A\u6218\u6597\u91CC\u624D\u4F1A\u62BD\u5230\u5B83\u3002",
		"\u52A0\u5B8C\u4E4B\u540E\u8BB0\u5F97\u6309\u300E\u4FDD\u5B58\u6784\u7B51\u300F\u3002\u6CA1\u4FDD\u5B58\u7684\u8BDD\uff0c\u4E0B\u4E00\u6218\u8FD8\u662F\u4F1A\u7528\u65E7\u724C\u7EC4\u3002",
	};

	public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if (_popupActive || MapTextBlocker.IsBlockingTextVisible(this))
		{
			return;
		}

		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (session == null || !GetFlag(session, TutorialsUnlockedFlagId))
		{
			_wasMenuVisible = false;
			_lastTabIndex = -1;
			return;
		}

		_overlay ??= ResolveOverlay();
		bool menuVisible = _overlay != null && GodotObject.IsInstanceValid(_overlay) && _overlay.IsMenuVisible;
		if (!menuVisible)
		{
			_wasMenuVisible = false;
			_lastTabIndex = -1;
			return;
		}

		int currentTabIndex = _overlay!.CurrentTabIndex;
		bool enteredNewTab = !_wasMenuVisible || currentTabIndex != _lastTabIndex;
		_wasMenuVisible = true;
		_lastTabIndex = currentTabIndex;
		if (!enteredNewTab)
		{
			return;
		}

		TryShowTabTutorial(session, currentTabIndex);
	}

	private SystemFeatureLabController? ResolveOverlay()
	{
		Node sceneRoot = GetTree().CurrentScene ?? this;
		if (!OverlayPath.IsEmpty && sceneRoot.GetNodeOrNull<SystemFeatureLabController>(OverlayPath) is SystemFeatureLabController byPath)
		{
			return byPath;
		}

		return sceneRoot.FindChild("SystemFeatureOverlay", true, false) as SystemFeatureLabController;
	}

	private void TryShowTabTutorial(GlobalGameSession session, int tabIndex)
	{
		switch (tabIndex)
		{
			case SystemFeatureLabController.StatusTabIndex:
				ShowOnce(session, StatusTutorialShownFlagId, "状态页教程", StatusPages);
				break;
			case SystemFeatureLabController.InventoryTabIndex:
				ShowOnce(session, InventoryTutorialShownFlagId, "背包页教程", InventoryPages);
				break;
			case SystemFeatureLabController.TalentTabIndex:
				ShowOnce(session, TalentTutorialShownFlagId, "天赋页教程", TalentPages, FocusLearningTalentTarget);
				break;
			case SystemFeatureLabController.CodexTabIndex:
				ShowOnce(session, CodexTutorialShownFlagId, "图鉴页教程", CodexPages);
				break;
			case SystemFeatureLabController.DeckTabIndex:
				if (IsLearningUnlocked(session))
				{
					ShowOnce(session, DeckLearningTutorialShownFlagId, "构筑页教程", DeckLearningPages, FocusLearningCardTarget, DeckTutorialShownFlagId);
				}
				else
				{
					ShowOnce(session, DeckTutorialShownFlagId, "构筑页教程", DeckPages);
				}

				break;
		}
	}

	private bool IsLearningUnlocked(GlobalGameSession session)
	{
		return session.ProgressionState.UnlockedCardIds.Contains(LearningCardId, StringComparer.Ordinal)
			|| session.ProgressionState.TalentIds.Contains(LearningTalentId, StringComparer.Ordinal);
	}

	private void FocusLearningTalentTarget()
	{
		if (_overlay == null)
		{
			return;
		}

		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		string targetTalentId = session != null && session.ProgressionState.TalentIds.Contains(LearningTalentRootId, StringComparer.Ordinal)
			? LearningTalentId
			: LearningTalentRootId;
		_overlay.FocusTalentForTutorial(targetTalentId);
	}

	private void FocusLearningCardTarget()
	{
		_overlay?.FocusDeckCardForTutorial(LearningCardId);
	}

	private void ShowOnce(GlobalGameSession session, string shownFlagId, string title, string[] pages, Action? onFinished = null, params string[] additionalFlags)
	{
		if (string.IsNullOrWhiteSpace(shownFlagId) || GetFlag(session, shownFlagId))
		{
			return;
		}

		if (PopupScene?.Instantiate() is not PagedTutorialPopup popup)
		{
			return;
		}

		_popupActive = true;
		(GetTree().CurrentScene ?? this).AddChild(popup);
		popup.Present(
			title,
			pages,
			onCompleted: () => FinishPopup(session, shownFlagId, onFinished, additionalFlags),
			onClosed: () => FinishPopup(session, shownFlagId, onFinished, additionalFlags));
	}

	private void FinishPopup(GlobalGameSession session, string shownFlagId, Action? onFinished, string[] additionalFlags)
	{
		SetFlag(session, shownFlagId);
		foreach (string additionalFlag in additionalFlags.Where(flag => !string.IsNullOrWhiteSpace(flag)))
		{
			SetFlag(session, additionalFlag);
		}

		_popupActive = false;
		onFinished?.Invoke();
	}

	private static bool GetFlag(GlobalGameSession session, string flagId)
	{
		if (string.IsNullOrWhiteSpace(flagId))
		{
			return false;
		}

		return session.TryGetFlag(new StringName(flagId), out Variant shownVariant) && shownVariant.AsBool();
	}

	private static void SetFlag(GlobalGameSession session, string flagId)
	{
		if (string.IsNullOrWhiteSpace(flagId))
		{
			return;
		}

		session.SetFlag(new StringName(flagId), true);
	}
}
