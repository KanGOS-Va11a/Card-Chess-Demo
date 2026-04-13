using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using CardChessDemo.Battle.Rooms;

namespace CardChessDemo.Battle.Visual;

public partial class BattleBoardOverlay : Node2D
{
    private const string ArcTerrainSpriteSheetPath = "res://Assets/Tilemap/Battle/terrain/electiric_terrain.png";
    private const string OverlayFontPath = "res://Assets/Fonts/unifont_t-17.0.04.otf";
    private static readonly Texture2D? ArcTerrainSpriteSheet = ResourceLoader.Load<Texture2D>(ArcTerrainSpriteSheetPath);
    private static readonly FontFile? OverlayFont = ResourceLoader.Load<FontFile>(OverlayFontPath);

    [Export] public Color HoverColor { get; set; } = new(0.2f, 0.85f, 1.0f, 0.28f);
    [Export] public Color ReachableColor { get; set; } = new(0.42f, 0.98f, 0.22f, 0.18f);
    [Export] public Color AttackTargetColor { get; set; } = new(1.0f, 0.35f, 0.35f, 0.22f);
    [Export] public Color SupportTargetColor { get; set; } = new(0.16f, 0.92f, 0.86f, 0.22f);
    [Export] public Color ArcTerrainColor { get; set; } = new(0.34f, 0.46f, 1.0f, 0.22f);
    [Export] public Color FireTerrainColor { get; set; } = new(1.0f, 0.55f, 0.14f, 0.20f);
    [Export] public Color TelegraphColor { get; set; } = new(1.0f, 0.88f, 0.16f, 0.26f);
    [Export] public Color EscapeCellColor { get; set; } = new(0.18f, 0.72f, 0.28f, 0.28f);
    [Export] public Color EscapeArrowColor { get; set; } = new(0.94f, 1.0f, 0.74f, 0.95f);
    [Export] public Color PathColor { get; set; } = new(1.0f, 0.9f, 0.3f, 0.82f);
    [Export(PropertyHint.Range, "0.01,0.40,0.01")] public float CellRevealDuration { get; set; } = 0.16f;
    [Export(PropertyHint.Range, "0.00,0.20,0.005")] public float CellRingDelaySeconds { get; set; } = 0.018f;
    [Export(PropertyHint.Range, "0.05,0.60,0.01")] public float MinCellRevealScale { get; set; } = 0.12f;
    [Export(PropertyHint.Range, "0.01,0.30,0.01")] public float PathSegmentRevealDuration { get; set; } = 0.12f;
    [Export(PropertyHint.Range, "0.00,0.16,0.005")] public float PathSegmentDelaySeconds { get; set; } = 0.028f;
    [Export(PropertyHint.Range, "1,24,1")] public int ArcTerrainAnimationFps { get; set; } = 8;

    private BattleRoomTemplate? _room;
    private readonly AnimatedCellLayer _reachableCells = new();
    private readonly AnimatedCellLayer _attackTargetCells = new();
    private readonly AnimatedCellLayer _supportTargetCells = new();
    private readonly List<Vector2I> _arcTerrainCells = new();
    private readonly List<Vector2I> _fireTerrainCells = new();
    private readonly List<Vector2I> _telegraphCells = new();
    private readonly List<Vector2I> _escapeCells = new();
    private readonly List<Vector2I> _previewPath = new();
    private double _previewPathAnimationStartTimeSeconds;
    private bool _isPreviewPathAnimating;
    private bool _hasHoveredCell;
    private Vector2I _hoveredCell;

    public void Bind(BattleRoomTemplate room)
    {
        _room = room;
        QueueRedraw();
    }

    public void SetReachableCells(IEnumerable<Vector2I> cells, Vector2I? originCell = null)
    {
        SetAnimatedLayerCells(_reachableCells, cells, originCell);
        QueueRedraw();
    }

    public void SetAttackTargetCells(IEnumerable<Vector2I> cells, Vector2I? originCell = null)
    {
        SetAnimatedLayerCells(_attackTargetCells, cells, originCell);
        QueueRedraw();
    }

    public void SetSupportTargetCells(IEnumerable<Vector2I> cells, Vector2I? originCell = null)
    {
        SetAnimatedLayerCells(_supportTargetCells, cells, originCell);
        QueueRedraw();
    }

    public void SetEscapeCells(IEnumerable<Vector2I> cells)
    {
        Vector2I[] orderedCells = cells
            .Distinct()
            .OrderBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .ToArray();

        if (_escapeCells.SequenceEqual(orderedCells))
        {
            return;
        }

        _escapeCells.Clear();
        _escapeCells.AddRange(orderedCells);
        QueueRedraw();
    }

