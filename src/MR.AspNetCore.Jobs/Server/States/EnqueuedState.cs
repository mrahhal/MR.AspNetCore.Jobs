using System;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class EnqueuedState : IState
	{
		public const string StateName = "Enqueued";

		public TimeSpan? ExpiresAfter => null;

		public string Name => StateName;

		public void Apply(Job job, IStorageTransaction transaction)
		{
			transaction.EnqueueJob(job.Id);
		}
	}
}
