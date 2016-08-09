using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Util;

namespace MR.AspNetCore.Jobs.Server
{
	public abstract class BackgroundJobProcessorBase : IProcessor
	{
		private readonly TimeSpan _pollingDelay;
		protected JobsOptions _options;
		protected ILogger _logger;
		internal static readonly AutoResetEvent PulseEvent = new AutoResetEvent(true);

		public BackgroundJobProcessorBase(
			JobsOptions options,
			ILogger logger)
		{
			_options = options;
			_logger = logger;
			_pollingDelay = TimeSpan.FromSeconds(_options.PollingDelay);
		}

		public bool Waiting { get; private set; }

		public Task ProcessAsync(ProcessingContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			return ProcessCoreAsync(context);
		}

		public async Task ProcessCoreAsync(ProcessingContext context)
		{
			while (!context.IsStopping)
			{
				OnStepEnter(context);
				try
				{
					await Step(context);

					context.ThrowIfStopping();

					Waiting = true;
					var token = GetTokenToWaitOn(context);
					await WaitHandleEx.WaitAnyAsync(PulseEvent, token.WaitHandle, _pollingDelay);
				}
				finally
				{
					Waiting = false;
					OnStepExit(context);
				}
			}
		}

		public virtual void Pulse()
		{
		}

		private async Task Step(ProcessingContext context)
		{
			using (var connection = context.Storage.GetConnection())
			{
				var fetched = default(IFetchedJob);
				while (
					!context.IsStopping &&
					(fetched = await FetchNextJobCoreAsync(connection)) != null)
				{
					using (fetched)
					using (var scopedContext = context.CreateScope())
					{
						var job = await connection.GetJobAsync(fetched.JobId);
						var invocationData = Helper.FromJson<InvocationData>(job.Data);
						var method = invocationData.Deserialize();
						var factory = scopedContext.Provider.GetService<IJobFactory>();

						var instance = default(object);
						if (!method.Method.IsStatic)
						{
							instance = factory.Create(method.Type);
						}

						try
						{
							var sp = Stopwatch.StartNew();
							var result =
								method.Method.Invoke(instance, method.Args.ToArray()) as Task;
							if (result != null)
							{
								await result;
							}
							sp.Stop();
							fetched.RemoveFromQueue();
							_logger.LogInformation(
								"Job executed succesfully. Took: {seconds} secs.",
								sp.Elapsed.TotalSeconds);
						}
						catch (Exception ex)
						{
							var shouldRetry = await UpdateJobForRetryAsync(instance, job, connection);
							if (shouldRetry)
							{
								_logger.LogWarning(
									$"Job failed to execute: '{ex.Message}'. Requeuing for another retry.");
								fetched.Requeue();
							}
							else
							{
								_logger.LogWarning(
									$"Job failed to execute: '{ex.Message}'.");
								// TODO: Send to DJQ
							}
						}
					}
				}
			}
		}

		private async Task<bool> UpdateJobForRetryAsync(object instance, Job job, IStorageConnection connection)
		{
			var retryBehavior =
				(instance as IRetryable)?.RetryBehavior ??
				RetryBehavior.DefaultRetry;

			if (!retryBehavior.Retry)
			{
				return false;
			}

			var now = DateTime.UtcNow;
			var retries = ++job.Retries;
			if (retries >= retryBehavior.RetryCount)
			{
				return false;
			}

			var due = job.Added.AddSeconds(retryBehavior.RetryIn(retries));
			job.Due = due;
			using (var transaction = connection.CreateTransaction())
			{
				transaction.UpdateJob(job);
				await transaction.CommitAsync();
			}
			return true;
		}

		protected virtual CancellationToken GetTokenToWaitOn(ProcessingContext context)
		{
			return context.CancellationToken;
		}

		protected abstract Task<IFetchedJob> FetchNextJobCoreAsync(IStorageConnection connection);

		protected virtual void OnStepEnter(ProcessingContext context)
		{
		}

		protected virtual void OnStepExit(ProcessingContext context)
		{
		}
	}
}
