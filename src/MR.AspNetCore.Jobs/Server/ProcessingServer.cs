using System;
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

		public ProcessingServer(
			IServiceProvider provider,
			IStorage storage,
			ILoggerFactory loggerFactory,
			ILogger<ProcessingServer> logger)
		{
			_provider = provider;
			_storage = storage;
			_loggerFactory = loggerFactory;
			_logger = logger;
			_cts = new CancellationTokenSource();
		}

		public void Start()
		{
			_logger.LogInformation("Starting the processing server.");
			_processors = GetProcessors();
			_logger.LogInformation($"Initiating {_processors.Length} processors.");

			_context = new ProcessingContext(
				_provider,
				_storage,
				_cts.Token);

			var processorTasks = _processors
				.Select(p => InfiniteRetry(p))
				.Select(p => p.ProcessAsync(_context));
			_compositeTask = Task.WhenAll(processorTasks);
		}

		public void Pulse(PulseKind kind)
		{
			_context.Pulse(kind);
		}

		public void Dispose()
		{
			_cts.Cancel();
			try
			{
				_compositeTask.Wait(60000);
			}
			catch (AggregateException ex) when (ex.InnerExceptions[0] is OperationCanceledException)
			{
			}
		}

		private IProcessor InfiniteRetry(IProcessor inner)
		{
			return new InfiniteRetryProcessor(inner, _loggerFactory);
		}

		private IProcessor[] GetProcessors()
		{
			return new IProcessor[]
			{
				_provider.GetService<FireAndForgetJobProcessor>(),
				_provider.GetService<DelayedJobProcessor>(),
				_provider.GetService<CronJobProcessor>()
			};
		}
	}
}
