using System;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class ScheduledState : IState
	{
		public const string StateName = "Scheduled";

		public TimeSpan? ExpiresAfter => null;

		public string Name => StateName;
	}
}
