using System;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class FailedState : IState
	{
		public const string StateName = "Failed";

		public TimeSpan? ExpiresAfter => TimeSpan.FromDays(15);

		public string Name => StateName;
	}
}
