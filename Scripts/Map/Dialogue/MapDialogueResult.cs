namespace CardChessDemo.Map;

public sealed class MapDialogueResult
{
	public MapDialogueResult(
		MapDialogueCompletionStatus status,
		int pageCount,
		string failureReason = "",
		MapDialogueFollowUpResult? followUpResult = null)
	{
		Status = status;
		PageCount = pageCount;
		FailureReason = failureReason ?? string.Empty;
		FollowUpResult = followUpResult;
	}

	public MapDialogueCompletionStatus Status { get; }
	public int PageCount { get; }
	public string FailureReason { get; }
	public MapDialogueFollowUpResult? FollowUpResult { get; }

	public bool IsCompleted => Status == MapDialogueCompletionStatus.Completed;
	public bool IsClosed => Status == MapDialogueCompletionStatus.Closed;
	public bool IsBlocked => Status == MapDialogueCompletionStatus.Blocked;
	public bool IsFailed => Status == MapDialogueCompletionStatus.Failed;
}
