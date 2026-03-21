namespace CardChessDemo.Battle.Cards;

public sealed class BattleCardInstance
{
    public BattleCardInstance(string instanceId, BattleCardDefinition definition)
    {
        InstanceId = instanceId;
        Definition = definition;
    }

    public string InstanceId { get; }

    public BattleCardDefinition Definition { get; }
}
