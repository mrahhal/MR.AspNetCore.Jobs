using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs.Server
{
	public class InfiniteLoopProcessor : IProcessor
	{
		private IProcessor _inner;
		private ILogger _logger;

		public InfiniteLoopProcessor(
			IProcessor inner,
			ILoggerFactory loggerFactory)
		{
			_inner = inner;
			_logger = loggerFactory.CreateLogger<InfiniteLoopProcessor>();
		}

		public override string ToString() => _inner.ToString();

		public async Task ProcessAsync(ProcessingContext context)
		{
			while (!context.IsStopping)
			{
				try
				{
					await _inner.ProcessAsync(context);
				}
				catch (OperationCanceledException)
				{
					return;
				}
				catch (Exception ex)
				{
					_logger.LogWarning($"Prcessor '{_inner.ToString()}' failed: '{ex.Message}'. Retrying...");
				}
			}
		}
	}
}
