using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Map;

public static class MapGridService
{
	public static Vector2I WorldToCell(Vector2 worldPosition, int tileSize)
	{
		float safeTileSize = Mathf.Max(1.0f, tileSize);
		float halfTile = safeTileSize * 0.5f;
		return new Vector2I(
			Mathf.RoundToInt((worldPosition.X - halfTile) / safeTileSize),
			Mathf.RoundToInt((worldPosition.Y - halfTile) / safeTileSize));
	}

	public static Vector2 CellToWorldCenter(Vector2I cell, int tileSize)
	{
		float safeTileSize = Mathf.Max(1.0f, tileSize);
		float halfTile = safeTileSize * 0.5f;
		return new Vector2(
			cell.X * safeTileSize + halfTile,
			cell.Y * safeTileSize + halfTile);
	}

	public static Vector2I NormalizeFacingToCardinalOffset(Vector2 facingDirection)
	{
		if (facingDirection == Vector2.Zero)
		{
			return Vector2I.Down;
		}

		Vector2 normalized = facingDirection.Normalized();
		if (Mathf.Abs(normalized.X) > Mathf.Abs(normalized.Y))
		{
			return normalized.X < 0.0f ? Vector2I.Left : Vector2I.Right;
		}

		return normalized.Y < 0.0f ? Vector2I.Up : Vector2I.Down;
	}

	public static Vector2I GetFacingCell(Vector2 worldPosition, Vector2 facingDirection, int tileSize)
	{
		return WorldToCell(worldPosition, tileSize) + NormalizeFacingToCardinalOffset(facingDirection);
	}

	public static int GetManhattanDistance(Vector2I a, Vector2I b)
	{
		return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
	}

	public static IEnumerable<InteractableTemplate> EnumerateInteractables(Node? sceneRoot)
	{
		if (sceneRoot == null)
		{
			yield break;
		}

		foreach (InteractableTemplate interactable in EnumerateInteractablesRecursive(sceneRoot))
		{
			yield return interactable;
		}
	}

	public static Vector2 ResolveLocalPositionFromCell(Node2D node, Vector2I cell, int tileSize)
	{
		if (node.GetParent() is Node2D parentNode)
		{
			return parentNode.ToLocal(CellToWorldCenter(cell, tileSize));
		}

		return CellToWorldCenter(cell, tileSize);
	}

	private static IEnumerable<InteractableTemplate> EnumerateInteractablesRecursive(Node current)
	{
		if (current is InteractableTemplate interactable)
		{
			yield return interactable;
		}

		foreach (Node child in current.GetChildren())
		{
			foreach (InteractableTemplate nested in EnumerateInteractablesRecursive(child))
			{
				yield return nested;
			}
		}
	}
}
