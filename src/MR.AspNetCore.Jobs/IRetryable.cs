namespace MR.AspNetCore.Jobs
{
	public interface IRetryable
	{
		RetryBehavior RetryBehavior { get; }
	}
}
