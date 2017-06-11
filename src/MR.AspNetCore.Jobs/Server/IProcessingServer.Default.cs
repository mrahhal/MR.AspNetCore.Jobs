using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs.Server
{
	public class ProcessingServer : IProcessingServer, IDisposable
	{
		private ILogger _logger;
		private ILoggerFactory _loggerFactory;
		private IServiceProvider _provider;
		private IStorage _storage;
		private JobsOptions _options;

		private CancellationTokenSource _cts;
		private IProcessor[] _processors;
		private DelayedJobProcessor[] _delayedJobProcessors;
		private ProcessingContext _context;
		private Task _compositeTask;
		private bool _disposed;

		public ProcessingServer(
			ILogger<ProcessingServer> logger,
			ILoggerFactory loggerFactory,
			IServiceProvider provider,
			IStorage storage,
			JobsOptions options)
		{
			_logger = logger;
			_loggerFactory = loggerFactory;
			_provider = provider;
			_storage = storage;
			_options = options;
			_cts = new CancellationTokenSource();
		}

		public void Start()
		{
			var processorCount = Environment.ProcessorCount;
			_processors = GetProcessors(processorCount);
			_logger.ServerStarting(processorCount, _processors.Length);

			_context = new ProcessingContext(
				_provider,
				_storage,
				_options.CronJobRegistry,
				_cts.Token);

			var processorTasks = _processors
				.Select(p => InfiniteRetry(p))
				.Select(p => p.ProcessAsync(_context));
			_compositeTask = Task.WhenAll(processorTasks);
		}

		public void Pulse()
		{
			if (!AllProcessorsWaiting())
			{
				// Some processor is still executing jobs so no need to pulse.
				return;
			}

			_logger.LogTrace("Pulsing the JobQueuer.");
			JobQueuer.PulseEvent.Set();
		}

		private bool AllProcessorsWaiting()
		{
			foreach (var processor in _delayedJobProcessors)
			{
				if (!processor.Waiting)
				{
					return false;
				}
			}
			return true;
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;

			_logger.ServerShuttingDown();
			_cts.Cancel();
			try
			{
				_compositeTask.Wait((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
			}
			catch (AggregateException ex)
			{
				var innerEx = ex.InnerExceptions[0];
				if (!(innerEx is OperationCanceledException))
				{
					_logger.ExpectedOperationCanceledException(innerEx);
				}
			}
		}

		private IProcessor InfiniteRetry(IProcessor inner)
		{
			return new InfiniteRetryProcessor(inner, _loggerFactory);
		}

		private IProcessor[] GetProcessors(int processorCount)
		{
			var processors = new List<IProcessor>();
			var delayedJobProcessors = new List<DelayedJobProcessor>(processorCount);

			for (var i = 0; i < processorCount; i++)
			{
				delayedJobProcessors.Add(_provider.GetService<DelayedJobProcessor>());
				_delayedJobProcessors = delayedJobProcessors.ToArray();
			}
			processors.AddRange(delayedJobProcessors);

			processors.Add(_provider.GetService<CronJobProcessor>());

			processors.Add(_provider.GetService<JobQueuer>());

			processors.AddRange(_provider.GetServices<IAdditionalProcessor>());

			return processors.ToArray();
		}
	}
}
