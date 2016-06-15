using System;

namespace MR.AspNetCore.Jobs.Server
{
	public interface IProcessingServer : IDisposable
	{
		void Pulse(PulseKind kind);
		void Start();
	}
}
