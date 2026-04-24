using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Cards;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Battle.UI;

public partial class BattleDeckBuilderController : Control
{
	[Export] public BattleCardLibrary? BattleCardLibrary { get; set; }
	[Export] public BattleDeckBuildRules? BattleDeckBuildRules { get; set; }

	private GlobalGameSession? _session;
	private BattleDeckConstructionService? _constructionService;
	private ItemList _availableList = null!;
	private ItemList _deckList = null!;
	private Label _poolSummaryLabel = null!;
	private Label _deckSummaryLabel = null!;
	private RichTextLabel _detailLabel = null!;
	private RichTextLabel _validationLabel = null!;
	private Button _addButton = null!;
	private Button _removeButton = null!;
	private Button _saveButton = null!;
	private Button _resetButton = null!;
	private Button _starterButton = null!;

	private BattleCardTemplate[] _availableTemplates = Array.Empty<BattleCardTemplate>();
	private DeckListEntry[] _visibleDeckEntries = Array.Empty<DeckListEntry>();
	private List<string> _workingDeck = new();
	private bool _deckDirty;

	public override void _Ready()
	{
		_session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		BattleCardLibrary ??= GD.Load<BattleCardLibrary>("res://Resources/Battle/Cards/DefaultBattleCardLibrary.tres");
		BattleDeckBuildRules ??= GD.Load<BattleDeckBuildRules>("res://Resources/Battle/Cards/DefaultBattleDeckBuildRules.tres");

		if (_session == null || BattleCardLibrary == null || BattleDeckBuildRules == null)
		{
			GD.PushError("BattleDeckBuilderController: required session or resources are missing.");
			return;
		}

		_session.EnsureDeckBuildInitialized(BattleCardLibrary);
		_constructionService = new BattleDeckConstructionService(BattleCardLibrary, BattleDeckBuildRules);

		_availableList = GetNode<ItemList>("Margin/Root/Columns/AvailableColumn/AvailableList");
		_deckList = GetNode<ItemList>("Margin/Root/Columns/DeckColumn/DeckList");
		_poolSummaryLabel = GetNode<Label>("Margin/Root/Columns/AvailableColumn/PoolSummary");
		_deckSummaryLabel = GetNode<Label>("Margin/Root/Columns/DeckColumn/DeckSummary");
		_detailLabel = GetNode<RichTextLabel>("Margin/Root/DetailPanel/DetailText");
		_validationLabel = GetNode<RichTextLabel>("Margin/Root/ValidationPanel/ValidationText");
		_addButton = GetNode<Button>("Margin/Root/Columns/ControlColumn/AddButton");
		_removeButton = GetNode<Button>("Margin/Root/Columns/ControlColumn/RemoveButton");
		_saveButton = GetNode<Button>("Margin/Root/Footer/SaveButton");
		_resetButton = GetNode<Button>("Margin/Root/Footer/ResetButton");
		_starterButton = GetNode<Button>("Margin/Root/Footer/StarterButton");
		_resetButton.Text = "Clear Deck";

		_availableList.ItemSelected += OnAvailableSelected;
		_deckList.ItemSelected += OnDeckSelected;
		_addButton.Pressed += OnAddPressed;
		_removeButton.Pressed += OnRemovePressed;
		_saveButton.Pressed += OnSavePressed;
		_resetButton.Pressed += OnResetPressed;
		_starterButton.Pressed += OnStarterPressed;

		LoadWorkingDeckFromSession();
		RefreshAll();
	}

	private void LoadWorkingDeckFromSession()
	{
		if (_session == null)
		{
			return;
		}

		_workingDeck = _session.DeckBuildState.CardIds.ToList();
		RefreshDeckDirtyFlag();
	}

	public void RefreshFromExternalState()
	{
		LoadWorkingDeckFromSession();
		RefreshAll();
	}

