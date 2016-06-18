using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MR.AspNetCore.Jobs.Server
{
	public class DelayedJobProcessor : BackgroundJobProcessorBase
	{
		public DelayedJobProcessor(ILogger<DelayedJobProcessor> logger)
			: base(logger)
		{
		}

		protected override Task<IFetchedJob> FetchNextJobCoreAsync(IStorageConnection connection)
		{
			return connection.FetchNextDelayedJob();
		}

		public override string ToString() => nameof(DelayedJobProcessor);
	}
}
