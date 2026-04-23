using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Map;

public partial class GridTriggerNode2D : GridPlacedNode2D
{
	[Export] public Vector2I SizeInCells { get; set; } = Vector2I.One;

	public bool ContainsCell(Vector2I cell)
	{
		Vector2I origin = ResolveCell();
		Vector2I safeSize = new(
			System.Math.Max(1, SizeInCells.X),
			System.Math.Max(1, SizeInCells.Y));
		return cell.X >= origin.X
			&& cell.Y >= origin.Y
			&& cell.X < origin.X + safeSize.X
			&& cell.Y < origin.Y + safeSize.Y;
	}

	public IEnumerable<Vector2I> EnumerateCoveredCells()
	{
		Vector2I origin = ResolveCell();
		Vector2I safeSize = new(
			System.Math.Max(1, SizeInCells.X),
			System.Math.Max(1, SizeInCells.Y));
		for (int y = 0; y < safeSize.Y; y++)
		{
			for (int x = 0; x < safeSize.X; x++)
			{
				yield return new Vector2I(origin.X + x, origin.Y + y);
			}
		}
	}
}
