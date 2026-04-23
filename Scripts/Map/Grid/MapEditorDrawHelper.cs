using Godot;

namespace CardChessDemo.Map;

public static class MapEditorDrawHelper
{
	private const string OverlayFontPath = "res://Assets/Fonts/unifont_t-17.0.04.otf";
	private static readonly FontFile? OverlayFont = ResourceLoader.Load<FontFile>(OverlayFontPath);

	public static void DrawLabel(
		CanvasItem canvas,
		Vector2 position,
		string text,
		int fontSize = 12,
		Color? textColor = null,
		Color? backgroundColor = null,
		float padding = 2.0f)
	{
		if (canvas == null || string.IsNullOrWhiteSpace(text))
		{
			return;
		}

		Font font = OverlayFont ?? ThemeDB.FallbackFont;
		if (font == null)
		{
			return;
		}

		Color resolvedTextColor = textColor ?? Colors.White;
		Color resolvedBackgroundColor = backgroundColor ?? new Color(0.05f, 0.08f, 0.12f, 0.86f);
		float lineHeight = font.GetHeight(fontSize);
		string[] lines = text.Split('\n');

		float width = 0.0f;
		foreach (string line in lines)
		{
			width = Mathf.Max(width, font.GetStringSize(line, HorizontalAlignment.Left, -1.0f, fontSize).X);
		}

		float height = Mathf.Max(lineHeight * lines.Length, fontSize);
		Rect2 backgroundRect = new Rect2(
			position - new Vector2(padding, lineHeight),
			new Vector2(width + padding * 2.0f, height + padding * 2.0f));
		canvas.DrawRect(backgroundRect, resolvedBackgroundColor, true);
		canvas.DrawRect(backgroundRect, resolvedTextColor.Darkened(0.55f), false, 1.0f);

		for (int i = 0; i < lines.Length; i++)
		{
			Vector2 baseline = position + new Vector2(0.0f, i * lineHeight);
			canvas.DrawString(font, baseline, lines[i], HorizontalAlignment.Left, -1.0f, fontSize, resolvedTextColor);
		}
	}
}
