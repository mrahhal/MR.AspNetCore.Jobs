using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public class CronJobProcessor : IProcessor
	{
		private ILogger _logger;

		public CronJobProcessor(ILogger<CronJobProcessor> logger)
		{
			_logger = logger;
		}

		public override string ToString() => nameof(CronJobProcessor);

		public Task ProcessAsync(ProcessingContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			return ProcessCoreAsync(context);
		}

		private async Task ProcessCoreAsync(ProcessingContext context)
		{
			var storage = context.Storage;
			var jobs = await GetJobsAsync(storage);
			if (!jobs.Any())
			{
				_logger.CronJobsNotFound();

				// This will cancel this processor.
				throw new OperationCanceledException();
			}
			_logger.CronJobsScheduling(jobs);

			context.ThrowIfStopping();

			var computedJobs = Compute(jobs, context.CronJobRegistry.Build());
			if (context.IsStopping)
			{
				return;
			}

			await Task.WhenAll(computedJobs.Select(j => RunAsync(j, context)));
		}

		private async Task RunAsync(ComputedCronJob computedJob, ProcessingContext context)
		{
			var storage = context.Storage;
			var retryBehavior = computedJob.RetryBehavior;

			while (!context.IsStopping)
			{
				var now = DateTime.UtcNow;

				var due = ComputeDue(computedJob, now);
				var timeSpan = due - now;

				if (timeSpan.TotalSeconds > 0)
				{
					await context.WaitAsync(timeSpan);
				}

				context.ThrowIfStopping();

				if (computedJob.Retries > 0 && computedJob.FirstTry < computedJob.Next)
				{
					computedJob.Retries = 0;
				}

				using (var scopedContext = context.CreateScope())
				{
					var factory = scopedContext.Provider.GetService<IJobFactory>();
					var job = (IJob)factory.Create(computedJob.JobType);
					var success = true;

					try
					{
						var sw = Stopwatch.StartNew();
						await job.ExecuteAsync();
						sw.Stop();
						computedJob.Retries = 0;
						_logger.CronJobExecuted(computedJob.Job.Name, sw.Elapsed.TotalSeconds);
					}
					catch (Exception ex)
					{
						success = false;
						computedJob.Retries++;
						_logger.CronJobFailed(computedJob.Job.Name, ex);
					}

					if (success)
					{
						using (var connection = storage.GetConnection())
						{
							now = DateTime.UtcNow;
							computedJob.Update(now);
							await connection.UpdateCronJobAsync(computedJob.Job);
						}
					}
				}
			}
		}

		private DateTime ComputeDue(ComputedCronJob computedJob, DateTime now)
		{
			computedJob.UpdateNext(now);

			var retryBehavior = computedJob.RetryBehavior;
			var retries = computedJob.Retries;

			if (retries == 0)
			{
				return computedJob.Next;
			}

			var realNext = computedJob.Schedule.GetNextOccurrence(now);

			if (retries > 0 && !retryBehavior.Retry)
			{
				return realNext;
			}

			if (retries >= retryBehavior.RetryCount)
			{
				return realNext;
			}

			return computedJob.FirstTry.AddSeconds(retryBehavior.RetryIn(retries));
		}

		private async Task<CronJob[]> GetJobsAsync(IStorage storage)
		{
			using (var connection = storage.GetConnection())
			{
				return await connection.GetCronJobsAsync();
			}
		}

		private ComputedCronJob[] Compute(IEnumerable<CronJob> jobs, CronJobRegistry.Entry[] entries)
			=> jobs.Select(j => CreateComputedCronJob(j, entries)).ToArray();

		private ComputedCronJob CreateComputedCronJob(CronJob job, CronJobRegistry.Entry[] entries)
		{
			var entry = entries.First(e => e.Name == job.Name);
			return new ComputedCronJob(job, entry);
		}
	}
}
