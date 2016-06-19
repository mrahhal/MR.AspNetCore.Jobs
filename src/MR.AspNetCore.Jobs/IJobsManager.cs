using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
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
