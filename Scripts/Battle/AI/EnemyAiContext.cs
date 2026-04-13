using CardChessDemo.Battle.Actions;
using CardChessDemo.Battle.Board;
using CardChessDemo.Battle.State;
using CardChessDemo.Battle.Rooms;

namespace CardChessDemo.Battle.AI;

public sealed class EnemyAiContext
{
    public EnemyAiContext(
        BoardObject self,
        BattleObjectState selfState,
        BoardObjectRegistry registry,
        BattleObjectStateManager stateManager,
        BoardQueryService queryService,
        BoardPathfinder pathfinder,
        BattleRoomTemplate room,
        BoardTargetingService targetingService,
        BattleActionService actionService)
    {
        Self = self;
        SelfState = selfState;
        Registry = registry;
        StateManager = stateManager;
        QueryService = queryService;
        Pathfinder = pathfinder;
        Room = room;
        TargetingService = targetingService;
        ActionService = actionService;
    }

    public BoardObject Self { get; }
    public BattleObjectState SelfState { get; }
    public BoardObjectRegistry Registry { get; }
    public BattleObjectStateManager StateManager { get; }
    public BoardQueryService QueryService { get; }
    public BoardPathfinder Pathfinder { get; }
    public BattleRoomTemplate Room { get; }
    public BoardTargetingService TargetingService { get; }
    public BattleActionService ActionService { get; }
}
