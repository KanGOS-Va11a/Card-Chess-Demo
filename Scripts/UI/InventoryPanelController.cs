using Godot;
using System.Text;

public partial class InventoryPanelController : Node
{
    [Export] public NodePath PanelPath = new("../InventoryPanel");
    [Export] public NodePath ContentLabelPath = new("../InventoryPanel/Margin/VBox/ContentLabel");
    [Export] public NodePath HintLabelPath = new("../InventoryPanel/Margin/VBox/HintLabel");

    private Panel _panel;
    private Label _contentLabel;
    private Label _hintLabel;
    private GameSession _session;

    public override void _Ready()
    {
        _panel = GetNodeOrNull<Panel>(PanelPath);
        _contentLabel = GetNodeOrNull<Label>(ContentLabelPath);
        _hintLabel = GetNodeOrNull<Label>(HintLabelPath);
        _session = GetNodeOrNull<GameSession>("/root/GameSession");

        if (_panel != null)
        {
            _panel.Visible = false;
        }

        RefreshInventoryText();
    }

    public override void _Input(InputEvent @event)
    {
        bool isToggleInventory = @event.IsActionPressed("open_inventory");

        if (!isToggleInventory && @event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            isToggleInventory = keyEvent.Keycode == Key.Tab || keyEvent.PhysicalKeycode == Key.Tab;
        }

        if (isToggleInventory)
        {
            ToggleInventory();
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event.IsActionPressed("ui_cancel") && _panel != null && _panel.Visible)
        {
            _panel.Visible = false;
            GetViewport().SetInputAsHandled();
        }
    }

    private void ToggleInventory()
    {
        if (_panel == null)
        {
            return;
        }

        _panel.Visible = !_panel.Visible;
        if (_panel.Visible)
        {
            RefreshInventoryText();
        }
    }

    private void RefreshInventoryText()
    {
        if (_contentLabel == null)
        {
            return;
        }

        if (_session == null)
        {
            _contentLabel.Text = "未找到 GameSession";
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("普通物品：");

        bool hasItems = false;
        foreach (StringName itemId in _session.inventory_state.items.Keys)
        {
            int amount = _session.inventory_state.items[itemId];
            if (amount <= 0)
            {
                continue;
            }

            sb.Append("- ");
            sb.Append(itemId);
            sb.Append(" x");
            sb.AppendLine(amount.ToString());
            hasItems = true;
        }

        if (!hasItems)
        {
            sb.AppendLine("- （空）");
        }

        sb.AppendLine();
        sb.AppendLine("关键物品：");
        if (_session.inventory_state.key_items.Count == 0)
        {
            sb.AppendLine("- （无）");
        }
        else
        {
            for (int i = 0; i < _session.inventory_state.key_items.Count; i++)
            {
                sb.Append("- ");
                sb.AppendLine(_session.inventory_state.key_items[i]);
            }
        }

        _contentLabel.Text = sb.ToString().TrimEnd();

        if (_hintLabel != null)
        {
            _hintLabel.Text = "Tab 关闭背包";
        }
    }
}
