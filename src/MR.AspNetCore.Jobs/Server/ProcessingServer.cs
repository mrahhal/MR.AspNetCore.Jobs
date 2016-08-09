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
		private CancellationTokenSource _cts;
		private Task _compositeTask;
		private IProcessor[] _processors;
		private ILogger<ProcessingServer> _logger;
		private ProcessingContext _context;
		private IStorage _storage;
		private IServiceProvider _provider;
		private ILoggerFactory _loggerFactory;
		private BackgroundJobProcessorBase[] _backgroundJobProcessors;
		private JobsOptions _options;
		private bool _disposed;

		public ProcessingServer(
			IServiceProvider provider,
			IStorage storage,
			JobsOptions options,
			ILoggerFactory loggerFactory,
			ILogger<ProcessingServer> logger)
		{
			_provider = provider;
			_storage = storage;
			_options = options;
			_loggerFactory = loggerFactory;
			_logger = logger;
			_cts = new CancellationTokenSource();
		}

		public void Start()
		{
			_logger.LogInformation("Starting the processing server.");
			var processorCount = Environment.ProcessorCount;
			_logger.LogInformation($"Detected {processorCount} machine processor(s).");
			_processors = GetProcessors(processorCount);
			_logger.LogInformation($"Initiating {_processors.Length} job processors.");

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

			JobQueuer.PulseEvent.Set();
		}

		private bool AllProcessorsWaiting()
		{
			// Perf: avoid allocation
			for (int i = 0; i < _backgroundJobProcessors.Length; i++)
			{
				if (!_backgroundJobProcessors[i].Waiting)
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

			_logger.LogInformation("Shutting down Jobs processing server.");
			_cts.Cancel();
			try
			{
				_compositeTask.Wait(60000);
			}
			catch (AggregateException ex)
			{
				if (!(ex.InnerExceptions[0] is OperationCanceledException))
				{
					_logger.LogWarning(
						$"Expected an OperationCanceledException, but found '{ex.InnerExceptions[0].Message}'.");
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
			var backgroundJobProcessors = new List<BackgroundJobProcessorBase>(processorCount);

			for (int i = 0; i < processorCount; i++)
			{
				backgroundJobProcessors.Add(_provider.GetService<DelayedJobProcessor>());
				_backgroundJobProcessors = backgroundJobProcessors.ToArray();
			}
			processors.AddRange(backgroundJobProcessors);

			processors.Add(_provider.GetService<CronJobProcessor>());

			processors.Add(_provider.GetService<JobQueuer>());

			processors.AddRange(_provider.GetServices<IAdditionalProcessor>());

			return processors.ToArray();
		}
	}
}