	private void RefreshAll()
	{
		if (_session == null || _constructionService == null || BattleCardLibrary == null || BattleDeckBuildRules == null)
		{
			return;
		}

		string selectedAvailableCardId = GetSelectedAvailableCardId();
		string selectedDeckCardId = GetSelectedDeckCardId();
		ProgressionSnapshot progression = _session.BuildProgressionSnapshotModel();
		_availableTemplates = _constructionService.GetAvailableCardPool(progression).ToArray();
		_visibleDeckEntries = BuildVisibleDeckEntries();
		BattleDeckValidationResult validation = _constructionService.ValidateDeck(BuildWorkingDeckSnapshot(), progression);

		_availableList.Clear();
		foreach (BattleCardTemplate template in _availableTemplates)
		{
			bool isOverlimitCandidate = !template.CanCarryNormally(progression) && template.CanCarryOverlimit(progression);
			bool atCopyLimit = IsAtDeckCopyLimit(template, progression);
			string learnedLabel = template.IsLearnedCard ? "  [学习]" : string.Empty;
			string overlimitLabel = isOverlimitCandidate ? "  [超规]" : string.Empty;
			string copyState = $"{GetCurrentCopies(_workingDeck, template.CardId)}/{GetDeckCopyLimit(template, progression)}";
			int itemIndex = _availableList.AddItem($"{template.DisplayName}{learnedLabel}{overlimitLabel}  C{template.Cost}  I{template.GetEffectiveBuildPoints()}  {copyState}");
			_availableList.SetItemCustomFgColor(itemIndex, atCopyLimit ? new Color(0.42f, 0.42f, 0.42f, 1.0f) : Colors.White);
		}

		_deckList.Clear();
		foreach (DeckListEntry entry in _visibleDeckEntries)
		{
			_deckList.AddItem(BuildDeckEntryText(entry, progression));
		}

		_poolSummaryLabel.Text = $"可选牌库 {_availableTemplates.Length} 张";
		_deckSummaryLabel.Text = $"当前牌组 {validation.TotalCardCount} 张 / 影响 {validation.TotalBuildPoints} / 超规 {validation.UsedOverlimitCarrySlots} / 正式版本 #{_session.DeckBuildState.Revision}";
		_validationLabel.Text = _deckDirty
			? BuildValidationText(validation) + $"\n正式构筑版本 #{_session.DeckBuildState.Revision}\n未保存"
			: BuildValidationText(validation) + $"\n正式构筑版本 #{_session.DeckBuildState.Revision}\n已保存";
		_saveButton.Disabled = !_deckDirty || !validation.IsValid;
		_resetButton.Disabled = _workingDeck.Count == 0;

		RestoreAvailableSelection(selectedAvailableCardId);
		RestoreDeckSelection(selectedDeckCardId);
		if (_availableList.ItemCount > 0 && _availableList.GetSelectedItems().Length == 0)
		{
			_availableList.Select(0);
		}

		if (_deckList.ItemCount > 0 && _deckList.GetSelectedItems().Length == 0)
		{
			_deckList.Select(0);
		}

		UpdateActionButtons();
	}

	private static string BuildValidationText(BattleDeckValidationResult validation)
	{
		List<string> lines = new()
		{
			$"最低卡数: {validation.TotalCardCount}/{validation.EffectiveMinDeckSize}",
			$"影响因子: {validation.TotalBuildPoints}/{validation.EffectivePointBudget}",
			$"同名上限: {validation.EffectiveMaxCopiesPerCard}",
			$"超规槽位: {validation.UsedOverlimitCarrySlots}/{validation.EffectiveOverlimitCarrySlots}",
			$"循环限制: cycle {validation.EffectiveCycleCardLimit} / quick_cycle {validation.EffectiveQuickCycleCardLimit} / energy_positive {validation.EffectiveEnergyPositiveCardLimit}",
		};

		if (validation.Errors.Count == 0)
		{
			lines.Add("状态: 通过");
		}
		else
		{
			lines.Add("状态: 不通过");
			lines.AddRange(validation.Errors.Select(error => $"- {error}"));
		}

		if (validation.Warnings.Count > 0)
		{
			lines.Add("提示:");
			lines.AddRange(validation.Warnings.Select(warning => $"- {warning}"));
		}

		return string.Join('\n', lines);
	}

	private void OnAvailableSelected(long index)
	{
		if (index < 0 || index >= _availableTemplates.Length)
		{
			return;
		}

		BattleCardTemplate template = _availableTemplates[index];
		_detailLabel.Text = BuildTemplateDetailText(template);
	}

	private void OnDeckSelected(long index)
	{
		if (index < 0 || index >= _visibleDeckEntries.Length || BattleCardLibrary == null)
		{
			return;
		}

		string cardId = _visibleDeckEntries[(int)index].CardId;
		BattleCardTemplate? template = BattleCardLibrary.FindTemplate(cardId);
		_detailLabel.Text = template != null ? BuildTemplateDetailText(template) : cardId;
	}

