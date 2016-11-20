using System;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs;

namespace Basic.Jobs
{
	public class RetryableJob : IRetryable
	{
		public RetryBehavior RetryBehavior => RetryBehavior.NoRetry;

		public async Task PrintSomething()
		{
			await Task.Delay(100);
			Console.WriteLine("Something");
		}
	}
}
