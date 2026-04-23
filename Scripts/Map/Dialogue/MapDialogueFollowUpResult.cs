namespace CardChessDemo.Map;

public sealed class MapDialogueFollowUpResult
{
	public MapDialogueFollowUpResult(bool executed, bool succeeded, string failureReason = "")
	{
		Executed = executed;
		Succeeded = succeeded;
		FailureReason = failureReason ?? string.Empty;
	}

	public bool Executed { get; }
	public bool Succeeded { get; }
	public string FailureReason { get; }
}
