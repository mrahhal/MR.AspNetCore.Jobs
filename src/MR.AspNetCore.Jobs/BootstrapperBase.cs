using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public abstract class BootstrapperBase : IBootstrapper
	{
		public BootstrapperBase(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server)
		{
			Options = options;
			Storage = storage;
			Server = server;
		}

		protected JobsOptions Options { get; }

		protected IStorage Storage { get; }

		protected IProcessingServer Server { get; }

		public void Bootstrap()
		{
			Storage.Initialize();

			BootstrapCore();

			Server.Start();
		}

		public virtual void BootstrapCore()
		{
		}
	}
}
