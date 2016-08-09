using System;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class EnqueuedState : IState
	{
		public const string StateName = "Enqueued";

		public TimeSpan? ExpiresAfter => null;

		public string Name => StateName;
	}
}
