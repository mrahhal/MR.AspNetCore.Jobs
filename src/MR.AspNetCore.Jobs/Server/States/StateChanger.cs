using System;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server.States
{
	public class StateChanger : IStateChanger
	{
		public void ChangeState(Job job, IState state, IStorageTransaction transaction)
		{
			var now = DateTime.UtcNow;
			if (state.ExpiresAfter != null)
			{
				job.ExpiresAt = now.Add(state.ExpiresAfter.Value);
			}
			else
			{
				job.ExpiresAt = null;
			}

			job.StateName = state.Name;

			transaction.UpdateJob(job);
		}
	}
}
