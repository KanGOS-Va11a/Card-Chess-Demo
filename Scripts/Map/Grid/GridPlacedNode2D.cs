using Godot;

namespace CardChessDemo.Map;

[Tool]
public partial class GridPlacedNode2D : Node2D
{
	[Export] public bool UseGridPlacement { get; set; }
	[Export] public Vector2I Cell { get; set; } = Vector2I.Zero;
	[Export(PropertyHint.Range, "8,128,1")] public int GridTileSize { get; set; } = 16;
	[Export] public bool ShowEditorCellLabel { get; set; } = true;
	[Export] public Vector2 EditorLabelOffset { get; set; } = new(-14.0f, -18.0f);
	[Export(PropertyHint.Range, "8,32,1")] public int EditorLabelFontSize { get; set; } = 12;
	[Export] public Color EditorLabelColor { get; set; } = new(0.78f, 0.96f, 1.0f, 1.0f);

	public override void _Ready()
	{
		ApplyGridPlacement();
		if (Engine.IsEditorHint())
		{
			SetProcess(true);
		}
	}

	public override void _Process(double delta)
	{
		if (!Engine.IsEditorHint())
		{
			SetProcess(false);
			return;
		}

		ApplyGridPlacement();
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (!Engine.IsEditorHint() || !ShowEditorCellLabel)
		{
			return;
		}

		Vector2I cell = ResolveCell();
		string labelText = BuildEditorLabelText(cell);
		if (string.IsNullOrWhiteSpace(labelText))
		{
			return;
		}

		MapEditorDrawHelper.DrawLabel(
			this,
			EditorLabelOffset,
			labelText,
			EditorLabelFontSize,
			EditorLabelColor);
	}

	public virtual Vector2I ResolveCell()
	{
		return UseGridPlacement ? Cell : MapGridService.WorldToCell(GlobalPosition, GridTileSize);
	}

	public void ApplyGridPlacement()
	{
		if (!UseGridPlacement)
		{
			return;
		}

		Vector2 localPosition = MapGridService.ResolveLocalPositionFromCell(this, Cell, GridTileSize);
		if (Position != localPosition)
		{
			Position = localPosition;
		}
	}

	protected virtual string BuildEditorLabelText(Vector2I cell)
	{
		return $"({cell.X},{cell.Y})";
	}
}
