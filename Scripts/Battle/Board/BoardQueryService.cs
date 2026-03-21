using System;
using System.Collections.Generic;
using Godot;

namespace CardChessDemo.Battle.Board;

public sealed class BoardQueryService
{
    private readonly BoardState _boardState;
    private readonly BoardObjectRegistry _registry;

    public BoardQueryService(BoardState boardState, BoardObjectRegistry registry)
    {
        _boardState = boardState;
        _registry = registry;
    }

    public IReadOnlyList<BoardObject> GetObjectsAtCell(Vector2I cell)
    {
        if (!_boardState.TryGetCell(cell, out BoardCellState? cellState) || cellState == null)
        {
            return Array.Empty<BoardObject>();
        }

        List<BoardObject> objects = new();

        // 返回顺序固定为：单位 -> blocking -> resident。
        // 这样上层调试或绘制时更容易先拿到主占位对象。
        if (!string.IsNullOrWhiteSpace(cellState.UnitObjectId) && _registry.TryGet(cellState.UnitObjectId, out BoardObject? unitObject) && unitObject != null)
        {
            objects.Add(unitObject);
        }

        foreach (string objectId in cellState.BlockingObjectIds)
        {
            if (_registry.TryGet(objectId, out BoardObject? boardObject) && boardObject != null)
            {
                objects.Add(boardObject);
            }
        }

        foreach (string objectId in cellState.ResidentObjectIds)
        {
            if (_registry.TryGet(objectId, out BoardObject? boardObject) && boardObject != null)
            {
                objects.Add(boardObject);
            }
        }

        return objects;
    }

    public int GetMoveCost(Vector2I cell)
    {
        BoardCellState cellState = _boardState.GetCell(cell);
        int moveCost = cellState.BaseMoveCost;

        foreach (BoardObject boardObject in GetObjectsAtCell(cell))
        {
            moveCost += boardObject.MoveCostModifier;
        }

        return Math.Max(0, moveCost);
    }

    public bool CanOccupyCell(string objectId, Vector2I targetCell, out string failureReason)
    {
        failureReason = string.Empty;

        if (!_registry.TryGet(objectId, out BoardObject? boardObject) || boardObject == null)
        {
            failureReason = $"Object {objectId} was not found in the registry.";
            return false;
        }

        return OccupancyRules.CanPlaceObject(_boardState, _registry, boardObject, targetCell, out failureReason);
    }

    public bool TryMoveObject(string objectId, Vector2I targetCell, out string failureReason)
    {
        failureReason = string.Empty;

        if (!_registry.TryGet(objectId, out BoardObject? boardObject) || boardObject == null)
        {
            failureReason = $"Object {objectId} was not found in the registry.";
            return false;
        }

        if (boardObject.Cell == targetCell)
        {
            return true;
        }

        if (!OccupancyRules.CanPlaceObject(_boardState, _registry, boardObject, targetCell, out failureReason))
        {
            return false;
        }

        // 当前移动只处理 board 层位置交换。
        // 还没有攻击触发、机会攻击、地形事件、时间推进或动画时序系统。
        _boardState.RemoveObject(boardObject);
        boardObject.SetCell(targetCell);
        _boardState.PlaceObject(boardObject);
        return true;
    }
}
