using System;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public interface IStorageConnection : IDisposable
	{
		// Delayed jobs

		Task StoreDelayedJobAsync(DelayedJob job, DateTime? Due);

		Task<IFetchedJob> FetchNextDelayedJobAsync();

		Task<string> GetDelayedJobParameterAsync(string id, string name);

		Task SetDelayedJobParameterAsync(string id, string name, string value);

		Task SetDelayedJobDue(string id, DateTime? due);

		// Cron jobs

		Task StoreCronJobAsync(CronJob job);

		Task UpdateCronJobAsync(CronJob job);

		Task<CronJob[]> GetCronJobsAsync();

		Task<CronJob> GetCronJobByNameAsync(string name);

		Task RemoveCronJobAsync(string name);
	}

	public static class StorageConnectionExtensions
	{
		public static Task SetDelayedJobParameterAsync<T>(
			this IStorageConnection connection, string id, string name, T value)
		{
			return connection.SetDelayedJobParameterAsync(id, name, Helper.ToJson(value));
		}

		public static async Task<T> GetDelayedJobParameterAsync<T>(
			this IStorageConnection connection, string id, string name)
		{
			return Helper.FromJson<T>(await connection.GetDelayedJobParameterAsync(id, name));
		}
	}
}
