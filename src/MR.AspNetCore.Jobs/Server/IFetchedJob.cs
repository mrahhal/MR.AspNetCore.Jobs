using System;
using System.Threading.Tasks;

namespace MR.AspNetCore.Jobs.Server
{
	public interface IFetchedJob : IDisposable
	{
		int JobId { get; }

		Task RemoveFromQueueAsync();

		Task RequeueAsync();
	}
}
