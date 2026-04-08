using Godot;

namespace CardChessDemo.Map;

public partial class SystemFeatureOverlayMount : Node
{
	[Export(PropertyHint.File, "*.tscn")] public string SourceScenePath = "res://Scene/UI/SystemFeatureOverlay.tscn";
	[Export] public NodePath PlayerPath = new("../MainPlayer/Player");
	[Export] public bool SkipIfOverlayAlreadyExists = true;
	[Export] public string OverlayNodeName = "SystemFeatureOverlay";

	public override async void _Ready()
	{
		Node sceneRoot = GetTree().CurrentScene ?? GetParent() ?? this;
		if (SkipIfOverlayAlreadyExists && sceneRoot.GetNodeOrNull(OverlayNodeName) != null)
		{
			return;
		}

		PackedScene? sourceScene = ResourceLoader.Load<PackedScene>(SourceScenePath);
		if (sourceScene == null)
		{
			GD.PushWarning($"SystemFeatureOverlayMount: failed to load source scene '{SourceScenePath}'.");
			return;
		}

		CanvasLayer? overlay = sourceScene.Instantiate<CanvasLayer>();
		if (overlay == null)
		{
			GD.PushWarning($"SystemFeatureOverlayMount: failed to instantiate overlay scene '{SourceScenePath}'.");
			return;
		}

		if (overlay is SystemFeatureLabController controller)
		{
			controller.PlayerPath = PlayerPath;
		}

		// Wait one frame so the destination scene finishes building its initial child tree.
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		if (!GodotObject.IsInstanceValid(sceneRoot))
		{
			overlay.QueueFree();
			return;
		}

		sceneRoot.AddChild(overlay);
		overlay.Owner = sceneRoot;
	}
}


