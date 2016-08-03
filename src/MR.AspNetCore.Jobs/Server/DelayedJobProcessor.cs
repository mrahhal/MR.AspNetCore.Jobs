using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs.Server
{
	public class DelayedJobProcessor : BackgroundJobProcessorBase
	{
		private CancellationTokenSource _cts;
		private CancellationTokenSource _linkedCts;

		public DelayedJobProcessor(
			JobsOptions options,
			ILogger<DelayedJobProcessor> logger)
			: base(options, logger)
		{
		}

		public override string ToString() => nameof(BackgroundJobProcessorBase);

		protected override Task<IFetchedJob> FetchNextJobCoreAsync(IStorageConnection connection)
		{
			return connection.FetchNextDelayedJobAsync();
		}

		protected override void OnStepEnter(ProcessingContext context)
		{
			_cts = new CancellationTokenSource();
			_linkedCts = CreateLinked(context);
		}

		protected override void OnStepExit(ProcessingContext context)
		{
			_linkedCts.Dispose();
			_cts.Dispose();
		}

		protected override CancellationToken GetTokenToWaitOn(ProcessingContext context)
		{
			return _linkedCts.Token;
		}

		private CancellationTokenSource CreateLinked(ProcessingContext context)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(
				context.CancellationToken,
				_cts.Token);
		}

		public override void Pulse()
		{
			_cts.Cancel();
		}
	}
}