	private static string BuildTemplateDetailText(BattleCardTemplate template)
	{
		List<string> lines = new()
		{
			$"[b]{template.DisplayName}[/b] ({template.CardId})",
			template.Description,
			$"费用 {template.Cost} / 范围 {template.Range} / 影响因子 {template.GetEffectiveBuildPoints()}",
			$"伤害 {template.Damage} / 治疗 {template.HealingAmount} / 抽牌 {template.DrawCount} / 回能 {template.EnergyGain} / 护盾 {template.ShieldGain}",
			$"目标 {template.TargetingMode}",
		};

		List<string> keywords = new();
		if (template.IsQuick)
		{
			keywords.Add("快速");
		}

		if (template.ExhaustsOnPlay)
		{
			keywords.Add("消耗");
		}

		if (keywords.Count > 0)
		{
			lines.Add($"词条: {string.Join(" / ", keywords)}");
		}

		lines.Add($"学习牌 {template.IsLearnedCard} / 可超规 {(!template.DisallowOverlimitCarry)} / 同名上限 {template.MaxCopiesInDeck}");
		lines.Add($"循环标签: {(template.CycleTags.Length == 0 ? "无" : string.Join(", ", template.CycleTags))}");
		return string.Join('\n', lines);
	}

	private DeckBuildSnapshot BuildWorkingDeckSnapshot()
	{
		return new DeckBuildSnapshot
		{
			BuildName = _session?.DeckBuildState.BuildName ?? "default",
			CardIds = _workingDeck.ToArray(),
			RelicIds = _session?.DeckBuildState.RelicIds ?? Array.Empty<string>(),
		};
	}

	private DeckListEntry[] BuildVisibleDeckEntries()
	{
		return _workingDeck
			.GroupBy(cardId => cardId, StringComparer.Ordinal)
			.OrderBy(group => group.Key, StringComparer.Ordinal)
			.Select(group => new DeckListEntry(group.Key, group.Count()))
			.ToArray();
	}

	private string BuildDeckEntryText(DeckListEntry entry, ProgressionSnapshot progression)
	{
		BattleCardTemplate? template = BattleCardLibrary?.FindTemplate(entry.CardId);
		bool usesOverlimitCarry = template != null && !template.CanCarryNormally(progression) && template.CanCarryOverlimit(progression);
		string displayName = template?.DisplayName ?? entry.CardId;
		return usesOverlimitCarry ? $"{displayName} x{entry.Count} [超规]" : $"{displayName} x{entry.Count}";
	}

	private void RefreshDeckDirtyFlag()
	{
		if (_session == null)
		{
			_deckDirty = false;
			return;
		}

		_deckDirty = !_workingDeck.SequenceEqual(_session.DeckBuildState.CardIds, StringComparer.Ordinal);
	}

	private static int GetCurrentCopies(IReadOnlyList<string> workingDeck, string cardId)
	{
		return workingDeck.Count(value => string.Equals(value, cardId, StringComparison.Ordinal));
	}

	private int GetDeckCopyLimit(BattleCardTemplate template, ProgressionSnapshot progression)
	{
		bool usesOverlimitCarry = !template.CanCarryNormally(progression) && template.CanCarryOverlimit(progression);
		if (usesOverlimitCarry)
		{
			return 1;
		}

		int effectiveMaxCopies = Math.Max(1, (BattleDeckBuildRules?.BaseMaxCopiesPerCard ?? 1) + progression.DeckMaxCopiesPerCardBonus);
		return Math.Min(effectiveMaxCopies, Math.Max(1, template.MaxCopiesInDeck));
	}

	private bool IsAtDeckCopyLimit(BattleCardTemplate template, ProgressionSnapshot progression)
	{
		return GetCurrentCopies(_workingDeck, template.CardId) >= GetDeckCopyLimit(template, progression);
	}

	private bool CanAddTemplateToWorkingDeck(BattleCardTemplate template, ProgressionSnapshot progression, out BattleDeckValidationResult validation)
	{
		if (_constructionService == null || _session == null || IsAtDeckCopyLimit(template, progression))
		{
			validation = new BattleDeckValidationResult();
			return false;
		}

		List<string> candidateDeck = new(_workingDeck) { template.CardId };
		validation = _constructionService.ValidateDeck(
			new DeckBuildSnapshot
			{
				BuildName = _session.DeckBuildState.BuildName,
				CardIds = candidateDeck.ToArray(),
				RelicIds = _session.DeckBuildState.RelicIds,
			},
			progression);
		return validation.CanAddCards;
	}

	private string GetSelectedAvailableCardId()
	{
		int[] selected = _availableList.GetSelectedItems();
		if (selected.Length == 0)
		{
			return string.Empty;
		}

		int selectedIndex = selected[0];
		return selectedIndex >= 0 && selectedIndex < _availableTemplates.Length
			? _availableTemplates[selectedIndex].CardId
			: string.Empty;
	}

	private string GetSelectedDeckCardId()
	{
		int[] selected = _deckList.GetSelectedItems();
		if (selected.Length == 0)
		{
			return string.Empty;
		}

		int selectedIndex = selected[0];
		return selectedIndex >= 0 && selectedIndex < _visibleDeckEntries.Length
			? _visibleDeckEntries[selectedIndex].CardId
			: string.Empty;
	}

