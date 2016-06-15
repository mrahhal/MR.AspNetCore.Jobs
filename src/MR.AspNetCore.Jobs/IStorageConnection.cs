using System;
using MR.AspNetCore.Jobs.Client;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs
{
	public interface IStorageConnection : IDisposable
	{
		void StoreJob(DelayedJob job);

		void StoreJob(CronJob job);

		void UpdateCronJob(CronJob job);

		IFetchedJob FetchNextJob();

		IFetchedJob FetchNextDelayedJob(DateTime from, DateTime to);

		CronJob[] GetCronJobs();

		CronJob GetCronJobByName(string name);

		void RemoveCronJob(string name);
	}

	public static class JobStorageConnectionExtensions
	{
		public static IFetchedJob FetchNextDelayedJob(this IStorageConnection @this, DateTime to)
			=> @this.FetchNextDelayedJob(DateTime.MinValue, to);

		public static IFetchedJob FetchNextDelayedJob(this IStorageConnection @this)
			=> @this.FetchNextDelayedJob(DateTime.MinValue, DateTime.UtcNow);
	}
}
