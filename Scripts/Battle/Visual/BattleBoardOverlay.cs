using System.Collections.Generic;
using Godot;
using CardChessDemo.Battle.Rooms;

namespace CardChessDemo.Battle.Visual;

public partial class BattleBoardOverlay : Node2D
{
    [Export] public Color HoverColor { get; set; } = new(0.2f, 0.85f, 1.0f, 0.28f);
    [Export] public Color ReachableColor { get; set; } = new(0.2f, 1.0f, 0.45f, 0.18f);
    [Export] public Color PathColor { get; set; } = new(1.0f, 0.9f, 0.3f, 0.82f);

    private BattleRoomTemplate? _room;
    private readonly List<Vector2I> _reachableCells = new();
    private readonly List<Vector2I> _previewPath = new();
    private bool _hasHoveredCell;
    private Vector2I _hoveredCell;

    public void Bind(BattleRoomTemplate room)
    {
        _room = room;
        QueueRedraw();
    }

    public void SetReachableCells(IEnumerable<Vector2I> cells)
    {
        _reachableCells.Clear();
        _reachableCells.AddRange(cells);
        QueueRedraw();
    }

    public void SetPreviewPath(IEnumerable<Vector2I> cells)
    {
        _previewPath.Clear();
        _previewPath.AddRange(cells);
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_room == null)
        {
            return;
        }

        Vector2 mouseGlobal = GetGlobalMousePosition();
        _hasHoveredCell = _room.TryScreenToCell(mouseGlobal, out _hoveredCell);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_room == null)
        {
            return;
        }

        foreach (Vector2I cell in _reachableCells)
        {
            DrawRect(_room.GetCellRect(cell), ReachableColor, true);
        }

        if (_previewPath.Count > 1)
        {
            for (int i = 0; i < _previewPath.Count - 1; i++)
            {
                DrawLine(
                    _room.CellToLocalCenter(_previewPath[i]),
                    _room.CellToLocalCenter(_previewPath[i + 1]),
                    PathColor,
                    2.0f,
                    true);
            }
        }

        if (_hasHoveredCell)
        {
            Rect2 hoverRect = _room.GetCellRect(_hoveredCell);
            DrawRect(hoverRect, HoverColor, true);
            DrawRect(hoverRect.Grow(-1.0f), PathColor, false, 2.0f);
        }
    }
}
