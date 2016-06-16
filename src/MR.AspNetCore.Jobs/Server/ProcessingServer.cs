using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs.Server
{
	public class ProcessingServer : IProcessingServer, IProcessor, IDisposable
	{
		private CancellationTokenSource _cts;
		private Task _mainTask;
		private IProcessor[] _processors;
		private ILogger<ProcessingServer> _logger;
		private ProcessingContext _context;
		private IStorage _storage;
		private IServiceProvider _provider;

		public ProcessingServer(
			IServiceProvider provider,
			IStorage storage,
			ILogger<ProcessingServer> logger)
		{
			_provider = provider;
			_storage = storage;
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

			_mainTask = Wrap(this).CreateTask(_context, _logger);
		}

		public void Dispose()
		{
			_cts.Cancel();
			_mainTask.Wait(60000);
		}

		public void Process(ProcessingContext context)
		{
			var tasks = _processors
				.Select(Wrap)
				.Select(p => p.CreateTask(context, _logger))
				.ToArray();

			Task.WaitAll(tasks);
		}

		public void Pulse(PulseKind kind)
		{
			_context.Pulse(kind);
		}

		private IProcessor Wrap(IProcessor processor)
		{
			return new InfiniteLoopProcessor(new AutomaticRetryProcessor(processor));
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
