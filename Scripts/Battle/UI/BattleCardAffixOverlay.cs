using Godot;

namespace CardChessDemo.Battle.UI;

public partial class BattleCardAffixOverlay : Control
{
	private const float BorderInset = 1.5f;
	private const float DashLength = 5.0f;
	private const float GapLength = 2.5f;
	private const float DashWidth = 2.0f;
	private bool _isExhaust;
	private bool _isSelected;
	private bool _isPlayable;

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
	}

	public void SetAffixes(bool isQuick, bool isExhaust, bool isSelected, bool isPlayable)
	{
		_isExhaust = isExhaust;
		_isSelected = isSelected;
		_isPlayable = isPlayable;
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (!_isExhaust)
		{
			return;
		}

		Rect2 rect = new(Vector2.One * BorderInset, Size - Vector2.One * BorderInset * 2.0f);
		if (rect.Size.X <= 0.0f || rect.Size.Y <= 0.0f)
		{
			return;
		}

		if (_isExhaust)
		{
			Color exhaustColor = _isSelected
				? new Color(1.0f, 0.84f, 0.52f, _isPlayable ? 0.95f : 0.55f)
				: new Color(0.95f, 0.76f, 0.44f, _isPlayable ? 0.90f : 0.50f);
			DrawDashedRect(rect, exhaustColor, DashWidth, DashLength, GapLength);
		}
	}

	private void DrawDashedRect(Rect2 rect, Color color, float width, float dashLength, float gapLength)
	{
		Vector2 topLeft = rect.Position;
		Vector2 topRight = new(rect.End.X, rect.Position.Y);
		Vector2 bottomLeft = new(rect.Position.X, rect.End.Y);
		Vector2 bottomRight = rect.End;

		DrawDashedSegment(topLeft, topRight, color, width, dashLength, gapLength);
		DrawDashedSegment(topRight, bottomRight, color, width, dashLength, gapLength);
		DrawDashedSegment(bottomRight, bottomLeft, color, width, dashLength, gapLength);
		DrawDashedSegment(bottomLeft, topLeft, color, width, dashLength, gapLength);
	}

	private void DrawDashedSegment(Vector2 from, Vector2 to, Color color, float width, float dashLength, float gapLength)
	{
		Vector2 delta = to - from;
		float totalLength = delta.Length();
		if (totalLength <= 0.01f)
		{
			return;
		}

		Vector2 direction = delta / totalLength;
		float cursor = 0.0f;
		while (cursor < totalLength)
		{
			float segmentLength = Mathf.Min(dashLength, totalLength - cursor);
			Vector2 segmentFrom = from + direction * cursor;
			Vector2 segmentTo = from + direction * (cursor + segmentLength);
			DrawLine(segmentFrom, segmentTo, color, width, true);
			cursor += dashLength + gapLength;
		}
	}
}
