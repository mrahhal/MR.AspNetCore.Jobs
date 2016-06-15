using Microsoft.Extensions.Logging;
using MR.AspNetCore.Jobs.Client;

namespace MR.AspNetCore.Jobs.Server
{
	public class DelayedJobProcessor : BackgroundJobProcessorBase
	{
		public DelayedJobProcessor(ILogger<DelayedJobProcessor> logger)
			: base(logger)
		{
		}

		protected override IFetchedJob FetchNextJobCore(IStorageConnection connection)
		{
			return connection.FetchNextDelayedJob();
		}

		public override string ToString() => nameof(DelayedJobProcessor);
	}
}
