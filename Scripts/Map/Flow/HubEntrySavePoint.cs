using Godot;
using System.Text;

public partial class HubEntrySavePoint : InteractableTemplate
{
    [Export] public string SaveFilePath = "user://saves/min_checkpoint.cfg";

    public override string GetInteractText(Player player)
    {
        if (IsDisabled)
        {
            return "存档点不可用";
        }

        return "记录进度";
    }

    protected override void OnInteract(Player player)
    {
        GameSession session = GetNodeOrNull<GameSession>("/root/GameSession");
        if (session == null)
        {
            GD.PushWarning("HubEntrySavePoint: 未找到 /root/GameSession，无法保存。");
            return;
        }

        string saveDir = SaveFilePath.GetBaseDir();
        if (!string.IsNullOrWhiteSpace(saveDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(saveDir);
        }

        ConfigFile cfg = new ConfigFile();
        cfg.SetValue("meta", "saved_at", Time.GetDatetimeStringFromSystem());
        cfg.SetValue("meta", "save_point", Name);

        cfg.SetValue("session", "session_id", session.session_id);
        cfg.SetValue("session", "current_map_id", session.current_map_id.ToString());
        cfg.SetValue("session", "current_map_spawn_id", session.current_map_spawn_id.ToString());
        cfg.SetValue("session", "scan_risk", session.scan_risk);

        cfg.SetValue("player", "global_position_x", player.GlobalPosition.X);
        cfg.SetValue("player", "global_position_y", player.GlobalPosition.Y);

        string activeNode = "";
        if (session.world_flags.TryGetValue(new StringName("flow.active_node"), out Variant activeVariant))
        {
            activeNode = activeVariant.AsString();
        }

        cfg.SetValue("flow", "active_node", activeNode);
        cfg.SetValue("flow", "completed_nodes", GetCompletedNodesCsv(session));

        Error saveResult = cfg.Save(SaveFilePath);
        if (saveResult != Error.Ok)
        {
            GD.PushError($"HubEntrySavePoint: 保存失败 error={saveResult}, path={SaveFilePath}");
            return;
        }

        GD.Print($"存档点：进度已记录 -> {SaveFilePath}");
        Vector2 baseScale = Scale;
        Tween tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        tween.TweenProperty(this, "scale", baseScale * 1.08f, 0.08f);
        tween.TweenProperty(this, "scale", baseScale, 0.10f);
    }

    private static string GetCompletedNodesCsv(GameSession session)
    {
        StringBuilder sb = new StringBuilder();
        bool first = true;

        foreach (StringName key in session.world_flags.Keys)
        {
            string keyText = key.ToString();
            if (!keyText.StartsWith("flow.completed."))
            {
                continue;
            }

            Variant value = session.world_flags[key];
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
