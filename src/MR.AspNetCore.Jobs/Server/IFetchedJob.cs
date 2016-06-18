using System;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server
{
	public interface IFetchedJob : IDisposable
	{
		DelayedJob Job { get; }

		void RemoveFromQueue();
		void Requeue();
	}
}
