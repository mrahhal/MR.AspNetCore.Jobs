using System;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class SucceededState : IState
	{
		public const string StateName = "Succeeded";

		public TimeSpan? ExpiresAfter => TimeSpan.FromHours(1);

		public string Name => StateName;
	}
}