	private void RestoreAvailableSelection(string cardId)
	{
		if (_availableTemplates.Length == 0)
		{
			return;
		}

		int selectedIndex = Array.FindIndex(_availableTemplates, template => string.Equals(template.CardId, cardId, StringComparison.Ordinal));
		_availableList.Select(selectedIndex >= 0 ? selectedIndex : 0);
	}

	private void RestoreDeckSelection(string cardId)
	{
		if (_visibleDeckEntries.Length == 0)
		{
			return;
		}

		int selectedIndex = Array.FindIndex(_visibleDeckEntries, entry => string.Equals(entry.CardId, cardId, StringComparison.Ordinal));
		_deckList.Select(selectedIndex >= 0 ? selectedIndex : 0);
	}

	private void UpdateActionButtons()
	{
		if (_constructionService == null || _session == null)
		{
			_addButton.Disabled = true;
			_removeButton.Disabled = true;
			return;
		}

		ProgressionSnapshot progression = _session.BuildProgressionSnapshotModel();
		bool canAdd = false;
		int[] selected = _availableList.GetSelectedItems();
		if (selected.Length > 0)
		{
			int selectedIndex = selected[0];
			if (selectedIndex >= 0 && selectedIndex < _availableTemplates.Length)
			{
				canAdd = CanAddTemplateToWorkingDeck(_availableTemplates[selectedIndex], progression, out _);
			}
		}

		_addButton.Disabled = !canAdd;
		_removeButton.Disabled = _deckList.GetSelectedItems().Length == 0;
	}

	private void OnAddPressed()
	{
		if (_constructionService == null || _session == null || BattleCardLibrary == null)
		{
			return;
		}

		int[] selected = _availableList.GetSelectedItems();
		if (selected.Length == 0)
		{
			return;
		}

		BattleCardTemplate template = _availableTemplates[selected[0]];
		ProgressionSnapshot progression = _session.BuildProgressionSnapshotModel();
		if (!CanAddTemplateToWorkingDeck(template, progression, out BattleDeckValidationResult validation))
		{
			_validationLabel.Text = IsAtDeckCopyLimit(template, progression)
				? "该卡已达携带上限"
				: BuildValidationText(validation) + (_deckDirty ? "\n未保存" : string.Empty);
			return;
		}

		_workingDeck.Add(template.CardId);
		RefreshDeckDirtyFlag();
		RefreshAll();
		RestoreDeckSelection(template.CardId);
	}

	private void OnRemovePressed()
	{
		int[] selected = _deckList.GetSelectedItems();
		if (selected.Length == 0)
		{
			return;
		}

		int selectedIndex = selected[0];
		if (selectedIndex < 0 || selectedIndex >= _visibleDeckEntries.Length)
		{
			return;
		}

		string cardId = _visibleDeckEntries[selectedIndex].CardId;
		int removeIndex = _workingDeck.FindIndex(value => string.Equals(value, cardId, StringComparison.Ordinal));
		if (removeIndex < 0)
		{
			return;
		}

		_workingDeck.RemoveAt(removeIndex);
		RefreshDeckDirtyFlag();
		RefreshAll();
		RestoreDeckSelection(cardId);
	}

	private void OnSavePressed()
	{
		if (_session == null || _constructionService == null)
		{
			return;
		}

		DeckBuildSnapshot snapshot = new()
		{
			BuildName = _session.DeckBuildState.BuildName,
			CardIds = _workingDeck.ToArray(),
			RelicIds = _session.DeckBuildState.RelicIds,
		};
		BattleDeckValidationResult validation = _constructionService.ValidateDeck(snapshot, _session.BuildProgressionSnapshotModel());
		if (!validation.IsValid)
		{
			_validationLabel.Text = BuildValidationText(validation);
			return;
		}

		_session.ApplyDeckBuildSnapshot(snapshot.ToDictionary());
		LoadWorkingDeckFromSession();
		RefreshAll();
		_validationLabel.Text = BuildValidationText(validation) + "\n已保存到 GlobalGameSession";
	}

	private void OnResetPressed()
	{
		_workingDeck.Clear();
		RefreshDeckDirtyFlag();
		RefreshAll();
		_detailLabel.Text = "Select a card.";
	}

	private void OnStarterPressed()
	{
		if (BattleCardLibrary == null)
		{
			return;
		}

		_workingDeck = BattleCardLibrary.BuildStarterDeckCardIds().ToList();
		RefreshDeckDirtyFlag();
		RefreshAll();
	}

	private sealed class DeckListEntry
	{
		public DeckListEntry(string cardId, int count)
		{
			CardId = cardId;
			Count = Math.Max(0, count);
		}

		public string CardId { get; }

		public int Count { get; }
	}
}
