using System;
using System.Linq;
using CardChessDemo.Battle.Shared;
using CardChessDemo.UI.Dialogue;
using Godot;

namespace CardChessDemo.Map;

public partial class Scene04To05CutsceneController : Node
{
	[Export] public PackedScene? DialoguePanelScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/DialogueSequencePanel.tscn");
	[Export(PropertyHint.File, "*.tscn")] public string NextScenePath { get; set; } = "res://Scene/Maps/Scene05.tscn";
	[Export] public string NextSpawnId { get; set; } = string.Empty;
	[Export] public string[] DialogueLines { get; set; } =
	{
		"\u65C1\u767D|\u9003\u751F\u8231\u8131\u79BB\u6BCD\u8239\uFF0C\u8231\u4F53\u5267\u70C8\u9707\u98A4\u3002\u65E0\u6570\u5F69\u8272\u50CF\u7D20\u5757\u95EF\u5165\u89C6\u91CE\uFF0C\u51E0\u4E4E\u541E\u6CA1\u6574\u4E2A\u72ED\u7A84\u8231\u5BA4\u3002",
		"\u672A\u77E5\u58F0\u97F3|\u2026\u2026\u54FC\u3002",
		"\u4E3B\u89D2|\u8C01\u5728\u8BF4\u8BDD\uFF1F",
		"\u8352\u5DDD|\u522B\u627E\u4E86\u3002\u4F60\u7684\u4F20\u611F\u5668\u521A\u88AB\u6211\u501F\u7528\u4E86\u4E00\u4E0B\u3002\u53EB\u6211\u8352\u5DDD\u3002",
		"\u4E3B\u89D2|\u2026\u2026\u4F60\u5230\u5E95\u662F\u4EC0\u4E48\u4E1C\u897F\uFF1F",
		"\u8352\u5DDD|\u7B54\u6848\u5F88\u957F\u3002\u4F46\u5BFC\u822A\u7CFB\u7EDF\u5DF2\u7ECF\u5148\u66FF\u6211\u4EEC\u9009\u597D\u4E86\u4E0B\u4E00\u7AD9\u3002",
		"\u8352\u5DDD|\u6700\u8FD1\u7684\u964D\u843D\u70B9\u662F\u4E00\u5EA7\u96B6\u5C5E\u4E8E\u661F\u9645\u8054\u76DF\u7684\u8FB9\u5883\u661F\u6E2F\u3002\u522B\u8BEF\u4F1A\uFF0C\u90A3\u91CC\u4E0D\u4F1A\u662F\u5B89\u5168\u533A\u3002",
		"\u8352\u5DDD|\u5148\u6D3B\u7740\u843D\u4E0B\u53BB\u3002\u5176\u4F59\u7684\u95EE\u9898\uFF0C\u7B49\u4F60\u8FD8\u80FD\u7AD9\u7740\u7684\u65F6\u5019\u518D\u95EE\u6211\u3002",
	};

	private bool _started;

	public override async void _Ready()
	{
		if (_started)
		{
			return;
		}

		_started = true;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		PresentCutscene();
	}

	private async void PresentCutscene()
	{
		DialoguePage[] pages = DialogueLines
			.Where(line => !string.IsNullOrWhiteSpace(line))
			.Select(ParseLine)
			.ToArray();
		if (DialoguePanelScene == null || pages.Length == 0)
		{
			GoToNextScene();
			return;
		}

		MapDialogueResult result = await MapDialogueService.PresentAsync(
			this,
			new MapDialogueRequest
			{
				PanelScene = DialoguePanelScene,
				Pages = pages,
				LockPlayerInput = true,
				RejectIfAnotherDialogueVisible = false,
				SourceId = "scene04_to_05_cutscene",
				CompletedFollowUpActions = new[]
				{
					new MapDialogueFollowUpAction
					{
						Kind = MapDialogueFollowUpKind.ChangeScene,
						NextScenePath = NextScenePath,
						NextSceneSpawnId = NextSpawnId,
					},
				},
				ClosedFollowUpActions = new[]
				{
					new MapDialogueFollowUpAction
					{
						Kind = MapDialogueFollowUpKind.ChangeScene,
						NextScenePath = NextScenePath,
						NextSceneSpawnId = NextSpawnId,
					},
				},
			});

		if (result.IsFailed)
		{
			GoToNextScene();
		}
	}

	private void GoToNextScene()
	{
		if (GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession") is { } session)
		{
			session.ClearPendingBattleReturnContext();
			session.SetPendingSceneTransfer(NextScenePath, NextSpawnId);
		}

		if (!string.IsNullOrWhiteSpace(NextScenePath))
		{
			GetTree().ChangeSceneToFile(NextScenePath);
		}
	}

	private static DialoguePage ParseLine(string line)
	{
		string trimmed = line.Trim();
		int separatorIndex = trimmed.IndexOf('|');
		if (separatorIndex > 0)
		{
			return new DialoguePage
			{
				Speaker = trimmed[..separatorIndex].Trim(),
				Content = trimmed[(separatorIndex + 1)..].Trim(),
			};
		}

		return new DialoguePage
		{
			Speaker = "\u65C1\u767D",
			Content = trimmed,
		};
	}
}
