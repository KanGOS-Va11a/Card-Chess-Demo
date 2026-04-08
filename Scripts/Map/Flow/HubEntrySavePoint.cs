using Godot;
using System.Text;
using CardChessDemo.Battle.Shared;

namespace CardChessDemo.Map;

public partial class HubEntrySavePoint : InteractableTemplate
{
	[Export] public string SaveFilePath = "user://saves/min_checkpoint.cfg";

	public override string GetInteractText(Player player)
	{
		return IsDisabled ? "存档点不可用" : "记录进度";
	}

	protected override void OnInteract(Player player)
	{
		GlobalGameSession? session = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
		if (session == null)
		{
			GD.PushWarning("HubEntrySavePoint: 未找到 /root/GlobalGameSession，无法保存。");
			return;
		}

		string saveDir = SaveFilePath.GetBaseDir();
		if (!string.IsNullOrWhiteSpace(saveDir))
		{
			DirAccess.MakeDirRecursiveAbsolute(saveDir);
		}

		ConfigFile cfg = new();
		cfg.SetValue("meta", "saved_at", Time.GetDatetimeStringFromSystem());
		cfg.SetValue("meta", "save_point", Name);

		cfg.SetValue("session", "session_id", session.SessionId);
		cfg.SetValue("session", "current_map_id", session.CurrentMapId);
		cfg.SetValue("session", "current_map_spawn_id", session.CurrentMapSpawnId);
		cfg.SetValue("session", "scan_risk", session.ScanRisk);

		cfg.SetValue("player", "global_position_x", player.GlobalPosition.X);
		cfg.SetValue("player", "global_position_y", player.GlobalPosition.Y);

		string activeNode = session.TryGetFlag(new StringName("flow.active_node"), out Variant activeVariant)
			? activeVariant.AsString()
			: string.Empty;
		cfg.SetValue("flow", "active_node", activeNode);
		cfg.SetValue("flow", "completed_nodes", GetCompletedNodesCsv(session));

		Error saveResult = cfg.Save(SaveFilePath);
		if (saveResult != Error.Ok)
		{
			GD.PushError($"HubEntrySavePoint: 保存失败 error={saveResult}, path={SaveFilePath}");
			return;
		}

		GD.Print($"存档点：进度已记录 -> {SaveFilePath}");
		PlayInteractionPulse();
	}

	private static string GetCompletedNodesCsv(GlobalGameSession session)
	{
		StringBuilder sb = new();
		bool first = true;

		foreach (StringName key in session.WorldFlags.Keys)
		{
			string keyText = key.ToString();
			if (!keyText.StartsWith("flow.completed.", System.StringComparison.Ordinal))
			{
				continue;
			}

			Variant value = session.WorldFlags[key];
			if (!value.AsBool())
			{
				continue;
			}

			string nodeId = keyText.Substring("flow.completed.".Length);
			if (string.IsNullOrWhiteSpace(nodeId))
			{
				continue;
			}

			if (!first)
			{
				sb.Append(',');
			}

			sb.Append(nodeId);
			first = false;
		}

		return sb.ToString();
	}
}
