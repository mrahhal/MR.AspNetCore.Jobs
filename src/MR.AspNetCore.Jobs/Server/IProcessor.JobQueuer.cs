using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
		private IServiceProvider _provider;

		internal static readonly AutoResetEvent PulseEvent = new AutoResetEvent(true);
		private TimeSpan _pollingDelay;

		public JobQueuer(
			ILogger<JobQueuer> logger,
			JobsOptions options,
			IStateChanger stateChanger,
			IServiceProvider provider)
		{
			_logger = logger;
			_options = options;
			_stateChanger = stateChanger;
			_provider = provider;

			_pollingDelay = TimeSpan.FromSeconds(_options.PollingDelay);
		}

		public async Task ProcessAsync(ProcessingContext context)
		{
			using (var scope = _provider.CreateScope())
			{
				Job job;
				var provider = scope.ServiceProvider;
				var connection = provider.GetRequiredService<IStorageConnection>();

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
