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
		/// Stores the job.
		/// </summary>
		/// <param name="job">The job to store.</param>
		Task StoreJobAsync(Job job);

		/// <summary>
		/// Returns the job with the given id.
		/// </summary>
		/// <param name="id">The job's id.</param>
		Task<Job> GetJobAsync(int id);

		/// <summary>
		/// Fetches the next job to be executed.
		/// </summary>
		Task<IFetchedJob> FetchNextJobAsync();

		/// <summary>
		/// Gets the next job to be enqueued.
		/// </summary>
		Task<Job> GetNextJobToBeEnqueuedAsync();

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
		/// <param name="name">The name of the cron job.</param>
		Task RemoveCronJobAsync(string name);

		//-----------------------------------------

		/// <summary>
		/// Creates and returns an <see cref="IStorageTransaction"/>.
		/// </summary>
		IStorageTransaction CreateTransaction();
	}
}
