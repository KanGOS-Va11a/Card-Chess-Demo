using System;
using CardChessDemo.Battle.Shared;
using CardChessDemo.UI;
using Godot;

namespace CardChessDemo.Map;

public partial class Scene03PostBattleTutorialController : Node
{
	[Export] public PackedScene? PopupScene { get; set; } = GD.Load<PackedScene>("res://Scene/UI/PagedTutorialPopup.tscn");
	[Export] public NodePath PlayerPath { get; set; } = new("MainPlayer/Player");
	[Export] public string EncounterId { get; set; } = "grunt_debug";
	[Export] public string ShownFlagId { get; set; } = "scene03_c_menu_tutorial_shown";
	[Export] public string RestoredFlagId { get; set; } = "scene03_post_battle_recovered";

	private static readonly string[] PopupPages =
	{
		"\u73B0\u5728\u6309 C \u53EF\u4EE5\u6253\u5F00\u7CFB\u7EDF\u83DC\u5355\u3002\u8FD9\u91CC\u80FD\u67E5\u770B\u89D2\u8272\u72B6\u6001\u3001\u80CC\u5305\u3001\u56FE\u9274\u548C\u6784\u7B51\u3002",
		"\u5929\u8D4B\u9875\u9762\u4F1A\u6D88\u8017\u4E13\u7CBE\u70B9\u89E3\u9501\u8282\u70B9\u3002\u8282\u70B9\u4E0D\u53EA\u52A0\u5C5E\u6027\uFF0C\u8FD8\u4F1A\u89E3\u9501\u65B0\u5361\u724C\u3001\u63D0\u9AD8\u8FD1\u6218/\u8FDC\u7A0B/\u7075\u6D3B\u4E13\u7CBE\u3002",
		"\u9AD8\u9636\u5361\u724C\u9700\u8981\u5BF9\u5E94\u7684\u5206\u7CFB\u4E13\u7CBE\u624D\u80FD\u6B63\u5E38\u643A\u5E26\u3002\u4E13\u7CBE\u4E0D\u8DB3\u65F6\uFF0C\u90E8\u5206\u5361\u53EA\u80FD\u8D85\u89C4\u643A\u5E26\uFF0C\u4F1A\u66F4\u8D39\u80FD\u91CF\u4E14\u6548\u679C\u53D8\u5F31\u3002",
		"\u6784\u7B51\u9875\u9762\u53EF\u4EE5\u4ECE\u5361\u6C60\u91CC\u52A0\u5165\u6216\u79FB\u9664\u5361\u724C\u3002\u8981\u6CE8\u610F\u5F71\u54CD\u56E0\u5B50\u3001\u540C\u540D\u4E0A\u9650\u548C\u8D85\u89C4\u69FD\u4F4D\uFF0C\u4E0D\u662F\u6BCF\u5F20\u5F3A\u5361\u90FD\u80FD\u968F\u4FBF\u585E\u8FDB\u724C\u7EC4\u3002",
	};

	public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (session == null || string.IsNullOrWhiteSpace(EncounterId) || string.IsNullOrWhiteSpace(ShownFlagId))
		{
			return;
		}

		if (!session.ClearedEncounters.Contains(new StringName(EncounterId)))
		{
			return;
		}

		RestorePlayerStateIfNeeded(session);

		if (session.TryGetFlag(new StringName(ShownFlagId), out Variant shownVariant) && shownVariant.AsBool())
		{
			return;
		}

		if (PopupScene?.Instantiate() is not PagedTutorialPopup popup)
		{
			return;
		}

		CharacterBody2D? player = GetNodeOrNull<CharacterBody2D>(PlayerPath);
		SetPlayerControlEnabled(player, false);

		(GetTree().CurrentScene ?? this).AddChild(popup);
		popup.Present(
			"\u7CFB\u7EDF\u83DC\u5355\u6559\u7A0B",
			PopupPages,
			onCompleted: () => Finish(session, player),
			onClosed: () => Finish(session, player));
	}

	private void Finish(GlobalGameSession session, CharacterBody2D? player)
	{
		session.SetFlag(new StringName(ShownFlagId), true);
		SetPlayerControlEnabled(player, true);
	}

	private void RestorePlayerStateIfNeeded(GlobalGameSession session)
	{
		if (string.IsNullOrWhiteSpace(RestoredFlagId))
		{
			session.SetPlayerCurrentHp(session.GetResolvedPlayerMaxHp());
			session.SetArakawaCurrentEnergy(session.ArakawaMaxEnergy);
			return;
		}

		StringName restoredFlag = new(RestoredFlagId);
		if (session.TryGetFlag(restoredFlag, out Variant restoredVariant) && restoredVariant.AsBool())
		{
			return;
		}

		session.SetPlayerCurrentHp(session.GetResolvedPlayerMaxHp());
		session.SetArakawaCurrentEnergy(session.ArakawaMaxEnergy);
		session.SetFlag(restoredFlag, true);
	}

	private static void SetPlayerControlEnabled(CharacterBody2D? player, bool enabled)
	{
		if (player == null)
		{
			return;
		}

		player.SetPhysicsProcess(enabled);
		player.SetProcessUnhandledInput(enabled);
	}
}
