using System;
using MR.AspNetCore.Jobs.Models;

namespace MR.AspNetCore.Jobs.Server.States
{
	public interface IState
	{
		TimeSpan? ExpiresAfter { get; }
		string Name { get; }

		void Apply(Job job, IStorageTransaction transaction);
	}
}
