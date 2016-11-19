using System;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class ScheduledState : IState
	{
		public const string StateName = "Scheduled";

		public TimeSpan? ExpiresAfter => null;

		public string Name => StateName;

		public void Apply(Job job, IStorageTransaction transaction)
		{
		}
	}
}
