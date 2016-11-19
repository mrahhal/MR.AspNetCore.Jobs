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
		private ILogger _logger;
		private JobsOptions _options;
		private IStateChanger _stateChanger;

		internal static readonly AutoResetEvent PulseEvent = new AutoResetEvent(true);
		private TimeSpan _pollingDelay;

		public JobQueuer(
			ILogger<JobQueuer> logger,
			JobsOptions options,
			IStateChanger stateChanger)
		{
			_logger = logger;
			_options = options;
			_stateChanger = stateChanger;

			_pollingDelay = TimeSpan.FromSeconds(_options.PollingDelay);
		}

		public async Task ProcessAsync(ProcessingContext context)
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
						await transaction.CommitAsync();
					}
				}
			}

			context.ThrowIfStopping();

			DelayedJobProcessor.PulseEvent.Set();
			await WaitHandleEx.WaitAnyAsync(PulseEvent, context.CancellationToken.WaitHandle, _pollingDelay);
		}
	}
}
