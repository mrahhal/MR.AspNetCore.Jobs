using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Models;
using MR.AspNetCore.Jobs.Server.States;
using MR.AspNetCore.Jobs.Util;

namespace MR.AspNetCore.Jobs.Server
{
	public class DelayedJobProcessor : IProcessor
	{
		protected ILogger _logger;
		protected JobsOptions _options;
		private IStateChanger _stateChanger;
		private IServiceProvider _provider;

		private readonly TimeSpan _pollingDelay;
		internal static readonly AutoResetEvent PulseEvent = new AutoResetEvent(true);

		public DelayedJobProcessor(
			ILogger<DelayedJobProcessor> logger,
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

		public bool Waiting { get; private set; }

		public Task ProcessAsync(ProcessingContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.ThrowIfStopping();
			return ProcessCoreAsync(context);
		}

		public async Task ProcessCoreAsync(ProcessingContext context)
		{
			try
			{
				var worked = await Step(context);

				context.ThrowIfStopping();

				Waiting = true;
				if (!worked)
				{
					var token = GetTokenToWaitOn(context);
					await WaitHandleEx.WaitAnyAsync(PulseEvent, token.WaitHandle, _pollingDelay);
				}
			}
			finally
			{
				Waiting = false;
			}
		}

		private async Task<bool> Step(ProcessingContext context)
		{
			var fetched = default(IFetchedJob);
			using (var scope = _provider.CreateScope())
			{
				var provider = scope.ServiceProvider;
				var connection = provider.GetRequiredService<IStorageConnection>();

				if ((fetched = await connection.FetchNextJobAsync()) != null)
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
							await _stateChanger.ChangeStateAsync(job, new ProcessingState(), connection);

							if (job.Retries > 0)
							{
								_logger.JobRetrying(job.Retries);
							}

							var result = await ExecuteJob(method, instance);
							sp.Stop();

							var newState = default(IState);
							if (!result.Succeeded)
							{
								var shouldRetry = await UpdateJobForRetryAsync(instance, job, connection);
								if (shouldRetry)
								{
									newState = new ScheduledState();
									_logger.JobFailedWillRetry(result.Exception);
								}
								else
								{
									newState = new FailedState();
									_logger.JobFailed(result.Exception);
								}
							}
							else
							{
								newState = new SucceededState();
							}

							await _stateChanger.ChangeStateAsync(job, newState, connection);

							fetched.RemoveFromQueue();
							if (result.Succeeded)
							{
								_logger.JobExecuted(sp.Elapsed.TotalSeconds);
							}
						}
						catch (JobLoadException ex)
						{
							_logger.JobCouldNotBeLoaded(job.Id, ex);

							await _stateChanger.ChangeStateAsync(job, new FailedState(), connection);
							fetched.RemoveFromQueue();
						}
						catch (Exception ex)
						{
							_logger.ExceptionOccuredWhileExecutingJob(job.Id, ex);

							fetched.Requeue();
						}
					}
				}
			}
			return fetched != null;
		}

		private async Task<ExecuteJobResult> ExecuteJob(MethodInvocation method, object instance)
		{
			try
			{
				var result = method.Method.Invoke(instance, method.Args.ToArray());
				if (result is Task taskResult)
				{
					await taskResult;
				}
				return ExecuteJobResult.Success;
			}
			catch (Exception ex)
			{
				return new ExecuteJobResult(false, ex);
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

		private class ExecuteJobResult
		{
			public static readonly ExecuteJobResult Success = new ExecuteJobResult(true);

			public ExecuteJobResult(bool succeeded, Exception exception = null)
			{
				Succeeded = succeeded;
				Exception = exception;
			}

			public bool Succeeded { get; set; }

			public Exception Exception { get; set; }
		}
	}
}
