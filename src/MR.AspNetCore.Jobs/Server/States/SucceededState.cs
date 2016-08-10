using System;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class SucceededState : IState
	{
		public const string StateName = "Succeeded";

		public TimeSpan? ExpiresAfter => TimeSpan.FromHours(1);

		public string Name => StateName;

		public void Apply(Job job, IStorageTransaction transaction)
		{
		}
	}
}
