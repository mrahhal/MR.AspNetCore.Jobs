using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server.States;
using MR.AspNetCore.Jobs.Util;

namespace MR.AspNetCore.Jobs.Server
{
	public class JobQueuer : IProcessor
	{
		private ILogger<JobQueuer> _logger;
		internal static readonly AutoResetEvent PulseEvent = new AutoResetEvent(true);
		private JobsOptions _options;
		private TimeSpan _pollingDelay;
		private IStateChanger _stateChanger;

		public JobQueuer(
			IStateChanger stateChanger,
			JobsOptions options,
			ILogger<JobQueuer> logger)
		{
			_stateChanger = stateChanger;
			_options = options;
			_logger = logger;
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
						var state = new EnqueuedState();

						using (var transaction = connection.CreateTransaction())
						{
							_stateChanger.ChangeState(job, state, transaction);
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
