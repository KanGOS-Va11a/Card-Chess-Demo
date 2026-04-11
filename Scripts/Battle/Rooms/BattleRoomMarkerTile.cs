using Godot;

namespace CardChessDemo.Battle.Rooms;

[Tool]
public partial class BattleRoomMarkerTile : Node2D
{
	[Export] public Color FillColor { get; set; } = new(0.35f, 0.35f, 0.35f, 0.95f);
	[Export] public Color OutlineColor { get; set; } = Colors.Black;
	[Export] public Color TextColor { get; set; } = Colors.White;
	[Export] public string MarkerText { get; set; } = "?";

	public override void _Ready()
	{
		if (GetNodeOrNull<Polygon2D>("Fill") is Polygon2D fill)
		{
			fill.Color = FillColor;
		}

		if (GetNodeOrNull<Line2D>("Outline") is Line2D outline)
		{
			outline.DefaultColor = OutlineColor;
		}

		if (GetNodeOrNull<Label>("Label") is Label label)
		{
			label.Text = MarkerText;
			label.AddThemeFontSizeOverride("font_size", MarkerText.Length > 1 ? 7 : 8);
			label.AddThemeColorOverride("font_color", TextColor);
			label.AddThemeColorOverride("font_outline_color", OutlineColor);
			label.AddThemeConstantOverride("outline_size", 1);
		}
	}
}
