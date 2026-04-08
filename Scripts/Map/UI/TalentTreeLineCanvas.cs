using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace CardChessDemo.Map;

public partial class TalentTreeLineCanvas : Control
{
	public readonly record struct PolylineData(Vector2[] Points, Color Color, float Width);

	private PolylineData[] _lines = Array.Empty<PolylineData>();

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
	}

	public void SetLines(IEnumerable<PolylineData> lines)
	{
		_lines = lines?.ToArray() ?? Array.Empty<PolylineData>();
		QueueRedraw();
	}

	public override void _Draw()
	{
		foreach (PolylineData line in _lines)
		{
			if (line.Points == null || line.Points.Length < 2)
			{
				continue;
			}

			DrawPolyline(line.Points, line.Color, line.Width, true);
		}
	}
}
