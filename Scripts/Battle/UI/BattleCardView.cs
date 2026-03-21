using System;
using System.Collections.Generic;
using Godot;
using CardChessDemo.Battle.Cards;

namespace CardChessDemo.Battle.UI;

public partial class BattleCardView : Button
{
    private static readonly Dictionary<string, Texture2D> BadgeTextures = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Texture2D> ArtTextures = new(StringComparer.Ordinal);
    private static readonly string[] ArtSearchPaths =
    {
        "res://Assets/Cards/{0}.png",
        "res://Assets/Battle/Cards/{0}.png",
    };

    private TextureRect _costIcon = null!;
    private Label _costLabel = null!;
    private Label _nameLabel = null!;
    private TextureRect _artTexture = null!;
    private Label _artSymbolLabel = null!;
    private Label _keywordLabel = null!;
    private PanelContainer _artFrame = null!;

    public string CardInstanceId { get; private set; } = string.Empty;

    public override void _Ready()
    {
        CacheNodes();
        Text = string.Empty;
        Flat = true;
        FocusMode = FocusModeEnum.None;
        TooltipText = string.Empty;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        Resized += OnResized;
        OnResized();
    }

    public override void _ExitTree()
    {
        if (!IsNodeReady())
        {
            return;
        }

        MouseEntered -= OnMouseEntered;
        MouseExited -= OnMouseExited;
        Resized -= OnResized;
    }

    public void Bind(BattleCardInstance card, bool isSelected, bool isPlayable)
    {
        CacheNodes();
        CardInstanceId = card.InstanceId;
        Disabled = !isPlayable;

        _costLabel.Text = card.Definition.Cost.ToString();
        _nameLabel.Text = card.Definition.DisplayName;
        _keywordLabel.Text = BuildKeywordText(card.Definition);
        _artSymbolLabel.Text = BuildArtSymbol(card.Definition);
        _costIcon.Texture = GetBadgeTexture(card.Definition);
        _artTexture.Texture = GetArtTexture(card.Definition);

        ApplyCardFrameStyle(card.Definition, isSelected, isPlayable);
        Modulate = isPlayable ? Colors.White : new Color(1.0f, 1.0f, 1.0f, 0.55f);
        Scale = Vector2.One;
    }

    private void CacheNodes()
    {
        if (_costIcon != null)
        {
            return;
        }

        _costIcon = GetNode<TextureRect>("Margin/Root/Head/CostBox/CostIcon");
        _costLabel = GetNode<Label>("Margin/Root/Head/CostBox/CostLabel");
        _nameLabel = GetNode<Label>("Margin/Root/Head/NameLabel");
        _artFrame = GetNode<PanelContainer>("Margin/Root/ArtFrame");
        _artTexture = GetNode<TextureRect>("Margin/Root/ArtFrame/ArtTexture");
        _artSymbolLabel = GetNode<Label>("Margin/Root/ArtFrame/ArtSymbol");
        _keywordLabel = GetNode<Label>("Margin/Root/Foot/KeywordLabel");
    }

    private void ApplyCardFrameStyle(BattleCardDefinition definition, bool isSelected, bool isPlayable)
    {
        Color frameColor = definition.Category == BattleCardCategory.Attack
            ? new Color(0.72f, 0.24f, 0.16f)
            : new Color(0.18f, 0.44f, 0.62f);
        Color fillColor = definition.Category == BattleCardCategory.Attack
            ? new Color(0.20f, 0.11f, 0.10f)
            : new Color(0.09f, 0.15f, 0.20f);

        if (isSelected)
        {
            frameColor = new Color(0.95f, 0.83f, 0.42f);
        }

        StyleBoxFlat normalStyle = new()
        {
            BgColor = fillColor,
            BorderColor = frameColor,
            BorderWidthLeft = isSelected ? 3 : 2,
            BorderWidthTop = isSelected ? 3 : 2,
            BorderWidthRight = isSelected ? 3 : 2,
            BorderWidthBottom = isSelected ? 3 : 2,
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomRight = 6,
            CornerRadiusBottomLeft = 6,
            ShadowSize = isPlayable ? 2 : 0,
            ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.30f),
        };

        AddThemeStyleboxOverride("normal", normalStyle);
        AddThemeStyleboxOverride("hover", normalStyle.Duplicate() as StyleBoxFlat ?? normalStyle);
        AddThemeStyleboxOverride("pressed", normalStyle.Duplicate() as StyleBoxFlat ?? normalStyle);
        AddThemeStyleboxOverride("disabled", normalStyle.Duplicate() as StyleBoxFlat ?? normalStyle);

        StyleBoxFlat artStyle = new()
        {
            BgColor = new Color(0.06f, 0.07f, 0.09f),
            BorderColor = frameColor.Lightened(0.15f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomRight = 6,
            CornerRadiusBottomLeft = 6,
        };
        _artFrame.AddThemeStyleboxOverride("panel", artStyle);
    }

