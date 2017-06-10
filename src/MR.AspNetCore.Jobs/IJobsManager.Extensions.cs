using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs
{
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
