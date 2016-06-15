using System;

namespace MR.AspNetCore.Jobs.Server
{
	public class AutomaticRetryProcessor : IProcessor
	{
		private const int DefaultMaxRetries = 3;
		private int? _maxEntries;
		private IProcessor _inner;

		public AutomaticRetryProcessor(
			IProcessor inner,
			int? maxEntries = null)
		{
			_inner = inner;
			_maxEntries = maxEntries;
		}

		public override string ToString() => _inner.ToString();

		private int MaxEntries => _maxEntries.HasValue ? _maxEntries.Value : DefaultMaxRetries;

		public void Process(ProcessingContext context)
		{
			for (var i = 0; i <= MaxEntries; i++)
			{
				try
				{
					_inner.Process(context);
					return;
				}
				catch (ProcessingCanceledException)
				{
					throw;
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch
				{
					if (i >= MaxEntries - 1) throw;

					if (context.IsStopping)
					{
						break;
					}
				}
			}
		}
	}
}
