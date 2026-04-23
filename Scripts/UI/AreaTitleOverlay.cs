using System;
using Godot;

namespace CardChessDemo.UI;

public partial class AreaTitleOverlay : CanvasLayer
{
	private static readonly FontFile OverlayFont = GD.Load<FontFile>("res://Assets/Fonts/unifont_t-17.0.04.otf");

	[Export] public string TitleId { get; set; } = string.Empty;
	[Export] public string TitleText { get; set; } = string.Empty;
	[Export(PropertyHint.Range, "16,64,1")] public int FontSize { get; set; } = 28;
	[Export(PropertyHint.Range, "0.0,3.0,0.05")] public double FadeInSeconds { get; set; } = 0.18d;
	[Export(PropertyHint.Range, "0.0,5.0,0.05")] public double HoldSeconds { get; set; } = 2.3d;
	[Export(PropertyHint.Range, "0.0,3.0,0.05")] public double FadeOutSeconds { get; set; } = 0.55d;

	private Label _titleLabel = null!;

	public override async void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_titleLabel = GetNode<Label>("Root/Title");
		_titleLabel.Text = ResolveTitleText();
		if (string.IsNullOrWhiteSpace(_titleLabel.Text))
		{
			QueueFree();
			return;
		}

		if (OverlayFont != null)
		{
			_titleLabel.AddThemeFontOverride("font", OverlayFont);
		}

		_titleLabel.AddThemeFontSizeOverride("font_size", FontSize);
		_titleLabel.Modulate = new Color(1.0f, 1.0f, 1.0f, 0.0f);

		Tween tween = CreateTween();
		tween.TweenProperty(_titleLabel, "modulate", Colors.White, Math.Max(0.01d, FadeInSeconds));
		tween.TweenInterval(Math.Max(0.0d, HoldSeconds));
		tween.TweenProperty(_titleLabel, "modulate", new Color(1.0f, 1.0f, 1.0f, 0.0f), Math.Max(0.01d, FadeOutSeconds));
		await ToSignal(tween, Tween.SignalName.Finished);
		QueueFree();
	}

	private string ResolveTitleText()
	{
		if (!string.IsNullOrWhiteSpace(TitleText))
		{
			return TitleText;
		}

		return TitleId switch
		{
			"scene05_port" => "\u2014\u2014\u57A3426\u661F\u6E2F\u2014\u2014",
			"scene06_base" => "\u2014\u2014\u642D\u8239\u5BA2\u57FA\u5730\u2014\u2014",
			_ => string.Empty,
		};
	}
}
