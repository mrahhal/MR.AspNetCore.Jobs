using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Server.States;

namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// Manages delayed jobs.
	/// </summary>
	public interface IJobsManager
	{
		/// <summary>
		/// Enqueues a job that will be executed some time in the future.
		/// </summary>
		Task EnqueueAsync(Expression<Action> methodCall);

		/// <summary>
		/// Enqueues a job that will be executed some time in the future.
		/// </summary>
		Task EnqueueAsync<T>(Expression<Action<T>> methodCall);

		/// <summary>
		/// Enqueues a job that will be executed some time in the future.
		/// </summary>
		Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall);

		/// <summary>
		/// Enqueues a job that will be executed at the given moment of time.
		/// </summary>
		Task EnqueueAsync(Expression<Action> methodCall, DateTimeOffset due);

		/// <summary>
		/// Enqueues a job that will be executed at the given moment of time.
		/// </summary>
		Task EnqueueAsync<T>(Expression<Action<T>> methodCall, DateTimeOffset due);

		/// <summary>
		/// Enqueues a job that will be executed at the given moment of time.
		/// </summary>
		Task EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset due);

		/// <summary>
		/// Changes the state of a job.
		/// </summary>
		/// <param name="jobId">The job's id.</param>
		/// <param name="state">The state to transition to.</param>
		/// <param name="expectedState">The expected current state.</param>
		/// <returns>Whether the state transition succeeded.</returns>
		Task<bool> ChangeStateAsync(int jobId, IState state, string expectedState);
	}

	public static class JobsManagerExtensions
	{
		public static Task EnqueueAsync(this IJobsManager @this, Expression<Action> methodCall, TimeSpan delay)
		{
			return @this.EnqueueAsync(methodCall, DateTimeOffset.Now + delay);
		}

		public static Task EnqueueAsync<T>(this IJobsManager @this, Expression<Action<T>> methodCall, TimeSpan delay)
		{
			return @this.EnqueueAsync(methodCall, DateTimeOffset.Now + delay);
		}
	}
}
