using System;

namespace MR.AspNetCore.Jobs.Server.States
{
	public interface IState
	{
		TimeSpan? ExpiresAfter { get; }
		string Name { get; }
	}
}
