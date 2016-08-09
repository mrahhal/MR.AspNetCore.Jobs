using System;

namespace MR.AspNetCore.Jobs.Server
{
	public interface IFetchedJob : IDisposable
	{
		int JobId { get; }

		// REVIEW: Shouldn't these be async too?
		void RemoveFromQueue();
		void Requeue();
	}
}
