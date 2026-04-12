using Godot;

namespace CardChessDemo.Battle.UI;

public partial class BattleCardArtView : Control
{
	private Texture2D? _artTexture;

	public Texture2D? Texture
	{
		get => _artTexture;
		set
		{
			_artTexture = value;
			QueueRedraw();
		}
	}

	public override void _Ready()
	{
		MouseFilter = MouseFilterEnum.Ignore;
		TextureFilter = TextureFilterEnum.Nearest;
		Resized += OnResized;
	}

	public override void _ExitTree()
	{
		if (!IsNodeReady())
		{
			return;
		}

		Resized -= OnResized;
	}

	public override void _Draw()
	{
		if (_artTexture == null)
		{
			return;
		}

		Vector2 viewportSize = Size;
		Vector2 textureSize = _artTexture.GetSize();
		if (viewportSize.X <= 0.0f || viewportSize.Y <= 0.0f || textureSize.X <= 0.0f || textureSize.Y <= 0.0f)
		{
			return;
		}

		float scale = viewportSize.X / textureSize.X;
		float scaledHeight = textureSize.Y * scale;

		if (scaledHeight <= viewportSize.Y)
		{
			float offsetY = Mathf.Round((viewportSize.Y - scaledHeight) * 0.5f);
			DrawTextureRectRegion(
				_artTexture,
				new Rect2(0.0f, offsetY, viewportSize.X, scaledHeight),
				new Rect2(Vector2.Zero, textureSize));
			return;
		}

		float visibleSourceHeight = viewportSize.Y / scale;
		float sourceY = Mathf.Round((textureSize.Y - visibleSourceHeight) * 0.5f);
		DrawTextureRectRegion(
			_artTexture,
			new Rect2(Vector2.Zero, viewportSize),
			new Rect2(0.0f, sourceY, textureSize.X, visibleSourceHeight));
	}

	private void OnResized()
	{
		QueueRedraw();
	}
}
