using System;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class ProcessingState : IState
	{
		public const string StateName = "Processing";

		public TimeSpan? ExpiresAfter => null;

		public string Name => StateName;
	}
}
