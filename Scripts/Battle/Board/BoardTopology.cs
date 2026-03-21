using System;
using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Battle.Board;

public sealed class BoardTopology
{
    private static readonly Vector2I[] CardinalDirectionsInternal =
    {
        new Vector2I(1, 0),
        new Vector2I(-1, 0),
        new Vector2I(0, 1),
        new Vector2I(0, -1),
    };

    public BoardTopology(Vector2I boardSize, int cellSizePixels)
    {
        BoardSize = boardSize;
        CellSizePixels = cellSizePixels;
    }

    public static IReadOnlyList<Vector2I> CardinalDirections => CardinalDirectionsInternal;

    public Vector2I BoardSize { get; }

    public int CellSizePixels { get; }

    public Vector2 BoardPixelSize => new(BoardSize.X * CellSizePixels, BoardSize.Y * CellSizePixels);

    public bool IsInsideBoard(Vector2I cell)
    {
        return cell.X >= 0 && cell.Y >= 0 && cell.X < BoardSize.X && cell.Y < BoardSize.Y;
    }

    public IEnumerable<Vector2I> EnumerateCardinalNeighbors(Vector2I cell)
    {
        foreach (Vector2I direction in CardinalDirectionsInternal)
        {
            Vector2I neighbor = cell + direction;
            if (IsInsideBoard(neighbor))
            {
                yield return neighbor;
            }
        }
    }

    public bool TryNormalizeCardinalDirection(Vector2I direction, out Vector2I normalizedDirection)
    {
        normalizedDirection = Vector2I.Zero;

        if (direction == Vector2I.Zero)
        {
            return false;
        }

        if (direction.X != 0 && direction.Y != 0)
        {
            return false;
        }

        normalizedDirection = new Vector2I(
            direction.X == 0 ? 0 : Math.Sign(direction.X),
            direction.Y == 0 ? 0 : Math.Sign(direction.Y));

        return true;
    }

    public bool TryLocalToCell(Vector2 localPosition, out Vector2I cell)
    {
        cell = new Vector2I(
            Mathf.FloorToInt(localPosition.X / CellSizePixels),
            Mathf.FloorToInt(localPosition.Y / CellSizePixels));

        return IsInsideBoard(cell);
    }

    public Vector2 CellToLocalCenter(Vector2I cell)
    {
        return new Vector2(
            cell.X * CellSizePixels + CellSizePixels * 0.5f,
            cell.Y * CellSizePixels + CellSizePixels * 0.5f);
    }

    public Rect2 GetCellRect(Vector2I cell)
    {
        return new Rect2(
            new Vector2(cell.X * CellSizePixels, cell.Y * CellSizePixels),
            new Vector2(CellSizePixels, CellSizePixels));
    }
}
