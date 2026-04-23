using System;
using System.Collections.Generic;
using System.Linq;
using CardChessDemo.UI.Dialogue;
using Godot;

namespace CardChessDemo.Map;

public partial class Npc : InteractableTemplate
{
	[Export] public string NpcName = "\u6751\u6C11";
	[Export] public string DialogueText = "\u4F60\u597D\uff0c\u65C5\u884C\u8005\u3002";
	[Export] public PackedScene? DialoguePanelScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/DialogueSequencePanel.tscn");
	[Export] public Godot.Collections.Array<DialoguePage> DialoguePages { get; set; } = new();
	[Export] public string[] DialogueLineEntries { get; set; } = Array.Empty<string>();
	[Export] public string DialogueScriptId { get; set; } = string.Empty;

	private bool _showingDialogue;

	private static readonly Dictionary<string, string[]> DialogueScripts = new(StringComparer.Ordinal)
	{
		["scene05_back_alley_hint_a"] = new[]
		{
			"\u6E2F\u533A\u5E73\u6C11|\u6B63\u9762\u8857\u533A\u5DF2\u7ECF\u4E0D\u5B89\u5168\u4E86\uff0c\u522B\u5728\u4EBA\u7FA4\u91CC\u77B1\u8F6C\u3002",
			"\u6E2F\u533A\u5E73\u6C11|\u4F60\u8981\u662F\u8FD8\u60F3\u5F80\u4E0B\u67E5\uff0c\u5C31\u53BB\u540E\u5DF7\u627E\u90A3\u4E2A\u88AB\u5E06\u5E03\u906E\u4F4F\u7684\u795E\u79D8\u5165\u53E3\u3002",
			"\u6E2F\u533A\u5E73\u6C11|\u642D\u8239\u5BA2\u7684\u533A\u57DF\u603B\u90E8\uff0C\u5C31\u85CF\u5728\u90A3\u91CC\u9762\u3002",
		},
		["scene05_back_alley_hint_b"] = new[]
		{
			"\u7EF4\u4FEE\u673A\u5668|\u540E\u5DF7\u6700\u8FD1\u603B\u6709\u4EBA\u88AB\u8C03\u8D70\uff0C\u4F46\u6CA1\u4EBA\u6562\u8FFD\u8FC7\u53BB\u3002",
			"\u7EF4\u4FEE\u673A\u5668|\u987A\u7740\u6697\u706F\u548C\u5C01\u6B7B\u7684\u8D27\u7BB1\u5F80\u91CC\u627E\uff0C\u4F60\u4F1A\u770B\u5230\u4E00\u4E2A\u795E\u79D8\u533A\u57DF\u5165\u53E3\u3002",
			"\u7EF4\u4FEE\u673A\u5668|\u8FDB\u4E86\u90A3\u9053\u5165\u53E3\uff0C\u5C31\u662F\u642D\u8239\u5BA2\u63A7\u5236\u7684\u533A\u57DF\u603B\u90E8\u3002",
		},
	};

	public override bool CanInteract(Player player)
	{
		return !_showingDialogue && base.CanInteract(player);
	}

	protected override void OnInteract(Player player)
	{
		if (_showingDialogue)
		{
			return;
		}

		DialoguePage[] pages = BuildDialoguePages();
		if (pages.Length == 0 || DialoguePanelScene?.Instantiate() is not DialogueSequencePanel panel)
		{
			SceneTextOverlay.Show(this, DialogueText);
			PlayInteractionPulse();
			return;
		}

		Node currentScene = GetTree().CurrentScene ?? this;
		currentScene.AddChild(panel);
		_showingDialogue = true;
		SetPlayerInputEnabled(player, false);
		panel.Present(
			pages,
			onCompleted: () => FinishDialogue(player),
			onClosed: () => FinishDialogue(player));
		PlayInteractionPulse();
	}

	private void FinishDialogue(Player player)
	{
		_showingDialogue = false;
		SetPlayerInputEnabled(player, true);
	}

	private DialoguePage[] BuildDialoguePages()
	{
		if (DialoguePages.Count > 0)
		{
			return DialoguePages.ToArray();
		}

		if (!string.IsNullOrWhiteSpace(DialogueScriptId)
			&& DialogueScripts.TryGetValue(DialogueScriptId, out string[]? scriptedLines))
		{
			return scriptedLines
				.Select(ParseDialogueLineEntry)
				.ToArray();
		}

		if (DialogueLineEntries != null && DialogueLineEntries.Length > 0)
		{
			return DialogueLineEntries
				.Where(line => !string.IsNullOrWhiteSpace(line))
				.Select(ParseDialogueLineEntry)
				.ToArray();
		}

		if (string.IsNullOrWhiteSpace(DialogueText))
		{
			return Array.Empty<DialoguePage>();
		}

		return new[]
		{
			new DialoguePage
			{
				Speaker = string.IsNullOrWhiteSpace(NpcName) ? "\u65C1\u767D" : NpcName,
				Content = DialogueText,
			},
		};
	}

	private static DialoguePage ParseDialogueLineEntry(string entry)
	{
		string[] parts = entry.Split('|', 2, StringSplitOptions.None);
		if (parts.Length == 2)
		{
			return new DialoguePage
			{
				Speaker = parts[0].Trim(),
				Content = parts[1].Trim(),
			};
		}

		return new DialoguePage
		{
			Speaker = "\u65C1\u767D",
			Content = entry.Trim(),
		};
	}

	private static void SetPlayerInputEnabled(Player player, bool enabled)
	{
		if (player == null)
		{
			return;
		}

		player.SetPhysicsProcess(enabled);
		player.SetProcessUnhandledInput(enabled);
	}
}
