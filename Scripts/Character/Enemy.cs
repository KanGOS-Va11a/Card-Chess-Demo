using Godot;
using CardChessDemo.Battle.Boundary;
using CardChessDemo.Battle.Shared;

public partial class Enemy : InteractableTemplate
{
    [Export] public StringName EncounterId = new StringName("enemy_001");
    [Export] public string EnemyName = "敌人";
    [Export(PropertyHint.File, "*.tscn")] public string BattleScenePath = "res://Scene/Battle.tscn";
    [Export] public bool OneTimeEncounter = false;
    [Export] public bool EnablePatrol = false;
    [Export] public Godot.Collections.Array<Vector2> PatrolPoints = new Godot.Collections.Array<Vector2>();
    [Export(PropertyHint.Range, "10,300,1")] public float PatrolSpeed = 60.0f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float PatrolArriveDistance = 2.0f;
    [Export(PropertyHint.Range, "0,3,0.05")] public float PatrolWaitSeconds = 0.2f;
    [Export] public bool PatrolLoop = true;

    private bool _isTransitioning = false;
    private int _patrolIndex = 0;
    private float _waitTimer = 0.0f;
    private Vector2 _patrolOrigin = Vector2.Zero;

    public override void _Ready()
    {
        base._Ready();
        _patrolOrigin = GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!EnablePatrol || _isTransitioning || PatrolPoints == null || PatrolPoints.Count == 0)
        {
            return;
        }

        if (_waitTimer > 0.0f)
        {
            _waitTimer -= (float)delta;
            return;
        }

        Vector2 target = _patrolOrigin + PatrolPoints[_patrolIndex];
        Vector2 toTarget = target - GlobalPosition;
        float distance = toTarget.Length();

        if (distance <= PatrolArriveDistance)
        {
            _waitTimer = PatrolWaitSeconds;
            AdvancePatrolIndex();
            return;
        }

        float step = PatrolSpeed * (float)delta;
        if (step > distance)
        {
            step = distance;
        }

        GlobalPosition += toTarget.Normalized() * step;
    }

    public override string GetInteractText(Player player)
    {
        if (_isTransitioning)
        {
            return "进入战斗中...";
        }

        if (!CanInteract(player))
        {
            return "已清理";
        }

        return string.IsNullOrWhiteSpace(PromptText) ? "发起战斗" : PromptText;
    }

    public override bool CanInteract(Player player)
    {
        if (_isTransitioning || !base.CanInteract(player))
        {
            return false;
        }

        if (!OneTimeEncounter)
        {
            return true;
        }

        GameSession session = GetNodeOrNull<GameSession>("/root/GameSession");
        if (session == null)
        {
            return true;
        }

        return !session.cleared_encounters.Contains(EncounterId);
    }

    protected override void OnInteract(Player player)
    {
        if (string.IsNullOrWhiteSpace(BattleScenePath))
        {
            GD.PushError("Enemy: BattleScenePath 为空，无法进入战斗场景。");
            return;
        }

        GlobalGameSession globalSession = GetNodeOrNull<GlobalGameSession>("/root/GlobalGameSession");
        if (globalSession == null)
        {
            GD.PushError("Enemy: 未找到 /root/GlobalGameSession，无法构建 BattleRequest。");
            return;
        }

        _isTransitioning = true;
        Node currentScene = GetTree().CurrentScene;
        string returnScenePath = currentScene?.SceneFilePath ?? string.Empty;
        Vector2 returnPlayerPos = player?.GlobalPosition ?? Vector2.Zero;
        string encounterId = EncounterId.ToString();

        globalSession.BeginBattle(BattleRequest.FromSession(globalSession, encounterId));
        globalSession.SetPendingBattleEncounterId(encounterId);
        globalSession.SetPendingMapResumeContext(new MapResumeContext(returnScenePath, returnPlayerPos));

        Error result = ChangeToBattleScene();
        if (result != Error.Ok)
        {
            globalSession.CancelPendingBattleTransition();
            _isTransitioning = false;
            GD.PushError($"Enemy: 切换战斗场景失败，错误码={result}，BattleScenePath='{BattleScenePath}'");
        }
    }

    private Error ChangeToBattleScene()
    {
        string rawPath = BattleScenePath?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(rawPath))
        {
            return Error.InvalidParameter;
        }

        if (rawPath.StartsWith("uid://"))
        {
            PackedScene byUid = ResourceLoader.Load<PackedScene>(rawPath);
            if (byUid == null)
            {
                return Error.CantOpen;
            }

            return GetTree().ChangeSceneToPacked(byUid);
        }

        string normalizedPath = NormalizeLegacyScenePath(rawPath);
        if (!ResourceLoader.Exists(normalizedPath))
        {
            return Error.CantOpen;
        }

        return GetTree().ChangeSceneToFile(normalizedPath);
    }

    private static string NormalizeLegacyScenePath(string path)
    {
        if (path.StartsWith("res://Scene(garbage)/"))
        {
            return path.Replace("res://Scene(garbage)/", "res://Scene/");
        }

        return path;
    }

    private void AdvancePatrolIndex()
    {
        if (PatrolPoints == null || PatrolPoints.Count == 0)
        {
            _patrolIndex = 0;
            return;
        }

        _patrolIndex++;
        if (_patrolIndex < PatrolPoints.Count)
        {
            return;
        }

        if (PatrolLoop)
        {
            _patrolIndex = 0;
        }
        else
        {
            _patrolIndex = PatrolPoints.Count - 1;
            EnablePatrol = false;
        }
    }
}
