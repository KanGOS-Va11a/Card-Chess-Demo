using Godot;

namespace CardChessDemo.Battle.Board;

public sealed class BoardTopology
{
    public BoardTopology(Vector2I boardSize, int cellSizePixels)
    {
        BoardSize = boardSize;
        CellSizePixels = cellSizePixels;
    }

    public Vector2I BoardSize { get; }

    public int CellSizePixels { get; }

    public Vector2 BoardPixelSize => new(BoardSize.X * CellSizePixels, BoardSize.Y * CellSizePixels);

    public bool IsInsideBoard(Vector2I cell)
    {
        return cell.X >= 0 && cell.Y >= 0 && cell.X < BoardSize.X && cell.Y < BoardSize.Y;
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
