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
		private ComputedCronJobElector _jobElector = new ComputedCronJobElector();
		private ILogger<CronJobProcessor> _logger;

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
				_logger.LogInformation(
					"Couldn't find any cron jobs to schedule, cancelling processing of cron jobs.");
				throw new OperationCanceledException();
			}
			LogInfoAboutCronJobs(jobs);

			context.ThrowIfStopping();

			var computed = Compute(jobs);
			while (!context.IsStopping)
			{
				var now = DateTime.UtcNow;
				var nextJob = ElectNextJob(computed);
				var due = nextJob.Next;
				var timeSpan = due - now;

				if (timeSpan.TotalSeconds > 0)
				{
					await context.WaitAsync(timeSpan);
				}

				context.ThrowIfStopping();

				using (var scopedContext = context.CreateScope())
				{
					var factory = scopedContext.Provider.GetService<IJobFactory>();
					var job = (IJob)factory.Create(nextJob.JobType);

					try
					{
						var sw = Stopwatch.StartNew();
						await job.ExecuteAsync();
						sw.Stop();
						_logger.LogInformation(
							"Cron job '{jobName}' executed succesfully. Took: {seconds} secs.",
							nextJob.Job.Name, sw.Elapsed.TotalSeconds);
					}
					catch (Exception ex)
					{
						_logger.LogWarning(
							$"Cron job '{{jobName}}' failed to execute: '{ex.Message}'.",
							nextJob.Job.Name);
					}

					using (var connection = storage.GetConnection())
					{
						now = DateTime.UtcNow;
						nextJob.Update(now);
						await connection.UpdateCronJobAsync(nextJob.Job);
					}
				}
			}
		}

		private void LogInfoAboutCronJobs(CronJob[] jobs)
		{
			_logger.LogInformation($"Found {jobs.Length} cron job(s) to schedule.");
			foreach (var job in jobs)
			{
				_logger.LogDebug($"Will schedule '{job.Name}' with cron '{job.Cron}'.");
			}
		}

		private async Task<CronJob[]> GetJobsAsync(IStorage storage)
		{
			using (var connection = storage.GetConnection())
			{
				return await connection.GetCronJobsAsync();
			}
		}

		private ComputedCronJob[] Compute(IEnumerable<CronJob> jobs)
			=> jobs.Select(CreateComputedCronJob).ToArray();

		private ComputedCronJob CreateComputedCronJob(CronJob job)
			=> new ComputedCronJob(job);

		private ComputedCronJob ElectNextJob(IEnumerable<ComputedCronJob> jobs)
			=> _jobElector.Elect(jobs);
	}
}
