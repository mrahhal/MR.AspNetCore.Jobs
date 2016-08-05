using System;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public interface IFetchedJob : IDisposable
	{
		DelayedJob Job { get; }

		// REVIEW: Shouldn't these be async too?
		void RemoveFromQueue();
		void Requeue();
	}
}
