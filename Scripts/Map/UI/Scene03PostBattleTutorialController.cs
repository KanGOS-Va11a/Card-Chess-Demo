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
	[Export] public string MenuTutorialsUnlockedFlagId { get; set; } = "scene03_menu_page_tutorials_unlocked";
	[Export] public string RestoredFlagId { get; set; } = "scene03_post_battle_recovered";

	private static readonly string[] PopupPages =
	{
		"\u73B0\u5728\u6309 C \u53EF\u4EE5\u6253\u5F00\u7CFB\u7EDF\u83DC\u5355\u3002\u72B6\u6001\u3001\u80CC\u5305\u3001\u5929\u8D4B\u3001\u56FE\u9274\u548C\u6784\u7B51\u90FD\u5728\u91CC\u9762\u3002",
		"\u63A5\u4E0B\u6765\u4F60\u4F1A\u5728\u8FD9\u4E9B\u9875\u9762\u91CC\u89E3\u9501\u65B0\u80FD\u529B\u3001\u6574\u7406\u724C\u7EC4\uff0C\u6700\u540E\u5B66\u4F1A\u600E\u4E48\u628A\u654C\u4EBA\u7684\u62DB\u5F0F\u53D8\u6210\u81EA\u5DF1\u7684\u724C\u3002\u5148\u6253\u5F00 C \u83DC\u5355\u770B\u770B\u3002",
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
		if (!string.IsNullOrWhiteSpace(MenuTutorialsUnlockedFlagId))
		{
			session.SetFlag(new StringName(MenuTutorialsUnlockedFlagId), true);
		}

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
