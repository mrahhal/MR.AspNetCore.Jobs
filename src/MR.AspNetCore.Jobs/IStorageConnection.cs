using System;
using System.Threading.Tasks;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	/// <summary>
	/// Represents a connection to the storage.
	/// </summary>
	public interface IStorageConnection : IDisposable
	{
		// Delayed jobs

		/// <summary>
		/// Stores a delayed job and associates its due time.
		/// </summary>
		/// <param name="job">The job to store.</param>
		/// <param name="Due">The due time to associate the job with.</param>
		Task StoreDelayedJobAsync(DelayedJob job, DateTime? Due);

		/// <summary>
		/// Fetches the next delayed job to be executed.
		/// </summary>
		Task<IFetchedJob> FetchNextDelayedJobAsync();

		/// <summary>
		/// Gets a delayed job parameter.
		/// </summary>
		/// <param name="id">The id of the job.</param>
		/// <param name="name">The name of the parameter.</param>
		/// <returns>The value of the parameter.</returns>
		Task<string> GetDelayedJobParameterAsync(string id, string name);

		/// <summary>
		/// Sets a delayed job parameter.
		/// </summary>
		/// <param name="id">The id of the job.</param>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		Task SetDelayedJobParameterAsync(string id, string name, string value);

		/// <summary>
		/// Associates a delayed job with a due time.
		/// </summary>
		/// <param name="id">The id of the job.</param>
		/// <param name="due">The due time to associate the job with.</param>
		Task SetDelayedJobDue(string id, DateTime? due);

		// Cron jobs

		/// <summary>
		/// Stores a cron job.
		/// </summary>
		/// <param name="job">The job to store.</param>
		Task StoreCronJobAsync(CronJob job);

		/// <summary>
		/// Updates a cron job.
		/// </summary>
		/// <param name="job">The job to update.</param>
		Task UpdateCronJobAsync(CronJob job);

		/// <summary>
		/// Gets all cron jobs.
		/// </summary>
		Task<CronJob[]> GetCronJobsAsync();

		/// <summary>
		/// Removes a cron job.
		/// </summary>
		/// <param name="name">The name if the cron job.</param>
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
