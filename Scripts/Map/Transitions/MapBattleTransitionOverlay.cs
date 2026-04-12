using Godot;
using System.Threading.Tasks;

namespace CardChessDemo.Map;

public partial class MapBattleTransitionOverlay : CanvasLayer
{
	[Export] public NodePath FullscreenRectPath { get; set; } = new("FullscreenRect");
	[Export(PropertyHint.Range, "0.05,2.0,0.01")] public float TransitionDurationSeconds { get; set; } = 0.45f;
	[Export(PropertyHint.Range, "0.05,2.0,0.01")] public float RevealDurationSeconds { get; set; } = 0.30f;
	[Export(PropertyHint.Range, "0.0,0.5,0.01")] public float EndHoldSeconds { get; set; } = 0.04f;
	[Export] public string ProgressShaderParameter { get; set; } = "progress";

	private ColorRect? _fullscreenRect;
	private ShaderMaterial? _shaderMaterial;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Layer = 200;
		_fullscreenRect = GetNodeOrNull<ColorRect>(FullscreenRectPath);
		_shaderMaterial = _fullscreenRect?.Material as ShaderMaterial;
		if (_fullscreenRect != null)
		{
			_fullscreenRect.MouseFilter = Control.MouseFilterEnum.Stop;
		}

		SetProgress(0.0f);
	}

	public async Task PlayAsync()
	{
		if (_shaderMaterial == null)
		{
			return;
		}

		await PlayDissolveOutAsync();

		if (EndHoldSeconds > 0.0f)
		{
			await ToSignal(GetTree().CreateTimer(EndHoldSeconds, false, false, true), SceneTreeTimer.SignalName.Timeout);
		}
	}

	public async Task PlayDissolveOutAsync()
	{
		if (_shaderMaterial == null)
		{
			return;
		}

		SetProgress(0.0f);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await TweenProgressAsync(0.0f, 1.0f, Mathf.Max(0.01f, TransitionDurationSeconds), Tween.EaseType.In);
	}

	public async Task PlayRestoreAsync()
	{
		if (_shaderMaterial == null)
		{
			return;
		}

		SetProgress(1.0f);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await TweenProgressAsync(1.0f, 0.0f, Mathf.Max(0.01f, RevealDurationSeconds), Tween.EaseType.Out);
	}

	private async Task TweenProgressAsync(float from, float to, float duration, Tween.EaseType ease)
	{
		Tween tween = CreateTween();
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.SetEase(ease);
		tween.TweenMethod(Callable.From<float>(SetProgress), from, to, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private void SetProgress(float value)
	{
		_shaderMaterial?.SetShaderParameter(ProgressShaderParameter, value);
	}
}