    private static string BuildKeywordText(BattleCardDefinition definition)
    {
        List<string> tags = new();
        if (definition.IsQuick)
        {
            tags.Add("迅");
        }

        if (definition.ExhaustsOnPlay)
        {
            tags.Add("消");
        }

        return string.Join(" ", tags);
    }

    private static string BuildArtSymbol(BattleCardDefinition definition)
    {
        return definition.Category switch
        {
            BattleCardCategory.Attack => definition.TargetingMode == BattleCardTargetingMode.StraightLineEnemy ? "射" : "斩",
            _ => definition.EnergyGain > 0 ? "能" : definition.DrawCount > 0 ? "抽" : "技",
        };
    }

    private static Texture2D GetBadgeTexture(BattleCardDefinition definition)
    {
        string cacheKey = definition.Category == BattleCardCategory.Attack ? "attack" : "skill";
        if (BadgeTextures.TryGetValue(cacheKey, out Texture2D? cached))
        {
            return cached;
        }

        Color fill = definition.Category == BattleCardCategory.Attack
            ? new Color(0.91f, 0.35f, 0.24f)
            : new Color(0.25f, 0.63f, 0.86f);
        Color ring = new Color(1.0f, 0.95f, 0.78f);

        Image image = Image.CreateEmpty(48, 48, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);
        Vector2 center = new(24.0f, 24.0f);
        for (int y = 0; y < 48; y++)
        {
            for (int x = 0; x < 48; x++)
            {
                float distance = center.DistanceTo(new Vector2(x + 0.5f, y + 0.5f));
                if (distance <= 22.0f)
                {
                    image.SetPixel(x, y, fill);
                }

                if (distance <= 22.0f && distance >= 18.5f)
                {
                    image.SetPixel(x, y, ring);
                }
            }
        }

        ImageTexture texture = ImageTexture.CreateFromImage(image);
        BadgeTextures[cacheKey] = texture;
        return texture;
    }

    private static Texture2D GetArtTexture(BattleCardDefinition definition)
    {
        foreach (string template in ArtSearchPaths)
        {
            string artPath = string.Format(template, definition.CardId);
            if (ResourceLoader.Exists(artPath))
            {
                Texture2D? loadedTexture = GD.Load<Texture2D>(artPath);
                if (loadedTexture != null)
                {
                    return loadedTexture;
                }
            }
        }

        string cacheKey = $"{definition.CardId}:{definition.Category}:{definition.TargetingMode}:{definition.DrawCount}:{definition.EnergyGain}:{definition.ExhaustsOnPlay}";
        if (ArtTextures.TryGetValue(cacheKey, out Texture2D? cached))
        {
            return cached;
        }

        Color colorA = definition.Category == BattleCardCategory.Attack
            ? new Color(0.64f, 0.20f, 0.18f)
            : new Color(0.16f, 0.36f, 0.57f);
        Color colorB = definition.TargetingMode == BattleCardTargetingMode.StraightLineEnemy
            ? new Color(0.92f, 0.76f, 0.27f)
            : definition.DrawCount > 0
                ? new Color(0.36f, 0.84f, 0.71f)
                : definition.EnergyGain > 0
                    ? new Color(0.47f, 0.87f, 0.98f)
                    : new Color(0.84f, 0.54f, 0.30f);

        Image image = Image.CreateEmpty(88, 48, false, Image.Format.Rgba8);
        for (int y = 0; y < image.GetHeight(); y++)
        {
            float t = image.GetHeight() <= 1 ? 0.0f : (float)y / (image.GetHeight() - 1);
            Color rowColor = colorA.Lerp(colorB, t);
            for (int x = 0; x < image.GetWidth(); x++)
            {
                float stripe = Mathf.Abs(Mathf.Sin((x + y * 0.6f) * 0.18f));
                Color finalColor = rowColor.Lerp(Colors.White, stripe * 0.16f);
                if ((x + y) % 11 < 2)
                {
                    finalColor = finalColor.Darkened(0.10f);
                }

                image.SetPixel(x, y, finalColor);
            }
        }

        ImageTexture texture = ImageTexture.CreateFromImage(image);
        ArtTextures[cacheKey] = texture;
        return texture;
    }

    private void OnMouseEntered()
    {
        if (Disabled)
        {
            return;
        }

        CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic)
            .TweenProperty(this, "scale", new Vector2(1.03f, 1.03f), 0.10f);
    }

    private void OnMouseExited()
    {
        if (Disabled)
        {
            return;
        }

        CreateTween()
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic)
            .TweenProperty(this, "scale", Vector2.One, 0.08f);
    }

    private void OnResized()
    {
        PivotOffset = Size * 0.5f;
    }
}
