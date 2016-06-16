using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Client;

namespace MR.AspNetCore.Jobs.Server
{
	public abstract class BackgroundJobProcessorBase : IProcessor
	{
		private readonly TimeSpan _pollingDelay = TimeSpan.FromSeconds(15);
		protected ILogger _logger;

		public BackgroundJobProcessorBase(ILogger logger)
		{
			_logger = logger;
		}

		public void Process(ProcessingContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			OnProcessEnter(context);

			try
			{
				var storage = context.Storage;
				using (var connection = storage.GetConnection())
				{
					var fetched = default(IFetchedJob);
					while (!context.IsStopping && (fetched = FetchNextJobCore(connection)) != null)
					{
						using (fetched)
						using (var scopedContext = context.CreateScope())
						{
							var job = fetched.Job;
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
								result?.GetAwaiter().GetResult();
								sp.Stop();
								fetched.RemoveFromQueue();
								_logger.LogInformation(
									$"Job executed succesfully. Took: {sp.Elapsed.TotalSeconds} secs.");
							}
							catch (Exception ex)
							{
								_logger.LogWarning(
									$"Failed to execute a job: \"{ex.Message}\". Requeuing.");
								fetched.Requeue();
								throw;
							}
						}
					}
				}

				var token = GetTokenToWaitOn(context);
				token.WaitHandle.WaitOne(_pollingDelay);
			}
			finally
			{
				OnProcessExit(context);
			}
		}

		protected virtual CancellationToken GetTokenToWaitOn(ProcessingContext context)
		{
			return context.CancellationToken;
		}

		protected abstract IFetchedJob FetchNextJobCore(IStorageConnection connection);

		protected virtual void OnProcessEnter(ProcessingContext context)
		{
		}

		protected virtual void OnProcessExit(ProcessingContext context)
		{
		}
	}
}
