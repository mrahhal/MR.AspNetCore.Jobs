using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Util;

namespace MR.AspNetCore.Jobs.Server
{
	public class JobQueuer : IProcessor
	{
		private ILogger<JobQueuer> _logger;
		internal static readonly AutoResetEvent PulseEvent = new AutoResetEvent(true);
		private JobsOptions _options;
		private TimeSpan _pollingDelay;

		public JobQueuer(
			ILogger<JobQueuer> logger,
			JobsOptions options)
		{
			_logger = logger;
			_options = options;
			_pollingDelay = TimeSpan.FromSeconds(_options.PollingDelay);
		}

		public async Task ProcessAsync(ProcessingContext context)
		{
			while (!context.IsStopping)
			{
				Job job;
				using (var connection = context.Storage.GetConnection())
				{
					while (
						!context.IsStopping &&
						(job = await connection.GetNextJobToBeEnqueuedAsync()) != null)
					{
						job.StateName = States.Enqueued;

						using (var transaction = connection.CreateTransaction())
						{
							transaction.UpdateJob(job);
							transaction.EnqueueJob(job.Id);
							await transaction.CommitAsync();
						}
					}
				}

				context.ThrowIfStopping();

				BackgroundJobProcessorBase.PulseEvent.Set();
				await WaitHandleEx.WaitAnyAsync(PulseEvent, context.CancellationToken.WaitHandle, _pollingDelay);
			}
		}
	}
}