    public void SetArcTerrainCells(IEnumerable<Vector2I> cells)
    {
        Vector2I[] orderedCells = cells
            .Distinct()
            .OrderBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .ToArray();

        if (_arcTerrainCells.SequenceEqual(orderedCells))
        {
            return;
        }

        _arcTerrainCells.Clear();
        _arcTerrainCells.AddRange(orderedCells);
        QueueRedraw();
    }

    public void SetFireTerrainCells(IEnumerable<Vector2I> cells)
    {
        Vector2I[] orderedCells = cells
            .Distinct()
            .OrderBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .ToArray();

        if (_fireTerrainCells.SequenceEqual(orderedCells))
        {
            return;
        }

        _fireTerrainCells.Clear();
        _fireTerrainCells.AddRange(orderedCells);
        QueueRedraw();
    }

    public void SetTelegraphCells(IEnumerable<Vector2I> cells)
    {
        Vector2I[] orderedCells = cells
            .Distinct()
            .OrderBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .ToArray();

        if (_telegraphCells.SequenceEqual(orderedCells))
        {
            return;
        }

        _telegraphCells.Clear();
        _telegraphCells.AddRange(orderedCells);
        QueueRedraw();
    }

    public void SetPreviewPath(IEnumerable<Vector2I> cells)
    {
        Vector2I[] orderedCells = cells.ToArray();
        if (_previewPath.SequenceEqual(orderedCells))
        {
            return;
        }

        _previewPath.Clear();
        _previewPath.AddRange(orderedCells);
        _previewPathAnimationStartTimeSeconds = GetNowSeconds();
        _isPreviewPathAnimating = _previewPath.Count > 1;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (_room == null)
        {
            return;
        }

        if (_reachableCells.IsAnimating || _attackTargetCells.IsAnimating || _supportTargetCells.IsAnimating || _isPreviewPathAnimating)
        {
            QueueRedraw();
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

        DrawAnimatedCells(_reachableCells, ReachableColor);
        DrawAnimatedCells(_attackTargetCells, AttackTargetColor);
        DrawAnimatedCells(_supportTargetCells, SupportTargetColor);
        DrawArcTerrainCells();
        DrawFireTerrainCells();
        DrawTelegraphCells();
        DrawEscapeCells();

        if (_previewPath.Count > 1)
        {
            bool hasAnimatingSegment = false;
            double nowSeconds = GetNowSeconds();
            for (int i = 0; i < _previewPath.Count - 1; i++)
            {
                float reveal = GetPathSegmentRevealProgress(i, nowSeconds);
                if (reveal <= 0.0f)
                {
                    hasAnimatingSegment = true;
                    continue;
                }

                Vector2 from = _room.CellToLocalCenter(_previewPath[i]);
                Vector2 to = _room.CellToLocalCenter(_previewPath[i + 1]);
                Vector2 animatedTo = from.Lerp(to, EaseOutCubic(reveal));
                DrawLine(from, animatedTo, PathColor, 2.0f, true);

                if (reveal < 1.0f)
                {
                    hasAnimatingSegment = true;
                }
            }

            _isPreviewPathAnimating = hasAnimatingSegment;
        }

        if (_hasHoveredCell)
        {
            Rect2 hoverRect = _room.GetCellRect(_hoveredCell);
        DrawRect(hoverRect, HoverColor, true);
        DrawRect(hoverRect.Grow(-1.0f), PathColor, false, 2.0f);
    }
    }

    private void SetAnimatedLayerCells(AnimatedCellLayer layer, IEnumerable<Vector2I> cells, Vector2I? originCell)
    {
        Vector2I[] orderedCells = cells
            .Distinct()
            .OrderBy(cell => cell.Y)
            .ThenBy(cell => cell.X)
            .ToArray();

        bool hasOrigin = originCell.HasValue;
        Vector2I resolvedOrigin = originCell ?? Vector2I.Zero;
        if (layer.Cells.SequenceEqual(orderedCells)
            && layer.HasOrigin == hasOrigin
            && (!hasOrigin || layer.OriginCell == resolvedOrigin))
        {
            return;
        }

        layer.Cells.Clear();
        layer.Cells.AddRange(orderedCells);
        layer.HasOrigin = hasOrigin;
        layer.OriginCell = resolvedOrigin;
        layer.AnimationStartTimeSeconds = GetNowSeconds();
        layer.IsAnimating = orderedCells.Length > 0 && hasOrigin;
    }

    private void DrawAnimatedCells(
        AnimatedCellLayer layer,
        Color fillColor)
    {
        if (_room == null)
        {
            return;
        }

        bool hasAnimatingCell = false;
        double nowSeconds = GetNowSeconds();

        foreach (Vector2I cell in layer.Cells)
        {
            Rect2 cellRect = _room.GetCellRect(cell);
            float reveal = GetCellRevealProgress(layer, cell, nowSeconds);
            if (reveal < 1.0f)
            {
                hasAnimatingCell = true;
            }

            Color borderColor = BuildCellBorderColor(fillColor);
            DrawExpandedCell(cellRect, fillColor, reveal, true, 0.0f);
            DrawExpandedCell(cellRect.Grow(-1.0f), borderColor, reveal, false, 2.0f);
        }

        layer.IsAnimating = hasAnimatingCell;
    }

    private void DrawExpandedCell(Rect2 cellRect, Color color, float reveal, bool filled, float width)
    {
        reveal = Mathf.Clamp(reveal, 0.0f, 1.0f);
        if (reveal <= 0.0f)
        {
            return;
        }

        float eased = EaseOutCubic(reveal);
        float scale = Mathf.Lerp(MinCellRevealScale, 1.0f, eased);
        Vector2 targetSize = cellRect.Size * scale;
        Rect2 expandedRect = new(cellRect.GetCenter() - targetSize * 0.5f, targetSize);

        Color drawColor = color;
        drawColor.A *= eased;
        if (filled)
        {
            DrawRect(expandedRect, drawColor, true);
            return;
        }

        DrawRect(expandedRect, drawColor, false, width);
    }

    private float GetCellRevealProgress(AnimatedCellLayer layer, Vector2I cell, double nowSeconds)
    {
        if (!layer.HasOrigin)
        {
            return 1.0f;
        }

        int ringDistance = Mathf.Abs(cell.X - layer.OriginCell.X) + Mathf.Abs(cell.Y - layer.OriginCell.Y);
        double delay = ringDistance * CellRingDelaySeconds;
        double elapsed = nowSeconds - layer.AnimationStartTimeSeconds - delay;
        if (elapsed <= 0.0d)
        {
            return 0.0f;
        }

        return Mathf.Clamp((float)(elapsed / Math.Max(CellRevealDuration, 0.001f)), 0.0f, 1.0f);
    }

    private static float EaseOutCubic(float t)
    {
        float inverse = 1.0f - t;
        return 1.0f - inverse * inverse * inverse;
    }

    private float GetPathSegmentRevealProgress(int segmentIndex, double nowSeconds)
    {
        double delay = segmentIndex * PathSegmentDelaySeconds;
        double elapsed = nowSeconds - _previewPathAnimationStartTimeSeconds - delay;
        if (elapsed <= 0.0d)
        {
            return 0.0f;
        }

        return Mathf.Clamp((float)(elapsed / Math.Max(PathSegmentRevealDuration, 0.001f)), 0.0f, 1.0f);
    }

    private static Color BuildCellBorderColor(Color fillColor)
    {
        Color borderColor = fillColor.Darkened(0.42f);
        borderColor.A = Mathf.Clamp(fillColor.A + 0.24f, 0.0f, 1.0f);
        return borderColor;
    }

    private void DrawEscapeCells()
    {
        if (_room == null)
        {
            return;
        }

        Color borderColor = BuildCellBorderColor(EscapeCellColor);
        foreach (Vector2I cell in _escapeCells)
        {
            Rect2 cellRect = _room.GetCellRect(cell);
            DrawRect(cellRect, EscapeCellColor, true);
            DrawRect(cellRect.Grow(-1.0f), borderColor, false, 2.0f);
            DrawEscapeArrow(cellRect, ResolveEscapeArrowDirection(cell));
        }
    }

    private void DrawArcTerrainCells()
    {
        if (_room == null)
        {
            return;
        }

        Color borderColor = BuildCellBorderColor(ArcTerrainColor);
        Rect2? frameRegion = ResolveArcTerrainFrameRegion();
        foreach (Vector2I cell in _arcTerrainCells)
        {
            Rect2 cellRect = _room.GetCellRect(cell);
            if (ArcTerrainSpriteSheet != null && frameRegion.HasValue)
            {
                DrawTextureRectRegion(ArcTerrainSpriteSheet, cellRect, frameRegion.Value, Colors.White);
                DrawRect(cellRect, new Color(0.34f, 0.46f, 1.0f, 0.08f), true);
            }
            else
            {
                DrawRect(cellRect, ArcTerrainColor, true);
            }
            DrawRect(cellRect.Grow(-1.0f), borderColor, false, 2.0f);
        }
    }

    private void DrawFireTerrainCells()
    {
        if (_room == null)
        {
            return;
        }

        Color borderColor = BuildCellBorderColor(FireTerrainColor);
        foreach (Vector2I cell in _fireTerrainCells)
        {
            Rect2 cellRect = _room.GetCellRect(cell);
            DrawRect(cellRect, FireTerrainColor, true);
            DrawRect(cellRect.Grow(-1.0f), borderColor, false, 2.0f);
        }
    }

    private void DrawTelegraphCells()
    {
        if (_room == null)
        {
            return;
        }

        Color borderColor = BuildCellBorderColor(TelegraphColor);
        foreach (Vector2I cell in _telegraphCells)
        {
            Rect2 cellRect = _room.GetCellRect(cell);
            DrawRect(cellRect, TelegraphColor, true);
            DrawRect(cellRect.Grow(-1.0f), borderColor, false, 2.0f);
            DrawTelegraphMarker(cellRect);
        }
    }

    private void DrawTelegraphMarker(Rect2 cellRect)
    {
        if (OverlayFont == null)
        {
            return;
        }

        const string markerText = "!";
        const int fontSize = 16;
        Vector2 textSize = OverlayFont.GetStringSize(markerText, HorizontalAlignment.Left, -1.0f, fontSize);
        Vector2 baseline = new(
            cellRect.GetCenter().X - textSize.X * 0.5f,
            cellRect.GetCenter().Y + textSize.Y * 0.35f);

        DrawStringOutline(OverlayFont, baseline, markerText, HorizontalAlignment.Left, -1.0f, fontSize, 2, Colors.Black);
        DrawString(OverlayFont, baseline, markerText, HorizontalAlignment.Left, -1.0f, fontSize, new Color(1.0f, 0.96f, 0.88f, 1.0f));
    }

    private void DrawEscapeArrow(Rect2 cellRect, Vector2 direction)
    {
        Vector2 center = cellRect.GetCenter();
        Vector2 shaftEnd = center + direction * (Mathf.Min(cellRect.Size.X, cellRect.Size.Y) * 0.26f);
        DrawLine(center, shaftEnd, EscapeArrowColor, 2.0f, true);

        Vector2 normal = new(-direction.Y, direction.X);
        Vector2 tip = center + direction * (Mathf.Min(cellRect.Size.X, cellRect.Size.Y) * 0.40f);
        Vector2 headBase = shaftEnd;
        Vector2[] arrowHead =
        {
            tip,
            headBase - direction * 2.0f + normal * 3.0f,
            headBase - direction * 2.0f - normal * 3.0f,
        };
        DrawColoredPolygon(arrowHead, EscapeArrowColor);
    }

    private Vector2 ResolveEscapeArrowDirection(Vector2I cell)
    {
        if (_room == null)
        {
            return Vector2.Right;
        }

        int maxX = _room.BoardSize.X - 1;
        int maxY = _room.BoardSize.Y - 1;
        if (cell == Vector2I.Zero)
        {
            return Vector2.Left;
        }

        if (cell.X == maxX && cell.Y == 0)
        {
            return Vector2.Up;
        }

        if (cell.X == maxX && cell.Y == maxY)
        {
            return Vector2.Right;
        }

        if (cell.X == 0 && cell.Y == maxY)
        {
            return Vector2.Down;
        }

        if (cell.Y == 0)
        {
            return Vector2.Up;
        }

        if (cell.X == maxX)
        {
            return Vector2.Right;
        }

        if (cell.Y == maxY)
        {
            return Vector2.Down;
        }

        return Vector2.Left;
    }

    private static double GetNowSeconds()
    {
        return Time.GetTicksMsec() / 1000.0d;
    }

    private Rect2? ResolveArcTerrainFrameRegion()
    {
        if (ArcTerrainSpriteSheet == null || ArcTerrainAnimationFps <= 0)
        {
            return null;
        }

        const int frameWidth = 16;
        const int frameHeight = 16;
        int frameCount = Mathf.Max(1, ArcTerrainSpriteSheet.GetWidth() / frameWidth);
        int millisecondsPerFrame = Mathf.Max(1, 1000 / ArcTerrainAnimationFps);
        long ticksMs = (long)Time.GetTicksMsec();
        int frameIndex = (int)(ticksMs / millisecondsPerFrame) % frameCount;
        return new Rect2(frameIndex * frameWidth, 0.0f, frameWidth, frameHeight);
    }

    private sealed class AnimatedCellLayer
    {
        public List<Vector2I> Cells { get; } = new();
        public bool HasOrigin { get; set; }
        public Vector2I OriginCell { get; set; }
        public double AnimationStartTimeSeconds { get; set; }
        public bool IsAnimating { get; set; }
    }
}
