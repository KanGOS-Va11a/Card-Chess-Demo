using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class LootContainerOverlay : CanvasLayer
{
	public readonly struct LootEntry
	{
		public LootEntry(string itemId, int amount)
		{
			ItemId = itemId;
			Amount = amount;
		}

		public string ItemId { get; }
		public int Amount { get; }
	}

	private ColorRect? _dimRect;
	private PanelContainer? _panel;
	private Label? _titleLabel;
	private Label? _leftTitleLabel;
	private Label? _statusLabel;
	private Label? _bottomHintLabel;
	private ItemList? _sourceList;
	private ItemList? _bagList;

	private readonly List<LootEntry> _sourceEntries = new();
	private Func<string, int, bool>? _onTakeItem;
	private Action? _onClosed;
	private GlobalGameSession? _session;
	private Player? _player;
	private int _selectedIndex;
	private bool _isOpen;

	private bool _playerPrevPhysics;
	private bool _playerPrevProcess;
	private bool _playerPrevInput;
	private bool _playerPrevUnhandledInput;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Layer = 235;
		SetProcessUnhandledInput(true);
		EnsureUiBuilt();
		SetVisibleInternal(_isOpen);
	}

	public void Open(
		string containerTitle,
		string sourceTitle,
		GlobalGameSession? session,
		Player? player,
		IEnumerable<LootEntry> sourceEntries,
		Func<string, int, bool> onTakeItem,
		Action? onClosed = null)
	{
		EnsureUiBuilt();

		_session = session;
		_player = player;
		_onTakeItem = onTakeItem;
		_onClosed = onClosed;
		_selectedIndex = 0;
		_sourceEntries.Clear();
		_sourceEntries.AddRange(sourceEntries.Where(item => !string.IsNullOrWhiteSpace(item.ItemId) && item.Amount > 0));

		if (_titleLabel != null)
		{
			_titleLabel.Text = string.IsNullOrWhiteSpace(containerTitle) ? "容器交互" : $"{containerTitle} - 物品选择";
		}

		if (_leftTitleLabel != null)
		{
			_leftTitleLabel.Text = string.IsNullOrWhiteSpace(sourceTitle) ? "容器内容" : sourceTitle;
		}

		SetVisibleInternal(true);
		LockPlayerInput();
		RefreshAll("左侧选择物品后按 E 拾取，按 C 退出");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!_isOpen || @event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
		{
			return;
		}

		if (keyEvent.Keycode == Key.C || keyEvent.Keycode == Key.Escape)
		{
			Close();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (keyEvent.Keycode == Key.E || keyEvent.Keycode == Key.Enter || keyEvent.Keycode == Key.KpEnter)
		{
			TryTakeSelectedItem();
			GetViewport().SetInputAsHandled();
		}
	}

	private void EnsureUiBuilt()
	{
		if (_panel != null)
		{
			return;
		}

		_dimRect = new ColorRect
		{
			Name = "Dim",
			AnchorLeft = 0.0f,
			AnchorTop = 0.0f,
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			Color = new Color(0.0f, 0.0f, 0.0f, 0.62f),
			MouseFilter = Control.MouseFilterEnum.Stop,
		};
		AddChild(_dimRect);

		_panel = new PanelContainer
		{
			Name = "LootPanel",
			AnchorLeft = 0.5f,
			AnchorTop = 0.5f,
			AnchorRight = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -312.0f,
			OffsetTop = -176.0f,
			OffsetRight = 312.0f,
			OffsetBottom = 176.0f,
			MouseFilter = Control.MouseFilterEnum.Stop,
		};
		AddChild(_panel);

		MarginContainer margin = new()
		{
			AnchorLeft = 0.0f,
			AnchorTop = 0.0f,
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			OffsetLeft = 12.0f,
			OffsetTop = 10.0f,
			OffsetRight = -12.0f,
			OffsetBottom = -10.0f,
		};
		_panel.AddChild(margin);

		VBoxContainer root = new()
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
		};
		margin.AddChild(root);

		_titleLabel = new Label
		{
			Text = "容器交互",
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		root.AddChild(_titleLabel);

		HSplitContainer split = new()
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
		};
		root.AddChild(split);

		VBoxContainer leftColumn = BuildColumn("容器内容", out _leftTitleLabel, out _sourceList);
		split.AddChild(leftColumn);

		VBoxContainer rightColumn = BuildColumn("当前背包", out _, out _bagList);
		split.AddChild(rightColumn);

		_bottomHintLabel = new Label
		{
			Name = "BottomHintLabel",
			AnchorLeft = 0.0f,
			AnchorTop = 1.0f,
			AnchorRight = 1.0f,
			AnchorBottom = 1.0f,
			OffsetLeft = 20.0f,
			OffsetTop = -30.0f,
			OffsetRight = -20.0f,
			OffsetBottom = -6.0f,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Text = "",
		};
		AddChild(_bottomHintLabel);
		_statusLabel = _bottomHintLabel;

		if (_sourceList != null)
		{
			_sourceList.ItemSelected += OnSourceItemSelected;
		}
	}

	private static VBoxContainer BuildColumn(string title, out Label titleLabel, out ItemList itemList)
	{
		VBoxContainer column = new()
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
		};

		titleLabel = new Label
		{
			Text = title,
			HorizontalAlignment = HorizontalAlignment.Center,
		};
		column.AddChild(titleLabel);

		itemList = new ItemList
		{
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			SizeFlagsVertical = Control.SizeFlags.ExpandFill,
			SelectMode = ItemList.SelectModeEnum.Single,
		};
		column.AddChild(itemList);
		return column;
	}

	private void OnSourceItemSelected(long index)
	{
		_selectedIndex = Mathf.Max(0, (int)index);
	}

	private void TryTakeSelectedItem()
	{
		if (!_isOpen || _sourceEntries.Count == 0 || _onTakeItem == null)
		{
			return;
		}

		int index = Mathf.Clamp(_selectedIndex, 0, _sourceEntries.Count - 1);
		LootEntry selected = _sourceEntries[index];
		if (!_onTakeItem.Invoke(selected.ItemId, selected.Amount))
		{
			RefreshAll("拾取失败，请重试");
			return;
		}

		_sourceEntries.RemoveAt(index);
		_selectedIndex = Mathf.Clamp(_selectedIndex, 0, Math.Max(0, _sourceEntries.Count - 1));
		string statusText = _sourceEntries.Count == 0
			? $"已拾取 {selected.ItemId} x{selected.Amount}，容器已空，按 C 退出"
			: $"已拾取 {selected.ItemId} x{selected.Amount}";
		RefreshAll(statusText);
	}

	private void RefreshAll(string statusText)
	{
		RefreshSourceList();
		RefreshBagList();
		if (_statusLabel != null)
		{
			_statusLabel.Text = statusText;
		}
	}

	private void RefreshSourceList()
	{
		if (_sourceList == null)
		{
			return;
		}

		_sourceList.Clear();
		if (_sourceEntries.Count == 0)
		{
			_sourceList.AddItem("- （空）");
			return;
		}

		foreach (LootEntry entry in _sourceEntries)
		{
			_sourceList.AddItem($"{entry.ItemId} x{entry.Amount}");
		}

		_selectedIndex = Mathf.Clamp(_selectedIndex, 0, _sourceEntries.Count - 1);
		_sourceList.Select(_selectedIndex);
	}

	private void RefreshBagList()
	{
		if (_bagList == null)
		{
			return;
		}

		_bagList.Clear();
		if (_session == null || _session.InventoryState.ItemCounts.Count == 0)
		{
			_bagList.AddItem("- （空）");
			return;
		}

		IEnumerable<string> keys = _session.InventoryState.ItemCounts.Keys
			.Select(item => item.AsString())
			.OrderBy(item => item, StringComparer.Ordinal);

		foreach (string key in keys)
		{
			int amount = _session.InventoryState.ItemCounts.TryGetValue(key, out Variant countValue)
				? countValue.AsInt32()
				: 0;
			_bagList.AddItem($"{key} x{amount}");
		}
	}

	private void Close()
	{
		if (!_isOpen)
		{
			return;
		}

		SetVisibleInternal(false);
		UnlockPlayerInput();
		_onClosed?.Invoke();
		_onClosed = null;
		_onTakeItem = null;
		_sourceEntries.Clear();
	}

	private void SetVisibleInternal(bool visible)
	{
		_isOpen = visible;
		if (_dimRect != null)
		{
			_dimRect.Visible = visible;
		}

		if (_panel != null)
		{
			_panel.Visible = visible;
		}

		if (_bottomHintLabel != null)
		{
			_bottomHintLabel.Visible = visible;
		}

		Visible = visible;
	}

	private void LockPlayerInput()
	{
		if (_player == null)
		{
			return;
		}

		_playerPrevPhysics = _player.IsPhysicsProcessing();
		_playerPrevProcess = _player.IsProcessing();
		_playerPrevInput = _player.IsProcessingInput();
		_playerPrevUnhandledInput = _player.IsProcessingUnhandledInput();

		_player.SetPhysicsProcess(false);
		_player.SetProcess(false);
		_player.SetProcessInput(false);
		_player.SetProcessUnhandledInput(false);
	}

	private void UnlockPlayerInput()
	{
		if (_player == null)
		{
			return;
		}

		_player.SetPhysicsProcess(_playerPrevPhysics);
		_player.SetProcess(_playerPrevProcess);
		_player.SetProcessInput(_playerPrevInput);
		_player.SetProcessUnhandledInput(_playerPrevUnhandledInput);
		_player = null;
	}
}