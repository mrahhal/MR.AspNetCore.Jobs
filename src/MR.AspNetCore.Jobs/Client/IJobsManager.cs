using System;
using System.Linq.Expressions;

namespace MR.AspNetCore.Jobs.Client
{
	public interface IJobsManager
	{
		/// <summary>
		/// Enqueues a job that will be executed some time in the future.
		/// </summary>
		void Enqueue(Expression<Action> methodCall);

		/// <summary>
		/// Enqueues a job that will be executed some time in the future.
		/// </summary>
		void Enqueue<T>(Expression<Action<T>> methodCall);

		/// <summary>
		/// Enqueues a job that will be executed at the given moment of time.
		/// </summary>
		void Enqueue(Expression<Action> methodCall, DateTimeOffset due);

		/// <summary>
		/// Enqueues a job that will be executed at the given moment of time.
		/// </summary>
		void Enqueue<T>(Expression<Action<T>> methodCall, DateTimeOffset due);
	}

	public static class JobsManagerExtensions
	{
		public static void Enqueue(this IJobsManager @this, Expression<Action> methodCall, TimeSpan delay)
		{
			@this.Enqueue(methodCall, DateTimeOffset.Now + delay);
		}

		public static void Enqueue<T>(this IJobsManager @this, Expression<Action<T>> methodCall, TimeSpan delay)
		{
			@this.Enqueue(methodCall, DateTimeOffset.Now + delay);
		}
	}
}
