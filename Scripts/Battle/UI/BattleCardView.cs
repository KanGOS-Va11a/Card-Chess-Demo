using System;
using System.Collections.Generic;
using Godot;
using CardChessDemo.Battle.Cards;

namespace CardChessDemo.Battle.UI;

public partial class BattleCardView : Button
{
	private static readonly Dictionary<string, Texture2D> BadgeTextures = new(StringComparer.Ordinal);
	private static readonly Dictionary<string, Texture2D> ArtTextures = new(StringComparer.Ordinal);
	private static readonly IReadOnlyDictionary<string, string> CardArtNameOverrides = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["card_punch"] = "\u62F3\u51FB",
		["card_guard"] = "\u62A4\u8EAB",
		["card_calm"] = "\u51B7\u9759",
		["card_boot"] = "\u542F\u52A8",
		["draw_revolver"] = "\u62D4\u67AA",
		["card_stance"] = "\u67B6\u52BF",
		["card_heavy_blow"] = "\u91CD\u51FB",
		["card_pressure_breach"] = "\u9AD8\u538B\u7A81\u8FDB",
		["card_weathering"] = "\u98CE\u5316",
		["card_ram"] = "\u51B2\u649E",
		["card_arc_leak"] = "\u7535\u5F27\u6CC4\u9732",
		["card_roll"] = "\u7FFB\u6EDA",
		["card_charge_up"] = "\u84C4\u52BF",
		["card_learning"] = "\u5B66\u4E60",
		["card_tactical_shift"] = "\u6218\u7565\u8F6C\u79FB",
		["card_quick_shot"] = "\u5FEB\u67AA",
		["card_alert"] = "\u6212\u5907",
		["card_alert_guard"] = "\u8B66\u60D5",
		["card_plunder"] = "\u6380\u593A",
		["card_contemplate"] = "\u6C89\u601D",
		["card_repair"] = "\u4FEE\u590D",
		["card_structural_boost"] = "\u7ED3\u6784\u6027\u8865\u5F3A",
		["card_concussion_shot"] = "\u9707\u8361\u5C04\u51FB",
		["card_snipe"] = "\u72D9\u51FB",
		["card_aim"] = "\u7784\u51C6",
		["card_mark"] = "\u70B9\u540D",
		["card_vault"] = "\u5B9D\u5E93",
		["card_momentum_slice"] = "\u987A\u52BF\u5207\u5F00",
		["card_salvage_focus"] = "\u56DE\u6536\u4E13\u6CE8",
		["card_overclock_beam"] = "\u8FC7\u8F7D\u5149\u675F",
	};
	private static readonly string[] ArtSearchPaths =
	{
		"res://Assets/Cards/card_arts/{0}.png",
		"res://Assets/Cards/{0}.png",
		"res://Assets/Battle/Cards/{0}.png",
	};

	private TextureRect _costIcon = null!;
	private Label _costLabel = null!;
	private Label _nameLabel = null!;
	private BattleCardArtView _artTexture = null!;
	private ColorRect _artShade = null!;
	private Label _artSymbolLabel = null!;
	private Label _keywordLabel = null!;
	private Label _descriptionLabel = null!;
	private PanelContainer _cardPanel = null!;
	private PanelContainer _artFrame = null!;
	private PanelContainer _titleBanner = null!;
	private PanelContainer _typePlate = null!;
	private PanelContainer _descriptionPanel = null!;
	private BattleCardAffixOverlay _affixOverlay = null!;

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
		_keywordLabel.Text = BuildTypeText(card.Definition);
		_descriptionLabel.Text = card.Definition.Description;
		_artSymbolLabel.Text = string.Empty;
		_costIcon.Texture = GetBadgeTexture(card.Definition);
		_artTexture.Texture = GetArtTexture(card.Definition);
		_descriptionPanel.Visible = false;

		ApplyCardFrameStyle(card, isSelected, isPlayable);
		_affixOverlay.SetAffixes(card.Definition.IsQuick, card.Definition.ExhaustsOnPlay, isSelected, isPlayable);
		Modulate = isPlayable ? Colors.White : new Color(1.0f, 1.0f, 1.0f, 0.55f);
		Scale = Vector2.One;
	}

	private void CacheNodes()
	{
		if (_costIcon != null)
		{
			return;
		}

		_cardPanel = GetNode<PanelContainer>("CardPanel");
		_costIcon = GetNode<TextureRect>("CardPanel/Margin/Root/Head/CostBox/CostIcon");
		_costLabel = GetNode<Label>("CardPanel/Margin/Root/Head/CostBox/CostLabel");
		_nameLabel = GetNode<Label>("CardPanel/Margin/Root/Head/TitleBanner/NameLabel");
		_artFrame = GetNode<PanelContainer>("CardPanel/Margin/Root/ArtFrame");
		_artTexture = GetNode<BattleCardArtView>("CardPanel/Margin/Root/ArtFrame/ArtTexture");
		_artTexture.CustomMinimumSize = Vector2.Zero;
		_artShade = GetNode<ColorRect>("CardPanel/Margin/Root/ArtFrame/ArtShade");
		_artSymbolLabel = GetNode<Label>("CardPanel/Margin/Root/ArtFrame/ArtSymbol");
		_titleBanner = GetNode<PanelContainer>("CardPanel/Margin/Root/Head/TitleBanner");
		_typePlate = GetNode<PanelContainer>("CardPanel/Margin/Root/TypePlate");
		_keywordLabel = GetNode<Label>("CardPanel/Margin/Root/TypePlate/KeywordLabel");
		_descriptionPanel = GetNode<PanelContainer>("CardPanel/Margin/Root/DescriptionPanel");
		_descriptionLabel = GetNode<Label>("CardPanel/Margin/Root/DescriptionPanel/DescriptionLabel");
		_affixOverlay = GetNode<BattleCardAffixOverlay>("AffixOverlay");
	}

	public void PlayEnhancementPulse()
	{
		Color originalShade = _artShade.Color;
		Tween tween = CreateTween();
		tween.SetParallel();
		tween.SetEase(Tween.EaseType.Out);
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.TweenProperty(this, "scale", new Vector2(1.10f, 1.10f), 0.10f);
		tween.TweenProperty(_artShade, "color", new Color(0.24f, 0.72f, 1.0f, 0.42f), 0.08f);
		tween.TweenProperty(this, "scale", Vector2.One, 0.16f).SetDelay(0.10f);
		tween.TweenProperty(_artShade, "color", originalShade, 0.18f).SetDelay(0.08f);
	}

	private void ApplyCardFrameStyle(BattleCardInstance card, bool isSelected, bool isPlayable)
	{
		BattleCardDefinition definition = card.Definition;
		Color frameColor = definition.Category == BattleCardCategory.Attack
			? new Color(0.68f, 0.30f, 0.22f)
			: new Color(0.26f, 0.44f, 0.64f);
		Color fillColor = definition.Category == BattleCardCategory.Attack
			? new Color(0.27f, 0.16f, 0.13f)
			: new Color(0.13f, 0.18f, 0.25f);
		Color bannerColor = new Color(0.81f, 0.82f, 0.84f);
		Color bannerBorderColor = new Color(0.58f, 0.58f, 0.60f);
		Color nameColor = new Color(0.10f, 0.11f, 0.13f);
		Color descColor = new Color(0.23f, 0.20f, 0.19f);
		Color typeColor = definition.Category == BattleCardCategory.Attack
			? new Color(0.72f, 0.72f, 0.76f)
			: new Color(0.64f, 0.77f, 0.90f);

		if (definition.IsQuick)
		{
			bannerColor = new Color(0.23f, 0.50f, 0.82f);
			bannerBorderColor = new Color(0.70f, 0.90f, 1.0f);
			nameColor = new Color(0.96f, 0.99f, 1.0f);
		}

		if (isSelected)
		{
			frameColor = new Color(0.95f, 0.83f, 0.42f);
		}

		if (card.IsEnhanced)
		{
			frameColor = new Color(0.30f, 0.84f, 1.0f);
			fillColor = fillColor.Lerp(new Color(0.12f, 0.22f, 0.36f), 0.40f);
			typeColor = typeColor.Lerp(new Color(0.46f, 0.88f, 1.0f), 0.70f);
			if (definition.IsQuick)
			{
				bannerColor = bannerColor.Lerp(new Color(0.34f, 0.74f, 1.0f), 0.35f);
				bannerBorderColor = bannerBorderColor.Lerp(new Color(0.90f, 0.98f, 1.0f), 0.35f);
			}
		}

		StyleBoxFlat panelStyle = new()
		{
			BgColor = fillColor,
			BorderColor = frameColor,
			BorderWidthLeft = isSelected ? 3 : 2,
			BorderWidthTop = isSelected ? 3 : 2,
			BorderWidthRight = isSelected ? 3 : 2,
			BorderWidthBottom = isSelected ? 3 : 2,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomRight = 5,
			CornerRadiusBottomLeft = 5,
			ShadowSize = isPlayable ? 2 : 0,
			ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.30f),
		};
		_cardPanel.AddThemeStyleboxOverride("panel", panelStyle);

		StyleBoxFlat artStyle = new()
		{
			BgColor = new Color(0.08f, 0.08f, 0.10f),
			BorderColor = frameColor.Lightened(0.12f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 3,
			CornerRadiusTopRight = 3,
			CornerRadiusBottomRight = 3,
			CornerRadiusBottomLeft = 3,
		};
		_artFrame.AddThemeStyleboxOverride("panel", artStyle);
		_artShade.Color = card.IsEnhanced
			? new Color(0.18f, 0.52f, 0.92f, 0.22f)
			: new Color(0.0f, 0.0f, 0.0f, 0.12f);

		StyleBoxFlat titleStyle = new()
		{
			BgColor = bannerColor,
			BorderColor = bannerBorderColor,
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 8,
			CornerRadiusTopRight = 8,
			CornerRadiusBottomRight = 2,
			CornerRadiusBottomLeft = 2,
		};
		_titleBanner.AddThemeStyleboxOverride("panel", titleStyle);
		_nameLabel.AddThemeColorOverride("font_color", nameColor);
		_nameLabel.AddThemeColorOverride("font_outline_color", definition.IsQuick
			? new Color(0.07f, 0.13f, 0.22f)
			: new Color(0.82f, 0.84f, 0.88f, 0.0f));
		_nameLabel.AddThemeConstantOverride("outline_size", definition.IsQuick ? 1 : 0);

		StyleBoxFlat typeStyle = new()
		{
			BgColor = typeColor,
			BorderColor = frameColor.Darkened(0.1f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 3,
			CornerRadiusTopRight = 3,
			CornerRadiusBottomRight = 3,
			CornerRadiusBottomLeft = 3,
		};
		_typePlate.AddThemeStyleboxOverride("panel", typeStyle);

		StyleBoxFlat descriptionStyle = new()
		{
			BgColor = descColor,
			BorderColor = frameColor.Darkened(0.18f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 2,
			CornerRadiusTopRight = 2,
			CornerRadiusBottomRight = 4,
			CornerRadiusBottomLeft = 4,
		};
		_descriptionPanel.AddThemeStyleboxOverride("panel", descriptionStyle);
	}

	private static string BuildTypeText(BattleCardDefinition definition)
	{
		return definition.Category == BattleCardCategory.Attack ? "\u653B\u51FB" : "\u6280\u80FD";
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
		foreach (string candidateName in EnumerateArtCandidateNames(definition))
		{
			foreach (string template in ArtSearchPaths)
			{
				string artPath = string.Format(template, candidateName);
				if (!ResourceLoader.Exists(artPath))
				{
					continue;
				}

				Texture2D? loadedTexture = GD.Load<Texture2D>(artPath);
				if (loadedTexture != null)
				{
					return loadedTexture;
				}
			}
		}

		string cacheKey = $"{definition.CardId}:{definition.Category}:{definition.TargetingMode}:{definition.DrawCount}:{definition.EnergyGain}:{definition.HealingAmount}:{definition.ExhaustsOnPlay}";
		if (ArtTextures.TryGetValue(cacheKey, out Texture2D? cached))
		{
			return cached;
		}

		Color colorA = definition.Category == BattleCardCategory.Attack
			? new Color(0.64f, 0.20f, 0.18f)
			: new Color(0.16f, 0.36f, 0.57f);
		Color colorB = definition.TargetingMode == BattleCardTargetingMode.StraightLineEnemy
			? new Color(0.92f, 0.76f, 0.27f)
			: definition.HealingAmount > 0
				? new Color(0.36f, 0.88f, 0.56f)
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

	private static IEnumerable<string> EnumerateArtCandidateNames(BattleCardDefinition definition)
	{
		HashSet<string> names = new(StringComparer.Ordinal);
		if (!string.IsNullOrWhiteSpace(definition.CardId))
		{
			names.Add(definition.CardId.Trim());
		}

		if (CardArtNameOverrides.TryGetValue(definition.CardId, out string overrideName)
			&& !string.IsNullOrWhiteSpace(overrideName))
		{
			names.Add(overrideName.Trim());
		}

		if (!string.IsNullOrWhiteSpace(definition.DisplayName))
		{
			names.Add(definition.DisplayName.Trim());
		}

		return names;
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
