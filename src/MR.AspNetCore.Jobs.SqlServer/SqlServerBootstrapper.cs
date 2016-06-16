using Microsoft.AspNetCore.Hosting;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerBootstrapper : BootstrapperBase
	{
		private IApplicationLifetime _appLifetime;

		public SqlServerBootstrapper(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server,
			IApplicationLifetime appLifetime)
			: base(options, storage, server)
		{
			_appLifetime = appLifetime;
		}

		public override void BootstrapCore()
		{
			_appLifetime.ApplicationStopping.Register(() => Server.Dispose());
		}
	}
}
