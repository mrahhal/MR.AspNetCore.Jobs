using System;
using Microsoft.Extensions.DependencyInjection;
using MR.AspNetCore.Jobs.Server;

namespace MR.AspNetCore.Jobs
{
	public class SqlServerJobsOptionsExtension : IJobsOptionsExtension
	{
		private Action<SqlServerOptions> _configure;

		public SqlServerJobsOptionsExtension(Action<SqlServerOptions> configure)
		{
			_configure = configure;
		}

		public void AddServices(IServiceCollection services)
		{
			services.AddSingleton<IBootstrapper, SqlServerBootstrapper>();
			services.AddSingleton<IStorage, SqlServerStorage>();
			services.AddSingleton<IStorageConnection, SqlServerStorageConnection>();

			services.AddSingleton<IAdditionalProcessor, ExpirationManager>();

			services.Configure(_configure);
		}
	}
}
