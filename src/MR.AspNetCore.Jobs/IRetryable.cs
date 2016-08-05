namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// Represents an object that has a <see cref="RetryBehavior"/>.
	/// </summary>
	public interface IRetryable
	{
		/// <summary>
		/// Gets the <see cref="RetryBehavior"/> of this object.
		/// </summary>
		RetryBehavior RetryBehavior { get; }
	}
}
